using UnityEngine;
using UnityEditor;
using System.IO;
using Unity.InferenceEngine;

public class AnimeGANTest
{
    [MenuItem("GodBabys/AnimeGAN テスト")]
    static void Run()
    {
        // ── モデル読み込み ──
        var modelAsset = AssetDatabase.LoadAssetAtPath<ModelAsset>("Assets/ML/Models/AnimeGANv2_FacePaint.onnx");
        if (modelAsset == null)
        {
            Debug.LogError("[AnimeGAN] ONNX model not found at Assets/ML/Models/AnimeGANv2_FacePaint.onnx");
            return;
        }

        // ── 入力画像読み込み ──
        var srcTex = Resources.Load<Texture2D>("BabySynth/Swaddles/aki2");
        if (srcTex == null)
        {
            Debug.LogError("[AnimeGAN] aki2.png not found");
            return;
        }

        Texture2D readable = MakeReadable(srcTex);

        // ── 512x512にリサイズ ──
        int size = 512;
        RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(readable, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D resized = new Texture2D(size, size, TextureFormat.RGBA32, false);
        resized.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        resized.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        // ── テンソル作成: [-1, 1] 正規化, NCHW ──
        float[] inputData = new float[3 * size * size];
        Color[] pixels = resized.GetPixels();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Unity GetPixels は bottom-left origin なので Y を反転
                Color c = pixels[(size - 1 - y) * size + x];
                int idx = y * size + x;
                inputData[0 * size * size + idx] = c.r * 2f - 1f; // R
                inputData[1 * size * size + idx] = c.g * 2f - 1f; // G
                inputData[2 * size * size + idx] = c.b * 2f - 1f; // B
            }
        }

        var inputTensor = new Tensor<float>(new TensorShape(1, 3, size, size), inputData);

        // ── 推論 ──
        Debug.Log("[AnimeGAN] Starting inference...");
        var model = ModelLoader.Load(modelAsset);
        var worker = new Worker(model, BackendType.GPUCompute);

        worker.Schedule(inputTensor);
        var outputTensorGPU = worker.PeekOutput() as Tensor<float>;
        var outputTensor = outputTensorGPU.ReadbackAndClone();
        float[] outputData = outputTensor.DownloadToArray();
        outputTensor.Dispose();

        Debug.Log($"[AnimeGAN] Inference done. Output length: {outputData.Length}");

        // ── 出力をテクスチャに変換: [-1,1] → [0,1] ──
        Texture2D result = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] outPixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int idx = y * size + x;
                float r = Mathf.Clamp01(outputData[0 * size * size + idx] * 0.5f + 0.5f);
                float g = Mathf.Clamp01(outputData[1 * size * size + idx] * 0.5f + 0.5f);
                float b = Mathf.Clamp01(outputData[2 * size * size + idx] * 0.5f + 0.5f);
                // Y反転して戻す
                outPixels[(size - 1 - y) * size + x] = new Color(r, g, b, 1f);
            }
        }
        result.SetPixels(outPixels);
        result.Apply();

        // ── 保存 ──
        string outDir = Path.Combine(Application.dataPath, "../BabyFilterTestResults");
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);
        string outPath = Path.Combine(outDir, "AnimeGAN_Result.png");
        File.WriteAllBytes(outPath, result.EncodeToPNG());

        // ── クリーンアップ ──
        worker.Dispose();
        inputTensor.Dispose();
        Object.DestroyImmediate(readable);
        Object.DestroyImmediate(resized);
        Object.DestroyImmediate(result);

        Debug.Log($"[AnimeGAN] Saved to: {outPath}");
        System.Diagnostics.Process.Start("open", outDir);
    }

    static Texture2D MakeReadable(Texture2D src)
    {
        if (src.isReadable)
        {
            var copy = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
            copy.SetPixels(src.GetPixels());
            copy.Apply();
            return copy;
        }
        var rt = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return tex;
    }
}
