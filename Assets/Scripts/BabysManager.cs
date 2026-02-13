using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BabysManager : MonoBehaviour
{
    private Canvas mainCanvas;
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

        BuildUI();
    }

    void BuildUI()
    {
        var dc = DataCarrier.Instance;

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

        // Title
        float titleY = -164f;
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainCanvas.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, titleY);
        titleRect.sizeDelta = new Vector2(800, 70);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = "babys";
        titleText.fontSize = 44;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.15f, 0.15f, 0.15f);
        titleText.raycastTarget = false;

        // Back button
        float backY = -164f;
        var backObj = new GameObject("BackButton");
        backObj.transform.SetParent(mainCanvas.transform, false);
        var backRect = backObj.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 1f);
        backRect.anchorMax = new Vector2(0, 1f);
        backRect.anchoredPosition = new Vector2(80, backY);
        backRect.sizeDelta = new Vector2(100, 60);
        var backImg = backObj.AddComponent<Image>();
        backImg.color = new Color(0, 0, 0, 0);
        var backBtn = backObj.AddComponent<Button>();
        backBtn.targetGraphic = backImg;
        backBtn.onClick.AddListener(() => SceneManager.LoadScene("HomeScene"));
        var backTextObj = new GameObject("Text");
        backTextObj.transform.SetParent(backObj.transform, false);
        var backTextRect = backTextObj.AddComponent<RectTransform>();
        backTextRect.anchorMin = Vector2.zero;
        backTextRect.anchorMax = Vector2.one;
        backTextRect.offsetMin = Vector2.zero;
        backTextRect.offsetMax = Vector2.zero;
        var backText = backTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(backText);
        backText.text = "< " + Localization.Get("ui_back");
        backText.fontSize = 28;
        backText.alignment = TextAlignmentOptions.Center;
        backText.color = new Color(0.3f, 0.5f, 0.9f);
        backText.fontStyle = FontStyles.Bold;
        backText.raycastTarget = false;

        // Info button (top-right)
        var infoObj = new GameObject("InfoButton");
        infoObj.transform.SetParent(mainCanvas.transform, false);
        var infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(1, 1f);
        infoRect.anchorMax = new Vector2(1, 1f);
        infoRect.anchoredPosition = new Vector2(-80, backY);
        infoRect.sizeDelta = new Vector2(60, 60);
        var infoBg = infoObj.AddComponent<Image>();
        infoBg.sprite = GetCircleSprite();
        infoBg.color = new Color(0.3f, 0.5f, 0.9f);
        var infoBtn = infoObj.AddComponent<Button>();
        infoBtn.targetGraphic = infoBg;
        infoBtn.onClick.AddListener(ShowStorySelect);
        var infoTextObj = new GameObject("Text");
        infoTextObj.transform.SetParent(infoObj.transform, false);
        var infoTextRect = infoTextObj.AddComponent<RectTransform>();
        infoTextRect.anchorMin = Vector2.zero;
        infoTextRect.anchorMax = Vector2.one;
        infoTextRect.offsetMin = Vector2.zero;
        infoTextRect.offsetMax = Vector2.zero;
        var infoText = infoTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(infoText);
        infoText.text = "i";
        infoText.fontSize = 32;
        infoText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.color = Color.white;
        infoText.raycastTarget = false;

        // ScrollView
        float scrollTop = titleY - 70f;
        var scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(mainCanvas.transform, false);
        var scrollViewRect = scrollObj.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0);
        scrollViewRect.anchorMax = new Vector2(1, 1);
        scrollViewRect.offsetMin = new Vector2(0, 0);
        scrollViewRect.offsetMax = new Vector2(0, scrollTop);
        scrollObj.AddComponent<RectMask2D>();
        var scrollView = scrollObj.AddComponent<ScrollRect>();
        scrollView.horizontal = false;
        scrollView.vertical = true;
        scrollView.movementType = ScrollRect.MovementType.Elastic;
        scrollView.elasticity = 0.1f;

        // Content container
        var contentContainer = new GameObject("Content");
        contentContainer.transform.SetParent(scrollObj.transform, false);
        var contentContainerRect = contentContainer.AddComponent<RectTransform>();
        contentContainerRect.anchorMin = new Vector2(0, 1);
        contentContainerRect.anchorMax = new Vector2(1, 1);
        contentContainerRect.pivot = new Vector2(0.5f, 1);
        contentContainerRect.anchoredPosition = Vector2.zero;

        scrollView.content = contentContainerRect;

        // Save slot list
        float cardHeight = 560f;
        float slotGap = 30f;
        float yOffset = -20f;
        int currentSlot = dc != null ? dc.currentSlot : -1;

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            bool exists = DataCarrier.SlotExists(i);
            float thisHeight = exists ? cardHeight : 100f;
            CreateSlotEntry(contentContainer.transform, i, yOffset, currentSlot == i);
            yOffset -= thisHeight + slotGap;
        }

        contentContainerRect.sizeDelta = new Vector2(0, -yOffset);
    }

    void CreateSlotEntry(Transform parent, int slot, float yPos, bool isCurrent)
    {
        bool exists = DataCarrier.SlotExists(slot);
        float imgSize = 280f;
        float cardHeight = exists ? 560f : 100f;

        var cardObj = new GameObject($"SlotCard{slot}");
        cardObj.transform.SetParent(parent, false);
        var cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.anchoredPosition = new Vector2(0, yPos - cardHeight / 2f);
        cardRect.sizeDelta = new Vector2(900, cardHeight);

        var cardImg = cardObj.AddComponent<Image>();
        if (isCurrent)
            cardImg.color = new Color(0.85f, 0.92f, 1f);
        else
            cardImg.color = exists ? new Color(0.93f, 0.93f, 0.95f) : new Color(0.88f, 0.88f, 0.88f);

        if (exists)
        {
            var btn = cardObj.AddComponent<Button>();
            btn.targetGraphic = cardImg;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.80f, 0.88f, 1f);
            colors.pressedColor = new Color(0.70f, 0.82f, 0.95f);
            btn.colors = colors;
            int slotIndex = slot;
            btn.onClick.AddListener(() => LoadSlot(slotIndex));
        }
        else
        {
            cardImg.raycastTarget = false;
        }

        if (exists)
        {
            string fatherName = DataCarrier.GetSlotFatherName(slot);
            string motherName = DataCarrier.GetSlotMotherName(slot);
            string babyGender = DataCarrier.GetSlotBabyGender(slot);
            string babyName = DataCarrier.GetSlotBabyName(slot);
            int age = DataCarrier.GetSlotAge(slot);
            bool isGod = DataCarrier.GetSlotIsGodBaby(slot);
            string godMark = isGod ? "  <color=#FFD700>\u2605GOD BABY\u2605</color>" : "";
            string p = $"slot{slot}_";

            // Baby image (280x280) with circle mask — same size as EnishiManager
            float imgY = cardHeight / 2f - 30f - imgSize / 2f;

            var borderObj = new GameObject("ImgBorder");
            borderObj.transform.SetParent(cardObj.transform, false);
            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0.5f, 0.5f);
            borderRect.anchorMax = new Vector2(0.5f, 0.5f);
            borderRect.anchoredPosition = new Vector2(0, imgY);
            borderRect.sizeDelta = new Vector2(imgSize + 6, imgSize + 6);
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.sprite = GetCircleSprite();
            borderImg.type = Image.Type.Simple;
            borderImg.color = new Color(0.82f, 0.82f, 0.82f);
            borderImg.raycastTarget = false;

            var babyMaskObj = new GameObject("BabyMask");
            babyMaskObj.transform.SetParent(cardObj.transform, false);
            var babyMaskRect = babyMaskObj.AddComponent<RectTransform>();
            babyMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
            babyMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
            babyMaskRect.anchoredPosition = new Vector2(0, imgY);
            babyMaskRect.sizeDelta = new Vector2(imgSize, imgSize);
            var babyMaskImg = babyMaskObj.AddComponent<Image>();
            babyMaskImg.sprite = GetCircleSprite();
            babyMaskImg.type = Image.Type.Simple;
            babyMaskImg.color = Color.white;
            babyMaskImg.raycastTarget = false;
            var babyMask = babyMaskObj.AddComponent<Mask>();
            babyMask.showMaskGraphic = false;

            var babyIconObj = new GameObject("BabyIcon");
            babyIconObj.transform.SetParent(babyMaskObj.transform, false);
            var babyIconRect = babyIconObj.AddComponent<RectTransform>();
            babyIconRect.anchorMin = Vector2.zero;
            babyIconRect.anchorMax = Vector2.one;
            babyIconRect.offsetMin = Vector2.zero;
            babyIconRect.offsetMax = Vector2.zero;
            var babyIconImg = babyIconObj.AddComponent<Image>();
            babyIconImg.raycastTarget = false;

            string genderKey = babyGender == "\u7537\u306e\u5b50" ? "male" : "female";
            string fatherKey = GetParentImageName(fatherName);
            string motherKey = GetParentImageName(motherName);
            string babyImagePath = $"babys/{fatherKey}_{motherKey}_{genderKey}";
            Sprite babySprite = Resources.Load<Sprite>(babyImagePath);

            if (babySprite != null)
            {
                babyIconImg.sprite = babySprite;
                babyIconImg.color = Color.white;
                babyIconImg.preserveAspect = true;
            }
            else
            {
                babyIconImg.color = babyGender == "\u7537\u306e\u5b50"
                    ? new Color(0f, 0.75f, 1f, 0.3f)
                    : new Color(1f, 0.41f, 0.71f, 0.3f);
                babyMask.showMaskGraphic = true;
                babyMaskImg.color = babyIconImg.color;
                babyIconImg.color = new Color(1, 1, 1, 0);
            }

            // NOW badge
            if (isCurrent)
            {
                var nowObj = new GameObject("NowBadge");
                nowObj.transform.SetParent(cardObj.transform, false);
                var nowRect = nowObj.AddComponent<RectTransform>();
                nowRect.anchorMin = new Vector2(1, 1);
                nowRect.anchorMax = new Vector2(1, 1);
                nowRect.anchoredPosition = new Vector2(-60, -15);
                nowRect.sizeDelta = new Vector2(90, 36);
                var nowBg = nowObj.AddComponent<Image>();
                nowBg.sprite = GetCircleSprite();
                nowBg.color = new Color(0.20f, 0.50f, 1f);
                nowBg.raycastTarget = false;

                var nowTextObj = new GameObject("Text");
                nowTextObj.transform.SetParent(nowObj.transform, false);
                var nowTextRect = nowTextObj.AddComponent<RectTransform>();
                nowTextRect.anchorMin = Vector2.zero;
                nowTextRect.anchorMax = Vector2.one;
                nowTextRect.offsetMin = Vector2.zero;
                nowTextRect.offsetMax = Vector2.zero;
                var nowText = nowTextObj.AddComponent<TextMeshProUGUI>();
                FontHelper.Apply(nowText);
                nowText.text = "NOW";
                nowText.fontSize = 20;
                nowText.fontStyle = FontStyles.Bold;
                nowText.alignment = TextAlignmentOptions.Center;
                nowText.color = Color.white;
                nowText.raycastTarget = false;
            }

            // Baby name + age + parents
            float infoY = imgY - imgSize / 2f - 20f;
            var nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(cardObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 0.5f);
            nameRect.anchorMax = new Vector2(0.5f, 0.5f);
            nameRect.anchoredPosition = new Vector2(0, infoY);
            nameRect.sizeDelta = new Vector2(800, 45);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(nameText);
            nameText.text = $"<b>{babyName}</b>{godMark}";
            nameText.fontSize = 34;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = new Color(0.2f, 0.2f, 0.2f);
            nameText.richText = true;
            nameText.raycastTarget = false;

            float subInfoY = infoY - 38f;
            var subObj = new GameObject("SubInfo");
            subObj.transform.SetParent(cardObj.transform, false);
            var subRect = subObj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.5f);
            subRect.anchorMax = new Vector2(0.5f, 0.5f);
            subRect.anchoredPosition = new Vector2(0, subInfoY);
            subRect.sizeDelta = new Vector2(800, 30);
            var subText = subObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(subText);
            subText.text = $"{Localization.GetAge(age)}  <color=#888888>{fatherName} \u00d7 {motherName}</color>";
            subText.fontSize = 24;
            subText.alignment = TextAlignmentOptions.Center;
            subText.color = new Color(0.4f, 0.4f, 0.4f);
            subText.richText = true;
            subText.raycastTarget = false;

            // Stats
            int atk = PlayerPrefs.GetInt(p + "babyAtk", 0);
            int def = PlayerPrefs.GetInt(p + "babyDef", 0);
            int hp = PlayerPrefs.GetInt(p + "babyHp", 0);
            int athletic = PlayerPrefs.GetInt(p + "babyAthletic", 0);

            float statsY = subInfoY - 40f;
            var statsObj = new GameObject("Stats");
            statsObj.transform.SetParent(cardObj.transform, false);
            var statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 0.5f);
            statsRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsRect.anchoredPosition = new Vector2(0, statsY);
            statsRect.sizeDelta = new Vector2(700, 36);
            var statsText = statsObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(statsText);
            statsText.text = $"<color=#cc4444>HP {hp}</color>   " +
                $"<color=#dd6622>{Localization.Get("battle_stat_atk")} {atk}</color>   " +
                $"<color=#2266bb>{Localization.Get("battle_stat_def")} {def}</color>   " +
                $"<color=#22aa44>{Localization.Get("battle_stat_athletic")} {athletic}</color>";
            statsText.fontSize = 24;
            statsText.alignment = TextAlignmentOptions.Center;
            statsText.color = new Color(0.3f, 0.3f, 0.3f);
            statsText.richText = true;
            statsText.raycastTarget = false;
        }
        else
        {
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(cardObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(40, 10);
            contentRect.offsetMax = new Vector2(-20, -10);
            var contentText = contentObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(contentText);
            contentText.fontSize = 28;
            contentText.alignment = TextAlignmentOptions.Left;
            contentText.color = new Color(0.6f, 0.6f, 0.6f);
            contentText.raycastTarget = false;
            contentText.text = Localization.Get("map_save_slot_empty");
        }
    }

    void ShowStorySelect()
    {
        // Overlay
        var overlay = new GameObject("StoryOverlay");
        overlay.transform.SetParent(mainCanvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.6f);
        var overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImg;
        var oc = overlayBtn.colors;
        oc.normalColor = overlayImg.color;
        oc.highlightedColor = overlayImg.color;
        oc.pressedColor = overlayImg.color;
        oc.selectedColor = overlayImg.color;
        overlayBtn.colors = oc;
        overlayBtn.onClick.AddListener(() => Destroy(overlay));

        // Panel
        var panel = new GameObject("StorySelectPanel");
        panel.transform.SetParent(overlay.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(800, 900);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.97f, 0.95f, 0.98f);

        // Block click-through
        var panelBtn = panel.AddComponent<Button>();
        panelBtn.targetGraphic = panelImg;
        var pc = panelBtn.colors;
        pc.normalColor = panelImg.color;
        pc.highlightedColor = panelImg.color;
        pc.pressedColor = panelImg.color;
        pc.selectedColor = panelImg.color;
        panelBtn.colors = pc;

        // Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(700, 60);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = Localization.Get("birth_story_title");
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.35f, 0.2f, 0.45f);
        titleText.richText = true;
        titleText.raycastTarget = false;

        // Slot buttons
        float btnStartY = -100f;
        float btnHeight = 120f;
        float btnGap = 15f;
        int count = 0;

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            if (!DataCarrier.SlotExists(i)) continue;

            string father = DataCarrier.GetSlotFatherName(i);
            string mother = DataCarrier.GetSlotMotherName(i);
            string baby = DataCarrier.GetSlotBabyName(i);
            bool isGod = DataCarrier.GetSlotIsGodBaby(i);
            string godMark = isGod ? " <color=#FFD700>\u2605</color>" : "";

            float y = btnStartY - count * (btnHeight + btnGap);
            var slotBtnObj = new GameObject($"StorySlot{i}");
            slotBtnObj.transform.SetParent(panel.transform, false);
            var slotRect = slotBtnObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 1);
            slotRect.anchorMax = new Vector2(0.5f, 1);
            slotRect.anchoredPosition = new Vector2(0, y - btnHeight / 2f);
            slotRect.sizeDelta = new Vector2(680, btnHeight);

            var slotBg = slotBtnObj.AddComponent<Image>();
            slotBg.color = new Color(0.92f, 0.90f, 0.96f);

            int slotIdx = i;
            string f = father, m = mother;
            var slotBtn = slotBtnObj.AddComponent<Button>();
            slotBtn.targetGraphic = slotBg;
            slotBtn.onClick.AddListener(() =>
            {
                Destroy(overlay);
                ShowStoryPanel(f, m);
            });

            var slotTextObj = new GameObject("Text");
            slotTextObj.transform.SetParent(slotBtnObj.transform, false);
            var slotTextRect = slotTextObj.AddComponent<RectTransform>();
            slotTextRect.anchorMin = Vector2.zero;
            slotTextRect.anchorMax = Vector2.one;
            slotTextRect.offsetMin = new Vector2(20, 5);
            slotTextRect.offsetMax = new Vector2(-20, -5);
            var slotText = slotTextObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(slotText);
            slotText.text = $"<b>{baby}</b>{godMark}\n<size=24><color=#666666>{father} \u00d7 {mother}</color></size>";
            slotText.fontSize = 30;
            slotText.alignment = TextAlignmentOptions.Center;
            slotText.color = new Color(0.2f, 0.2f, 0.2f);
            slotText.richText = true;
            slotText.raycastTarget = false;

            count++;
        }

        if (count == 0)
        {
            var emptyObj = new GameObject("Empty");
            emptyObj.transform.SetParent(panel.transform, false);
            var emptyRect = emptyObj.AddComponent<RectTransform>();
            emptyRect.anchorMin = new Vector2(0.5f, 0.5f);
            emptyRect.anchorMax = new Vector2(0.5f, 0.5f);
            emptyRect.anchoredPosition = Vector2.zero;
            emptyRect.sizeDelta = new Vector2(600, 80);
            var emptyText = emptyObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(emptyText);
            emptyText.text = Localization.Get("enishi_empty");
            emptyText.fontSize = 28;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyText.color = new Color(0.5f, 0.5f, 0.5f);
            emptyText.raycastTarget = false;
        }

        // Close button
        var closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panel.transform, false);
        var closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0, 50f);
        closeRect.sizeDelta = new Vector2(250, 70);
        var closeBg = closeObj.AddComponent<Image>();
        closeBg.sprite = GetCircleSprite();
        closeBg.color = new Color(0.55f, 0.35f, 0.65f);
        var closeBtn = closeObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        closeBtn.onClick.AddListener(() => Destroy(overlay));
        var closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        var closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        var closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(closeText);
        closeText.text = Localization.Get("ui_close");
        closeText.fontSize = 28;
        closeText.fontStyle = FontStyles.Bold;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
        closeText.raycastTarget = false;
    }

    void ShowStoryPanel(string fatherName, string motherName)
    {
        string loveStory = Localization.GetLoveStory(fatherName, motherName);

        // Overlay
        var overlay = new GameObject("StoryDetailOverlay");
        overlay.transform.SetParent(mainCanvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.7f);
        var overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImg;
        var oc = overlayBtn.colors;
        oc.normalColor = overlayImg.color;
        oc.highlightedColor = overlayImg.color;
        oc.pressedColor = overlayImg.color;
        oc.selectedColor = overlayImg.color;
        overlayBtn.colors = oc;
        overlayBtn.onClick.AddListener(() => Destroy(overlay));

        // Panel
        var panel = new GameObject("StoryDetailPanel");
        panel.transform.SetParent(overlay.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(900, 1200);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.97f);

        // Block click-through
        var panelBtn = panel.AddComponent<Button>();
        panelBtn.targetGraphic = panelImg;
        var pc = panelBtn.colors;
        pc.normalColor = panelImg.color;
        pc.highlightedColor = panelImg.color;
        pc.pressedColor = panelImg.color;
        pc.selectedColor = panelImg.color;
        panelBtn.colors = pc;

        // Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -40);
        titleRect.sizeDelta = new Vector2(800, 60);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = Localization.Get("birth_story_title");
        titleText.fontSize = 36;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.42f, 0.70f);
        titleText.richText = true;
        titleText.raycastTarget = false;

        // Parents
        var parentsObj = new GameObject("Parents");
        parentsObj.transform.SetParent(panel.transform, false);
        var parentsRect = parentsObj.AddComponent<RectTransform>();
        parentsRect.anchorMin = new Vector2(0.5f, 1);
        parentsRect.anchorMax = new Vector2(0.5f, 1);
        parentsRect.anchoredPosition = new Vector2(0, -90);
        parentsRect.sizeDelta = new Vector2(800, 40);
        var parentsText = parentsObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(parentsText);
        parentsText.text = $"{fatherName} \u00d7 {motherName}";
        parentsText.fontSize = 28;
        parentsText.alignment = TextAlignmentOptions.Center;
        parentsText.color = new Color(0.7f, 0.6f, 0.8f);
        parentsText.raycastTarget = false;

        // Scrollable story text
        float storyTop = -130f;
        float storyBottom = 120f;

        var storyScrollObj = new GameObject("StoryScroll");
        storyScrollObj.transform.SetParent(panel.transform, false);
        var storyScrollRect = storyScrollObj.AddComponent<RectTransform>();
        storyScrollRect.anchorMin = new Vector2(0, 0);
        storyScrollRect.anchorMax = new Vector2(1, 1);
        storyScrollRect.offsetMin = new Vector2(50, storyBottom);
        storyScrollRect.offsetMax = new Vector2(-50, storyTop);
        storyScrollObj.AddComponent<RectMask2D>();
        var storyScroll = storyScrollObj.AddComponent<ScrollRect>();
        storyScroll.horizontal = false;
        storyScroll.vertical = true;
        storyScroll.movementType = ScrollRect.MovementType.Elastic;

        var storyContentObj = new GameObject("Content");
        storyContentObj.transform.SetParent(storyScrollObj.transform, false);
        var storyContentRect = storyContentObj.AddComponent<RectTransform>();
        storyContentRect.anchorMin = new Vector2(0, 1);
        storyContentRect.anchorMax = new Vector2(1, 1);
        storyContentRect.pivot = new Vector2(0.5f, 1);
        storyContentRect.anchoredPosition = Vector2.zero;

        storyScroll.content = storyContentRect;

        var storyTextObj = new GameObject("StoryText");
        storyTextObj.transform.SetParent(storyContentObj.transform, false);
        var storyTextRect = storyTextObj.AddComponent<RectTransform>();
        storyTextRect.anchorMin = new Vector2(0, 1);
        storyTextRect.anchorMax = new Vector2(1, 1);
        storyTextRect.pivot = new Vector2(0.5f, 1);
        storyTextRect.anchoredPosition = Vector2.zero;
        var storyText = storyTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(storyText);
        storyText.text = loveStory;
        storyText.fontSize = 32;
        storyText.alignment = TextAlignmentOptions.Center;
        storyText.color = new Color(0.9f, 0.88f, 0.95f);
        storyText.raycastTarget = false;
        storyText.enableWordWrapping = true;
        storyText.lineSpacing = 15f;

        storyText.ForceMeshUpdate();
        float textHeight = storyText.preferredHeight;
        storyTextRect.sizeDelta = new Vector2(0, textHeight);
        storyContentRect.sizeDelta = new Vector2(0, textHeight);

        // Close button
        var closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panel.transform, false);
        var closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0, 55f);
        closeRect.sizeDelta = new Vector2(250, 70);
        var closeBg = closeObj.AddComponent<Image>();
        closeBg.sprite = GetCircleSprite();
        closeBg.color = new Color(0.55f, 0.35f, 0.65f);
        var closeBtn = closeObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        closeBtn.onClick.AddListener(() => Destroy(overlay));
        var closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        var closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        var closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(closeText);
        closeText.text = Localization.Get("ui_close");
        closeText.fontSize = 28;
        closeText.fontStyle = FontStyles.Bold;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
        closeText.raycastTarget = false;
    }

    void LoadSlot(int slot)
    {
        if (DataCarrier.Instance == null)
        {
            GameObject carrierObj = new GameObject("DataCarrier");
            carrierObj.AddComponent<DataCarrier>();
        }

        DataCarrier.Instance.LoadFromSlot(slot);

        if (DataCarrier.Instance.defeatedEnemies > 0)
        {
            DataCarrier.Instance.cameFromMap = false;
            SceneManager.LoadScene("MapScene");
        }
        else
        {
            SceneManager.LoadScene("BattleScene");
        }
    }

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

    static string GetParentImageName(string japaneseName)
    {
        switch (japaneseName)
        {
            case "タケシ": return "takeshi";
            case "ユウキ": return "yuuki";
            case "ゴウ": return "gou";
            case "シンジ": return "shinji";
            case "リョウマ": return "ryouma";
            case "テツヤ": return "tetuya";
            case "サクラ": return "sakura";
            case "ヒナタ": return "hinata";
            case "アキラ": return "akira";
            case "ミサト": return "misato";
            case "カエデ": return "kaede";
            case "ルナ": return "luna";
            default: return japaneseName != null ? japaneseName.ToLower() : "";
        }
    }
}
