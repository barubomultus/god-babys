using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UIE = UnityEngine.UIElements;

public class BabysManager : MonoBehaviour
{
    private UIE.PanelSettings panelSettings;
    private UIE.VisualElement root;

    void Start()
    {
        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/BabysStyle" }, panelSettings);
        UIHelper.RegisterTapSE(root);

        BuildUI();
    }

    void OnDestroy()
    {
        if (panelSettings != null)
            Destroy(panelSettings);
    }

    void BuildUI()
    {
        var dc = DataCarrier.Instance;

        // Background
        var bg = new UIE.VisualElement();
        bg.AddToClassList("bg-screen");
        root.Add(bg);

        // Content
        var content = new UIE.VisualElement();
        content.AddToClassList("fill");
        content.style.alignItems = UIE.Align.Center;
        root.Add(content);

        var (safeTop, _, _, safeBottom) = UIHelper.GetSafeMargins();

        // Header area
        var headerArea = new UIE.VisualElement();
        headerArea.AddToClassList("babys-header-area");
        headerArea.style.paddingTop = 80 + safeTop;

        // Back button (round pastel circle, absolute left)
        var backBtn = new UIE.Button();
        backBtn.AddToClassList("babys-back-btn");
        UIHelper.ApplyFontBold(backBtn);
        backBtn.text = "\u2190";
        backBtn.clicked += () => SceneManager.LoadScene("HomeScene");
        headerArea.Add(backBtn);

        // Title (centered)
        var title = UIHelper.CreateLabel(
            "\u3069\u306E\u5B50\u3068 \u3042\u305D\u3073\u306B\u3044\u304F\uFF1F \u2728", "babys-title");
        UIHelper.ApplyFontBold(title);
        headerArea.Add(title);

        // Subtitle (centered)
        var subtitle = UIHelper.CreateLabel(Localization.Get("home_babys"), "babys-subtitle");
        UIHelper.ApplyFont(subtitle);
        headerArea.Add(subtitle);

        content.Add(headerArea);

        // ScrollView
        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.flexGrow = 1;
        scrollView.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        scrollView.contentContainer.style.paddingTop = 16;
        scrollView.contentContainer.style.paddingBottom = 40 + safeBottom;
        content.Add(scrollView);

        int currentSlot = dc != null ? dc.currentSlot : -1;

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
            CreateSlotEntry(scrollView.contentContainer, i, currentSlot == i);
    }

    void CreateSlotEntry(UIE.VisualElement parent, int slot, bool isCurrent)
    {
        bool exists = DataCarrier.SlotExists(slot);

        if (!exists)
        {
            var emptyCard = new UIE.VisualElement();
            emptyCard.AddToClassList("slot-card-empty");
            var emptyText = UIHelper.CreateLabel(
                "- - -   " + Localization.Get("map_save_slot_empty") + "   - - -", "slot-empty-label");
            emptyCard.Add(emptyText);
            parent.Add(emptyCard);
            return;
        }

        string fatherName = DataCarrier.GetSlotFatherName(slot);
        string motherName = DataCarrier.GetSlotMotherName(slot);
        string babyGender = DataCarrier.GetSlotBabyGender(slot);
        string babyName = DataCarrier.GetSlotBabyName(slot);
        int age = DataCarrier.GetSlotAge(slot);
        bool isGod = DataCarrier.GetSlotIsGodBaby(slot);
        string p = $"slot{slot}_";

        var card = new UIE.Button();
        card.AddToClassList("slot-card");
        int slotIndex = slot;
        card.clicked += () =>
        {
            StartCoroutine(PuniBounceEffect(card));
            StartCoroutine(CardSparkleEffect(card));
            StartCoroutine(DelayedLoadSlot(slotIndex, 0.25f));
        };

        // Baby circle
        var circleArea = new UIE.VisualElement();
        circleArea.style.position = UIE.Position.Relative;

        var circleBorder = new UIE.VisualElement();
        circleBorder.AddToClassList("baby-circle-border");
        Color borderColor;
        if (isCurrent) borderColor = new Color(0.30f, 0.55f, 1f);
        else if (isGod) borderColor = new Color(0.85f, 0.65f, 0.13f);
        else borderColor = new Color(0.88f, 0.88f, 0.90f);
        circleBorder.style.backgroundColor = borderColor;

        var circleMask = new UIE.VisualElement();
        circleMask.AddToClassList("baby-circle-mask");

        var babyImg = new UIE.VisualElement();
        babyImg.AddToClassList("baby-icon-image");

        // Load sprite
        Sprite displaySprite = LoadBabySprite(slot, fatherName, motherName, babyGender);
        if (displaySprite != null)
        {
            babyImg.style.backgroundImage = new UIE.StyleBackground(displaySprite);
        }
        else
        {
            babyImg.style.backgroundColor = babyGender == "\u7537\u306e\u5b50"
                ? new Color(0.75f, 0.88f, 1f)
                : new Color(1f, 0.82f, 0.90f);
        }

        circleMask.Add(babyImg);
        circleBorder.Add(circleMask);

        // Camera icon
        var camIcon = new UIE.VisualElement();
        camIcon.AddToClassList("cam-icon");
        var camText = UIHelper.CreateLabel("\u270E", "cam-icon-text");
        camIcon.Add(camText);
        circleBorder.Add(camIcon);

        // Make face tappable for image upload
        var faceBtn = new UIE.Button();
        faceBtn.style.position = UIE.Position.Absolute;
        faceBtn.style.left = 0;
        faceBtn.style.top = 0;
        faceBtn.style.right = 0;
        faceBtn.style.bottom = 0;
        faceBtn.style.backgroundColor = new Color(0, 0, 0, 0);
        faceBtn.style.borderTopWidth = 0;
        faceBtn.style.borderBottomWidth = 0;
        faceBtn.style.borderLeftWidth = 0;
        faceBtn.style.borderRightWidth = 0;
        int uploadSlot = slot;
        UIE.VisualElement uploadTarget = babyImg;
        faceBtn.clicked += () => PickImageForSlot(uploadSlot, uploadTarget);
        circleBorder.Add(faceBtn);

        circleArea.Add(circleBorder);
        card.Add(circleArea);

        // Info section
        var info = new UIE.VisualElement();
        info.AddToClassList("slot-info-section");

        var nameLabel = UIHelper.CreateLabel(babyName, "baby-name-text");
        UIHelper.ApplyFontBold(nameLabel);
        info.Add(nameLabel);

        var subInfo = UIHelper.CreateLabel(
            $"{Localization.GetAge(age)}    {fatherName} \u00d7 {motherName}", "baby-sub-info");
        info.Add(subInfo);

        // Stats row
        int atk = PlayerPrefs.GetInt(p + "babyAtk", 0);
        int def = PlayerPrefs.GetInt(p + "babyDef", 0);
        int hp = PlayerPrefs.GetInt(p + "babyHp", 0);
        int athletic = PlayerPrefs.GetInt(p + "babyAthletic", 0);

        var statsRow = new UIE.VisualElement();
        statsRow.AddToClassList("stats-row");

        AddStatPill(statsRow, "\u2764 HP", hp.ToString(), "#E05555");
        AddStatPill(statsRow, "\uD83D\uDD25 " + Localization.Get("battle_stat_atk"), atk.ToString(), "#DD7722");
        AddStatPill(statsRow, "\uD83D\uDC8E " + Localization.Get("battle_stat_def"), def.ToString(), "#3366CC");
        AddStatPill(statsRow, "\u26A1 " + Localization.Get("battle_stat_athletic"), athletic.ToString(), "#22AA55");

        // Omoide (story) button at end of stats row
        var omoidBtn = new UIE.Button();
        omoidBtn.AddToClassList("omoide-btn");
        UIHelper.ApplyFont(omoidBtn);
        omoidBtn.text = "\uD83D\uDCD6 \u304A\u3082\u3044\u3067\u5E33";
        string fa = fatherName, mo = motherName;
        omoidBtn.clicked += () => ShowStoryPanel(fa, mo);
        omoidBtn.RegisterCallback<UIE.ClickEvent>(evt => evt.StopPropagation());
        statsRow.Add(omoidBtn);

        info.Add(statsRow);
        card.Add(info);

        // Badges
        if (isCurrent)
        {
            var nowBadge = new UIE.VisualElement();
            nowBadge.AddToClassList("now-badge");
            var nowText = UIHelper.CreateLabel("NOW", "badge-text");
            nowBadge.Add(nowText);
            card.Add(nowBadge);
        }

        if (isGod)
        {
            var godShadow = new UIE.VisualElement();
            godShadow.AddToClassList("god-badge-shadow");
            godShadow.pickingMode = UIE.PickingMode.Ignore;
            if (isCurrent) godShadow.style.right = 106;
            card.Add(godShadow);

            var godBadge = new UIE.VisualElement();
            godBadge.AddToClassList("god-badge");
            if (isCurrent) godBadge.style.right = 108;
            var godText = UIHelper.CreateLabel("\u2605 STAR", "badge-text");
            UIHelper.ApplyFontBold(godText);
            godBadge.Add(godText);
            card.Add(godBadge);
        }

        parent.Add(card);
    }

    void AddStatPill(UIE.VisualElement parent, string label, string value, string colorHex)
    {
        var pill = new UIE.VisualElement();
        pill.AddToClassList("stat-pill");

        var text = UIHelper.CreateLabel($"<color={colorHex}>{label}</color> {value}", "stat-text");
        text.enableRichText = true;
        pill.Add(text);

        parent.Add(pill);
    }

    Sprite LoadBabySprite(int slot, string fatherName, string motherName, string babyGender)
    {
        // Try custom image first
        string customPath = PlayerPrefs.GetString($"slot{slot}_customBabyImagePath", "");
        if (!string.IsNullOrEmpty(customPath))
        {
            string fullPath = Path.Combine(Application.persistentDataPath, customPath);
            if (File.Exists(fullPath))
            {
                byte[] data = File.ReadAllBytes(fullPath);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(data);
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }

        // Fall back to resource sprite
        string genderKey = babyGender == "\u7537\u306e\u5b50" ? "male" : "female";
        string fatherKey = GetParentImageName(fatherName);
        string motherKey = GetParentImageName(motherName);
        string babyImagePath = $"babys/{fatherKey}_{motherKey}_{genderKey}";
        return Resources.Load<Sprite>(babyImagePath);
    }

    void ShowStoryPanel(string fatherName, string motherName)
    {
        string loveStory = Localization.GetLoveStory(fatherName, motherName);

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("overlay-dark");
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay) overlay.RemoveFromHierarchy();
        });

        var card = new UIE.VisualElement();
        card.AddToClassList("story-detail-card");
        card.style.maxHeight = new UIE.StyleLength(new UIE.Length(85, UIE.LengthUnit.Percent));

        var title = UIHelper.CreateLabel(Localization.Get("birth_story_title"), "story-detail-title");
        card.Add(title);

        var parents = UIHelper.CreateLabel($"{fatherName} \u00d7 {motherName}", "story-detail-parents");
        card.Add(parents);

        // Scrollable story text
        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.flexGrow = 1;
        scrollView.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));

        var storyText = UIHelper.CreateLabel(loveStory, "story-text");
        scrollView.contentContainer.Add(storyText);
        card.Add(scrollView);

        // Close
        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("story-close-btn");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = Localization.Get("ui_close");
        closeBtn.clicked += () => overlay.RemoveFromHierarchy();
        card.Add(closeBtn);

        overlay.Add(card);
        root.Add(overlay);
    }

    void PickImageForSlot(int slot, UIE.VisualElement targetImage)
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
            targetImage.style.backgroundImage = new UIE.StyleBackground(spr);
            targetImage.style.backgroundColor = UIE.StyleKeyword.None;
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

    IEnumerator PuniBounceEffect(UIE.VisualElement el)
    {
        if (el == null) yield break;
        float shrinkDur = 0.06f;
        float elapsed = 0f;
        while (elapsed < shrinkDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shrinkDur);
            float s = Mathf.Lerp(1f, 0.88f, t);
            el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
        float bounceDur = 0.15f;
        elapsed = 0f;
        while (elapsed < bounceDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounceDur);
            float s;
            if (t < 0.5f)
                s = Mathf.Lerp(0.88f, 1.08f, t * 2f);
            else
                s = Mathf.Lerp(1.08f, 1f, (t - 0.5f) * 2f);
            el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
        el.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));
    }

    IEnumerator CardSparkleEffect(UIE.VisualElement card)
    {
        if (card == null) yield break;
        var parent = card.parent;
        if (parent == null) yield break;

        var layout = card.layout;
        float cx = layout.x + layout.width * 0.5f;
        float cy = layout.y + layout.height * 0.5f;

        var container = new UIE.VisualElement();
        container.pickingMode = UIE.PickingMode.Ignore;
        container.style.position = UIE.Position.Absolute;
        container.style.left = 0; container.style.top = 0;
        container.style.right = 0; container.style.bottom = 0;
        container.style.overflow = UIE.Overflow.Visible;
        parent.Add(container);

        int count = 5;
        var sparkles = new List<(UIE.Label el, float vx, float vy)>();
        for (int i = 0; i < count; i++)
        {
            var sp = new UIE.Label();
            sp.pickingMode = UIE.PickingMode.Ignore;
            sp.text = "\u2728";
            sp.style.position = UIE.Position.Absolute;
            sp.style.fontSize = Random.Range(14, 24);
            sp.style.left = cx + Random.Range(-30f, 30f);
            sp.style.top = cy;
            container.Add(sp);

            float vx = Random.Range(-60f, 60f);
            float vy = Random.Range(-180f, -80f);
            sparkles.Add((sp, vx, vy));
        }

        float duration = 0.6f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < sparkles.Count; i++)
            {
                var (el, vx, vy) = sparkles[i];
                el.style.left = cx + Random.Range(-30f, 30f) + vx * t;
                el.style.top = cy + vy * t;
                el.style.opacity = 1f - t;
            }
            yield return null;
        }
        container.RemoveFromHierarchy();
    }

    IEnumerator DelayedLoadSlot(int slot, float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadSlot(slot);
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
            case "\u30BC\u30CB\u30AC\u30BF": return "zenigata";
            case "\u30C4\u30AF\u30E2": return "tukumo";
            case "\u30B5\u30C8\u30A6": return "satou";
            case "\u30A4\u30EF\u30AA": return "iwao";
            case "\u30A2\u30AD\u30C8\u30B7": return "akitoshi";
            case "\u30CD\u30AA": return "neo";
            case "\u30A4\u30B6\u30CA\u30DF": return "izanami";
            case "\u30DF\u30AF": return "miku";
            case "\u30AB\u30E8\u30B3": return "kayoko";
            case "\u30D5\u30AF\u30C8\u30AF": return "hukutoku";
            case "\u30E8\u30CD": return "yone";
            case "\u30C9\u30AF\u30B3": return "dokuko";
            default: return japaneseName != null ? japaneseName.ToLower() : "";
        }
    }
}
