using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UIE = UnityEngine.UIElements;

public class BattleManager : MonoBehaviour
{
    [Header("Baby Sprites")]
    public Sprite[] babySprites;

    [Header("Enemy Sprites")]
    public Sprite firstEnemySprite;

    // Resources から自動読み込み
    Sprite loadedEnemySprite;

    // プレイヤーステータス
    int playerHp, playerMaxHp, playerAtk, playerDef;
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
    int enemyAge; // 敵の月齢（経験値計算用）

    // UI要素 (UI Toolkit — メイン)
    UIE.PanelSettings mainPanelSettings;
    UIE.VisualElement root;
    UIE.VisualElement playerPanel, enemyPanel;
    UIE.VisualElement playerFaceMask, enemyFaceMask;
    UIE.VisualElement playerFaceEl, enemyFaceEl;
    UIE.Label playerNameLabel, enemyNameLabel;
    UIE.VisualElement playerHpFill, enemyHpFill;
    UIE.Label playerHpLabel, enemyHpLabel;
    UIE.Label playerAgeEl, enemyAgeEl;
    UIE.Label battleLogLabel;
    UIE.VisualElement vsTextEl;
    UIE.VisualElement actionPanelEl;
    UIE.Button attackBtn, defendBtn, specialBtn, motherBtn;
    UIE.VisualElement flashOverlay;
    UIE.VisualElement battleLogEl;

    // UI Toolkit — オーバーレイ
    UIE.PanelSettings overlayPanelSettings;
    UIE.VisualElement overlayRoot;
    UIE.VisualElement menuOverlayEl;
    UIE.VisualElement skillInfoEl;

    // バトル状態
    bool isBattleActive;
    bool isPlayerTurn;
    bool waitingForAction;
    int battleTurnCount;
    int enemyTurnCount; // 敵ターンカウント（シバ必殺技用）


    void Start()
    {
        InitializePlayer();
        InitializeEnemy();

        // メインUI
        mainPanelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/BattleStyle" }, mainPanelSettings);
        root.pickingMode = UIE.PickingMode.Ignore;

        BuildBattleUI();

        // UI Toolkit overlay for menus & dialogs
        overlayPanelSettings = UIHelper.CreatePanelSettings(10f);
        var overlayObj = new GameObject("BattleOverlayUI");
        overlayRoot = UIHelper.SetupUIDocument(overlayObj,
            new[] { "UI/CommonStyle", "UI/BattleStyle" }, overlayPanelSettings);
        overlayRoot.pickingMode = UIE.PickingMode.Ignore;

        CreateMenuBar();

        StartCoroutine(BattleStart());
    }

    void OnDestroy()
    {
        if (mainPanelSettings != null)
            Destroy(mainPanelSettings);
        if (overlayPanelSettings != null)
            Destroy(overlayPanelSettings);
    }

    void InitializePlayer()
    {
        if (DataCarrier.Instance != null)
        {
            playerMaxHp = DataCarrier.Instance.babyHp;
            playerHp = playerMaxHp;
            if (DataCarrier.Instance.babyCurrentHp > 0)
                playerHp = Mathf.Min(DataCarrier.Instance.babyCurrentHp, playerMaxHp);
            playerAtk = DataCarrier.Instance.babyAtk;
            playerDef = DataCarrier.Instance.babyDef;

            isMale = DataCarrier.Instance.babyGender == "男の子";

            if (isMale)
            {
                playerAtk = (int)(playerAtk * 1.5f);
                playerEvasion = 5;
            }
            else
            {
                playerEvasion = 15;
            }

            playerEvasion = Mathf.Min(playerEvasion, 70);

            isGodBaby = DataCarrier.Instance.isGodBaby;
            if (isGodBaby)
            {
                playerAtk = (int)(playerAtk * 1.12f);
                playerDef = (int)(playerDef * 1.12f);
                playerMaxHp = (int)(playerMaxHp * 1.12f);
                if (DataCarrier.Instance.babyCurrentHp > 0)
                    playerHp = Mathf.Min(DataCarrier.Instance.babyCurrentHp, playerMaxHp);
                else
                    playerHp = playerMaxHp;
            }

            playerPoisonTurns = DataCarrier.Instance.babyPoisonTurns;

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
            playerMaxHp = 100;
            playerHp = playerMaxHp;
            playerAtk = 30;
            playerDef = 20;
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

        if (!string.IsNullOrEmpty(DataCarrier.Instance?.fixedEncounterEnemy))
        {
            InitializeFixedEnemy(DataCarrier.Instance.fixedEncounterEnemy);
            DataCarrier.Instance.fixedEncounterEnemy = "";
            return;
        }

        if (fromMap && bossBattle && area == 4)
        {
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
            enemyName = "村の王シバ";
            enemyAge = -1;
            enemyMaxHp = 500;
            enemyHp = enemyMaxHp;
            enemyAtk = 80;
            enemyDef = 45;
            enemySpeed = 70;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/boss/first-boss-shiba");
        }
        else if (fromMap)
        {
            int age = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 1;
            InitializeRandomEnemy(age);
        }
        else
        {
            enemyName = "いじわるベイビー";
            enemyAge = -1;
            enemyMaxHp = 155;
            enemyHp = enemyMaxHp;
            enemyAtk = 50;
            enemyDef = 45;
            enemySpeed = 45;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/frist-enemy");
        }
    }

    void InitializeRandomEnemy(int playerAge)
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        if (area == 0)
        {
            int minAge = Mathf.Max(0, playerAge - 2);
            int maxAge = Mathf.Min(8, playerAge + 2);
            enemyAge = Random.Range(minAge, maxAge + 1);
        }
        else if (area == 3)
        {
            int minAge = Mathf.Max(14, playerAge - 2);
            int maxAge = Mathf.Min(24, playerAge + 3);
            enemyAge = Random.Range(minAge, maxAge + 1);
        }
        else
        {
            int minAge = Mathf.Max(6, playerAge - 2);
            int maxAge = Mathf.Min(15, playerAge + 3);
            enemyAge = Random.Range(minAge, maxAge + 1);
        }

        object[][] enemyDefs;

        if (area == 3)
        {
            enemyDefs = new object[][] {
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
            enemyDefs = new object[][] {
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
                new object[]{ "なきむしベイビー",     "EnemyBabys/common-nakimushi",    60, 15, 10, 20, 0, 3 },
                new object[]{ "やんちゃベイビー",     "EnemyBabys/common-yantya",       75, 22, 12, 30, 1, 5 },
                new object[]{ "いじわるベイビー",     "EnemyBabys/frist-enemy",         85, 28, 18, 25, 3, 6 },
                new object[]{ "わがままベイビー",     "EnemyBabys/common-wagamama",    100, 32, 22, 28, 4, 7 },
                new object[]{ "あばれんぼうベイビー", "EnemyBabys/common-abarennbou",  120, 40, 25, 35, 5, 8 },
            };
        }

        var candidates = new System.Collections.Generic.List<object[]>();
        foreach (var def in enemyDefs)
        {
            int appearMin = (int)def[6];
            int appearMax = (int)def[7];
            if (enemyAge >= appearMin && enemyAge <= appearMax)
                candidates.Add(def);
        }
        if (candidates.Count == 0)
            candidates.Add(enemyDefs[enemyDefs.Length - 1]);

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

        if (chosen.Length > 8 && chosen[8] is Color bgCol)
        {
            isDevilEnemy = true;
            enemyBgColor = bgCol;
        }

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

        enemyMaxHp = 280;
        enemyHp = enemyMaxHp;
        enemyAtk = 60;
        enemyDef = 35;
        enemySpeed = 55;
        enemyBgColor = new Color(0.35f, 0.05f, 0.05f);
    }

    // ===== バトルUI作成 (UI Toolkit) =====

    void BuildBattleUI()
    {
        var (safeTop, safeBottom, _, _) = UIHelper.GetSafeMargins();

        var battleRoot = new UIE.VisualElement();
        battleRoot.AddToClassList("battle-root");
        root.Add(battleRoot);

        // 背景
        var bg = new UIE.VisualElement();
        bg.AddToClassList("battle-bg");
        battleRoot.Add(bg);

        // フラッシュオーバーレイ
        flashOverlay = new UIE.VisualElement();
        flashOverlay.AddToClassList("battle-flash");
        flashOverlay.pickingMode = UIE.PickingMode.Ignore;
        battleRoot.Add(flashOverlay);

        // 敵エリア（上）
        var enemyArea = new UIE.VisualElement();
        enemyArea.AddToClassList("battle-enemy-area");
        enemyArea.style.paddingTop = safeTop + 30;
        battleRoot.Add(enemyArea);
        enemyPanel = BuildCharPanel(enemyArea, false);

        // 中央エリア（VS + ログ）
        var centerArea = new UIE.VisualElement();
        centerArea.AddToClassList("battle-center");
        battleRoot.Add(centerArea);

        vsTextEl = UIHelper.CreateLabel("VS", "battle-vs");
        centerArea.Add(vsTextEl);

        battleLogEl = new UIE.VisualElement();
        battleLogEl.AddToClassList("battle-log");
        centerArea.Add(battleLogEl);

        battleLogLabel = UIHelper.CreateLabel("", "battle-log-text");
        battleLogEl.Add(battleLogLabel);

        // プレイヤーエリア（下）
        var playerArea = new UIE.VisualElement();
        playerArea.AddToClassList("battle-player-area");
        battleRoot.Add(playerArea);
        playerPanel = BuildCharPanel(playerArea, true);

        // アクションエリア
        actionPanelEl = new UIE.VisualElement();
        actionPanelEl.AddToClassList("battle-action-area");
        actionPanelEl.style.paddingBottom = safeBottom + 40;
        battleRoot.Add(actionPanelEl);

        // 上段ボタン行（攻撃, 母スキル）
        var topRow = new UIE.VisualElement();
        topRow.AddToClassList("battle-action-row");
        topRow.style.marginBottom = 32;
        actionPanelEl.Add(topRow);

        attackBtn = BuildActionButton(topRow, Localization.Get("battle_normal_attack"),
            normalAttackName, normalAttackDesc, OnAttack);
        motherBtn = BuildActionButton(topRow, Localization.Get("battle_special_attack"),
            motherAttackName, motherAttackDesc, OnMotherAttack);

        // 下段ボタン行（防御, 必殺技）
        var bottomRow = new UIE.VisualElement();
        bottomRow.AddToClassList("battle-action-row");
        actionPanelEl.Add(bottomRow);

        defendBtn = BuildActionButton(bottomRow, Localization.Get("battle_defend"),
            Localization.Get("battle_defend_name"), defendDesc, OnDefend);
        specialBtn = BuildActionButton(bottomRow, Localization.Get("battle_special_skill"),
            specialAttackName, specialAttackDesc, OnSpecial);

        actionPanelEl.style.display = UIE.DisplayStyle.None;

        // 初期表示
        UpdatePlayerDisplay();
        UpdateEnemyDisplay();
    }

    UIE.VisualElement BuildCharPanel(UIE.VisualElement area, bool isPlayer)
    {
        var panel = new UIE.VisualElement();
        panel.AddToClassList("battle-char-panel");
        area.Add(panel);

        // 名前
        var nameLabel = UIHelper.CreateLabel("", "battle-char-name");
        nameLabel.AddToClassList(isPlayer ? "battle-name-player" : "battle-name-enemy");
        panel.Add(nameLabel);
        if (isPlayer) playerNameLabel = nameLabel; else enemyNameLabel = nameLabel;

        // 顔マスク
        var faceMask = new UIE.VisualElement();
        faceMask.AddToClassList("battle-face-mask");
        panel.Add(faceMask);
        if (isPlayer) playerFaceMask = faceMask; else enemyFaceMask = faceMask;

        // 顔要素（スプライト or 自動生成パーツの親）
        var faceEl = new UIE.VisualElement();
        faceEl.name = "Face";
        faceEl.style.position = UIE.Position.Absolute;
        faceEl.style.left = 0; faceEl.style.top = 0;
        faceEl.style.right = 0; faceEl.style.bottom = 0;
        faceMask.Add(faceEl);
        if (isPlayer) playerFaceEl = faceEl; else enemyFaceEl = faceEl;

        // 年齢ラベル
        var ageLabel = UIHelper.CreateLabel("", "battle-age-label");
        faceMask.Add(ageLabel);
        if (isPlayer) playerAgeEl = ageLabel; else enemyAgeEl = ageLabel;

        // HPバー
        var hpBg = new UIE.VisualElement();
        hpBg.AddToClassList("battle-hp-bg");
        panel.Add(hpBg);

        var hpFill = new UIE.VisualElement();
        hpFill.AddToClassList("battle-hp-fill");
        hpFill.AddToClassList(isPlayer ? "battle-hp-fill-player" : "battle-hp-fill-enemy");
        hpFill.style.width = new UIE.Length(100, UIE.LengthUnit.Percent);
        hpBg.Add(hpFill);
        if (isPlayer) playerHpFill = hpFill; else enemyHpFill = hpFill;

        // HPテキスト
        var hpLabel = UIHelper.CreateLabel("", "battle-hp-text");
        panel.Add(hpLabel);
        if (isPlayer) playerHpLabel = hpLabel; else enemyHpLabel = hpLabel;

        return panel;
    }

    UIE.Button BuildActionButton(UIE.VisualElement row, string categoryLabel, string skillName, string description, System.Action onClick)
    {
        var wrapper = new UIE.VisualElement();
        wrapper.style.position = UIE.Position.Relative;
        row.Add(wrapper);

        // カテゴリラベル（ボタンの上）
        var catLabel = UIHelper.CreateLabel(categoryLabel, "battle-action-label");
        wrapper.Add(catLabel);

        // ボタン本体
        var btn = new UIE.Button();
        btn.AddToClassList("battle-action-btn");
        UIHelper.ApplyFont(btn);
        wrapper.Add(btn);

        // 技名テキスト
        var btnText = UIHelper.CreateLabel(skillName, "battle-action-btn-text");
        btn.Add(btnText);

        btn.clicked += () => onClick();

        // infoボタン
        var infoBtn = new UIE.Button();
        infoBtn.AddToClassList("battle-action-info");
        UIHelper.ApplyFont(infoBtn);
        infoBtn.text = "i";
        string descCapture = description;
        string nameCapture = skillName;
        string catCapture = categoryLabel;
        infoBtn.clicked += () => ShowSkillInfo(catCapture, nameCapture, descCapture);
        btn.Add(infoBtn);

        return btn;
    }

    void ShowSkillInfo(string category, string skillName, string description)
    {
        // 既に開いていたら閉じる
        if (skillInfoEl != null)
        {
            skillInfoEl.RemoveFromHierarchy();
            skillInfoEl = null;
            return;
        }

        skillInfoEl = UIHelper.CreateOverlay();
        skillInfoEl.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == skillInfoEl)
            { skillInfoEl.RemoveFromHierarchy(); skillInfoEl = null; }
        });

        var popup = new UIE.VisualElement();
        popup.AddToClassList("battle-skill-popup");

        // アイコンエリア
        var iconArea = new UIE.VisualElement();
        iconArea.AddToClassList("battle-skill-icon-area");
        popup.Add(iconArea);

        bool isSpecialSkill = category == Localization.Get("battle_special_skill");
        if (category == Localization.Get("battle_defend"))
            DrawShieldIconEl(iconArea, 80);
        else if (category == Localization.Get("battle_special_attack"))
            DrawMotherIconEl(iconArea, playerMotherName, 80);
        else
            GenerateSkillIconEl(iconArea, playerFatherName, isSpecialSkill, 70);

        // タイトル
        var title = UIHelper.CreateLabel(skillName, "battle-skill-title");
        title.style.color = new Color(1f, 0.87f, 0.27f);
        popup.Add(title);

        // 説明文
        var desc = UIHelper.CreateLabel(description, "battle-skill-desc");
        popup.Add(desc);

        skillInfoEl.Add(popup);
        overlayRoot.Add(skillInfoEl);
    }

    // ===== 表示更新 =====

    void UpdatePlayerDisplay()
    {
        string babyName = Localization.Get("birth_default_name");
        int babyAge = 0;
        if (DataCarrier.Instance != null)
        {
            if (!string.IsNullOrEmpty(DataCarrier.Instance.babyName))
                babyName = DataCarrier.Instance.babyName;
            else
            {
                string gender = DataCarrier.Instance.babyGender;
                if (!string.IsNullOrEmpty(gender))
                    babyName = gender == "男の子" ? "GOD BOY" : "GOD GIRL";
            }
            babyAge = DataCarrier.Instance.babyAge;
        }

        if (isGodBaby)
            playerNameLabel.text = $"<color=#FFD700>{babyName}</color>";
        else
            playerNameLabel.text = babyName;
        playerNameLabel.enableRichText = true;

        if (playerAgeEl != null)
            playerAgeEl.text = Localization.GetAge(babyAge);
        playerHpLabel.text = $"HP:{playerHp}/{playerMaxHp}";

        float hpRatio = (float)playerHp / playerMaxHp;
        playerHpFill.style.width = new UIE.Length(hpRatio * 100f, UIE.LengthUnit.Percent);

        if (!TryShowBabySprite(playerFaceEl))
        {
            GenerateBabyFaceForBattle(playerFaceEl);
        }
    }

    bool TryShowBabySprite(UIE.VisualElement faceEl)
    {
        if (DataCarrier.Instance == null) return false;

        Sprite customSprite = DataCarrier.LoadCustomBabySprite();
        if (customSprite != null)
        {
            ClearFaceParts(faceEl);
            faceEl.style.backgroundImage = new UIE.StyleBackground(customSprite);
            faceEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            if (DataCarrier.Instance.isGodBaby)
                AddGodAuraEl(faceEl);
            return true;
        }

        string fatherName = GetParentImageName(DataCarrier.Instance.fatherName);
        string motherName = GetParentImageName(DataCarrier.Instance.motherName);
        string genderKey = DataCarrier.Instance.babyGender == "男の子" ? "male" : "female";

        string babyImagePath = $"babys/{fatherName}_{motherName}_{genderKey}";
        Sprite babySprite = Resources.Load<Sprite>(babyImagePath);

        if (babySprite != null)
        {
            ClearFaceParts(faceEl);
            faceEl.style.backgroundImage = new UIE.StyleBackground(babySprite);
            faceEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            if (DataCarrier.Instance.isGodBaby)
                AddGodAuraEl(faceEl);
            return true;
        }
        return false;
    }

    void ClearFaceParts(UIE.VisualElement el)
    {
        el.Clear();
        el.style.backgroundImage = UIE.StyleKeyword.None;
    }

    void AddGodAuraEl(UIE.VisualElement parent)
    {
        for (int i = 2; i >= 0; i--)
        {
            var aura = BPartEl("Aura" + i, parent, Vector2.zero, new Vector2(130 + i * 12, 150 + i * 12));
            aura.style.backgroundColor = new Color(1f, 0.85f, 0.2f, 0.1f - i * 0.02f);
            aura.SendToBack();
        }
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

    void UpdateEnemyDisplay()
    {
        enemyNameLabel.text = Localization.GetEnemy(enemyName);
        if (enemyAgeEl != null)
            enemyAgeEl.text = enemyAge >= 0 ? Localization.GetAge(enemyAge) : "";
        enemyHpLabel.text = $"HP:{enemyHp}/{enemyMaxHp}  ATK:{enemyAtk}";

        float hpRatio = (float)enemyHp / enemyMaxHp;
        enemyHpFill.style.width = new UIE.Length(hpRatio * 100f, UIE.LengthUnit.Percent);

        Sprite enemySprite = loadedEnemySprite ?? firstEnemySprite;
        if (enemySprite != null)
        {
            enemyFaceEl.style.backgroundImage = new UIE.StyleBackground(enemySprite);
            enemyFaceEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        else if (enemyBgColor != Color.clear)
        {
            enemyFaceEl.style.backgroundImage = UIE.StyleKeyword.None;
            enemyFaceMask.style.backgroundColor = enemyBgColor;
        }
        else
        {
            enemyFaceMask.style.backgroundColor = new Color(1f, 0.5f, 0.5f);
        }
    }

    // ===== BPartEl (UI Toolkit版のプロシージャル描画) =====

    UIE.VisualElement BPartEl(string name, UIE.VisualElement parent, Vector2 pos, Vector2 size)
    {
        var el = new UIE.VisualElement();
        el.name = name;
        el.style.position = UIE.Position.Absolute;
        el.style.width = size.x;
        el.style.height = size.y;
        el.style.left = new UIE.Length(50, UIE.LengthUnit.Percent);
        el.style.top = new UIE.Length(50, UIE.LengthUnit.Percent);
        el.style.translate = new UIE.StyleTranslate(
            new UIE.Translate(new UIE.Length(pos.x - size.x / 2f), new UIE.Length(-pos.y - size.y / 2f)));
        el.pickingMode = UIE.PickingMode.Ignore;
        parent.Add(el);
        return el;
    }

    void GenerateBabyFaceForBattle(UIE.VisualElement parent)
    {
        ClearFaceParts(parent);

        int atk = 50, academic = 60, athletic = 60;
        string gender = "男の子";
        bool godBaby = false;

        if (DataCarrier.Instance != null)
        {
            atk = DataCarrier.Instance.babyAtk;
            academic = DataCarrier.Instance.babyAcademic;
            athletic = DataCarrier.Instance.babyAthletic;
            gender = DataCarrier.Instance.babyGender;
            godBaby = DataCarrier.Instance.isGodBaby;
        }

        float s = 1.0f;

        Color[] skinTones = { new Color(0.98f, 0.89f, 0.82f), new Color(0.95f, 0.83f, 0.74f), new Color(0.88f, 0.73f, 0.62f) };
        Color skin = skinTones[Random.Range(0, skinTones.Length)];
        Color skinShadow = new Color(skin.r * 0.85f, skin.g * 0.82f, skin.b * 0.8f);

        Color[] hairTones = { new Color(0.08f, 0.06f, 0.05f), new Color(0.2f, 0.12f, 0.08f), new Color(0.35f, 0.22f, 0.12f), new Color(0.55f, 0.38f, 0.2f) };
        Color hair = hairTones[Mathf.Clamp(academic / 25, 0, 3)];
        Color hairShadow = new Color(hair.r * 0.6f, hair.g * 0.6f, hair.b * 0.6f);

        if (godBaby)
        {
            for (int i = 2; i >= 0; i--)
            {
                var aura = BPartEl("Aura" + i, parent, Vector2.zero, new Vector2((130 + i * 12) * s, (150 + i * 12) * s));
                aura.style.backgroundColor = new Color(1f, 0.85f, 0.2f, 0.1f - i * 0.02f);
            }
        }

        BPartEl("FaceShadow", parent, new Vector2(2 * s, -3 * s), new Vector2(105 * s, 125 * s)).style.backgroundColor = new Color(0, 0, 0, 0.12f);
        BPartEl("Face", parent, Vector2.zero, new Vector2(100 * s, 120 * s)).style.backgroundColor = skin;
        BPartEl("FaceHL", parent, new Vector2(18 * s, 12 * s), new Vector2(30 * s, 60 * s)).style.backgroundColor = new Color(1, 1, 1, 0.12f);
        BPartEl("LeftShadow", parent, new Vector2(-38 * s, 0), new Vector2(25 * s, 90 * s)).style.backgroundColor = new Color(skinShadow.r, skinShadow.g, skinShadow.b, 0.35f);

        BPartEl("HairShadow", parent, new Vector2(2 * s, 43 * s), new Vector2(112 * s, 52 * s)).style.backgroundColor = new Color(0, 0, 0, 0.15f);
        BPartEl("Hair", parent, new Vector2(0, 45 * s), new Vector2(108 * s, 48 * s)).style.backgroundColor = hair;
        BPartEl("HairHL", parent, new Vector2(12 * s, 50 * s), new Vector2(30 * s, 20 * s)).style.backgroundColor = new Color(hair.r * 1.4f, hair.g * 1.4f, hair.b * 1.3f, 0.4f);
        BPartEl("TopHair", parent, new Vector2(0, 62 * s), new Vector2(85 * s, 25 * s)).style.backgroundColor = hair;
        BPartEl("LeftHair", parent, new Vector2(-44 * s, 12 * s), new Vector2(22 * s, 60 * s)).style.backgroundColor = hairShadow;
        BPartEl("RightHair", parent, new Vector2(44 * s, 12 * s), new Vector2(22 * s, 60 * s)).style.backgroundColor = hair;

        for (int i = 0; i < 4; i++)
        {
            float bx = -25 * s + (50f / 3) * i * s;
            BPartEl($"Bang{i}", parent, new Vector2(bx, 32 * s), new Vector2(15 * s, 22 * s)).style.backgroundColor = i % 2 == 0 ? hair : hairShadow;
        }

        Create3DBattleEye(parent, -18 * s, 6 * s, s, gender == "女の子", skin);
        Create3DBattleEye(parent, 18 * s, 6 * s, s, gender == "女の子", skin);

        float browAngle = (atk - 50) * 0.2f;
        var browL = BPartEl("BrowL", parent, new Vector2(-20 * s, 26 * s), new Vector2(22 * s, 4 * s));
        browL.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(browAngle, UIE.AngleUnit.Degree)));
        browL.style.backgroundColor = new Color(hair.r * 0.7f, hair.g * 0.7f, hair.b * 0.7f);
        var browR = BPartEl("BrowR", parent, new Vector2(20 * s, 26 * s), new Vector2(22 * s, 4 * s));
        browR.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(-browAngle, UIE.AngleUnit.Degree)));
        browR.style.backgroundColor = new Color(hair.r * 0.7f, hair.g * 0.7f, hair.b * 0.7f);

        BPartEl("NoseHL", parent, new Vector2(1 * s, -3 * s), new Vector2(5 * s, 10 * s)).style.backgroundColor = new Color(1, 1, 1, 0.15f);
        BPartEl("NoseShadow", parent, new Vector2(-3 * s, -5 * s), new Vector2(4 * s, 8 * s)).style.backgroundColor = new Color(skinShadow.r, skinShadow.g, skinShadow.b, 0.2f);
        BPartEl("NoseTip", parent, new Vector2(0, -10 * s), new Vector2(10 * s, 8 * s)).style.backgroundColor = new Color(skin.r * 0.95f, skin.g * 0.92f, skin.b * 0.9f, 0.5f);

        float smile = athletic / 100f;
        Color lip = godBaby ? new Color(0.85f, 0.35f, 0.4f) : new Color(0.82f, 0.55f, 0.55f);
        float mw = (18 + smile * 10) * s;
        BPartEl("MouthShadow", parent, new Vector2(0, -28 * s), new Vector2((mw + 4) * s, 8 * s)).style.backgroundColor = new Color(skin.r * 0.85f, skin.g * 0.8f, skin.b * 0.78f, 0.3f);
        BPartEl("UpperLip", parent, new Vector2(0, -25 * s), new Vector2(mw, 5 * s)).style.backgroundColor = lip;
        BPartEl("LowerLip", parent, new Vector2(0, -30 * s), new Vector2(mw * 0.9f, 6 * s)).style.backgroundColor = lip;
        BPartEl("MouthLine", parent, new Vector2(0, -27 * s), new Vector2(mw * 0.8f, 1.5f * s)).style.backgroundColor = new Color(lip.r * 0.7f, lip.g * 0.6f, lip.b * 0.6f);

        float cheekAlpha = gender == "女の子" ? 0.3f : 0.15f;
        Color cheekC = gender == "女の子" ? new Color(1f, 0.5f, 0.55f, cheekAlpha) : new Color(1f, 0.7f, 0.7f, cheekAlpha);
        BPartEl("CheekL", parent, new Vector2(-28 * s, -10 * s), new Vector2(22 * s, 18 * s)).style.backgroundColor = cheekC;
        BPartEl("CheekR", parent, new Vector2(28 * s, -10 * s), new Vector2(22 * s, 18 * s)).style.backgroundColor = cheekC;

        BPartEl("EarL", parent, new Vector2(-48 * s, 4 * s), new Vector2(12 * s, 20 * s)).style.backgroundColor = skin;
        BPartEl("EarR", parent, new Vector2(48 * s, 4 * s), new Vector2(12 * s, 20 * s)).style.backgroundColor = skin;
    }

    void Create3DBattleEye(UIE.VisualElement parent, float x, float y, float s, bool isFemale, Color skin)
    {
        float sz = 9 * s;

        BPartEl("LidShadow", parent, new Vector2(x, y + sz * 0.35f), new Vector2(sz * 1.7f, sz * 0.4f)).style.backgroundColor = new Color(skin.r * 0.8f, skin.g * 0.75f, skin.b * 0.7f, 0.4f);
        BPartEl("EyeWhite", parent, new Vector2(x, y), new Vector2(sz * 1.5f, sz)).style.backgroundColor = new Color(0.95f, 0.95f, 0.97f);

        Color[] irisColors = { new Color(0.18f, 0.12f, 0.08f), new Color(0.35f, 0.5f, 0.65f), new Color(0.3f, 0.55f, 0.35f) };
        Color iris = irisColors[Random.Range(0, irisColors.Length)];
        BPartEl("Iris", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.85f, sz * 0.85f)).style.backgroundColor = iris;
        BPartEl("IrisInner", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.55f, sz * 0.55f)).style.backgroundColor = new Color(iris.r * 0.6f, iris.g * 0.6f, iris.b * 0.6f);
        BPartEl("Pupil", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.3f, sz * 0.3f)).style.backgroundColor = new Color(0.02f, 0.02f, 0.02f);

        BPartEl("HL1", parent, new Vector2(x - sz * 0.12f, y + sz * 0.12f), new Vector2(sz * 0.22f, sz * 0.22f)).style.backgroundColor = Color.white;
        BPartEl("HL2", parent, new Vector2(x + sz * 0.15f, y - sz * 0.08f), new Vector2(sz * 0.1f, sz * 0.1f)).style.backgroundColor = new Color(1, 1, 1, 0.6f);

        if (isFemale)
        {
            for (int i = 0; i < 4; i++)
            {
                float lx = x - sz * 0.4f + (sz * 0.8f / 3) * i;
                var lash = BPartEl($"Lash{i}", parent, new Vector2(lx, y + sz * 0.5f), new Vector2(1.5f * s, (4 + Random.Range(0, 2)) * s));
                lash.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(-15 + i * 10, UIE.AngleUnit.Degree)));
                lash.style.backgroundColor = new Color(0.1f, 0.08f, 0.06f);
            }
        }
    }

    // ===== アイコン生成 (UI Toolkit版) =====

    void GenerateSkillIconEl(UIE.VisualElement parent, string fatherName, bool isSpecial, float size)
    {
        float s = size / 120f;
        Color mainColor, accentColor;

        switch (fatherName)
        {
            case "タケシ":
                mainColor = new Color(0.9f, 0.3f, 0.2f);
                accentColor = new Color(1f, 0.6f, 0.2f);
                DrawFistIconEl(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "ユウキ":
                mainColor = new Color(0.1f, 0.8f, 0.9f);
                accentColor = new Color(0.3f, 1f, 0.5f);
                DrawCircuitIconEl(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "ゴウ":
                mainColor = new Color(0.4f, 0.6f, 0.3f);
                accentColor = new Color(0.8f, 0.8f, 0.8f);
                DrawBladeIconEl(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "シンジ":
                mainColor = new Color(0.5f, 0.3f, 0.9f);
                accentColor = new Color(0.8f, 0.5f, 1f);
                DrawAtomIconEl(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "リョウマ":
                mainColor = new Color(1f, 0.84f, 0f);
                accentColor = new Color(1f, 0.95f, 0.5f);
                DrawCoinIconEl(parent, s, mainColor, accentColor, isSpecial);
                break;
            case "テツヤ":
                mainColor = new Color(0.9f, 0.3f, 0.7f);
                accentColor = new Color(1f, 0.5f, 0.9f);
                DrawMusicIconEl(parent, s, mainColor, accentColor, isSpecial);
                break;
            default:
                mainColor = new Color(1f, 1f, 1f);
                accentColor = new Color(0.8f, 0.8f, 0.8f);
                DrawFistIconEl(parent, s, mainColor, accentColor, isSpecial);
                break;
        }

        if (isSpecial)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                float rad = angle * Mathf.Deg2Rad;
                float dist = 48 * s;
                var ray = BPartEl($"Ray{i}", parent,
                    new Vector2(Mathf.Cos(rad) * dist, Mathf.Sin(rad) * dist),
                    new Vector2(6 * s, 22 * s));
                ray.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(angle - 90, UIE.AngleUnit.Degree)));
                ray.style.backgroundColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.5f);
            }
        }
    }

    void DrawFistIconEl(UIE.VisualElement p, float s, Color main, Color accent, bool sp)
    {
        BPartEl("Shadow", p, new Vector2(2*s, -2*s), new Vector2(50*s, 55*s)).style.backgroundColor = new Color(0,0,0,0.3f);
        BPartEl("Palm", p, Vector2.zero, new Vector2(45*s, 50*s)).style.backgroundColor = main;
        for (int i = 0; i < 4; i++)
        {
            float fx = -14*s + i * 9*s;
            BPartEl($"Knuckle{i}", p, new Vector2(fx, 22*s), new Vector2(8*s, 16*s)).style.backgroundColor = new Color(main.r*0.8f, main.g*0.8f, main.b*0.8f);
        }
        BPartEl("Thumb", p, new Vector2(-20*s, 0), new Vector2(12*s, 20*s)).style.backgroundColor = new Color(main.r*0.9f, main.g*0.85f, main.b*0.85f);
        BPartEl("HL", p, new Vector2(5*s, 8*s), new Vector2(15*s, 20*s)).style.backgroundColor = new Color(1,1,1,0.2f);
        if (sp)
        {
            for (int i = 0; i < 3; i++)
            {
                float lx = 28*s + i*8*s;
                BPartEl($"Impact{i}", p, new Vector2(lx, (10 - i*8)*s), new Vector2(12*s, 3*s)).style.backgroundColor = accent;
            }
        }
    }

    void DrawCircuitIconEl(UIE.VisualElement p, float s, Color main, Color accent, bool sp)
    {
        BPartEl("Board", p, Vector2.zero, new Vector2(50*s, 50*s)).style.backgroundColor = new Color(0.08f, 0.12f, 0.18f);
        for (int i = 0; i < 3; i++)
            BPartEl($"HLine{i}", p, new Vector2(0, (15 - i*15)*s), new Vector2(46*s, 2*s)).style.backgroundColor = main;
        for (int i = 0; i < 3; i++)
            BPartEl($"VLine{i}", p, new Vector2((-15 + i*15)*s, 0), new Vector2(2*s, 46*s)).style.backgroundColor = main;
        float[] nx = {-15, 0, 15, -15, 15};
        float[] ny = {15, 0, 15, -15, -15};
        for (int i = 0; i < nx.Length; i++)
            BPartEl($"Node{i}", p, new Vector2(nx[i]*s, ny[i]*s), new Vector2(7*s, 7*s)).style.backgroundColor = accent;
        BPartEl("Chip", p, Vector2.zero, new Vector2(14*s, 14*s)).style.backgroundColor = sp ? accent : main;
    }

    void DrawBladeIconEl(UIE.VisualElement p, float s, Color main, Color accent, bool sp)
    {
        BPartEl("Shadow", p, new Vector2(2*s, -2*s), new Vector2(12*s, 60*s)).style.backgroundColor = new Color(0,0,0,0.3f);
        BPartEl("Blade", p, new Vector2(0, 8*s), new Vector2(10*s, 55*s)).style.backgroundColor = accent;
        BPartEl("BladeHL", p, new Vector2(-2*s, 8*s), new Vector2(3*s, 50*s)).style.backgroundColor = new Color(1,1,1,0.3f);
        BPartEl("Guard", p, new Vector2(0, -18*s), new Vector2(24*s, 6*s)).style.backgroundColor = main;
        BPartEl("Hilt", p, new Vector2(0, -30*s), new Vector2(7*s, 20*s)).style.backgroundColor = new Color(main.r*0.6f, main.g*0.5f, main.b*0.3f);
        if (sp)
        {
            var slash = BPartEl("Slash", p, new Vector2(0, 10*s), new Vector2(50*s, 4*s));
            slash.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(35, UIE.AngleUnit.Degree)));
            slash.style.backgroundColor = new Color(1, 1, 1, 0.6f);
        }
    }

    void DrawAtomIconEl(UIE.VisualElement p, float s, Color main, Color accent, bool sp)
    {
        BPartEl("Nucleus", p, Vector2.zero, new Vector2(14*s, 14*s)).style.backgroundColor = accent;
        float[] angles = {0, 60, -60};
        foreach (float a in angles)
        {
            var orbit = BPartEl("Orbit", p, Vector2.zero, new Vector2(50*s, 18*s));
            orbit.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(a, UIE.AngleUnit.Degree)));
            orbit.style.backgroundColor = new Color(main.r, main.g, main.b, 0.4f);
        }
        float[] ea = {30, 150, 270};
        for (int i = 0; i < 3; i++)
        {
            float rad = ea[i] * Mathf.Deg2Rad;
            float ex = Mathf.Cos(rad) * 22 * s;
            float ey = Mathf.Sin(rad) * 22 * s;
            BPartEl($"Electron{i}", p, new Vector2(ex, ey), new Vector2(6*s, 6*s)).style.backgroundColor = sp ? accent : main;
        }
    }

    void DrawCoinIconEl(UIE.VisualElement p, float s, Color main, Color accent, bool sp)
    {
        BPartEl("Shadow", p, new Vector2(2*s, -2*s), new Vector2(48*s, 48*s)).style.backgroundColor = new Color(0,0,0,0.3f);
        BPartEl("Outer", p, Vector2.zero, new Vector2(46*s, 46*s)).style.backgroundColor = main;
        BPartEl("Inner", p, Vector2.zero, new Vector2(36*s, 36*s)).style.backgroundColor = new Color(main.r*0.85f, main.g*0.75f, main.b*0.1f);
        // ¥マーク
        var yenEl = new UIE.Label("¥");
        yenEl.name = "Yen";
        yenEl.style.position = UIE.Position.Absolute;
        yenEl.style.left = new UIE.Length(50, UIE.LengthUnit.Percent);
        yenEl.style.top = new UIE.Length(50, UIE.LengthUnit.Percent);
        yenEl.style.translate = new UIE.StyleTranslate(new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-50, UIE.LengthUnit.Percent)));
        yenEl.style.fontSize = Mathf.RoundToInt(24 * s);
        yenEl.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
        yenEl.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        yenEl.style.color = accent;
        UIHelper.ApplyFont(yenEl);
        p.Add(yenEl);
        if (sp)
        {
            float[] sx = {20, -18, 5};
            float[] sy = {18, 20, -22};
            for (int i = 0; i < 3; i++)
                BPartEl($"Spark{i}", p, new Vector2(sx[i]*s, sy[i]*s), new Vector2(5*s, 5*s)).style.backgroundColor = new Color(1,1,1,0.8f);
        }
    }

    void DrawMusicIconEl(UIE.VisualElement p, float s, Color main, Color accent, bool sp)
    {
        BPartEl("NoteHead1", p, new Vector2(-8*s, -12*s), new Vector2(16*s, 12*s)).style.backgroundColor = main;
        BPartEl("NoteHead2", p, new Vector2(12*s, -6*s), new Vector2(16*s, 12*s)).style.backgroundColor = main;
        BPartEl("Stem1", p, new Vector2(-1*s, 10*s), new Vector2(3*s, 38*s)).style.backgroundColor = main;
        BPartEl("Stem2", p, new Vector2(19*s, 16*s), new Vector2(3*s, 38*s)).style.backgroundColor = main;
        BPartEl("Flag", p, new Vector2(9*s, 28*s), new Vector2(22*s, 4*s)).style.backgroundColor = accent;
        BPartEl("Flag2", p, new Vector2(9*s, 22*s), new Vector2(22*s, 4*s)).style.backgroundColor = accent;
        if (sp)
        {
            for (int i = 1; i <= 3; i++)
            {
                var wave = BPartEl($"Wave{i}", p, new Vector2(28*s, (12 - i*6)*s), new Vector2(4*s, (10+i*4)*s));
                wave.style.backgroundColor = new Color(accent.r, accent.g, accent.b, 0.7f - i*0.15f);
            }
        }
    }

    void DrawShieldIconEl(UIE.VisualElement parent, float size)
    {
        float s = size / 120f;
        Color main = new Color(0.3f, 0.5f, 0.8f);
        BPartEl("Shadow", parent, new Vector2(2*s, -2*s), new Vector2(42*s, 50*s)).style.backgroundColor = new Color(0,0,0,0.3f);
        BPartEl("Shield", parent, Vector2.zero, new Vector2(40*s, 48*s)).style.backgroundColor = main;
        BPartEl("Inner", parent, new Vector2(0, 2*s), new Vector2(30*s, 36*s)).style.backgroundColor = new Color(main.r*0.7f, main.g*0.7f, main.b*0.9f);
        BPartEl("CrossH", parent, Vector2.zero, new Vector2(20*s, 5*s)).style.backgroundColor = new Color(1,1,1,0.5f);
        BPartEl("CrossV", parent, Vector2.zero, new Vector2(5*s, 20*s)).style.backgroundColor = new Color(1,1,1,0.5f);
        BPartEl("Tip", parent, new Vector2(0, -22*s), new Vector2(16*s, 10*s)).style.backgroundColor = main;
    }

    void DrawMotherIconEl(UIE.VisualElement parent, string motherName, float size)
    {
        float s = size / 120f;
        switch (motherName)
        {
            case "サクラ":
                BPartEl("Bottle", parent, new Vector2(0, -5*s), new Vector2(20*s, 30*s)).style.backgroundColor = new Color(0.3f, 0.1f, 0.4f);
                BPartEl("Neck", parent, new Vector2(0, 12*s), new Vector2(10*s, 12*s)).style.backgroundColor = new Color(0.3f, 0.1f, 0.4f);
                BPartEl("Cork", parent, new Vector2(0, 20*s), new Vector2(14*s, 6*s)).style.backgroundColor = new Color(0.6f, 0.4f, 0.2f);
                BPartEl("Skull", parent, new Vector2(0, -4*s), new Vector2(10*s, 10*s)).style.backgroundColor = new Color(0.8f, 0.2f, 1f);
                BPartEl("Smoke1", parent, new Vector2(-6*s, 26*s), new Vector2(6*s, 8*s)).style.backgroundColor = new Color(0.6f, 0f, 0.8f, 0.4f);
                BPartEl("Smoke2", parent, new Vector2(4*s, 30*s), new Vector2(8*s, 6*s)).style.backgroundColor = new Color(0.6f, 0f, 0.8f, 0.3f);
                break;
            case "ヒナタ":
                BPartEl("EyeWhite", parent, Vector2.zero, new Vector2(40*s, 22*s)).style.backgroundColor = new Color(0.95f, 0.9f, 0.8f);
                BPartEl("Iris", parent, Vector2.zero, new Vector2(18*s, 18*s)).style.backgroundColor = new Color(0.8f, 0.6f, 0.1f);
                BPartEl("Pupil", parent, Vector2.zero, new Vector2(8*s, 8*s)).style.backgroundColor = new Color(0.1f, 0.05f, 0f);
                BPartEl("HL", parent, new Vector2(-3*s, 3*s), new Vector2(4*s, 4*s)).style.backgroundColor = Color.white;
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    var line = BPartEl($"Pressure{i}", parent, new Vector2(Mathf.Cos(a)*28*s, Mathf.Sin(a)*28*s), new Vector2(10*s, 3*s));
                    line.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(i*60, UIE.AngleUnit.Degree)));
                    line.style.backgroundColor = new Color(1f, 0.7f, 0.1f, 0.5f);
                }
                break;
            case "アキラ":
                var bolt1 = BPartEl("Bolt1", parent, new Vector2(-4*s, 12*s), new Vector2(18*s, 6*s));
                bolt1.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(30, UIE.AngleUnit.Degree)));
                bolt1.style.backgroundColor = new Color(0f, 0.9f, 1f);
                var bolt2 = BPartEl("Bolt2", parent, new Vector2(4*s, 0), new Vector2(18*s, 6*s));
                bolt2.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(-30, UIE.AngleUnit.Degree)));
                bolt2.style.backgroundColor = new Color(0f, 0.9f, 1f);
                var bolt3 = BPartEl("Bolt3", parent, new Vector2(-4*s, -12*s), new Vector2(18*s, 6*s));
                bolt3.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(30, UIE.AngleUnit.Degree)));
                bolt3.style.backgroundColor = new Color(0f, 0.9f, 1f);
                for (int i = 0; i < 3; i++)
                    BPartEl($"Wind{i}", parent, new Vector2(22*s, (10-i*10)*s), new Vector2(14*s, 2*s)).style.backgroundColor = new Color(0.5f, 1f, 1f, 0.5f);
                break;
            case "ミサト":
                for (int i = 1; i <= 3; i++)
                {
                    float sz = i * 16 * s;
                    BPartEl($"Ring{i}", parent, Vector2.zero, new Vector2(sz, sz)).style.backgroundColor = new Color(0.4f, 0.2f, 1f, 0.5f - i*0.1f);
                }
                BPartEl("Core", parent, Vector2.zero, new Vector2(10*s, 10*s)).style.backgroundColor = new Color(0.8f, 0.5f, 1f);
                break;
            case "カエデ":
                BPartEl("CrossH", parent, Vector2.zero, new Vector2(45*s, 15*s)).style.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
                BPartEl("CrossV", parent, Vector2.zero, new Vector2(15*s, 45*s)).style.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
                BPartEl("Heart", parent, new Vector2(0, 15*s), new Vector2(10*s, 10*s)).style.backgroundColor = new Color(1f, 0.3f, 0.4f);
                break;
            case "ルナ":
                float[] starAngles = {0, 72, 144, 216, 288};
                for (int i = 0; i < 5; i++)
                {
                    float rad = starAngles[i] * Mathf.Deg2Rad;
                    float dist = 18 * s;
                    BPartEl($"Star{i}", parent, new Vector2(Mathf.Cos(rad)*dist, Mathf.Sin(rad)*dist), new Vector2(8*s, 8*s))
                        .style.backgroundColor = new Color(1f, 0.9f, 0.5f);
                }
                BPartEl("Center", parent, Vector2.zero, new Vector2(14*s, 14*s)).style.backgroundColor = new Color(1f, 0.7f, 0.9f);
                for (int i = 0; i < 4; i++)
                {
                    float a2 = (i * 90 + 45) * Mathf.Deg2Rad;
                    BPartEl($"Sparkle{i}", parent, new Vector2(Mathf.Cos(a2)*30*s, Mathf.Sin(a2)*30*s), new Vector2(4*s, 4*s))
                        .style.backgroundColor = new Color(1f, 1f, 1f, 0.6f);
                }
                break;
            default:
                BPartEl("Default", parent, Vector2.zero, new Vector2(30*s, 30*s)).style.backgroundColor = Color.white;
                break;
        }
    }

    // ===== カットイン =====

    IEnumerator ShowSkillCutIn(bool isSpecial)
    {
        float iconSize = isSpecial ? 160f : 120f;

        var cutIn = new UIE.VisualElement();
        cutIn.AddToClassList("battle-cutin");
        cutIn.style.width = iconSize;
        cutIn.style.height = iconSize;
        cutIn.style.translate = new UIE.StyleTranslate(new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-50, UIE.LengthUnit.Percent)));

        GenerateSkillIconEl(cutIn, playerFatherName, isSpecial, iconSize * 0.8f);
        root.Add(cutIn);

        float startX = -300f;
        float endX = 300f;

        // Fade in
        float fadeIn = 0.1f;
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeIn;
            cutIn.style.opacity = t;
            float sc = Mathf.Lerp(0.5f, 1f, t);
            cutIn.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(sc, sc)));
            yield return null;
        }
        cutIn.style.opacity = 1f;
        cutIn.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));

        // Slide player → enemy
        float slideDuration = isSpecial ? 0.5f : 0.35f;
        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            float x = Mathf.Lerp(startX, endX, t);
            cutIn.style.translate = new UIE.StyleTranslate(new UIE.Translate(x, new UIE.Length(-50, UIE.LengthUnit.Percent)));
            yield return null;
        }

        // Fade out
        float fadeOut = 0.15f;
        elapsed = 0f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            cutIn.style.opacity = 1f - elapsed / fadeOut;
            yield return null;
        }

        cutIn.RemoveFromHierarchy();
    }

    IEnumerator ShowMotherSkillCutIn()
    {
        var cutIn = new UIE.VisualElement();
        cutIn.AddToClassList("battle-cutin");
        cutIn.style.width = 130;
        cutIn.style.height = 130;
        cutIn.style.translate = new UIE.StyleTranslate(new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), new UIE.Length(-50, UIE.LengthUnit.Percent)));

        DrawMotherIconEl(cutIn, playerMotherName, 100);
        root.Add(cutIn);

        float startX = -300f;
        float endX = 300f;

        float fadeIn = 0.1f;
        float elapsed = 0f;
        while (elapsed < fadeIn)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeIn;
            cutIn.style.opacity = t;
            float sc = Mathf.Lerp(0.5f, 1f, t);
            cutIn.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(sc, sc)));
            yield return null;
        }
        cutIn.style.opacity = 1f;
        cutIn.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));

        float slideDuration = 0.35f;
        elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / slideDuration);
            float x = Mathf.Lerp(startX, endX, t);
            cutIn.style.translate = new UIE.StyleTranslate(new UIE.Translate(x, new UIE.Length(-50, UIE.LengthUnit.Percent)));
            yield return null;
        }

        elapsed = 0f;
        float fadeOut = 0.15f;
        while (elapsed < fadeOut)
        {
            elapsed += Time.deltaTime;
            cutIn.style.opacity = 1f - elapsed / fadeOut;
            yield return null;
        }
        cutIn.RemoveFromHierarchy();
    }

    // ===== 演出ヘルパー =====

    IEnumerator ShakeEffect(UIE.VisualElement target, float duration, float magnitude)
    {
        if (target == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);
            target.style.translate = new UIE.StyleTranslate(new UIE.Translate(x, y));
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
    }

    IEnumerator FlashEffect(Color color, float duration)
    {
        if (flashOverlay == null) yield break;
        flashOverlay.style.opacity = 0.6f;
        flashOverlay.style.backgroundColor = color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            flashOverlay.style.opacity = Mathf.Lerp(0.6f, 0f, elapsed / duration);
            yield return null;
        }
        flashOverlay.style.opacity = 0f;
    }

    IEnumerator AttackAnimation(UIE.VisualElement attacker, UIE.VisualElement target, bool isSpecial)
    {
        if (attacker == null || target == null) yield break;

        // Determine lunge direction: player (left) lunges right, enemy (right) lunges left
        float direction = (attacker == playerPanel) ? 100f : -100f;

        // Lunge forward
        float lungeDuration = 0.1f;
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lungeDuration;
            attacker.style.translate = new UIE.StyleTranslate(new UIE.Translate(direction * t, 0));
            yield return null;
        }

        // Hit flash + shake
        Color flashColor = isSpecial ? new Color(1f, 0.8f, 0f) : Color.white;
        float shakeMagnitude = isSpecial ? 20f : 10f;
        float shakeDuration = isSpecial ? 0.4f : 0.25f;
        StartCoroutine(FlashEffect(flashColor, isSpecial ? 0.4f : 0.2f));
        StartCoroutine(ShakeEffect(target, shakeDuration, shakeMagnitude));

        // Return
        elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lungeDuration;
            attacker.style.translate = new UIE.StyleTranslate(new UIE.Translate(direction * (1f - t), 0));
            yield return null;
        }
        attacker.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));

        yield return new WaitForSeconds(shakeDuration);
    }

    IEnumerator DamageFlash(UIE.VisualElement faceEl)
    {
        if (faceEl == null) yield break;
        for (int i = 0; i < 3; i++)
        {
            faceEl.style.opacity = 0.3f;
            yield return new WaitForSeconds(0.08f);
            faceEl.style.opacity = 1f;
            yield return new WaitForSeconds(0.08f);
        }
    }

    // ===== バトルフロー =====

    IEnumerator BattleStart()
    {
        isBattleActive = true;
        battleLogLabel.text = Localization.Get("battle_enemy_appeared", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(3.0f);

        if (isMale)
            battleLogLabel.text = Localization.Get("battle_boy_power", playerAtk);
        else
            battleLogLabel.text = Localization.Get("battle_girl_power", playerEvasion);
        yield return new WaitForSeconds(3.0f);

        // バトルスタート画像を表示
        battleLogLabel.text = "";
        var startSpr = Resources.Load<Sprite>("UI/battle-start");
        UIE.VisualElement startWrapper = null;
        if (startSpr != null)
        {
            startWrapper = new UIE.VisualElement();
            startWrapper.AddToClassList("battle-image-wrapper");
            startWrapper.pickingMode = UIE.PickingMode.Ignore;
            var startImg = new UIE.VisualElement();
            startImg.AddToClassList("battle-image-center");
            startImg.style.backgroundImage = new UIE.StyleBackground(startSpr);
            startWrapper.Add(startImg);
            root.Add(startWrapper);
        }
        if (vsTextEl != null) vsTextEl.style.display = UIE.DisplayStyle.None;
        yield return new WaitForSeconds(2.0f);
        if (startWrapper != null) startWrapper.RemoveFromHierarchy();

        int playerSpeed = DataCarrier.Instance != null ? DataCarrier.Instance.babyAthletic : 50;
        if (playerSpeed >= enemySpeed)
        {
            battleLogLabel.text = Localization.Get("battle_player_first");
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(PlayerTurn());
        }
        else
        {
            battleLogLabel.text = Localization.Get("battle_enemy_first", Localization.GetEnemy(enemyName));
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator PlayerTurn()
    {
        if (!isBattleActive) yield break;

        isPlayerTurn = true;
        playerDefending = false;

        if (playerPoisonTurns > 0)
        {
            int poisonDmg = Mathf.Max(3, playerMaxHp / 12);
            playerHp = Mathf.Max(0, playerHp - poisonDmg);
            playerPoisonTurns--;
            UpdatePlayerDisplay();
            battleLogLabel.text = Localization.Get("battle_player_poison_damage", poisonDmg);
            yield return new WaitForSeconds(0.8f);
            if (playerHp <= 0)
            {
                StartCoroutine(BattleLose());
                yield break;
            }
        }

        bool isBoss = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBoss && enemyName == "村の王シバ" && (enemyTurnCount + 1) % 3 == 0 && enemyTurnCount > 0)
        {
            battleLogLabel.text = Localization.Get("battle_shiba_charging");
            yield return new WaitForSeconds(1.5f);
        }
        if (isBoss && enemyName == "デヴィル夫人" && (enemyTurnCount + 1) % 4 == 0 && enemyTurnCount > 0)
        {
            battleLogLabel.text = Localization.Get("battle_devil_lady_charge");
            yield return new WaitForSeconds(1.5f);
        }
        if (isBoss && enemyName == "メロディアス女王" && (enemyTurnCount + 1) % 5 == 0 && enemyTurnCount > 0)
        {
            battleLogLabel.text = Localization.Get("battle_melodias_charge");
            yield return new WaitForSeconds(1.5f);
        }

        battleLogLabel.text = Localization.Get("battle_your_turn");
        actionPanelEl.style.display = UIE.DisplayStyle.Flex;
        waitingForAction = true;

        while (waitingForAction)
            yield return null;

        actionPanelEl.style.display = UIE.DisplayStyle.None;
    }

    IEnumerator EnemyTurn()
    {
        if (!isBattleActive) yield break;

        isPlayerTurn = false;
        enemyDefending = false;
        yield return new WaitForSeconds(0.5f);

        if (enemyPoisonTurns > 0)
        {
            int poisonDmg = Mathf.Max(3, enemyMaxHp / 12);
            enemyHp = Mathf.Max(0, enemyHp - poisonDmg);
            enemyPoisonTurns--;
            UpdateEnemyDisplay();
            battleLogLabel.text = Localization.Get("battle_poison_damage", Localization.GetEnemy(enemyName), poisonDmg);
            yield return new WaitForSeconds(0.8f);
            if (enemyHp <= 0)
            {
                StartCoroutine(BattleWin());
                yield break;
            }
        }

        if (enemyDefDebuffTurns > 0) enemyDefDebuffTurns--;
        if (enemyAtkDebuffTurns > 0) enemyAtkDebuffTurns--;
        if (playerEvasionBuffTurns > 0) playerEvasionBuffTurns--;
        if (playerAtkDebuffTurns > 0) playerAtkDebuffTurns--;

        enemyTurnCount++;

        bool isBoss = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBoss && enemyName == "村の王シバ" && enemyTurnCount % 3 == 0)
        {
            yield return StartCoroutine(EnemyDoUltimate());
            yield break;
        }
        if (isBoss && enemyName == "デヴィル夫人" && enemyTurnCount % 4 == 0)
        {
            yield return StartCoroutine(EnemyDoDevilLadyUltimate());
            yield break;
        }
        if (isBoss && enemyName == "メロディアス女王" && enemyTurnCount % 5 == 0)
        {
            yield return StartCoroutine(EnemyDoMelodiasUltimate());
            yield break;
        }

        float hpPercent = (float)enemyHp / enemyMaxHp;
        int roll = Random.Range(0, 100);
        int defendChance = hpPercent < 0.3f ? 35 : 15;
        int specialChance = 25;

        if (roll < defendChance)
            yield return StartCoroutine(EnemyDoDefend());
        else if (roll < defendChance + specialChance)
            yield return StartCoroutine(EnemyDoSpecialAttack());
        else
            yield return StartCoroutine(EnemyDoNormalAttack());
    }

    IEnumerator EnemyDoNormalAttack()
    {
        battleLogLabel.text = Localization.Get("battle_enemy_attack", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, false));

        int effectiveEvasion = playerEvasion + (playerEvasionBuffTurns > 0 ? 20 : 0);
        if (Random.Range(0, 100) < effectiveEvasion)
        {
            battleLogLabel.text = Localization.Get("battle_evaded");
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(PlayerTurn());
            yield break;
        }

        int effectiveEnemyAtk = enemyAtkDebuffTurns > 0 ? (int)(enemyAtk * 0.6f) : enemyAtk;
        int damage = CalculateDamage(effectiveEnemyAtk, playerDef, playerDefending);
        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceMask));

        battleLogLabel.text = playerDefending
            ? Localization.Get("battle_defended", damage)
            : Localization.Get("battle_took_damage", damage);
        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator EnemyDoSpecialAttack()
    {
        battleLogLabel.text = Localization.Get("battle_enemy_special", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(0.6f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, true));

        int effectiveEvasion = Mathf.Max(0, playerEvasion + (playerEvasionBuffTurns > 0 ? 20 : 0) - 10);
        if (Random.Range(0, 100) < effectiveEvasion)
        {
            battleLogLabel.text = Localization.Get("battle_evaded");
            yield return new WaitForSeconds(1.0f);
            StartCoroutine(PlayerTurn());
            yield break;
        }

        int effectiveEnemyAtk = enemyAtkDebuffTurns > 0 ? (int)(enemyAtk * 0.6f) : enemyAtk;
        int specialAtk = (int)(effectiveEnemyAtk * 1.5f);
        int damage = CalculateDamage(specialAtk, playerDef, playerDefending);
        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceMask));

        battleLogLabel.text = Localization.Get("battle_enemy_special_hit", Localization.GetEnemy(enemyName), damage);
        yield return new WaitForSeconds(1.0f);

        if (isDevilEnemy && playerHp > 0)
        {
            int poisonDuration = (enemyName == "デヴィル夫人") ? 4 : 3;
            if (playerPoisonTurns <= 0)
            {
                playerPoisonTurns = poisonDuration;
                battleLogLabel.text = Localization.Get("battle_player_poisoned");
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                battleLogLabel.text = Localization.Get("battle_player_poison_already");
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

        battleLogLabel.text = Localization.Get("battle_enemy_defend_heal", Localization.GetEnemy(enemyName), healAmt);
        yield return new WaitForSeconds(1.2f);

        StartCoroutine(PlayerTurn());
    }

    IEnumerator EnemyDoUltimate()
    {
        battleLogLabel.text = Localization.Get("battle_shiba_ultimate_announce");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(FlashEffect(new Color(1f, 0.2f, 0.1f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogLabel.text = Localization.Get("battle_shiba_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, true));
        StartCoroutine(FlashEffect(new Color(0.8f, 0f, 0f), 0.6f));
        StartCoroutine(ShakeEffect(playerPanel, 0.5f, 25f));

        int ultimatePower = 200;
        int damage = CalculateDamage(ultimatePower, playerDef, playerDefending);

        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceMask));

        battleLogLabel.text = playerDefending
            ? Localization.Get("battle_shiba_ultimate_blocked", damage)
            : Localization.Get("battle_shiba_ultimate_hit", damage);
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator EnemyDoDevilLadyUltimate()
    {
        battleLogLabel.text = Localization.Get("battle_devil_lady_ultimate");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(FlashEffect(new Color(0.6f, 0.0f, 0.8f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogLabel.text = Localization.Get("battle_devil_lady_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, true));
        StartCoroutine(FlashEffect(new Color(0.5f, 0f, 0.6f), 0.6f));
        StartCoroutine(ShakeEffect(playerPanel, 0.5f, 25f));

        int ultimatePower = 200;
        int damage = CalculateDamage(ultimatePower, playerDef, playerDefending);

        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceMask));

        battleLogLabel.text = playerDefending
            ? Localization.Get("battle_devil_lady_ultimate_blocked", damage)
            : Localization.Get("battle_devil_lady_ultimate_hit", damage);
        yield return new WaitForSeconds(1.0f);

        if (playerHp > 0)
        {
            playerPoisonTurns = 5;
            battleLogLabel.text = Localization.Get("battle_devil_lady_poisoned");
            yield return new WaitForSeconds(1.0f);
        }

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator EnemyDoMelodiasUltimate()
    {
        battleLogLabel.text = Localization.Get("battle_melodias_ultimate");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(FlashEffect(new Color(1.0f, 0.4f, 0.7f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogLabel.text = Localization.Get("battle_melodias_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, true));
        StartCoroutine(FlashEffect(new Color(1.0f, 0.84f, 0.0f), 0.6f));
        StartCoroutine(ShakeEffect(playerPanel, 0.5f, 25f));

        int ultimatePower = 180;
        int damage = CalculateDamage(ultimatePower, playerDef, playerDefending);

        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageFlash(playerFaceMask));

        battleLogLabel.text = playerDefending
            ? Localization.Get("battle_melodias_ultimate_blocked", damage)
            : Localization.Get("battle_melodias_ultimate_hit", damage);
        yield return new WaitForSeconds(1.0f);

        if (playerHp > 0)
        {
            playerAtkDebuffTurns = 3;
            battleLogLabel.text = Localization.Get("battle_melodias_debuffed");
            yield return new WaitForSeconds(1.0f);
        }

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator CheckPlayerDefeatAndContinue()
    {
        if (playerHp <= 0)
        {
            if (DataCarrier.Instance != null && DataCarrier.Instance.trait1 == "覇王色" && Random.Range(0, 3) == 0)
            {
                playerHp = 300;
                float hpRatio = Mathf.Clamp01((float)playerHp / playerMaxHp);
                playerHpFill.style.width = new UIE.Length(hpRatio * 100, UIE.LengthUnit.Percent);
                battleLogLabel.text = Localization.Get("battle_conqueror_revive");
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
        if (skillInfoEl != null) { skillInfoEl.RemoveFromHierarchy(); skillInfoEl = null; }
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
        bool isBoss = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBoss || cannotRun)
        {
            actionPanelEl.style.display = UIE.DisplayStyle.None;
            battleLogLabel.text = cannotRun && !isBoss
                ? Localization.Get("battle_run_fixed")
                : Localization.Get("battle_run_boss");
            yield return new WaitForSeconds(1.5f);
            actionPanelEl.style.display = UIE.DisplayStyle.Flex;
            waitingForAction = true;
            yield break;
        }

        actionPanelEl.style.display = UIE.DisplayStyle.None;

        if (Random.Range(0f, 1f) < 0.5f)
        {
            battleLogLabel.text = Localization.Get("battle_run_success");
            yield return new WaitForSeconds(1.2f);

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
            battleLogLabel.text = Localization.Get("battle_run_fail");
            yield return new WaitForSeconds(1.2f);
            battleTurnCount++;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator DoAttack()
    {
        battleLogLabel.text = Localization.Get("battle_punch", normalAttackName);
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(ShowSkillCutIn(false));
        yield return StartCoroutine(AttackAnimation(playerPanel, enemyPanel, false));

        int effectiveDef = enemyDefDebuffTurns > 0 ? (int)(enemyDef * 0.6f) : enemyDef;
        int effectivePlayerAtk = playerAtkDebuffTurns > 0 ? (int)(playerAtk * 0.6f) : playerAtk;
        int damage = CalculateDamage(effectivePlayerAtk, effectiveDef, enemyDefending);
        enemyDefending = false;
        enemyHp = Mathf.Max(0, enemyHp - damage);
        UpdateEnemyDisplay();
        StartCoroutine(DamageFlash(enemyFaceMask));

        battleLogLabel.text = Localization.Get("battle_attack_hit", normalAttackName, Localization.GetEnemy(enemyName), damage);
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
        battleLogLabel.text = Localization.Get("battle_defend_stance");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(EnemyTurn());
    }

    IEnumerator DoMotherAttack()
    {
        battleLogLabel.text = Localization.Get("battle_mother_skill", motherAttackName);
        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(ShowMotherSkillCutIn());

        int effectiveDef = enemyDefDebuffTurns > 0 ? (int)(enemyDef * 0.6f) : enemyDef;
        int effectivePlayerAtk = playerAtkDebuffTurns > 0 ? (int)(playerAtk * 0.6f) : playerAtk;
        int damage = CalculateDamage((int)(effectivePlayerAtk * 0.7f), effectiveDef, enemyDefending);
        enemyDefending = false;

        yield return StartCoroutine(AttackAnimation(playerPanel, enemyPanel, false));

        enemyHp = Mathf.Max(0, enemyHp - damage);
        UpdateEnemyDisplay();
        StartCoroutine(DamageFlash(enemyFaceMask));

        battleLogLabel.text = Localization.Get("battle_mother_damage", motherAttackName, damage);
        yield return new WaitForSeconds(0.8f);

        switch (motherAttackEffect)
        {
            case "heal":
                int healAmt = Mathf.Max(5, playerMaxHp / 8);
                playerHp = Mathf.Min(playerMaxHp, playerHp + healAmt);
                UpdatePlayerDisplay();
                battleLogLabel.text = Localization.Get("battle_heal", healAmt);
                yield return new WaitForSeconds(0.8f);
                break;
            case "poison":
                enemyPoisonTurns = 3;
                battleLogLabel.text = Localization.Get("battle_poison", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(0.8f);
                break;
            case "evasion":
                playerEvasionBuffTurns = 2;
                battleLogLabel.text = Localization.Get("battle_evasion_up");
                yield return new WaitForSeconds(0.8f);
                break;
            case "defdown":
                enemyDefDebuffTurns = 2;
                battleLogLabel.text = Localization.Get("battle_def_down", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(0.8f);
                break;
            case "atkdown":
                enemyAtkDebuffTurns = 2;
                battleLogLabel.text = Localization.Get("battle_atk_down", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(0.8f);
                break;
            case "drain":
                int drainAmt = damage / 3;
                playerHp = Mathf.Min(playerMaxHp, playerHp + drainAmt);
                UpdatePlayerDisplay();
                battleLogLabel.text = Localization.Get("battle_drain", drainAmt);
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
            battleLogLabel.text = Localization.Get("battle_god_special", specialAttackName);
        else
            battleLogLabel.text = Localization.Get("battle_special", specialAttackName);
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(ShowSkillCutIn(true));

        bool hit = Random.Range(0, 100) < (isGodBaby ? 66 : 40);
        if (hit)
        {
            yield return StartCoroutine(AttackAnimation(playerPanel, enemyPanel, true));

            float multiplier = isGodBaby ? 2.5f : 2.0f;
            int effectivePlayerAtk = playerAtkDebuffTurns > 0 ? (int)(playerAtk * 0.6f) : playerAtk;
            int damage = (int)(effectivePlayerAtk * multiplier) + Random.Range(5, 15);
            if (enemyDefending) damage = damage / 2;
            enemyDefending = false;
            enemyHp = Mathf.Max(0, enemyHp - damage);
            UpdateEnemyDisplay();
            StartCoroutine(DamageFlash(enemyFaceMask));

            battleLogLabel.text = isGodBaby
                ? Localization.Get("battle_god_special_hit", specialAttackName, damage)
                : Localization.Get("battle_special_hit", specialAttackName, damage);
        }
        else
        {
            battleLogLabel.text = Localization.Get("battle_special_miss", specialAttackName);
        }

        yield return new WaitForSeconds(1.2f);

        battleLogLabel.text = Localization.Get("battle_fighting_spirit");
        playerAtk = (int)(playerAtk * 1.15f);
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
        battleLogLabel.text = Localization.Get("battle_enemy_defeated", Localization.GetEnemy(enemyName));
        yield return new WaitForSeconds(1.5f);

        if (DataCarrier.Instance != null)
        {
            bool isFirstDefeat = DataCarrier.Instance.AddDefeatedEnemy(enemyName);
            if (isFirstDefeat)
            {
                battleLogLabel.text = Localization.Get("enishi_added", Localization.GetEnemy(enemyName));
                yield return new WaitForSeconds(2f);
            }
        }

        // 勝利画像を表示
        battleLogLabel.text = "";
        var victorySpr = Resources.Load<Sprite>("UI/victory");
        UIE.VisualElement victoryWrapper = null;
        if (victorySpr != null)
        {
            victoryWrapper = new UIE.VisualElement();
            victoryWrapper.AddToClassList("battle-image-wrapper");
            victoryWrapper.pickingMode = UIE.PickingMode.Ignore;
            var victoryImg = new UIE.VisualElement();
            victoryImg.AddToClassList("battle-image-center");
            victoryImg.style.backgroundImage = new UIE.StyleBackground(victorySpr);
            victoryWrapper.Add(victoryImg);
            root.Add(victoryWrapper);
        }
        yield return new WaitForSeconds(1.5f);
        if (victoryWrapper != null) victoryWrapper.RemoveFromHierarchy();

        yield return StartCoroutine(GainExpSequence());

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyCurrentHp = playerHp;
            DataCarrier.Instance.babyPoisonTurns = playerPoisonTurns;
        }

        bool isBossWin = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        bool isFixedWin = cannotRun;
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.SaveData();

        // バトルUIを非表示
        root.style.display = UIE.DisplayStyle.None;

        if (isFixedWin && !isBossWin)
            StartCoroutine(FixedEncounterVictory());
        else if (isBossWin)
            StartCoroutine(BossDefeatSequence());
        else
            StartCoroutine(VictoryToMap());
    }

    IEnumerator GainExpSequence()
    {
        if (DataCarrier.Instance == null) yield break;

        int baseExp = (enemyMaxHp + enemyAtk * 3 + enemyDef * 2) / 4 + battleTurnCount * 3;
        int academic = DataCarrier.Instance.babyAcademic;
        float academicBonus = 1.0f + academic * 0.005f;
        int expGained = Mathf.RoundToInt(baseExp * academicBonus);
        DataCarrier.Instance.babyExp += expGained;
        DataCarrier.Instance.defeatedEnemies++;

        int currentAge = DataCarrier.Instance.babyAge;
        int needed = DataCarrier.ExpForNextAge(currentAge);
        int currentExp = DataCarrier.Instance.babyExp;

        battleLogLabel.text = Localization.Get("battle_exp_gained", expGained);
        yield return new WaitForSeconds(1.2f);

        if (currentExp >= needed)
        {
            int oldAtk = DataCarrier.Instance.babyAtk;
            int oldDef = DataCarrier.Instance.babyDef;
            int oldHp = DataCarrier.Instance.babyHp;
            int oldAcademic = DataCarrier.Instance.babyAcademic;
            int oldAthletic = DataCarrier.Instance.babyAthletic;

            DataCarrier.Instance.babyExp -= needed;
            DataCarrier.Instance.AgeUp();
            int newAge = DataCarrier.Instance.babyAge;

            battleLogLabel.text = Localization.Get("battle_age_up", newAge);
            yield return new WaitForSeconds(1.5f);

            yield return StartCoroutine(ShowStatGrowth(
                oldAtk, oldDef, oldHp, oldAcademic, oldAthletic,
                DataCarrier.Instance.babyAtk, DataCarrier.Instance.babyDef,
                DataCarrier.Instance.babyHp, DataCarrier.Instance.babyAcademic,
                DataCarrier.Instance.babyAthletic
            ));

            playerAtk = DataCarrier.Instance.babyAtk;
            playerDef = DataCarrier.Instance.babyDef;
            playerMaxHp = DataCarrier.Instance.babyHp;
            playerHp = playerMaxHp;
            DataCarrier.Instance.babyCurrentHp = -1;
            playerPoisonTurns = 0;
            DataCarrier.Instance.babyPoisonTurns = 0;

            if (isMale)
                playerAtk = (int)(playerAtk * 1.5f);

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
            int remaining = needed - currentExp;
            battleLogLabel.text = Localization.Get("battle_exp_remaining", remaining, currentExp, needed);

            // EXPバーをbattleLogの下に追加
            var barBg = new UIE.VisualElement();
            barBg.AddToClassList("battle-exp-bar-bg");
            var barFill = new UIE.VisualElement();
            barFill.AddToClassList("battle-exp-bar-fill");
            barFill.style.width = new UIE.Length(0, UIE.LengthUnit.Percent);
            barBg.Add(barFill);
            battleLogEl.Add(barBg);

            float targetRatio = Mathf.Clamp01((float)currentExp / needed);
            float animDuration = 0.8f;
            float animElapsed = 0f;
            while (animElapsed < animDuration)
            {
                animElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, animElapsed / animDuration);
                barFill.style.width = new UIE.Length(t * targetRatio * 100, UIE.LengthUnit.Percent);
                yield return null;
            }
            barFill.style.width = new UIE.Length(targetRatio * 100, UIE.LengthUnit.Percent);

            yield return new WaitForSeconds(1.2f);
            barBg.RemoveFromHierarchy();
        }
    }

    IEnumerator ShowStatGrowth(int oldAtk, int oldDef, int oldHp, int oldAcademic, int oldAthletic,
                                int newAtk, int newDef, int newHp, int newAcademic, int newAthletic)
    {
        int newAge = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 0;
        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "";

        // 全画面オーバーレイ (on root)
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("battle-growth-overlay");

        var (safeTop, safeBottom, _, _) = UIHelper.GetSafeMargins();
        overlay.style.paddingTop = 80 + safeTop;
        overlay.style.paddingBottom = 40 + safeBottom;

        // タイトル
        var title = UIHelper.CreateLabel(Localization.Get("battle_growth_title"), "battle-growth-title");
        overlay.Add(title);

        // 名前 + 月齢
        var subtitle = UIHelper.CreateLabel($"{babyName}    {Localization.GetAge(newAge)}", "battle-growth-subtitle");
        overlay.Add(subtitle);

        // ステータス行定義
        var statDefs = new[] {
            new { label = Localization.Get("battle_stat_hp_label"), oldV = oldHp, newV = newHp, color = new Color(0.88f, 0.27f, 0.27f) },
            new { label = Localization.Get("battle_stat_atk"), oldV = oldAtk, newV = newAtk, color = new Color(0.87f, 0.47f, 0.14f) },
            new { label = Localization.Get("battle_stat_def"), oldV = oldDef, newV = newDef, color = new Color(0.20f, 0.40f, 0.80f) },
            new { label = Localization.Get("battle_stat_athletic"), oldV = oldAthletic, newV = newAthletic, color = new Color(0.13f, 0.67f, 0.33f) },
        };

        var rows = new UIE.VisualElement[statDefs.Length];
        for (int i = 0; i < statDefs.Length; i++)
        {
            var sd = statDefs[i];
            int diff = sd.newV - sd.oldV;

            var row = new UIE.VisualElement();
            row.AddToClassList("battle-growth-row");
            rows[i] = row;

            // Color accent bar
            var bar = new UIE.VisualElement();
            bar.AddToClassList("battle-growth-bar");
            bar.style.backgroundColor = sd.color;
            row.Add(bar);

            // Stat label
            var lbl = UIHelper.CreateLabel(sd.label, "battle-growth-label");
            lbl.style.color = sd.color;
            row.Add(lbl);

            // Old value
            var oldLabel = UIHelper.CreateLabel(sd.oldV.ToString(), "battle-growth-old");
            row.Add(oldLabel);

            // Arrow
            var arrow = UIHelper.CreateLabel("\u2192", "battle-growth-arrow");
            row.Add(arrow);

            // New value
            var newLabel = UIHelper.CreateLabel(sd.newV.ToString(), "battle-growth-new");
            row.Add(newLabel);

            // Diff badge
            if (diff > 0)
            {
                var diffBadge = new UIE.VisualElement();
                diffBadge.AddToClassList("battle-growth-diff");
                diffBadge.style.backgroundColor = new Color(sd.color.r, sd.color.g, sd.color.b, 0.12f);

                var diffText = UIHelper.CreateLabel($"+{diff}", "battle-growth-diff-text");
                diffText.style.color = sd.color;
                diffBadge.Add(diffText);

                row.Add(diffBadge);
            }

            overlay.Add(row);
        }

        root.Add(overlay);

        // Animate rows in
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].AddToClassList("battle-growth-row-visible");
            yield return new WaitForSeconds(0.35f);
        }

        // OKボタン
        bool dismissed = false;
        var okBtn = UIHelper.CreatePillButton("OK", "pill-button-medium");
        okBtn.style.marginTop = 30;
        okBtn.clicked += () => dismissed = true;
        overlay.Add(okBtn);

        // Fade in OK
        okBtn.style.opacity = 0f;
        float okElapsed = 0f;
        while (okElapsed < 0.2f)
        {
            okElapsed += Time.deltaTime;
            okBtn.style.opacity = Mathf.Clamp01(okElapsed / 0.2f);
            yield return null;
        }
        okBtn.style.opacity = 1f;

        while (!dismissed)
            yield return null;

        overlay.RemoveFromHierarchy();

        battleLogLabel.text = Localization.Get("battle_hp_full_heal");
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator BattleLose()
    {
        isBattleActive = false;
        battleLogLabel.text = Localization.Get("battle_defeat");
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

        var retryBtn = UIHelper.CreatePillButton(Localization.Get("ui_back_to_title_long"), "pill-button-medium");
        retryBtn.clicked += () => SceneManager.LoadScene("TitleScene");
        card.Add(retryBtn);

        overlay.Add(card);
        overlayRoot.Add(overlay);
    }

    // ===== 固定エンカウント（傭兵）勝利 =====

    IEnumerator FixedEncounterVictory()
    {
        yield return new WaitForSeconds(1.5f);

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
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

        // 暗転パネル (overlayRoot に追加して他UIの上に)
        var panel = new UIE.VisualElement();
        panel.AddToClassList("battle-boss-panel");
        overlayRoot.Add(panel);

        // Fade to dark via USS transition
        yield return null; // 1フレーム待ってtransition開始
        panel.AddToClassList("battle-boss-panel-dark");
        yield return new WaitForSeconds(0.8f);

        yield return new WaitForSeconds(0.5f);

        // テキストをスライドイン (USS transition使用)
        var lineEls = new UIE.Label[bossLines.Length];
        for (int i = 0; i < bossLines.Length; i++)
        {
            var line = UIHelper.CreateLabel(bossLines[i], "battle-boss-line");
            if (i == 0) line.AddToClassList("battle-boss-line-title");
            panel.Add(line);
            lineEls[i] = line;

            yield return null; // 1フレーム待って transition 開始
            line.AddToClassList("battle-boss-line-visible");

            if (i < bossLines.Length - 1)
                yield return new WaitForSeconds(1.5f + 1.2f);
            else
                yield return new WaitForSeconds(1.5f);
        }

        yield return new WaitForSeconds(2.5f);

        // フェードアウト
        float elapsed = 0f;
        float fadeOutDuration = 1.0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            panel.style.opacity = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        panel.RemoveFromHierarchy();

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.babyCurrentHp = -1;

            if (area == 4 && enemyName == "メロディアス女王")
            {
                DataCarrier.Instance.currentArea = 3;
                DataCarrier.Instance.mapPlayerX = 5;
                DataCarrier.Instance.mapPlayerY = 17;
            }
            else if (area == 2 && enemyName == "デヴィル夫人")
            {
                DataCarrier.Instance.currentArea = 3;
                DataCarrier.Instance.mapPlayerX = 5;
                DataCarrier.Instance.mapPlayerY = 2;
            }
            else
            {
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
        // 勝利パネル (overlayRoot に追加)
        var panel = new UIE.VisualElement();
        panel.AddToClassList("battle-victory-panel");

        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "ベイビー";
        int currentAge = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 0;

        var text = UIHelper.CreateLabel(Localization.Get("battle_victory_return", babyName, currentAge), "battle-victory-text");
        panel.Add(text);
        overlayRoot.Add(panel);

        yield return new WaitForSeconds(1.5f);

        text.text = Localization.Get("battle_returning");
        yield return new WaitForSeconds(1.0f);

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
        PlayerPrefs.SetInt("babyAthletic", dc.babyAthletic);
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
        string original = battleLogLabel.text;
        battleLogLabel.text = Localization.Get("ui_saved");
        yield return new WaitForSeconds(1.2f);
        battleLogLabel.text = original;
    }

    void OnGoTop()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
