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
    string normalAttackName;   // 通常攻撃名(父ベース)
    string specialAttackName;  // 必殺技名
    string motherAttackName;   // 第2攻撃名(母ベース)
    string normalAttackDesc;   // 通常攻撃説明
    string specialAttackDesc;  // 必殺技説明
    string motherAttackDesc;   // 第2攻撃説明
    string defendDesc;
    string playerFatherName;   // アイコン決定用
    string playerMotherName;   // アイコン決定用
    string motherAttackEffect; // 第2攻撃の効果タイプ

    // 状態効果
    int enemyDefDebuffTurns;   // 敵DEFデバフ残りターン
    int enemyAtkDebuffTurns;   // 敵ATKデバフ残りターン
    int enemyPoisonTurns;      // 毒残りターン
    int playerEvasionBuffTurns;// 回避バフ残りターン

    // 技情報ポップアップ
    GameObject skillInfoPopup;

    // 母親ベースの第2攻撃データ（母親名 → [技名, 説明, 効果タイプ]）
    static readonly System.Collections.Generic.Dictionary<string, string[]> MotherSkillData = new System.Collections.Generic.Dictionary<string, string[]>
    {
        {"サクラ", new[]{"ヒーリングストライク", "攻撃しつつ自分のHPを回復する医療の技", "heal"}},
        {"ヒナタ", new[]{"毒霧", "敵に毒を浴びせ、3ターンの間じわじわダメージを与える", "poison"}},
        {"アキラ", new[]{"疾風ステップ", "素早い動きで攻撃し、2ターンの間回避率が上がる", "evasion"}},
        {"ミサト", new[]{"分析波動", "敵の弱点を解析し、2ターンの間敵の防御を下げる", "defdown"}},
        {"カエデ", new[]{"威圧のオーラ", "圧倒的な威圧感で、2ターンの間敵の攻撃力を下げる", "atkdown"}},
        {"ルナ", new[]{"スターダスト", "星屑をまとった攻撃。与ダメージの一部をHPとして吸収する", "drain"}},
    };

    // 36通りの固有技説明（父親名_母親名 → [通常技説明, 必殺技説明]）
    static readonly System.Collections.Generic.Dictionary<string, string[]> SkillDescData = new System.Collections.Generic.Dictionary<string, string[]>
    {
        {"タケシ_サクラ", new[]{"外科の精密さで急所を突くパンチ", "父の拳と母のメスが融合した一撃必殺の手術パンチ"}},
        {"タケシ_ヒナタ", new[]{"暗殺術を応用した見えないキック", "闇に溶ける格闘技の奥義。回避不能"}},
        {"タケシ_アキラ", new[]{"黄金に輝く連続パンチ", "オリンピック級の破壊力で叩き潰す"}},
        {"タケシ_ミサト", new[]{"量子力学で軌道を読めないパンチ", "重力すら歪める究極の一撃"}},
        {"タケシ_カエデ", new[]{"華麗なフォームの美しい一撃", "全財産を込めた黄金に輝く鉄拳"}},
        {"タケシ_ルナ", new[]{"カリスマ性を纏った蹴り", "見る者全てを魅了し打ち砕く衝撃波"}},

        {"ユウキ_サクラ", new[]{"電子メスで敵のデータを切り裂く", "完全なるサイバー手術で敵を分解する"}},
        {"ユウキ_ヒナタ", new[]{"痕跡を残さないハッキング攻撃", "対象のシステムを完全に暗殺するプログラム"}},
        {"ユウキ_アキラ", new[]{"高速データ転送のような連続攻撃", "電脳空間でのオリンピック級演算攻撃"}},
        {"ユウキ_ミサト", new[]{"量子コンピュータで弱点を解析", "技術的特異点を超えた破壊コード"}},
        {"ユウキ_カエデ", new[]{"敵の資産データを食い荒らすウイルス", "全世界の資産をハックする究極ウイルス"}},
        {"ユウキ_ルナ", new[]{"仮想空間から放つ光線", "デジタルとリアルを超越したオーラ攻撃"}},

        {"ゴウ_サクラ", new[]{"戦場仕込みの精密な切開攻撃", "戦場で命を救い命を奪う天使の一撃"}},
        {"ゴウ_ヒナタ", new[]{"傭兵と暗殺者の合わせ技", "影から影へ、姿なき暗殺者の最終奥義"}},
        {"ゴウ_アキラ", new[]{"軍事訓練で鍛えた突進攻撃", "戦場を駆け抜ける全力スプリント攻撃"}},
        {"ゴウ_ミサト", new[]{"戦術的量子機動による奇襲", "核融合エネルギーを戦術転用した究極兵器"}},
        {"ゴウ_カエデ", new[]{"金で雇った傭兵団の一斉攻撃", "黄金の弾丸で全てを制圧する"}},
        {"ゴウ_ルナ", new[]{"美しさで敵を油断させる迷彩術", "完全透明化からの奇襲攻撃"}},

        {"シンジ_サクラ", new[]{"論理的に最適な切開ポイントを突く", "ノーベル賞級の完璧な外科手術攻撃"}},
        {"シンジ_ヒナタ", new[]{"計算し尽くされた正確なキック", "IQ300の頭脳が導く暗殺の方程式"}},
        {"シンジ_アキラ", new[]{"物理法則を最大活用した攻撃", "科学の全てを結集したオリンピック級攻撃"}},
        {"シンジ_ミサト", new[]{"量子もつれで離れた敵にもダメージ", "二つの天才頭脳が融合した究極の知性攻撃"}},
        {"シンジ_カエデ", new[]{"経済理論に基づく効率的な攻撃", "ノーベル経済学賞の理論を破壊力に変換"}},
        {"シンジ_ルナ", new[]{"美の黄金比を計算した攻撃", "相対性理論で時空を歪めるオーラ"}},

        {"リョウマ_サクラ", new[]{"札束を投げつけて攻撃", "医療ビジネスの全資産を投入した一撃"}},
        {"リョウマ_ヒナタ", new[]{"金の力で繰り出すキック", "暗殺ビジネスの全てを賭けた攻撃"}},
        {"リョウマ_アキラ", new[]{"投資のように確実にリターンを得る攻撃", "金メダルごと買収する圧倒的資金力攻撃"}},
        {"リョウマ_ミサト", new[]{"量子投資理論に基づく攻撃", "無限の資産を生む理論の破壊的応用"}},
        {"リョウマ_カエデ", new[]{"財閥の力を見せつける連続攻撃", "兆を超える資産で全てを支配する"}},
        {"リョウマ_ルナ", new[]{"セレブの品格で圧倒する", "世界最高峰の富と美の融合攻撃"}},

        {"テツヤ_サクラ", new[]{"ロックのリズムで切り刻む", "ライブ会場が手術室になる究極パフォーマンス"}},
        {"テツヤ_ヒナタ", new[]{"静寂から放つロックの衝撃波", "音のない暗殺メロディ"}},
        {"テツヤ_アキラ", new[]{"高速ビートのような連続攻撃", "オリンピック級のライブパフォーマンス攻撃"}},
        {"テツヤ_ミサト", new[]{"量子力学的な音波攻撃", "観測するまで生死不明な究極の歌"}},
        {"テツヤ_カエデ", new[]{"プラチナディスク級の衝撃を与える", "音楽史に刻まれる究極のアンセム攻撃"}},
        {"テツヤ_ルナ", new[]{"スターのオーラで圧倒する", "超新星のように全てを飲み込むハーモニー"}},
    };

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
    int enemyHp, enemyMaxHp, enemyAtk, enemyDef, enemySpeed;
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

    // 演出用
    Image battleFlashOverlay;
    RectTransform playerPanelRect;
    RectTransform enemyPanelRect;

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
                // GOD BABYボーナス: 全ステータス+12%、必殺技命中率100%
                playerAtk = (int)(playerAtk * 1.12f);
                playerDef = (int)(playerDef * 1.12f);
                playerMaxHp = (int)(playerMaxHp * 1.12f);
                playerHp = playerMaxHp;
            }

            // 親の組み合わせから固有技を取得
            playerFatherName = DataCarrier.Instance.fatherName ?? "";
            playerMotherName = DataCarrier.Instance.motherName ?? "";
            string parentKey = $"{DataCarrier.Instance.fatherName}_{DataCarrier.Instance.motherName}";
            if (SkillData.TryGetValue(parentKey, out string[] skills))
            {
                normalAttackName = skills[0];
                specialAttackName = skills[1];
            }
            else
            {
                normalAttackName = Localization.Get("battle_default_attack");
                specialAttackName = Localization.Get("battle_default_special");
            }
            if (SkillDescData.TryGetValue(parentKey, out string[] descs))
            {
                normalAttackDesc = descs[0];
                specialAttackDesc = descs[1];
            }
            else
            {
                normalAttackDesc = Localization.Get("battle_default_attack_desc");
                specialAttackDesc = Localization.Get("battle_default_special_desc");
            }
            defendDesc = Localization.Get("battle_defend_desc");
            // 母親ベースの第2攻撃
            if (MotherSkillData.TryGetValue(playerMotherName, out string[] mSkill))
            {
                motherAttackName = Localization.GetMotherSkillName(playerMotherName);
                motherAttackDesc = Localization.GetMotherSkillDesc(playerMotherName);
                motherAttackEffect = mSkill[2];
            }
            else
            {
                motherAttackName = Localization.Get("battle_default_mother_attack");
                motherAttackDesc = Localization.Get("battle_default_mother_desc");
                motherAttackEffect = "none";
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
            normalAttackName = Localization.Get("battle_default_attack");
            specialAttackName = Localization.Get("battle_default_special");
            normalAttackDesc = Localization.Get("battle_default_attack_desc");
            specialAttackDesc = Localization.Get("battle_default_special_desc");
            defendDesc = Localization.Get("battle_defend_desc");
            motherAttackName = Localization.Get("battle_default_mother_attack");
            motherAttackDesc = Localization.Get("battle_default_mother_desc");
            motherAttackEffect = "none";
            playerFatherName = "";
            playerMotherName = "";
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
            enemySpeed = 70;
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
            enemyName = "あばれんぼうベイビー";
            enemyMaxHp = 150;
            enemyHp = enemyMaxHp;
            enemyAtk = 45;
            enemyDef = 25;
            enemySpeed = 40;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/common-abarennbou");
        }
    }

    void InitializeRandomEnemy(int playerAge)
    {
        // 月齢に応じてスケーリング
        float scale = 1.0f + (playerAge - 1) * 0.08f;

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
        enemySpeed = Mathf.RoundToInt(30 * scale + Random.Range(0, 20));
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
        panelRect.sizeDelta = new Vector2(900, 420);

        // プレイヤー側（左）
        CreateCharacterPanel(battlePanel.transform, -300, true, out playerFaceImage, out playerNameText, out playerHpBar, out playerHpText);
        playerPanelRect = playerFaceImage.transform.parent.GetComponent<RectTransform>();

        // VS テキスト
        CreateVsText(battlePanel.transform);

        // 敵側（右）
        CreateCharacterPanel(battlePanel.transform, 300, false, out enemyFaceImage, out enemyNameText, out enemyHpBar, out enemyHpText);
        enemyPanelRect = enemyFaceImage.transform.parent.GetComponent<RectTransform>();

        // バトルログ
        CreateBattleLog();

        // アクションボタン
        CreateActionButtons();

        // 演出用フラッシュオーバーレイ
        var flashObj = new GameObject("BattleFlash");
        flashObj.transform.SetParent(canvas.transform, false);
        var flashRect = flashObj.AddComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;
        battleFlashOverlay = flashObj.AddComponent<Image>();
        battleFlashOverlay.color = new Color(1f, 1f, 1f, 0f);
        battleFlashOverlay.raycastTarget = false;

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
        panelRect.sizeDelta = new Vector2(340, 440);

        var panelBg = panel.AddComponent<Image>();
        panelBg.sprite = CreateRoundedRectSprite(64, 64, 16);
        panelBg.type = Image.Type.Sliced;
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
        faceRect.sizeDelta = new Vector2(260, 260);
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
        hpBgImage.sprite = CreateRoundedRectSprite(32, 32, 6);
        hpBgImage.type = Image.Type.Sliced;
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
        logBg.sprite = CreateRoundedRectSprite(64, 64, 12);
        logBg.type = Image.Type.Sliced;
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
        panelRect.sizeDelta = new Vector2(700, 100);

        var layout = actionPanel.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        attackButton = CreateActionButton(actionPanel.transform, Localization.Get("battle_normal_attack"), normalAttackName, normalAttackDesc, new Color(0.8f, 0.3f, 0.3f), OnAttack, out attackButtonText);
        CreateActionButton(actionPanel.transform, Localization.Get("battle_special_attack"), motherAttackName, motherAttackDesc, new Color(0.3f, 0.7f, 0.5f), OnMotherAttack, out _);
        defendButton = CreateActionButton(actionPanel.transform, Localization.Get("battle_defend"), Localization.Get("battle_defend_name"), defendDesc, new Color(0.3f, 0.5f, 0.8f), OnDefend, out _);
        specialButton = CreateActionButton(actionPanel.transform, Localization.Get("battle_special_skill"), specialAttackName, specialAttackDesc, new Color(0.8f, 0.6f, 0.2f), OnSpecial, out specialButtonText);

        actionPanel.SetActive(false);
    }

    Button CreateActionButton(Transform parent, string categoryLabel, string skillName, string description, Color bgColor, UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI buttonText)
    {
        var btnObj = new GameObject(skillName + "Button");
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.sprite = CreateRoundedRectSprite(64, 64, 14);
        btnImg.type = Image.Type.Sliced;
        btnImg.color = bgColor;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(onClick);

        // カテゴリラベル（上部）
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.6f);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(4, 0);
        labelRect.offsetMax = new Vector2(-4, -4);
        var labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        labelTmp.text = categoryLabel;
        labelTmp.fontSize = 14;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = new Color(1f, 1f, 1f, 0.7f);
        labelTmp.raycastTarget = false;

        // 技名（下部）
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 0.65f);
        textRect.offsetMin = new Vector2(4, 4);
        textRect.offsetMax = new Vector2(-4, 0);
        buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = skillName;
        buttonText.fontSize = 20;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;

        // infoボタン（右上）
        var infoObj = new GameObject("InfoButton");
        infoObj.transform.SetParent(btnObj.transform, false);
        var infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(1, 1);
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.pivot = new Vector2(1, 1);
        infoRect.anchoredPosition = new Vector2(-2, -2);
        infoRect.sizeDelta = new Vector2(24, 24);

        var infoBg = infoObj.AddComponent<Image>();
        infoBg.sprite = CreateRoundedRectSprite(32, 32, 16);
        infoBg.type = Image.Type.Sliced;
        infoBg.color = new Color(1f, 1f, 1f, 0.3f);

        var infoBtn = infoObj.AddComponent<Button>();
        infoBtn.targetGraphic = infoBg;
        string descCapture = description;
        string nameCapture = skillName;
        string catCapture = categoryLabel;
        infoBtn.onClick.AddListener(() => ShowSkillInfo(catCapture, nameCapture, descCapture));

        var infoTextObj = new GameObject("InfoText");
        infoTextObj.transform.SetParent(infoObj.transform, false);
        var infoTextRect = infoTextObj.AddComponent<RectTransform>();
        infoTextRect.anchorMin = Vector2.zero;
        infoTextRect.anchorMax = Vector2.one;
        infoTextRect.offsetMin = Vector2.zero;
        infoTextRect.offsetMax = Vector2.zero;
        var infoTmp = infoTextObj.AddComponent<TextMeshProUGUI>();
        infoTmp.text = "i";
        infoTmp.fontSize = 16;
        infoTmp.alignment = TextAlignmentOptions.Center;
        infoTmp.color = Color.white;
        infoTmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
        infoTmp.raycastTarget = false;

        return btn;
    }

    void ShowSkillInfo(string category, string skillName, string description)
    {
        // 既に開いていたら閉じる
        if (skillInfoPopup != null)
        {
            Destroy(skillInfoPopup);
            skillInfoPopup = null;
            return;
        }

        skillInfoPopup = new GameObject("SkillInfoPopup");
        skillInfoPopup.transform.SetParent(canvas.transform, false);

        // 背景タップで閉じるオーバーレイ
        var overlayObj = new GameObject("Overlay");
        overlayObj.transform.SetParent(skillInfoPopup.transform, false);
        var overlayRect = overlayObj.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.4f);
        var overlayBtn = overlayObj.AddComponent<Button>();
        overlayBtn.targetGraphic = overlayImg;
        overlayBtn.onClick.AddListener(() => { Destroy(skillInfoPopup); skillInfoPopup = null; });

        // ポップアップパネル
        var popupObj = new GameObject("PopupPanel");
        popupObj.transform.SetParent(skillInfoPopup.transform, false);
        var popupRect = popupObj.AddComponent<RectTransform>();
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.anchoredPosition = new Vector2(0, -50);
        popupRect.sizeDelta = new Vector2(420, 260);

        var popupBg = popupObj.AddComponent<Image>();
        popupBg.sprite = CreateRoundedRectSprite(64, 64, 16);
        popupBg.type = Image.Type.Sliced;
        popupBg.color = new Color(0.12f, 0.12f, 0.22f, 0.95f);

        // アイコン表示エリア（上部）
        var iconArea = new GameObject("IconArea");
        iconArea.transform.SetParent(popupObj.transform, false);
        var iconRect = iconArea.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 1);
        iconRect.anchorMax = new Vector2(0.5f, 1);
        iconRect.anchoredPosition = new Vector2(0, -60);
        iconRect.sizeDelta = new Vector2(90, 90);
        // アイコン背景円
        var iconBg = iconArea.AddComponent<Image>();
        iconBg.sprite = CreateRoundedRectSprite(64, 64, 32);
        iconBg.type = Image.Type.Sliced;
        iconBg.color = new Color(0.08f, 0.08f, 0.15f, 0.8f);
        iconBg.raycastTarget = false;
        // アイコン描画
        bool isSpecialSkill = category == Localization.Get("battle_special_skill");
        if (category == Localization.Get("battle_defend"))
            DrawShieldIcon(iconArea.transform, 80);
        else if (category == Localization.Get("battle_special_attack"))
            DrawMotherIcon(iconArea.transform, playerMotherName, 80);
        else
            GenerateSkillIcon(iconArea.transform, playerFatherName, isSpecialSkill, 70);

        // カテゴリ + 技名
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(popupObj.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -115);
        titleRect.sizeDelta = new Vector2(-30, 40);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = $"<color=#AAAAAA><size=70%>{category}</size></color>  <color=#FFDD44>{skillName}</color>";
        titleText.fontSize = 26;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.raycastTarget = false;

        // 説明文
        var descObj = new GameObject("Desc");
        descObj.transform.SetParent(popupObj.transform, false);
        var descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0, 0);
        descRect.anchorMax = new Vector2(1, 1);
        descRect.offsetMin = new Vector2(20, 20);
        descRect.offsetMax = new Vector2(-20, -140);
        var descText = descObj.AddComponent<TextMeshProUGUI>();
        descText.text = description;
        descText.fontSize = 20;
        descText.alignment = TextAlignmentOptions.Center;
        descText.color = Color.white;
        descText.enableWordWrapping = true;
        descText.raycastTarget = false;
    }

    // ===== 表示更新 =====

    void UpdatePlayerDisplay()
    {
        string babyName = Localization.Get("birth_default_name");
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
        string ageText = $"({Localization.GetAge(babyAge)})";
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
            ? $"<color=#66ccff>ATK:{playerAtk}</color> EVA:{playerEvasion}%"
            : $"<color=#ff99cc>EVA:{playerEvasion}%</color> ({sizeInfo})";
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

    Sprite CreateRoundedRectSprite(int width, int height, int radius)
    {
        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 clear = new Color32(0, 0, 0, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // 四隅の角丸判定
                bool inside = true;
                if (x < radius && y < radius)
                    inside = (radius - x) * (radius - x) + (radius - y) * (radius - y) <= radius * radius;
                else if (x >= width - radius && y < radius)
                    inside = (x - (width - radius - 1)) * (x - (width - radius - 1)) + (radius - y) * (radius - y) <= radius * radius;
                else if (x < radius && y >= height - radius)
                    inside = (radius - x) * (radius - x) + (y - (height - radius - 1)) * (y - (height - radius - 1)) <= radius * radius;
                else if (x >= width - radius && y >= height - radius)
                    inside = (x - (width - radius - 1)) * (x - (width - radius - 1)) + (y - (height - radius - 1)) * (y - (height - radius - 1)) <= radius * radius;

                tex.SetPixel(x, y, inside ? white : clear);
            }
        }

        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f,
            0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return sprite;
    }

    // ===== 技アイコン生成 =====

    void GenerateSkillIcon(Transform parent, string fatherName, bool isSpecial, float size)
    {
        float s = size / 120f;
        Color mainColor, accentColor;

        // 父親の職業でアイコンテーマを決定
        switch (fatherName)
        {
            case "タケシ": // 格闘家 → 拳
                mainColor = new Color(0.9f, 0.3f, 0.2f);
                accentColor = new Color(1f, 0.6f, 0.2f);
                DrawFistIcon(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "ユウキ": // ハッカー → 回路
                mainColor = new Color(0.1f, 0.8f, 0.9f);
                accentColor = new Color(0.3f, 1f, 0.5f);
                DrawCircuitIcon(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "ゴウ": // 傭兵 → 剣
                mainColor = new Color(0.4f, 0.6f, 0.3f);
                accentColor = new Color(0.8f, 0.8f, 0.8f);
                DrawBladeIcon(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "シンジ": // 天才科学者 → 原子
                mainColor = new Color(0.5f, 0.3f, 0.9f);
                accentColor = new Color(0.8f, 0.5f, 1f);
                DrawAtomIcon(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "リョウマ": // 実業家 → コイン
                mainColor = new Color(1f, 0.84f, 0f);
                accentColor = new Color(1f, 0.95f, 0.5f);
                DrawCoinIcon(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "テツヤ": // ロックスター → 音符
                mainColor = new Color(0.9f, 0.3f, 0.7f);
                accentColor = new Color(1f, 0.5f, 0.9f);
                DrawMusicIcon(parent, s, mainColor, accentColor, isSpecial);
                break;
            default:
                mainColor = new Color(1f, 1f, 1f);
                accentColor = new Color(0.8f, 0.8f, 0.8f);
                DrawFistIcon(parent, s, mainColor, accentColor, isSpecial);
                break;
        }

        // 必殺技は外側に放射エフェクト
        if (isSpecial)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                float rad = angle * Mathf.Deg2Rad;
                float dist = 48 * s;
                var ray = BPart($"Ray{i}", parent,
                    new Vector2(Mathf.Cos(rad) * dist, Mathf.Sin(rad) * dist),
                    new Vector2(6 * s, 22 * s));
                ray.transform.localRotation = Quaternion.Euler(0, 0, angle - 90);
                ray.AddComponent<Image>().color = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
            }
        }
    }

    void DrawFistIcon(Transform p, float s, Color main, Color accent, bool sp)
    {
        // 拳の影
        BPart("Shadow", p, new Vector2(2*s, -2*s), new Vector2(50*s, 55*s)).AddComponent<Image>().color = new Color(0,0,0,0.3f);
        // 拳ベース
        BPart("Palm", p, Vector2.zero, new Vector2(45*s, 50*s)).AddComponent<Image>().color = main;
        // 指の関節（4本）
        for (int i = 0; i < 4; i++)
        {
            float fx = -14*s + i * 9*s;
            BPart($"Knuckle{i}", p, new Vector2(fx, 22*s), new Vector2(8*s, 16*s)).AddComponent<Image>().color = new Color(main.r*0.8f, main.g*0.8f, main.b*0.8f);
        }
        // 親指
        BPart("Thumb", p, new Vector2(-20*s, 0), new Vector2(12*s, 20*s)).AddComponent<Image>().color = new Color(main.r*0.9f, main.g*0.85f, main.b*0.85f);
        // ハイライト
        BPart("HL", p, new Vector2(5*s, 8*s), new Vector2(15*s, 20*s)).AddComponent<Image>().color = new Color(1,1,1,0.2f);
        // 衝撃線
        if (sp)
        {
            for (int i = 0; i < 3; i++)
            {
                float lx = 28*s + i*8*s;
                BPart($"Impact{i}", p, new Vector2(lx, (10 - i*8)*s), new Vector2(12*s, 3*s)).AddComponent<Image>().color = accent;
            }
        }
    }

    void DrawCircuitIcon(Transform p, float s, Color main, Color accent, bool sp)
    {
        // 基盤背景
        BPart("Board", p, Vector2.zero, new Vector2(50*s, 50*s)).AddComponent<Image>().color = new Color(0.08f, 0.12f, 0.18f);
        // 横線
        for (int i = 0; i < 3; i++)
        {
            BPart($"HLine{i}", p, new Vector2(0, (15 - i*15)*s), new Vector2(46*s, 2*s)).AddComponent<Image>().color = main;
        }
        // 縦線
        for (int i = 0; i < 3; i++)
        {
            BPart($"VLine{i}", p, new Vector2((-15 + i*15)*s, 0), new Vector2(2*s, 46*s)).AddComponent<Image>().color = main;
        }
        // ノード
        float[] nx = {-15, 0, 15, -15, 15};
        float[] ny = {15, 0, 15, -15, -15};
        for (int i = 0; i < nx.Length; i++)
        {
            BPart($"Node{i}", p, new Vector2(nx[i]*s, ny[i]*s), new Vector2(7*s, 7*s)).AddComponent<Image>().color = accent;
        }
        // 中央チップ
        BPart("Chip", p, Vector2.zero, new Vector2(14*s, 14*s)).AddComponent<Image>().color = sp ? accent : main;
    }

    void DrawBladeIcon(Transform p, float s, Color main, Color accent, bool sp)
    {
        // 刀身影
        BPart("Shadow", p, new Vector2(2*s, -2*s), new Vector2(12*s, 60*s)).AddComponent<Image>().color = new Color(0,0,0,0.3f);
        // 刀身
        BPart("Blade", p, new Vector2(0, 8*s), new Vector2(10*s, 55*s)).AddComponent<Image>().color = accent;
        // 刀身ハイライト
        BPart("BladeHL", p, new Vector2(-2*s, 8*s), new Vector2(3*s, 50*s)).AddComponent<Image>().color = new Color(1,1,1,0.3f);
        // 鍔
        BPart("Guard", p, new Vector2(0, -18*s), new Vector2(24*s, 6*s)).AddComponent<Image>().color = main;
        // 柄
        BPart("Hilt", p, new Vector2(0, -30*s), new Vector2(7*s, 20*s)).AddComponent<Image>().color = new Color(main.r*0.6f, main.g*0.5f, main.b*0.3f);
        // 切れ味エフェクト
        if (sp)
        {
            var slash = BPart("Slash", p, new Vector2(0, 10*s), new Vector2(50*s, 4*s));
            slash.transform.localRotation = Quaternion.Euler(0, 0, 35);
            slash.AddComponent<Image>().color = new Color(1, 1, 1, 0.6f);
        }
    }

    void DrawAtomIcon(Transform p, float s, Color main, Color accent, bool sp)
    {
        // 核
        BPart("Nucleus", p, Vector2.zero, new Vector2(14*s, 14*s)).AddComponent<Image>().color = accent;
        // 軌道（3本 楕円を矩形で近似）
        float[] angles = {0, 60, -60};
        foreach (float a in angles)
        {
            var orbit = BPart("Orbit", p, Vector2.zero, new Vector2(50*s, 18*s));
            orbit.transform.localRotation = Quaternion.Euler(0, 0, a);
            var img = orbit.AddComponent<Image>();
            img.color = new Color(main.r, main.g, main.b, 0.4f);
        }
        // 電子（3個）
        float[] ea = {30, 150, 270};
        for (int i = 0; i < 3; i++)
        {
            float rad = ea[i] * Mathf.Deg2Rad;
            float ex = Mathf.Cos(rad) * 22 * s;
            float ey = Mathf.Sin(rad) * 22 * s;
            BPart($"Electron{i}", p, new Vector2(ex, ey), new Vector2(6*s, 6*s)).AddComponent<Image>().color = sp ? accent : main;
        }
    }

    void DrawCoinIcon(Transform p, float s, Color main, Color accent, bool sp)
    {
        // コイン影
        BPart("Shadow", p, new Vector2(2*s, -2*s), new Vector2(48*s, 48*s)).AddComponent<Image>().color = new Color(0,0,0,0.3f);
        // コイン外側
        BPart("Outer", p, Vector2.zero, new Vector2(46*s, 46*s)).AddComponent<Image>().color = main;
        // コイン内側
        BPart("Inner", p, Vector2.zero, new Vector2(36*s, 36*s)).AddComponent<Image>().color = new Color(main.r*0.85f, main.g*0.75f, main.b*0.1f);
        // ¥マーク用テキスト
        var yenObj = BPart("Yen", p, Vector2.zero, new Vector2(30*s, 30*s));
        var yenText = yenObj.AddComponent<TextMeshProUGUI>();
        yenText.text = "¥";
        yenText.fontSize = Mathf.RoundToInt(24 * s);
        yenText.alignment = TextAlignmentOptions.Center;
        yenText.color = accent;
        yenText.fontStyle = FontStyles.Bold;
        yenText.raycastTarget = false;
        // キラキラ
        if (sp)
        {
            float[] sx = {20, -18, 5};
            float[] sy = {18, 20, -22};
            for (int i = 0; i < 3; i++)
            {
                BPart($"Spark{i}", p, new Vector2(sx[i]*s, sy[i]*s), new Vector2(5*s, 5*s)).AddComponent<Image>().color = new Color(1,1,1,0.8f);
            }
        }
    }

    void DrawMusicIcon(Transform p, float s, Color main, Color accent, bool sp)
    {
        // 音符の頭
        BPart("NoteHead1", p, new Vector2(-8*s, -12*s), new Vector2(16*s, 12*s)).AddComponent<Image>().color = main;
        BPart("NoteHead2", p, new Vector2(12*s, -6*s), new Vector2(16*s, 12*s)).AddComponent<Image>().color = main;
        // 棒
        BPart("Stem1", p, new Vector2(-1*s, 10*s), new Vector2(3*s, 38*s)).AddComponent<Image>().color = main;
        BPart("Stem2", p, new Vector2(19*s, 16*s), new Vector2(3*s, 38*s)).AddComponent<Image>().color = main;
        // 連結旗
        BPart("Flag", p, new Vector2(9*s, 28*s), new Vector2(22*s, 4*s)).AddComponent<Image>().color = accent;
        BPart("Flag2", p, new Vector2(9*s, 22*s), new Vector2(22*s, 4*s)).AddComponent<Image>().color = accent;
        // 音波エフェクト
        if (sp)
        {
            for (int i = 1; i <= 3; i++)
            {
                var wave = BPart($"Wave{i}", p, new Vector2(28*s, (12 - i*6)*s), new Vector2(4*s, (10+i*4)*s));
                wave.AddComponent<Image>().color = new Color(accent.r, accent.g, accent.b, 0.7f - i*0.15f);
            }
        }
    }

    // 防御アイコン（盾）
    void DrawShieldIcon(Transform parent, float size)
    {
        float s = size / 120f;
        Color main = new Color(0.3f, 0.5f, 0.8f);
        // 盾影
        BPart("Shadow", parent, new Vector2(2*s, -2*s), new Vector2(42*s, 50*s)).AddComponent<Image>().color = new Color(0,0,0,0.3f);
        // 盾ベース
        BPart("Shield", parent, Vector2.zero, new Vector2(40*s, 48*s)).AddComponent<Image>().color = main;
        // 盾内側
        BPart("Inner", parent, new Vector2(0, 2*s), new Vector2(30*s, 36*s)).AddComponent<Image>().color = new Color(main.r*0.7f, main.g*0.7f, main.b*0.9f);
        // 十字
        BPart("CrossH", parent, Vector2.zero, new Vector2(20*s, 5*s)).AddComponent<Image>().color = new Color(1,1,1,0.5f);
        BPart("CrossV", parent, Vector2.zero, new Vector2(5*s, 20*s)).AddComponent<Image>().color = new Color(1,1,1,0.5f);
        // 下部の尖り
        BPart("Tip", parent, new Vector2(0, -22*s), new Vector2(16*s, 10*s)).AddComponent<Image>().color = main;
    }

    // 攻撃時に画面中央にアイコンをカットイン表示
    IEnumerator ShowSkillCutIn(bool isSpecial)
    {
        var cutIn = new GameObject("SkillCutIn");
        cutIn.transform.SetParent(canvas.transform, false);

        var bgRect = cutIn.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        float iconSize = isSpecial ? 160f : 120f;
        bgRect.sizeDelta = new Vector2(iconSize, iconSize);

        // 背景円
        var bgImg = cutIn.AddComponent<Image>();
        bgImg.sprite = CreateRoundedRectSprite(64, 64, 32);
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0, 0, 0, 0.5f);
        bgImg.raycastTarget = false;

        // アイコン生成
        GenerateSkillIcon(cutIn.transform, playerFatherName, isSpecial, iconSize * 0.8f);

        // プレイヤー側から敵側へスライド
        CanvasGroup cg = cutIn.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        float startX = playerPanelRect != null ? playerPanelRect.anchoredPosition.x : -300f;
        float endX = enemyPanelRect != null ? enemyPanelRect.anchoredPosition.x : 300f;
        bgRect.anchoredPosition = new Vector2(startX, 0);

        float fadeIn = 0.1f;
        float elapsed = 0f;
        cutIn.transform.localScale = Vector3.one * 0.5f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeIn;
            cg.alpha = t;
            cutIn.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, t);
            yield return null;
        }
        cg.alpha = 1f;
        cutIn.transform.localScale = Vector3.one;

        // プレイヤー→敵へスライド
        float slideDuration = isSpecial ? 0.5f : 0.35f;
        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            bgRect.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, t), 0);
            yield return null;
        }
        bgRect.anchoredPosition = new Vector2(endX, 0);

        // フェードアウト
        float fadeOut = 0.15f;
        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - elapsed / fadeOut;
            yield return null;
        }

        Destroy(cutIn);
    }

    // 母ベース技のカットイン
    IEnumerator ShowMotherSkillCutIn()
    {
        var cutIn = new GameObject("MotherCutIn");
        cutIn.transform.SetParent(canvas.transform, false);

        var bgRect = cutIn.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        bgRect.sizeDelta = new Vector2(130, 130);

        var bgImg = cutIn.AddComponent<Image>();
        bgImg.sprite = CreateRoundedRectSprite(64, 64, 32);
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0, 0, 0, 0.5f);
        bgImg.raycastTarget = false;

        DrawMotherIcon(cutIn.transform, playerMotherName, 100);

        CanvasGroup cg = cutIn.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        float startX = playerPanelRect != null ? playerPanelRect.anchoredPosition.x : -300f;
        float endX = enemyPanelRect != null ? enemyPanelRect.anchoredPosition.x : 300f;
        bgRect.anchoredPosition = new Vector2(startX, 0);

        cutIn.transform.localScale = Vector3.one * 0.5f;
        float elapsed = 0f;
        float fadeIn = 0.1f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeIn;
            cg.alpha = t;
            cutIn.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1f, t);
            yield return null;
        }
        cg.alpha = 1f;
        cutIn.transform.localScale = Vector3.one;

        float slideDuration = 0.35f;
        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            bgRect.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, t), 0);
            yield return null;
        }
        bgRect.anchoredPosition = new Vector2(endX, 0);

        elapsed = 0f;
        float fadeOut = 0.15f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            cg.alpha = 1f - elapsed / fadeOut;
            yield return null;
        }
        Destroy(cutIn);
    }

    // 母親別アイコン
    void DrawMotherIcon(Transform parent, string motherName, float size)
    {
        float s = size / 120f;
        switch (motherName)
        {
            case "サクラ": // 暗殺拳 → 毒瓶
                BPart("Bottle", parent, new Vector2(0, -5*s), new Vector2(20*s, 30*s)).AddComponent<Image>().color = new Color(0.3f, 0.1f, 0.4f);
                BPart("Neck", parent, new Vector2(0, 12*s), new Vector2(10*s, 12*s)).AddComponent<Image>().color = new Color(0.3f, 0.1f, 0.4f);
                BPart("Cork", parent, new Vector2(0, 20*s), new Vector2(14*s, 6*s)).AddComponent<Image>().color = new Color(0.6f, 0.4f, 0.2f);
                BPart("Skull", parent, new Vector2(0, -4*s), new Vector2(10*s, 10*s)).AddComponent<Image>().color = new Color(0.8f, 0.2f, 1f);
                // 煙
                BPart("Smoke1", parent, new Vector2(-6*s, 26*s), new Vector2(6*s, 8*s)).AddComponent<Image>().color = new Color(0.6f, 0f, 0.8f, 0.4f);
                BPart("Smoke2", parent, new Vector2(4*s, 30*s), new Vector2(8*s, 6*s)).AddComponent<Image>().color = new Color(0.6f, 0f, 0.8f, 0.3f);
                break;
            case "ヒナタ": // 美容帝国CEO → 威圧の目
                BPart("EyeWhite", parent, Vector2.zero, new Vector2(40*s, 22*s)).AddComponent<Image>().color = new Color(0.95f, 0.9f, 0.8f);
                BPart("Iris", parent, Vector2.zero, new Vector2(18*s, 18*s)).AddComponent<Image>().color = new Color(0.8f, 0.6f, 0.1f);
                BPart("Pupil", parent, Vector2.zero, new Vector2(8*s, 8*s)).AddComponent<Image>().color = new Color(0.1f, 0.05f, 0f);
                BPart("HL", parent, new Vector2(-3*s, 3*s), new Vector2(4*s, 4*s)).AddComponent<Image>().color = Color.white;
                // 威圧線
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    var line = BPart($"Pressure{i}", parent, new Vector2(Mathf.Cos(a)*28*s, Mathf.Sin(a)*28*s), new Vector2(10*s, 3*s));
                    line.transform.localRotation = Quaternion.Euler(0, 0, i*60);
                    line.AddComponent<Image>().color = new Color(1f, 0.7f, 0.1f, 0.5f);
                }
                break;
            case "アキラ": // アスリート → 風/稲妻
                var bolt1 = BPart("Bolt1", parent, new Vector2(-4*s, 12*s), new Vector2(18*s, 6*s));
                bolt1.transform.localRotation = Quaternion.Euler(0, 0, 30);
                bolt1.AddComponent<Image>().color = new Color(0f, 0.9f, 1f);
                var bolt2 = BPart("Bolt2", parent, new Vector2(4*s, 0), new Vector2(18*s, 6*s));
                bolt2.transform.localRotation = Quaternion.Euler(0, 0, -30);
                bolt2.AddComponent<Image>().color = new Color(0f, 0.9f, 1f);
                var bolt3 = BPart("Bolt3", parent, new Vector2(-4*s, -12*s), new Vector2(18*s, 6*s));
                bolt3.transform.localRotation = Quaternion.Euler(0, 0, 30);
                bolt3.AddComponent<Image>().color = new Color(0f, 0.9f, 1f);
                // 風線
                for (int i = 0; i < 3; i++)
                {
                    BPart($"Wind{i}", parent, new Vector2(22*s, (10-i*10)*s), new Vector2(14*s, 2*s)).AddComponent<Image>().color = new Color(0.5f, 1f, 1f, 0.5f);
                }
                break;
            case "ミサト": // 量子科学者 → 波動
                for (int i = 1; i <= 3; i++)
                {
                    float sz = i * 16 * s;
                    var ring = BPart($"Ring{i}", parent, Vector2.zero, new Vector2(sz, sz));
                    ring.AddComponent<Image>().color = new Color(0.4f, 0.2f, 1f, 0.5f - i*0.1f);
                }
                BPart("Core", parent, Vector2.zero, new Vector2(10*s, 10*s)).AddComponent<Image>().color = new Color(0.8f, 0.5f, 1f);
                break;
            case "カエデ": // 天才外科医 → 十字
                BPart("CrossH", parent, Vector2.zero, new Vector2(45*s, 15*s)).AddComponent<Image>().color = new Color(0.2f, 0.9f, 0.4f);
                BPart("CrossV", parent, Vector2.zero, new Vector2(15*s, 45*s)).AddComponent<Image>().color = new Color(0.2f, 0.9f, 0.4f);
                BPart("Heart", parent, new Vector2(0, 15*s), new Vector2(10*s, 10*s)).AddComponent<Image>().color = new Color(1f, 0.3f, 0.4f);
                break;
            case "ルナ": // モデル → 星屑
                float[] starAngles = {0, 72, 144, 216, 288};
                for (int i = 0; i < 5; i++)
                {
                    float rad = starAngles[i] * Mathf.Deg2Rad;
                    float dist = 18 * s;
                    BPart($"Star{i}", parent, new Vector2(Mathf.Cos(rad)*dist, Mathf.Sin(rad)*dist), new Vector2(8*s, 8*s))
                        .AddComponent<Image>().color = new Color(1f, 0.9f, 0.5f);
                }
                BPart("Center", parent, Vector2.zero, new Vector2(14*s, 14*s)).AddComponent<Image>().color = new Color(1f, 0.7f, 0.9f);
                // きらきら
                for (int i = 0; i < 4; i++)
                {
                    float a2 = (i * 90 + 45) * Mathf.Deg2Rad;
                    BPart($"Sparkle{i}", parent, new Vector2(Mathf.Cos(a2)*30*s, Mathf.Sin(a2)*30*s), new Vector2(4*s, 4*s))
                        .AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.6f);
                }
                break;
            default:
                BPart("Default", parent, Vector2.zero, new Vector2(30*s, 30*s)).AddComponent<Image>().color = Color.white;
                break;
        }
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
        enemyNameText.text = Localization.GetEnemy(enemyName);
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

    // ===== 演出ヘルパー =====

    IEnumerator ShakeEffect(RectTransform target, float duration, float magnitude)
    {
        if (target == null) yield break;
        Vector2 originalPos = target.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = originalPos.x + Random.Range(-magnitude, magnitude);
            float y = originalPos.y + Random.Range(-magnitude, magnitude);
            target.anchoredPosition = new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.anchoredPosition = originalPos;
    }

    IEnumerator FlashEffect(Color color, float duration)
    {
        if (battleFlashOverlay == null) yield break;
        battleFlashOverlay.color = new Color(color.r, color.g, color.b, 0.6f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.6f, 0f, elapsed / duration);
            battleFlashOverlay.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        battleFlashOverlay.color = new Color(1f, 1f, 1f, 0f);
    }

    IEnumerator AttackAnimation(RectTransform attacker, RectTransform target, bool isSpecial)
    {
        if (attacker == null || target == null) yield break;

        // 突進アニメーション
        Vector2 originalPos = attacker.anchoredPosition;
        Vector2 direction = (target.anchoredPosition - attacker.anchoredPosition).normalized;
        Vector2 lungePos = originalPos + direction * 40f;

        // 前方に突進
        float lungeDuration = 0.1f;
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            attacker.anchoredPosition = Vector2.Lerp(originalPos, lungePos, elapsed / lungeDuration);
            yield return null;
        }

        // ヒットフラッシュ + シェイク
        Color flashColor = isSpecial ? new Color(1f, 0.8f, 0f) : Color.white;
        float shakeMagnitude = isSpecial ? 20f : 10f;
        float shakeDuration = isSpecial ? 0.4f : 0.25f;
        StartCoroutine(FlashEffect(flashColor, isSpecial ? 0.4f : 0.2f));
        StartCoroutine(ShakeEffect(target, shakeDuration, shakeMagnitude));

        // 元に戻る
        elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            attacker.anchoredPosition = Vector2.Lerp(lungePos, originalPos, elapsed / lungeDuration);
            yield return null;
        }
        attacker.anchoredPosition = originalPos;

        yield return new WaitForSeconds(shakeDuration);
    }

    IEnumerator DamageFlash(Image faceImage)
    {
        if (faceImage == null) yield break;
        Color originalColor = faceImage.color;
        for (int i = 0; i < 3; i++)
        {
            faceImage.color = new Color(1f, 0.3f, 0.3f, originalColor.a);
            yield return new WaitForSeconds(0.08f);
            faceImage.color = originalColor;
            yield return new WaitForSeconds(0.08f);
        }
    }

    // ===== バトルフロー =====

    IEnumerator BattleStart()
    {
        isBattleActive = true;
        battleLogText.text = Localization.Get("battle_enemy_appeared", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(3.0f);

        // 性別・サイズボーナス表示
        if (isMale)
        {
            battleLogText.text = Localization.Get("battle_boy_power", playerAtk);
        }
        else
        {
            battleLogText.text = Localization.Get("battle_girl_power", playerEvasion);
        }
        yield return new WaitForSeconds(3.0f);

        battleLogText.text = Localization.Get("battle_start");
        yield return new WaitForSeconds(2.0f);

        // 運動値 vs 敵の素早さで先攻判定
        int playerSpeed = DataCarrier.Instance != null ? DataCarrier.Instance.babyAthletic : 50;
        if (playerSpeed >= enemySpeed)
        {
            battleLogText.text = Localization.Get("battle_player_first");
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(PlayerTurn());
        }
        else
        {
            battleLogText.text = Localization.Get("battle_enemy_first", Localization.GetEnemy(enemyName));
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerTurn()
    {
        if (!isBattleActive) yield break;

        isPlayerTurn = true;
        playerDefending = false;
        battleLogText.text = Localization.Get("battle_your_turn");
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
        yield return new WaitForSeconds(0.5f);

        // 毒ダメージ処理
        if (enemyPoisonTurns > 0)
        {
            int poisonDmg = Mathf.Max(3, enemyMaxHp / 12);
            enemyHp = Mathf.Max(0, enemyHp - poisonDmg);
            enemyPoisonTurns--;
            UpdateEnemyDisplay();
            battleLogText.text = Localization.Get("battle_poison_damage", Localization.GetEnemy(enemyName), poisonDmg);
            yield return new WaitForSeconds(0.8f);
            if (enemyHp <= 0)
            {
                StartCoroutine(BattleWin());
                yield break;
            }
        }

        // デバフターン減衰
        if (enemyDefDebuffTurns > 0) enemyDefDebuffTurns--;
        if (enemyAtkDebuffTurns > 0) enemyAtkDebuffTurns--;
        if (playerEvasionBuffTurns > 0) playerEvasionBuffTurns--;

        battleLogText.text = Localization.Get("battle_enemy_attack", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(0.6f);

        // 敵の突進演出
        yield return StartCoroutine(AttackAnimation(enemyPanelRect, playerPanelRect, false));

        // 回避判定（バフ中は+20%）
        int effectiveEvasion = playerEvasion + (playerEvasionBuffTurns > 0 ? 20 : 0);
        bool evaded = Random.Range(0, 100) < effectiveEvasion;

        if (evaded)
        {
            battleLogText.text = Localization.Get("battle_evaded");
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(PlayerTurn());
            yield break;
        }

        // ATKデバフ中の敵は攻撃力低下
        int effectiveEnemyAtk = enemyAtkDebuffTurns > 0 ? (int)(enemyAtk * 0.6f) : enemyAtk;
        int damage = CalculateDamage(effectiveEnemyAtk, playerDef, playerDefending);
        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceImage));

        if (playerDefending)
        {
            battleLogText.text = Localization.Get("battle_defended", damage);
        }
        else
        {
            battleLogText.text = Localization.Get("battle_took_damage", damage);
        }

        yield return new WaitForSeconds(1.0f);

        // プレイヤー敗北チェック
        if (playerHp <= 0)
        {
            // 覇王色の特徴: 1/3の確率でHP300回復
            if (DataCarrier.Instance != null && DataCarrier.Instance.trait1 == "覇王色" && Random.Range(0, 3) == 0)
            {
                playerHp = 300;
                float hpRatio = Mathf.Clamp01((float)playerHp / playerMaxHp);
                playerHpBar.rectTransform.anchorMax = new Vector2(hpRatio, 1);
                battleLogText.text = Localization.Get("battle_conqueror_revive");
                yield return new WaitForSeconds(1.5f);
            }
            else
            {
                StartCoroutine(BattleLose());
                yield break;
            }
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

    void CloseSkillInfo()
    {
        if (skillInfoPopup != null) { Destroy(skillInfoPopup); skillInfoPopup = null; }
    }

    void OnAttack()
    {
        CloseSkillInfo();
        if (!waitingForAction) return;
        waitingForAction = false;
        battleTurnCount++;
        StartCoroutine(DoAttack());
    }

    void OnDefend()
    {
        CloseSkillInfo();
        if (!waitingForAction) return;
        waitingForAction = false;
        battleTurnCount++;
        StartCoroutine(DoDefend());
    }

    void OnMotherAttack()
    {
        CloseSkillInfo();
        if (!waitingForAction) return;
        waitingForAction = false;
        battleTurnCount++;
        StartCoroutine(DoMotherAttack());
    }

    void OnSpecial()
    {
        CloseSkillInfo();
        if (!waitingForAction) return;
        waitingForAction = false;
        battleTurnCount++;
        StartCoroutine(DoSpecial());
    }

    IEnumerator DoAttack()
    {
        // パンチ + 固有技を表示
        battleLogText.text = Localization.Get("battle_punch", normalAttackName);
        yield return new WaitForSeconds(0.3f);

        // カットイン
        yield return StartCoroutine(ShowSkillCutIn(false));

        // 突進 + ヒット演出
        yield return StartCoroutine(AttackAnimation(playerPanelRect, enemyPanelRect, false));

        int effectiveDef = enemyDefDebuffTurns > 0 ? (int)(enemyDef * 0.6f) : enemyDef;
        int damage = CalculateDamage(playerAtk, effectiveDef, false);
        enemyHp = Mathf.Max(0, enemyHp - damage);
        UpdateEnemyDisplay();
        StartCoroutine(DamageFlash(enemyFaceImage));

        battleLogText.text = Localization.Get("battle_attack_hit", normalAttackName, Localization.GetEnemy(enemyName), damage);
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
        battleLogText.text = Localization.Get("battle_defend_stance");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(EnemyTurn());
    }

    IEnumerator DoMotherAttack()
    {
        battleLogText.text = Localization.Get("battle_mother_skill", motherAttackName);
        yield return new WaitForSeconds(0.3f);

        // 母アイコンカットイン
        yield return StartCoroutine(ShowMotherSkillCutIn());

        // ダメージ計算（通常攻撃の70%威力）
        int baseDamage = CalculateDamage((int)(playerAtk * 0.7f), enemyDef, false);
        // デバフ中の敵はDEF低下
        int effectiveDef = enemyDefDebuffTurns > 0 ? (int)(enemyDef * 0.6f) : enemyDef;
        int damage = CalculateDamage((int)(playerAtk * 0.7f), effectiveDef, false);

        yield return StartCoroutine(AttackAnimation(playerPanelRect, enemyPanelRect, false));

        enemyHp = Mathf.Max(0, enemyHp - damage);
        UpdateEnemyDisplay();
        StartCoroutine(DamageFlash(enemyFaceImage));

        battleLogText.text = Localization.Get("battle_mother_damage", motherAttackName, damage);
        yield return new WaitForSeconds(0.8f);

        // 特殊効果
        switch (motherAttackEffect)
        {
            case "heal":
                int healAmt = Mathf.Max(5, playerMaxHp / 8);
                playerHp = Mathf.Min(playerMaxHp, playerHp + healAmt);
                UpdatePlayerDisplay();
                battleLogText.text = Localization.Get("battle_heal", healAmt);
                yield return new WaitForSeconds(0.8f);
                break;
            case "poison":
                enemyPoisonTurns = 3;
                battleLogText.text = Localization.Get("battle_poison", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(0.8f);
                break;
            case "evasion":
                playerEvasionBuffTurns = 2;
                battleLogText.text = Localization.Get("battle_evasion_up");
                yield return new WaitForSeconds(0.8f);
                break;
            case "defdown":
                enemyDefDebuffTurns = 2;
                battleLogText.text = Localization.Get("battle_def_down", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(0.8f);
                break;
            case "atkdown":
                enemyAtkDebuffTurns = 2;
                battleLogText.text = Localization.Get("battle_atk_down", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(0.8f);
                break;
            case "drain":
                int drainAmt = damage / 3;
                playerHp = Mathf.Min(playerMaxHp, playerHp + drainAmt);
                UpdatePlayerDisplay();
                battleLogText.text = Localization.Get("battle_drain", drainAmt);
                yield return new WaitForSeconds(0.8f);
                break;
        }

        if (enemyHp <= 0)
        {
            StartCoroutine(BattleWin());
            yield break;
        }

        StartCoroutine(EnemyTurn());
    }

    IEnumerator DoSpecial()
    {
        if (isGodBaby)
        {
            battleLogText.text = Localization.Get("battle_god_special", specialAttackName);
        }
        else
        {
            battleLogText.text = Localization.Get("battle_special", specialAttackName);
        }
        yield return new WaitForSeconds(0.4f);

        // カットイン（必殺技）
        yield return StartCoroutine(ShowSkillCutIn(true));

        bool hit = Random.Range(0, 100) < (isGodBaby ? 66 : 40);
        if (hit)
        {
            // 必殺技はスペシャル演出（大きいシェイク + 金色フラッシュ）
            yield return StartCoroutine(AttackAnimation(playerPanelRect, enemyPanelRect, true));

            float multiplier = isGodBaby ? 2.5f : 2.0f;
            int damage = (int)(playerAtk * multiplier) + Random.Range(5, 15);
            enemyHp = Mathf.Max(0, enemyHp - damage);
            UpdateEnemyDisplay();
            StartCoroutine(DamageFlash(enemyFaceImage));

            if (isGodBaby)
            {
                battleLogText.text = Localization.Get("battle_god_special_hit", specialAttackName, damage);
            }
            else
            {
                battleLogText.text = Localization.Get("battle_special_hit", specialAttackName, damage);
            }
        }
        else
        {
            battleLogText.text = Localization.Get("battle_special_miss", specialAttackName);
        }

        yield return new WaitForSeconds(1.2f);

        // 必殺技追加効果: 命中/外れに関わらず、闘志で全デバフ解除 + 一時的にATK上昇
        battleLogText.text = Localization.Get("battle_fighting_spirit");
        playerAtk = (int)(playerAtk * 1.15f);
        // 自分のデバフ解除
        playerEvasionBuffTurns = 0;
        yield return new WaitForSeconds(0.8f);

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
        battleLogText.text = Localization.Get("battle_enemy_defeated", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(1.5f);

        battleLogText.text = Localization.Get("battle_victory");
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

        // 経験値計算: 敵ステータスベース + ターン数ボーナス + 学力ボーナス
        int baseExp = (enemyMaxHp + enemyAtk * 3 + enemyDef * 2) / 4 + battleTurnCount * 3;
        int academic = DataCarrier.Instance.babyAcademic;
        float academicBonus = 1.0f + academic * 0.005f; // 学力100で+50%
        int expGained = Mathf.RoundToInt(baseExp * academicBonus);
        DataCarrier.Instance.babyExp += expGained;
        DataCarrier.Instance.defeatedEnemies++;

        int currentAge = DataCarrier.Instance.babyAge;
        int needed = DataCarrier.ExpForNextAge(currentAge);
        int currentExp = DataCarrier.Instance.babyExp;

        battleLogText.text = Localization.Get("battle_exp_gained", expGained);
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
            battleLogText.text = Localization.Get("battle_age_up", newAge);
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
                playerAtk = (int)(playerAtk * 1.12f);
                playerDef = (int)(playerDef * 1.12f);
                playerMaxHp = (int)(playerMaxHp * 1.12f);
                playerHp = playerMaxHp;
            }

            UpdatePlayerDisplay();
        }
        else
        {
            // レベルアップまで足りない — 経験値状況を表示
            int remaining = needed - currentExp;
            battleLogText.text = Localization.Get("battle_exp_remaining", remaining, currentExp, needed);
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
        titleText.text = Localization.Get("battle_growth_title");
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
            $"{Localization.Get("battle_stat_atk")}: {oldAtk} → <color=#00FF00>{newAtk}</color> <color=#FFFF00>(+{newAtk - oldAtk})</color>",
            $"{Localization.Get("battle_stat_def")}: {oldDef} → <color=#00FF00>{newDef}</color> <color=#FFFF00>(+{newDef - oldDef})</color>",
            $"{Localization.Get("battle_stat_hp_label")}: {oldHp} → <color=#00FF00>{newHp}</color> <color=#FFFF00>(+{newHp - oldHp})</color>",
            $"{Localization.Get("battle_stat_athletic")}: {oldAthletic} → <color=#00FF00>{newAthletic}</color> <color=#FFFF00>(+{newAthletic - oldAthletic})</color>"
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

        battleLogText.text = Localization.Get("battle_hp_full_heal");
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator BattleLose()
    {
        isBattleActive = false;
        battleLogText.text = Localization.Get("battle_defeat");
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
        resultText.text = Localization.Get("battle_game_over");
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
        retryTmp.text = string.Format(Localization.Get("ui_try_again"), DataCarrier.Instance.babyName);
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
            Localization.Get("boss_defeat_line1"),
            Localization.Get("boss_defeat_line2"),
            Localization.Get("boss_defeat_line3"),
            Localization.Get("boss_defeat_line4"),
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

        victoryText.text = Localization.Get("battle_saved_return", babyName, currentAge);
        yield return new WaitForSeconds(1.5f);

        victoryText.text = Localization.Get("battle_returning");
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

        CreateMenuButton(bar.transform, Localization.Get("battle_save_button"), OnSave);
        CreateMenuButton(bar.transform, Localization.Get("battle_top_button"), OnGoTop);
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
        battleLogText.text = Localization.Get("ui_saved");
        yield return new WaitForSeconds(1.2f);
        battleLogText.text = original;
    }

    void OnGoTop()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
