using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 1024x1024 のアプリアイコンを自動生成し、iOSビルド用アイコンとして設定するエディタツール。
/// メニュー: Tools > Generate App Icon
/// </summary>
public class AppIconGenerator
{
    static readonly string IconDir = "Assets/AppIcon";
    static readonly string IconPath = IconDir + "/AppIcon_1024.png";

    [MenuItem("Tools/Generate App Icon")]
    public static void GenerateIcon()
    {
        if (!Directory.Exists(IconDir))
            Directory.CreateDirectory(IconDir);

        // 既にアイコン画像がある場合はそれを使う
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (existing != null)
        {
            Debug.Log("[AppIconGenerator] Existing icon found: " + IconPath);
            AssignIconToAllPlatforms(existing);
            return;
        }

        // アイコン画像がない場合: テーマカラーで自動生成
        int size = 1024;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color mainColor = new Color(0.969f, 0.906f, 0.808f); // #F7E7CE
        Color subColor = new Color(1f, 0.718f, 0.773f);      // #FFB7C5
        Color accentColor = new Color(0.667f, 0.941f, 0.82f); // #AAF0D1

        // 背景グラデーション (メインカラー → サブカラー)
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / size;
            Color bg = Color.Lerp(mainColor, subColor, t * 0.5f);
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, bg);
            }
        }

        // 中央に星マーク (アクセントカラー)
        float centerX = size / 2f;
        float centerY = size / 2f;
        float starOuterR = size * 0.32f;
        float starInnerR = size * 0.15f;
        int points = 5;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - centerX;
                float dy = y - centerY;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);

                // 星型の距離
                float starAngle = angle + Mathf.PI / 2f;
                float segAngle = Mathf.PI * 2f / points;
                float halfSeg = segAngle / 2f;
                float modAngle = Mathf.Repeat(starAngle, segAngle);
                float t2 = modAngle < halfSeg
                    ? modAngle / halfSeg
                    : 1f - (modAngle - halfSeg) / halfSeg;
                float starDist = Mathf.Lerp(starInnerR, starOuterR, t2 > 0.5f ? (t2 - 0.5f) * 2f : 0f)
                    + Mathf.Lerp(starOuterR, starInnerR, t2 > 0.5f ? 0f : (0.5f - t2) * 2f) * 0f;
                starDist = Mathf.Lerp(starInnerR, starOuterR,
                    Mathf.Abs(Mathf.Sin(modAngle / segAngle * Mathf.PI)));

                if (dist < starDist)
                {
                    float edge = 1f - Mathf.Clamp01((starDist - dist) / 3f);
                    Color starCol = Color.Lerp(accentColor, Color.white, 0.3f);
                    Color prev = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, Color.Lerp(starCol, prev, edge * 0.3f));
                }

                // 中央の円（赤ちゃんの頬のイメージ）
                float circleR = size * 0.12f;
                if (dist < circleR)
                {
                    float cEdge = Mathf.Clamp01((circleR - dist) / 4f);
                    Color circleCol = new Color(1f, 0.92f, 0.85f); // 白に近い暖色
                    Color prev2 = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, Color.Lerp(prev2, circleCol, cEdge * 0.8f));
                }
            }
        }

        // テキスト "SB" は Texture2D では描けないので、星マークのみ

        tex.Apply();
        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(IconPath, pngData);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        // Import設定
        var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
        }

        var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (iconTex != null)
            AssignIconToAllPlatforms(iconTex);

        Debug.Log("[AppIconGenerator] App icon generated and assigned: " + IconPath);
    }

    static void AssignIconToAllPlatforms(Texture2D icon)
    {
        // iOS
        var iosGroup = BuildTargetGroup.iOS;
        var iosKinds = new[] {
            UnityEditor.iOS.iOSPlatformIconKind.Application,
            UnityEditor.iOS.iOSPlatformIconKind.Spotlight,
            UnityEditor.iOS.iOSPlatformIconKind.Settings,
            UnityEditor.iOS.iOSPlatformIconKind.Notification,
            UnityEditor.iOS.iOSPlatformIconKind.Marketing,
        };

        foreach (var kind in iosKinds)
        {
            var icons = PlayerSettings.GetPlatformIcons(iosGroup, kind);
            foreach (var ic in icons)
            {
                var texArr = new Texture2D[ic.maxLayerCount];
                for (int l = 0; l < ic.maxLayerCount; l++)
                    texArr[l] = icon;
                ic.SetTextures(texArr);
            }
            PlayerSettings.SetPlatformIcons(iosGroup, kind, icons);
        }

        // Default icon
        var defaultIcons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
        if (defaultIcons == null || defaultIcons.Length == 0)
            defaultIcons = new Texture2D[1];
        for (int i = 0; i < defaultIcons.Length; i++)
            defaultIcons[i] = icon;
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, defaultIcons);

        AssetDatabase.SaveAssets();
    }
}
