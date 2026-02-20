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
    private UIE.VisualElement titleButtons;
    private UIE.Button langJaBtn;
    private UIE.Button langEnBtn;
    private bool hasProfile;

    void Start()
    {
        // デフォルト言語を設定（未設定の場合）
        if (!Localization.HasLanguageSet())
            Localization.SetLanguage("ja");

        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/TitleStyle" }, panelSettings);
        UIHelper.RegisterTapSE(root);

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

        // 全画面背景（装飾用）
        var bg = new UIE.VisualElement();
        bg.AddToClassList("bg-screen");
        root.Add(bg);

        // フレックスボックスのルートコンテナ
        var titleRoot = new UIE.VisualElement();
        titleRoot.AddToClassList("title-root");
        root.Add(titleRoot);

        // --- 上部: セーフエリア + devリセット ---
        var (safeTop, _, _, safeBottom) = UIHelper.GetSafeMargins();

        var topRow = new UIE.VisualElement();
        topRow.AddToClassList("title-top-row");
        topRow.style.paddingTop = 30 + safeTop;
        titleRoot.Add(topRow);

        CreateDevResetButton(topRow);

        // --- 中央: ロゴ + ボタン群 ---
        var center = new UIE.VisualElement();
        center.AddToClassList("title-center");
        titleRoot.Add(center);

        // タイトルロゴ
        var logoSprite = Resources.Load<Sprite>("UI/title-logo");
        if (logoSprite != null)
        {
            var logo = new UIE.VisualElement();
            logo.AddToClassList("title-logo");
            logo.style.backgroundImage = new UIE.StyleBackground(logoSprite);
            center.Add(logo);
        }

        // ボタン群コンテナ
        hasProfile = DataCarrier.HasProfile();

        titleButtons = new UIE.VisualElement();
        titleButtons.AddToClassList("title-buttons");
        center.Add(titleButtons);

        if (hasProfile)
        {
            CreateStartButton();
        }
        else
        {
            bool hasSave = DataCarrier.HasAnySaveData();
            if (hasSave)
            {
                CreateSaveDataButton();
                CreateStartButton();
            }
            else
            {
                CreateStartButton();
            }
        }

        // --- 下部: 言語ボタン ---
        var langPanel = new UIE.VisualElement();
        langPanel.AddToClassList("lang-panel");
        langPanel.style.paddingBottom = 40 + safeBottom;
        titleRoot.Add(langPanel);

        CreateLanguageButtons(langPanel);
    }

    void CreateStartButton()
    {
        var row = new UIE.VisualElement();
        row.AddToClassList("title-btn-row");

        var btn = new UIE.Button();
        btn.AddToClassList("pill-button");
        UIHelper.ApplyFont(btn);
        btn.text = Localization.Get(hasProfile ? "title_tap_start" : "title_new_game");
        btn.clicked += StartGame;
        row.Add(btn);

        titleButtons.Add(row);
    }

    void CreateSaveDataButton()
    {
        var row = new UIE.VisualElement();
        row.AddToClassList("title-btn-row");

        var btn = new UIE.Button();
        btn.AddToClassList("pill-button");
        UIHelper.ApplyFont(btn);
        btn.text = Localization.Get("title_save_data");
        btn.clicked += LoadFirstSaveAndGoHome;
        row.Add(btn);

        titleButtons.Add(row);
    }

    void CreateLanguageButtons(UIE.VisualElement panel)
    {
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

        // overlay-darkで全画面覆い、flexboxで中央配置
        saveDataListPanel = new UIE.VisualElement();
        saveDataListPanel.AddToClassList("overlay-dark");

        var panel = new UIE.VisualElement();
        panel.AddToClassList("save-panel");

        // Title
        var title = UIHelper.CreateLabel(Localization.Get("title_save_data_list"), "save-panel-title");
        panel.Add(title);

        // Slots container
        var slotsContainer = new UIE.VisualElement();
        slotsContainer.AddToClassList("save-panel-slots");
        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
            CreateSlotEntry(slotsContainer, i);
        panel.Add(slotsContainer);

        // Close button row
        var closeRow = new UIE.VisualElement();
        closeRow.AddToClassList("save-panel-close-row");

        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("close-btn-gray");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = Localization.Get("ui_close");
        closeBtn.clicked += CloseSaveDataList;
        closeRow.Add(closeBtn);

        panel.Add(closeRow);
        saveDataListPanel.Add(panel);
        root.Add(saveDataListPanel);
    }

    void CreateSlotEntry(UIE.VisualElement container, int slot)
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

        container.Add(row);
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
        // overlay-darkで全画面覆い、flexboxで中央配置
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

    void CreateDevResetButton(UIE.VisualElement parent)
    {
        var btn = new UIE.Button();
        btn.AddToClassList("dev-reset-btn");
        UIHelper.ApplyFont(btn);
        btn.text = Localization.Get("title_reset_profile");
        btn.clicked += () =>
        {
            DataCarrier.DeleteProfile();
            SceneManager.LoadScene("TitleScene");
        };
        parent.Add(btn);
    }

    private IEnumerator IntroSequence()
    {
        // 全画面オーバーレイ（アニメ用なのでabsolute維持）
        var panel = new UIE.VisualElement();
        panel.AddToClassList("intro-panel");
        root.Add(panel);

        // フレックスボックスでテキストを縦並びに配置
        var textsContainer = new UIE.VisualElement();
        textsContainer.AddToClassList("intro-texts");
        panel.Add(textsContainer);

        // Fade in
        yield return null;
        panel.AddToClassList("intro-panel-visible");

        yield return new WaitForSeconds(0.6f);

        for (int i = 0; i < introLines.Length; i++)
        {
            var textEl = new UIE.Label(introLines[i]);
            textEl.AddToClassList("intro-text");
            UIHelper.ApplyFont(textEl);
            textsContainer.Add(textEl);

            // Trigger slide-in next frame
            yield return null;
            textEl.AddToClassList("intro-text-visible");

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
