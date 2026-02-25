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

    // はいはいアニメーション
    RectTransform crawlHandL, crawlHandR;
    RectTransform crawlKneeL, crawlKneeR;
    Vector2 crawlHandLBase, crawlHandRBase;
    Vector2 crawlKneeLBase, crawlKneeRBase;
    float crawlAnimTime;

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
    Sprite poisonLakeSprite, poisonFenceSprite, poisonRoadSprite;

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
    const int GOLDEN_EGG_X_AREA3 = 4;
    const int GOLDEN_EGG_Y_AREA3 = 31;

    // ミルクポイント（回復）
    GameObject milkPointObj;
    int milkPointX = 3;
    int milkPointY = 10;
    bool milkCutinActive = false;

    // 門番（長老）NPC
    GameObject elderNpcObj;
    int elderNpcX = 6;
    int elderNpcY = 10;
    bool elderDialogueActive = false;

    // 実家（母親）NPC
    GameObject motherNpcObj;
    int motherNpcX = 9;
    int motherNpcY = 5;
    bool motherDialogueActive = false;

    // 父親の家NPC（家の中にいる設定）
    GameObject fatherNpcObj;
    int fatherNpcX = 3;
    int fatherNpcY = 32;
    bool fatherDialogueActive = false;

    // 武器屋の商人NPC
    GameObject merchantNpcObj;
    int merchantNpcX = 4;
    int merchantNpcY = 7;
    bool merchantDialogueActive = false;

    // 塾の先生NPC
    GameObject jukuTeacherObj;
    int jukuTeacherX = 4;
    int jukuTeacherY = 6;
    bool jukuQuizActive = false;

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
            mapWidth = 24;
            mapHeight = 40;
        }
        else if (area == 5 || area == 6 || area == 7)
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
        }

        LoadTileset();

        // SE読み込み
        seSource = gameObject.AddComponent<AudioSource>();
        seQuizCorrect = Resources.Load<AudioClip>("SE/クイズ正解1");
        seQuizWrong = Resources.Load<AudioClip>("SE/クイズ不正解1");
        seFootstep = Resources.Load<AudioClip>("SE/可愛い足音");

        if (area == 7)
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

        if (area == 7)
        {
            // 塾内部: ランダムエンカウントなし
            CreateJukuTeacherNPC();
        }
        else if (area == 6)
        {
            // 武器屋内部: ランダムエンカウントなし
            CreateMerchantNPC();
        }
        else if (area == 5)
        {
            // 父親の家: ランダムエンカウントなし
            fatherNpcX = 4;
            fatherNpcY = 7;
            CreateFatherInteriorNPC();
        }
        else if (area == 4)
        {
            // 109館内: ランダムエンカウントなし、ミルクポイントなし、おともだちなし
        }
        else if (area == 3)
        {
            milkPointX = 6;
            milkPointY = 18;
            CreateMilkPoint();
            CreateGoldenEgg();
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

        // プレイヤーを最前面に
        if (playerObj != null)
            playerObj.transform.SetAsLastSibling();

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
        if (area == 0 || area == 5 || area == 6)
            clipPath = "BGM/mura1";

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
        poisonRoadTex = Resources.Load<Texture2D>("Map/Poison_Road");
        if (poisonLakeTex != null)
            poisonLakeSprite = Sprite.Create(poisonLakeTex, new Rect(0, 0, poisonLakeTex.width, poisonLakeTex.height), new Vector2(0.5f, 0.5f));
        if (poisonFenceTex != null)
            poisonFenceSprite = Sprite.Create(poisonFenceTex, new Rect(0, 0, poisonFenceTex.width, poisonFenceTex.height), new Vector2(0.5f, 0.5f));
        if (poisonRoadTex != null)
            poisonRoadSprite = Sprite.Create(poisonRoadTex, new Rect(0, 0, poisonRoadTex.width, poisonRoadTex.height), new Vector2(0.5f, 0.5f));

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
            case TILE_BOSS_MANSION:
            case TILE_BOSS_GATE:
                ApplyDQDirtStyle(tile, true);
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

        // 基本は草で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_GRASS;
                walkable[x, y] = true;
            }
        }

        // === 外周 ===
        // 上下（正面向き柵）
        for (int x = 0; x < mapWidth; x++)
        {
            mapData[x, 0] = TILE_FENCE;
            walkable[x, 0] = false;
            mapData[x, mapHeight - 1] = TILE_FENCE;
            walkable[x, mapHeight - 1] = false;
        }
        // 左右（芝で塞ぐ — 柵画像が横向きに合わないため）
        for (int y = 0; y < mapHeight; y++)
        {
            mapData[0, y] = TILE_GRASS;
            walkable[0, y] = false;
            mapData[mapWidth - 1, y] = TILE_GRASS;
            walkable[mapWidth - 1, y] = false;
        }

        // === エリア出口（南端） ===
        mapData[5, 0] = TILE_AREA_EXIT;
        walkable[5, 0] = true;

        // ============================================================
        // 南エリア（y=0〜13）— ゴージャス・ヴィレッジの中心
        // ============================================================

        // === 暗い土の道（メインストリート + 縦パス） ===
        // 横メインストリート y=9, y=10 (x=1〜x=10)
        for (int x = 1; x <= 10; x++)
        {
            mapData[x, 9] = TILE_DARK_DIRT;
            mapData[x, 10] = TILE_DARK_DIRT;
        }

        // 縦メインパス x=5 (y=1〜y=35 — 南端からボス手前まで)
        for (int y = 1; y <= 35; y++)
        {
            mapData[5, y] = TILE_DARK_DIRT;
        }

        // === 溶岩池1（x=4〜6, y=2〜3） ===
        for (int x = 4; x <= 6; x++)
        {
            for (int y = 2; y <= 3; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 溶岩池2（x=2〜3, y=18） ===
        mapData[2, 18] = TILE_WATER;
        walkable[2, 18] = false;
        mapData[3, 18] = TILE_WATER;
        walkable[3, 18] = false;

        // === 溶岩池3（x=7〜9, y=30） ===
        mapData[7, 30] = TILE_WATER;
        walkable[7, 30] = false;
        mapData[8, 30] = TILE_WATER;
        walkable[8, 30] = false;
        mapData[9, 30] = TILE_WATER;
        walkable[9, 30] = false;

        // ============================================================
        // 北エリア（y=33〜38）— ボスの館
        // ============================================================

        // ボス接続パス y=35 (x=5〜7)
        for (int x = 5; x <= 7; x++)
        {
            mapData[x, 35] = TILE_DARK_DIRT;
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

        // 柵（館の左右 y=37）
        mapData[6, 37] = TILE_FENCE;
        walkable[6, 37] = false;
        mapData[10, 37] = TILE_FENCE;
        walkable[10, 37] = false;

        // ボスの門 (8, 35)
        mapData[8, 35] = TILE_BOSS_GATE;
        walkable[8, 35] = false;

        // === 装備ショップ (2x2: x=2〜3, y=14〜15) ===
        for (int sx = 2; sx <= 3; sx++)
        {
            for (int sy = 14; sy <= 15; sy++)
            {
                mapData[sx, sy] = TILE_WEAPON_SHOP;
                walkable[sx, sy] = false;
            }
        }
        // ショップへの接続道
        mapData[4, 14] = TILE_DARK_DIRT;

        // プレイヤーの初期位置は必ず歩けるようにする
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_DARK_DIRT && mapData[playerTileX, playerTileY] != TILE_GRASS)
        {
            mapData[playerTileX, playerTileY] = TILE_DARK_DIRT;
        }
    }

    void GenerateFatherHouseMapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 全て壁で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_FATHER_WALL;
                walkable[x, y] = false;
            }
        }

        // 床エリア (x=1〜6, y=2〜8)
        for (int x = 1; x <= 6; x++)
        {
            for (int y = 2; y <= 8; y++)
            {
                mapData[x, y] = TILE_FATHER_FLOOR;
                walkable[x, y] = true;
            }
        }

        // 出口 (3,1)(4,1)
        mapData[3, 1] = TILE_FATHER_EXIT;
        walkable[3, 1] = false;
        mapData[4, 1] = TILE_FATHER_EXIT;
        walkable[4, 1] = false;

        // 父親NPC位置 (4, 7) — 歩行可能（NPC上に乗って会話トリガー）
        // walkable[4, 7] は true のまま

        // 家具: テーブル (2,5)(3,5)
        walkable[2, 5] = false;
        walkable[3, 5] = false;
        // 棚 (6, 8)
        walkable[6, 8] = false;
        // 暖炉 (1, 7)(1, 8)
        walkable[1, 7] = false;
        walkable[1, 8] = false;
    }

    void CreateFatherHouseFurniture()
    {
        if (tilesContainer == null) return;

        // ラグ (3-4, 3-4) — 淡い赤のカーペット
        CreateFurnitureOverlay(3.5f, 3.5f, DISPLAY_TILE * 2.2f, DISPLAY_TILE * 2.2f,
            new Color(0.6f, 0.3f, 0.25f, 0.5f));

        // テーブル (2,5)(3,5)
        CreateFurnitureOverlay(2.5f, 5f, DISPLAY_TILE * 2f, DISPLAY_TILE * 0.8f,
            new Color(0.5f, 0.35f, 0.2f));

        // 棚 (6, 8)
        CreateFurnitureOverlay(6f, 8f, DISPLAY_TILE * 0.8f, DISPLAY_TILE * 0.9f,
            new Color(0.4f, 0.28f, 0.15f));

        // 暖炉 (1, 7-8) — 赤みのある茶色
        CreateFurnitureOverlay(1f, 7.5f, DISPLAY_TILE * 0.9f, DISPLAY_TILE * 1.8f,
            new Color(0.45f, 0.18f, 0.1f));
        // 暖炉の炎マーク
        CreateFurnitureLabel(1f, 7.5f, "\U0001F525", 22, new Color(1f, 0.5f, 0.2f));

        // 出口マーク (3,1)(4,1) — ▽矢印
        CreateFurnitureLabel(3.5f, 1f, "▽ 出口", 16, new Color(0.8f, 0.9f, 1f));
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
        // 棚 (6, 8)
        walkable[6, 8] = false;
        // 武器ラック (1, 7)(1, 8)
        walkable[1, 7] = false;
        walkable[1, 8] = false;
        // 樽 (6, 6)
        walkable[6, 6] = false;
    }

    void GenerateJukuMapData()
    {
        mapData = new int[mapWidth, mapHeight];
        walkable = new bool[mapWidth, mapHeight];

        // 全て白壁で埋める
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                mapData[x, y] = TILE_JUKU_WALL;
                walkable[x, y] = false;
            }
        }

        // 明るいベージュの床エリア (x=1〜6, y=2〜8)
        for (int x = 1; x <= 6; x++)
        {
            for (int y = 2; y <= 8; y++)
            {
                mapData[x, y] = TILE_JUKU_FLOOR;
                walkable[x, y] = true;
            }
        }

        // 出口 (3,1)(4,1)
        mapData[3, 1] = TILE_FATHER_EXIT;
        walkable[3, 1] = false;
        mapData[4, 1] = TILE_FATHER_EXIT;
        walkable[4, 1] = false;

        // 机 (2,4)(3,4)(4,4)(5,4) — 生徒用
        walkable[2, 4] = false;
        walkable[3, 4] = false;
        walkable[4, 4] = false;
        walkable[5, 4] = false;

        // 黒板 (3,8)(4,8)
        walkable[3, 8] = false;
        walkable[4, 8] = false;
        // 本棚 (1, 7)(1, 8)
        walkable[1, 7] = false;
        walkable[1, 8] = false;
        // 教卓 (3, 6) — (4, 6)は先生NPCがいるので歩行可能のまま
        walkable[3, 6] = false;
    }

    void CreateJukuFurniture()
    {
        if (tilesContainer == null) return;

        // 机 (2〜5, 4) — 横4マス分（生徒用）
        CreateFurnitureOverlay(3.5f, 4f, DISPLAY_TILE * 4f, DISPLAY_TILE * 0.7f,
            new Color(0.6f, 0.45f, 0.3f));

        // 黒板 (3〜4, 8) — 横2マス分（濃い緑）
        CreateFurnitureOverlay(3.5f, 8f, DISPLAY_TILE * 2.2f, DISPLAY_TILE * 0.9f,
            new Color(0.1f, 0.3f, 0.12f));
        // 黒板のチョーク文字
        CreateFurnitureLabel(3.5f, 8f, "ABC", 14, new Color(0.9f, 0.9f, 0.85f));

        // 教卓 (3, 6) — 黒板の手前（(4,6)は先生NPCの位置）
        CreateFurnitureOverlay(3f, 6f, DISPLAY_TILE * 1f, DISPLAY_TILE * 0.7f,
            new Color(0.45f, 0.30f, 0.18f));

        // 本棚 (1, 7-8) — 壁際に茶色の棚
        CreateFurnitureOverlay(1f, 7.5f, DISPLAY_TILE * 0.8f, DISPLAY_TILE * 1.8f,
            new Color(0.5f, 0.32f, 0.18f));
        // 本棚のアイコン
        CreateFurnitureLabel(1f, 7.5f, "\U0001F4DA", 18, new Color(0.3f, 0.5f, 0.3f));

        // 時計 — 壁上部
        CreateFurnitureLabel(6f, 8f, "\U0001F552", 22, new Color(0.3f, 0.3f, 0.3f));

        // 出口マーク (3.5, 1) — ▽矢印
        CreateFurnitureLabel(3.5f, 1f, "▽ 出口", 18, new Color(0.3f, 0.6f, 0.3f));
    }

    void CreateWeaponShopFurniture()
    {
        if (tilesContainer == null) return;

        // カウンター (2〜4, 5) — 横3マス分
        CreateFurnitureOverlay(3f, 5f, DISPLAY_TILE * 3f, DISPLAY_TILE * 0.8f,
            new Color(0.35f, 0.2f, 0.45f));

        // 棚 (6, 8)
        CreateFurnitureOverlay(6f, 8f, DISPLAY_TILE * 0.8f, DISPLAY_TILE * 0.9f,
            new Color(0.3f, 0.15f, 0.4f));

        // 武器ラック (1, 7-8) — 壁際に縦長の灰色ラック
        CreateFurnitureOverlay(1f, 7.5f, DISPLAY_TILE * 0.8f, DISPLAY_TILE * 1.8f,
            new Color(0.4f, 0.35f, 0.45f));
        // 武器ラックのアイコン
        CreateFurnitureLabel(1f, 7.5f, "\u2694", 22, new Color(0.8f, 0.7f, 1f));

        // 樽 (6, 6) — 茶色の丸っぽいオブジェクト
        CreateFurnitureOverlay(6f, 6f, DISPLAY_TILE * 0.7f, DISPLAY_TILE * 0.7f,
            new Color(0.4f, 0.25f, 0.12f));

        // ランタン — 壁の上部に配置
        CreateFurnitureLabel(6f, 8f, "\U0001F3EE", 20, new Color(1f, 0.8f, 0.3f));

        // 出口マーク (3.5, 1) — ▽矢印
        CreateFurnitureLabel(3.5f, 1f, "▽ 出口", 18, new Color(0.8f, 0.7f, 1f));
    }

    void GenerateImpTownMapData()
    {
        // 24x40 の広大な小悪魔の街
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

        // === 外周（木の壁） ===
        for (int x = 0; x < mapWidth; x++)
        {
            mapData[x, 0] = TILE_TREE;
            walkable[x, 0] = false;
            mapData[x, mapHeight - 1] = TILE_TREE;
            walkable[x, mapHeight - 1] = false;
        }
        for (int y = 0; y < mapHeight; y++)
        {
            mapData[0, y] = TILE_TREE;
            walkable[0, y] = false;
            mapData[mapWidth - 1, y] = TILE_TREE;
            walkable[mapWidth - 1, y] = false;
        }

        // === エリア出口（南端） ===
        mapData[11, 0] = TILE_AREA_EXIT;
        walkable[11, 0] = true;
        mapData[12, 0] = TILE_AREA_EXIT;
        walkable[12, 0] = true;

        // === メイン街道（曲がりくねった長い道） ===
        // 入口から北へ (y=1〜5, x=11〜12)
        for (int y = 1; y <= 5; y++)
        {
            mapData[11, y] = TILE_PATH;
            mapData[12, y] = TILE_PATH;
        }
        // 右に曲がる (y=5, x=12〜17)
        for (int x = 12; x <= 17; x++)
            mapData[x, 5] = TILE_PATH;
        // 北へ (y=5〜12, x=17)
        for (int y = 5; y <= 12; y++)
            mapData[17, y] = TILE_PATH;
        // 左へ (y=12, x=10〜17)
        for (int x = 10; x <= 17; x++)
            mapData[x, 12] = TILE_PATH;
        // 北へ (y=12〜19, x=10)
        for (int y = 12; y <= 19; y++)
            mapData[10, y] = TILE_PATH;
        // 右へ (y=19, x=10〜18)
        for (int x = 10; x <= 18; x++)
            mapData[x, 19] = TILE_PATH;
        // 北へ (y=19〜26, x=18)
        for (int y = 19; y <= 26; y++)
            mapData[18, y] = TILE_PATH;
        // 左へ (y=26, x=8〜18)
        for (int x = 8; x <= 18; x++)
            mapData[x, 26] = TILE_PATH;
        // 北へ (y=26〜32, x=8)
        for (int y = 26; y <= 32; y++)
            mapData[8, y] = TILE_PATH;
        // 右へ (y=32, x=8〜14)
        for (int x = 8; x <= 14; x++)
            mapData[x, 32] = TILE_PATH;
        // 北へ (y=32〜37, x=14)
        for (int y = 32; y <= 37; y++)
            mapData[14, y] = TILE_PATH;

        // === 毒沼（水場）===
        // 西の大きな沼
        for (int x = 2; x <= 6; x++)
        {
            for (int y = 8; y <= 10; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }
        // 東の沼
        for (int x = 19; x <= 22; x++)
        {
            for (int y = 14; y <= 16; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }
        // 北東の小さな沼
        for (int x = 17; x <= 20; x++)
        {
            for (int y = 30; y <= 31; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 暗い土の地面 ===
        mapData[9, 3] = TILE_DARK_DIRT;
        mapData[10, 3] = TILE_DARK_DIRT;
        mapData[15, 8] = TILE_DARK_DIRT;
        mapData[16, 8] = TILE_DARK_DIRT;
        mapData[5, 15] = TILE_DARK_DIRT;
        mapData[6, 15] = TILE_DARK_DIRT;
        mapData[20, 22] = TILE_DARK_DIRT;
        mapData[21, 22] = TILE_DARK_DIRT;
        mapData[3, 28] = TILE_DARK_DIRT;
        mapData[4, 28] = TILE_DARK_DIRT;
        mapData[11, 35] = TILE_DARK_DIRT;
        mapData[12, 35] = TILE_DARK_DIRT;
        mapData[19, 26] = TILE_DARK_DIRT;
        mapData[6, 20] = TILE_DARK_DIRT;
        mapData[7, 20] = TILE_DARK_DIRT;

        // === 岩 ===
        int[][] rocks = {
            new[]{2, 4}, new[]{20, 7}, new[]{7, 14}, new[]{22, 20},
            new[]{3, 22}, new[]{15, 24}, new[]{21, 28}, new[]{5, 33},
            new[]{19, 35}, new[]{1, 18}, new[]{13, 16}
        };
        foreach (var r in rocks)
        {
            mapData[r[0], r[1]] = TILE_ROCK;
            walkable[r[0], r[1]] = false;
        }

        // === 花畑（エンカウント率高め） ===
        int[][] flowers = {
            new[]{4, 5}, new[]{5, 5}, new[]{4, 6}, new[]{5, 6},
            new[]{14, 9}, new[]{15, 9}, new[]{14, 10}, new[]{15, 10},
            new[]{8, 16}, new[]{9, 16}, new[]{9, 17},
            new[]{20, 20}, new[]{21, 20}, new[]{20, 21},
            new[]{3, 25}, new[]{4, 25}, new[]{3, 26},
            new[]{12, 28}, new[]{13, 28}, new[]{13, 29},
            new[]{18, 34}, new[]{19, 34}, new[]{18, 35},
            new[]{6, 36}, new[]{7, 36}
        };
        foreach (var f in flowers)
        {
            mapData[f[0], f[1]] = TILE_FLOWER;
        }

        // === 武器ショップ (2x2: x=7〜8, y=27〜28) ===
        for (int sx = 7; sx <= 8; sx++)
        {
            for (int sy = 27; sy <= 28; sy++)
            {
                mapData[sx, sy] = TILE_WEAPON_SHOP;
                walkable[sx, sy] = false;
            }
        }

        // === 109 館 (小道の北端付近) ===
        for (int x = 13; x <= 15; x++)
        {
            mapData[x, 38] = TILE_BOSS_MANSION;
            walkable[x, 38] = false;
        }

        // === 追加の木（内部に散在） ===
        int[][] trees = {
            new[]{2, 2}, new[]{8, 3}, new[]{19, 4}, new[]{22, 10},
            new[]{1, 13}, new[]{13, 7}, new[]{3, 17}, new[]{16, 17},
            new[]{22, 24}, new[]{1, 27}, new[]{5, 30}, new[]{20, 33},
            new[]{10, 36}, new[]{2, 35}, new[]{16, 29}, new[]{21, 12},
            new[]{11, 22}, new[]{4, 14}, new[]{18, 10}, new[]{9, 24},
            new[]{22, 37}, new[]{1, 38}, new[]{6, 13}
        };
        foreach (var t in trees)
        {
            mapData[t[0], t[1]] = TILE_TREE;
            walkable[t[0], t[1]] = false;
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

        if (areaForTile == 1 && tileType == TILE_GRASS && poisonRoadSprite != null)
        {
            img.sprite = poisonRoadSprite;
            img.color = Color.white;
        }
        else if (tileType == TILE_GRASS)
        {
            CreateModernGrassTile(tileObj.transform, img, x, y);
        }
        else if (areaForTile == 1 && tileType == TILE_WATER && poisonLakeSprite != null)
        {
            img.sprite = poisonLakeSprite;
            img.color = Color.white;
        }
        else if (areaForTile == 1 && tileType == TILE_FENCE && poisonFenceSprite != null)
        {
            img.sprite = poisonFenceSprite;
            img.color = Color.white;
        }
        else if (tileSprites.ContainsKey(tileType) && tileSprites[tileType] != null)
        {
            img.sprite = tileSprites[tileType];
            if (areaForTile == 3 && tileType == TILE_WATER)
                img.color = new Color(0.6f, 0.3f, 0.8f);
            else if (areaForTile == 1 && tileType == TILE_WATER)
                img.color = new Color(1f, 0.4f, 0.3f);
            else
                img.color = Color.white;
        }
        else if (tileType == TILE_SHOP_FLOOR)
        {
            // 武器屋: 市松模様の石畳
            if ((x + y) % 2 == 0)
                img.color = new Color(0.30f, 0.22f, 0.35f);
            else
                img.color = new Color(0.24f, 0.16f, 0.28f);
        }
        else if (tileType == TILE_JUKU_FLOOR)
        {
            // 塾: ストライプ模様の床
            if (x % 2 == 0)
                img.color = new Color(0.85f, 0.80f, 0.70f);
            else
                img.color = new Color(0.80f, 0.75f, 0.65f);
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
            case TILE_FATHER_FLOOR: return new Color(0.45f, 0.32f, 0.2f);
            case TILE_FATHER_WALL: return new Color(0.35f, 0.25f, 0.15f);
            case TILE_FATHER_EXIT: return new Color(0.1f, 0.3f, 0.3f);
            case TILE_WEAPON_SHOP: return new Color(0.25f, 0.12f, 0.3f);
            case TILE_JUKU: return new Color(0.2f, 0.45f, 0.25f);
            case TILE_SHOP_FLOOR: return new Color(0.30f, 0.22f, 0.35f);
            case TILE_SHOP_WALL: return new Color(0.18f, 0.08f, 0.22f);
            case TILE_JUKU_FLOOR: return new Color(0.85f, 0.80f, 0.70f);
            case TILE_JUKU_WALL: return new Color(0.65f, 0.60f, 0.55f);
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
        playerImage.enabled = false; // 背景は不要、パーツで描画

        GenerateRpgCharacter(playerObj.transform);

        playerObj.transform.SetAsLastSibling();
        targetPosition = playerRect.anchoredPosition;
    }

    void RebuildPlayerSprite()
    {
        if (playerObj == null) return;
        crawlHandL = crawlHandR = null;
        crawlKneeL = crawlKneeR = null;
        // 子オブジェクト（キャラパーツ）を全削除
        for (int i = playerObj.transform.childCount - 1; i >= 0; i--)
            Destroy(playerObj.transform.GetChild(i).gameObject);
        GenerateRpgCharacter(playerObj.transform);
    }

    // SDチビキャラ生成（4方向対応）
    void GenerateRpgCharacter(Transform parent)
    {
        string gender = "男の子";
        bool godBaby = false;
        int intelligence = 60;

        if (DataCarrier.Instance != null)
        {
            gender = DataCarrier.Instance.babyGender;
            godBaby = DataCarrier.Instance.isGodBaby;
            intelligence = DataCarrier.Instance.babyIntelligence;
        }

        bool isFemale = gender == "女の子";
        int dir = playerDirection; // 0=下, 1=上, 2=左, 3=右
        float s = 1.0f;

        // 色の決定
        Color skin = new Color(0.98f, 0.89f, 0.82f);
        Color[] hairTones = {
            new Color(0.08f, 0.06f, 0.05f),
            new Color(0.2f, 0.12f, 0.08f),
            new Color(0.35f, 0.22f, 0.12f),
            new Color(0.55f, 0.38f, 0.2f)
        };
        Color hair = hairTones[Mathf.Clamp(intelligence / 25, 0, 3)];
        Color clothMain = isFemale
            ? new Color(0.95f, 0.45f, 0.6f)
            : new Color(0.3f, 0.5f, 0.85f);
        Color clothDark = isFemale
            ? new Color(0.8f, 0.3f, 0.45f)
            : new Color(0.2f, 0.35f, 0.7f);
        float xFlip = (dir == 3) ? -1f : 1f;

        // STAR BABYオーラ（低重心に合わせて調整）
        if (godBaby)
        {
            var aura = FacePart("Aura", parent, new Vector2(0, -4 * s), new Vector2(76 * s, 56 * s));
            aura.AddComponent<Image>().color = new Color(1f, 0.85f, 0.2f, 0.18f);
        }

        // === 影（四つん這いなので横に広い） ===
        FacePart("Shadow", parent, new Vector2(0, -28 * s), new Vector2(48 * s, 10 * s))
            .AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);

        if (dir == 0) // ===== 正面（手前にはいはい）=====
        {
            // 膝（地面）
            var kneeLObj0 = FacePart("KneeL", parent, new Vector2(-8 * s, -22 * s), new Vector2(10 * s, 8 * s));
            kneeLObj0.AddComponent<Image>().color = clothDark;
            crawlKneeL = kneeLObj0.GetComponent<RectTransform>();
            crawlKneeLBase = crawlKneeL.anchoredPosition;
            var kneeRObj0 = FacePart("KneeR", parent, new Vector2(8 * s, -22 * s), new Vector2(10 * s, 8 * s));
            kneeRObj0.AddComponent<Image>().color = clothDark;
            crawlKneeR = kneeRObj0.GetComponent<RectTransform>();
            crawlKneeRBase = crawlKneeR.anchoredPosition;

            // 武器（背中に横に寝かせる：胴体の後ろ）
            var sword = FacePart("Sword", parent, new Vector2(0, 2 * s), new Vector2(22 * s, 4 * s));
            sword.AddComponent<Image>().color = new Color(0.75f, 0.75f, 0.8f);
            FacePart("SwordHilt", parent, new Vector2(14 * s, 2 * s), new Vector2(4 * s, 8 * s))
                .AddComponent<Image>().color = new Color(0.55f, 0.35f, 0.15f);

            // 胴体（横長）
            FacePart("Body", parent, new Vector2(0, -4 * s), new Vector2(30 * s, 18 * s))
                .AddComponent<Image>().color = clothMain;
            if (godBaby)
            {
                FacePart("BodyTrim", parent, new Vector2(0, -4 * s), new Vector2(34 * s, 20 * s))
                    .AddComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.3f);
                FacePart("BodyOver", parent, new Vector2(0, -4 * s), new Vector2(30 * s, 18 * s))
                    .AddComponent<Image>().color = clothMain;
            }

            // 腕（体の横から地面へ）
            FacePart("ArmL", parent, new Vector2(-18 * s, -8 * s), new Vector2(8 * s, 14 * s))
                .AddComponent<Image>().color = clothDark;
            FacePart("ArmR", parent, new Vector2(18 * s, -8 * s), new Vector2(8 * s, 14 * s))
                .AddComponent<Image>().color = clothDark;
            // 手（地面に）
            var handLObj0 = FacePart("HandL", parent, new Vector2(-18 * s, -18 * s), new Vector2(8 * s, 8 * s));
            handLObj0.AddComponent<Image>().color = skin;
            crawlHandL = handLObj0.GetComponent<RectTransform>();
            crawlHandLBase = crawlHandL.anchoredPosition;
            var handRObj0 = FacePart("HandR", parent, new Vector2(18 * s, -18 * s), new Vector2(8 * s, 8 * s));
            handRObj0.AddComponent<Image>().color = skin;
            crawlHandR = handRObj0.GetComponent<RectTransform>();
            crawlHandRBase = crawlHandR.anchoredPosition;

            // 頭（大きめ、持ち上げてる）
            FacePart("Head", parent, new Vector2(0, 12 * s), new Vector2(36 * s, 32 * s))
                .AddComponent<Image>().color = skin;
            // 耳
            FacePart("EarL", parent, new Vector2(-19 * s, 10 * s), new Vector2(6 * s, 10 * s))
                .AddComponent<Image>().color = skin;
            FacePart("EarR", parent, new Vector2(19 * s, 10 * s), new Vector2(6 * s, 10 * s))
                .AddComponent<Image>().color = skin;
            // 髪
            FacePart("HairTop", parent, new Vector2(0, 26 * s), new Vector2(40 * s, 14 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairBangs", parent, new Vector2(0, 20 * s), new Vector2(36 * s, 8 * s))
                .AddComponent<Image>().color = new Color(hair.r * 0.85f, hair.g * 0.85f, hair.b * 0.85f);
            FacePart("HairSideL", parent, new Vector2(-17 * s, 14 * s), new Vector2(6 * s, 12 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairSideR", parent, new Vector2(17 * s, 14 * s), new Vector2(6 * s, 12 * s))
                .AddComponent<Image>().color = hair;
            // 目
            DrawSdEye(parent, -9 * s, 12 * s, s, isFemale);
            DrawSdEye(parent, 9 * s, 12 * s, s, isFemale);
            // 口
            FacePart("Mouth", parent, new Vector2(0, 2 * s), new Vector2(8 * s, 3 * s))
                .AddComponent<Image>().color = new Color(0.82f, 0.55f, 0.55f);
            // ほっぺ
            float cheekAlpha = isFemale ? 0.4f : 0.15f;
            Color cheekC = isFemale
                ? new Color(1f, 0.5f, 0.55f, cheekAlpha)
                : new Color(1f, 0.7f, 0.7f, cheekAlpha);
            FacePart("CheekL", parent, new Vector2(-12 * s, 6 * s), new Vector2(8 * s, 6 * s))
                .AddComponent<Image>().color = cheekC;
            FacePart("CheekR", parent, new Vector2(12 * s, 6 * s), new Vector2(8 * s, 6 * s))
                .AddComponent<Image>().color = cheekC;
        }
        else if (dir == 1) // ===== 背面（奥にはいはい）=====
        {
            // 膝（カメラ側＝手前に見える）
            var kneeLObj1 = FacePart("KneeL", parent, new Vector2(-8 * s, -22 * s), new Vector2(10 * s, 8 * s));
            kneeLObj1.AddComponent<Image>().color = clothDark;
            crawlKneeL = kneeLObj1.GetComponent<RectTransform>();
            crawlKneeLBase = crawlKneeL.anchoredPosition;
            var kneeRObj1 = FacePart("KneeR", parent, new Vector2(8 * s, -22 * s), new Vector2(10 * s, 8 * s));
            kneeRObj1.AddComponent<Image>().color = clothDark;
            crawlKneeR = kneeRObj1.GetComponent<RectTransform>();
            crawlKneeRBase = crawlKneeR.anchoredPosition;

            // 手・腕（奥側、胴体に隠れ気味）
            var handLObj1 = FacePart("HandL", parent, new Vector2(-18 * s, -18 * s), new Vector2(8 * s, 8 * s));
            handLObj1.AddComponent<Image>().color = skin;
            crawlHandL = handLObj1.GetComponent<RectTransform>();
            crawlHandLBase = crawlHandL.anchoredPosition;
            var handRObj1 = FacePart("HandR", parent, new Vector2(18 * s, -18 * s), new Vector2(8 * s, 8 * s));
            handRObj1.AddComponent<Image>().color = skin;
            crawlHandR = handRObj1.GetComponent<RectTransform>();
            crawlHandRBase = crawlHandR.anchoredPosition;
            FacePart("ArmL", parent, new Vector2(-16 * s, -10 * s), new Vector2(8 * s, 12 * s))
                .AddComponent<Image>().color = clothDark;
            FacePart("ArmR", parent, new Vector2(16 * s, -10 * s), new Vector2(8 * s, 12 * s))
                .AddComponent<Image>().color = clothDark;

            // 頭（後頭部、胴体より下に隠れ気味）
            FacePart("Head", parent, new Vector2(0, 6 * s), new Vector2(32 * s, 26 * s))
                .AddComponent<Image>().color = skin;
            // 後ろ髪
            FacePart("HairBack", parent, new Vector2(0, 12 * s), new Vector2(36 * s, 24 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairBackHL", parent, new Vector2(0, 18 * s), new Vector2(30 * s, 10 * s))
                .AddComponent<Image>().color = new Color(hair.r * 1.15f, hair.g * 1.15f, hair.b * 1.15f);

            // 胴体（横長、頭に被さる）
            FacePart("Body", parent, new Vector2(0, -4 * s), new Vector2(30 * s, 18 * s))
                .AddComponent<Image>().color = clothMain;
            if (godBaby)
            {
                FacePart("BodyTrim", parent, new Vector2(0, -4 * s), new Vector2(34 * s, 20 * s))
                    .AddComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.3f);
                FacePart("BodyOver", parent, new Vector2(0, -4 * s), new Vector2(30 * s, 18 * s))
                    .AddComponent<Image>().color = clothMain;
                // ケープ（背面で見える）
                FacePart("Cape", parent, new Vector2(0, -2 * s), new Vector2(34 * s, 22 * s))
                    .AddComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.5f);
            }

            // 武器（背中の上に見える）
            var sword = FacePart("Sword", parent, new Vector2(0, 0), new Vector2(22 * s, 4 * s));
            sword.AddComponent<Image>().color = new Color(0.75f, 0.75f, 0.8f);
            FacePart("SwordHilt", parent, new Vector2(14 * s, 0), new Vector2(4 * s, 8 * s))
                .AddComponent<Image>().color = new Color(0.55f, 0.35f, 0.15f);

            // おしり（おむつ風、目立つ位置）
            FacePart("Butt", parent, new Vector2(0, 8 * s), new Vector2(22 * s, 14 * s))
                .AddComponent<Image>().color = new Color(
                    Mathf.Min(clothMain.r * 1.15f, 1f),
                    Mathf.Min(clothMain.g * 1.15f, 1f),
                    Mathf.Min(clothMain.b * 1.15f, 1f));
        }
        else // ===== 左右（横向き四つん這い）=====
        {
            // 膝（後方＝進行方向の逆側）
            var kneeObjS = FacePart("Knee", parent, new Vector2(12 * xFlip * s, -22 * s), new Vector2(10 * s, 8 * s));
            kneeObjS.AddComponent<Image>().color = clothDark;
            crawlKneeL = kneeObjS.GetComponent<RectTransform>();
            crawlKneeLBase = crawlKneeL.anchoredPosition;
            crawlKneeR = null;

            // 胴体（横長）
            FacePart("Body", parent, new Vector2(0, -4 * s), new Vector2(34 * s, 16 * s))
                .AddComponent<Image>().color = clothMain;
            if (godBaby)
            {
                FacePart("BodyTrim", parent, new Vector2(0, -4 * s), new Vector2(38 * s, 18 * s))
                    .AddComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.3f);
                FacePart("BodyOver", parent, new Vector2(0, -4 * s), new Vector2(34 * s, 16 * s))
                    .AddComponent<Image>().color = clothMain;
                // ケープ（後方）
                FacePart("Cape", parent, new Vector2(4 * xFlip * s, 0), new Vector2(26 * s, 18 * s))
                    .AddComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.5f);
            }

            // 腕・手（進行方向側、地面に）
            FacePart("Arm", parent, new Vector2(-14 * xFlip * s, -10 * s), new Vector2(8 * s, 12 * s))
                .AddComponent<Image>().color = clothDark;
            var handObjS = FacePart("Hand", parent, new Vector2(-14 * xFlip * s, -20 * s), new Vector2(8 * s, 8 * s));
            handObjS.AddComponent<Image>().color = skin;
            crawlHandL = handObjS.GetComponent<RectTransform>();
            crawlHandLBase = crawlHandL.anchoredPosition;
            crawlHandR = null;

            // 武器（背中に斜め）
            var sword = FacePart("Sword", parent, new Vector2(2 * xFlip * s, 2 * s), new Vector2(22 * s, 4 * s));
            sword.transform.localRotation = Quaternion.Euler(0, 0, -20 * xFlip);
            sword.AddComponent<Image>().color = new Color(0.75f, 0.75f, 0.8f);
            FacePart("SwordHilt", parent, new Vector2(12 * xFlip * s, 0), new Vector2(4 * s, 8 * s))
                .AddComponent<Image>().color = new Color(0.55f, 0.35f, 0.15f);

            // 頭（進行方向側、低め）
            FacePart("Head", parent, new Vector2(-12 * xFlip * s, 8 * s), new Vector2(32 * s, 28 * s))
                .AddComponent<Image>().color = skin;
            // 耳（頭の進行方向側）
            FacePart("Ear", parent, new Vector2(-26 * xFlip * s, 6 * s), new Vector2(6 * s, 10 * s))
                .AddComponent<Image>().color = skin;
            // 髪
            FacePart("HairTop", parent, new Vector2(-14 * xFlip * s, 20 * s), new Vector2(36 * s, 14 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairFront", parent, new Vector2(-22 * xFlip * s, 14 * s), new Vector2(12 * s, 12 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairBack", parent, new Vector2(-4 * xFlip * s, 10 * s), new Vector2(12 * s, 16 * s))
                .AddComponent<Image>().color = hair;
            // 目（頭のカメラ側）
            DrawSdEye(parent, -8 * xFlip * s, 8 * s, s, isFemale);
            // ほっぺ
            if (isFemale)
            {
                FacePart("Cheek", parent, new Vector2(-4 * xFlip * s, 2 * s), new Vector2(8 * s, 6 * s))
                    .AddComponent<Image>().color = new Color(1f, 0.5f, 0.55f, 0.35f);
            }
        }

        // === STARオーラ（半透明オーバーレイ、最前面、低重心対応） ===
        if (godBaby)
        {
            var glow = FacePart("GodGlow", parent, new Vector2(0, -4 * s), new Vector2(56 * s, 50 * s));
            glow.AddComponent<Image>().color = new Color(1f, 0.9f, 0.4f, 0.1f);
        }
    }

    void DrawSdEye(Transform parent, float x, float y, float s, bool isFemale)
    {
        // 白目（SD風で大きめ）
        float ew = 10 * s;
        float eh = 9 * s;
        FacePart("EyeW", parent, new Vector2(x, y), new Vector2(ew, eh))
            .AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.97f);
        // 虹彩
        FacePart("Iris", parent, new Vector2(x, y - 1 * s), new Vector2(ew * 0.7f, eh * 0.75f))
            .AddComponent<Image>().color = new Color(0.18f, 0.12f, 0.08f);
        // 瞳孔
        FacePart("Pupil", parent, new Vector2(x, y - 1 * s), new Vector2(ew * 0.3f, eh * 0.3f))
            .AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.02f);
        // ハイライト
        FacePart("HL", parent, new Vector2(x - 2 * s, y + 1 * s), new Vector2(3 * s, 3 * s))
            .AddComponent<Image>().color = Color.white;
        // まつげ（女の子）
        if (isFemale)
        {
            FacePart("Lash", parent, new Vector2(x, y + eh * 0.45f), new Vector2(ew * 1.1f, 2 * s))
                .AddComponent<Image>().color = new Color(0.1f, 0.08f, 0.06f);
        }
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
        overlayRoot.Add(bar);

        statusLabel = UIHelper.CreateLabel("", "map-status-text");
        statusLabel.enableRichText = true;
        bar.Add(statusLabel);

        UpdateStatusText();
    }

    void CreateMenuButton()
    {
        if (overlayRoot == null) return;

        var btn = new UIE.Button();
        btn.AddToClassList("map-menu-btn");
        btn.focusable = false;

        // menu.png を背景に表示
        var menuSpr = Resources.Load<Sprite>("UI/menu");
        if (menuSpr != null)
            btn.style.backgroundImage = new UIE.StyleBackground(menuSpr);
        btn.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;

        // 「メニュー」ラベル
        var menuLabel = UIHelper.CreateLabel("メニュー", "map-menu-label");
        UIHelper.ApplyFontBold(menuLabel);
        btn.Add(menuLabel);

        btn.clicked += ToggleMenu;
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

        if (!walkable[newX, newY])
            return;

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

        // DataCarrierに位置保存
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.mapPlayerX = playerTileX;
            DataCarrier.Instance.mapPlayerY = playerTileY;
        }
    }

    void UpdateMovement()
    {
        if (!isMoving || playerRect == null) return;

        playerRect.anchoredPosition = Vector2.MoveTowards(
            playerRect.anchoredPosition,
            targetPosition,
            moveSpeed * DISPLAY_TILE * Time.deltaTime
        );

        // はいはいアニメーション（手足を交互に動かす：クロスクロール）
        if (crawlHandL != null)
        {
            crawlAnimTime += Time.deltaTime * 10f;
            float offset = Mathf.Sin(crawlAnimTime) * 6f;
            // 左手↑ ↔ 右膝↑（交差パターン）
            crawlHandL.anchoredPosition = crawlHandLBase + new Vector2(0, offset);
            if (crawlHandR != null)
                crawlHandR.anchoredPosition = crawlHandRBase + new Vector2(0, -offset);
            if (crawlKneeL != null)
                crawlKneeL.anchoredPosition = crawlKneeLBase + new Vector2(0, -offset);
            if (crawlKneeR != null)
                crawlKneeR.anchoredPosition = crawlKneeRBase + new Vector2(0, offset);
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
            // はいはいアニメーションリセット
            crawlAnimTime = 0f;
            if (crawlHandL != null) crawlHandL.anchoredPosition = crawlHandLBase;
            if (crawlHandR != null) crawlHandR.anchoredPosition = crawlHandRBase;
            if (crawlKneeL != null) crawlKneeL.anchoredPosition = crawlKneeLBase;
            if (crawlKneeR != null) crawlKneeR.anchoredPosition = crawlKneeRBase;

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

            // 商人NPC判定
            CheckMerchantNPC();

            // 塾の先生NPC判定
            CheckJukuTeacher();
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
            DataCarrier.Instance.mapPlayerX = 4;
            DataCarrier.Instance.mapPlayerY = 2;
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
            DataCarrier.Instance.mapPlayerX = 4;
            DataCarrier.Instance.mapPlayerY = 2;
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
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        // 金色の楕円（たまご本体）
        var eggBody = FacePart("EggBody", goldenEggObj.transform, new Vector2(0, -3), new Vector2(52, 66));
        eggBody.AddComponent<Image>().color = new Color(1f, 0.84f, 0f);

        // 上部ハイライト
        var highlight = FacePart("Highlight", goldenEggObj.transform, new Vector2(-7, 10), new Vector2(18, 24));
        highlight.AddComponent<Image>().color = new Color(1f, 1f, 0.7f, 0.7f);

        // 輝きエフェクト（小さい星）
        var sparkle = FacePart("Sparkle", goldenEggObj.transform, new Vector2(14, 21), new Vector2(10, 10));
        sparkle.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.8f);

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

        Destroy(goldenEggObj);
        goldenEggObj = null;

        ShowMessage(Localization.Get("map_golden_egg"));
        StartCoroutine(ShowGoldenEggHint());
    }

    IEnumerator ShowGoldenEggHint()
    {
        yield return new WaitForSeconds(1.5f);

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

        var dialogBox = new UIE.VisualElement();
        dialogBox.AddToClassList("milk-dialog-box");
        overlay.Add(dialogBox);

        var textLabel = UIHelper.CreateLabel(Localization.Get("map_golden_egg_hint"), "milk-dialog-text");
        dialogBox.Add(textLabel);

        var tapHint = UIHelper.CreateLabel("▼ タップで閉じる", "milk-dialog-hint");
        dialogBox.Add(tapHint);

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
            yield return null;
        }
        overlay.RemoveFromHierarchy();
        menuOpen = false;
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
            messageList.Add(Localization.Get("map_mother_goodbye"));
        }
        else
        {
            messageList.Add(Localization.Get("map_mother_msg3"));
        }

        string[] messages = messageList.ToArray();

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

        // アイテムをくれたら装備チュートリアル → 母親は家に帰る
        if (giveItem)
        {
            yield return StartCoroutine(ShowEquipmentTutorial(itemName));

            if (motherNpcObj != null)
            {
                Destroy(motherNpcObj);
                motherNpcObj = null;
            }
        }
    }

    IEnumerator ShowEquipmentTutorial(string itemName)
    {
        menuOpen = true;
        SetTouchControlsVisible(false);

        // チュートリアルオーバーレイ
        var tutOverlay = UIHelper.CreateOverlay();
        tutOverlay.style.backgroundColor = new Color(0, 0, 0, 0);
        tutOverlay.style.justifyContent = UIE.Justify.Center;
        tutOverlay.style.alignItems = UIE.Align.Center;

        // フェードイン
        float fadeDur = 0.3f;
        float elapsed = 0f;
        while (elapsed < fadeDur)
        {
            elapsed += Time.deltaTime;
            tutOverlay.style.backgroundColor = new Color(0, 0, 0, Mathf.Lerp(0, 0.6f, elapsed / fadeDur));
            yield return null;
        }
        tutOverlay.style.backgroundColor = new Color(0, 0, 0, 0.6f);

        // チュートリアルカード
        var card = new UIE.VisualElement();
        card.style.width = 900;
        card.style.borderTopLeftRadius = 48;
        card.style.borderTopRightRadius = 48;
        card.style.borderBottomLeftRadius = 48;
        card.style.borderBottomRightRadius = 48;
        card.style.backgroundColor = new Color(1, 1, 1, 1);
        card.style.paddingTop = 40;
        card.style.paddingBottom = 36;
        card.style.paddingLeft = 40;
        card.style.paddingRight = 40;
        card.style.alignItems = UIE.Align.Center;
        card.style.opacity = 0;
        tutOverlay.Add(card);

        // タイトル
        var titleLabel = UIHelper.CreateLabel("そうびを てにいれた！");
        UIHelper.ApplyFontBold(titleLabel);
        titleLabel.style.fontSize = 40;
        titleLabel.style.color = new Color(0.051f, 0.051f, 0.078f);
        titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        titleLabel.style.marginBottom = 24;
        card.Add(titleLabel);

        // 説明テキスト
        var descLabel = UIHelper.CreateLabel(
            "メニューの「そうび」から\nそうびの つけはずし ができるよ！\n\nそうびすると おあそびで つよくなるよ！");
        UIHelper.ApplyFont(descLabel);
        descLabel.style.fontSize = 30;
        descLabel.style.color = new Color(0.051f, 0.051f, 0.078f, 0.7f);
        descLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        descLabel.style.whiteSpace = UIE.WhiteSpace.Normal;
        descLabel.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        descLabel.style.marginBottom = 32;
        card.Add(descLabel);

        // 「そうびを みる」ボタン
        var openBtn = UIHelper.CreatePillButton("そうびを みる");
        openBtn.style.width = 500;
        openBtn.style.height = 100;
        openBtn.style.borderTopLeftRadius = 50;
        openBtn.style.borderTopRightRadius = 50;
        openBtn.style.borderBottomLeftRadius = 50;
        openBtn.style.borderBottomRightRadius = 50;
        openBtn.style.backgroundColor = new Color(1f, 0.718f, 0.773f); // #FFB7C5
        UIHelper.ApplyFontBold(openBtn);
        openBtn.style.fontSize = 34;
        openBtn.style.color = Color.white;
        card.Add(openBtn);

        // カードフェードイン
        elapsed = 0f;
        float cardFade = 0.3f;
        while (elapsed < cardFade)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / cardFade);
            card.style.opacity = t;
            card.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(0, 30 * (1f - t)));
            yield return null;
        }
        card.style.opacity = 1;
        card.style.translate = new UIE.StyleTranslate(new UIE.Translate(0, 0));

        // ボタンタップ待ち
        bool btnTapped = false;
        openBtn.clicked += () => btnTapped = true;
        while (!btnTapped) yield return null;

        // チュートリアルオーバーレイ除去
        tutOverlay.RemoveFromHierarchy();
        menuOpen = false;
        SetTouchControlsVisible(true);

        // 装備パネルを自動で開く
        OpenEquipmentPanel();
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

        DrawJukuTeacherNPC(jukuTeacherObj.transform, 0.8f);
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
            OnElderTapped();
    }

    void OnElderTapped()
    {
        if (elderDialogueActive) return;
        if (menuOpen) return;

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
                string ek = "shop_effect_" + si.key;
                string et = Localization.Get(ek);
                if (et != ek) effectText = et;
                return;
            }
        }
        foreach (var si in villageShopItems)
        {
            if (si.equipName == equipName)
            {
                string ek = "shop_effect_" + si.key;
                string et = Localization.Get(ek);
                if (et != ek) effectText = et;
                return;
            }
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
        public string key;     // localization key suffix
        public string equipName; // equipment name for DataCarrier
        public int price;
        public bool requiresS;  // Sランク限定
    }

    static readonly ShopItem[] shopItems = new ShopItem[]
    {
        new ShopItem { key = "garagara", equipName = "ガラガラソード", price = 30 },
        new ShopItem { key = "yodare", equipName = "よだれかけシールド", price = 30 },
        new ShopItem { key = "oshaburi", equipName = "おしゃぶりチャーム", price = 50 },
        new ShopItem { key = "omutsu", equipName = "魔法のおむつ", price = 60 },
        new ShopItem { key = "honyubin", equipName = "黄金のほ乳瓶", price = 80 },
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
        bool isSRank = DataCarrier.Instance != null
            && BabySynthesizer.DetermineRank(DataCarrier.Instance.babyFortune) == BabySynthesizer.BabyRank.S;

        foreach (var item in currentShopItems)
        {
            var row = new UIE.VisualElement();
            row.AddToClassList("map-shop-row");

            var infoCol = new UIE.VisualElement();
            infoCol.style.flexDirection = UIE.FlexDirection.Column;
            infoCol.style.flexGrow = 1;

            var nameLabel = UIHelper.CreateLabel(Localization.Get("shop_item_" + item.key), "map-shop-item-name");
            infoCol.Add(nameLabel);

            var effectLabel = UIHelper.CreateLabel(Localization.Get("shop_effect_" + item.key), "map-shop-item-effect");
            infoCol.Add(effectLabel);

            // Sランク限定表示
            if (item.requiresS)
            {
                var sLabel = UIHelper.CreateLabel(Localization.Get("shop_s_rank_only"), "map-shop-item-effect");
                sLabel.style.color = new Color(0.9f, 0.6f, 0.1f);
                infoCol.Add(sLabel);
            }

            row.Add(infoCol);

            bool owned = DataCarrier.Instance != null && DataCarrier.Instance.HasEquipment(item.equipName);

            if (owned)
            {
                var purchasedBtn = UIHelper.CreatePillButton(Localization.Get("shop_purchased"), "map-shop-buy-btn");
                purchasedBtn.style.opacity = 0.4f;
                purchasedBtn.clicked += () =>
                {
                    CloseWeaponShop();
                    ShowMerchantMessage(Localization.Get("shop_merchant_already_owned"));
                };
                row.Add(purchasedBtn);
            }
            else if (item.requiresS && !isSRank)
            {
                // Sランク限定で、Sランクでない場合は購入不可
                var lockedBtn = UIHelper.CreatePillButton(Localization.Get("shop_s_rank_only"), "map-shop-buy-btn");
                lockedBtn.style.opacity = 0.4f;
                lockedBtn.clicked += () =>
                {
                    CloseWeaponShop();
                    ShowMerchantMessage(Localization.Get("shop_merchant_need_s_rank"));
                };
                row.Add(lockedBtn);
            }
            else
            {
                var buyBtn = UIHelper.CreatePillButton(Localization.Get("shop_buy", item.price), "map-shop-buy-btn");
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
                    // スロットに空きがあれば自動装備
                    DataCarrier.Instance.EquipItem(capturedEquipName);
                    DataCarrier.Instance.SaveData();
                    UpdateStatusText();

                    // ショップUIを再構築
                    CloseWeaponShop();
                    ShowMessage(Localization.Get("shop_bought", Localization.Get("shop_item_" + capturedKey)));
                    // 少し遅れてショップを再度開く
                    StartCoroutine(ReopenShopAfterDelay());
                };

                row.Add(buyBtn);
            }

            shopOverlayEl.Add(row);
        }

        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button-small");
        closeBtn.style.marginTop = 20;
        closeBtn.clicked += () => CloseWeaponShop();
        shopOverlayEl.Add(closeBtn);

        overlay.Add(shopOverlayEl);
        overlayRoot.Add(overlay);
    }

    IEnumerator ReopenShopAfterDelay()
    {
        yield return new WaitForSeconds(0.8f);
        OpenWeaponShop();
    }

    void CloseWeaponShop()
    {
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
