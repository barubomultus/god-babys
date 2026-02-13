using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

public static class SetupJapaneseFontFallback
{
    private const string FontAssetPath = "Assets/Resources/NotoSansJP-Medium SDF.asset";
    private const string DefaultFontPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    [MenuItem("Tools/Setup Japanese Font")]
    public static void Setup()
    {
        // 壊れたアセットがあれば削除
        var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(FontAssetPath);
            Debug.Log("Deleted existing font asset to recreate.");
        }

        // DynamicOS: OS のシステムフォントから直接生成
        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset("Hiragino Sans", "W4", 32);

        if (fontAsset == null)
            fontAsset = TMP_FontAsset.CreateFontAsset("Hiragino Kaku Gothic ProN", "W4", 32);

        if (fontAsset == null)
        {
            Debug.LogError("Japanese font not found on this system.");
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Fonts"))
            AssetDatabase.CreateFolder("Assets", "Fonts");

        // メインアセットを保存
        AssetDatabase.CreateAsset(fontAsset, FontAssetPath);

        // アトラステクスチャをサブアセットとして保存
        if (fontAsset.atlasTextures != null)
        {
            for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
            {
                var tex = fontAsset.atlasTextures[i];
                if (tex != null)
                {
                    tex.name = fontAsset.name + " Atlas " + i;
                    AssetDatabase.AddObjectToAsset(tex, fontAsset);
                }
            }
        }

        // マテリアルをサブアセットとして保存
        if (fontAsset.material != null)
        {
            fontAsset.material.name = fontAsset.name + " Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created Japanese font asset: " + FontAssetPath);

        AssignFallback(fontAsset);
    }

    static void AssignFallback(TMP_FontAsset japaneseFontAsset)
    {
        var defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontPath);
        if (defaultFont == null)
        {
            Debug.LogError("Default font not found: " + DefaultFontPath);
            return;
        }

        if (defaultFont.fallbackFontAssetTable == null)
            defaultFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();

        // 既存のフォールバックをクリアして再登録
        defaultFont.fallbackFontAssetTable.RemoveAll(fb => fb == null || fb == japaneseFontAsset);
        defaultFont.fallbackFontAssetTable.Add(japaneseFontAsset);

        EditorUtility.SetDirty(defaultFont);
        AssetDatabase.SaveAssets();
        Debug.Log("Added Japanese font as fallback to LiberationSans SDF.");
    }
}
