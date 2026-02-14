using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UIElements;
using UIE = UnityEngine.UIElements;

public class TitleManager : MonoBehaviour
{
    private string[] introLines;

    private UIE.PanelSettings panelSettings;
    private UIE.VisualElement root;

    // UI references
    private UIE.VisualElement saveDataListPanel;
    private UIE.Button langJaBtn;
    private UIE.Button langEnBtn;
    private UIE.Label startButtonLabel;
    private UIE.Label saveDataButtonLabel;
    private bool hasProfile;

    void Start()
    {
        // デフォルト言語を設定（未設定の場合）
        if (!Localization.HasLanguageSet())
            Localization.SetLanguage("ja");

        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/TitleStyle" }, panelSettings);

        InitTitle();
    }

    void OnDestroy()
    {
        if (panelSettings != null)
            Destroy(panelSettings);
    }

    void InitTitle()
    {
        introLines = new string[]
        {
            Localization.Get("intro_line1"),
            Localization.Get("intro_line2"),
            Localization.Get("intro_line3"),
            Localization.Get("intro_line4")
        };

        // 全画面背景
        var bg = new UIE.VisualElement();
        bg.AddToClassList("bg-screen");
        root.Add(bg);

        // タイトルロゴ
        var logoSprite = Resources.Load<Sprite>("UI/title-logo");
        if (logoSprite != null)
        {
            var logo = new UIE.VisualElement();
            logo.AddToClassList("title-logo");
            logo.style.backgroundImage = new UIE.StyleBackground(logoSprite);
            logo.style.position = UIE.Position.Absolute;
            logo.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            logo.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            logo.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent),
                    new UIE.Length(-70, UIE.LengthUnit.Percent)));
            root.Add(logo);
        }

        hasProfile = DataCarrier.HasProfile();

        if (hasProfile)
        {
            CreateStartButton(200);
        }
        else
        {
            bool hasSave = DataCarrier.HasAnySaveData();
            if (hasSave)
            {
                CreateSaveDataButton(200);
                CreateStartButton(200 + 120 + 40);
            }
            else
            {
                CreateStartButton(280);
            }
        }

        CreateLanguageButtons();
        CreateDevResetButton();
    }

    void CreateStartButton(float yOffset)
    {
        var wrapper = new UIE.VisualElement();
        wrapper.style.position = UIE.Position.Absolute;
        wrapper.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        wrapper.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        wrapper.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), yOffset));

        // Shadow
        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        wrapper.Add(shadow);

        var btn = new UIE.Button();
        btn.AddToClassList("pill-button");
        UIHelper.ApplyFont(btn);
        startButtonLabel = btn.Q<UIE.Label>();

        btn.text = Localization.Get(hasProfile ? "title_tap_start" : "title_new_game");
        btn.clicked += StartGame;
        wrapper.Add(btn);

        // Store label reference for language switch
        startButtonLabel = btn.Q<UIE.TextElement>() as UIE.Label;

        root.Add(wrapper);
    }

    void CreateSaveDataButton(float yOffset)
    {
        var wrapper = new UIE.VisualElement();
        wrapper.style.position = UIE.Position.Absolute;
        wrapper.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        wrapper.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        wrapper.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), yOffset));

        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        wrapper.Add(shadow);

        var btn = new UIE.Button();
        btn.AddToClassList("pill-button");
        UIHelper.ApplyFont(btn);
        btn.text = Localization.Get("title_save_data");
        btn.clicked += LoadFirstSaveAndGoHome;
        wrapper.Add(btn);

        saveDataButtonLabel = btn.Q<UIE.TextElement>() as UIE.Label;

        root.Add(wrapper);
    }

    void CreateLanguageButtons()
    {
        var (_, _, safeTop, safeBottom) = UIHelper.GetSafeMargins();

        var panel = new UIE.VisualElement();
        panel.AddToClassList("lang-panel");
        panel.style.bottom = 40 + safeBottom;

        langJaBtn = new UIE.Button();
        langJaBtn.AddToClassList("lang-btn");
        UIHelper.ApplyFont(langJaBtn);
        langJaBtn.text = "日本語";
        langJaBtn.clicked += () => SwitchLanguage("ja");
        panel.Add(langJaBtn);

        langEnBtn = new UIE.Button();
        langEnBtn.AddToClassList("lang-btn");
        UIHelper.ApplyFont(langEnBtn);
        langEnBtn.text = "English";
        langEnBtn.clicked += () => SwitchLanguage("en");
        panel.Add(langEnBtn);

        UpdateLanguageButtonColors();
        root.Add(panel);
    }

    void SwitchLanguage(string lang)
    {
        Localization.SetLanguage(lang);
        UpdateLanguageButtonColors();

        introLines = new string[]
        {
            Localization.Get("intro_line1"),
            Localization.Get("intro_line2"),
            Localization.Get("intro_line3"),
            Localization.Get("intro_line4")
        };

        // Update button texts by finding all pill-buttons
        var buttons = root.Query<UIE.Button>(className: "pill-button").ToList();
        if (buttons.Count > 0)
            buttons[0].text = Localization.Get(hasProfile ? "title_tap_start" : "title_new_game");
        if (buttons.Count > 1)
            buttons[1].text = Localization.Get("title_save_data");
    }

    void UpdateLanguageButtonColors()
    {
        string current = Localization.CurrentLanguage;

        langJaBtn.RemoveFromClassList("lang-btn-active");
        langJaBtn.RemoveFromClassList("lang-btn-inactive");
        langEnBtn.RemoveFromClassList("lang-btn-active");
        langEnBtn.RemoveFromClassList("lang-btn-inactive");

        langJaBtn.AddToClassList(current == "ja" ? "lang-btn-active" : "lang-btn-inactive");
        langEnBtn.AddToClassList(current == "en" ? "lang-btn-active" : "lang-btn-inactive");
    }

    public void StartGame()
    {
        if (hasProfile)
        {
            if (DataCarrier.Instance != null)
                DataCarrier.Instance.LoadProfile();
            SceneManager.LoadScene("HomeScene");
        }
        else
        {
            if (DataCarrier.Instance != null)
                DataCarrier.Instance.currentSlot = -1;
            StartCoroutine(IntroSequence());
        }
    }

    void ShowSaveDataList()
    {
        if (saveDataListPanel != null)
        {
            saveDataListPanel.RemoveFromHierarchy();
            saveDataListPanel = null;
            return;
        }

        saveDataListPanel = new UIE.VisualElement();
        saveDataListPanel.style.position = UIE.Position.Absolute;
        saveDataListPanel.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        saveDataListPanel.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
        saveDataListPanel.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent),
                new UIE.Length(-50, UIE.LengthUnit.Percent)));
        saveDataListPanel.AddToClassList("save-panel");

        // Title
        var title = UIHelper.CreateLabel(Localization.Get("title_save_data_list"), "save-panel-title");
        saveDataListPanel.Add(title);

        // Slots
        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
            CreateSlotEntry(i);

        // Close button
        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("close-btn-gray");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = Localization.Get("ui_close");
        closeBtn.clicked += CloseSaveDataList;
        saveDataListPanel.Add(closeBtn);

        root.Add(saveDataListPanel);
    }

    void CreateSlotEntry(int slot)
    {
        bool exists = DataCarrier.SlotExists(slot);

        var row = new UIE.VisualElement();
        row.AddToClassList("slot-row");
        row.AddToClassList(exists ? "slot-row-exists" : "slot-row-empty");

        if (exists)
        {
            string babyName = DataCarrier.GetSlotBabyName(slot);
            int age = DataCarrier.GetSlotAge(slot);
            string fatherName = DataCarrier.GetSlotFatherName(slot);
            string motherName = DataCarrier.GetSlotMotherName(slot);
            bool isGodBaby = DataCarrier.GetSlotIsGodBaby(slot);

            string nameColor = isGodBaby ? "#FFD700" : "#FFFFFF";
            string fatherDisplay = Localization.GetParent(fatherName);
            string motherDisplay = Localization.GetParent(motherName);

            var info = UIHelper.CreateLabel(
                Localization.Get("title_slot_info", nameColor, babyName, age, fatherDisplay, motherDisplay),
                "slot-info");
            info.enableRichText = true;
            row.Add(info);

            int slotIndex = slot;

            var loadBtn = new UIE.Button();
            loadBtn.AddToClassList("slot-load-btn");
            UIHelper.ApplyFont(loadBtn);
            loadBtn.text = Localization.Get("ui_load");
            loadBtn.clicked += () => LoadSlot(slotIndex);
            row.Add(loadBtn);

            var delBtn = new UIE.Button();
            delBtn.AddToClassList("slot-del-btn");
            UIHelper.ApplyFont(delBtn);
            delBtn.text = "\u00d7";
            delBtn.clicked += () => ConfirmDeleteSlot(slotIndex);
            row.Add(delBtn);
        }
        else
        {
            var empty = UIHelper.CreateLabel(
                Localization.Get("title_slot_empty", slot + 1), "slot-empty-text");
            row.Add(empty);
        }

        saveDataListPanel.Add(row);
    }

    void LoadFirstSaveAndGoHome()
    {
        if (DataCarrier.Instance == null)
        {
            GameObject carrierObj = new GameObject("DataCarrier");
            carrierObj.AddComponent<DataCarrier>();
        }

        // 最初に見つかったセーブスロットをロード
        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            if (DataCarrier.SlotExists(i))
            {
                DataCarrier.Instance.LoadFromSlot(i);
                break;
            }
        }

        SceneManager.LoadScene("HomeScene");
    }

    void LoadSlot(int slot)
    {
        if (DataCarrier.Instance == null)
        {
            GameObject carrierObj = new GameObject("DataCarrier");
            carrierObj.AddComponent<DataCarrier>();
        }

        DataCarrier.Instance.LoadFromSlot(slot);
        SceneManager.LoadScene("HomeScene");
    }

    void ConfirmDeleteSlot(int slot)
    {
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("overlay-dark");

        var panel = new UIE.VisualElement();
        panel.AddToClassList("confirm-panel");

        var msg = UIHelper.CreateLabel(Localization.Get("title_confirm_delete", slot + 1), "confirm-msg");
        panel.Add(msg);

        var btnRow = new UIE.VisualElement();
        btnRow.AddToClassList("confirm-btn-row");

        var yesBtn = new UIE.Button();
        yesBtn.AddToClassList("confirm-yes");
        UIHelper.ApplyFont(yesBtn);
        yesBtn.text = Localization.Get("ui_delete");
        yesBtn.clicked += () =>
        {
            DataCarrier.DeleteSlot(slot);
            overlay.RemoveFromHierarchy();
            RefreshSaveDataList();
        };
        btnRow.Add(yesBtn);

        var noBtn = new UIE.Button();
        noBtn.AddToClassList("confirm-no");
        UIHelper.ApplyFont(noBtn);
        noBtn.text = Localization.Get("ui_cancel");
        noBtn.clicked += () => overlay.RemoveFromHierarchy();
        btnRow.Add(noBtn);

        panel.Add(btnRow);
        overlay.Add(panel);
        root.Add(overlay);
    }

    void RefreshSaveDataList()
    {
        if (saveDataListPanel != null)
        {
            saveDataListPanel.RemoveFromHierarchy();
            saveDataListPanel = null;
        }

        if (DataCarrier.HasAnySaveData())
            ShowSaveDataList();
    }

    void CloseSaveDataList()
    {
        if (saveDataListPanel != null)
        {
            saveDataListPanel.RemoveFromHierarchy();
            saveDataListPanel = null;
        }
    }

    void CreateDevResetButton()
    {
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        var btn = new UIE.Button();
        btn.AddToClassList("dev-reset-btn");
        UIHelper.ApplyFont(btn);
        btn.style.top = 30 + safeTop;
        btn.text = Localization.Get("title_reset_profile");
        btn.clicked += () =>
        {
            DataCarrier.DeleteProfile();
            SceneManager.LoadScene("TitleScene");
        };
        root.Add(btn);
    }

    private IEnumerator IntroSequence()
    {
        // Dark overlay panel
        var panel = new UIE.VisualElement();
        panel.AddToClassList("intro-panel");
        root.Add(panel);

        // Fade in
        yield return null; // wait one frame for style to apply
        panel.AddToClassList("intro-panel-visible");

        yield return new WaitForSeconds(0.6f);

        float lineSpacing = 120f;
        float startY = -((introLines.Length - 1) * lineSpacing) / 2f;

        for (int i = 0; i < introLines.Length; i++)
        {
            var textEl = new UIE.Label(introLines[i]);
            textEl.AddToClassList("intro-text");
            UIHelper.ApplyFont(textEl);
            textEl.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            textEl.style.position = UIE.Position.Absolute;
            textEl.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            float yPos = startY + i * lineSpacing;
            textEl.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(-800, yPos));
            panel.Add(textEl);

            // Trigger slide-in next frame
            yield return null;
            textEl.AddToClassList("intro-text-visible");
            textEl.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), yPos));

            if (i < introLines.Length - 1)
                yield return new WaitForSeconds(2.5f);
        }

        yield return new WaitForSeconds(2.0f);

        // Fade out
        panel.style.opacity = 0f;
        panel.style.transitionProperty = new System.Collections.Generic.List<UIE.StylePropertyName>
            { new UIE.StylePropertyName("opacity") };
        panel.style.transitionDuration = new System.Collections.Generic.List<UIE.TimeValue>
            { new UIE.TimeValue(1f) };
        panel.style.opacity = 0f;

        yield return new WaitForSeconds(1.2f);

        if (DataCarrier.HasProfile())
        {
            if (DataCarrier.Instance != null)
                DataCarrier.Instance.LoadProfile();
            SceneManager.LoadScene("BirthScene");
        }
        else
        {
            SceneManager.LoadScene("ProfileScene");
        }
    }
}
