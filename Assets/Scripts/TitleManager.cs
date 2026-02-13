using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    private string[] introLines;

    private const float slideDuration = 1.5f;
    private const float lineInterval = 1.0f;
    private const float endWaitTime = 2.0f;
    private const float startOffsetX = -800f;

    // UI
    private GameObject saveDataButton;
    private GameObject saveDataListPanel;
    private Canvas mainCanvas;
    private GameObject langPanel;
    private TextMeshProUGUI jaText;
    private TextMeshProUGUI enText;
    private Image jaBg;
    private Image enBg;
    private TextMeshProUGUI startButtonText;
    private TextMeshProUGUI saveDataButtonText;
    private bool hasProfile;

    private const float BTN_WIDTH = 700f;
    private const float BTN_HEIGHT = 120f;
    private const int BTN_BLUR = 20;
    private static Sprite _pillSprite;
    private static Sprite _shadowSprite;

    void Start()
    {
        mainCanvas = FindObjectOfType<Canvas>();

        // CanvasScaler調整: 縦向き9:16、幅基準
        var canvasScaler = mainCanvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.matchWidthOrHeight = 0f;
        }

        // デフォルト言語を設定（未設定の場合）
        if (!Localization.HasLanguageSet())
        {
            Localization.SetLanguage("ja");
        }

        InitTitle();
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

        // 全画面背景 #f3f7f8
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

        // タイトルロゴ
        var logoSprite = Resources.Load<Sprite>("UI/title-logo");
        if (logoSprite != null)
        {
            var logoObj = new GameObject("TitleLogo");
            logoObj.transform.SetParent(mainCanvas.transform, false);
            var logoRect = logoObj.AddComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = new Vector2(0, 200);
            logoRect.sizeDelta = new Vector2(1040, 498);
            var logoImg = logoObj.AddComponent<Image>();
            logoImg.sprite = logoSprite;
            logoImg.preserveAspect = true;
            logoImg.raycastTarget = false;
        }

        int pillRadius = (int)(BTN_HEIGHT / 2);

        hasProfile = DataCarrier.HasProfile();

        if (hasProfile)
        {
            // プロフィール済み → 「タップでスタート」のみ表示
            StyleStartButton(-270, pillRadius);
        }
        else
        {
            // 未設定 → 「はじめから」表示、セーブがあれば「つづきから」も
            bool hasSave = DataCarrier.HasAnySaveData();
            float startY = hasSave ? -220 : -270;
            StyleStartButton(startY, pillRadius);

            if (hasSave)
            {
                float continueY = startY - BTN_HEIGHT - 40;
                CreateSaveDataButton(continueY, pillRadius);
            }
        }

        CreateLanguageButtons();
        CreateDevResetButton();
    }

    void CreateLanguageButtons()
    {
        var (safeLeft, safeRight, safeTop, safeBottom) = SafeAreaHelper.GetSafeAreaInsets(mainCanvas);

        langPanel = new GameObject("LanguagePanel");
        langPanel.transform.SetParent(mainCanvas.transform, false);

        var panelRect = langPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 40 + safeBottom);
        panelRect.sizeDelta = new Vector2(200, 36);

        // Japanese button
        var jaBtn = new GameObject("LangBtn_JA");
        jaBtn.transform.SetParent(langPanel.transform, false);
        var jaRect = jaBtn.AddComponent<RectTransform>();
        jaRect.anchorMin = new Vector2(0.5f, 0.5f);
        jaRect.anchorMax = new Vector2(0.5f, 0.5f);
        jaRect.anchoredPosition = new Vector2(-52, 0);
        jaRect.sizeDelta = new Vector2(90, 36);
        jaBg = jaBtn.AddComponent<Image>();
        var jaBtnComp = jaBtn.AddComponent<Button>();
        jaBtnComp.targetGraphic = jaBg;
        jaBtnComp.onClick.AddListener(() => SwitchLanguage("ja"));
        var jaTextObj = new GameObject("Text");
        jaTextObj.transform.SetParent(jaBtn.transform, false);
        var jaTextRect = jaTextObj.AddComponent<RectTransform>();
        jaTextRect.anchorMin = Vector2.zero;
        jaTextRect.anchorMax = Vector2.one;
        jaTextRect.offsetMin = Vector2.zero;
        jaTextRect.offsetMax = Vector2.zero;
        jaText = jaTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(jaText);
        jaText.text = "日本語";
        jaText.fontSize = 20;
        jaText.alignment = TextAlignmentOptions.Center;
        jaText.fontStyle = FontStyles.Bold;
        jaText.raycastTarget = false;

        // English button
        var enBtn = new GameObject("LangBtn_EN");
        enBtn.transform.SetParent(langPanel.transform, false);
        var enRect = enBtn.AddComponent<RectTransform>();
        enRect.anchorMin = new Vector2(0.5f, 0.5f);
        enRect.anchorMax = new Vector2(0.5f, 0.5f);
        enRect.anchoredPosition = new Vector2(52, 0);
        enRect.sizeDelta = new Vector2(90, 36);
        enBg = enBtn.AddComponent<Image>();
        var enBtnComp = enBtn.AddComponent<Button>();
        enBtnComp.targetGraphic = enBg;
        enBtnComp.onClick.AddListener(() => SwitchLanguage("en"));
        var enTextObj = new GameObject("Text");
        enTextObj.transform.SetParent(enBtn.transform, false);
        var enTextRect = enTextObj.AddComponent<RectTransform>();
        enTextRect.anchorMin = Vector2.zero;
        enTextRect.anchorMax = Vector2.one;
        enTextRect.offsetMin = Vector2.zero;
        enTextRect.offsetMax = Vector2.zero;
        enText = enTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(enText);
        enText.text = "English";
        enText.fontSize = 20;
        enText.alignment = TextAlignmentOptions.Center;
        enText.fontStyle = FontStyles.Bold;
        enText.raycastTarget = false;

        UpdateLanguageButtonColors();
    }

    void SwitchLanguage(string lang)
    {
        Localization.SetLanguage(lang);
        UpdateLanguageButtonColors();

        // introLinesを更新
        introLines = new string[]
        {
            Localization.Get("intro_line1"),
            Localization.Get("intro_line2"),
            Localization.Get("intro_line3"),
            Localization.Get("intro_line4")
        };

        // ボタンテキスト更新
        if (startButtonText != null)
            startButtonText.text = Localization.Get(hasProfile ? "title_tap_start" : "title_new_game");
        if (saveDataButtonText != null)
            saveDataButtonText.text = Localization.Get("title_save_data");
    }

    void UpdateLanguageButtonColors()
    {
        string current = Localization.CurrentLanguage;
        Color activeColor = new Color(0.3f, 0.5f, 0.8f);
        Color inactiveColor = new Color(0.25f, 0.25f, 0.3f);

        jaBg.color = (current == "ja") ? activeColor : inactiveColor;
        jaText.color = (current == "ja") ? Color.white : new Color(0.6f, 0.6f, 0.6f);
        enBg.color = (current == "en") ? activeColor : inactiveColor;
        enText.color = (current == "en") ? Color.white : new Color(0.6f, 0.6f, 0.6f);
    }

    public void StartGame()
    {
        if (hasProfile)
        {
            // プロフィール済み → HomeSceneへ直行
            if (DataCarrier.Instance != null)
                DataCarrier.Instance.LoadProfile();
            SceneManager.LoadScene("HomeScene");
        }
        else
        {
            // 新規 → イントロ → ProfileScene
            if (DataCarrier.Instance != null)
            {
                DataCarrier.Instance.currentSlot = -1;
            }
            StartCoroutine(IntroSequence());
        }
    }

    void CreateSaveDataButton(float yPos, int pillRadius)
    {
        if (mainCanvas == null) return;

        saveDataButton = new GameObject("SaveDataButton");
        saveDataButton.transform.SetParent(mainCanvas.transform, false);

        var btnRect = saveDataButton.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0, yPos);
        btnRect.sizeDelta = new Vector2(BTN_WIDTH, BTN_HEIGHT);

        var btnBg = saveDataButton.AddComponent<Image>();
        btnBg.sprite = GetPillSprite(pillRadius);
        btnBg.type = Image.Type.Sliced;
        btnBg.color = Color.white;

        var btn = saveDataButton.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        btn.onClick.AddListener(ShowSaveDataList);
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        // Box shadow（ボタンの子要素、中央透明で外周のみ影）
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(saveDataButton.transform, false);
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

        // テキスト
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(saveDataButton.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        saveDataButtonText = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(saveDataButtonText);
        saveDataButtonText.text = Localization.Get("title_save_data");
        saveDataButtonText.fontSize = 36;
        saveDataButtonText.alignment = TextAlignmentOptions.Center;
        saveDataButtonText.color = new Color(0.45f, 0.45f, 0.5f);
        saveDataButtonText.fontStyle = FontStyles.Bold;
        saveDataButtonText.raycastTarget = false;

        // 押下時スケールアニメーション
        AddPressAnimation(saveDataButton);
    }

    void ShowSaveDataList()
    {
        if (saveDataListPanel != null)
        {
            Destroy(saveDataListPanel);
            saveDataListPanel = null;
            return;
        }

        saveDataListPanel = new GameObject("SaveDataListPanel");
        saveDataListPanel.transform.SetParent(mainCanvas.transform, false);

        var panelRect = saveDataListPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(500, 450);

        var panelBg = saveDataListPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.2f, 0.98f);

        // タイトル
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(saveDataListPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -25);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = Localization.Get("title_save_data_list");
        titleText.fontSize = 30;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        // スロット一覧
        float slotStartY = -70;
        float slotHeight = 70;

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            CreateSlotEntry(i, slotStartY - (i * slotHeight));
        }

        // 閉じるボタン
        var closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(saveDataListPanel.transform, false);
        var closeRect = closeBtn.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(0.5f, 0);
        closeRect.anchorMax = new Vector2(0.5f, 0);
        closeRect.anchoredPosition = new Vector2(0, 35);
        closeRect.sizeDelta = new Vector2(150, 45);

        var closeBg = closeBtn.AddComponent<Image>();
        closeBg.color = new Color(0.5f, 0.5f, 0.5f);

        var closeBtnComp = closeBtn.AddComponent<Button>();
        closeBtnComp.targetGraphic = closeBg;
        closeBtnComp.onClick.AddListener(CloseSaveDataList);

        var closeTextObj = new GameObject("Text");
        closeTextObj.transform.SetParent(closeBtn.transform, false);
        var closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        var closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(closeText);
        closeText.text = Localization.Get("ui_close");
        closeText.fontSize = 22;
        closeText.alignment = TextAlignmentOptions.Center;
        closeText.color = Color.white;
        closeText.raycastTarget = false;
    }

    void CreateSlotEntry(int slot, float yPos)
    {
        bool exists = DataCarrier.SlotExists(slot);

        var slotObj = new GameObject($"Slot{slot}");
        slotObj.transform.SetParent(saveDataListPanel.transform, false);
        var slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0, 1);
        slotRect.anchorMax = new Vector2(1, 1);
        slotRect.anchoredPosition = new Vector2(0, yPos);
        slotRect.sizeDelta = new Vector2(-40, 60);

        var slotBg = slotObj.AddComponent<Image>();
        slotBg.color = exists ? new Color(0.2f, 0.25f, 0.35f) : new Color(0.15f, 0.15f, 0.2f);

        if (exists)
        {
            string babyName = DataCarrier.GetSlotBabyName(slot);
            int age = DataCarrier.GetSlotAge(slot);
            string fatherName = DataCarrier.GetSlotFatherName(slot);
            string motherName = DataCarrier.GetSlotMotherName(slot);
            bool isGodBaby = DataCarrier.GetSlotIsGodBaby(slot);

            // 赤ちゃん情報
            var infoObj = new GameObject("Info");
            infoObj.transform.SetParent(slotObj.transform, false);
            var infoRect = infoObj.AddComponent<RectTransform>();
            infoRect.anchorMin = new Vector2(0, 0);
            infoRect.anchorMax = new Vector2(0.6f, 1);
            infoRect.offsetMin = new Vector2(15, 5);
            infoRect.offsetMax = new Vector2(0, -5);
            var infoText = infoObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(infoText);

            string nameColor = isGodBaby ? "#FFD700" : "#FFFFFF";
            string fatherDisplay = Localization.GetParent(fatherName);
            string motherDisplay = Localization.GetParent(motherName);
            infoText.text = Localization.Get("title_slot_info", nameColor, babyName, age, fatherDisplay, motherDisplay);
            infoText.fontSize = 20;
            infoText.alignment = TextAlignmentOptions.Left;
            infoText.color = Color.white;
            infoText.raycastTarget = false;

            // ロードボタン
            var loadBtn = new GameObject("LoadBtn");
            loadBtn.transform.SetParent(slotObj.transform, false);
            var loadRect = loadBtn.AddComponent<RectTransform>();
            loadRect.anchorMin = new Vector2(1, 0.5f);
            loadRect.anchorMax = new Vector2(1, 0.5f);
            loadRect.anchoredPosition = new Vector2(-100, 0);
            loadRect.sizeDelta = new Vector2(70, 40);

            var loadBg = loadBtn.AddComponent<Image>();
            loadBg.color = new Color(0.3f, 0.6f, 0.4f);

            int slotIndex = slot; // キャプチャ用
            var loadBtnComp = loadBtn.AddComponent<Button>();
            loadBtnComp.targetGraphic = loadBg;
            loadBtnComp.onClick.AddListener(() => LoadSlot(slotIndex));

            var loadTextObj = new GameObject("Text");
            loadTextObj.transform.SetParent(loadBtn.transform, false);
            var loadTextRect = loadTextObj.AddComponent<RectTransform>();
            loadTextRect.anchorMin = Vector2.zero;
            loadTextRect.anchorMax = Vector2.one;
            loadTextRect.offsetMin = Vector2.zero;
            loadTextRect.offsetMax = Vector2.zero;
            var loadText = loadTextObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(loadText);
            loadText.text = Localization.Get("ui_load");
            loadText.fontSize = 18;
            loadText.alignment = TextAlignmentOptions.Center;
            loadText.color = Color.white;
            loadText.raycastTarget = false;

            // 削除ボタン
            var delBtn = new GameObject("DeleteBtn");
            delBtn.transform.SetParent(slotObj.transform, false);
            var delRect = delBtn.AddComponent<RectTransform>();
            delRect.anchorMin = new Vector2(1, 0.5f);
            delRect.anchorMax = new Vector2(1, 0.5f);
            delRect.anchoredPosition = new Vector2(-25, 0);
            delRect.sizeDelta = new Vector2(40, 40);

            var delBg = delBtn.AddComponent<Image>();
            delBg.color = new Color(0.7f, 0.3f, 0.3f);

            var delBtnComp = delBtn.AddComponent<Button>();
            delBtnComp.targetGraphic = delBg;
            delBtnComp.onClick.AddListener(() => ConfirmDeleteSlot(slotIndex));

            var delTextObj = new GameObject("Text");
            delTextObj.transform.SetParent(delBtn.transform, false);
            var delTextRect = delTextObj.AddComponent<RectTransform>();
            delTextRect.anchorMin = Vector2.zero;
            delTextRect.anchorMax = Vector2.one;
            delTextRect.offsetMin = Vector2.zero;
            delTextRect.offsetMax = Vector2.zero;
            var delText = delTextObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(delText);
            delText.text = "×";
            delText.fontSize = 24;
            delText.alignment = TextAlignmentOptions.Center;
            delText.color = Color.white;
            delText.fontStyle = FontStyles.Bold;
            delText.raycastTarget = false;
        }
        else
        {
            // 空スロット表示
            var emptyObj = new GameObject("Empty");
            emptyObj.transform.SetParent(slotObj.transform, false);
            var emptyRect = emptyObj.AddComponent<RectTransform>();
            emptyRect.anchorMin = Vector2.zero;
            emptyRect.anchorMax = Vector2.one;
            emptyRect.offsetMin = new Vector2(15, 0);
            emptyRect.offsetMax = new Vector2(-15, 0);
            var emptyText = emptyObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(emptyText);
            emptyText.text = Localization.Get("title_slot_empty", slot + 1);
            emptyText.fontSize = 20;
            emptyText.alignment = TextAlignmentOptions.Left;
            emptyText.color = new Color(0.5f, 0.5f, 0.5f);
            emptyText.raycastTarget = false;
        }
    }

    void LoadSlot(int slot)
    {
        if (DataCarrier.Instance == null)
        {
            GameObject carrierObj = new GameObject("DataCarrier");
            carrierObj.AddComponent<DataCarrier>();
        }

        DataCarrier.Instance.LoadFromSlot(slot);

        // 最初のボスを倒済み（defeatedEnemies > 0）→ マップへ復帰
        // まだ倒していない → バトルシーンへ
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

    void ConfirmDeleteSlot(int slot)
    {
        // 削除確認パネル
        var confirmPanel = new GameObject("ConfirmDelete");
        confirmPanel.transform.SetParent(mainCanvas.transform, false);

        var panelRect = confirmPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(350, 150);

        var panelBg = confirmPanel.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.1f, 0.1f, 0.98f);

        // メッセージ
        var msgObj = new GameObject("Message");
        msgObj.transform.SetParent(confirmPanel.transform, false);
        var msgRect = msgObj.AddComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0, 0.5f);
        msgRect.anchorMax = new Vector2(1, 1);
        msgRect.offsetMin = new Vector2(10, 10);
        msgRect.offsetMax = new Vector2(-10, -10);
        var msgText = msgObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(msgText);
        msgText.text = Localization.Get("title_confirm_delete", slot + 1);
        msgText.fontSize = 22;
        msgText.alignment = TextAlignmentOptions.Center;
        msgText.color = Color.white;
        msgText.raycastTarget = false;

        // はいボタン
        var yesBtn = new GameObject("YesBtn");
        yesBtn.transform.SetParent(confirmPanel.transform, false);
        var yesRect = yesBtn.AddComponent<RectTransform>();
        yesRect.anchorMin = new Vector2(0.5f, 0);
        yesRect.anchorMax = new Vector2(0.5f, 0);
        yesRect.anchoredPosition = new Vector2(-60, 35);
        yesRect.sizeDelta = new Vector2(90, 40);

        var yesBg = yesBtn.AddComponent<Image>();
        yesBg.color = new Color(0.7f, 0.3f, 0.3f);

        var yesBtnComp = yesBtn.AddComponent<Button>();
        yesBtnComp.targetGraphic = yesBg;
        yesBtnComp.onClick.AddListener(() => {
            DataCarrier.DeleteSlot(slot);
            Destroy(confirmPanel);
            RefreshSaveDataList();
        });

        var yesTextObj = new GameObject("Text");
        yesTextObj.transform.SetParent(yesBtn.transform, false);
        var yesTextRect = yesTextObj.AddComponent<RectTransform>();
        yesTextRect.anchorMin = Vector2.zero;
        yesTextRect.anchorMax = Vector2.one;
        yesTextRect.offsetMin = Vector2.zero;
        yesTextRect.offsetMax = Vector2.zero;
        var yesText = yesTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(yesText);
        yesText.text = Localization.Get("ui_delete");
        yesText.fontSize = 20;
        yesText.alignment = TextAlignmentOptions.Center;
        yesText.color = Color.white;
        yesText.raycastTarget = false;

        // いいえボタン
        var noBtn = new GameObject("NoBtn");
        noBtn.transform.SetParent(confirmPanel.transform, false);
        var noRect = noBtn.AddComponent<RectTransform>();
        noRect.anchorMin = new Vector2(0.5f, 0);
        noRect.anchorMax = new Vector2(0.5f, 0);
        noRect.anchoredPosition = new Vector2(60, 35);
        noRect.sizeDelta = new Vector2(90, 40);

        var noBg = noBtn.AddComponent<Image>();
        noBg.color = new Color(0.4f, 0.4f, 0.5f);

        var noBtnComp = noBtn.AddComponent<Button>();
        noBtnComp.targetGraphic = noBg;
        noBtnComp.onClick.AddListener(() => Destroy(confirmPanel));

        var noTextObj = new GameObject("Text");
        noTextObj.transform.SetParent(noBtn.transform, false);
        var noTextRect = noTextObj.AddComponent<RectTransform>();
        noTextRect.anchorMin = Vector2.zero;
        noTextRect.anchorMax = Vector2.one;
        noTextRect.offsetMin = Vector2.zero;
        noTextRect.offsetMax = Vector2.zero;
        var noText = noTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(noText);
        noText.text = Localization.Get("ui_cancel");
        noText.fontSize = 20;
        noText.alignment = TextAlignmentOptions.Center;
        noText.color = Color.white;
        noText.raycastTarget = false;
    }

    void RefreshSaveDataList()
    {
        if (saveDataListPanel != null)
        {
            Destroy(saveDataListPanel);
            saveDataListPanel = null;
        }

        // セーブデータがなくなったらボタンも消す（shadowは子要素なので一緒に消える）
        if (!DataCarrier.HasAnySaveData())
        {
            if (saveDataButton != null)
            {
                Destroy(saveDataButton);
                saveDataButton = null;
            }
        }
        else
        {
            ShowSaveDataList();
        }
    }

    void CloseSaveDataList()
    {
        if (saveDataListPanel != null)
        {
            Destroy(saveDataListPanel);
            saveDataListPanel = null;
        }
    }

    void CreateDevResetButton()
    {
        var (safeLeft, safeRight, safeTop, safeBottom) = SafeAreaHelper.GetSafeAreaInsets(mainCanvas);

        var btnObj = new GameObject("DevResetButton");
        btnObj.transform.SetParent(mainCanvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-100, -30 - safeTop);
        btnRect.sizeDelta = new Vector2(180, 40);

        var btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnBg;
        btn.onClick.AddListener(() =>
        {
            DataCarrier.DeleteProfile();
            SceneManager.LoadScene("TitleScene");
        });

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var text = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(text);
        text.text = Localization.Get("title_reset_profile");
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.4f, 0.4f);
        text.raycastTarget = false;
    }

    void StyleStartButton(float yPos, int pillRadius)
    {
        var button = gameObject;

        // サイズ・位置変更
        var rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(0, yPos);
            rect.sizeDelta = new Vector2(BTN_WIDTH, BTN_HEIGHT);
        }

        // 既存の子オブジェクトを削除
        for (int i = button.transform.childCount - 1; i >= 0; i--)
            Destroy(button.transform.GetChild(i).gameObject);

        // 背景Image: pill shape, 白背景
        var img = button.GetComponent<Image>();
        if (img != null)
        {
            img.enabled = true;
            img.sprite = GetPillSprite(pillRadius);
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        // Box shadow（ボタンの子要素、中央透明で外周のみ影）
        var shadowObj = new GameObject("Shadow");
        shadowObj.transform.SetParent(button.transform, false);
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

        // テキスト
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        startButtonText = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(startButtonText);
        startButtonText.text = Localization.Get(hasProfile ? "title_tap_start" : "title_new_game");
        startButtonText.fontSize = 36;
        startButtonText.alignment = TextAlignmentOptions.Center;
        startButtonText.color = new Color(0.45f, 0.45f, 0.5f);
        startButtonText.fontStyle = FontStyles.Bold;
        startButtonText.raycastTarget = false;

        // Button設定
        var btn = button.GetComponent<Button>();
        if (btn != null)
        {
            btn.targetGraphic = img;
            btn.navigation = new Navigation { mode = Navigation.Mode.None };
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
        }

        AddPressAnimation(button);
    }

    void AddPressAnimation(GameObject button)
    {
        var trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

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

    // Pill型スプライト（border-radius: 50%）
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

    // Box shadow用スプライト（ぼかし付きpill型影）
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
                // SDFで円（pill形状の角部分）からの距離を計算
                float dist = Mathf.Sqrt(px * px + py * py) - radius;

                float alpha;
                if (dist <= 0f)
                    alpha = 0f; // 中央は透明（ボタン白背景と重なる部分）
                else if (dist >= blur)
                    alpha = 0f;
                else
                {
                    float t = dist / blur;
                    alpha = (1f - t) * (1f - t); // 二次関数で自然なフォールオフ
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

    private IEnumerator IntroSequence()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("IntroCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        GameObject panel = new GameObject("IntroPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0f);

        float fadeInDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.9f, elapsed / fadeInDuration);
            panelImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);

        float verticalStart = 100f;
        float lineSpacing = 120f;

        for (int i = 0; i < introLines.Length; i++)
        {
            GameObject textObj = new GameObject("IntroText_" + i);
            textObj.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(tmp);

            tmp.text = introLines[i];
            tmp.fontSize = 36;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;

            float yPos = verticalStart - (i * lineSpacing);
            textRect.sizeDelta = new Vector2(800f, 100f);
            textRect.anchoredPosition = new Vector2(startOffsetX, yPos);

            float slideElapsed = 0f;
            Vector2 startPos = new Vector2(startOffsetX, yPos);
            Vector2 endPos = new Vector2(0f, yPos);

            while (slideElapsed < slideDuration)
            {
                slideElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, slideElapsed / slideDuration);
                textRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            textRect.anchoredPosition = endPos;

            if (i < introLines.Length - 1)
            {
                yield return new WaitForSeconds(lineInterval);
            }
        }

        yield return new WaitForSeconds(endWaitTime);

        float fadeOutDuration = 1.0f;
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        if (DataCarrier.HasProfile())
        {
            // プロフィール登録済み → グローバルプロフィールを読み込んでBirthSceneへ
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
