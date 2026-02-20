using UnityEngine;
using UIE = UnityEngine.UIElements;

public static class UIHelper
{
    private static Font _font;
    private static bool _fontSearched;

    // タップSE
    private static AudioClip _tapSE;
    private static bool _tapSESearched;
    private static GameObject _sePlayer;

    public static void PlayTapSE()
    {
        if (!_tapSESearched)
        {
            _tapSESearched = true;
            _tapSE = Resources.Load<AudioClip>("SE/tap-effect");
        }
        if (_tapSE == null) return;

        if (_sePlayer == null)
        {
            _sePlayer = new GameObject("UIHelper_SE");
            Object.DontDestroyOnLoad(_sePlayer);
            _sePlayer.AddComponent<AudioSource>();
        }
        var src = _sePlayer.GetComponent<AudioSource>();
        src.pitch = Random.Range(0.96f, 1.08f);
        src.PlayOneShot(_tapSE, 0.7f);
    }

    public static UIE.PanelSettings CreatePanelSettings(float sortingOrder = 0f)
    {
        var ps = ScriptableObject.CreateInstance<UIE.PanelSettings>();
        ps.scaleMode = UIE.PanelScaleMode.ScaleWithScreenSize;
        ps.referenceResolution = new Vector2Int(1080, 1920);
        ps.screenMatchMode = UIE.PanelScreenMatchMode.MatchWidthOrHeight;
        ps.match = 0f;
        ps.sortingOrder = sortingOrder;
        return ps;
    }

    public static UIE.VisualElement SetupUIDocument(GameObject go, string[] ussPaths, UIE.PanelSettings ps)
    {
        var doc = go.GetComponent<UIE.UIDocument>();
        if (doc == null)
            doc = go.AddComponent<UIE.UIDocument>();
        doc.panelSettings = ps;

        var root = doc.rootVisualElement;
        root.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        root.style.height = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        root.pickingMode = UIE.PickingMode.Ignore;
        root.focusable = false;

        foreach (var path in ussPaths)
        {
            var ss = Resources.Load<UIE.StyleSheet>(path);
            if (ss != null)
                root.styleSheets.Add(ss);
            else
                Debug.LogWarning($"[UIHelper] USS not found: {path}");
        }

        return root;
    }

    public static UIE.VisualElement SetupUIDocument(GameObject go, string ussPath, UIE.PanelSettings ps)
    {
        return SetupUIDocument(go, new[] { ussPath }, ps);
    }

    public static (float top, float bottom, float left, float right) GetSafeMargins()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return (0f, 0f, 0f, 0f);
#else
        Rect safeArea = Screen.safeArea;
        float top = Screen.height - safeArea.yMax;
        float bottom = safeArea.y;
        float left = safeArea.x;
        float right = Screen.width - safeArea.xMax;

        // Convert to reference resolution scale (1080x1920, match width)
        float scale = Screen.width / 1080f;
        if (scale <= 0f) scale = 1f;

        return (top / scale, bottom / scale, left / scale, right / scale);
#endif
    }

    public static void ApplyFont(UIE.VisualElement element)
    {
        var font = GetFont();
        if (font != null)
            element.style.unityFontDefinition = new UIE.StyleFontDefinition(font);
    }

    public static Font GetFont()
    {
        if (!_fontSearched)
        {
            _fontSearched = true;
            _font = Resources.Load<Font>("Fonts/NotoSansJP-Medium");
            if (_font == null)
                Debug.LogWarning("[UIHelper] No Japanese font found at Resources/Fonts/NotoSansJP-Medium");
        }
        return _font;
    }

    // --- Common element builders ---

    public static UIE.Button CreatePillButton(string text, string className = "pill-button")
    {
        var btn = new UIE.Button();
        btn.AddToClassList(className);
        btn.text = text;
        ApplyFont(btn);
        return btn;
    }

    /// <summary>
    /// ルート要素にタップSEリスナーを登録。
    /// 配下の全Button/ClickEventでSEが鳴る。
    /// </summary>
    public static void RegisterTapSE(UIE.VisualElement root)
    {
        root.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            // クリックされた要素またはその親がButtonならSE再生
            var target = evt.target as UIE.VisualElement;
            while (target != null)
            {
                if (target is UIE.Button)
                {
                    PlayTapSE();
                    break;
                }
                target = target.parent;
            }
        }, UIE.TrickleDown.TrickleDown);
    }

    public static UIE.Label CreateLabel(string text, string className = null)
    {
        var label = new UIE.Label(text);
        if (className != null)
            label.AddToClassList(className);
        ApplyFont(label);
        return label;
    }

    public static UIE.VisualElement CreateOverlay(string className = "overlay-dark")
    {
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList(className);
        return overlay;
    }

    public static UIE.VisualElement CreateCircleMask(float size)
    {
        var mask = new UIE.VisualElement();
        mask.AddToClassList("circle-mask");
        mask.style.width = size;
        mask.style.height = size;
        return mask;
    }

    public static UIE.VisualElement CreateBackground(Sprite sprite, float width = 0, float height = 0)
    {
        var el = new UIE.VisualElement();
        if (sprite != null)
            el.style.backgroundImage = new UIE.StyleBackground(sprite);
        if (width > 0)
            el.style.width = width;
        if (height > 0)
            el.style.height = height;
        return el;
    }
}
