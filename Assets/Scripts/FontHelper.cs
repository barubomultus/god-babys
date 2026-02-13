using UnityEngine;
using TMPro;

public static class FontHelper
{
    private static TMP_FontAsset _jaFont;
    private static bool _searched;

    public static void Apply(TextMeshProUGUI tmp)
    {
        if (!_searched)
        {
            _searched = true;

            // まず既存のSDFアセットを試す
            _jaFont = Resources.Load<TMP_FontAsset>("NotoSansJP-Medium SDF");
            if (_jaFont == null)
                _jaFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");

            // SDFが見つからなければTTFからDynamic生成
            if (_jaFont == null)
            {
                var ttfFont = Resources.Load<Font>("Fonts/NotoSansJP-Medium");
                if (ttfFont != null)
                {
                    _jaFont = TMP_FontAsset.CreateFontAsset(ttfFont);
                    if (_jaFont != null)
                    {
                        _jaFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                        Debug.Log("[FontHelper] Created dynamic font from TTF");
                    }
                }
            }

            if (_jaFont != null)
                Debug.Log("[FontHelper] Font ready: " + _jaFont.name);
            else
                Debug.LogWarning("[FontHelper] No Japanese font found!");
        }
        if (_jaFont != null)
            tmp.font = _jaFont;
    }
}
