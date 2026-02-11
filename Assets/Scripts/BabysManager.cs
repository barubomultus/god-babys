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
        float titleY = -100f;
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mainCanvas.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, titleY);
        titleRect.sizeDelta = new Vector2(800, 70);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "babys";
        titleText.fontSize = 44;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.15f, 0.15f, 0.15f);
        titleText.raycastTarget = false;

        // Back button
        float backY = -100f;
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
        backText.text = "< " + Localization.Get("ui_back");
        backText.fontSize = 28;
        backText.alignment = TextAlignmentOptions.Center;
        backText.color = new Color(0.3f, 0.5f, 0.9f);
        backText.fontStyle = FontStyles.Bold;
        backText.raycastTarget = false;

        // Save slot list
        float slotStartY = titleY - 120f;
        float slotHeight = 220f;
        float slotGap = 20f;
        int currentSlot = dc != null ? dc.currentSlot : -1;

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            CreateSlotEntry(i, slotStartY - i * (slotHeight + slotGap), currentSlot == i);
        }
    }

    void CreateSlotEntry(int slot, float yPos, bool isCurrent)
    {
        bool exists = DataCarrier.SlotExists(slot);

        var cardObj = new GameObject($"SlotCard{slot}");
        cardObj.transform.SetParent(mainCanvas.transform, false);
        var cardRect = cardObj.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 1f);
        cardRect.anchorMax = new Vector2(0.5f, 1f);
        cardRect.anchoredPosition = new Vector2(0, yPos);
        cardRect.sizeDelta = new Vector2(900, 220);

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
            string godMark = isGod ? "  <color=#FFD700>★GOD BABY★</color>" : "";

            // Baby icon (130x130) with circle mask
            var babyMaskObj = new GameObject("BabyMask");
            babyMaskObj.transform.SetParent(cardObj.transform, false);
            var babyMaskRect = babyMaskObj.AddComponent<RectTransform>();
            babyMaskRect.anchorMin = new Vector2(0, 0.5f);
            babyMaskRect.anchorMax = new Vector2(0, 0.5f);
            babyMaskRect.anchoredPosition = new Vector2(100, 0);
            babyMaskRect.sizeDelta = new Vector2(130, 130);
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

            string genderKey = babyGender == "男の子" ? "male" : "female";
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
                babyIconImg.color = babyGender == "男の子"
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
                nowText.text = "NOW";
                nowText.fontSize = 20;
                nowText.fontStyle = FontStyles.Bold;
                nowText.alignment = TextAlignmentOptions.Center;
                nowText.color = Color.white;
                nowText.raycastTarget = false;
            }

            // Baby name + age
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(cardObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(200, 15);
            contentRect.offsetMax = new Vector2(-20, -15);
            var contentText = contentObj.AddComponent<TextMeshProUGUI>();
            contentText.fontSize = 34;
            contentText.alignment = TextAlignmentOptions.Left;
            contentText.color = new Color(0.2f, 0.2f, 0.2f);
            contentText.richText = true;
            contentText.raycastTarget = false;
            contentText.text = $"<b>{babyName}</b>{godMark}\n" +
                $"<size=28>{Localization.GetAge(age)}</size>\n" +
                $"<size=24><color=#888888>{fatherName} × {motherName}</color></size>";
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
            contentText.fontSize = 28;
            contentText.alignment = TextAlignmentOptions.Left;
            contentText.color = new Color(0.6f, 0.6f, 0.6f);
            contentText.raycastTarget = false;
            contentText.text = Localization.Get("map_save_slot_empty");
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
