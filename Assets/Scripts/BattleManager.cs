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
        {"エゴ・マザー・マシーン", "しはいど"},
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
                playerAtk = (int)(playerAtk * 1.3f);
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
            if (DataCarrier.Instance.IsEquipped("黄金のほ乳瓶")) { playerMaxHp += 100; playerHp += 100; playerDef += 50; }
            if (DataCarrier.Instance.IsEquipped("悪魔のティアラ")) { playerAtk += 6; playerDef -= 2; }
            if (DataCarrier.Instance.IsEquipped("泣き猫パンチ")) playerAtk += 2;
            if (DataCarrier.Instance.IsEquipped("ミニよだれかけ")) playerDef += 2;
            if (DataCarrier.Instance.IsEquipped("にじいろガラガラ")) { playerAtk += 5; playerDef += 2; }
            if (DataCarrier.Instance.IsEquipped("ゴールデン・ベビーステッキ")) { playerAtk += 50; playerMaxHp += 50; playerHp += 50; playerDef += 50; }
            if (DataCarrier.Instance.IsEquipped("かぐやのリボン")) { playerDef += 200; }

            // かぐやちゃん相思相愛バフ（常時）
            if (DataCarrier.Instance.kaguyaLover) { playerAtk += 5; playerDef += 5; }

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

        if (fromMap && bossBattle && area == 8)
        {
            enemyName = "エゴ・マザー・マシーン";
            enemyAge = -1;
            enemyMaxHp = 1500;
            enemyHp = enemyMaxHp;
            enemyAtk = 130;
            enemyDef = 60;
            enemySpeed = 90;
            isDevilEnemy = false;
            enemyBgColor = new Color(0.1f, 0.05f, 0.15f);
            loadedEnemySprite = Resources.Load<Sprite>("EnemyBabys/boss/ego-mother");
        }
        else if (fromMap && bossBattle && area == 4)
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
                new object[]{ "どたばたベイビー",       "EnemyBabys/common-abarennbou",  105, 34, 22, 32, 0, 99 },
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

        if (fixedName == "かぐやちゃん")
        {
            cannotRun = false;
            isDevilEnemy = false;
            loadedEnemySprite = Resources.Load<Sprite>("MapCharacters/heroine/kaguya3");
            enemyMaxHp = 120;
            enemyHp = enemyMaxHp;
            enemyAtk = 25;
            enemyDef = 20;
            enemySpeed = 60;
            enemyBgColor = new Color(1f, 0.72f, 0.77f);
            return;
        }

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
        bool isFinalBossUI = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle && DataCarrier.Instance.currentArea == 8;
        if (isFinalBossUI)
            battleRoot.AddToClassList("battle-dark-theme");
        root.Add(battleRoot);

        // 背景（マップスクショがあれば使用 — ぼかし＋暗く）
        battleBgEl = new UIE.VisualElement();
        battleBgEl.AddToClassList("battle-bg");
        battleBgEl.pickingMode = UIE.PickingMode.Ignore;
        bool isFinalBoss = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle && DataCarrier.Instance.currentArea == 8;
        if (isFinalBoss)
        {
            // エゴマザー戦: background.png + 暗いオーバーレイ
            var bgSprite = Resources.Load<Sprite>("Map/4th/background");
            if (bgSprite != null)
                battleBgEl.style.backgroundImage = new UIE.StyleBackground(bgSprite);
            else
            {
                var bgTex = Resources.Load<Texture2D>("Map/4th/background");
                if (bgTex != null) battleBgEl.style.backgroundImage = new UIE.StyleBackground(bgTex);
            }
            battleBgEl.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;

            // 暗いオーバーレイをバトル背景の上に重ねる
            var battleDarkLayer = new UIE.VisualElement();
            battleDarkLayer.pickingMode = UIE.PickingMode.Ignore;
            battleDarkLayer.style.position = UIE.Position.Absolute;
            battleDarkLayer.style.left = 0; battleDarkLayer.style.top = 0;
            battleDarkLayer.style.right = 0; battleDarkLayer.style.bottom = 0;
            battleDarkLayer.style.backgroundColor = new Color(0, 0, 0, 0.75f);
            battleBgEl.Add(battleDarkLayer);
        }
        else if (DataCarrier.Instance != null && DataCarrier.Instance.battleBgTexture != null)
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
        if (isFinalBossUI && enemyFaceMask != null)
        {
            enemyFaceMask.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(1.2f, 1.2f, 1f)));
        }

        // 中央エリア（VS + ログ）
        centerAreaEl = new UIE.VisualElement();
        centerAreaEl.AddToClassList("battle-center");
        centerAreaEl.style.opacity = 0;
        battleRoot.Add(centerAreaEl);

        vsTextEl = UIHelper.CreateLabel("VS", "battle-vs");
        centerAreaEl.Add(vsTextEl);

        var logGlow = new UIE.VisualElement();
        logGlow.AddToClassList("battle-log-glow");
        logGlow.pickingMode = UIE.PickingMode.Ignore;
        centerAreaEl.Add(logGlow);

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
        runBtn.text = "バイバイ \U0001F44B";
        runBtn.clicked += OnRun;
        bottomBar.Add(runBtn);

        autoBattleBtn = new UIE.Button();
        autoBattleBtn.AddToClassList("battle-auto-pill");
        autoBattleBtn.AddToClassList("battle-auto-off");
        UIHelper.ApplyFont(autoBattleBtn);
        autoBattleBtn.text = "オート \u25B6\u25B6";
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

        BuildCmdButton(commandRow, "あそぶ \u2728", OnAttack, 0);
        BuildCmdButton(commandRow, "とくぎ \U0001FA84", ShowSkillSubmenu, 1);
        BuildCmdButton(commandRow, "おもちゃ \U0001F9F8", ShowItemSubmenu, 2);
        BuildCmdButton(commandRow, "みまもる \U0001F6E1\uFE0F", OnDefend, 3);

        // サブメニュー（初期非表示）
        submenuEl = new UIE.VisualElement();
        submenuEl.AddToClassList("battle-submenu");
        submenuEl.style.display = UIE.DisplayStyle.None;
        actionPanelEl.Add(submenuEl);

        // top-bar と player-area の bottom を actionPanelEl の高さに合わせる
        actionPanelEl.RegisterCallback<UIE.GeometryChangedEvent>(evt =>
        {
            float actionHeight = evt.newRect.height;
            bottomBar.style.bottom = actionHeight;
            playerAreaEl.style.marginBottom = actionHeight + 60;
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
        area.Add(panel);

        // 情報カラム（名前+年齢, HPバー, HPテキスト）
        var infoCol = new UIE.VisualElement();
        infoCol.AddToClassList("battle-info-col");
        infoCol.AddToClassList("battle-info-col-enemy");

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

        // 顔マスク — プレイヤーも敵と同じ透明丸形スタイル
        var faceMask = new UIE.VisualElement();
        faceMask.AddToClassList("battle-face-mask-enemy");
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

    void BuildCmdButton(UIE.VisualElement parent, string label, System.Action onClick, int index = 0)
    {
        var btn = new UIE.Button();
        btn.AddToClassList("battle-cmd-btn");
        UIHelper.ApplyFont(btn);
        btn.text = label;

        // アーチ配置: 中央が高く、両端が低い
        float[] archOffsets = { 12f, -4f, -4f, 12f };
        btn.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, archOffsets[index]));

        // 中央2ボタンを少し大きくする
        if (index == 1 || index == 2)
            btn.AddToClassList("battle-cmd-btn-center");

        // インナーハイライト（ぷっくり3D感）
        var highlight = new UIE.VisualElement();
        highlight.AddToClassList("battle-cmd-highlight");
        highlight.pickingMode = UIE.PickingMode.Ignore;
        btn.Add(highlight);

        btn.clicked += () =>
        {
            if (seSource != null && seTap != null)
            {
                seSource.pitch = Random.Range(0.96f, 1.08f);
                seSource.PlayOneShot(seTap, 0.7f);
            }
            // ぷにっとバウンス + キラキラ
            StartCoroutine(PuniBounceEffect(btn));
            StartCoroutine(ButtonSparkleEffect(btn));
            onClick();
        };
        parent.Add(btn);
    }

    IEnumerator PuniBounceEffect(UIE.VisualElement btn)
    {
        if (btn == null) yield break;
        // ぷにっと縮む
        float shrinkDur = 0.06f;
        float elapsed = 0f;
        while (elapsed < shrinkDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shrinkDur);
            float s = Mathf.Lerp(1f, 0.88f, t);
            btn.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
        // 弾んで戻る
        float bounceDur = 0.15f;
        elapsed = 0f;
        while (elapsed < bounceDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounceDur);
            // overshoot: 0.88 → 1.08 → 1.0
            float s;
            if (t < 0.5f)
                s = Mathf.Lerp(0.88f, 1.08f, t * 2f);
            else
                s = Mathf.Lerp(1.08f, 1f, (t - 0.5f) * 2f);
            btn.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            yield return null;
        }
        btn.style.scale = new UIE.StyleScale(new UIE.Scale(Vector2.one));
    }

    IEnumerator ButtonSparkleEffect(UIE.VisualElement btn)
    {
        if (btn == null) yield break;
        var parent = btn.parent;
        if (parent == null) yield break;

        var layout = btn.layout;
        float cx = layout.x + layout.width * 0.5f;
        float cy = layout.y + layout.height * 0.5f;

        var container = new UIE.VisualElement();
        container.pickingMode = UIE.PickingMode.Ignore;
        container.style.position = UIE.Position.Absolute;
        container.style.left = 0; container.style.top = 0;
        container.style.right = 0; container.style.bottom = 0;
        container.style.overflow = UIE.Overflow.Visible;
        parent.Add(container);

        int count = 5;
        var sparkles = new List<(UIE.Label el, float vx, float vy)>();
        for (int i = 0; i < count; i++)
        {
            var sp = new UIE.Label();
            sp.pickingMode = UIE.PickingMode.Ignore;
            sp.text = "\u2728";
            sp.style.position = UIE.Position.Absolute;
            sp.style.fontSize = Random.Range(14, 24);
            sp.style.left = cx + Random.Range(-30f, 30f);
            sp.style.top = cy;
            container.Add(sp);

            float vx = Random.Range(-60f, 60f);
            float vy = Random.Range(-180f, -80f);
            sparkles.Add((sp, vx, vy));
        }

        float duration = 0.6f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < sparkles.Count; i++)
            {
                var (el, vx, vy) = sparkles[i];
                el.style.left = cx + Random.Range(-30f, 30f) + vx * t;
                el.style.top = cy + vy * t;
                el.style.opacity = 1f - t;
            }
            yield return null;
        }
        container.RemoveFromHierarchy();
    }

    IEnumerator ResultSparkleEffect(UIE.VisualElement card)
    {
        if (card == null) yield break;
        var parent = card.parent;
        if (parent == null) yield break;

        var layout = card.layout;
        float cx = layout.x + layout.width * 0.5f;
        float cy = layout.y + layout.height * 0.5f;

        var container = new UIE.VisualElement();
        container.pickingMode = UIE.PickingMode.Ignore;
        container.style.position = UIE.Position.Absolute;
        container.style.left = 0; container.style.top = 0;
        container.style.right = 0; container.style.bottom = 0;
        container.style.overflow = UIE.Overflow.Visible;
        parent.Add(container);

        string[] symbols = { "\u2728", "\U0001F497", "\u2728", "\U0001F496", "\u2728", "\U0001F497", "\u2728", "\U0001F496" };
        int count = 8;
        var particles = new List<(UIE.Label el, float vx, float vy)>();
        for (int i = 0; i < count; i++)
        {
            var p = new UIE.Label();
            p.pickingMode = UIE.PickingMode.Ignore;
            p.text = symbols[i];
            p.style.position = UIE.Position.Absolute;
            p.style.fontSize = Random.Range(18, 30);
            p.style.left = cx + Random.Range(-40f, 40f);
            p.style.top = cy + Random.Range(-20f, 20f);
            container.Add(p);

            float angle = (360f / count * i + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
            float speed = Random.Range(120f, 260f);
            particles.Add((p, Mathf.Cos(angle) * speed, -Mathf.Sin(angle) * speed));
        }

        float duration = 0.7f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < particles.Count; i++)
            {
                var (el, vx, vy) = particles[i];
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                el.style.left = cx + Random.Range(-40f, 40f) + vx * eased;
                el.style.top = cy + Random.Range(-20f, 20f) + vy * eased;
                el.style.opacity = 1f - t;
            }
            yield return null;
        }
        container.RemoveFromHierarchy();
    }

    IEnumerator HappyBounceLoop(UIE.VisualElement el)
    {
        if (el == null) yield break;
        float cycle = 1.6f;
        while (el.parent != null)
        {
            float elapsed = 0f;
            while (elapsed < cycle && el.parent != null)
            {
                elapsed += Time.deltaTime;
                float t = (elapsed / cycle) * Mathf.PI * 2f;
                float sx = 1f + Mathf.Sin(t) * 0.04f;
                float sy = 1f + Mathf.Cos(t) * 0.03f;
                el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(sx, sy)));
                yield return null;
            }
        }
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
            autoBattleBtn.text = "オート：ON";
            // 現在ターン中なら即座に攻撃
            if (waitingForAction) OnAttack();
        }
        else
        {
            autoBattleBtn.RemoveFromClassList("battle-auto-on");
            autoBattleBtn.AddToClassList("battle-auto-off");
            autoBattleBtn.text = "オート \u25B6\u25B6";
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
        playerHpLabel.text = $"ごきげん:{playerHp}/{playerMaxHp}";

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

    IEnumerator ShowBattleTutorial()
    {
        if (overlayRoot == null) yield break;

        string[] tips = new[] {
            "はじめてのバトル！\nあいてと「あそび」でしょうぶしよう",
            "「あそぶ」でこうげき！\nあいてのメーターをゼロにしたら かち",
            "「とくぎ」はとくべつなわざ。\nかいふくやつよいこうげきがあるよ",
            "「おもちゃ」はアイテムをつかえるよ。\nそうびすると バトルがラクになる！",
            "「みまもる」はぼうぎょ。\nダメージがへって すこしかいふくするよ",
            "ごきげん（HP）がゼロにならないように\nじょうずにたたかおう！",
        };

        var overlay = new UIE.VisualElement();
        overlay.style.position = UIE.Position.Absolute;
        overlay.style.left = 0; overlay.style.top = 0;
        overlay.style.right = 0; overlay.style.bottom = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.55f);
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(overlay);

        var card = new UIE.VisualElement();
        card.style.width = 860;
        card.style.backgroundColor = new Color(1f, 0.98f, 0.94f, 0.97f);
        card.style.borderTopLeftRadius = 48;
        card.style.borderTopRightRadius = 48;
        card.style.borderBottomLeftRadius = 48;
        card.style.borderBottomRightRadius = 48;
        card.style.paddingTop = 36;
        card.style.paddingBottom = 32;
        card.style.paddingLeft = 32;
        card.style.paddingRight = 32;
        card.style.alignItems = UIE.Align.Center;
        overlay.Add(card);

        var titleLabel = UIHelper.CreateLabel("あそびかた");
        UIHelper.ApplyFontBold(titleLabel);
        titleLabel.style.fontSize = 40;
        titleLabel.style.color = new Color(0.47f, 0.22f, 0.33f);
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.marginBottom = 20;
        card.Add(titleLabel);

        var textLabel = UIHelper.CreateLabel("");
        UIHelper.ApplyFont(textLabel);
        textLabel.style.fontSize = 32;
        textLabel.style.color = new Color(0.2f, 0.15f, 0.1f);
        textLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        textLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
        textLabel.style.marginBottom = 24;
        textLabel.style.minHeight = 100;
        card.Add(textLabel);

        var pageLabel = UIHelper.CreateLabel("");
        UIHelper.ApplyFont(pageLabel);
        pageLabel.style.fontSize = 24;
        pageLabel.style.color = new Color(0.6f, 0.5f, 0.4f);
        pageLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        pageLabel.style.marginBottom = 16;
        card.Add(pageLabel);

        var nextBtn = new UIE.Button();
        nextBtn.style.width = 400;
        nextBtn.style.height = 80;
        nextBtn.style.borderTopLeftRadius = 40;
        nextBtn.style.borderTopRightRadius = 40;
        nextBtn.style.borderBottomLeftRadius = 40;
        nextBtn.style.borderBottomRightRadius = 40;
        nextBtn.style.backgroundColor = new Color(1f, 0.718f, 0.773f);
        nextBtn.style.borderTopWidth = 0;
        nextBtn.style.borderBottomWidth = 0;
        nextBtn.style.borderLeftWidth = 0;
        nextBtn.style.borderRightWidth = 0;
        nextBtn.style.fontSize = 30;
        nextBtn.style.color = Color.white;
        nextBtn.style.unityTextAlign = TextAnchor.MiddleCenter;
        UIHelper.ApplyFontBold(nextBtn);
        card.Add(nextBtn);

        for (int i = 0; i < tips.Length; i++)
        {
            textLabel.text = tips[i];
            pageLabel.text = $"{i + 1} / {tips.Length}";
            nextBtn.text = (i < tips.Length - 1) ? "つぎへ" : "バトルかいし！";

            bool tapped = false;
            System.Action handler = () => tapped = true;
            nextBtn.clicked += handler;
            while (!tapped) yield return null;
            nextBtn.clicked -= handler;
        }

        overlay.RemoveFromHierarchy();
    }

    IEnumerator IntroHeartBurstEffect()
    {
        if (overlayRoot == null) yield break;

        var container = new UIE.VisualElement();
        container.pickingMode = UIE.PickingMode.Ignore;
        container.style.position = UIE.Position.Absolute;
        container.style.left = 0; container.style.top = 0;
        container.style.right = 0; container.style.bottom = 0;
        container.style.overflow = UIE.Overflow.Visible;
        overlayRoot.Add(container);

        string[] symbols = { "\u2665", "\U0001F497", "\U0001F496", "\u2665", "\U0001F497" };
        Color[] colors = {
            new Color(1f, 0.72f, 0.77f),
            new Color(1f, 0.44f, 0.56f),
            new Color(1f, 0.84f, 0.88f),
            new Color(0.67f, 0.94f, 0.82f),
            new Color(1f, 0.92f, 0.81f),
        };

        int count = 12;
        float centerX = 540f;
        float centerY = 960f;
        var hearts = new List<(UIE.Label el, float vx, float vy)>();

        for (int i = 0; i < count; i++)
        {
            var h = new UIE.Label();
            h.pickingMode = UIE.PickingMode.Ignore;
            h.text = symbols[i % symbols.Length];
            h.style.position = UIE.Position.Absolute;
            h.style.fontSize = Random.Range(24, 48);
            h.style.color = colors[i % colors.Length];
            h.style.left = centerX;
            h.style.top = centerY;
            h.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            h.style.opacity = 0f;
            container.Add(h);

            float angle = (360f / count * i + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            float speed = Random.Range(300f, 500f);
            hearts.Add((h, Mathf.Cos(angle) * speed, -Mathf.Sin(angle) * speed));
        }

        float duration = 0.8f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float fadeIn = Mathf.Clamp01(t * 4f);
            float fadeOut = t > 0.5f ? 1f - (t - 0.5f) * 2f : 1f;

            for (int i = 0; i < hearts.Count; i++)
            {
                var (el, vx, vy) = hearts[i];
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                el.style.left = centerX + vx * eased;
                el.style.top = centerY + vy * eased;
                el.style.opacity = fadeIn * Mathf.Max(0f, fadeOut);
                float s = Mathf.Lerp(0.3f, 1.2f, eased);
                el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            }
            yield return null;
        }

        container.RemoveFromHierarchy();
    }

    IEnumerator EncounterSparkleEffect(UIE.VisualElement parent)
    {
        if (parent == null) yield break;

        var container = new UIE.VisualElement();
        container.pickingMode = UIE.PickingMode.Ignore;
        container.style.position = UIE.Position.Absolute;
        container.style.left = 0; container.style.top = 0;
        container.style.right = 0; container.style.bottom = 0;
        container.style.overflow = UIE.Overflow.Visible;
        parent.Add(container);

        string[] symbols = { "✨", "💗", "⭐", "💖", "✨", "💗" };
        Color[] colors = {
            new Color(1f, 0.72f, 0.77f),
            new Color(1f, 0.84f, 0.88f),
            new Color(1f, 0.92f, 0.81f),
            new Color(0.67f, 0.94f, 0.82f),
            new Color(1f, 0.60f, 0.70f),
            new Color(1f, 0.80f, 0.65f),
        };

        int count = 10;
        float centerX = 540f;
        float centerY = 960f;
        var particles = new List<(UIE.Label el, float vx, float vy)>();

        for (int i = 0; i < count; i++)
        {
            var p = new UIE.Label();
            p.pickingMode = UIE.PickingMode.Ignore;
            p.text = symbols[i % symbols.Length];
            p.style.position = UIE.Position.Absolute;
            p.style.fontSize = Random.Range(20, 40);
            p.style.color = colors[i % colors.Length];
            p.style.left = centerX;
            p.style.top = centerY;
            p.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            p.style.opacity = 0f;
            container.Add(p);

            float angle = (360f / count * i + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
            float speed = Random.Range(250f, 450f);
            particles.Add((p, Mathf.Cos(angle) * speed, -Mathf.Sin(angle) * speed));
        }

        float duration = 0.9f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float fadeIn = Mathf.Clamp01(t * 5f);
            float fadeOut = t > 0.4f ? 1f - (t - 0.4f) / 0.6f : 1f;

            for (int i = 0; i < particles.Count; i++)
            {
                var (el, vx, vy) = particles[i];
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                el.style.left = centerX + vx * eased;
                el.style.top = centerY + vy * eased;
                el.style.opacity = fadeIn * Mathf.Max(0f, fadeOut);
                float s = Mathf.Lerp(0.4f, 1.0f, eased);
                el.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector2(s, s)));
            }
            yield return null;
        }

        container.RemoveFromHierarchy();
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

        // ぽんっとアタッカーが跳ねる
        float direction = (attacker == playerPanel) ? 60f : -60f;

        // 軽くジャンプ前進
        float lungeDuration = 0.12f;
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lungeDuration;
            float bounce = Mathf.Sin(t * Mathf.PI) * -20f;
            attacker.style.translate = new UIE.StyleTranslate(new UIE.Translate(direction * t, bounce));
            yield return null;
        }

        // ヒット時: ハートぽんっ + ターゲットバウンス
        StartCoroutine(HeartPopEffect(target, isSpecial));
        StartCoroutine(BounceTargetEffect(target, isSpecial));
        if (isSpecial)
            StartCoroutine(FlashEffect(new Color(1f, 0.72f, 0.77f), 0.3f)); // ピンクフラッシュ

        // 戻り
        elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lungeDuration;
            attacker.style.translate = new UIE.StyleTranslate(new UIE.Translate(direction * (1f - t), 0));
            yield return null;
        }
        attacker.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));

        yield return new WaitForSeconds(isSpecial ? 0.4f : 0.25f);
    }

    IEnumerator HeartPopEffect(UIE.VisualElement target, bool isSpecial)
    {
        if (target == null) yield break;
        var parent = target.parent;
        if (parent == null) yield break;

        var layout = target.layout;
        float cx = layout.x + layout.width * 0.5f;
        float cy = layout.y + layout.height * 0.3f;

        int count = isSpecial ? 8 : 4;
        string symbol = isSpecial ? "\U0001F496" : "\U0001F497";
        Color[] colors = {
            new Color(1f, 0.72f, 0.77f),
            new Color(1f, 0.44f, 0.56f),
            new Color(1f, 0.84f, 0.88f),
            new Color(0.67f, 0.94f, 0.82f),
        };

        var container = new UIE.VisualElement();
        container.pickingMode = UIE.PickingMode.Ignore;
        container.style.position = UIE.Position.Absolute;
        container.style.left = 0; container.style.top = 0;
        container.style.right = 0; container.style.bottom = 0;
        container.style.overflow = UIE.Overflow.Visible;
        parent.Add(container);

        var hearts = new List<(UIE.Label el, float vx, float vy)>();
        for (int i = 0; i < count; i++)
        {
            var h = new UIE.Label();
            h.pickingMode = UIE.PickingMode.Ignore;
            h.text = symbol;
            h.style.position = UIE.Position.Absolute;
            h.style.fontSize = isSpecial ? Random.Range(28, 42) : Random.Range(20, 32);
            h.style.color = colors[i % colors.Length];
            h.style.left = cx;
            h.style.top = cy;
            h.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            container.Add(h);

            float angle = (360f / count * i + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
            float speed = Random.Range(150f, 300f);
            hearts.Add((h, Mathf.Cos(angle) * speed, -Mathf.Sin(angle) * speed));
        }

        // キラキラ追加（必殺技のみ）
        if (isSpecial)
        {
            for (int i = 0; i < 6; i++)
            {
                var sparkle = new UIE.Label();
                sparkle.pickingMode = UIE.PickingMode.Ignore;
                sparkle.text = "\u2728";
                sparkle.style.position = UIE.Position.Absolute;
                sparkle.style.fontSize = Random.Range(16, 28);
                sparkle.style.left = cx;
                sparkle.style.top = cy;
                container.Add(sparkle);

                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float speed = Random.Range(100f, 250f);
                hearts.Add((sparkle, Mathf.Cos(angle) * speed, -Mathf.Sin(angle) * speed));
            }
        }

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            for (int i = 0; i < hearts.Count; i++)
            {
                var (el, vx, vy) = hearts[i];
                float eased = 1f - Mathf.Pow(1f - t, 2f);
                el.style.left = cx + vx * eased;
                el.style.top = cy + vy * eased;
                el.style.opacity = 1f - t;
            }
            yield return null;
        }
        container.RemoveFromHierarchy();
    }

    IEnumerator BounceTargetEffect(UIE.VisualElement target, bool isSpecial)
    {
        if (target == null) yield break;
        float bounceDuration = isSpecial ? 0.35f : 0.25f;
        float elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / bounceDuration);
            // ぷるんとバウンス: scaleY 0.85→1.12→1.0
            float sy, sx;
            if (t < 0.3f)
            {
                float p = t / 0.3f;
                sy = Mathf.Lerp(1f, 0.85f, p);
                sx = Mathf.Lerp(1f, 1.1f, p);
            }
            else if (t < 0.6f)
            {
                float p = (t - 0.3f) / 0.3f;
                sy = Mathf.Lerp(0.85f, 1.12f, p);
                sx = Mathf.Lerp(1.1f, 0.95f, p);
            }
            else
            {
                float p = (t - 0.6f) / 0.4f;
                sy = Mathf.Lerp(1.12f, 1f, p);
                sx = Mathf.Lerp(0.95f, 1f, p);
            }
            target.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(sx, sy, 1f)));
            yield return null;
        }
        target.style.scale = new UIE.StyleScale(new UIE.Scale(Vector3.one));
    }

    IEnumerator DamageEffect(UIE.VisualElement faceEl, UIE.VisualElement panel, bool isHeavy = false)
    {
        if (faceEl == null) yield break;

        // ピンクフラッシュ（軽め）
        float flashOpacity = isHeavy ? 0.35f : 0.2f;
        if (flashOverlay != null)
        {
            flashOverlay.style.backgroundColor = new Color(1f, 0.72f, 0.77f);
            flashOverlay.style.opacity = flashOpacity;
        }

        // ぷるぷる震え（小刻み、かわいい揺れ）
        if (panel != null)
        {
            float wobbleDur = isHeavy ? 0.35f : 0.2f;
            StartCoroutine(WobbleEffect(panel, wobbleDur, isHeavy ? 4f : 2.5f));
        }

        // スプライト: ピンク点滅（痛くない表現）
        for (int i = 0; i < 3; i++)
        {
            faceEl.style.unityBackgroundImageTintColor = new Color(1f, 0.75f, 0.8f);
            faceEl.style.opacity = 0.6f;
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

    IEnumerator WobbleEffect(UIE.VisualElement target, float duration, float magnitude)
    {
        if (target == null) yield break;
        float elapsed = 0f;
        float speed = 30f;
        while (elapsed < duration)
        {
            float x = Mathf.Sin(elapsed * speed) * magnitude * (1f - elapsed / duration);
            target.style.translate = new UIE.StyleTranslate(new UIE.Translate(x, 0));
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));
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
            p.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
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

    IEnumerator SleepyDefeatEffect(UIE.VisualElement panel)
    {
        if (panel == null) yield break;

        var parent = panel.parent;
        if (parent == null) yield break;

        var layout = panel.layout;
        float panelCenterX = layout.x + layout.width * 0.5f;
        float panelCenterY = layout.y;

        // 💤 を浮かべるコンテナ
        var zzContainer = new UIE.VisualElement();
        zzContainer.pickingMode = UIE.PickingMode.Ignore;
        zzContainer.style.position = UIE.Position.Absolute;
        zzContainer.style.left = 0; zzContainer.style.top = 0;
        zzContainer.style.right = 0; zzContainer.style.bottom = 0;
        zzContainer.style.overflow = UIE.Overflow.Visible;
        parent.Add(zzContainer);

        // Phase 1: ゆらゆら揺れ (0.6s)
        float wobbleDur = 0.6f;
        float elapsed = 0f;
        while (elapsed < wobbleDur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / wobbleDur;
            float wobble = Mathf.Sin(t * Mathf.PI * 4f) * 5f * (1f - t);
            panel.style.translate = new UIE.StyleTranslate(new UIE.Translate(wobble, 0));
            yield return null;
        }

        // Phase 2: 💤 を3つ順番に浮かべる
        for (int i = 0; i < 3; i++)
        {
            var zz = new UIE.Label();
            zz.pickingMode = UIE.PickingMode.Ignore;
            zz.text = "\U0001F4A4";
            zz.style.position = UIE.Position.Absolute;
            zz.style.fontSize = 28 + i * 6;
            zz.style.left = panelCenterX + (i - 1) * 30f;
            zz.style.top = panelCenterY;
            zz.style.opacity = 0f;
            zzContainer.Add(zz);

            float zzDur = 0.4f;
            float zzElapsed = 0f;
            while (zzElapsed < zzDur)
            {
                zzElapsed += Time.deltaTime;
                float zt = Mathf.Clamp01(zzElapsed / zzDur);
                zz.style.top = panelCenterY - 60f * zt;
                zz.style.opacity = Mathf.Sin(zt * Mathf.PI);
                yield return null;
            }
        }

        // Phase 3: ゆっくり傾いてフェードアウト (1.0s)
        float fallDur = 1.0f;
        elapsed = 0f;
        while (elapsed < fallDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDur);
            float ease = t * t;
            float angle = ease * 15f;
            panel.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(angle, UIE.AngleUnit.Degree)));
            panel.style.opacity = 1f - ease;
            panel.style.translate = new UIE.StyleTranslate(new UIE.Translate(ease * 30f, ease * 20f));
            yield return null;
        }

        panel.style.opacity = 0f;
        panel.style.visibility = UIE.Visibility.Hidden;
        zzContainer.RemoveFromHierarchy();
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
        bool isEgoMother = enemyName == "エゴ・マザー・マシーン";

        if (isEgoMother)
        {
            // エゴマザー: エンカウント画面をスキップ（MapSceneの降臨演出で十分）
        }
        else
        {
            encounterOverlay.Add(encounterImg);

            var encounterPlate = new UIE.VisualElement();
            encounterPlate.AddToClassList("battle-encounter-plate");
            encounterPlate.pickingMode = UIE.PickingMode.Ignore;
            var encounterText = new UIE.Label();
            encounterText.AddToClassList("battle-encounter-text");
            UIHelper.ApplyFontBold(encounterText);
            encounterText.text = Localization.GetEnemy(enemyName) + " が あそびにきたよ！ ✨";
            encounterPlate.Add(encounterText);
            encounterOverlay.Add(encounterPlate);

            overlayRoot.Add(encounterOverlay);

            // 通常エンカウント演出
            StartCoroutine(EncounterSparkleEffect(encounterOverlay));

            yield return null;
            yield return StartCoroutine(BounceInEffect(encounterImg));

            encounterPlate.AddToClassList("battle-encounter-plate-visible");

            yield return new WaitForSeconds(0.8f);

            encounterOverlay.AddToClassList("battle-encounter-overlay-hide");
            yield return new WaitForSeconds(0.5f);
            encounterOverlay.RemoveFromHierarchy();
        }

        // ボス戦前イントロ台詞
        bool isBossStart = DataCarrier.Instance != null && DataCarrier.Instance.isBossBattle;
        if (isBossStart && enemyName == "デヴィル夫人")
        {
            yield return StartCoroutine(ShowDevilLadyIntro());
        }
        // エゴ・マザー・マシーン: イントロカット（MapSceneの降臨演出で十分）

        // バトルUI フェードイン
        battleBgEl.AddToClassList("battle-ui-fadein");
        enemyAreaEl.AddToClassList("battle-ui-fadein");
        playerAreaEl.AddToClassList("battle-ui-fadein");
        centerAreaEl.AddToClassList("battle-ui-fadein");
        battleBgEl.style.opacity = 1;
        enemyAreaEl.style.opacity = 1;
        playerAreaEl.style.opacity = 1;
        centerAreaEl.style.opacity = 1;

        // バトルイントロ演出: ハート放射
        yield return StartCoroutine(IntroHeartBurstEffect());

        // BGM再生
        if (bgmSource != null && bgmSource.clip != null)
            bgmSource.Play();

        // 最初のバトル（BirthScene直後）ならチュートリアル表示
        bool isFirstBattle = DataCarrier.Instance != null && !DataCarrier.Instance.cameFromMap;
        if (isFirstBattle)
            yield return StartCoroutine(ShowBattleTutorial());

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

        // かぐやちゃんの応援（15%の確率でHP+5）
        if (DataCarrier.Instance != null && DataCarrier.Instance.kaguyaLover && Random.Range(0, 100) < 15)
        {
            playerHp = Mathf.Min(playerMaxHp, playerHp + 5);
            UpdatePlayerDisplay();
            battleLogLabel.text = Localization.Get("kaguya_cheer");
            yield return new WaitForSeconds(0.8f);
        }

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

        StartCoroutine(FlashEffect(new Color(1f, 0.72f, 0.77f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogLabel.text = Localization.Get("battle_shiba_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, true));

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

    IEnumerator ShowEgoMotherIntro()
    {
        var introOverlay = new UIE.VisualElement();
        introOverlay.AddToClassList("fill");
        introOverlay.style.flexDirection = UIE.FlexDirection.Column;
        introOverlay.style.justifyContent = UIE.Justify.Center;
        introOverlay.style.alignItems = UIE.Align.Center;
        introOverlay.style.backgroundColor = new Color(0.02f, 0.01f, 0.06f, 0.95f);
        overlayRoot.Add(introOverlay);

        var introText = new UIE.Label();
        introText.enableRichText = true;
        introText.style.color = new Color(1f, 0.84f, 0f);
        introText.style.fontSize = 32;
        introText.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        introText.style.whiteSpace = UIE.WhiteSpace.Normal;
        introText.style.width = 900;
        introText.style.letterSpacing = 4;
        UIHelper.ApplyFont(introText);
        introOverlay.Add(introText);

        string[] lines = new string[]
        {
            Localization.Get("battle_ego_mother_intro_1"),
            Localization.Get("battle_ego_mother_intro_2"),
            Localization.Get("battle_ego_mother_intro_3"),
        };

        foreach (var line in lines)
        {
            introText.text = line;
            introText.style.opacity = 0f;
            float fadeIn = 0f;
            while (fadeIn < 0.6f)
            {
                fadeIn += Time.deltaTime;
                introText.style.opacity = Mathf.Clamp01(fadeIn / 0.6f);
                yield return null;
            }
            yield return new WaitForSeconds(2f);
            float fadeOut = 0f;
            while (fadeOut < 0.4f)
            {
                fadeOut += Time.deltaTime;
                introText.style.opacity = 1f - Mathf.Clamp01(fadeOut / 0.4f);
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
        }

        // 最終フェードアウト
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            introOverlay.style.opacity = 1f - Mathf.Clamp01(elapsed / 0.5f);
            yield return null;
        }
        introOverlay.RemoveFromHierarchy();
    }

    IEnumerator EnemyDoDevilLadyUltimate()
    {
        battleLogLabel.text = Localization.Get("battle_devil_lady_ultimate");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(FlashEffect(new Color(1f, 0.72f, 0.77f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogLabel.text = Localization.Get("battle_devil_lady_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, true));

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

        StartCoroutine(FlashEffect(new Color(1f, 0.72f, 0.77f), 0.5f));
        yield return new WaitForSeconds(0.3f);

        battleLogLabel.text = Localization.Get("battle_melodias_ultimate_name");
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(AttackAnimation(enemyPanel, playerPanel, true));

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
        bool isFixedWin = cannotRun || enemyName == "かぐやちゃん";
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

        // お祝いタイトル（ぷるん揺れアニメーション付き）
        var happyTitle = UIHelper.CreateLabel("\U0001F38A HAPPY! \U0001F38A", "battle-result-happy");
        UIHelper.ApplyFontBold(happyTitle);
        overlay.Add(happyTitle);

        var title = UIHelper.CreateLabel(Localization.Get("battle_enemy_defeated", Localization.GetEnemy(enemyName)), "battle-result-title");
        overlay.Add(title);

        // 報酬カード
        var card = new UIE.VisualElement();
        card.AddToClassList("battle-result-card");
        overlay.Add(card);

        // EXP行（アイコン付き）
        var expRow = new UIE.VisualElement();
        expRow.AddToClassList("battle-result-row");
        var expLabel = UIHelper.CreateLabel("\u2728 " + Localization.Get("battle_result_exp"), "battle-result-row-label");
        var expValue = UIHelper.CreateLabel("+0", "battle-result-row-value");
        expValue.AddToClassList("battle-result-value-accent");
        expRow.Add(expLabel);
        expRow.Add(expValue);
        card.Add(expRow);

        // ミルク行（アイコン付き）
        var milkRow = new UIE.VisualElement();
        milkRow.AddToClassList("battle-result-row");
        var milkLabel = UIHelper.CreateLabel("\U0001F37C " + Localization.Get("battle_result_milk"), "battle-result-row-label");
        var milkValue = UIHelper.CreateLabel("+0", "battle-result-row-value");
        milkValue.AddToClassList("battle-result-value-sub");
        milkRow.Add(milkLabel);
        milkRow.Add(milkValue);
        card.Add(milkRow);

        // EXPバー or レベルアップ表示
        UIE.VisualElement barFill = null;
        float targetRatio = 0f;
        if (!willLevelUp)
        {
            var barLabel = UIHelper.CreateLabel($"\u2728 おもいで  {currentExp} / {needed}", "battle-result-bar-label");
            card.Add(barLabel);

            var barBg = new UIE.VisualElement();
            barBg.AddToClassList("battle-result-bar-bg");
            barFill = new UIE.VisualElement();
            barFill.AddToClassList("battle-result-bar-fill");
            barFill.style.width = new UIE.Length(0, UIE.LengthUnit.Percent);
            barBg.Add(barFill);
            card.Add(barBg);
            targetRatio = Mathf.Clamp01((float)currentExp / needed);
        }
        else
        {
            var lvUpLabel = UIHelper.CreateLabel(Localization.Get("battle_age_up", currentAge + 1), "battle-result-levelup");
            card.Add(lvUpLabel);
        }

        // overlay表示
        overlayRoot.Add(overlay);
        yield return null;

        // HAPPY! ぷるん揺れアニメーション
        StartCoroutine(HappyBounceLoop(happyTitle));

        // カウントアップアニメーション (0.6s)
        float countDur = 0.6f;
        float countElapsed = 0f;
        while (countElapsed < countDur)
        {
            countElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(countElapsed / countDur);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            int dispExp = Mathf.RoundToInt(eased * expGained);
            int dispMilk = Mathf.RoundToInt(eased * milkGained);
            expValue.text = $"+{dispExp}";
            milkValue.text = $"+{dispMilk}";
            yield return null;
        }
        expValue.text = $"+{expGained}";
        milkValue.text = $"+{milkGained}";

        // カウントアップ完了時キラキラ
        StartCoroutine(ResultSparkleEffect(card));

        // EXPバーアニメーション
        if (barFill != null)
        {
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

        // OKボタン（ぷっくり質感）
        bool dismissed = false;
        var okBtn = new UIE.Button();
        okBtn.AddToClassList("battle-result-ok-btn");
        UIHelper.ApplyFontBold(okBtn);
        okBtn.text = "OK";
        okBtn.style.opacity = 0f;
        okBtn.clicked += () =>
        {
            if (seSource != null && seTap != null)
            {
                seSource.pitch = Random.Range(0.96f, 1.08f);
                seSource.PlayOneShot(seTap, 0.7f);
            }
            StartCoroutine(PuniBounceEffect(okBtn));
            dismissed = true;
        };
        // インナーハイライト
        var okHighlight = new UIE.VisualElement();
        okHighlight.AddToClassList("battle-cmd-highlight");
        okHighlight.pickingMode = UIE.PickingMode.Ignore;
        okBtn.Add(okHighlight);
        overlay.Add(okBtn);

        yield return new WaitForSeconds(0.3f);
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
    }

    IEnumerator BattleLose()
    {
        isBattleActive = false;
        StartCoroutine(FadeOutBGM(1.5f));

        // おやすみなさい演出（💤フェードアウト）
        yield return StartCoroutine(SleepyDefeatEffect(playerPanel));
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
        // かぐやちゃん勝利 → 告白演出（選択肢あり）
        if (enemyName == "かぐやちゃん" && DataCarrier.Instance != null)
        {
            yield return StartCoroutine(KaguyaConfessionSequence());
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
            SceneManager.LoadScene("MapScene");
            yield break;
        }

        yield return new WaitForSeconds(1.5f);

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
        }

        SceneManager.LoadScene("MapScene");
    }

    IEnumerator KaguyaConfessionSequence()
    {
        // 全画面オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(1f, 0.72f, 0.77f, 0f);
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlayRoot.Add(overlay);

        // ピンク背景フェードイン
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(1f, 0.72f, 0.77f, Mathf.Lerp(0f, 0.85f, elapsed / 0.5f));
            yield return null;
        }

        // kaguya3 画像（画面いっぱい）
        var kaguyaImg = new UIE.VisualElement();
        var kSpr = Resources.Load<Sprite>("MapCharacters/heroine/kaguya3");
        if (kSpr != null)
        {
            kaguyaImg.style.backgroundImage = new UIE.StyleBackground(kSpr);
            kaguyaImg.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        kaguyaImg.style.position = UIE.Position.Absolute;
        kaguyaImg.style.top = 0;
        kaguyaImg.style.bottom = 0;
        kaguyaImg.style.left = 0;
        kaguyaImg.style.right = 0;
        kaguyaImg.style.opacity = 0f;
        overlay.Add(kaguyaImg);

        // 画像フェードイン
        elapsed = 0f;
        while (elapsed < 0.8f)
        {
            elapsed += Time.deltaTime;
            kaguyaImg.style.opacity = elapsed / 0.8f;
            yield return null;
        }
        kaguyaImg.style.opacity = 1f;

        // ハートを散らす
        var hearts = new System.Collections.Generic.List<UIE.VisualElement>();
        for (int i = 0; i < 15; i++)
        {
            var heart = UIHelper.CreateLabel("\u2764");
            heart.style.fontSize = 24 + Random.Range(0, 24);
            heart.style.position = UIE.Position.Absolute;
            heart.style.left = Random.Range(30, 620);
            heart.style.top = Random.Range(80, 1000);
            heart.style.color = new UIE.StyleColor(new Color(1f, Random.Range(0.3f, 0.6f), Random.Range(0.5f, 0.8f), 0f));
            overlay.Add(heart);
            hearts.Add(heart);
        }
        StartCoroutine(AnimateConfessionHearts(hearts));

        // 吹き出し
        var bubble = new UIE.VisualElement();
        bubble.style.position = UIE.Position.Absolute;
        bubble.style.bottom = 140;
        bubble.style.left = new UIE.Length(5, UIE.LengthUnit.Percent);
        bubble.style.right = new UIE.Length(5, UIE.LengthUnit.Percent);
        bubble.style.backgroundColor = new Color(1f, 1f, 1f, 0.92f);
        bubble.style.borderTopLeftRadius = 36;
        bubble.style.borderTopRightRadius = 36;
        bubble.style.borderBottomLeftRadius = 36;
        bubble.style.borderBottomRightRadius = 36;
        bubble.style.paddingTop = 20;
        bubble.style.paddingBottom = 20;
        bubble.style.paddingLeft = 28;
        bubble.style.paddingRight = 28;
        bubble.style.alignItems = UIE.Align.FlexStart;
        overlay.Add(bubble);

        // 吹き出しのしっぽ
        var bubbleTail = new UIE.VisualElement();
        bubbleTail.style.position = UIE.Position.Absolute;
        bubbleTail.style.bottom = 126;
        bubbleTail.style.left = new UIE.Length(15, UIE.LengthUnit.Percent);
        bubbleTail.style.width = 0;
        bubbleTail.style.height = 0;
        bubbleTail.style.borderTopWidth = 14;
        bubbleTail.style.borderLeftWidth = 10;
        bubbleTail.style.borderRightWidth = 10;
        bubbleTail.style.borderBottomWidth = 0;
        bubbleTail.style.borderTopColor = new Color(1f, 1f, 1f, 0.92f);
        bubbleTail.style.borderLeftColor = Color.clear;
        bubbleTail.style.borderRightColor = Color.clear;
        bubbleTail.style.borderBottomColor = Color.clear;
        overlay.Add(bubbleTail);

        var bubbleName = UIHelper.CreateLabel("かぐやちゃん");
        bubbleName.style.fontSize = 24;
        bubbleName.style.color = new Color(1f, 0.4f, 0.6f);
        UIHelper.ApplyFontBold(bubbleName);
        bubbleName.style.marginBottom = 8;
        bubble.Add(bubbleName);

        var bubbleText = UIHelper.CreateLabel("");
        bubbleText.style.fontSize = 30;
        bubbleText.style.color = Color.black;
        bubbleText.style.whiteSpace = UIE.WhiteSpace.Normal;
        bubble.Add(bubbleText);

        var bubbleHint = UIHelper.CreateLabel("");
        bubbleHint.style.fontSize = 20;
        bubbleHint.style.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        bubbleHint.style.marginTop = 8;
        bubbleHint.style.alignSelf = UIE.Align.FlexEnd;
        bubble.Add(bubbleHint);

        // 告白台詞（タップで進む）
        string[] confMsgs = new string[] {
            Localization.Get("kaguya_confess1"),
            Localization.Get("kaguya_confess2"),
            Localization.Get("kaguya_confess3")
        };

        bool tapped = false;
        for (int i = 0; i < confMsgs.Length; i++)
        {
            bubbleText.text = confMsgs[i];
            bubbleHint.text = "\u25bc \u30bf\u30c3\u30d7\u3067\u7d9a\u304f";
            yield return new WaitForSeconds(0.3f);
            tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }

        // --- 選択肢: 付き合う / 付き合わない ---
        bubbleText.text = Localization.Get("kaguya_confess_ask");
        bubbleHint.RemoveFromHierarchy();

        var btnCol = new UIE.VisualElement();
        btnCol.style.flexDirection = UIE.FlexDirection.Column;
        btnCol.style.alignItems = UIE.Align.Center;
        btnCol.style.marginTop = 16;
        btnCol.style.width = new UIE.Length(100, UIE.LengthUnit.Percent);
        bubble.Add(btnCol);

        int choice = -1;
        var yesBtn = UIHelper.CreatePillButton(Localization.Get("kaguya_confess_yes"), "pill-button");
        yesBtn.clicked += () => choice = 1;
        btnCol.Add(yesBtn);

        var noBtn = UIHelper.CreatePillButton(Localization.Get("kaguya_confess_no"), "pill-button");
        noBtn.style.marginTop = 12;
        noBtn.clicked += () => choice = 0;
        btnCol.Add(noBtn);

        while (choice < 0) yield return null;

        if (choice == 1)
        {
            // --- 付き合う → 相思相愛 ---
            btnCol.RemoveFromHierarchy();
            bubbleText.text = Localization.Get("kaguya_confess_happy");
            yield return new WaitForSeconds(0.5f);
            tapped = false;
            var hintYes = UIHelper.CreateLabel("\u25bc \u30bf\u30c3\u30d7\u3067\u7d9a\u304f");
            hintYes.style.fontSize = 20;
            hintYes.style.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            hintYes.style.marginTop = 8;
            hintYes.style.alignSelf = UIE.Align.FlexEnd;
            bubble.Add(hintYes);
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            DataCarrier.Instance.kaguyaLover = true;
            DataCarrier.Instance.AddEquipment("かぐやのリボン");
            DataCarrier.Instance.EquipItem("かぐやのリボン");
            DataCarrier.Instance.SaveData();

            // 相思相愛テキスト
            hintYes.RemoveFromHierarchy();
            bubbleName.text = "";
            bubbleText.text = Localization.Get("kaguya_lover_won");
            bubbleText.style.unityTextAlign = TextAnchor.MiddleCenter;
            UIHelper.ApplyFontBold(bubbleText);
            bubbleText.style.color = new Color(1f, 0.4f, 0.6f);

            yield return new WaitForSeconds(1.5f);

            // リボン取得テキスト
            bubbleText.text = Localization.Get("kaguya_lover_item");
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            // --- 付き合わない → かぐやが悲しんで去る ---
            btnCol.RemoveFromHierarchy();
            bubbleText.text = Localization.Get("kaguya_confess_sad");
            yield return new WaitForSeconds(0.5f);
            tapped = false;
            var hintNo = UIHelper.CreateLabel("\u25bc \u30bf\u30c3\u30d7\u3067\u9589\u3058\u308b");
            hintNo.style.fontSize = 20;
            hintNo.style.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            hintNo.style.marginTop = 8;
            hintNo.style.alignSelf = UIE.Align.FlexEnd;
            bubble.Add(hintNo);
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            // metCountを2に戻してプール再出現可能に
            DataCarrier.Instance.kaguyaMetCount = 2;
            DataCarrier.Instance.SaveData();
        }

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            overlay.style.opacity = 1f - (elapsed / 0.5f);
            yield return null;
        }
        overlay.RemoveFromHierarchy();
    }

    IEnumerator AnimateConfessionHearts(System.Collections.Generic.List<UIE.VisualElement> hearts)
    {
        while (true)
        {
            for (int i = 0; i < hearts.Count; i++)
            {
                var h = hearts[i];
                if (h == null || h.parent == null) yield break;
                float phase = i * Mathf.PI * 2f / hearts.Count;
                float alpha = (Mathf.Sin(Time.time * 1.5f + phase) + 1f) * 0.5f * 0.7f;
                float r = h.resolvedStyle.color.r;
                float g = h.resolvedStyle.color.g;
                float b = h.resolvedStyle.color.b;
                h.style.color = new Color(r, g, b, alpha);
                h.style.top = h.resolvedStyle.top - Time.deltaTime * (10f + i * 3f);
                float drift = Mathf.Sin(Time.time * 2f + phase) * 0.5f;
                h.style.left = h.resolvedStyle.left + drift;
            }
            yield return null;
        }
    }

    // ===== エゴ・マザー・マシーン 専用カットシーン =====
    // 『エゴからの解放』

    IEnumerator EgoMotherDefeatCutscene()
    {
        float scw = 1080f, sch = 1920f;
        float cx = scw * 0.5f, cy = sch * 0.4f;

        // --- Phase 1: 黄金パーティクル放出 ---
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.position = UIE.Position.Absolute;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.pickingMode = UIE.PickingMode.Ignore;
        overlayRoot.Add(overlay);

        // 暗転
        float fadeEl = 0f;
        while (fadeEl < 1f)
        {
            fadeEl += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Clamp01(fadeEl / 1f) * 0.85f);
            yield return null;
        }

        // テキスト: マシーンの崩壊
        var text1 = UIHelper.CreateLabel(
            Localization.Get("ego_mother_defeat_1"), "melodias-cutscene-text");
        UIHelper.ApplyFont(text1);
        text1.style.opacity = 0f;
        overlay.Add(text1);

        fadeEl = 0f;
        while (fadeEl < 0.8f)
        {
            fadeEl += Time.deltaTime;
            text1.style.opacity = Mathf.Clamp01(fadeEl / 0.8f);
            yield return null;
        }
        yield return new WaitForSeconds(2f);

        // 黄金パーティクル放出(銀河のベロア・ハイライトと同色)
        var particleContainer = new UIE.VisualElement();
        particleContainer.pickingMode = UIE.PickingMode.Ignore;
        particleContainer.style.position = UIE.Position.Absolute;
        particleContainer.style.left = 0; particleContainer.style.top = 0;
        particleContainer.style.right = 0; particleContainer.style.bottom = 0;
        overlay.Add(particleContainer);

        // SE: きらきら
        var seKira = Resources.Load<AudioClip>("SE/きらきら輝く6");
        if (seSource != null && seKira != null)
        {
            seSource.pitch = 0.8f;
            seSource.PlayOneShot(seKira, 0.9f);
        }

        var goldParticles = new List<(UIE.VisualElement el, float vx, float vy, float born)>();
        int burstCount = 60;
        for (int i = 0; i < burstCount; i++)
        {
            var p = new UIE.VisualElement();
            float size = Random.Range(4f, 14f);
            p.style.width = size;
            p.style.height = size;
            p.style.borderTopLeftRadius = size;
            p.style.borderTopRightRadius = size;
            p.style.borderBottomLeftRadius = size;
            p.style.borderBottomRightRadius = size;
            p.style.position = UIE.Position.Absolute;
            // 銀河ベロアのハイライト色: 紫-金-ピンクのグラデーション
            float hue = Random.Range(0f, 1f);
            Color pCol;
            if (hue < 0.4f) pCol = new Color(1f, 0.84f, 0f, 0.9f); // 黄金
            else if (hue < 0.7f) pCol = new Color(1f, 0.72f, 0.77f, 0.8f); // パステルピンク
            else pCol = new Color(0.67f, 0.83f, 1f, 0.7f); // 淡い青
            p.style.backgroundColor = pCol;
            p.style.left = cx;
            p.style.top = cy;
            particleContainer.Add(p);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(100f, 400f);
            goldParticles.Add((p, Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f));
        }

        // パーティクルアニメーション 2秒
        float burstDur = 2f;
        fadeEl = 0f;
        while (fadeEl < burstDur)
        {
            fadeEl += Time.deltaTime;
            float t = fadeEl / burstDur;
            for (int i = 0; i < goldParticles.Count; i++)
            {
                var (el, vx, vy, born) = goldParticles[i];
                float curL = el.resolvedStyle.left;
                float curT = el.resolvedStyle.top;
                el.style.left = curL + vx * Time.deltaTime;
                el.style.top = curT + vy * Time.deltaTime;
                el.style.opacity = Mathf.Clamp01(1f - t * 0.8f);
            }
            yield return null;
        }

        yield return StartCoroutine(FadeOutElement(text1, 0.5f));
        text1.RemoveFromHierarchy();

        // --- Phase 2: エゴの呪縛が解ける → 赤ちゃんたちの解放 ---
        var text2 = UIHelper.CreateLabel(
            Localization.Get("ego_mother_defeat_2"), "melodias-cutscene-text");
        UIHelper.ApplyFont(text2);
        text2.style.opacity = 0f;
        overlay.Add(text2);
        fadeEl = 0f;
        while (fadeEl < 0.8f)
        {
            fadeEl += Time.deltaTime;
            text2.style.opacity = Mathf.Clamp01(fadeEl / 0.8f);
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);

        // シルエットの赤ちゃんたちが浮かび上がる → キラキラに変換
        particleContainer.Clear();
        string[] babyEmojis = { "\U0001F476", "\u2728", "\U0001F31F", "\U0001F4AB", "\U0001F49B" };
        var babyEls = new List<(UIE.Label el, float delay)>();

        for (int i = 0; i < 12; i++)
        {
            var baby = new UIE.Label();
            baby.pickingMode = UIE.PickingMode.Ignore;
            baby.text = babyEmojis[i % babyEmojis.Length];
            baby.style.position = UIE.Position.Absolute;
            baby.style.fontSize = Random.Range(28, 48);
            baby.style.left = Random.Range(80f, scw - 80f);
            baby.style.top = Random.Range(sch * 0.3f, sch * 0.7f);
            baby.style.opacity = 0f;
            baby.style.color = Color.white;
            particleContainer.Add(baby);
            babyEls.Add((baby, i * 0.15f));
        }

        // SE: きらきら(高音)
        if (seSource != null && seKira != null)
        {
            seSource.pitch = 1.2f;
            seSource.PlayOneShot(seKira, 0.8f);
        }

        // 赤ちゃん出現アニメーション + 白エフェクト
        float babyDur = 2.5f;
        fadeEl = 0f;
        while (fadeEl < babyDur)
        {
            fadeEl += Time.deltaTime;
            foreach (var (el, delay) in babyEls)
            {
                float localT = fadeEl - delay;
                if (localT < 0f) continue;
                float appear = Mathf.Clamp01(localT / 0.4f);
                el.style.opacity = appear;
                // ゆっくり上昇
                float curTop = el.resolvedStyle.top;
                el.style.top = curTop - 15f * Time.deltaTime;
            }
            yield return null;
        }

        yield return StartCoroutine(FadeOutElement(text2, 0.5f));
        text2.RemoveFromHierarchy();

        // --- Phase 3: 解放のメッセージ ---
        var text3 = UIHelper.CreateLabel(
            Localization.Get("ego_mother_defeat_3"), "melodias-cutscene-text");
        UIHelper.ApplyFont(text3);
        UIHelper.ApplyFontBold(text3);
        text3.style.color = new Color(1f, 0.84f, 0f);
        text3.style.fontSize = 36;
        text3.style.letterSpacing = 6;
        text3.style.opacity = 0f;
        overlay.Add(text3);
        fadeEl = 0f;
        while (fadeEl < 1f)
        {
            fadeEl += Time.deltaTime;
            text3.style.opacity = Mathf.Clamp01(fadeEl / 1f);
            yield return null;
        }
        yield return new WaitForSeconds(2.5f);

        yield return StartCoroutine(FadeOutElement(text3, 0.5f));
        text3.RemoveFromHierarchy();

        // --- Phase 4: カメラ上昇 → 雲突き抜け → 聖域の光 ---
        var text4 = UIHelper.CreateLabel(
            Localization.Get("ego_mother_defeat_4"), "melodias-cutscene-text");
        UIHelper.ApplyFont(text4);
        text4.style.opacity = 0f;
        overlay.Add(text4);
        fadeEl = 0f;
        while (fadeEl < 0.8f)
        {
            fadeEl += Time.deltaTime;
            text4.style.opacity = Mathf.Clamp01(fadeEl / 0.8f);
            yield return null;
        }

        // 赤ちゃんたちが上昇しながら消えていく
        float riseDur = 3f;
        fadeEl = 0f;
        while (fadeEl < riseDur)
        {
            fadeEl += Time.deltaTime;
            float t = fadeEl / riseDur;
            foreach (var (el, _) in babyEls)
            {
                float curTop = el.resolvedStyle.top;
                el.style.top = curTop - 40f * Time.deltaTime;
                el.style.opacity = Mathf.Clamp01(1f - t * 0.7f);
            }
            // 背景が徐々に明るくなる（雲を突き抜ける表現）
            float brightness = t * 0.6f;
            overlay.style.backgroundColor = new Color(brightness, brightness, brightness * 0.9f + 0.1f, 0.9f);
            yield return null;
        }

        yield return StartCoroutine(FadeOutElement(text4, 0.5f));
        text4.RemoveFromHierarchy();
        particleContainer.RemoveFromHierarchy();

        // --- Phase 5: BGMクロスフェード → 聖域の光 ---
        // BGMを透明感のある旋律にクロスフェード
        // (stella_origin_clear BGMがなければ mura1 を高pitch + リバーブで代用)
        var clearBgm = Resources.Load<AudioClip>("BGM/stella_origin_clear");
        if (clearBgm == null)
            clearBgm = Resources.Load<AudioClip>("BGM/mura1");

        if (bgmSource != null && clearBgm != null)
        {
            bgmSource.clip = clearBgm;
            bgmSource.pitch = clearBgm.name.Contains("mura") ? 1.3f : 1f;
            bgmSource.volume = 0f;
            bgmSource.Play();

            // リバーブ追加（オルゴール感）
            var reverb = bgmSource.gameObject.GetComponent<AudioReverbFilter>();
            if (reverb == null)
                reverb = bgmSource.gameObject.AddComponent<AudioReverbFilter>();
            reverb.reverbPreset = AudioReverbPreset.Cave;
            reverb.reverbLevel = 800f;
        }

        // 画面を白 → 黄金 → 聖域の光へ
        float holyDur = 3f;
        fadeEl = 0f;

        var holyText = UIHelper.CreateLabel(
            Localization.Get("ego_mother_defeat_5"), "melodias-cutscene-text");
        UIHelper.ApplyFont(holyText);
        UIHelper.ApplyFontBold(holyText);
        holyText.style.color = new Color(1f, 0.84f, 0f);
        holyText.style.fontSize = 40;
        holyText.style.letterSpacing = 10;
        holyText.style.opacity = 0f;
        overlay.Add(holyText);

        while (fadeEl < holyDur)
        {
            fadeEl += Time.deltaTime;
            float t = Mathf.Clamp01(fadeEl / holyDur);

            // 白 → 温かい黄金
            float r = Mathf.Lerp(0.8f, 1f, t);
            float g = Mathf.Lerp(0.8f, 0.96f, t);
            float b = Mathf.Lerp(0.85f, 0.8f, t);
            overlay.style.backgroundColor = new Color(r, g, b, 1f);

            // BGMフェードイン
            if (bgmSource != null)
                bgmSource.volume = t * 0.4f;

            // テキストフェードイン
            holyText.style.opacity = Mathf.Clamp01((fadeEl - 1f) / 1.5f);

            yield return null;
        }

        yield return new WaitForSeconds(2f);

        // --- Phase 6: フェードアウト → エンディングへ ---
        fadeEl = 0f;
        while (fadeEl < 1.5f)
        {
            fadeEl += Time.deltaTime;
            float t = Mathf.Clamp01(fadeEl / 1.5f);
            overlay.style.backgroundColor = new Color(
                Mathf.Lerp(1f, 0f, t),
                Mathf.Lerp(0.96f, 0f, t),
                Mathf.Lerp(0.8f, 0f, t), 1f);
            holyText.style.opacity = 1f - t;
            if (bgmSource != null)
                bgmSource.volume = 0.4f * (1f - t);
            yield return null;
        }

        if (bgmSource != null)
        {
            bgmSource.Stop();
            var reverb = bgmSource.gameObject.GetComponent<AudioReverbFilter>();
            if (reverb != null) Object.Destroy(reverb);
        }
        if (seSource != null) seSource.pitch = 1f;

        yield return new WaitForSeconds(0.5f);
        overlay.RemoveFromHierarchy();

        // --- 遷移: エンディングシーン or タイトルへ ---
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.babyCurrentHp = -1;
            DataCarrier.Instance.pendingWipeIn = false;
            DataCarrier.Instance.SaveData();
        }

        // エンディングシーンが存在すれば遷移、なければタイトルへ
        if (Application.CanStreamedLevelBeLoaded("EndingScene"))
            SceneManager.LoadScene("EndingScene");
        else
            SceneManager.LoadScene("TitleScene");
    }

    // ===== メロディアス女王 専用カットシーン =====
    // 『黄金の産声と解き放たれた音色』

    IEnumerator MelodiasDefeatCutscene()
    {
        // --- Phase 1: 暗転 ---
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("melodias-cutscene-overlay");
        overlayRoot.Add(overlay);

        yield return null;
        overlay.AddToClassList("melodias-cutscene-overlay-dark");
        yield return new WaitForSeconds(1.2f);

        // --- Phase 2: 歪んだ楽器が砕け散る ---
        var text1 = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_shatter"), "melodias-cutscene-text");
        UIHelper.ApplyFont(text1);
        text1.AddToClassList("melodias-cutscene-title");
        overlay.Add(text1);
        yield return null;
        text1.AddToClassList("melodias-cutscene-text-visible");
        yield return new WaitForSeconds(2.5f);

        // 楽器の破片エフェクト（小さなラベルが散らばる）
        var shatterContainer = new UIE.VisualElement();
        shatterContainer.pickingMode = UIE.PickingMode.Ignore;
        shatterContainer.style.position = UIE.Position.Absolute;
        shatterContainer.style.left = 0; shatterContainer.style.top = 0;
        shatterContainer.style.right = 0; shatterContainer.style.bottom = 0;
        overlay.Add(shatterContainer);

        string[] shardEmojis = { "\u2726", "\u2727", "\u2728", "\u2736", "\u2605" };
        int shardCount = 12;
        var shards = new List<(UIE.Label el, float vx, float vy)>();
        float scw = 1080f, sch = 1920f;
        float cx = scw * 0.5f, cy = sch * 0.4f;
        for (int i = 0; i < shardCount; i++)
        {
            var shard = new UIE.Label();
            shard.pickingMode = UIE.PickingMode.Ignore;
            shard.text = shardEmojis[i % shardEmojis.Length];
            shard.style.position = UIE.Position.Absolute;
            shard.style.fontSize = Random.Range(18, 36);
            shard.style.color = new Color(1f, 0.84f, 0f); // gold
            shard.style.left = cx;
            shard.style.top = cy;
            shard.style.opacity = 1f;
            shatterContainer.Add(shard);
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(200f, 500f);
            shards.Add((shard, Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed));
        }

        float shatterDur = 1.0f, shatterElapsed = 0f;
        while (shatterElapsed < shatterDur)
        {
            shatterElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(shatterElapsed / shatterDur);
            for (int i = 0; i < shards.Count; i++)
            {
                var (el, vx, vy) = shards[i];
                el.style.left = cx + vx * t;
                el.style.top = cy + vy * t;
                el.style.opacity = 1f - t;
            }
            yield return null;
        }
        shatterContainer.RemoveFromHierarchy();

        // テキストフェードアウト
        yield return StartCoroutine(FadeOutElement(text1, 0.5f));
        text1.RemoveFromHierarchy();

        // --- Phase 3: 音の精霊（音符）が空へ昇る ---
        var text2 = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_notes_free"), "melodias-cutscene-text");
        UIHelper.ApplyFont(text2);
        overlay.Add(text2);
        yield return null;
        text2.AddToClassList("melodias-cutscene-text-visible");

        // 音符パーティクル上昇
        var noteContainer = new UIE.VisualElement();
        noteContainer.pickingMode = UIE.PickingMode.Ignore;
        noteContainer.style.position = UIE.Position.Absolute;
        noteContainer.style.left = 0; noteContainer.style.top = 0;
        noteContainer.style.right = 0; noteContainer.style.bottom = 0;
        overlay.Add(noteContainer);

        string[] noteEmojis = { "\u266A", "\u266B", "\U0001F3B5", "\U0001F3B6", "\u2728" };
        int noteCount = 16;
        var notes = new (UIE.Label el, float x, float vy, float sway)[ noteCount ];
        for (int i = 0; i < noteCount; i++)
        {
            var note = new UIE.Label();
            note.pickingMode = UIE.PickingMode.Ignore;
            note.text = noteEmojis[i % noteEmojis.Length];
            note.style.position = UIE.Position.Absolute;
            note.style.fontSize = Random.Range(20, 40);
            float startX = Random.Range(scw * 0.15f, scw * 0.85f);
            float startY = Random.Range(sch * 0.5f, sch * 0.8f);
            note.style.left = startX;
            note.style.top = startY;
            note.style.opacity = 0f;
            note.style.color = new Color(
                Random.Range(0.8f, 1f), Random.Range(0.7f, 1f), Random.Range(0.9f, 1f));
            noteContainer.Add(note);
            notes[i] = (note, startX, Random.Range(-200f, -400f), Random.Range(-40f, 40f));
        }

        float noteDur = 3.0f, noteElapsed = 0f;
        while (noteElapsed < noteDur)
        {
            noteElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(noteElapsed / noteDur);
            for (int i = 0; i < notes.Length; i++)
            {
                var (el, x, vy, sway) = notes[i];
                el.style.left = x + Mathf.Sin(t * Mathf.PI * 2f + i) * sway;
                el.style.top = notes[i].el.resolvedStyle.top + vy * Time.deltaTime;
                // フェードイン→フェードアウト
                el.style.opacity = t < 0.2f ? t / 0.2f : (t > 0.7f ? (1f - t) / 0.3f : 1f);
            }
            yield return null;
        }
        noteContainer.RemoveFromHierarchy();

        yield return StartCoroutine(FadeOutElement(text2, 0.5f));
        text2.RemoveFromHierarchy();

        // --- Phase 4: 女王がテディベアに変化 ---
        var text3 = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_transform"), "melodias-cutscene-text");
        UIHelper.ApplyFont(text3);
        overlay.Add(text3);
        yield return null;
        text3.AddToClassList("melodias-cutscene-text-visible");
        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(FadeOutElement(text3, 0.5f));
        text3.RemoveFromHierarchy();

        // 女王スプライト → テディベア変身
        UIE.VisualElement queenSprite = null;
        if (loadedEnemySprite != null)
        {
            queenSprite = new UIE.VisualElement();
            queenSprite.AddToClassList("melodias-cutscene-sprite");
            queenSprite.style.backgroundImage = new UIE.StyleBackground(loadedEnemySprite);
            overlay.Add(queenSprite);
            queenSprite.style.opacity = 0f;
            yield return null;

            // フェードイン
            float qe = 0f;
            while (qe < 0.8f)
            {
                qe += Time.deltaTime;
                queenSprite.style.opacity = Mathf.Clamp01(qe / 0.8f);
                yield return null;
            }
            yield return new WaitForSeconds(0.8f);

            // 縮小 + フェードアウト
            float shrinkDur = 1.2f; qe = 0f;
            while (qe < shrinkDur)
            {
                qe += Time.deltaTime;
                float t = Mathf.Clamp01(qe / shrinkDur);
                float scale = Mathf.Lerp(1f, 0.3f, t);
                queenSprite.style.scale = new UIE.StyleScale(new Vector2(scale, scale));
                queenSprite.style.opacity = 1f - t * 0.5f;
                yield return null;
            }
            queenSprite.RemoveFromHierarchy();
        }

        // テディベア登場
        var teddy = new UIE.Label();
        teddy.AddToClassList("melodias-cutscene-teddy");
        teddy.text = "\U0001F9F8"; // テディベア絵文字
        teddy.style.opacity = 0f;
        overlay.Add(teddy);
        {
            float te = 0f;
            while (te < 0.6f)
            {
                te += Time.deltaTime;
                teddy.style.opacity = Mathf.Clamp01(te / 0.6f);
                yield return null;
            }
        }
        yield return new WaitForSeconds(0.5f);

        // 女王の最後の台詞
        var queenLine = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_queen_line"), "melodias-cutscene-text");
        UIHelper.ApplyFont(queenLine);
        queenLine.style.color = new Color(1f, 0.72f, 0.77f); // パステルピンク
        overlay.Add(queenLine);
        yield return null;
        queenLine.AddToClassList("melodias-cutscene-text-visible");
        yield return new WaitForSeconds(3.0f);
        yield return StartCoroutine(FadeOutElement(queenLine, 0.8f));
        queenLine.RemoveFromHierarchy();

        // スヤスヤ演出
        var sleepText = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_queen_sleep"), "melodias-cutscene-text");
        UIHelper.ApplyFont(sleepText);
        overlay.Add(sleepText);
        yield return null;
        sleepText.AddToClassList("melodias-cutscene-text-visible");

        // 💤 パーティクル
        var zzz = new UIE.Label();
        zzz.pickingMode = UIE.PickingMode.Ignore;
        zzz.text = "\U0001F4A4";
        zzz.style.position = UIE.Position.Absolute;
        zzz.style.fontSize = 48;
        zzz.style.left = scw * 0.55f;
        zzz.style.top = sch * 0.42f;
        zzz.style.opacity = 0f;
        overlay.Add(zzz);
        {
            float ze = 0f;
            while (ze < 2.0f)
            {
                ze += Time.deltaTime;
                float t = Mathf.Clamp01(ze / 2.0f);
                zzz.style.top = sch * 0.42f - t * 80f;
                zzz.style.opacity = t < 0.3f ? t / 0.3f : (t > 0.7f ? (1f - t) / 0.3f : 1f);
                yield return null;
            }
        }
        zzz.RemoveFromHierarchy();

        // テディベアとテキストフェードアウト
        yield return StartCoroutine(FadeOutElement(teddy, 0.6f));
        teddy.RemoveFromHierarchy();
        yield return StartCoroutine(FadeOutElement(sleepText, 0.5f));
        sleepText.RemoveFromHierarchy();

        yield return new WaitForSeconds(0.5f);

        // --- Phase 5: お爺さん（gold_ikemen）登場 ---
        // 「…！？」テキスト
        var surpriseText = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_goldikemen_arrive"), "melodias-cutscene-text");
        UIHelper.ApplyFont(surpriseText);
        surpriseText.style.fontSize = 48;
        surpriseText.style.color = new Color(1f, 0.84f, 0f); // gold
        overlay.Add(surpriseText);
        yield return null;
        surpriseText.AddToClassList("melodias-cutscene-text-visible");
        yield return new WaitForSeconds(1.2f);
        yield return StartCoroutine(FadeOutElement(surpriseText, 0.3f));
        surpriseText.RemoveFromHierarchy();

        // gold_ikemen スプライト登場（下から超人ジャンプ）
        var ikemenSprite = Resources.Load<Sprite>("MapCharacters/gold_ikemen");
        var ikemenEl = new UIE.VisualElement();
        ikemenEl.AddToClassList("melodias-cutscene-goldikemen");
        if (ikemenSprite != null)
            ikemenEl.style.backgroundImage = new UIE.StyleBackground(ikemenSprite);
        overlay.Add(ikemenEl);

        // ジャンプアニメーション（下→中央、放物線）
        float jumpDur = 0.8f, jumpElapsed = 0f;
        float jumpStartY = sch + 200f;
        float jumpEndY = sch * 0.35f;
        while (jumpElapsed < jumpDur)
        {
            jumpElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(jumpElapsed / jumpDur);
            // イーズアウト
            float et = 1f - (1f - t) * (1f - t);
            float currentY = Mathf.Lerp(jumpStartY, jumpEndY, et);
            ikemenEl.style.bottom = UIE.StyleKeyword.Auto;
            ikemenEl.style.top = currentY;
            yield return null;
        }

        // 着地時の衝撃波（小さな波紋）
        var impactRing = new UIE.VisualElement();
        impactRing.pickingMode = UIE.PickingMode.Ignore;
        impactRing.style.position = UIE.Position.Absolute;
        impactRing.style.width = 20;
        impactRing.style.height = 20;
        impactRing.style.borderTopLeftRadius = 100;
        impactRing.style.borderTopRightRadius = 100;
        impactRing.style.borderBottomLeftRadius = 100;
        impactRing.style.borderBottomRightRadius = 100;
        impactRing.style.borderTopWidth = 3;
        impactRing.style.borderBottomWidth = 3;
        impactRing.style.borderLeftWidth = 3;
        impactRing.style.borderRightWidth = 3;
        impactRing.style.borderTopColor = new Color(1f, 0.84f, 0f, 0.8f);
        impactRing.style.borderBottomColor = new Color(1f, 0.84f, 0f, 0.8f);
        impactRing.style.borderLeftColor = new Color(1f, 0.84f, 0f, 0.8f);
        impactRing.style.borderRightColor = new Color(1f, 0.84f, 0f, 0.8f);
        impactRing.style.left = scw * 0.5f;
        impactRing.style.top = jumpEndY + 180f;
        overlay.Add(impactRing);

        float ringDur = 0.6f, ringElapsed = 0f;
        while (ringElapsed < ringDur)
        {
            ringElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(ringElapsed / ringDur);
            float size = Mathf.Lerp(20f, 300f, t);
            impactRing.style.width = size;
            impactRing.style.height = size * 0.4f;
            impactRing.style.left = scw * 0.5f - size * 0.5f;
            impactRing.style.top = jumpEndY + 180f - size * 0.2f;
            impactRing.style.opacity = 1f - t;
            yield return null;
        }
        impactRing.RemoveFromHierarchy();

        yield return new WaitForSeconds(0.3f);

        // お爺さんの台詞1
        var gLine1 = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_goldikemen_line1"), "melodias-cutscene-dialogue");
        UIHelper.ApplyFont(gLine1);
        UIHelper.ApplyFontBold(gLine1);
        overlay.Add(gLine1);
        yield return null;
        gLine1.AddToClassList("melodias-cutscene-dialogue-visible");
        yield return new WaitForSeconds(2.0f);
        yield return StartCoroutine(FadeOutElement(gLine1, 0.4f));
        gLine1.RemoveFromHierarchy();

        // お爺さんの台詞2
        var gLine2 = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_goldikemen_line2"), "melodias-cutscene-dialogue");
        UIHelper.ApplyFont(gLine2);
        UIHelper.ApplyFontBold(gLine2);
        overlay.Add(gLine2);
        yield return null;
        gLine2.AddToClassList("melodias-cutscene-dialogue-visible");
        yield return new WaitForSeconds(2.5f);
        yield return StartCoroutine(FadeOutElement(gLine2, 0.4f));
        gLine2.RemoveFromHierarchy();

        // --- Phase 6: 打ち上げ（発射） ---
        // エネルギー注入テキスト
        var launchText1 = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_launch"), "melodias-cutscene-text");
        UIHelper.ApplyFont(launchText1);
        launchText1.style.color = new Color(1f, 0.84f, 0f);
        overlay.Add(launchText1);
        yield return null;
        launchText1.AddToClassList("melodias-cutscene-text-visible");

        // 金色パーティクルがお爺さんから集まる演出
        var goldenContainer = new UIE.VisualElement();
        goldenContainer.pickingMode = UIE.PickingMode.Ignore;
        goldenContainer.style.position = UIE.Position.Absolute;
        goldenContainer.style.left = 0; goldenContainer.style.top = 0;
        goldenContainer.style.right = 0; goldenContainer.style.bottom = 0;
        overlay.Add(goldenContainer);

        int goldenCount = 24;
        var goldenParts = new (UIE.VisualElement el, float startX, float startY, float delay)[ goldenCount ];
        float targetX = scw * 0.5f, targetY = sch * 0.25f;
        for (int i = 0; i < goldenCount; i++)
        {
            var gp = new UIE.VisualElement();
            gp.AddToClassList("melodias-golden-particle");
            float sx = scw * 0.5f + Random.Range(-60f, 60f);
            float sy = jumpEndY + 90f + Random.Range(-30f, 30f);
            gp.style.left = sx;
            gp.style.top = sy;
            float size = Random.Range(6f, 14f);
            gp.style.width = size;
            gp.style.height = size;
            gp.style.borderTopLeftRadius = size * 0.5f;
            gp.style.borderTopRightRadius = size * 0.5f;
            gp.style.borderBottomLeftRadius = size * 0.5f;
            gp.style.borderBottomRightRadius = size * 0.5f;
            goldenContainer.Add(gp);
            goldenParts[i] = (gp, sx, sy, Random.Range(0f, 0.5f));
        }

        float gatherDur = 2.0f, gatherElapsed = 0f;
        while (gatherElapsed < gatherDur)
        {
            gatherElapsed += Time.deltaTime;
            for (int i = 0; i < goldenParts.Length; i++)
            {
                var (el, sx, sy, delay) = goldenParts[i];
                float localT = Mathf.Clamp01((gatherElapsed - delay) / (gatherDur - delay));
                float et = localT * localT; // ease-in
                el.style.left = Mathf.Lerp(sx, targetX, et);
                el.style.top = Mathf.Lerp(sy, targetY, et);
                el.style.opacity = localT > 0f ? (localT < 0.8f ? 1f : (1f - localT) / 0.2f) : 0f;
            }
            yield return null;
        }
        goldenContainer.RemoveFromHierarchy();

        yield return StartCoroutine(FadeOutElement(launchText1, 0.3f));
        launchText1.RemoveFromHierarchy();

        // 発射テキスト
        var launchText2 = UIHelper.CreateLabel(
            Localization.Get("melodias_cutscene_launch2"), "melodias-cutscene-text");
        UIHelper.ApplyFont(launchText2);
        launchText2.style.color = new Color(1f, 0.84f, 0f);
        overlay.Add(launchText2);
        yield return null;
        launchText2.AddToClassList("melodias-cutscene-text-visible");
        yield return new WaitForSeconds(1.0f);

        // お爺さんフェードアウト
        yield return StartCoroutine(FadeOutElement(ikemenEl, 0.5f));
        ikemenEl.RemoveFromHierarchy();

        // 赤ちゃんが黄金の光弾として上昇
        var babyLight = new UIE.VisualElement();
        babyLight.style.position = UIE.Position.Absolute;
        babyLight.style.width = 40;
        babyLight.style.height = 40;
        babyLight.style.borderTopLeftRadius = 20;
        babyLight.style.borderTopRightRadius = 20;
        babyLight.style.borderBottomLeftRadius = 20;
        babyLight.style.borderBottomRightRadius = 20;
        babyLight.style.backgroundColor = new Color(1f, 0.84f, 0f);
        babyLight.style.left = scw * 0.5f - 20f;
        babyLight.style.top = sch * 0.5f;
        overlay.Add(babyLight);

        // 光の軌跡パーティクル
        var trailContainer = new UIE.VisualElement();
        trailContainer.pickingMode = UIE.PickingMode.Ignore;
        trailContainer.style.position = UIE.Position.Absolute;
        trailContainer.style.left = 0; trailContainer.style.top = 0;
        trailContainer.style.right = 0; trailContainer.style.bottom = 0;
        overlay.Add(trailContainer);

        float launchDur = 1.5f, launchElapsed = 0f;
        float babyStartY = sch * 0.5f;
        var trails = new List<(UIE.VisualElement el, float born)>();
        while (launchElapsed < launchDur)
        {
            launchElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(launchElapsed / launchDur);
            // 加速度的に上昇
            float accelT = t * t * t;
            float currentBabyY = Mathf.Lerp(babyStartY, -100f, accelT);
            babyLight.style.top = currentBabyY;

            // 軌跡パーティクル生成
            if (Random.value < 0.6f)
            {
                var trail = new UIE.VisualElement();
                trail.pickingMode = UIE.PickingMode.Ignore;
                float ts = Random.Range(4f, 10f);
                trail.style.position = UIE.Position.Absolute;
                trail.style.width = ts;
                trail.style.height = ts;
                trail.style.borderTopLeftRadius = ts * 0.5f;
                trail.style.borderTopRightRadius = ts * 0.5f;
                trail.style.borderBottomLeftRadius = ts * 0.5f;
                trail.style.borderBottomRightRadius = ts * 0.5f;
                trail.style.backgroundColor = new Color(1f, Random.Range(0.7f, 1f), Random.Range(0f, 0.4f));
                trail.style.left = scw * 0.5f - ts * 0.5f + Random.Range(-15f, 15f);
                trail.style.top = currentBabyY + Random.Range(10f, 40f);
                trail.style.opacity = 0.8f;
                trailContainer.Add(trail);
                trails.Add((trail, launchElapsed));
            }

            // 古い軌跡をフェードアウト
            for (int i = trails.Count - 1; i >= 0; i--)
            {
                float age = launchElapsed - trails[i].born;
                if (age > 0.5f)
                {
                    trails[i].el.RemoveFromHierarchy();
                    trails.RemoveAt(i);
                }
                else
                {
                    trails[i].el.style.opacity = 0.8f * (1f - age / 0.5f);
                }
            }
            yield return null;
        }
        babyLight.RemoveFromHierarchy();
        trailContainer.RemoveFromHierarchy();

        yield return StartCoroutine(FadeOutElement(launchText2, 0.5f));
        launchText2.RemoveFromHierarchy();

        // --- Phase 7: 黄金フラッシュ → フェードアウト ---
        var flash = new UIE.VisualElement();
        flash.AddToClassList("melodias-launch-flash");
        overlay.Add(flash);
        yield return null;
        flash.AddToClassList("melodias-launch-flash-bright");
        yield return new WaitForSeconds(0.6f);

        // ゆっくりフェードアウト（黄金 → 白 → 黒）
        float finalFade = 2.0f, finalElapsed = 0f;
        while (finalElapsed < finalFade)
        {
            finalElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(finalElapsed / finalFade);
            float r = Mathf.Lerp(1f, 0f, t);
            float g = Mathf.Lerp(0.84f, 0f, t);
            float b = Mathf.Lerp(0f, 0f, t);
            flash.style.backgroundColor = new Color(r, g, b, 1f);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // クリーンアップ
        overlay.RemoveFromHierarchy();

        // --- 遷移: Area 8（ステラ・オリジン） ---
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = false;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.babyCurrentHp = -1;
            DataCarrier.Instance.currentArea = 8;
            DataCarrier.Instance.mapPlayerX = 12;
            DataCarrier.Instance.mapPlayerY = 35;
            DataCarrier.Instance.pendingWipeIn = false;
            DataCarrier.Instance.SaveData();
        }

        SceneManager.LoadScene("MapScene");
    }

    // カットシーン用フェードアウトヘルパー
    IEnumerator FadeOutElement(UIE.VisualElement el, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            el.style.opacity = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        el.style.opacity = 0f;
    }

    // ===== ボス撃破演出 =====

    IEnumerator BossDefeatSequence()
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        string[] bossLines;
        if (area == 8 && enemyName == "エゴ・マザー・マシーン")
        {
            // 専用カットシーン『エゴからの解放』
            yield return StartCoroutine(EgoMotherDefeatCutscene());
            yield break;
        }
        else if (area == 4 && enemyName == "メロディアス女王")
        {
            // 専用カットシーン『黄金の産声と解き放たれた音色』
            yield return StartCoroutine(MelodiasDefeatCutscene());
            yield break;
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
            cutinLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
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
                DataCarrier.Instance.mapPlayerX = 6;
                DataCarrier.Instance.mapPlayerY = 37;
            }
            else if (area == 2 && enemyName == "デヴィル夫人")
            {
                DataCarrier.Instance.currentArea = 3;
                DataCarrier.Instance.mapPlayerX = 5;
                DataCarrier.Instance.mapPlayerY = 2;
            }
            else
            {
                // シバ撃破後: Area 0に戻る（シバの家から次のエリアへ進む）
                DataCarrier.Instance.currentArea = 0;
                DataCarrier.Instance.mapPlayerX = 8;
                DataCarrier.Instance.mapPlayerY = 34;
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
