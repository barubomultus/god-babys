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
        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/HomeStyle" }, panelSettings);

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

        // Background
        var bg = new UIE.VisualElement();
        bg.AddToClassList("bg-screen");
        root.Add(bg);

        // Content column
        var content = new UIE.VisualElement();
        content.style.position = UIE.Position.Absolute;
        content.style.left = 0;
        content.style.right = 0;
        content.style.top = 0;
        content.style.bottom = 0;
        content.style.alignItems = UIE.Align.Center;
        root.Add(content);

        // Safe area top spacer
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();
        var topSpacer = new UIE.VisualElement();
        topSpacer.style.height = 140 + safeTop;
        content.Add(topSpacer);

        // Player icon (circle with border)
        CreatePlayerIcon(content);

        // Player name
        string pName = dc != null ? dc.playerName : DataCarrier.GetProfileName();
        var nameLabel = UIHelper.CreateLabel(
            string.IsNullOrEmpty(pName) ? "???" : pName, "home-player-name");
        content.Add(nameLabel);

        // Separator
        var sep = new UIE.VisualElement();
        sep.AddToClassList("separator");
        sep.style.marginTop = 20;
        content.Add(sep);

        // Gacha description
        var descLabel = UIHelper.CreateLabel(Localization.Get("home_gacha_desc"), "gacha-desc");
        content.Add(descLabel);

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

            content.Add(koimikoshiContainer);
        }

        // Spacer
        var spacer = new UIE.VisualElement();
        spacer.style.flexGrow = 1;
        content.Add(spacer);

        // === Buttons ===

        // Meet button (pink border)
        CreateMeetButton(content);

        // Babys button (gold border)
        CreateBorderedButton(content, Localization.Get("home_babys"),
            new Color(0.85f, 0.65f, 0.13f),
            new Color(0.45f, 0.45f, 0.5f),
            () => SceneManager.LoadScene("BabysScene"));

        // Enishi button (purple border)
        CreateBorderedButton(content, Localization.Get("home_enishi"),
            new Color(0.55f, 0.35f, 0.65f),
            new Color(0.45f, 0.45f, 0.5f),
            () => SceneManager.LoadScene("EnishiScene"));

        // Bottom spacer
        var bottomSpacer = new UIE.VisualElement();
        bottomSpacer.style.height = 260;
        content.Add(bottomSpacer);
    }

    void CreatePlayerIcon(UIE.VisualElement parent)
    {
        var border = new UIE.VisualElement();
        border.AddToClassList("home-icon-border");

        var maskBtn = new UIE.Button();
        maskBtn.AddToClassList("home-icon-mask");
        maskBtn.clicked += () => SceneManager.LoadScene("ProfileScene");

        var iconImg = new UIE.VisualElement();
        iconImg.AddToClassList("home-icon-image");
        ApplyIconSprite(iconImg, selectedIcon);
        maskBtn.Add(iconImg);

        border.Add(maskBtn);
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
        var wrapper = new UIE.VisualElement();
        wrapper.style.alignItems = UIE.Align.Center;
        wrapper.style.marginTop = 20;

        // Pink border behind
        var borderEl = new UIE.VisualElement();
        borderEl.AddToClassList("meet-btn-border");
        borderEl.style.backgroundColor = new Color(0.95f, 0.30f, 0.55f);

        // Shadow
        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        shadow.style.backgroundColor = new Color(0.95f, 0.30f, 0.55f, 0.15f);
        borderEl.Add(shadow);

        // White button
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

        wrapper.Add(borderEl);
        parent.Add(wrapper);
    }

    void CreateBorderedButton(UIE.VisualElement parent, string label, Color borderColor,
        Color textColor, System.Action onClick)
    {
        var wrapper = new UIE.VisualElement();
        wrapper.style.alignItems = UIE.Align.Center;
        wrapper.style.marginTop = 20;

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

        wrapper.Add(btn);
        parent.Add(wrapper);
    }
}
