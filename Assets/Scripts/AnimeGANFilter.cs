using UnityEngine;
using Unity.InferenceEngine;

public static class AnimeGANFilter
{
    const int MODEL_SIZE = 512;
    const string MODEL_PATH = "ML/Models/AnimeGANv2_FacePaint";

    static Model cachedModel;
    static ModelAsset cachedModelAsset;

    /// <summary>
    /// 既にクロップ済みの顔画像にAnimeGANv2を適用する。
    /// 入力は任意サイズ（内部で512x512にリサイズ）。
    /// </summary>
    public static Texture2D Apply(Texture2D source)
    {
        if (source == null) return null;

        // モデル読み込み（初回のみ）
        if (cachedModel == null)
        {
            cachedModelAsset = Resources.Load<ModelAsset>(MODEL_PATH);
            if (cachedModelAsset == null)
            {
                Debug.LogError($"[AnimeGAN] Model not found at Resources/{MODEL_PATH}");
                return null;
            }
            cachedModel = ModelLoader.Load(cachedModelAsset);
        }

        int srcW = source.width;
        int srcH = source.height;

        Debug.Log($"[AnimeGAN] Input: {srcW}x{srcH}");

        // Blit で 512x512 にリサイズ
        RenderTexture rt = RenderTexture.GetTemporary(MODEL_SIZE, MODEL_SIZE, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D resized = new Texture2D(MODEL_SIZE, MODEL_SIZE, TextureFormat.RGBA32, false);
        resized.ReadPixels(new Rect(0, 0, MODEL_SIZE, MODEL_SIZE), 0, 0);
        resized.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        // テンソル作成: NCHW, [-1, 1]
        float[] inputData = new float[3 * MODEL_SIZE * MODEL_SIZE];
        Color[] pixels = resized.GetPixels();
        for (int y = 0; y < MODEL_SIZE; y++)
        {
            for (int x = 0; x < MODEL_SIZE; x++)
            {
                Color c = pixels[(MODEL_SIZE - 1 - y) * MODEL_SIZE + x];
                int idx = y * MODEL_SIZE + x;
                inputData[0 * MODEL_SIZE * MODEL_SIZE + idx] = c.r * 2f - 1f;
                inputData[1 * MODEL_SIZE * MODEL_SIZE + idx] = c.g * 2f - 1f;
                inputData[2 * MODEL_SIZE * MODEL_SIZE + idx] = c.b * 2f - 1f;
            }
        }
        Object.Destroy(resized);

        var inputTensor = new Tensor<float>(new TensorShape(1, 3, MODEL_SIZE, MODEL_SIZE), inputData);

        // 推論
        var worker = new Worker(cachedModel, BackendType.CPU);
        worker.Schedule(inputTensor);
        var outputGPU = worker.PeekOutput() as Tensor<float>;
        var outputTensor = outputGPU.ReadbackAndClone();
        float[] outputData = outputTensor.DownloadToArray();

        // 出力をテクスチャに変換: [-1,1] → [0,1]
        Texture2D result = new Texture2D(MODEL_SIZE, MODEL_SIZE, TextureFormat.RGBA32, false);
        Color[] outPixels = new Color[MODEL_SIZE * MODEL_SIZE];
        for (int y = 0; y < MODEL_SIZE; y++)
        {
            for (int x = 0; x < MODEL_SIZE; x++)
            {
                int idx = y * MODEL_SIZE + x;
                float r = Mathf.Clamp01(outputData[0 * MODEL_SIZE * MODEL_SIZE + idx] * 0.5f + 0.5f);
                float g = Mathf.Clamp01(outputData[1 * MODEL_SIZE * MODEL_SIZE + idx] * 0.5f + 0.5f);
                float b = Mathf.Clamp01(outputData[2 * MODEL_SIZE * MODEL_SIZE + idx] * 0.5f + 0.5f);
                outPixels[(MODEL_SIZE - 1 - y) * MODEL_SIZE + x] = new Color(r, g, b, 1f);
            }
        }
        result.SetPixels(outPixels);
        result.Apply();

        // クリーンアップ
        outputTensor.Dispose();
        inputTensor.Dispose();
        worker.Dispose();

        Debug.Log("[AnimeGAN] Filter applied successfully");
        return result;
    }
}
