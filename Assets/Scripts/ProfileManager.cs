using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ProfileManager : MonoBehaviour
{
    private Canvas mainCanvas;
    private int selectedIcon = 0;

    private static readonly string[] iconNames = new string[]
    {
        "kayo", "ikemen", "inteli", "matcho", "old-women", "sexy-lady"
    };

    private Image avatarImg;
    private GameObject iconPickerPanel;

    private TMP_InputField nameInputField;
    private Button startButton;
    private Image startButtonBg;
    private TextMeshProUGUI startButtonText;

    private bool isEditing;

    private static readonly Color[] iconColors = new Color[]
    {
        new Color(0.90f, 0.30f, 0.30f), // Red
        new Color(0.30f, 0.50f, 0.90f), // Blue
        new Color(0.30f, 0.80f, 0.40f), // Green
        new Color(0.60f, 0.35f, 0.85f), // Purple
        new Color(0.95f, 0.60f, 0.20f), // Orange
        new Color(0.95f, 0.45f, 0.65f), // Pink
    };

    private const float BTN_WIDTH = 700f;
    private const float BTN_HEIGHT = 120f;
    private const int BTN_BLUR = 20;
    private static Sprite _pillSprite;
    private static Sprite _shadowSprite;
    private static Sprite _circleSprite;

    void Start()
    {
        mainCanvas = FindObjectOfType<Canvas>();

        var canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.matchWidthOrHeight = 0f;
        }

        isEditing = DataCarrier.HasProfile();

        BuildUI();

        if (isEditing)
        {
            string existingName = DataCarrier.GetProfileName();
            int existingIcon = DataCarrier.GetProfileIcon();
            if (nameInputField != null && !string.IsNullOrEmpty(existingName))
                nameInputField.text = existingName;
            SelectIcon(existingIcon);
        }
        else
        {
            SelectIcon(0);
        }

        UpdateStartButton();
    }

    void BuildUI()
    {
        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(mainCanvas.transform, false);
        bgObj.transform.SetAsFirstSibling();
        var bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.953f, 0.969f, 0.973f);
        bgImg.raycastTarget = false;

        // Avatar display (large circular icon with edit badge)
        CreateAvatarDisplay();

        // Name label
        var labelObj = new GameObject("NameLabel");
        labelObj.transform.SetParent(mainCanvas.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0, -20);
        labelRect.sizeDelta = new Vector2(800, 60);
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(labelText);
        labelText.text = Localization.Get("profile_name_label");
        labelText.fontSize = 42;
        labelText.fontStyle = FontStyles.Bold;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = new Color(0.05f, 0.05f, 0.08f);
        labelText.raycastTarget = false;

        // Name input field
        CreateNameInput();

        // Start button
        CreateStartButton();
    }

    void CreateAvatarDisplay()
    {
        float avatarSize = 220f;
        float avatarY = 400f;

        // Tappable area
        var avatarBtnObj = new GameObject("AvatarButton");
        avatarBtnObj.transform.SetParent(mainCanvas.transform, false);
        var avatarBtnRect = avatarBtnObj.AddComponent<RectTransform>();
        avatarBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
        avatarBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
        avatarBtnRect.anchoredPosition = new Vector2(0, avatarY);
        avatarBtnRect.sizeDelta = new Vector2(avatarSize + 16, avatarSize + 16);
        var avatarBtnImg = avatarBtnObj.AddComponent<Image>();
        avatarBtnImg.color = new Color(0, 0, 0, 0);
        var avatarBtn = avatarBtnObj.AddComponent<Button>();
        avatarBtn.targetGraphic = avatarBtnImg;
        avatarBtn.onClick.AddListener(ToggleIconPicker);

        // Circle mask
        var maskObj = new GameObject("AvatarMask");
        maskObj.transform.SetParent(avatarBtnObj.transform, false);
        var maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.5f, 0.5f);
        maskRect.anchorMax = new Vector2(0.5f, 0.5f);
        maskRect.anchoredPosition = Vector2.zero;
        maskRect.sizeDelta = new Vector2(avatarSize, avatarSize);
        var maskImg = maskObj.AddComponent<Image>();
        maskImg.sprite = GetCircleSprite();
        maskImg.color = Color.white;
        maskImg.raycastTarget = false;
        var mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Avatar image inside mask
        var imgObj = new GameObject("AvatarImage");
        imgObj.transform.SetParent(maskObj.transform, false);
        var imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
        avatarImg = imgObj.AddComponent<Image>();
        avatarImg.raycastTarget = false;
        avatarImg.preserveAspect = true;

        // Load default kayo.png
        ApplyAvatarSprite(0);

        // Edit badge (bottom-right, on the circle edge)
        float badgeSize = 56f;
        float badgeAngle = -45f * Mathf.Deg2Rad; // 右下45度
        float badgeDist = avatarSize / 2f - badgeSize / 4f;
        var badgeObj = new GameObject("EditBadge");
        badgeObj.transform.SetParent(avatarBtnObj.transform, false);
        var badgeRect = badgeObj.AddComponent<RectTransform>();
        badgeRect.anchorMin = new Vector2(0.5f, 0.5f);
        badgeRect.anchorMax = new Vector2(0.5f, 0.5f);
        badgeRect.anchoredPosition = new Vector2(Mathf.Cos(badgeAngle) * badgeDist, Mathf.Sin(badgeAngle) * badgeDist);
        badgeRect.sizeDelta = new Vector2(badgeSize, badgeSize);
        var badgeBg = badgeObj.AddComponent<Image>();
        badgeBg.sprite = GetCircleSprite();
        badgeBg.color = new Color(0.3f, 0.5f, 0.9f);
        badgeBg.raycastTarget = false;

        // Edit icon (pencil unicode)
        var penObj = new GameObject("EditIcon");
        penObj.transform.SetParent(badgeObj.transform, false);
        var penRect = penObj.AddComponent<RectTransform>();
        penRect.anchorMin = Vector2.zero;
        penRect.anchorMax = Vector2.one;
        penRect.offsetMin = Vector2.zero;
        penRect.offsetMax = Vector2.zero;
        var penText = penObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(penText);
        penText.text = "\u270E";
        penText.fontSize = 28;
        penText.alignment = TextAlignmentOptions.Center;
        penText.color = Color.white;
        penText.raycastTarget = false;
    }

    void ApplyAvatarSprite(int index)
    {
        string name = (index >= 0 && index < iconNames.Length) ? iconNames[index] : iconNames[0];
        Sprite spr = Resources.Load<Sprite>($"Icons/{name}");

        if (spr != null)
        {
            avatarImg.sprite = spr;
            avatarImg.color = Color.white;
        }
        else
        {
            avatarImg.sprite = null;
            avatarImg.color = iconColors[Mathf.Clamp(index, 0, iconColors.Length - 1)];
        }
    }

    void ToggleIconPicker()
    {
        if (iconPickerPanel != null)
        {
            Destroy(iconPickerPanel);
            iconPickerPanel = null;
            return;
        }
        ShowIconPicker();
    }

    void ShowIconPicker()
    {
        // 全画面オーバーレイ
        iconPickerPanel = new GameObject("IconPickerPanel");
        iconPickerPanel.transform.SetParent(mainCanvas.transform, false);
        var overlayRect = iconPickerPanel.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayBg = iconPickerPanel.AddComponent<Image>();
        overlayBg.color = new Color(0f, 0f, 0f, 0.5f);

        // 白カード
        var card = new GameObject("Card");
        card.transform.SetParent(iconPickerPanel.transform, false);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = new Vector2(0, 50);
        cardRect.sizeDelta = new Vector2(900, 700);
        var cardBg = card.AddComponent<Image>();
        cardBg.sprite = GetPillSprite(24);
        cardBg.type = Image.Type.Sliced;
        cardBg.color = Color.white;

        // Title
        var titleObj = new GameObject("PickerTitle");
        titleObj.transform.SetParent(card.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(0, 60);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = Localization.Get("profile_icon_select");
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.25f, 0.25f, 0.3f);
        titleText.raycastTarget = false;

        // 2x3 grid of circular icons
        float iconSize = 140f;
        float gapX = 40f;
        float gapY = 30f;
        float totalWidth = iconSize * 3 + gapX * 2;
        float startX = -totalWidth / 2f + iconSize / 2f;
        float gridStartY = -110f;

        for (int i = 0; i < iconNames.Length; i++)
        {
            int col = i % 3;
            int row = i / 3;
            float x = startX + col * (iconSize + gapX);
            float y = gridStartY - row * (iconSize + gapY);
            CreatePickerIcon(card.transform, i, x, y, iconSize);
        }

        // ボタンエリア（保存・閉じる横並び）
        float btnW = 350f;
        float btnH = 90f;
        int pillRadius = (int)(btnH / 2);
        int blur = 20;
        float btnGap = 30f;
        float btnY = 75f;

        // 保存ボタン
        CreatePillButton(card.transform, Localization.Get("profile_save"),
            new Vector2(-btnW / 2f - btnGap / 2f, btnY), new Vector2(btnW, btnH),
            pillRadius, blur, () =>
            {
                if (DataCarrier.Instance != null)
                {
                    DataCarrier.Instance.playerIcon = selectedIcon;
                    DataCarrier.Instance.SaveProfile();
                }
                Destroy(iconPickerPanel);
                iconPickerPanel = null;
                StartCoroutine(ShowToast(Localization.Get("profile_icon_saved")));
            });

        // 閉じるボタン
        CreatePillButton(card.transform, Localization.Get("ui_close"),
            new Vector2(btnW / 2f + btnGap / 2f, btnY), new Vector2(btnW, btnH),
            pillRadius, blur, () =>
            {
                Destroy(iconPickerPanel);
                iconPickerPanel = null;
            });
    }

    void CreatePillButton(Transform parent, string label, Vector2 pos, Vector2 size,
        int pillRadius, int blur, UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = new GameObject("PillButton");
        btnObj.transform.SetParent(parent, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0);
        btnRect.anchorMax = new Vector2(0.5f, 0);
        btnRect.anchoredPosition = pos;
        btnRect.sizeDelta = size;

        var btnBg = btnObj.AddComponent<Image>();
        btnBg.sprite = GetPillSprite(pillRadius);
        btnBg.type = Image.Type.Sliced;
        btnBg.color = Color.white;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        btn.onClick.AddListener(onClick);
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        // Shadow
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(btnObj.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-blur, -blur - 4);
        shadowRect.offsetMax = new Vector2(blur, blur - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(pillRadius, blur);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.18f);
        shadowImg.raycastTarget = false;

        // Text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(tmp);
        tmp.text = label;
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.45f, 0.45f, 0.5f);
        tmp.raycastTarget = false;

        // Press animation
        var trigger = btnObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => {
            btnObj.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        });
        trigger.triggers.Add(pointerDown);
        var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => {
            btnObj.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerUp);
        var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) => {
            btnObj.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerExit);
    }

    IEnumerator ShowToast(string message)
    {
        float toastW = 700f;
        float toastH = 100f;
        int toastPill = (int)(toastH / 2);
        int toastBlur = 20;

        var toastObj = new GameObject("Toast");
        toastObj.transform.SetParent(mainCanvas.transform, false);
        var toastRect = toastObj.AddComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 0);
        toastRect.anchorMax = new Vector2(0.5f, 0);
        toastRect.anchoredPosition = new Vector2(0, 200);
        toastRect.sizeDelta = new Vector2(toastW, toastH);
        var toastBg = toastObj.AddComponent<Image>();
        toastBg.sprite = GetPillSprite(toastPill);
        toastBg.type = Image.Type.Sliced;
        toastBg.color = Color.white;

        // Shadow
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(toastObj.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-toastBlur, -toastBlur - 4);
        shadowRect.offsetMax = new Vector2(toastBlur, toastBlur - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(toastPill, toastBlur);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.18f);
        shadowImg.raycastTarget = false;

        // Text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(toastObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(tmp);
        tmp.text = message;
        tmp.fontSize = 32;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.45f, 0.45f, 0.5f);
        tmp.raycastTarget = false;

        yield return new WaitForSeconds(2f);
        Destroy(toastObj);
    }

    void CreatePickerIcon(Transform parent, int index, float x, float y, float size)
    {
        var containerObj = new GameObject($"PickerIcon_{index}");
        containerObj.transform.SetParent(parent, false);
        var containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1);
        containerRect.anchorMax = new Vector2(0.5f, 1);
        containerRect.anchoredPosition = new Vector2(x, y);
        containerRect.sizeDelta = new Vector2(size, size);

        // Circle border
        var borderImg = containerObj.AddComponent<Image>();
        borderImg.sprite = GetCircleSprite();
        borderImg.color = (index == selectedIcon)
            ? new Color(1f, 0.84f, 0f) // gold
            : new Color(0.82f, 0.82f, 0.85f);

        // Mask for circular clipping
        var maskObj = new GameObject("Mask");
        maskObj.transform.SetParent(containerObj.transform, false);
        var maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.5f, 0.5f);
        maskRect.anchorMax = new Vector2(0.5f, 0.5f);
        maskRect.anchoredPosition = Vector2.zero;
        maskRect.sizeDelta = new Vector2(size - 8, size - 8);
        var maskImg = maskObj.AddComponent<Image>();
        maskImg.sprite = GetCircleSprite();
        maskImg.color = Color.white;
        maskImg.raycastTarget = false;
        var mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Icon image
        var imgObj = new GameObject("Image");
        imgObj.transform.SetParent(maskObj.transform, false);
        var imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
        var img = imgObj.AddComponent<Image>();
        img.raycastTarget = false;
        img.preserveAspect = true;

        string iconName = (index >= 0 && index < iconNames.Length) ? iconNames[index] : iconNames[0];
        Sprite spr = Resources.Load<Sprite>($"Icons/{iconName}");

        if (spr != null)
        {
            img.sprite = spr;
            img.color = Color.white;
        }
        else
        {
            img.sprite = null;
            img.color = iconColors[Mathf.Clamp(index, 0, iconColors.Length - 1)];
            mask.showMaskGraphic = true;
            maskImg.color = img.color;
            img.color = new Color(1, 1, 1, 0);

            // Number label
            var numObj = new GameObject("Number");
            numObj.transform.SetParent(maskObj.transform, false);
            var numRect = numObj.AddComponent<RectTransform>();
            numRect.anchorMin = Vector2.zero;
            numRect.anchorMax = Vector2.one;
            numRect.offsetMin = Vector2.zero;
            numRect.offsetMax = Vector2.zero;
            var numText = numObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(numText);
            numText.text = (index + 1).ToString();
            numText.fontSize = 36;
            numText.fontStyle = FontStyles.Bold;
            numText.alignment = TextAlignmentOptions.Center;
            numText.color = Color.white;
            numText.raycastTarget = false;
        }

        var btn = containerObj.AddComponent<Button>();
        btn.targetGraphic = borderImg;
        int idx = index;
        btn.onClick.AddListener(() =>
        {
            SelectIcon(idx);
            // 選択状態を更新するためにピッカーを再構築
            Destroy(iconPickerPanel);
            iconPickerPanel = null;
            ShowIconPicker();
        });
    }

    void SelectIcon(int index)
    {
        selectedIcon = index;
        ApplyAvatarSprite(index);
    }

    void CreateNameInput()
    {
        var inputObj = new GameObject("NameInputField");
        inputObj.transform.SetParent(mainCanvas.transform, false);
        var inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, 0.5f);
        inputRect.anchorMax = new Vector2(0.5f, 0.5f);
        inputRect.anchoredPosition = new Vector2(0, -130);
        inputRect.sizeDelta = new Vector2(800, 100);

        var inputBg = inputObj.AddComponent<Image>();
        inputBg.color = Color.white;

        nameInputField = inputObj.AddComponent<TMP_InputField>();
        nameInputField.characterLimit = 12;

        var textAreaObj = new GameObject("TextArea");
        textAreaObj.transform.SetParent(inputObj.transform, false);
        var textAreaRect = textAreaObj.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        textAreaRect.offsetMin = new Vector2(20, 5);
        textAreaRect.offsetMax = new Vector2(-20, -5);
        textAreaObj.AddComponent<RectMask2D>();

        var inputTextObj = new GameObject("Text");
        inputTextObj.transform.SetParent(textAreaObj.transform, false);
        var inputTextRect = inputTextObj.AddComponent<RectTransform>();
        inputTextRect.anchorMin = Vector2.zero;
        inputTextRect.anchorMax = Vector2.one;
        inputTextRect.offsetMin = Vector2.zero;
        inputTextRect.offsetMax = Vector2.zero;
        var inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(inputText);
        inputText.fontSize = 44;
        inputText.alignment = TextAlignmentOptions.Left;
        inputText.color = Color.black;

        var placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(textAreaObj.transform, false);
        var placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(placeholderText);
        placeholderText.text = Localization.Get("profile_name_placeholder");
        placeholderText.fontSize = 32;
        placeholderText.alignment = TextAlignmentOptions.Left;
        placeholderText.color = new Color(0.6f, 0.6f, 0.6f);
        placeholderText.fontStyle = FontStyles.Italic;

        nameInputField.textComponent = inputText;
        nameInputField.textViewport = textAreaRect;
        nameInputField.placeholder = placeholderText;

        nameInputField.onValueChanged.AddListener((string val) => UpdateStartButton());
    }

    void CreateStartButton()
    {
        int pillRadius = (int)(BTN_HEIGHT / 2);

        var shadowObj = new GameObject("StartButtonShadow");
        shadowObj.transform.SetParent(mainCanvas.transform, false);
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = new Vector2(0.5f, 0.5f);
        shadowRect.anchorMax = new Vector2(0.5f, 0.5f);
        shadowRect.anchoredPosition = new Vector2(0, -400 - 2);
        shadowRect.sizeDelta = new Vector2(BTN_WIDTH + BTN_BLUR * 2, BTN_HEIGHT + BTN_BLUR * 2);

        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(pillRadius, BTN_BLUR);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.18f);
        shadowImg.raycastTarget = false;

        var btnObj = new GameObject("StartButton");
        btnObj.transform.SetParent(mainCanvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, -400);
        btnRect.sizeDelta = new Vector2(BTN_WIDTH, BTN_HEIGHT);

        startButtonBg = btnObj.AddComponent<Image>();
        startButtonBg.sprite = GetPillSprite(pillRadius);
        startButtonBg.type = Image.Type.Sliced;
        startButtonBg.color = Color.white;

        startButton = btnObj.AddComponent<Button>();
        startButton.onClick.AddListener(OnStartPressed);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        startButtonText = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(startButtonText);
        startButtonText.text = isEditing ? Localization.Get("profile_save") : Localization.Get("profile_start");
        startButtonText.fontSize = 36;
        startButtonText.fontStyle = FontStyles.Bold;
        startButtonText.alignment = TextAlignmentOptions.Center;
        startButtonText.color = new Color(0.45f, 0.45f, 0.5f);
        startButtonText.raycastTarget = false;
    }

    void UpdateStartButton()
    {
        bool hasName = nameInputField != null && !string.IsNullOrEmpty(nameInputField.text.Trim());
        if (startButton != null)
            startButton.interactable = hasName;
        if (startButtonBg != null)
            startButtonBg.color = hasName ? Color.white : new Color(0.85f, 0.85f, 0.85f);
        if (startButtonText != null)
            startButtonText.color = hasName ? new Color(0.45f, 0.45f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
    }

    void OnStartPressed()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.playerName = nameInputField.text.Trim();
            DataCarrier.Instance.playerIcon = selectedIcon;
            DataCarrier.Instance.SaveProfile();
        }

        if (isEditing)
            SceneManager.LoadScene("HomeScene");
        else
            StartCoroutine(CutsceneSequence());
    }

    IEnumerator CutsceneSequence()
    {
        // Disable start button to prevent double tap
        if (startButton != null)
            startButton.interactable = false;

        string[] lines = new string[]
        {
            Localization.Get("cutscene_line1"),
            Localization.Get("cutscene_line2"),
        };

        // Full-screen dark overlay
        var panel = new GameObject("CutscenePanel");
        panel.transform.SetParent(mainCanvas.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0f);

        // Fade in
        float elapsed = 0f;
        float fadeInDuration = 0.6f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            panelImg.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.92f, elapsed / fadeInDuration));
            yield return null;
        }
        panelImg.color = new Color(0f, 0f, 0f, 0.92f);

        yield return new WaitForSeconds(0.4f);

        // Show lines one by one with slide-in
        float slideFrom = -600f;
        float slideDuration = 1.2f;
        float lineSpacing = 140f;
        float startY = 80f;

        for (int i = 0; i < lines.Length; i++)
        {
            var textObj = new GameObject($"CutsceneText_{i}");
            textObj.transform.SetParent(panel.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(900f, 120f);
            float yPos = startY - i * lineSpacing;
            textRect.anchoredPosition = new Vector2(slideFrom, yPos);

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(tmp);
            tmp.text = lines[i];
            tmp.fontSize = 38;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;

            // Slide in
            float slideElapsed = 0f;
            Vector2 from = new Vector2(slideFrom, yPos);
            Vector2 to = new Vector2(0f, yPos);
            while (slideElapsed < slideDuration)
            {
                slideElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, slideElapsed / slideDuration);
                textRect.anchoredPosition = Vector2.Lerp(from, to, t);
                yield return null;
            }
            textRect.anchoredPosition = to;

            if (i < lines.Length - 1)
                yield return new WaitForSeconds(0.8f);
        }

        yield return new WaitForSeconds(2.0f);

        // Fade out
        var cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        elapsed = 0f;
        float fadeOutDuration = 0.8f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        SceneManager.LoadScene("BirthScene");
    }

    // ===== Sprite helpers =====

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

    static Sprite GetPillSprite(int radius)
    {
        if (_pillSprite != null) return _pillSprite;

        int size = radius * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center + 0.5f);
                float dy = Mathf.Abs(y - center + 0.5f);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius + 0.5f - dist);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
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
        float center = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x - center;
                float py = y - center;
                float dist = Mathf.Sqrt(px * px + py * py) - radius;

                float alpha;
                if (dist <= 0)
                    alpha = 1f;
                else if (dist >= blur)
                    alpha = 0f;
                else
                    alpha = 1f - (dist / blur);

                alpha *= alpha;
                tex.SetPixel(x, y, new Color(0, 0, 0, alpha));
            }
        }
        tex.Apply();

        int borderVal = radius + blur;
        var border = new Vector4(borderVal, borderVal, borderVal, borderVal);
        _shadowSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _shadowSprite;
    }
}
