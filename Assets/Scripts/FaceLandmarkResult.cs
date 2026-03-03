using UnityEngine;

/// <summary>
/// 顔ランドマーク検出結果。全座標は正規化0-1、top-left原点。
/// iOS Vision / Android MLKit 共通フォーマット。
/// </summary>
[System.Serializable]
public struct FaceLandmarkResult
{
    /// <summary>顔バウンディングボックス (正規化0-1, top-left原点)</summary>
    public Rect faceBounds;

    public Vector2 leftEyeCenter;
    public Vector2 rightEyeCenter;
    public Vector2 noseCenter;
    public Vector2 mouthCenter;
    public Vector2 leftCheek;
    public Vector2 rightCheek;

    /// <summary>輪郭ポイント（あご変形用、左→右順）</summary>
    public Vector2[] jawlinePoints;

    /// <summary>顔の中心（バウンディングボックスの重心）</summary>
    public Vector2 FaceCenter => new Vector2(
        faceBounds.x + faceBounds.width * 0.5f,
        faceBounds.y + faceBounds.height * 0.5f);

    /// <summary>瞳間距離（スケール判定用）</summary>
    public float EyeDistance => Vector2.Distance(leftEyeCenter, rightEyeCenter);
}

/// <summary>JSON デシリアライズ用ラッパー（JsonUtility対応）</summary>
[System.Serializable]
public class FaceLandmarkJsonWrapper
{
    public bool detected;
    public float faceBoundsX, faceBoundsY, faceBoundsW, faceBoundsH;
    public float leftEyeX, leftEyeY;
    public float rightEyeX, rightEyeY;
    public float noseX, noseY;
    public float mouthX, mouthY;
    public float leftCheekX, leftCheekY;
    public float rightCheekX, rightCheekY;
    public float[] jawline; // flat array [x0,y0,x1,y1,...]

    public FaceLandmarkResult ToResult()
    {
        var result = new FaceLandmarkResult
        {
            faceBounds = new Rect(faceBoundsX, faceBoundsY, faceBoundsW, faceBoundsH),
            leftEyeCenter = new Vector2(leftEyeX, leftEyeY),
            rightEyeCenter = new Vector2(rightEyeX, rightEyeY),
            noseCenter = new Vector2(noseX, noseY),
            mouthCenter = new Vector2(mouthX, mouthY),
            leftCheek = new Vector2(leftCheekX, leftCheekY),
            rightCheek = new Vector2(rightCheekX, rightCheekY)
        };

        if (jawline != null && jawline.Length >= 2)
        {
            result.jawlinePoints = new Vector2[jawline.Length / 2];
            for (int i = 0; i < jawline.Length - 1; i += 2)
                result.jawlinePoints[i / 2] = new Vector2(jawline[i], jawline[i + 1]);
        }
        else
        {
            result.jawlinePoints = new Vector2[0];
        }

        return result;
    }
}
