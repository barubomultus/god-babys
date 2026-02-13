using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class BabysManager : MonoBehaviour
{
    private Canvas mainCanvas;
    private static Sprite _circleSprite;
    private static Sprite _pillSprite;
    private static Sprite _shadowSprite;

    const int PILL_RADIUS = 32;
    const int SHADOW_BLUR = 16;

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

        // Header area
        float headerY = -140f;

        // Back button (left)
        var backObj = new GameObject("BackButton");
        backObj.transform.SetParent(mainCanvas.transform, false);
        var backRect = backObj.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0, 1f);
        backRect.anchorMax = new Vector2(0, 1f);
        backRect.anchoredPosition = new Vector2(80, headerY);
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
        backText.color = new Color(0.45f, 0.45f, 0.5f);
        backText.fontStyle = FontStyles.Bold;
        backText.raycastTarget = false;

        // Title (center)
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainCanvas.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, headerY);
        titleRect.sizeDelta = new Vector2(800, 70);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = "babys";
        titleText.fontSize = 44;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.15f, 0.15f, 0.18f);
        titleText.raycastTarget = false;

        // Info button (top-right) — white circle + shadow
        var infoObj = new GameObject("InfoButton");
        infoObj.transform.SetParent(mainCanvas.transform, false);
        var infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(1, 1f);
        infoRect.anchorMax = new Vector2(1, 1f);
        infoRect.anchoredPosition = new Vector2(-70, headerY);
        infoRect.sizeDelta = new Vector2(56, 56);
        var infoBg = infoObj.AddComponent<Image>();
        infoBg.sprite = GetCircleSprite();
        infoBg.color = Color.white;

        // Info shadow
        var infoShadow = new GameObject("Shadow");
        infoShadow.transform.SetParent(infoObj.transform, false);
        infoShadow.transform.SetAsFirstSibling();
        var infoShadowRect = infoShadow.AddComponent<RectTransform>();
        infoShadowRect.anchorMin = Vector2.zero;
        infoShadowRect.anchorMax = Vector2.one;
        infoShadowRect.offsetMin = new Vector2(-10, -13);
        infoShadowRect.offsetMax = new Vector2(10, 7);
        var infoShadowImg = infoShadow.AddComponent<Image>();
        infoShadowImg.sprite = GetCircleSprite();
        infoShadowImg.color = new Color(0f, 0f, 0f, 0.12f);
        infoShadowImg.raycastTarget = false;

        var infoBtn = infoObj.AddComponent<Button>();
        infoBtn.targetGraphic = infoBg;
        var ic = infoBtn.colors;
        ic.normalColor = Color.white;
        ic.highlightedColor = Color.white;
        ic.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        ic.selectedColor = Color.white;
        ic.fadeDuration = 0.08f;
        infoBtn.colors = ic;
        infoBtn.onClick.AddListener(ShowStorySelect);
        AddPressAnimation(infoObj);

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
        infoText.fontSize = 28;
        infoText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.color = new Color(0.45f, 0.45f, 0.5f);
        infoText.raycastTarget = false;

        // ScrollView
        float scrollTop = headerY - 60f;
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
        float cardHeight = 220f;
        float emptyHeight = 80f;
        float slotGap = 24f;
        float yOffset = -24f;
        int currentSlot = dc != null ? dc.currentSlot : -1;

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            bool exists = DataCarrier.SlotExists(i);
            float thisHeight = exists ? cardHeight : emptyHeight;
            CreateSlotEntry(contentContainer.transform, i, yOffset, currentSlot == i);
            yOffset -= thisHeight + slotGap;
        }

        contentContainerRect.sizeDelta = new Vector2(0, -yOffset + 40f);
    }

    void CreateSlotEntry(Transform parent, int slot, float yPos, bool isCurrent)
    {
        bool exists = DataCarrier.SlotExists(slot);
        float cardW = 980f;
        float cardHeight = exists ? 220f : 80f;
        float imgSize = 140f;

        var cardObj = new GameObject($"SlotCard{slot}");
        cardObj.transform.SetParent(parent, false);
        var cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.anchoredPosition = new Vector2(0, yPos - cardHeight / 2f);
        cardRect.sizeDelta = new Vector2(cardW, cardHeight);

        // Card background (white pill with shadow)
        var cardImg = cardObj.AddComponent<Image>();
        cardImg.sprite = GetPillSprite();
        cardImg.type = Image.Type.Sliced;
        cardImg.color = Color.white;

        // Shadow
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(cardObj.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-SHADOW_BLUR, -SHADOW_BLUR - 4);
        shadowRect.offsetMax = new Vector2(SHADOW_BLUR, SHADOW_BLUR - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite();
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.10f);
        shadowImg.raycastTarget = false;

        if (exists)
        {
            // Button behavior
            var btn = cardObj.AddComponent<Button>();
            btn.targetGraphic = cardImg;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.94f, 0.94f, 0.96f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            int slotIndex = slot;
            btn.onClick.AddListener(() => LoadSlot(slotIndex));
            AddPressAnimation(cardObj);

            string fatherName = DataCarrier.GetSlotFatherName(slot);
            string motherName = DataCarrier.GetSlotMotherName(slot);
            string babyGender = DataCarrier.GetSlotBabyGender(slot);
            string babyName = DataCarrier.GetSlotBabyName(slot);
            int age = DataCarrier.GetSlotAge(slot);
            bool isGod = DataCarrier.GetSlotIsGodBaby(slot);
            string p = $"slot{slot}_";

            // === Left: Baby image (circle) ===
            float leftPad = 36f;
            float imgCenterX = -cardW / 2f + leftPad + imgSize / 2f;

            // Circle border (accent color for current, light gray otherwise)
            var borderObj = new GameObject("ImgBorder");
            borderObj.transform.SetParent(cardObj.transform, false);
            var borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = new Vector2(0.5f, 0.5f);
            borderRect.anchorMax = new Vector2(0.5f, 0.5f);
            borderRect.anchoredPosition = new Vector2(imgCenterX, 0);
            borderRect.sizeDelta = new Vector2(imgSize + 8, imgSize + 8);
            var borderImg = borderObj.AddComponent<Image>();
            borderImg.sprite = GetCircleSprite();
            borderImg.type = Image.Type.Simple;
            if (isCurrent)
                borderImg.color = new Color(0.30f, 0.55f, 1f);
            else if (isGod)
                borderImg.color = new Color(0.85f, 0.65f, 0.13f);
            else
                borderImg.color = new Color(0.88f, 0.88f, 0.90f);
            borderImg.raycastTarget = false;

            // Circle mask
            var babyMaskObj = new GameObject("BabyMask");
            babyMaskObj.transform.SetParent(cardObj.transform, false);
            var babyMaskRect = babyMaskObj.AddComponent<RectTransform>();
            babyMaskRect.anchorMin = new Vector2(0.5f, 0.5f);
            babyMaskRect.anchorMax = new Vector2(0.5f, 0.5f);
            babyMaskRect.anchoredPosition = new Vector2(imgCenterX, 0);
            babyMaskRect.sizeDelta = new Vector2(imgSize, imgSize);
            var babyMaskImg = babyMaskObj.AddComponent<Image>();
            babyMaskImg.sprite = GetCircleSprite();
            babyMaskImg.type = Image.Type.Simple;
            babyMaskImg.color = Color.white;
            babyMaskImg.raycastTarget = false;
            var babyMask = babyMaskObj.AddComponent<Mask>();
            babyMask.showMaskGraphic = false;

            // Baby icon
            var babyIconObj = new GameObject("BabyIcon");
            babyIconObj.transform.SetParent(babyMaskObj.transform, false);
            var babyIconRect = babyIconObj.AddComponent<RectTransform>();
            babyIconRect.anchorMin = Vector2.zero;
            babyIconRect.anchorMax = Vector2.one;
            babyIconRect.offsetMin = Vector2.zero;
            babyIconRect.offsetMax = Vector2.zero;
            var babyIconImg = babyIconObj.AddComponent<Image>();
            babyIconImg.raycastTarget = false;

            // Load sprite
            string customPath = PlayerPrefs.GetString($"slot{slot}_customBabyImagePath", "");
            Sprite customSprite = null;
            if (!string.IsNullOrEmpty(customPath))
            {
                string fullPath = Path.Combine(Application.persistentDataPath, customPath);
                if (File.Exists(fullPath))
                {
                    byte[] data = File.ReadAllBytes(fullPath);
                    var tex = new Texture2D(2, 2);
                    tex.LoadImage(data);
                    customSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }

            if (customSprite != null)
            {
                babyIconImg.sprite = customSprite;
                babyIconImg.color = Color.white;
                babyIconImg.preserveAspect = true;
            }
            else
            {
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
                    // Placeholder gradient
                    babyMask.showMaskGraphic = true;
                    babyMaskImg.color = babyGender == "\u7537\u306e\u5b50"
                        ? new Color(0.75f, 0.88f, 1f)
                        : new Color(1f, 0.82f, 0.90f);
                    babyIconImg.color = new Color(1, 1, 1, 0);
                }
            }

            // Face tap → image upload (reuse border as tap target)
            borderImg.raycastTarget = true;
            var faceBtn = borderObj.AddComponent<Button>();
            faceBtn.targetGraphic = borderImg;
            var faceColors = faceBtn.colors;
            faceColors.pressedColor = new Color(0.75f, 0.75f, 0.80f);
            faceBtn.colors = faceColors;
            int uploadSlot = slot;
            Image uploadTarget = babyIconImg;
            Mask uploadMask = babyMask;
            Image uploadMaskImg = babyMaskImg;
            faceBtn.onClick.AddListener(() => PickImageForSlot(uploadSlot, uploadTarget, uploadMask, uploadMaskImg));

            // Camera icon overlay on face
            var camObj = new GameObject("CamIcon");
            camObj.transform.SetParent(borderObj.transform, false);
            var camRect = camObj.AddComponent<RectTransform>();
            camRect.anchorMin = new Vector2(1, 0);
            camRect.anchorMax = new Vector2(1, 0);
            camRect.anchoredPosition = new Vector2(2, 2);
            camRect.sizeDelta = new Vector2(36, 36);
            var camBg = camObj.AddComponent<Image>();
            camBg.sprite = GetCircleSprite();
            camBg.color = new Color(0.45f, 0.45f, 0.5f, 0.8f);
            camBg.raycastTarget = false;
            var camText = new GameObject("T").AddComponent<TextMeshProUGUI>();
            camText.transform.SetParent(camObj.transform, false);
            var camTR = camText.GetComponent<RectTransform>();
            camTR.anchorMin = Vector2.zero;
            camTR.anchorMax = Vector2.one;
            camTR.offsetMin = Vector2.zero;
            camTR.offsetMax = Vector2.zero;
            FontHelper.Apply(camText);
            camText.text = "\u270E";
            camText.fontSize = 18;
            camText.alignment = TextAlignmentOptions.Center;
            camText.color = Color.white;
            camText.raycastTarget = false;

            // === Right: Info section ===
            float infoLeft = imgCenterX + imgSize / 2f + 28f;
            float infoW = cardW / 2f - leftPad + imgCenterX - imgSize / 2f - 28f + cardW / 2f - 36f;

            // NOW badge (top-right if current)
            if (isCurrent)
            {
                var nowObj = new GameObject("NowBadge");
                nowObj.transform.SetParent(cardObj.transform, false);
                var nowRect = nowObj.AddComponent<RectTransform>();
                nowRect.anchorMin = new Vector2(1, 1);
                nowRect.anchorMax = new Vector2(1, 1);
                nowRect.anchoredPosition = new Vector2(-28, -16);
                nowRect.sizeDelta = new Vector2(80, 32);
                var nowBg = nowObj.AddComponent<Image>();
                nowBg.sprite = GetPillSprite();
                nowBg.type = Image.Type.Sliced;
                nowBg.color = new Color(0.30f, 0.55f, 1f);
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
                nowText.fontSize = 18;
                nowText.fontStyle = FontStyles.Bold;
                nowText.alignment = TextAlignmentOptions.Center;
                nowText.color = Color.white;
                nowText.raycastTarget = false;
            }

            // GOD BABY badge
            if (isGod)
            {
                var godObj = new GameObject("GodBadge");
                godObj.transform.SetParent(cardObj.transform, false);
                var godRect = godObj.AddComponent<RectTransform>();
                godRect.anchorMin = new Vector2(1, 1);
                godRect.anchorMax = new Vector2(1, 1);
                float godX = isCurrent ? -116 : -28;
                godRect.anchoredPosition = new Vector2(godX, -16);
                godRect.sizeDelta = new Vector2(130, 32);
                var godBg = godObj.AddComponent<Image>();
                godBg.sprite = GetPillSprite();
                godBg.type = Image.Type.Sliced;
                godBg.color = new Color(0.85f, 0.65f, 0.13f);
                godBg.raycastTarget = false;

                var godTextObj = new GameObject("Text");
                godTextObj.transform.SetParent(godObj.transform, false);
                var godTextRect = godTextObj.AddComponent<RectTransform>();
                godTextRect.anchorMin = Vector2.zero;
                godTextRect.anchorMax = Vector2.one;
                godTextRect.offsetMin = Vector2.zero;
                godTextRect.offsetMax = Vector2.zero;
                var godText = godTextObj.AddComponent<TextMeshProUGUI>();
                FontHelper.Apply(godText);
                godText.text = "\u2605 GOD";
                godText.fontSize = 16;
                godText.fontStyle = FontStyles.Bold;
                godText.alignment = TextAlignmentOptions.Center;
                godText.color = Color.white;
                godText.raycastTarget = false;
            }

            // Baby name
            float nameY = 50f;
            var nameObj = new GameObject("NameText");
            nameObj.transform.SetParent(cardObj.transform, false);
            var nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 0.5f);
            nameRect.anchoredPosition = new Vector2(infoLeft / 2f + 16f, nameY);
            nameRect.sizeDelta = new Vector2(cardW - (imgCenterX + cardW / 2f + imgSize / 2f + 28f) - 36f, 48);
            var nameText = nameObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(nameText);
            nameText.text = babyName;
            nameText.fontSize = 36;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = new Color(0.15f, 0.15f, 0.18f);
            nameText.raycastTarget = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            // Age + Parents line
            float subY = nameY - 40f;
            var subObj = new GameObject("SubInfo");
            subObj.transform.SetParent(cardObj.transform, false);
            var subRect = subObj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0, 0.5f);
            subRect.anchorMax = new Vector2(1, 0.5f);
            subRect.anchoredPosition = new Vector2(infoLeft / 2f + 16f, subY);
            subRect.sizeDelta = new Vector2(cardW - (imgCenterX + cardW / 2f + imgSize / 2f + 28f) - 36f, 30);
            var subText = subObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(subText);
            subText.text = $"{Localization.GetAge(age)}    {fatherName} \u00d7 {motherName}";
            subText.fontSize = 22;
            subText.alignment = TextAlignmentOptions.Left;
            subText.color = new Color(0.55f, 0.55f, 0.6f);
            subText.raycastTarget = false;
            subText.overflowMode = TextOverflowModes.Ellipsis;

            // Stats row (pill badges)
            int atk = PlayerPrefs.GetInt(p + "babyAtk", 0);
            int def = PlayerPrefs.GetInt(p + "babyDef", 0);
            int hp = PlayerPrefs.GetInt(p + "babyHp", 0);
            int athletic = PlayerPrefs.GetInt(p + "babyAthletic", 0);

            float statsY = subY - 44f;
            float statX = imgCenterX + imgSize / 2f + 28f;
            float statGap = 8f;

            string[][] statDefs = new string[][] {
                new[] { "HP", hp.ToString(), "#E05555" },
                new[] { Localization.Get("battle_stat_atk"), atk.ToString(), "#DD7722" },
                new[] { Localization.Get("battle_stat_def"), def.ToString(), "#3366CC" },
                new[] { Localization.Get("battle_stat_athletic"), athletic.ToString(), "#22AA55" },
            };

            foreach (var sd in statDefs)
            {
                float pillW = 120f + (sd[1].Length > 2 ? 16f : 0);
                var statObj = new GameObject("Stat_" + sd[0]);
                statObj.transform.SetParent(cardObj.transform, false);
                var statRect = statObj.AddComponent<RectTransform>();
                statRect.anchorMin = new Vector2(0.5f, 0.5f);
                statRect.anchorMax = new Vector2(0.5f, 0.5f);
                statRect.anchoredPosition = new Vector2(statX + pillW / 2f, statsY);
                statRect.sizeDelta = new Vector2(pillW, 34);

                var statBg = statObj.AddComponent<Image>();
                statBg.sprite = GetPillSprite();
                statBg.type = Image.Type.Sliced;
                statBg.color = new Color(0.95f, 0.95f, 0.97f);
                statBg.raycastTarget = false;

                var statTextObj = new GameObject("Text");
                statTextObj.transform.SetParent(statObj.transform, false);
                var stRect = statTextObj.AddComponent<RectTransform>();
                stRect.anchorMin = Vector2.zero;
                stRect.anchorMax = Vector2.one;
                stRect.offsetMin = new Vector2(8, 0);
                stRect.offsetMax = new Vector2(-8, 0);
                var statText = statTextObj.AddComponent<TextMeshProUGUI>();
                FontHelper.Apply(statText);
                statText.text = $"<color={sd[2]}>{sd[0]}</color> {sd[1]}";
                statText.fontSize = 18;
                statText.fontStyle = FontStyles.Bold;
                statText.alignment = TextAlignmentOptions.Center;
                statText.color = new Color(0.3f, 0.3f, 0.35f);
                statText.richText = true;
                statText.raycastTarget = false;

                statX += pillW + statGap;
            }
        }
        else
        {
            // Empty slot — dashed outline style
            cardImg.color = new Color(0.94f, 0.94f, 0.95f);
            shadowImg.color = new Color(0f, 0f, 0f, 0.04f);
            cardImg.raycastTarget = false;

            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(cardObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var contentText = contentObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(contentText);
            contentText.fontSize = 24;
            contentText.alignment = TextAlignmentOptions.Center;
            contentText.color = new Color(0.65f, 0.65f, 0.68f);
            contentText.raycastTarget = false;
            contentText.text = "- - -   " + Localization.Get("map_save_slot_empty") + "   - - -";
        }
    }

    void ShowStorySelect()
    {
        // Full-screen white overlay
        var overlay = new GameObject("StoryOverlay");
        overlay.transform.SetParent(mainCanvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.5f);
        var overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImg;
        var oc = overlayBtn.colors;
        oc.normalColor = overlayImg.color;
        oc.highlightedColor = overlayImg.color;
        oc.pressedColor = overlayImg.color;
        oc.selectedColor = overlayImg.color;
        overlayBtn.colors = oc;
        overlayBtn.onClick.AddListener(() => Destroy(overlay));

        // Panel (white card)
        var panel = new GameObject("StorySelectPanel");
        panel.transform.SetParent(overlay.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(860, 900);
        var panelImg = panel.AddComponent<Image>();
        panelImg.sprite = GetPillSprite();
        panelImg.type = Image.Type.Sliced;
        panelImg.color = Color.white;

        // Panel shadow
        var panelShadow = new GameObject("Shadow");
        panelShadow.transform.SetParent(panel.transform, false);
        panelShadow.transform.SetAsFirstSibling();
        var psr = panelShadow.AddComponent<RectTransform>();
        psr.anchorMin = Vector2.zero;
        psr.anchorMax = Vector2.one;
        psr.offsetMin = new Vector2(-24, -28);
        psr.offsetMax = new Vector2(24, 20);
        var psi = panelShadow.AddComponent<Image>();
        psi.sprite = GetShadowSprite();
        psi.type = Image.Type.Sliced;
        psi.color = new Color(0f, 0f, 0f, 0.18f);
        psi.raycastTarget = false;

        // Block click-through
        var panelBtn = panel.AddComponent<Button>();
        panelBtn.targetGraphic = panelImg;
        var pc = panelBtn.colors;
        pc.normalColor = Color.white;
        pc.highlightedColor = Color.white;
        pc.pressedColor = Color.white;
        pc.selectedColor = Color.white;
        panelBtn.colors = pc;

        // Title
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -44);
        titleRect.sizeDelta = new Vector2(700, 60);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = Localization.Get("birth_story_title");
        titleText.fontSize = 34;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.15f, 0.15f, 0.18f);
        titleText.raycastTarget = false;

        // Slot buttons
        float btnStartY = -110f;
        float btnHeight = 100f;
        float btnGap = 16f;
        int count = 0;

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            if (!DataCarrier.SlotExists(i)) continue;

            string father = DataCarrier.GetSlotFatherName(i);
            string mother = DataCarrier.GetSlotMotherName(i);
            string baby = DataCarrier.GetSlotBabyName(i);
            bool isGod = DataCarrier.GetSlotIsGodBaby(i);
            string godMark = isGod ? "  <color=#D4A017>\u2605</color>" : "";

            float y = btnStartY - count * (btnHeight + btnGap);
            var slotBtnObj = new GameObject($"StorySlot{i}");
            slotBtnObj.transform.SetParent(panel.transform, false);
            var slotRect = slotBtnObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0.5f, 1);
            slotRect.anchorMax = new Vector2(0.5f, 1);
            slotRect.anchoredPosition = new Vector2(0, y - btnHeight / 2f);
            slotRect.sizeDelta = new Vector2(740, btnHeight);

            var slotBg = slotBtnObj.AddComponent<Image>();
            slotBg.sprite = GetPillSprite();
            slotBg.type = Image.Type.Sliced;
            slotBg.color = new Color(0.96f, 0.96f, 0.98f);

            // Slot shadow
            var slotShadow = new GameObject("Shadow");
            slotShadow.transform.SetParent(slotBtnObj.transform, false);
            slotShadow.transform.SetAsFirstSibling();
            var ssr = slotShadow.AddComponent<RectTransform>();
            ssr.anchorMin = Vector2.zero;
            ssr.anchorMax = Vector2.one;
            ssr.offsetMin = new Vector2(-12, -14);
            ssr.offsetMax = new Vector2(12, 10);
            var ssi = slotShadow.AddComponent<Image>();
            ssi.sprite = GetShadowSprite();
            ssi.type = Image.Type.Sliced;
            ssi.color = new Color(0f, 0f, 0f, 0.08f);
            ssi.raycastTarget = false;

            string f = father, m = mother;
            var slotBtn = slotBtnObj.AddComponent<Button>();
            slotBtn.targetGraphic = slotBg;
            slotBtn.navigation = new Navigation { mode = Navigation.Mode.None };
            var sc = slotBtn.colors;
            sc.normalColor = new Color(0.96f, 0.96f, 0.98f);
            sc.highlightedColor = new Color(0.96f, 0.96f, 0.98f);
            sc.pressedColor = new Color(0.90f, 0.90f, 0.94f);
            sc.selectedColor = new Color(0.96f, 0.96f, 0.98f);
            sc.fadeDuration = 0.08f;
            slotBtn.colors = sc;
            slotBtn.onClick.AddListener(() =>
            {
                Destroy(overlay);
                ShowStoryPanel(f, m);
            });
            AddPressAnimation(slotBtnObj);

            var slotTextObj = new GameObject("Text");
            slotTextObj.transform.SetParent(slotBtnObj.transform, false);
            var slotTextRect = slotTextObj.AddComponent<RectTransform>();
            slotTextRect.anchorMin = Vector2.zero;
            slotTextRect.anchorMax = Vector2.one;
            slotTextRect.offsetMin = new Vector2(28, 8);
            slotTextRect.offsetMax = new Vector2(-28, -8);
            var slotText = slotTextObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(slotText);
            slotText.text = $"<b>{baby}</b>{godMark}\n<size=22><color=#888888>{father} \u00d7 {mother}</color></size>";
            slotText.fontSize = 30;
            slotText.alignment = TextAlignmentOptions.Center;
            slotText.color = new Color(0.15f, 0.15f, 0.18f);
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
            emptyText.fontSize = 26;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyText.color = new Color(0.55f, 0.55f, 0.58f);
            emptyText.raycastTarget = false;
        }

        // Close button (white pill)
        var closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panel.transform, false);
        var closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0, 55f);
        closeRect.sizeDelta = new Vector2(280, 64);
        var closeBg = closeObj.AddComponent<Image>();
        closeBg.sprite = GetPillSprite();
        closeBg.type = Image.Type.Sliced;
        closeBg.color = Color.white;

        var closeShadow = new GameObject("Shadow");
        closeShadow.transform.SetParent(closeObj.transform, false);
        closeShadow.transform.SetAsFirstSibling();
        var csr = closeShadow.AddComponent<RectTransform>();
        csr.anchorMin = Vector2.zero;
        csr.anchorMax = Vector2.one;
        csr.offsetMin = new Vector2(-12, -14);
        csr.offsetMax = new Vector2(12, 10);
        var csi = closeShadow.AddComponent<Image>();
        csi.sprite = GetShadowSprite();
        csi.type = Image.Type.Sliced;
        csi.color = new Color(0f, 0f, 0f, 0.12f);
        csi.raycastTarget = false;

        var closeBtn = closeObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        closeBtn.navigation = new Navigation { mode = Navigation.Mode.None };
        var cc = closeBtn.colors;
        cc.normalColor = Color.white;
        cc.highlightedColor = Color.white;
        cc.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        cc.selectedColor = Color.white;
        cc.fadeDuration = 0.08f;
        closeBtn.colors = cc;
        closeBtn.onClick.AddListener(() => Destroy(overlay));
        AddPressAnimation(closeObj);

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
        closeText.fontSize = 26;
        closeText.fontStyle = FontStyles.Bold;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = new Color(0.45f, 0.45f, 0.5f);
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

        // Panel (dark card for story atmosphere)
        var panel = new GameObject("StoryDetailPanel");
        panel.transform.SetParent(overlay.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(920, 1200);
        var panelImg = panel.AddComponent<Image>();
        panelImg.sprite = GetPillSprite();
        panelImg.type = Image.Type.Sliced;
        panelImg.color = new Color(0.08f, 0.06f, 0.14f, 0.97f);

        // Block click-through
        var panelBtn = panel.AddComponent<Button>();
        panelBtn.targetGraphic = panelImg;
        var pc2 = panelBtn.colors;
        pc2.normalColor = panelImg.color;
        pc2.highlightedColor = panelImg.color;
        pc2.pressedColor = panelImg.color;
        pc2.selectedColor = panelImg.color;
        panelBtn.colors = pc2;

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

        // Close button (white pill)
        var closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panel.transform, false);
        var closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0, 55f);
        closeRect.sizeDelta = new Vector2(280, 64);
        var closeBg = closeObj.AddComponent<Image>();
        closeBg.sprite = GetPillSprite();
        closeBg.type = Image.Type.Sliced;
        closeBg.color = new Color(0.55f, 0.35f, 0.65f);

        var closeShadow = new GameObject("Shadow");
        closeShadow.transform.SetParent(closeObj.transform, false);
        closeShadow.transform.SetAsFirstSibling();
        var csr2 = closeShadow.AddComponent<RectTransform>();
        csr2.anchorMin = Vector2.zero;
        csr2.anchorMax = Vector2.one;
        csr2.offsetMin = new Vector2(-12, -14);
        csr2.offsetMax = new Vector2(12, 10);
        var csi2 = closeShadow.AddComponent<Image>();
        csi2.sprite = GetShadowSprite();
        csi2.type = Image.Type.Sliced;
        csi2.color = new Color(0.35f, 0.2f, 0.45f, 0.25f);
        csi2.raycastTarget = false;

        var closeBtn = closeObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        closeBtn.navigation = new Navigation { mode = Navigation.Mode.None };
        var cc2 = closeBtn.colors;
        cc2.normalColor = new Color(0.55f, 0.35f, 0.65f);
        cc2.highlightedColor = new Color(0.55f, 0.35f, 0.65f);
        cc2.pressedColor = new Color(0.45f, 0.28f, 0.55f);
        cc2.selectedColor = new Color(0.55f, 0.35f, 0.65f);
        cc2.fadeDuration = 0.08f;
        closeBtn.colors = cc2;
        closeBtn.onClick.AddListener(() => Destroy(overlay));
        AddPressAnimation(closeObj);

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
        closeText.fontSize = 26;
        closeText.fontStyle = FontStyles.Bold;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
        closeText.raycastTarget = false;
    }

    void PickImageForSlot(int slot, Image targetImage, Mask mask, Image maskImg)
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path == null) return;

            byte[] fileData = File.ReadAllBytes(path);
            string fileName = $"baby_custom_slot{slot}.png";
            string savePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(savePath, fileData);

            var tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);

            PlayerPrefs.SetString($"slot{slot}_customBabyImagePath", fileName);
            PlayerPrefs.Save();

            if (DataCarrier.Instance != null && DataCarrier.Instance.currentSlot == slot)
                DataCarrier.Instance.customBabyImagePath = fileName;

            Sprite spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = spr;
            targetImage.color = Color.white;
            targetImage.preserveAspect = true;
            if (mask != null) mask.showMaskGraphic = false;
        }, "\u8d64\u3061\u3083\u3093\u306e\u753b\u50cf\u3092\u9078\u629e");
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

    // === Sprite Helpers ===

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

    static Sprite GetPillSprite()
    {
        if (_pillSprite != null) return _pillSprite;
        int r = PILL_RADIUS;
        int size = r * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float px = Mathf.Max(0, Mathf.Abs(x - center) - 0);
                float py = Mathf.Max(0, Mathf.Abs(y - center) - 0);
                float dist = Mathf.Sqrt(px * px + py * py) - r;
                if (dist < -1f) tex.SetPixel(x, y, Color.white);
                else if (dist <= 0f) tex.SetPixel(x, y, new Color(1, 1, 1, -dist));
                else tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        tex.Apply();
        var border = new Vector4(r, r, r, r);
        _pillSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _pillSprite;
    }

    static Sprite GetShadowSprite()
    {
        if (_shadowSprite != null) return _shadowSprite;
        int r = PILL_RADIUS;
        int blur = SHADOW_BLUR;
        int size = (r + blur) * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float px = Mathf.Max(0, Mathf.Abs(x - center) - 0);
                float py = Mathf.Max(0, Mathf.Abs(y - center) - 0);
                float dist = Mathf.Sqrt(px * px + py * py) - r;
                float alpha;
                if (dist <= 0f) alpha = 1f;
                else if (dist >= blur) alpha = 0f;
                else { float t = dist / blur; alpha = (1f - t) * (1f - t); }
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        tex.Apply();
        int borderVal = r + blur;
        var border = new Vector4(borderVal, borderVal, borderVal, borderVal);
        _shadowSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _shadowSprite;
    }

    void AddPressAnimation(GameObject obj)
    {
        var trigger = obj.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) =>
        {
            obj.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) =>
        {
            obj.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerUp);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) =>
        {
            obj.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerExit);
    }

    static string GetParentImageName(string japaneseName)
    {
        switch (japaneseName)
        {
            case "\u30BF\u30B1\u30B7": return "takeshi";
            case "\u30E6\u30A6\u30AD": return "yuuki";
            case "\u30B4\u30A6": return "gou";
            case "\u30B7\u30F3\u30B8": return "shinji";
            case "\u30EA\u30E7\u30A6\u30DE": return "ryouma";
            case "\u30C6\u30C4\u30E4": return "tetuya";
            case "\u30B5\u30AF\u30E9": return "sakura";
            case "\u30D2\u30CA\u30BF": return "hinata";
            case "\u30A2\u30AD\u30E9": return "akira";
            case "\u30DF\u30B5\u30C8": return "misato";
            case "\u30AB\u30A8\u30C7": return "kaede";
            case "\u30EB\u30CA": return "luna";
            default: return japaneseName != null ? japaneseName.ToLower() : "";
        }
    }
}
