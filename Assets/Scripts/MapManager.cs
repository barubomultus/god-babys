using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UIE = UnityEngine.UIElements;

public class MapManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Canvas canvas;

    // タイルサイズ
    const int TILE_SIZE = 32;
    int mapWidth = 12;
    int mapHeight = 20;

    // 表示スケール（UI上でのタイルの大きさ）- 9:16縦画面用
    const float DISPLAY_SCALE = 3.2f;
    const float DISPLAY_TILE = TILE_SIZE * DISPLAY_SCALE;

    // タイルID定義
    const int TILE_GRASS = 0;
    const int TILE_WATER = 1;
    const int TILE_DIRT = 2;
    const int TILE_PATH = 3;
    const int TILE_TREE = 4;
    const int TILE_HOUSE_RED = 5;
    const int TILE_HOUSE_GREEN = 6;
    const int TILE_HOUSE_BLUE = 7;
    const int TILE_ROCK = 8;
    const int TILE_FENCE = 9;
    const int TILE_FLOWER = 10;
    const int TILE_BOSS_MANSION = 11;
    const int TILE_BOSS_GATE = 12;
    const int TILE_DARK_DIRT = 13;
    const int TILE_MANSION_FLOOR = 14;
    const int TILE_MANSION_WALL = 15;
    const int TILE_MERCENARY_A = 16;
    const int TILE_MERCENARY_B = 17;
    const int TILE_BOSS_DOOR = 18;
    const int TILE_MANSION_EXIT = 19;
    const int TILE_AREA_EXIT = 20;
    const int TILE_FATHER_HOUSE = 21;
    const int TILE_FATHER_FLOOR = 22;
    const int TILE_FATHER_WALL = 23;
    const int TILE_FATHER_EXIT = 24;
    const int TILE_WEAPON_SHOP = 25;
    const int TILE_JUKU = 26;
    const int TILE_SHOP_FLOOR = 27;
    const int TILE_SHOP_WALL = 28;
    const int TILE_JUKU_FLOOR = 29;
    const int TILE_JUKU_WALL = 30;
    const int TILE_CHAMPAGNE = 31; // 儀式の回廊用シャンパン柵
    const int TILE_CRYSTAL = 32;     // ステラ・オリジン: クリスタルの床
    const int TILE_CRYSTAL_WALL = 33; // ステラ・オリジン: 宇宙の壁（通行不可）
    // TILE 34, 35: 廃止（光の川・音叉ワープ）

    // マップデータ
    int[,] mapData;
    bool[,] walkable;

    // プレイヤー
    GameObject playerObj;
    RectTransform playerRect;
    int playerTileX = 5;
    int playerTileY = 9;
    float moveSpeed = 8f;
    bool isMoving = false;
    Vector2 targetPosition;

    // かぐやフォロワー（相思相愛後、後ろについてくる）
    GameObject kaguyaFollowerObj;
    RectTransform kaguyaFollowerRect;
    Vector2 kaguyaFollowerTarget;
    bool kaguyaFollowerMoving = false;

    // キャラクタースプライト
    Sprite[] playerIdleSprites = new Sprite[4]; // 0=south,1=north,2=west,3=east
    Sprite[][] playerWalkSprites = new Sprite[4][]; // [dir][frame]
    int walkFrameIndex;
    float walkFrameTimer;
    const float WALK_FRAME_INTERVAL = 0.1f;

    // UI要素
    GameObject mapPanel;
    RectTransform tilesContainerRect;
    GameObject tilesContainer;
    Texture2D tilesetTexture;
    Dictionary<int, Sprite> tileSprites = new Dictionary<int, Sprite>();

    // Pixel Crawler テクスチャ（area==0 よちよちの里マップ用）
    Texture2D pcTreesTex, pcVegetationTex, pcRocksTex, pcRoofsTex, pcWallsTex;
    // タイル背景テクスチャ
    Texture2D lawnTex, dirtRoadTex, dirtBgTex, lakeBgTex, poisonLakeTex, poisonFenceTex, poisonRoadTex;
    Texture2D nobilityTileTex;
    Texture2D leftWallTex, rightWallTex, cornerWallTex;
    Sprite poisonLakeSprite, poisonFenceSprite, poisonRoadSprite, poisonFeltSprite, champagneSprite, toyShopBgSprite, toyShopDeskSprite, toyShopCaseSprite, toyShopBallSprite;
    Sprite thirdBgSprite, thirdRoadSprite, thirdLakeSprite, thirdFenceSprite, thirdWoodtowerSprite, thirdPoolSprite, thirdWaterfallSprite;

    // UI Toolkit（area==0 DQタイル描画用）
    UIE.UIDocument villageUIDoc;
    UIE.PanelSettings villagePanelSettings;
    UIE.VisualElement tileGridRoot;

    // プレイヤースプライト
    Image playerImage;
    int playerDirection = 0; // 0=下(正面), 1=上(背面), 2=左, 3=右

    // メニュー
    UIE.VisualElement menuOverlayEl;
    bool menuOpen = false;

    // シンボルエンカウント
    List<EnemySymbol> enemySymbols = new List<EnemySymbol>();
    bool symbolEncounterActive = false;

    // 金のたまご
    GameObject goldenEggObj;
    const int GOLDEN_EGG_X = 9;
    const int GOLDEN_EGG_Y = 14;
    const int GOLDEN_EGG_X_AREA3 = 2;
    const int GOLDEN_EGG_Y_AREA3 = 31;

    // ミルクポイント（回復）
    GameObject milkPointObj;
    int milkPointX = 3;
    int milkPointY = 10;
    bool milkCutinActive = false;

    // 黄金の滝（Area 3）
    GameObject waterfallObj;
    TMPro.TextMeshProUGUI[] waterfallStars;
    int waterfallX = 10, waterfallY = 24;
    float waterfallMilkTimer;
    int waterfallMilkGained;
    TMPro.TextMeshProUGUI waterfallMilkText;

    // 金の卵の老人NPC（Area 3）
    GameObject goldenEggOldManObj;
    TMPro.TextMeshProUGUI[] oldManStars;
    int goldenEggOldManX = 8, goldenEggOldManY = 30;
    bool goldenEggOldManDialogueActive = false;

    // かぐやちゃんNPC（全エリア）
    GameObject kaguyaNpcObj;
    int kaguyaNpcX, kaguyaNpcY;
    bool kaguyaDialogueActive = false;
    TMPro.TextMeshProUGUI[] kaguyaHearts;
    TMPro.TextMeshProUGUI[] poolBubbles;

    // 門番（長老）NPC
    GameObject elderNpcObj;
    int elderNpcX = 6;
    int elderNpcY = 10;
    bool elderDialogueActive = false;
    bool elderDialogueCooldown = false; // 連続発動防止
    float elderBossBlockCooldown = 0f; // ボスブロック連続発動防止（秒数）

    // 実家（母親）NPC
    GameObject motherNpcObj;
    int motherNpcX = 9;
    int motherNpcY = 5;
    bool motherDialogueActive = false;

    // 実家NPC（父親・母親）
    GameObject fatherNpcObj;
    int fatherNpcX = 3;
    int fatherNpcY = 32;
    bool fatherDialogueActive = false;
    GameObject homeMotherNpcObj;
    int homeMotherNpcX = 2;
    int homeMotherNpcY = 3;
    bool parentsInBed = false;

    // お手伝いさんNPC（6人）
    GameObject[] maidNpcObjs = new GameObject[6];
    int[] maidNpcX = { 2, 13, 2, 13, 2, 13 };
    int[] maidNpcY = { 3, 3, 6, 6, 9, 9 };
    bool maidDialogueActive = false;

    // 武器屋の商人NPC
    GameObject merchantNpcObj;
    int merchantNpcX = 4;
    int merchantNpcY = 7;
    bool merchantDialogueActive = false;

    // 塾の先生NPC
    GameObject jukuTeacherObj;
    int jukuTeacherX = 6;
    int jukuTeacherY = 10;
    bool jukuQuizActive = false;

    // 塾の生徒NPC（5人）
    GameObject[] jukuStudentObjs = new GameObject[5];
    int[] jukuStudentX = { 3, 7, 3, 7, 9 };
    int[] jukuStudentY = { 5, 5, 8, 8, 5 };
    bool jukuStudentDialogueActive = false;

    // ステラ・オリジン（Area 8）
    Image stellaPlayerGlow;       // 中間層（紫）
    Image stellaPlayerAuraOuter;   // 外周（青白）
    Image stellaPlayerAuraInner;   // 内周（白）
    RectTransform stellaBgRect; // 背景回転用
    Image stellaBgImg;           // 背景Image（虹色明滅用）
    Vector2 stellaBgBasePos;     // 背景の基本位置（パララックス用）
    RectTransform stellaDarkOverlayRect; // ダークオーバーレイ（パララックス用）
    Vector2 stellaDarkBasePos;
    List<(RectTransform rt, Image img, float phase, float speed)> stellaGlitters
        = new List<(RectTransform, Image, float, float)>(); // グリッターパーティクル
    // 浮遊タイル: 座標→(obj, img, targetAlpha, currentAlpha, baseY, phase, speed, tileX, tileY)
    Dictionary<(int, int), StellaFloatingTile> stellaTileMap = new Dictionary<(int, int), StellaFloatingTile>();
    class StellaFloatingTile
    {
        public RectTransform rt;
        public Image img;
        public float maxAlpha;     // タイル種別で決まる最大不透明度
        public float currentAlpha; // 現在のアルファ値
        public float goalAlpha;    // 補間先のアルファ値
        public float baseY;
        public float phase;
        public float speed;
        public int tileX, tileY;
        public bool discovered;    // 一度でも3マス以内に入ったか
        public bool visited;       // 一度でもプレイヤーが乗ったか
    }
    List<(RectTransform rt, float speed, float phase, float baseAlpha)> stellaPhotons = new List<(RectTransform, float, float, float)>(); // 光子パーティクル
    // Aura波紋システム
    Sprite stellaAuraSprite;
    Material stellaAdditiveMat; // 加算合成マテリアル
    class StellaAuraRipple
    {
        public RectTransform rt;
        public Image img;
        public float elapsed;
        public float duration;
        public Color tintColor;
    }
    List<StellaAuraRipple> stellaAuraRipples = new List<StellaAuraRipple>();
    float stellaCrystalPulseTime;
    List<(RectTransform rt, float speed, float phase)> stellaStarParticles = new List<(RectTransform, float, float)>();
    // ガーディアンNPC
    GameObject stellaGuardianDevilObj;
    int stellaGuardianDevilX = 3, stellaGuardianDevilY = 19;
    GameObject stellaGuardianMelodiasObj;
    int stellaGuardianMelodiasX = 21, stellaGuardianMelodiasY = 19;
    bool stellaGuardianDialogueActive = false;
    // 光のゆりかご
    GameObject stellaCradleObj;
    int stellaCradleX = 12, stellaCradleY = 35;
    bool stellaCradleActive = false;
    // 宇宙の門
    GameObject stellaCosmicGateObj;
    int stellaCosmicGateX = 12, stellaCosmicGateY = 37;
    List<(RectTransform rt, float phase)> stellaGateParticles = new List<(RectTransform, float)>();
    bool stellaGateOpened = false; // ゲート開放済み（walkable化済み）
    bool stellaGateOpenAnimPlaying = false;
    // 音叉ワープ（行き: 左下/右下→ボス広場、帰り: ボス広場→中央）
    (int x, int y) stellaForkA = (8, 5);   // 左下の音叉 → 西ボス広場(4,19)
    (int x, int y) stellaForkADest = (4, 19);
    (int x, int y) stellaForkB = (15, 5);  // 右下の音叉 → 東ボス広場(21,19)
    (int x, int y) stellaForkBDest = (21, 19);
    (int x, int y) stellaForkReturnW = (4, 20); // 西ボス広場の帰還音叉 → 中央(12,19)
    (int x, int y) stellaForkReturnE = (21, 20); // 東ボス広場の帰還音叉 → 中央(12,19)
    (int x, int y) stellaForkReturnDest = (12, 19); // 帰還先: 中央交差点
    List<(RectTransform rt, Image img, float baseY, float phase)> stellaForkVisuals
        = new List<(RectTransform, Image, float, float)>();

    // SE
    AudioSource seSource;
    AudioClip seQuizCorrect;
    AudioClip seQuizWrong;
    AudioClip seFootstep;

    // 持ち物・装備パネル
    UIE.VisualElement inventoryOverlayEl;
    UIE.VisualElement equipmentOverlayEl;
    UIE.VisualElement shopOverlayEl;
    UIE.VisualElement statusDetailEl;
    UIE.VisualElement saveOverlayEl;

    // UI Toolkit overlay layer
    UIE.PanelSettings overlayPanelSettings;
    UIE.VisualElement overlayRoot;
    UIE.Label statusLabel;

    // タッチ操作
    UIE.VisualElement touchSwipeEl;

    // 移動コントローラ
    PlayerMovementController moveCtrl;

    IEnumerator CaptureMapScreenshot()
    {
        // UIオーバーレイを一時非表示にしてマップのみ撮影
        if (overlayRoot != null) overlayRoot.style.display = UIE.DisplayStyle.None;
        yield return new WaitForEndOfFrame();
        var tex = ScreenCapture.CaptureScreenshotAsTexture();
        if (overlayRoot != null) overlayRoot.style.display = UIE.DisplayStyle.Flex;
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.battleBgTexture = tex;
    }

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

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        // よちよちの里マップ・ゴージャス・ヴィレッジは2倍の広さ
        if (area == 0 || area == 1)
            mapHeight = 40;
        else if (area == 3)
        {
            mapHeight = 40;
        }
        else if (area == 8)
        {
            mapWidth = 24;
            mapHeight = 40;
        }
        else if (area == 5)
        {
            mapWidth = 16;
            mapHeight = 18;
        }
        else if (area == 7)
        {
            mapWidth = 12;
            mapHeight = 14;
        }
        else if (area == 6)
        {
            mapWidth = 8;
            mapHeight = 10;
        }

        // DataCarrierからマップ位置を復元
        if (DataCarrier.Instance != null)
        {
            playerTileX = DataCarrier.Instance.mapPlayerX;
            playerTileY = DataCarrier.Instance.mapPlayerY;
            // 旧マップの保存座標が新マップ外にならないようバウンドチェック
            if (playerTileX < 1 || playerTileX >= mapWidth - 1) playerTileX = mapWidth / 2;
            if (playerTileY < 1 || playerTileY >= mapHeight - 1) playerTileY = mapHeight / 4;
            // ステラ・オリジン: 初回は中央交差点からスタート
            if (area == 8 && DataCarrier.Instance.stellaOriginProgress == 0)
            {
                playerTileX = 12;
                playerTileY = 19;
            }
        }

        LoadTileset();

        // SE読み込み
        seSource = gameObject.AddComponent<AudioSource>();
        seQuizCorrect = Resources.Load<AudioClip>("SE/クイズ正解1");
        seQuizWrong = Resources.Load<AudioClip>("SE/クイズ不正解1");
        seFootstep = Resources.Load<AudioClip>("SE/可愛い足音");

        // 実家ベッド判定はマップ生成前に決定
        if (area == 5)
            parentsInBed = Random.Range(0, 10) == 0;

        if (area == 8)
            GenerateStellaOriginMapData();
        else if (area == 7)
            GenerateJukuMapData();
        else if (area == 6)
            GenerateWeaponShopMapData();
        else if (area == 5)
            GenerateFatherHouseMapData();
        else if (area == 4)
            Generate109MapData();
        else if (area == 3)
            GenerateImpTownMapData();
        else if (area == 2)
            GenerateMansionMapData();
        else if (area == 1)
            GenerateDevilMapData();
        else
            GenerateMapData();

        CreateMapUI();
        CreatePlayer();
        UpdateCameraFollow();

        if (area == 8)
        {
            CreateStellaOriginEffects();
            CreateStellaTuningForks();
            CreateStellaGuardianNPCs();
            CreateStellaCradle();
            CreateStellaCosmicGate();
        }
        else if (area == 7)
        {
            // 塾内部: ランダムエンカウントなし
            CreateJukuTeacherNPC();
            CreateJukuStudentNPCs();
        }
        else if (area == 6)
        {
            // 武器屋内部: ランダムエンカウントなし
            CreateMerchantNPC();
        }
        else if (area == 5)
        {
            // 実家（宮殿）: ランダムエンカウントなし
            if (parentsInBed)
            {
                fatherNpcX = 12;
                fatherNpcY = 15;
                CreateFatherInteriorNPC();
            }
            else
            {
                fatherNpcX = 7;
                fatherNpcY = 6;
                CreateFatherInteriorNPC();
            }
            // 母親がアイテムを渡した後は実家にいる
            bool motherHome = DataCarrier.Instance != null && DataCarrier.Instance.motherGaveItem;
            if (motherHome)
            {
                if (parentsInBed)
                {
                    homeMotherNpcX = 13;
                    homeMotherNpcY = 15;
                }
                else
                {
                    homeMotherNpcX = 5;
                    homeMotherNpcY = 14;
                }
                CreateHomeMotherNPC();
            }
            CreateMaidNPCs();
        }
        else if (area == 4)
        {
            // 109館内: ランダムエンカウントなし、ミルクポイントなし、おともだちなし
        }
        else if (area == 3)
        {
            milkPointX = 3;
            milkPointY = 18;
            CreateMilkPoint();
            CreateGoldenEgg();
            CreateWaterfallOverlay();
            CreateGoldenEggOldMan();
        }
        else if (area == 2)
        {
            // 館内: ランダムエンカウントなし、ミルクポイントなし
            CreateMansionNPCs();
        }
        else if (area == 1)
        {
            milkPointX = 9;
            milkPointY = 4;
            CreateMilkPoint();
        }
        else
        {
            CreateGoldenEgg();
            CreateMilkPoint();
            elderNpcX = 4;
            elderNpcY = 10;
            CreateElderNPC();
            motherNpcX = 5;
            motherNpcY = 21;
            bool motherGone = DataCarrier.Instance != null && DataCarrier.Instance.motherGaveItem;
            if (!motherGone)
                CreateMotherNPC();
            fatherNpcX = 3;
            fatherNpcY = 32;
        }

        // かぐやちゃんNPC（全エリア共通）
        CreateKaguyaNPC();

        // UI Toolkit overlay layer (above Canvas, separate GameObject to avoid UIDocument conflict)
        overlayPanelSettings = UIHelper.CreatePanelSettings(10f);
        var overlayObj = new GameObject("MapOverlayUI");
        overlayObj.transform.SetParent(transform, false);
        overlayRoot = UIHelper.SetupUIDocument(overlayObj,
            new[] { "UI/CommonStyle", "UI/MapStyle" }, overlayPanelSettings);
        overlayRoot.pickingMode = UIE.PickingMode.Ignore;
        overlayRoot.focusable = false;

        if (SafeAreaHelper.IsTouchDevice())
            CreateTouchControls();

        CreateStatusUI();
        CreateMenuButton();
        if (area == 8) CreateDebugResetButton();

        // 移動コントローラ初期化
        moveCtrl = new PlayerMovementController();
        moveCtrl.Initialize(
            tryMove: (dx, dy) => { TryMove(dx, dy); return isMoving; },
            isWalkable: (x, y) => x >= 0 && x < mapWidth && y >= 0 && y < mapHeight && walkable[x, y],
            getPlayerTile: () => new Vector2Int(playerTileX, playerTileY),
            getIsMoving: () => isMoving,
            mapWidth, mapHeight, DISPLAY_TILE,
            tilesContainerRect, tilesContainer.transform
        );

        // シンボルエンカウント生成
        SpawnEnemySymbols(area);

        // レイヤー順: 背景 < タイル < プレイヤー/アイテム < エフェクト
        if (playerObj != null)
        {
            if (area == 8)
            {
                // Area 8: エフェクト（光子・波紋）をプレイヤーの上に配置
                // まずプレイヤーを最前面にして、その後エフェクト系を上に移動
                playerObj.transform.SetAsLastSibling();
                // 光子パーティクルをプレイヤーの上に
                foreach (var (phRt, _, _, _) in stellaPhotons)
                    if (phRt != null) phRt.transform.SetAsLastSibling();
                // 足元波紋削除済み
            }
            else
            {
                playerObj.transform.SetAsLastSibling();
            }
        }

        // ボス撃破後のワイプイン演出 + 初回到着時の案内
        bool firstVillitVisit = area == 0
            && DataCarrier.Instance != null
            && !DataCarrier.Instance.HasMetNpc("長老");
        bool firstGorgeousVisit = area == 1
            && DataCarrier.Instance != null
            && !DataCarrier.Instance.HasMetNpc("gorgeous_intro");

        if (DataCarrier.Instance != null && DataCarrier.Instance.pendingWipeIn)
        {
            DataCarrier.Instance.pendingWipeIn = false;
            if (firstVillitVisit)
                StartCoroutine(PlayWipeInThenElderGuide());
            else if (firstGorgeousVisit)
                StartCoroutine(PlayWipeInThenGorgeousIntro());
            else
                StartCoroutine(PlayWipeIn());
        }
        else if (firstVillitVisit)
        {
            StartCoroutine(AutoElderGuide());
        }

        // BGM再生
        PlayBGM(area);
    }

    void PlayBGM(int area)
    {
        string clipPath = null;
        if (area == 8)
            clipPath = "BGM/stella_origin";
        else if (area == 0 || area == 5 || area == 6)
            clipPath = "BGM/mura1";

        if (area == 8)
        {
            // ステラ・オリジン専用BGM、なければArea3 BGMを宇宙アレンジ
            var stellaClip = Resources.Load<AudioClip>("BGM/stella_origin");
            if (stellaClip != null)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.clip = stellaClip;
                src.loop = true;
                src.volume = 0.5f;
                src.Play();
            }
            else
            {
                // フォールバック: Area 3 BGM を Pitch 0.5 + リバーブ風
                var fallbackClip = Resources.Load<AudioClip>("BGM/mura1");
                if (fallbackClip != null)
                {
                    var src = gameObject.AddComponent<AudioSource>();
                    src.clip = fallbackClip;
                    src.loop = true;
                    src.volume = 0.35f;
                    src.pitch = 0.5f;
                    src.Play();
                    // リバーブ追加
                    var reverb = gameObject.AddComponent<AudioReverbFilter>();
                    reverb.reverbPreset = AudioReverbPreset.Cave;
                    reverb.reverbLevel = 800f;
                }
            }
            return;
        }

        if (clipPath == null) return;

        var clip = Resources.Load<AudioClip>(clipPath);
        if (clip == null) return;

        var audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.volume = 0.5f;
        audioSource.Play();
    }

    void LoadTileset()
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        if (area == 0)
        {
            // よちよちの里マップ: Pixel Crawlerテクスチャをロード（DQ風プロシージャル + スプライトオーバーレイ）
            LoadPixelCrawlerTextures();
        }

        // ゴージャス・ヴィレッジ用テクスチャ（area==1）
        poisonLakeTex = Resources.Load<Texture2D>("Map/Poison_Lake");
        poisonFenceTex = Resources.Load<Texture2D>("Map/Poison_Fence");
        if (poisonLakeTex != null)
            poisonLakeSprite = Sprite.Create(poisonLakeTex, new Rect(0, 0, poisonLakeTex.width, poisonLakeTex.height), new Vector2(0.5f, 0.5f));
        if (poisonFenceTex != null)
            poisonFenceSprite = Sprite.Create(poisonFenceTex, new Rect(0, 0, poisonFenceTex.width, poisonFenceTex.height), new Vector2(0.5f, 0.5f));
        poisonRoadTex = Resources.Load<Texture2D>("Map/Poison_Road");
        if (poisonRoadTex != null)
            poisonRoadSprite = Sprite.Create(poisonRoadTex, new Rect(0, 0, poisonRoadTex.width, poisonRoadTex.height), new Vector2(0.5f, 0.5f));
        var poisonFeltTex = Resources.Load<Texture2D>("Map/Poison_Felt");
        if (poisonFeltTex != null)
            poisonFeltSprite = Sprite.Create(poisonFeltTex, new Rect(0, 0, poisonFeltTex.width, poisonFeltTex.height), new Vector2(0.5f, 0.5f));
        var champagneTex = Resources.Load<Texture2D>("Map/champagne");
        if (champagneTex != null)
            champagneSprite = Sprite.Create(champagneTex, new Rect(0, 0, champagneTex.width, champagneTex.height), new Vector2(0.5f, 0.5f));
        var toyShopBgTex = Resources.Load<Texture2D>("Map/Toy_Shop_Background");
        if (toyShopBgTex != null)
            toyShopBgSprite = Sprite.Create(toyShopBgTex, new Rect(0, 0, toyShopBgTex.width, toyShopBgTex.height), new Vector2(0.5f, 0.5f));
        var toyShopDeskTex = Resources.Load<Texture2D>("Map/Toy_Shop_Desk");
        if (toyShopDeskTex != null)
            toyShopDeskSprite = Sprite.Create(toyShopDeskTex, new Rect(0, 0, toyShopDeskTex.width, toyShopDeskTex.height), new Vector2(0.5f, 0.5f));
        var toyShopCaseTex = Resources.Load<Texture2D>("Map/Toy_Shop _Case");
        if (toyShopCaseTex != null)
            toyShopCaseSprite = Sprite.Create(toyShopCaseTex, new Rect(0, 0, toyShopCaseTex.width, toyShopCaseTex.height), new Vector2(0.5f, 0.5f));
        var toyShopBallTex = Resources.Load<Texture2D>("Map/Toy_Shop_Ball");
        if (toyShopBallTex != null)
            toyShopBallSprite = Sprite.Create(toyShopBallTex, new Rect(0, 0, toyShopBallTex.width, toyShopBallTex.height), new Vector2(0.5f, 0.5f));
        var toyShopWallTex = Resources.Load<Texture2D>("Map/Toy_Shop_Wall");
        if (toyShopWallTex != null)
            toyShopBallSprite = Sprite.Create(toyShopWallTex, new Rect(0, 0, toyShopWallTex.width, toyShopWallTex.height), new Vector2(0.5f, 0.5f));

        // 小悪魔の街（Area 3）用スプライト
        var thirdBgTex = Resources.Load<Texture2D>("Map/3rd/background");
        if (thirdBgTex != null)
            thirdBgSprite = Sprite.Create(thirdBgTex, new Rect(0, 0, thirdBgTex.width, thirdBgTex.height), new Vector2(0.5f, 0.5f));
        var thirdRoadTex = Resources.Load<Texture2D>("Map/3rd/road");
        if (thirdRoadTex != null)
            thirdRoadSprite = Sprite.Create(thirdRoadTex, new Rect(0, 0, thirdRoadTex.width, thirdRoadTex.height), new Vector2(0.5f, 0.5f));
        var thirdLakeTex = Resources.Load<Texture2D>("Map/3rd/lake");
        if (thirdLakeTex != null)
            thirdLakeSprite = Sprite.Create(thirdLakeTex, new Rect(0, 0, thirdLakeTex.width, thirdLakeTex.height), new Vector2(0.5f, 0.5f));
        var thirdFenceTex = Resources.Load<Texture2D>("Map/3rd/fence");
        if (thirdFenceTex != null)
            thirdFenceSprite = Sprite.Create(thirdFenceTex, new Rect(0, 0, thirdFenceTex.width, thirdFenceTex.height), new Vector2(0.5f, 0.5f));
        var thirdWoodtowerTex = Resources.Load<Texture2D>("Map/3rd/woodtower");
        if (thirdWoodtowerTex != null)
            thirdWoodtowerSprite = Sprite.Create(thirdWoodtowerTex, new Rect(0, 0, thirdWoodtowerTex.width, thirdWoodtowerTex.height), new Vector2(0.5f, 0.5f));
        var thirdPoolTex = Resources.Load<Texture2D>("Map/3rd/pool");
        if (thirdPoolTex != null)
            thirdPoolSprite = Sprite.Create(thirdPoolTex, new Rect(0, 0, thirdPoolTex.width, thirdPoolTex.height), new Vector2(0.5f, 0.5f));
        var thirdWaterfallTex = Resources.Load<Texture2D>("Map/3rd/waterfall");
        if (thirdWaterfallTex != null)
            thirdWaterfallSprite = Sprite.Create(thirdWaterfallTex, new Rect(0, 0, thirdWaterfallTex.width, thirdWaterfallTex.height), new Vector2(0.5f, 0.5f));

        // 実家のタイル
        nobilityTileTex = Resources.Load<Texture2D>("Map/Nobility_Tile");
        leftWallTex = Resources.Load<Texture2D>("Map/Left_wall");
        rightWallTex = Resources.Load<Texture2D>("Map/Right_Wall");
        cornerWallTex = Resources.Load<Texture2D>("Map/Corner_Wall");

        // 全エリアで Serene_Village をロード（area!=0 で使用、area==0 でもフォールバック用）
        tilesetTexture = Resources.Load<Texture2D>("Map/Serene_Village_32x32");
        if (tilesetTexture == null)
        {
            Debug.LogError("[MapManager] タイルセットが見つかりません");
            return;
        }

        // 各タイルのスプライトを作成
        tileSprites[TILE_GRASS] = CreateTileSprite(4, 0);
        tileSprites[TILE_WATER] = CreateTileSprite(0, 0);
        tileSprites[TILE_DIRT] = CreateTileSprite(6, 0);
        tileSprites[TILE_PATH] = CreateTileSprite(8, 1);
        tileSprites[TILE_TREE] = CreateTileSprite(11, 5);
        tileSprites[TILE_ROCK] = CreateTileSprite(0, 6);
        tileSprites[TILE_FENCE] = CreateTileSprite(4, 7);
        tileSprites[TILE_FLOWER] = CreateTileSprite(4, 6);
        tileSprites[TILE_HOUSE_RED] = CreateTileSprite(0, 4);
        tileSprites[TILE_HOUSE_GREEN] = CreateTileSprite(2, 4);
        tileSprites[TILE_HOUSE_BLUE] = CreateTileSprite(4, 4);
    }

    void LoadPixelCrawlerTextures()
    {
        pcTreesTex = Resources.Load<Texture2D>("Map/Pixel Crawler - Free Pack/Environment/Props/Static/Trees/Model_01/Size_02");
        pcVegetationTex = Resources.Load<Texture2D>("Map/Pixel Crawler - Free Pack/Environment/Props/Static/Vegetation");
        pcRocksTex = Resources.Load<Texture2D>("Map/Pixel Crawler - Free Pack/Environment/Props/Static/Rocks");
        pcRoofsTex = Resources.Load<Texture2D>("Map/Pixel Crawler - Free Pack/Environment/Structures/Buildings/Roofs");
        pcWallsTex = Resources.Load<Texture2D>("Map/Pixel Crawler - Free Pack/Environment/Structures/Buildings/Walls");

        lawnTex = Resources.Load<Texture2D>("Map/Lawn_Road");
        dirtRoadTex = Resources.Load<Texture2D>("Map/Dirt_Road");
        dirtBgTex = Resources.Load<Texture2D>("Map/Dirt_BackGround");
        lakeBgTex = Resources.Load<Texture2D>("Map/Lake_Background_1");
        if (lawnTex == null) Debug.LogWarning("[MapManager] Lawn_Road テクスチャが見つかりません");
        if (dirtRoadTex == null) Debug.LogWarning("[MapManager] Dirt_Road テクスチャが見つかりません");
        if (dirtBgTex == null) Debug.LogWarning("[MapManager] Dirt_BackGround テクスチャが見つかりません");
        if (lakeBgTex == null) Debug.LogWarning("[MapManager] Lake_BackGround2 テクスチャが見つかりません");
        if (pcTreesTex == null) Debug.LogWarning("[MapManager] PC Trees テクスチャが見つかりません");
        if (pcVegetationTex == null) Debug.LogWarning("[MapManager] PC Vegetation テクスチャが見つかりません");
        if (pcRocksTex == null) Debug.LogWarning("[MapManager] PC Rocks テクスチャが見つかりません");
        if (pcRoofsTex == null) Debug.LogWarning("[MapManager] PC Roofs テクスチャが見つかりません");
        if (pcWallsTex == null) Debug.LogWarning("[MapManager] PC Walls テクスチャが見つかりません");
    }

    // 画像左上座標指定 → Unity底辺原点に変換して Sprite.Create()
    Sprite CreatePCSprite(Texture2D tex, int fromLeftX, int fromTopY, int w, int h)
    {
        if (tex == null) return null;
        int unityY = tex.height - fromTopY - h;
        if (fromLeftX < 0 || unityY < 0 || fromLeftX + w > tex.width || unityY + h > tex.height)
        {
            Debug.LogWarning($"[MapManager] PC Sprite 座標範囲外: tex={tex.name} x={fromLeftX} y={fromTopY} w={w} h={h}");
            return null;
        }
        Rect rect = new Rect(fromLeftX, unityY, w, h);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f);
    }

    // ========== UI Toolkit セットアップ（area==0 DQタイル用） ==========

    void CreateVillageTileGrid()
    {
        // PanelSettings を動的生成
        villagePanelSettings = ScriptableObject.CreateInstance<UIE.PanelSettings>();
        villagePanelSettings.scaleMode = UIE.PanelScaleMode.ScaleWithScreenSize;
        villagePanelSettings.referenceResolution = new Vector2Int(1080, 1920);
        villagePanelSettings.screenMatchMode = UIE.PanelScreenMatchMode.MatchWidthOrHeight;
        villagePanelSettings.match = 0f;
        villagePanelSettings.sortingOrder = -1f; // Canvas より背面に描画

        // UIDocument を独立した子GameObjectに追加（親にUIDocumentがあると子が継承して衝突する）
        var villageObj = new GameObject("VillageTileUI");
        villageObj.transform.SetParent(transform, false);
        villageUIDoc = villageObj.AddComponent<UIE.UIDocument>();
        villageUIDoc.panelSettings = villagePanelSettings;

        var root = villageUIDoc.rootVisualElement;
        root.pickingMode = UIE.PickingMode.Ignore;
        root.focusable = false;

        // USS ロード
        var styleSheet = Resources.Load<UIE.StyleSheet>("UI/MapVillageStyle");
        if (styleSheet != null)
            root.styleSheets.Add(styleSheet);
        else
            Debug.LogWarning("[MapManager] MapVillageStyle.uss が見つかりません");

        root.AddToClassList("map-background");

        // タイルグリッドコンテナ（サイズはマップに合わせて動的に設定）
        tileGridRoot = new UIE.VisualElement();
        tileGridRoot.name = "tile-grid";
        tileGridRoot.AddToClassList("tile-grid");
        tileGridRoot.style.width = mapWidth * DISPLAY_TILE;
        tileGridRoot.style.height = mapHeight * DISPLAY_TILE;
        root.Add(tileGridRoot);
    }

    void CreateDQTileElement(int x, int y, int tileType)
    {
        if (tileGridRoot == null) return;

        var tile = new UIE.VisualElement();
        tile.name = $"tile-{x}-{y}";
        tile.AddToClassList("dq-tile");
        tile.style.left = x * DISPLAY_TILE;
        tile.style.top = (mapHeight - 1 - y) * DISPLAY_TILE;

        Random.State oldState = Random.state;
        Random.InitState(x * 100 + y);

        switch (tileType)
        {
            case TILE_GRASS:
            case TILE_TREE:
            case TILE_ROCK:
            case TILE_FLOWER:
            case TILE_FENCE:
            case TILE_HOUSE_RED:
            case TILE_HOUSE_GREEN:
            case TILE_HOUSE_BLUE:
            case TILE_FATHER_HOUSE:
            case TILE_JUKU:
                ApplyDQGrassStyle(tile);
                break;
            case TILE_PATH:
                ApplyDQPathStyle(tile);
                break;
            case TILE_WATER:
                ApplyDQWaterStyle(tile);
                break;
            case TILE_DIRT:
                ApplyDQDirtStyle(tile, false);
                break;
            case TILE_DARK_DIRT:
            case TILE_BOSS_GATE:
                ApplyDQDirtStyle(tile, true);
                break;
            case TILE_BOSS_MANSION:
            case TILE_WEAPON_SHOP:
                ApplyDQGrassStyle(tile);
                break;
            case TILE_FATHER_FLOOR:
                if (nobilityTileTex != null)
                {
                    tile.style.backgroundImage = new UIE.StyleBackground(nobilityTileTex);
                    tile.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
                }
                else
                {
                    tile.style.backgroundColor = GetTileColor(tileType);
                }
                break;
            default:
                Color c = GetTileColor(tileType);
                tile.style.backgroundColor = c;
                break;
        }

        Random.state = oldState;
        tileGridRoot.Add(tile);
    }

    void ApplyDQGrassStyle(UIE.VisualElement tile)
    {
        tile.AddToClassList("dq-grass");

        if (lawnTex != null)
        {
            tile.style.backgroundImage = new UIE.StyleBackground(lawnTex);
            tile.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
        }
        else
        {
            // フォールバック: プロシージャル
            float h = Random.Range(-0.02f, 0.02f);
            float b = Random.Range(-0.03f, 0.03f);
            tile.style.backgroundColor = new Color(0.25f + h, 0.55f + b, 0.18f + h);
            AddDQTileBorder(tile, "dq-grass-border");
        }
    }

    void ApplyDQPathStyle(UIE.VisualElement tile)
    {
        tile.AddToClassList("dq-path");

        if (dirtRoadTex != null)
        {
            tile.style.backgroundImage = new UIE.StyleBackground(dirtRoadTex);
            tile.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
        }
        else
        {
            // フォールバック: プロシージャル
            float h = Random.Range(-0.02f, 0.02f);
            float b = Random.Range(-0.03f, 0.03f);
            tile.style.backgroundColor = new Color(0.72f + h, 0.58f + b, 0.38f + h);
            AddDQTileBorder(tile, "dq-path-border");
        }
    }

    void ApplyDQWaterStyle(UIE.VisualElement tile)
    {
        tile.AddToClassList("dq-water");

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        Texture2D tex = (area == 1 && poisonLakeTex != null) ? poisonLakeTex : lakeBgTex;

        if (tex != null)
        {
            tile.style.backgroundImage = new UIE.StyleBackground(tex);
            tile.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
        }
        else
        {
            // フォールバック: プロシージャル
            float h = Random.Range(-0.02f, 0.02f);
            float b = Random.Range(-0.03f, 0.03f);
            tile.style.backgroundColor = new Color(0.20f + h, 0.45f + b, 0.75f + h);
            AddDQTileBorder(tile, "dq-water-border");
        }
    }

    void ApplyDQDirtStyle(UIE.VisualElement tile, bool dark)
    {
        tile.AddToClassList(dark ? "dq-dark-dirt" : "dq-dirt");

        if (lawnTex != null)
        {
            tile.style.backgroundImage = new UIE.StyleBackground(lawnTex);
            tile.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
        }
        else
        {
            // フォールバック: プロシージャル
            float h = Random.Range(-0.02f, 0.02f);
            if (dark)
                tile.style.backgroundColor = new Color(0.30f + h, 0.22f + h, 0.15f + h);
            else
                tile.style.backgroundColor = new Color(0.52f + h, 0.38f + h, 0.22f + h);
            AddDQTileBorder(tile, dark ? "dq-dark-dirt-border" : "dq-dirt-border");
        }
    }

    void AddDQTileBorder(UIE.VisualElement tile, string borderClass)
    {
        var borderTop = new UIE.VisualElement();
        borderTop.AddToClassList("tile-border-top");
        borderTop.AddToClassList(borderClass);
        tile.Add(borderTop);

        var borderLeft = new UIE.VisualElement();
        borderLeft.AddToClassList("tile-border-left");
        borderLeft.AddToClassList(borderClass);
        tile.Add(borderLeft);
    }

    // ========== UI Toolkit 柵オーバーレイ（area==0） ==========

    void CreateDQFenceElement(int tileX, int tileY)
    {
        if (tileGridRoot == null) return;

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        var fence = new UIE.VisualElement();
        fence.name = $"fence-{tileX}-{tileY}";
        fence.AddToClassList("fence-container");
        fence.style.left = tileX * DISPLAY_TILE;
        fence.style.top = (mapHeight - 1 - tileY) * DISPLAY_TILE;
        fence.style.width = DISPLAY_TILE;
        fence.style.height = DISPLAY_TILE;

        if (area == 1 && poisonFenceTex != null)
        {
            // ゴージャス・ヴィレッジ: Poison_Fence スプライトを使用
            fence.style.backgroundImage = new UIE.StyleBackground(poisonFenceTex);
            fence.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
        }
        else
        {
            // 通常: プロシージャル柵
            // 横棒2本
            for (int i = 0; i < 2; i++)
            {
                float ry = (i == 0) ? DISPLAY_TILE * 0.35f : DISPLAY_TILE * 0.65f;
                var rail = new UIE.VisualElement();
                rail.AddToClassList("fence-rail");
                rail.style.left = DISPLAY_TILE * 0.05f;
                rail.style.top = ry - 3f;
                rail.style.width = DISPLAY_TILE * 0.9f;
                fence.Add(rail);
            }
            // 縦柱3本
            for (int i = 0; i < 3; i++)
            {
                float px = (i * 0.35f + 0.15f) * DISPLAY_TILE;
                var post = new UIE.VisualElement();
                post.AddToClassList("fence-post");
                post.style.left = px - 2.5f;
                post.style.top = DISPLAY_TILE * 0.2f;
                post.style.height = DISPLAY_TILE * 0.6f;
                fence.Add(post);
            }
        }

        tileGridRoot.Add(fence);
    }

    // ========== UI Toolkit 家ドアオーバーレイ（area==0） ==========

    void CreateDQHouseDoorElement(float centerLeft, float centerTop)
    {
        if (tileGridRoot == null) return;

        var door = new UIE.VisualElement();
        door.AddToClassList("house-door");
        float doorW = DISPLAY_TILE * 0.4f;
        float doorH = DISPLAY_TILE * 0.55f;
        door.style.left = centerLeft - doorW / 2f;
        door.style.top = centerTop + DISPLAY_TILE * 0.55f - doorH / 2f;
        door.style.width = doorW;
        door.style.height = doorH;
        tileGridRoot.Add(door);
    }

    Sprite CreateTileSprite(int col, int row)
    {
        if (tilesetTexture == null) return null;

        int x = col * TILE_SIZE;
        int y = tilesetTexture.height - (row + 1) * TILE_SIZE;

        if (x < 0 || y < 0 || x + TILE_SIZE > tilesetTexture.width || y + TILE_SIZE > tilesetTexture.height)
        {
            Debug.LogWarning($"[MapManager] タイル座標が範囲外: col={col}, row={row}");
            return null;
        }

        Rect rect = new Rect(x, y, TILE_SIZE, TILE_SIZE);
        return Sprite.Create(tilesetTexture, rect, new Vector2(0.5f, 0.5f), 100f);
    }

    void GenerateMapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 基本は草で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_GRASS;
                walkable[x, y] = true;
            }
        }

        // === 外周（通行不可） ===
        for (int x = 0; x < mapWidth; x++)
        {
            walkable[x, 0] = false;
            walkable[x, mapHeight - 1] = false;
        }
        for (int y = 0; y < mapHeight; y++)
        {
            walkable[0, y] = false;
            walkable[mapWidth - 1, y] = false;
        }

        // === 南端の森（y=0〜1）— 木で埋めて空白を防ぐ ===
        for (int x = 0; x < mapWidth; x++)
        {
            mapData[x, 0] = TILE_TREE;
            walkable[x, 0] = false;
            mapData[x, 1] = TILE_TREE;
            walkable[x, 1] = false;
        }

        // === 南端フェンス（y=2）— 池以外を柵で塞ぐ ===
        for (int x = 1; x <= 10; x++)
        {
            if (x < 4 || x > 6)  // 池(x=4〜6)以外
            {
                mapData[x, 2] = TILE_FENCE;
                walkable[x, 2] = false;
            }
        }

        // === 池周りの花（y=2, y=3の池の外側） ===
        mapData[3, 2] = TILE_FLOWER; walkable[3, 2] = false;
        mapData[7, 2] = TILE_FLOWER; walkable[7, 2] = false;
        mapData[3, 3] = TILE_FLOWER; walkable[3, 3] = false;
        mapData[7, 3] = TILE_FLOWER; walkable[7, 3] = false;

        // ============================================================
        // 南エリア（y=0〜13）— 村の中心
        // ============================================================

        // === 道（メインストリート + 縦パス） ===
        // 横メインストリート y=9, y=10 (x=1〜x=10)
        for (int x = 1; x <= 10; x++)
        {
            mapData[x, 9] = TILE_PATH;
            mapData[x, 10] = TILE_PATH;
        }

        // 縦メインパス x=5 (y=4〜y=35 — 村の南端からボス手前まで)
        for (int y = 4; y <= 35; y++)
        {
            mapData[5, y] = TILE_PATH;
        }

        // === 池（x=4〜6, y=2〜3） ===
        for (int x = 4; x <= 6; x++)
        {
            for (int y = 2; y <= 3; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 実家（x=9〜10, y=6〜7） ===
        mapData[9, 6] = TILE_HOUSE_BLUE;
        walkable[9, 6] = false;
        mapData[10, 6] = TILE_HOUSE_BLUE;
        walkable[10, 6] = false;
        mapData[9, 7] = TILE_HOUSE_BLUE;
        walkable[9, 7] = false;
        mapData[10, 7] = TILE_HOUSE_BLUE;
        walkable[10, 7] = false;
        // 母親NPC位置は歩けるようにする
        mapData[9, 5] = TILE_PATH;
        walkable[9, 5] = true;

        // ============================================================
        // 中央エリア（y=14〜32）— 村の北部・拡張地域
        // ============================================================

        // 2本目の横道 y=16 (x=2〜9)
        for (int x = 2; x <= 9; x++)
        {
            mapData[x, 16] = TILE_PATH;
        }

        // === 武器ショップ（x=2〜3, y=14〜15） ===
        for (int sx = 2; sx <= 3; sx++)
        {
            for (int sy = 14; sy <= 15; sy++)
            {
                mapData[sx, sy] = TILE_WEAPON_SHOP;
                walkable[sx, sy] = false;
            }
        }
        // ショップへの接続パス
        mapData[4, 14] = TILE_PATH;
        walkable[4, 14] = true;

        // 小さな池2（x=2〜3, y=18）
        mapData[2, 18] = TILE_WATER;
        walkable[2, 18] = false;
        mapData[3, 18] = TILE_WATER;
        walkable[3, 18] = false;

        // 3本目の横道 y=24 (x=3〜10)
        for (int x = 3; x <= 10; x++)
        {
            mapData[x, 24] = TILE_PATH;
        }

        // === 塾（x=9〜10, y=26〜27） ===
        mapData[9, 26] = TILE_HOUSE_BLUE;
        walkable[9, 26] = false;
        mapData[10, 26] = TILE_HOUSE_BLUE;
        walkable[10, 26] = false;
        mapData[9, 27] = TILE_HOUSE_BLUE;
        walkable[9, 27] = false;
        mapData[10, 27] = TILE_HOUSE_BLUE;
        walkable[10, 27] = false;
        // 塾の入口タイル
        mapData[9, 25] = TILE_JUKU;
        walkable[9, 25] = false;

        // === 父親の家（左上 x=2〜3, y=33〜34） ===
        mapData[2, 33] = TILE_HOUSE_RED;
        walkable[2, 33] = false;
        mapData[3, 33] = TILE_HOUSE_RED;
        walkable[3, 33] = false;
        mapData[2, 34] = TILE_HOUSE_RED;
        walkable[2, 34] = false;
        mapData[3, 34] = TILE_HOUSE_RED;
        walkable[3, 34] = false;
        // 入口タイル（家に入れる）
        mapData[3, 32] = TILE_FATHER_HOUSE;
        walkable[3, 32] = false;
        // 入口パス（メインストリートx=5への接続）
        mapData[4, 32] = TILE_PATH;
        walkable[4, 32] = true;

        // ============================================================
        // 北エリア（y=33〜38）— ボスの館
        // ============================================================

        // ボス接続パス y=35 (x=5〜7)
        for (int x = 5; x <= 7; x++)
        {
            mapData[x, 35] = TILE_PATH;
        }

        // 暗い土 (x=6〜10, y=38), (x=6, y=36), (x=10, y=36)
        for (int x = 6; x <= 10; x++)
        {
            mapData[x, 38] = TILE_DARK_DIRT;
        }
        mapData[6, 36] = TILE_DARK_DIRT;
        mapData[10, 36] = TILE_DARK_DIRT;

        // 館本体 3x2 (x=7〜9, y=36〜37)
        for (int x = 7; x <= 9; x++)
        {
            for (int y = 36; y <= 37; y++)
            {
                mapData[x, y] = TILE_BOSS_MANSION;
                walkable[x, y] = false;
            }
        }

        // ボスの門 (8, 35)
        mapData[8, 35] = TILE_BOSS_GATE;
        walkable[8, 35] = false;

        // プレイヤーの初期位置は必ず歩けるようにする
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_PATH && mapData[playerTileX, playerTileY] != TILE_GRASS)
        {
            mapData[playerTileX, playerTileY] = TILE_PATH;
        }
    }

    void GenerateDevilMapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // =============================================
        // Step 1: 全タイルを毒草壁（非歩行）で初期化
        // =============================================
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_GRASS;
                walkable[x, y] = false;
            }

        // === 外周 ===
        for (int x = 0; x < mapWidth; x++)
        {
            mapData[x, 0] = TILE_FENCE; walkable[x, 0] = false;
            mapData[x, mapHeight - 1] = TILE_FENCE; walkable[x, mapHeight - 1] = false;
        }
        for (int y = 0; y < mapHeight; y++)
        {
            mapData[0, y] = TILE_GRASS; walkable[0, y] = false;
            mapData[mapWidth - 1, y] = TILE_GRASS; walkable[mapWidth - 1, y] = false;
        }
        mapData[0, 0] = TILE_FENCE; mapData[mapWidth - 1, 0] = TILE_FENCE;
        mapData[0, mapHeight - 1] = TILE_FENCE; mapData[mapWidth - 1, mapHeight - 1] = TILE_FENCE;

        // === エリア出口（南端） ===
        mapData[5, 0] = TILE_AREA_EXIT;
        walkable[5, 0] = true;

        // =============================================
        // Step 2: Perlinノイズで蛇行するメインパス (y=1〜32)
        // =============================================
        float perlinSeed = 42.5f;
        int[] pathCenterX = new int[mapHeight]; // 各yでのパス中心xを記録

        int prevCX = 5; // 出口のx位置から開始
        for (int y = 1; y <= 32; y++)
        {
            // 溶岩川ゾーン(y=17〜21)はスキップ（後で橋を通す）
            if (y >= 17 && y <= 21)
            {
                pathCenterX[y] = prevCX;
                continue;
            }

            float noise = Mathf.PerlinNoise(perlinSeed, y * 0.15f);
            float offset = (noise - 0.5f) * 7.0f;
            int cx = Mathf.RoundToInt(5 + offset);
            cx = Mathf.Clamp(cx, 2, mapWidth - 3);

            // 3タイル幅の道を掘る
            for (int dx = -1; dx <= 1; dx++)
            {
                int px = cx + dx;
                if (px >= 1 && px <= mapWidth - 2)
                {
                    mapData[px, y] = TILE_DARK_DIRT;
                    walkable[px, y] = true;
                }
            }

            // 前行との段差が大きい場合、横に繋ぎを入れて接続を保証
            if (y > 1 && y != 22) // y=22は橋接続で別途処理
            {
                int bridgeMin = Mathf.Min(cx, prevCX) - 1;
                int bridgeMax = Mathf.Max(cx, prevCX) + 1;
                for (int bx = bridgeMin; bx <= bridgeMax; bx++)
                {
                    if (bx >= 1 && bx <= mapWidth - 2 && !walkable[bx, y])
                    {
                        mapData[bx, y] = TILE_DARK_DIRT;
                        walkable[bx, y] = true;
                    }
                }
            }

            pathCenterX[y] = cx;
            prevCX = cx;
        }

        // =============================================
        // Step 3: 溶岩の川 (y=17〜21) + 橋
        // =============================================
        // 橋の位置: y=16のパス中心に合わせる
        int bridgeCX = pathCenterX[16];
        if (bridgeCX < 3) bridgeCX = 3;
        if (bridgeCX > mapWidth - 4) bridgeCX = mapWidth - 4;

        // 溶岩川をPerlinで不規則に配置
        for (int y = 17; y <= 21; y++)
        {
            for (int x = 1; x <= mapWidth - 2; x++)
            {
                float riverNoise = Mathf.PerlinNoise(x * 0.3f + 10f, y * 0.3f + 10f);
                int distFromCenter = Mathf.Abs(y - 19);
                float threshold = 0.4f + distFromCenter * 0.12f;

                bool isRiver = riverNoise < threshold;
                bool isBridge = (x >= bridgeCX - 1 && x <= bridgeCX + 1);

                if (isRiver && !isBridge)
                {
                    mapData[x, y] = TILE_WATER;
                    walkable[x, y] = false;
                }
            }
        }

        // 橋を掘る (TILE_DARK_DIRT, 3タイル幅)
        for (int y = 17; y <= 21; y++)
        {
            for (int bx = bridgeCX - 1; bx <= bridgeCX + 1; bx++)
            {
                if (bx >= 1 && bx <= mapWidth - 2)
                {
                    mapData[bx, y] = TILE_DARK_DIRT;
                    walkable[bx, y] = true;
                }
            }
        }

        // 橋の前後を道に接続 (y=16, y=22)
        for (int bx = bridgeCX - 1; bx <= bridgeCX + 1; bx++)
        {
            if (bx >= 1 && bx <= mapWidth - 2)
            {
                if (mapData[bx, 16] == TILE_GRASS) { mapData[bx, 16] = TILE_DARK_DIRT; walkable[bx, 16] = true; }
                if (mapData[bx, 22] == TILE_GRASS) { mapData[bx, 22] = TILE_DARK_DIRT; walkable[bx, 22] = true; }
            }
        }
        // y=16とy=22のパス中心も橋と繋げる
        int cx16 = pathCenterX[16];
        int cx22 = pathCenterX[22];
        for (int x = Mathf.Min(cx16, bridgeCX) - 1; x <= Mathf.Max(cx16, bridgeCX) + 1; x++)
            if (x >= 1 && x <= mapWidth - 2) { mapData[x, 16] = TILE_DARK_DIRT; walkable[x, 16] = true; }
        for (int x = Mathf.Min(cx22, bridgeCX) - 1; x <= Mathf.Max(cx22, bridgeCX) + 1; x++)
            if (x >= 1 && x <= mapWidth - 2) { mapData[x, 22] = TILE_DARK_DIRT; walkable[x, 22] = true; }

        // =============================================
        // Step 4: 儀式の回廊 — L字アプローチ + ボスの館
        // =============================================
        // 館本体 3x2 (x=8〜10, y=37〜38) — 右上隅
        for (int x = 8; x <= 10; x++)
            for (int y = 37; y <= 38; y++)
            {
                mapData[x, y] = TILE_BOSS_MANSION;
                walkable[x, y] = false;
            }

        // ボスの門 (9, 36) — 館の直下中央
        mapData[9, 36] = TILE_BOSS_GATE;
        walkable[9, 36] = false;

        // L字パス: y=32のパス中心から北上 → y=36で東に折れて門へ
        int approachX = pathCenterX[32];
        approachX = Mathf.Clamp(approachX, 3, 7);

        // 縦セグメント: (approachX, y=33〜36) を掘る
        for (int y = 33; y <= 36; y++)
        {
            mapData[approachX, y] = TILE_DARK_DIRT;
            walkable[approachX, y] = true;
        }
        // y=32からの接続も保証
        {
            int cx32 = pathCenterX[32];
            for (int x = Mathf.Min(cx32, approachX); x <= Mathf.Max(cx32, approachX); x++)
                if (x >= 1 && x <= mapWidth - 2)
                {
                    if (mapData[x, 32] == TILE_GRASS) { mapData[x, 32] = TILE_DARK_DIRT; walkable[x, 32] = true; }
                }
        }

        // 横セグメント: (approachX〜8, y=36) を掘る（門x=9の手前まで）
        for (int x = approachX; x <= 8; x++)
        {
            mapData[x, 36] = TILE_DARK_DIRT;
            walkable[x, 36] = true;
        }

        // シャンパン柵で両側を囲む（儀式的な警備感）
        // 縦セグメントの左右にシャンパン柵
        for (int y = 33; y <= 36; y++)
        {
            if (approachX - 1 >= 1 && mapData[approachX - 1, y] == TILE_GRASS)
            {
                mapData[approachX - 1, y] = TILE_CHAMPAGNE;
                walkable[approachX - 1, y] = false;
            }
            // 横セグメントとの角を避ける
            if (y < 36 && approachX + 1 <= mapWidth - 2 && mapData[approachX + 1, y] == TILE_GRASS)
            {
                mapData[approachX + 1, y] = TILE_CHAMPAGNE;
                walkable[approachX + 1, y] = false;
            }
        }
        // 横セグメントの上下にシャンパン柵
        for (int x = approachX + 1; x <= 8; x++)
        {
            if (mapData[x, 37] == TILE_GRASS)
            {
                mapData[x, 37] = TILE_CHAMPAGNE;
                walkable[x, 37] = false;
            }
            if (mapData[x, 35] == TILE_GRASS)
            {
                mapData[x, 35] = TILE_CHAMPAGNE;
                walkable[x, 35] = false;
            }
        }

        // =============================================
        // Step 5: 武器屋 (x=2〜3, y=11〜12) + 枝道
        // =============================================
        for (int sx = 2; sx <= 3; sx++)
            for (int sy = 11; sy <= 12; sy++)
            {
                mapData[sx, sy] = TILE_WEAPON_SHOP;
                walkable[sx, sy] = false;
            }

        // メインパスからショップ入口(x=4, y=11)への枝道
        int shopConnectY = 11;
        int mainAtShopY = pathCenterX[shopConnectY];
        mapData[4, shopConnectY] = TILE_DARK_DIRT;
        walkable[4, shopConnectY] = true;
        for (int x = Mathf.Min(4, mainAtShopY - 1); x <= Mathf.Max(4, mainAtShopY + 1); x++)
        {
            if (x >= 1 && x <= mapWidth - 2 && mapData[x, shopConnectY] == TILE_GRASS)
            {
                mapData[x, shopConnectY] = TILE_DARK_DIRT;
                walkable[x, shopConnectY] = true;
            }
        }

        // =============================================
        // Step 6: 歩行可能な草地広場（エンカウント用）
        // =============================================
        int[,] clearings = new int[,] {
            { 8, 4 },   // 南東（ミルクポイント9,4を含む）
            { 3, 6 },   // 南西
            { 8, 14 },  // 中央東
            { 3, 25 },  // 北西
            { 8, 28 },  // 北東
            { 3, 31 },  // 北端西
        };

        for (int i = 0; i < clearings.GetLength(0); i++)
        {
            int ccx = clearings[i, 0];
            int ccy = clearings[i, 1];

            // 3x2 の歩行可能な草地を配置
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = 0; dy <= 1; dy++)
                {
                    int px = ccx + dx, py = ccy + dy;
                    if (px >= 1 && px <= mapWidth - 2 && py >= 1 && py <= mapHeight - 2)
                    {
                        if (mapData[px, py] == TILE_GRASS)
                            walkable[px, py] = true; // TILE_GRASSのまま歩行可能に
                    }
                }

            // 広場からメインパスへ1タイル幅の接続道を掘る
            DevilConnectToPath(ccx, ccy);
        }

        // ミルクポイント(9,4)を確実に歩行可能にする
        if (mapData[9, 4] == TILE_GRASS) walkable[9, 4] = true;

        // =============================================
        // Step 7: 隠し通路（2〜3箇所）
        // =============================================
        // 隠し通路1: y=6付近、パスの東側にショートカット
        int hp1cx = pathCenterX[6];
        if (hp1cx + 3 <= mapWidth - 2 && mapData[hp1cx + 3, 6] == TILE_GRASS)
        {
            walkable[hp1cx + 3, 6] = true;
            walkable[hp1cx + 3, 7] = true;
        }

        // 隠し通路2: y=26付近、パスの西側にショートカット
        int hp2cx = pathCenterX[26];
        if (hp2cx - 3 >= 1 && mapData[hp2cx - 3, 26] == TILE_GRASS)
        {
            walkable[hp2cx - 3, 26] = true;
            walkable[hp2cx - 3, 27] = true;
        }

        // 隠し通路3: 溶岩川の東側、危険な裏道 (橋から離れた位置)
        int hp3x = bridgeCX + 4;
        if (hp3x <= mapWidth - 2)
        {
            for (int y = 17; y <= 21; y++)
            {
                if (mapData[hp3x, y] == TILE_GRASS)
                    walkable[hp3x, y] = true;
            }
        }

        // =============================================
        // Step 8: 接続性検証（BFS）
        // =============================================
        DevilVerifyConnectivity();

        // =============================================
        // Step 9: プレイヤー初期位置の安全確保
        // =============================================
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_DARK_DIRT && mapData[playerTileX, playerTileY] != TILE_AREA_EXIT)
            mapData[playerTileX, playerTileY] = TILE_DARK_DIRT;
    }

    // --- Devil Map ヘルパー ---

    /// <summary>Area 3: 湖エリアに1枚絵のlake.pngオーバーレイを配置</summary>
    void CreateLakeOverlay(int x1, int x2, int y1, int y2)
    {
        float cx = ((x1 + x2) / 2f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float cy = ((y1 + y2) / 2f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        float w = (x2 - x1 + 1) * DISPLAY_TILE;
        float h = (y2 - y1 + 1) * DISPLAY_TILE;

        var lake = new GameObject("LakeOverlay");
        lake.transform.SetParent(tilesContainer.transform, false);
        var rect = lake.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(cx, cy);
        rect.sizeDelta = new Vector2(w, h);
        var img = lake.AddComponent<Image>();
        img.sprite = thirdLakeSprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = false;
    }

    /// <summary>広場からメインパスへ横方向に接続道を掘る</summary>
    void DevilConnectToPath(int fromX, int fromY)
    {
        // 右方向を探索
        for (int dist = 1; dist < mapWidth; dist++)
        {
            int rx = fromX + dist;
            if (rx > mapWidth - 2) break;
            if (walkable[rx, fromY] && mapData[rx, fromY] == TILE_DARK_DIRT)
            {
                for (int x = fromX + 1; x < rx; x++)
                    if (mapData[x, fromY] == TILE_GRASS)
                    {
                        mapData[x, fromY] = TILE_DARK_DIRT;
                        walkable[x, fromY] = true;
                    }
                return;
            }
        }
        // 左方向を探索
        for (int dist = 1; dist < mapWidth; dist++)
        {
            int lx = fromX - dist;
            if (lx < 1) break;
            if (walkable[lx, fromY] && mapData[lx, fromY] == TILE_DARK_DIRT)
            {
                for (int x = lx + 1; x < fromX; x++)
                    if (mapData[x, fromY] == TILE_GRASS)
                    {
                        mapData[x, fromY] = TILE_DARK_DIRT;
                        walkable[x, fromY] = true;
                    }
                return;
            }
        }
        // 同一yにパスが見つからない場合、上下1行ずらして再試行
        if (fromY + 1 < mapHeight - 1)
        {
            for (int dist = 1; dist < mapWidth; dist++)
            {
                int rx = fromX + dist;
                if (rx > mapWidth - 2) break;
                if (walkable[rx, fromY + 1] && mapData[rx, fromY + 1] == TILE_DARK_DIRT)
                {
                    // まず縦に1タイル繋ぐ
                    if (mapData[fromX, fromY + 1] == TILE_GRASS)
                    {
                        mapData[fromX, fromY + 1] = TILE_DARK_DIRT;
                        walkable[fromX, fromY + 1] = true;
                    }
                    for (int x = fromX + 1; x < rx; x++)
                        if (mapData[x, fromY + 1] == TILE_GRASS)
                        {
                            mapData[x, fromY + 1] = TILE_DARK_DIRT;
                            walkable[x, fromY + 1] = true;
                        }
                    return;
                }
            }
        }
    }

    /// <summary>出口からBFSで全歩行可能タイルの到達性を検証、孤立タイルは非歩行に戻す</summary>
    void DevilVerifyConnectivity()
    {
        bool[,] visited = new bool[mapWidth, mapHeight];
        var queue = new Queue<Vector2Int>();

        // 出口付近（5,1）か、歩行可能な最初のタイルからBFS開始
        int startX = 5, startY = 1;
        if (!walkable[startX, startY])
        {
            // 出口付近で歩行可能タイルを探す
            for (int y = 1; y <= 3 && !walkable[startX, startY]; y++)
                for (int x = 4; x <= 6 && !walkable[startX, startY]; x++)
                    if (walkable[x, y]) { startX = x; startY = y; }
        }

        if (!walkable[startX, startY]) return; // 安全弁

        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();
            for (int d = 0; d < 4; d++)
            {
                int nx = pos.x + dx[d], ny = pos.y + dy[d];
                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight
                    && !visited[nx, ny] && walkable[nx, ny])
                {
                    visited[nx, ny] = true;
                    queue.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        // 到達できなかった歩行可能タイルは非歩行に戻す
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                if (walkable[x, y] && !visited[x, y])
                    walkable[x, y] = false;
    }

    void GenerateFatherHouseMapData()
    {
        // 16x18 の宮殿
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 全て壁で埋める
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_FATHER_WALL;
                walkable[x, y] = false;
            }

        // メインフロア (x=1〜14, y=1〜16)
        for (int x = 1; x <= 14; x++)
            for (int y = 1; y <= 16; y++)
            {
                mapData[x, y] = TILE_FATHER_FLOOR;
                walkable[x, y] = true;
            }

        // 出口 (7,0)(8,0)
        mapData[7, 0] = TILE_FATHER_EXIT;
        walkable[7, 0] = false;
        mapData[8, 0] = TILE_FATHER_EXIT;
        walkable[8, 0] = false;

        // ===== お手伝いさんNPC位置 (歩行可能) =====
        // (2,3)(13,3)(2,7)(13,7)(2,11)(13,11)

        // ===== 仕切り壁 (y=12, 寝室とリビングの間) =====
        for (int x = 1; x <= 6; x++)
        {
            walkable[x, 12] = false;
        }
        // 通路 x=7,8 は歩行可能のまま
        for (int x = 9; x <= 14; x++)
        {
            walkable[x, 12] = false;
        }

        // ===== 寝室エリア (x=9-14, y=13-16) =====
        // ベッド (12-13, 15-16) — 通行不可
        walkable[12, 16] = false;
        walkable[13, 16] = false;
        walkable[12, 15] = false;
        walkable[13, 15] = false;
        if (parentsInBed)
        {
            // ベッドの上はNPC位置なので歩行可能
            walkable[12, 15] = true;
            walkable[13, 15] = true;
        }

        // ===== 書斎エリア (x=1-6, y=13-16) =====
        // 机 (3,14)(4,14)
        walkable[3, 14] = false;
        walkable[4, 14] = false;

        // ===== 大広間（ソファ・テーブル削除済み） =====
    }

    void CreateFatherHouseFurniture()
    {
        if (tilesContainer == null) return;

        Color gold = new Color(0.85f, 0.70f, 0.35f);
        Color darkGold = new Color(0.65f, 0.50f, 0.20f);
        Color marble = new Color(0.92f, 0.90f, 0.88f);
        Color carpet = new Color(0.6f, 0.15f, 0.15f, 0.45f);
        Color woodDark = new Color(0.35f, 0.22f, 0.12f);

        // ===== レッドカーペット（エントランス→大広間 中央通路） =====
        for (int y = 1; y <= 11; y++)
            CreateFurnitureOverlay(7.5f, y, DISPLAY_TILE * 2.2f, DISPLAY_TILE * 1.05f, carpet);
        // 寝室への通路カーペット
        for (int y = 12; y <= 13; y++)
            CreateFurnitureOverlay(7.5f, y, DISPLAY_TILE * 2.2f, DISPLAY_TILE * 1.05f, carpet);

        // お手伝いさんNPCの下にラグを敷く
        Sprite maidRagSprite = Resources.Load<Sprite>("Map/Maid_Rag");
        if (maidRagSprite != null)
        {
            for (int i = 0; i < maidNpcX.Length; i++)
                CreateFurnitureSprite(maidNpcX[i], maidNpcY[i], DISPLAY_TILE * 0.75f, DISPLAY_TILE * 0.75f, maidRagSprite);
        }

        // ===== 仕切り壁 =====
        Sprite divideWallSprite = Resources.Load<Sprite>("Map/Divide_Wall");
        if (divideWallSprite != null)
        {
            // 左側 (x=1〜6, 左端から1枚)
            CreateFurnitureSprite(3.5f, 12f, DISPLAY_TILE * 6f, DISPLAY_TILE, divideWallSprite);
            // 右側 (x=9〜14, 右端から1枚)
            CreateFurnitureSprite(11.5f, 12f, DISPLAY_TILE * 6f, DISPLAY_TILE, divideWallSprite);
            // 通路の隙間 (x=7,8) はレッドカーペットが通るので床タイル不要
        }
        else
        {
            CreateFurnitureOverlay(3.5f, 12f, DISPLAY_TILE * 6.2f, DISPLAY_TILE * 0.7f, darkGold);
            CreateFurnitureOverlay(11.5f, 12f, DISPLAY_TILE * 6.2f, DISPLAY_TILE * 0.7f, darkGold);
        }

        // ===== シャンデリア（大広間中央） =====
        CreateFurnitureLabel(7.5f, 6f, "✨", 28, gold);


        // ===== 寝室エリア (右上) =====
        // 寝室ラグ（半透明で家具が見えるように）
        Sprite bedCarpetSprite = Resources.Load<Sprite>("Map/Bed_Carpet");
        if (bedCarpetSprite != null)
            CreateFurnitureSprite(11.5f, 14.5f, DISPLAY_TILE * 5.5f, DISPLAY_TILE * 3.5f, bedCarpetSprite, 0.45f);
        else
            CreateFurnitureOverlay(11.5f, 14.5f, DISPLAY_TILE * 5.5f, DISPLAY_TILE * 3.5f,
                new Color(0.5f, 0.18f, 0.22f, 0.35f));

        // ベッド (12-13, 15-16) — スプライト表示
        Sprite bedSprite = Resources.Load<Sprite>("Map/Parents_Bed");
        if (bedSprite != null)
        {
            CreateFurnitureSprite(12.5f, 15.5f, DISPLAY_TILE * 2.6f, DISPLAY_TILE * 2.6f, bedSprite);
        }
        else
        {
            CreateFurnitureOverlay(12.5f, 15.5f, DISPLAY_TILE * 2.3f, DISPLAY_TILE * 2.3f, woodDark);
            CreateFurnitureOverlay(12.5f, 15.5f, DISPLAY_TILE * 2f, DISPLAY_TILE * 2f,
                new Color(1f, 0.718f, 0.773f, 0.7f));
        }
        if (parentsInBed)
            CreateFurnitureLabel(12.5f, 16.5f, "💤", 24, Color.white);

        // サイドテーブル
        Sprite nobilityChairSprite = Resources.Load<Sprite>("Map/Nobility _Chair");
        if (nobilityChairSprite != null)
            CreateFurnitureSprite(10f, 15.5f, DISPLAY_TILE * 0.8f, DISPLAY_TILE * 0.8f, nobilityChairSprite);
        else
        {
            CreateFurnitureOverlay(10f, 15.5f, DISPLAY_TILE * 0.6f, DISPLAY_TILE * 0.6f, woodDark);
            CreateFurnitureLabel(10f, 15.5f, "🕯", 14, gold);
        }

        // ===== 書斎エリア (左上) =====
        // 書斎ラグ（半透明で家具が見えるように、先に描画）
        Sprite studyCarpetSprite = Resources.Load<Sprite>("Map/Bed_Carpet");
        if (studyCarpetSprite != null)
            CreateFurnitureSprite(3.5f, 14.5f, DISPLAY_TILE * 4.5f, DISPLAY_TILE * 3.5f, studyCarpetSprite, 0.45f);
        else
            CreateFurnitureOverlay(3.5f, 14.5f, DISPLAY_TILE * 4.5f, DISPLAY_TILE * 3.5f,
                new Color(0.15f, 0.20f, 0.45f, 0.25f));
        // 机 (3-4, 14)（ラグの上に描画）
        Sprite nobilityDeskSprite = Resources.Load<Sprite>("Map/Nobility_Desk");
        if (nobilityDeskSprite != null)
            CreateFurnitureSprite(3.5f, 14f, DISPLAY_TILE * 1.7f, DISPLAY_TILE * 1.7f, nobilityDeskSprite);
        else
        {
            CreateFurnitureOverlay(3.5f, 14f, DISPLAY_TILE * 2.2f, DISPLAY_TILE * 0.8f, woodDark);
            CreateFurnitureLabel(3.5f, 14f, "📝", 14, gold);
        }

        // ===== 壁の絵画（装飾ラベル） =====
        CreateFurnitureLabel(1f, 5f, "🖼", 20, gold);
        CreateFurnitureLabel(14f, 5f, "🖼", 20, gold);
        CreateFurnitureLabel(1f, 10f, "🎭", 18, gold);
        CreateFurnitureLabel(14f, 10f, "🎭", 18, gold);

        // ===== 花瓶 =====
        CreateFurnitureLabel(1f, 1f, "🏺", 16, darkGold);
        CreateFurnitureLabel(14f, 1f, "🏺", 16, darkGold);

        // ===== 前面コーナー壁（1枚画像で5マス分） =====
        if (cornerWallTex != null)
        {
            Sprite cornerSprite = Sprite.Create(cornerWallTex,
                new Rect(0, 0, cornerWallTex.width, cornerWallTex.height),
                new Vector2(0.5f, 0.5f));
            // 左前コーナー (x=0, y=0〜2)
            CreateFurnitureSprite(0f, 1f, DISPLAY_TILE, DISPLAY_TILE * 3f, cornerSprite);
            // 右前コーナー (x=15, y=0〜2)
            CreateFurnitureSprite(15f, 1f, DISPLAY_TILE, DISPLAY_TILE * 3f, cornerSprite);
            // 左奥コーナー (x=0, y=15〜17)
            CreateFurnitureSprite(0f, 16f, DISPLAY_TILE, DISPLAY_TILE * 3f, cornerSprite);
            // 右奥コーナー (x=15, y=15〜17)
            CreateFurnitureSprite(15f, 16f, DISPLAY_TILE, DISPLAY_TILE * 3f, cornerSprite);
        }

        // ===== 出口マーク =====
        Sprite exitSprite = Resources.Load<Sprite>("Map/Exit");
        if (exitSprite != null)
            CreateFurnitureSprite(7.5f, 0f, DISPLAY_TILE * 3f, DISPLAY_TILE * 1.5f, exitSprite);
        else
            CreateFurnitureLabel(7.5f, 0f, "▽ 出口", 16, new Color(0.8f, 0.9f, 1f));
    }

    void CreateFurnitureOverlay(float tileX, float tileY, float width, float height, Color color)
    {
        float posX = (tileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (tileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var obj = new GameObject("Furniture");
        obj.transform.SetParent(tilesContainer.transform, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(width, height);
        var img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }

    void CreateFurnitureSprite(float tileX, float tileY, float width, float height, Sprite sprite, float alpha = 1f)
    {
        float posX = (tileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (tileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var obj = new GameObject("FurnitureSprite");
        obj.transform.SetParent(tilesContainer.transform, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(width, height);
        var img = obj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        if (alpha < 1f) img.color = new Color(1f, 1f, 1f, alpha);
    }

    void CreateFurnitureLabel(float tileX, float tileY, string text, int fontSize, Color color)
    {
        float posX = (tileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (tileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var obj = new GameObject("FurnitureLabel");
        obj.transform.SetParent(tilesContainer.transform, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE * 1.5f, DISPLAY_TILE * 0.6f);
        var tmp = obj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.raycastTarget = false;
    }

    void GenerateWeaponShopMapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 全て石壁で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_SHOP_WALL;
                walkable[x, y] = false;
            }
        }

        // 石畳の床エリア (x=1〜6, y=2〜8)
        for (int x = 1; x <= 6; x++)
        {
            for (int y = 2; y <= 8; y++)
            {
                mapData[x, y] = TILE_SHOP_FLOOR;
                walkable[x, y] = true;
            }
        }

        // 出口 (3,1)(4,1)
        mapData[3, 1] = TILE_FATHER_EXIT;
        walkable[3, 1] = false;
        mapData[4, 1] = TILE_FATHER_EXIT;
        walkable[4, 1] = false;

        // カウンター (2,5)(3,5)(4,5)
        walkable[2, 5] = false;
        walkable[3, 5] = false;
        walkable[4, 5] = false;
        // 右奥の机 (6, 7)(6, 8)
        walkable[6, 7] = false;
        walkable[6, 8] = false;
    }

    void GenerateJukuMapData()
    {
        // 12x14 の教室
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_JUKU_WALL;
                walkable[x, y] = false;
            }

        // 床エリア (x=1〜10, y=1〜12)
        for (int x = 1; x <= 10; x++)
            for (int y = 1; y <= 12; y++)
            {
                mapData[x, y] = TILE_JUKU_FLOOR;
                walkable[x, y] = true;
            }

        // 出口 (5,0)(6,0)
        mapData[5, 0] = TILE_FATHER_EXIT;
        walkable[5, 0] = false;
        mapData[6, 0] = TILE_FATHER_EXIT;
        walkable[6, 0] = false;

        // ===== 黒板 (4-7, 12) =====
        walkable[4, 12] = false;
        walkable[5, 12] = false;
        walkable[6, 12] = false;
        walkable[7, 12] = false;

        // ===== 教卓 (5, 10) =====
        walkable[5, 10] = false;
        // 先生NPC (6, 10) — 歩行可能

        // ===== 生徒用の机（各生徒の前 = y+1） =====
        // 後列 (y=5の生徒 → 机はy=6)
        walkable[3, 6] = false;
        walkable[7, 6] = false;
        walkable[9, 6] = false;
        // 前列 (y=8の生徒 → 机はy=9)
        walkable[3, 9] = false;
        walkable[7, 9] = false;

        // ===== 本棚 (1, 11)(1, 12)(10, 11)(10, 12) =====
        walkable[1, 11] = false;
        walkable[1, 12] = false;
        walkable[10, 11] = false;
        walkable[10, 12] = false;

        // ===== ロッカー (10, 1)(10, 2) =====
        walkable[10, 1] = false;
        walkable[10, 2] = false;
    }

    void CreateJukuFurniture()
    {
        if (tilesContainer == null) return;

        Color deskBrown = new Color(0.6f, 0.45f, 0.3f);
        Color darkWood = new Color(0.45f, 0.30f, 0.18f);

        // ===== 黒板 (4-7, 12) =====
        Sprite kokubanSprite = Resources.Load<Sprite>("Map/Kokuban");
        if (kokubanSprite != null)
            CreateFurnitureSprite(5.5f, 12f, DISPLAY_TILE * 4.5f, DISPLAY_TILE * 2.5f, kokubanSprite);
        else
            CreateFurnitureOverlay(5.5f, 12f, DISPLAY_TILE * 4.2f, DISPLAY_TILE * 0.95f,
                new Color(0.08f, 0.28f, 0.10f));

        // ===== 教卓 (5, 10) =====
        Sprite kyoudanSprite = Resources.Load<Sprite>("Map/Kyoudan");
        if (kyoudanSprite != null)
            CreateFurnitureSprite(5f, 10f, DISPLAY_TILE * 1.6f, DISPLAY_TILE * 1.6f, kyoudanSprite);
        else
            CreateFurnitureOverlay(5f, 10f, DISPLAY_TILE * 1.2f, DISPLAY_TILE * 0.75f, darkWood);

        // ===== 生徒の机 =====
        Sprite deskSprite = Resources.Load<Sprite>("Map/Study_Desk");
        if (deskSprite != null)
        {
            float deskW = DISPLAY_TILE * 0.8f;
            float deskH = DISPLAY_TILE * 0.8f;
            // 後列 (y=6): x=3, 7, 9
            CreateFurnitureSprite(3f, 6f, deskW, deskH, deskSprite);
            CreateFurnitureSprite(7f, 6f, deskW, deskH, deskSprite);
            CreateFurnitureSprite(9f, 6f, deskW, deskH, deskSprite);
            // 前列 (y=9): x=3, 7
            CreateFurnitureSprite(3f, 9f, deskW, deskH, deskSprite);
            CreateFurnitureSprite(7f, 9f, deskW, deskH, deskSprite);
        }
        else
        {
            // フォールバック: プロシージャル机
            CreateFurnitureOverlay(3f, 6f, DISPLAY_TILE * 1.1f, DISPLAY_TILE * 0.65f, deskBrown);
            CreateFurnitureOverlay(7f, 6f, DISPLAY_TILE * 1.1f, DISPLAY_TILE * 0.65f, deskBrown);
            CreateFurnitureOverlay(9f, 6f, DISPLAY_TILE * 1.1f, DISPLAY_TILE * 0.65f, deskBrown);
            CreateFurnitureOverlay(3f, 9f, DISPLAY_TILE * 1.1f, DISPLAY_TILE * 0.65f, deskBrown);
            CreateFurnitureOverlay(7f, 9f, DISPLAY_TILE * 1.1f, DISPLAY_TILE * 0.65f, deskBrown);
        }

        // ===== 本棚（左右壁際） =====
        CreateFurnitureOverlay(1f, 11.5f, DISPLAY_TILE * 0.85f, DISPLAY_TILE * 1.8f,
            new Color(0.5f, 0.32f, 0.18f));
        CreateFurnitureLabel(1f, 11.5f, "📚", 18, new Color(0.3f, 0.5f, 0.3f));
        CreateFurnitureOverlay(10f, 11.5f, DISPLAY_TILE * 0.85f, DISPLAY_TILE * 1.8f,
            new Color(0.5f, 0.32f, 0.18f));
        CreateFurnitureLabel(10f, 11.5f, "📚", 18, new Color(0.3f, 0.5f, 0.3f));

        // ===== ロッカー (10, 1-2) =====
        CreateFurnitureOverlay(10f, 1.5f, DISPLAY_TILE * 0.85f, DISPLAY_TILE * 1.8f,
            new Color(0.6f, 0.62f, 0.65f));
        CreateFurnitureLabel(10f, 1.5f, "🎒", 16, new Color(0.4f, 0.4f, 0.4f));

        // ===== 時計 =====
        CreateFurnitureLabel(9f, 12f, "🕐", 22, new Color(0.3f, 0.3f, 0.3f));

        // ===== 出口マーク =====
        CreateFurnitureLabel(5.5f, 0f, "▽ 出口", 16, new Color(0.3f, 0.6f, 0.3f));
    }

    void CreateWeaponShopFurniture()
    {
        if (tilesContainer == null) return;

        // カウンター (2〜4, 5) — Toy_Shop_Case で配置
        if (toyShopCaseSprite != null)
        {
            float posX = (3f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
            float posY = (5f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
            var caseObj = new GameObject("ToyShopCase");
            caseObj.transform.SetParent(tilesContainer.transform, false);
            var cRect = caseObj.AddComponent<RectTransform>();
            cRect.anchoredPosition = new Vector2(posX, posY);
            cRect.sizeDelta = new Vector2(DISPLAY_TILE * 4.5f, DISPLAY_TILE * 1.8f);
            var cImg = caseObj.AddComponent<Image>();
            cImg.sprite = toyShopCaseSprite;
            cImg.preserveAspect = true;
            cImg.raycastTarget = false;
        }
        else
        {
            CreateFurnitureOverlay(3f, 5f, DISPLAY_TILE * 3f, DISPLAY_TILE * 0.8f,
                new Color(0.35f, 0.2f, 0.45f));
        }

        // Toy_Shop_Desk — 右奥に配置
        CreateToyShopDeskOverlay(5.8f, 7.5f);

        // 出口マーク (3.5, 1) — ▽矢印
        CreateFurnitureLabel(3.5f, 1f, "▽ 出口", 18, new Color(0.8f, 0.7f, 1f));
    }

    void GenerateImpTownMapData()
    {
        // 12x40 の小悪魔の街（ステージ2と同サイズ）
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 基本は草で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_GRASS;
                walkable[x, y] = true;
            }
        }

        // === 外周（上下の柵のみ） ===
        for (int x = 0; x < mapWidth; x++)
        {
            mapData[x, 0] = TILE_TREE;
            walkable[x, 0] = false;
            mapData[x, mapHeight - 1] = TILE_TREE;
            walkable[x, mapHeight - 1] = false;
        }
        // 左右は壁なし（草のまま通行不可）
        for (int y = 1; y < mapHeight - 1; y++)
        {
            walkable[0, y] = false;
            walkable[mapWidth - 1, y] = false;
        }

        // === エリア出口（南端） ===
        mapData[5, 0] = TILE_AREA_EXIT;
        walkable[5, 0] = true;
        mapData[6, 0] = TILE_AREA_EXIT;
        walkable[6, 0] = true;

        // === メイン街道（ジグザグ） ===
        // 入口から北へ (y=1〜5, x=5〜6)
        for (int y = 1; y <= 5; y++)
        {
            mapData[5, y] = TILE_PATH;
            mapData[6, y] = TILE_PATH;
        }
        // 右に曲がる (y=5, x=6〜9)
        for (int x = 6; x <= 9; x++)
            mapData[x, 5] = TILE_PATH;
        // 北へ (y=5〜12, x=9)
        for (int y = 5; y <= 12; y++)
            mapData[9, y] = TILE_PATH;
        // 左へ (y=12, x=5〜9)
        for (int x = 5; x <= 9; x++)
            mapData[x, 12] = TILE_PATH;
        // 北へ (y=12〜19, x=5)
        for (int y = 12; y <= 19; y++)
            mapData[5, y] = TILE_PATH;
        // 右へ (y=19, x=5〜9)
        for (int x = 5; x <= 9; x++)
            mapData[x, 19] = TILE_PATH;
        // 北へ (y=19〜26, x=9)
        for (int y = 19; y <= 26; y++)
            mapData[9, y] = TILE_PATH;
        // 左へ (y=26, x=4〜9)
        for (int x = 4; x <= 9; x++)
            mapData[x, 26] = TILE_PATH;
        // 北へ (y=26〜32, x=4)
        for (int y = 26; y <= 32; y++)
            mapData[4, y] = TILE_PATH;
        // 右へ (y=32, x=4〜6)
        for (int x = 4; x <= 6; x++)
            mapData[x, 32] = TILE_PATH;
        // 北へ (y=32〜37, x=6)
        for (int y = 32; y <= 37; y++)
            mapData[6, y] = TILE_PATH;

        // === 毒沼（水場）===
        // 沼 (x=1-2, y=15-16)
        for (int x = 1; x <= 2; x++)
        {
            for (int y = 15; y <= 16; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 岩（woodtower） ===
        int[][] rocks = {
            new[]{1, 4}, new[]{10, 7}, new[]{10, 20}, new[]{1, 28}
        };
        foreach (var r in rocks)
        {
            mapData[r[0], r[1]] = TILE_ROCK;
            walkable[r[0], r[1]] = false;
        }

        // === 武器ショップ (2x2: x=3〜4, y=27〜28) ===
        for (int sx = 3; sx <= 4; sx++)
        {
            for (int sy = 27; sy <= 28; sy++)
            {
                mapData[sx, sy] = TILE_WEAPON_SHOP;
                walkable[sx, sy] = false;
            }
        }

        // === 109 館 (x=5〜7, y=38) ===
        for (int x = 5; x <= 7; x++)
        {
            mapData[x, 38] = TILE_BOSS_MANSION;
            walkable[x, 38] = false;
        }

        // === プール (x=1-4, y=34-36) — 左上エリア ===
        for (int px = 1; px <= 4; px++)
        {
            for (int py = 34; py <= 36; py++)
            {
                mapData[px, py] = TILE_WATER;
                walkable[px, py] = false;
            }
        }

        // === ブラッシュアップ ===

        // 沼の触手 — 道に向かって1タイル延伸
        mapData[3, 15] = TILE_WATER;
        walkable[3, 15] = false;

        // 秘密のショートカット — x=2, y=5〜25 を歩行可能に（タイルはそのまま＝隠しルート）
        for (int sy = 5; sy <= 25; sy++)
        {
            walkable[2, sy] = true;
        }

        // プレイヤーの初期位置は必ず歩けるようにする
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_PATH && mapData[playerTileX, playerTileY] != TILE_GRASS)
        {
            mapData[playerTileX, playerTileY] = TILE_PATH;
        }
    }

    void GenerateMansionMapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 全て壁で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_MANSION_WALL;
                walkable[x, y] = false;
            }
        }

        // === 床エリア ===
        // ボス部屋 (y=16〜18, x=2〜9)
        for (int x = 2; x <= 9; x++)
        {
            for (int y = 16; y <= 18; y++)
            {
                mapData[x, y] = TILE_MANSION_FLOOR;
                walkable[x, y] = true;
            }
        }

        // 中央通路 (y=15, x=5〜6) — 壁の中の通路
        mapData[5, 15] = TILE_MANSION_FLOOR;
        walkable[5, 15] = true;
        mapData[6, 15] = TILE_MANSION_FLOOR;
        walkable[6, 15] = true;

        // ボス扉 (y=14, x=5〜6)
        mapData[5, 14] = TILE_BOSS_DOOR;
        walkable[5, 14] = false; // TryMoveで特殊処理
        mapData[6, 14] = TILE_BOSS_DOOR;
        walkable[6, 14] = false;

        // 中央ホール (y=11〜13, x=2〜9)
        for (int x = 2; x <= 9; x++)
        {
            for (int y = 11; y <= 13; y++)
            {
                mapData[x, y] = TILE_MANSION_FLOOR;
                walkable[x, y] = true;
            }
        }

        // おともだち (3,10) と (8,10)
        mapData[3, 10] = TILE_MERCENARY_A;
        walkable[3, 10] = false; // TryMoveで特殊処理
        mapData[8, 10] = TILE_MERCENARY_B;
        walkable[8, 10] = false;

        // y=10: おともだちの行の床
        for (int x = 2; x <= 9; x++)
        {
            if (x != 3 && x != 8)
            {
                mapData[x, 10] = TILE_MANSION_FLOOR;
                walkable[x, 10] = true;
            }
        }

        // 入口エリア (y=2〜9, x=2〜9)
        for (int x = 2; x <= 9; x++)
        {
            for (int y = 2; y <= 9; y++)
            {
                mapData[x, y] = TILE_MANSION_FLOOR;
                walkable[x, y] = true;
            }
        }

        // y=1: 壁 + 中央通路 (x=5〜6)
        mapData[5, 1] = TILE_MANSION_FLOOR;
        walkable[5, 1] = true;
        mapData[6, 1] = TILE_MANSION_FLOOR;
        walkable[6, 1] = true;

        // y=0: 出口 (x=5〜6)
        mapData[5, 0] = TILE_MANSION_EXIT;
        walkable[5, 0] = false; // TryMoveで特殊処理
        mapData[6, 0] = TILE_MANSION_EXIT;
        walkable[6, 0] = false;

        // あそび済みおともだちのタイルを床に変更
        if (DataCarrier.Instance != null)
        {
            if (DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵A"))
            {
                mapData[3, 10] = TILE_MANSION_FLOOR;
                walkable[3, 10] = true;
            }
            if (DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵B"))
            {
                mapData[8, 10] = TILE_MANSION_FLOOR;
                walkable[8, 10] = true;
            }
        }

        // プレイヤー初期位置は歩けるようにする
        walkable[playerTileX, playerTileY] = true;
    }

    void Generate109MapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 全て壁で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_MANSION_WALL;
                walkable[x, y] = false;
            }
        }

        // === 床エリア ===
        // ボス部屋 (y=14〜18, x=2〜9)
        for (int x = 2; x <= 9; x++)
        {
            for (int y = 14; y <= 18; y++)
            {
                mapData[x, y] = TILE_MANSION_FLOOR;
                walkable[x, y] = true;
            }
        }

        // 中央通路 (y=13, x=5〜6) — 壁の中の通路
        mapData[5, 13] = TILE_MANSION_FLOOR;
        walkable[5, 13] = true;
        mapData[6, 13] = TILE_MANSION_FLOOR;
        walkable[6, 13] = true;

        // ボス扉 (y=12, x=5〜6) — 直接開く（おともだちチェック不要）
        mapData[5, 12] = TILE_BOSS_DOOR;
        walkable[5, 12] = false;
        mapData[6, 12] = TILE_BOSS_DOOR;
        walkable[6, 12] = false;

        // メインホール (y=2〜11, x=2〜9)
        for (int x = 2; x <= 9; x++)
        {
            for (int y = 2; y <= 11; y++)
            {
                mapData[x, y] = TILE_MANSION_FLOOR;
                walkable[x, y] = true;
            }
        }

        // y=1: 壁 + 中央通路 (x=5〜6)
        mapData[5, 1] = TILE_MANSION_FLOOR;
        walkable[5, 1] = true;
        mapData[6, 1] = TILE_MANSION_FLOOR;
        walkable[6, 1] = true;

        // y=0: 出口 (x=5〜6)
        mapData[5, 0] = TILE_MANSION_EXIT;
        walkable[5, 0] = false;
        mapData[6, 0] = TILE_MANSION_EXIT;
        walkable[6, 0] = false;

        // プレイヤー初期位置は歩けるようにする
        walkable[playerTileX, playerTileY] = true;
    }

    void CreateMansionNPCs()
    {
        if (tilesContainer == null || DataCarrier.Instance == null) return;

        // おともだちA (3, 10)
        if (!DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵A"))
        {
            CreateMercenaryOverlay(3, 10, "A");
        }
        // おともだちB (8, 10)
        if (!DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵B"))
        {
            CreateMercenaryOverlay(8, 10, "B");
        }
    }

    void CreateMercenaryOverlay(int tx, int ty, string label)
    {
        float posX = (tx - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (ty - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var npc = new GameObject("Mercenary" + label);
        npc.transform.SetParent(tilesContainer.transform, false);
        var npcRect = npc.AddComponent<RectTransform>();
        npcRect.anchoredPosition = new Vector2(posX, posY);
        npcRect.sizeDelta = new Vector2(DISPLAY_TILE - 4, DISPLAY_TILE - 4);

        var npcImg = npc.AddComponent<Image>();
        npcImg.color = new Color(0.4f, 0.1f, 0.1f);
        npcImg.raycastTarget = false;

        var textObj = new GameObject("MercLabel");
        textObj.transform.SetParent(npc.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(tmp);
        tmp.text = "\u5175"; // 兵
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
    }

    // ===== ステラ・オリジン（Area 8）=====

    void GenerateStellaOriginMapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 全て宇宙壁で埋める（床なし＝進入不可）
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_CRYSTAL_WALL;
                walkable[x, y] = false;
            }

        // --- ヘルパー: タイル配置 ---
        System.Action<int, int> placeCrystal = (px, py) =>
        {
            if (px >= 0 && px < mapWidth && py >= 0 && py < mapHeight)
            {
                mapData[px, py] = TILE_CRYSTAL;
                walkable[px, py] = true;
            }
        };
        // 3x3広場
        System.Action<int, int> placePlaza = (cx, cy) =>
        {
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    placeCrystal(cx + dx, cy + dy);
        };

        // ============================================================
        // 迷路のような一本道 — 蛇行しながら南から北へ
        // プレイヤーが「光の道」を切り拓いていく感覚
        // ============================================================

        // --- 南の着地広場: 3x3 (center: 12,3) ---
        placePlaza(12, 3);

        // --- Phase 1: 南から蛇行して西の音叉へ ---
        // 着地点から北へ (12, 4→7)
        for (int y = 4; y <= 7; y++) placeCrystal(12, y);
        // 西へ折れる (11→9, 7)
        for (int x = 9; x <= 11; x++) placeCrystal(x, 7);
        // 南に戻る (9, 6→5)
        placeCrystal(9, 6); placeCrystal(9, 5);
        // 西の音叉広場: 3x3 around (8,5)
        placePlaza(8, 5);

        // --- Phase 2: 東の音叉へ ---
        // 着地点から東へ (13→15, 3)
        for (int x = 13; x <= 15; x++) placeCrystal(x, 3);
        // 北へ (15, 4→5)
        placeCrystal(15, 4);
        // 東の音叉広場: 3x3 around (15,5)
        placePlaza(15, 5);

        // --- Phase 3: メインの蛇行路（北上） ---
        // (12,7) から北上 → 東へ蛇行 → 西へ蛇行を繰り返す
        // 北へ (12, 8→11)
        for (int y = 8; y <= 11; y++) placeCrystal(12, y);
        // 東へ折れる (13→16, 11)
        for (int x = 13; x <= 16; x++) placeCrystal(x, 11);
        // 北へ (16, 12→14)
        for (int y = 12; y <= 14; y++) placeCrystal(16, y);
        // 西へ折れる (15→8, 14)
        for (int x = 8; x <= 15; x++) placeCrystal(x, 14);
        // 北へ (8, 15→17)
        for (int y = 15; y <= 17; y++) placeCrystal(8, y);
        // 東へ折れる (9→11, 17)
        for (int x = 9; x <= 11; x++) placeCrystal(x, 17);

        // --- 中央交差点: 3x3 (center: 12,19) ---
        // (11,17) から北へ (11, 18→19)
        placeCrystal(11, 18); placeCrystal(11, 19);
        placePlaza(12, 19);

        // --- Phase 4: 西のガーディアンへの道 ---
        // 中央から西へ (10→6, 19) — x=5を欠落させて直接到達不可
        for (int x = 6; x <= 10; x++) placeCrystal(x, 19);
        // ガーディアン広場: 3x3 around (3,19)
        placePlaza(3, 19);
        placeCrystal(4, 19);

        // --- Phase 5: 東のガーディアンへの道 ---
        // 中央から東へ (14→19, 19) — 広場との間にギャップ
        for (int x = 14; x <= 19; x++) placeCrystal(x, 19);
        // ガーディアン広場: 3x3 around (21,19)
        placePlaza(21, 19);
        // (20,19)を通行不可にしてギャップを作る（広場の端を削る）
        mapData[20, 19] = TILE_CRYSTAL_WALL;
        walkable[20, 19] = false;

        // --- Phase 6: 北への蛇行路 ---
        // 中央から北へ (12, 20→22)
        for (int y = 20; y <= 22; y++) placeCrystal(12, y);
        // 西へ折れる (11→7, 22)
        for (int x = 7; x <= 11; x++) placeCrystal(x, 22);
        // 北へ (7, 23→25)
        for (int y = 23; y <= 25; y++) placeCrystal(7, y);
        // 東へ折れる (8→16, 25)
        for (int x = 8; x <= 16; x++) placeCrystal(x, 25);
        // 北へ (16, 26→28)
        for (int y = 26; y <= 28; y++) placeCrystal(16, y);
        // 西へ折れる (15→12, 28)
        for (int x = 12; x <= 15; x++) placeCrystal(x, 28);
        // 北へ (12, 29→33)
        for (int y = 29; y <= 33; y++) placeCrystal(12, y);
        // 一本道の最上(y=33)の左右にタイル追加
        placeCrystal(11, 33);
        placeCrystal(13, 33);

        // 北へ続く (12, 34)
        placeCrystal(12, 34);

        // --- 北のゆりかご広場: 3x3 around (12,35) ---
        placePlaza(12, 35);
        // y=36を壁に（ゲートとの隙間 — 直接歩けないようにする）
        for (int rx = 11; rx <= 13; rx++)
        {
            mapData[rx, 36] = TILE_CRYSTAL_WALL;
            walkable[rx, 36] = false;
        }

        // --- 天の川の橋 (y=36): 条件達成時にwalkable化 ---

        // === 宇宙の門 (y=37): 通行不可 ===
        for (int x = 11; x <= 13; x++)
        {
            mapData[x, 37] = TILE_CRYSTAL_WALL;
            walkable[x, 37] = false;
        }

        // プレイヤー初期位置を確保
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] == TILE_CRYSTAL_WALL)
            mapData[playerTileX, playerTileY] = TILE_CRYSTAL;
    }

    void CreateStellaOriginBackground()
    {
        if (tilesContainer == null) return;

        float mapW = mapWidth * DISPLAY_TILE + 200;
        float mapH = mapHeight * DISPLAY_TILE + 200;

        // Layer 0: 一枚絵の宇宙背景（4th/background）— ゆっくり回転
        var cosmicBg = new GameObject("CosmicBg");
        cosmicBg.transform.SetParent(tilesContainer.transform, false);
        var bgRect = cosmicBg.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.anchoredPosition = Vector2.zero;
        var bgImg = cosmicBg.AddComponent<Image>();

        Sprite stellaBgSpr = Resources.Load<Sprite>("Map/4th/background");
        if (stellaBgSpr == null)
        {
            var tex = Resources.Load<Texture2D>("Map/4th/background");
            if (tex != null && tex.isReadable)
                stellaBgSpr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        if (stellaBgSpr != null)
        {
            bgImg.sprite = stellaBgSpr;
            bgImg.preserveAspect = false;
            float imgAspect = (float)stellaBgSpr.texture.width / stellaBgSpr.texture.height;
            float mapAspect = mapW / mapH;
            float bgW, bgH;
            if (mapAspect > imgAspect)
            {
                bgW = mapW;
                bgH = mapW / imgAspect;
            }
            else
            {
                bgH = mapH;
                bgW = mapH * imgAspect;
            }
            // 回転時に角が見えないよう1.5倍に拡大
            bgRect.sizeDelta = new Vector2(bgW * 1.5f, bgH * 1.5f);
            bgImg.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        else
        {
            bgRect.sizeDelta = new Vector2(mapW * 1.5f, mapH * 1.5f);
            bgImg.color = new Color(0.03f, 0.02f, 0.08f);
        }
        bgImg.raycastTarget = false;
        cosmicBg.transform.SetAsFirstSibling();
        stellaBgRect = bgRect; // Update()で回転させる
        stellaBgImg = bgImg;   // 虹色明滅用
        stellaBgBasePos = Vector2.zero; // パララックス基準

        // Layer 0.5: 40%黒オーバーレイ（重厚感）
        var darkOverlay = new GameObject("DarkOverlay");
        darkOverlay.transform.SetParent(tilesContainer.transform, false);
        var darkRect = darkOverlay.AddComponent<RectTransform>();
        darkRect.anchorMin = new Vector2(0.5f, 0.5f);
        darkRect.anchorMax = new Vector2(0.5f, 0.5f);
        darkRect.anchoredPosition = Vector2.zero;
        darkRect.sizeDelta = bgRect.sizeDelta;
        var darkImg = darkOverlay.AddComponent<Image>();
        darkImg.color = new Color(0f, 0f, 0f, 0.4f);
        darkImg.raycastTarget = false;
        darkOverlay.transform.SetSiblingIndex(1);
        stellaDarkOverlayRect = darkRect;
        stellaDarkBasePos = Vector2.zero;

        // Layer 0.3: グリッターパーティクル（背景とオーバーレイの間）
        CreateStellaGlitters(bgRect.sizeDelta);
    }

    void CreateStellaGlitters(Vector2 areaSize)
    {
        int count = 60;
        float halfW = areaSize.x * 0.4f;
        float halfH = areaSize.y * 0.4f;

        for (int i = 0; i < count; i++)
        {
            var obj = new GameObject($"Glitter_{i}");
            obj.transform.SetParent(tilesContainer.transform, false);
            var rect = obj.AddComponent<RectTransform>();
            float size = Random.Range(2f, 6f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(
                Random.Range(-halfW, halfW),
                Random.Range(-halfH, halfH));
            var img = obj.AddComponent<Image>();
            img.raycastTarget = false;
            // パステル系の白〜青〜ピンクからランダム
            float hue = Random.Range(0f, 1f);
            img.color = Color.HSVToRGB(hue, 0.15f, 1f) * new Color(1, 1, 1, 0f);
            obj.transform.SetSiblingIndex(1); // 背景の直上

            stellaGlitters.Add((rect, img,
                Random.Range(0f, Mathf.PI * 2f),
                Random.Range(0.3f, 1.2f)));
        }
    }

    void CreateNebula(Vector2 pos, Color color, float size)
    {
        if (tilesContainer == null) return;
        var nebula = new GameObject("Nebula");
        nebula.transform.SetParent(tilesContainer.transform, false);
        var nRect = nebula.AddComponent<RectTransform>();
        nRect.anchoredPosition = pos;
        nRect.sizeDelta = new Vector2(size, size);
        var nImg = nebula.AddComponent<Image>();
        nImg.color = color;
        nImg.raycastTarget = false;
        // 丸みを帯びた星雲: 角丸を使用
        // UnityのImageに丸はないが、CanvasRendererなので大きな角丸で代用
    }

    void CreateConstellationSilhouette(Vector2 pos, string symbol, string name)
    {
        if (tilesContainer == null) return;

        // 星座の「星」を複数配置して線で繋ぐイメージ
        var group = new GameObject("Constellation_" + name);
        group.transform.SetParent(tilesContainer.transform, false);
        var gRect = group.AddComponent<RectTransform>();
        gRect.anchoredPosition = pos;
        gRect.sizeDelta = new Vector2(200, 200);

        // 中央シンボル（大きめ、半透明）
        var symbolObj = new GameObject("Symbol");
        symbolObj.transform.SetParent(group.transform, false);
        var symRect = symbolObj.AddComponent<RectTransform>();
        symRect.anchoredPosition = Vector2.zero;
        symRect.sizeDelta = new Vector2(120, 120);
        var symTmp = symbolObj.AddComponent<TMPro.TextMeshProUGUI>();
        FontHelper.Apply(symTmp);
        symTmp.text = symbol;
        symTmp.fontSize = 60;
        symTmp.alignment = TMPro.TextAlignmentOptions.Center;
        symTmp.color = new Color(0.5f, 0.5f, 0.7f, 0.15f);
        symTmp.raycastTarget = false;

        // 周囲に小さな星（星座の点）
        int points = 6;
        for (int i = 0; i < points; i++)
        {
            float angle = i * Mathf.PI * 2f / points;
            float radius = Random.Range(50f, 90f);
            var point = new GameObject("Point" + i);
            point.transform.SetParent(group.transform, false);
            var pRect = point.AddComponent<RectTransform>();
            pRect.anchoredPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            pRect.sizeDelta = new Vector2(4, 4);
            var pImg = point.AddComponent<Image>();
            pImg.color = new Color(0.6f, 0.6f, 0.8f, 0.3f);
            pImg.raycastTarget = false;
        }
    }

    void CreateStellaOriginEffects()
    {
        if (tilesContainer == null) return;

        // Aura_Sprite.png をロード + 加算合成マテリアル生成
        stellaAuraSprite = Resources.Load<Sprite>("Map/4th/Aura_Sprite");
        if (stellaAuraSprite == null)
        {
            var auraTex = Resources.Load<Texture2D>("Map/4th/Aura_Sprite");
            if (auraTex != null && auraTex.isReadable)
                stellaAuraSprite = Sprite.Create(auraTex, new Rect(0, 0, auraTex.width, auraTex.height), new Vector2(0.5f, 0.5f));
        }
        // 加算合成マテリアル（UI用）
        var addShader = Shader.Find("UI/Default");
        if (addShader != null)
        {
            stellaAdditiveMat = new Material(addShader);
            // SrcAlpha One で加算合成
            stellaAdditiveMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            stellaAdditiveMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
        }

        // 光子パーティクル（20個）— ゆっくり漂う発光体（エフェクトレイヤー）
        for (int i = 0; i < 20; i++)
        {
            var photon = new GameObject("Photon" + i);
            photon.transform.SetParent(tilesContainer.transform, false);
            var phRect = photon.AddComponent<RectTransform>();
            float phx = Random.Range(-mapWidth * DISPLAY_TILE * 0.45f, mapWidth * DISPLAY_TILE * 0.45f);
            float phy = Random.Range(-mapHeight * DISPLAY_TILE * 0.45f, mapHeight * DISPLAY_TILE * 0.45f);
            phRect.anchoredPosition = new Vector2(phx, phy);
            float phSize = Random.Range(6f, 14f);
            phRect.sizeDelta = new Vector2(phSize, phSize);
            var phImg = photon.AddComponent<Image>();
            float baseAlpha = Random.Range(0.15f, 0.4f);
            // 暖色〜寒色のランダム光子
            float hue = Random.Range(0f, 1f);
            Color phColor;
            if (hue < 0.3f)
                phColor = new Color(1f, 0.84f, 0.5f, baseAlpha); // ゴールド
            else if (hue < 0.6f)
                phColor = new Color(0.6f, 0.8f, 1f, baseAlpha); // 水色
            else
                phColor = new Color(0.8f, 0.6f, 1f, baseAlpha); // 薄紫
            phImg.color = phColor;
            phImg.raycastTarget = false;
            stellaPhotons.Add((phRect, Random.Range(0.2f, 0.8f), Random.Range(0f, Mathf.PI * 2f), baseAlpha));
        }

        // 足元波紋・トレイル削除済み
    }

    // --- 音叉ビジュアル＋ワープ ---
    void CreateStellaTuningForks()
    {
        if (tilesContainer == null) return;

        Sprite forkSpr = Resources.Load<Sprite>("Map/4th/Tuning_Fork");
        if (forkSpr == null)
        {
            var forkTex = Resources.Load<Texture2D>("Map/4th/Tuning_Fork");
            if (forkTex != null && forkTex.isReadable)
                forkSpr = Sprite.Create(forkTex, new Rect(0, 0, forkTex.width, forkTex.height), new Vector2(0.5f, 0.5f));
        }
        if (forkSpr == null) return;

        var forkPositions = new[] { stellaForkA, stellaForkB, stellaForkReturnW, stellaForkReturnE };
        for (int i = 0; i < forkPositions.Length; i++)
        {
            int tx = forkPositions[i].x, ty = forkPositions[i].y;
            float posX = (tx - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
            float posY = (ty - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

            bool isReturn = (i >= 2); // 後半2つは帰還用（小さめ）
            float forkScale = isReturn ? 1.0f : 1.6f;

            var fork = new GameObject("TuningFork_" + tx + "_" + ty);
            fork.transform.SetParent(tilesContainer.transform, false);
            var rt = fork.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(posX, posY + DISPLAY_TILE * 0.15f);
            rt.sizeDelta = new Vector2(DISPLAY_TILE * forkScale, DISPLAY_TILE * forkScale);

            var img = fork.AddComponent<Image>();
            img.sprite = forkSpr;
            img.preserveAspect = true;
            img.color = new Color(1f, 1f, 1f, 0.9f);
            img.raycastTarget = false;
            if (stellaAdditiveMat != null)
                img.material = stellaAdditiveMat;

            stellaForkVisuals.Add((rt, img, posY + DISPLAY_TILE * 0.15f, Random.Range(0f, Mathf.PI * 2f)));

            // 音叉の足元にAura_Gold（小さめ）
            if (!isReturn)
            {
                Sprite goldSpr = Resources.Load<Sprite>("Map/4th/Aura_Gold");
                if (goldSpr == null)
                {
                    var goldTex = Resources.Load<Texture2D>("Map/4th/Aura_Gold");
                    if (goldTex != null && goldTex.isReadable)
                        goldSpr = Sprite.Create(goldTex, new Rect(0, 0, goldTex.width, goldTex.height), new Vector2(0.5f, 0.5f));
                }
                if (goldSpr != null)
                {
                    var goldObj = new GameObject("ForkAuraGold_" + tx + "_" + ty);
                    goldObj.transform.SetParent(tilesContainer.transform, false);
                    var grt = goldObj.AddComponent<RectTransform>();
                    grt.anchoredPosition = new Vector2(posX, posY - DISPLAY_TILE * 0.1f);
                    grt.sizeDelta = new Vector2(DISPLAY_TILE * 0.8f, DISPLAY_TILE * 0.8f);
                    var gimg = goldObj.AddComponent<Image>();
                    gimg.sprite = goldSpr;
                    gimg.preserveAspect = true;
                    gimg.color = new Color(1f, 1f, 1f, 0.5f);
                    gimg.raycastTarget = false;
                    if (stellaAdditiveMat != null)
                        gimg.material = stellaAdditiveMat;
                }
            }
        }
    }

    // --- ガーディアンNPC ---

    void CreateStellaGuardianNPCs()
    {
        if (tilesContainer == null) return;

        // 西: 清らかな夫人（デヴィル夫人）— 紫オーラ
        stellaGuardianDevilObj = CreateStellaGuardianObj(
            "GuardianDevil", stellaGuardianDevilX, stellaGuardianDevilY,
            "EnemyBabys/boss/devil-wife",
            "Map/4th/Aura_Purple",
            () => StartCoroutine(ShowStellaGuardianDialogue(false)));

        // 東: 穏やかな女王（メロディアス女王）— ピンクオーラ
        stellaGuardianMelodiasObj = CreateStellaGuardianObj(
            "GuardianMelodias", stellaGuardianMelodiasX, stellaGuardianMelodiasY,
            "EnemyBabys/boss/melodias",
            "Map/4th/Aura _Pink",
            () => StartCoroutine(ShowStellaGuardianDialogue(true)));
    }

    GameObject CreateStellaGuardianObj(string name, int tx, int ty, string spritePath, string auraSpritePath, UnityEngine.Events.UnityAction onClick)
    {
        float posX = (tx - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (ty - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var npc = new GameObject(name);
        npc.transform.SetParent(tilesContainer.transform, false);
        var rect = npc.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE * 1.5f, DISPLAY_TILE * 1.5f);

        // オーラ（Aura画像スプライト + 加算合成 + 回転）
        var aura = new GameObject("Aura");
        aura.transform.SetParent(npc.transform, false);
        var auraRect = aura.AddComponent<RectTransform>();
        auraRect.anchoredPosition = Vector2.zero;
        auraRect.sizeDelta = new Vector2(DISPLAY_TILE * 3f, DISPLAY_TILE * 3f);
        var auraImg = aura.AddComponent<Image>();
        Sprite auraSpr = Resources.Load<Sprite>(auraSpritePath);
        if (auraSpr == null)
        {
            var auraTex = Resources.Load<Texture2D>(auraSpritePath);
            if (auraTex != null && auraTex.isReadable)
                auraSpr = Sprite.Create(auraTex, new Rect(0, 0, auraTex.width, auraTex.height), new Vector2(0.5f, 0.5f));
        }
        if (auraSpr != null)
        {
            auraImg.sprite = auraSpr;
            auraImg.preserveAspect = true;
            auraImg.color = new Color(1f, 1f, 1f, 0.7f);
            // 加算合成で背景に溶け込む
            if (stellaAdditiveMat != null)
                auraImg.material = stellaAdditiveMat;
        }
        else
        {
            auraImg.color = Color.clear;
        }
        auraImg.raycastTarget = false;

        // スプライト
        var spr = Resources.Load<Sprite>(spritePath);
        var sprObj = new GameObject("Sprite");
        sprObj.transform.SetParent(npc.transform, false);
        var sprRect = sprObj.AddComponent<RectTransform>();
        sprRect.anchoredPosition = Vector2.zero;
        sprRect.sizeDelta = new Vector2(DISPLAY_TILE * 1.2f, DISPLAY_TILE * 1.2f);
        var sprImg = sprObj.AddComponent<Image>();
        if (spr != null) sprImg.sprite = spr;
        sprImg.preserveAspect = true;
        sprImg.color = new Color(1f, 1f, 1f, 0.85f);
        sprImg.raycastTarget = false;

        // タップ用ボタン
        var btnImg = npc.AddComponent<Image>();
        btnImg.color = Color.clear;
        var btn = npc.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(onClick);

        npc.transform.SetAsLastSibling();
        return npc;
    }

    IEnumerator ShowStellaGuardianDialogue(bool isMelodias)
    {
        if (stellaGuardianDialogueActive) yield break;
        stellaGuardianDialogueActive = true;
        menuOpen = true;

        int flag = isMelodias ? 2 : 1; // bit 1=DEF, bit 2=ATK
        bool alreadyDone = DataCarrier.Instance != null && (DataCarrier.Instance.stellaOriginProgress & flag) != 0;

        string npcName = isMelodias
            ? Localization.Get("stella_guardian_melodias_name")
            : Localization.Get("stella_guardian_devil_name");

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.6f * (elapsed / 0.3f));
            yield return null;
        }

        // NPC画像
        string spritePath = isMelodias ? "EnemyBabys/boss/melodias" : "EnemyBabys/boss/devil-wife";
        var portraitSpr = Resources.Load<Sprite>(spritePath);
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        if (portraitSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(portraitSpr);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel(npcName, "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        if (alreadyDone)
        {
            textLabel.text = isMelodias
                ? Localization.Get("stella_guardian_melodias_done")
                : Localization.Get("stella_guardian_devil_done");
            tapHint.text = "\u25BC タップで閉じる";
            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }
        else
        {
            // メイン台詞
            textLabel.text = isMelodias
                ? Localization.Get("stella_guardian_melodias_line")
                : Localization.Get("stella_guardian_devil_line");
            tapHint.text = "\u25BC タップで続く";
            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            // バフ付与
            if (DataCarrier.Instance != null)
            {
                if (isMelodias)
                    DataCarrier.Instance.babyAtk += 20;
                else
                    DataCarrier.Instance.babyDef += 20;

                DataCarrier.Instance.stellaOriginProgress |= flag;
                DataCarrier.Instance.SaveData();
            }

            textLabel.text = isMelodias
                ? Localization.Get("stella_guardian_melodias_buff")
                : Localization.Get("stella_guardian_devil_buff");
            tapHint.text = "\u25BC タップで閉じる";
            tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            // 門の状態を更新
            UpdateCosmicGateVisual();
        }

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.opacity = 1f - (elapsed / 0.3f);
            yield return null;
        }
        overlay.RemoveFromHierarchy();

        menuOpen = false;
        stellaGuardianDialogueActive = false;

        // 全条件が揃ったらゲート開放演出を自動発動
        CheckCosmicGateAutoOpen();
    }

    void CheckStellaGuardianAutoTrigger()
    {
        if (menuOpen || stellaGuardianDialogueActive || stellaCradleActive) return;
        // デヴィル夫人（西）
        if (IsAdjacentToPos(stellaGuardianDevilX, stellaGuardianDevilY))
        {
            StartCoroutine(ShowStellaGuardianDialogue(false));
            return;
        }
        // メロディアス女王（東）
        if (IsAdjacentToPos(stellaGuardianMelodiasX, stellaGuardianMelodiasY))
        {
            StartCoroutine(ShowStellaGuardianDialogue(true));
            return;
        }
        // 光のゆりかご
        if (IsAdjacentToPos(stellaCradleX, stellaCradleY) ||
            (playerTileX == stellaCradleX && playerTileY == stellaCradleY))
        {
            if (!stellaCradleActive)
            {
                StartCoroutine(ShowStellaCradleEvent());
                return;
            }
        }
    }

    void CheckCosmicGateAutoOpen()
    {
        if (stellaGateOpened || stellaGateOpenAnimPlaying) return;
        int progress = DataCarrier.Instance != null ? DataCarrier.Instance.stellaOriginProgress : 0;
        if ((progress & 7) == 7)
            StartCoroutine(ShowCosmicGateOpenFromDistance());
    }

    IEnumerator ShowCosmicGateOpenFromDistance()
    {
        // 離れた場所からゲートが開く演出（短縮版）
        stellaGateOpenAnimPlaying = true;
        menuOpen = true;

        // 画面揺れ（軽め）
        if (seSource != null && seQuizCorrect != null)
        {
            seSource.pitch = 0.3f;
            seSource.PlayOneShot(seQuizCorrect, 0.8f);
        }

        // メッセージ表示
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        overlayRoot.Add(overlay);

        var label = UIHelper.CreateLabel("……遠くで 何かが 動く音がした……", "map-message-text");
        label.style.color = new Color(0.8f, 0.85f, 1f);
        label.style.fontSize = 30;
        label.style.whiteSpace = UIE.WhiteSpace.Normal;
        label.style.width = 800;
        label.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        UIHelper.ApplyFont(label);
        overlay.Add(label);

        yield return new WaitForSeconds(1.5f);

        // walkable更新 + ビジュアル更新
        UpdateCosmicGateVisual();

        // ゲートオブジェクトを非アクティブに
        if (stellaCosmicGateObj != null)
            stellaCosmicGateObj.SetActive(false);

        if (seSource != null) seSource.pitch = 1f;
        if (seSource != null && seQuizCorrect != null)
            seSource.PlayOneShot(seQuizCorrect, 0.6f);

        yield return new WaitForSeconds(0.5f);

        // フェードアウト
        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            overlay.style.opacity = 1f - (elapsed / 0.4f);
            yield return null;
        }
        overlay.RemoveFromHierarchy();

        menuOpen = false;
        stellaGateOpenAnimPlaying = false;
    }

    // --- 光のゆりかご ---

    List<GameObject> lightRiverObjs = new List<GameObject>();

    void CreateLightRiverBridge()
    {
        if (tilesContainer == null) return;
        if (lightRiverObjs.Count > 0) return; // 二重生成防止

        // River_of_Light.png をロード
        Sprite riverSpr = Resources.Load<Sprite>("Map/4th/River_of_Light");
        if (riverSpr == null)
        {
            var riverTex = Resources.Load<Texture2D>("Map/4th/River_of_Light");
            if (riverTex != null && riverTex.isReadable)
                riverSpr = Sprite.Create(riverTex, new Rect(0, 0, riverTex.width, riverTex.height), new Vector2(0.5f, 0.5f));
        }

        float centerX = (12 - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float bandWidth = DISPLAY_TILE * 4.8f;   // 横1.5倍
        float bandHeight = DISPLAY_TILE * 0.85f;

        float posY = (36.0f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var riverObj = new GameObject("LightRiver");
        riverObj.transform.SetParent(tilesContainer.transform, false);
        var rect = riverObj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(centerX, posY);
        rect.sizeDelta = new Vector2(bandWidth, bandHeight);

        var img = riverObj.AddComponent<Image>();
        img.raycastTarget = false;
        if (riverSpr != null)
        {
            img.sprite = riverSpr;
            img.color = new Color(1f, 1f, 1f, 0.8f);
        }
        else
        {
            img.color = new Color(0.7f, 0.85f, 1f, 0.35f);
        }

        // プレイヤーより後ろに配置
        if (playerObj != null)
            riverObj.transform.SetSiblingIndex(playerObj.transform.GetSiblingIndex());

        lightRiverObjs.Add(riverObj);
    }

    void CreateStellaCradle()
    {
        if (tilesContainer == null) return;

        float posX = (stellaCradleX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (stellaCradleY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        stellaCradleObj = new GameObject("CrystalCradle");
        stellaCradleObj.transform.SetParent(tilesContainer.transform, false);
        var rect = stellaCradleObj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE * 1.8f, DISPLAY_TILE * 1.8f);

        // 光のオーラ（背景色なし）
        var glow = new GameObject("Glow");
        glow.transform.SetParent(stellaCradleObj.transform, false);
        var glowRect = glow.AddComponent<RectTransform>();
        glowRect.anchoredPosition = Vector2.zero;
        glowRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.5f, DISPLAY_TILE * 2.5f);
        var glowImg = glow.AddComponent<Image>();
        glowImg.color = Color.clear;
        glowImg.raycastTarget = false;

        // ゆりかご画像
        var symbol = new GameObject("Symbol");
        symbol.transform.SetParent(stellaCradleObj.transform, false);
        var symRect = symbol.AddComponent<RectTransform>();
        symRect.anchoredPosition = Vector2.zero;
        symRect.sizeDelta = new Vector2(DISPLAY_TILE * 2f, DISPLAY_TILE * 2f);
        var symRaw = symbol.AddComponent<RawImage>();
        var cradleTex = Resources.Load<Texture2D>("Map/4th/cradle_of_life");
        if (cradleTex != null)
            symRaw.texture = cradleTex;
        symRaw.raycastTarget = false;

        // 完了済みなら見た目を変える（グローは既にクリアなので不要）

        // タップ
        var btnImg = stellaCradleObj.AddComponent<Image>();
        btnImg.color = Color.clear;
        var btn = stellaCradleObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => {
            if (!stellaCradleActive) StartCoroutine(ShowStellaCradleEvent());
        });

        stellaCradleObj.transform.SetAsLastSibling();
    }

    IEnumerator ShowStellaCradleEvent()
    {
        stellaCradleActive = true;
        menuOpen = true;

        int progress = DataCarrier.Instance != null ? DataCarrier.Instance.stellaOriginProgress : 0;
        bool done = (progress & 4) != 0;
        bool bossesVisited = (progress & 3) == 3; // bit1=DEF, bit2=ATK 両方必要

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.7f * (elapsed / 0.3f));
            yield return null;
        }

        // ゆりかご画像を表示
        var cradleImgEl = new UIE.VisualElement();
        cradleImgEl.pickingMode = UIE.PickingMode.Ignore;
        var cradleTex2 = Resources.Load<Texture2D>("Map/4th/cradle_of_life");
        if (cradleTex2 != null)
            cradleImgEl.style.backgroundImage = new UIE.StyleBackground(cradleTex2);
        cradleImgEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        cradleImgEl.style.width = 300;
        cradleImgEl.style.height = 300;
        cradleImgEl.style.alignSelf = UIE.Align.Center;
        cradleImgEl.style.marginBottom = 20;
        overlay.Add(cradleImgEl);

        if (!bossesVisited && !done)
        {
            // 両ボス未訪問: ゆりかごはまだ使えない
            var lockedLabel = UIHelper.CreateLabel(
                "ゆりかごは まだ 目覚めていない…\nふたりの 守護者に 会いに行こう", "map-message-text");
            lockedLabel.style.color = new Color(0.7f, 0.7f, 0.9f);
            lockedLabel.style.fontSize = 28;
            lockedLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
            lockedLabel.style.width = 800;
            lockedLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            UIHelper.ApplyFont(lockedLabel);
            overlay.Add(lockedLabel);

            yield return new WaitForSeconds(0.3f);
            bool tapped2 = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped2 = true);
            while (!tapped2) yield return null;
        }
        else if (done)
        {
            string title = DataCarrier.Instance != null ? DataCarrier.Instance.stellaOriginTitle : "";
            var doneLabel = UIHelper.CreateLabel(
                $"『{title}』の ゆりかご", "map-message-text");
            doneLabel.style.color = new Color(1f, 0.84f, 0f);
            doneLabel.style.fontSize = 34;
            doneLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
            doneLabel.style.width = 800;
            doneLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            UIHelper.ApplyFont(doneLabel);
            UIHelper.ApplyFontBold(doneLabel);
            overlay.Add(doneLabel);

            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }
        else
        {
            // おじさんの声
            var introLabel = UIHelper.CreateLabel(
                Localization.Get("stella_cradle_intro"), "map-message-text");
            introLabel.style.color = new Color(1f, 0.84f, 0f);
            introLabel.style.fontSize = 28;
            introLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
            introLabel.style.width = 800;
            introLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            UIHelper.ApplyFont(introLabel);
            overlay.Add(introLabel);

            yield return new WaitForSeconds(0.5f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
            introLabel.RemoveFromHierarchy();

            // 選択UI
            var chooseLabel = UIHelper.CreateLabel(
                Localization.Get("stella_cradle_choose"), "map-message-text");
            chooseLabel.style.color = Color.white;
            chooseLabel.style.fontSize = 32;
            chooseLabel.style.marginBottom = 30;
            UIHelper.ApplyFont(chooseLabel);
            UIHelper.ApplyFontBold(chooseLabel);
            overlay.Add(chooseLabel);

            string[] titleKeys = {
                "stella_cradle_title_courage",
                "stella_cradle_title_kindness",
                "stella_cradle_title_wisdom",
                "stella_cradle_title_harmony"
            };

            string chosenTitle = null;
            for (int i = 0; i < titleKeys.Length; i++)
            {
                string title = Localization.Get(titleKeys[i]);
                var btn = new UIE.Button();
                btn.AddToClassList("map-menu-item-btn");
                btn.text = title;
                UIHelper.ApplyFont(btn);
                UIHelper.ApplyFontBold(btn);
                string captured = title;
                btn.clicked += () => { chosenTitle = captured; };
                overlay.Add(btn);
            }

            while (chosenTitle == null) yield return null;

            // 選択完了 → ボタン群除去
            overlay.Clear();

            // 称号付与
            if (DataCarrier.Instance != null)
            {
                DataCarrier.Instance.stellaOriginTitle = chosenTitle;
                DataCarrier.Instance.stellaOriginProgress |= 4;
                DataCarrier.Instance.SaveData();
            }

            // 完了演出: ゆりかご画像 + 選んだ称号
            var cradleImgEl2 = new UIE.VisualElement();
            cradleImgEl2.pickingMode = UIE.PickingMode.Ignore;
            if (cradleTex2 != null)
                cradleImgEl2.style.backgroundImage = new UIE.StyleBackground(cradleTex2);
            cradleImgEl2.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            cradleImgEl2.style.width = 300;
            cradleImgEl2.style.height = 300;
            cradleImgEl2.style.alignSelf = UIE.Align.Center;
            cradleImgEl2.style.marginBottom = 20;
            overlay.Add(cradleImgEl2);

            var titleLabel = UIHelper.CreateLabel(
                $"『{chosenTitle}』の ゆりかご", "map-message-text");
            titleLabel.style.color = new Color(1f, 0.84f, 0f);
            titleLabel.style.fontSize = 34;
            titleLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
            titleLabel.style.width = 800;
            titleLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            UIHelper.ApplyFont(titleLabel);
            UIHelper.ApplyFontBold(titleLabel);
            overlay.Add(titleLabel);

            string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "???";
            var completeLabel = UIHelper.CreateLabel(
                Localization.Get("stella_cradle_complete", chosenTitle, babyName), "map-message-text");
            completeLabel.style.color = new Color(0.85f, 0.85f, 1f);
            completeLabel.style.fontSize = 28;
            completeLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
            completeLabel.style.width = 800;
            completeLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            UIHelper.ApplyFont(completeLabel);
            overlay.Add(completeLabel);

            yield return new WaitForSeconds(0.5f);
            tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            // 門の状態を更新
            UpdateCosmicGateVisual();
        }

        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.opacity = 1f - (elapsed / 0.3f);
            yield return null;
        }
        overlay.RemoveFromHierarchy();

        menuOpen = false;
        stellaCradleActive = false;

        // 全条件が揃ったらゲート開放演出を自動発動
        CheckCosmicGateAutoOpen();
    }

    // --- 宇宙の門 ---

    void CreateStellaCosmicGate()
    {
        if (tilesContainer == null) return;

        float posX = (stellaCosmicGateX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (stellaCosmicGateY - mapHeight / 2f + 0.5f) * DISPLAY_TILE; // y=37

        stellaCosmicGateObj = new GameObject("CosmicGate");
        stellaCosmicGateObj.transform.SetParent(tilesContainer.transform, false);
        var rect = stellaCosmicGateObj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE * 3f, DISPLAY_TILE * 3f);

        // 門の本体: gate.png を使用
        var gateBody = new GameObject("GateBody");
        gateBody.transform.SetParent(stellaCosmicGateObj.transform, false);
        var gRect = gateBody.AddComponent<RectTransform>();
        gRect.anchoredPosition = Vector2.zero;
        gRect.sizeDelta = new Vector2(DISPLAY_TILE * 3f, DISPLAY_TILE * 3f);
        var gImg = gateBody.AddComponent<Image>();
        gImg.raycastTarget = false;
        gImg.preserveAspect = true;

        // gate.png をロード
        Sprite gateSpr = Resources.Load<Sprite>("Map/4th/gate");
        if (gateSpr == null)
        {
            var gateTex = Resources.Load<Texture2D>("Map/4th/gate");
            if (gateTex != null && gateTex.isReadable)
                gateSpr = Sprite.Create(gateTex, new Rect(0, 0, gateTex.width, gateTex.height), new Vector2(0.5f, 0.5f));
        }
        if (gateSpr != null)
            gImg.sprite = gateSpr;
        gImg.color = Color.white;

        stellaCosmicGateObj.transform.SetAsLastSibling();

        UpdateCosmicGateVisual();
    }

    void UpdateCosmicGateVisual()
    {
        if (stellaCosmicGateObj == null) return;

        int progress = DataCarrier.Instance != null ? DataCarrier.Instance.stellaOriginProgress : 0;
        bool allDone = (progress & 7) == 7; // bit 1 + 2 + 4

        var gateBody = stellaCosmicGateObj.transform.Find("GateBody");

        if (allDone && !stellaGateOpened)
        {
            // ゲート開放: 天の川(y=36) + 門(y=37)をwalkableにする
            stellaGateOpened = true;

            // 天の川の橋: y=36を歩行可能にする
            for (int gx = 11; gx <= 13; gx++)
            {
                int gy = 36;
                if (gx >= 0 && gx < mapWidth && gy >= 0 && gy < mapHeight)
                {
                    mapData[gx, gy] = TILE_CRYSTAL;
                    walkable[gx, gy] = true;
                }
                // 門本体 (y=37)
                if (gx >= 0 && gx < mapWidth && 37 < mapHeight)
                    walkable[gx, 37] = true;
            }
            // 光の川ビジュアル（一本の帯）
            CreateLightRiverBridge();

        }

        // gate.png の表示制御（テキストなし、画像のみ）
        if (gateBody != null)
        {
            var img = gateBody.GetComponent<Image>();
            if (img != null)
            {
                img.color = Color.white; // 全状態でくっきり表示
            }
        }

        // ゲートパーティクル削除済み
    }

    IEnumerator ShowCosmicGateEvent()
    {
        if (stellaGateOpenAnimPlaying) yield break;
        menuOpen = true;

        int progress = DataCarrier.Instance != null ? DataCarrier.Instance.stellaOriginProgress : 0;
        bool allDone = (progress & 7) == 7;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (elapsed / 0.3f));
            yield return null;
        }

        if (!allDone)
        {
            // 未完了: ヒントメッセージ
            string hint = Localization.Get("stella_gate_locked");
            if ((progress & 1) == 0)
                hint += "\n" + Localization.Get("stella_gate_hint_devil");
            else if ((progress & 2) == 0)
                hint += "\n" + Localization.Get("stella_gate_hint_melodias");
            else if ((progress & 4) == 0)
                hint += "\n" + Localization.Get("stella_gate_hint_cradle");

            var label = UIHelper.CreateLabel(hint, "map-message-text");
            label.style.color = new Color(0.7f, 0.7f, 0.8f);
            label.style.fontSize = 28;
            label.style.whiteSpace = UIE.WhiteSpace.Normal;
            label.style.width = 800;
            label.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            UIHelper.ApplyFont(label);
            overlay.Add(label);

            yield return new WaitForSeconds(0.5f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                overlay.style.opacity = 1f - (elapsed / 0.3f);
                yield return null;
            }
            overlay.RemoveFromHierarchy();
            menuOpen = false;
            yield break;
        }

        // === 全条件クリア: ゲート開放演出 ===
        stellaGateOpenAnimPlaying = true;

        // ゴゴゴ…SE（重低音: 正解SEをpitch 0.3で再生）
        if (seSource != null && seQuizCorrect != null)
        {
            seSource.pitch = 0.3f;
            seSource.PlayOneShot(seQuizCorrect, 1f);
        }

        yield return new WaitForSeconds(1.5f);

        // ゲート画像がゆっくり光を帯びる → フェードアウトして開放
        elapsed = 0f;
        float gateFadeDur = 2f;
        var gateBody = stellaCosmicGateObj != null ? stellaCosmicGateObj.transform.Find("GateBody") : null;

        while (elapsed < gateFadeDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / gateFadeDur);

            // 門画像を白く光らせながらフェードアウト
            if (gateBody != null)
            {
                var img = gateBody.GetComponent<Image>();
                if (img != null)
                    img.color = new Color(1f, 1f, 1f, 1f - t * 0.6f);
            }

            yield return null;
        }

        // SE pitch戻す
        if (seSource != null) seSource.pitch = 1f;

        if (seSource != null && seQuizCorrect != null)
            seSource.PlayOneShot(seQuizCorrect, 0.8f);

        // walkable更新 + ビジュアル更新
        UpdateCosmicGateVisual();

        // ゲートオブジェクトを非アクティブに
        if (stellaCosmicGateObj != null)
            stellaCosmicGateObj.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        // 静かにフェードアウト
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            overlay.style.opacity = 1f - (elapsed / 0.5f);
            yield return null;
        }
        overlay.RemoveFromHierarchy();

        menuOpen = false;
        stellaGateOpenAnimPlaying = false;

    }

    // === ラスボス「エゴ・マザー・マシーン」降臨シークエンス ===

    // カットシーン用: Canvas上にスプライトImage(加算合成対応)を生成
    GameObject cutsceneCanvasRoot;
    Image CreateCutsceneImage(string resourcePath, Vector2 size, bool additive = false)
    {
        if (cutsceneCanvasRoot == null) return null;
        var obj = new GameObject(resourcePath);
        obj.transform.SetParent(cutsceneCanvasRoot.transform, false);
        var rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        var img = obj.AddComponent<Image>();
        img.raycastTarget = false;
        Sprite spr = Resources.Load<Sprite>(resourcePath);
        if (spr == null)
        {
            var tex = Resources.Load<Texture2D>(resourcePath);
            if (tex != null && tex.isReadable)
                spr = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        if (spr != null)
        {
            img.sprite = spr;
            img.preserveAspect = true;
        }
        img.type = Image.Type.Simple;
        img.color = new Color(1, 1, 1, 0); // 初期は完全透明
        if (spr == null)
            img.color = Color.clear; // spriteなしなら常にクリア
        if (additive && stellaAdditiveMat != null)
            img.material = stellaAdditiveMat;
        return img;
    }

    IEnumerator CosmicGateEntrySequence()
    {
        menuOpen = true;
        if (moveCtrl != null) moveCtrl.IsLocked = true;

        // --- Phase 1: ゲート進入 ---
        float gateY = (stellaCosmicGateY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        float gatePosX = (stellaCosmicGateX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        playerTileX = stellaCosmicGateX;
        playerTileY = stellaCosmicGateY;
        Vector2 gatePos = new Vector2(gatePosX, gateY);
        float walkDur = 0.5f, walkEl = 0f;
        Vector2 walkStart = playerRect.anchoredPosition;
        while (walkEl < walkDur)
        {
            walkEl += Time.deltaTime;
            playerRect.anchoredPosition = Vector2.Lerp(walkStart, gatePos, walkEl / walkDur);
            yield return null;
        }
        playerRect.anchoredPosition = gatePos;
        yield return new WaitForSeconds(0.3f);

        // カットシーン専用Canvas（ソート順を最前面に）
        cutsceneCanvasRoot = new GameObject("CutsceneCanvas");
        var cCanvas = cutsceneCanvasRoot.AddComponent<Canvas>();
        cCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cCanvas.sortingOrder = 999;
        var cScaler = cutsceneCanvasRoot.AddComponent<UnityEngine.UI.CanvasScaler>();
        cScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cScaler.referenceResolution = new Vector2(1080, 1920);
        cutsceneCanvasRoot.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // 黒幕（全画面、最初から黒）
        var blackObj = new GameObject("Black");
        blackObj.transform.SetParent(cutsceneCanvasRoot.transform, false);
        var blackRect = blackObj.AddComponent<RectTransform>();
        blackRect.anchorMin = Vector2.zero; blackRect.anchorMax = Vector2.one;
        blackRect.sizeDelta = Vector2.zero;
        var blackImg = blackObj.AddComponent<Image>();
        blackImg.color = Color.black;
        blackImg.raycastTarget = false;

        // --- Phase 2: レンズフレア明滅 ---
        yield return new WaitForSeconds(0.5f);

        // ② 黄金のレンズフレア（加算合成）
        var lensFlareImg = CreateCutsceneImage("Map/4th/黄金のレンズフレア", new Vector2(1200, 1200), true);
        var lensFlareRect = lensFlareImg.GetComponent<RectTransform>();

        // 明滅3回
        for (int flash = 0; flash < 3; flash++)
        {
            float flashIn = 0f;
            while (flashIn < 0.15f)
            {
                flashIn += Time.deltaTime;
                lensFlareImg.color = new Color(1, 1, 1, Mathf.Clamp01(flashIn / 0.15f) * 0.9f);
                yield return null;
            }
            float flashOut = 0f;
            while (flashOut < 0.2f)
            {
                flashOut += Time.deltaTime;
                lensFlareImg.color = new Color(1, 1, 1, (1f - Mathf.Clamp01(flashOut / 0.2f)) * 0.9f);
                yield return null;
            }
            lensFlareImg.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.1f);
        }

        // フルブラスト
        float fadeEl = 0f;
        while (fadeEl < 0.5f)
        {
            fadeEl += Time.deltaTime;
            float t = Mathf.Clamp01(fadeEl / 0.5f);
            lensFlareImg.color = new Color(1, 1, 1, t * 0.95f);
            float scale = 1f + t * 0.3f;
            lensFlareRect.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        // SE: 重低音
        if (seSource != null && seQuizCorrect != null)
        {
            seSource.pitch = 0.2f;
            seSource.PlayOneShot(seQuizCorrect, 1f);
        }
        yield return new WaitForSeconds(1f);

        // レンズフレアをフェードアウト
        fadeEl = 0f;
        while (fadeEl < 0.5f)
        {
            fadeEl += Time.deltaTime;
            lensFlareImg.color = new Color(1, 1, 1, 0.95f * (1f - Mathf.Clamp01(fadeEl / 0.5f)));
            yield return null;
        }
        lensFlareImg.color = new Color(1, 1, 1, 0);

        // --- Phase 3: 深宇宙の星雲 + ゴッドレイ + ボス浮上 ---

        // ① 深宇宙の星雲（最背面、ゆっくり拡大）
        var nebulaImg = CreateCutsceneImage("Map/4th/深宇宙の星雲", new Vector2(1200, 2100), false);
        var nebulaRect = nebulaImg.GetComponent<RectTransform>();
        nebulaImg.color = new Color(1, 1, 1, 0);
        nebulaImg.transform.SetSiblingIndex(1); // 黒幕の直上

        // 渦巻く星パーティクル（星雲の上に）
        var galaxyParticles = new List<(RectTransform rt, Image img, float angle, float radius, float speed)>();
        for (int i = 0; i < 80; i++)
        {
            var starObj = new GameObject($"Star_{i}");
            starObj.transform.SetParent(cutsceneCanvasRoot.transform, false);
            var srt = starObj.AddComponent<RectTransform>();
            float size = Random.Range(2f, 6f);
            srt.sizeDelta = new Vector2(size, size);
            var simg = starObj.AddComponent<Image>();
            simg.raycastTarget = false;
            if (stellaAdditiveMat != null) simg.material = stellaAdditiveMat;
            float b = Random.Range(0.5f, 1f);
            bool isGold = Random.value < 0.4f;
            simg.color = isGold
                ? new Color(1f, 0.84f * b, 0f, 0)
                : new Color(b, b, b * 0.9f + 0.1f, 0);
            float ang = Random.Range(0f, Mathf.PI * 2f);
            float rad = Random.Range(50f, 450f);
            float spd = Random.Range(0.3f, 1.2f);
            galaxyParticles.Add((srt, simg, ang, rad, spd));
        }

        // 吸引パーティクル
        var suckParticles = new List<(RectTransform rt, Image img, float angle, float startR, float life)>();
        for (int i = 0; i < 40; i++)
        {
            var pObj = new GameObject($"Suck_{i}");
            pObj.transform.SetParent(cutsceneCanvasRoot.transform, false);
            var prt = pObj.AddComponent<RectTransform>();
            float size = Random.Range(3f, 8f);
            prt.sizeDelta = new Vector2(size, size);
            var pimg = pObj.AddComponent<Image>();
            pimg.raycastTarget = false;
            if (stellaAdditiveMat != null) pimg.material = stellaAdditiveMat;
            pimg.color = new Color(1f, 0.84f, 0f, 0);
            suckParticles.Add((prt, pimg, Random.Range(0f, Mathf.PI * 2f), Random.Range(300f, 500f), Random.Range(0f, 1f)));
        }

        // ③ 放射状の光（ボスの直背後、加算合成、回転）
        var godRayImg = CreateCutsceneImage("Map/4th/放射状の光", new Vector2(600, 600), true);
        var godRayRect = godRayImg.GetComponent<RectTransform>();
        godRayImg.color = new Color(1, 1, 1, 0);
        godRayRect.anchoredPosition = new Vector2(0, 300); // 画面下に隠す

        // ④ ボス画像（ego-mother）1.5倍サイズ
        var bossImg = CreateCutsceneImage("EnemyBabys/boss/ego-mother", new Vector2(480, 480), false);
        var bossImgRect = bossImg.GetComponent<RectTransform>();
        bossImg.color = new Color(1, 1, 1, 0);
        bossImgRect.anchoredPosition = new Vector2(0, 300); // 画面下に隠す

        // アニメーション: 星雲拡大 + 銀河回転 + ボス浮上 (5秒)
        float phase3Dur = 5f;
        float phase3El = 0f;
        float galaxyTime = 0f;

        while (phase3El < phase3Dur)
        {
            phase3El += Time.deltaTime;
            galaxyTime += Time.deltaTime;
            float t = Mathf.Clamp01(phase3El / phase3Dur);

            // 星雲: フェードイン + ゆっくり拡大 (1.0→1.2)
            float nebulaAlpha = Mathf.Clamp01(t / 0.3f);
            nebulaImg.color = new Color(1, 1, 1, nebulaAlpha);
            float nebulaScale = 1f + t * 0.2f;
            nebulaRect.localScale = new Vector3(nebulaScale, nebulaScale, 1f);

            // パーティクル: 徐々に表示
            float particleAlpha = Mathf.Clamp01((t - 0.1f) / 0.2f);
            foreach (var (srt, simg, ang, rad, spd) in galaxyParticles)
            {
                float curAngle = ang + galaxyTime * spd;
                float curRad = rad * (1f - t * 0.3f);
                srt.anchoredPosition = new Vector2(
                    Mathf.Cos(curAngle) * curRad,
                    Mathf.Sin(curAngle) * curRad);
                var sc = simg.color;
                simg.color = new Color(sc.r, sc.g, sc.b, particleAlpha * (sc.r > 0.9f ? 0.8f : 0.6f));
            }
            foreach (var (prt, pimg, ang, startR, life) in suckParticles)
            {
                float lt = Mathf.Repeat(galaxyTime * 0.5f + life, 1f);
                float curR = startR * (1f - lt);
                prt.anchoredPosition = new Vector2(
                    Mathf.Cos(ang + galaxyTime * 0.3f) * curR,
                    Mathf.Sin(ang + galaxyTime * 0.3f) * curR);
                float pa = particleAlpha * (lt < 0.8f ? 0.7f : (1f - (lt - 0.8f) / 0.2f) * 0.7f);
                pimg.color = new Color(1f, 0.84f, 0f, pa);
            }

            // ボス浮上: 下(y=300)→中央(y=0) ease-out cubic
            float riseT = t < 0.25f ? 0f : Mathf.Clamp01((t - 0.25f) / 0.6f);
            float easeRise = 1f - Mathf.Pow(1f - riseT, 3f);
            float bossY = Mathf.Lerp(300f, 0f, easeRise);
            bossImgRect.anchoredPosition = new Vector2(0, bossY);
            bossImg.color = new Color(1, 1, 1, riseT);

            // ゴッドレイ: ボスに追従 + 回転
            godRayRect.anchoredPosition = new Vector2(0, bossY);
            godRayRect.localRotation = Quaternion.Euler(0, 0, galaxyTime * 15f);
            float godRayAlpha = riseT * 0.7f;
            godRayImg.color = new Color(1, 1, 1, godRayAlpha);
            // ゴッドレイを少し拡大
            float grScale = 1f + riseT * 0.5f;
            godRayRect.localScale = new Vector3(grScale, grScale, 1f);

            yield return null;
        }

        // SE: 高音 — ボスが見えた
        if (seSource != null)
        {
            seSource.pitch = 1.5f;
            if (seQuizCorrect != null) seSource.PlayOneShot(seQuizCorrect, 0.7f);
        }

        // ボス完全表示
        bossImg.color = Color.white;
        godRayImg.color = new Color(1, 1, 1, 0.8f);

        yield return new WaitForSeconds(0.5f);

        // --- Phase 4: ロゴタイトル ---
        // パーティクルを暗くする
        foreach (var (_, simg, _, _, _) in galaxyParticles)
        {
            var sc = simg.color;
            simg.color = new Color(sc.r, sc.g, sc.b, sc.a * 0.3f);
        }

        // ⑤ 質感のあるロゴテキスト画像（RawImageで背景色なし）
        var logoObj = new GameObject("ego_mother_logo");
        logoObj.transform.SetParent(cutsceneCanvasRoot.transform, false);
        var logoRect = logoObj.AddComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(900, 450);
        logoRect.anchoredPosition = new Vector2(0, 700);
        var logoRaw = logoObj.AddComponent<RawImage>();
        logoRaw.raycastTarget = false;
        var logoTex = Resources.Load<Texture2D>("Map/4th/ego_mother_logo");
        if (logoTex != null)
            logoRaw.texture = logoTex;
        logoRaw.color = new Color(1, 1, 1, 0); // 初期透明
        // ロゴ背後の小さなレンズフレア（加算合成）
        var logoFlareImg = CreateCutsceneImage("Map/4th/黄金のレンズフレア", new Vector2(500, 500), true);
        var logoFlareRect = logoFlareImg.GetComponent<RectTransform>();
        logoFlareRect.anchoredPosition = new Vector2(0, 700);
        logoFlareImg.transform.SetSiblingIndex(logoObj.transform.GetSiblingIndex()); // ロゴの後ろに

        // ロゴフェードイン + ゴッドレイ回転継続
        fadeEl = 0f;
        while (fadeEl < 1.5f)
        {
            fadeEl += Time.deltaTime;
            float t = Mathf.Clamp01(fadeEl / 1.5f);
            galaxyTime += Time.deltaTime;

            // ロゴ画像フェードイン
            logoRaw.color = new Color(1, 1, 1, t);

            // ロゴ背後フレア: 少し遅れてフェードイン + 脈動
            float flareT = Mathf.Clamp01((fadeEl - 0.3f) / 1f);
            float flarePulse = flareT * (0.3f + 0.1f * Mathf.Sin(fadeEl * 4f));
            logoFlareImg.color = new Color(1, 1, 1, flarePulse);

            // ゴッドレイ回転継続 + 脈動
            godRayRect.localRotation = Quaternion.Euler(0, 0, galaxyTime * 15f);
            float grPulse = 0.7f + 0.15f * Mathf.Sin(fadeEl * 3f);
            godRayImg.color = new Color(1, 1, 1, grPulse);

            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        // SE pitch戻す
        if (seSource != null) seSource.pitch = 1f;

        // --- Phase 5: レンズフレア → ブラックアウト ---
        // レンズフレアを全画面に拡大しながらフェードアウト
        lensFlareRect.localScale = Vector3.one;
        fadeEl = 0f;
        while (fadeEl < 1.2f)
        {
            fadeEl += Time.deltaTime;
            float t = Mathf.Clamp01(fadeEl / 1.2f);
            galaxyTime += Time.deltaTime;

            // レンズフレア: 拡大しながら輝く→消える
            float flareAlpha = t < 0.4f ? t / 0.4f : 1f - (t - 0.4f) / 0.6f;
            lensFlareImg.color = new Color(1, 1, 1, flareAlpha * 0.8f);
            float flareScale = 1f + t * 1.5f;
            lensFlareRect.localScale = new Vector3(flareScale, flareScale, 1f);

            // 他の要素をフェードアウト
            float fadeOut = Mathf.Clamp01((t - 0.3f) / 0.7f);
            nebulaImg.color = new Color(1, 1, 1, 1f - fadeOut);
            bossImg.color = new Color(1, 1, 1, 1f - fadeOut);
            godRayImg.color = new Color(1, 1, 1, (1f - fadeOut) * 0.7f);
            logoRaw.color = new Color(1, 1, 1, 1f - fadeOut);
            logoFlareImg.color = new Color(1, 1, 1, (1f - fadeOut) * 0.3f);

            // 黒幕を徐々に重ねる
            blackImg.color = new Color(0, 0, 0, fadeOut);

            // ゴッドレイ回転
            godRayRect.localRotation = Quaternion.Euler(0, 0, galaxyTime * 15f);

            yield return null;
        }

        blackImg.color = Color.black;

        // DataCarrier設定
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = true;
            DataCarrier.Instance.SaveData();
        }

        yield return StartCoroutine(CaptureMapScreenshot());

        // カットシーンCanvas破棄
        Destroy(cutsceneCanvasRoot);
        cutsceneCanvasRoot = null;

        SceneManager.LoadScene("BattleScene");
    }

    void SpawnStellaRipple()
    {
        if (tilesContainer == null || stellaAuraSprite == null) return;
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area != 8) return;

        float posX = (playerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (playerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        // タイルの色を取得してTintに使用
        Color tint = new Color(0.6f, 0.8f, 1f); // デフォルト: 水色
        if (stellaTileMap.TryGetValue((playerTileX, playerTileY), out var tile) && tile.img != null)
        {
            var tc = tile.img.color;
            tint = new Color(tc.r, tc.g, tc.b);
        }

        var aura = new GameObject("AuraRipple");
        aura.transform.SetParent(tilesContainer.transform, false);
        var rt = aura.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(posX, posY);
        float startSize = DISPLAY_TILE * 0.5f;
        rt.sizeDelta = new Vector2(startSize, startSize);

        var img = aura.AddComponent<Image>();
        img.sprite = stellaAuraSprite;
        img.preserveAspect = true;
        img.color = new Color(tint.r, tint.g, tint.b, 1f);
        img.raycastTarget = false;
        // 加算合成マテリアル適用
        if (stellaAdditiveMat != null)
            img.material = stellaAdditiveMat;

        // プレイヤーの直下に配置
        aura.transform.SetSiblingIndex(playerObj.transform.GetSiblingIndex());

        stellaAuraRipples.Add(new StellaAuraRipple
        {
            rt = rt,
            img = img,
            elapsed = 0f,
            duration = Random.Range(0.4f, 0.6f),
            tintColor = tint
        });
    }

    void UpdateStellaOrigin()
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area != 8) return;

        stellaCrystalPulseTime += Time.deltaTime;

        // Aura波紋の拡大 + フェードアウト（加算合成）
        for (int i = stellaAuraRipples.Count - 1; i >= 0; i--)
        {
            var ar = stellaAuraRipples[i];
            if (ar.rt == null) { stellaAuraRipples.RemoveAt(i); continue; }

            ar.elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(ar.elapsed / ar.duration);

            // サイズ: 0.5x → 2.0x (DISPLAY_TILE基準)
            float size = Mathf.Lerp(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 2.0f, t);
            ar.rt.sizeDelta = new Vector2(size, size);

            // 不透明度: 1.0 → 0.0
            float alpha = Mathf.Lerp(1f, 0f, t);
            ar.img.color = new Color(ar.tintColor.r, ar.tintColor.g, ar.tintColor.b, alpha);

            if (t >= 1f)
            {
                Destroy(ar.rt.gameObject);
                stellaAuraRipples.RemoveAt(i);
            }
        }

        // 背景のゆっくり回転（1分で1回転 = 6°/秒）
        if (stellaBgRect != null)
        {
            stellaBgRect.localRotation = Quaternion.Euler(0, 0, stellaCrystalPulseTime * 6f);
        }

        // ■1. 虹色（レインボー）明滅: パステルピンク〜青〜紫を緩やかにクロスフェード
        if (stellaBgImg != null)
        {
            float hue = Mathf.Repeat(stellaCrystalPulseTime * 0.03f, 1f); // 約33秒で1周
            float emission = 0.35f + 0.15f * Mathf.Sin(stellaCrystalPulseTime * 0.5f); // 明滅
            Color rainbow = Color.HSVToRGB(hue, 0.25f, emission + 0.3f);
            stellaBgImg.color = rainbow;
        }

        // ■2. 視差効果（パララックス）: カメラ移動の5%分だけ背景を逆方向にずらす
        if (stellaBgRect != null)
        {
            float camX = (playerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
            float camY = (playerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
            Vector2 parallaxOffset = new Vector2(-camX * 0.05f, -camY * 0.05f);
            stellaBgRect.anchoredPosition = stellaBgBasePos + parallaxOffset;
            if (stellaDarkOverlayRect != null)
                stellaDarkOverlayRect.anchoredPosition = stellaDarkBasePos + parallaxOffset * 0.5f;
        }

        // ■3. グリッター（光の瞬き）: sin波による明滅＋微細なUVドリフト
        for (int gi = 0; gi < stellaGlitters.Count; gi++)
        {
            var (grt, gimg, gphase, gspeed) = stellaGlitters[gi];
            if (grt == null) continue;

            // 明滅: ゆっくり現れて消える
            float glitterAlpha = Mathf.Max(0f,
                Mathf.Sin(stellaCrystalPulseTime * gspeed + gphase));
            glitterAlpha = glitterAlpha * glitterAlpha * 0.8f; // 二乗で鋭いきらめき
            var gc = gimg.color;
            gimg.color = new Color(gc.r, gc.g, gc.b, glitterAlpha);

            // UVドリフト: 極めてゆっくりsin波で揺れる
            var pos = grt.anchoredPosition;
            pos.x += Mathf.Cos(stellaCrystalPulseTime * 0.01f + gphase) * 0.3f * Time.deltaTime;
            pos.y += Mathf.Sin(stellaCrystalPulseTime * 0.01f + gphase * 1.3f) * 0.2f * Time.deltaTime;
            grt.anchoredPosition = pos;
        }

        // ガーディアンオーラリング回転
        if (stellaGuardianDevilObj != null)
        {
            var devilAura = stellaGuardianDevilObj.transform.Find("Aura");
            if (devilAura != null)
                devilAura.localRotation = Quaternion.Euler(0, 0, -stellaCrystalPulseTime * 20f);
        }
        if (stellaGuardianMelodiasObj != null)
        {
            var melAura = stellaGuardianMelodiasObj.transform.Find("Aura");
            if (melAura != null)
                melAura.localRotation = Quaternion.Euler(0, 0, stellaCrystalPulseTime * 20f);
        }

        // 音叉: 浮遊アニメーション + 明滅
        for (int i = 0; i < stellaForkVisuals.Count; i++)
        {
            var (frt, fimg, fbaseY, fphase) = stellaForkVisuals[i];
            if (frt == null) continue;
            float floatY = Mathf.Sin(stellaCrystalPulseTime * 0.7f + fphase) * 4f;
            frt.anchoredPosition = new Vector2(frt.anchoredPosition.x, fbaseY + floatY);
            if (fimg != null)
            {
                float pulse = 0.8f + 0.15f * Mathf.Sin(stellaCrystalPulseTime * 1.5f + fphase);
                fimg.color = new Color(1f, 1f, 1f, pulse);
            }
        }

        // クリスタルタイルの浮遊 + 現在地ハイライト
        // 全タイル常時表示。現在地だけ100%輝度、それ以外はベースアルファ
        float playerWorldX = (playerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float playerWorldY = (playerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        float fadeSpeed = 1f / 0.2f;

        foreach (var kv in stellaTileMap)
        {
            var t = kv.Value;
            if (t.rt == null) continue;

            int dist = Mathf.Abs(t.tileX - playerTileX) + Mathf.Abs(t.tileY - playerTileY);

            // 目標アルファ: 現在地=100%, 隣接=基本値, 遠方=基本値
            t.goalAlpha = (dist == 0) ? t.maxAlpha : t.maxAlpha * 0.7f;

            // スムーズ補間
            t.currentAlpha = Mathf.MoveTowards(t.currentAlpha, t.goalAlpha, fadeSpeed * Time.deltaTime * t.maxAlpha);

            // 色更新
            var c = t.img.color;
            t.img.color = new Color(c.r, c.g, c.b, t.currentAlpha);

            // 浮遊アニメーション（sin波）
            float floatY = Mathf.Sin(stellaCrystalPulseTime * t.speed + t.phase) * 2.5f;
            t.rt.anchoredPosition = new Vector2(t.rt.anchoredPosition.x, t.baseY + floatY);
        }

        // 光子パーティクルのドリフト + アルファ脈動
        float halfMapW = mapWidth * DISPLAY_TILE * 0.45f;
        float halfMapH = mapHeight * DISPLAY_TILE * 0.45f;
        for (int i = 0; i < stellaPhotons.Count; i++)
        {
            var (phRt, phSpeed, phPhase, phBaseAlpha) = stellaPhotons[i];
            if (phRt == null) continue;
            // ゆっくりドリフト
            var phPos = phRt.anchoredPosition;
            phPos.x += Mathf.Cos(stellaCrystalPulseTime * phSpeed + phPhase) * 8f * Time.deltaTime;
            phPos.y += Mathf.Sin(stellaCrystalPulseTime * phSpeed * 0.7f + phPhase) * 6f * Time.deltaTime;
            // 画面外に出たらワープ
            if (phPos.x > halfMapW) phPos.x = -halfMapW;
            if (phPos.x < -halfMapW) phPos.x = halfMapW;
            if (phPos.y > halfMapH) phPos.y = -halfMapH;
            if (phPos.y < -halfMapH) phPos.y = halfMapH;
            phRt.anchoredPosition = phPos;
            // アルファ脈動
            var phImg = phRt.GetComponent<Image>();
            if (phImg != null)
            {
                float a = phBaseAlpha * (0.5f + 0.5f * Mathf.Sin(stellaCrystalPulseTime * phSpeed * 1.5f + phPhase));
                phImg.color = new Color(phImg.color.r, phImg.color.g, phImg.color.b, a);
            }
        }

        // 足元波紋・トレイル削除済み
    }

    void CreateMapUI()
    {
        if (canvas == null) return;

        int areaForColor = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        // area==0: UI Toolkit でDQタイルを描画（Canvas背面）
        if (areaForColor == 0)
            CreateVillageTileGrid();

        // マップパネル（画面全体の背景）
        mapPanel = new GameObject("MapPanel");
        mapPanel.transform.SetParent(canvas.transform, false);
        var mapRect = mapPanel.AddComponent<RectTransform>();
        mapRect.anchorMin = Vector2.zero;
        mapRect.anchorMax = Vector2.one;
        mapRect.offsetMin = Vector2.zero;
        mapRect.offsetMax = Vector2.zero;

        var bg = mapPanel.AddComponent<UnityEngine.UI.Image>();
        if (areaForColor == 0)
            bg.color = Color.clear;  // 村: UI Toolkit が背景を担当
        else if (areaForColor == 3)
            bg.color = new Color(0.12f, 0.06f, 0.18f);  // 小悪魔の森: 暗い紫
        else if (areaForColor == 6)
            bg.color = new Color(0.2f, 0.1f, 0.25f);    // 武器屋内部: 暗い紫
        else if (areaForColor == 5)
            bg.color = new Color(0.35f, 0.25f, 0.15f);  // 父親の家: 木の温かい色
        else if (areaForColor == 2)
            bg.color = new Color(0.1f, 0.08f, 0.08f);   // 館内: 暗灰
        else if (areaForColor == 1)
            bg.color = new Color(0.2f, 0.08f, 0.15f);   // ゴージャス・ヴィレッジ: 暗い赤紫
        else if (areaForColor == 8)
            bg.color = new Color(0.02f, 0.01f, 0.06f);  // ステラ・オリジン: 深宇宙
        else if (areaForColor == 7)
            bg.color = new Color(0.75f, 0.70f, 0.60f);  // 塾: 明るいベージュ
        else
            bg.color = new Color(0.15f, 0.35f, 0.15f);  // フォールバック
        bg.raycastTarget = false;

        // タイルコンテナ（オーバーレイ・プレイヤー配置用、area==0 でもuGUIコンテナは維持）
        tilesContainer = new GameObject("TilesContainer");
        tilesContainer.transform.SetParent(mapPanel.transform, false);
        tilesContainerRect = tilesContainer.AddComponent<RectTransform>();
        tilesContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        tilesContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        tilesContainerRect.anchoredPosition = new Vector2(0, -40);
        tilesContainerRect.sizeDelta = new Vector2(mapWidth * DISPLAY_TILE, mapHeight * DISPLAY_TILE);

        // Area 8: ステラ・オリジン宇宙背景
        if (areaForColor == 8)
            CreateStellaOriginBackground();

        // Area 3: 1枚絵の背景（銀河のベロア）をLayer 0として敷く
        if (areaForColor == 3 && thirdBgSprite != null)
        {
            var bgLayer = new GameObject("ThirdBgLayer");
            bgLayer.transform.SetParent(tilesContainer.transform, false);
            var bgRect = bgLayer.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(mapWidth * DISPLAY_TILE, mapHeight * DISPLAY_TILE);
            var bgImg = bgLayer.AddComponent<Image>();
            bgImg.sprite = thirdBgSprite;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
            bgImg.raycastTarget = false;
            bgLayer.transform.SetAsFirstSibling();
        }

        // Area 3: 湖オーバーレイ（背景の上、タイルの下）
        if (areaForColor == 3 && thirdLakeSprite != null)
        {
            // 沼 (x=1-3, y=15-16)
            CreateLakeOverlay(1, 3, 15, 16);

            // プール (x=1-5, y=33-36)
            if (thirdPoolSprite != null)
            {
                float cx = ((1 + 4) / 2f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
                float cy = ((34 + 36) / 2f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
                float w = 4 * DISPLAY_TILE;
                float h = 3 * DISPLAY_TILE;
                var pool = new GameObject("PoolOverlay");
                pool.transform.SetParent(tilesContainer.transform, false);
                var pRect = pool.AddComponent<RectTransform>();
                pRect.anchoredPosition = new Vector2(cx, cy);
                pRect.sizeDelta = new Vector2(w, h);

                var pImg = pool.AddComponent<Image>();
                pImg.sprite = thirdPoolSprite;
                pImg.type = Image.Type.Simple;
                pImg.preserveAspect = false;
                pImg.raycastTarget = false;

                // かぐや3回目: プールに泡ヒント
                if (DataCarrier.Instance != null && DataCarrier.Instance.kaguyaMetCount == 2 && !DataCarrier.Instance.kaguyaLover)
                {
                    poolBubbles = new TMPro.TextMeshProUGUI[3];
                    for (int bi = 0; bi < poolBubbles.Length; bi++)
                    {
                        var bubObj = new GameObject("PoolBubble" + bi);
                        bubObj.transform.SetParent(pool.transform, false);
                        var bRect = bubObj.AddComponent<RectTransform>();
                        bRect.anchoredPosition = new Vector2(Random.Range(-w * 0.3f, w * 0.3f), Random.Range(-h * 0.2f, h * 0.2f));
                        bRect.sizeDelta = new Vector2(20, 20);
                        var bTmp = bubObj.AddComponent<TMPro.TextMeshProUGUI>();
                        FontHelper.Apply(bTmp);
                        bTmp.text = "\u25CB";
                        bTmp.fontSize = 10 + bi * 3;
                        bTmp.alignment = TMPro.TextAlignmentOptions.Center;
                        bTmp.color = new Color(0.8f, 0.95f, 1f, 0f);
                        bTmp.raycastTarget = false;
                        poolBubbles[bi] = bTmp;
                    }
                }
            }
        }

        // タイルを配置
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (areaForColor == 0)
                    CreateDQTileElement(x, y, mapData[x, y]); // UI Toolkit
                else
                    CreateTile(x, y, mapData[x, y]); // uGUI
            }
        }

        // ボスの館オーバーレイ（タイルの上に大きな館を描画）
        CreateBossMansionOverlay();

        // 装備ショップオーバーレイ（村・ゴージャス・ヴィレッジ・小悪魔の街）
        if (areaForColor == 0 || areaForColor == 1 || areaForColor == 3)
            CreateWeaponShopOverlay();

        // 父親の家オーバーレイ
        if (areaForColor == 0)
            CreateFatherHouseOverlay();

        // 塾の看板
        if (areaForColor == 0)
            CreateJukuSign();

        // よちよちの里マップ（area==0）: スプライトオーバーレイ（uGUI、UI Toolkit の上に表示）
        if (areaForColor == 0)
            CreateVillageOverlays();

        // 父親の家（area==5）: 家具オーバーレイ
        if (areaForColor == 5)
            CreateFatherHouseFurniture();

        // 武器屋内部（area==6）: 家具オーバーレイ
        if (areaForColor == 6)
            CreateWeaponShopFurniture();

        // 塾内部（area==7）: 家具オーバーレイ
        if (areaForColor == 7)
            CreateJukuFurniture();
    }

    void CreateBossMansionOverlay()
    {
        if (tilesContainer == null) return;

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area == 2) return; // 館内ではオーバーレイ不要

        // TILE_BOSS_MANSION の中心を動的に検出
        int bMinX = mapWidth, bMaxX = 0, bMinY = mapHeight, bMaxY = 0;
        for (int bx = 0; bx < mapWidth; bx++)
            for (int by = 0; by < mapHeight; by++)
                if (mapData[bx, by] == TILE_BOSS_MANSION)
                {
                    if (bx < bMinX) bMinX = bx;
                    if (bx > bMaxX) bMaxX = bx;
                    if (by < bMinY) bMinY = by;
                    if (by > bMaxY) bMaxY = by;
                }
        if (bMaxX < bMinX) return; // 館タイルなし
        float centerX = ((bMinX + bMaxX) / 2f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float centerY = ((bMinY + bMaxY) / 2f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var mansion = new GameObject("BossMansion");
        mansion.transform.SetParent(tilesContainer.transform, false);
        var mansionRect = mansion.AddComponent<RectTransform>();
        mansionRect.anchoredPosition = new Vector2(centerX, centerY);
        mansionRect.sizeDelta = new Vector2(DISPLAY_TILE * 3, DISPLAY_TILE * 2.5f);

        // スプライト画像を試みる
        Sprite mansionSprite = (area == 1) ? Resources.Load<Sprite>("Map/Devil_Home")
            : (area == 3) ? Resources.Load<Sprite>("Map/Devil_Home")
            : Resources.Load<Sprite>("Map/Shiba_Home");
        if (mansionSprite != null)
        {
            var img = mansion.AddComponent<Image>();
            img.sprite = mansionSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
        else
        {
            // プロシージャル描画 — ゴージャス・ヴィレッジ・小悪魔の街は紫系
            bool darkTheme = (area == 1 || area == 3);
            var wall = FacePart("Wall", mansion.transform, Vector2.zero, new Vector2(DISPLAY_TILE * 2.8f, DISPLAY_TILE * 2.0f));
            wall.AddComponent<Image>().color = darkTheme
                ? new Color(0.15f, 0.05f, 0.2f)
                : new Color(0.2f, 0.07f, 0.1f);

            // 屋根（三角形風）
            var roof = FacePart("Roof", mansion.transform, new Vector2(0, DISPLAY_TILE * 0.8f), new Vector2(DISPLAY_TILE * 3.0f, DISPLAY_TILE * 0.7f));
            roof.AddComponent<Image>().color = darkTheme
                ? new Color(0.25f, 0.05f, 0.3f)
                : new Color(0.35f, 0.05f, 0.08f);

            // 屋根の先端
            var roofTop = FacePart("RoofTop", mansion.transform, new Vector2(0, DISPLAY_TILE * 1.2f), new Vector2(DISPLAY_TILE * 1.8f, DISPLAY_TILE * 0.45f));
            roofTop.AddComponent<Image>().color = darkTheme
                ? new Color(0.3f, 0.05f, 0.35f)
                : new Color(0.4f, 0.05f, 0.05f);

            // 窓（左）
            var winL = FacePart("WinL", mansion.transform, new Vector2(-DISPLAY_TILE * 0.55f, DISPLAY_TILE * 0.1f), new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.5f));
            winL.AddComponent<Image>().color = darkTheme
                ? new Color(0.7f, 0.3f, 0.9f, 0.8f)
                : new Color(0.9f, 0.7f, 0.2f, 0.8f);

            // 窓（右）
            var winR = FacePart("WinR", mansion.transform, new Vector2(DISPLAY_TILE * 0.55f, DISPLAY_TILE * 0.1f), new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.5f));
            winR.AddComponent<Image>().color = darkTheme
                ? new Color(0.7f, 0.3f, 0.9f, 0.8f)
                : new Color(0.9f, 0.7f, 0.2f, 0.8f);

            // ドア
            var door = FacePart("Door", mansion.transform, new Vector2(0, -DISPLAY_TILE * 0.6f), new Vector2(DISPLAY_TILE * 0.6f, DISPLAY_TILE * 0.8f));
            door.AddComponent<Image>().color = darkTheme
                ? new Color(0.2f, 0.1f, 0.25f)
                : new Color(0.35f, 0.15f, 0.1f);

            // ドアノブ
            var knob = FacePart("Knob", mansion.transform, new Vector2(DISPLAY_TILE * 0.12f, -DISPLAY_TILE * 0.6f), new Vector2(12, 12));
            knob.AddComponent<Image>().color = new Color(0.8f, 0.65f, 0.2f);
        }

        // 看板 — 館の手前
        var signObj = new GameObject("BossSign");
        signObj.transform.SetParent(tilesContainer.transform, false);
        var signRect = signObj.AddComponent<RectTransform>();
        float signX = ((bMinX + bMaxX) / 2f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float signY = (bMinY - 1f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        signRect.anchoredPosition = new Vector2(signX, signY - DISPLAY_TILE * 0.1f);
        signRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.2f, DISPLAY_TILE * 0.55f);

        var signBg = signObj.AddComponent<Image>();
        signBg.color = (area == 1 || area == 3)
            ? new Color(0.1f, 0.02f, 0.15f, 0.9f)
            : new Color(0.15f, 0.05f, 0.05f, 0.9f);
        signBg.raycastTarget = false;

        var signTextObj = new GameObject("SignText");
        signTextObj.transform.SetParent(signObj.transform, false);
        var signTextRect = signTextObj.AddComponent<RectTransform>();
        signTextRect.anchorMin = Vector2.zero;
        signTextRect.anchorMax = Vector2.one;
        signTextRect.offsetMin = Vector2.zero;
        signTextRect.offsetMax = Vector2.zero;

        var signText = signTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(signText);
        signText.text = (area == 3) ? Localization.Get("map_109_sign")
            : (area == 1) ? Localization.Get("map_devil_boss_sign")
            : Localization.Get("map_boss_sign");
        signText.fontSize = 18;
        signText.alignment = TextAlignmentOptions.Center;
        signText.color = Color.white;
        signText.fontStyle = FontStyles.Bold;
        signText.raycastTarget = false;
    }

    void CreateWeaponShopOverlay()
    {
        if (tilesContainer == null) return;

        // ショップタイルの中心を動的に検出
        int minSX = mapWidth, maxSX = 0, minSY = mapHeight, maxSY = 0;
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                if (mapData[x, y] == TILE_WEAPON_SHOP)
                {
                    if (x < minSX) minSX = x;
                    if (x > maxSX) maxSX = x;
                    if (y < minSY) minSY = y;
                    if (y > maxSY) maxSY = y;
                }
        if (maxSX < minSX) return; // ショップなし
        float shopTileCX = (minSX + maxSX) / 2f;
        float shopTileCY = (minSY + maxSY) / 2f;
        float shopCenterX = (shopTileCX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float shopCenterY = (shopTileCY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var shop = new GameObject("WeaponShop");
        shop.transform.SetParent(tilesContainer.transform, false);
        var shopRect = shop.AddComponent<RectTransform>();
        shopRect.anchoredPosition = new Vector2(shopCenterX, shopCenterY);
        shopRect.sizeDelta = new Vector2(DISPLAY_TILE * 2, DISPLAY_TILE * 2.5f);

        Sprite shopSprite = Resources.Load<Sprite>("Map/Wapon_Shop");
        if (shopSprite != null)
        {
            var img = shop.AddComponent<Image>();
            img.sprite = shopSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
        else
        {
            // フォールバック: プロシージャル描画
            var wall = FacePart("ShopWall", shop.transform, Vector2.zero, new Vector2(DISPLAY_TILE * 1.8f, DISPLAY_TILE * 1.6f));
            wall.AddComponent<Image>().color = new Color(0.3f, 0.15f, 0.35f);

            var roof = FacePart("ShopRoof", shop.transform, new Vector2(0, DISPLAY_TILE * 0.6f), new Vector2(DISPLAY_TILE * 2.0f, DISPLAY_TILE * 0.5f));
            roof.AddComponent<Image>().color = new Color(0.4f, 0.1f, 0.45f);

            var door = FacePart("ShopDoor", shop.transform, new Vector2(0, -DISPLAY_TILE * 0.4f), new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.6f));
            door.AddComponent<Image>().color = new Color(0.2f, 0.1f, 0.25f);
        }

        // 看板
        var signObj = new GameObject("ShopSign");
        signObj.transform.SetParent(tilesContainer.transform, false);
        var signRect2 = signObj.AddComponent<RectTransform>();
        float signX = (shopTileCX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float signY = (minSY - 1f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        signRect2.anchoredPosition = new Vector2(signX, signY);
        signRect2.sizeDelta = new Vector2(DISPLAY_TILE * 1.8f, DISPLAY_TILE * 0.45f);

        var signBg2 = signObj.AddComponent<Image>();
        signBg2.color = new Color(0.15f, 0.05f, 0.2f, 0.9f);
        signBg2.raycastTarget = false;

        var signTextObj = new GameObject("ShopSignText");
        signTextObj.transform.SetParent(signObj.transform, false);
        var signTextRect = signTextObj.AddComponent<RectTransform>();
        signTextRect.anchorMin = Vector2.zero;
        signTextRect.anchorMax = Vector2.one;
        signTextRect.offsetMin = Vector2.zero;
        signTextRect.offsetMax = Vector2.zero;

        var signText = signTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(signText);
        signText.text = Localization.Get("shop_title");
        signText.fontSize = 16;
        signText.alignment = TextAlignmentOptions.Center;
        signText.color = Color.white;
        signText.fontStyle = FontStyles.Bold;
        signText.raycastTarget = false;
    }

    void CreateFatherHouseOverlay()
    {
        if (tilesContainer == null) return;

        // 家の2×2タイル中心（x=2-3, y=33-34）
        float houseX = (2.5f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float houseY = (33.5f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var house = new GameObject("FatherHouse");
        house.transform.SetParent(tilesContainer.transform, false);
        var houseRect = house.AddComponent<RectTransform>();
        houseRect.anchoredPosition = new Vector2(houseX, houseY);
        houseRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.5f, DISPLAY_TILE * 2.5f);

        // スプライト画像を試みる
        Sprite houseSprite = Resources.Load<Sprite>("Map/S_Home");
        if (houseSprite != null)
        {
            var img = house.AddComponent<Image>();
            img.sprite = houseSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
        else
        {
            // フォールバック: プロシージャル描画
            // 壁（木造の温かい色）
            var wall = FacePart("Wall", house.transform, new Vector2(0, -DISPLAY_TILE * 0.1f),
                new Vector2(DISPLAY_TILE * 2.2f, DISPLAY_TILE * 1.6f));
            wall.AddComponent<Image>().color = new Color(0.55f, 0.38f, 0.22f);

            // 屋根（赤茶色）
            var roof = FacePart("Roof", house.transform, new Vector2(0, DISPLAY_TILE * 0.65f),
                new Vector2(DISPLAY_TILE * 2.6f, DISPLAY_TILE * 0.7f));
            roof.AddComponent<Image>().color = new Color(0.6f, 0.18f, 0.12f);

            // 屋根の先端
            var roofTop = FacePart("RoofTop", house.transform, new Vector2(0, DISPLAY_TILE * 1.0f),
                new Vector2(DISPLAY_TILE * 1.6f, DISPLAY_TILE * 0.4f));
            roofTop.AddComponent<Image>().color = new Color(0.65f, 0.2f, 0.13f);

            // 窓（左）— 温かい光
            var winL = FacePart("WinL", house.transform, new Vector2(-DISPLAY_TILE * 0.45f, DISPLAY_TILE * 0.05f),
                new Vector2(DISPLAY_TILE * 0.4f, DISPLAY_TILE * 0.4f));
            winL.AddComponent<Image>().color = new Color(1f, 0.85f, 0.4f, 0.85f);

            // 窓（右）
            var winR = FacePart("WinR", house.transform, new Vector2(DISPLAY_TILE * 0.45f, DISPLAY_TILE * 0.05f),
                new Vector2(DISPLAY_TILE * 0.4f, DISPLAY_TILE * 0.4f));
            winR.AddComponent<Image>().color = new Color(1f, 0.85f, 0.4f, 0.85f);

            // ドア（中央下部）
            var door = FacePart("Door", house.transform, new Vector2(0, -DISPLAY_TILE * 0.55f),
                new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.65f));
            door.AddComponent<Image>().color = new Color(0.35f, 0.2f, 0.1f);

            // ドアノブ
            var knob = FacePart("Knob", house.transform, new Vector2(DISPLAY_TILE * 0.1f, -DISPLAY_TILE * 0.55f),
                new Vector2(10, 10));
            knob.AddComponent<Image>().color = new Color(0.85f, 0.7f, 0.25f);
        }

        // 看板（「実家」）
        var signObj = new GameObject("FatherSign");
        signObj.transform.SetParent(tilesContainer.transform, false);
        var signRect = signObj.AddComponent<RectTransform>();
        float signX = houseX;
        float signY = (32f - mapHeight / 2f + 0.5f) * DISPLAY_TILE - DISPLAY_TILE * 0.1f;
        signRect.anchoredPosition = new Vector2(signX, signY);
        signRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.0f, DISPLAY_TILE * 0.5f);

        var signBg = signObj.AddComponent<Image>();
        signBg.color = new Color(0.3f, 0.18f, 0.08f, 0.9f);
        signBg.raycastTarget = false;

        var signTextObj = new GameObject("SignText");
        signTextObj.transform.SetParent(signObj.transform, false);
        var signTextRect = signTextObj.AddComponent<RectTransform>();
        signTextRect.anchorMin = Vector2.zero;
        signTextRect.anchorMax = Vector2.one;
        signTextRect.offsetMin = Vector2.zero;
        signTextRect.offsetMax = Vector2.zero;

        var signText = signTextObj.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(signText);
        signText.text = "実家";
        signText.fontSize = 18;
        signText.alignment = TextAlignmentOptions.Center;
        signText.color = new Color(1f, 0.9f, 0.7f);
        signText.fontStyle = FontStyles.Bold;
        signText.raycastTarget = false;
    }

    void CreateJukuSign()
    {
        if (tilesContainer == null) return;

        // 塾の2×2タイル中心（x=9-10, y=26-27）
        float houseX = (9.5f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float houseY = (26.5f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var jukuObj = new GameObject("JukuOverlay");
        jukuObj.transform.SetParent(tilesContainer.transform, false);
        var jukuRect = jukuObj.AddComponent<RectTransform>();
        jukuRect.anchoredPosition = new Vector2(houseX, houseY);
        jukuRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.5f, DISPLAY_TILE * 2.5f);

        // zyuku.png 画像を使用
        Sprite jukuSprite = Resources.Load<Sprite>("Map/zyuku");
        if (jukuSprite != null)
        {
            var img = jukuObj.AddComponent<Image>();
            img.sprite = jukuSprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        // 看板テキスト（入口タイル付近）
        float signX = houseX;
        float signY = (25f - mapHeight / 2f + 0.5f) * DISPLAY_TILE - DISPLAY_TILE * 0.1f;

        var signObj = new GameObject("JukuSignText");
        signObj.transform.SetParent(tilesContainer.transform, false);
        var signRect = signObj.AddComponent<RectTransform>();
        signRect.anchoredPosition = new Vector2(signX, signY);
        signRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.0f, DISPLAY_TILE * 0.5f);

        var signBg = signObj.AddComponent<Image>();
        signBg.color = new Color(0.1f, 0.3f, 0.15f, 0.9f);
        signBg.raycastTarget = false;

        var signTextChild = new GameObject("SignLabel");
        signTextChild.transform.SetParent(signObj.transform, false);
        var signTextRect = signTextChild.AddComponent<RectTransform>();
        signTextRect.anchorMin = Vector2.zero;
        signTextRect.anchorMax = Vector2.one;
        signTextRect.offsetMin = Vector2.zero;
        signTextRect.offsetMax = Vector2.zero;

        var signText = signTextChild.AddComponent<TextMeshProUGUI>();
        FontHelper.Apply(signText);
        signText.text = "じゅく";
        signText.fontSize = 18;
        signText.alignment = TextAlignmentOptions.Center;
        signText.color = new Color(1f, 1f, 0.85f);
        signText.fontStyle = FontStyles.Bold;
        signText.raycastTarget = false;
    }

    // CreateTile: area!=0 専用（area==0 は CreateDQTileElement で UI Toolkit 描画）
    void CreateTile(int x, int y, int tileType)
    {
        if (tilesContainer == null) return;

        var tileObj = new GameObject($"Tile_{x}_{y}");
        tileObj.transform.SetParent(tilesContainer.transform, false);

        var rect = tileObj.AddComponent<RectTransform>();
        float posX = (x - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (y - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var img = tileObj.AddComponent<UnityEngine.UI.Image>();
        img.raycastTarget = false;

        int areaForTile = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        if (areaForTile == 3 && tileType == TILE_GRASS)
        {
            // 背景1枚絵を透かすため透明に
            img.color = Color.clear;
        }
        else if (areaForTile == 1 && tileType == TILE_GRASS && poisonRoadSprite != null)
        {
            img.sprite = poisonRoadSprite;
            img.color = Color.white;
        }
        else if (tileType == TILE_GRASS)
        {
            CreateModernGrassTile(tileObj.transform, img, x, y);
        }
        else if (areaForTile == 3 && tileType == TILE_WATER)
        {
            // 背景1枚絵+湖オーバーレイを透かすため透明に
            img.color = Color.clear;
        }
        else if (areaForTile == 1 && tileType == TILE_WATER && poisonLakeSprite != null)
        {
            img.sprite = poisonLakeSprite;
            img.color = Color.white;
        }
        else if (tileType == TILE_CHAMPAGNE)
        {
            // 背景: フェルト
            if (poisonFeltSprite != null)
            {
                img.sprite = poisonFeltSprite;
                img.color = Color.white;
            }
            // 前面: シャンパン（引き伸ばしなし）
            if (champagneSprite != null)
            {
                var overlay = new GameObject("Champagne");
                overlay.transform.SetParent(tileObj.transform, false);
                var oRect = overlay.AddComponent<RectTransform>();
                oRect.anchoredPosition = Vector2.zero;
                oRect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);
                var oImg = overlay.AddComponent<Image>();
                oImg.sprite = champagneSprite;
                oImg.preserveAspect = true;
                oImg.raycastTarget = false;
            }
        }
        else if (areaForTile == 1 && tileType == TILE_FENCE && poisonFenceSprite != null)
        {
            img.sprite = poisonFenceSprite;
            img.color = Color.white;
        }
        else if (areaForTile == 3 && tileType == TILE_TREE && thirdFenceSprite != null)
        {
            img.sprite = thirdFenceSprite;
            img.color = Color.white;
        }
        else if (areaForTile == 3 && tileType == TILE_ROCK && thirdWoodtowerSprite != null)
        {
            img.color = Color.clear; // 背景を透かす
            var tower = new GameObject("Woodtower");
            tower.transform.SetParent(tileObj.transform, false);
            var tRect = tower.AddComponent<RectTransform>();
            tRect.anchoredPosition = Vector2.zero;
            tRect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);
            var tImg = tower.AddComponent<Image>();
            tImg.sprite = thirdWoodtowerSprite;
            tImg.preserveAspect = true;
            tImg.raycastTarget = false;
        }
        else if (areaForTile == 3 && (tileType == TILE_PATH || tileType == TILE_DARK_DIRT) && thirdRoadSprite != null)
        {
            img.sprite = thirdRoadSprite;
            img.color = Color.white;
        }
        else if (areaForTile == 1 && tileType == TILE_DARK_DIRT && poisonFeltSprite != null)
        {
            img.sprite = poisonFeltSprite;
            img.color = Color.white;
        }
        else if (tileSprites.ContainsKey(tileType) && tileSprites[tileType] != null)
        {
            img.sprite = tileSprites[tileType];
            if (areaForTile == 1 && tileType == TILE_WATER)
                img.color = new Color(1f, 0.4f, 0.3f);
            else
                img.color = Color.white;
        }
        else if (tileType == TILE_SHOP_FLOOR)
        {
            if (toyShopBgSprite != null)
            {
                img.sprite = toyShopBgSprite;
                img.color = Color.white;
            }
            else
            {
                if ((x + y) % 2 == 0)
                    img.color = new Color(0.30f, 0.22f, 0.35f);
                else
                    img.color = new Color(0.24f, 0.16f, 0.28f);
            }
        }
        else if (tileType == TILE_JUKU_FLOOR)
        {
            // 塾: ストライプ模様の床
            if (x % 2 == 0)
                img.color = new Color(0.85f, 0.80f, 0.70f);
            else
                img.color = new Color(0.80f, 0.75f, 0.65f);
        }
        else if (tileType == TILE_FATHER_FLOOR && nobilityTileTex != null)
        {
            var sprite = Sprite.Create(nobilityTileTex,
                new Rect(0, 0, nobilityTileTex.width, nobilityTileTex.height),
                new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            img.color = Color.white;
        }
        else if (tileType == TILE_FATHER_WALL && x == 0 && y <= 2 && cornerWallTex != null)
        {
            // コーナーは CreateFatherHouseFurniture でスプライト配置するので透明にする
            img.color = new Color(0, 0, 0, 0);
        }
        else if (tileType == TILE_FATHER_WALL && x == mapWidth - 1 && y <= 2 && cornerWallTex != null)
        {
            img.color = new Color(0, 0, 0, 0);
        }
        else if (tileType == TILE_FATHER_WALL && x == 0 && y >= mapHeight - 3 && cornerWallTex != null)
        {
            img.color = new Color(0, 0, 0, 0);
        }
        else if (tileType == TILE_FATHER_WALL && x == mapWidth - 1 && y >= mapHeight - 3 && cornerWallTex != null)
        {
            img.color = new Color(0, 0, 0, 0);
        }
        else if (tileType == TILE_FATHER_WALL && x == 0 && leftWallTex != null)
        {
            var sprite = Sprite.Create(leftWallTex,
                new Rect(0, 0, leftWallTex.width, leftWallTex.height),
                new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            img.color = Color.white;
        }
        else if (tileType == TILE_FATHER_WALL && x == mapWidth - 1 && rightWallTex != null)
        {
            var sprite = Sprite.Create(rightWallTex,
                new Rect(0, 0, rightWallTex.width, rightWallTex.height),
                new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            img.color = Color.white;
        }
        else if (tileType == TILE_FATHER_WALL && leftWallTex != null)
        {
            // 上壁・下壁: left_wallを時計回り90度回転
            var sprite = Sprite.Create(leftWallTex,
                new Rect(0, 0, leftWallTex.width, leftWallTex.height),
                new Vector2(0.5f, 0.5f));
            img.sprite = sprite;
            img.color = Color.white;
            rect.localRotation = Quaternion.Euler(0, 0, -90);
        }
        else if (tileType == TILE_BOSS_MANSION || tileType == TILE_WEAPON_SHOP)
        {
            // 建物タイルの背景: 周囲の草タイルと同じ見た目にする
            if (areaForTile == 3)
            {
                // 背景1枚絵を透かすため透明に
                img.color = Color.clear;
            }
            else if (areaForTile == 1 && poisonRoadSprite != null)
            {
                img.sprite = poisonRoadSprite;
                img.color = Color.white;
            }
            else
            {
                CreateModernGrassTile(tileObj.transform, img, x, y);
            }
        }
        else if (tileType == TILE_SHOP_WALL && toyShopBallSprite != null)
        {
            img.sprite = toyShopBallSprite;
            img.color = Color.white;
        }
        else if (areaForTile == 8 && tileType == TILE_CRYSTAL)
        {
            // タイルサイズを75%に縮小（隙間から宇宙背景がしっかり見える）
            float crystalSize = DISPLAY_TILE * 0.75f;
            rect.sizeDelta = new Vector2(crystalSize, crystalSize);

            // Crystal_Tile.png でタイルを描画
            Sprite crystalSpr = Resources.Load<Sprite>("Map/4th/Crystal_Tile");
            if (crystalSpr == null)
            {
                var cTex = Resources.Load<Texture2D>("Map/4th/Crystal_Tile");
                if (cTex != null && cTex.isReadable)
                    crystalSpr = Sprite.Create(cTex, new Rect(0, 0, cTex.width, cTex.height), new Vector2(0.5f, 0.5f));
            }

            float maxAlpha;
            if (crystalSpr != null)
            {
                img.sprite = crystalSpr;
                maxAlpha = 0.7f;
            }
            else
            {
                maxAlpha = 0.5f;
            }

            if (crystalSpr != null)
                img.color = new Color(0.9f, 0.95f, 1f, maxAlpha);
            else
                img.color = new Color(0.3f, 0.3f, 0.5f, maxAlpha);

            // 浮遊タイルマップに登録（最初から全て表示）
            var tile = new StellaFloatingTile
            {
                rt = rect,
                img = img,
                maxAlpha = maxAlpha,
                currentAlpha = maxAlpha,
                goalAlpha = maxAlpha,
                baseY = rect.anchoredPosition.y,
                phase = Random.Range(0f, Mathf.PI * 2f),
                speed = Random.Range(0.8f, 1.6f),
                tileX = x,
                tileY = y,
                discovered = true,
                visited = false
            };
            stellaTileMap[(x, y)] = tile;

            // 音叉タイルのラベルは不要（Tuning_Fork.png ビジュアルが別途作成される）
        }
        else if (areaForTile == 8 && tileType == TILE_CRYSTAL_WALL)
        {
            img.color = Color.clear; // 透明（宇宙背景を透かす）
        }
        else
        {
            img.color = GetTileColor(tileType);
        }
    }

    void CreateModernGrassTile(Transform parent, Image baseImg, int tileX, int tileY)
    {
        // タイルごとに固定シードでランダム感を出す（再現性あり）
        Random.State oldState = Random.state;
        Random.InitState(tileX * 100 + tileY);

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        // Lawn_Road テクスチャがあれば使用
        if (lawnTex != null && area != 1 && area != 3)
        {
            var sprite = Sprite.Create(lawnTex, new Rect(0, 0, lawnTex.width, lawnTex.height), new Vector2(0.5f, 0.5f));
            baseImg.sprite = sprite;
            baseImg.color = Color.white;
            baseImg.type = Image.Type.Simple;
            baseImg.preserveAspect = false;
            Random.state = oldState;
            return;
        }

        // ベースカラー: 深めの緑にタイルごとの微妙な色ムラ
        float hueShift = Random.Range(-0.02f, 0.02f);
        float brightShift = Random.Range(-0.04f, 0.04f);
        Color baseColor;
        if (area == 3)
            baseColor = new Color(0.18f + hueShift, 0.20f + brightShift, 0.25f + hueShift); // 暗い紫がかった緑
        else if (area == 1)
            baseColor = new Color(0.22f + hueShift, 0.15f + brightShift, 0.28f + hueShift); // 暗い紫灰
        else
            baseColor = new Color(0.28f + hueShift, 0.62f + brightShift, 0.25f + hueShift);
        baseImg.color = baseColor;

        // グラデーションオーバーレイ（上部を少し明るく）
        var gradTop = FacePart("GradTop", parent, new Vector2(0, DISPLAY_TILE * 0.2f),
            new Vector2(DISPLAY_TILE, DISPLAY_TILE * 0.5f));
        var gradImg = gradTop.AddComponent<Image>();
        gradImg.color = (area == 3)
            ? new Color(0.25f, 0.22f, 0.38f, 0.25f)
            : (area == 1)
            ? new Color(0.3f, 0.2f, 0.4f, 0.25f)
            : new Color(0.4f, 0.75f, 0.35f, 0.25f);
        gradImg.raycastTarget = false;

        // 下部の影
        var gradBot = FacePart("GradBot", parent, new Vector2(0, -DISPLAY_TILE * 0.25f),
            new Vector2(DISPLAY_TILE, DISPLAY_TILE * 0.4f));
        var gradBotImg = gradBot.AddComponent<Image>();
        gradBotImg.color = (area == 3)
            ? new Color(0.08f, 0.04f, 0.12f, 0.15f)
            : (area == 1)
            ? new Color(0.1f, 0.05f, 0.15f, 0.15f)
            : new Color(0.1f, 0.3f, 0.1f, 0.15f);
        gradBotImg.raycastTarget = false;

        // 草のブレード（小さな明るい縦線を3〜5本ランダムに配置）
        int bladeCount = Random.Range(3, 6);
        for (int i = 0; i < bladeCount; i++)
        {
            float bx = Random.Range(-DISPLAY_TILE * 0.35f, DISPLAY_TILE * 0.35f);
            float by = Random.Range(-DISPLAY_TILE * 0.1f, DISPLAY_TILE * 0.2f);
            float bw = Random.Range(2f, 4f);
            float bh = Random.Range(8f, 16f);
            float angle = Random.Range(-15f, 15f);

            var blade = FacePart($"Blade{i}", parent, new Vector2(bx, by), new Vector2(bw, bh));
            blade.transform.localRotation = Quaternion.Euler(0, 0, angle);
            var bladeImg = blade.AddComponent<Image>();
            float bladeAlpha = Random.Range(0.15f, 0.35f);
            bladeImg.color = (area == 3)
                ? new Color(0.28f, 0.30f, 0.42f, bladeAlpha)  // 暗い紫がかった草
                : (area == 1)
                ? new Color(0.35f, 0.2f, 0.45f, bladeAlpha)  // 暗い紫の草
                : new Color(0.45f, 0.82f, 0.38f, bladeAlpha);
            bladeImg.raycastTarget = false;
        }

        // アクセントドット（花や小石）をまばらに
        if (Random.Range(0f, 1f) < 0.3f)
        {
            float dx = Random.Range(-DISPLAY_TILE * 0.3f, DISPLAY_TILE * 0.3f);
            float dy = Random.Range(-DISPLAY_TILE * 0.3f, DISPLAY_TILE * 0.1f);
            float dotSize = Random.Range(3f, 6f);
            var dot = FacePart("Accent", parent, new Vector2(dx, dy), new Vector2(dotSize, dotSize));
            var dotImg = dot.AddComponent<Image>();
            // ランダムで白い花 or 黄色い花
            Color[] accentColors = {
                new Color(1f, 1f, 0.9f, 0.5f),
                new Color(1f, 0.9f, 0.4f, 0.5f),
                new Color(0.9f, 0.8f, 1f, 0.4f)
            };
            dotImg.color = accentColors[Random.Range(0, accentColors.Length)];
            dotImg.raycastTarget = false;
        }

        Random.state = oldState;
    }

    // ========== DQ風オーバーレイシステム（area==0 専用） ==========

    void CreateVillageOverlays()
    {
        if (tilesContainer == null || mapData == null) return;
        // 装飾オブジェクトなし — 実家・塾は専用オーバーレイで描画
    }

    void CreateSpriteOverlay(int tileX, int tileY, Sprite sprite, float scaleX, float scaleY, float offsetY)
    {
        if (tilesContainer == null || sprite == null) return;

        var overlayObj = new GameObject($"Overlay_{tileX}_{tileY}");
        overlayObj.transform.SetParent(tilesContainer.transform, false);

        var rect = overlayObj.AddComponent<RectTransform>();
        float posX = (tileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (tileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE + offsetY;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(sprite.rect.width * scaleX, sprite.rect.height * scaleY);

        var img = overlayObj.AddComponent<Image>();
        img.sprite = sprite;
        img.color = Color.white;
        img.raycastTarget = false;
        img.preserveAspect = true;
    }

    void CreateHouseOverlays()
    {
        // 家のタイプと位置を検出（2×2の左下タイルを基準）
        HashSet<string> processed = new HashSet<string>();

        // 屋根スプライト (Roofs.png 400×400)
        // 茶屋根(赤家用): 左上の茶色い屋根
        Sprite roofBrown = CreatePCSprite(pcRoofsTex, 0, 0, 128, 96);
        // 緑屋根(緑家用): 中央の緑屋根
        Sprite roofGreen = CreatePCSprite(pcRoofsTex, 144, 0, 112, 96);
        // 青灰屋根(青家用): 右の青灰屋根
        Sprite roofBlue = CreatePCSprite(pcRoofsTex, 280, 0, 112, 96);

        // 壁スプライト (Walls.png 672×800): 丸太壁テクスチャ
        Sprite wallLog = CreatePCSprite(pcWallsTex, 0, 0, 96, 64);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int tile = mapData[x, y];
                if (tile != TILE_HOUSE_RED && tile != TILE_HOUSE_GREEN && tile != TILE_HOUSE_BLUE) continue;

                string key = $"{x},{y}";
                if (processed.Contains(key)) continue;

                // 2×2ブロックの左下を探す
                int bx = x, by = y;
                // この位置が2×2ブロックの左下かチェック
                if (x + 1 < mapWidth && y + 1 < mapHeight &&
                    mapData[x + 1, y] == tile && mapData[x, y + 1] == tile && mapData[x + 1, y + 1] == tile)
                {
                    // (x,y) が左下
                    processed.Add($"{x},{y}");
                    processed.Add($"{x + 1},{y}");
                    processed.Add($"{x},{y + 1}");
                    processed.Add($"{x + 1},{y + 1}");
                }
                else
                {
                    processed.Add(key);
                    continue;
                }

                // 家の中心座標
                float centerX = (bx + 0.5f - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
                float centerY = (by + 0.5f - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

                // 壁（下半分）
                if (wallLog != null)
                {
                    var wallObj = new GameObject($"HouseWall_{bx}_{by}");
                    wallObj.transform.SetParent(tilesContainer.transform, false);
                    var wallRect = wallObj.AddComponent<RectTransform>();
                    wallRect.anchoredPosition = new Vector2(centerX, centerY - DISPLAY_TILE * 0.15f);
                    wallRect.sizeDelta = new Vector2(DISPLAY_TILE * 1.9f, DISPLAY_TILE * 1.2f);
                    var wallImg = wallObj.AddComponent<Image>();
                    wallImg.sprite = wallLog;
                    wallImg.color = Color.white;
                    wallImg.raycastTarget = false;
                }

                // 屋根（上半分）
                Sprite roof = (tile == TILE_HOUSE_RED) ? roofBrown :
                              (tile == TILE_HOUSE_GREEN) ? roofGreen : roofBlue;
                if (roof != null)
                {
                    var roofObj = new GameObject($"HouseRoof_{bx}_{by}");
                    roofObj.transform.SetParent(tilesContainer.transform, false);
                    var roofRect = roofObj.AddComponent<RectTransform>();
                    roofRect.anchoredPosition = new Vector2(centerX, centerY + DISPLAY_TILE * 0.55f);
                    roofRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.2f, DISPLAY_TILE * 1.1f);
                    var roofImg = roofObj.AddComponent<Image>();
                    roofImg.sprite = roof;
                    roofImg.color = Color.white;
                    roofImg.raycastTarget = false;
                    roofImg.preserveAspect = true;
                }

                // ドア（UI Toolkit — USS .house-door でスタイル定義）
                // UI Toolkit座標: uGUI中心座標 → 左上座標に変換
                float doorGridLeft = (bx + 0.5f) * DISPLAY_TILE;
                float doorGridTop = (mapHeight - 1 - by - 0.5f) * DISPLAY_TILE;
                CreateDQHouseDoorElement(doorGridLeft, doorGridTop);
            }
        }
    }

    Color GetTileColor(int tileType)
    {
        switch (tileType)
        {
            case TILE_GRASS: return new Color(0.3f, 0.7f, 0.3f);
            case TILE_WATER: return new Color(0.2f, 0.5f, 0.9f);
            case TILE_DIRT: return new Color(0.6f, 0.4f, 0.2f);
            case TILE_PATH: return new Color(0.8f, 0.7f, 0.5f);
            case TILE_TREE: return new Color(0.1f, 0.4f, 0.1f);
            case TILE_HOUSE_RED: return new Color(0.7f, 0.2f, 0.2f);
            case TILE_HOUSE_GREEN: return new Color(0.2f, 0.6f, 0.3f);
            case TILE_HOUSE_BLUE: return new Color(0.2f, 0.3f, 0.7f);
            case TILE_ROCK: return new Color(0.5f, 0.5f, 0.5f);
            case TILE_FENCE: return new Color(0.55f, 0.35f, 0.2f);
            case TILE_FLOWER: return new Color(1f, 0.8f, 0.3f);
            case TILE_BOSS_MANSION: return new Color(0.25f, 0.08f, 0.12f);
            case TILE_BOSS_GATE: return new Color(0.4f, 0.15f, 0.15f);
            case TILE_DARK_DIRT: return new Color(0.3f, 0.22f, 0.15f);
            case TILE_MANSION_FLOOR: return new Color(0.15f, 0.12f, 0.12f);
            case TILE_MANSION_WALL: return new Color(0.25f, 0.15f, 0.1f);
            case TILE_MERCENARY_A: return new Color(0.4f, 0.1f, 0.1f);
            case TILE_MERCENARY_B: return new Color(0.4f, 0.1f, 0.1f);
            case TILE_BOSS_DOOR: return new Color(0.6f, 0.5f, 0.1f);
            case TILE_MANSION_EXIT: return new Color(0.1f, 0.3f, 0.3f);
            case TILE_AREA_EXIT: return new Color(0.6f, 0.5f, 0.3f);
            case TILE_FATHER_HOUSE: return new Color(0.55f, 0.38f, 0.22f);
            case TILE_FATHER_FLOOR: return new Color(0.93f, 0.90f, 0.85f); // 大理石の白い床
            case TILE_FATHER_WALL: return new Color(0.72f, 0.58f, 0.32f);  // 金色の壁
            case TILE_FATHER_EXIT: return new Color(0.55f, 0.45f, 0.25f);  // 金色の出口
            case TILE_WEAPON_SHOP: return new Color(0.25f, 0.12f, 0.3f);
            case TILE_JUKU: return new Color(0.2f, 0.45f, 0.25f);
            case TILE_SHOP_FLOOR: return new Color(0.30f, 0.22f, 0.35f);
            case TILE_SHOP_WALL: return new Color(0.18f, 0.08f, 0.22f);
            case TILE_JUKU_FLOOR: return new Color(0.85f, 0.80f, 0.70f);
            case TILE_JUKU_WALL: return new Color(0.65f, 0.60f, 0.55f);
            case TILE_CHAMPAGNE: return new Color(0.95f, 0.85f, 0.55f); // シャンパンゴールド
            case TILE_CRYSTAL: return new Color(0.15f, 0.18f, 0.35f, 0.3f);       // クリスタル床（半透明）
            case TILE_CRYSTAL_WALL: return new Color(0.02f, 0.01f, 0.06f, 0f);    // 宇宙壁（透明）
            default: return Color.magenta;
        }
    }

    void CreatePlayer()
    {
        if (tilesContainer == null) return;

        playerObj = new GameObject("Player");
        playerObj.transform.SetParent(tilesContainer.transform, false);

        playerRect = playerObj.AddComponent<RectTransform>();
        UpdatePlayerPosition();
        playerRect.sizeDelta = new Vector2(DISPLAY_TILE - 4, DISPLAY_TILE - 4);

        playerImage = playerObj.AddComponent<Image>();
        playerImage.raycastTarget = false;

        LoadPlayerSprites();
        ApplyPlayerSprite();

        // ステラ・オリジン: オーラ削除済み

        playerObj.transform.SetAsLastSibling();
        targetPosition = playerRect.anchoredPosition;

        // 相思相愛後: かぐやフォロワー作成
        CreateKaguyaFollower();
    }

    void CreateKaguyaFollower()
    {
        if (tilesContainer == null) return;
        if (DataCarrier.Instance == null || !DataCarrier.Instance.kaguyaLover) return;
        if (kaguyaFollowerObj != null) return; // 既に作成済み

        kaguyaFollowerObj = new GameObject("KaguyaFollower");
        kaguyaFollowerObj.transform.SetParent(tilesContainer.transform, false);

        kaguyaFollowerRect = kaguyaFollowerObj.AddComponent<RectTransform>();
        kaguyaFollowerRect.sizeDelta = new Vector2((DISPLAY_TILE - 4) * 0.5f, (DISPLAY_TILE - 4) * 0.5f);

        // プレイヤーの1タイル後ろに配置
        int followerX = playerTileX;
        int followerY = playerTileY - 1;
        if (followerY < 0) followerY = playerTileY;
        float fx = (followerX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float fy = (followerY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        kaguyaFollowerRect.anchoredPosition = new Vector2(fx, fy);
        kaguyaFollowerTarget = kaguyaFollowerRect.anchoredPosition;

        var followerImg = kaguyaFollowerObj.AddComponent<Image>();
        var kSpr = Resources.Load<Sprite>("MapCharacters/heroine/kaguya3");
        if (kSpr != null)
        {
            followerImg.sprite = kSpr;
            followerImg.preserveAspect = true;
            followerImg.color = Color.white;
        }
        else
        {
            followerImg.color = new Color(0, 0, 0, 0);
        }
        followerImg.raycastTarget = false;

        // ハートを頭上に
        var heartObj = new GameObject("FollowerHeart");
        heartObj.transform.SetParent(kaguyaFollowerObj.transform, false);
        var hRect = heartObj.AddComponent<RectTransform>();
        hRect.anchoredPosition = new Vector2(0, 30);
        hRect.sizeDelta = new Vector2(20, 20);
        var heartTmp = heartObj.AddComponent<TMPro.TextMeshProUGUI>();
        FontHelper.Apply(heartTmp);
        heartTmp.text = "\u2764";
        heartTmp.fontSize = 12;
        heartTmp.alignment = TMPro.TextAlignmentOptions.Center;
        heartTmp.color = new Color(1f, 0.4f, 0.6f, 0.8f);
        heartTmp.raycastTarget = false;

        // プレイヤーより手前に表示（後ろ歩きなのでプレイヤーの下レイヤー）
        kaguyaFollowerObj.transform.SetSiblingIndex(playerObj.transform.GetSiblingIndex());
    }

    void RebuildPlayerSprite()
    {
        if (playerImage == null) return;
        int dir = playerDirection; // 0=south,1=north,2=west,3=east
        if (playerIdleSprites[dir] != null)
        {
            playerImage.sprite = playerIdleSprites[dir];
        }
        walkFrameIndex = 0;
        walkFrameTimer = 0f;
    }

    void LoadPlayerSprites()
    {
        string[] dirNames = { "south", "north", "west", "east" };
        for (int i = 0; i < 4; i++)
        {
            playerIdleSprites[i] = Resources.Load<Sprite>("Character/cute_baby/rotations/" + dirNames[i]);
            var frames = new List<Sprite>();
            for (int f = 0; f < 6; f++)
            {
                var spr = Resources.Load<Sprite>(
                    "Character/cute_baby/animations/walk/" + dirNames[i] + "/frame_00" + f);
                if (spr != null) frames.Add(spr);
            }
            playerWalkSprites[i] = frames.ToArray();
        }
    }

    void ApplyPlayerSprite()
    {
        if (playerImage == null) return;
        int dir = playerDirection;
        playerImage.enabled = true;
        playerImage.preserveAspect = true;
        playerImage.color = Color.white;
        if (playerIdleSprites[dir] != null)
            playerImage.sprite = playerIdleSprites[dir];
    }

    GameObject FacePart(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var r = obj.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = pos;
        r.sizeDelta = size;
        return obj;
    }

    void UpdatePlayerPosition()
    {
        if (playerRect == null) return;
        float posX = (playerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (playerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        playerRect.anchoredPosition = new Vector2(posX, posY);
        targetPosition = playerRect.anchoredPosition;
    }

    void CreateStatusUI()
    {
        if (overlayRoot == null) return;
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        var bar = new UIE.VisualElement();
        bar.AddToClassList("map-status-bar");
        bar.style.top = safeTop;
        // ステータスバーを非表示
        bar.style.display = UIE.DisplayStyle.None;
        overlayRoot.Add(bar);

        statusLabel = UIHelper.CreateLabel("", "map-status-text");
        statusLabel.enableRichText = true;
        bar.Add(statusLabel);

        UpdateStatusText();
    }

    void CreateMenuButton()
    {
        if (overlayRoot == null) return;

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        var btn = new UIE.Button();
        btn.AddToClassList("map-menu-btn");
        btn.focusable = false;

        if (area == 8)
        {
            // Area 8: Crystal_UI_Button.png を使用
            var crystalBtnSpr = Resources.Load<Sprite>("Map/4th/Crystal_UI_Button");
            if (crystalBtnSpr == null)
            {
                var crystalBtnTex = Resources.Load<Texture2D>("Map/4th/Crystal_UI_Button");
                if (crystalBtnTex != null && crystalBtnTex.isReadable)
                    crystalBtnSpr = Sprite.Create(crystalBtnTex, new Rect(0, 0, crystalBtnTex.width, crystalBtnTex.height), new Vector2(0.5f, 0.5f));
            }
            if (crystalBtnSpr != null)
                btn.style.backgroundImage = new UIE.StyleBackground(crystalBtnSpr);
            btn.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            btn.style.backgroundColor = new Color(0, 0, 0, 0);
            btn.style.borderTopWidth = 0;
            btn.style.borderBottomWidth = 0;
            btn.style.borderLeftWidth = 0;
            btn.style.borderRightWidth = 0;
            // サイズを少し大きく（クリスタルの視認性向上）
            btn.style.width = 180;
            btn.style.height = 180;

            // ラベル不要（ボタン画像にSTELLA ORIGINの文字が含まれる）
        }
        else
        {
            // 通常エリア: menu.png を背景に表示
            var menuSpr = Resources.Load<Sprite>("UI/menu");
            if (menuSpr != null)
                btn.style.backgroundImage = new UIE.StyleBackground(menuSpr);
            btn.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

            // 「メニュー」ラベル
            var menuLabel = UIHelper.CreateLabel("メニュー", "map-menu-label");
            UIHelper.ApplyFontBold(menuLabel);
            btn.Add(menuLabel);
        }

        btn.clicked += ToggleMenu;
        overlayRoot.Add(btn);
    }

    void CreateDebugResetButton()
    {
        if (overlayRoot == null) return;

        var btn = new UIE.Button();
        btn.text = "RESET";
        btn.style.position = UIE.Position.Absolute;
        btn.style.top = 60;
        btn.style.right = 20;
        btn.style.width = 100;
        btn.style.height = 50;
        btn.style.fontSize = 18;
        btn.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        btn.style.color = Color.white;
        btn.style.borderTopLeftRadius = 12;
        btn.style.borderTopRightRadius = 12;
        btn.style.borderBottomLeftRadius = 12;
        btn.style.borderBottomRightRadius = 12;
        btn.focusable = false;
        UIHelper.ApplyFont(btn);

        btn.clicked += () =>
        {
            if (DataCarrier.Instance != null)
            {
                // イベント進行をリセット
                int oldProgress = DataCarrier.Instance.stellaOriginProgress;
                DataCarrier.Instance.stellaOriginProgress = 0;
                DataCarrier.Instance.stellaOriginTitle = "";
                // バフもリセット（重複付与を防ぐ）
                if ((oldProgress & 1) != 0) DataCarrier.Instance.babyDef -= 20; // DEFバフ取消
                if ((oldProgress & 2) != 0) DataCarrier.Instance.babyAtk -= 20; // ATKバフ取消
                DataCarrier.Instance.mapPlayerX = 12;
                DataCarrier.Instance.mapPlayerY = 19;
                DataCarrier.Instance.SaveData();
            }
            // ローカル状態もクリア
            stellaGateOpened = false;
            stellaGateOpenAnimPlaying = false;
            stellaGuardianDialogueActive = false;
            stellaCradleActive = false;
            menuOpen = false;
            // シーンリロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        };

        overlayRoot.Add(btn);
    }

    void UpdateStatusText()
    {
        if (statusLabel == null) return;

        string babyName = DataCarrier.Instance?.babyName ?? "???";
        int age = DataCarrier.Instance?.babyAge ?? 0;
        int maxHp = DataCarrier.Instance?.babyHp ?? 100;
        int atk = DataCarrier.Instance?.babyAtk ?? 10;
        int def = DataCarrier.Instance?.babyDef ?? 5;
        bool isGod = DataCarrier.Instance != null && DataCarrier.Instance.isGodBaby;
        int currentHp = DataCarrier.Instance != null ? DataCarrier.Instance.babyCurrentHp : -1;
        if (currentHp < 0) currentHp = maxHp;
        int poisonTurns = DataCarrier.Instance != null ? DataCarrier.Instance.babyPoisonTurns : 0;

        int milkAmt = DataCarrier.Instance != null ? DataCarrier.Instance.milk : 0;
        string nameColor = isGod ? "<color=#FFD700>" : "<color=#FFFFFF>";
        string godLabel = isGod ? " <color=#FFD700>STAR BABY</color>" : "";
        string poisonLabel = poisonTurns > 0 ? $" <color=#AA00FF>毒({poisonTurns})</color>" : "";

        statusLabel.text = $"{nameColor}{babyName}</color>  {Localization.GetAge(age)}{godLabel}\n" +
                           $"HP:<color=#00FF00>{currentHp}/{maxHp}</color>  ATK:<color=#FF6666>{atk}</color>  DEF:<color=#6699FF>{def}</color>  <color=#FFB6C1>🍼{milkAmt}</color>{poisonLabel}";
    }

    void Update()
    {
        if (elderBossBlockCooldown > 0f)
            elderBossBlockCooldown -= Time.deltaTime;

        var kb = Keyboard.current;
        bool escPressed = kb != null && kb.escapeKey.wasPressedThisFrame;

        // ESCでパネルを閉じる
        if (saveOverlayEl != null)
        {
            if (escPressed) CloseSavePanel();
            return;
        }
        if (statusDetailEl != null)
        {
            if (escPressed) CloseStatusPanel();
            return;
        }
        if (inventoryOverlayEl != null)
        {
            if (escPressed) CloseInventoryPanel();
            return;
        }
        if (shopOverlayEl != null)
        {
            if (escPressed) CloseWeaponShop();
            return;
        }

        if (menuOpen)
        {
            if (moveCtrl != null) moveCtrl.IsLocked = true;
            return;
        }

        // 移動コントローラ
        if (moveCtrl != null)
        {
            moveCtrl.IsLocked = false;
            moveCtrl.Tick();
        }

        UpdateMovement();
        UpdateCameraFollow();
        UpdateEnemySymbols();
        UpdateWaterfall();
        UpdateGoldenEggOldMan();
        UpdateKaguyaHearts();
        UpdateStellaOrigin();

        // インタラクト
        if (kb != null && kb.spaceKey.wasPressedThisFrame)
            Interact();

        // メニュー
        if (escPressed)
            ToggleMenu();
    }

    void TryMove(int dx, int dy)
    {
        // 方向更新（移動不可でも向きは変える）
        int newDir = (dy > 0) ? 1 : (dy < 0) ? 0 : (dx < 0) ? 2 : 3;
        if (newDir != playerDirection)
        {
            playerDirection = newDir;
            RebuildPlayerSprite();
        }

        int newX = playerTileX + dx;
        int newY = playerTileY + dy;

        if (newX < 0 || newX >= mapWidth || newY < 0 || newY >= mapHeight)
            return;

        // 父親の家に歩いて入ろうとした場合
        if (mapData[newX, newY] == TILE_FATHER_HOUSE)
        {
            StartCoroutine(EnterFatherHouse());
            return;
        }
        // 装備ショップに歩いて入ろうとした場合 → 武器屋内部（area 6）へ
        if (mapData[newX, newY] == TILE_WEAPON_SHOP)
        {
            StartCoroutine(EnterWeaponShop());
            return;
        }
        // 塾に歩いて入ろうとした場合
        if (mapData[newX, newY] == TILE_JUKU)
        {
            StartCoroutine(EnterJuku());
            return;
        }
        // ボスの門に歩いて入ろうとした場合
        if (mapData[newX, newY] == TILE_BOSS_GATE)
        {
            int areaForGate2 = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            // Area 0: シバ撃破済みならシバの家→次エリアへ
            if (areaForGate2 == 0 && DataCarrier.Instance != null && DataCarrier.Instance.HasDefeatedEnemy("青年のシバ"))
            {
                StartCoroutine(EnterShibaHouse());
                return;
            }
            if (IsBabyTooYoungForBoss())
            {
                if (elderBossBlockCooldown > 0f) return;
                StartCoroutine(ShowElderBossBlockDialogue());
                return;
            }
            if (areaForGate2 == 1)
            {
                StartCoroutine(EnterMansionArea());
                return;
            }
            StartCoroutine(EnterBossMansion());
            return;
        }
        // ゴージャス・ヴィレッジ: 館に歩いて入ろうとした場合 → 館内（area 2）へ
        // 小悪魔の街: 館に歩いて入ろうとした場合 → 109（area 4）へ
        if (mapData[newX, newY] == TILE_BOSS_MANSION)
        {
            int areaForGate = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            // Area 0: シバ撃破済みならシバの家→次エリアへ
            if (areaForGate == 0 && DataCarrier.Instance != null && DataCarrier.Instance.HasDefeatedEnemy("青年のシバ"))
            {
                StartCoroutine(EnterShibaHouse());
                return;
            }
            if (IsBabyTooYoungForBoss())
            {
                if (elderBossBlockCooldown > 0f) return;
                StartCoroutine(ShowElderBossBlockDialogue());
                return;
            }
            if (areaForGate == 3)
            {
                StartCoroutine(Enter109());
                return;
            }
            else if (areaForGate == 1)
            {
                StartCoroutine(EnterMansionArea());
                return;
            }
        }

        // 館内タイル処理
        int tileAtDest = mapData[newX, newY];
        if (tileAtDest == TILE_MERCENARY_A || tileAtDest == TILE_MERCENARY_B)
        {
            string mercName = tileAtDest == TILE_MERCENARY_A ? "デヴィル傭兵A" : "デヴィル傭兵B";
            if (DataCarrier.Instance != null && DataCarrier.Instance.HasDefeatedEnemy(mercName))
            {
                // 撃破済み: 通過可能
                mapData[newX, newY] = TILE_MANSION_FLOOR;
                walkable[newX, newY] = true;
            }
            else
            {
                ShowMessage(Localization.Get("map_mansion_mercenary_block"));
                StartCoroutine(StartFixedEncounter(mercName));
                return;
            }
        }
        if (tileAtDest == TILE_BOSS_DOOR)
        {
            int areaForDoor = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (areaForDoor == 4)
            {
                // 109: おともだちなし、直接ボス戦
                StartCoroutine(EnterMelodiasQueen());
                return;
            }
            bool mercADone = DataCarrier.Instance != null && DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵A");
            bool mercBDone = DataCarrier.Instance != null && DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵B");
            if (mercADone && mercBDone)
            {
                StartCoroutine(EnterDevilLadyBoss());
                return;
            }
            else
            {
                ShowMessage(Localization.Get("map_mansion_boss_locked"));
                return;
            }
        }
        if (tileAtDest == TILE_MANSION_EXIT)
        {
            int areaForExit = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (areaForExit == 4)
                Exit109();
            else
                ExitMansion();
            return;
        }
        if (tileAtDest == TILE_FATHER_EXIT)
        {
            int areaForFatherExit = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (areaForFatherExit == 7)
                ExitJuku();
            else if (areaForFatherExit == 6)
                ExitWeaponShop();
            else
                ExitFatherHouse();
            return;
        }
        if (tileAtDest == TILE_AREA_EXIT)
        {
            ShowReturnAreaConfirm();
            return;
        }

        // ステラ・オリジン: 宇宙の門 (y=37)
        if (DataCarrier.Instance != null && DataCarrier.Instance.currentArea == 8
            && newY == 37 && newX >= 11 && newX <= 13)
        {
            if (!stellaGateOpened)
            {
                StartCoroutine(ShowCosmicGateEvent());
                return;
            }
            // 開放済み: ラスボス演出へ
            StartCoroutine(CosmicGateEntrySequence());
            return;
        }

        if (!walkable[newX, newY])
            return;

        // ステラ・オリジン: 音叉ワープ（音叉→ボス広場）
        if (DataCarrier.Instance != null && DataCarrier.Instance.currentArea == 8)
        {
            int warpDestX = 0, warpDestY = 0;
            bool doWarp = false;
            if (newX == stellaForkA.x && newY == stellaForkA.y)
            {
                warpDestX = stellaForkADest.x; warpDestY = stellaForkADest.y; doWarp = true;
            }
            else if (newX == stellaForkB.x && newY == stellaForkB.y)
            {
                warpDestX = stellaForkBDest.x; warpDestY = stellaForkBDest.y; doWarp = true;
            }
            else if (newX == stellaForkReturnW.x && newY == stellaForkReturnW.y)
            {
                warpDestX = stellaForkReturnDest.x; warpDestY = stellaForkReturnDest.y; doWarp = true;
            }
            else if (newX == stellaForkReturnE.x && newY == stellaForkReturnE.y)
            {
                warpDestX = stellaForkReturnDest.x; warpDestY = stellaForkReturnDest.y; doWarp = true;
            }
            if (doWarp)
            {
                playerTileX = warpDestX;
                playerTileY = warpDestY;
                float wpX = (playerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
                float wpY = (playerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
                playerRect.anchoredPosition = new Vector2(wpX, wpY);
                targetPosition = playerRect.anchoredPosition;
                isMoving = false;
                UpdateCameraFollow();
                SpawnStellaRipple();
                // ワープ先でガーディアンに隣接していたら自動で対話開始
                CheckStellaGuardianAutoTrigger();
                return;
            }
        }

        // かぐやフォロワー: プレイヤーの今の位置を目標にする（1歩遅れてついてくる）
        if (kaguyaFollowerRect != null)
        {
            float fTargetX = (playerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
            float fTargetY = (playerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
            kaguyaFollowerTarget = new Vector2(fTargetX, fTargetY);
            kaguyaFollowerMoving = true;
            // プレイヤーの下レイヤーに維持
            kaguyaFollowerObj.transform.SetSiblingIndex(playerObj.transform.GetSiblingIndex());
        }

        playerTileX = newX;
        playerTileY = newY;

        float posX = (playerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (playerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        targetPosition = new Vector2(posX, posY);
        isMoving = true;

        // 足音SE（前の音を止めてから再生）
        if (seSource != null && seFootstep != null)
        {
            seSource.Stop();
            seSource.pitch = Random.Range(0.92f, 1.08f);
            seSource.clip = seFootstep;
            seSource.volume = 0.2f;
            seSource.Play();
        }

        // ステラ・オリジン: Aura波紋
        int areaForRipple = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (areaForRipple == 8)
            SpawnStellaRipple();

        // DataCarrierに位置保存
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.mapPlayerX = playerTileX;
            DataCarrier.Instance.mapPlayerY = playerTileY;
        }

        // ステラ・オリジン: ガーディアンに隣接したら自動対話
        if (DataCarrier.Instance != null && DataCarrier.Instance.currentArea == 8)
            CheckStellaGuardianAutoTrigger();
    }

    void UpdateMovement()
    {
        if (!isMoving || playerRect == null) return;

        playerRect.anchoredPosition = Vector2.MoveTowards(
            playerRect.anchoredPosition,
            targetPosition,
            moveSpeed * DISPLAY_TILE * Time.deltaTime
        );

        // かぐやフォロワー追従（少し遅めにふわっとついてくる）
        if (kaguyaFollowerMoving && kaguyaFollowerRect != null)
        {
            kaguyaFollowerRect.anchoredPosition = Vector2.MoveTowards(
                kaguyaFollowerRect.anchoredPosition,
                kaguyaFollowerTarget,
                moveSpeed * DISPLAY_TILE * 0.85f * Time.deltaTime
            );
            if (Vector2.Distance(kaguyaFollowerRect.anchoredPosition, kaguyaFollowerTarget) < 0.5f)
            {
                kaguyaFollowerRect.anchoredPosition = kaguyaFollowerTarget;
                kaguyaFollowerMoving = false;
            }
        }

        // 歩行スプライトアニメーション
        if (playerWalkSprites[playerDirection] != null && playerWalkSprites[playerDirection].Length > 0)
        {
            walkFrameTimer += Time.deltaTime;
            if (walkFrameTimer >= WALK_FRAME_INTERVAL)
            {
                walkFrameTimer -= WALK_FRAME_INTERVAL;
                walkFrameIndex = (walkFrameIndex + 1) % playerWalkSprites[playerDirection].Length;
                playerImage.sprite = playerWalkSprites[playerDirection][walkFrameIndex];
            }
        }

        if (Vector2.Distance(playerRect.anchoredPosition, targetPosition) < 0.5f)
        {
            playerRect.anchoredPosition = targetPosition;
            isMoving = false;
            // 足音停止
            if (seSource != null && seSource.isPlaying && seSource.clip == seFootstep)
                seSource.Stop();
            // 歩行パーティクル
            SpawnStepParticles(playerRect.anchoredPosition.x, playerRect.anchoredPosition.y);
            // 歩行アニメリセット → 静止スプライトに戻す
            walkFrameIndex = 0;
            walkFrameTimer = 0f;
            if (playerIdleSprites[playerDirection] != null)
                playerImage.sprite = playerIdleSprites[playerDirection];

            // 歩行毒ダメージ
            ApplyPoisonStep();

            // アイテム拾得判定
            CheckItemPickup();

            // ミルクポイント回復判定
            CheckMilkPoint();

            // 長老NPC判定
            CheckElderNPC();

            // 母親NPC判定
            CheckMotherNPC();

            // 父親NPC判定
            CheckFatherNPC();

            // 実家の母親NPC判定
            CheckHomeMotherNPC();

            // お手伝いさんNPC判定
            CheckMaidNPCs();

            // 商人NPC判定
            CheckMerchantNPC();

            // 塾の先生NPC判定
            CheckJukuTeacher();

            // 塾の生徒NPC判定
            CheckJukuStudents();

            // 金の卵の老人NPC判定
            CheckGoldenEggOldMan();

            // かぐやちゃんNPC判定
            CheckKaguyaNPC();
        }
    }

    void SpawnStepParticles(float px, float py)
    {
        int count = Random.Range(3, 6);
        for (int i = 0; i < count; i++)
            StartCoroutine(StepParticle(px, py));
    }

    IEnumerator StepParticle(float px, float py)
    {
        var go = new GameObject("StepStar");
        go.transform.SetParent(tilesContainer.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        float size = Random.Range(8f, 18f);
        rt.sizeDelta = new Vector2(size, size);
        float offsetX = Random.Range(-DISPLAY_TILE * 0.4f, DISPLAY_TILE * 0.4f);
        float offsetY = Random.Range(-DISPLAY_TILE * 0.3f, DISPLAY_TILE * 0.3f);
        rt.anchoredPosition = new Vector2(px + offsetX, py + offsetY);

        var img = go.AddComponent<Image>();
        Color[] pastels = {
            new Color(1f, 0.8f, 0.9f),
            new Color(0.8f, 0.9f, 1f),
            new Color(1f, 1f, 0.7f),
            new Color(0.7f, 1f, 0.85f),
            new Color(0.9f, 0.8f, 1f),
        };
        img.color = pastels[Random.Range(0, pastels.Length)];
        rt.localRotation = Quaternion.Euler(0, 0, 45f);

        float duration = Random.Range(0.4f, 0.7f);
        float elapsed = 0f;
        float riseSpeed = Random.Range(30f, 60f);
        Vector2 startPos = rt.anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rt.anchoredPosition = startPos + new Vector2(0, riseSpeed * t);
            float scale = Mathf.Lerp(1f, 0.3f, t);
            rt.localScale = Vector3.one * scale;
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f - t);
            yield return null;
        }

        Destroy(go);
    }

    void UpdateCameraFollow()
    {
        if (tilesContainerRect == null || playerRect == null) return;

        // プレイヤーのコンテナ内ローカル位置（スムーズアニメーション中の値を使用）
        Vector2 playerPos = playerRect.anchoredPosition;

        // プレイヤーを画面中央に配置するためのスクロールオフセット
        float scrollX = -playerPos.x;
        float scrollY = -playerPos.y;

        // マップ端がはみ出さないようクランプ
        float halfMapW = mapWidth * DISPLAY_TILE / 2f;
        float halfMapH = mapHeight * DISPLAY_TILE / 2f;
        float halfScreenW = 540f;   // 1080 / 2
        float halfScreenH = 960f;   // 1920 / 2

        float maxScrollX = Mathf.Max(0f, halfMapW - halfScreenW);
        float maxScrollY = Mathf.Max(0f, halfMapH - halfScreenH);

        scrollX = Mathf.Clamp(scrollX, -maxScrollX, maxScrollX);
        scrollY = Mathf.Clamp(scrollY, -maxScrollY, maxScrollY);

        tilesContainerRect.anchoredPosition = new Vector2(scrollX, scrollY - 40f);

        // UI Toolkit タイルグリッド（area==0 のDQタイル）も同期
        if (tileGridRoot != null)
        {
            float gridLeft = 540f + scrollX - mapWidth * DISPLAY_TILE / 2f;
            float gridTop = 1000f - scrollY - mapHeight * DISPLAY_TILE / 2f;
            tileGridRoot.style.left = gridLeft;
            tileGridRoot.style.top = gridTop;
        }
    }

    // ===== シンボルエンカウント =====

    static readonly Color[] EnemyColorsArea0 = {
        new Color(0.6f, 0.8f, 1f),       // 水色
        new Color(1f, 0.7f, 0.3f),        // オレンジ
        new Color(0.8f, 0.5f, 0.9f),      // パープル
        new Color(0.5f, 0.9f, 0.6f),      // グリーン
    };
    static readonly Color[] EnemyColorsArea1 = {
        new Color(0.5f, 0.2f, 0.6f),      // ダーク紫
        new Color(0.7f, 0.15f, 0.2f),     // 暗赤
        new Color(0.3f, 0.2f, 0.5f),      // ダークインディゴ
        new Color(0.6f, 0.1f, 0.4f),      // マゼンタ
    };
    static readonly Color[] EnemyColorsArea3 = {
        new Color(1f, 0.5f, 0.6f),        // ピンク
        new Color(0.9f, 0.3f, 0.5f),      // ローズ
        new Color(1f, 0.4f, 0.8f),        // ホットピンク
        new Color(0.8f, 0.35f, 0.65f),    // フューシャ
    };

    void SpawnEnemySymbols(int area)
    {
        int count;
        Color[] palette;

        switch (area)
        {
            case 0: count = 4; palette = EnemyColorsArea0; break;
            case 1: count = 5; palette = EnemyColorsArea1; break;
            case 3: count = 6; palette = EnemyColorsArea3; break;
            default: return; // area 2,4,5,6 はエンカウントなし
        }

        var candidates = new List<Vector2Int>();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!walkable[x, y]) continue;
                // プレイヤーから3タイル以上離れた位置
                int dist = Mathf.Abs(x - playerTileX) + Mathf.Abs(y - playerTileY);
                if (dist < 3) continue;
                // 草タイルか花タイルのみ
                if (mapData[x, y] != TILE_GRASS && mapData[x, y] != TILE_FLOWER) continue;
                candidates.Add(new Vector2Int(x, y));
            }
        }

        // シャッフル
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;
        }

        int spawnCount = Mathf.Min(count, candidates.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            var pos = candidates[i];
            Color color = palette[i % palette.Length];

            var sym = new EnemySymbol();
            sym.Initialize("", color, pos.x, pos.y,
                tilesContainer.transform, DISPLAY_TILE, mapWidth, mapHeight);
            enemySymbols.Add(sym);
        }
    }

    void UpdateEnemySymbols()
    {
        if (menuOpen || symbolEncounterActive) return;

        foreach (var sym in enemySymbols)
        {
            sym.Tick(playerTileX, playerTileY, walkable, mapWidth, mapHeight);

            if (!sym.isMoving && !isMoving && sym.IsContactWith(playerTileX, playerTileY))
            {
                StartCoroutine(StartSymbolEncounter(sym));
                return;
            }
        }
    }

    IEnumerator StartSymbolEncounter(EnemySymbol sym)
    {
        symbolEncounterActive = true;
        menuOpen = true;
        if (moveCtrl != null) moveCtrl.IsLocked = true;

        // シンボルを非表示
        sym.SetActive(false);

        // フラッシュ演出
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.backgroundColor = new Color(1, 1, 1, 0);
        overlayRoot.Add(overlay);

        for (int i = 0; i < 3; i++)
        {
            float flashTime = 0.15f;
            float elapsed = 0f;
            while (elapsed < flashTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashTime;
                overlay.style.backgroundColor = new Color(1, 1, 1, 1f - t);
                yield return null;
            }
            yield return new WaitForSeconds(0.05f);
        }

        overlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);

        yield return new WaitForSeconds(0.5f);

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.fixedEncounterEnemy = "";
            DataCarrier.Instance.SaveData();
        }

        yield return StartCoroutine(CaptureMapScreenshot());
        SceneManager.LoadScene("BattleScene");
    }

    IEnumerator PlayWipeIn()
    {
        var wipePanel = new UIE.VisualElement();
        wipePanel.style.position = UIE.Position.Absolute;
        wipePanel.style.left = 0; wipePanel.style.top = 0;
        wipePanel.style.right = 0; wipePanel.style.bottom = 0;
        wipePanel.style.backgroundColor = Color.black;
        overlayRoot.Add(wipePanel);

        yield return null; // 1フレーム待ちでマップのレンダリング完了

        float elapsed = 0f;
        float wipeDuration = 0.8f;
        while (elapsed < wipeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / wipeDuration);
            // ease-in-out
            t = t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
            float pct = Mathf.Lerp(0f, -100f, t);
            wipePanel.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(pct, UIE.LengthUnit.Percent), 0));
            yield return null;
        }
        wipePanel.RemoveFromHierarchy();
    }

    IEnumerator PlayWipeInThenElderGuide()
    {
        yield return StartCoroutine(PlayWipeIn());
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowElderDialogue());
    }

    IEnumerator PlayWipeInThenGorgeousIntro()
    {
        yield return StartCoroutine(PlayWipeIn());
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(ShowGorgeousIntro());
    }

    IEnumerator ShowGorgeousIntro()
    {
        menuOpen = true;

        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc("gorgeous_intro");

        bool tapped = false;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.7f * (elapsed / 0.5f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);

        // テキストラベル
        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        textLabel.style.color = Color.white;
        textLabel.style.fontSize = 34;
        textLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        textLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
        textLabel.style.width = 900;
        UIHelper.ApplyFont(textLabel);
        overlay.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        tapHint.style.color = new Color(1, 1, 1, 0.5f);
        tapHint.style.marginTop = 40;
        UIHelper.ApplyFont(tapHint);
        overlay.Add(tapHint);

        string[] messages = new string[]
        {
            Localization.Get("gorgeous_intro_1"),
            Localization.Get("gorgeous_intro_2"),
            Localization.Get("gorgeous_intro_3"),
        };

        for (int i = 0; i < messages.Length; i++)
        {
            textLabel.text = messages[i];
            tapHint.text = (i < messages.Length - 1) ? "▼ タップで続く" : "▼ タップで閉じる";

            yield return new WaitForSeconds(0.3f);
            tapped = false;
            while (!tapped) yield return null;
        }

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.3f);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.7f * fadeT);
            textLabel.style.opacity = fadeT;
            tapHint.style.opacity = fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        menuOpen = false;
    }

    IEnumerator AutoElderGuide()
    {
        yield return new WaitForSeconds(0.8f);
        yield return StartCoroutine(ShowElderDialogue());
    }

    void Interact()
    {
        // 父親の家の入口に隣接している場合
        if (IsAdjacentTo(TILE_FATHER_HOUSE))
        {
            StartCoroutine(EnterFatherHouse());
            return;
        }

        // 父親の家・武器屋の出口に隣接している場合
        if (IsAdjacentTo(TILE_FATHER_EXIT))
        {
            int areaForExit = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (areaForExit == 7)
                ExitJuku();
            else if (areaForExit == 6)
                ExitWeaponShop();
            else
                ExitFatherHouse();
            return;
        }

        // 装備ショップに隣接している場合 → 武器屋内部へ
        if (IsAdjacentTo(TILE_WEAPON_SHOP))
        {
            StartCoroutine(EnterWeaponShop());
            return;
        }

        // 塾に隣接している場合 → 塾内部へ
        if (IsAdjacentTo(TILE_JUKU))
        {
            StartCoroutine(EnterJuku());
            return;
        }

        // ボスの館の門に隣接している場合
        if (IsAdjacentTo(TILE_BOSS_GATE) || IsAdjacentTo(TILE_BOSS_MANSION))
        {
            if (IsBabyTooYoungForBoss())
            {
                if (elderBossBlockCooldown > 0f) return;
                StartCoroutine(ShowElderBossBlockDialogue());
                return;
            }
            int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (area == 3)
            {
                // 小悪魔の街: 109（area 4）に入る
                StartCoroutine(Enter109());
                return;
            }
            if (area == 1)
            {
                // ゴージャス・ヴィレッジ: 館内（area 2）に入る
                StartCoroutine(EnterMansionArea());
                return;
            }
            StartCoroutine(EnterBossMansion());
            return;
        }

        // ステラ・オリジン（Area 8）: NPC・ゆりかご・門
        int areaForExamine = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (areaForExamine == 8)
        {
            // ガーディアン（デヴィル夫人）
            if (IsAdjacentToPos(stellaGuardianDevilX, stellaGuardianDevilY))
            {
                StartCoroutine(ShowStellaGuardianDialogue(false));
                return;
            }
            // ガーディアン（メロディアス女王）
            if (IsAdjacentToPos(stellaGuardianMelodiasX, stellaGuardianMelodiasY))
            {
                StartCoroutine(ShowStellaGuardianDialogue(true));
                return;
            }
            // 光のゆりかご（完了済みならブロックしない）
            if (IsAdjacentToPos(stellaCradleX, stellaCradleY) ||
                (playerTileX == stellaCradleX && playerTileY == stellaCradleY))
            {
                bool cradleDone = DataCarrier.Instance != null && (DataCarrier.Instance.stellaOriginProgress & 4) != 0;
                if (!stellaCradleActive && !cradleDone)
                {
                    StartCoroutine(ShowStellaCradleEvent());
                    return;
                }
                // 完了済み: ゆりかごのタップではなく通常操作に流す
            }
            // 宇宙の門（y=36〜37, x=11〜13 付近）
            if (playerTileY >= 36 && playerTileX >= 10 && playerTileX <= 14)
            {
                if (!stellaGateOpened)
                {
                    StartCoroutine(ShowCosmicGateEvent());
                    return;
                }
            }
        }

        // 玩具屋内部: 家具に隣接して調べた時のメッセージ
        if (areaForExamine == 6)
        {
            // カウンター (2-4, 5) に隣接
            if (IsAdjacentToPos(2, 5) || IsAdjacentToPos(3, 5) || IsAdjacentToPos(4, 5))
            {
                ShowMessage("ショーケースには おもちゃが ずらり！\nどれも キラキラ してるよ✨");
                return;
            }
            // 右奥の机 (6, 7-8) に隣接
            if (IsAdjacentToPos(6, 7) || IsAdjacentToPos(6, 8))
            {
                ShowMessage("たくさんの おもちゃが\nきれいに ならべられている🧸");
                return;
            }
        }

        int tileType = mapData[playerTileX, playerTileY];

        switch (tileType)
        {
            case TILE_FLOWER:
                ShowMessage(Localization.Get("map_flower"));
                break;
            case TILE_PATH:
                ShowMessage(Localization.Get("map_path"));
                break;
            case TILE_DARK_DIRT:
                ShowMessage(Localization.Get("map_dark_dirt"));
                break;
            default:
                if (IsAdjacentTo(TILE_WATER))
                    ShowMessage(Localization.Get("map_water"));
                else if (IsAdjacentTo(TILE_HOUSE_RED) || IsAdjacentTo(TILE_HOUSE_GREEN) || IsAdjacentTo(TILE_HOUSE_BLUE))
                    ShowMessage(Localization.Get("map_house"));
                else if (IsAdjacentTo(TILE_ROCK))
                    ShowMessage(Localization.Get("map_rock"));
                else
                    ShowMessage(Localization.Get("map_nothing"));
                break;
        }
    }

    // シバ撃破後: シバの家に入り、次のエリアへ進む
    IEnumerator EnterShibaHouse()
    {
        yield return StartCoroutine(ShowTransitionOverlay("シバの家に入った…"));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 1;
            DataCarrier.Instance.mapPlayerX = 5;
            DataCarrier.Instance.mapPlayerY = 1;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    IEnumerator EnterBossMansion()
    {
        yield return StartCoroutine(ShowTransitionOverlay(Localization.Get("map_boss_enter")));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = true;
            DataCarrier.Instance.SaveData();
        }
        yield return StartCoroutine(CaptureMapScreenshot());
        SceneManager.LoadScene("BattleScene");
    }

    // ゴージャス・ヴィレッジから館内（area 2）に入る
    IEnumerator EnterMansionArea()
    {
        yield return StartCoroutine(ShowTransitionOverlay(Localization.Get("map_mansion_enter")));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 2;
            DataCarrier.Instance.mapPlayerX = 5;
            DataCarrier.Instance.mapPlayerY = 2;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 固定エンカウント発動
    IEnumerator StartFixedEncounter(string enemyName)
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.fixedEncounterEnemy = enemyName;
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.SaveData();
        }
        yield return StartCoroutine(CaptureMapScreenshot());
        SceneManager.LoadScene("BattleScene");
    }

    // デヴィル夫人ボス戦へ
    IEnumerator EnterDevilLadyBoss()
    {
        yield return StartCoroutine(ShowTransitionOverlay(Localization.Get("map_mansion_boss_enter")));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = true;
            DataCarrier.Instance.SaveData();
        }
        yield return StartCoroutine(CaptureMapScreenshot());
        SceneManager.LoadScene("BattleScene");
    }

    // 館からゴージャス・ヴィレッジに戻る
    void ExitMansion()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 1;
            DataCarrier.Instance.mapPlayerX = 8;
            DataCarrier.Instance.mapPlayerY = 14;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 村から父親の家（area 5）に入る
    IEnumerator EnterFatherHouse()
    {
        yield return StartCoroutine(ShowTransitionOverlay(Localization.Get("map_father_house_enter")));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 5;
            DataCarrier.Instance.mapPlayerX = 8;
            DataCarrier.Instance.mapPlayerY = 1;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 父親の家から村に戻る
    void ExitFatherHouse()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 0;
            DataCarrier.Instance.mapPlayerX = 3;
            DataCarrier.Instance.mapPlayerY = 31;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    /// <summary>玩具屋の床全体をToy_Shop_Background 1枚で覆う</summary>
    void CreateToyShopFloorOverlay()
    {
        if (tilesContainer == null || toyShopBgSprite == null) return;

        // 床タイル範囲: x=1〜6, y=2〜8 (6幅 × 7高)
        float minX = 1, maxX = 6, minY = 2, maxY = 8;
        float centerTileX = (minX + maxX) / 2f;
        float centerTileY = (minY + maxY) / 2f;
        float posX = (centerTileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (centerTileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        float width = (maxX - minX + 1) * DISPLAY_TILE;
        float height = (maxY - minY + 1) * DISPLAY_TILE;

        var bg = new GameObject("ToyShopFloorBg");
        bg.transform.SetParent(tilesContainer.transform, false);
        var rect = bg.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(width, height);

        var img = bg.AddComponent<Image>();
        img.sprite = toyShopBgSprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.raycastTarget = false;

        // タイルより上、プレイヤーやNPCより下に配置
        bg.transform.SetAsLastSibling();
    }

    void CreateToyShopDeskOverlay(float tileX, float tileY)
    {
        if (tilesContainer == null || toyShopDeskSprite == null) return;

        float posX = (tileX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (tileY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;

        var desk = new GameObject("ToyShopDesk");
        desk.transform.SetParent(tilesContainer.transform, false);
        var rect = desk.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(posX, posY);
        // 2タイル分の高さで引き伸ばしなし
        rect.sizeDelta = new Vector2(DISPLAY_TILE * 1.2f, DISPLAY_TILE * 2.2f);

        var img = desk.AddComponent<Image>();
        img.sprite = toyShopDeskSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    // ゴージャス・ヴィレッジから武器屋内部（area 6）に入る
    IEnumerator EnterWeaponShop()
    {
        yield return StartCoroutine(ShowTransitionOverlay(Localization.Get("shop_enter")));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.shopEntryArea = DataCarrier.Instance.currentArea;
            DataCarrier.Instance.currentArea = 6;
            DataCarrier.Instance.mapPlayerX = 4;
            DataCarrier.Instance.mapPlayerY = 2;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 武器屋内部から元のエリアに戻る
    void ExitWeaponShop()
    {
        if (DataCarrier.Instance != null)
        {
            int returnArea = DataCarrier.Instance.shopEntryArea;
            DataCarrier.Instance.currentArea = returnArea;
            if (returnArea == 0)
            {
                // 村: 武器屋の近くに戻る
                DataCarrier.Instance.mapPlayerX = 4;
                DataCarrier.Instance.mapPlayerY = 14;
            }
            else if (returnArea == 3)
            {
                // 小悪魔の街: 武器屋の近くに戻る
                DataCarrier.Instance.mapPlayerX = 8;
                DataCarrier.Instance.mapPlayerY = 26;
            }
            else
            {
                // ゴージャス・ヴィレッジ: 既存の戻り位置
                DataCarrier.Instance.mapPlayerX = 4;
                DataCarrier.Instance.mapPlayerY = 14;
            }
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 村から塾内部（area 7）に入る
    IEnumerator EnterJuku()
    {
        yield return StartCoroutine(ShowTransitionOverlay("じゅくに はいった"));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 7;
            DataCarrier.Instance.mapPlayerX = 6;
            DataCarrier.Instance.mapPlayerY = 1;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 塾内部から村に戻る
    void ExitJuku()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 0;
            DataCarrier.Instance.mapPlayerX = 9;
            DataCarrier.Instance.mapPlayerY = 24;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 小悪魔の街から109（area 4）に入る
    IEnumerator Enter109()
    {
        yield return StartCoroutine(ShowTransitionOverlay(Localization.Get("map_109_enter")));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 4;
            DataCarrier.Instance.mapPlayerX = 5;
            DataCarrier.Instance.mapPlayerY = 2;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // メロディアス女王ボス戦へ
    IEnumerator EnterMelodiasQueen()
    {
        yield return StartCoroutine(ShowTransitionOverlay(Localization.Get("map_109_boss_enter")));

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = true;
            DataCarrier.Instance.SaveData();
        }
        yield return StartCoroutine(CaptureMapScreenshot());
        SceneManager.LoadScene("BattleScene");
    }

    // 共通トランジションオーバーレイ
    IEnumerator ShowTransitionOverlay(string text, float fadeTime = 0.8f, float holdTime = 2.0f)
    {
        menuOpen = true;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, elapsed / fadeTime);
            yield return null;
        }
        overlay.style.backgroundColor = Color.black;

        var label = UIHelper.CreateLabel(text, "map-transition-text");
        overlay.Add(label);

        yield return new WaitForSeconds(holdTime);
    }

    // 109から小悪魔の街に戻る
    void Exit109()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.currentArea = 3;
            DataCarrier.Instance.mapPlayerX = 14;
            DataCarrier.Instance.mapPlayerY = 37;
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    // 前のステージに戻る確認ダイアログ
    UIE.VisualElement returnConfirmOverlay;

    void ShowReturnAreaConfirm()
    {
        if (returnConfirmOverlay != null || overlayRoot == null) return;
        menuOpen = true;

        returnConfirmOverlay = new UIE.VisualElement();
        returnConfirmOverlay.AddToClassList("fill");
        returnConfirmOverlay.style.alignItems = UIE.Align.Center;
        returnConfirmOverlay.style.justifyContent = UIE.Justify.Center;
        returnConfirmOverlay.style.backgroundColor = new Color(0, 0, 0, 0.6f);
        overlayRoot.Add(returnConfirmOverlay);

        string prevAreaName = GetPreviousAreaName();
        var msg = UIHelper.CreateLabel(
            Localization.Get("map_return_confirm", prevAreaName), "map-confirm-text");
        msg.enableRichText = true;
        returnConfirmOverlay.Add(msg);

        var yesBtn = new UIE.Button();
        yesBtn.AddToClassList("pill-button");
        yesBtn.style.marginTop = 64;
        yesBtn.text = Localization.Get("map_return_yes");
        UIHelper.ApplyFont(yesBtn);
        yesBtn.clicked += () =>
        {
            CloseReturnConfirm();
            StartCoroutine(ReturnToPreviousArea());
        };
        returnConfirmOverlay.Add(yesBtn);

        var noBtn = new UIE.Button();
        noBtn.AddToClassList("pill-button");
        noBtn.style.marginTop = 32;
        noBtn.text = Localization.Get("map_return_no");
        UIHelper.ApplyFont(noBtn);
        noBtn.clicked += () => CloseReturnConfirm();
        returnConfirmOverlay.Add(noBtn);
    }

    void CloseReturnConfirm()
    {
        if (returnConfirmOverlay != null)
        {
            returnConfirmOverlay.RemoveFromHierarchy();
            returnConfirmOverlay = null;
        }
        menuOpen = false;
    }

    string GetPreviousAreaName()
    {
        int curArea = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        switch (curArea)
        {
            case 1: return Localization.Get("area_name_0");
            case 3: return Localization.Get("area_name_1");
            default: return "";
        }
    }

    IEnumerator ReturnToPreviousArea()
    {
        int curArea = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        string prevName = GetPreviousAreaName();

        yield return StartCoroutine(ShowTransitionOverlay(
            Localization.Get("map_return_transition", prevName)));

        if (DataCarrier.Instance != null)
        {
            switch (curArea)
            {
                case 1: // ゴージャス・ヴィレッジ → 村
                    DataCarrier.Instance.currentArea = 0;
                    DataCarrier.Instance.mapPlayerX = 5;
                    DataCarrier.Instance.mapPlayerY = 33;
                    break;
                case 3: // 小悪魔の街 → ゴージャス・ヴィレッジ
                    DataCarrier.Instance.currentArea = 1;
                    DataCarrier.Instance.mapPlayerX = 5;
                    DataCarrier.Instance.mapPlayerY = 9;
                    break;
            }
            DataCarrier.Instance.SaveData();
        }
        SceneManager.LoadScene("MapScene");
    }

    bool IsAdjacentTo(int tileType)
    {
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };
        for (int i = 0; i < 4; i++)
        {
            int nx = playerTileX + dx[i];
            int ny = playerTileY + dy[i];
            if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight)
            {
                if (mapData[nx, ny] == tileType) return true;
            }
        }
        return false;
    }

    bool IsAdjacentToPos(int tx, int ty)
    {
        int dx = Mathf.Abs(playerTileX - tx);
        int dy = Mathf.Abs(playerTileY - ty);
        return (dx + dy) <= 1;
    }

    // ===== メニュー =====

    void ToggleMenu()
    {
        if (menuOpen && menuOverlayEl != null)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    void OpenMenu()
    {
        menuOpen = true;
        SetTouchControlsVisible(false);

        menuOverlayEl = UIHelper.CreateOverlay();
        menuOverlayEl.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == menuOverlayEl) CloseMenu();
        });

        var card = new UIE.VisualElement();
        card.AddToClassList("map-menu-card");

        var title = UIHelper.CreateLabel("MENU", "map-menu-title");
        card.Add(title);

        string[] labels = {
            Localization.Get("map_menu_status"),
            Localization.Get("map_menu_equipment"),
            Localization.Get("map_menu_inventory"),
            Localization.Get("map_menu_home"),
            Localization.Get("map_menu_save"),
            Localization.Get("map_menu_title"),
            Localization.Get("map_menu_close")
        };
        System.Action[] actions = {
            () => { CloseMenu(); OpenStatusPanel(); },
            () => { CloseMenu(); OpenEquipmentPanel(); },
            () => { CloseMenu(); OpenInventoryPanel(); },
            () => OnGoHome(),
            () => OnSave(),
            () => OnGoTitle(),
            () => CloseMenu()
        };

        for (int i = 0; i < labels.Length; i++)
        {
            var btn = new UIE.Button();
            btn.AddToClassList("map-menu-item-btn");
            btn.text = labels[i];
            UIHelper.ApplyFont(btn);
            int idx = i;
            btn.clicked += () => actions[idx]();
            card.Add(btn);
        }

        menuOverlayEl.Add(card);
        overlayRoot.Add(menuOverlayEl);
    }

    void CloseMenu()
    {
        menuOpen = false;
        if (menuOverlayEl != null)
        {
            menuOverlayEl.RemoveFromHierarchy();
            menuOverlayEl = null;
        }
        SetTouchControlsVisible(true);
    }

    void OnSave()
    {
        CloseMenu();
        OpenSavePanel();
    }

    void OpenSavePanel()
    {
        if (saveOverlayEl != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        saveOverlayEl = new UIE.VisualElement();
        saveOverlayEl.AddToClassList("map-save-panel");

        var title = UIHelper.CreateLabel(Localization.Get("map_save_title"), "map-save-title");
        saveOverlayEl.Add(title);

        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            int idx = i;
            bool exists = DataCarrier.SlotExists(idx);

            var slotBtn = new UIE.Button();
            slotBtn.AddToClassList("map-save-slot");
            slotBtn.AddToClassList(exists ? "map-save-slot-exists" : "map-save-slot-empty");
            slotBtn.enableRichText = true;
            UIHelper.ApplyFont(slotBtn);

            if (exists)
            {
                string babyName = DataCarrier.GetSlotBabyName(idx);
                int age = DataCarrier.GetSlotAge(idx);
                bool isGod = DataCarrier.GetSlotIsGodBaby(idx);
                string godMark = isGod ? " <color=#FFD700>\u2605</color>" : "";
                slotBtn.text = $"{Localization.Get("map_save_slot", idx + 1)}{babyName}{godMark}  ({Localization.GetAge(age)})";
            }
            else
            {
                slotBtn.text = $"{Localization.Get("map_save_slot", idx + 1)}{Localization.Get("map_save_slot_empty")}";
            }

            slotBtn.clicked += () => OnSaveSlotSelected(idx);
            saveOverlayEl.Add(slotBtn);
        }

        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("map-save-close-btn");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = Localization.Get("ui_close");
        closeBtn.clicked += () => CloseSavePanel();
        saveOverlayEl.Add(closeBtn);

        overlayRoot.Add(saveOverlayEl);
    }

    void OnSaveSlotSelected(int slot)
    {
        if (DataCarrier.SlotExists(slot))
        {
            ShowOverwriteConfirm(slot);
        }
        else
        {
            DoSaveToSlot(slot);
        }
    }

    void ShowOverwriteConfirm(int slot)
    {
        if (saveOverlayEl == null) return;
        saveOverlayEl.Clear();

        string babyName = DataCarrier.GetSlotBabyName(slot);
        var msg = UIHelper.CreateLabel(
            Localization.Get("map_save_overwrite_msg", slot + 1, babyName), "map-confirm-text");
        msg.enableRichText = true;
        saveOverlayEl.Add(msg);

        var yesBtn = new UIE.Button();
        yesBtn.AddToClassList("map-save-confirm-btn");
        yesBtn.text = Localization.Get("map_save_overwrite");
        UIHelper.ApplyFont(yesBtn);
        yesBtn.clicked += () => DoSaveToSlot(slot);
        saveOverlayEl.Add(yesBtn);

        var cancelBtn = new UIE.Button();
        cancelBtn.AddToClassList("map-save-cancel-btn");
        cancelBtn.text = Localization.Get("map_save_cancel");
        UIHelper.ApplyFont(cancelBtn);
        cancelBtn.clicked += () => { CloseSavePanel(); OpenSavePanel(); };
        saveOverlayEl.Add(cancelBtn);
    }

    void DoSaveToSlot(int slot)
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.SaveToSlot(slot);
        }
        CloseSavePanel();
        ShowSavedNotification();
    }

    void CloseSavePanel()
    {
        if (saveOverlayEl != null)
        {
            saveOverlayEl.RemoveFromHierarchy();
            saveOverlayEl = null;
            menuOpen = false;
            SetTouchControlsVisible(true);
        }
    }

    void ShowSavedNotification()
    {
        StartCoroutine(ShowSavedNotificationCoroutine());
    }

    IEnumerator ShowSavedNotificationCoroutine()
    {
        var box = new UIE.VisualElement();
        box.style.position = UIE.Position.Absolute;
        box.style.left = 0;
        box.style.right = 0;
        box.style.top = 0;
        box.style.bottom = 0;
        box.pickingMode = UIE.PickingMode.Ignore;
        box.style.alignItems = UIE.Align.Center;
        box.style.justifyContent = UIE.Justify.Center;

        var pill = new UIE.VisualElement();
        pill.style.height = 120;
        pill.style.width = 700;
        pill.style.borderTopLeftRadius = 60;
        pill.style.borderTopRightRadius = 60;
        pill.style.borderBottomLeftRadius = 60;
        pill.style.borderBottomRightRadius = 60;
        pill.style.backgroundColor = Color.white;
        pill.style.alignItems = UIE.Align.Center;
        pill.style.justifyContent = UIE.Justify.Center;

        var label = UIHelper.CreateLabel(Localization.Get("ui_saved"), "");
        label.style.fontSize = 36;
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.color = new Color(0.45f, 0.45f, 0.5f);
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.enableRichText = false;
        pill.Add(label);

        box.Add(pill);
        overlayRoot.Add(box);

        yield return new WaitForSeconds(2f);

        box.RemoveFromHierarchy();
    }

    void OnGoTitle()
    {
        // スロットが決まっている場合のみ自動セーブ
        if (DataCarrier.Instance != null && DataCarrier.Instance.currentSlot >= 0)
        {
            DataCarrier.Instance.SaveToSlot(DataCarrier.Instance.currentSlot);
        }
        CloseMenu();
        SceneManager.LoadScene("TitleScene");
    }

    // ===== 金のたまご =====

    void CreateGoldenEgg()
    {
        if (tilesContainer == null) return;
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        string eggItemName = area == 3 ? "金のたまご2" : "金のたまご";
        // 既に持っていたら生成しない
        if (DataCarrier.Instance != null && DataCarrier.Instance.HasItem(eggItemName)) return;

        int eggX = area == 3 ? GOLDEN_EGG_X_AREA3 : GOLDEN_EGG_X;
        int eggY = area == 3 ? GOLDEN_EGG_Y_AREA3 : GOLDEN_EGG_Y;

        goldenEggObj = new GameObject("GoldenEgg");
        goldenEggObj.transform.SetParent(tilesContainer.transform, false);

        var rect = goldenEggObj.AddComponent<RectTransform>();
        float posX = (eggX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (eggY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.5f);

        var eggImg = goldenEggObj.AddComponent<Image>();
        var eggSpr = Resources.Load<Sprite>("Map/goldenegg");
        if (eggSpr != null)
        {
            eggImg.sprite = eggSpr;
            eggImg.preserveAspect = true;
        }
        else
        {
            // Spriteで読めない場合はTexture2Dから生成
            var eggTex = Resources.Load<Texture2D>("Map/goldenegg");
            if (eggTex != null && eggTex.isReadable)
            {
                eggImg.sprite = Sprite.Create(eggTex, new Rect(0, 0, eggTex.width, eggTex.height),
                    new Vector2(0.5f, 0.5f));
                eggImg.preserveAspect = true;
            }
            else
            {
                eggImg.color = new Color(1f, 0.84f, 0f);
            }
        }

        goldenEggObj.transform.SetAsLastSibling();
    }

    void CheckItemPickup()
    {
        if (goldenEggObj == null) return;
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        int eggX = area == 3 ? GOLDEN_EGG_X_AREA3 : GOLDEN_EGG_X;
        int eggY = area == 3 ? GOLDEN_EGG_Y_AREA3 : GOLDEN_EGG_Y;
        if (playerTileX != eggX || playerTileY != eggY) return;

        // 金のたまごを取得
        if (DataCarrier.Instance != null)
        {
            string eggItemName = area == 3 ? "金のたまご2" : "金のたまご";
            DataCarrier.Instance.AddItem(eggItemName);
        }

        StartCoroutine(ShowGoldenEggEvent());
    }

    IEnumerator ShowGoldenEggEvent()
    {
        menuOpen = true;

        // --- Phase 1: 暗転 + 卵が画面中央に浮き上がる ---
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        // 中央に卵画像を配置
        var eggEl = new UIE.VisualElement();
        eggEl.style.width = 200;
        eggEl.style.height = 200;
        eggEl.style.opacity = 0f;
        var eggTex = Resources.Load<Texture2D>("Map/goldenegg");
        if (eggTex != null)
            eggEl.style.backgroundImage = new UIE.StyleBackground(eggTex);
        else
            eggEl.style.backgroundColor = new Color(1f, 0.84f, 0f);
        overlay.Add(eggEl);

        // マップ上の卵を消す
        if (goldenEggObj != null) { Destroy(goldenEggObj); goldenEggObj = null; }

        // 暗転 + 卵フェードイン (0.6s)
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.6f);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.6f * t);
            eggEl.style.opacity = t;
            float s = 0.5f + 0.5f * t;
            eggEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.6f);
        eggEl.style.opacity = 1f;
        eggEl.style.scale = new UIE.StyleScale(new UIE.Scale(Vector3.one));

        // --- Phase 2: 卵が輝く + ゆっくり回転 + キラキラパーティクル ---
        // 金色グロー
        var glowEl = new UIE.VisualElement();
        glowEl.style.position = UIE.Position.Absolute;
        glowEl.style.width = 320;
        glowEl.style.height = 320;
        glowEl.style.left = UIE.Length.Percent(50);
        glowEl.style.top = UIE.Length.Percent(50);
        glowEl.style.translate = new UIE.StyleTranslate(new UIE.Translate(-160, -160));
        glowEl.style.borderTopLeftRadius = 160;
        glowEl.style.borderTopRightRadius = 160;
        glowEl.style.borderBottomLeftRadius = 160;
        glowEl.style.borderBottomRightRadius = 160;
        glowEl.style.backgroundColor = new Color(1f, 0.84f, 0f, 0f);
        overlay.Insert(0, glowEl);

        // キラキラパーティクル生成
        Color[] sparkleColors = {
            new Color(1f, 0.84f, 0f),      // ゴールド
            new Color(1f, 0.72f, 0.77f),    // ピンク
            new Color(1f, 1f, 0.8f),        // ライトイエロー
            new Color(0.67f, 0.94f, 0.82f), // ミントグリーン
            new Color(1f, 1f, 1f)           // ホワイト
        };

        elapsed = 0f;
        float shineDuration = 2.0f;
        float nextSparkle = 0f;
        while (elapsed < shineDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shineDuration;

            // 卵ぷるんバウンス
            float bounceY = 1f + 0.04f * Mathf.Sin(elapsed * 4f * Mathf.PI);
            float bounceX = 1f - 0.02f * Mathf.Sin(elapsed * 4f * Mathf.PI);
            eggEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(bounceX, bounceY, 1f)));

            // ゆっくり回転
            float rot = Mathf.Sin(elapsed * 1.5f * Mathf.PI) * 8f;
            eggEl.style.rotate = new UIE.StyleRotate(new UIE.Rotate(rot));

            // グロー脈動
            float glowAlpha = 0.15f + 0.15f * Mathf.Sin(elapsed * 3f * Mathf.PI);
            glowEl.style.backgroundColor = new Color(1f, 0.84f, 0f, glowAlpha);
            float gs = 1f + 0.1f * Mathf.Sin(elapsed * 3f * Mathf.PI);
            glowEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(gs, gs, 1f)));

            // キラキラ生成
            if (elapsed >= nextSparkle)
            {
                nextSparkle = elapsed + 0.12f;
                var color = sparkleColors[UnityEngine.Random.Range(0, sparkleColors.Length)];
                StartCoroutine(SpawnEggSparkle(overlay, color));
            }

            yield return null;
        }

        eggEl.style.rotate = new UIE.StyleRotate(new UIE.Rotate(0));
        eggEl.style.scale = new UIE.StyleScale(new UIE.Scale(Vector3.one));

        // --- Phase 3: テキスト表示 ---
        var textContainer = new UIE.VisualElement();
        textContainer.style.position = UIE.Position.Absolute;
        textContainer.style.bottom = UIE.Length.Percent(22);
        textContainer.style.left = UIE.Length.Percent(50);
        textContainer.style.translate = new UIE.StyleTranslate(new UIE.Translate(UIE.Length.Percent(-50), 0));
        textContainer.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        textContainer.style.paddingTop = 16;
        textContainer.style.paddingBottom = 16;
        textContainer.style.paddingLeft = 40;
        textContainer.style.paddingRight = 40;
        textContainer.style.borderTopLeftRadius = 40;
        textContainer.style.borderTopRightRadius = 40;
        textContainer.style.borderBottomLeftRadius = 40;
        textContainer.style.borderBottomRightRadius = 40;
        overlay.Add(textContainer);

        var titleLabel = UIHelper.CreateLabel("", "");
        titleLabel.text = Localization.Get("map_golden_egg");
        titleLabel.style.fontSize = 34;
        titleLabel.style.color = new Color(1f, 0.84f, 0f);
        titleLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        UIHelper.ApplyFontBold(titleLabel);
        textContainer.Add(titleLabel);

        var hintLabel = UIHelper.CreateLabel("", "");
        hintLabel.text = Localization.Get("map_golden_egg_hint");
        hintLabel.style.fontSize = 26;
        hintLabel.style.color = Color.white;
        hintLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        hintLabel.style.marginTop = 12;
        textContainer.Add(hintLabel);

        var tapHint = UIHelper.CreateLabel("\u25BC \u30BF\u30C3\u30D7\u3067\u3068\u3058\u308B", "");
        tapHint.style.fontSize = 22;
        tapHint.style.color = new Color(1f, 1f, 1f, 0.6f);
        tapHint.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
        tapHint.style.marginTop = 8;
        textContainer.Add(tapHint);

        // テキストフェードイン
        textContainer.style.opacity = 0f;
        elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            textContainer.style.opacity = Mathf.Clamp01(elapsed / 0.4f);
            yield return null;
        }

        // タップ待ち
        yield return new WaitForSeconds(0.3f);
        bool tapped = false;
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        while (!tapped) yield return null;

        // --- Phase 4: 卵が縮んで消える + フェードアウト ---
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.5f);
            float s = 1f - t;
            eggEl.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(s, s, 1f)));
            eggEl.style.opacity = 1f - t;
            glowEl.style.opacity = 1f - t;
            textContainer.style.opacity = 1f - t;
            float bgA = 0.6f * (1f - t);
            overlay.style.backgroundColor = new Color(0, 0, 0, bgA);
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        menuOpen = false;
    }

    IEnumerator SpawnEggSparkle(UIE.VisualElement parent, Color color)
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float dist = UnityEngine.Random.Range(60f, 160f);
        float startX = Mathf.Cos(angle) * dist * 0.3f;
        float startY = Mathf.Sin(angle) * dist * 0.3f;
        float endX = Mathf.Cos(angle) * dist;
        float endY = Mathf.Sin(angle) * dist;
        float size = UnityEngine.Random.Range(6f, 14f);

        var sp = new UIE.VisualElement();
        sp.style.position = UIE.Position.Absolute;
        sp.style.width = size;
        sp.style.height = size;
        sp.style.left = UIE.Length.Percent(50);
        sp.style.top = UIE.Length.Percent(50);
        sp.style.borderTopLeftRadius = size / 2;
        sp.style.borderTopRightRadius = size / 2;
        sp.style.borderBottomLeftRadius = size / 2;
        sp.style.borderBottomRightRadius = size / 2;
        sp.style.backgroundColor = color;
        parent.Add(sp);

        float dur = UnityEngine.Random.Range(0.6f, 1.2f);
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dur);
            float x = Mathf.Lerp(startX, endX, t);
            float y = Mathf.Lerp(startY, endY, t);
            sp.style.translate = new UIE.StyleTranslate(new UIE.Translate(x - size / 2, y - size / 2));
            sp.style.opacity = t < 0.3f ? t / 0.3f : 1f - (t - 0.3f) / 0.7f;
            float sc = t < 0.3f ? 0.5f + 0.5f * (t / 0.3f) : 1f - 0.5f * ((t - 0.3f) / 0.7f);
            sp.style.scale = new UIE.StyleScale(new UIE.Scale(new Vector3(sc, sc, 1f)));
            yield return null;
        }
        sp.RemoveFromHierarchy();
    }

    // ===== ミルクポイント（回復） =====

    void CreateMilkPoint()
    {
        if (tilesContainer == null) return;

        milkPointObj = new GameObject("MilkPoint");
        milkPointObj.transform.SetParent(tilesContainer.transform, false);

        var rect = milkPointObj.AddComponent<RectTransform>();
        float posX = (milkPointX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (milkPointY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        // タップ用の透明ボタン背景
        var btnImg = milkPointObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);  // 透明だがraycast受け取り可能
        var btn = milkPointObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnMilkPointTapped());

        // NPC描画（ミルクの母さん）
        DrawMilkNPC(milkPointObj.transform, 0.8f);

        milkPointObj.transform.SetAsLastSibling();
    }

    void CreateWaterfallOverlay()
    {
        if (tilesContainer == null || thirdWaterfallSprite == null) return;

        waterfallObj = new GameObject("Waterfall");
        waterfallObj.transform.SetParent(tilesContainer.transform, false);

        var rect = waterfallObj.AddComponent<RectTransform>();
        float posX = (waterfallX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (waterfallY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE * 2f, DISPLAY_TILE * 2f);

        // 滝画像
        var img = waterfallObj.AddComponent<Image>();
        img.sprite = thirdWaterfallSprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        // キラキラ星（小さな✦を複数配置、各々異なる位相で明滅）
        waterfallStars = new TMPro.TextMeshProUGUI[6];
        Vector2[] starPositions = {
            new Vector2(-20, 30), new Vector2(25, 15), new Vector2(-10, -20),
            new Vector2(15, -10), new Vector2(-25, 5), new Vector2(5, 25)
        };
        for (int i = 0; i < waterfallStars.Length; i++)
        {
            var starObj = new GameObject("Star" + i);
            starObj.transform.SetParent(waterfallObj.transform, false);
            var sRect = starObj.AddComponent<RectTransform>();
            sRect.anchoredPosition = starPositions[i];
            sRect.sizeDelta = new Vector2(30, 30);
            var star = starObj.AddComponent<TMPro.TextMeshProUGUI>();
            FontHelper.Apply(star);
            star.text = "*";
            star.fontSize = 18 + (i % 3) * 4;
            star.alignment = TMPro.TextAlignmentOptions.Center;
            star.color = new Color(1f, 0.84f, 0f, 0f);
            star.raycastTarget = false;
            waterfallStars[i] = star;
        }

        // ミルク獲得テキスト（上に浮かぶ）
        var textObj = new GameObject("WaterfallMilkText");
        textObj.transform.SetParent(waterfallObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, DISPLAY_TILE * 1.2f);
        textRect.sizeDelta = new Vector2(DISPLAY_TILE * 3f, 60);
        waterfallMilkText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        FontHelper.Apply(waterfallMilkText);
        waterfallMilkText.fontSize = 28;
        waterfallMilkText.alignment = TMPro.TextAlignmentOptions.Center;
        waterfallMilkText.color = new Color(1f, 0.84f, 0f);
        waterfallMilkText.text = "";
        waterfallMilkText.raycastTarget = false;

        waterfallMilkTimer = 0f;
        waterfallMilkGained = 0;

        waterfallObj.transform.SetAsLastSibling();
    }

    void UpdateWaterfall()
    {
        if (waterfallObj == null) return;

        // キラキラ星アニメーション（常時、各星が異なる位相で明滅）
        if (waterfallStars != null)
        {
            for (int i = 0; i < waterfallStars.Length; i++)
            {
                if (waterfallStars[i] == null) continue;
                float phase = i * Mathf.PI * 2f / waterfallStars.Length;
                float alpha = (Mathf.Sin(Time.time * 2.5f + phase) + 1f) * 0.5f;
                waterfallStars[i].color = new Color(1f, 0.84f, 0f, alpha * 0.9f);
            }
        }

        // プレイヤーが滝の上にいるかチェック
        bool onWaterfall = (playerTileX == waterfallX && playerTileY == waterfallY);
        if (onWaterfall && !menuOpen && DataCarrier.Instance != null)
        {
            waterfallMilkTimer += Time.deltaTime;
            if (waterfallMilkTimer >= 1f)
            {
                waterfallMilkTimer -= 1f;
                int gain = 2;
                DataCarrier.Instance.milk += gain;
                DataCarrier.Instance.SaveData();
                waterfallMilkGained += gain;
                UpdateStatusText();
            }
            if (waterfallMilkText != null)
                waterfallMilkText.text = waterfallMilkGained > 0
                    ? "+" + waterfallMilkGained + " 🍼"
                    : "";
        }
        else
        {
            if (waterfallMilkGained > 0)
            {
                waterfallMilkGained = 0;
                if (waterfallMilkText != null)
                    waterfallMilkText.text = "";
            }
            waterfallMilkTimer = 0f;
        }
    }

    // ===== 金の卵の老人NPC =====

    void CreateGoldenEggOldMan()
    {
        if (tilesContainer == null) return;
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area != 3) return;

        goldenEggOldManObj = new GameObject("GoldenEggOldMan");
        goldenEggOldManObj.transform.SetParent(tilesContainer.transform, false);

        var rect = goldenEggOldManObj.AddComponent<RectTransform>();
        float posX = (goldenEggOldManX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (goldenEggOldManY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        int state = DataCarrier.Instance != null ? DataCarrier.Instance.goldenEggOldManState : 0;
        string spriteName = state >= 1 ? "MapCharacters/gold_ikemen" : "MapCharacters/gold_men";
        var spr = Resources.Load<Sprite>(spriteName);
        var btnImg = goldenEggOldManObj.AddComponent<Image>();
        if (spr != null)
        {
            btnImg.sprite = spr;
            btnImg.preserveAspect = true;
        }
        else
            btnImg.color = new Color(0, 0, 0, 0);

        var btn = goldenEggOldManObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnGoldenEggOldManTapped());

        // 変身済みなら金パーティクル追加
        if (state >= 1)
            AddOldManGoldenStars(goldenEggOldManObj.transform, 0.8f);

        goldenEggOldManObj.transform.SetAsLastSibling();
    }

    void AddOldManGoldenStars(Transform parent, float scale)
    {
        float s = scale;
        oldManStars = new TMPro.TextMeshProUGUI[4];
        Vector2[] starPos = {
            new Vector2(-22 * s, 20 * s), new Vector2(22 * s, 10 * s),
            new Vector2(-15 * s, -25 * s), new Vector2(18 * s, -15 * s)
        };
        for (int i = 0; i < oldManStars.Length; i++)
        {
            var starObj = new GameObject("OldManStar" + i);
            starObj.transform.SetParent(parent, false);
            var sRect = starObj.AddComponent<RectTransform>();
            sRect.anchoredPosition = starPos[i];
            sRect.sizeDelta = new Vector2(20, 20);
            var star = starObj.AddComponent<TMPro.TextMeshProUGUI>();
            FontHelper.Apply(star);
            star.text = "*";
            star.fontSize = 14 + (i % 2) * 4;
            star.alignment = TMPro.TextAlignmentOptions.Center;
            star.color = new Color(1f, 0.84f, 0f, 0f);
            star.raycastTarget = false;
            oldManStars[i] = star;
        }
    }

    void UpdateGoldenEggOldMan()
    {
        if (oldManStars == null) return;
        for (int i = 0; i < oldManStars.Length; i++)
        {
            if (oldManStars[i] == null) continue;
            float phase = i * Mathf.PI * 2f / oldManStars.Length;
            float alpha = (Mathf.Sin(Time.time * 2.5f + phase) + 1f) * 0.5f;
            oldManStars[i].color = new Color(1f, 0.84f, 0f, alpha * 0.9f);
        }
    }

    void OnGoldenEggOldManTapped()
    {
        if (goldenEggOldManDialogueActive || menuOpen) return;
        StartCoroutine(ShowGoldenEggOldManDialogue());
    }

    void CheckGoldenEggOldMan()
    {
        if (goldenEggOldManObj == null || goldenEggOldManDialogueActive) return;
        if (playerTileX == goldenEggOldManX && playerTileY == goldenEggOldManY)
            OnGoldenEggOldManTapped();
    }

    IEnumerator ShowGoldenEggOldManDialogue()
    {
        goldenEggOldManDialogueActive = true;
        menuOpen = true;

        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc("金の卵の老人");

        int state = DataCarrier.Instance != null ? DataCarrier.Instance.goldenEggOldManState : 0;
        bool hasEgg = DataCarrier.Instance != null && DataCarrier.Instance.HasItem("金のたまご2");

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (elapsed / 0.3f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // NPC画像
        string portraitPath = state >= 1 ? "MapCharacters/gold_ikemen" : "MapCharacters/gold_men";
        var portraitSpr = Resources.Load<Sprite>(portraitPath);
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        if (portraitSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(portraitSpr);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("金の卵の老人", "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        if (state >= 1)
        {
            // 変身済み — 一言だけ
            textLabel.text = Localization.Get("oldman_after");
            tapHint.text = "▼ タップで閉じる";
            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }
        else if (!hasEgg)
        {
            // 卵なし — 震える台詞
            textLabel.text = Localization.Get("oldman_wait");
            tapHint.text = "▼ タップで閉じる";
            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }
        else
        {
            // 卵所持 — イベント開始
            string[] msgs = {
                Localization.Get("oldman_wait"),
                Localization.Get("oldman_notice"),
                Localization.Get("oldman_request")
            };

            for (int i = 0; i < msgs.Length; i++)
            {
                textLabel.text = msgs[i];
                tapHint.text = "▼ タップで続く";
                yield return new WaitForSeconds(0.3f);
                bool t = false;
                overlay.RegisterCallback<UIE.ClickEvent>(evt => t = true);
                while (!t) yield return null;
            }

            // 選択肢表示
            tapHint.style.display = UIE.DisplayStyle.None;
            textLabel.style.display = UIE.DisplayStyle.None;

            var choiceBox = new UIE.VisualElement();
            choiceBox.style.flexDirection = UIE.FlexDirection.Column;
            choiceBox.style.alignItems = UIE.Align.Center;
            choiceBox.style.marginTop = 20;
            dialogBox.Add(choiceBox);

            int choice = -1;

            var yesBtn = UIHelper.CreatePillButton(Localization.Get("oldman_choice_yes"), "map-save-close-btn");
            yesBtn.style.marginBottom = 12;
            yesBtn.clicked += () => choice = 1;
            choiceBox.Add(yesBtn);

            var noBtn = UIHelper.CreatePillButton(Localization.Get("oldman_choice_no"), "map-save-close-btn");
            noBtn.clicked += () => choice = 0;
            choiceBox.Add(noBtn);

            while (choice < 0) yield return null;

            choiceBox.RemoveFromHierarchy();
            tapHint.style.display = UIE.DisplayStyle.Flex;
            textLabel.style.display = UIE.DisplayStyle.Flex;

            if (choice == 0)
            {
                // やめとく — 閉じる
            }
            else
            {
                // 装着演出 — 暗転
                float darkElapsed = 0f;
                while (darkElapsed < 0.5f)
                {
                    darkElapsed += Time.deltaTime;
                    overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.5f, 0.85f, darkElapsed / 0.5f));
                    yield return null;
                }

                yield return new WaitForSeconds(0.3f);

                // 老人のスプライトを差し替え（gold_men → gold_ikemen）
                if (goldenEggOldManObj != null)
                {
                    var ikemenSpr = Resources.Load<Sprite>("MapCharacters/gold_ikemen");
                    var img = goldenEggOldManObj.GetComponent<Image>();
                    if (ikemenSpr != null && img != null)
                        img.sprite = ikemenSpr;
                    AddOldManGoldenStars(goldenEggOldManObj.transform, 0.8f);
                }

                // 明転
                darkElapsed = 0f;
                while (darkElapsed < 0.5f)
                {
                    darkElapsed += Time.deltaTime;
                    overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.85f, 0.5f, darkElapsed / 0.5f));
                    yield return null;
                }

                // 装着後台詞
                string[] afterMsgs = {
                    Localization.Get("oldman_power"),
                    Localization.Get("oldman_reward")
                };
                for (int i = 0; i < afterMsgs.Length; i++)
                {
                    textLabel.text = afterMsgs[i];
                    tapHint.text = (i < afterMsgs.Length - 1) ? "▼ タップで続く" : "▼ タップで閉じる";
                    yield return new WaitForSeconds(0.3f);
                    bool t = false;
                    overlay.RegisterCallback<UIE.ClickEvent>(evt => t = true);
                    while (!t) yield return null;
                }

                // 報酬付与
                if (DataCarrier.Instance != null)
                {
                    DataCarrier.Instance.RemoveItem("金のたまご2");
                    DataCarrier.Instance.AddEquipment("ゴールデン・ベビーステッキ");
                    DataCarrier.Instance.EquipItem("ゴールデン・ベビーステッキ");
                    DataCarrier.Instance.goldenEggOldManState = 1;
                    DataCarrier.Instance.SaveData();
                }

                // ステッキ画像表示
                var stickTex = Resources.Load<Texture2D>("Shop/ゴールデンベビーステッキ");
                if (stickTex != null)
                {
                    var stickSprite = Sprite.Create(stickTex, new Rect(0, 0, stickTex.width, stickTex.height), new Vector2(0.5f, 0.5f));
                    var stickImg = new UIE.VisualElement();
                    stickImg.style.width = 250;
                    stickImg.style.height = 250;
                    stickImg.style.backgroundImage = new UIE.StyleBackground(stickSprite);
                    stickImg.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    stickImg.style.alignSelf = UIE.Align.Center;
                    stickImg.style.marginBottom = 16;
                    dialogBox.Insert(dialogBox.IndexOf(textLabel), stickImg);
                }

                // 取得メッセージ表示
                textLabel.text = Localization.Get("oldman_got_item");
                tapHint.text = "▼ タップで閉じる";
                yield return new WaitForSeconds(0.3f);
                bool finalTap = false;
                overlay.RegisterCallback<UIE.ClickEvent>(evt => finalTap = true);
                while (!finalTap) yield return null;
            }
        }

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.3f);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * fadeT);
            portraitEl.style.opacity = fadeT;
            dialogBox.style.opacity = fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        menuOpen = false;
        goldenEggOldManDialogueActive = false;
    }

    // ===== かぐやちゃんNPC =====
    bool kaguyaPoolHidden = false; // 3回目: プールに潜んでいる

    void CreateKaguyaNPC()
    {
        if (tilesContainer == null || DataCarrier.Instance == null) return;

        int area = DataCarrier.Instance.currentArea;
        int metCount = DataCarrier.Instance.kaguyaMetCount;
        bool lover = DataCarrier.Instance.kaguyaLover;
        kaguyaPoolHidden = false;

        // 表示条件判定 & 座標決定
        bool shouldShow = false;
        if (lover)
        {
            // 相思相愛後 → フォロワーとしてついてくるのでNPC配置不要
            return;
        }
        else
        {
            // かぐやは教室（area 7）の端っこにだけ隠れている
            if (metCount < 2 && area == 7)
            {
                kaguyaNpcX = 10; kaguyaNpcY = 1;
                shouldShow = true;
            }
            else if (metCount >= 2 && area == 3)
            {
                // 3回目以降（バトル勝利まで）: プールに潜んでいる → NPC非表示、プール横で判定
                kaguyaPoolHidden = true;
                return;
            }
        }

        if (!shouldShow) return;

        kaguyaNpcObj = new GameObject("KaguyaNPC");
        kaguyaNpcObj.transform.SetParent(tilesContainer.transform, false);

        var rect = kaguyaNpcObj.AddComponent<RectTransform>();
        float posX = (kaguyaNpcX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (kaguyaNpcY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        // 教室(area7)では端っこに隠れている = 半分サイズ
        float kaguyaScale = (area == 7) ? 0.5f : 1f;
        rect.sizeDelta = new Vector2(DISPLAY_TILE * kaguyaScale, DISPLAY_TILE * kaguyaScale);

        var btnImg = kaguyaNpcObj.AddComponent<Image>();
        // metCountに応じた画像を表示 (0→kaguya1, 1→kaguya2, lover→kaguya3, Area3→kaguya_pool)
        string kaguyaSprName;
        if (lover) kaguyaSprName = "kaguya3";
        else if (area == 3) kaguyaSprName = "kaguya_pool";
        else if (metCount <= 0) kaguyaSprName = "kaguya1";
        else kaguyaSprName = "kaguya2";
        var kaguyaSpr = Resources.Load<Sprite>("MapCharacters/heroine/" + kaguyaSprName);
        if (kaguyaSpr != null)
        {
            btnImg.sprite = kaguyaSpr;
            btnImg.preserveAspect = true;
            btnImg.color = Color.white;
        }
        else
        {
            btnImg.color = new Color(0, 0, 0, 0);
        }
        // タップ領域を広げる（半分サイズでもタップしやすく）
        var tapArea = new GameObject("KaguyaTapArea");
        tapArea.transform.SetParent(kaguyaNpcObj.transform, false);
        var tapRect = tapArea.AddComponent<RectTransform>();
        tapRect.anchoredPosition = Vector2.zero;
        tapRect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);
        var tapImg = tapArea.AddComponent<Image>();
        tapImg.color = new Color(0, 0, 0, 0); // 透明
        tapImg.raycastTarget = true;
        var btn = tapArea.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnKaguyaTapped());

        // 相思相愛後はハートを周囲にふわふわ
        if (lover)
            AddKaguyaLoverHearts(kaguyaNpcObj.transform, 0.8f);

        kaguyaNpcObj.transform.SetAsLastSibling();
    }

    void AddKaguyaLoverHearts(Transform parent, float scale)
    {
        float s = scale;
        kaguyaHearts = new TMPro.TextMeshProUGUI[3];
        Vector2[] heartPos = {
            new Vector2(-20 * s, 25 * s), new Vector2(22 * s, 18 * s), new Vector2(0, 35 * s)
        };
        for (int i = 0; i < kaguyaHearts.Length; i++)
        {
            var heartObj = new GameObject("KaguyaHeart" + i);
            heartObj.transform.SetParent(parent, false);
            var hRect = heartObj.AddComponent<RectTransform>();
            hRect.anchoredPosition = heartPos[i];
            hRect.sizeDelta = new Vector2(20, 20);
            var heart = heartObj.AddComponent<TMPro.TextMeshProUGUI>();
            FontHelper.Apply(heart);
            heart.text = "\u2764";
            heart.fontSize = 12 + (i % 2) * 4;
            heart.alignment = TMPro.TextAlignmentOptions.Center;
            heart.color = new Color(1f, 0.4f, 0.6f, 0f);
            heart.raycastTarget = false;
            kaguyaHearts[i] = heart;
        }
    }

    void UpdateKaguyaHearts()
    {
        if (kaguyaHearts == null) return;
        for (int i = 0; i < kaguyaHearts.Length; i++)
        {
            if (kaguyaHearts[i] == null) continue;
            float phase = i * Mathf.PI * 2f / kaguyaHearts.Length;
            float alpha = (Mathf.Sin(Time.time * 2f + phase) + 1f) * 0.5f;
            kaguyaHearts[i].color = new Color(1f, 0.4f, 0.6f, alpha * 0.8f);
        }
        // プールの泡ヒントアニメーション
        if (poolBubbles != null)
        {
            for (int i = 0; i < poolBubbles.Length; i++)
            {
                if (poolBubbles[i] == null) continue;
                float phase = i * Mathf.PI * 2f / poolBubbles.Length;
                float alpha = (Mathf.Sin(Time.time * 1.5f + phase) + 1f) * 0.5f;
                poolBubbles[i].color = new Color(0.8f, 0.95f, 1f, alpha * 0.6f);
                var rt = poolBubbles[i].rectTransform;
                rt.anchoredPosition += new Vector2(0, Time.deltaTime * 8f);
            }
        }
    }

    void OnKaguyaTapped()
    {
        if (kaguyaDialogueActive || menuOpen) return;
        StartCoroutine(ShowKaguyaDialogue());
    }

    void CheckKaguyaNPC()
    {
        // 3回目: プールに潜んでいるパターン
        if (kaguyaPoolHidden && !kaguyaDialogueActive)
        {
            // プール付近（x=5-6, y=33-37）を通ったらトリガー
            // プール=x1-4,y34-36、街道=x6,y32-37
            if (playerTileX >= 5 && playerTileX <= 6 && playerTileY >= 33 && playerTileY <= 37)
            {
                kaguyaPoolHidden = false; // 一度トリガーしたら再発火しない
                StartCoroutine(ShowPoolDiveSequence());
            }
            return;
        }

        if (kaguyaNpcObj == null || kaguyaDialogueActive) return;
        // 同じタイルまたは隣接タイルで発動（半分サイズで踏めない場合の対策）
        int dx = Mathf.Abs(playerTileX - kaguyaNpcX);
        int dy = Mathf.Abs(playerTileY - kaguyaNpcY);
        if (dx + dy <= 1)
            OnKaguyaTapped();
    }

    IEnumerator ShowKaguyaDialogue()
    {
        kaguyaDialogueActive = true;
        menuOpen = true;

        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc("かぐやちゃん");

        int metCount = DataCarrier.Instance != null ? DataCarrier.Instance.kaguyaMetCount : 0;
        bool lover = DataCarrier.Instance != null && DataCarrier.Instance.kaguyaLover;

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (elapsed / 0.3f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("かぐやちゃん", "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("\u25bc \u30bf\u30c3\u30d7\u3067\u7d9a\u304f", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        if (lover)
        {
            // 相思相愛後
            textLabel.text = Localization.Get("kaguya_lover");
            tapHint.text = "\u25bc \u30bf\u30c3\u30d7\u3067\u9589\u3058\u308b";
            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }
        else
        {
            // かくれんぼ台詞
            string[] msgs;
            if (metCount == 0)
                msgs = new string[] {
                    Localization.Get("kaguya_meet1_1"),
                    Localization.Get("kaguya_meet1_2"),
                    Localization.Get("kaguya_meet1_3")
                };
            else if (metCount == 1)
                msgs = new string[] {
                    Localization.Get("kaguya_meet2_1"),
                    Localization.Get("kaguya_meet2_2"),
                    Localization.Get("kaguya_meet2_3")
                };
            else
                msgs = new string[] {
                    Localization.Get("kaguya_meet3_1"),
                    Localization.Get("kaguya_meet3_2"),
                    Localization.Get("kaguya_meet3_3")
                };

            for (int i = 0; i < msgs.Length; i++)
            {
                textLabel.text = msgs[i];
                tapHint.text = (i < msgs.Length - 1) ? "\u25bc \u30bf\u30c3\u30d7\u3067\u7d9a\u304f" : "\u25bc \u30bf\u30c3\u30d7\u3067\u9589\u3058\u308b";
                yield return new WaitForSeconds(0.3f);
                bool t = false;
                overlay.RegisterCallback<UIE.ClickEvent>(evt => t = true);
                while (!t) yield return null;
            }

            // 状態更新
            if (DataCarrier.Instance != null)
            {
                DataCarrier.Instance.kaguyaMetCount = metCount + 1;
                DataCarrier.Instance.SaveData();
            }

            if (metCount == 2)
            {
                // 3回目 → バトル突入
                // フェードアウトしてバトルへ
                elapsed = 0f;
                while (elapsed < 0.3f)
                {
                    elapsed += Time.deltaTime;
                    float fadeT = 1f - (elapsed / 0.3f);
                    overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * fadeT);
                    dialogBox.style.opacity = fadeT;
                    yield return null;
                }
                overlay.RemoveFromHierarchy();
                kaguyaDialogueActive = false;
                menuOpen = false;
                StartCoroutine(StartFixedEncounter("かぐやちゃん"));
                yield break;
            }

            // 1回目・2回目 → 消える演出
            if (kaguyaNpcObj != null)
            {
                // ハートエフェクト（uGUI側）
                for (int h = 0; h < 3; h++)
                {
                    var heartObj = new GameObject("DisappearHeart" + h);
                    heartObj.transform.SetParent(kaguyaNpcObj.transform, false);
                    var hRect = heartObj.AddComponent<RectTransform>();
                    hRect.anchoredPosition = new Vector2(Random.Range(-20f, 20f), Random.Range(10f, 35f));
                    hRect.sizeDelta = new Vector2(25, 25);
                    var heartTmp = heartObj.AddComponent<TMPro.TextMeshProUGUI>();
                    FontHelper.Apply(heartTmp);
                    heartTmp.text = "\u2764";
                    heartTmp.fontSize = 16;
                    heartTmp.alignment = TMPro.TextAlignmentOptions.Center;
                    heartTmp.color = new Color(1f, 0.4f, 0.6f, 0.9f);
                    heartTmp.raycastTarget = false;
                }

                yield return new WaitForSeconds(0.5f);

                Destroy(kaguyaNpcObj);
                kaguyaNpcObj = null;
            }
        }

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.3f);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * fadeT);
            dialogBox.style.opacity = fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        menuOpen = false;
        kaguyaDialogueActive = false;
    }

    // ===== 3回目: プール飛び込み演出 =====
    IEnumerator ShowPoolDiveSequence()
    {
        kaguyaDialogueActive = true;
        menuOpen = true;

        // --- 選択肢: プールに飛び込む？ ---
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (elapsed / 0.3f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // ダイアログ: 「プールの中に何かが光っている……」
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var textLabel = UIHelper.CreateLabel(Localization.Get("kaguya_pool_notice"), "milk-dialog-text");
        dialogBox.Add(textLabel);

        // 選択ボタン（縦並び）
        var btnCol = new UIE.VisualElement();
        btnCol.style.flexDirection = UIE.FlexDirection.Column;
        btnCol.style.alignItems = UIE.Align.Center;
        btnCol.style.marginTop = 16;
        dialogBox.Add(btnCol);

        int choice = -1;
        var diveBtn = UIHelper.CreatePillButton(Localization.Get("kaguya_pool_dive"), "pill-button");
        diveBtn.clicked += () => choice = 1;
        btnCol.Add(diveBtn);

        var leaveBtn = UIHelper.CreatePillButton(Localization.Get("kaguya_pool_leave"), "pill-button");
        leaveBtn.style.marginTop = 12;
        leaveBtn.clicked += () => choice = 0;
        btnCol.Add(leaveBtn);

        while (choice < 0) yield return null;

        if (choice == 0)
        {
            // やめておく → 閉じる（再トリガー可にする）
            kaguyaPoolHidden = true;

            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (1f - elapsed / 0.3f));
                dialogBox.style.opacity = 1f - elapsed / 0.3f;
                yield return null;
            }
            overlay.RemoveFromHierarchy();
            menuOpen = false;
            kaguyaDialogueActive = false;
            yield break;
        }

        // --- 飛び込み演出 ---
        dialogBox.RemoveFromHierarchy();

        // 画面が青くフラッシュ（ざぶーん！）
        var splashLabel = UIHelper.CreateLabel(Localization.Get("kaguya_pool_splash"));
        splashLabel.style.fontSize = 48;
        splashLabel.style.color = Color.white;
        splashLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        UIHelper.ApplyFontBold(splashLabel);
        overlay.Add(splashLabel);

        Color waterBlue = new Color(0.2f, 0.55f, 0.85f);
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.5f;
            overlay.style.backgroundColor = Color.Lerp(new Color(0, 0, 0, 0.5f), new Color(waterBlue.r, waterBlue.g, waterBlue.b, 0.9f), t);
            yield return null;
        }

        // 泡ぶくぶく演出
        var bubbles = new UIE.VisualElement[12];
        for (int i = 0; i < bubbles.Length; i++)
        {
            var bub = new UIE.VisualElement();
            float size = Random.Range(12f, 30f);
            bub.style.width = size;
            bub.style.height = size;
            bub.style.borderTopLeftRadius = size;
            bub.style.borderTopRightRadius = size;
            bub.style.borderBottomLeftRadius = size;
            bub.style.borderBottomRightRadius = size;
            bub.style.backgroundColor = new Color(0.7f, 0.9f, 1f, 0.5f);
            bub.style.position = UIE.Position.Absolute;
            bub.style.left = Random.Range(50f, 550f);
            bub.style.top = Random.Range(400f, 900f);
            overlay.Add(bub);
            bubbles[i] = bub;
        }

        splashLabel.text = "";
        yield return new WaitForSeconds(0.4f);

        // 泡が上に浮かぶアニメーション
        elapsed = 0f;
        while (elapsed < 1.2f)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (bubbles[i] == null) continue;
                float speed = 80f + i * 20f;
                var pos = bubbles[i].resolvedStyle.top;
                bubbles[i].style.top = pos - speed * Time.deltaTime;
                float wobble = Mathf.Sin(Time.time * 3f + i) * 1.5f;
                bubbles[i].style.left = bubbles[i].resolvedStyle.left + wobble;
            }
            yield return null;
        }

        // 泡を消す
        foreach (var b in bubbles) b?.RemoveFromHierarchy();

        // --- 水中でかぐや発見！ ---
        // かぐやの画像を画面いっぱいに表示
        var kaguyaContainer = new UIE.VisualElement();
        kaguyaContainer.AddToClassList("fill");
        kaguyaContainer.style.alignItems = UIE.Align.Center;
        kaguyaContainer.style.justifyContent = UIE.Justify.Center;
        kaguyaContainer.style.opacity = 0f;
        overlay.Add(kaguyaContainer);

        var kaguyaImg = new UIE.VisualElement();
        var kSpr = Resources.Load<Sprite>("MapCharacters/heroine/kaguya_pool");
        if (kSpr != null)
        {
            kaguyaImg.style.backgroundImage = new UIE.StyleBackground(kSpr);
            kaguyaImg.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        kaguyaImg.style.width = new UIE.Length(100, UIE.LengthUnit.Percent);
        kaguyaImg.style.height = new UIE.Length(100, UIE.LengthUnit.Percent);
        kaguyaImg.style.flexGrow = 1;
        kaguyaContainer.Add(kaguyaImg);

        // ゆらゆらフェードイン
        elapsed = 0f;
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 1f;
            kaguyaContainer.style.opacity = t;
            float sway = Mathf.Sin(Time.time * 2f) * 8f;
            kaguyaImg.style.translate = new UIE.Translate(new UIE.Length(sway), 0);
            yield return null;
        }
        kaguyaContainer.style.opacity = 1f;

        // 吹き出し（画像の上に重ねて表示）
        var bubble = new UIE.VisualElement();
        bubble.style.position = UIE.Position.Absolute;
        bubble.style.bottom = 120;
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
        kaguyaContainer.Add(bubble);

        // 吹き出しのしっぽ（三角）
        var bubbleTail = new UIE.VisualElement();
        bubbleTail.style.position = UIE.Position.Absolute;
        bubbleTail.style.bottom = 106;
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
        kaguyaContainer.Add(bubbleTail);

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
        bubbleHint.style.unityTextAlign = TextAnchor.MiddleRight;
        bubbleHint.style.alignSelf = UIE.Align.FlexEnd;
        bubble.Add(bubbleHint);

        // 台詞（吹き出しで順番に表示）
        string[] msgs = new string[] {
            Localization.Get("kaguya_pool_found"),
            Localization.Get("kaguya_pool_line1"),
            Localization.Get("kaguya_meet3_2"),
            Localization.Get("kaguya_meet3_3")
        };

        // ハートを散りばめる
        var heartAnims = new System.Collections.Generic.List<UIE.VisualElement>();
        for (int hi = 0; hi < 15; hi++)
        {
            var heart = new UIE.Label();
            heart.text = "\u2764";
            heart.style.position = UIE.Position.Absolute;
            heart.style.fontSize = Random.Range(18, 36);
            heart.style.color = new Color(1f, Random.Range(0.3f, 0.6f), Random.Range(0.5f, 0.8f), 0f);
            heart.style.left = Random.Range(20f, 580f);
            heart.style.top = Random.Range(100f, 800f);
            kaguyaContainer.Add(heart);
            heartAnims.Add(heart);
        }
        StartCoroutine(AnimatePoolHearts(heartAnims));

        bool tapped = false;
        for (int i = 0; i < msgs.Length; i++)
        {
            bubbleText.text = msgs[i];
            bubbleHint.text = (i < msgs.Length - 1) ? "\u25bc \u30bf\u30c3\u30d7\u3067\u7d9a\u304f" : "\u25bc \u30bf\u30c3\u30d7\u3067\u9589\u3058\u308b";
            yield return new WaitForSeconds(0.3f);
            tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }

        // ハートを消す
        foreach (var h in heartAnims) h?.RemoveFromHierarchy();

        // 状態更新
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.kaguyaMetCount = 3;
            DataCarrier.Instance.SaveData();
        }

        // フェードアウトしてバトルへ
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.3f);
            overlay.style.opacity = fadeT;
            yield return null;
        }
        overlay.RemoveFromHierarchy();
        kaguyaDialogueActive = false;
        menuOpen = false;
        StartCoroutine(StartFixedEncounter("かぐやちゃん"));
    }

    IEnumerator AnimatePoolHearts(System.Collections.Generic.List<UIE.VisualElement> hearts)
    {
        float duration = 8f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < hearts.Count; i++)
            {
                var h = hearts[i];
                if (h == null || h.parent == null) continue;
                float phase = i * Mathf.PI * 2f / hearts.Count;
                float alpha = (Mathf.Sin(Time.time * 1.5f + phase) + 1f) * 0.5f * 0.8f;
                float r = h.resolvedStyle.color.r;
                float g = h.resolvedStyle.color.g;
                float b = h.resolvedStyle.color.b;
                h.style.color = new Color(r, g, b, alpha);
                float drift = Mathf.Sin(Time.time * 2f + phase) * 0.8f;
                h.style.top = h.resolvedStyle.top - Time.deltaTime * (15f + i * 2f);
                h.style.left = h.resolvedStyle.left + drift;
            }
            yield return null;
        }
    }

    void DrawBabyBottle(Transform parent, float scale)
    {
        float s = scale;
        // ボトル本体（白い長方形）
        var body = FacePart("BottleBody", parent, new Vector2(0, -6 * s), new Vector2(30 * s, 50 * s));
        body.AddComponent<Image>().color = new Color(0.95f, 0.95f, 1f, 0.95f);

        // ミルク部分（下半分、クリーム色）
        var milk = FacePart("Milk", parent, new Vector2(0, -18 * s), new Vector2(26 * s, 26 * s));
        milk.AddComponent<Image>().color = new Color(1f, 0.97f, 0.85f);

        // キャップ（ピンク）
        var cap = FacePart("Cap", parent, new Vector2(0, 22 * s), new Vector2(24 * s, 12 * s));
        cap.AddComponent<Image>().color = new Color(1f, 0.6f, 0.7f);

        // 乳首（ピンク、上部の丸い部分）
        var nipple = FacePart("Nipple", parent, new Vector2(0, 32 * s), new Vector2(14 * s, 14 * s));
        nipple.AddComponent<Image>().color = new Color(1f, 0.5f, 0.6f);

        // 目盛り線（2本）
        var line1 = FacePart("Line1", parent, new Vector2(-8 * s, -8 * s), new Vector2(8 * s, 2 * s));
        line1.AddComponent<Image>().color = new Color(0.7f, 0.85f, 1f, 0.6f);

        var line2 = FacePart("Line2", parent, new Vector2(-8 * s, -16 * s), new Vector2(8 * s, 2 * s));
        line2.AddComponent<Image>().color = new Color(0.7f, 0.85f, 1f, 0.6f);

        // ハートマーク（小さいピンクの四角でハート風）
        var heart = FacePart("Heart", parent, new Vector2(6 * s, 0), new Vector2(8 * s, 8 * s));
        heart.AddComponent<Image>().color = new Color(1f, 0.4f, 0.5f, 0.7f);
    }

    void DrawMilkNPC(Transform parent, float scale)
    {
        float s = scale;
        Color skin = new Color(0.98f, 0.89f, 0.82f);
        Color dress = new Color(0.55f, 0.3f, 0.5f);
        Color apron = new Color(1f, 0.96f, 0.92f);
        Color hair = new Color(0.35f, 0.22f, 0.12f);

        // 影
        FacePart("Shadow", parent, new Vector2(0, -34 * s), new Vector2(34 * s, 10 * s))
            .AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);

        // 足
        FacePart("FootL", parent, new Vector2(-6 * s, -30 * s), new Vector2(9 * s, 7 * s))
            .AddComponent<Image>().color = new Color(0.45f, 0.25f, 0.2f);
        FacePart("FootR", parent, new Vector2(6 * s, -30 * s), new Vector2(9 * s, 7 * s))
            .AddComponent<Image>().color = new Color(0.45f, 0.25f, 0.2f);

        // ロングドレス（胴体下部）
        FacePart("Skirt", parent, new Vector2(0, -20 * s), new Vector2(28 * s, 22 * s))
            .AddComponent<Image>().color = dress;

        // 胴体上部
        FacePart("Bodice", parent, new Vector2(0, -6 * s), new Vector2(26 * s, 18 * s))
            .AddComponent<Image>().color = dress;

        // エプロン
        FacePart("Apron", parent, new Vector2(0, -14 * s), new Vector2(20 * s, 26 * s))
            .AddComponent<Image>().color = apron;

        // 腕
        FacePart("ArmL", parent, new Vector2(-16 * s, -8 * s), new Vector2(7 * s, 16 * s))
            .AddComponent<Image>().color = new Color(0.65f, 0.4f, 0.6f);
        FacePart("ArmR", parent, new Vector2(16 * s, -8 * s), new Vector2(7 * s, 16 * s))
            .AddComponent<Image>().color = new Color(0.65f, 0.4f, 0.6f);

        // 手
        FacePart("HandL", parent, new Vector2(-16 * s, -18 * s), new Vector2(6 * s, 6 * s))
            .AddComponent<Image>().color = skin;
        FacePart("HandR", parent, new Vector2(16 * s, -18 * s), new Vector2(6 * s, 6 * s))
            .AddComponent<Image>().color = skin;

        // 頭
        FacePart("Head", parent, new Vector2(0, 14 * s), new Vector2(32 * s, 28 * s))
            .AddComponent<Image>().color = skin;

        // 髪（上）
        FacePart("HairTop", parent, new Vector2(0, 26 * s), new Vector2(36 * s, 14 * s))
            .AddComponent<Image>().color = hair;

        // 髪（サイド）
        FacePart("HairL", parent, new Vector2(-15 * s, 8 * s), new Vector2(8 * s, 20 * s))
            .AddComponent<Image>().color = hair;
        FacePart("HairR", parent, new Vector2(15 * s, 8 * s), new Vector2(8 * s, 20 * s))
            .AddComponent<Image>().color = hair;

        // 目（優しい半月型）
        FacePart("EyeL", parent, new Vector2(-7 * s, 14 * s), new Vector2(5 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);
        FacePart("EyeR", parent, new Vector2(7 * s, 14 * s), new Vector2(5 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);

        // 口（微笑み）
        FacePart("Smile", parent, new Vector2(0, 8 * s), new Vector2(7 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.85f, 0.5f, 0.55f);

        // ほっぺ（チーク）
        FacePart("CheekL", parent, new Vector2(-10 * s, 10 * s), new Vector2(6 * s, 4 * s))
            .AddComponent<Image>().color = new Color(1f, 0.7f, 0.7f, 0.5f);
        FacePart("CheekR", parent, new Vector2(10 * s, 10 * s), new Vector2(6 * s, 4 * s))
            .AddComponent<Image>().color = new Color(1f, 0.7f, 0.7f, 0.5f);
    }

    // ===== 門番（長老）NPC =====

    void CreateElderNPC()
    {
        if (tilesContainer == null) return;

        elderNpcObj = new GameObject("ElderNPC");
        elderNpcObj.transform.SetParent(tilesContainer.transform, false);

        var rect = elderNpcObj.AddComponent<RectTransform>();
        float posX = (elderNpcX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (elderNpcY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var btnImg = elderNpcObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);
        var btn = elderNpcObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnElderTapped());

        DrawElderNPC(elderNpcObj.transform, 0.8f);
        elderNpcObj.transform.SetAsLastSibling();
    }

    void DrawElderNPC(Transform parent, float scale)
    {
        float s = scale;
        Color skin = new Color(0.92f, 0.82f, 0.72f);
        Color robe = new Color(0.35f, 0.3f, 0.5f);
        Color robeLight = new Color(0.45f, 0.4f, 0.6f);
        Color hair = new Color(0.85f, 0.85f, 0.85f);
        Color beard = new Color(0.8f, 0.8f, 0.8f);

        // 影
        FacePart("Shadow", parent, new Vector2(0, -34 * s), new Vector2(36 * s, 10 * s))
            .AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);

        // 足
        FacePart("FootL", parent, new Vector2(-6 * s, -30 * s), new Vector2(9 * s, 7 * s))
            .AddComponent<Image>().color = new Color(0.35f, 0.25f, 0.15f);
        FacePart("FootR", parent, new Vector2(6 * s, -30 * s), new Vector2(9 * s, 7 * s))
            .AddComponent<Image>().color = new Color(0.35f, 0.25f, 0.15f);

        // ローブ（下部）
        FacePart("RobeLower", parent, new Vector2(0, -20 * s), new Vector2(30 * s, 22 * s))
            .AddComponent<Image>().color = robe;

        // ローブ（上部）
        FacePart("RobeUpper", parent, new Vector2(0, -6 * s), new Vector2(28 * s, 18 * s))
            .AddComponent<Image>().color = robe;

        // 帯（腰）
        FacePart("Belt", parent, new Vector2(0, -10 * s), new Vector2(30 * s, 5 * s))
            .AddComponent<Image>().color = new Color(0.6f, 0.5f, 0.2f);

        // 腕
        FacePart("ArmL", parent, new Vector2(-17 * s, -8 * s), new Vector2(7 * s, 16 * s))
            .AddComponent<Image>().color = robeLight;
        FacePart("ArmR", parent, new Vector2(17 * s, -8 * s), new Vector2(7 * s, 16 * s))
            .AddComponent<Image>().color = robeLight;

        // 手
        FacePart("HandL", parent, new Vector2(-17 * s, -18 * s), new Vector2(6 * s, 6 * s))
            .AddComponent<Image>().color = skin;
        FacePart("HandR", parent, new Vector2(17 * s, -18 * s), new Vector2(6 * s, 6 * s))
            .AddComponent<Image>().color = skin;

        // 杖
        FacePart("Staff", parent, new Vector2(22 * s, -6 * s), new Vector2(4 * s, 46 * s))
            .AddComponent<Image>().color = new Color(0.55f, 0.35f, 0.15f);
        FacePart("StaffGem", parent, new Vector2(22 * s, 18 * s), new Vector2(8 * s, 8 * s))
            .AddComponent<Image>().color = new Color(0.3f, 0.8f, 0.5f);

        // 頭
        FacePart("Head", parent, new Vector2(0, 14 * s), new Vector2(30 * s, 28 * s))
            .AddComponent<Image>().color = skin;

        // 髪（白髪、後ろ）
        FacePart("HairBack", parent, new Vector2(0, 26 * s), new Vector2(34 * s, 16 * s))
            .AddComponent<Image>().color = hair;

        // 眉（太い）
        FacePart("BrowL", parent, new Vector2(-8 * s, 20 * s), new Vector2(7 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
        FacePart("BrowR", parent, new Vector2(8 * s, 20 * s), new Vector2(7 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);

        // 目（小さく温かい）
        FacePart("EyeL", parent, new Vector2(-7 * s, 15 * s), new Vector2(4 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);
        FacePart("EyeR", parent, new Vector2(7 * s, 15 * s), new Vector2(4 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);

        // ヒゲ（白い）
        FacePart("Beard", parent, new Vector2(0, 4 * s), new Vector2(16 * s, 14 * s))
            .AddComponent<Image>().color = beard;
    }

    // ===== 実家（母親NPC） =====

    void CreateMotherNPC()
    {
        if (tilesContainer == null) return;

        motherNpcObj = new GameObject("MotherNPC");
        motherNpcObj.transform.SetParent(tilesContainer.transform, false);

        var rect = motherNpcObj.AddComponent<RectTransform>();
        float posX = (motherNpcX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (motherNpcY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var btnImg = motherNpcObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);
        var btn = motherNpcObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnMotherTapped());

        DrawMotherNPC(motherNpcObj.transform, 0.8f);
        motherNpcObj.transform.SetAsLastSibling();
    }

    void DrawMotherNPC(Transform parent, float scale)
    {
        // 母親のスプライトを読み込んで表示
        string motherName = DataCarrier.Instance != null ? DataCarrier.Instance.motherName : "";
        string imgKey = GetParentImageNameForMap(motherName);
        Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);

        if (sprite != null)
        {
            var obj = FacePart("MotherSprite", parent, Vector2.zero, new Vector2(DISPLAY_TILE * 0.9f, DISPLAY_TILE * 0.9f));
            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        else
        {
            // フォールバック: 簡易キャラ描画
            float s = scale;
            Color skin = new Color(0.95f, 0.85f, 0.75f);
            Color dress = new Color(0.75f, 0.35f, 0.45f);
            Color hair = new Color(0.25f, 0.15f, 0.1f);

            FacePart("Shadow", parent, new Vector2(0, -34 * s), new Vector2(36 * s, 10 * s))
                .AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);
            FacePart("Body", parent, new Vector2(0, -14 * s), new Vector2(26 * s, 30 * s))
                .AddComponent<Image>().color = dress;
            FacePart("Head", parent, new Vector2(0, 14 * s), new Vector2(28 * s, 28 * s))
                .AddComponent<Image>().color = skin;
            FacePart("Hair", parent, new Vector2(0, 26 * s), new Vector2(32 * s, 14 * s))
                .AddComponent<Image>().color = hair;
            FacePart("EyeL", parent, new Vector2(-6 * s, 15 * s), new Vector2(4 * s, 3 * s))
                .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);
            FacePart("EyeR", parent, new Vector2(6 * s, 15 * s), new Vector2(4 * s, 3 * s))
                .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);
        }
    }

    void CheckMotherNPC()
    {
        if (motherNpcObj == null) return;
        if (playerTileX == motherNpcX && playerTileY == motherNpcY)
            OnMotherTapped();
    }

    void OnMotherTapped()
    {
        if (motherDialogueActive) return;
        if (menuOpen) return;

        StartCoroutine(ShowMotherDialogue());
    }

    string GetMotherEquipmentItem(string motherJaName)
    {
        string key = "equip_" + motherJaName;
        string val = Localization.Get(key);
        return (val != key) ? val : "";
    }

    IEnumerator ShowMotherDialogue()
    {
        motherDialogueActive = true;
        menuOpen = true;

        // 縁の書に登録
        string motherJaName = DataCarrier.Instance != null ? DataCarrier.Instance.motherName : "";
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc(motherJaName);

        bool alreadyGave = DataCarrier.Instance != null && DataCarrier.Instance.motherGaveItem;

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        // フェードイン
        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // ポートレート
        string imgKey = GetParentImageNameForMap(motherJaName);
        Sprite portrait = Resources.Load<Sprite>("Parents/" + imgKey);
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        if (portrait != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(portrait);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel(motherJaName, "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        // メッセージ
        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "";

        var messageList = new System.Collections.Generic.List<string> {
            Localization.Get("map_mother_msg1", babyName),
            Localization.Get("map_mother_msg2"),
        };

        // アイテム未取得なら付与メッセージを追加
        string itemName = GetMotherEquipmentItem(motherJaName);
        bool giveItem = !alreadyGave && !string.IsNullOrEmpty(itemName);
        if (giveItem)
        {
            messageList.Add(Localization.Get("map_mother_give_item"));
            messageList.Add(Localization.Get("map_mother_got_item", itemName));
            messageList.Add(Localization.Get("map_mother_equip_hint"));
            messageList.Add(Localization.Get("map_mother_goodbye"));
        }
        else
        {
            messageList.Add(Localization.Get("map_mother_msg3"));
        }

        string[] messages = messageList.ToArray();

        // アイテム取得メッセージのインデックスを特定（"〜を手に入れた！"）
        int gotItemMsgIdx = -1;
        if (giveItem)
        {
            for (int j = 0; j < messages.Length; j++)
            {
                if (messages[j].Contains("手に入れた") || messages[j].Contains("Got "))
                { gotItemMsgIdx = j; break; }
            }
        }

        UIE.VisualElement itemCard = null;

        // メッセージループ
        for (int i = 0; i < messages.Length; i++)
        {
            textLabel.text = messages[i];
            tapHint.text = (i < messages.Length - 1) ? "▼ タップで続く" : "▼ タップで閉じる";

            // アイテム取得時にアイテムカードを表示
            if (i == gotItemMsgIdx && giveItem)
            {
                itemCard = new UIE.VisualElement();
                itemCard.style.position = UIE.Position.Absolute;
                itemCard.style.left = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
                itemCard.style.top = new UIE.StyleLength(new UIE.Length(25, UIE.LengthUnit.Percent));
                itemCard.style.width = 400;
                itemCard.style.translate = new UIE.StyleTranslate(new UIE.Translate(-200, 0));
                itemCard.style.backgroundColor = new Color(1f, 0.98f, 0.92f, 0.95f);
                itemCard.style.borderTopLeftRadius = 32;
                itemCard.style.borderTopRightRadius = 32;
                itemCard.style.borderBottomLeftRadius = 32;
                itemCard.style.borderBottomRightRadius = 32;
                itemCard.style.paddingTop = 24;
                itemCard.style.paddingBottom = 24;
                itemCard.style.paddingLeft = 20;
                itemCard.style.paddingRight = 20;
                itemCard.style.alignItems = UIE.Align.Center;
                itemCard.style.borderTopWidth = 3;
                itemCard.style.borderBottomWidth = 3;
                itemCard.style.borderLeftWidth = 3;
                itemCard.style.borderRightWidth = 3;
                itemCard.style.borderTopColor = new Color(1f, 0.718f, 0.773f, 0.6f);
                itemCard.style.borderBottomColor = new Color(1f, 0.718f, 0.773f, 0.6f);
                itemCard.style.borderLeftColor = new Color(1f, 0.718f, 0.773f, 0.6f);
                itemCard.style.borderRightColor = new Color(1f, 0.718f, 0.773f, 0.6f);

                var itemNameLabel = UIHelper.CreateLabel(itemName);
                UIHelper.ApplyFontBold(itemNameLabel);
                itemNameLabel.style.fontSize = 36;
                itemNameLabel.style.color = new Color(0.47f, 0.22f, 0.33f);
                itemNameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                itemNameLabel.style.marginBottom = 8;
                itemCard.Add(itemNameLabel);

                string effectKey = "equip_effect_" + motherJaName;
                string effect = Localization.Get(effectKey);
                if (!string.IsNullOrEmpty(effect) && effect != effectKey)
                {
                    var effectLabel = UIHelper.CreateLabel(effect);
                    UIHelper.ApplyFont(effectLabel);
                    effectLabel.style.fontSize = 26;
                    effectLabel.style.color = new Color(0.4f, 0.6f, 0.3f);
                    effectLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    itemCard.Add(effectLabel);
                }

                overlay.Add(itemCard);
            }

            yield return new WaitForSeconds(0.3f);

            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            // アイテムカードは次のメッセージまで表示し続ける（ヒントメッセージの間も見える）
        }

        // アイテムカード除去
        if (itemCard != null) itemCard.RemoveFromHierarchy();

        // アイテム付与処理
        if (giveItem && DataCarrier.Instance != null)
        {
            DataCarrier.Instance.AddEquipment(itemName);
            DataCarrier.Instance.motherGaveItem = true;
            DataCarrier.Instance.SaveData();
        }

        // フェードアウト
        float fadeOut = 0.3f;
        fadeElapsed = 0f;
        float startOpacity = 0.5f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = fadeElapsed / fadeOut;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(startOpacity, 0, fadeT));
            dialogBox.style.opacity = 1f - fadeT;
            portraitEl.style.opacity = 1f - fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        motherDialogueActive = false;
        menuOpen = false;

        // アイテムをくれたら母親は実家に帰る
        if (giveItem && motherNpcObj != null)
        {
            Destroy(motherNpcObj);
            motherNpcObj = null;
        }
    }

    // ===== 父親の家（家の中にいる） =====

    void CreateFatherInteriorNPC()
    {
        if (tilesContainer == null) return;

        fatherNpcObj = new GameObject("FatherNPC");
        fatherNpcObj.transform.SetParent(tilesContainer.transform, false);

        var rect = fatherNpcObj.AddComponent<RectTransform>();
        float posX = (fatherNpcX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (fatherNpcY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var btnImg = fatherNpcObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);
        var btn = fatherNpcObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnFatherTapped());

        if (!parentsInBed)
            DrawFatherNPC(fatherNpcObj.transform, 0.8f);
        fatherNpcObj.transform.SetAsLastSibling();
    }

    void DrawFatherNPC(Transform parent, float scale)
    {
        string fatherName = DataCarrier.Instance != null ? DataCarrier.Instance.fatherName : "";
        string imgKey = GetParentImageNameForMap(fatherName);
        Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);

        if (sprite != null)
        {
            var obj = FacePart("FatherSprite", parent, Vector2.zero, new Vector2(DISPLAY_TILE * 0.9f, DISPLAY_TILE * 0.9f));
            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
        }
        else
        {
            // フォールバック: 簡易キャラ描画
            float s = scale;
            Color skin = new Color(0.9f, 0.78f, 0.65f);
            Color shirt = new Color(0.3f, 0.35f, 0.5f);
            Color hair = new Color(0.2f, 0.15f, 0.1f);

            FacePart("Shadow", parent, new Vector2(0, -34 * s), new Vector2(36 * s, 10 * s))
                .AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);
            FacePart("Body", parent, new Vector2(0, -14 * s), new Vector2(28 * s, 32 * s))
                .AddComponent<Image>().color = shirt;
            FacePart("Head", parent, new Vector2(0, 14 * s), new Vector2(30 * s, 30 * s))
                .AddComponent<Image>().color = skin;
            FacePart("Hair", parent, new Vector2(0, 28 * s), new Vector2(34 * s, 12 * s))
                .AddComponent<Image>().color = hair;
            FacePart("EyeL", parent, new Vector2(-7 * s, 15 * s), new Vector2(4 * s, 3 * s))
                .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);
            FacePart("EyeR", parent, new Vector2(7 * s, 15 * s), new Vector2(4 * s, 3 * s))
                .AddComponent<Image>().color = new Color(0.2f, 0.15f, 0.1f);
        }
    }

    void CheckFatherNPC()
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area != 0 && area != 5) return;
        if (playerTileX == fatherNpcX && playerTileY == fatherNpcY)
            OnFatherTapped();
    }

    void OnFatherTapped()
    {
        if (fatherDialogueActive) return;
        if (menuOpen) return;

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area == 5 && parentsInBed)
            StartCoroutine(ShowParentsInBedDialogue());
        else
            StartCoroutine(ShowFatherDialogue());
    }

    IEnumerator ShowFatherDialogue()
    {
        fatherDialogueActive = true;
        menuOpen = true;

        // 縁の書に登録
        string fatherJaName = DataCarrier.Instance != null ? DataCarrier.Instance.fatherName : "";
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc(fatherJaName);

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        // フェードイン
        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // ポートレート
        string imgKey = GetParentImageNameForMap(fatherJaName);
        Sprite portrait = Resources.Load<Sprite>("Parents/" + imgKey);
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        if (portrait != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(portrait);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel(fatherJaName, "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        // メッセージ
        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "";

        string[] messages = new[] {
            Localization.Get("map_father_msg1", babyName),
            Localization.Get("map_father_msg2"),
            Localization.Get("map_father_msg3"),
        };

        // メッセージループ
        for (int i = 0; i < messages.Length; i++)
        {
            textLabel.text = messages[i];
            tapHint.text = (i < messages.Length - 1) ? "▼ タップで続く" : "▼ タップで閉じる";

            yield return new WaitForSeconds(0.3f);

            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }

        // フェードアウト
        float fadeOut = 0.3f;
        fadeElapsed = 0f;
        float startOpacity = 0.5f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = fadeElapsed / fadeOut;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(startOpacity, 0, fadeT));
            dialogBox.style.opacity = 1f - fadeT;
            portraitEl.style.opacity = 1f - fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        fatherDialogueActive = false;
        menuOpen = false;
    }

    // ===== 実家の母親NPC =====

    void CreateHomeMotherNPC()
    {
        if (tilesContainer == null) return;

        homeMotherNpcObj = new GameObject("HomeMotherNPC");
        homeMotherNpcObj.transform.SetParent(tilesContainer.transform, false);

        var rect = homeMotherNpcObj.AddComponent<RectTransform>();
        float posX = (homeMotherNpcX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (homeMotherNpcY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var btnImg = homeMotherNpcObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);
        var btn = homeMotherNpcObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnHomeMotherTapped());

        if (!parentsInBed)
            DrawMotherNPC(homeMotherNpcObj.transform, 0.8f);
        homeMotherNpcObj.transform.SetAsLastSibling();
    }

    void CheckHomeMotherNPC()
    {
        if (homeMotherNpcObj == null) return;
        if (playerTileX == homeMotherNpcX && playerTileY == homeMotherNpcY)
            OnHomeMotherTapped();
    }

    void OnHomeMotherTapped()
    {
        if (motherDialogueActive) return;
        if (menuOpen) return;

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area == 5 && parentsInBed)
            StartCoroutine(ShowParentsInBedDialogue());
        else
            StartCoroutine(ShowHomeMotherDialogue());
    }

    IEnumerator ShowHomeMotherDialogue()
    {
        motherDialogueActive = true;
        menuOpen = true;

        string motherJaName = DataCarrier.Instance != null ? DataCarrier.Instance.motherName : "";

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        string imgKey = GetParentImageNameForMap(motherJaName);
        Sprite portrait = Resources.Load<Sprite>("Parents/" + imgKey);
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        if (portrait != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(portrait);
        overlay.Add(portraitEl);

        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel(motherJaName, "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで閉じる", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        string babyName = DataCarrier.Instance != null ? DataCarrier.Instance.babyName : "";
        string[] msgs = new[] {
            Localization.Get("home_mother_msg1", babyName),
            Localization.Get("home_mother_msg2"),
        };

        for (int i = 0; i < msgs.Length; i++)
        {
            textLabel.text = msgs[i];
            tapHint.text = (i < msgs.Length - 1) ? "▼ タップで続く" : "▼ タップで閉じる";
            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }

        float fadeOut = 0.3f;
        fadeElapsed = 0f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = fadeElapsed / fadeOut;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.5f, 0, fadeT));
            dialogBox.style.opacity = 1f - fadeT;
            portraitEl.style.opacity = 1f - fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        motherDialogueActive = false;
        menuOpen = false;
    }

    // ===== ベッドにいる時の親対話 =====

    IEnumerator ShowParentsInBedDialogue()
    {
        fatherDialogueActive = true;
        menuOpen = true;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // ポートレートなし（ベッドで寝てる）
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("？？？", "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで閉じる", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        // ランダムでベッド時のセリフを選択
        string[] bedLines = new[] {
            "bed_line1", "bed_line2", "bed_line3", "bed_line4", "bed_line5"
        };
        string key = bedLines[Random.Range(0, bedLines.Length)];
        string[] msgs = new[] { Localization.Get(key) };

        for (int i = 0; i < msgs.Length; i++)
        {
            textLabel.text = msgs[i];
            yield return new WaitForSeconds(0.3f);
            bool tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;
        }

        float fadeOut = 0.3f;
        fadeElapsed = 0f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = fadeElapsed / fadeOut;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.5f, 0, fadeT));
            dialogBox.style.opacity = 1f - fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        fatherDialogueActive = false;
        menuOpen = false;
    }

    // ===== お手伝いさんNPC =====

    static readonly Color[] MaidDressColors = {
        new Color(0.55f, 0.35f, 0.65f),  // 紫
        new Color(0.35f, 0.55f, 0.75f),  // 青
        new Color(0.75f, 0.40f, 0.45f),  // 赤
        new Color(0.40f, 0.65f, 0.50f),  // 緑
        new Color(0.80f, 0.60f, 0.35f),  // オレンジ
        new Color(0.65f, 0.45f, 0.55f),  // ピンク
    };

    static readonly Color[] MaidHairColors = {
        new Color(0.20f, 0.12f, 0.08f),
        new Color(0.10f, 0.08f, 0.06f),
        new Color(0.35f, 0.22f, 0.12f),
        new Color(0.25f, 0.18f, 0.10f),
        new Color(0.15f, 0.10f, 0.08f),
        new Color(0.30f, 0.20f, 0.15f),
    };

    void CreateMaidNPCs()
    {
        if (tilesContainer == null) return;
        for (int i = 0; i < 6; i++)
        {
            var obj = new GameObject("MaidNPC" + i);
            obj.transform.SetParent(tilesContainer.transform, false);

            var rect = obj.AddComponent<RectTransform>();
            float posX = (maidNpcX[i] - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
            float posY = (maidNpcY[i] - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
            rect.anchoredPosition = new Vector2(posX, posY);
            rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

            var btnImg = obj.AddComponent<Image>();
            btnImg.color = new Color(0, 0, 0, 0);
            var btn = obj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            int idx = i;
            btn.onClick.AddListener(() => OnMaidTapped(idx));

            DrawMaidNPC(obj.transform, 0.75f, i);
            obj.transform.SetAsLastSibling();
            maidNpcObjs[i] = obj;
        }
    }

    void DrawMaidNPC(Transform parent, float scale, int index)
    {
        float s = scale;
        Color skin = new Color(0.95f, 0.85f, 0.75f);
        Color dress = MaidDressColors[index % MaidDressColors.Length];
        Color hair = MaidHairColors[index % MaidHairColors.Length];
        Color apron = new Color(1f, 1f, 1f);

        // 影
        FacePart("Shadow", parent, new Vector2(0, -34 * s), new Vector2(32 * s, 8 * s))
            .AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
        // ドレス
        FacePart("Body", parent, new Vector2(0, -14 * s), new Vector2(24 * s, 28 * s))
            .AddComponent<Image>().color = dress;
        // エプロン
        FacePart("Apron", parent, new Vector2(0, -10 * s), new Vector2(16 * s, 20 * s))
            .AddComponent<Image>().color = apron;
        // 頭
        FacePart("Head", parent, new Vector2(0, 14 * s), new Vector2(26 * s, 26 * s))
            .AddComponent<Image>().color = skin;
        // 髪
        FacePart("Hair", parent, new Vector2(0, 25 * s), new Vector2(30 * s, 12 * s))
            .AddComponent<Image>().color = hair;
        // ヘッドバンド（メイドらしさ）
        FacePart("Band", parent, new Vector2(0, 30 * s), new Vector2(20 * s, 5 * s))
            .AddComponent<Image>().color = apron;
        // 目
        FacePart("EyeL", parent, new Vector2(-5 * s, 14 * s), new Vector2(3 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.10f);
        FacePart("EyeR", parent, new Vector2(5 * s, 14 * s), new Vector2(3 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.10f);
    }

    void CheckMaidNPCs()
    {
        if (DataCarrier.Instance == null || DataCarrier.Instance.currentArea != 5) return;
        for (int i = 0; i < maidNpcObjs.Length; i++)
        {
            if (maidNpcObjs[i] == null) continue;
            if (playerTileX == maidNpcX[i] && playerTileY == maidNpcY[i])
            {
                OnMaidTapped(i);
                return;
            }
        }
    }

    void OnMaidTapped(int index)
    {
        if (maidDialogueActive) return;
        if (menuOpen) return;
        StartCoroutine(ShowMaidDialogue(index));
    }

    IEnumerator ShowMaidDialogue(int index)
    {
        maidDialogueActive = true;
        menuOpen = true;

        string maidName = Localization.Get("maid_name_" + index);

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel(maidName, "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで閉じる", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        string[] keys = new[] {
            "maid_line_" + index + "_0",
            "maid_line_" + index + "_1",
            "maid_line_" + index + "_2",
        };
        string msg = Localization.Get(keys[Random.Range(0, keys.Length)]);
        textLabel.text = msg;

        yield return new WaitForSeconds(0.3f);
        bool tapped = false;
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        while (!tapped) yield return null;

        float fadeOut = 0.3f;
        fadeElapsed = 0f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = fadeElapsed / fadeOut;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.5f, 0, fadeT));
            dialogBox.style.opacity = 1f - fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        maidDialogueActive = false;
        menuOpen = false;
    }

    // ===== 武器屋の商人NPC =====

    void CreateMerchantNPC()
    {
        if (tilesContainer == null) return;

        merchantNpcObj = new GameObject("MerchantNPC");
        merchantNpcObj.transform.SetParent(tilesContainer.transform, false);

        var rect = merchantNpcObj.AddComponent<RectTransform>();
        float posX = (merchantNpcX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (merchantNpcY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var btnImg = merchantNpcObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);
        var btn = merchantNpcObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnMerchantTapped());

        DrawMerchantNPC(merchantNpcObj.transform, 0.8f);
        merchantNpcObj.transform.SetAsLastSibling();
    }

    void DrawMerchantNPC(Transform parent, float scale)
    {
        Sprite spr = Resources.Load<Sprite>("MapCharacters/Weapon_men");
        if (spr != null)
        {
            var go = new GameObject("MerchantSprite");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(DISPLAY_TILE * scale, DISPLAY_TILE * scale);
            var img = go.AddComponent<Image>();
            img.sprite = spr;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
    }

    void CheckMerchantNPC()
    {
        if (merchantNpcObj == null) return;
        if (playerTileX == merchantNpcX && playerTileY == merchantNpcY)
            OnMerchantTapped();
    }

    void OnMerchantTapped()
    {
        if (merchantDialogueActive) return;
        if (menuOpen) return;

        StartCoroutine(ShowMerchantDialogue());
    }

    IEnumerator ShowMerchantDialogue()
    {
        merchantDialogueActive = true;
        menuOpen = true;

        // 縁の書に登録
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc("おあそびどうぐやさん");

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        // フェードイン
        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // ポートレート
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        var merchantSpr = Resources.Load<Sprite>("MapCharacters/Weapon_men");
        if (merchantSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(merchantSpr);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("おあそびどうぐやさん", "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        // 挨拶メッセージ
        textLabel.text = Localization.Get("shop_merchant_greet");
        tapHint.text = "▼ タップで続く";

        yield return new WaitForSeconds(0.3f);

        bool tapped = false;
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        while (!tapped) yield return null;

        // ダイアログをフェードアウト
        float fadeOut = 0.2f;
        fadeElapsed = 0f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = fadeElapsed / fadeOut;
            dialogBox.style.opacity = 1f - fadeT;
            yield return null;
        }
        overlay.RemoveFromHierarchy();
        merchantDialogueActive = false;
        menuOpen = false;

        // ショップUIを開く
        OpenWeaponShop();
    }

    void ShowMerchantMessage(string message)
    {
        StartCoroutine(ShowMerchantMessageCoroutine(message));
    }

    IEnumerator ShowMerchantMessageCoroutine(string message)
    {
        merchantDialogueActive = true;
        menuOpen = true;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        // ポートレート
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        var merchantSpr = Resources.Load<Sprite>("MapCharacters/Weapon_men");
        if (merchantSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(merchantSpr);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("おあそびどうぐやさん", "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel(message, "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        yield return new WaitForSeconds(0.3f);

        bool tapped = false;
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        while (!tapped) yield return null;

        // フェードアウト
        float fadeOut = 0.2f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            dialogBox.style.opacity = 1f - fadeElapsed / fadeOut;
            yield return null;
        }
        overlay.RemoveFromHierarchy();
        merchantDialogueActive = false;
        menuOpen = false;

        // ショップを再度開く
        OpenWeaponShop();
    }

    // ============================================================
    // 塾の先生NPC & クイズシステム
    // ============================================================

    static readonly (string question, string[] choices, int answer)[] JukuQuizData = {
        ("にほんで いちばん たかい やまは？", new[]{"ふじさん","たかおさん","あそさん","つくばさん"}, 0),
        ("たいようけいで いちばん おおきい わくせいは？", new[]{"ちきゅう","かせい","もくせい","どせい"}, 2),
        ("みずの かがくしきは？", new[]{"CO2","H2O","O2","NaCl"}, 1),
        ("にじは なんしょく？", new[]{"5しょく","6しょく","7しょく","8しょく"}, 2),
        ("いちねんで いちばん ひが ながいのは？", new[]{"はるぶん","げし","しゅうぶん","とうじ"}, 1),
        ("ちきゅうの えいせいは？", new[]{"たいよう","つき","かせい","きんせい"}, 1),
        ("1ダースは なんこ？", new[]{"6こ","10こ","12こ","24こ"}, 2),
        ("おんがくの 「ド」は えいごで？", new[]{"A","B","C","D"}, 2),
        ("にほんの しゅとは？", new[]{"おおさか","とうきょう","きょうと","なごや"}, 1),
        ("パンダの しょくじは おもに？", new[]{"にく","さかな","ささ","くだもの"}, 2),
        ("いっしゅうかんは なんにち？", new[]{"5にち","6にち","7にち","8にち"}, 2),
        ("ひかりの さんげんしょくで ないのは？", new[]{"あか","きいろ","あお","みどり"}, 1),
        ("にんげんの ほねの かずは やく？", new[]{"100ほん","150ほん","200ほん","300ほん"}, 2),
        ("さんかくけいの かくどの ごうけいは？", new[]{"90ど","180ど","270ど","360ど"}, 1),
        ("かんじの 「山」の かくすうは？", new[]{"2かく","3かく","4かく","5かく"}, 1),
        ("ちきゅうから いちばん ちかい ほしは？", new[]{"かせい","きんせい","もくせい","どせい"}, 1),
        ("1キロメートルは なんメートル？", new[]{"100m","500m","1000m","10000m"}, 2),
        ("にほんの こっかは？", new[]{"さくら","きく","うめ","ばら"}, 0),
        ("おおきい じゅんに ならべると？", new[]{"kg, g, mg","mg, g, kg","g, kg, mg","kg, mg, g"}, 0),
        ("でんきを とおさない ものは？", new[]{"てつ","どう","ゴム","アルミ"}, 2),
    };

    void CreateJukuTeacherNPC()
    {
        if (tilesContainer == null) return;

        jukuTeacherObj = new GameObject("JukuTeacherNPC");
        jukuTeacherObj.transform.SetParent(tilesContainer.transform, false);

        var rect = jukuTeacherObj.AddComponent<RectTransform>();
        float posX = (jukuTeacherX - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (jukuTeacherY - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var btnImg = jukuTeacherObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);
        var btn = jukuTeacherObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnJukuTeacherTapped());

        DrawJukuTeacherNPC(jukuTeacherObj.transform, 1.6f);
        jukuTeacherObj.transform.SetAsLastSibling();
    }

    void DrawJukuTeacherNPC(Transform parent, float scale)
    {
        Sprite spr = Resources.Load<Sprite>("MapCharacters/Teacher_Lady");
        if (spr != null)
        {
            var go = new GameObject("TeacherSprite");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(DISPLAY_TILE * scale, DISPLAY_TILE * scale);
            var img = go.AddComponent<Image>();
            img.sprite = spr;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }
    }

    void CheckJukuTeacher()
    {
        if (jukuTeacherObj == null) return;
        if (playerTileX == jukuTeacherX && playerTileY == jukuTeacherY)
            OnJukuTeacherTapped();
    }

    // ===== 塾の生徒NPC =====

    static readonly Color[] StudentColors = {
        new Color(1f, 0.45f, 0.45f),   // 赤
        new Color(0.45f, 0.65f, 1f),   // 青
        new Color(0.45f, 0.85f, 0.50f), // 緑
        new Color(1f, 0.75f, 0.30f),   // オレンジ
        new Color(0.80f, 0.50f, 0.90f), // 紫
    };

    static readonly Color[] StudentHairColors = {
        new Color(0.2f, 0.12f, 0.08f),
        new Color(0.35f, 0.25f, 0.15f),
        new Color(0.15f, 0.10f, 0.10f),
        new Color(0.40f, 0.30f, 0.10f),
        new Color(0.10f, 0.08f, 0.15f),
    };

    void CreateJukuStudentNPCs()
    {
        if (tilesContainer == null) return;
        for (int i = 0; i < 5; i++)
        {
            var obj = new GameObject("JukuStudent" + i);
            obj.transform.SetParent(tilesContainer.transform, false);

            var rect = obj.AddComponent<RectTransform>();
            float posX = (jukuStudentX[i] - mapWidth / 2f + 0.5f) * DISPLAY_TILE;
            float posY = (jukuStudentY[i] - mapHeight / 2f + 0.5f) * DISPLAY_TILE;
            rect.anchoredPosition = new Vector2(posX, posY);
            rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

            var btnImg = obj.AddComponent<Image>();
            btnImg.color = new Color(0, 0, 0, 0);
            var btn = obj.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            int idx = i;
            btn.onClick.AddListener(() => OnJukuStudentTapped(idx));

            DrawJukuStudentNPC(obj.transform, 0.75f, i);
            obj.transform.SetAsLastSibling();
            jukuStudentObjs[i] = obj;
        }
    }

    void DrawJukuStudentNPC(Transform parent, float scale, int index)
    {
        float s = scale;
        Color skin = new Color(0.95f, 0.85f, 0.75f);
        Color clothes = StudentColors[index % StudentColors.Length];
        Color hair = StudentHairColors[index % StudentHairColors.Length];

        FacePart("Shadow", parent, new Vector2(0, -34 * s), new Vector2(32 * s, 8 * s))
            .AddComponent<Image>().color = new Color(0, 0, 0, 0.2f);
        FacePart("Body", parent, new Vector2(0, -14 * s), new Vector2(24 * s, 28 * s))
            .AddComponent<Image>().color = clothes;
        FacePart("Head", parent, new Vector2(0, 14 * s), new Vector2(26 * s, 26 * s))
            .AddComponent<Image>().color = skin;
        FacePart("Hair", parent, new Vector2(0, 25 * s), new Vector2(30 * s, 12 * s))
            .AddComponent<Image>().color = hair;
        FacePart("EyeL", parent, new Vector2(-5 * s, 14 * s), new Vector2(3 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.10f);
        FacePart("EyeR", parent, new Vector2(5 * s, 14 * s), new Vector2(3 * s, 3 * s))
            .AddComponent<Image>().color = new Color(0.15f, 0.12f, 0.10f);
    }

    void CheckJukuStudents()
    {
        for (int i = 0; i < jukuStudentObjs.Length; i++)
        {
            if (jukuStudentObjs[i] == null) continue;
            if (playerTileX == jukuStudentX[i] && playerTileY == jukuStudentY[i])
            {
                OnJukuStudentTapped(i);
                return;
            }
        }
    }

    void OnJukuStudentTapped(int index)
    {
        if (jukuStudentDialogueActive) return;
        if (menuOpen) return;
        StartCoroutine(ShowJukuStudentDialogue(index));
    }

    IEnumerator ShowJukuStudentDialogue(int index)
    {
        jukuStudentDialogueActive = true;
        menuOpen = true;

        string studentName = Localization.Get("juku_student_name_" + index);

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.FlexEnd;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel(studentName, "milk-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで閉じる", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        // 各生徒ごとに複数セリフからランダム
        string[] keys = new[] {
            "juku_student_line_" + index + "_0",
            "juku_student_line_" + index + "_1",
            "juku_student_line_" + index + "_2",
        };
        string msg = Localization.Get(keys[Random.Range(0, keys.Length)]);
        textLabel.text = msg;

        yield return new WaitForSeconds(0.3f);
        bool tapped = false;
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        while (!tapped) yield return null;

        float fadeOut = 0.3f;
        fadeElapsed = 0f;
        while (fadeElapsed < fadeOut)
        {
            fadeElapsed += Time.deltaTime;
            float fadeT = fadeElapsed / fadeOut;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.5f, 0, fadeT));
            dialogBox.style.opacity = 1f - fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        jukuStudentDialogueActive = false;
        menuOpen = false;
    }

    void OnJukuTeacherTapped()
    {
        if (jukuQuizActive) return;
        if (menuOpen) return;

        StartCoroutine(JukuQuizSequence());
    }

    IEnumerator JukuQuizSequence()
    {
        jukuQuizActive = true;
        menuOpen = true;

        // 縁の書に登録
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc("じゅくの先生");

        // オーバーレイ
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlayRoot.Add(overlay);

        // フェードイン
        float fadeIn = 0.3f;
        float fadeElapsed = 0f;
        while (fadeElapsed < fadeIn)
        {
            fadeElapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.5f, fadeElapsed / fadeIn));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // ポートレート
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        var teacherSpr = Resources.Load<Sprite>("MapCharacters/Teacher_Lady");
        if (teacherSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(teacherSpr);
        overlay.Add(portraitEl);

        // 導入ダイアログ
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("じゅくの先生", "milk-dialog-name");
        dialogBox.Add(nameLabel);

        int currentWisdom = DataCarrier.Instance != null ? DataCarrier.Instance.babyIntelligence : 0;
        var textLabel = UIHelper.CreateLabel(string.Format(Localization.Get("juku_intro"), currentWisdom), "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで はじめる", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        yield return new WaitForSeconds(0.3f);

        bool tapped = false;
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        while (!tapped) yield return null;

        // ダイアログを消す
        dialogBox.RemoveFromHierarchy();

        // クイズ出題: ランダムに3問選ぶ
        var indices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < JukuQuizData.Length; i++) indices.Add(i);
        // シャッフル
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (indices[i], indices[j]) = (indices[j], indices[i]);
        }

        int correctCount = 0;
        bool failed = false;

        for (int q = 0; q < 3 && !failed; q++)
        {
            var quiz = JukuQuizData[indices[q]];

            // クイズパネル
            var panel = new UIE.VisualElement();
            panel.AddToClassList("juku-panel");
            overlay.Add(panel);

            var numLabel = UIHelper.CreateLabel($"もんだい {q + 1}/3", "juku-question-num");
            panel.Add(numLabel);

            var qLabel = UIHelper.CreateLabel(quiz.question, "juku-question-text");
            panel.Add(qLabel);

            int selectedChoice = -1;
            var choiceBtns = new UIE.VisualElement[4];
            for (int c = 0; c < 4; c++)
            {
                int choiceIdx = c;
                var choiceBtn = new UIE.VisualElement();
                choiceBtn.AddToClassList("juku-choice-btn");

                var choiceLabel = UIHelper.CreateLabel(quiz.choices[c], "juku-choice-label");
                choiceBtn.Add(choiceLabel);

                choiceBtn.RegisterCallback<UIE.ClickEvent>(evt =>
                {
                    if (selectedChoice < 0) selectedChoice = choiceIdx;
                });
                panel.Add(choiceBtn);
                choiceBtns[c] = choiceBtn;
            }

            // 結果テキスト
            var resultLabel = UIHelper.CreateLabel("", "juku-result-text");
            panel.Add(resultLabel);

            yield return new WaitForSeconds(0.2f);

            // 回答待ち
            while (selectedChoice < 0) yield return null;

            if (selectedChoice == quiz.answer)
            {
                // 正解
                if (seSource != null && seQuizCorrect != null)
                    seSource.PlayOneShot(seQuizCorrect, 0.8f);
                choiceBtns[selectedChoice].style.backgroundColor = new Color(0.3f, 0.75f, 0.35f);
                resultLabel.text = "せいかい！";
                resultLabel.style.color = new Color(0.2f, 0.65f, 0.25f);
                correctCount++;
            }
            else
            {
                // 不正解
                if (seSource != null && seQuizWrong != null)
                    seSource.PlayOneShot(seQuizWrong, 0.8f);
                choiceBtns[selectedChoice].style.backgroundColor = new Color(0.85f, 0.3f, 0.3f);
                choiceBtns[quiz.answer].style.backgroundColor = new Color(0.3f, 0.75f, 0.35f);
                resultLabel.text = "ざんねん…";
                resultLabel.style.color = new Color(0.8f, 0.25f, 0.25f);
                failed = true;
            }

            yield return new WaitForSeconds(1.0f);

            panel.RemoveFromHierarchy();
        }

        // 結果ダイアログ
        var resultDialog = new UIE.VisualElement();
        resultDialog.AddToClassList("milk-dialog-box");
        overlay.Add(resultDialog);

        var resultName = UIHelper.CreateLabel("じゅくの先生", "milk-dialog-name");
        resultDialog.Add(resultName);

        if (correctCount == 3)
        {
            var resultText = UIHelper.CreateLabel(Localization.Get("juku_reward_text"), "milk-dialog-text");
            resultDialog.Add(resultText);

            // 旧ちえ値を記憶してから+1
            int oldWisdom = DataCarrier.Instance != null ? DataCarrier.Instance.babyIntelligence : 0;
            if (DataCarrier.Instance != null)
            {
                DataCarrier.Instance.babyIntelligence += 1;
                DataCarrier.Instance.SaveData();
            }
            int newWisdom = DataCarrier.Instance != null ? DataCarrier.Instance.babyIntelligence : oldWisdom + 1;

            var closeTap = UIHelper.CreateLabel("▼ タップ", "milk-dialog-hint");
            resultDialog.Add(closeTap);

            yield return new WaitForSeconds(0.3f);
            tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            // ちえアップ演出パネル
            resultDialog.RemoveFromHierarchy();
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

            var growthPanel = new UIE.VisualElement();
            growthPanel.style.width = 900;
            growthPanel.style.backgroundColor = new Color(0.969f, 0.906f, 0.808f, 1f); // #F7E7CE
            growthPanel.style.borderTopLeftRadius = 48;
            growthPanel.style.borderTopRightRadius = 48;
            growthPanel.style.borderBottomLeftRadius = 48;
            growthPanel.style.borderBottomRightRadius = 48;
            growthPanel.style.paddingTop = 40;
            growthPanel.style.paddingBottom = 40;
            growthPanel.style.paddingLeft = 32;
            growthPanel.style.paddingRight = 32;
            growthPanel.style.alignItems = UIE.Align.Center;
            growthPanel.style.alignSelf = UIE.Align.Center;
            overlay.Add(growthPanel);

            // タイトル「ちえ アップ！」
            var growthTitle = UIHelper.CreateLabel(Localization.Get("juku_wisdom_up_title"), "");
            growthTitle.style.fontSize = 52;
            growthTitle.style.color = Color.black;
            growthTitle.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            UIHelper.ApplyFontBold(growthTitle);
            growthPanel.Add(growthTitle);

            // ちえ行 (フェードイン + スケールアニメーション)
            var row = new UIE.VisualElement();
            row.style.flexDirection = UIE.FlexDirection.Row;
            row.style.alignItems = UIE.Align.Center;
            row.style.justifyContent = UIE.Justify.Center;
            row.style.width = UIE.Length.Percent(100);
            row.style.height = 120;
            row.style.marginTop = 24;
            row.style.backgroundColor = new Color(1f, 1f, 1f, 0.6f);
            row.style.borderTopLeftRadius = 30;
            row.style.borderTopRightRadius = 30;
            row.style.borderBottomLeftRadius = 30;
            row.style.borderBottomRightRadius = 30;
            row.style.opacity = 0f;
            row.style.scale = new UIE.Scale(new Vector2(0.85f, 0.85f));
            row.style.transitionProperty = new System.Collections.Generic.List<UIE.StylePropertyName> {
                new UIE.StylePropertyName("opacity"),
                new UIE.StylePropertyName("scale")
            };
            row.style.transitionDuration = new System.Collections.Generic.List<UIE.TimeValue> {
                new UIE.TimeValue(0.4f),
                new UIE.TimeValue(0.4f)
            };
            row.style.transitionTimingFunction = new System.Collections.Generic.List<UIE.EasingFunction> {
                new UIE.EasingFunction(UIE.EasingMode.EaseOut),
                new UIE.EasingFunction(UIE.EasingMode.EaseOut)
            };
            growthPanel.Add(row);

            // カラーバー
            var bar = new UIE.VisualElement();
            bar.style.width = 6;
            bar.style.height = UIE.Length.Percent(70);
            bar.style.borderTopLeftRadius = 3;
            bar.style.borderTopRightRadius = 3;
            bar.style.borderBottomLeftRadius = 3;
            bar.style.borderBottomRightRadius = 3;
            bar.style.backgroundColor = new Color(0.667f, 0.941f, 0.82f); // accent #AAF0D1
            bar.style.marginLeft = 28;
            row.Add(bar);

            // ラベル「ちえ」
            var statLabel = UIHelper.CreateLabel("ちえ", "");
            statLabel.style.fontSize = 36;
            statLabel.style.color = new Color(0.667f, 0.941f, 0.82f);
            statLabel.style.width = 120;
            statLabel.style.marginLeft = 16;
            statLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleLeft;
            UIHelper.ApplyFontBold(statLabel);
            row.Add(statLabel);

            // 旧値
            var oldLabel = UIHelper.CreateLabel(oldWisdom.ToString(), "");
            oldLabel.style.fontSize = 38;
            oldLabel.style.color = new Color(0, 0, 0, 0.35f);
            oldLabel.style.width = 100;
            oldLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleRight;
            row.Add(oldLabel);

            // 矢印
            var arrow = UIHelper.CreateLabel("\u2192", "");
            arrow.style.fontSize = 36;
            arrow.style.color = new Color(1f, 0.718f, 0.773f); // sub #FFB7C5
            arrow.style.width = 70;
            arrow.style.unityTextAlign = UnityEngine.TextAnchor.MiddleCenter;
            row.Add(arrow);

            // 新値
            var newLabel = UIHelper.CreateLabel(newWisdom.ToString(), "");
            newLabel.style.fontSize = 44;
            newLabel.style.color = Color.black;
            newLabel.style.width = 100;
            newLabel.style.unityTextAlign = UnityEngine.TextAnchor.MiddleRight;
            UIHelper.ApplyFontBold(newLabel);
            row.Add(newLabel);

            // +1 バッジ
            var diffBadge = new UIE.VisualElement();
            diffBadge.style.height = 44;
            diffBadge.style.borderTopLeftRadius = 22;
            diffBadge.style.borderTopRightRadius = 22;
            diffBadge.style.borderBottomLeftRadius = 22;
            diffBadge.style.borderBottomRightRadius = 22;
            diffBadge.style.paddingLeft = 14;
            diffBadge.style.paddingRight = 14;
            diffBadge.style.alignItems = UIE.Align.Center;
            diffBadge.style.justifyContent = UIE.Justify.Center;
            diffBadge.style.marginLeft = 10;
            diffBadge.style.backgroundColor = new Color(0.667f, 0.941f, 0.82f, 0.12f);
            var diffText = UIHelper.CreateLabel("+1", "");
            diffText.style.fontSize = 26;
            diffText.style.color = new Color(0.667f, 0.941f, 0.82f);
            UIHelper.ApplyFontBold(diffText);
            diffBadge.Add(diffText);
            row.Add(diffBadge);

            // 行のフェードインアニメーション開始
            yield return null; // 1フレーム待ってtransition発火
            row.style.opacity = 1f;
            row.style.scale = new UIE.Scale(new Vector2(1f, 1f));
            yield return new WaitForSeconds(0.5f);

            // OKボタン
            bool dismissed = false;
            var okBtn = UIHelper.CreatePillButton("OK", "");
            okBtn.style.width = 500;
            okBtn.style.height = 100;
            okBtn.style.borderTopLeftRadius = 50;
            okBtn.style.borderTopRightRadius = 50;
            okBtn.style.borderBottomLeftRadius = 50;
            okBtn.style.borderBottomRightRadius = 50;
            okBtn.style.fontSize = 34;
            okBtn.style.marginTop = 30;
            okBtn.style.backgroundColor = new Color(1f, 0.718f, 0.773f); // sub
            okBtn.style.color = Color.white;
            okBtn.clicked += () => dismissed = true;
            UIHelper.ApplyFontBold(okBtn);
            growthPanel.Add(okBtn);

            // OKフェードイン
            okBtn.style.opacity = 0f;
            float okElapsed = 0f;
            while (okElapsed < 0.2f)
            {
                okElapsed += Time.deltaTime;
                okBtn.style.opacity = Mathf.Clamp01(okElapsed / 0.2f);
                yield return null;
            }
            okBtn.style.opacity = 1f;

            while (!dismissed) yield return null;

            // フェードアウト
            float fadeOut = 0.2f;
            fadeElapsed = 0f;
            while (fadeElapsed < fadeOut)
            {
                fadeElapsed += Time.deltaTime;
                overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.5f, 0, fadeElapsed / fadeOut));
                growthPanel.style.opacity = 1f - fadeElapsed / fadeOut;
                yield return null;
            }
            overlay.RemoveFromHierarchy();
        }
        else
        {
            var resultText = UIHelper.CreateLabel("また ちょうせん してね！", "milk-dialog-text");
            resultDialog.Add(resultText);

            var closeTap = UIHelper.CreateLabel("▼ タップで とじる", "milk-dialog-hint");
            resultDialog.Add(closeTap);

            yield return new WaitForSeconds(0.3f);

            tapped = false;
            overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
            while (!tapped) yield return null;

            // フェードアウト
            float fadeOut = 0.2f;
            fadeElapsed = 0f;
            while (fadeElapsed < fadeOut)
            {
                fadeElapsed += Time.deltaTime;
                overlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0.5f, 0, fadeElapsed / fadeOut));
                resultDialog.style.opacity = 1f - fadeElapsed / fadeOut;
                yield return null;
            }
            overlay.RemoveFromHierarchy();
        }

        jukuQuizActive = false;
        menuOpen = false;
    }

    void CheckElderNPC()
    {
        if (elderNpcObj == null) return;
        if (playerTileX == elderNpcX && playerTileY == elderNpcY)
        {
            if (!elderDialogueCooldown)
                OnElderTapped();
        }
        else
        {
            // プレイヤーが長老タイルから離れたらクールダウン解除
            elderDialogueCooldown = false;
        }
    }

    void OnElderTapped()
    {
        if (elderDialogueActive) return;
        if (menuOpen) return;

        elderDialogueCooldown = true;
        StartCoroutine(ShowElderDialogue());
    }

    IEnumerator ShowElderDialogue()
    {
        elderDialogueActive = true;
        menuOpen = true;

        // 縁の書に登録
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc("長老");

        bool tapped = false;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (elapsed / 0.3f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // NPC画像
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        var elderSpr = Resources.Load<Sprite>("MapCharacters/Old_Men");
        if (elderSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(elderSpr);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("門番の長老", "elder-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        string[] messages;
        if (area >= 1)
        {
            messages = new string[]
            {
                "おお…お前が ぷんぷんベイビーと あそんだのか。たいした あかんぼうだ。この さとは ながく あいつに こまっていたからな…。",
                "この先には もっと あそびたがりの おともだちも いる。つかれたら わしの左にいる ミルクの母さんを頼るといい。温かいミルクで げんきに してくれるぞ。",
                "気をつけて行くのだぞ。お前の ぼうけんは まだ はじまったばかりだ。"
            };
        }
        else
        {
            messages = new string[]
            {
                "おお、ちいさな ぼうけんしゃさんよ。ここは ひろい せかいへの いりぐちじゃ。",
                "わしの左にいる ミルクの母さんを頼るといい。温かいミルクで げんきに してくれるぞ。",
                "気をつけて行くのだぞ。お前の ぼうけんは まだ はじまったばかりだ。"
            };
        }

        for (int i = 0; i < messages.Length; i++)
        {
            textLabel.text = messages[i];
            tapHint.text = (i < messages.Length - 1) ? "▼ タップで続く" : "▼ タップで閉じる";

            yield return new WaitForSeconds(0.3f);
            tapped = false;
            while (!tapped) yield return null;
        }

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.3f);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * fadeT);
            portraitEl.style.opacity = fadeT;
            dialogBox.style.opacity = fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        menuOpen = false;
        elderDialogueActive = false;
    }

    bool IsBabyTooYoungForBoss()
    {
        return DataCarrier.Instance != null && DataCarrier.Instance.babyAge < 5;
    }

    IEnumerator ShowElderBossBlockDialogue()
    {
        elderDialogueActive = true;
        menuOpen = true;

        bool tapped = false;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (elapsed / 0.3f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // NPC画像
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        var elderSpr = Resources.Load<Sprite>("MapCharacters/Old_Men");
        if (elderSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(elderSpr);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var nameLabel = UIHelper.CreateLabel("門番の長老", "elder-dialog-name");
        dialogBox.Add(nameLabel);

        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで閉じる", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        textLabel.text = Localization.Get("elder_boss_block");

        yield return new WaitForSeconds(0.3f);
        tapped = false;
        while (!tapped) yield return null;

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.3f);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * fadeT);
            portraitEl.style.opacity = fadeT;
            dialogBox.style.opacity = fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        elderBossBlockCooldown = 3f;
        menuOpen = false;
        elderDialogueActive = false;
    }

    void ApplyPoisonStep()
    {
        if (DataCarrier.Instance == null) return;
        if (DataCarrier.Instance.babyPoisonTurns <= 0) return;

        int maxHp = DataCarrier.Instance.babyHp;
        int currentHp = DataCarrier.Instance.babyCurrentHp;
        if (currentHp < 0) currentHp = maxHp;

        int poisonDmg = Mathf.Max(2, maxHp / 16);
        currentHp = Mathf.Max(1, currentHp - poisonDmg); // HP1で止まる
        DataCarrier.Instance.babyCurrentHp = currentHp;
        DataCarrier.Instance.babyPoisonTurns--;

        ShowMessage(Localization.Get("map_poison_damage", poisonDmg));
        UpdateStatusText();

        if (DataCarrier.Instance.babyPoisonTurns <= 0)
        {
            ShowMessage(Localization.Get("map_poison_cured"));
        }
    }

    void CheckMilkPoint()
    {
        if (milkPointObj == null) return;
        // walk-over時もタップと同じ処理
        if (playerTileX == milkPointX && playerTileY == milkPointY)
            OnMilkPointTapped();
    }

    void OnMilkPointTapped()
    {
        if (milkCutinActive) return;
        if (menuOpen) return;

        bool needsHeal = false;
        if (DataCarrier.Instance != null)
        {
            int currentHp = DataCarrier.Instance.babyCurrentHp;
            int maxHp = DataCarrier.Instance.babyHp;
            bool isPoisoned = DataCarrier.Instance.babyPoisonTurns > 0;
            needsHeal = !((currentHp == -1 || currentHp >= maxHp) && !isPoisoned);
        }

        StartCoroutine(ShowMilkDialogue(needsHeal));
    }

    IEnumerator ShowMilkDialogue(bool needsHeal)
    {
        milkCutinActive = true;
        menuOpen = true;

        // 縁の書に登録
        if (DataCarrier.Instance != null)
            DataCarrier.Instance.AddMetNpc("ミルク母さん");

        bool tapped = false;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.flexDirection = UIE.FlexDirection.Column;
        overlay.style.justifyContent = UIE.Justify.FlexEnd;
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlay.RegisterCallback<UIE.ClickEvent>(evt => tapped = true);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * (elapsed / 0.3f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f);

        // NPC画像
        var portraitEl = new UIE.VisualElement();
        portraitEl.AddToClassList("milk-dialog-portrait");
        var milkMotherSpr = Resources.Load<Sprite>("MapCharacters/Milk_Mother");
        if (milkMotherSpr != null)
            portraitEl.style.backgroundImage = new UIE.StyleBackground(milkMotherSpr);
        overlay.Add(portraitEl);

        // ダイアログボックス
        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        // NPC名
        var nameLabel = UIHelper.CreateLabel("ミルクの母さん", "milk-dialog-name");
        dialogBox.Add(nameLabel);

        // セリフ
        var textLabel = UIHelper.CreateLabel("", "milk-dialog-text");
        dialogBox.Add(textLabel);

        // タップ送りヒント
        var tapHint = UIHelper.CreateLabel("▼ タップで続く", "milk-dialog-hint");
        dialogBox.Add(tapHint);

        // メッセージ一覧
        string[] messages;
        if (needsHeal)
        {
            messages = new string[]
            {
                "あらあら、こんなに小さなおててで おもちゃを にぎって…。お腹が空いたでしょう？さあ、ミルクをお飲み。",
                "よしよし、いっぱい飲んで大きくなるのよ。母さんの祈りがあなたを守ってくれるわ。",
                "危なくなったら、いつでも戻っておいで。温かいミルクを用意して待っているからね。"
            };
        }
        else
        {
            messages = new string[]
            {
                "あらあら、元気いっぱいね。何かあったらいつでも来るのよ。"
            };
        }

        for (int i = 0; i < messages.Length; i++)
        {
            textLabel.text = messages[i];
            tapHint.text = (i < messages.Length - 1) ? "▼ タップで続く" : "▼ タップで閉じる";

            // 回復は最初のメッセージをタップした後
            if (needsHeal && i == 1 && DataCarrier.Instance != null)
            {
                DataCarrier.Instance.babyCurrentHp = -1;
                DataCarrier.Instance.babyPoisonTurns = 0;
                UpdateStatusText();
            }

            // 誤タップ防止の短い待機
            yield return new WaitForSeconds(0.3f);
            tapped = false;

            // タップ待ち
            while (!tapped) yield return null;
        }

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float fadeT = 1f - (elapsed / 0.3f);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.5f * fadeT);
            portraitEl.style.opacity = fadeT;
            dialogBox.style.opacity = fadeT;
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        menuOpen = false;
        milkCutinActive = false;
        UpdateStatusText();
    }

    // ===== ステータスパネル =====

    void OpenStatusPanel()
    {
        if (statusDetailEl != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        var dc = DataCarrier.Instance;
        if (dc == null) return;

        var overlay = UIHelper.CreateOverlay();
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay) CloseStatusPanel();
        });

        statusDetailEl = new UIE.VisualElement();
        statusDetailEl.AddToClassList("map-detail-panel");

        var title = UIHelper.CreateLabel(Localization.Get("map_status_title"), "map-detail-title");
        statusDetailEl.Add(title);

        // 赤ちゃん画像
        var babyImg = new UIE.VisualElement();
        babyImg.style.width = 260;
        babyImg.style.height = 260;
        babyImg.style.marginBottom = 20;
        babyImg.style.borderTopLeftRadius = 16;
        babyImg.style.borderTopRightRadius = 16;
        babyImg.style.borderBottomLeftRadius = 16;
        babyImg.style.borderBottomRightRadius = 16;
        babyImg.style.backgroundColor = new Color(1f, 1f, 1f, 1f);

        Sprite babySprite = LoadBabySpriteForStatus(dc);
        if (babySprite != null)
        {
            babyImg.style.backgroundImage = new UIE.StyleBackground(babySprite);
            babyImg.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        statusDetailEl.Add(babyImg);

        string genderColor = dc.babyGender == "\u7537\u306e\u5b50" ? "#00BFFF" : "#FF69B4";
        string traitColor = dc.trait1 == "\u8987\u738b\u8272" ? "#FF4500" : "#FFA500";
        string godLabel = dc.isGodBaby ? "  <color=#FFD700>\u2605STAR BABY\u2605</color>" : "";

        string content = $"<b>{dc.babyName}</b>{godLabel}\n" +
            $"<color={genderColor}>{Localization.GetGender(dc.babyGender)}</color>\u3000\u3000{Localization.GetAge(dc.babyAge)}\n" +
            "\n" +
            $"{Localization.Get("map_status_hp")} {dc.babyHp}\u3000\u3000{Localization.Get("map_status_atk")} {dc.babyAtk}\u3000\u3000{Localization.Get("map_status_def")} {dc.babyDef}\n" +
            $"{Localization.Get("map_status_intelligence")} {dc.babyIntelligence}\u3000\u3000{Localization.Get("map_status_athletic")} {dc.babyAthletic}\n" +
            $"{Localization.Get("map_status_luck")} {dc.babyLuck}\u3000\u3000{Localization.Get("map_status_fortune")} {dc.babyFortune}\n" +
            "\n" +
            $"{Localization.Get("map_status_trait")} <color={traitColor}>{Localization.GetTrait(dc.trait1)}</color>\n" +
            "\n" +
            $"{Localization.Get("map_status_exp")} {dc.babyExp} / {DataCarrier.ExpForNextAge(dc.babyAge)}\n" +
            $"{Localization.Get("map_status_enemies")} {dc.defeatedEnemies}";

        var contentLabel = UIHelper.CreateLabel(content, "map-detail-content");
        contentLabel.enableRichText = true;
        contentLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        contentLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        contentLabel.style.width = 680;
        statusDetailEl.Add(contentLabel);

        // 両親画像
        var parentsRow = new UIE.VisualElement();
        parentsRow.style.flexDirection = UIE.FlexDirection.Row;
        parentsRow.style.justifyContent = UIE.Justify.Center;
        parentsRow.style.marginTop = 20;
        parentsRow.style.marginBottom = 10;

        // 父親
        string fatherImgKey = GetParentImageNameForMap(dc.fatherName);
        Sprite fatherSprite = Resources.Load<Sprite>("Parents/" + fatherImgKey);
        var fatherCol = new UIE.VisualElement();
        fatherCol.style.alignItems = UIE.Align.Center;
        fatherCol.style.marginLeft = 20;
        fatherCol.style.marginRight = 20;
        var fatherImg = new UIE.VisualElement();
        fatherImg.style.width = 140;
        fatherImg.style.height = 140;
        fatherImg.style.borderTopLeftRadius = 12;
        fatherImg.style.borderTopRightRadius = 12;
        fatherImg.style.borderBottomLeftRadius = 12;
        fatherImg.style.borderBottomRightRadius = 12;
        fatherImg.style.backgroundColor = new Color(1f, 1f, 1f, 1f);
        if (fatherSprite != null)
        {
            fatherImg.style.backgroundImage = new UIE.StyleBackground(fatherSprite);
            fatherImg.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        string capturedFatherName = dc.fatherName;
        fatherImg.RegisterCallback<UIE.ClickEvent>(evt => { UIHelper.PlayTapSE(); ShowParentBioPanel(capturedFatherName); });
        fatherCol.Add(fatherImg);
        var fatherLabel = UIHelper.CreateLabel($"{Localization.Get("map_status_father")} {dc.fatherName}", "");
        fatherLabel.style.fontSize = 22;
        fatherLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        fatherLabel.style.color = new Color(0f, 0f, 0f, 1f);
        fatherLabel.style.marginTop = 8;
        fatherCol.Add(fatherLabel);
        parentsRow.Add(fatherCol);

        // 母親
        string motherImgKey = GetParentImageNameForMap(dc.motherName);
        Sprite motherSprite = Resources.Load<Sprite>("Parents/" + motherImgKey);
        var motherCol = new UIE.VisualElement();
        motherCol.style.alignItems = UIE.Align.Center;
        motherCol.style.marginLeft = 20;
        motherCol.style.marginRight = 20;
        var motherImg = new UIE.VisualElement();
        motherImg.style.width = 140;
        motherImg.style.height = 140;
        motherImg.style.borderTopLeftRadius = 12;
        motherImg.style.borderTopRightRadius = 12;
        motherImg.style.borderBottomLeftRadius = 12;
        motherImg.style.borderBottomRightRadius = 12;
        motherImg.style.backgroundColor = new Color(1f, 1f, 1f, 1f);
        if (motherSprite != null)
        {
            motherImg.style.backgroundImage = new UIE.StyleBackground(motherSprite);
            motherImg.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
        }
        string capturedMotherName = dc.motherName;
        motherImg.RegisterCallback<UIE.ClickEvent>(evt => { UIHelper.PlayTapSE(); ShowParentBioPanel(capturedMotherName); });
        motherCol.Add(motherImg);
        var motherLabel = UIHelper.CreateLabel($"{Localization.Get("map_status_mother")} {dc.motherName}", "");
        motherLabel.style.fontSize = 22;
        motherLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        motherLabel.style.color = new Color(0f, 0f, 0f, 1f);
        motherLabel.style.marginTop = 8;
        motherCol.Add(motherLabel);
        parentsRow.Add(motherCol);

        statusDetailEl.Add(parentsRow);

        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "map-save-close-btn");
        closeBtn.clicked += () => CloseStatusPanel();
        statusDetailEl.Add(closeBtn);

        overlay.Add(statusDetailEl);
        overlayRoot.Add(overlay);
    }

    Sprite LoadBabySpriteForStatus(DataCarrier dc)
    {
        // 合成画像（おくるみ付き）を優先
        Sprite synthSprite = DataCarrier.LoadSynthBabySprite();
        if (synthSprite != null) return synthSprite;

        // フォールバック: カスタム画像 → Resources
        Sprite customSprite = DataCarrier.LoadCustomBabySprite();
        if (customSprite != null) return customSprite;

        string fatherName = GetParentImageNameForMap(dc.fatherName);
        string motherName = GetParentImageNameForMap(dc.motherName);
        string genderKey = dc.babyGender == "\u7537\u306e\u5b50" ? "male" : "female";
        string path = $"babys/{fatherName}_{motherName}_{genderKey}";
        return Resources.Load<Sprite>(path);
    }

    string GetParentImageNameForMap(string japaneseName)
    {
        switch (japaneseName)
        {
            case "\u30bf\u30b1\u30b7": return "takeshi";
            case "\u30e6\u30a6\u30ad": return "yuuki";
            case "\u30b4\u30a6": return "gou";
            case "\u30b7\u30f3\u30b8": return "shinji";
            case "\u30ea\u30e7\u30a6\u30de": return "ryouma";
            case "\u30c6\u30c4\u30e4": return "tetuya";
            case "\u30b5\u30af\u30e9": return "sakura";
            case "\u30d2\u30ca\u30bf": return "hinata";
            case "\u30a2\u30ad\u30e9": return "akira";
            case "\u30df\u30b5\u30c8": return "misato";
            case "\u30ab\u30a8\u30c7": return "kaede";
            case "\u30eb\u30ca": return "luna";
            case "\u30bc\u30cb\u30ac\u30bf": return "zenigata";
            case "\u30c4\u30af\u30e2": return "tukumo";
            case "\u30b5\u30c8\u30a6": return "satou";
            case "\u30a4\u30ef\u30aa": return "iwao";
            case "\u30a2\u30ad\u30c8\u30b7": return "akitoshi";
            case "\u30cd\u30aa": return "neo";
            case "\u30a4\u30b6\u30ca\u30df": return "izanami";
            case "\u30df\u30af": return "miku";
            case "\u30ab\u30e8\u30b3": return "kayoko";
            case "\u30d5\u30af\u30c8\u30af": return "hukutoku";
            case "\u30e8\u30cd": return "yone";
            case "\u30c9\u30af\u30b3": return "dokuko";
            default: return japaneseName.ToLower();
        }
    }

    void ShowParentBioPanel(string jaName)
    {
        string imgKey = GetParentImageNameForMap(jaName);
        Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);
        string intro = Localization.GetParentIntro(jaName);
        string bio = Localization.GetParentBio(jaName);
        string description = "";
        if (!string.IsNullOrEmpty(intro)) description += intro;
        if (!string.IsNullOrEmpty(bio))
        {
            if (description.Length > 0) description += "\n\n";
            description += bio;
        }

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("overlay-dark");
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay)
                overlay.RemoveFromHierarchy();
        });

        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        scrollView.style.maxHeight = new UIE.StyleLength(new UIE.Length(85, UIE.LengthUnit.Percent));

        var panel = new UIE.VisualElement();
        panel.style.backgroundColor = new Color(0.97f, 0.95f, 0.98f, 1f);
        panel.style.borderTopLeftRadius = 12;
        panel.style.borderTopRightRadius = 12;
        panel.style.borderBottomLeftRadius = 12;
        panel.style.borderBottomRightRadius = 12;
        panel.style.width = 980;
        panel.style.alignSelf = UIE.Align.Center;
        panel.style.alignItems = UIE.Align.Center;
        panel.style.paddingTop = 40;
        panel.style.paddingBottom = 20;

        // 画像（円形）
        var imgBorder = new UIE.VisualElement();
        imgBorder.style.width = 358;
        imgBorder.style.height = 358;
        imgBorder.style.borderTopLeftRadius = 179;
        imgBorder.style.borderTopRightRadius = 179;
        imgBorder.style.borderBottomLeftRadius = 179;
        imgBorder.style.borderBottomRightRadius = 179;
        imgBorder.style.backgroundColor = new Color(0.55f, 0.35f, 0.65f, 0.5f);
        imgBorder.style.alignItems = UIE.Align.Center;
        imgBorder.style.justifyContent = UIE.Justify.Center;

        var imgMask = new UIE.VisualElement();
        imgMask.style.width = 350;
        imgMask.style.height = 350;
        imgMask.style.borderTopLeftRadius = 175;
        imgMask.style.borderTopRightRadius = 175;
        imgMask.style.borderBottomLeftRadius = 175;
        imgMask.style.borderBottomRightRadius = 175;
        imgMask.style.overflow = UIE.Overflow.Hidden;
        imgMask.style.alignItems = UIE.Align.Center;
        imgMask.style.justifyContent = UIE.Justify.Center;
        imgMask.style.backgroundColor = Color.white;

        if (sprite != null)
        {
            var img = new UIE.VisualElement();
            img.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
            img.style.height = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
            img.style.backgroundImage = new UIE.StyleBackground(sprite);
            img.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            imgMask.Add(img);
        }
        else
        {
            imgMask.style.backgroundColor = new Color(0.85f, 0.85f, 0.85f);
        }
        imgBorder.Add(imgMask);
        panel.Add(imgBorder);

        // 名前
        var nameLabel = UIHelper.CreateLabel(jaName, "");
        nameLabel.style.fontSize = 40;
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        nameLabel.style.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        nameLabel.style.marginTop = 20;
        panel.Add(nameLabel);

        // 説明文
        if (!string.IsNullOrEmpty(description))
        {
            var descLabel = UIHelper.CreateLabel(description, "");
            descLabel.style.fontSize = 36;
            descLabel.style.unityTextAlign = TextAnchor.UpperLeft;
            descLabel.style.color = new Color(0.25f, 0.25f, 0.25f, 1f);
            descLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
            descLabel.style.marginTop = 20;
            descLabel.style.paddingLeft = 40;
            descLabel.style.paddingRight = 40;
            panel.Add(descLabel);
        }

        // 閉じるボタン
        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button");
        closeBtn.style.marginTop = 30;
        closeBtn.style.marginBottom = 20;
        closeBtn.clicked += () => { UIHelper.PlayTapSE(); overlay.RemoveFromHierarchy(); };
        panel.Add(closeBtn);

        scrollView.Add(panel);
        overlay.Add(scrollView);
        overlayRoot.Add(overlay);
    }

    void CloseStatusPanel()
    {
        if (statusDetailEl != null)
        {
            statusDetailEl.parent?.RemoveFromHierarchy();
            statusDetailEl = null;
            menuOpen = false;
            SetTouchControlsVisible(true);
        }
    }

    void OnGoHome()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.mapPlayerX = playerTileX;
            DataCarrier.Instance.mapPlayerY = playerTileY;
            if (DataCarrier.Instance.currentSlot >= 0)
                DataCarrier.Instance.SaveToSlot(DataCarrier.Instance.currentSlot);
        }
        CloseMenu();
        SceneManager.LoadScene("HomeScene");
    }

    // ===== 持ち物パネル =====

    void OpenInventoryPanel()
    {
        if (inventoryOverlayEl != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        var overlay = UIHelper.CreateOverlay();
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay) CloseInventoryPanel();
        });

        inventoryOverlayEl = new UIE.VisualElement();
        inventoryOverlayEl.AddToClassList("map-inv-panel");

        var title = UIHelper.CreateLabel(Localization.Get("map_inventory_title"), "map-detail-title");
        inventoryOverlayEl.Add(title);

        // 所持装備一覧（タップで詳細、装備の入れ替えはそうびらんで）
        string[] allOwned = DataCarrier.Instance != null ? DataCarrier.Instance.GetEquipmentList() : new string[0];
        if (allOwned.Length == 0)
        {
            var emptyEquip = UIHelper.CreateLabel(Localization.Get("map_equipment_empty"), "map-inv-empty");
            inventoryOverlayEl.Add(emptyEquip);
        }
        else
        {
            foreach (var eq in allOwned)
            {
                bool isEquipped = DataCarrier.Instance != null && DataCarrier.Instance.IsEquipped(eq);
                GetEquipItemInfo(eq, out Sprite eqSprite, out string eqEffect, out string eqBio);

                var row = new UIE.VisualElement();
                row.style.flexDirection = UIE.FlexDirection.Row;
                row.style.alignItems = UIE.Align.Center;
                row.style.minHeight = 110;
                row.style.marginTop = 8;
                row.style.paddingLeft = 16;
                row.style.paddingRight = 16;
                row.style.borderTopLeftRadius = 24;
                row.style.borderTopRightRadius = 24;
                row.style.borderBottomLeftRadius = 24;
                row.style.borderBottomRightRadius = 24;
                row.style.backgroundColor = isEquipped
                    ? new UIE.StyleColor(new Color(1f, 1f, 1f, 1f))
                    : new UIE.StyleColor(new Color(1f, 1f, 1f, 0.5f));

                // 左端：画像
                if (eqSprite != null)
                {
                    var imgEl = new UIE.VisualElement();
                    imgEl.style.width = 80;
                    imgEl.style.height = 80;
                    imgEl.style.backgroundImage = new UIE.StyleBackground(eqSprite);
                    imgEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                    imgEl.style.marginRight = 16;
                    imgEl.style.flexShrink = 0;
                    row.Add(imgEl);
                }
                else
                {
                    // ショップ装備：色付き丸プレースホルダ
                    var placeholder = new UIE.VisualElement();
                    placeholder.style.width = 80;
                    placeholder.style.height = 80;
                    placeholder.style.borderTopLeftRadius = 40;
                    placeholder.style.borderTopRightRadius = 40;
                    placeholder.style.borderBottomLeftRadius = 40;
                    placeholder.style.borderBottomRightRadius = 40;
                    placeholder.style.backgroundColor = new UIE.StyleColor(new Color(1f, 0.718f, 0.773f, 1f)); // sub color
                    placeholder.style.alignItems = UIE.Align.Center;
                    placeholder.style.justifyContent = UIE.Justify.Center;
                    placeholder.style.marginRight = 16;
                    placeholder.style.flexShrink = 0;
                    var phLabel = UIHelper.CreateLabel(eq.Substring(0, 1));
                    phLabel.style.fontSize = 32;
                    phLabel.style.color = new UIE.StyleColor(Color.white);
                    phLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    phLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    placeholder.Add(phLabel);
                    row.Add(placeholder);
                }

                // 中央：名前 + 効果
                var textCol = new UIE.VisualElement();
                textCol.style.flexDirection = UIE.FlexDirection.Column;
                textCol.style.flexGrow = 1;
                textCol.style.justifyContent = UIE.Justify.Center;

                string namePrefix = isEquipped ? "\ud83d\udee1 " : "";
                var nameLabel = UIHelper.CreateLabel(namePrefix + eq, "map-equip-item-name");
                nameLabel.enableRichText = true;
                if (!isEquipped) nameLabel.style.opacity = 0.6f;
                textCol.Add(nameLabel);

                if (!string.IsNullOrEmpty(eqEffect))
                {
                    var effectLabel = UIHelper.CreateLabel(eqEffect, "map-equip-item-effect");
                    if (!isEquipped) effectLabel.style.opacity = 0.6f;
                    textCol.Add(effectLabel);
                }

                row.Add(textCol);

                // タップで詳細表示
                string capturedEq = eq;
                row.RegisterCallback<UIE.ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    ShowEquipDetailOverlay(capturedEq);
                });

                inventoryOverlayEl.Add(row);
            }
        }

        // ミルク（通貨）
        int currentMilk = DataCarrier.Instance != null ? DataCarrier.Instance.milk : 0;
        var milkLabel = UIHelper.CreateLabel(Localization.Get("inv_milk", currentMilk), "map-inv-item");
        milkLabel.enableRichText = true;
        milkLabel.style.marginTop = 20;
        milkLabel.style.minHeight = 80;
        milkLabel.style.paddingLeft = 16;
        inventoryOverlayEl.Add(milkLabel);

        // 持ち物欄
        var itemTitle = UIHelper.CreateLabel(Localization.Get("map_inventory_title"), "map-detail-title");
        itemTitle.style.marginTop = 16;
        inventoryOverlayEl.Add(itemTitle);

        string[] items = DataCarrier.Instance != null ? DataCarrier.Instance.GetItemList() : new string[0];

        if (items.Length == 0)
        {
            var emptyLabel = UIHelper.CreateLabel(Localization.Get("map_inventory_empty"), "map-inv-empty");
            inventoryOverlayEl.Add(emptyLabel);
        }
        else
        {
            foreach (var item in items)
            {
                string displayText = item == "\u91d1\u306e\u305f\u307e\u3054" ?
                    "<color=#FFD700>\u2605 " + Localization.Get("map_item_golden_egg") + "</color>" : item;
                var itemLabel = UIHelper.CreateLabel(displayText, "map-inv-item");
                itemLabel.enableRichText = true;
                itemLabel.style.minHeight = 80;
                itemLabel.style.paddingLeft = 16;
                inventoryOverlayEl.Add(itemLabel);
            }
        }

        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "map-save-close-btn");
        closeBtn.clicked += () => CloseInventoryPanel();
        inventoryOverlayEl.Add(closeBtn);

        overlay.Add(inventoryOverlayEl);
        overlayRoot.Add(overlay);
    }

    void CloseInventoryPanel()
    {
        if (inventoryOverlayEl != null)
        {
            inventoryOverlayEl.parent?.RemoveFromHierarchy();
            inventoryOverlayEl = null;
            menuOpen = false;
            SetTouchControlsVisible(true);
        }
    }

    // ===== 装備パネル =====

    string GetMotherItemSpritePath(string motherJaName)
    {
        string imgKey = GetParentImageNameForMap(motherJaName);
        string pascal = char.ToUpper(imgKey[0]) + imgKey.Substring(1);
        return "ParentItems/Mother_" + pascal + "_Item";
    }

    static readonly string[] allMotherNames = { "\u30a4\u30b6\u30ca\u30df", "\u30df\u30af", "\u30ab\u30e8\u30b3", "\u30d5\u30af\u30c8\u30af", "\u30e8\u30cd", "\u30c9\u30af\u30b3" };

    /// <summary>装備名からスプライト・効果・bioを取得</summary>
    void GetEquipItemInfo(string equipName, out Sprite sprite, out string effectText, out string bioText)
    {
        sprite = null;
        effectText = "";
        bioText = "";

        // 母親装備チェック
        foreach (var m in allMotherNames)
        {
            string motherEquipName = Localization.Get("equip_" + m);
            if (motherEquipName == equipName)
            {
                string path = GetMotherItemSpritePath(m);
                sprite = Resources.Load<Sprite>(path);
                string ek = "equip_effect_" + m;
                string et = Localization.Get(ek);
                if (et != ek) effectText = et;
                string bk = "equip_bio_" + m;
                string bt = Localization.Get(bk);
                if (bt != bk) bioText = bt;
                return;
            }
        }

        // ショップ装備チェック
        foreach (var si in shopItems)
        {
            if (si.equipName == equipName)
            {
                // 装備画面用の詳細効果があればそちらを優先
                string realKey = "equip_real_effect_" + si.key;
                string realText = Localization.Get(realKey);
                if (realText != realKey)
                    effectText = realText;
                else
                {
                    string ek = "shop_effect_" + si.key;
                    string et = Localization.Get(ek);
                    if (et != ek) effectText = et;
                }
                return;
            }
        }
        foreach (var si in villageShopItems)
        {
            if (si.equipName == equipName)
            {
                string realKey = "equip_real_effect_" + si.key;
                string realText = Localization.Get(realKey);
                if (realText != realKey)
                    effectText = realText;
                else
                {
                    string ek = "shop_effect_" + si.key;
                    string et = Localization.Get(ek);
                    if (et != ek) effectText = et;
                }
                return;
            }
        }

        // イベント報酬装備
        if (equipName == "ゴールデン・ベビーステッキ")
        {
            var tex = Resources.Load<Texture2D>("Shop/ゴールデンベビーステッキ");
            if (tex != null)
                sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            effectText = Localization.Get("shop_effect_golden_stick");
            return;
        }
        if (equipName == "かぐやのリボン")
        {
            effectText = Localization.Get("shop_effect_kaguya_ribbon");
            return;
        }
    }

    void OpenEquipmentPanel()
    {
        if (equipmentOverlayEl != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        var overlay = UIHelper.CreateOverlay();
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay) CloseEquipmentPanel();
        });

        equipmentOverlayEl = new UIE.VisualElement();
        equipmentOverlayEl.AddToClassList("map-equip-panel");

        int equippedCount = DataCarrier.Instance != null ? DataCarrier.Instance.GetEquippedCount() : 0;
        var title = UIHelper.CreateLabel(
            Localization.Get("map_equipment_slot_title", equippedCount, DataCarrier.MAX_EQUIP_SLOTS),
            "map-equip-title");
        equipmentOverlayEl.Add(title);

        string[] allOwned = DataCarrier.Instance != null ? DataCarrier.Instance.GetEquipmentList() : new string[0];

        if (allOwned.Length == 0)
        {
            var emptyLabel = UIHelper.CreateLabel(Localization.Get("map_equipment_empty"), "map-inv-empty");
            equipmentOverlayEl.Add(emptyLabel);
        }
        else
        {
            // 装備中
            string[] equipped = DataCarrier.Instance != null ? DataCarrier.Instance.GetEquippedList() : new string[0];
            if (equipped.Length > 0)
            {
                foreach (var eq in equipped)
                {
                    GetEquipItemInfo(eq, out Sprite eqSprite, out string eqEffect, out string _);
                    var itemRow = BuildEquipRow(eq, eqSprite, eqEffect, true);

                    // タップで詳細表示
                    string capturedEqDetail = eq;
                    itemRow.RegisterCallback<UIE.ClickEvent>(evt =>
                    {
                        evt.StopPropagation();
                        ShowEquipDetailOverlay(capturedEqDetail);
                    });

                    string capturedEq = eq;
                    var unequipBtn = UIHelper.CreatePillButton(Localization.Get("equip_remove"), "pill-button-small");
                    unequipBtn.style.width = 120;
                    unequipBtn.style.height = 50;
                    unequipBtn.style.flexShrink = 0;
                    unequipBtn.clicked += () =>
                    {
                        if (DataCarrier.Instance == null) return;
                        DataCarrier.Instance.UnequipItem(capturedEq);
                        DataCarrier.Instance.SaveData();
                        UpdateStatusText();
                        CloseEquipmentPanel();
                        OpenEquipmentPanel();
                    };
                    itemRow.Add(unequipBtn);

                    equipmentOverlayEl.Add(itemRow);
                }
            }

            // 未装備
            bool hasUnequipped = false;
            foreach (var ow in allOwned)
            {
                if (DataCarrier.Instance != null && DataCarrier.Instance.IsEquipped(ow)) continue;
                if (!hasUnequipped)
                {
                    var secTitle = UIHelper.CreateLabel(Localization.Get("equip_unequipped_section"), "map-equip-item-effect");
                    secTitle.style.marginTop = 16;
                    secTitle.style.marginBottom = 4;
                    secTitle.style.unityTextAlign = TextAnchor.MiddleCenter;
                    secTitle.style.fontSize = 22;
                    equipmentOverlayEl.Add(secTitle);
                    hasUnequipped = true;
                }

                GetEquipItemInfo(ow, out Sprite owSprite, out string owEffect, out string _);
                var itemRow = BuildEquipRow(ow, owSprite, owEffect, false);

                // タップで詳細表示
                string capturedOwDetail = ow;
                itemRow.RegisterCallback<UIE.ClickEvent>(evt =>
                {
                    evt.StopPropagation();
                    ShowEquipDetailOverlay(capturedOwDetail);
                });

                bool slotsFull = DataCarrier.Instance != null && DataCarrier.Instance.GetEquippedCount() >= DataCarrier.MAX_EQUIP_SLOTS;
                string capturedOw = ow;
                var equipBtn = UIHelper.CreatePillButton(Localization.Get("equip_set"), "pill-button-small");
                equipBtn.style.width = 120;
                equipBtn.style.height = 50;
                equipBtn.style.flexShrink = 0;
                if (slotsFull)
                {
                    equipBtn.SetEnabled(false);
                    equipBtn.style.opacity = 0.4f;
                }
                equipBtn.clicked += () =>
                {
                    if (DataCarrier.Instance == null) return;
                    DataCarrier.Instance.EquipItem(capturedOw);
                    DataCarrier.Instance.SaveData();
                    UpdateStatusText();
                    CloseEquipmentPanel();
                    OpenEquipmentPanel();
                };
                itemRow.Add(equipBtn);

                equipmentOverlayEl.Add(itemRow);
            }
        }

        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "map-save-close-btn");
        closeBtn.clicked += () => CloseEquipmentPanel();
        equipmentOverlayEl.Add(closeBtn);

        overlay.Add(equipmentOverlayEl);
        overlayRoot.Add(overlay);
    }

    /// <summary>装備パネル用の行を構築（画像 + 名前 + 効果）</summary>
    UIE.VisualElement BuildEquipRow(string equipName, Sprite sprite, string effectText, bool isEquipped)
    {
        var row = new UIE.VisualElement();
        row.style.flexDirection = UIE.FlexDirection.Row;
        row.style.alignItems = UIE.Align.Center;
        row.style.justifyContent = UIE.Justify.SpaceBetween;
        row.style.minHeight = 110;
        row.style.marginTop = 8;
        row.style.paddingLeft = 16;
        row.style.paddingRight = 16;
        row.style.borderTopLeftRadius = 24;
        row.style.borderTopRightRadius = 24;
        row.style.borderBottomLeftRadius = 24;
        row.style.borderBottomRightRadius = 24;
        row.style.backgroundColor = isEquipped
            ? new UIE.StyleColor(new Color(0.93f, 0.93f, 1f))
            : new UIE.StyleColor(new Color(0.96f, 0.96f, 0.96f));
        row.style.width = new UIE.StyleLength(UIE.Length.Percent(100));

        // 画像
        if (sprite != null)
        {
            var imgEl = new UIE.VisualElement();
            imgEl.style.width = 80;
            imgEl.style.height = 80;
            imgEl.style.backgroundImage = new UIE.StyleBackground(sprite);
            imgEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            imgEl.style.marginRight = 16;
            imgEl.style.flexShrink = 0;
            row.Add(imgEl);
        }
        else
        {
            var placeholder = new UIE.VisualElement();
            placeholder.style.width = 80;
            placeholder.style.height = 80;
            placeholder.style.borderTopLeftRadius = 40;
            placeholder.style.borderTopRightRadius = 40;
            placeholder.style.borderBottomLeftRadius = 40;
            placeholder.style.borderBottomRightRadius = 40;
            placeholder.style.backgroundColor = new UIE.StyleColor(new Color(0.7f, 0.75f, 0.9f));
            placeholder.style.alignItems = UIE.Align.Center;
            placeholder.style.justifyContent = UIE.Justify.Center;
            placeholder.style.marginRight = 16;
            placeholder.style.flexShrink = 0;
            var phLabel = UIHelper.CreateLabel(equipName.Substring(0, 1));
            phLabel.style.fontSize = 32;
            phLabel.style.color = new UIE.StyleColor(Color.white);
            phLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            phLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholder.Add(phLabel);
            row.Add(placeholder);
        }

        // 名前 + 効果
        var textCol = new UIE.VisualElement();
        textCol.style.flexDirection = UIE.FlexDirection.Column;
        textCol.style.flexGrow = 1;
        textCol.style.justifyContent = UIE.Justify.Center;

        var nameLabel = UIHelper.CreateLabel(equipName, "map-equip-item-name");
        if (!isEquipped) nameLabel.style.opacity = 0.7f;
        textCol.Add(nameLabel);

        if (!string.IsNullOrEmpty(effectText))
        {
            var effectLabel = UIHelper.CreateLabel(effectText, "map-equip-item-effect");
            if (!isEquipped) effectLabel.style.opacity = 0.7f;
            textCol.Add(effectLabel);
        }

        row.Add(textCol);
        return row;
    }

    void CloseEquipmentPanel()
    {
        if (equipmentOverlayEl != null)
        {
            equipmentOverlayEl.parent?.RemoveFromHierarchy();
            equipmentOverlayEl = null;
            menuOpen = false;
            SetTouchControlsVisible(true);
        }
    }

    /// <summary>装備詳細オーバーレイを表示（持ち物・装備パネルからタップ）</summary>
    void ShowEquipDetailOverlay(string equipName)
    {
        GetEquipItemInfo(equipName, out Sprite sprite, out string effectText, out string bioText);
        bool isEquipped = DataCarrier.Instance != null && DataCarrier.Instance.IsEquipped(equipName);

        var overlay = UIHelper.CreateOverlay();
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay) overlay.RemoveFromHierarchy();
        });

        var card = new UIE.VisualElement();
        card.AddToClassList("map-equip-bio-panel");

        // 画像（大きく表示）
        if (sprite != null)
        {
            var imgEl = new UIE.VisualElement();
            imgEl.style.width = 200;
            imgEl.style.height = 200;
            imgEl.style.backgroundImage = new UIE.StyleBackground(sprite);
            imgEl.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            imgEl.style.marginBottom = 16;
            card.Add(imgEl);
        }
        else
        {
            // プレースホルダー（頭文字）
            var placeholder = new UIE.VisualElement();
            placeholder.style.width = 200;
            placeholder.style.height = 200;
            placeholder.style.borderTopLeftRadius = 100;
            placeholder.style.borderTopRightRadius = 100;
            placeholder.style.borderBottomLeftRadius = 100;
            placeholder.style.borderBottomRightRadius = 100;
            placeholder.style.backgroundColor = new UIE.StyleColor(new Color(0.7f, 0.75f, 0.9f));
            placeholder.style.alignItems = UIE.Align.Center;
            placeholder.style.justifyContent = UIE.Justify.Center;
            placeholder.style.marginBottom = 16;
            var phLabel = UIHelper.CreateLabel(equipName.Substring(0, 1));
            phLabel.style.fontSize = 60;
            phLabel.style.color = new UIE.StyleColor(Color.white);
            phLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            phLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            placeholder.Add(phLabel);
            card.Add(placeholder);
        }

        // 名前
        var nameLabel = UIHelper.CreateLabel(equipName, "map-equip-item-name");
        nameLabel.style.fontSize = 38;
        nameLabel.style.marginBottom = 12;
        nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        card.Add(nameLabel);

        // 装備中バッジ
        if (isEquipped)
        {
            var badge = UIHelper.CreateLabel("\ud83d\udee1 " + Localization.Get("equip_equipped_badge"));
            badge.style.fontSize = 24;
            badge.style.color = new UIE.StyleColor(new Color(0.3f, 0.5f, 0.8f));
            badge.style.unityFontStyleAndWeight = FontStyle.Bold;
            badge.style.unityTextAlign = TextAnchor.MiddleCenter;
            badge.style.marginBottom = 8;
            card.Add(badge);
        }

        // 効果（目立つ表示）
        if (!string.IsNullOrEmpty(effectText))
        {
            var effectBox = new UIE.VisualElement();
            effectBox.style.backgroundColor = new UIE.StyleColor(new Color(0.95f, 0.93f, 1f));
            effectBox.style.borderTopLeftRadius = 20;
            effectBox.style.borderTopRightRadius = 20;
            effectBox.style.borderBottomLeftRadius = 20;
            effectBox.style.borderBottomRightRadius = 20;
            effectBox.style.paddingTop = 14;
            effectBox.style.paddingBottom = 14;
            effectBox.style.paddingLeft = 24;
            effectBox.style.paddingRight = 24;
            effectBox.style.marginBottom = 12;
            effectBox.style.alignItems = UIE.Align.Center;

            var effectLabel = UIHelper.CreateLabel(effectText);
            effectLabel.style.fontSize = 32;
            effectLabel.style.color = new UIE.StyleColor(new Color(0.4f, 0.2f, 0.6f));
            effectLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            effectLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            effectBox.Add(effectLabel);
            card.Add(effectBox);
        }

        // Bio
        if (!string.IsNullOrEmpty(bioText))
        {
            var bioLabel = UIHelper.CreateLabel(bioText, "map-equip-bio-text");
            bioLabel.style.marginTop = 8;
            card.Add(bioLabel);
        }

        // 閉じるボタン
        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "map-save-close-btn");
        closeBtn.style.marginTop = 20;
        closeBtn.clicked += () => overlay.RemoveFromHierarchy();
        card.Add(closeBtn);

        overlay.Add(card);
        overlayRoot.Add(overlay);
    }

    // ===== 装備ショップ =====

    struct ShopItem
    {
        public string key;       // localization key suffix
        public string equipName; // equipment name for DataCarrier
        public int price;
        public bool requiresS;   // Sランク限定
        public string imagePath; // Resources.Loadパス（null=画像なし）
    }

    static readonly ShopItem[] shopItems = new ShopItem[]
    {
        new ShopItem { key = "garagara", equipName = "ガラガラソード", price = 30, imagePath = "Shop/ガラガラ" },
        new ShopItem { key = "yodare", equipName = "よだれかけシールド", price = 30, imagePath = "Shop/よだれかけ" },
        new ShopItem { key = "oshaburi", equipName = "おしゃぶりチャーム", price = 50 },
        new ShopItem { key = "omutsu", equipName = "魔法のおむつ", price = 60, imagePath = "Shop/おむつ" },
        new ShopItem { key = "honyubin", equipName = "黄金のほ乳瓶", price = 80, imagePath = "Shop/きんいろの哺乳瓶" },
        new ShopItem { key = "tiara", equipName = "悪魔のティアラ", price = 100 },
    };

    static readonly ShopItem[] villageShopItems = new ShopItem[]
    {
        new ShopItem { key = "nakineko", equipName = "泣き猫パンチ", price = 20 },
        new ShopItem { key = "yodarekake_mini", equipName = "ミニよだれかけ", price = 15 },
        new ShopItem { key = "niji_rattle", equipName = "にじいろガラガラ", price = 50, requiresS = true },
    };

    void OpenWeaponShop()
    {
        if (shopOverlayEl != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        var overlay = UIHelper.CreateOverlay();
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay) CloseWeaponShop();
        });

        shopOverlayEl = new UIE.VisualElement();
        shopOverlayEl.AddToClassList("map-shop-panel");

        var title = UIHelper.CreateLabel(Localization.Get("shop_title"), "map-shop-title");
        shopOverlayEl.Add(title);

        int currentMilk = DataCarrier.Instance != null ? DataCarrier.Instance.milk : 0;
        var milkLabel = UIHelper.CreateLabel(Localization.Get("shop_milk_label", currentMilk), "map-shop-milk");
        milkLabel.name = "shop-milk-label";
        shopOverlayEl.Add(milkLabel);

        // エリアに応じて商品リストを切り替え
        int entryArea = DataCarrier.Instance != null ? DataCarrier.Instance.shopEntryArea : 1;
        ShopItem[] currentShopItems = entryArea == 0 ? villageShopItems : shopItems;

        // スクロール可能なグリッド表示
        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.flexGrow = 1;
        scrollView.style.maxHeight = 850;

        var grid = new UIE.VisualElement();
        grid.AddToClassList("map-shop-grid");

        foreach (var item in currentShopItems)
        {
            bool owned = DataCarrier.Instance != null && DataCarrier.Instance.HasEquipment(item.equipName);

            var card = new UIE.VisualElement();
            card.AddToClassList("map-shop-card");
            if (owned) card.style.opacity = 0.5f;

            // 画像
            var imgEl = new UIE.VisualElement();
            imgEl.AddToClassList("map-shop-card-img");
            if (!string.IsNullOrEmpty(item.imagePath))
            {
                var spr = Resources.Load<Sprite>(item.imagePath);
                if (spr != null)
                    imgEl.style.backgroundImage = new UIE.StyleBackground(spr);
                else
                    imgEl.style.backgroundColor = new Color(0.85f, 0.80f, 0.90f);
            }
            else
            {
                imgEl.style.backgroundColor = new Color(0.85f, 0.80f, 0.90f);
                var qLabel = UIHelper.CreateLabel("?", null);
                qLabel.style.fontSize = 60;
                qLabel.style.color = new Color(0.6f, 0.5f, 0.7f);
                qLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                qLabel.style.flexGrow = 1;
                imgEl.Add(qLabel);
            }
            card.Add(imgEl);

            // 名前
            var nameLabel = UIHelper.CreateLabel(Localization.Get("shop_item_" + item.key), "map-shop-card-name");
            card.Add(nameLabel);

            // 購入済みバッジ
            if (owned)
            {
                var badge = UIHelper.CreateLabel("持ってるよ！", null);
                UIHelper.ApplyFont(badge);
                badge.style.fontSize = 24;
                badge.style.color = new Color(0.47f, 0.71f, 0.47f);
                badge.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
                badge.style.unityTextAlign = TextAnchor.MiddleCenter;
                badge.style.marginTop = 4;
                card.Add(badge);
            }

            // タップで詳細を開く
            ShopItem capturedItem = item;
            card.RegisterCallback<UIE.ClickEvent>(evt =>
            {
                evt.StopPropagation();
                ShowShopItemDetail(capturedItem);
            });

            grid.Add(card);
        }

        scrollView.Add(grid);
        shopOverlayEl.Add(scrollView);

        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button");
        closeBtn.style.marginTop = 20;
        closeBtn.style.height = 100;
        closeBtn.style.fontSize = 34;
        closeBtn.clicked += () => CloseWeaponShop();
        shopOverlayEl.Add(closeBtn);

        overlay.Add(shopOverlayEl);
        overlayRoot.Add(overlay);
    }

    UIE.VisualElement shopDetailOverlay;

    void ShowShopItemDetail(ShopItem item)
    {
        if (shopDetailOverlay != null)
        {
            shopDetailOverlay.RemoveFromHierarchy();
            shopDetailOverlay = null;
        }

        shopDetailOverlay = UIHelper.CreateOverlay();
        shopDetailOverlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == shopDetailOverlay)
            {
                shopDetailOverlay.RemoveFromHierarchy();
                shopDetailOverlay = null;
            }
        });

        var panel = new UIE.VisualElement();
        panel.AddToClassList("map-shop-detail-panel");

        // 画像
        var imgEl = new UIE.VisualElement();
        imgEl.AddToClassList("map-shop-detail-img");
        if (!string.IsNullOrEmpty(item.imagePath))
        {
            var spr = Resources.Load<Sprite>(item.imagePath);
            if (spr != null)
                imgEl.style.backgroundImage = new UIE.StyleBackground(spr);
            else
                imgEl.style.backgroundColor = new Color(0.85f, 0.80f, 0.90f);
        }
        else
        {
            imgEl.style.backgroundColor = new Color(0.85f, 0.80f, 0.90f);
        }
        panel.Add(imgEl);

        // 名前
        var nameLabel = UIHelper.CreateLabel(Localization.Get("shop_item_" + item.key), "map-shop-detail-name");
        panel.Add(nameLabel);

        // 効果
        var effectLabel = UIHelper.CreateLabel(Localization.Get("shop_effect_" + item.key), "map-shop-detail-effect");
        panel.Add(effectLabel);

        // 価格
        int currentMilk = DataCarrier.Instance != null ? DataCarrier.Instance.milk : 0;
        var priceLabel = UIHelper.CreateLabel("🍼 " + item.price + " ミルク", "map-shop-detail-price");
        panel.Add(priceLabel);

        bool owned = DataCarrier.Instance != null && DataCarrier.Instance.HasEquipment(item.equipName);
        bool isSRank = DataCarrier.Instance != null
            && BabySynthesizer.DetermineRank(DataCarrier.Instance.babyFortune) == BabySynthesizer.BabyRank.S;

        if (owned)
        {
            var purchasedBtn = UIHelper.CreatePillButton("持ってるよ！", "pill-button");
            purchasedBtn.style.height = 100;
            purchasedBtn.style.fontSize = 34;
            purchasedBtn.style.opacity = 0.4f;
            purchasedBtn.SetEnabled(false);
            panel.Add(purchasedBtn);
        }
        else if (item.requiresS && !isSRank)
        {
            var lockedBtn = UIHelper.CreatePillButton(Localization.Get("shop_s_rank_only"), "pill-button");
            lockedBtn.style.height = 100;
            lockedBtn.style.fontSize = 34;
            lockedBtn.style.opacity = 0.4f;
            lockedBtn.SetEnabled(false);
            panel.Add(lockedBtn);
        }
        else
        {
            var buyBtn = UIHelper.CreatePillButton(Localization.Get("shop_buy", item.price), "pill-button");
            buyBtn.style.height = 100;
            buyBtn.style.fontSize = 34;
            bool canAfford = currentMilk >= item.price;
            if (!canAfford)
            {
                buyBtn.SetEnabled(false);
                buyBtn.style.opacity = 0.4f;
            }

            string capturedEquipName = item.equipName;
            string capturedKey = item.key;
            int capturedPrice = item.price;
            buyBtn.clicked += () =>
            {
                if (DataCarrier.Instance == null) return;
                if (DataCarrier.Instance.milk < capturedPrice) return;

                DataCarrier.Instance.milk -= capturedPrice;
                DataCarrier.Instance.AddEquipment(capturedEquipName);
                DataCarrier.Instance.EquipItem(capturedEquipName);
                DataCarrier.Instance.SaveData();
                UpdateStatusText();

                // 詳細を閉じてショップを再構築
                shopDetailOverlay.RemoveFromHierarchy();
                shopDetailOverlay = null;
                CloseWeaponShop();
                ShowMessage(Localization.Get("shop_bought", Localization.Get("shop_item_" + capturedKey)));
                StartCoroutine(ReopenShopAfterDelay());
            };

            panel.Add(buyBtn);
        }

        // 閉じるボタン
        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button");
        closeBtn.style.height = 100;
        closeBtn.style.fontSize = 34;
        closeBtn.style.marginTop = 12;
        closeBtn.clicked += () =>
        {
            shopDetailOverlay.RemoveFromHierarchy();
            shopDetailOverlay = null;
        };
        panel.Add(closeBtn);

        shopDetailOverlay.Add(panel);
        overlayRoot.Add(shopDetailOverlay);
    }

    IEnumerator ReopenShopAfterDelay()
    {
        yield return new WaitForSeconds(0.8f);
        OpenWeaponShop();
    }

    void CloseWeaponShop()
    {
        if (shopDetailOverlay != null)
        {
            shopDetailOverlay.RemoveFromHierarchy();
            shopDetailOverlay = null;
        }
        if (shopOverlayEl != null)
        {
            shopOverlayEl.parent?.RemoveFromHierarchy();
            shopOverlayEl = null;
            menuOpen = false;
            SetTouchControlsVisible(true);
        }
    }

    // ===== メッセージ表示 =====

    Coroutine messageCoroutine;
    UIE.VisualElement currentMessageBox;

    void ShowMessage(string msg)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        if (currentMessageBox != null)
        {
            currentMessageBox.RemoveFromHierarchy();
            currentMessageBox = null;
        }
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(msg));
    }

    IEnumerator ShowMessageCoroutine(string msg)
    {
        var box = new UIE.VisualElement();
        box.AddToClassList("map-message-box");
        box.style.bottom = new UIE.StyleLength(new UIE.Length(35, UIE.LengthUnit.Percent));

        var label = UIHelper.CreateLabel(msg, "map-message-text");
        label.enableRichText = true;
        box.Add(label);

        overlayRoot.Add(box);
        currentMessageBox = box;

        yield return new WaitForSeconds(2f);

        box.RemoveFromHierarchy();
        if (currentMessageBox == box) currentMessageBox = null;
    }

    // ===== タッチコントロール =====

    void CreateTouchControls()
    {
        if (overlayRoot == null) return;

        touchSwipeEl = new UIE.VisualElement();
        touchSwipeEl.AddToClassList("map-swipe-area");

        touchSwipeEl.RegisterCallback<UIE.PointerDownEvent>(evt =>
        {
            if (moveCtrl != null)
                moveCtrl.OnPointerDown(new Vector2(evt.position.x, evt.position.y));
        });

        touchSwipeEl.RegisterCallback<UIE.PointerMoveEvent>(evt =>
        {
            if (moveCtrl != null)
                moveCtrl.OnPointerMove(new Vector2(evt.position.x, evt.position.y));
        });

        touchSwipeEl.RegisterCallback<UIE.PointerUpEvent>(evt =>
        {
            if (moveCtrl != null)
                moveCtrl.OnPointerUp(new Vector2(evt.position.x, evt.position.y));
        });

        overlayRoot.Add(touchSwipeEl);
    }

    void SetTouchControlsVisible(bool visible)
    {
        if (touchSwipeEl != null)
            touchSwipeEl.style.display = visible ?
                UIE.DisplayStyle.Flex : UIE.DisplayStyle.None;
    }

    void OnDestroy()
    {
        foreach (var sym in enemySymbols) sym.Destroy();
        if (moveCtrl != null) moveCtrl.Destroy();
        if (villagePanelSettings != null)
            Destroy(villagePanelSettings);
        if (overlayPanelSettings != null)
            Destroy(overlayPanelSettings);
    }
}
