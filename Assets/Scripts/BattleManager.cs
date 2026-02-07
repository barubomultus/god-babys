using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI statusText;
    public Canvas canvas;

    [Header("Baby Sprites")]
    public Sprite[] babySprites;

    [Header("Enemy Sprites")]
    public Sprite firstEnemySprite;

    // Resources から自動読み込み
    Sprite loadedEnemySprite;

    // プレイヤーステータス
    int playerHp, playerMaxHp, playerAtk, playerDef;
    int playerHeight, playerWeight;
    bool playerDefending;
    bool isMale;
    bool isGodBaby;
    int playerEvasion; // 回避率(身長・体重で変動)

    // 固有技データ
    string normalAttackName;   // 通常攻撃名
    string specialAttackName;  // 必殺技名

    // 36通りの固有技データ（父親名_母親名 → [通常技, 必殺技]）
    static readonly System.Collections.Generic.Dictionary<string, string[]> SkillData = new System.Collections.Generic.Dictionary<string, string[]>
    {
        // タケシ（格闘家）× 各母親
        {"タケシ_サクラ", new[]{"メスパンチ", "外科キングブロー"}},
        {"タケシ_ヒナタ", new[]{"暗殺キック", "暗黒格闘技"}},
        {"タケシ_アキラ", new[]{"ゴールドラッシュ", "オリンピック・スマッシュ"}},
        {"タケシ_ミサト", new[]{"量子パンチ", "ブラックホール・ストライク"}},
        {"タケシ_カエデ", new[]{"ビューティーブロー", "黄金の拳"}},
        {"タケシ_ルナ", new[]{"モデルキック", "カリスマ・インパクト"}},

        // ユウキ（ハッカー）× 各母親
        {"ユウキ_サクラ", new[]{"電脳メス", "サイバー・オペレーション"}},
        {"ユウキ_ヒナタ", new[]{"ステルスハック", "暗殺プログラム"}},
        {"ユウキ_アキラ", new[]{"データストリーム", "電脳オリンピック"}},
        {"ユウキ_ミサト", new[]{"量子ハッキング", "シンギュラリティ・コード"}},
        {"ユウキ_カエデ", new[]{"マネーウイルス", "ビリオネア・ハック"}},
        {"ユウキ_ルナ", new[]{"バーチャルビーム", "デジタル・オーラ"}},

        // ゴウ（傭兵）× 各母親
        {"ゴウ_サクラ", new[]{"コンバットメス", "戦場の天使"}},
        {"ゴウ_ヒナタ", new[]{"暗殺コンボ", "シャドウ・アサシン"}},
        {"ゴウ_アキラ", new[]{"ミリタリーダッシュ", "ウォー・スプリント"}},
        {"ゴウ_ミサト", new[]{"タクティカル量子", "戦術核融合"}},
        {"ゴウ_カエデ", new[]{"傭兵マネー", "ゴールド・ウォーフェア"}},
        {"ゴウ_ルナ", new[]{"カモフラージュ", "ステルス・グラマー"}},

        // シンジ（天才科学者）× 各母親
        {"シンジ_サクラ", new[]{"論理メス", "ノーベル・サージェリー"}},
        {"シンジ_ヒナタ", new[]{"計算キック", "IQ暗殺術"}},
        {"シンジ_アキラ", new[]{"物理エンジン", "科学オリンピック"}},
        {"シンジ_ミサト", new[]{"量子もつれ", "ダブルIQ・フュージョン"}},
        {"シンジ_カエデ", new[]{"経済理論", "ノーベル経済砲"}},
        {"シンジ_ルナ", new[]{"美の方程式", "相対性オーラ"}},

        // リョウマ（実業家）× 各母親
        {"リョウマ_サクラ", new[]{"札束メス", "メディカル・ビリオン"}},
        {"リョウマ_ヒナタ", new[]{"マネーキック", "暗殺ビジネス"}},
        {"リョウマ_アキラ", new[]{"投資ダッシュ", "ゴールドメダル買収"}},
        {"リョウマ_ミサト", new[]{"量子投資", "無限マネー理論"}},
        {"リョウマ_カエデ", new[]{"帝国コンボ", "トリリオン・エンパイア"}},
        {"リョウマ_ルナ", new[]{"セレブオーラ", "ワールドクラス・リッチ"}},

        // テツヤ（ロックスター）× 各母親
        {"テツヤ_サクラ", new[]{"ロックメス", "ライブ・サージェリー"}},
        {"テツヤ_ヒナタ", new[]{"サイレントロック", "暗殺セレナーデ"}},
        {"テツヤ_アキラ", new[]{"スピードビート", "オリンピック・ライブ"}},
        {"テツヤ_ミサト", new[]{"量子メロディ", "シュレディンガーズ・ソング"}},
        {"テツヤ_カエデ", new[]{"ゴールドレコード", "プラチナ・アンセム"}},
        {"テツヤ_ルナ", new[]{"スターオーラ", "スーパースター・ハーモニー"}},
    };

    // 敵ステータス
    int enemyHp, enemyMaxHp, enemyAtk, enemyDef;
    string enemyName;

    // UI要素
    GameObject battlePanel;
    Image playerFaceImage;
    Image enemyFaceImage;
    TextMeshProUGUI playerNameText;
    TextMeshProUGUI enemyNameText;
    Image playerHpBar;
    Image enemyHpBar;
    TextMeshProUGUI playerHpText;
    TextMeshProUGUI enemyHpText;
    TextMeshProUGUI battleLogText;

    // アクションボタン
    GameObject actionPanel;
    Button attackButton;
    Button defendButton;
    Button specialButton;
    TextMeshProUGUI attackButtonText;
    TextMeshProUGUI specialButtonText;

    // バトル状態
    bool isBattleActive;
    bool isPlayerTurn;
    bool waitingForAction;
    int battleTurnCount;

    void Start()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        InitializePlayer();
        InitializeEnemy();
        CreateBattleUI();
        CreateMenuBar();

        StartCoroutine(BattleStart());
    }

    void InitializePlayer()
    {
        if (DataCarrier.Instance != null)
        {
            playerMaxHp = DataCarrier.Instance.babyHp;
            playerHp = playerMaxHp;
            playerAtk = DataCarrier.Instance.babyAtk;
            playerDef = DataCarrier.Instance.babyDef;

            playerHeight = DataCarrier.Instance.babyHeight;
            playerWeight = DataCarrier.Instance.babyWeight;

            // 性別判定
            isMale = DataCarrier.Instance.babyGender == "男の子";

            if (isMale)
            {
                // 男の子: 攻撃力+50%ボーナス
                playerAtk = (int)(playerAtk * 1.5f);
                // 基本回避率5% + 小さいほどボーナス
                int heightBonus = Mathf.Max(0, (100 - playerHeight) / 5);  // 100cm以下で最大+20%
                int weightBonus = Mathf.Max(0, (10000 - playerWeight) / 667); // 10kg以下で最大+7%
                playerEvasion = 5 + heightBonus + weightBonus;
            }
            else
            {
                // 女の子: 基本回避率15% + 小さいほど大ボーナス
                int heightBonus = Mathf.Max(0, (100 - playerHeight) / 3);  // 100cm以下で最大+33%
                int weightBonus = Mathf.Max(0, (10000 - playerWeight) / 333); // 10kg以下で最大+18%
                playerEvasion = 15 + heightBonus + weightBonus;
            }

            // 回避率上限
            playerEvasion = Mathf.Min(playerEvasion, 70);

            // GOD BABY判定
            isGodBaby = DataCarrier.Instance.isGodBaby;
            if (isGodBaby)
            {
                // GOD BABYボーナス: 全ステータス+20%、必殺技命中率100%
                playerAtk = (int)(playerAtk * 1.2f);
                playerDef = (int)(playerDef * 1.2f);
                playerMaxHp = (int)(playerMaxHp * 1.2f);
                playerHp = playerMaxHp;
            }

            // 親の組み合わせから固有技を取得
            string parentKey = $"{DataCarrier.Instance.fatherName}_{DataCarrier.Instance.motherName}";
            if (SkillData.TryGetValue(parentKey, out string[] skills))
            {
                normalAttackName = skills[0];
                specialAttackName = skills[1];
            }
            else
            {
                // デフォルト技
                normalAttackName = "パンチ";
                specialAttackName = "GOD SMASH";
            }
        }
        else
        {
            // デフォルト値
            playerMaxHp = 100;
            playerHp = playerMaxHp;
            playerAtk = 30;
            playerDef = 20;
            playerHeight = 100;
            playerWeight = 3000;
            isMale = true;
            isGodBaby = false;
            playerEvasion = 5;
            normalAttackName = "パンチ";
            specialAttackName = "GOD SMASH";
        }
    }

    void InitializeEnemy()
    {
        bool fromMap = DataCarrier.Instance != null && DataCarrier.Instance.cameFromMap;
        bool bossBattle = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;

        if (fromMap && bossBattle)
        {
            // ボスの館 — 村の王シバ
            enemyName = "村の王シバ";
            enemyMaxHp = 500;
            enemyHp = enemyMaxHp;
            enemyAtk = 80;
            enemyDef = 45;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/first-boss-shiba");
        }
        else if (fromMap)
        {
            // マップからのランダムエンカウント — 年齢に応じた敵
            int age = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 1;
            InitializeRandomEnemy(age);
        }
        else
        {
            // 初回ボス
            enemyName = "わるいベイビー";
            enemyMaxHp = 150;
            enemyHp = enemyMaxHp;
            enemyAtk = 45;
            enemyDef = 25;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/frist-enemy");
        }
    }

    void InitializeRandomEnemy(int playerAge)
    {
        // 年齢に応じてスケーリング
        float scale = 1.0f + (playerAge - 1) * 0.3f;

        // 敵名とスプライトの対応
        string[][] enemyTable = {
            new[]{ "やんちゃベイビー",     "EnemyBabys/common-yantya" },
            new[]{ "いじわるベイビー",     "EnemyBabys/frist-enemy" },
            new[]{ "なきむしベイビー",     "EnemyBabys/common-nakimushi" },
            new[]{ "あばれんぼうベイビー", "EnemyBabys/common-abarennbou" },
            new[]{ "わがままベイビー",     "EnemyBabys/common-wagamama" },
        };

        int idx = Random.Range(0, enemyTable.Length);
        enemyName = enemyTable[idx][0];
        loadedEnemySprite = Resources.Load<Sprite>(enemyTable[idx][1]);

        enemyMaxHp = Mathf.RoundToInt(80 * scale + Random.Range(0, 30));
        enemyHp = enemyMaxHp;
        enemyAtk = Mathf.RoundToInt(25 * scale + Random.Range(0, 10));
        enemyDef = Mathf.RoundToInt(15 * scale + Random.Range(0, 8));
    }

    // ===== バトルUI作成 =====

    void CreateBattleUI()
    {
        // バトルパネル（中央上部）
        battlePanel = new GameObject("BattlePanel");
        battlePanel.transform.SetParent(canvas.transform, false);

        var panelRect = battlePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 100);
        panelRect.sizeDelta = new Vector2(800, 350);

        // プレイヤー側（左）
        CreateCharacterPanel(battlePanel.transform, -200, true, out playerFaceImage, out playerNameText, out playerHpBar, out playerHpText);

        // VS テキスト
        CreateVsText(battlePanel.transform);

        // 敵側（右）
        CreateCharacterPanel(battlePanel.transform, 200, false, out enemyFaceImage, out enemyNameText, out enemyHpBar, out enemyHpText);

        // バトルログ
        CreateBattleLog();

        // アクションボタン
        CreateActionButtons();

        // 初期表示
        UpdatePlayerDisplay();
        UpdateEnemyDisplay();
    }

    void CreateCharacterPanel(Transform parent, float xPos, bool isPlayer, out Image faceImage, out TextMeshProUGUI nameText, out Image hpBar, out TextMeshProUGUI hpText)
    {
        var panel = new GameObject(isPlayer ? "PlayerPanel" : "EnemyPanel");
        panel.transform.SetParent(parent, false);

        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(xPos, 0);
        panelRect.sizeDelta = new Vector2(280, 380);

        var panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
        panelBg.raycastTarget = false;

        // 名前
        var nameObj = new GameObject("Name");
        nameObj.transform.SetParent(panel.transform, false);
        var nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.anchoredPosition = new Vector2(0, -25);
        nameRect.sizeDelta = new Vector2(0, 50);
        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 28;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = isPlayer ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.5f, 0.5f);
        nameText.fontStyle = FontStyles.Bold;
        nameText.raycastTarget = false;
        nameText.richText = true; // GOD BABYの金色表示用

        // 顔
        var faceObj = new GameObject("Face");
        faceObj.transform.SetParent(panel.transform, false);
        var faceRect = faceObj.AddComponent<RectTransform>();
        faceRect.anchorMin = new Vector2(0.5f, 0.5f);
        faceRect.anchorMax = new Vector2(0.5f, 0.5f);
        faceRect.anchoredPosition = new Vector2(0, 15);
        faceRect.sizeDelta = new Vector2(200, 200);
        faceImage = faceObj.AddComponent<Image>();
        faceImage.color = Color.white;
        faceImage.raycastTarget = false;

        // HPバー背景
        var hpBgObj = new GameObject("HpBarBg");
        hpBgObj.transform.SetParent(panel.transform, false);
        var hpBgRect = hpBgObj.AddComponent<RectTransform>();
        hpBgRect.anchorMin = new Vector2(0, 0);
        hpBgRect.anchorMax = new Vector2(1, 0);
        hpBgRect.anchoredPosition = new Vector2(0, 55);
        hpBgRect.sizeDelta = new Vector2(-30, 20);
        var hpBgImage = hpBgObj.AddComponent<Image>();
        hpBgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);
        hpBgImage.raycastTarget = false;

        // HPバー
        var hpBarObj = new GameObject("HpBar");
        hpBarObj.transform.SetParent(hpBgObj.transform, false);
        var hpBarRect = hpBarObj.AddComponent<RectTransform>();
        hpBarRect.anchorMin = new Vector2(0, 0);
        hpBarRect.anchorMax = new Vector2(1, 1);
        hpBarRect.offsetMin = new Vector2(2, 2);
        hpBarRect.offsetMax = new Vector2(-2, -2);
        hpBarRect.pivot = new Vector2(0, 0.5f);
        hpBar = hpBarObj.AddComponent<Image>();
        hpBar.color = isPlayer ? new Color(0.2f, 0.8f, 0.3f) : new Color(0.8f, 0.2f, 0.2f);
        hpBar.raycastTarget = false;

        // HPテキスト
        var hpTextObj = new GameObject("HpText");
        hpTextObj.transform.SetParent(panel.transform, false);
        var hpTextRect = hpTextObj.AddComponent<RectTransform>();
        hpTextRect.anchorMin = new Vector2(0, 0);
        hpTextRect.anchorMax = new Vector2(1, 0);
        hpTextRect.anchoredPosition = new Vector2(0, 30);
        hpTextRect.sizeDelta = new Vector2(0, 50);
        hpText = hpTextObj.AddComponent<TextMeshProUGUI>();
        hpText.fontSize = 16;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = Color.white;
        hpText.richText = true;
        hpText.raycastTarget = false;
    }

    void CreateVsText(Transform parent)
    {
        var vsObj = new GameObject("VsText");
        vsObj.transform.SetParent(parent, false);
        var vsRect = vsObj.AddComponent<RectTransform>();
        vsRect.anchorMin = new Vector2(0.5f, 0.5f);
        vsRect.anchorMax = new Vector2(0.5f, 0.5f);
        vsRect.anchoredPosition = new Vector2(0, 30);
        vsRect.sizeDelta = new Vector2(100, 80);
        var vsText = vsObj.AddComponent<TextMeshProUGUI>();
        vsText.text = "VS";
        vsText.fontSize = 48;
        vsText.alignment = TextAlignmentOptions.Center;
        vsText.color = new Color(1f, 0.8f, 0.2f);
        vsText.fontStyle = FontStyles.Bold;
        vsText.raycastTarget = false;
    }

    void CreateBattleLog()
    {
        var logPanel = new GameObject("BattleLogPanel");
        logPanel.transform.SetParent(canvas.transform, false);

        var logRect = logPanel.AddComponent<RectTransform>();
        logRect.anchorMin = new Vector2(0.5f, 0.5f);
        logRect.anchorMax = new Vector2(0.5f, 0.5f);
        logRect.anchoredPosition = new Vector2(0, -200);
        logRect.sizeDelta = new Vector2(600, 100);

        var logBg = logPanel.AddComponent<Image>();
        logBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        logBg.raycastTarget = false;

        var textObj = new GameObject("LogText");
        textObj.transform.SetParent(logPanel.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 10);
        textRect.offsetMax = new Vector2(-15, -10);
        battleLogText = textObj.AddComponent<TextMeshProUGUI>();
        battleLogText.fontSize = 22;
        battleLogText.alignment = TextAlignmentOptions.Center;
        battleLogText.color = Color.white;
        battleLogText.raycastTarget = false;
        battleLogText.text = "";
    }

    void CreateActionButtons()
    {
        actionPanel = new GameObject("ActionPanel");
        actionPanel.transform.SetParent(canvas.transform, false);

        var panelRect = actionPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 80);
        panelRect.sizeDelta = new Vector2(500, 80);

        var layout = actionPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 20;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        attackButton = CreateActionButton(actionPanel.transform, normalAttackName, new Color(0.8f, 0.3f, 0.3f), OnAttack, out attackButtonText);
        defendButton = CreateActionButton(actionPanel.transform, "ぼうぎょ", new Color(0.3f, 0.5f, 0.8f), OnDefend, out _);
        specialButton = CreateActionButton(actionPanel.transform, specialAttackName, new Color(0.8f, 0.6f, 0.2f), OnSpecial, out specialButtonText);

        actionPanel.SetActive(false);
    }

    Button CreateActionButton(Transform parent, string label, Color bgColor, UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI buttonText)
    {
        var btnObj = new GameObject(label + "Button");
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = bgColor;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(onClick);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = label;
        buttonText.fontSize = 22;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;

        return btn;
    }

    // ===== 表示更新 =====

    void UpdatePlayerDisplay()
    {
        string babyName = "名無しベイビー";
        int babyAge = 0;
        if (DataCarrier.Instance != null)
        {
            // DataCarrierに名前があればそれを使用
            if (!string.IsNullOrEmpty(DataCarrier.Instance.babyName))
            {
                babyName = DataCarrier.Instance.babyName;
            }
            else
            {
                // 名前がなければ性別で決定
                string gender = DataCarrier.Instance.babyGender;
                if (!string.IsNullOrEmpty(gender))
                    babyName = gender == "男の子" ? "GOD BOY" : "GOD GIRL";
            }
            babyAge = DataCarrier.Instance.babyAge;
        }

        // 名前と年齢を表示（GOD BABYは金色）
        string ageText = $"({babyAge}さい)";
        if (isGodBaby)
        {
            playerNameText.text = $"<color=#FFD700>{babyName}</color> <size=70%>{ageText}</size>";
        }
        else
        {
            playerNameText.text = $"{babyName} <size=70%>{ageText}</size>";
        }
        string sizeInfo = $"{playerHeight}cm/{playerWeight}g";
        string bonusText = isMale
            ? $"<color=#66ccff>ATK:{playerAtk}</color> 回避:{playerEvasion}%"
            : $"<color=#ff99cc>回避:{playerEvasion}%</color> ({sizeInfo})";
        playerHpText.text = $"HP:{playerHp}/{playerMaxHp}\n{bonusText}";

        float hpRatio = (float)playerHp / playerMaxHp;
        playerHpBar.rectTransform.anchorMax = new Vector2(hpRatio, 1);

        // 赤ちゃんの顔：専用画像があればそれを使用、なければ自動生成
        if (!TryShowBabySprite(playerFaceImage))
        {
            GenerateBabyFaceForBattle(playerFaceImage.transform);
        }
    }

    bool TryShowBabySprite(Image targetImage)
    {
        if (DataCarrier.Instance == null) return false;

        string fatherName = GetParentImageName(DataCarrier.Instance.fatherName);
        string motherName = GetParentImageName(DataCarrier.Instance.motherName);
        string genderKey = DataCarrier.Instance.babyGender == "男の子" ? "male" : "female";

        string babyImagePath = $"babys/{fatherName}_{motherName}_{genderKey}";
        Sprite babySprite = Resources.Load<Sprite>(babyImagePath);

        if (babySprite != null)
        {
            // 既存の子要素をクリア
            foreach (Transform child in targetImage.transform)
            {
                Destroy(child.gameObject);
            }

            targetImage.enabled = true;
            targetImage.sprite = babySprite;
            targetImage.color = Color.white;
            targetImage.preserveAspect = true;

            // GOD BABYオーラを追加
            if (DataCarrier.Instance.isGodBaby)
            {
                for (int i = 2; i >= 0; i--)
                {
                    var aura = BPart("Aura" + i, targetImage.transform, Vector2.zero, new Vector2(130 + i * 12, 150 + i * 12));
                    var auraImg = aura.AddComponent<Image>();
                    auraImg.color = new Color(1f, 0.85f, 0.2f, 0.1f - i * 0.02f);
                    auraImg.raycastTarget = false;
                    aura.transform.SetAsFirstSibling();
                }
            }
            return true;
        }
        return false;
    }

    string GetParentImageName(string japaneseName)
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
            default: return japaneseName.ToLower();
        }
    }

    void GenerateBabyFaceForBattle(Transform parent)
    {
        // 既存の子要素をクリア
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }

        var parentImg = parent.GetComponent<Image>();
        if (parentImg != null) parentImg.enabled = false;

        int weight = 10000, height = 100, atk = 50, academic = 60, athletic = 60;
        string gender = "男の子";
        bool godBaby = false;

        if (DataCarrier.Instance != null)
        {
            weight = DataCarrier.Instance.babyWeight;
            height = DataCarrier.Instance.babyHeight;
            atk = DataCarrier.Instance.babyAtk;
            academic = DataCarrier.Instance.babyAcademic;
            athletic = DataCarrier.Instance.babyAthletic;
            gender = DataCarrier.Instance.babyGender;
            godBaby = DataCarrier.Instance.isGodBaby;
        }

        float s = 0.7f; // スケール

        // 肌色
        Color[] skinTones = { new Color(0.98f, 0.89f, 0.82f), new Color(0.95f, 0.83f, 0.74f), new Color(0.88f, 0.73f, 0.62f) };
        Color skin = skinTones[Random.Range(0, skinTones.Length)];
        Color skinShadow = new Color(skin.r * 0.85f, skin.g * 0.82f, skin.b * 0.8f);

        // 髪色
        Color[] hairTones = { new Color(0.08f, 0.06f, 0.05f), new Color(0.2f, 0.12f, 0.08f), new Color(0.35f, 0.22f, 0.12f), new Color(0.55f, 0.38f, 0.2f) };
        Color hair = hairTones[Mathf.Clamp(academic / 25, 0, 3)];
        Color hairShadow = new Color(hair.r * 0.6f, hair.g * 0.6f, hair.b * 0.6f);

        // GOD BABYオーラ
        if (godBaby)
        {
            for (int i = 2; i >= 0; i--)
            {
                var aura = BPart("Aura" + i, parent, Vector2.zero, new Vector2((130 + i * 12) * s, (150 + i * 12) * s));
                aura.AddComponent<Image>().color = new Color(1f, 0.85f, 0.2f, 0.1f - i * 0.02f);
            }
        }

        // 顔影
        BPart("FaceShadow", parent, new Vector2(2 * s, -3 * s), new Vector2(105 * s, 125 * s)).AddComponent<Image>().color = new Color(0, 0, 0, 0.12f);

        // 顔ベース
        BPart("Face", parent, Vector2.zero, new Vector2(100 * s, 120 * s)).AddComponent<Image>().color = skin;

        // 顔ハイライト
        BPart("FaceHL", parent, new Vector2(18 * s, 12 * s), new Vector2(30 * s, 60 * s)).AddComponent<Image>().color = new Color(1, 1, 1, 0.12f);

        // 左影
        BPart("LeftShadow", parent, new Vector2(-38 * s, 0), new Vector2(25 * s, 90 * s)).AddComponent<Image>().color = new Color(skinShadow.r, skinShadow.g, skinShadow.b, 0.35f);

        // 髪影
        BPart("HairShadow", parent, new Vector2(2 * s, 43 * s), new Vector2(112 * s, 52 * s)).AddComponent<Image>().color = new Color(0, 0, 0, 0.15f);

        // 髪メイン
        BPart("Hair", parent, new Vector2(0, 45 * s), new Vector2(108 * s, 48 * s)).AddComponent<Image>().color = hair;

        // 髪ハイライト
        BPart("HairHL", parent, new Vector2(12 * s, 50 * s), new Vector2(30 * s, 20 * s)).AddComponent<Image>().color = new Color(hair.r * 1.4f, hair.g * 1.4f, hair.b * 1.3f, 0.4f);

        // トップ髪
        BPart("TopHair", parent, new Vector2(0, 62 * s), new Vector2(85 * s, 25 * s)).AddComponent<Image>().color = hair;

        // サイド髪
        BPart("LeftHair", parent, new Vector2(-44 * s, 12 * s), new Vector2(22 * s, 60 * s)).AddComponent<Image>().color = hairShadow;
        BPart("RightHair", parent, new Vector2(44 * s, 12 * s), new Vector2(22 * s, 60 * s)).AddComponent<Image>().color = hair;

        // 前髪
        for (int i = 0; i < 4; i++)
        {
            float bx = -25 * s + (50f / 3) * i * s;
            var bang = BPart($"Bang{i}", parent, new Vector2(bx, 32 * s), new Vector2(15 * s, 22 * s));
            bang.AddComponent<Image>().color = i % 2 == 0 ? hair : hairShadow;
        }

        // 目
        Create3DBattleEye(parent, -18 * s, 6 * s, s, gender == "女の子", skin);
        Create3DBattleEye(parent, 18 * s, 6 * s, s, gender == "女の子", skin);

        // 眉
        float browAngle = (atk - 50) * 0.2f;
        var browL = BPart("BrowL", parent, new Vector2(-20 * s, 26 * s), new Vector2(22 * s, 4 * s));
        browL.transform.localRotation = Quaternion.Euler(0, 0, browAngle);
        browL.AddComponent<Image>().color = new Color(hair.r * 0.7f, hair.g * 0.7f, hair.b * 0.7f);
        var browR = BPart("BrowR", parent, new Vector2(20 * s, 26 * s), new Vector2(22 * s, 4 * s));
        browR.transform.localRotation = Quaternion.Euler(0, 0, -browAngle);
        browR.AddComponent<Image>().color = new Color(hair.r * 0.7f, hair.g * 0.7f, hair.b * 0.7f);

        // 鼻
        BPart("NoseHL", parent, new Vector2(1 * s, -3 * s), new Vector2(5 * s, 10 * s)).AddComponent<Image>().color = new Color(1, 1, 1, 0.15f);
        BPart("NoseShadow", parent, new Vector2(-3 * s, -5 * s), new Vector2(4 * s, 8 * s)).AddComponent<Image>().color = new Color(skinShadow.r, skinShadow.g, skinShadow.b, 0.2f);
        BPart("NoseTip", parent, new Vector2(0, -10 * s), new Vector2(10 * s, 8 * s)).AddComponent<Image>().color = new Color(skin.r * 0.95f, skin.g * 0.92f, skin.b * 0.9f, 0.5f);

        // 口
        float smile = athletic / 100f;
        Color lip = godBaby ? new Color(0.85f, 0.35f, 0.4f) : new Color(0.82f, 0.55f, 0.55f);
        float mw = (18 + smile * 10) * s;
        BPart("MouthShadow", parent, new Vector2(0, -28 * s), new Vector2((mw + 4) * s, 8 * s)).AddComponent<Image>().color = new Color(skin.r * 0.85f, skin.g * 0.8f, skin.b * 0.78f, 0.3f);
        BPart("UpperLip", parent, new Vector2(0, -25 * s), new Vector2(mw, 5 * s)).AddComponent<Image>().color = lip;
        BPart("LowerLip", parent, new Vector2(0, -30 * s), new Vector2(mw * 0.9f, 6 * s)).AddComponent<Image>().color = lip;
        BPart("MouthLine", parent, new Vector2(0, -27 * s), new Vector2(mw * 0.8f, 1.5f * s)).AddComponent<Image>().color = new Color(lip.r * 0.7f, lip.g * 0.6f, lip.b * 0.6f);

        // ほっぺ
        float cheekAlpha = gender == "女の子" ? 0.3f : 0.15f;
        Color cheekC = gender == "女の子" ? new Color(1f, 0.5f, 0.55f, cheekAlpha) : new Color(1f, 0.7f, 0.7f, cheekAlpha);
        BPart("CheekL", parent, new Vector2(-28 * s, -10 * s), new Vector2(22 * s, 18 * s)).AddComponent<Image>().color = cheekC;
        BPart("CheekR", parent, new Vector2(28 * s, -10 * s), new Vector2(22 * s, 18 * s)).AddComponent<Image>().color = cheekC;

        // 耳
        BPart("EarL", parent, new Vector2(-48 * s, 4 * s), new Vector2(12 * s, 20 * s)).AddComponent<Image>().color = skin;
        BPart("EarR", parent, new Vector2(48 * s, 4 * s), new Vector2(12 * s, 20 * s)).AddComponent<Image>().color = skin;
    }

    GameObject BPart(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var r = obj.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        return obj;
    }

    void Create3DBattleEye(Transform parent, float x, float y, float s, bool isFemale, Color skin)
    {
        float sz = 9 * s;

        // まぶた影
        BPart("LidShadow", parent, new Vector2(x, y + sz * 0.35f), new Vector2(sz * 1.7f, sz * 0.4f)).AddComponent<Image>().color = new Color(skin.r * 0.8f, skin.g * 0.75f, skin.b * 0.7f, 0.4f);

        // 白目
        BPart("EyeWhite", parent, new Vector2(x, y), new Vector2(sz * 1.5f, sz)).AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.97f);

        // 虹彩
        Color[] irisColors = { new Color(0.18f, 0.12f, 0.08f), new Color(0.35f, 0.5f, 0.65f), new Color(0.3f, 0.55f, 0.35f) };
        Color iris = irisColors[Random.Range(0, irisColors.Length)];
        BPart("Iris", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.85f, sz * 0.85f)).AddComponent<Image>().color = iris;
        BPart("IrisInner", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.55f, sz * 0.55f)).AddComponent<Image>().color = new Color(iris.r * 0.6f, iris.g * 0.6f, iris.b * 0.6f);

        // 瞳孔
        BPart("Pupil", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.3f, sz * 0.3f)).AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.02f);

        // ハイライト
        BPart("HL1", parent, new Vector2(x - sz * 0.12f, y + sz * 0.12f), new Vector2(sz * 0.22f, sz * 0.22f)).AddComponent<Image>().color = Color.white;
        BPart("HL2", parent, new Vector2(x + sz * 0.15f, y - sz * 0.08f), new Vector2(sz * 0.1f, sz * 0.1f)).AddComponent<Image>().color = new Color(1, 1, 1, 0.6f);

        // まつげ
        if (isFemale)
        {
            for (int i = 0; i < 4; i++)
            {
                float lx = x - sz * 0.4f + (sz * 0.8f / 3) * i;
                var lash = BPart($"Lash{i}", parent, new Vector2(lx, y + sz * 0.5f), new Vector2(1.5f * s, (4 + Random.Range(0, 2)) * s));
                lash.transform.localRotation = Quaternion.Euler(0, 0, -15 + i * 10);
                lash.AddComponent<Image>().color = new Color(0.1f, 0.08f, 0.06f);
            }
        }
    }

    void UpdateEnemyDisplay()
    {
        enemyNameText.text = enemyName;
        enemyHpText.text = $"HP:{enemyHp}/{enemyMaxHp} <color=#FF0000>ATK:{enemyAtk}</color>";

        float hpRatio = (float)enemyHp / enemyMaxHp;
        enemyHpBar.rectTransform.anchorMax = new Vector2(hpRatio, 1);

        // 敵画像 (Resources読み込み優先、なければInspector設定を使用)
        Sprite enemySprite = loadedEnemySprite ?? firstEnemySprite;
        if (enemySprite != null)
        {
            enemyFaceImage.enabled = true;
            enemyFaceImage.sprite = enemySprite;
            enemyFaceImage.color = Color.white;
            enemyFaceImage.preserveAspect = true;
        }
        else
        {
            enemyFaceImage.color = new Color(1f, 0.5f, 0.5f);
        }
    }

    // ===== バトルフロー =====

    IEnumerator BattleStart()
    {
        isBattleActive = true;
        battleLogText.text = $"<color=#FF0000>{enemyName}</color> があらわれた！";
        yield return new WaitForSeconds(1.5f);

        // GOD BABY特別演出
        if (isGodBaby)
        {
            battleLogText.text = "<color=#FFD700><size=120%>GOD BABY 降臨！</size></color>\n全ステータス +20%！ 必殺技 必中！";
            yield return new WaitForSeconds(2.0f);
        }

        // 性別・サイズボーナス表示
        if (isMale)
        {
            battleLogText.text = $"<color=#66ccff>おとこのこパワー！</color>\nこうげきりょく UP！ (ATK:{playerAtk})";
        }
        else
        {
            battleLogText.text = $"<color=#ff99cc>ちいさくて すばしっこい！</color>\nかいひりょく {playerEvasion}%！";
        }
        yield return new WaitForSeconds(1.5f);

        battleLogText.text = "バトル スタート！";
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(PlayerTurn());
    }

    IEnumerator PlayerTurn()
    {
        if (!isBattleActive) yield break;

        isPlayerTurn = true;
        playerDefending = false;
        battleLogText.text = "あなたのターン！ 行動を選んでください";
        actionPanel.SetActive(true);
        waitingForAction = true;

        while (waitingForAction)
        {
            yield return null;
        }

        actionPanel.SetActive(false);
    }

    IEnumerator EnemyTurn()
    {
        if (!isBattleActive) yield break;

        isPlayerTurn = false;
        yield return new WaitForSeconds(0.8f);

        battleLogText.text = $"{enemyName} のこうげき！";
        yield return new WaitForSeconds(0.6f);

        // 回避判定（女の子は回避率が高い）
        bool evaded = Random.Range(0, 100) < playerEvasion;

        if (evaded)
        {
            battleLogText.text = "<color=#00FFFF>ひらりとかわした！</color>";
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(PlayerTurn());
            yield break;
        }

        int damage = CalculateDamage(enemyAtk, playerDef, playerDefending);
        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();

        if (playerDefending)
        {
            battleLogText.text = $"ぼうぎょした！ {damage} ダメージ！";
        }
        else
        {
            battleLogText.text = $"{damage} ダメージをうけた！";
        }

        yield return new WaitForSeconds(1.0f);

        // プレイヤー敗北チェック
        if (playerHp <= 0)
        {
            StartCoroutine(BattleLose());
            yield break;
        }

        StartCoroutine(PlayerTurn());
    }

    int CalculateDamage(int atk, int def, bool defending)
    {
        float defMultiplier = defending ? 2.0f : 1.0f;
        int damage = Mathf.Max(1, atk - (int)(def * defMultiplier / 2));
        damage += Random.Range(-3, 4);
        return Mathf.Max(1, damage);
    }

    // ===== アクション =====

    void OnAttack()
    {
        if (!waitingForAction) return;
        waitingForAction = false;
        battleTurnCount++;
        StartCoroutine(DoAttack());
    }

    void OnDefend()
    {
        if (!waitingForAction) return;
        waitingForAction = false;
        battleTurnCount++;
        StartCoroutine(DoDefend());
    }

    void OnSpecial()
    {
        if (!waitingForAction) return;
        waitingForAction = false;
        battleTurnCount++;
        StartCoroutine(DoSpecial());
    }

    IEnumerator DoAttack()
    {
        // パンチ + 固有技を表示
        battleLogText.text = $"パンチ！ <color=#FFA500>{normalAttackName}</color>！";
        yield return new WaitForSeconds(0.6f);

        int damage = CalculateDamage(playerAtk, enemyDef, false);
        enemyHp = Mathf.Max(0, enemyHp - damage);
        UpdateEnemyDisplay();

        battleLogText.text = $"<color=#FFA500>{normalAttackName}</color> が きまった！\n{enemyName} に {damage} ダメージ！";
        yield return new WaitForSeconds(1.0f);

        if (enemyHp <= 0)
        {
            StartCoroutine(BattleWin());
            yield break;
        }

        StartCoroutine(EnemyTurn());
    }

    IEnumerator DoDefend()
    {
        playerDefending = true;
        battleLogText.text = "ぼうぎょ体勢をとった！";
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(EnemyTurn());
    }

    IEnumerator DoSpecial()
    {
        if (isGodBaby)
        {
            battleLogText.text = $"<color=#FFD700>GOD BABY の ひっさつわざ！</color>\n<color=#FFD700>神・{specialAttackName}！</color>";
        }
        else
        {
            battleLogText.text = $"ひっさつわざ！\n<color=#FFFF00>{specialAttackName}！</color>";
        }
        yield return new WaitForSeconds(0.8f);

        // 必殺技: GOD BABYは100%命中＆威力2.5倍、通常は75%命中＆威力2倍
        bool hit = isGodBaby ? true : Random.Range(0, 100) < 75;
        if (hit)
        {
            float multiplier = isGodBaby ? 2.5f : 2.0f;
            int damage = (int)(playerAtk * multiplier) + Random.Range(5, 15);
            enemyHp = Mathf.Max(0, enemyHp - damage);
            UpdateEnemyDisplay();

            if (isGodBaby)
            {
                battleLogText.text = $"<color=#FFD700>神・{specialAttackName}</color> が さくれつ！\n{damage} ダメージ！";
            }
            else
            {
                battleLogText.text = $"<color=#FFFF00>{specialAttackName}</color> が さくれつ！\n{damage} ダメージ！";
            }
        }
        else
        {
            battleLogText.text = $"{specialAttackName}...\nしかし はずれてしまった...";
        }

        yield return new WaitForSeconds(1.2f);

        if (enemyHp <= 0)
        {
            StartCoroutine(BattleWin());
            yield break;
        }

        StartCoroutine(EnemyTurn());
    }

    // ===== 勝敗 =====

    IEnumerator BattleWin()
    {
        isBattleActive = false;
        battleLogText.text = $"<color=#FFFF00>{enemyName} をたおした！</color>";
        yield return new WaitForSeconds(1.5f);

        battleLogText.text = "<color=#00FF00><size=130%>しょうり！</size></color>";
        yield return new WaitForSeconds(1.5f);

        // 経験値獲得と年齢アップ演出
        yield return StartCoroutine(GainExpSequence());

        // 自動セーブ
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.SaveData();
        }

        // バトルUIを非表示
        if (battlePanel != null) battlePanel.SetActive(false);
        if (actionPanel != null) actionPanel.SetActive(false);
        battleLogText.transform.parent.gameObject.SetActive(false);

        // ボス戦なら特別演出、通常なら村へ帰還
        bool wasBossBattle = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (wasBossBattle)
        {
            StartCoroutine(BossDefeatSequence());
        }
        else
        {
            StartCoroutine(VictoryToMap());
        }
    }

    IEnumerator GainExpSequence()
    {
        if (DataCarrier.Instance == null) yield break;

        // 経験値計算: 敵ステータスベース + ターン数ボーナス
        int expGained = (enemyMaxHp + enemyAtk * 3 + enemyDef * 2) / 4 + battleTurnCount * 3;
        DataCarrier.Instance.babyExp += expGained;
        DataCarrier.Instance.defeatedEnemies++;

        int currentAge = DataCarrier.Instance.babyAge;
        int needed = DataCarrier.ExpForNextAge(currentAge);
        int currentExp = DataCarrier.Instance.babyExp;

        battleLogText.text = $"<color=#00FFFF>けいけんち {expGained} をかくとく！</color>";
        yield return new WaitForSeconds(1.2f);

        if (currentExp >= needed)
        {
            // 年齢アップ前のステータスを保存
            int oldAtk = DataCarrier.Instance.babyAtk;
            int oldDef = DataCarrier.Instance.babyDef;
            int oldHp = DataCarrier.Instance.babyHp;
            int oldAcademic = DataCarrier.Instance.babyAcademic;
            int oldAthletic = DataCarrier.Instance.babyAthletic;
            int oldHeight = DataCarrier.Instance.babyHeight;
            int oldWeight = DataCarrier.Instance.babyWeight;

            // 年齢アップ & 累計経験値リセット
            DataCarrier.Instance.babyExp -= needed;
            DataCarrier.Instance.AgeUp();
            int newAge = DataCarrier.Instance.babyAge;

            // 年齢アップ演出
            battleLogText.text = $"<color=#FFD700><size=150%>\ud83c\udf82 {newAge}さいになった！ \ud83c\udf82</size></color>";
            yield return new WaitForSeconds(1.5f);

            // ステータスアップ演出パネルを表示
            yield return StartCoroutine(ShowStatGrowth(
                oldAtk, oldDef, oldHp, oldAcademic, oldAthletic, oldHeight, oldWeight,
                DataCarrier.Instance.babyAtk, DataCarrier.Instance.babyDef,
                DataCarrier.Instance.babyHp, DataCarrier.Instance.babyAcademic,
                DataCarrier.Instance.babyAthletic, DataCarrier.Instance.babyHeight,
                DataCarrier.Instance.babyWeight
            ));

            // プレイヤーステータスを更新
            playerAtk = DataCarrier.Instance.babyAtk;
            playerDef = DataCarrier.Instance.babyDef;
            playerMaxHp = DataCarrier.Instance.babyHp;
            playerHp = playerMaxHp; // HP全回復
            playerHeight = DataCarrier.Instance.babyHeight;
            playerWeight = DataCarrier.Instance.babyWeight;

            // 性別ボーナス再計算
            if (isMale)
            {
                playerAtk = (int)(playerAtk * 1.5f);
            }

            // GOD BABYボーナス再計算
            if (isGodBaby)
            {
                playerAtk = (int)(playerAtk * 1.2f);
                playerDef = (int)(playerDef * 1.2f);
                playerMaxHp = (int)(playerMaxHp * 1.2f);
                playerHp = playerMaxHp;
            }

            UpdatePlayerDisplay();
        }
        else
        {
            // レベルアップまで足りない — 経験値状況を表示
            int remaining = needed - currentExp;
            battleLogText.text = $"つぎのせいちょうまで あと <color=#FFFF00>{remaining}</color> けいけんち\n({currentExp}/{needed})";
            yield return new WaitForSeconds(1.5f);
        }
    }

    IEnumerator ShowStatGrowth(int oldAtk, int oldDef, int oldHp, int oldAcademic, int oldAthletic, int oldHeight, int oldWeight,
                                int newAtk, int newDef, int newHp, int newAcademic, int newAthletic, int newHeight, int newWeight)
    {
        // 成長パネルを作成
        var growthPanel = new GameObject("GrowthPanel");
        growthPanel.transform.SetParent(canvas.transform, false);

        var panelRect = growthPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(450, 350);

        var panelBg = growthPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

        // タイトル
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(growthPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -25);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "<color=#FFD700>✨ せいちょう！ ✨</color>";
        titleText.fontSize = 32;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        // ステータス表示
        var statsObj = new GameObject("Stats");
        statsObj.transform.SetParent(growthPanel.transform, false);
        var statsRect = statsObj.AddComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0);
        statsRect.anchorMax = new Vector2(1, 1);
        statsRect.offsetMin = new Vector2(30, 60);
        statsRect.offsetMax = new Vector2(-30, -60);
        var statsText = statsObj.AddComponent<TextMeshProUGUI>();
        statsText.fontSize = 22;
        statsText.alignment = TextAlignmentOptions.Left;
        statsText.raycastTarget = false;

        // ステータスを1行ずつ表示
        string[] statLines = new string[]
        {
            $"こうげき: {oldAtk} → <color=#00FF00>{newAtk}</color> <color=#FFFF00>(+{newAtk - oldAtk})</color>",
            $"ぼうぎょ: {oldDef} → <color=#00FF00>{newDef}</color> <color=#FFFF00>(+{newDef - oldDef})</color>",
            $"HP: {oldHp} → <color=#00FF00>{newHp}</color> <color=#FFFF00>(+{newHp - oldHp})</color>",
            $"がくりょく: {oldAcademic} → <color=#00FF00>{newAcademic}</color> <color=#FFFF00>(+{newAcademic - oldAcademic})</color>",
            $"うんどう: {oldAthletic} → <color=#00FF00>{newAthletic}</color> <color=#FFFF00>(+{newAthletic - oldAthletic})</color>",
            $"しんちょう: {oldHeight}cm → <color=#00FFFF>{newHeight}cm</color> <color=#FFFF00>(+{newHeight - oldHeight})</color>",
            $"たいじゅう: {oldWeight}g → <color=#00FFFF>{newWeight}g</color> <color=#FFFF00>(+{newWeight - oldWeight})</color>"
        };

        string displayText = "";
        foreach (string line in statLines)
        {
            displayText += line + "\n";
            statsText.text = displayText;
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(1.5f);

        // パネルを削除
        Destroy(growthPanel);

        battleLogText.text = "<color=#00FF00>HPがぜんかいふく！</color>";
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator BattleLose()
    {
        isBattleActive = false;
        battleLogText.text = "<color=#FF0000>たおれてしまった...</color>";
        yield return new WaitForSeconds(2.0f);

        ShowGameOverPanel();
    }

    void ShowGameOverPanel()
    {
        var resultPanel = new GameObject("ResultPanel");
        resultPanel.transform.SetParent(canvas.transform, false);

        var panelRect = resultPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 250);

        var panelBg = resultPanel.AddComponent<Image>();
        panelBg.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);

        var textObj = new GameObject("ResultText");
        textObj.transform.SetParent(resultPanel.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(20, 20);
        textRect.offsetMax = new Vector2(-20, -20);
        var resultText = textObj.AddComponent<TextMeshProUGUI>();
        resultText.text = "<color=#FF0000>GAME OVER</color>";
        resultText.fontSize = 48;
        resultText.alignment = TextAlignmentOptions.Center;
        resultText.fontStyle = FontStyles.Bold;
        resultText.raycastTarget = false;

        var retryBtn = new GameObject("RetryButton");
        retryBtn.transform.SetParent(resultPanel.transform, false);
        var retryRect = retryBtn.AddComponent<RectTransform>();
        retryRect.anchorMin = new Vector2(0.5f, 0);
        retryRect.anchorMax = new Vector2(0.5f, 0);
        retryRect.anchoredPosition = new Vector2(0, 50);
        retryRect.sizeDelta = new Vector2(200, 60);
        var retryImg = retryBtn.AddComponent<Image>();
        retryImg.color = new Color(0.3f, 0.5f, 0.7f);
        var retryButton = retryBtn.AddComponent<Button>();
        retryButton.targetGraphic = retryImg;
        retryButton.onClick.AddListener(() => SceneManager.LoadScene("TitleScene"));

        var retryTextObj = new GameObject("Text");
        retryTextObj.transform.SetParent(retryBtn.transform, false);
        var retryTextRect = retryTextObj.AddComponent<RectTransform>();
        retryTextRect.anchorMin = Vector2.zero;
        retryTextRect.anchorMax = Vector2.one;
        retryTextRect.offsetMin = Vector2.zero;
        retryTextRect.offsetMax = Vector2.zero;
        var retryTmp = retryTextObj.AddComponent<TextMeshProUGUI>();
        retryTmp.text = "もういちど";
        retryTmp.fontSize = 28;
        retryTmp.alignment = TextAlignmentOptions.Center;
        retryTmp.color = Color.white;
        retryTmp.raycastTarget = false;
    }

    // ===== ボス撃破演出 =====

    IEnumerator BossDefeatSequence()
    {
        string[] bossLines = new string[]
        {
            "村の王シバ が たおれた...\n",
            "「これは 試練に すぎなかった。」\n",
            "村の外には さらに強大な 敵が待っている。\nおまえの ちからは まだ 足りない。\n",
            "成長し、すべての 敵を 打ち砕け。\n世界は おまえを 待っている。\n",
        };

        float slideDuration = 1.5f;
        float lineInterval = 1.2f;
        float startOffsetX = -800f;

        // 暗転パネル
        var panel = new GameObject("BossDefeatPanel");
        panel.transform.SetParent(canvas.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0f);
        panelImage.raycastTarget = true;

        // フェードイン
        float elapsed = 0f;
        float fadeInDuration = 0.8f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            panelImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0f, 0.95f, elapsed / fadeInDuration));
            yield return null;
        }
        panelImage.color = new Color(0f, 0f, 0f, 0.95f);

        yield return new WaitForSeconds(0.5f);

        // テキストをスライドイン
        float verticalStart = 120f;
        float lineSpacing = 130f;

        for (int i = 0; i < bossLines.Length; i++)
        {
            var textObj = new GameObject("BossLine_" + i);
            textObj.transform.SetParent(panel.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            var tmp = textObj.AddComponent<TextMeshProUGUI>();

            tmp.text = bossLines[i];
            tmp.fontSize = i == 0 ? 40 : 32;
            tmp.color = i == 0 ? new Color(1f, 0.3f, 0.3f) : Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.fontStyle = i == 0 ? FontStyles.Bold : FontStyles.Normal;
            tmp.raycastTarget = false;

            float yPos = verticalStart - (i * lineSpacing);
            textRect.sizeDelta = new Vector2(800f, 120f);
            textRect.anchoredPosition = new Vector2(startOffsetX, yPos);

            // スライドインアニメーション
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

            if (i < bossLines.Length - 1)
            {
                yield return new WaitForSeconds(lineInterval);
            }
        }

        yield return new WaitForSeconds(2.5f);

        // フェードアウト
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        elapsed = 0f;
        float fadeOutDuration = 1.0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        // フラグをリセットしてマップへ
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
        }

        SceneManager.LoadScene("MapScene");
    }

    // ===== 勝利後 MapScene 遷移 =====

    IEnumerator VictoryToMap()
    {
        // 短い勝利演出
        var victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(canvas.transform, false);
        var panelRect = victoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        var panelBg = victoryPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.7f);
        panelBg.raycastTarget = false;

        var textObj = new GameObject("VictoryText");
        textObj.transform.SetParent(victoryPanel.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(600, 200);
        var victoryText = textObj.AddComponent<TextMeshProUGUI>();
        victoryText.fontSize = 36;
        victoryText.alignment = TextAlignmentOptions.Center;
        victoryText.color = Color.white;
        victoryText.fontStyle = FontStyles.Bold;
        victoryText.raycastTarget = false;

        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "ベイビー";
        int currentAge = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 0;

        victoryText.text = $"<color=#FFD700>{babyName}({currentAge}さい)</color>\n<color=#00FF00>セーブしました！</color>";
        yield return new WaitForSeconds(1.5f);

        victoryText.text = "むらに もどります...";
        yield return new WaitForSeconds(1.0f);

        // フラグをリセット
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
        }

        SceneManager.LoadScene("MapScene");
    }

    // ===== メニューバー =====

    void CreateMenuBar()
    {
        var bar = new GameObject("MenuBar");
        bar.transform.SetParent(canvas.transform, false);

        var barRect = bar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(1, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.pivot = new Vector2(1, 1);
        barRect.anchoredPosition = new Vector2(-20, -20);
        barRect.sizeDelta = new Vector2(360, 60);

        var barBg = bar.AddComponent<Image>();
        barBg.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
        barBg.raycastTarget = false;

        var layout = bar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.padding = new RectOffset(12, 12, 6, 6);
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateMenuButton(bar.transform, "セーブ", OnSave);
        CreateMenuButton(bar.transform, "トップへ", OnGoTop);
    }

    void CreateMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        var btnObj = new GameObject(label);
        btnObj.transform.SetParent(parent, false);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.45f, 1f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        var colors = btn.colors;
        colors.highlightedColor = new Color(0.45f, 0.45f, 0.65f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.35f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(action);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }

    void OnSave()
    {
        if (DataCarrier.Instance == null)
        {
            Debug.LogWarning("DataCarrierが見つかりません");
            return;
        }

        var dc = DataCarrier.Instance;
        PlayerPrefs.SetInt("babyAtk", dc.babyAtk);
        PlayerPrefs.SetInt("babyDef", dc.babyDef);
        PlayerPrefs.SetInt("babyHp", dc.babyHp);
        PlayerPrefs.SetInt("babyAcademic", dc.babyAcademic);
        PlayerPrefs.SetInt("babyWeight", dc.babyWeight);
        PlayerPrefs.SetInt("babyAthletic", dc.babyAthletic);
        PlayerPrefs.SetInt("babyHeight", dc.babyHeight);
        PlayerPrefs.SetString("trait1", dc.trait1 ?? "");
        PlayerPrefs.SetString("trait2", dc.trait2 ?? "");
        PlayerPrefs.SetString("fatherName", dc.fatherName ?? "");
        PlayerPrefs.SetString("motherName", dc.motherName ?? "");
        PlayerPrefs.SetString("babyGender", dc.babyGender ?? "");
        PlayerPrefs.SetInt("hasSaveData", 1);
        PlayerPrefs.Save();

        Debug.Log("セーブ完了！");
        StartCoroutine(ShowSaveMessage());
    }

    IEnumerator ShowSaveMessage()
    {
        string original = battleLogText.text;
        battleLogText.text = "<color=#00FF00>セーブしました！</color>";
        yield return new WaitForSeconds(1.2f);
        battleLogText.text = original;
    }

    void OnGoTop()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
