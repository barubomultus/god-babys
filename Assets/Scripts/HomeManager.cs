using UnityEngine;
using UnityEngine.SceneManagement;
using UIE = UnityEngine.UIElements;

public class HomeManager : MonoBehaviour
{
    private UIE.PanelSettings panelSettings;
    private UIE.VisualElement root;
    private int selectedIcon;

    private UIE.VisualElement koimikoshiContainer;
    private float sparkleTimer = 0f;

    private static readonly string[] iconNames = new string[]
    {
        "kayo", "ikemen", "inteli", "matcho", "old-women", "sexy-lady"
    };

    void Start()
    {
        Debug.Log("[STARTUP] HomeManager.Start() called");
        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/HomeStyle" }, panelSettings);
        UIHelper.RegisterTapSE(root);

        var dc = DataCarrier.Instance;
        selectedIcon = dc != null ? dc.playerIcon : DataCarrier.GetProfileIcon();

        BuildUI();
    }

    void OnDestroy()
    {
        if (panelSettings != null)
            Destroy(panelSettings);
    }

    void Update()
    {
        if (koimikoshiContainer == null) return;

        sparkleTimer += Time.deltaTime;

        float pulse = 0.94f + 0.06f * Mathf.Sin(sparkleTimer * Mathf.PI);
        float cycle = sparkleTimer % 3f;
        float flash = 0f;
        if (cycle < 0.15f)
            flash = Mathf.Sin(cycle / 0.15f * Mathf.PI) * 0.10f;

        koimikoshiContainer.style.opacity = Mathf.Clamp01(pulse + flash);
        float s = 1f + flash * 0.24f;
        koimikoshiContainer.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
    }

    void BuildUI()
    {
        var dc = DataCarrier.Instance;

        // 背景
        var bg = new UIE.VisualElement();
        bg.AddToClassList("home-bg");
        bg.pickingMode = UIE.PickingMode.Ignore;
        root.Add(bg);

        // ビネットレイヤー（四隅→中央へごく薄いピンク）
        string[] vignetteClasses = { "home-vignette-1", "home-vignette-2", "home-vignette-3" };
        foreach (var vc in vignetteClasses)
        {
            var v = new UIE.VisualElement();
            v.AddToClassList(vc);
            v.pickingMode = UIE.PickingMode.Ignore;
            root.Add(v);
        }

        // 星の地紋（ゴールド 5%透過度でうっすら散布）
        var starField = new UIE.VisualElement();
        starField.pickingMode = UIE.PickingMode.Ignore;
        starField.style.position = UIE.Position.Absolute;
        starField.style.left = 0; starField.style.top = 0;
        starField.style.right = 0; starField.style.bottom = 0;
        root.Add(starField);

        string[] starSymbols = { "\u2726", "\u22C6", "\u2727" };
        for (int i = 0; i < 25; i++)
        {
            var star = new UIE.Label();
            star.pickingMode = UIE.PickingMode.Ignore;
            star.text = starSymbols[i % starSymbols.Length];
            star.style.position = UIE.Position.Absolute;
            star.style.left = Random.Range(20f, 1060f);
            star.style.top = Random.Range(40f, 1880f);
            star.style.fontSize = Random.Range(12, 22);
            star.style.color = new Color(0.78f, 0.66f, 0.31f, 0.05f);
            star.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            starField.Add(star);
        }

        // フレックスボックスのルートコンテナ
        var homeRoot = new UIE.VisualElement();
        homeRoot.AddToClassList("home-root");
        root.Add(homeRoot);

        // --- 上部: セーフエリア + プロフィール ---
        var (safeTop, _, _, safeBottom) = UIHelper.GetSafeMargins();

        var profile = new UIE.VisualElement();
        profile.AddToClassList("home-profile");
        profile.style.paddingTop = 140 + safeTop;
        homeRoot.Add(profile);

        // Player icon（ダブルゴールドリング）
        CreatePlayerIcon(profile);

        // Player name
        string pName = dc != null ? dc.playerName : DataCarrier.GetProfileName();
        var nameLabel = UIHelper.CreateLabel(
            string.IsNullOrEmpty(pName) ? "???" : pName, "home-player-name");
        UIHelper.ApplyFontBold(nameLabel);
        profile.Add(nameLabel);

        // Separator
        var sep = new UIE.VisualElement();
        sep.AddToClassList("home-separator");
        profile.Add(sep);

        // --- 中央: ガチャ説明 + こいみこし ---
        var center = new UIE.VisualElement();
        center.AddToClassList("home-center");
        homeRoot.Add(center);

        // Gacha description
        var descLabel = UIHelper.CreateLabel(Localization.Get("home_gacha_desc"), "gacha-desc");
        center.Add(descLabel);

        // Koimikoshi image
        var koimikoshiSprite = Resources.Load<Sprite>("UI/koimikoshi");
        if (koimikoshiSprite != null)
        {
            koimikoshiContainer = new UIE.VisualElement();
            koimikoshiContainer.AddToClassList("koimikoshi-container");

            var koimiImg = new UIE.VisualElement();
            koimiImg.AddToClassList("koimikoshi-image");
            koimiImg.style.backgroundImage = new UIE.StyleBackground(koimikoshiSprite);
            koimikoshiContainer.Add(koimiImg);

            center.Add(koimikoshiContainer);
        }

        // --- 下部: ボタン群 ---
        var buttons = new UIE.VisualElement();
        buttons.AddToClassList("home-buttons");
        buttons.style.paddingBottom = 260 + safeBottom;
        homeRoot.Add(buttons);

        // Meet button (ピンク塗りつぶし)
        CreateMeetButton(buttons);

        // Babys button (ゴールド枠)
        CreateBorderedButton(buttons, Localization.Get("home_babys"),
            new Color(0.82f, 0.63f, 0.18f),
            new Color(0.55f, 0.45f, 0.15f),
            () => SceneManager.LoadScene("BabysScene"));

        // Enishi button (パープル枠)
        CreateBorderedButton(buttons, Localization.Get("home_enishi"),
            new Color(0.55f, 0.38f, 0.65f),
            new Color(0.40f, 0.28f, 0.50f),
            () => SceneManager.LoadScene("EnishiScene"));
    }

    void CreatePlayerIcon(UIE.VisualElement parent)
    {
        var border = new UIE.VisualElement();
        border.AddToClassList("home-icon-border");

        var innerRing = new UIE.VisualElement();
        innerRing.AddToClassList("home-icon-inner-ring");

        var maskBtn = new UIE.Button();
        maskBtn.AddToClassList("home-icon-mask");
        maskBtn.clicked += () => SceneManager.LoadScene("ProfileScene");

        var iconImg = new UIE.VisualElement();
        iconImg.AddToClassList("home-icon-image");
        ApplyIconSprite(iconImg, selectedIcon);
        maskBtn.Add(iconImg);

        innerRing.Add(maskBtn);
        border.Add(innerRing);
        parent.Add(border);
    }

    void ApplyIconSprite(UIE.VisualElement target, int index)
    {
        string name = (index >= 0 && index < iconNames.Length) ? iconNames[index] : iconNames[0];
        Sprite spr = Resources.Load<Sprite>($"Icons/{name}");
        if (spr != null)
        {
            target.style.backgroundImage = new UIE.StyleBackground(spr);
            target.style.backgroundColor = UIE.StyleKeyword.None;
        }
        else
        {
            target.style.backgroundImage = UIE.StyleKeyword.None;
            target.style.backgroundColor = new Color(0.7f, 0.7f, 0.7f);
        }
    }

    void CreateMeetButton(UIE.VisualElement parent)
    {
        var row = new UIE.VisualElement();
        row.AddToClassList("home-btn-row");

        var borderEl = new UIE.VisualElement();
        borderEl.AddToClassList("meet-btn-border");
        borderEl.style.backgroundColor = new Color(0.85f, 0.25f, 0.45f);

        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        shadow.style.backgroundColor = new Color(0.85f, 0.25f, 0.45f, 0.15f);
        borderEl.Add(shadow);

        var btn = new UIE.Button();
        btn.AddToClassList("meet-btn");
        UIHelper.ApplyFont(btn);
        btn.text = "\u2665  " + Localization.Get("home_meet");
        btn.clicked += () =>
        {
            if (DataCarrier.Instance != null)
                DataCarrier.Instance.currentSlot = -1;
            SceneManager.LoadScene("BirthScene");
        };
        borderEl.Add(btn);

        row.Add(borderEl);
        parent.Add(row);
    }

    void CreateBorderedButton(UIE.VisualElement parent, string label, Color borderColor,
        Color textColor, System.Action onClick)
    {
        var row = new UIE.VisualElement();
        row.AddToClassList("home-btn-row");

        var btn = new UIE.Button();
        btn.AddToClassList("bordered-pill");
        btn.style.borderTopColor = borderColor;
        btn.style.borderBottomColor = borderColor;
        btn.style.borderLeftColor = borderColor;
        btn.style.borderRightColor = borderColor;
        btn.style.color = textColor;
        UIHelper.ApplyFont(btn);
        btn.text = label;
        btn.clicked += () => onClick();

        row.Add(btn);
        parent.Add(row);
    }
}
