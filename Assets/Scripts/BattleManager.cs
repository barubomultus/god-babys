using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using UIE = UnityEngine.UIElements;

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
    bool enemyDefending;
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
    int playerPoisonTurns;     // プレイヤー毒残りターン
    int playerAtkDebuffTurns;  // プレイヤーATKデバフ残りターン
    bool isDevilEnemy;         // 悪魔村敵フラグ
    bool cannotRun;            // 逃走不可フラグ（固定エンカウント）
    Color enemyBgColor = Color.clear; // スプライト無し時の背景色

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
    TextMeshProUGUI playerAgeLabel;
    TextMeshProUGUI enemyAgeLabel;
    TextMeshProUGUI enemyHpText;
    TextMeshProUGUI battleLogText;
    GameObject vsTextObj;

    // アクションボタン
    GameObject actionPanel;
    Button attackButton;
    Button defendButton;
    Button specialButton;
    TextMeshProUGUI attackButtonText;
    TextMeshProUGUI specialButtonText;

    // UI Toolkit overlay
    UIE.PanelSettings overlayPanelSettings;
    UIE.VisualElement overlayRoot;
    UIE.VisualElement menuOverlayEl;

    // バトル状態
    bool isBattleActive;
    bool isPlayerTurn;
    bool waitingForAction;
    int battleTurnCount;
    int enemyTurnCount; // 敵ターンカウント（シバ必殺技用）

    // 演出用
    Image battleFlashOverlay;
    RectTransform playerPanelRect;
    RectTransform enemyPanelRect;

    void Start()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        // CanvasScaler調整: 縦向き9:16、幅基準
        var canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (canvasScaler != null)
        {
            canvasScaler.referenceResolution = new Vector2(1080, 1920);
            canvasScaler.matchWidthOrHeight = 0f;
        }

        InitializePlayer();
        InitializeEnemy();
        CreateBattleUI();

        // UI Toolkit overlay for menus & dialogs
        overlayPanelSettings = UIHelper.CreatePanelSettings(10f);
        var overlayObj = new GameObject("BattleOverlayUI");
        overlayObj.transform.SetParent(transform, false);
        overlayRoot = UIHelper.SetupUIDocument(overlayObj,
            new[] { "UI/CommonStyle", "UI/BattleStyle" }, overlayPanelSettings);
        overlayRoot.pickingMode = UIE.PickingMode.Ignore;

        CreateMenuBar();

        StartCoroutine(BattleStart());
    }

    void OnDestroy()
    {
        if (overlayPanelSettings != null)
            Destroy(overlayPanelSettings);
    }

    void InitializePlayer()
    {
        if (DataCarrier.Instance != null)
        {
            playerMaxHp = DataCarrier.Instance.babyHp;
            playerHp = playerMaxHp;
            // HP持越し: 前の戦闘のHPが残っていればそれを使う
            if (DataCarrier.Instance.babyCurrentHp > 0)
                playerHp = Mathf.Min(DataCarrier.Instance.babyCurrentHp, playerMaxHp);
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
                // HP持越し: GOD BABYボーナス適用後のmaxHPに対してクランプ
                if (DataCarrier.Instance.babyCurrentHp > 0)
                    playerHp = Mathf.Min(DataCarrier.Instance.babyCurrentHp, playerMaxHp);
                else
                    playerHp = playerMaxHp;
            }

            // 毒状態読込
            playerPoisonTurns = DataCarrier.Instance.babyPoisonTurns;

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
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        // 固定エンカウント（デヴィル傭兵）
        if (!string.IsNullOrEmpty(DataCarrier.Instance?.fixedEncounterEnemy))
        {
            InitializeFixedEnemy(DataCarrier.Instance.fixedEncounterEnemy);
            DataCarrier.Instance.fixedEncounterEnemy = "";
            return;
        }

        if (fromMap && bossBattle && area == 4)
        {
            // 109 — メロディアス女王
            enemyName = "メロディアス女王";
            enemyAge = -1;
            enemyMaxHp = 800;
            enemyHp = enemyMaxHp;
            enemyAtk = 100;
            enemyDef = 45;
            enemySpeed = 80;
            isDevilEnemy = false;
            enemyBgColor = new Color(0.4f, 0.15f, 0.3f);
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/boss/melodias");
        }
        else if (fromMap && bossBattle && area == 2)
        {
            // デヴィル夫人のやかた — デヴィル夫人
            enemyName = "デヴィル夫人";
            enemyAge = -1;
            enemyMaxHp = 650;
            enemyHp = enemyMaxHp;
            enemyAtk = 95;
            enemyDef = 40;
            enemySpeed = 75;
            isDevilEnemy = true;
            enemyBgColor = new Color(0.3f, 0.0f, 0.2f);
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/boss/devil-wife");
        }
        else if (fromMap && bossBattle)
        {
            // ボスの館 — 村の王シバ
            enemyName = "村の王シバ";
            enemyAge = -1; // ボスは月齢非表示
            enemyMaxHp = 500;
            enemyHp = enemyMaxHp;
            enemyAtk = 80;
            enemyDef = 45;
            enemySpeed = 70;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/boss/first-boss-shiba");
        }
        else if (fromMap)
        {
            // マップからのランダムエンカウント — 年齢に応じた敵
            int age = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 1;
            InitializeRandomEnemy(age);
        }
        else
        {
            // 初回ボス（ガチャ平均程度の強さ）
            enemyName = "あばれんぼうベイビー";
            enemyAge = -1; // 初回ボスは月齢非表示
            enemyMaxHp = 155;
            enemyHp = enemyMaxHp;
            enemyAtk = 50;
            enemyDef = 45;
            enemySpeed = 45;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/common-abarennbou");
        }
    }

    int enemyAge; // 敵の月齢（経験値計算用）

    void InitializeRandomEnemy(int playerAge)
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        if (area == 0)
        {
            // 村: 0~8ヶ月の敵（プレイヤー月齢付近を中心にランダム）
            int minAge = Mathf.Max(0, playerAge - 2);
            int maxAge = Mathf.Min(8, playerAge + 2);
            enemyAge = Random.Range(minAge, maxAge + 1);
        }
        else if (area == 3)
        {
            // 小悪魔の街: 14~24ヶ月
            int minAge = Mathf.Max(14, playerAge - 2);
            int maxAge = Mathf.Min(24, playerAge + 3);
            enemyAge = Random.Range(minAge, maxAge + 1);
        }
        else
        {
            // 悪魔村: 6~15ヶ月
            int minAge = Mathf.Max(6, playerAge - 2);
            int maxAge = Mathf.Min(15, playerAge + 3);
            enemyAge = Random.Range(minAge, maxAge + 1);
        }

        // 敵キャラ定義: 名前, スプライト, 基礎HP, 基礎ATK, 基礎DEF, 基礎SPD
        // 弱い順に並べ、月齢に応じて出現テーブルを変える
        object[][] enemyDefs;

        if (area == 3)
        {
            // 小悪魔の街専用テーブル
            enemyDefs = new object[][] {
                //                    名前              スプライト  HP  ATK DEF SPD 出現月齢  背景色
                new object[]{ "小悪魔ひとみ", Resources.Load<Sprite>("EnemyBabys/cute/hitomi"),  160, 45, 30, 40, 14, 17, new Color(0.35f, 0.1f, 0.4f) },
                new object[]{ "小悪魔あやか", Resources.Load<Sprite>("EnemyBabys/cute/ayaka"),  190, 52, 35, 45, 15, 18, new Color(0.3f, 0.05f, 0.35f) },
                new object[]{ "小悪魔りん",   Resources.Load<Sprite>("EnemyBabys/cute/rin"),    220, 60, 38, 50, 16, 19, new Color(0.2f, 0.05f, 0.3f) },
                new object[]{ "小悪魔みく",   Resources.Load<Sprite>("EnemyBabys/cute/miku"),   250, 68, 42, 55, 17, 20, new Color(0.45f, 0.05f, 0.15f) },
                new object[]{ "小悪魔なな",   Resources.Load<Sprite>("EnemyBabys/cute/nana"),   280, 75, 48, 60, 18, 22, new Color(0.25f, 0.0f, 0.05f) },
                new object[]{ "小悪魔れい",   Resources.Load<Sprite>("EnemyBabys/cute/rei"),    320, 85, 55, 65, 20, 24, new Color(0.15f, 0.0f, 0.2f) },
            };
        }
        else if (area == 1)
        {
            // 悪魔村専用テーブル
            enemyDefs = new object[][] {
                //                    名前              スプライト  HP  ATK DEF SPD 出現月齢  背景色
                new object[]{ "どくベイビー",     Resources.Load<Sprite>("EnemyBabys/poison/doku-baby"),  90, 25, 15, 25,  6,  9, new Color(0.4f, 0.1f, 0.5f) },
                new object[]{ "のろいベイビー",   Resources.Load<Sprite>("EnemyBabys/poison/noroi-baby"), 100, 30, 20, 22,  7, 10, new Color(0.3f, 0.0f, 0.3f) },
                new object[]{ "やみベイビー",     Resources.Load<Sprite>("EnemyBabys/poison/yami-baby"), 110, 35, 22, 30,  8, 11, new Color(0.15f, 0.05f, 0.2f) },
                new object[]{ "あくまベイビー",   Resources.Load<Sprite>("EnemyBabys/poison/akuma-baby"), 130, 42, 28, 35,  9, 13, new Color(0.5f, 0.0f, 0.1f) },
                new object[]{ "じゃあくベイビー", Resources.Load<Sprite>("EnemyBabys/poison/jyaaku-baby"), 150, 48, 32, 38, 11, 14, new Color(0.2f, 0.0f, 0.0f) },
                new object[]{ "まおうベイビー",   Resources.Load<Sprite>("EnemyBabys/poison/maou-baby"), 180, 55, 38, 42, 13, 15, new Color(0.1f, 0.0f, 0.15f) },
            };
        }
        else
        {
            enemyDefs = new object[][] {
                //                    名前                    スプライト                      HP  ATK DEF SPD  出現月齢
                new object[]{ "なきむしベイビー",     "EnemyBabys/common-nakimushi",    60, 15, 10, 20, 0, 3 },
                new object[]{ "やんちゃベイビー",     "EnemyBabys/common-yantya",       75, 22, 12, 30, 1, 5 },
                new object[]{ "いじわるベイビー",     "EnemyBabys/frist-enemy",         85, 28, 18, 25, 3, 6 },
                new object[]{ "わがままベイビー",     "EnemyBabys/common-wagamama",    100, 32, 22, 28, 4, 7 },
                new object[]{ "あばれんぼうベイビー", "EnemyBabys/common-abarennbou",  120, 40, 25, 35, 5, 8 },
            };
        }

        // 敵月齢に合う敵を候補に絞る
        var candidates = new System.Collections.Generic.List<object[]>();
        foreach (var def in enemyDefs)
        {
            int appearMin = (int)def[6];
            int appearMax = (int)def[7];
            if (enemyAge >= appearMin && enemyAge <= appearMax)
                candidates.Add(def);
        }
        if (candidates.Count == 0)
            candidates.Add(enemyDefs[enemyDefs.Length - 1]); // fallback

        // レアスポーン制限: まおうベイビー・小悪魔れいは3%の確率でのみ出現
        object[] chosen;
        int safetyCount = 0;
        do
        {
            chosen = candidates[Random.Range(0, candidates.Count)];
            safetyCount++;
            string cName = (string)chosen[0];
            if ((cName == "まおうベイビー" || cName == "小悪魔れい") && Random.value > 0.03f && candidates.Count > 1)
                continue;
            break;
        } while (safetyCount < 20);
        enemyName = (string)chosen[0];
        if (chosen[1] is Sprite spr)
            loadedEnemySprite = spr;
        else if (chosen[1] is string spritePath)
            loadedEnemySprite = Resources.Load<Sprite>(spritePath);
        else
            loadedEnemySprite = null;

        // 悪魔村敵: 背景色設定
        if (chosen.Length > 8 && chosen[8] is Color bgCol)
        {
            isDevilEnemy = true;
            enemyBgColor = bgCol;
        }

        // 月齢スケーリング: 1ヶ月ごとに+12%成長
        float scale = 1.0f + enemyAge * 0.12f;
        int baseHp  = (int)chosen[2];
        int baseAtk = (int)chosen[3];
        int baseDef = (int)chosen[4];
        int baseSpd = (int)chosen[5];

        enemyMaxHp = Mathf.RoundToInt(baseHp * scale + Random.Range(0, 20));
        enemyHp = enemyMaxHp;
        enemyAtk = Mathf.RoundToInt(baseAtk * scale + Random.Range(0, 5));
        enemyDef = Mathf.RoundToInt(baseDef * scale + Random.Range(0, 5));
        enemySpeed = Mathf.RoundToInt(baseSpd * scale + Random.Range(0, 10));
    }

    void InitializeFixedEnemy(string fixedName)
    {
        enemyName = fixedName;
        enemyAge = -1;
        cannotRun = true;
        isDevilEnemy = true;
        loadedEnemySprite = Resources.Load<Sprite>(
            fixedName == "デヴィル傭兵A" ? "EnemyBabys/poison/katchu-a" : "EnemyBabys/poison/katchu-b");

        // デヴィル傭兵A/B: 同じステータス
        enemyMaxHp = 280;
        enemyHp = enemyMaxHp;
        enemyAtk = 60;
        enemyDef = 35;
        enemySpeed = 55;
        enemyBgColor = new Color(0.35f, 0.05f, 0.05f);
    }

    // ===== バトルUI作成 =====

    void CreateBattleUI()
    {
        var (safeLeft, safeRight, safeTop, safeBottom) = SafeAreaHelper.GetSafeAreaInsets(canvas);

        // バトルパネル（画面全体を使う）
        battlePanel = new GameObject("BattlePanel");
        battlePanel.transform.SetParent(canvas.transform, false);

        var panelRect = battlePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 背景色（暗いグラデーション風）
        var bgImg = battlePanel.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.15f);
        bgImg.raycastTarget = false;

        // 敵側（右上）
        CreateCharacterPanel(battlePanel.transform, 220, 620 + safeTop * 0.5f, false, out enemyFaceImage, out enemyNameText, out enemyHpBar, out enemyHpText, out enemyAgeLabel);
        enemyPanelRect = enemyFaceImage.transform.parent.GetComponent<RectTransform>();

        // VS テキスト
        CreateVsText(battlePanel.transform);

        // プレイヤー側（左下）
        CreateCharacterPanel(battlePanel.transform, -220, -280, true, out playerFaceImage, out playerNameText, out playerHpBar, out playerHpText, out playerAgeLabel);
        playerPanelRect = playerFaceImage.transform.parent.GetComponent<RectTransform>();

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

    void CreateCharacterPanel(Transform parent, float xPos, float yPos, bool isPlayer, out Image faceImage, out TextMeshProUGUI nameText, out Image hpBar, out TextMeshProUGUI hpText, out TextMeshProUGUI ageLabel)
    {
        var panel = new GameObject(isPlayer ? "PlayerPanel" : "EnemyPanel");
        panel.transform.SetParent(parent, false);

        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(xPos, yPos);
        panelRect.sizeDelta = new Vector2(450, 650);

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
        nameRect.anchoredPosition = new Vector2(0, -40);
        nameRect.sizeDelta = new Vector2(0, 80);
        nameText = nameObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(nameText);
        nameText.fontSize = 46;
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 28;
        nameText.fontSizeMax = 46;
        nameText.alignment = TextAlignmentOptions.Center;
        nameText.color = isPlayer ? new Color(0.5f, 0.8f, 1f) : new Color(1f, 0.5f, 0.5f);
        nameText.fontStyle = FontStyles.Bold;
        nameText.raycastTarget = false;
        nameText.richText = true; // GOD BABYの金色表示用

        // 顔マスク（border-radius 32px）
        var faceMaskObj = new GameObject("FaceMask");
        faceMaskObj.transform.SetParent(panel.transform, false);
        var faceMaskRect = faceMaskObj.AddComponent<RectTransform>();
        faceMaskRect.anchorMin = new Vector2(0, 0);
        faceMaskRect.anchorMax = new Vector2(1, 1);
        faceMaskRect.offsetMin = new Vector2(15, 68);
        faceMaskRect.offsetMax = new Vector2(-15, -65);
        var faceMaskImg = faceMaskObj.AddComponent<Image>();
        faceMaskImg.sprite = CreateRoundedRectSprite(64, 64, 32);
        faceMaskImg.type = Image.Type.Sliced;
        faceMaskImg.color = Color.white;
        faceMaskImg.raycastTarget = false;
        var faceMask = faceMaskObj.AddComponent<Mask>();
        faceMask.showMaskGraphic = false;

        // 顔画像（マスク内）
        var faceObj = new GameObject("Face");
        faceObj.transform.SetParent(faceMaskObj.transform, false);
        var faceRect = faceObj.AddComponent<RectTransform>();
        faceRect.anchorMin = Vector2.zero;
        faceRect.anchorMax = Vector2.one;
        faceRect.offsetMin = Vector2.zero;
        faceRect.offsetMax = Vector2.zero;
        faceImage = faceObj.AddComponent<Image>();
        faceImage.color = Color.white;
        faceImage.preserveAspect = true;
        faceImage.raycastTarget = false;

        // 年齢ラベル（顔画像の右下）
        var ageLabelObj = new GameObject("AgeLabel");
        ageLabelObj.transform.SetParent(faceObj.transform, false);
        var ageLabelRect = ageLabelObj.AddComponent<RectTransform>();
        ageLabelRect.anchorMin = new Vector2(1, 0);
        ageLabelRect.anchorMax = new Vector2(1, 0);
        ageLabelRect.pivot = new Vector2(1, 0);
        ageLabelRect.anchoredPosition = new Vector2(-4, 4);
        ageLabelRect.sizeDelta = new Vector2(200, 30);
        ageLabel = ageLabelObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(ageLabel);
        ageLabel.fontSize = 22;
        ageLabel.alignment = TextAlignmentOptions.BottomRight;
        ageLabel.color = new Color(1f, 1f, 1f, 0.85f);
        ageLabel.raycastTarget = false;

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

        // HPテキスト（HPバーの上）
        var hpTextObj = new GameObject("HpText");
        hpTextObj.transform.SetParent(panel.transform, false);
        var hpTextRect = hpTextObj.AddComponent<RectTransform>();
        hpTextRect.anchorMin = new Vector2(0, 0);
        hpTextRect.anchorMax = new Vector2(1, 0);
        hpTextRect.anchoredPosition = new Vector2(0, 78);
        hpTextRect.sizeDelta = new Vector2(-30, 24);
        hpText = hpTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(hpText);
        hpText.fontSize = 22;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = Color.white;
        hpText.richText = true;
        hpText.raycastTarget = false;
    }

    void CreateVsText(Transform parent)
    {
        vsTextObj = new GameObject("VsText");
        var vsObj = vsTextObj;
        vsObj.transform.SetParent(parent, false);
        var vsRect = vsObj.AddComponent<RectTransform>();
        vsRect.anchorMin = new Vector2(0.5f, 0.5f);
        vsRect.anchorMax = new Vector2(0.5f, 0.5f);
        vsRect.anchoredPosition = new Vector2(0, 180);
        vsRect.sizeDelta = new Vector2(200, 100);
        var vsText = vsObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(vsText);
        vsText.text = "VS";
        vsText.fontSize = 80;
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
        logRect.anchoredPosition = new Vector2(0, 180);
        logRect.sizeDelta = new Vector2(1000, 200);

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
        FontHelper.Apply(battleLogText);
        battleLogText.fontSize = 40;
        battleLogText.alignment = TextAlignmentOptions.Center;
        battleLogText.color = Color.white;
        battleLogText.raycastTarget = false;
        battleLogText.text = "";
    }

    void CreateActionButtons()
    {
        var (safeLeft, safeRight, safeTop, safeBottom) = SafeAreaHelper.GetSafeAreaInsets(canvas);

        actionPanel = new GameObject("ActionPanel");
        actionPanel.transform.SetParent(canvas.transform, false);

        var panelRect = actionPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 320 + safeBottom);
        panelRect.sizeDelta = new Vector2(1020, 560);

        float btnHeight = 120f;
        float btnSpacing = 32f;
        float runBtnHeight = 80f;
        float bottomPad = 20f;

        // 中段（2ボタン: 防御, 必殺技）
        float midY = bottomPad + btnHeight * 0.5f;
        float btnGap = 20f;
        float halfW = 460f;

        defendButton = CreateActionButton(actionPanel.transform, Localization.Get("battle_defend"), Localization.Get("battle_defend_name"), defendDesc, Color.white, OnDefend, out _);
        var defendRect = defendButton.GetComponent<RectTransform>();
        defendRect.anchorMin = new Vector2(0.5f, 0);
        defendRect.anchorMax = new Vector2(0.5f, 0);
        defendRect.anchoredPosition = new Vector2(-halfW * 0.5f - btnGap, midY);
        defendRect.sizeDelta = new Vector2(halfW, btnHeight);

        specialButton = CreateActionButton(actionPanel.transform, Localization.Get("battle_special_skill"), specialAttackName, specialAttackDesc, Color.white, OnSpecial, out specialButtonText);
        var specialRect = specialButton.GetComponent<RectTransform>();
        specialRect.anchorMin = new Vector2(0.5f, 0);
        specialRect.anchorMax = new Vector2(0.5f, 0);
        specialRect.anchoredPosition = new Vector2(halfW * 0.5f + btnGap, midY);
        specialRect.sizeDelta = new Vector2(halfW, btnHeight);

        // 上段（2ボタン: 攻撃, 母スキル）
        float topY = midY + btnHeight * 0.5f + btnSpacing + btnHeight * 0.5f;

        attackButton = CreateActionButton(actionPanel.transform, Localization.Get("battle_normal_attack"), normalAttackName, normalAttackDesc, Color.white, OnAttack, out attackButtonText);
        var attackRect = attackButton.GetComponent<RectTransform>();
        attackRect.anchorMin = new Vector2(0.5f, 0);
        attackRect.anchorMax = new Vector2(0.5f, 0);
        attackRect.anchoredPosition = new Vector2(-halfW * 0.5f - btnGap, topY);
        attackRect.sizeDelta = new Vector2(halfW, btnHeight);

        var motherBtn = CreateActionButton(actionPanel.transform, Localization.Get("battle_special_attack"), motherAttackName, motherAttackDesc, Color.white, OnMotherAttack, out _);
        var motherRect = motherBtn.GetComponent<RectTransform>();
        motherRect.anchorMin = new Vector2(0.5f, 0);
        motherRect.anchorMax = new Vector2(0.5f, 0);
        motherRect.anchoredPosition = new Vector2(halfW * 0.5f + btnGap, topY);
        motherRect.sizeDelta = new Vector2(halfW, btnHeight);

        actionPanel.SetActive(false);
    }

    Button CreateActionButton(Transform parent, string categoryLabel, string skillName, string description, Color bgColor, UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI buttonText)
    {
        var btnObj = new GameObject(skillName + "Button");
        btnObj.transform.SetParent(parent, false);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.sprite = CreateRoundedRectSprite(64, 64, 16);
        btnImg.type = Image.Type.Sliced;
        btnImg.color = Color.white;

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(onClick);
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        // 枠線（ボタンの後ろにOutline）
        var outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(btnObj.transform, false);
        outlineObj.transform.SetAsFirstSibling();
        var outlineRect = outlineObj.AddComponent<RectTransform>();
        outlineRect.anchorMin = Vector2.zero;
        outlineRect.anchorMax = Vector2.one;
        outlineRect.offsetMin = new Vector2(-2, -2);
        outlineRect.offsetMax = new Vector2(2, 2);
        var outlineImg = outlineObj.AddComponent<Image>();
        outlineImg.sprite = CreateRoundedRectSprite(64, 64, 16);
        outlineImg.type = Image.Type.Sliced;
        outlineImg.color = new Color(0.75f, 0.75f, 0.8f);
        outlineImg.raycastTarget = false;

        // カテゴリラベル（ボタン外側の上）
        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        var labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.anchoredPosition = new Vector2(0, 2);
        labelRect.sizeDelta = new Vector2(0, 24);
        var labelTmp = labelObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(labelTmp);
        labelTmp.text = categoryLabel;
        labelTmp.fontSize = 20;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = new Color(0.8f, 0.8f, 0.85f);
        labelTmp.raycastTarget = false;

        // 技名（ボタン中央）
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8, 4);
        textRect.offsetMax = new Vector2(-8, -4);
        buttonText = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(buttonText);
        buttonText.text = skillName;
        buttonText.fontSize = 36;
        buttonText.enableAutoSizing = true;
        buttonText.fontSizeMin = 20;
        buttonText.fontSizeMax = 36;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = new Color(0.45f, 0.45f, 0.5f);
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.raycastTarget = false;

        // infoボタン（右上）
        var infoObj = new GameObject("InfoButton");
        infoObj.transform.SetParent(btnObj.transform, false);
        var infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(1, 1);
        infoRect.anchorMax = new Vector2(1, 1);
        infoRect.pivot = new Vector2(1, 1);
        infoRect.anchoredPosition = new Vector2(-4, -4);
        infoRect.sizeDelta = new Vector2(40, 40);

        var infoBg = infoObj.AddComponent<Image>();
        infoBg.sprite = CreateRoundedRectSprite(32, 32, 16);
        infoBg.type = Image.Type.Sliced;
        infoBg.color = new Color(0.85f, 0.85f, 0.9f);

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
        FontHelper.Apply(infoTmp);
        infoTmp.text = "i";
        infoTmp.fontSize = 28;
        infoTmp.alignment = TextAlignmentOptions.Center;
        infoTmp.color = new Color(0.5f, 0.5f, 0.55f);
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
        popupRect.sizeDelta = new Vector2(500, 280);

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
        FontHelper.Apply(titleText);
        titleText.text = $"<color=#AAAAAA><size=70%>{category}</size></color>  <color=#FFDD44>{skillName}</color>";
        titleText.fontSize = 44;
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
        FontHelper.Apply(descText);
        descText.text = description;
        descText.fontSize = 36;
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

        // 名前を表示（GOD BABYは金色）
        if (isGodBaby)
        {
            playerNameText.text = $"<color=#FFD700>{babyName}</color>";
        }
        else
        {
            playerNameText.text = babyName;
        }
        // 年齢を顔画像の右下に表示
        if (playerAgeLabel != null)
            playerAgeLabel.text = Localization.GetAge(babyAge);
        playerHpText.text = $"HP:{playerHp}/{playerMaxHp}";

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

        // カスタム画像があればそちらを優先
        Sprite customSprite = DataCarrier.LoadCustomBabySprite();
        if (customSprite != null)
        {
            foreach (Transform child in targetImage.transform)
                Destroy(child.gameObject);
            targetImage.enabled = true;
            targetImage.sprite = customSprite;
            targetImage.color = Color.white;
            targetImage.preserveAspect = true;

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

        float s = 1.0f; // スケール（パネル幅に合わせて大きく）

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
        FontHelper.Apply(yenText);
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
        if (enemyAgeLabel != null)
            enemyAgeLabel.text = enemyAge >= 0 ? Localization.GetAge(enemyAge) : "";
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
        else if (enemyBgColor != Color.clear)
        {
            // 悪魔村敵: スプライト無し、背景色で表示
            enemyFaceImage.enabled = true;
            enemyFaceImage.sprite = null;
            enemyFaceImage.color = enemyBgColor;
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
        Vector2 lungePos = originalPos + direction * 100f;

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

        // バトルスタート画像を表示
        battleLogText.text = "";
        var startImgObj = new GameObject("BattleStartImage");
        startImgObj.transform.SetParent(canvas.transform, false);
        var startImgRect = startImgObj.AddComponent<RectTransform>();
        startImgRect.anchorMin = new Vector2(0.5f, 0.5f);
        startImgRect.anchorMax = new Vector2(0.5f, 0.5f);
        startImgRect.anchoredPosition = Vector2.zero;
        startImgRect.sizeDelta = new Vector2(800, 400);
        var startImg = startImgObj.AddComponent<Image>();
        var startSpr = Resources.Load<Sprite>("UI/battle-start");
        if (startSpr != null)
        {
            startImg.sprite = startSpr;
            startImg.preserveAspect = true;
            startImg.color = Color.white;
        }
        startImg.raycastTarget = false;
        if (vsTextObj != null) vsTextObj.SetActive(false);
        yield return new WaitForSeconds(2.0f);
        Destroy(startImgObj);

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

        // プレイヤー毒ダメージ処理
        if (playerPoisonTurns > 0)
        {
            int poisonDmg = Mathf.Max(3, playerMaxHp / 12);
            playerHp = Mathf.Max(0, playerHp - poisonDmg);
            playerPoisonTurns--;
            UpdatePlayerDisplay();
            battleLogText.text = Localization.Get("battle_player_poison_damage", poisonDmg);
            yield return new WaitForSeconds(0.8f);
            if (playerHp <= 0)
            {
                StartCoroutine(BattleLose());
                yield break;
            }
        }

        // シバ必殺技の予告（次の敵ターンが3の倍数の時）
        bool isBoss = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBoss && enemyName == "村の王シバ" && (enemyTurnCount + 1) % 3 == 0 && enemyTurnCount > 0)
        {
            battleLogText.text = Localization.Get("battle_shiba_charging");
            yield return new WaitForSeconds(1.5f);
        }
        // デヴィル夫人: 4ターンごとのチャージ警告
        if (isBoss && enemyName == "デヴィル夫人" && (enemyTurnCount + 1) % 4 == 0 && enemyTurnCount > 0)
        {
            battleLogText.text = Localization.Get("battle_devil_lady_charge");
            yield return new WaitForSeconds(1.5f);
        }
        // メロディアス女王: 5ターンごとのチャージ警告
        if (isBoss && enemyName == "メロディアス女王" && (enemyTurnCount + 1) % 5 == 0 && enemyTurnCount > 0)
        {
            battleLogText.text = Localization.Get("battle_melodias_charge");
            yield return new WaitForSeconds(1.5f);
        }

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
        enemyDefending = false;
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
        if (playerAtkDebuffTurns > 0) playerAtkDebuffTurns--;

        enemyTurnCount++;

        // シバ: 3ターン毎に必殺技（回避不可・超ダメージ）
        bool isBoss = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBoss && enemyName == "村の王シバ" && enemyTurnCount % 3 == 0)
        {
            yield return StartCoroutine(EnemyDoUltimate());
            yield break;
        }
        // デヴィル夫人: 4ターン毎に必殺技「毒の洗礼」
        if (isBoss && enemyName == "デヴィル夫人" && enemyTurnCount % 4 == 0)
        {
            yield return StartCoroutine(EnemyDoDevilLadyUltimate());
            yield break;
        }
        // メロディアス女王: 5ターン毎に必殺技「魅惑のメロディ」
        if (isBoss && enemyName == "メロディアス女王" && enemyTurnCount % 5 == 0)
        {
            yield return StartCoroutine(EnemyDoMelodiasUltimate());
            yield break;
        }

        // 敵の行動選択: 通常攻撃60%, 特殊攻撃25%, 防御15%
        // HPが30%以下なら防御確率UP (35%)
        float hpPercent = (float)enemyHp / enemyMaxHp;
        int roll = Random.Range(0, 100);
        int defendChance = hpPercent < 0.3f ? 35 : 15;
        int specialChance = 25;

        if (roll < defendChance)
        {
            // === 防御（HP少し回復） ===
            yield return StartCoroutine(EnemyDoDefend());
        }
        else if (roll < defendChance + specialChance)
        {
            // === 特殊攻撃（1.5倍ダメージ） ===
            yield return StartCoroutine(EnemyDoSpecialAttack());
        }
        else
        {
            // === 通常攻撃 ===
            yield return StartCoroutine(EnemyDoNormalAttack());
        }
    }

    IEnumerator EnemyDoNormalAttack()
    {
        battleLogText.text = Localization.Get("battle_enemy_attack", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(AttackAnimation(enemyPanelRect, playerPanelRect, false));

        // 回避判定
        int effectiveEvasion = playerEvasion + (playerEvasionBuffTurns > 0 ? 20 : 0);
        if (Random.Range(0, 100) < effectiveEvasion)
        {
            battleLogText.text = Localization.Get("battle_evaded");
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(PlayerTurn());
            yield break;
        }

        int effectiveEnemyAtk = enemyAtkDebuffTurns > 0 ? (int)(enemyAtk * 0.6f) : enemyAtk;
        int damage = CalculateDamage(effectiveEnemyAtk, playerDef, playerDefending);
        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceImage));

        battleLogText.text = playerDefending
            ? Localization.Get("battle_defended", damage)
            : Localization.Get("battle_took_damage", damage);
        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator EnemyDoSpecialAttack()
    {
        battleLogText.text = Localization.Get("battle_enemy_special", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(AttackAnimation(enemyPanelRect, playerPanelRect, true));

        // 回避判定（特殊攻撃は回避しにくい: -10%）
        int effectiveEvasion = Mathf.Max(0, playerEvasion + (playerEvasionBuffTurns > 0 ? 20 : 0) - 10);
        if (Random.Range(0, 100) < effectiveEvasion)
        {
            battleLogText.text = Localization.Get("battle_evaded");
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(PlayerTurn());
            yield break;
        }

        // 1.5倍ダメージ
        int effectiveEnemyAtk = enemyAtkDebuffTurns > 0 ? (int)(enemyAtk * 0.6f) : enemyAtk;
        int specialAtk = (int)(effectiveEnemyAtk * 1.5f);
        int damage = CalculateDamage(specialAtk, playerDef, playerDefending);
        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceImage));

        battleLogText.text = Localization.Get("battle_enemy_special_hit", Localization.GetEnemy(enemyName), damage);
        yield return new WaitForSeconds(1.0f);

        // 悪魔村敵・デヴィル系: 毒付与
        if (isDevilEnemy && playerHp > 0)
        {
            int poisonDuration = (enemyName == "デヴィル夫人") ? 4 : 3;
            if (playerPoisonTurns <= 0)
            {
                playerPoisonTurns = poisonDuration;
                battleLogText.text = Localization.Get("battle_player_poisoned");
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                battleLogText.text = Localization.Get("battle_player_poison_already");
                yield return new WaitForSeconds(0.8f);
            }
        }

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator EnemyDoDefend()
    {
        enemyDefending = true;
        int healAmt = Mathf.Max(3, enemyMaxHp / 10);
        enemyHp = Mathf.Min(enemyMaxHp, enemyHp + healAmt);
        UpdateEnemyDisplay();

        battleLogText.text = Localization.Get("battle_enemy_defend_heal", Localization.GetEnemy(enemyName), healAmt);
        yield return new WaitForSeconds(1.2f);

        StartCoroutine(PlayerTurn());
    }

    IEnumerator EnemyDoUltimate()
    {
        // 予告演出
        battleLogText.text = Localization.Get("battle_shiba_ultimate_announce");
        yield return new WaitForSeconds(1.0f);

        // 画面フラッシュ（赤）
        StartCoroutine(FlashEffect(new Color(1f, 0.2f, 0.1f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogText.text = Localization.Get("battle_shiba_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        // 必殺技演出（大振りシェイク）
        yield return StartCoroutine(AttackAnimation(enemyPanelRect, playerPanelRect, true));
        StartCoroutine(FlashEffect(new Color(0.8f, 0f, 0f), 0.6f));
        StartCoroutine(ShakeEffect(playerPanelRect, 0.5f, 25f));

        // ダメージ計算: 固定威力200 — 防御+育成で生存可能、無防備なら致命的
        // 防御時: 200 - DEF（月齢8~10で耐えられる想定）
        // 非防御時: 200 - DEF/2（ほぼ即死）
        int ultimatePower = 200;
        int damage = CalculateDamage(ultimatePower, playerDef, playerDefending);

        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceImage));

        if (playerDefending)
        {
            battleLogText.text = Localization.Get("battle_shiba_ultimate_blocked", damage);
        }
        else
        {
            battleLogText.text = Localization.Get("battle_shiba_ultimate_hit", damage);
        }
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator EnemyDoDevilLadyUltimate()
    {
        // 予告演出
        battleLogText.text = Localization.Get("battle_devil_lady_ultimate");
        yield return new WaitForSeconds(1.0f);

        // 画面フラッシュ（紫）
        StartCoroutine(FlashEffect(new Color(0.6f, 0.0f, 0.8f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogText.text = Localization.Get("battle_devil_lady_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        // 必殺技演出
        yield return StartCoroutine(AttackAnimation(enemyPanelRect, playerPanelRect, true));
        StartCoroutine(FlashEffect(new Color(0.5f, 0f, 0.6f), 0.6f));
        StartCoroutine(ShakeEffect(playerPanelRect, 0.5f, 25f));

        // 固定ダメージ200 + 防御で半減可能
        int ultimatePower = 200;
        int damage = CalculateDamage(ultimatePower, playerDef, playerDefending);

        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceImage));

        if (playerDefending)
        {
            battleLogText.text = Localization.Get("battle_devil_lady_ultimate_blocked", damage);
        }
        else
        {
            battleLogText.text = Localization.Get("battle_devil_lady_ultimate_hit", damage);
        }
        yield return new WaitForSeconds(1.0f);

        // 5ターン毒付与（上書き）
        if (playerHp > 0)
        {
            playerPoisonTurns = 5;
            battleLogText.text = Localization.Get("battle_devil_lady_poisoned");
            yield return new WaitForSeconds(1.0f);
        }

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator EnemyDoMelodiasUltimate()
    {
        // 予告演出
        battleLogText.text = Localization.Get("battle_melodias_ultimate");
        yield return new WaitForSeconds(1.0f);

        // 画面フラッシュ（ピンク/ゴールド）
        StartCoroutine(FlashEffect(new Color(1.0f, 0.4f, 0.7f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogText.text = Localization.Get("battle_melodias_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        // 必殺技演出
        yield return StartCoroutine(AttackAnimation(enemyPanelRect, playerPanelRect, true));
        StartCoroutine(FlashEffect(new Color(1.0f, 0.84f, 0.0f), 0.6f));
        StartCoroutine(ShakeEffect(playerPanelRect, 0.5f, 25f));

        // 固定ダメージ180 + 防御で半減可能
        int ultimatePower = 180;
        int damage = CalculateDamage(ultimatePower, playerDef, playerDefending);

        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceImage));

        if (playerDefending)
        {
            battleLogText.text = Localization.Get("battle_melodias_ultimate_blocked", damage);
        }
        else
        {
            battleLogText.text = Localization.Get("battle_melodias_ultimate_hit", damage);
        }
        yield return new WaitForSeconds(1.0f);

        // ATKデバフ3ターン付与
        if (playerHp > 0)
        {
            playerAtkDebuffTurns = 3;
            battleLogText.text = Localization.Get("battle_melodias_debuffed");
            yield return new WaitForSeconds(1.0f);
        }

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator CheckPlayerDefeatAndContinue()
    {
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

    void OnRun()
    {
        CloseSkillInfo();
        if (!waitingForAction) return;
        waitingForAction = false;
        StartCoroutine(DoRun());
    }

    IEnumerator DoRun()
    {
        // ボス戦・固定エンカウントでは逃げられない
        bool isBoss = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBoss || cannotRun)
        {
            actionPanel.SetActive(false);
            battleLogText.text = cannotRun && !isBoss
                ? Localization.Get("battle_run_fixed")
                : Localization.Get("battle_run_boss");
            yield return new WaitForSeconds(1.5f);
            // ターン消費なし — 再度行動選択
            actionPanel.SetActive(true);
            waitingForAction = true;
            yield break;
        }

        actionPanel.SetActive(false);

        // 50%の確率で成功
        if (Random.Range(0f, 1f) < 0.5f)
        {
            battleLogText.text = Localization.Get("battle_run_success");
            yield return new WaitForSeconds(1.2f);

            // HP・毒保存して村へ（セーブはしない）
            if (DataCarrier.Instance != null)
            {
                DataCarrier.Instance.babyCurrentHp = playerHp;
                DataCarrier.Instance.babyPoisonTurns = playerPoisonTurns;
                DataCarrier.Instance.cameFromMap = false;
                DataCarrier.Instance.isBossBattle = false;
            }
            SceneManager.LoadScene("MapScene");
        }
        else
        {
            battleLogText.text = Localization.Get("battle_run_fail");
            yield return new WaitForSeconds(1.2f);
            battleTurnCount++;

            // 敵ターンへ移行
            StartCoroutine(EnemyTurn());
        }
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
        int effectivePlayerAtk = playerAtkDebuffTurns > 0 ? (int)(playerAtk * 0.6f) : playerAtk;
        int damage = CalculateDamage(effectivePlayerAtk, effectiveDef, enemyDefending);
        enemyDefending = false;
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
        int effectiveDef = enemyDefDebuffTurns > 0 ? (int)(enemyDef * 0.6f) : enemyDef;
        int effectivePlayerAtk = playerAtkDebuffTurns > 0 ? (int)(playerAtk * 0.6f) : playerAtk;
        int damage = CalculateDamage((int)(effectivePlayerAtk * 0.7f), effectiveDef, enemyDefending);
        enemyDefending = false;

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
            int effectivePlayerAtk = playerAtkDebuffTurns > 0 ? (int)(playerAtk * 0.6f) : playerAtk;
            int damage = (int)(effectivePlayerAtk * multiplier) + Random.Range(5, 15);
            if (enemyDefending) damage = damage / 2;
            enemyDefending = false;
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
        playerAtkDebuffTurns = 0;
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

        // 縁の書: 初めて倒した敵なら通知
        if (DataCarrier.Instance != null)
        {
            bool isFirstDefeat = DataCarrier.Instance.AddDefeatedEnemy(enemyName);
            if (isFirstDefeat)
            {
                battleLogText.text = Localization.Get("enishi_added", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(2f);
            }
        }

        // 勝利画像を表示
        battleLogText.text = "";
        var victoryImgObj = new GameObject("VictoryImage");
        victoryImgObj.transform.SetParent(canvas.transform, false);
        var victoryImgRect = victoryImgObj.AddComponent<RectTransform>();
        victoryImgRect.anchorMin = new Vector2(0.5f, 0.5f);
        victoryImgRect.anchorMax = new Vector2(0.5f, 0.5f);
        victoryImgRect.anchoredPosition = Vector2.zero;
        victoryImgRect.sizeDelta = new Vector2(800, 400);
        var victoryImg = victoryImgObj.AddComponent<Image>();
        var victorySpr = Resources.Load<Sprite>("UI/victory");
        if (victorySpr != null)
        {
            victoryImg.sprite = victorySpr;
            victoryImg.preserveAspect = true;
            victoryImg.color = Color.white;
        }
        victoryImg.raycastTarget = false;
        yield return new WaitForSeconds(1.5f);
        Destroy(victoryImgObj);

        // 経験値獲得と年齢アップ演出
        yield return StartCoroutine(GainExpSequence());

        // HP・毒持越し保存
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyCurrentHp = playerHp;
            DataCarrier.Instance.babyPoisonTurns = playerPoisonTurns;
        }

        // ボス戦・固定エンカウントは自動セーブ
        bool isBossWin = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        bool isFixedWin = cannotRun; // 固定エンカウント勝利
        if ((isBossWin || isFixedWin) && DataCarrier.Instance != null)
        {
            DataCarrier.Instance.SaveData();
        }

        // バトルUIを非表示
        if (battlePanel != null) battlePanel.SetActive(false);
        if (actionPanel != null) actionPanel.SetActive(false);
        battleLogText.transform.parent.gameObject.SetActive(false);

        // 固定エンカウント勝利（傭兵）→ 館に戻る
        if (isFixedWin && !isBossWin)
        {
            StartCoroutine(FixedEncounterVictory());
        }
        // ボス戦なら特別演出、通常なら村へ帰還
        else if (isBossWin)
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
            playerHp = playerMaxHp; // レベルアップで全回復
            DataCarrier.Instance.babyCurrentHp = -1;
            playerPoisonTurns = 0; // レベルアップで毒治療
            DataCarrier.Instance.babyPoisonTurns = 0;
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
            // レベルアップまで足りない — 経験値状況を表示 + プログレスバー
            int remaining = needed - currentExp;
            battleLogText.text = Localization.Get("battle_exp_remaining", remaining, currentExp, needed);

            // プログレスバーを作成（battleLogTextの親パネル内）
            var logPanel = battleLogText.transform.parent;

            // バー背景
            var barBgObj = new GameObject("ExpBarBg");
            barBgObj.transform.SetParent(logPanel, false);
            var barBgRect = barBgObj.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.1f, 0f);
            barBgRect.anchorMax = new Vector2(0.9f, 0f);
            barBgRect.anchoredPosition = new Vector2(0, 25);
            barBgRect.sizeDelta = new Vector2(0, 20);
            var barBgImg = barBgObj.AddComponent<Image>();
            barBgImg.sprite = CreateRoundedRectSprite(32, 32, 6);
            barBgImg.type = Image.Type.Sliced;
            barBgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            barBgImg.raycastTarget = false;

            // バー本体
            var barFillObj = new GameObject("ExpBarFill");
            barFillObj.transform.SetParent(barBgObj.transform, false);
            var barFillRect = barFillObj.AddComponent<RectTransform>();
            barFillRect.anchorMin = new Vector2(0, 0);
            barFillRect.anchorMax = new Vector2(0, 1); // 0から始めてアニメーション
            barFillRect.offsetMin = new Vector2(2, 2);
            barFillRect.offsetMax = new Vector2(-2, -2);
            barFillRect.pivot = new Vector2(0, 0.5f);
            var barFillImg = barFillObj.AddComponent<Image>();
            barFillImg.color = new Color(1.0f, 0.85f, 0.0f); // 金色
            barFillImg.raycastTarget = false;

            // アニメーション: 0% → 現在の割合
            float targetRatio = Mathf.Clamp01((float)currentExp / needed);
            float animDuration = 0.8f;
            float animElapsed = 0f;
            while (animElapsed < animDuration)
            {
                animElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, animElapsed / animDuration);
                barFillRect.anchorMax = new Vector2(t * targetRatio, 1);
                yield return null;
            }
            barFillRect.anchorMax = new Vector2(targetRatio, 1);

            yield return new WaitForSeconds(1.2f);

            Destroy(barBgObj);
        }
    }

    IEnumerator ShowStatGrowth(int oldAtk, int oldDef, int oldHp, int oldAcademic, int oldAthletic, int oldHeight, int oldWeight,
                                int newAtk, int newDef, int newHp, int newAcademic, int newAthletic, int newHeight, int newWeight)
    {
        int pillRadius = 30;
        int blur = 16;
        float rowW = 860f;
        float rowH = 80f;
        float rowGap = 14f;
        int newAge = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 0;
        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "";

        // 全画面オーバーレイ
        var overlay = new GameObject("GrowthOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0.953f, 0.969f, 0.973f);
        overlayImg.raycastTarget = true;

        // === ヘッダー: タイトル ===
        float topY = -100f;
        var titleObj = new GameObject("Title");
        titleObj.transform.SetParent(overlay.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, topY);
        titleRect.sizeDelta = new Vector2(900, 70);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(titleText);
        titleText.text = Localization.Get("battle_growth_title");
        titleText.fontSize = 52;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.15f, 0.15f, 0.18f);
        titleText.richText = true;
        titleText.raycastTarget = false;

        // === 名前 + 月齢 ===
        float subY = topY - 60f;
        var subObj = new GameObject("SubTitle");
        subObj.transform.SetParent(overlay.transform, false);
        var subRect = subObj.AddComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 1);
        subRect.anchorMax = new Vector2(0.5f, 1);
        subRect.anchoredPosition = new Vector2(0, subY);
        subRect.sizeDelta = new Vector2(900, 44);
        var subText = subObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(subText);
        subText.text = $"{babyName}    {Localization.GetAge(newAge)}";
        subText.fontSize = 30;
        subText.alignment = TextAlignmentOptions.Center;
        subText.color = new Color(0.50f, 0.50f, 0.55f);
        subText.raycastTarget = false;

        // === ステータス行を1つずつ表示 ===
        float statsStartY = subY - 70f;

        // 定義: [label, oldVal, newVal, accentColor]
        var statDefs = new[] {
            new { label = Localization.Get("battle_stat_hp_label"), oldV = oldHp, newV = newHp, color = new Color(0.88f, 0.27f, 0.27f) },
            new { label = Localization.Get("battle_stat_atk"), oldV = oldAtk, newV = newAtk, color = new Color(0.87f, 0.47f, 0.14f) },
            new { label = Localization.Get("battle_stat_def"), oldV = oldDef, newV = newDef, color = new Color(0.20f, 0.40f, 0.80f) },
            new { label = Localization.Get("battle_stat_athletic"), oldV = oldAthletic, newV = newAthletic, color = new Color(0.13f, 0.67f, 0.33f) },
        };

        var rowObjects = new GameObject[statDefs.Length];

        for (int i = 0; i < statDefs.Length; i++)
        {
            var sd = statDefs[i];
            int diff = sd.newV - sd.oldV;
            float y = statsStartY - i * (rowH + rowGap);

            // Row container
            var rowObj = new GameObject($"StatRow_{sd.label}");
            rowObj.transform.SetParent(overlay.transform, false);
            var rr = rowObj.AddComponent<RectTransform>();
            rr.anchorMin = new Vector2(0.5f, 1);
            rr.anchorMax = new Vector2(0.5f, 1);
            rr.anchoredPosition = new Vector2(0, y);
            rr.sizeDelta = new Vector2(rowW, rowH);
            rowObjects[i] = rowObj;

            // White pill bg
            var rowBg = rowObj.AddComponent<Image>();
            rowBg.sprite = GetPillSprite(pillRadius);
            rowBg.type = Image.Type.Sliced;
            rowBg.color = Color.white;
            rowBg.raycastTarget = false;

            // Shadow
            var sh = new GameObject("Shadow");
            sh.transform.SetParent(rowObj.transform, false);
            sh.transform.SetAsFirstSibling();
            var shR = sh.AddComponent<RectTransform>();
            shR.anchorMin = Vector2.zero;
            shR.anchorMax = Vector2.one;
            shR.offsetMin = new Vector2(-blur, -blur - 3);
            shR.offsetMax = new Vector2(blur, blur - 3);
            var shImg = sh.AddComponent<Image>();
            shImg.sprite = GetShadowSprite(pillRadius, blur);
            shImg.type = Image.Type.Sliced;
            shImg.color = new Color(0f, 0f, 0f, 0.08f);
            shImg.raycastTarget = false;

            // Left: Color accent bar
            var barObj = new GameObject("AccentBar");
            barObj.transform.SetParent(rowObj.transform, false);
            var barRect = barObj.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0, 0.15f);
            barRect.anchorMax = new Vector2(0, 0.85f);
            barRect.anchoredPosition = new Vector2(28, 0);
            barRect.sizeDelta = new Vector2(6, 0);
            var barImg = barObj.AddComponent<Image>();
            barImg.color = sd.color;
            barImg.raycastTarget = false;

            // Stat label
            var lblObj = new GameObject("Label");
            lblObj.transform.SetParent(rowObj.transform, false);
            var lblRect = lblObj.AddComponent<RectTransform>();
            lblRect.anchorMin = new Vector2(0, 0);
            lblRect.anchorMax = new Vector2(0, 1);
            lblRect.anchoredPosition = new Vector2(120, 0);
            lblRect.sizeDelta = new Vector2(160, 0);
            var lblText = lblObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(lblText);
            lblText.text = sd.label;
            lblText.fontSize = 30;
            lblText.fontStyle = FontStyles.Bold;
            lblText.alignment = TextAlignmentOptions.Left;
            lblText.color = sd.color;
            lblText.raycastTarget = false;

            // Old value
            var oldObj = new GameObject("OldVal");
            oldObj.transform.SetParent(rowObj.transform, false);
            var oldRect = oldObj.AddComponent<RectTransform>();
            oldRect.anchorMin = new Vector2(0.5f, 0);
            oldRect.anchorMax = new Vector2(0.5f, 1);
            oldRect.anchoredPosition = new Vector2(-80, 0);
            oldRect.sizeDelta = new Vector2(120, 0);
            var oldText = oldObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(oldText);
            oldText.text = sd.oldV.ToString();
            oldText.fontSize = 34;
            oldText.alignment = TextAlignmentOptions.Right;
            oldText.color = new Color(0.55f, 0.55f, 0.6f);
            oldText.raycastTarget = false;

            // Arrow
            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(rowObj.transform, false);
            var arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(0.5f, 0);
            arrowRect.anchorMax = new Vector2(0.5f, 1);
            arrowRect.anchoredPosition = new Vector2(0, 0);
            arrowRect.sizeDelta = new Vector2(60, 0);
            var arrowText = arrowObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(arrowText);
            arrowText.text = "\u2192";
            arrowText.fontSize = 30;
            arrowText.alignment = TextAlignmentOptions.Center;
            arrowText.color = new Color(0.70f, 0.70f, 0.72f);
            arrowText.raycastTarget = false;

            // New value
            var newObj = new GameObject("NewVal");
            newObj.transform.SetParent(rowObj.transform, false);
            var newRect = newObj.AddComponent<RectTransform>();
            newRect.anchorMin = new Vector2(0.5f, 0);
            newRect.anchorMax = new Vector2(0.5f, 1);
            newRect.anchoredPosition = new Vector2(80, 0);
            newRect.sizeDelta = new Vector2(120, 0);
            var newText = newObj.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(newText);
            newText.text = sd.newV.ToString();
            newText.fontSize = 38;
            newText.fontStyle = FontStyles.Bold;
            newText.alignment = TextAlignmentOptions.Left;
            newText.color = new Color(0.15f, 0.15f, 0.18f);
            newText.raycastTarget = false;

            // Diff badge (pill)
            if (diff > 0)
            {
                var diffObj = new GameObject("Diff");
                diffObj.transform.SetParent(rowObj.transform, false);
                var diffRect = diffObj.AddComponent<RectTransform>();
                diffRect.anchorMin = new Vector2(1, 0.5f);
                diffRect.anchorMax = new Vector2(1, 0.5f);
                diffRect.anchoredPosition = new Vector2(-50, 0);
                diffRect.sizeDelta = new Vector2(90, 38);
                var diffBg = diffObj.AddComponent<Image>();
                diffBg.sprite = GetPillSprite(pillRadius);
                diffBg.type = Image.Type.Sliced;
                diffBg.color = new Color(sd.color.r, sd.color.g, sd.color.b, 0.12f);
                diffBg.raycastTarget = false;

                var diffTextObj = new GameObject("Text");
                diffTextObj.transform.SetParent(diffObj.transform, false);
                var dtRect = diffTextObj.AddComponent<RectTransform>();
                dtRect.anchorMin = Vector2.zero;
                dtRect.anchorMax = Vector2.one;
                dtRect.offsetMin = Vector2.zero;
                dtRect.offsetMax = Vector2.zero;
                var diffText = diffTextObj.AddComponent<TextMeshProUGUI>();
                FontHelper.Apply(diffText);
                diffText.text = $"+{diff}";
                diffText.fontSize = 22;
                diffText.fontStyle = FontStyles.Bold;
                diffText.alignment = TextAlignmentOptions.Center;
                diffText.color = sd.color;
                diffText.raycastTarget = false;
            }

            // Start hidden, reveal with animation
            rowObj.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
            var cg = rowObj.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }

        // Animate rows in one by one
        for (int i = 0; i < rowObjects.Length; i++)
        {
            var row = rowObjects[i];
            var cg = row.GetComponent<CanvasGroup>();
            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float ease = 1f - (1f - t) * (1f - t); // ease-out quad
                cg.alpha = ease;
                row.transform.localScale = Vector3.Lerp(new Vector3(0.9f, 0.9f, 1f), Vector3.one, ease);
                yield return null;
            }
            cg.alpha = 1f;
            row.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }

        // === OKボタン ===
        float okY = statsStartY - statDefs.Length * (rowH + rowGap) - 40f;
        var okBtn = new GameObject("OkButton");
        okBtn.transform.SetParent(overlay.transform, false);
        var okRect = okBtn.AddComponent<RectTransform>();
        okRect.anchorMin = new Vector2(0.5f, 1);
        okRect.anchorMax = new Vector2(0.5f, 1);
        okRect.anchoredPosition = new Vector2(0, okY);
        okRect.sizeDelta = new Vector2(600, 100);

        var okImg = okBtn.AddComponent<Image>();
        okImg.sprite = GetPillSprite(pillRadius);
        okImg.type = Image.Type.Sliced;
        okImg.color = Color.white;

        var okShadowObj = new GameObject("Shadow");
        okShadowObj.transform.SetParent(okBtn.transform, false);
        okShadowObj.transform.SetAsFirstSibling();
        var okShadowRect = okShadowObj.AddComponent<RectTransform>();
        okShadowRect.anchorMin = Vector2.zero;
        okShadowRect.anchorMax = Vector2.one;
        okShadowRect.offsetMin = new Vector2(-blur, -blur - 3);
        okShadowRect.offsetMax = new Vector2(blur, blur - 3);
        var okShadowImg = okShadowObj.AddComponent<Image>();
        okShadowImg.sprite = GetShadowSprite(pillRadius, blur);
        okShadowImg.type = Image.Type.Sliced;
        okShadowImg.color = new Color(0f, 0f, 0f, 0.15f);
        okShadowImg.raycastTarget = false;

        bool dismissed = false;
        var okButton = okBtn.AddComponent<Button>();
        okButton.targetGraphic = okImg;
        okButton.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = okButton.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = new Color(0.92f, 0.92f, 0.92f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.08f;
        okButton.colors = colors;
        okButton.onClick.AddListener(() => dismissed = true);
        AddPressAnimation(okBtn);

        var okTextObj = new GameObject("Text");
        okTextObj.transform.SetParent(okBtn.transform, false);
        var okTextRect = okTextObj.AddComponent<RectTransform>();
        okTextRect.anchorMin = Vector2.zero;
        okTextRect.anchorMax = Vector2.one;
        okTextRect.offsetMin = Vector2.zero;
        okTextRect.offsetMax = Vector2.zero;
        var okTmp = okTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(okTmp);
        okTmp.text = "OK";
        okTmp.fontSize = 38;
        okTmp.alignment = TextAlignmentOptions.Center;
        okTmp.color = new Color(0.15f, 0.15f, 0.18f);
        okTmp.fontStyle = FontStyles.Bold;
        okTmp.raycastTarget = false;

        // Fade in OK button
        var okCg = okBtn.AddComponent<CanvasGroup>();
        okCg.alpha = 0f;
        float okElapsed = 0f;
        while (okElapsed < 0.2f)
        {
            okElapsed += Time.deltaTime;
            okCg.alpha = Mathf.Clamp01(okElapsed / 0.2f);
            yield return null;
        }
        okCg.alpha = 1f;

        while (!dismissed)
            yield return null;

        Destroy(overlay);

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
        var overlay = UIHelper.CreateOverlay();

        var card = new UIE.VisualElement();
        card.AddToClassList("battle-gameover-card");

        var title = UIHelper.CreateLabel(Localization.Get("battle_game_over"), "battle-gameover-title");
        card.Add(title);

        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "";
        var msg = UIHelper.CreateLabel(
            string.Format(Localization.Get("ui_try_again"), babyName),
            "battle-gameover-msg");
        card.Add(msg);

        var retryBtn = UIHelper.CreatePillButton(Localization.Get("ui_back_to_title_long"), "pill-button-large");
        retryBtn.clicked += () => SceneManager.LoadScene("TitleScene");
        card.Add(retryBtn);

        overlay.Add(card);
        overlayRoot.Add(overlay);
    }

    // ===== 固定エンカウント（傭兵）勝利 → 館に戻る =====

    IEnumerator FixedEncounterVictory()
    {
        yield return new WaitForSeconds(1.5f);

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
            // 館内（area 2）に戻る — 位置はそのまま
        }

        SceneManager.LoadScene("MapScene");
    }

    // ===== ボス撃破演出 =====

    IEnumerator BossDefeatSequence()
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        string[] bossLines;
        if (area == 4 && enemyName == "メロディアス女王")
        {
            bossLines = new string[]
            {
                Localization.Get("melodias_defeat_line1"),
                Localization.Get("melodias_defeat_line2"),
                Localization.Get("melodias_defeat_line3"),
            };
        }
        else if (area == 2 && enemyName == "デヴィル夫人")
        {
            bossLines = new string[]
            {
                Localization.Get("devil_lady_defeat_line1"),
                Localization.Get("devil_lady_defeat_line2"),
                Localization.Get("devil_lady_defeat_line3"),
            };
        }
        else
        {
            bossLines = new string[]
            {
                Localization.Get("boss_defeat_line1"),
                Localization.Get("boss_defeat_line2"),
                Localization.Get("boss_defeat_line3"),
                Localization.Get("boss_defeat_line4"),
            };
        }

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
            FontHelper.Apply(tmp);

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

        // フラグをリセットして遷移
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.babyCurrentHp = -1; // ボス撃破後は全回復

            if (area == 4 && enemyName == "メロディアス女王")
            {
                // メロディアス女王撃破 → 暫定で小悪魔の街に戻す
                DataCarrier.Instance.currentArea = 3;
                DataCarrier.Instance.mapPlayerX = 5;
                DataCarrier.Instance.mapPlayerY = 17;
            }
            else if (area == 2 && enemyName == "デヴィル夫人")
            {
                // デヴィル夫人撃破 → 小悪魔の街へ
                DataCarrier.Instance.currentArea = 3;
                DataCarrier.Instance.mapPlayerX = 5;
                DataCarrier.Instance.mapPlayerY = 2;
            }
            else
            {
                // シバ撃破 → 悪魔村へ
                DataCarrier.Instance.currentArea = 1;
                DataCarrier.Instance.mapPlayerX = 5;
                DataCarrier.Instance.mapPlayerY = 9;
            }
            DataCarrier.Instance.SaveData();
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
        textRect.sizeDelta = new Vector2(800, 200);
        var victoryText = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(victoryText);
        victoryText.fontSize = 64;
        victoryText.alignment = TextAlignmentOptions.Center;
        victoryText.color = Color.white;
        victoryText.fontStyle = FontStyles.Bold;
        victoryText.raycastTarget = false;

        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "ベイビー";
        int currentAge = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 0;

        victoryText.text = Localization.Get("battle_victory_return", babyName, currentAge);
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
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        // Hamburger button
        var menuBtn = new UIE.Button();
        menuBtn.AddToClassList("battle-menu-btn");
        menuBtn.style.top = 10 + safeTop;
        for (int i = 0; i < 3; i++)
        {
            var line = new UIE.VisualElement();
            line.AddToClassList("battle-menu-line");
            menuBtn.Add(line);
        }
        menuBtn.clicked += ToggleMenuPanel;
        overlayRoot.Add(menuBtn);
    }

    void ToggleMenuPanel()
    {
        if (menuOverlayEl != null)
        {
            menuOverlayEl.RemoveFromHierarchy();
            menuOverlayEl = null;
            return;
        }

        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        menuOverlayEl = UIHelper.CreateOverlay();
        menuOverlayEl.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == menuOverlayEl)
            { menuOverlayEl.RemoveFromHierarchy(); menuOverlayEl = null; }
        });

        var card = new UIE.VisualElement();
        card.AddToClassList("battle-menu-card");
        card.style.top = 100 + safeTop;

        var saveBtn = new UIE.Button();
        saveBtn.AddToClassList("battle-menu-item-btn");
        UIHelper.ApplyFont(saveBtn);
        saveBtn.text = Localization.Get("battle_save_button");
        saveBtn.clicked += () => { menuOverlayEl?.RemoveFromHierarchy(); menuOverlayEl = null; OnSave(); };
        card.Add(saveBtn);

        var topBtn = new UIE.Button();
        topBtn.AddToClassList("battle-menu-item-btn");
        UIHelper.ApplyFont(topBtn);
        topBtn.text = Localization.Get("battle_top_button");
        topBtn.clicked += () => { menuOverlayEl?.RemoveFromHierarchy(); menuOverlayEl = null; OnGoTop(); };
        card.Add(topBtn);

        menuOverlayEl.Add(card);
        overlayRoot.Add(menuOverlayEl);
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

    // ── Pill / Shadow sprite helpers ──
    static Sprite _pillSprite, _shadowSprite;

    static Sprite GetPillSprite(int radius)
    {
        if (_pillSprite != null) return _pillSprite;
        int size = radius * 2 + 2;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = (size - 1) / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) - radius;
                if (dist <= -1f) tex.SetPixel(x, y, Color.white);
                else if (dist <= 0f) tex.SetPixel(x, y, new Color(1, 1, 1, -dist));
                else tex.SetPixel(x, y, new Color(0, 0, 0, 0));
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
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center)) - radius;
                float alpha;
                if (dist <= 0f) alpha = 0f;
                else if (dist >= blur) alpha = 0f;
                else { float t = dist / blur; alpha = (1f - t) * (1f - t); }
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        tex.Apply();
        int borderVal = radius + blur;
        var border = new Vector4(borderVal, borderVal, borderVal, borderVal);
        _shadowSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
        return _shadowSprite;
    }

    void AddPressAnimation(GameObject obj)
    {
        var trigger = obj.AddComponent<EventTrigger>();
        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener((_) => obj.transform.localScale = new Vector3(0.95f, 0.95f, 1f));
        trigger.triggers.Add(down);
        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener((_) => obj.transform.localScale = Vector3.one);
        trigger.triggers.Add(up);
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((_) => obj.transform.localScale = Vector3.one);
        trigger.triggers.Add(exit);
    }
}
