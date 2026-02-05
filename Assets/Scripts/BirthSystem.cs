using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class BirthSystem : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI childStatusText;
    public Canvas canvas;

    [Header("Baby Face")]
    public RectTransform babyFace;
    public Image babyFaceImage;

    [Header("Baby Sprites (patterns)")]
    public Sprite[] babySprites;

    [Header("Parent Face Sprites (6 each)")]
    public Sprite[] fatherSprites;
    public Sprite[] motherSprites;

    [Header("Button Controls")]
    public GameObject generateLifeButton;
    public GameObject anotherGalButton;
    public GameObject gotoBattleButton;

    // 自動生成される親UI
    GameObject parentPanel;
    Image fatherFaceImage;
    TextMeshProUGUI fatherNameText;
    TextMeshProUGUI fatherIntroText;
    Image motherFaceImage;
    TextMeshProUGUI motherNameText;
    TextMeshProUGUI motherIntroText;

    // フラッシュ演出用
    Image flashOverlay;

    // ?マーク
    TextMeshProUGUI questionMark;

    // アニメーション中フラグ
    bool isAnimating;

    // 父親6パターン
    static readonly ParentData[] Fathers = new[]
    {
        new ParentData("タケシ",   70, 30, 180, 178, 40, 80, 85, new Color(0.9f, 0.7f, 0.5f), "元・格闘技世界王者 / 握力: 180kg"),
        new ParentData("ユウキ",   50, 50, 150, 172, 70, 68, 60, new Color(0.6f, 0.8f, 1.0f), "天才ハッカー / 特許数: 3,200件"),
        new ParentData("ゴウ",     80, 25, 190, 185, 30, 88, 90, new Color(1.0f, 0.5f, 0.4f), "伝説の傭兵 / 戦闘力: 計測不能"),
        new ParentData("シンジ",   30, 70, 140, 168, 95, 62, 35, new Color(0.7f, 0.7f, 1.0f), "ノーベル賞3回受賞 / IQ: 250"),
        new ParentData("リョウマ", 60, 60, 170, 180, 55, 75, 70, new Color(0.5f, 1.0f, 0.6f), "総資産: 43兆円 / 世界一の実業家"),
        new ParentData("テツヤ",   45, 45, 160, 170, 60, 72, 55, new Color(1.0f, 0.9f, 0.5f), "伝説のロックスター / ファン数: 8億人"),
    };

    // 母親6パターン
    static readonly ParentData[] Mothers = new[]
    {
        new ParentData("サクラ",   25, 70, 130, 158, 85, 50, 40, new Color(1.0f, 0.7f, 0.8f), "天才外科医 / 手術成功率: 100%"),
        new ParentData("ヒナタ",   40, 60, 150, 162, 60, 55, 65, new Color(0.8f, 0.6f, 1.0f), "暗殺拳の継承者 / 全戦全勝"),
        new ParentData("アキラ",   65, 30, 160, 170, 45, 58, 80, new Color(1.0f, 0.6f, 0.4f), "五輪金メダル7個 / 100m走: 10.1秒"),
        new ParentData("ミサト",   35, 55, 140, 155, 90, 48, 30, new Color(0.6f, 0.9f, 1.0f), "量子物理学者 / IQ: 270"),
        new ParentData("カエデ",   50, 50, 145, 165, 70, 52, 60, new Color(0.5f, 1.0f, 0.7f), "総資産: 28兆円 / 美容帝国CEO"),
        new ParentData("ルナ",     55, 40, 135, 160, 50, 46, 70, new Color(1.0f, 1.0f, 0.6f), "世界的スーパーモデル / 身長: 180cm"),
    };

    // 特徴リスト
    static readonly string[] Traits =
    {
        "天才肌", "努力家", "頑丈", "すばしっこい", "おだやか",
        "あまえんぼう", "なきむし", "くいしんぼう", "好奇心旺盛", "マイペース",
        "負けず嫌い", "やさしい", "ワイルド", "ミステリアス",
    };

    void Start()
    {
        if (childStatusText != null)
            childStatusText.richText = true;
        SetButtonText(generateLifeButton, "いでよ、GOD BABY!!!");
        SetButtonText(anotherGalButton, "もう一度うむ");
        SetButtonText(gotoBattleButton, "バトルへ");
        CreateParentUI();
        CreateFlashOverlay();
        CreateQuestionMark();
        //CreateCharacterListUI();
        if (parentPanel != null) parentPanel.SetActive(false);
    }

    // ===== ボタンから呼ばれるメソッド =====

    public void SpinRoulette()
    {
        if (isAnimating) return;
        StartCoroutine(SpinRouletteAnimation());
    }

    public void ResetParents()
    {
        if (isAnimating) return;
        StartCoroutine(SpinRouletteAnimation());
    }

    public void GoToBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }

    // ===== メインアニメーション =====

    IEnumerator SpinRouletteAnimation()
    {
        isAnimating = true;

        // ボタンを即非表示、?マークを隠す
        if (generateLifeButton != null) generateLifeButton.SetActive(false);
        if (anotherGalButton != null) anotherGalButton.SetActive(false);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(false);
        if (questionMark != null) questionMark.gameObject.SetActive(false);

        // 結果を先に決定
        int fIdx = Random.Range(0, Fathers.Length);
        int mIdx = Random.Range(0, Mothers.Length);
        ParentData father = Fathers[fIdx];
        ParentData mother = Mothers[mIdx];

        // ステータス計算
        int f_atk = father.atk + Random.Range(-10, 11);
        int f_def = father.def + Random.Range(-10, 11);
        int f_hp  = father.hp  + Random.Range(-15, 16);
        int m_atk = mother.atk + Random.Range(-10, 11);
        int m_def = mother.def + Random.Range(-10, 11);
        int m_hp  = mother.hp  + Random.Range(-15, 16);

        int c_atk      = (f_atk + m_atk) / 2 + Random.Range(-5, 6);
        int c_def      = (f_def + m_def) / 2 + Random.Range(-5, 6);
        int c_hp       = (f_hp + m_hp) / 2 + Random.Range(-10, 11);
        int c_academic = (father.academic + mother.academic) / 2 + Random.Range(-10, 11);
        int c_athletic = (father.athletic + mother.athletic) / 2 + Random.Range(-10, 11);
        int c_height   = 48 + Random.Range(-3, 4);
        int c_weight   = 3000 + Random.Range(-500, 501);

        string trait1 = DetermineTrait(c_atk, c_def, c_hp, c_academic, c_athletic);
        string trait2 = Traits[Random.Range(0, Traits.Length)];
        while (trait2 == trait1)
            trait2 = Traits[Random.Range(0, Traits.Length)];

        // ── フェーズ1: パネル表示、母親側は「???」で伏せる ──
        if (parentPanel != null) parentPanel.SetActive(true);
        childStatusText.text = "";

        // 紹介文を非表示にリセット
        if (fatherIntroText != null) fatherIntroText.gameObject.SetActive(false);
        if (motherIntroText != null) motherIntroText.gameObject.SetActive(false);

        // 母親側を「???」で伏せる
        if (motherFaceImage != null)
        {
            motherFaceImage.sprite = null;
            motherFaceImage.color = new Color(0.3f, 0.3f, 0.4f);
        }
        if (motherNameText != null)
            motherNameText.text = "母: ???";

        // ── フェーズ2: 父親ルーレット ──
        // 高速シャッフル（15回×0.06秒）
        for (int i = 0; i < 15; i++)
        {
            int tmpF = Random.Range(0, Fathers.Length);
            ShowSingleParentPreview(tmpF, Fathers[tmpF], true);
            yield return new WaitForSeconds(0.06f);
        }
        // 減速シャッフル（6回）
        for (int i = 0; i < 6; i++)
        {
            int tmpF = (i < 4) ? Random.Range(0, Fathers.Length) : fIdx;
            ShowSingleParentPreview(tmpF, Fathers[tmpF], true);
            float delay = Mathf.Lerp(0.12f, 0.35f, i / 5f);
            yield return new WaitForSeconds(delay);
        }
        // 父親確定
        ShowSingleParentPreview(fIdx, father, true);
        // 紹介文表示
        if (fatherIntroText != null)
        {
            fatherIntroText.text = father.intro;
            fatherIntroText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1.5f);

        // ── フェーズ3: 母親ルーレット ──
        // 高速シャッフル（15回×0.06秒）
        for (int i = 0; i < 15; i++)
        {
            int tmpM = Random.Range(0, Mothers.Length);
            ShowSingleParentPreview(tmpM, Mothers[tmpM], false);
            yield return new WaitForSeconds(0.06f);
        }
        // 減速シャッフル（6回）
        for (int i = 0; i < 6; i++)
        {
            int tmpM = (i < 4) ? Random.Range(0, Mothers.Length) : mIdx;
            ShowSingleParentPreview(tmpM, Mothers[tmpM], false);
            float delay = Mathf.Lerp(0.12f, 0.35f, i / 5f);
            yield return new WaitForSeconds(delay);
        }
        // 母親確定
        ShowSingleParentPreview(mIdx, mother, false);
        // 紹介文表示
        if (motherIntroText != null)
        {
            motherIntroText.text = mother.intro;
            motherIntroText.gameObject.SetActive(true);
        }
        yield return new WaitForSeconds(1.5f);

        // ── フェーズ4: フラッシュ ──
        yield return StartCoroutine(FlashEffect());

        // ── フェーズ5: ステータス1行ずつ表示 ──
        ApplyVisuals(c_weight, c_academic);

        string line1 = "<color=yellow><size=130%><b>【 新しい命が誕生！ 】</b></size></color>";
        string line2 = "──────────────────────────────────────";
        string line3 = $"<b>身長:</b> {c_height} cm    <b>体重:</b> {c_weight} g";
        string line4 = $"<b>HP:</b> {c_hp}    <b>攻撃:</b> {c_atk}    <b>防御:</b> {c_def}";
        string line5 = $"<b>学力:</b> {c_academic}    <b>運動:</b> {c_athletic}";
        string line6 = $"<b>特徴:</b>  <color=orange>{trait1}</color>    <color=lime>{trait2}</color>";

        childStatusText.text = line1;
        yield return new WaitForSeconds(0.3f);
        childStatusText.text = line1 + "\n" + line2;
        yield return new WaitForSeconds(0.15f);
        childStatusText.text = line1 + "\n" + line2 + "\n" + line3;
        yield return new WaitForSeconds(0.15f);
        childStatusText.text = line1 + "\n" + line2 + "\n" + line3 + "\n" + line4;
        yield return new WaitForSeconds(0.15f);
        childStatusText.text = line1 + "\n" + line2 + "\n" + line3 + "\n" + line4 + "\n" + line5;
        yield return new WaitForSeconds(0.25f);
        childStatusText.text = line1 + "\n" + line2 + "\n" + line3 + "\n" + line4 + "\n" + line5 + "\n\n" + line6;

        // ── DataCarrier に保存 ──
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyAtk = c_atk;
            DataCarrier.Instance.babyDef = c_def;
            DataCarrier.Instance.babyHp = c_hp;
            DataCarrier.Instance.babyAcademic = c_academic;
            DataCarrier.Instance.babyWeight = c_weight;
            DataCarrier.Instance.babyAthletic = c_athletic;
            DataCarrier.Instance.babyHeight = c_height;
            DataCarrier.Instance.trait1 = trait1;
            DataCarrier.Instance.trait2 = trait2;
            DataCarrier.Instance.fatherName = father.name;
            DataCarrier.Instance.motherName = mother.name;
        }

        // ── ボタン表示 ──
        yield return new WaitForSeconds(0.3f);
        if (anotherGalButton != null) anotherGalButton.SetActive(true);
        if (gotoBattleButton != null) gotoBattleButton.SetActive(true);

        isAnimating = false;
    }

    // ===== フラッシュ演出 =====

    IEnumerator FlashEffect()
    {
        if (flashOverlay == null) yield break;

        // 白フラッシュ
        flashOverlay.color = new Color(1f, 1f, 1f, 0.9f);
        flashOverlay.gameObject.SetActive(true);

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.9f, 0f, elapsed / duration);
            flashOverlay.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        flashOverlay.gameObject.SetActive(false);
    }

    // ===== ルーレット中のプレビュー表示（父 or 母 個別） =====

    void ShowSingleParentPreview(int idx, ParentData data, bool isFather)
    {
        Image faceImage = isFather ? fatherFaceImage : motherFaceImage;
        TextMeshProUGUI nameT = isFather ? fatherNameText : motherNameText;
        Sprite[] sprites = isFather ? fatherSprites : motherSprites;
        string prefix = isFather ? "父" : "母";

        if (faceImage != null)
        {
            Sprite sp = (sprites != null && sprites.Length > idx) ? sprites[idx] : null;
            if (sp != null)
            {
                faceImage.sprite = sp;
                faceImage.color = Color.white;
            }
            else
            {
                faceImage.sprite = null;
                faceImage.color = data.faceColor;
            }
        }
        if (nameT != null)
            nameT.text = $"{prefix}: {data.name}";
    }

    // ===== ステータス判定 =====

    string DetermineTrait(int atk, int def, int hp, int academic, int athletic)
    {
        int max = Mathf.Max(atk, def, hp, academic, athletic);
        if (max == academic && academic > 70) return "天才肌";
        if (max == atk && atk > 60)           return "ワイルド";
        if (max == def && def > 60)            return "頑丈";
        if (max == athletic && athletic > 70)  return "すばしっこい";
        if (max == hp && hp > 150)             return "タフ";
        return Traits[Random.Range(0, Traits.Length)];
    }

    void ApplyVisuals(int weight, int academic)
    {
        if (babyFace == null) return;

        float faceWidth = 1.0f + (weight - 3000) * 0.0002f;
        babyFace.localScale = new Vector3(faceWidth, 1.0f, 1.0f);

        // 赤ちゃんの顔パターンをランダム選択
        if (babySprites != null && babySprites.Length > 0 && babyFaceImage != null)
            babyFaceImage.sprite = babySprites[Random.Range(0, babySprites.Length)];
    }

    // ===== UI自動生成 =====

    void CreateParentUI()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        parentPanel = new GameObject("ParentPanel");
        parentPanel.transform.SetParent(canvas.transform, false);

        var panelRect = parentPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 280);
        panelRect.sizeDelta = new Vector2(0, 320);

        CreateParentCard(parentPanel.transform, -300, out fatherFaceImage, out fatherNameText, out fatherIntroText);
        CreateParentCard(parentPanel.transform, 300, out motherFaceImage, out motherNameText, out motherIntroText);
    }

    void CreateParentCard(Transform parent, float xPos, out Image faceImage, out TextMeshProUGUI nameText, out TextMeshProUGUI introText)
    {
        var card = new GameObject("ParentCard");
        card.transform.SetParent(parent, false);

        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = new Vector2(xPos, 0);
        cardRect.sizeDelta = new Vector2(200, 290);

        var bg = card.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        bg.raycastTarget = false;

        var faceObj = new GameObject("Face");
        faceObj.transform.SetParent(card.transform, false);
        var faceRect = faceObj.AddComponent<RectTransform>();
        faceRect.anchorMin = new Vector2(0.5f, 1f);
        faceRect.anchorMax = new Vector2(0.5f, 1f);
        faceRect.anchoredPosition = new Vector2(0, -90);
        faceRect.sizeDelta = new Vector2(150, 150);
        faceImage = faceObj.AddComponent<Image>();
        faceImage.color = Color.white;
        faceImage.raycastTarget = false;

        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(card.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0);
        nameRect.anchorMax = new Vector2(1, 0);
        nameRect.anchoredPosition = new Vector2(0, 50);
        nameRect.sizeDelta = new Vector2(0, 40);
        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 24;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = Color.white;
        nameText.raycastTarget = false;

        var introObj = new GameObject("Intro");
        introObj.transform.SetParent(card.transform, false);
        var introRect = introObj.AddComponent<RectTransform>();
        introRect.anchorMin = new Vector2(0, 0);
        introRect.anchorMax = new Vector2(1, 0);
        introRect.anchoredPosition = new Vector2(0, 15);
        introRect.sizeDelta = new Vector2(0, 40);
        introText = introObj.AddComponent<TextMeshProUGUI>();
        introText.fontSize = 18;
        introText.alignment = TextAlignmentOptions.Center;
        introText.color = new Color(1f, 0.9f, 0.5f);
        introText.raycastTarget = false;
        introObj.SetActive(false);
    }

    void CreateQuestionMark()
    {
        if (babyFace == null) return;

        // BabyFace白背景を完全に消す
        if (babyFaceImage != null)
            babyFaceImage.color = new Color(1f, 1f, 1f, 0f);

        var qObj = new GameObject("QuestionMark");
        qObj.transform.SetParent(babyFace, false);
        var qRect = qObj.AddComponent<RectTransform>();
        qRect.anchorMin = new Vector2(0.5f, 0.5f);
        qRect.anchorMax = new Vector2(0.5f, 0.5f);
        qRect.anchoredPosition = Vector2.zero;
        qRect.sizeDelta = new Vector2(180, 180);

        questionMark = qObj.AddComponent<TextMeshProUGUI>();
        questionMark.text = "?";
        questionMark.fontSize = 100;
        questionMark.horizontalAlignment = HorizontalAlignmentOptions.Center;
        questionMark.verticalAlignment = VerticalAlignmentOptions.Middle;
        questionMark.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
        questionMark.fontStyle = FontStyles.Bold;
        questionMark.raycastTarget = false;
        questionMark.enableWordWrapping = false;
    }

    void CreateFlashOverlay()
    {
        if (canvas == null) return;

        var flashObj = new GameObject("FlashOverlay");
        flashObj.transform.SetParent(canvas.transform, false);

        var rect = flashObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        flashOverlay = flashObj.AddComponent<Image>();
        flashOverlay.color = new Color(1f, 1f, 1f, 0f);
        flashOverlay.raycastTarget = false;
        flashObj.SetActive(false);
    }

    void SetButtonText(GameObject button, string text)
    {
        if (button == null) return;
        var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;
    }

    // ===== キャラクターリストUI =====

    void CreateCharacterListUI()
    {
        if (canvas == null) return;

        var listPanel = new GameObject("CharacterList");
        listPanel.transform.SetParent(canvas.transform, false);

        var listRect = listPanel.AddComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0, 0);
        listRect.anchorMax = new Vector2(1, 0);
        listRect.pivot = new Vector2(0.5f, 0);
        listRect.anchoredPosition = new Vector2(0, 5);
        listRect.sizeDelta = new Vector2(-20, 110);

        var listBg = listPanel.AddComponent<Image>();
        listBg.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);
        listBg.raycastTarget = false;

        // 父親行
        CreateCharacterRow(listPanel.transform, "父親", Fathers, fatherSprites, 25);
        // 母親行
        CreateCharacterRow(listPanel.transform, "母親", Mothers, motherSprites, -25);
    }

    void CreateCharacterRow(Transform parent, string label, ParentData[] parents, Sprite[] sprites, float yCards)
    {
        // ラベル
        var labelObj = new GameObject(label + "Label");
        labelObj.transform.SetParent(parent, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(40, yCards);
        labelRect.sizeDelta = new Vector2(50, 24);
        var labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.color = new Color(1f, 1f, 1f, 0.8f);
        labelText.fontStyle = FontStyles.Bold;
        labelText.raycastTarget = false;

        // キャラカード6個
        float startX = 100;
        float spacing = 120;
        for (int i = 0; i < parents.Length; i++)
        {
            float x = startX + i * spacing;
            CreateMiniCard(parent, x, yCards, parents[i], sprites, i);
        }
    }

    void CreateMiniCard(Transform parent, float x, float y, ParentData data, Sprite[] sprites, int idx)
    {
        var card = new GameObject("Mini_" + data.name);
        card.transform.SetParent(parent, false);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0, 0.5f);
        cardRect.anchorMax = new Vector2(0, 0.5f);
        cardRect.anchoredPosition = new Vector2(x, y);
        cardRect.sizeDelta = new Vector2(110, 45);

        // 顔画像
        var faceObj = new GameObject("Face");
        faceObj.transform.SetParent(card.transform, false);
        var faceRect = faceObj.AddComponent<RectTransform>();
        faceRect.anchorMin = new Vector2(0, 0.5f);
        faceRect.anchorMax = new Vector2(0, 0.5f);
        faceRect.anchoredPosition = new Vector2(18, 0);
        faceRect.sizeDelta = new Vector2(36, 36);
        var faceImg = faceObj.AddComponent<Image>();
        faceImg.raycastTarget = false;

        Sprite sp = (sprites != null && sprites.Length > idx) ? sprites[idx] : null;
        if (sp != null)
        {
            faceImg.sprite = sp;
            faceImg.color = Color.white;
        }
        else
        {
            faceImg.color = data.faceColor;
        }

        // 名前
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(card.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 0.5f);
        nameRect.anchoredPosition = new Vector2(15, 0);
        nameRect.sizeDelta = new Vector2(0, 20);
        var nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.text = data.name;
        nameText.fontSize = 12;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = new Color(1f, 1f, 1f, 0.9f);
        nameText.raycastTarget = false;
    }
}

public struct ParentData
{
    public string name;
    public int atk, def, hp, height, academic, weight, athletic;
    public Color faceColor;
    public string intro;

    public ParentData(string name, int atk, int def, int hp, int height, int academic, int weight, int athletic, Color faceColor, string intro)
    {
        this.name = name;
        this.atk = atk;
        this.def = def;
        this.hp = hp;
        this.height = height;
        this.academic = academic;
        this.weight = weight;
        this.athletic = athletic;
        this.faceColor = faceColor;
        this.intro = intro;
    }
}
