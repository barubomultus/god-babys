using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
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
    bool isDevilEnemy;         // ゴージャス・ヴィレッジ敵フラグ
    bool cannotRun;            // 逃走不可フラグ（固定エンカウント）
    Color enemyBgColor = Color.clear; // スプライト無し時の背景色

    // 敵ごとの固有メーター名（HPの呼称）
    static readonly System.Collections.Generic.Dictionary<string, string> EnemyMeterName = new System.Collections.Generic.Dictionary<string, string>
    {
        {"えんえんベイビー", "えんえんど"},
        {"うずうずベイビー", "うずうずど"},
        {"ぷんぷんベイビー", "ぷんぷんど"},
        {"いやだいやだベイビー", "いやいやど"},
        {"どたばたベイビー", "どたばたど"},
    };

    string GetEnemyMeterLabel()
    {
        if (EnemyMeterName.TryGetValue(enemyName, out string meter))
            return meter;
        return "HP";
    }

    // 母親ベースの第2攻撃データ（母親名 → [技名, 説明, 効果タイプ]）
    static readonly System.Collections.Generic.Dictionary<string, string[]> MotherSkillData = new System.Collections.Generic.Dictionary<string, string[]>
    {
        {"サクラ", new[]{"ヒーリングタッチ", "あそびつつ じぶんの ごきげんを かいふくする やさしい わざ", "heal"}},
        {"ヒナタ", new[]{"トゲトゲ・バブル", "おともだちに バブルをかけて、3ターンの あいだ じわじわ まんぞくさせる", "poison"}},
        {"アキラ", new[]{"かぜのステップ", "すばやい うごきで あそび、2ターンの あいだ かいひりょくが あがる", "evasion"}},
        {"ミサト", new[]{"おべんきょうウェーブ", "おともだちの にがてを みつけて、2ターンの あいだ おちつきを さげる", "defdown"}},
        {"カエデ", new[]{"ほんわかオーラ", "ほんわかした ふんいきで、2ターンの あいだ あそびぢからを さげる", "atkdown"}},
        {"ルナ", new[]{"スターダスト", "ほしくずを まとった あそび。まんぞく度の いちぶを ごきげんとして きゅうしゅうする", "drain"}},
    };

    // 36通りの固有技説明（父親名_母親名 → [通常技説明, 必殺技説明]）
    static readonly System.Collections.Generic.Dictionary<string, string[]> SkillDescData = new System.Collections.Generic.Dictionary<string, string[]>
    {
        {"タケシ_サクラ", new[]{"おいしゃさんの ていねいな たかいたかい", "パパのうでと ママの やさしさが あわさった とっておきの あそび"}},
        {"タケシ_ヒナタ", new[]{"ひみつの みえないステップ", "かくれんぼの おうぎ。みつからない！"}},
        {"タケシ_アキラ", new[]{"きんいろに かがやく れんぞくあそび", "オリンピックきゅうの パワーで おおよろこびさせる"}},
        {"タケシ_ミサト", new[]{"りょうしりきがくで よめない たかいたかい", "じゅうりょくすら まげる きゅうきょくの あそび"}},
        {"タケシ_カエデ", new[]{"けいざいりろんで さいてきかされた あそび", "かぶかのように きゅうじょうしょうする たのしさ"}},
        {"タケシ_ルナ", new[]{"ほしぞらの したで ほうつ たかいたかい", "せいざの ちからを やどした きゅうきょくの あそび"}},
        {"ユウキ_サクラ", new[]{"でんしメスで データを なおす", "かんぺきな サイバーしゅじゅつで おともだちを びっくりさせる"}},
        {"ユウキ_ヒナタ", new[]{"こんせきを のこさない ハッキング", "たいしょうの システムを こっそり かきかえる プログラム"}},
        {"ユウキ_アキラ", new[]{"AIが さいてきかした こうそくあそび", "きかいがくしゅうで しんかしつづける あそびパターン"}},
        {"ユウキ_ミサト", new[]{"りょうしコンピュータで にがてを かいせき", "ぎじゅつてき とくいてんを こえた びっくりコード"}},
        {"ユウキ_カエデ", new[]{"フィンテックで けいざいに アクセス", "ぜんせかいの きんゆうを ハッキングする びっくり"}},
        {"ユウキ_ルナ", new[]{"ほしの データを かいせきした あそび", "うちゅうの ソースコードを かきかえる"}},
        {"ゴウ_サクラ", new[]{"ぼうけんじこみの ていねいな あそび", "ぼうけんで みんなを えがおにする てんしの あそび"}},
        {"ゴウ_ヒナタ", new[]{"ぼうけんかと ひみつの あわせわざ", "かげから かげへ、すがたなき ダンサーの さいしゅうおうぎ"}},
        {"ゴウ_アキラ", new[]{"くんれんで きたえた とっしんあそび", "フィールドを かけぬける ぜんりょく スプリントあそび"}},
        {"ゴウ_ミサト", new[]{"せんじゅつてき りょうし きどうの サプライズ", "ふしぎエネルギーを つかった きゅうきょくの びっくり"}},
        {"ゴウ_カエデ", new[]{"ぼうけんけいざいを おうようした あそび", "ぜんりょくを つぎこんだ そうりょくあそび"}},
        {"ゴウ_ルナ", new[]{"ぼうけんの ほしぞらの したで ほうつ あそび", "ほしあかりだけを たよりにした ひみつの おうぎ"}},
        {"シンジ_サクラ", new[]{"ろんりてきに さいてきな ポイントを みつける", "ノーベルしょうきゅうの かんぺきな やさしいタッチ"}},
        {"シンジ_ヒナタ", new[]{"けいさんしつくされた せいかくな ステップ", "IQ300の ずのうが みちびく ひみつの ほうていしき"}},
        {"シンジ_アキラ", new[]{"科学的に最適化された動き", "人体工学の極致による完璧な身体運用"}},
        {"シンジ_ミサト", new[]{"りろんぶつりがくの おうよう あそび", "ちょうげんりろんを じったいかさせた きゅうきょくの びっくり"}},
        {"シンジ_カエデ", new[]{"けいざいりろんに もとづく こうりつてきな あそび", "ノーベルけいざいがくしょうの りろんを たのしさに へんかん"}},
        {"シンジ_ルナ", new[]{"てんたいかんそくデータを おうようした あそび", "うちゅうの しんりを ときあかす びっくり"}},
        {"リョウマ_サクラ", new[]{"さつたばを なげて びっくりさせる", "いりょうビジネスの ぜんしさんを とうにゅうした あそび"}},
        {"リョウマ_ヒナタ", new[]{"おかねの ちからで くりだす あそび", "ひみつの ビジネスの すべてを かけた あそび"}},
        {"リョウマ_アキラ", new[]{"とうしのように かくじつに リターンを える あそび", "きんメダルごと かいしゅうする あっとうてき しきんりょく あそび"}},
        {"リョウマ_ミサト", new[]{"りょうしとうしりろんに もとづく あそび", "むげんの しさんを うむ りろんの びっくりおうよう"}},
        {"リョウマ_カエデ", new[]{"ざいばつの ちからを みせつける れんぞくあそび", "ちょうを こえる しさんで みんなを えがおにする"}},
        {"リョウマ_ルナ", new[]{"セレブの ひんかくで あっとうする", "せかいさいこうほうの とみと びの ゆうごうあそび"}},
        {"テツヤ_サクラ", new[]{"ロックの リズムで ノリノリにする", "ライブかいじょうが えがおで いっぱいになる きゅうきょく パフォーマンス"}},
        {"テツヤ_ヒナタ", new[]{"せいじゃくから ほうつ ロックの しょうげきは", "おとのない ひみつの メロディ"}},
        {"テツヤ_アキラ", new[]{"こうそくビートのような れんぞくあそび", "オリンピックきゅうの ライブパフォーマンスあそび"}},
        {"テツヤ_ミサト", new[]{"りょうしりきがくてきな おんぱ あそび", "きくまで ふしぎな きゅうきょくの うた"}},
        {"テツヤ_カエデ", new[]{"プラチナディスクきゅうの びっくりを あたえる", "おんがくしに きざまれる きゅうきょくの アンセムあそび"}},
        {"テツヤ_ルナ", new[]{"スターの オーラで あっとうする", "ちょうしんせいのように すべてを つつみこむ ハーモニー"}},
    };

    // 36通りの固有技データ（父親名_母親名 → [通常技, 必殺技]）
    static readonly System.Collections.Generic.Dictionary<string, string[]> SkillData = new System.Collections.Generic.Dictionary<string, string[]>
    {
        // タケシ（格闘家）× 各母親
        {"タケシ_サクラ", new[]{"メスタッチ", "おいしゃさんのキングタッチ"}},
        {"タケシ_ヒナタ", new[]{"ひみつのキック", "ほしぞらダンス"}},
        {"タケシ_アキラ", new[]{"ゴールドラッシュ", "オリンピック・スマッシュ"}},
        {"タケシ_ミサト", new[]{"りょうしタッチ", "ブラックホール・ストライク"}},
        {"タケシ_カエデ", new[]{"ゴールデン・タッチ", "黄金のあそび"}},
        {"タケシ_ルナ", new[]{"モデルキック", "カリスマ・インパクト"}},
        // ユウキ（ハッカー）× 各母親
        {"ユウキ_サクラ", new[]{"電脳メス", "サイバー・オペレーション"}},
        {"ユウキ_ヒナタ", new[]{"ステルスハック", "ひみつのプログラム"}},
        {"ユウキ_アキラ", new[]{"データストリーム", "電脳オリンピック"}},
        {"ユウキ_ミサト", new[]{"量子ハッキング", "シンギュラリティ・コード"}},
        {"ユウキ_カエデ", new[]{"マネーウイルス", "ビリオネア・ハック"}},
        {"ユウキ_ルナ", new[]{"バーチャルビーム", "デジタル・オーラ"}},
        // ゴウ（ぼうけんか）× 各母親
        {"ゴウ_サクラ", new[]{"ぼうけんメス", "ぼうけんの天使"}},
        {"ゴウ_ヒナタ", new[]{"ひみつのコンボ", "シャドウ・ダンサー"}},
        {"ゴウ_アキラ", new[]{"ミリタリーダッシュ", "ウォー・スプリント"}},
        {"ゴウ_ミサト", new[]{"タクティカル量子", "タクティカル・ふしぎ"}},
        {"ゴウ_カエデ", new[]{"ぼうけんマネー", "ゴールド・パレード"}},
        {"ゴウ_ルナ", new[]{"カモフラージュ", "ステルス・グラマー"}},
        // シンジ（天才科学者）× 各母親
        {"シンジ_サクラ", new[]{"論理メス", "ノーベル・サージェリー"}},
        {"シンジ_ヒナタ", new[]{"計算キック", "IQひみつじゅつ"}},
        {"シンジ_アキラ", new[]{"物理エンジン", "科学オリンピック"}},
        {"シンジ_ミサト", new[]{"量子もつれ", "ダブルIQ・フュージョン"}},
        {"シンジ_カエデ", new[]{"経済理論", "ノーベルけいざいウェーブ"}},
        {"シンジ_ルナ", new[]{"美の方程式", "相対性オーラ"}},
        // リョウマ（実業家）× 各母親
        {"リョウマ_サクラ", new[]{"札束メス", "メディカル・ビリオン"}},
        {"リョウマ_ヒナタ", new[]{"マネーキック", "ひみつのビジネス"}},
        {"リョウマ_アキラ", new[]{"投資ダッシュ", "ゴールドメダル買収"}},
        {"リョウマ_ミサト", new[]{"量子投資", "無限マネー理論"}},
        {"リョウマ_カエデ", new[]{"帝国コンボ", "トリリオン・エンパイア"}},
        {"リョウマ_ルナ", new[]{"セレブオーラ", "ワールドクラス・リッチ"}},
        // テツヤ（ロックスター）× 各母親
        {"テツヤ_サクラ", new[]{"ロックメス", "ライブ・サージェリー"}},
        {"テツヤ_ヒナタ", new[]{"サイレントロック", "ひみつのセレナーデ"}},
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
    UIE.VisualElement commandRow, submenuEl, bottomBar;
    UIE.Button autoBattleBtn;
    bool isAutoBattle;
    UIE.VisualElement interactionBlocker;
    UIE.VisualElement flashOverlay;
    UIE.VisualElement battleLogEl;
    UIE.VisualElement battleBgEl, enemyAreaEl, playerAreaEl, centerAreaEl;

    // UI Toolkit — オーバーレイ
    UIE.PanelSettings overlayPanelSettings;
    UIE.VisualElement overlayRoot;
    UIE.VisualElement menuOverlayEl;
    UIE.VisualElement skillInfoEl;

    // オーディオ
    AudioSource bgmSource;
    AudioSource seSource;
    AudioClip seTap;

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

        // CreateMenuBar(); // メニュー無効化

        // 前シーンの残留BGMを全停止
        foreach (var src in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            src.Stop();

        // オーディオ初期化
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = 0.5f;
        var bgmClip = Resources.Load<AudioClip>("BGM/Battle_Bgm");
        if (bgmClip != null) { bgmSource.clip = bgmClip; }

        seSource = gameObject.AddComponent<AudioSource>();
        seTap = Resources.Load<AudioClip>("SE/SE_Tap");

        StartCoroutine(BattleStart());
    }

    void OnDestroy()
    {
        if (mainPanelSettings != null)
            Destroy(mainPanelSettings);
        if (overlayPanelSettings != null)
            Destroy(overlayPanelSettings);
        // マップスクショのメモリ解放
        if (DataCarrier.Instance != null && DataCarrier.Instance.battleBgTexture != null)
        {
            Destroy(DataCarrier.Instance.battleBgTexture);
            DataCarrier.Instance.battleBgTexture = null;
        }
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

            // ショップ装備ボーナス（装備スロットに入っているもののみ）
            if (DataCarrier.Instance.IsEquipped("ガラガラソード")) playerAtk += 4;
            if (DataCarrier.Instance.IsEquipped("よだれかけシールド")) playerDef += 4;
            if (DataCarrier.Instance.IsEquipped("魔法のおむつ")) playerDef += 3;
            if (DataCarrier.Instance.IsEquipped("黄金のほ乳瓶")) { playerAtk += 3; playerDef += 3; }
            if (DataCarrier.Instance.IsEquipped("悪魔のティアラ")) { playerAtk += 6; playerDef -= 2; }
            if (DataCarrier.Instance.IsEquipped("泣き猫パンチ")) playerAtk += 2;
            if (DataCarrier.Instance.IsEquipped("ミニよだれかけ")) playerDef += 2;
            if (DataCarrier.Instance.IsEquipped("にじいろガラガラ")) { playerAtk += 5; playerDef += 2; }

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
            enemyMaxHp = 580;
            enemyHp = enemyMaxHp;
            enemyAtk = 85;
            enemyDef = 36;
            enemySpeed = 68;
            isDevilEnemy = true;
            enemyBgColor = new Color(0.3f, 0.0f, 0.2f);
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/boss/devil-wife");
        }
        else if (fromMap && bossBattle)
        {
            enemyName = "青年のシバ";
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
            enemyName = "ぷんぷんベイビー";
            enemyAge = -1;
            enemyMaxHp = 108;
            enemyHp = enemyMaxHp;
            enemyAtk = 35;
            enemyDef = 31;
            enemySpeed = 31;
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/first-enemy");
        }
    }

    void InitializeRandomEnemy(int playerAge)
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        if (area == 0)
        {
            int minAge = Mathf.Max(0, playerAge - 2);
            int maxAge = Mathf.Min(15, playerAge + 4);
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
                new object[]{ "にがにがベイビー",     Resources.Load<Sprite>("EnemyBabys/poison/doku-baby"),  80, 22, 13, 22,  6,  9, new Color(0.4f, 0.1f, 0.5f) },
                new object[]{ "ぐちぐちベイビー",   Resources.Load<Sprite>("EnemyBabys/poison/noroi-baby"), 90, 27, 18, 20,  7, 10, new Color(0.3f, 0.0f, 0.3f) },
                new object[]{ "どよよんベイビー",     Resources.Load<Sprite>("EnemyBabys/poison/yami-baby"), 100, 32, 20, 27,  8, 11, new Color(0.15f, 0.05f, 0.2f) },
                new object[]{ "つんつんベイビー",   Resources.Load<Sprite>("EnemyBabys/poison/akuma-baby"), 115, 38, 25, 32,  9, 13, new Color(0.5f, 0.0f, 0.1f) },
                new object[]{ "いじいじベイビー", Resources.Load<Sprite>("EnemyBabys/poison/jyaaku-baby"), 135, 43, 29, 34, 11, 14, new Color(0.2f, 0.0f, 0.0f) },
                new object[]{ "ごーじゃすベイビー",   Resources.Load<Sprite>("EnemyBabys/poison/maou-baby"), 160, 50, 34, 38, 13, 15, new Color(0.1f, 0.0f, 0.15f) },
            };
        }
        else
        {
            enemyDefs = new object[][] {
                new object[]{ "えんえんベイビー",       "EnemyBabys/common-nakimushi",    60, 15, 10, 20, 0, 99 },
                new object[]{ "うずうずベイビー",       "EnemyBabys/common-yantya",       75, 22, 12, 30, 0, 99 },
                new object[]{ "ぷんぷんベイビー",       "EnemyBabys/first-enemy",         85, 28, 18, 25, 0, 99 },
                new object[]{ "いやだいやだベイビー",   "EnemyBabys/common-wagamama",    100, 32, 22, 28, 0, 99 },
                new object[]{ "どたばたベイビー",       "EnemyBabys/common-abarennbou",  120, 40, 25, 35, 0, 99 },
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
            if ((cName == "ごーじゃすベイビー" || cName == "小悪魔れい") && Random.value > 0.03f && candidates.Count > 1)
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

        enemyMaxHp = 250;
        enemyHp = enemyMaxHp;
        enemyAtk = 54;
        enemyDef = 32;
        enemySpeed = 50;
        enemyBgColor = new Color(0.35f, 0.05f, 0.05f);
    }

    // ===== バトルUI作成 (UI Toolkit) =====

    void BuildBattleUI()
    {
        var (safeTop, safeBottom, _, _) = UIHelper.GetSafeMargins();

        var battleRoot = new UIE.VisualElement();
        battleRoot.AddToClassList("battle-root");
        root.Add(battleRoot);

        // 背景（マップスクショがあれば使用 — ぼかし＋暗く）
        battleBgEl = new UIE.VisualElement();
        battleBgEl.AddToClassList("battle-bg");
        battleBgEl.pickingMode = UIE.PickingMode.Ignore;
        if (DataCarrier.Instance != null && DataCarrier.Instance.battleBgTexture != null)
        {
            var blurred = BlurTexture(DataCarrier.Instance.battleBgTexture, 4);
            var sprite = Sprite.Create(blurred, new Rect(0, 0, blurred.width, blurred.height), new Vector2(0.5f, 0.5f));
            battleBgEl.style.backgroundImage = new UIE.StyleBackground(sprite);
        }
        battleBgEl.style.opacity = 0;
        battleRoot.Add(battleBgEl);

        // フラッシュオーバーレイ
        flashOverlay = new UIE.VisualElement();
        flashOverlay.AddToClassList("battle-flash");
        flashOverlay.pickingMode = UIE.PickingMode.Ignore;
        battleRoot.Add(flashOverlay);

        // 敵エリア（上）
        enemyAreaEl = new UIE.VisualElement();
        enemyAreaEl.AddToClassList("battle-enemy-area");
        enemyAreaEl.style.paddingTop = safeTop + 30;
        enemyAreaEl.style.opacity = 0;
        battleRoot.Add(enemyAreaEl);
        enemyPanel = BuildCharPanel(enemyAreaEl, false);

        // 中央エリア（VS + ログ）
        centerAreaEl = new UIE.VisualElement();
        centerAreaEl.AddToClassList("battle-center");
        centerAreaEl.style.opacity = 0;
        battleRoot.Add(centerAreaEl);

        vsTextEl = UIHelper.CreateLabel("VS", "battle-vs");
        centerAreaEl.Add(vsTextEl);

        battleLogEl = new UIE.VisualElement();
        battleLogEl.AddToClassList("battle-log");
        centerAreaEl.Add(battleLogEl);

        battleLogLabel = UIHelper.CreateLabel("", "battle-log-text");
        battleLogEl.Add(battleLogLabel);

        // プレイヤーエリア（下）
        playerAreaEl = new UIE.VisualElement();
        playerAreaEl.AddToClassList("battle-player-area");
        playerAreaEl.style.opacity = 0;
        battleRoot.Add(playerAreaEl);
        playerPanel = BuildCharPanel(playerAreaEl, true);

        // 上部バー（にげる左端 / オートバトル右端）— アクションエリアの上
        bottomBar = new UIE.VisualElement();
        bottomBar.AddToClassList("battle-top-bar");
        battleRoot.Add(bottomBar);

        var runBtn = new UIE.Button();
        runBtn.AddToClassList("battle-run-pill");
        UIHelper.ApplyFont(runBtn);
        runBtn.text = "にげる";
        runBtn.clicked += OnRun;
        bottomBar.Add(runBtn);

        autoBattleBtn = new UIE.Button();
        autoBattleBtn.AddToClassList("battle-auto-pill");
        autoBattleBtn.AddToClassList("battle-auto-off");
        UIHelper.ApplyFont(autoBattleBtn);
        autoBattleBtn.text = "AUTO \u25B6\u25B6";
        autoBattleBtn.clicked += ToggleAutoBattle;
        bottomBar.Add(autoBattleBtn);

        // アクションエリア（画面下端に固定）
        actionPanelEl = new UIE.VisualElement();
        actionPanelEl.AddToClassList("battle-action-area");
        actionPanelEl.style.paddingBottom = safeBottom + 16;
        battleRoot.Add(actionPanelEl);

        // コマンド行（4ボタン横並び）
        commandRow = new UIE.VisualElement();
        commandRow.AddToClassList("battle-cmd-row");
        actionPanelEl.Add(commandRow);

        BuildCmdButton(commandRow, "こうげき", OnAttack);
        BuildCmdButton(commandRow, "スキル", ShowSkillSubmenu);
        BuildCmdButton(commandRow, "どうぐ", ShowItemSubmenu);
        BuildCmdButton(commandRow, "ぼうぎょ", OnDefend);

        // サブメニュー（初期非表示）
        submenuEl = new UIE.VisualElement();
        submenuEl.AddToClassList("battle-submenu");
        submenuEl.style.display = UIE.DisplayStyle.None;
        actionPanelEl.Add(submenuEl);

        // top-bar の bottom を actionPanelEl の高さに合わせる
        actionPanelEl.RegisterCallback<UIE.GeometryChangedEvent>(evt =>
        {
            bottomBar.style.bottom = evt.newRect.height;
        });

        // インタラクションブロッカー
        interactionBlocker = new UIE.VisualElement();
        interactionBlocker.style.position = UIE.Position.Absolute;
        interactionBlocker.style.left = 0;
        interactionBlocker.style.top = 0;
        interactionBlocker.style.right = 0;
        interactionBlocker.style.bottom = 0;
        interactionBlocker.pickingMode = UIE.PickingMode.Position;

        actionPanelEl.style.display = UIE.DisplayStyle.None;
        bottomBar.style.display = UIE.DisplayStyle.None;

        // 初期表示
        UpdatePlayerDisplay();
        UpdateEnemyDisplay();
    }

    UIE.VisualElement BuildCharPanel(UIE.VisualElement area, bool isPlayer)
    {
        var panel = new UIE.VisualElement();
        panel.AddToClassList("battle-char-panel");
        if (isPlayer) panel.AddToClassList("battle-char-card");
        area.Add(panel);

        // 情報カラム（名前+年齢, HPバー, HPテキスト）
        var infoCol = new UIE.VisualElement();
        infoCol.AddToClassList("battle-info-col");
        if (!isPlayer) infoCol.AddToClassList("battle-info-col-enemy");

        var nameAgeRow = new UIE.VisualElement();
        nameAgeRow.AddToClassList("battle-name-age-row");
        infoCol.Add(nameAgeRow);

        var nameLabel = UIHelper.CreateLabel("", "battle-char-name");
        nameLabel.AddToClassList(isPlayer ? "battle-name-player" : "battle-name-enemy");
        nameAgeRow.Add(nameLabel);
        if (isPlayer) playerNameLabel = nameLabel; else enemyNameLabel = nameLabel;

        var ageLabel = UIHelper.CreateLabel("", "battle-age-label");
        nameAgeRow.Add(ageLabel);
        if (isPlayer) playerAgeEl = ageLabel; else enemyAgeEl = ageLabel;

        var hpBg = new UIE.VisualElement();
        hpBg.AddToClassList("battle-hp-bg");
        infoCol.Add(hpBg);

        var hpFill = new UIE.VisualElement();
        hpFill.AddToClassList("battle-hp-fill");
        hpFill.AddToClassList(isPlayer ? "battle-hp-fill-player" : "battle-hp-fill-enemy");
        hpFill.style.width = new UIE.Length(100, UIE.LengthUnit.Percent);
        hpBg.Add(hpFill);
        if (isPlayer) playerHpFill = hpFill; else enemyHpFill = hpFill;

        var hpLabel = UIHelper.CreateLabel("", "battle-hp-text");
        infoCol.Add(hpLabel);
        if (isPlayer) playerHpLabel = hpLabel; else enemyHpLabel = hpLabel;

        // 顔マスク
        var faceMask = new UIE.VisualElement();
        faceMask.AddToClassList("battle-face-mask");
        if (!isPlayer) faceMask.AddToClassList("battle-face-mask-enemy");
        if (isPlayer) playerFaceMask = faceMask; else enemyFaceMask = faceMask;

        var faceEl = new UIE.VisualElement();
        faceEl.name = "Face";
        faceEl.style.position = UIE.Position.Absolute;
        faceEl.style.left = 0; faceEl.style.top = 0;
        faceEl.style.right = 0; faceEl.style.bottom = 0;
        faceMask.Add(faceEl);
        if (isPlayer) playerFaceEl = faceEl; else enemyFaceEl = faceEl;

        // 画像(上) → 情報(下)
        panel.Add(faceMask);
        panel.Add(infoCol);

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

    void BuildCmdButton(UIE.VisualElement parent, string label, System.Action onClick)
    {
        var btn = new UIE.Button();
        btn.AddToClassList("battle-cmd-btn");
        UIHelper.ApplyFont(btn);
        btn.text = label;
        btn.clicked += () =>
        {
            if (seSource != null && seTap != null)
            {
                seSource.pitch = Random.Range(0.96f, 1.08f);
                seSource.PlayOneShot(seTap, 0.7f);
            }
            onClick();
        };
        parent.Add(btn);
    }

    void ShowSkillSubmenu()
    {
        if (!waitingForAction) return;
        commandRow.style.display = UIE.DisplayStyle.None;
        submenuEl.Clear();
        submenuEl.style.display = UIE.DisplayStyle.Flex;

        // 通常攻撃
        BuildSubmenuItem(submenuEl, normalAttackName, normalAttackDesc, OnAttack);
        // 母スキル
        BuildSubmenuItem(submenuEl, motherAttackName, motherAttackDesc, OnMotherAttack);
        // 必殺技
        BuildSubmenuItem(submenuEl, specialAttackName, specialAttackDesc, OnSpecial);
        // もどる
        var backBtn = new UIE.Button();
        backBtn.AddToClassList("battle-submenu-item");
        backBtn.AddToClassList("battle-submenu-back");
        UIHelper.ApplyFont(backBtn);
        backBtn.text = "もどる";
        backBtn.clicked += CloseSubmenu;
        submenuEl.Add(backBtn);
    }

    void ShowItemSubmenu()
    {
        if (!waitingForAction) return;
        commandRow.style.display = UIE.DisplayStyle.None;
        submenuEl.Clear();
        submenuEl.style.display = UIE.DisplayStyle.Flex;

        var emptyLabel = UIHelper.CreateLabel("アイテムがない", "battle-submenu-empty");
        submenuEl.Add(emptyLabel);

        var backBtn = new UIE.Button();
        backBtn.AddToClassList("battle-submenu-item");
        backBtn.AddToClassList("battle-submenu-back");
        UIHelper.ApplyFont(backBtn);
        backBtn.text = "もどる";
        backBtn.clicked += CloseSubmenu;
        submenuEl.Add(backBtn);
    }

    void BuildSubmenuItem(UIE.VisualElement parent, string skillName, string desc, System.Action onClick)
    {
        var btn = new UIE.Button();
        btn.AddToClassList("battle-submenu-item");
        UIHelper.ApplyFont(btn);
        parent.Add(btn);

        var nameLabel = UIHelper.CreateLabel(skillName, "battle-submenu-item-name");
        btn.Add(nameLabel);

        var descLabel = UIHelper.CreateLabel(desc, "battle-submenu-item-desc");
        btn.Add(descLabel);

        btn.clicked += () =>
        {
            if (seSource != null && seTap != null)
            {
                seSource.pitch = Random.Range(0.96f, 1.08f);
                seSource.PlayOneShot(seTap, 0.7f);
            }
            CloseSubmenu();
            onClick();
        };
    }

    void CloseSubmenu()
    {
        submenuEl.style.display = UIE.DisplayStyle.None;
        submenuEl.Clear();
        commandRow.style.display = UIE.DisplayStyle.Flex;
    }

    void ToggleAutoBattle()
    {
        isAutoBattle = !isAutoBattle;
        if (isAutoBattle)
        {
            autoBattleBtn.RemoveFromClassList("battle-auto-off");
            autoBattleBtn.AddToClassList("battle-auto-on");
            autoBattleBtn.text = "AUTO：ON";
            // 現在ターン中なら即座に攻撃
            if (waitingForAction) OnAttack();
        }
        else
        {
            autoBattleBtn.RemoveFromClassList("battle-auto-on");
            autoBattleBtn.AddToClassList("battle-auto-off");
            autoBattleBtn.text = "AUTO \u25B6\u25B6";
        }
    }

    void SetInteractionEnabled(bool enabled)
    {
        if (enabled)
        {
            interactionBlocker.RemoveFromHierarchy();
        }
        else
        {
            if (interactionBlocker.parent == null)
                overlayRoot.Add(interactionBlocker);
        }
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
                    babyName = gender == "男の子" ? "STAR BOY" : "STAR GIRL";
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

        // 最優先: 合成画像（おくるみ+顔切り抜き）
        Sprite synthSprite = DataCarrier.LoadSynthBabySprite();
        if (synthSprite != null)
        {
            ClearFaceParts(faceEl);
            faceEl.style.backgroundImage = new UIE.StyleBackground(synthSprite);
            faceEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            if (DataCarrier.Instance.isGodBaby)
                AddGodAuraEl(faceEl);
            return true;
        }

        // フォールバック: Resources/babys/ のプリセット画像
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
            case "ゼニガタ": return "zenigata";
            case "ツクモ": return "tukumo";
            case "サトウ": return "satou";
            case "イワオ": return "iwao";
            case "アキトシ": return "akitoshi";
            case "ネオ": return "neo";
            case "イザナミ": return "izanami";
            case "ミク": return "miku";
            case "カヨコ": return "kayoko";
            case "フクトク": return "hukutoku";
            case "ヨネ": return "yone";
            case "ドクコ": return "dokuko";
            default: return japaneseName.ToLower();
        }
    }

    void UpdateEnemyDisplay()
    {
        enemyNameLabel.text = Localization.GetEnemy(enemyName);
        if (enemyAgeEl != null)
            enemyAgeEl.text = enemyAge >= 0 ? Localization.GetAge(enemyAge) : "";
        string meterLabel = GetEnemyMeterLabel();
        enemyHpLabel.text = $"{meterLabel}:{enemyHp}/{enemyMaxHp}";

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

        int atk = 50, intelligence = 60, athletic = 60;
        string gender = "男の子";
        bool godBaby = false;

        if (DataCarrier.Instance != null)
        {
            atk = DataCarrier.Instance.babyAtk;
            intelligence = DataCarrier.Instance.babyIntelligence;
            athletic = DataCarrier.Instance.babyAthletic;
            gender = DataCarrier.Instance.babyGender;
            godBaby = DataCarrier.Instance.isGodBaby;
        }

        float s = 1.0f;

        Color[] skinTones = { new Color(0.98f, 0.89f, 0.82f), new Color(0.95f, 0.83f, 0.74f), new Color(0.88f, 0.73f, 0.62f) };
        Color skin = skinTones[Random.Range(0, skinTones.Length)];
        Color skinShadow = new Color(skin.r * 0.85f, skin.g * 0.82f, skin.b * 0.8f);

        Color[] hairTones = { new Color(0.08f, 0.06f, 0.05f), new Color(0.2f, 0.12f, 0.08f), new Color(0.35f, 0.22f, 0.12f), new Color(0.55f, 0.38f, 0.2f) };
        Color hair = hairTones[Mathf.Clamp(intelligence / 25, 0, 3)];
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

    IEnumerator IntroFlashEffect()
    {
        if (flashOverlay == null) yield break;
        flashOverlay.style.backgroundColor = Color.white;
        flashOverlay.style.opacity = 1f;
        yield return null;
        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            flashOverlay.style.opacity = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        flashOverlay.style.opacity = 0f;
    }

    IEnumerator FadeOutBGM(float duration = 1.5f)
    {
        if (bgmSource == null || !bgmSource.isPlaying) yield break;
        float startVol = bgmSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }
        bgmSource.volume = 0f;
        bgmSource.Stop();
    }

    IEnumerator BounceInEffect(UIE.VisualElement el)
    {
        if (el == null) yield break;

        // Phase 1: 上から落下 (0.35s) — opacity + translate + scale
        float dropDuration = 0.35f;
        float elapsed = 0f;
        float startY = -300f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            // ease-in (加速)
            float eased = t * t;
            float y = Mathf.Lerp(startY, 0f, eased);
            float s = Mathf.Lerp(0.3f, 1f, eased);
            el.style.opacity = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t * 2f));
            el.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, y));
            el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
            yield return null;
        }

        // Phase 2: 着地スクワッシュ (0.1s) — 横に潰れる
        elapsed = 0f;
        float squashDuration = 0.1f;
        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / squashDuration;
            float sx = Mathf.Lerp(1f, 1.25f, t);
            float sy = Mathf.Lerp(1f, 0.75f, t);
            el.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
            el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(sx, sy, 1f)));
            yield return null;
        }

        // Phase 3: バウンス戻り (0.15s) — 縦に伸びて跳ねる
        elapsed = 0f;
        float stretchDuration = 0.15f;
        while (elapsed < stretchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stretchDuration;
            float sx = Mathf.Lerp(1.25f, 0.9f, t);
            float sy = Mathf.Lerp(0.75f, 1.1f, t);
            float y = Mathf.Lerp(0f, -40f, Mathf.Sin(t * Mathf.PI));
            el.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, y));
            el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(sx, sy, 1f)));
            yield return null;
        }

        // Phase 4: 小バウンス (0.12s) — 軽い着地
        elapsed = 0f;
        float settleDuration = 0.12f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / settleDuration;
            float sx = Mathf.Lerp(0.9f, 1.05f, t);
            float sy = Mathf.Lerp(1.1f, 0.97f, t);
            float y = Mathf.Lerp(0f, -10f, Mathf.Sin(t * Mathf.PI));
            el.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, y));
            el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(sx, sy, 1f)));
            yield return null;
        }

        // Phase 5: 安定 (0.08s)
        elapsed = 0f;
        float restoreDuration = 0.08f;
        while (elapsed < restoreDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / restoreDuration;
            float sx = Mathf.Lerp(1.05f, 1f, t);
            float sy = Mathf.Lerp(0.97f, 1f, t);
            el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(sx, sy, 1f)));
            yield return null;
        }

        el.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
        el.style.scale = new UIE.StyleScale(new UIE.Scale(Vector3.one));
        el.style.opacity = 1f;
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

    IEnumerator DamageEffect(UIE.VisualElement faceEl, UIE.VisualElement panel, bool isHeavy = false)
    {
        if (faceEl == null) yield break;

        // 画面フラッシュ（赤）
        float flashOpacity = isHeavy ? 0.5f : 0.3f;
        if (flashOverlay != null)
        {
            flashOverlay.style.backgroundColor = new Color(1f, 0.1f, 0.1f);
            flashOverlay.style.opacity = flashOpacity;
        }

        // パネルシェイク
        if (panel != null)
            StartCoroutine(ShakeEffect(panel, isHeavy ? 0.3f : 0.2f, isHeavy ? 15f : 8f));

        // スプライト赤点滅（tintColor + opacity フリッカー）
        for (int i = 0; i < 3; i++)
        {
            faceEl.style.unityBackgroundImageTintColor = new Color(1f, 0.3f, 0.3f);
            faceEl.style.opacity = 0.4f;
            yield return new WaitForSeconds(0.08f);
            faceEl.style.unityBackgroundImageTintColor = Color.white;
            faceEl.style.opacity = 1f;
            yield return new WaitForSeconds(0.08f);
        }

        // フラッシュフェードアウト
        if (flashOverlay != null)
        {
            float elapsed = 0f;
            float fadeDur = 0.15f;
            while (elapsed < fadeDur)
            {
                elapsed += Time.deltaTime;
                flashOverlay.style.opacity = Mathf.Lerp(flashOpacity * 0.5f, 0f, elapsed / fadeDur);
                yield return null;
            }
            flashOverlay.style.opacity = 0f;
        }
    }

    // ===== カード粉砕演出 =====

    IEnumerator NikonikoDefeatEffect(UIE.VisualElement panel)
    {
        if (panel == null) yield break;

        var parent = panel.parent;
        if (parent == null) yield break;

        // パネルの位置・サイズを保存
        var layout = panel.layout;
        float panelCenterX = layout.x + layout.width * 0.5f;
        float panelCenterY = layout.y + layout.height * 0.5f;

        // --- Step 1: ホワイトアウト (0.4s) ---
        // 白いオーバーレイをパネル内に追加して徐々に不透明にする
        var whiteOverlay = new UIE.VisualElement();
        whiteOverlay.pickingMode = UIE.PickingMode.Ignore;
        whiteOverlay.style.position = UIE.Position.Absolute;
        whiteOverlay.style.left = 0; whiteOverlay.style.top = 0;
        whiteOverlay.style.right = 0; whiteOverlay.style.bottom = 0;
        whiteOverlay.style.backgroundColor = new Color(1f, 0.96f, 0.88f, 0f); // パステルゴールド
        whiteOverlay.style.borderTopLeftRadius = 40;
        whiteOverlay.style.borderTopRightRadius = 40;
        whiteOverlay.style.borderBottomLeftRadius = 40;
        whiteOverlay.style.borderBottomRightRadius = 40;
        panel.Add(whiteOverlay);

        float whiteDur = 0.4f;
        float elapsed = 0f;
        while (elapsed < whiteDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / whiteDur);
            float ease = t * t; // ease-in
            whiteOverlay.style.backgroundColor = new Color(1f, 0.96f, 0.88f, ease);
            yield return null;
        }
        whiteOverlay.style.backgroundColor = new Color(1f, 0.96f, 0.88f, 1f);
        yield return new WaitForSeconds(0.15f);

        // --- Step 2: ぽんぽんバウンス (3回) ---
        float[] bounceHeights = { -30f, -20f, -12f };
        for (int i = 0; i < bounceHeights.Length; i++)
        {
            float bounceDur = 0.15f;
            elapsed = 0f;
            // 上へ
            while (elapsed < bounceDur)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / bounceDur);
                float ease = Mathf.Sin(t * Mathf.PI); // sin曲線で自然なバウンス
                panel.style.translate = new UIE.StyleTranslate(
                    new UIE.Translate(0, bounceHeights[i] * ease));
                yield return null;
            }
            panel.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
        }

        yield return new WaitForSeconds(0.1f);

        // --- Step 3 & 4: パーティクル噴出 + 昇天 (同時進行) ---
        // パーティクルコンテナ（parent上に配置）
        var particleContainer = new UIE.VisualElement();
        particleContainer.pickingMode = UIE.PickingMode.Ignore;
        particleContainer.style.position = UIE.Position.Absolute;
        particleContainer.style.left = 0; particleContainer.style.top = 0;
        particleContainer.style.right = 0; particleContainer.style.bottom = 0;
        particleContainer.style.overflow = UIE.Overflow.Visible;
        parent.Add(particleContainer);

        // パーティクルを生成
        string[] symbols = { "\u2665", "\u2605", "\u2665", "\u2606", "\u2665", "\u2605" }; // ♥ ★
        Color[] particleColors = {
            new Color(1f, 0.44f, 0.56f),  // ピンク
            new Color(1f, 0.84f, 0.3f),   // ゴールド
            new Color(0.67f, 0.94f, 0.82f), // ミント
            new Color(1f, 0.72f, 0.77f),  // ライトピンク
            new Color(1f, 0.92f, 0.5f),   // ライトゴールド
        };

        int particleCount = 18;
        var particles = new List<(UIE.Label el, float px, float py, float vx, float vy, float rotSpeed)>();

        for (int i = 0; i < particleCount; i++)
        {
            var p = new UIE.Label();
            p.pickingMode = UIE.PickingMode.Ignore;
            p.text = symbols[i % symbols.Length];
            p.style.position = UIE.Position.Absolute;
            p.style.fontSize = Random.Range(18, 36);
            p.style.color = particleColors[i % particleColors.Length];
            p.style.left = panelCenterX;
            p.style.top = panelCenterY;
            p.style.unityTextAlign = TextAnchor.MiddleCenter;
            particleContainer.Add(p);

            float angle = Random.Range(-70f, 70f) * Mathf.Deg2Rad; // 上方向中心に扇状
            float speed = Random.Range(200f, 450f);
            float vx = Mathf.Sin(angle) * speed;
            float vy = -Mathf.Cos(angle) * speed; // 上方向がマイナス
            float rotSpeed = Random.Range(-180f, 180f);
            particles.Add((p, panelCenterX, panelCenterY, vx, vy, rotSpeed));
        }

        // 昇天 + パーティクル同時アニメーション
        float ascendDur = 1.2f;
        elapsed = 0f;

        while (elapsed < ascendDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / ascendDur);

            // パネル昇天: 上方向に移動 + フェードアウト
            float ascendEase = t * t; // ease-in (加速しながら上昇)
            float moveY = -300f * ascendEase;
            float alpha = 1f - Mathf.Clamp01(t * 1.5f); // 後半で完全に消える
            panel.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, moveY));
            panel.style.opacity = alpha;

            // パーティクル更新
            float dt = Time.deltaTime;
            for (int i = 0; i < particles.Count; i++)
            {
                var (el, px, py, vx, vy, rotSpeed) = particles[i];
                px += vx * dt;
                py += vy * dt;
                vy += 200f * dt; // 軽い重力でアーチ状に
                el.style.left = px;
                el.style.top = py;
                el.style.rotate = new UIE.StyleRotate(
                    new UIE.Rotate(new UIE.Angle(rotSpeed * elapsed, UIE.AngleUnit.Degree)));
                el.style.opacity = Mathf.Clamp01(1f - t * 1.2f);
                particles[i] = (el, px, py, vx, vy, rotSpeed);
            }

            yield return null;
        }

        // クリーンアップ
        panel.style.opacity = 0f;
        panel.style.visibility = UIE.Visibility.Hidden;
        particleContainer.RemoveFromHierarchy();
        whiteOverlay.RemoveFromHierarchy();
    }

    IEnumerator ShatterCardEffect(UIE.VisualElement panel)
    {
        if (panel == null) yield break;

        // --- Phase 1: ひび割れ (0.6s) ---
        var crackOverlay = new UIE.VisualElement();
        crackOverlay.style.position = UIE.Position.Absolute;
        crackOverlay.style.left = 0; crackOverlay.style.top = 0;
        crackOverlay.style.right = 0; crackOverlay.style.bottom = 0;
        crackOverlay.style.overflow = UIE.Overflow.Hidden;
        crackOverlay.pickingMode = UIE.PickingMode.Ignore;
        panel.Add(crackOverlay);

        // ひび割れ線を生成
        var crackLines = new (float cx, float cy, float angle, float len)[] {
            (0.5f, 0.3f, -30f,  0.7f),
            (0.4f, 0.5f,  45f,  0.6f),
            (0.6f, 0.4f, -60f,  0.5f),
            (0.3f, 0.6f,  20f,  0.8f),
            (0.7f, 0.7f, -45f,  0.5f),
            (0.5f, 0.5f,  70f,  0.6f),
            (0.2f, 0.3f,  10f,  0.4f),
            (0.8f, 0.5f, -20f,  0.4f),
        };

        float crackDuration = 0.6f;
        float perCrack = crackDuration / crackLines.Length;

        for (int i = 0; i < crackLines.Length; i++)
        {
            var (cx, cy, angle, len) = crackLines[i];
            var line = new UIE.VisualElement();
            line.pickingMode = UIE.PickingMode.Ignore;
            line.style.position = UIE.Position.Absolute;
            line.style.width = new UIE.Length(len * 100f, UIE.LengthUnit.Percent);
            line.style.height = 3;
            line.style.left = new UIE.Length(cx * 100f, UIE.LengthUnit.Percent);
            line.style.top = new UIE.Length(cy * 100f, UIE.LengthUnit.Percent);
            line.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            line.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(angle, UIE.AngleUnit.Degree)));
            line.style.transformOrigin = new UIE.StyleTransformOrigin(
                new UIE.TransformOrigin(new UIE.Length(0), new UIE.Length(50, UIE.LengthUnit.Percent)));
            crackOverlay.Add(line);

            // 小さな揺れ
            StartCoroutine(ShakeEffect(panel, perCrack * 0.7f, 3f + i));
            yield return new WaitForSeconds(perCrack);
        }

        // 最後の大きな揺れ
        yield return StartCoroutine(ShakeEffect(panel, 0.15f, 15f));

        // --- Phase 2: 粉砕 (0.7s) ---
        crackOverlay.RemoveFromHierarchy();

        // カードの親要素を取得
        var parent = panel.parent;
        if (parent == null) yield break;

        // 破片を生成 (4x5 グリッド)
        int cols = 4, rows2 = 5;
        float panelW = 450f, panelH = 650f;
        float fragW = panelW / cols;
        float fragH = panelH / rows2;

        // カードの位置を取得
        var panelLayout = panel.layout;
        float baseX = panelLayout.x;
        float baseY = panelLayout.y;

        // 元のカードを非表示
        panel.style.visibility = UIE.Visibility.Hidden;

        var fragments = new List<UIE.VisualElement>();
        var velocities = new List<Vector2>();
        var rotations = new List<float>();

        for (int r = 0; r < rows2; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var frag = new UIE.VisualElement();
                frag.pickingMode = UIE.PickingMode.Ignore;
                frag.style.position = UIE.Position.Absolute;
                frag.style.width = fragW;
                frag.style.height = fragH;
                frag.style.left = baseX + c * fragW;
                frag.style.top = baseY + r * fragH;
                frag.style.backgroundColor = new Color(
                    Random.Range(0.15f, 0.35f),
                    Random.Range(0.15f, 0.30f),
                    Random.Range(0.25f, 0.45f),
                    0.9f);
                frag.style.borderTopLeftRadius = 2;
                frag.style.borderTopRightRadius = 2;
                frag.style.borderBottomLeftRadius = 2;
                frag.style.borderBottomRightRadius = 2;
                parent.Add(frag);
                fragments.Add(frag);

                // 中心からの方向 + ランダム
                float dirX = (c - cols / 2f + 0.5f) * 200f + Random.Range(-80f, 80f);
                float dirY = (r - rows2 / 2f + 0.5f) * 200f + Random.Range(-60f, -200f);
                velocities.Add(new Vector2(dirX, dirY));
                rotations.Add(Random.Range(-360f, 360f));
            }
        }

        // アニメーション
        float shatterDur = 0.7f;
        float elapsed = 0f;
        while (elapsed < shatterDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shatterDur;
            float ease = t * t; // ease-in

            for (int i = 0; i < fragments.Count; i++)
            {
                var f = fragments[i];
                float dx = velocities[i].x * ease;
                float dy = velocities[i].y * ease + 400f * ease * ease; // 重力
                f.style.translate = new UIE.StyleTranslate(new UIE.Translate(dx, dy));
                f.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(rotations[i] * ease, UIE.AngleUnit.Degree)));
                f.style.opacity = 1f - t;
                f.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(1f - ease * 0.5f, 1f - ease * 0.5f)));
            }
            yield return null;
        }

        // 破片を削除
        foreach (var f in fragments)
            f.RemoveFromHierarchy();
    }

    // ===== バトルフロー =====

    IEnumerator BattleStart()
    {
        isBattleActive = true;
        if (vsTextEl != null) vsTextEl.style.display = UIE.DisplayStyle.None;

        // --- エンカウント演出 ---
        var encounterOverlay = new UIE.VisualElement();
        encounterOverlay.AddToClassList("battle-encounter-overlay");

        // 敵画像
        var encounterImg = new UIE.VisualElement();
        encounterImg.AddToClassList("battle-encounter-img");
        Sprite enemySprite = loadedEnemySprite ?? firstEnemySprite;
        if (enemySprite != null)
        {
            encounterImg.style.backgroundImage = new UIE.StyleBackground(enemySprite);
            encounterImg.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        else if (enemyBgColor != Color.clear)
        {
            encounterImg.style.backgroundColor = enemyBgColor;
        }
        encounterOverlay.Add(encounterImg);

        // テキスト
        var encounterText = new UIE.Label();
        encounterText.AddToClassList("battle-encounter-text");
        encounterText.text = Localization.GetEnemy(enemyName) + " が あそびにきた！";
        encounterOverlay.Add(encounterText);

        overlayRoot.Add(encounterOverlay);

        // バウンス登場アニメーション
        yield return null;
        yield return StartCoroutine(BounceInEffect(encounterImg));

        // テキストフェードイン
        encounterText.AddToClassList("battle-encounter-text-visible");

        yield return new WaitForSeconds(1.5f);

        // フェードアウト
        encounterOverlay.AddToClassList("battle-encounter-overlay-hide");
        yield return new WaitForSeconds(0.5f);
        encounterOverlay.RemoveFromHierarchy();

        // デヴィル夫人ボス: 戦闘前イントロ台詞
        bool isBossStart = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBossStart && enemyName == "デヴィル夫人")
        {
            yield return StartCoroutine(ShowDevilLadyIntro());
        }

        // バトルUI フェードイン
        battleBgEl.AddToClassList("battle-ui-fadein");
        enemyAreaEl.AddToClassList("battle-ui-fadein");
        playerAreaEl.AddToClassList("battle-ui-fadein");
        centerAreaEl.AddToClassList("battle-ui-fadein");
        battleBgEl.style.opacity = 1;
        enemyAreaEl.style.opacity = 1;
        playerAreaEl.style.opacity = 1;
        centerAreaEl.style.opacity = 1;

        // バトルイントロ演出: 白フラッシュ + 画面シェイク
        StartCoroutine(IntroFlashEffect());
        StartCoroutine(ShakeEffect(root, 0.5f, 20f));
        yield return new WaitForSeconds(0.5f);

        // BGM再生
        if (bgmSource != null && bgmSource.clip != null)
            bgmSource.Play();

        // バトル開始
        battleLogLabel.text = "";
        int playerSpeed = DataCarrier.Instance != null ? DataCarrier.Instance.babyAthletic : 50;
        if (playerSpeed >= enemySpeed)
            StartCoroutine(PlayerTurn());
        else
            StartCoroutine(EnemyTurn());
    }

    IEnumerator PlayerTurn()
    {
        if (!isBattleActive) yield break;

        isPlayerTurn = true;
        playerDefending = false;

        // おしゃぶりチャーム: 毎ターンHP+3回復
        if (DataCarrier.Instance != null && DataCarrier.Instance.IsEquipped("おしゃぶりチャーム") && playerHp < playerMaxHp)
        {
            int healAmt = 3;
            playerHp = Mathf.Min(playerMaxHp, playerHp + healAmt);
            UpdatePlayerDisplay();
            battleLogLabel.text = Localization.Get("battle_oshaburi_heal", healAmt);
            yield return new WaitForSeconds(0.6f);
        }

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
        if (isBoss && enemyName == "青年のシバ" && (enemyTurnCount + 1) % 3 == 0 && enemyTurnCount > 0)
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
        SetInteractionEnabled(true);
        actionPanelEl.style.display = UIE.DisplayStyle.Flex;
        bottomBar.style.display = UIE.DisplayStyle.Flex;
        // サブメニューが開いていたら閉じてコマンド行を復帰
        if (submenuEl.style.display == UIE.DisplayStyle.Flex)
            CloseSubmenu();
        waitingForAction = true;

        // オートバトル: 自動で攻撃
        if (isAutoBattle)
        {
            yield return new WaitForSeconds(0.3f);
            OnAttack();
        }

        while (waitingForAction)
            yield return null;

        actionPanelEl.style.display = UIE.DisplayStyle.None;
        bottomBar.style.display = UIE.DisplayStyle.None;
        SetInteractionEnabled(false);
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
            string meterPoison = GetEnemyMeterLabel();
            battleLogLabel.text = (meterPoison != "HP")
                ? Localization.Get("battle_poison_damage_meter", Localization.GetEnemy(enemyName), poisonDmg, meterPoison)
                : Localization.Get("battle_poison_damage", Localization.GetEnemy(enemyName), poisonDmg);
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
        if (isBoss && enemyName == "青年のシバ" && enemyTurnCount % 3 == 0)
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

    bool HasGoldenSmartphone()
    {
        return DataCarrier.Instance != null && DataCarrier.Instance.IsEquipped("金色のスマホ");
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

        // 金色のスマホ: 10%で攻撃を跳ね返す
        if (HasGoldenSmartphone() && Random.Range(0, 10) == 0)
        {
            int effectiveEnemyAtk = enemyAtkDebuffTurns > 0 ? (int)(enemyAtk * 0.6f) : enemyAtk;
            int reflectDamage = CalculateDamage(effectiveEnemyAtk, enemyDef, enemyDefending);
            enemyHp = Mathf.Max(0, enemyHp - reflectDamage);
            UpdateEnemyDisplay();
            StartCoroutine(DamageEffect(enemyFaceMask, enemyPanel));
            string meterRef = GetEnemyMeterLabel();
            battleLogLabel.text = (meterRef != "HP")
                ? Localization.Get("battle_reflect_meter", reflectDamage, meterRef)
                : Localization.Get("battle_reflect", reflectDamage);
            yield return new WaitForSeconds(1.5f);
            yield return StartCoroutine(CheckEnemyDefeatAndContinue());
            yield break;
        }

        int effectiveEnemyAtk2 = enemyAtkDebuffTurns > 0 ? (int)(enemyAtk * 0.6f) : enemyAtk;
        int damage = CalculateDamage(effectiveEnemyAtk2, playerDef, playerDefending);
        playerHp = Mathf.Max(0, playerHp - damage);
        UpdatePlayerDisplay();
        StartCoroutine(DamageEffect(playerFaceMask, playerPanel));

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
        StartCoroutine(DamageEffect(playerFaceMask, playerPanel));

        battleLogLabel.text = Localization.Get("battle_enemy_special_hit", Localization.GetEnemy(enemyName), damage);
        yield return new WaitForSeconds(1.0f);

        if (isDevilEnemy && playerHp > 0)
        {
            int poisonDuration = (enemyName == "デヴィル夫人") ? 4 : 3;
            // 魔法のおむつ: 毒ターン-1
            if (DataCarrier.Instance != null && DataCarrier.Instance.IsEquipped("魔法のおむつ"))
                poisonDuration = Mathf.Max(1, poisonDuration - 1);
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
        StartCoroutine(DamageEffect(playerFaceMask, playerPanel, true));

        battleLogLabel.text = playerDefending
            ? Localization.Get("battle_shiba_ultimate_blocked", damage)
            : Localization.Get("battle_shiba_ultimate_hit", damage);
        yield return new WaitForSeconds(1.2f);

        yield return StartCoroutine(CheckPlayerDefeatAndContinue());
    }

    IEnumerator ShowDevilLadyIntro()
    {
        var introOverlay = new UIE.VisualElement();
        introOverlay.AddToClassList("fill");
        introOverlay.style.flexDirection = UIE.FlexDirection.Column;
        introOverlay.style.justifyContent = UIE.Justify.Center;
        introOverlay.style.alignItems = UIE.Align.Center;
        introOverlay.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        overlayRoot.Add(introOverlay);

        var introText = new UIE.Label();
        introText.enableRichText = true;
        introText.style.color = Color.white;
        introText.style.fontSize = 34;
        introText.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        introText.style.whiteSpace = UIE.WhiteSpace.Normal;
        introText.style.width = 900;
        UIHelper.ApplyFont(introText);
        introOverlay.Add(introText);

        string[] lines = new string[]
        {
            Localization.Get("battle_devil_lady_intro_1"),
            Localization.Get("battle_devil_lady_intro_2"),
            Localization.Get("battle_devil_lady_intro_3"),
        };

        foreach (var line in lines)
        {
            introText.text = line;
            yield return new WaitForSeconds(2.5f);
        }

        // フェードアウト
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.5f);
            introOverlay.style.backgroundColor = new Color(0, 0, 0, 0.8f * fadeT);
            introText.style.opacity = fadeT;
            yield return null;
        }
        introOverlay.RemoveFromHierarchy();
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
        StartCoroutine(DamageEffect(playerFaceMask, playerPanel, true));

        battleLogLabel.text = playerDefending
            ? Localization.Get("battle_devil_lady_ultimate_blocked", damage)
            : Localization.Get("battle_devil_lady_ultimate_hit", damage);
        yield return new WaitForSeconds(1.0f);

        if (playerHp > 0)
        {
            int devilPoison = 5;
            if (DataCarrier.Instance != null && DataCarrier.Instance.IsEquipped("魔法のおむつ"))
                devilPoison = Mathf.Max(1, devilPoison - 1);
            playerPoisonTurns = devilPoison;
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
        StartCoroutine(DamageEffect(playerFaceMask, playerPanel, true));

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

    IEnumerator CheckEnemyDefeatAndContinue()
    {
        if (enemyHp <= 0)
        {
            StartCoroutine(BattleWin());
            yield break;
        }

        StartCoroutine(EnemyTurn());
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
            bottomBar.style.display = UIE.DisplayStyle.None;
            battleLogLabel.text = cannotRun && !isBoss
                ? Localization.Get("battle_run_fixed")
                : Localization.Get("battle_run_boss");
            yield return new WaitForSeconds(1.5f);
            SetInteractionEnabled(true);
            actionPanelEl.style.display = UIE.DisplayStyle.Flex;
            bottomBar.style.display = UIE.DisplayStyle.Flex;
            waitingForAction = true;
            yield break;
        }

        actionPanelEl.style.display = UIE.DisplayStyle.None;
        bottomBar.style.display = UIE.DisplayStyle.None;

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
        StartCoroutine(DamageEffect(enemyFaceMask, enemyPanel));

        string meter = GetEnemyMeterLabel();
        battleLogLabel.text = (meter != "HP")
            ? Localization.Get("battle_attack_hit_meter", normalAttackName, Localization.GetEnemy(enemyName), damage, meter)
            : Localization.Get("battle_attack_hit", normalAttackName, Localization.GetEnemy(enemyName), damage);
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
        StartCoroutine(DamageEffect(enemyFaceMask, enemyPanel));

        string meterMom = GetEnemyMeterLabel();
        battleLogLabel.text = (meterMom != "HP")
            ? Localization.Get("battle_mother_damage_meter", motherAttackName, damage, meterMom)
            : Localization.Get("battle_mother_damage", motherAttackName, damage);
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
            StartCoroutine(DamageEffect(enemyFaceMask, enemyPanel));

            string meterSp = GetEnemyMeterLabel();
            if (meterSp != "HP")
                battleLogLabel.text = isGodBaby
                    ? Localization.Get("battle_god_special_hit_meter", specialAttackName, damage, meterSp)
                    : Localization.Get("battle_special_hit_meter", specialAttackName, damage, meterSp);
            else
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
        StartCoroutine(FadeOutBGM(1.5f));

        // にこにこ昇天演出（ホワイトアウト → バウンス → ハート噴出 → 昇天）
        yield return StartCoroutine(NikonikoDefeatEffect(enemyPanel));
        yield return new WaitForSeconds(0.3f);

        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddDefeatedEnemy(enemyName);

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
        int intelligence = DataCarrier.Instance.babyIntelligence;
        float intelligenceBonus = 1.0f + intelligence * 0.005f;
        int expGained = Mathf.RoundToInt(baseExp * intelligenceBonus);
        DataCarrier.Instance.babyExp += expGained;
        DataCarrier.Instance.defeatedEnemies++;

        int milkGained = Mathf.Max(1, expGained / 2);
        DataCarrier.Instance.milk += milkGained;

        int currentAge = DataCarrier.Instance.babyAge;
        int needed = DataCarrier.ExpForNextAge(currentAge);
        int currentExp = DataCarrier.Instance.babyExp;
        bool willLevelUp = currentExp >= needed;

        // === リザルト画面（1枚にまとめる） ===
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("battle-result-overlay");

        var title = UIHelper.CreateLabel(Localization.Get("battle_enemy_defeated", Localization.GetEnemy(enemyName)), "battle-result-title");
        overlay.Add(title);

        // 報酬カード
        var card = new UIE.VisualElement();
        card.AddToClassList("battle-result-card");
        overlay.Add(card);

        // EXP行
        var expRow = new UIE.VisualElement();
        expRow.AddToClassList("battle-result-row");
        var expLabel = UIHelper.CreateLabel(Localization.Get("battle_result_exp"), "battle-result-row-label");
        var expValue = UIHelper.CreateLabel($"+{expGained}", "battle-result-row-value");
        expValue.AddToClassList("battle-result-value-accent");
        expRow.Add(expLabel);
        expRow.Add(expValue);
        card.Add(expRow);

        // ミルク行
        var milkRow = new UIE.VisualElement();
        milkRow.AddToClassList("battle-result-row");
        var milkLabel = UIHelper.CreateLabel(Localization.Get("battle_result_milk"), "battle-result-row-label");
        var milkValue = UIHelper.CreateLabel($"+{milkGained}", "battle-result-row-value");
        milkValue.AddToClassList("battle-result-value-sub");
        milkRow.Add(milkLabel);
        milkRow.Add(milkValue);
        card.Add(milkRow);

        // EXPバー
        if (!willLevelUp)
        {
            var barLabel = UIHelper.CreateLabel($"EXP  {currentExp} / {needed}", "battle-result-bar-label");
            card.Add(barLabel);

            var barBg = new UIE.VisualElement();
            barBg.AddToClassList("battle-result-bar-bg");
            var barFill = new UIE.VisualElement();
            barFill.AddToClassList("battle-result-bar-fill");
            barFill.style.width = new UIE.Length(0, UIE.LengthUnit.Percent);
            barBg.Add(barFill);
            card.Add(barBg);

            overlayRoot.Add(overlay);

            // バーアニメーション
            yield return null;
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
        }
        else
        {
            var lvUpLabel = UIHelper.CreateLabel(Localization.Get("battle_age_up", currentAge + 1), "battle-result-levelup");
            card.Add(lvUpLabel);
            overlayRoot.Add(overlay);
        }

        // OKボタン
        bool dismissed = false;
        var okBtn = UIHelper.CreatePillButton("OK", "battle-growth-ok-btn");
        okBtn.style.marginTop = 40;
        okBtn.style.opacity = 0f;
        okBtn.clicked += () => dismissed = true;
        overlay.Add(okBtn);

        yield return new WaitForSeconds(0.5f);
        float okElapsed = 0f;
        while (okElapsed < 0.2f)
        {
            okElapsed += Time.deltaTime;
            okBtn.style.opacity = Mathf.Clamp01(okElapsed / 0.2f);
            yield return null;
        }
        okBtn.style.opacity = 1f;

        if (isAutoBattle)
            yield return new WaitForSeconds(1.0f);
        else
            while (!dismissed) yield return null;

        overlay.RemoveFromHierarchy();

        // === レベルアップ → せいちょう画面 ===
        if (willLevelUp)
        {
            int oldAtk = DataCarrier.Instance.babyAtk;
            int oldDef = DataCarrier.Instance.babyDef;
            int oldHp = DataCarrier.Instance.babyHp;
            int oldIntelligence = DataCarrier.Instance.babyIntelligence;
            int oldAthletic = DataCarrier.Instance.babyAthletic;

            DataCarrier.Instance.babyExp -= needed;
            DataCarrier.Instance.AgeUp();

            yield return StartCoroutine(ShowStatGrowth(
                oldAtk, oldDef, oldHp, oldIntelligence, oldAthletic,
                DataCarrier.Instance.babyAtk, DataCarrier.Instance.babyDef,
                DataCarrier.Instance.babyHp, DataCarrier.Instance.babyIntelligence,
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
    }

    IEnumerator ShowStatGrowth(int oldAtk, int oldDef, int oldHp, int oldIntelligence, int oldAthletic,
                                int newAtk, int newDef, int newHp, int newIntelligence, int newAthletic)
    {
        int newAge = DataCarrier.Instance != null ? DataCarrier.Instance.babyAge : 0;
        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "";

        // 全画面オーバーレイ (on root) — justify-content: center で中央配置
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("battle-growth-overlay");

        // タイトル
        var title = UIHelper.CreateLabel(Localization.Get("battle_growth_title"), "battle-growth-title");
        overlay.Add(title);

        // 名前 + 月齢
        var subtitle = UIHelper.CreateLabel($"{babyName}    {Localization.GetAge(newAge)}", "battle-growth-subtitle");
        overlay.Add(subtitle);

        // ステータス行定義 (テーマカラー: sub=#FFB7C5, accent=#AAF0D1)
        var colorSub = new Color(1f, 0.718f, 0.773f);    // #FFB7C5
        var colorAccent = new Color(0.667f, 0.941f, 0.82f); // #AAF0D1
        var statDefs = new[] {
            new { label = Localization.Get("battle_stat_hp_label"), oldV = oldHp, newV = newHp, color = colorSub },
            new { label = Localization.Get("battle_stat_atk"), oldV = oldAtk, newV = newAtk, color = colorSub },
            new { label = Localization.Get("battle_stat_def"), oldV = oldDef, newV = newDef, color = colorAccent },
            new { label = Localization.Get("battle_stat_athletic"), oldV = oldAthletic, newV = newAthletic, color = colorAccent },
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

        overlayRoot.Add(overlay);

        // Animate rows in
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].AddToClassList("battle-growth-row-visible");
            yield return new WaitForSeconds(0.35f);
        }

        // OKボタン
        bool dismissed = false;
        var okBtn = UIHelper.CreatePillButton("OK", "battle-growth-ok-btn");
        okBtn.style.marginTop = 40;
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

        if (isAutoBattle)
            yield return new WaitForSeconds(1.0f);
        else
            while (!dismissed) yield return null;

        overlay.RemoveFromHierarchy();

        battleLogLabel.text = Localization.Get("battle_hp_full_heal");
        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator BattleLose()
    {
        isBattleActive = false;
        StartCoroutine(FadeOutBGM(1.5f));

        // プレイヤーカード粉砕演出
        yield return StartCoroutine(ShatterCardEffect(playerPanel));
        yield return new WaitForSeconds(0.3f);

        string babyName = DataCarrier.Instance != null && !string.IsNullOrEmpty(DataCarrier.Instance.babyName)
            ? DataCarrier.Instance.babyName : "Baby";
        battleLogLabel.text = string.Format(Localization.Get("battle_defeat"), babyName);
        yield return new WaitForSeconds(1.5f);

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

        var retryBtn = UIHelper.CreatePillButton(Localization.Get("ui_back_to_title_long"), "pill-button");
        retryBtn.clicked += () => SceneManager.LoadScene("TitleScene");
        card.Add(retryBtn);

        overlay.Add(card);
        overlayRoot.Add(overlay);
    }

    // ===== 固定エンカウント（おともだち）勝利 =====

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

        // 初回ボス: カットインテキスト + ワイプ遷移
        bool isFirstBoss = !(area == 4 && enemyName == "メロディアス女王")
                        && !(area == 2 && enemyName == "デヴィル夫人");
        if (isFirstBoss)
        {
            // カットイン暗転パネル
            var cutinPanel = new UIE.VisualElement();
            cutinPanel.style.position = UIE.Position.Absolute;
            cutinPanel.style.left = 0; cutinPanel.style.top = 0;
            cutinPanel.style.right = 0; cutinPanel.style.bottom = 0;
            cutinPanel.style.backgroundColor = new Color(0f, 0f, 0f, 0.95f);
            cutinPanel.style.alignItems = UIE.Align.Center;
            cutinPanel.style.justifyContent = UIE.Justify.Center;

            var cutinLabel = UIHelper.CreateLabel(
                "運命の第一歩。\nその先に待つのは、安らぎか、それとも――", "");
            UIHelper.ApplyFont(cutinLabel);
            cutinLabel.style.fontSize = 36;
            cutinLabel.style.color = Color.white;
            cutinLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            cutinLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
            cutinLabel.style.width = 800;
            cutinLabel.style.opacity = 0f;
            cutinPanel.Add(cutinLabel);
            overlayRoot.Add(cutinPanel);

            // テキストフェードイン
            float cutElapsed = 0f;
            while (cutElapsed < 1.0f)
            {
                cutElapsed += Time.deltaTime;
                cutinLabel.style.opacity = Mathf.Clamp01(cutElapsed / 1.0f);
                yield return null;
            }
            cutinLabel.style.opacity = 1f;

            yield return new WaitForSeconds(2.5f);

            // テキストフェードアウト
            cutElapsed = 0f;
            while (cutElapsed < 1.0f)
            {
                cutElapsed += Time.deltaTime;
                cutinLabel.style.opacity = 1f - Mathf.Clamp01(cutElapsed / 1.0f);
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);
            cutinPanel.RemoveFromHierarchy();

            // ワイプ遷移 (右からスライドして画面を覆う)
            var wipePanel = new UIE.VisualElement();
            wipePanel.style.position = UIE.Position.Absolute;
            wipePanel.style.left = 0; wipePanel.style.top = 0;
            wipePanel.style.right = 0; wipePanel.style.bottom = 0;
            wipePanel.style.backgroundColor = Color.black;
            wipePanel.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(100, UIE.LengthUnit.Percent), 0));
            overlayRoot.Add(wipePanel);

            cutElapsed = 0f;
            float wipeDuration = 0.8f;
            while (cutElapsed < wipeDuration)
            {
                cutElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(cutElapsed / wipeDuration);
                // ease-in-out
                t = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
                float pct = Mathf.Lerp(100f, 0f, t);
                wipePanel.style.translate = new UIE.StyleTranslate(
                    new UIE.Translate(new UIE.Length(pct, UIE.LengthUnit.Percent), 0));
                yield return null;
            }
            wipePanel.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(0, 0));

            yield return new WaitForSeconds(0.3f);
        }

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.babyCurrentHp = -1;

            if (area == 4 && enemyName == "メロディアス女王")
            {
                DataCarrier.Instance.currentArea = 3;
                DataCarrier.Instance.mapPlayerX = 14;
                DataCarrier.Instance.mapPlayerY = 37;
            }
            else if (area == 2 && enemyName == "デヴィル夫人")
            {
                DataCarrier.Instance.currentArea = 3;
                DataCarrier.Instance.mapPlayerX = 11;
                DataCarrier.Instance.mapPlayerY = 2;
            }
            else
            {
                DataCarrier.Instance.currentArea = 1;
                DataCarrier.Instance.mapPlayerX = 5;
                DataCarrier.Instance.mapPlayerY = 9;
            }

            if (isFirstBoss)
                DataCarrier.Instance.pendingWipeIn = true;

            DataCarrier.Instance.SaveData();
        }

        SceneManager.LoadScene("MapScene");
    }

    // ===== 勝利後 MapScene 遷移 =====

    IEnumerator VictoryToMap()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
        }

        SceneManager.LoadScene("MapScene");
        yield break;
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
        PlayerPrefs.SetInt("babyIntelligence", dc.babyIntelligence);
        PlayerPrefs.SetInt("babyAthletic", dc.babyAthletic);
        PlayerPrefs.SetInt("babyLuck", dc.babyLuck);
        PlayerPrefs.SetInt("babyFortune", dc.babyFortune);
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

    // CPU側ガウスぼかし（縮小→ボックスブラー→復元）
    Texture2D BlurTexture(Texture2D src, int passes)
    {
        // 1/4 に縮小してからブラーすると高速
        int w = src.width / 4;
        int h = src.height / 4;
        var rt = RenderTexture.GetTemporary(w, h);
        Graphics.Blit(src, rt);
        var small = new Texture2D(w, h, TextureFormat.RGBA32, false);
        RenderTexture.active = rt;
        small.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        small.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        var pixels = small.GetPixels();
        for (int p = 0; p < passes; p++)
        {
            pixels = BoxBlur(pixels, w, h);
        }

        // 少し暗くする
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color(pixels[i].r * 0.65f, pixels[i].g * 0.65f, pixels[i].b * 0.65f, 1f);
        }

        small.SetPixels(pixels);
        small.Apply();

        // 元サイズに復元（ぼけた低解像度をアップスケール）
        var result = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        var rtUp = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.Blit(small, rtUp);
        RenderTexture.active = rtUp;
        result.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        result.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rtUp);
        Destroy(small);

        return result;
    }

    Color[] BoxBlur(Color[] src, int w, int h)
    {
        var dst = new Color[src.Length];
        // 水平パス
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color sum = Color.black;
                int count = 0;
                for (int dx = -2; dx <= 2; dx++)
                {
                    int nx = Mathf.Clamp(x + dx, 0, w - 1);
                    sum += src[y * w + nx];
                    count++;
                }
                dst[y * w + x] = sum / count;
            }
        }
        // 垂直パス
        var dst2 = new Color[src.Length];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color sum = Color.black;
                int count = 0;
                for (int dy = -2; dy <= 2; dy++)
                {
                    int ny = Mathf.Clamp(y + dy, 0, h - 1);
                    sum += dst[ny * w + x];
                    count++;
                }
                dst2[y * w + x] = sum / count;
            }
        }
        return dst2;
    }
}
