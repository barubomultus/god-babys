using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class EnishiManager : MonoBehaviour
{
    private Canvas mainCanvas;
    private static Sprite _circleSprite;
    private static Sprite _pillSprite;
    private static Sprite _shadowSprite;

    // Enemy name → sprite path mapping
    private static readonly Dictionary<string, string> EnemySpriteMap = new Dictionary<string, string>
    {
        { "やんちゃベイビー", "EnemyBabys/common-yantya" },
        { "いじわるベイビー", "EnemyBabys/frist-enemy" },
        { "なきむしベイビー", "EnemyBabys/common-nakimushi" },
        { "あばれんぼうベイビー", "EnemyBabys/common-abarennbou" },
        { "わがままベイビー", "EnemyBabys/common-wagamama" },
        { "村の王シバ", "EnemyBabys/boss/first-boss-shiba" },
        { "わるいベイビー", "EnemyBabys/frist-enemy" },
        { "どくベイビー", "EnemyBabys/poison/doku-baby" },
        { "のろいベイビー", "EnemyBabys/poison/noroi-baby" },
        { "あくまベイビー", "EnemyBabys/poison/akuma-baby" },
        { "デヴィル傭兵A", "EnemyBabys/poison/katchu-a" },
        { "デヴィル傭兵B", "EnemyBabys/poison/katchu-b" },
        { "デヴィル夫人", "EnemyBabys/boss/devil-wife" },
    };

    private static readonly string[] FatherNames = { "タケシ", "ユウキ", "ゴウ", "シンジ", "リョウマ", "テツヤ" };
    private static readonly string[] MotherNames = { "サクラ", "ヒナタ", "アキラ", "ミサト", "カエデ", "ルナ" };

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
        bgImg.color = new Color(0.953f, 0.969f, 0.973f); // #f3f7f8
        bgImg.raycastTarget = false;

        // Title
        float titleY = -188f;
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainCanvas.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, titleY);
        titleRect.sizeDelta = new Vector2(800, 70);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = Localization.Get("enishi_title");
        titleText.fontSize = 44;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.35f, 0.2f, 0.45f);
        titleText.raycastTarget = false;

        // Back button
        float backY = -188f;
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

        // ScrollView
        float scrollTop = titleY - 70f;
        var scrollObj = new GameObject("ScrollView");
        scrollObj.transform.SetParent(mainCanvas.transform, false);
        var scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0, 0);
        scrollRect.anchorMax = new Vector2(1, 1);
        scrollRect.offsetMin = new Vector2(0, 0);
        scrollRect.offsetMax = new Vector2(0, scrollTop);
        scrollObj.AddComponent<RectMask2D>();
        var scrollView = scrollObj.AddComponent<ScrollRect>();
        scrollView.horizontal = false;
        scrollView.vertical = true;
        scrollView.movementType = ScrollRect.MovementType.Elastic;
        scrollView.elasticity = 0.1f;

        // Content container
        var contentObj = new GameObject("Content");
        contentObj.transform.SetParent(scrollObj.transform, false);
        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;

        scrollView.content = contentRect;

        // Build sections
        float yOffset = 0f;
        float sectionPadding = 40f;
        float itemSize = 280f;
        float itemGap = 20f;
        float nameHeight = 50f;
        int columns = 3;
        float totalWidth = 1080f;
        float sideMargin = (totalWidth - columns * itemSize - (columns - 1) * itemGap) / 2f;

        // === Section 1: Defeated Enemies ===
        // Aggregate from all save slots + current session
        var uniqueEnemies = new List<string>();
        var seen = new HashSet<string>();

        // Current session data (if any)
        var dc = DataCarrier.Instance;
        if (dc != null)
        {
            foreach (var e in dc.GetDefeatedEnemyList())
            {
                if (!seen.Contains(e)) { seen.Add(e); uniqueEnemies.Add(e); }
            }
        }

        // All save slots in PlayerPrefs
        for (int slot = 0; slot < DataCarrier.MAX_SAVE_SLOTS; slot++)
        {
            if (!DataCarrier.SlotExists(slot)) continue;
            string list = PlayerPrefs.GetString($"slot{slot}_defeatedEnemyList", "");
            if (string.IsNullOrEmpty(list)) continue;
            foreach (var e in list.Split(','))
            {
                if (!string.IsNullOrEmpty(e) && !seen.Contains(e))
                {
                    seen.Add(e);
                    uniqueEnemies.Add(e);
                }
            }
        }

        yOffset = CreateSectionHeader(contentObj.transform, Localization.Get("enishi_section_enemies"), yOffset);
        yOffset -= sectionPadding;

        if (uniqueEnemies.Count == 0)
        {
            yOffset = CreateEmptyText(contentObj.transform, Localization.Get("enishi_empty"), yOffset);
        }
        else
        {
            int rows = Mathf.CeilToInt((float)uniqueEnemies.Count / columns);
            for (int i = 0; i < uniqueEnemies.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                float x = sideMargin + col * (itemSize + itemGap) + itemSize / 2f;
                float y = yOffset - row * (itemSize + nameHeight + itemGap);

                string jaName = uniqueEnemies[i];
                Sprite sprite = LoadEnemySprite(jaName);
                string displayName = Localization.GetEnemy(jaName);
                CreateGridItem(contentObj.transform, sprite, displayName, x, y, itemSize,
                    () => ShowEnemyDetail(jaName));
            }
            yOffset -= rows * (itemSize + nameHeight + itemGap);
        }

        // === Collect encountered parents from all save slots + current session ===
        var encounteredFathers = new List<string>();
        var encounteredMothers = new List<string>();
        var seenFathers = new HashSet<string>();
        var seenMothers = new HashSet<string>();

        // Current session
        if (dc != null)
        {
            if (!string.IsNullOrEmpty(dc.fatherName) && !seenFathers.Contains(dc.fatherName))
            { seenFathers.Add(dc.fatherName); encounteredFathers.Add(dc.fatherName); }
            if (!string.IsNullOrEmpty(dc.motherName) && !seenMothers.Contains(dc.motherName))
            { seenMothers.Add(dc.motherName); encounteredMothers.Add(dc.motherName); }
        }

        // All save slots
        for (int slot = 0; slot < DataCarrier.MAX_SAVE_SLOTS; slot++)
        {
            if (!DataCarrier.SlotExists(slot)) continue;
            string f = PlayerPrefs.GetString($"slot{slot}_fatherName", "");
            string m = PlayerPrefs.GetString($"slot{slot}_motherName", "");
            if (!string.IsNullOrEmpty(f) && !seenFathers.Contains(f))
            { seenFathers.Add(f); encounteredFathers.Add(f); }
            if (!string.IsNullOrEmpty(m) && !seenMothers.Contains(m))
            { seenMothers.Add(m); encounteredMothers.Add(m); }
        }

        // === Section 2: Fathers ===
        yOffset -= sectionPadding;
        yOffset = CreateSectionHeader(contentObj.transform, Localization.Get("enishi_section_fathers"), yOffset);
        yOffset -= sectionPadding;

        if (encounteredFathers.Count == 0)
        {
            yOffset = CreateEmptyText(contentObj.transform, Localization.Get("enishi_empty"), yOffset);
        }
        else
        {
            int rows = Mathf.CeilToInt((float)encounteredFathers.Count / columns);
            for (int i = 0; i < encounteredFathers.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                float x = sideMargin + col * (itemSize + itemGap) + itemSize / 2f;
                float y = yOffset - row * (itemSize + nameHeight + itemGap);

                string jaName = encounteredFathers[i];
                string imgKey = GetParentImageName(jaName);
                Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);
                CreateGridItem(contentObj.transform, sprite, jaName, x, y, itemSize,
                    () => ShowParentDetail(jaName));
            }
            yOffset -= rows * (itemSize + nameHeight + itemGap);
        }

        // === Section 3: Mothers ===
        yOffset -= sectionPadding;
        yOffset = CreateSectionHeader(contentObj.transform, Localization.Get("enishi_section_mothers"), yOffset);
        yOffset -= sectionPadding;

        if (encounteredMothers.Count == 0)
        {
            yOffset = CreateEmptyText(contentObj.transform, Localization.Get("enishi_empty"), yOffset);
        }
        else
        {
            int rows = Mathf.CeilToInt((float)encounteredMothers.Count / columns);
            for (int i = 0; i < encounteredMothers.Count; i++)
            {
                int col = i % columns;
                int row = i / columns;
                float x = sideMargin + col * (itemSize + itemGap) + itemSize / 2f;
                float y = yOffset - row * (itemSize + nameHeight + itemGap);

                string jaName = encounteredMothers[i];
                string imgKey = GetParentImageName(jaName);
                Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);
                CreateGridItem(contentObj.transform, sprite, jaName, x, y, itemSize,
                    () => ShowParentDetail(jaName));
            }
            yOffset -= rows * (itemSize + nameHeight + itemGap);
        }

        yOffset -= sectionPadding;

        // Set content size
        contentRect.sizeDelta = new Vector2(0, -yOffset);
    }

    float CreateSectionHeader(Transform parent, string text, float yOffset)
    {
        float headerHeight = 60f;
        var headerObj = new GameObject("SectionHeader");
        headerObj.transform.SetParent(parent, false);
        var headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.anchoredPosition = new Vector2(0, yOffset - headerHeight / 2f);
        headerRect.sizeDelta = new Vector2(0, headerHeight);
        var headerText = headerObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(headerText);
        headerText.text = text;
        headerText.fontSize = 36;
        headerText.fontStyle = FontStyles.Bold;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = new Color(0.35f, 0.2f, 0.45f);
        headerText.raycastTarget = false;

        // Separator line
        var sepObj = new GameObject("Separator");
        sepObj.transform.SetParent(parent, false);
        var sepRect = sepObj.AddComponent<RectTransform>();
        sepRect.anchorMin = new Vector2(0.5f, 1);
        sepRect.anchorMax = new Vector2(0.5f, 1);
        sepRect.anchoredPosition = new Vector2(0, yOffset - headerHeight - 5f);
        sepRect.sizeDelta = new Vector2(900, 3);
        var sepImg = sepObj.AddComponent<Image>();
        sepImg.color = new Color(0.55f, 0.35f, 0.65f, 0.4f);
        sepImg.raycastTarget = false;

        return yOffset - headerHeight - 15f;
    }

    float CreateEmptyText(Transform parent, string text, float yOffset)
    {
        var emptyObj = new GameObject("EmptyText");
        emptyObj.transform.SetParent(parent, false);
        var emptyRect = emptyObj.AddComponent<RectTransform>();
        emptyRect.anchorMin = new Vector2(0.5f, 1);
        emptyRect.anchorMax = new Vector2(0.5f, 1);
        emptyRect.anchoredPosition = new Vector2(0, yOffset - 40f);
        emptyRect.sizeDelta = new Vector2(700, 80);
        var emptyText = emptyObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(emptyText);
        emptyText.text = text;
        emptyText.fontSize = 30;
        emptyText.alignment = TextAlignmentOptions.Center;
        emptyText.color = new Color(0.5f, 0.5f, 0.5f);
        emptyText.raycastTarget = false;
        return yOffset - 100f;
    }

    void CreateGridItem(Transform parent, Sprite sprite, string name, float x, float y,
        float size, UnityEngine.Events.UnityAction onTap)
    {
        var itemObj = new GameObject("GridItem_" + name);
        itemObj.transform.SetParent(parent, false);
        var itemRect = itemObj.AddComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0, 1);
        itemRect.anchorMax = new Vector2(0, 1);
        itemRect.anchoredPosition = new Vector2(x, y - size / 2f);
        itemRect.sizeDelta = new Vector2(size, size + 50f);

        // Circle border
        var borderObj = new GameObject("Border");
        borderObj.transform.SetParent(itemObj.transform, false);
        var borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 1f);
        borderRect.anchorMax = new Vector2(0.5f, 1f);
        borderRect.anchoredPosition = new Vector2(0, -size / 2f);
        borderRect.sizeDelta = new Vector2(size + 6, size + 6);
        var borderImg = borderObj.AddComponent<Image>();
        borderImg.sprite = GetCircleSprite();
        borderImg.type = Image.Type.Simple;
        borderImg.color = new Color(0.82f, 0.82f, 0.82f);
        borderImg.raycastTarget = false;

        // Circle mask
        var maskObj = new GameObject("Mask");
        maskObj.transform.SetParent(itemObj.transform, false);
        var maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.5f, 1f);
        maskRect.anchorMax = new Vector2(0.5f, 1f);
        maskRect.anchoredPosition = new Vector2(0, -size / 2f);
        maskRect.sizeDelta = new Vector2(size, size);
        var maskImg = maskObj.AddComponent<Image>();
        maskImg.sprite = GetCircleSprite();
        maskImg.type = Image.Type.Simple;
        maskImg.color = Color.white;
        var mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Make tappable
        var btn = maskObj.AddComponent<Button>();
        btn.targetGraphic = maskImg;
        maskImg.raycastTarget = true;
        btn.onClick.AddListener(onTap);

        // Image inside mask
        var imgObj = new GameObject("Image");
        imgObj.transform.SetParent(maskObj.transform, false);
        var imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
        var img = imgObj.AddComponent<Image>();
        img.raycastTarget = false;

        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = true;
        }
        else
        {
            img.color = new Color(0.7f, 0.7f, 0.7f, 0.3f);
            mask.showMaskGraphic = true;
            maskImg.color = new Color(0.85f, 0.85f, 0.85f);
        }

        // Name text below circle
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(itemObj.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0, -size - 15f);
        nameRect.sizeDelta = new Vector2(size + 20f, 40f);
        var nameText = nameObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(nameText);
        nameText.text = name;
        nameText.fontSize = 24;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = new Color(0.2f, 0.2f, 0.2f);
        nameText.raycastTarget = false;
        nameText.overflowMode = TextOverflowModes.Ellipsis;
    }

    // === Detail Panels ===

    void ShowEnemyDetail(string jaName)
    {
        Sprite sprite = LoadEnemySprite(jaName);
        string displayName = Localization.GetEnemy(jaName);
        string bio = Localization.GetEnemyBio(jaName);

        ShowDetailPanel(sprite, displayName, bio);
    }

    void ShowParentDetail(string jaName)
    {
        string imgKey = GetParentImageName(jaName);
        Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);
        string intro = Localization.GetParentIntro(jaName);
        string bio = Localization.GetParentBio(jaName);

        string description = "";
        if (!string.IsNullOrEmpty(intro))
            description += intro;
        if (!string.IsNullOrEmpty(bio))
        {
            if (description.Length > 0) description += "\n\n";
            description += bio;
        }

        ShowDetailPanel(sprite, jaName, description);
    }

    void ShowDetailPanel(Sprite sprite, string name, string description)
    {
        float imgSize = 350f;

        // Overlay
        var overlay = new GameObject("DetailOverlay");
        overlay.transform.SetParent(mainCanvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.6f);

        // Tap overlay to close
        var overlayBtn = overlay.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImg;
        var overlayColors = overlayBtn.colors;
        overlayColors.normalColor = new Color(0f, 0f, 0f, 0.6f);
        overlayColors.highlightedColor = new Color(0f, 0f, 0f, 0.6f);
        overlayColors.pressedColor = new Color(0f, 0f, 0f, 0.6f);
        overlayColors.selectedColor = new Color(0f, 0f, 0f, 0.6f);
        overlayBtn.colors = overlayColors;
        overlayBtn.onClick.AddListener(() => Destroy(overlay));

        // Panel
        var panel = new GameObject("DetailPanel");
        panel.transform.SetParent(overlay.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = new Vector2(50, 80);
        panelRect.offsetMax = new Vector2(-50, -80);
        var panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.97f, 0.95f, 0.98f);

        // Block overlay click through panel
        var panelBtn = panel.AddComponent<Button>();
        panelBtn.targetGraphic = panelImg;
        var panelColors = panelBtn.colors;
        panelColors.normalColor = panelImg.color;
        panelColors.highlightedColor = panelImg.color;
        panelColors.pressedColor = panelImg.color;
        panelColors.selectedColor = panelImg.color;
        panelBtn.colors = panelColors;

        // Large circle image
        float imgY = -40f;
        var borderObj = new GameObject("ImgBorder");
        borderObj.transform.SetParent(panel.transform, false);
        var borderRect = borderObj.AddComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 1f);
        borderRect.anchorMax = new Vector2(0.5f, 1f);
        borderRect.anchoredPosition = new Vector2(0, imgY - imgSize / 2f);
        borderRect.sizeDelta = new Vector2(imgSize + 8, imgSize + 8);
        var borderImg = borderObj.AddComponent<Image>();
        borderImg.sprite = GetCircleSprite();
        borderImg.type = Image.Type.Simple;
        borderImg.color = new Color(0.55f, 0.35f, 0.65f, 0.5f);
        borderImg.raycastTarget = false;

        var maskObj = new GameObject("ImgMask");
        maskObj.transform.SetParent(panel.transform, false);
        var maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.5f, 1f);
        maskRect.anchorMax = new Vector2(0.5f, 1f);
        maskRect.anchoredPosition = new Vector2(0, imgY - imgSize / 2f);
        maskRect.sizeDelta = new Vector2(imgSize, imgSize);
        var maskImg = maskObj.AddComponent<Image>();
        maskImg.sprite = GetCircleSprite();
        maskImg.type = Image.Type.Simple;
        maskImg.color = Color.white;
        maskImg.raycastTarget = false;
        var mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        var imgObj = new GameObject("Image");
        imgObj.transform.SetParent(maskObj.transform, false);
        var imgRect = imgObj.AddComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;
        var img = imgObj.AddComponent<Image>();
        img.raycastTarget = false;

        if (sprite != null)
        {
            img.sprite = sprite;
            img.color = Color.white;
            img.preserveAspect = true;
        }
        else
        {
            img.color = new Color(0.7f, 0.7f, 0.7f, 0.3f);
            mask.showMaskGraphic = true;
            maskImg.color = new Color(0.85f, 0.85f, 0.85f);
        }

        // Name
        float nameY = imgY - imgSize - 30f;
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(panel.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0, nameY);
        nameRect.sizeDelta = new Vector2(800, 60);
        var nameText = nameObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(nameText);
        nameText.text = name;
        nameText.fontSize = 40;
        nameText.fontStyle = FontStyles.Bold;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = new Color(0.15f, 0.15f, 0.15f);
        nameText.raycastTarget = false;

        // Description (scrollable if needed)
        if (!string.IsNullOrEmpty(description))
        {
            float descTop = nameY - 50f;
            float descBottom = 160f; // above close button

            var descScrollObj = new GameObject("DescScroll");
            descScrollObj.transform.SetParent(panel.transform, false);
            var descScrollRect = descScrollObj.AddComponent<RectTransform>();
            descScrollRect.anchorMin = new Vector2(0, 0);
            descScrollRect.anchorMax = new Vector2(1, 1);
            descScrollRect.offsetMin = new Vector2(50, descBottom);
            descScrollRect.offsetMax = new Vector2(-50, descTop);
            descScrollObj.AddComponent<RectMask2D>();
            var descScroll = descScrollObj.AddComponent<ScrollRect>();
            descScroll.horizontal = false;
            descScroll.vertical = true;
            descScroll.movementType = ScrollRect.MovementType.Elastic;

            var descContentObj = new GameObject("DescContent");
            descContentObj.transform.SetParent(descScrollObj.transform, false);
            var descContentRect = descContentObj.AddComponent<RectTransform>();
            descContentRect.anchorMin = new Vector2(0, 1);
            descContentRect.anchorMax = new Vector2(1, 1);
            descContentRect.pivot = new Vector2(0.5f, 1);
            descContentRect.anchoredPosition = Vector2.zero;

            descScroll.content = descContentRect;

            var descTextObj = new GameObject("DescText");
            descTextObj.transform.SetParent(descContentObj.transform, false);
            var descTextRect = descTextObj.AddComponent<RectTransform>();
            descTextRect.anchorMin = new Vector2(0, 1);
            descTextRect.anchorMax = new Vector2(1, 1);
            descTextRect.pivot = new Vector2(0.5f, 1);
            descTextRect.anchoredPosition = Vector2.zero;
            var descText = descTextObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(descText);
            descText.text = description;
            descText.fontSize = 36;
            descText.alignment = TextAlignmentOptions.TopLeft;
            descText.color = new Color(0.25f, 0.25f, 0.25f);
            descText.raycastTarget = false;
            descText.enableWordWrapping = true;

            // Auto-size content to text
            descText.ForceMeshUpdate();
            float textHeight = descText.preferredHeight;
            descTextRect.sizeDelta = new Vector2(0, textHeight);
            descContentRect.sizeDelta = new Vector2(0, textHeight);
        }

        // Close button (pill style matching TitleScene)
        float closeBtnW = 500f;
        float closeBtnH = 100f;
        int closePillRadius = (int)(closeBtnH / 2);
        int closeBlur = 20;

        var closeObj = new GameObject("CloseButton");
        closeObj.transform.SetParent(panel.transform, false);
        var closeRect = closeObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0f);
        closeRect.anchorMax = new Vector2(0.5f, 0f);
        closeRect.anchoredPosition = new Vector2(0, 80f);
        closeRect.sizeDelta = new Vector2(closeBtnW, closeBtnH);

        var closeBg = closeObj.AddComponent<Image>();
        closeBg.sprite = GetPillSprite(closePillRadius);
        closeBg.type = Image.Type.Sliced;
        closeBg.color = Color.white;

        var closeBtn = closeObj.AddComponent<Button>();
        closeBtn.targetGraphic = closeBg;
        closeBtn.onClick.AddListener(() => Destroy(overlay));
        closeBtn.navigation = new Navigation { mode = Navigation.Mode.None };
        var closeColors = closeBtn.colors;
        closeColors.normalColor = Color.white;
        closeColors.highlightedColor = Color.white;
        closeColors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        closeColors.selectedColor = Color.white;
        closeColors.fadeDuration = 0.08f;
        closeBtn.colors = closeColors;

        // Shadow
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(closeObj.transform, false);
        shadowObj.transform.SetAsFirstSibling();
        var shadowRect = shadowObj.AddComponent<RectTransform>();
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = new Vector2(-closeBlur, -closeBlur - 4);
        shadowRect.offsetMax = new Vector2(closeBlur, closeBlur - 4);
        var shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.sprite = GetShadowSprite(closePillRadius, closeBlur);
        shadowImg.type = Image.Type.Sliced;
        shadowImg.color = new Color(0f, 0f, 0f, 0.18f);
        shadowImg.raycastTarget = false;

        // Text
        var closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeObj.transform, false);
        var closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        var closeTmp = closeTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(closeTmp);
        closeTmp.text = Localization.Get("ui_close");
        closeTmp.fontSize = 36;
        closeTmp.alignment = TextAlignmentOptions.Center;
        closeTmp.color = new Color(0.45f, 0.45f, 0.5f);
        closeTmp.fontStyle = FontStyles.Bold;
        closeTmp.raycastTarget = false;

        // Press animation
        AddPressAnimation(closeObj);
    }

    // === Helpers ===

    Sprite LoadEnemySprite(string jaName)
    {
        if (EnemySpriteMap.TryGetValue(jaName, out string path))
            return Resources.Load<Sprite>(path);
        return null;
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
        float center = (size - 1) / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x - center;
                float py = y - center;
                float dist = Mathf.Sqrt(px * px + py * py) - radius;

                if (dist <= -1f)
                    tex.SetPixel(x, y, Color.white);
                else if (dist <= 0f)
                    tex.SetPixel(x, y, new Color(1, 1, 1, -dist));
                else
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
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
                if (dist <= 0f)
                    alpha = 0f;
                else if (dist >= blur)
                    alpha = 0f;
                else
                {
                    float t = dist / blur;
                    alpha = (1f - t) * (1f - t);
                }

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
        if (trigger == null)
            trigger = button.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((data) => {
            button.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerUp);

        var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) => {
            button.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(pointerExit);
    }
}
