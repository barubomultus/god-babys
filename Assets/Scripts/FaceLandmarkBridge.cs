using UnityEngine;
using System.Runtime.InteropServices;

/// <summary>
/// ネイティブ顔認識ブリッジ。
/// iOS: Vision framework (VNDetectFaceLandmarksRequest)
/// Android: MLKit Face Detection
/// Editor: null (手動フォールバック)
/// </summary>
public static class FaceLandmarkBridge
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string _FaceLandmark_DetectFromPNG(byte[] pngData, int dataLength);
#endif

    /// <summary>
    /// Texture2D から顔ランドマークを検出する。
    /// 検出失敗・未対応プラットフォームでは null を返す。
    /// </summary>
    public static FaceLandmarkResult? DetectFace(Texture2D texture)
    {
        if (texture == null) return null;

        byte[] pngData = texture.EncodeToPNG();
        if (pngData == null || pngData.Length == 0) return null;

        string json = null;

#if UNITY_EDITOR
        // Editor: ネイティブ検出なし → null を返して手動目タップフローを起動
        Debug.Log("[FaceLandmarkBridge] Editor — no native detection, returning null for manual eye tap");
        return null;
#elif UNITY_IOS
        try
        {
            json = _FaceLandmark_DetectFromPNG(pngData, pngData.Length);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FaceLandmarkBridge] iOS detection error: {e.Message}");
            return null;
        }
#elif UNITY_ANDROID
        try
        {
            json = DetectFaceAndroid(pngData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FaceLandmarkBridge] Android detection error: {e.Message}");
            return null;
        }
#else
        return null;
#endif

#pragma warning disable CS0162 // Unreachable code
        return ParseResultJson(json);
#pragma warning restore CS0162
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static string DetectFaceAndroid(byte[] pngData)
    {
        using (var pluginClass = new AndroidJavaClass("com.godbabys.facedetection.FaceLandmarkPlugin"))
        {
            return pluginClass.CallStatic<string>("detectFromPNG", pngData);
        }
    }
#endif

#if UNITY_EDITOR
    /// <summary>
    /// Editor用: 顔テクスチャ空間（0-1, 顔画像中心=0.5,0.5）のダミーランドマーク。
    /// ネイティブ検出と同じ座標空間で返す。BirthSystem側で合成画像空間に変換される。
    /// </summary>
    private static FaceLandmarkResult CreateEditorDummyLandmarks()
    {
        // 顔テクスチャ空間（アップロード画像の正規化座標 0-1）
        return new FaceLandmarkResult
        {
            faceBounds = new Rect(0.15f, 0.15f, 0.70f, 0.70f),
            leftEyeCenter = new Vector2(0.35f, 0.38f),
            rightEyeCenter = new Vector2(0.65f, 0.38f),
            noseCenter = new Vector2(0.50f, 0.55f),
            mouthCenter = new Vector2(0.50f, 0.70f),
            leftCheek = new Vector2(0.28f, 0.55f),
            rightCheek = new Vector2(0.72f, 0.55f),
            jawlinePoints = new Vector2[]
            {
                new Vector2(0.15f, 0.50f),
                new Vector2(0.18f, 0.65f),
                new Vector2(0.25f, 0.78f),
                new Vector2(0.50f, 0.85f),
                new Vector2(0.75f, 0.78f),
                new Vector2(0.82f, 0.65f),
                new Vector2(0.85f, 0.50f),
            }
        };
    }
#endif

    private static FaceLandmarkResult? ParseResultJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.Log("[FaceLandmarkBridge] No JSON result from native");
            return null;
        }

        try
        {
            var wrapper = JsonUtility.FromJson<FaceLandmarkJsonWrapper>(json);
            if (wrapper == null || !wrapper.detected)
            {
                Debug.Log("[FaceLandmarkBridge] No face detected in image");
                return null;
            }

            var result = wrapper.ToResult();
            Debug.Log($"[FaceLandmarkBridge] Face detected: bounds=({result.faceBounds.x:F2},{result.faceBounds.y:F2},{result.faceBounds.width:F2},{result.faceBounds.height:F2}) " +
                       $"leftEye=({result.leftEyeCenter.x:F2},{result.leftEyeCenter.y:F2}) " +
                       $"rightEye=({result.rightEyeCenter.x:F2},{result.rightEyeCenter.y:F2})");
            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FaceLandmarkBridge] JSON parse error: {e.Message}\nJSON: {json}");
            return null;
        }
    }
}
