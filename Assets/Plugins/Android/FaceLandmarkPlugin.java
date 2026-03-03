package com.godbabys.facedetection;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.PointF;
import android.graphics.Rect;
import android.util.Log;

import com.google.mlkit.vision.common.InputImage;
import com.google.mlkit.vision.face.Face;
import com.google.mlkit.vision.face.FaceContour;
import com.google.mlkit.vision.face.FaceDetection;
import com.google.mlkit.vision.face.FaceDetector;
import com.google.mlkit.vision.face.FaceDetectorOptions;
import com.google.mlkit.vision.face.FaceLandmark;
import com.google.android.gms.tasks.Tasks;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.List;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;

public class FaceLandmarkPlugin {

    private static final String TAG = "FaceLandmark";
    private static final ExecutorService executor = Executors.newSingleThreadExecutor();

    /**
     * PNG byte配列から顔ランドマークを検出し、JSON文字列で返す。
     * Unity C# から AndroidJavaClass 経由で呼び出される。
     */
    public static String detectFromPNG(byte[] pngData) {
        if (pngData == null || pngData.length == 0) {
            return null;
        }

        try {
            // バックグラウンドスレッドで検出実行（メインスレッドでのTasks.awaitを回避）
            Future<String> future = executor.submit(new Callable<String>() {
                @Override
                public String call() throws Exception {
                    return detectInternal(pngData);
                }
            });
            return future.get(); // 結果を待つ
        } catch (Exception e) {
            Log.e(TAG, "Detection failed: " + e.getMessage());
            return null;
        }
    }

    private static String detectInternal(byte[] pngData) throws Exception {
        Bitmap bitmap = BitmapFactory.decodeByteArray(pngData, 0, pngData.length);
        if (bitmap == null) {
            Log.e(TAG, "Failed to decode PNG data");
            return null;
        }

        int imgW = bitmap.getWidth();
        int imgH = bitmap.getHeight();

        InputImage inputImage = InputImage.fromBitmap(bitmap, 0);

        FaceDetectorOptions options = new FaceDetectorOptions.Builder()
                .setPerformanceMode(FaceDetectorOptions.PERFORMANCE_MODE_ACCURATE)
                .setLandmarkMode(FaceDetectorOptions.LANDMARK_MODE_ALL)
                .setContourMode(FaceDetectorOptions.CONTOUR_MODE_ALL)
                .build();

        FaceDetector detector = FaceDetection.getClient(options);

        // 同期的に検出実行
        List<Face> faces = Tasks.await(detector.process(inputImage));

        if (faces == null || faces.isEmpty()) {
            Log.i(TAG, "No faces detected");
            JSONObject noFace = new JSONObject();
            noFace.put("detected", false);
            return noFace.toString();
        }

        // 最大面積の顔を選択
        Face bestFace = null;
        int maxArea = 0;
        for (Face face : faces) {
            Rect bounds = face.getBoundingBox();
            int area = bounds.width() * bounds.height();
            if (area > maxArea) {
                maxArea = area;
                bestFace = face;
            }
        }

        if (bestFace == null) {
            JSONObject noFace = new JSONObject();
            noFace.put("detected", false);
            return noFace.toString();
        }

        // ─── ランドマーク抽出 ───

        Rect bounds = bestFace.getBoundingBox();
        // ピクセル座標 → 正規化 (0-1, top-left原点)
        float fbX = (float) bounds.left / imgW;
        float fbY = (float) bounds.top / imgH;
        float fbW = (float) bounds.width() / imgW;
        float fbH = (float) bounds.height() / imgH;

        // ランドマーク
        float leftEyeX = -1, leftEyeY = -1;
        float rightEyeX = -1, rightEyeY = -1;
        float noseX = -1, noseY = -1;
        float mouthX = -1, mouthY = -1;
        float leftCheekX = -1, leftCheekY = -1;
        float rightCheekX = -1, rightCheekY = -1;

        FaceLandmark leftEye = bestFace.getLandmark(FaceLandmark.LEFT_EYE);
        if (leftEye != null) {
            leftEyeX = leftEye.getPosition().x / imgW;
            leftEyeY = leftEye.getPosition().y / imgH;
        }

        FaceLandmark rightEye = bestFace.getLandmark(FaceLandmark.RIGHT_EYE);
        if (rightEye != null) {
            rightEyeX = rightEye.getPosition().x / imgW;
            rightEyeY = rightEye.getPosition().y / imgH;
        }

        FaceLandmark noseBase = bestFace.getLandmark(FaceLandmark.NOSE_BASE);
        if (noseBase != null) {
            noseX = noseBase.getPosition().x / imgW;
            noseY = noseBase.getPosition().y / imgH;
        }

        // 口の中心（左口角・右口角・下唇の重心）
        FaceLandmark mouthLeft = bestFace.getLandmark(FaceLandmark.MOUTH_LEFT);
        FaceLandmark mouthRight = bestFace.getLandmark(FaceLandmark.MOUTH_RIGHT);
        FaceLandmark mouthBottom = bestFace.getLandmark(FaceLandmark.MOUTH_BOTTOM);
        if (mouthLeft != null && mouthRight != null && mouthBottom != null) {
            mouthX = (mouthLeft.getPosition().x + mouthRight.getPosition().x + mouthBottom.getPosition().x) / 3f / imgW;
            mouthY = (mouthLeft.getPosition().y + mouthRight.getPosition().y + mouthBottom.getPosition().y) / 3f / imgH;
        }

        // 頬
        FaceLandmark leftCheekLm = bestFace.getLandmark(FaceLandmark.LEFT_CHEEK);
        if (leftCheekLm != null) {
            leftCheekX = leftCheekLm.getPosition().x / imgW;
            leftCheekY = leftCheekLm.getPosition().y / imgH;
        } else if (leftEyeX >= 0 && mouthX >= 0) {
            // フォールバック: 目と口の中間
            leftCheekX = (leftEyeX + mouthX) * 0.5f - 0.06f;
            leftCheekY = (leftEyeY + mouthY) * 0.5f;
        }

        FaceLandmark rightCheekLm = bestFace.getLandmark(FaceLandmark.RIGHT_CHEEK);
        if (rightCheekLm != null) {
            rightCheekX = rightCheekLm.getPosition().x / imgW;
            rightCheekY = rightCheekLm.getPosition().y / imgH;
        } else if (rightEyeX >= 0 && mouthX >= 0) {
            rightCheekX = (rightEyeX + mouthX) * 0.5f + 0.06f;
            rightCheekY = (rightEyeY + mouthY) * 0.5f;
        }

        // 輪郭 (faceContour)
        JSONArray jawlineArray = new JSONArray();
        FaceContour faceContour = bestFace.getContour(FaceContour.FACE);
        if (faceContour != null) {
            List<PointF> points = faceContour.getPoints();
            for (PointF pt : points) {
                jawlineArray.put(Math.round(pt.x / imgW * 10000.0) / 10000.0);
                jawlineArray.put(Math.round(pt.y / imgH * 10000.0) / 10000.0);
            }
        }

        // ─── JSON 組み立て ───
        JSONObject result = new JSONObject();
        result.put("detected", true);
        result.put("faceBoundsX", round4(fbX));
        result.put("faceBoundsY", round4(fbY));
        result.put("faceBoundsW", round4(fbW));
        result.put("faceBoundsH", round4(fbH));
        result.put("leftEyeX", round4(leftEyeX));
        result.put("leftEyeY", round4(leftEyeY));
        result.put("rightEyeX", round4(rightEyeX));
        result.put("rightEyeY", round4(rightEyeY));
        result.put("noseX", round4(noseX));
        result.put("noseY", round4(noseY));
        result.put("mouthX", round4(mouthX));
        result.put("mouthY", round4(mouthY));
        result.put("leftCheekX", round4(leftCheekX));
        result.put("leftCheekY", round4(leftCheekY));
        result.put("rightCheekX", round4(rightCheekX));
        result.put("rightCheekY", round4(rightCheekY));
        result.put("jawline", jawlineArray);

        Log.i(TAG, "Detection result: " + result.toString());

        bitmap.recycle();
        detector.close();

        return result.toString();
    }

    private static double round4(float v) {
        return Math.round(v * 10000.0) / 10000.0;
    }
}
