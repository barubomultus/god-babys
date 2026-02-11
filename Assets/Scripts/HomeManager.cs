using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;


public class HomeManager : MonoBehaviour
{
    private Canvas mainCanvas;
    private Image playerIconImg;
    private int selectedIcon;

    private static Sprite _circleSprite;
    private static Sprite _pillSprite;
    private static Sprite _shadowSprite;
    private CanvasGroup koimikoshiGroup;
    private const float BTN_WIDTH = 700f;
    private const float BTN_HEIGHT = 120f;
    private const int BTN_BLUR = 20;

    void Start()
    {
        mainCanvas = FindObjectOfType<Canvas>();

        var canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.matchWidthOrHeight = 0f;
        }

        var dc = DataCarrier.Instance;
        selectedIcon = dc != null ? dc.playerIcon : DataCarrier.GetProfileIcon();

        BuildUI();
    }

    private float sparkleTimer = 0f;

    void Update()
    {
        if (koimikoshiGroup == null) return;

        sparkleTimer += Time.deltaTime;

        // Gentle pulsing glow (20% subtler)
        float pulse = 0.94f + 0.06f * Mathf.Sin(sparkleTimer * Mathf.PI);

        // Quick bright flash every 3 seconds
        float cycle = sparkleTimer % 3f;
        float flash = 0f;
        if (cycle < 0.15f)
            flash = Mathf.Sin(cycle / 0.15f * Mathf.PI) * 0.10f;

        koimikoshiGroup.alpha = Mathf.Clamp01(pulse + flash);

        // Slight scale bounce with the flash
        float s = 1f + flash * 0.24f;
        koimikoshiGroup.transform.localScale = new Vector3(s, s, 1f);
    }

    void BuildUI()
    {
        var dc = DataCarrier.Instance;

        // Background — same as TitleScene (#f3f7f8)
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(mainCanvas.transform, false);
        bgObj.transform.SetAsFirstSibling();
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.953f, 0.969f, 0.973f); // #f3f7f8
        bgImg.raycastTarget = false;

        // Player icon — centered, circular (160x160)
        float iconY = -160f;
        float iconSize = 160f;

        // Circle border ring (behind mask)
        var borderObj = new GameObject("IconBorder");
        borderObj.transform.SetParent(mainCanvas.transform, false);
        var borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 1f);
        borderRect.anchorMax = new Vector2(0.5f, 1f);
        borderRect.anchoredPosition = new Vector2(0, iconY);
        borderRect.sizeDelta = new Vector2(iconSize + 8, iconSize + 8);
        var borderImg = borderObj.AddComponent<Image>();
        borderImg.sprite = GetCircleSprite();
        borderImg.type = Image.Type.Simple;
        borderImg.color = new Color(0.82f, 0.82f, 0.82f);
        borderImg.raycastTarget = false;

        // Circle mask container (tappable → ProfileScene)
        var maskObj = new GameObject("IconMask");
        maskObj.transform.SetParent(mainCanvas.transform, false);
        var maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.5f, 1f);
        maskRect.anchorMax = new Vector2(0.5f, 1f);
        maskRect.anchoredPosition = new Vector2(0, iconY);
        maskRect.sizeDelta = new Vector2(iconSize, iconSize);
        var maskImg = maskObj.AddComponent<Image>();
        maskImg.sprite = GetCircleSprite();
        maskImg.type = Image.Type.Simple;
        maskImg.color = Color.white;
        var mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Make icon tappable
        var iconBtn = maskObj.AddComponent<Button>();
        iconBtn.targetGraphic = maskImg;
        maskImg.raycastTarget = true;
        iconBtn.onClick.AddListener(() => SceneManager.LoadScene("ProfileScene"));

        // Icon image inside mask
        var iconObj = new GameObject("PlayerIcon");
        iconObj.transform.SetParent(maskObj.transform, false);
        var iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        playerIconImg = iconObj.AddComponent<Image>();
        playerIconImg.raycastTarget = false;
        ApplyIconSprite(selectedIcon);

        // Player name (below icon)
        float nameY = iconY - iconSize / 2 - 30f;
        var nameObj = new GameObject("PlayerName");
        nameObj.transform.SetParent(mainCanvas.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0, nameY);
        nameRect.sizeDelta = new Vector2(800, 60);
        var nameText = nameObj.AddComponent<TextMeshProUGUI>();
        string pName = dc != null ? dc.playerName : DataCarrier.GetProfileName();
        nameText.text = string.IsNullOrEmpty(pName) ? "???" : pName;
        nameText.fontSize = 40;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = new Color(0.15f, 0.15f, 0.15f);
        nameText.raycastTarget = false;

        // Separator
        float sepY = nameY - 50f;
        var sepObj = new GameObject("Separator");
        sepObj.transform.SetParent(mainCanvas.transform, false);
        var sepRect = sepObj.AddComponent<RectTransform>();
        sepRect.anchorMin = new Vector2(0.5f, 1f);
        sepRect.anchorMax = new Vector2(0.5f, 1f);
        sepRect.anchoredPosition = new Vector2(0, sepY);
        sepRect.sizeDelta = new Vector2(900, 4);
        var sepImg = sepObj.AddComponent<Image>();
        sepImg.color = new Color(0.8f, 0.8f, 0.8f);
        sepImg.raycastTarget = false;

        // Koimikoshi image (same position/size as TitleScene logo) with borderRadius 32
        var koimikoshiSprite = Resources.Load<Sprite>("UI/koimikoshi");
        if (koimikoshiSprite != null)
        {
            var koimiContainer = new GameObject("KoimikoshiContainer");
            koimiContainer.transform.SetParent(mainCanvas.transform, false);
            var containerRect = koimiContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = new Vector2(0, 200);
            containerRect.sizeDelta = new Vector2(1040, 498);

            // Image (fills container)
            var koimiObj = new GameObject("KoimikoshiImg");
            koimiObj.transform.SetParent(koimiContainer.transform, false);
            var koimiRect = koimiObj.AddComponent<RectTransform>();
            koimiRect.anchorMin = Vector2.zero;
            koimiRect.anchorMax = Vector2.one;
            koimiRect.offsetMin = Vector2.zero;
            koimiRect.offsetMax = Vector2.zero;
            var koimiImg = koimiObj.AddComponent<Image>();
            koimiImg.sprite = koimikoshiSprite;
            koimiImg.preserveAspect = true;
            koimiImg.raycastTarget = false;

            // Corner overlays (background-colored rounded corners on top)
            Color bgColor = new Color(0.953f, 0.969f, 0.973f);
            CreateCornerOverlays(koimiContainer.transform, 32, bgColor);

            // CanvasGroup for sparkle pulse + scale animation
            koimikoshiGroup = koimiContainer.AddComponent<CanvasGroup>();
        }

        // Description text above koimikoshi
        var descObj = new GameObject("GachaDesc");
        descObj.transform.SetParent(mainCanvas.transform, false);
        var descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.5f, 0.5f);
        descRect.anchorMax = new Vector2(0.5f, 0.5f);
        descRect.anchoredPosition = new Vector2(0, 200 + 498f / 2f + 40f);
        descRect.sizeDelta = new Vector2(900, 50);
        var descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = Localization.Get("home_gacha_desc");
        descText.fontSize = 30;
        descText.alignment = TextAlignmentOptions.Center;
        descText.color = new Color(0.35f, 0.35f, 0.4f);
        descText.raycastTarget = false;

        // ===== "運命のガチャ" Button (same Y as TitleScene start button) =====
        CreateMeetButton(-220f);

        // ===== "babys" Button (same Y as TitleScene continue button) =====
        // babys button with gold border
        float babysY = -220f - BTN_HEIGHT - 40f;
        CreateBorderedPillButton(babysY, Localization.Get("home_babys"),
            new Color(0.85f, 0.65f, 0.13f), // gold border
            new Color(0.45f, 0.45f, 0.5f),  // text color
            () => SceneManager.LoadScene("BabysScene"));

        // ===== "縁の書" Button =====
        float enishiY = babysY - BTN_HEIGHT - 40f;
        CreateBorderedPillButton(enishiY, Localization.Get("home_enishi"),
            new Color(0.55f, 0.35f, 0.65f), // purple border
            new Color(0.45f, 0.45f, 0.5f),  // text color
            () => SceneManager.LoadScene("EnishiScene"));
    }

    // ===== Pill Buttons =====

    static readonly Color PINK = new Color(0.95f, 0.30f, 0.55f);

    void CreateMeetButton(float yPos)
    {
        int pillRadius = (int)(BTN_HEIGHT / 2);

        // Pink border (behind the button on canvas)
        var pinkBorderObj = new GameObject("MeetBorder");
        pinkBorderObj.transform.SetParent(mainCanvas.transform, false);
        var pinkBorderRect = pinkBorderObj.AddComponent<RectTransform>();
        pinkBorderRect.anchorMin = new Vector2(0.5f, 0.5f);
        pinkBorderRect.anchorMax = new Vector2(0.5f, 0.5f);
        pinkBorderRect.anchoredPosition = new Vector2(0, yPos);
        pinkBorderRect.sizeDelta = new Vector2(BTN_WIDTH + 8, BTN_HEIGHT + 8);
        var pinkBorderImg = pinkBorderObj.AddComponent<Image>();
        pinkBorderImg.sprite = GetPillSprite(pillRadius);
        pinkBorderImg.type = Image.Type.Sliced;
        pinkBorderImg.color = PINK;
        pinkBorderImg.raycastTarget = false;

        // White button (on top of border)
        var btnObj = new GameObject("MeetButton");
        btnObj.transform.SetParent(mainCanvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, yPos);
        btnRect.sizeDelta = new Vector2(BTN_WIDTH, BTN_HEIGHT);

        var btnBg = btnObj.AddComponent<Image>();
        btnBg.sprite = GetPillSprite(pillRadius);
        btnBg.type = Image.Type.Sliced;
        btnBg.color = Color.white;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(1f, 0.92f, 0.95f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        btn.onClick.AddListener(() =>
        {
            if (DataCarrier.Instance != null)
                DataCarrier.Instance.currentSlot = -1;
            SceneManager.LoadScene("BirthScene");
        });

        // Shadow
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(pinkBorderObj.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-BTN_BLUR, -BTN_BLUR - 4);
        shadowRect.offsetMax = new Vector2(BTN_BLUR, BTN_BLUR - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(pillRadius, BTN_BLUR);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0.95f, 0.30f, 0.55f, 0.15f);
        shadowImg.raycastTarget = false;

        // Heart icon + text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = "<color=#F24E80>\u2665</color>  " + Localization.Get("home_meet");
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = PINK;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;

        AddPressAnimation(btnObj);
    }

    void CreatePillButton(float yPos, string label, UnityEngine.Events.UnityAction onClick)
    {
        int pillRadius = (int)(BTN_HEIGHT / 2);

        var btnObj = new GameObject("PillButton");
        btnObj.transform.SetParent(mainCanvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, yPos);
        btnRect.sizeDelta = new Vector2(BTN_WIDTH, BTN_HEIGHT);

        var btnBg = btnObj.AddComponent<Image>();
        btnBg.sprite = GetPillSprite(pillRadius);
        btnBg.type = Image.Type.Sliced;
        btnBg.color = Color.white;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(btnObj.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-BTN_BLUR, -BTN_BLUR - 4);
        shadowRect.offsetMax = new Vector2(BTN_BLUR, BTN_BLUR - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(pillRadius, BTN_BLUR);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.18f);
        shadowImg.raycastTarget = false;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = label;
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = new Color(0.45f, 0.45f, 0.5f);
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;

        AddPressAnimation(btnObj);
    }

    void CreateBorderedPillButton(float yPos, string label, Color borderColor, Color textColor,
        UnityEngine.Events.UnityAction onClick)
    {
        int pillRadius = (int)(BTN_HEIGHT / 2);

        // Border (behind)
        var borderObj = new GameObject("PillBorder");
        borderObj.transform.SetParent(mainCanvas.transform, false);
        var borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = new Vector2(0, yPos);
        borderRect.sizeDelta = new Vector2(BTN_WIDTH + 8, BTN_HEIGHT + 8);
        var borderImg = borderObj.AddComponent<Image>();
        borderImg.sprite = GetPillSprite(pillRadius);
        borderImg.type = Image.Type.Sliced;
        borderImg.color = borderColor;
        borderImg.raycastTarget = false;

        // Shadow
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(borderObj.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-BTN_BLUR, -BTN_BLUR - 4);
        shadowRect.offsetMax = new Vector2(BTN_BLUR, BTN_BLUR - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(pillRadius, BTN_BLUR);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(borderColor.r, borderColor.g, borderColor.b, 0.15f);
        shadowImg.raycastTarget = false;

        // White button (on top)
        var btnObj = new GameObject("PillButton");
        btnObj.transform.SetParent(mainCanvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, yPos);
        btnRect.sizeDelta = new Vector2(BTN_WIDTH, BTN_HEIGHT);

        var btnBg = btnObj.AddComponent<Image>();
        btnBg.sprite = GetPillSprite(pillRadius);
        btnBg.type = Image.Type.Sliced;
        btnBg.color = Color.white;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.95f, 0.95f, 0.95f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        // Text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = label;
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = textColor;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;

        AddPressAnimation(btnObj);
    }

    // ===== Corner Overlays for border-radius =====

    void CreateCornerOverlays(Transform parent, int radius, Color bgColor)
    {
        Sprite cornerSprite = GetCornerSprite(radius);

        // Top-left
        CreateCorner(parent, cornerSprite, bgColor, radius,
            new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, false, false);
        // Top-right
        CreateCorner(parent, cornerSprite, bgColor, radius,
            new Vector2(1, 1), new Vector2(1, 1), Vector2.zero, true, false);
        // Bottom-left
        CreateCorner(parent, cornerSprite, bgColor, radius,
            new Vector2(0, 0), new Vector2(0, 0), Vector2.zero, false, true);
        // Bottom-right
        CreateCorner(parent, cornerSprite, bgColor, radius,
            new Vector2(1, 0), new Vector2(1, 0), Vector2.zero, true, true);
    }

    void CreateCorner(Transform parent, Sprite sprite, Color color, int radius,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, bool flipX, bool flipY)
    {
        var obj = new GameObject("Corner");
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(radius, radius);
        var img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        // Flip via scale
        rt.localScale = new Vector3(flipX ? -1 : 1, flipY ? -1 : 1, 1);
    }

    static Sprite _cornerSprite;

    static Sprite GetCornerSprite(int radius)
    {
        if (_cornerSprite != null) return _cornerSprite;
        int size = radius;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Distance from inner corner (radius, radius) = bottom-right of this quad
                float dx = radius - x - 0.5f;
                float dy = radius - y - 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // Outside the circle = background color (opaque), inside = transparent
                float alpha;
                if (dist >= radius + 1f)
                    alpha = 1f;
                else if (dist >= radius)
                    alpha = dist - radius;
                else
                    alpha = 0f;
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        _cornerSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0, 0), 100);
        return _cornerSprite;
    }

    void ApplyIconSprite(int index)
    {
        Sprite spr = Resources.Load<Sprite>($"Icons/icon_{index}");
        if (spr != null)
        {
            playerIconImg.sprite = spr;
            playerIconImg.color = Color.white;
            playerIconImg.preserveAspect = true;
        }
        else
        {
            // デフォルト: kayo.png
            Sprite defaultSpr = Resources.Load<Sprite>("Icons/kayo");
            if (defaultSpr != null)
            {
                playerIconImg.sprite = defaultSpr;
                playerIconImg.color = Color.white;
                playerIconImg.preserveAspect = true;
            }
            else
            {
                playerIconImg.sprite = null;
                playerIconImg.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }

    // ===== Circle Sprite =====

    static Sprite GetCircleSprite()
    {
        if (_circleSprite != null) return _circleSprite;

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius - dist);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100);
        return _circleSprite;
    }

    // ===== Pill / Shadow Sprites (same as TitleScene) =====

    static Sprite GetPillSprite(int radius)
    {
        if (_pillSprite != null) return _pillSprite;
        int size = radius * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x - center;
                float py = y - center;
                float dist = Mathf.Sqrt(px * px + py * py) - radius;
                if (dist <= -1f) tex.SetPixel(x, y, Color.white);
                else if (dist <= 0f) tex.SetPixel(x, y, new Color(1, 1, 1, -dist));
                else tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        tex.Apply();
        var border = new Vector4(radius, radius, radius, radius);
        _pillSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _pillSprite;
    }

    static Sprite GetShadowSprite(int radius, int blur)
    {
        if (_shadowSprite != null) return _shadowSprite;
        int size = (radius + blur) * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x - center;
                float py = y - center;
                float dist = Mathf.Sqrt(px * px + py * py) - radius;
                float alpha;
                if (dist <= 0f) alpha = 0f;
                else if (dist >= blur) alpha = 0f;
                else { float t = dist / blur; alpha = (1f - t) * (1f - t); }
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        int borderVal = radius + blur;
        var border = new Vector4(borderVal, borderVal, borderVal, borderVal);
        _shadowSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _shadowSprite;
    }

    void AddPressAnimation(GameObject button)
    {
        var trigger = button.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((d) => button.transform.localScale = new Vector3(0.95f, 0.95f, 1f));
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((d) => button.transform.localScale = Vector3.one);
        trigger.triggers.Add(up);

        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((d) => button.transform.localScale = Vector3.one);
        trigger.triggers.Add(exit);
    }

}
