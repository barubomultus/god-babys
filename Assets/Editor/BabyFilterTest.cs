using UnityEngine;
using UnityEditor;
using System.IO;

public class BabyFilterTest
{
    [MenuItem("GodBabys/赤ちゃんフィルターテスト")]
    static void RunEachStage()
    {
        var srcTex = Resources.Load<Texture2D>("BabySynth/Swaddles/aki2");
        if (srcTex == null)
        {
            Debug.LogError("[BabyFilterTest] aki2.png not found");
            return;
        }

        Texture2D readable = MakeReadable(srcTex);
        string outDir = Path.Combine(Application.dataPath, "../BabyFilterTestResults");
        if (!Directory.Exists(outDir)) Directory.CreateDirectory(outDir);

        Vector2 leftEye = new Vector2(0.35f, 0.38f);
        Vector2 rightEye = new Vector2(0.65f, 0.38f);

        // ── デッサン風（既存） ──
        var dessin = BabySynthesizer.CreateDessinFilteredTexture(readable, leftEye, rightEye);
        SaveAndDestroy(dessin, outDir, "A_Dessin.png");

        // ── ライトアニメ: Bilateral x1 + CelShading(デッサン) + 輪郭線 ──
        // bits: 0=Bilateral, 4=CelShading, 6=Outlines
        var lightAnime = BabySynthesizer.CreateAnimeFilteredTexture(readable, leftEye, rightEye,
            (1 << 0) | (1 << 4) | (1 << 6));
        SaveAndDestroy(lightAnime, outDir, "B_LightAnime.png");

        // ── ソフトアニメ: BabySoft + Bilateral x1 + GhibliCelShading + CheekPink ──
        // bits: 0=Bilateral, 2=BabySoft, 5=GhibliCelShading, 7=CheekPink
        var softAnime = BabySynthesizer.CreateAnimeFilteredTexture(readable, leftEye, rightEye,
            (1 << 0) | (1 << 2) | (1 << 5) | (1 << 7));
        SaveAndDestroy(softAnime, outDir, "C_SoftAnime.png");

        // ── エッジ保持+彩度+セル+輪郭 (デッサン風に近いが軽い) ──
        // bits: 1=EdgeSmooth, 3=Saturation, 4=CelShading, 6=Outlines
        var edgeAnime = BabySynthesizer.CreateAnimeFilteredTexture(readable, leftEye, rightEye,
            (1 << 1) | (1 << 3) | (1 << 4) | (1 << 6));
        SaveAndDestroy(edgeAnime, outDir, "D_EdgeAnime.png");

        // ── フル: Bilateral + BabySoft + GhibliCel + 輪郭線 + CheekPink + 目ハイライト ──
        // bits: 0=Bilateral, 2=BabySoft, 5=GhibliCelShading, 6=Outlines, 7=CheekPink, 8=Specular
        var fullAnime = BabySynthesizer.CreateAnimeFilteredTexture(readable, leftEye, rightEye,
            (1 << 0) | (1 << 2) | (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8));
        SaveAndDestroy(fullAnime, outDir, "E_FullAnime.png");

        Object.DestroyImmediate(readable);

        Debug.Log($"[BabyFilterTest] All results saved to: {outDir}");
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

    static void SaveAndDestroy(Texture2D tex, string dir, string filename)
    {
        if (tex == null) return;
        File.WriteAllBytes(Path.Combine(dir, filename), tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        Debug.Log($"[BabyFilterTest] Saved {filename}");
    }
}
