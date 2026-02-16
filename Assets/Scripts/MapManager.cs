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
    const int MAP_WIDTH = 12;
    const int MAP_HEIGHT = 20;

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

    // UI要素
    GameObject mapPanel;
    RectTransform tilesContainerRect;
    GameObject tilesContainer;
    Texture2D tilesetTexture;
    Dictionary<int, Sprite> tileSprites = new Dictionary<int, Sprite>();

    // Pixel Crawler テクスチャ（area==0 村マップ用）
    Texture2D pcTreesTex, pcVegetationTex, pcRocksTex, pcRoofsTex, pcWallsTex;

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

    // エンカウント
    float encounterCooldown = 0f;
    int stepCount = 0;

    // 金のたまご
    GameObject goldenEggObj;
    const int GOLDEN_EGG_X = 9;
    const int GOLDEN_EGG_Y = 14;
    const int GOLDEN_EGG_X_AREA3 = 2;
    const int GOLDEN_EGG_Y_AREA3 = 16;

    // ミルクポイント（回復）
    GameObject milkPointObj;
    int milkPointX = 3;
    int milkPointY = 10;
    bool milkCutinActive = false;

    // 持ち物パネル
    UIE.VisualElement inventoryOverlayEl;
    UIE.VisualElement statusDetailEl;
    UIE.VisualElement saveOverlayEl;

    // UI Toolkit overlay layer
    UIE.PanelSettings overlayPanelSettings;
    UIE.VisualElement overlayRoot;
    UIE.Label statusLabel;

    // タッチ操作
    UIE.VisualElement touchSwipeEl;
    int touchDx, touchDy;
    bool touchInteract;
    bool touchMenuPressed;
    Vector2 touchStartPos;
    bool isTouchDragging;
    const float SWIPE_THRESHOLD = 30f;

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

        // DataCarrierからマップ位置を復元
        if (DataCarrier.Instance != null)
        {
            playerTileX = DataCarrier.Instance.mapPlayerX;
            playerTileY = DataCarrier.Instance.mapPlayerY;
            // 旧マップの保存座標が新マップ外にならないようバウンドチェック
            if (playerTileX < 1 || playerTileX >= MAP_WIDTH - 1) playerTileX = 5;
            if (playerTileY < 1 || playerTileY >= MAP_HEIGHT - 1) playerTileY = 9;
        }

        LoadTileset();

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area == 4)
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

        if (area == 4)
        {
            // 109館内: ランダムエンカウントなし、ミルクポイントなし、傭兵なし
        }
        else if (area == 3)
        {
            milkPointX = 3;
            milkPointY = 8;
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
        }

        // UI Toolkit overlay layer (above Canvas, separate GameObject to avoid UIDocument conflict)
        overlayPanelSettings = UIHelper.CreatePanelSettings(10f);
        var overlayObj = new GameObject("MapOverlayUI");
        overlayObj.transform.SetParent(transform, false);
        overlayRoot = UIHelper.SetupUIDocument(overlayObj,
            new[] { "UI/CommonStyle", "UI/MapStyle" }, overlayPanelSettings);
        overlayRoot.pickingMode = UIE.PickingMode.Ignore;
        overlayRoot.focusable = false;

        CreateStatusUI();
        CreateMenuButton();

        if (SafeAreaHelper.IsTouchDevice())
            CreateTouchControls();

        // プレイヤーを最前面に
        if (playerObj != null)
            playerObj.transform.SetAsLastSibling();
    }

    void LoadTileset()
    {
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        if (area == 0)
        {
            // 村マップ: Pixel Crawlerテクスチャをロード（DQ風プロシージャル + スプライトオーバーレイ）
            LoadPixelCrawlerTextures();
        }

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

        // タイルグリッドコンテナ
        tileGridRoot = new UIE.VisualElement();
        tileGridRoot.name = "tile-grid";
        tileGridRoot.AddToClassList("tile-grid");
        root.Add(tileGridRoot);
    }

    void CreateDQTileElement(int x, int y, int tileType)
    {
        if (tileGridRoot == null) return;

        var tile = new UIE.VisualElement();
        tile.name = $"tile-{x}-{y}";
        tile.AddToClassList("dq-tile");
        tile.style.left = x * DISPLAY_TILE;
        tile.style.top = (MAP_HEIGHT - 1 - y) * DISPLAY_TILE;

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

        // Per-tile color variation (USS base をランダムでオーバーライド)
        float h = Random.Range(-0.02f, 0.02f);
        float b = Random.Range(-0.03f, 0.03f);
        tile.style.backgroundColor = new Color(0.25f + h, 0.55f + b, 0.18f + h);

        // タイル境界線
        AddDQTileBorder(tile, "dq-grass-border");

        // 40%の確率で草タフト
        if (Random.Range(0f, 1f) < 0.4f)
        {
            int count = Random.Range(1, 3);
            for (int i = 0; i < count; i++)
            {
                var tuft = new UIE.VisualElement();
                tuft.AddToClassList("grass-tuft");
                float cx = DISPLAY_TILE * 0.5f;
                float cy = DISPLAY_TILE * 0.5f;
                tuft.style.left = cx + Random.Range(-DISPLAY_TILE * 0.3f, DISPLAY_TILE * 0.3f) - 3f;
                tuft.style.top = cy + Random.Range(-DISPLAY_TILE * 0.3f, DISPLAY_TILE * 0.3f) - 5f;
                tuft.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
                tile.Add(tuft);
            }
        }
    }

    void ApplyDQPathStyle(UIE.VisualElement tile)
    {
        tile.AddToClassList("dq-path");

        float h = Random.Range(-0.02f, 0.02f);
        float b = Random.Range(-0.03f, 0.03f);
        tile.style.backgroundColor = new Color(0.72f + h, 0.58f + b, 0.38f + h);

        AddDQTileBorder(tile, "dq-path-border");

        // 小石パターン 2〜3個
        int stoneCount = Random.Range(2, 4);
        for (int i = 0; i < stoneCount; i++)
        {
            var stone = new UIE.VisualElement();
            stone.AddToClassList("path-stone");
            float cx = DISPLAY_TILE * 0.5f;
            float cy = DISPLAY_TILE * 0.5f;
            float sz = Random.Range(3f, 7f);
            stone.style.left = cx + Random.Range(-DISPLAY_TILE * 0.35f, DISPLAY_TILE * 0.35f) - sz / 2f;
            stone.style.top = cy + Random.Range(-DISPLAY_TILE * 0.35f, DISPLAY_TILE * 0.35f) - sz * 0.35f;
            stone.style.width = sz;
            stone.style.height = sz * 0.7f;
            tile.Add(stone);
        }
    }

    void ApplyDQWaterStyle(UIE.VisualElement tile)
    {
        tile.AddToClassList("dq-water");

        float h = Random.Range(-0.02f, 0.02f);
        float b = Random.Range(-0.03f, 0.03f);
        tile.style.backgroundColor = new Color(0.20f + h, 0.45f + b, 0.75f + h);

        AddDQTileBorder(tile, "dq-water-border");

        // 波紋ライン 2〜3本
        int waveCount = Random.Range(2, 4);
        for (int i = 0; i < waveCount; i++)
        {
            var wave = new UIE.VisualElement();
            wave.AddToClassList("water-wave");
            float cy = DISPLAY_TILE * 0.5f;
            float ww = Random.Range(DISPLAY_TILE * 0.3f, DISPLAY_TILE * 0.7f);
            wave.style.left = DISPLAY_TILE * 0.5f + Random.Range(-DISPLAY_TILE * 0.15f, DISPLAY_TILE * 0.15f) - ww / 2f;
            wave.style.top = cy + Random.Range(-DISPLAY_TILE * 0.35f, DISPLAY_TILE * 0.35f) - 1.5f;
            wave.style.width = ww;
            tile.Add(wave);
        }
    }

    void ApplyDQDirtStyle(UIE.VisualElement tile, bool dark)
    {
        tile.AddToClassList(dark ? "dq-dark-dirt" : "dq-dirt");

        float h = Random.Range(-0.02f, 0.02f);
        if (dark)
            tile.style.backgroundColor = new Color(0.30f + h, 0.22f + h, 0.15f + h);
        else
            tile.style.backgroundColor = new Color(0.52f + h, 0.38f + h, 0.22f + h);

        AddDQTileBorder(tile, dark ? "dq-dark-dirt-border" : "dq-dirt-border");
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

        var fence = new UIE.VisualElement();
        fence.name = $"fence-{tileX}-{tileY}";
        fence.AddToClassList("fence-container");
        fence.style.left = tileX * DISPLAY_TILE;
        fence.style.top = (MAP_HEIGHT - 1 - tileY) * DISPLAY_TILE;

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
        mapData = new int[MAP_WIDTH, MAP_HEIGHT];
        walkable = new bool[MAP_WIDTH, MAP_HEIGHT];

        // 基本は草で埋める
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                mapData[x, y] = TILE_GRASS;
                walkable[x, y] = true;
            }
        }

        // === 外周（木の壁） ===
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            mapData[x, 0] = TILE_TREE;
            walkable[x, 0] = false;
            mapData[x, MAP_HEIGHT - 1] = TILE_TREE;
            walkable[x, MAP_HEIGHT - 1] = false;
        }
        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            mapData[0, y] = TILE_TREE;
            walkable[0, y] = false;
            mapData[MAP_WIDTH - 1, y] = TILE_TREE;
            walkable[MAP_WIDTH - 1, y] = false;
        }

        // === 道（メインストリート + 縦パス） ===
        // 横メインストリート y=9, y=10 (x=1〜x=10)
        for (int x = 1; x <= 10; x++)
        {
            mapData[x, 9] = TILE_PATH;
            mapData[x, 10] = TILE_PATH;
        }
        // 縦メインパス x=5 (y=4〜y=15)
        for (int y = 4; y <= 15; y++)
        {
            mapData[5, y] = TILE_PATH;
        }
        // ボス接続パス y=15 (x=5〜x=7)
        for (int x = 5; x <= 7; x++)
        {
            mapData[x, 15] = TILE_PATH;
        }

        // === ボスの館（上部中央右寄り） ===
        // 暗い土 (x=6〜10, y=18), (x=6, y=16), (x=10, y=16)
        for (int x = 6; x <= 10; x++)
        {
            mapData[x, 18] = TILE_DARK_DIRT;
        }
        mapData[6, 16] = TILE_DARK_DIRT;
        mapData[10, 16] = TILE_DARK_DIRT;
        // 館本体 3x2 (x=7〜9, y=16〜17)
        for (int x = 7; x <= 9; x++)
        {
            for (int y = 16; y <= 17; y++)
            {
                mapData[x, y] = TILE_BOSS_MANSION;
                walkable[x, y] = false;
            }
        }
        // 柵（館の左右 y=17）
        mapData[6, 17] = TILE_FENCE;
        walkable[6, 17] = false;
        mapData[10, 17] = TILE_FENCE;
        walkable[10, 17] = false;
        // ボスの門 (8, 15)
        mapData[8, 15] = TILE_BOSS_GATE;
        walkable[8, 15] = false;

        // === 赤い家（左下 x=1〜2, y=6〜7） ===
        mapData[1, 6] = TILE_HOUSE_RED;
        walkable[1, 6] = false;
        mapData[2, 6] = TILE_HOUSE_RED;
        walkable[2, 6] = false;
        mapData[1, 7] = TILE_HOUSE_RED;
        walkable[1, 7] = false;
        mapData[2, 7] = TILE_HOUSE_RED;
        walkable[2, 7] = false;

        // === 青い家（右下 x=7〜8, y=7〜8） ===
        mapData[7, 7] = TILE_HOUSE_BLUE;
        walkable[7, 7] = false;
        mapData[8, 7] = TILE_HOUSE_BLUE;
        walkable[8, 7] = false;
        mapData[7, 8] = TILE_HOUSE_BLUE;
        walkable[7, 8] = false;
        mapData[8, 8] = TILE_HOUSE_BLUE;
        walkable[8, 8] = false;

        // === 緑の家（左上 x=2〜3, y=12〜13） ===
        mapData[2, 12] = TILE_HOUSE_GREEN;
        walkable[2, 12] = false;
        mapData[3, 12] = TILE_HOUSE_GREEN;
        walkable[3, 12] = false;
        mapData[2, 13] = TILE_HOUSE_GREEN;
        walkable[2, 13] = false;
        mapData[3, 13] = TILE_HOUSE_GREEN;
        walkable[3, 13] = false;

        // === 池（下部 x=4〜6, y=2〜3） ===
        for (int x = 4; x <= 6; x++)
        {
            for (int y = 2; y <= 3; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 柵（赤い家の庭 x=1〜3, y=5） ===
        mapData[1, 5] = TILE_FENCE;
        walkable[1, 5] = false;
        mapData[2, 5] = TILE_FENCE;
        walkable[2, 5] = false;
        mapData[3, 5] = TILE_FENCE;
        walkable[3, 5] = false;
        // 緑の家の上下の柵 (x=2, y=11), (x=3, y=11)
        mapData[2, 11] = TILE_FENCE;
        walkable[2, 11] = false;
        mapData[3, 11] = TILE_FENCE;
        walkable[3, 11] = false;

        // === 岩 ===
        mapData[9, 5] = TILE_ROCK;
        walkable[9, 5] = false;

        // === 花畑（右上 x=8〜9, y=12〜13） ===
        mapData[8, 12] = TILE_FLOWER;
        mapData[9, 12] = TILE_FLOWER;
        mapData[8, 13] = TILE_FLOWER;
        mapData[9, 13] = TILE_FLOWER;

        // 点在する花
        mapData[2, 3] = TILE_FLOWER;
        mapData[8, 3] = TILE_FLOWER;
        mapData[10, 1] = TILE_FLOWER;

        // プレイヤーの初期位置は必ず歩けるようにする
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_PATH && mapData[playerTileX, playerTileY] != TILE_GRASS)
        {
            mapData[playerTileX, playerTileY] = TILE_PATH;
        }
    }

    void GenerateDevilMapData()
    {
        mapData = new int[MAP_WIDTH, MAP_HEIGHT];
        walkable = new bool[MAP_WIDTH, MAP_HEIGHT];

        // 基本は草で埋める
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                mapData[x, y] = TILE_GRASS;
                walkable[x, y] = true;
            }
        }

        // === 外周（木の壁） ===
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            mapData[x, 0] = TILE_TREE;
            walkable[x, 0] = false;
            mapData[x, MAP_HEIGHT - 1] = TILE_TREE;
            walkable[x, MAP_HEIGHT - 1] = false;
        }
        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            mapData[0, y] = TILE_TREE;
            walkable[0, y] = false;
            mapData[MAP_WIDTH - 1, y] = TILE_TREE;
            walkable[MAP_WIDTH - 1, y] = false;
        }

        // === 暗い土の道（メインストリート） ===
        for (int x = 1; x <= 10; x++)
        {
            mapData[x, 9] = TILE_DARK_DIRT;
            mapData[x, 10] = TILE_DARK_DIRT;
        }
        // 縦パス
        for (int y = 4; y <= 15; y++)
        {
            mapData[5, y] = TILE_DARK_DIRT;
        }
        // ボス接続パス
        for (int x = 5; x <= 7; x++)
        {
            mapData[x, 15] = TILE_DARK_DIRT;
        }

        // === ボスの館（将来用 — 門なし） ===
        for (int x = 6; x <= 10; x++)
        {
            mapData[x, 18] = TILE_DARK_DIRT;
        }
        mapData[6, 16] = TILE_DARK_DIRT;
        mapData[10, 16] = TILE_DARK_DIRT;
        // 館本体 3x2
        for (int x = 7; x <= 9; x++)
        {
            for (int y = 16; y <= 17; y++)
            {
                mapData[x, y] = TILE_BOSS_MANSION;
                walkable[x, y] = false;
            }
        }
        // 柵
        mapData[6, 17] = TILE_FENCE;
        walkable[6, 17] = false;
        mapData[10, 17] = TILE_FENCE;
        walkable[10, 17] = false;
        // 門なし（将来用）— 通れないタイルだけ配置
        mapData[8, 15] = TILE_BOSS_MANSION;
        walkable[8, 15] = false;

        // === 悪魔の巣窟（赤い家 x3） ===
        mapData[1, 6] = TILE_HOUSE_RED;
        walkable[1, 6] = false;
        mapData[2, 6] = TILE_HOUSE_RED;
        walkable[2, 6] = false;
        mapData[1, 7] = TILE_HOUSE_RED;
        walkable[1, 7] = false;
        mapData[2, 7] = TILE_HOUSE_RED;
        walkable[2, 7] = false;

        mapData[7, 7] = TILE_HOUSE_RED;
        walkable[7, 7] = false;
        mapData[8, 7] = TILE_HOUSE_RED;
        walkable[8, 7] = false;
        mapData[7, 8] = TILE_HOUSE_RED;
        walkable[7, 8] = false;
        mapData[8, 8] = TILE_HOUSE_RED;
        walkable[8, 8] = false;

        mapData[2, 12] = TILE_HOUSE_RED;
        walkable[2, 12] = false;
        mapData[3, 12] = TILE_HOUSE_RED;
        walkable[3, 12] = false;
        mapData[2, 13] = TILE_HOUSE_RED;
        walkable[2, 13] = false;
        mapData[3, 13] = TILE_HOUSE_RED;
        walkable[3, 13] = false;

        // === 溶岩池（赤系の水） ===
        for (int x = 4; x <= 6; x++)
        {
            for (int y = 2; y <= 3; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 岩多め ===
        mapData[9, 5] = TILE_ROCK;
        walkable[9, 5] = false;
        mapData[3, 4] = TILE_ROCK;
        walkable[3, 4] = false;
        mapData[10, 12] = TILE_ROCK;
        walkable[10, 12] = false;
        mapData[1, 14] = TILE_ROCK;
        walkable[1, 14] = false;
        mapData[9, 3] = TILE_ROCK;
        walkable[9, 3] = false;

        // プレイヤーの初期位置は必ず歩けるようにする
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_DARK_DIRT && mapData[playerTileX, playerTileY] != TILE_GRASS)
        {
            mapData[playerTileX, playerTileY] = TILE_DARK_DIRT;
        }
    }

    void GenerateImpTownMapData()
    {
        mapData = new int[MAP_WIDTH, MAP_HEIGHT];
        walkable = new bool[MAP_WIDTH, MAP_HEIGHT];

        // 基本は草で埋める
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                mapData[x, y] = TILE_GRASS;
                walkable[x, y] = true;
            }
        }

        // === 外周（木の壁） ===
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            mapData[x, 0] = TILE_TREE;
            walkable[x, 0] = false;
            mapData[x, MAP_HEIGHT - 1] = TILE_TREE;
            walkable[x, MAP_HEIGHT - 1] = false;
        }
        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            mapData[0, y] = TILE_TREE;
            walkable[0, y] = false;
            mapData[MAP_WIDTH - 1, y] = TILE_TREE;
            walkable[MAP_WIDTH - 1, y] = false;
        }

        // === 曲がりくねった小道 ===
        // 入口 (y=1〜3, x=5)
        for (int y = 1; y <= 3; y++)
            mapData[5, y] = TILE_PATH;
        // 右に曲がる (y=3, x=5〜8)
        for (int x = 5; x <= 8; x++)
            mapData[x, 3] = TILE_PATH;
        // 上へ (y=3〜7, x=8)
        for (int y = 3; y <= 7; y++)
            mapData[8, y] = TILE_PATH;
        // 左へ (y=7, x=4〜8)
        for (int x = 4; x <= 8; x++)
            mapData[x, 7] = TILE_PATH;
        // 上へ (y=7〜11, x=4)
        for (int y = 7; y <= 11; y++)
            mapData[4, y] = TILE_PATH;
        // 右へ (y=11, x=4〜9)
        for (int x = 4; x <= 9; x++)
            mapData[x, 11] = TILE_PATH;
        // 上へ (y=11〜15, x=9)
        for (int y = 11; y <= 15; y++)
            mapData[9, y] = TILE_PATH;
        // 左へ (y=15, x=5〜9)
        for (int x = 5; x <= 9; x++)
            mapData[x, 15] = TILE_PATH;
        // 上へ (y=15〜18, x=5)
        for (int y = 15; y <= 18; y++)
            mapData[5, y] = TILE_PATH;

        // === 毒沼（水場） ===
        for (int x = 2; x <= 4; x++)
        {
            for (int y = 13; y <= 14; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 暗い土の地面 ===
        mapData[3, 2] = TILE_DARK_DIRT;
        mapData[4, 2] = TILE_DARK_DIRT;
        mapData[7, 5] = TILE_DARK_DIRT;
        mapData[9, 6] = TILE_DARK_DIRT;
        mapData[2, 9] = TILE_DARK_DIRT;
        mapData[3, 9] = TILE_DARK_DIRT;
        mapData[7, 14] = TILE_DARK_DIRT;
        mapData[8, 14] = TILE_DARK_DIRT;

        // === 岩 ===
        mapData[1, 5] = TILE_ROCK;
        walkable[1, 5] = false;
        mapData[10, 8] = TILE_ROCK;
        walkable[10, 8] = false;
        mapData[6, 13] = TILE_ROCK;
        walkable[6, 13] = false;
        mapData[3, 17] = TILE_ROCK;
        walkable[3, 17] = false;
        mapData[10, 3] = TILE_ROCK;
        walkable[10, 3] = false;

        // === 花畑（エンカウント率高め） ===
        mapData[2, 6] = TILE_FLOWER;
        mapData[3, 6] = TILE_FLOWER;
        mapData[2, 7] = TILE_FLOWER;
        mapData[6, 9] = TILE_FLOWER;
        mapData[7, 9] = TILE_FLOWER;
        mapData[7, 10] = TILE_FLOWER;
        mapData[1, 12] = TILE_FLOWER;
        mapData[10, 15] = TILE_FLOWER;
        mapData[10, 16] = TILE_FLOWER;
        mapData[7, 17] = TILE_FLOWER;

        // === 109 館 (小道の終点付近) ===
        // 館本体 (3x2: x=4〜6, y=18)
        for (int x = 4; x <= 6; x++)
        {
            mapData[x, 18] = TILE_BOSS_MANSION;
            walkable[x, 18] = false;
        }

        // === 追加の木（内部に散在） ===
        mapData[1, 3] = TILE_TREE;
        walkable[1, 3] = false;
        mapData[10, 5] = TILE_TREE;
        walkable[10, 5] = false;
        mapData[1, 10] = TILE_TREE;
        walkable[1, 10] = false;
        mapData[6, 16] = TILE_TREE;
        walkable[6, 16] = false;
        mapData[2, 17] = TILE_TREE;
        walkable[2, 17] = false;
        mapData[10, 11] = TILE_TREE;
        walkable[10, 11] = false;

        // プレイヤーの初期位置は必ず歩けるようにする
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_PATH && mapData[playerTileX, playerTileY] != TILE_GRASS)
        {
            mapData[playerTileX, playerTileY] = TILE_PATH;
        }
    }

    void GenerateMansionMapData()
    {
        mapData = new int[MAP_WIDTH, MAP_HEIGHT];
        walkable = new bool[MAP_WIDTH, MAP_HEIGHT];

        // 全て壁で埋める
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
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

        // 傭兵 (3,10) と (8,10)
        mapData[3, 10] = TILE_MERCENARY_A;
        walkable[3, 10] = false; // TryMoveで特殊処理
        mapData[8, 10] = TILE_MERCENARY_B;
        walkable[8, 10] = false;

        // y=10: 傭兵の行の床
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

        // 撃破済み傭兵のタイルを床に変更
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
        mapData = new int[MAP_WIDTH, MAP_HEIGHT];
        walkable = new bool[MAP_WIDTH, MAP_HEIGHT];

        // 全て壁で埋める
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
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

        // ボス扉 (y=12, x=5〜6) — 直接開く（傭兵チェック不要）
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

        // 傭兵A (3, 10)
        if (!DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵A"))
        {
            CreateMercenaryOverlay(3, 10, "A");
        }
        // 傭兵B (8, 10)
        if (!DataCarrier.Instance.HasDefeatedEnemy("デヴィル傭兵B"))
        {
            CreateMercenaryOverlay(8, 10, "B");
        }
    }

    void CreateMercenaryOverlay(int tx, int ty, string label)
    {
        float posX = (tx - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (ty - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;

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
        else if (areaForColor == 2)
            bg.color = new Color(0.1f, 0.08f, 0.08f);   // 館内: 暗灰
        else if (areaForColor == 1)
            bg.color = new Color(0.2f, 0.08f, 0.15f);   // 悪魔村: 暗い赤紫
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
        tilesContainerRect.sizeDelta = new Vector2(MAP_WIDTH * DISPLAY_TILE, MAP_HEIGHT * DISPLAY_TILE);

        // タイルを配置
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                if (areaForColor == 0)
                    CreateDQTileElement(x, y, mapData[x, y]); // UI Toolkit
                else
                    CreateTile(x, y, mapData[x, y]); // uGUI
            }
        }

        // ボスの館オーバーレイ（タイルの上に大きな館を描画）
        CreateBossMansionOverlay();

        // 村マップ（area==0）: スプライトオーバーレイ（uGUI、UI Toolkit の上に表示）
        if (areaForColor == 0)
            CreateVillageOverlays();
    }

    void CreateBossMansionOverlay()
    {
        if (tilesContainer == null) return;

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area == 2 || area == 3) return; // 館内・小悪魔の森ではオーバーレイ不要

        // 館本体（3x2タイル分の大きさ）中央: (8, 16.5)
        float centerX = (8 - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float centerY = (16.5f - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;

        var mansion = new GameObject("BossMansion");
        mansion.transform.SetParent(tilesContainer.transform, false);
        var mansionRect = mansion.AddComponent<RectTransform>();
        mansionRect.anchoredPosition = new Vector2(centerX, centerY);
        mansionRect.sizeDelta = new Vector2(DISPLAY_TILE * 3, DISPLAY_TILE * 2.5f);

        // 館の壁 — 悪魔村は紫系
        var wall = FacePart("Wall", mansion.transform, Vector2.zero, new Vector2(DISPLAY_TILE * 2.8f, DISPLAY_TILE * 2.0f));
        wall.AddComponent<Image>().color = (area == 1)
            ? new Color(0.15f, 0.05f, 0.2f)
            : new Color(0.2f, 0.07f, 0.1f);

        // 屋根（三角形風）
        var roof = FacePart("Roof", mansion.transform, new Vector2(0, DISPLAY_TILE * 0.8f), new Vector2(DISPLAY_TILE * 3.0f, DISPLAY_TILE * 0.7f));
        roof.AddComponent<Image>().color = (area == 1)
            ? new Color(0.25f, 0.05f, 0.3f)
            : new Color(0.35f, 0.05f, 0.08f);

        // 屋根の先端
        var roofTop = FacePart("RoofTop", mansion.transform, new Vector2(0, DISPLAY_TILE * 1.2f), new Vector2(DISPLAY_TILE * 1.8f, DISPLAY_TILE * 0.45f));
        roofTop.AddComponent<Image>().color = (area == 1)
            ? new Color(0.3f, 0.05f, 0.35f)
            : new Color(0.4f, 0.05f, 0.05f);

        // 窓（左）— 悪魔村は紫の光
        var winL = FacePart("WinL", mansion.transform, new Vector2(-DISPLAY_TILE * 0.55f, DISPLAY_TILE * 0.1f), new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.5f));
        winL.AddComponent<Image>().color = (area == 1)
            ? new Color(0.7f, 0.3f, 0.9f, 0.8f)
            : new Color(0.9f, 0.7f, 0.2f, 0.8f);

        // 窓（右）
        var winR = FacePart("WinR", mansion.transform, new Vector2(DISPLAY_TILE * 0.55f, DISPLAY_TILE * 0.1f), new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.5f));
        winR.AddComponent<Image>().color = (area == 1)
            ? new Color(0.7f, 0.3f, 0.9f, 0.8f)
            : new Color(0.9f, 0.7f, 0.2f, 0.8f);

        // ドア
        var door = FacePart("Door", mansion.transform, new Vector2(0, -DISPLAY_TILE * 0.6f), new Vector2(DISPLAY_TILE * 0.6f, DISPLAY_TILE * 0.8f));
        door.AddComponent<Image>().color = (area == 1)
            ? new Color(0.2f, 0.1f, 0.25f)
            : new Color(0.35f, 0.15f, 0.1f);

        // ドアノブ
        var knob = FacePart("Knob", mansion.transform, new Vector2(DISPLAY_TILE * 0.12f, -DISPLAY_TILE * 0.6f), new Vector2(12, 12));
        knob.AddComponent<Image>().color = new Color(0.8f, 0.65f, 0.2f);

        // 看板 — ゲート位置 (8, 15)
        var signObj = new GameObject("BossSign");
        signObj.transform.SetParent(tilesContainer.transform, false);
        var signRect = signObj.AddComponent<RectTransform>();
        float signX = (8 - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float signY = (15 - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
        signRect.anchoredPosition = new Vector2(signX, signY - DISPLAY_TILE * 0.1f);
        signRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.2f, DISPLAY_TILE * 0.55f);

        var signBg = signObj.AddComponent<Image>();
        signBg.color = (area == 1)
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
        signText.text = (area == 1)
            ? Localization.Get("map_devil_boss_sign")
            : Localization.Get("map_boss_sign");
        signText.fontSize = 18;
        signText.alignment = TextAlignmentOptions.Center;
        signText.color = Color.white;
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
        float posX = (x - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (y - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        var img = tileObj.AddComponent<UnityEngine.UI.Image>();
        img.raycastTarget = false;

        int areaForTile = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;

        if (tileType == TILE_GRASS)
        {
            CreateModernGrassTile(tileObj.transform, img, x, y);
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

        // 木オーバーレイ
        Sprite treeSprite = CreatePCSprite(pcTreesTex, 8, 4, 48, 56);

        // 岩オーバーレイ (Rocks.png: 最初の大きめ岩 — 左上付近)
        Sprite rockSprite = CreatePCSprite(pcRocksTex, 0, 16, 32, 32);

        // 花オーバーレイ (Vegetation.png: 小さな花 — 中段左付近の黄色い花)
        Sprite flowerSprite = CreatePCSprite(pcVegetationTex, 0, 224, 16, 16);

        // 柵はプロシージャルで描画（Pixel Crawlerに適した柵なし）

        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                int tile = mapData[x, y];
                switch (tile)
                {
                    case TILE_TREE:
                        if (treeSprite != null)
                            CreateSpriteOverlay(x, y, treeSprite, 2.0f, 2.0f, DISPLAY_TILE * 0.25f);
                        break;
                    case TILE_ROCK:
                        if (rockSprite != null)
                            CreateSpriteOverlay(x, y, rockSprite, 2.8f, 2.8f, 0f);
                        break;
                    case TILE_FLOWER:
                        if (flowerSprite != null)
                            CreateSpriteOverlay(x, y, flowerSprite, 4.0f, 4.0f, 0f);
                        break;
                    case TILE_FENCE:
                        CreateDQFenceElement(x, y); // UI Toolkit
                        break;
                }
            }
        }

        // 家オーバーレイ: 2×2タイルブロックを検出して描画
        CreateHouseOverlays();
    }

    void CreateSpriteOverlay(int tileX, int tileY, Sprite sprite, float scaleX, float scaleY, float offsetY)
    {
        if (tilesContainer == null || sprite == null) return;

        var overlayObj = new GameObject($"Overlay_{tileX}_{tileY}");
        overlayObj.transform.SetParent(tilesContainer.transform, false);

        var rect = overlayObj.AddComponent<RectTransform>();
        float posX = (tileX - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (tileY - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE + offsetY;
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

        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                int tile = mapData[x, y];
                if (tile != TILE_HOUSE_RED && tile != TILE_HOUSE_GREEN && tile != TILE_HOUSE_BLUE) continue;

                string key = $"{x},{y}";
                if (processed.Contains(key)) continue;

                // 2×2ブロックの左下を探す
                int bx = x, by = y;
                // この位置が2×2ブロックの左下かチェック
                if (x + 1 < MAP_WIDTH && y + 1 < MAP_HEIGHT &&
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
                float centerX = (bx + 0.5f - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
                float centerY = (by + 0.5f - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;

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
                float doorGridTop = (MAP_HEIGHT - 1 - by - 0.5f) * DISPLAY_TILE;
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
        int academic = 60;

        if (DataCarrier.Instance != null)
        {
            gender = DataCarrier.Instance.babyGender;
            godBaby = DataCarrier.Instance.isGodBaby;
            academic = DataCarrier.Instance.babyAcademic;
        }

        bool isFemale = gender == "女の子";
        int dir = playerDirection; // 0=下, 1=上, 2=左, 3=右
        float s = 1.0f; // スケール

        // 色の決定
        Color skin = new Color(0.98f, 0.89f, 0.82f);
        Color[] hairTones = {
            new Color(0.08f, 0.06f, 0.05f),
            new Color(0.2f, 0.12f, 0.08f),
            new Color(0.35f, 0.22f, 0.12f),
            new Color(0.55f, 0.38f, 0.2f)
        };
        Color hair = hairTones[Mathf.Clamp(academic / 25, 0, 3)];
        Color clothMain = isFemale
            ? new Color(0.95f, 0.45f, 0.6f)
            : new Color(0.3f, 0.5f, 0.85f);
        Color clothDark = isFemale
            ? new Color(0.8f, 0.3f, 0.45f)
            : new Color(0.2f, 0.35f, 0.7f);
        Color shoeColor = new Color(0.45f, 0.28f, 0.15f);
        float xFlip = (dir == 3) ? -1f : 1f; // 右向きは左向きの反転

        // GOD BABYオーラ（全方向共通）
        if (godBaby)
        {
            var aura = FacePart("Aura", parent, new Vector2(0, 4 * s), new Vector2(72 * s, 82 * s));
            aura.AddComponent<Image>().color = new Color(1f, 0.85f, 0.2f, 0.18f);
        }

        // === 影 ===
        FacePart("Shadow", parent, new Vector2(0, -36 * s), new Vector2(36 * s, 10 * s))
            .AddComponent<Image>().color = new Color(0, 0, 0, 0.25f);

        // === 足 ===
        if (dir == 0 || dir == 1) // 正面・背面: 2本
        {
            FacePart("FootL", parent, new Vector2(-8 * s, -30 * s), new Vector2(10 * s, 8 * s))
                .AddComponent<Image>().color = shoeColor;
            FacePart("FootR", parent, new Vector2(8 * s, -30 * s), new Vector2(10 * s, 8 * s))
                .AddComponent<Image>().color = shoeColor;
        }
        else // 左右: 1本見え
        {
            FacePart("Foot", parent, new Vector2(2 * xFlip * s, -30 * s), new Vector2(12 * s, 8 * s))
                .AddComponent<Image>().color = shoeColor;
        }

        // === 体（胴体） ===
        FacePart("Body", parent, new Vector2(0, -14 * s), new Vector2(28 * s, 26 * s))
            .AddComponent<Image>().color = clothMain;

        // GOD BABY金縁
        if (godBaby)
        {
            FacePart("BodyTrim", parent, new Vector2(0, -14 * s), new Vector2(32 * s, 28 * s))
                .AddComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.3f);
            // 体を上に再配置
            FacePart("BodyOver", parent, new Vector2(0, -14 * s), new Vector2(28 * s, 26 * s))
                .AddComponent<Image>().color = clothMain;
        }

        // === マント/ケープ（背面dir=1でGOD BABYのみ） ===
        if (dir == 1 && godBaby)
        {
            FacePart("Cape", parent, new Vector2(0, -10 * s), new Vector2(34 * s, 30 * s))
                .AddComponent<Image>().color = new Color(1f, 0.84f, 0f, 0.5f);
        }

        // === 腕 ===
        if (dir == 0) // 正面: 両腕
        {
            FacePart("ArmL", parent, new Vector2(-18 * s, -12 * s), new Vector2(8 * s, 18 * s))
                .AddComponent<Image>().color = clothDark;
            FacePart("ArmR", parent, new Vector2(18 * s, -12 * s), new Vector2(8 * s, 18 * s))
                .AddComponent<Image>().color = clothDark;
            // 手
            FacePart("HandL", parent, new Vector2(-18 * s, -22 * s), new Vector2(7 * s, 7 * s))
                .AddComponent<Image>().color = skin;
            FacePart("HandR", parent, new Vector2(18 * s, -22 * s), new Vector2(7 * s, 7 * s))
                .AddComponent<Image>().color = skin;
        }
        else if (dir == 1) // 背面: 腕は見えにくい（肩だけ）
        {
            FacePart("ShoulderL", parent, new Vector2(-16 * s, -6 * s), new Vector2(7 * s, 10 * s))
                .AddComponent<Image>().color = clothDark;
            FacePart("ShoulderR", parent, new Vector2(16 * s, -6 * s), new Vector2(7 * s, 10 * s))
                .AddComponent<Image>().color = clothDark;
        }
        else // 左右: 1本
        {
            FacePart("Arm", parent, new Vector2(-12 * xFlip * s, -12 * s), new Vector2(8 * s, 18 * s))
                .AddComponent<Image>().color = clothDark;
            FacePart("Hand", parent, new Vector2(-12 * xFlip * s, -22 * s), new Vector2(7 * s, 7 * s))
                .AddComponent<Image>().color = skin;
        }

        // === 武器（小さな剣） ===
        if (dir == 0) // 正面: 右手に剣
        {
            var sword = FacePart("Sword", parent, new Vector2(24 * s, -14 * s), new Vector2(4 * s, 22 * s));
            sword.transform.localRotation = Quaternion.Euler(0, 0, -15);
            sword.AddComponent<Image>().color = new Color(0.75f, 0.75f, 0.8f);
            FacePart("SwordHilt", parent, new Vector2(23 * s, -22 * s), new Vector2(8 * s, 4 * s))
                .AddComponent<Image>().color = new Color(0.55f, 0.35f, 0.15f);
        }
        else if (dir == 2 || dir == 3) // 横向き: 手前に剣
        {
            var sword = FacePart("Sword", parent, new Vector2(-16 * xFlip * s, -14 * s), new Vector2(4 * s, 22 * s));
            sword.transform.localRotation = Quaternion.Euler(0, 0, 15 * xFlip);
            sword.AddComponent<Image>().color = new Color(0.75f, 0.75f, 0.8f);
            FacePart("SwordHilt", parent, new Vector2(-15 * xFlip * s, -22 * s), new Vector2(8 * s, 4 * s))
                .AddComponent<Image>().color = new Color(0.55f, 0.35f, 0.15f);
        }

        // === 頭（SDなので大きめ） ===
        FacePart("Head", parent, new Vector2(0, 14 * s), new Vector2(38 * s, 34 * s))
            .AddComponent<Image>().color = skin;

        // === 耳 ===
        if (dir == 2 || dir == 3) // 横向き: 片方の耳だけ見える
        {
            FacePart("Ear", parent, new Vector2(-18 * xFlip * s, 12 * s), new Vector2(6 * s, 10 * s))
                .AddComponent<Image>().color = skin;
        }
        else if (dir == 0) // 正面: 両耳
        {
            FacePart("EarL", parent, new Vector2(-20 * s, 12 * s), new Vector2(6 * s, 10 * s))
                .AddComponent<Image>().color = skin;
            FacePart("EarR", parent, new Vector2(20 * s, 12 * s), new Vector2(6 * s, 10 * s))
                .AddComponent<Image>().color = skin;
        }

        // === 髪 ===
        if (dir == 0) // 正面: 前髪
        {
            FacePart("HairTop", parent, new Vector2(0, 28 * s), new Vector2(42 * s, 16 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairBangs", parent, new Vector2(0, 22 * s), new Vector2(38 * s, 8 * s))
                .AddComponent<Image>().color = new Color(hair.r * 0.85f, hair.g * 0.85f, hair.b * 0.85f);
            // サイドヘア
            FacePart("HairSideL", parent, new Vector2(-18 * s, 16 * s), new Vector2(6 * s, 14 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairSideR", parent, new Vector2(18 * s, 16 * s), new Vector2(6 * s, 14 * s))
                .AddComponent<Image>().color = hair;
        }
        else if (dir == 1) // 背面: 後ろ髪多め
        {
            FacePart("HairBack", parent, new Vector2(0, 18 * s), new Vector2(40 * s, 28 * s))
                .AddComponent<Image>().color = hair;
            FacePart("HairBackHighlight", parent, new Vector2(0, 26 * s), new Vector2(34 * s, 10 * s))
                .AddComponent<Image>().color = new Color(hair.r * 1.15f, hair.g * 1.15f, hair.b * 1.15f);
        }
        else // 左右: 横分けの髪
        {
            FacePart("HairTop", parent, new Vector2(2 * xFlip * s, 28 * s), new Vector2(40 * s, 16 * s))
                .AddComponent<Image>().color = hair;
            // 前髪が片方に流れる
            FacePart("HairFront", parent, new Vector2(10 * xFlip * s, 20 * s), new Vector2(14 * s, 12 * s))
                .AddComponent<Image>().color = hair;
            // 後ろ髪
            FacePart("HairBack", parent, new Vector2(-8 * xFlip * s, 14 * s), new Vector2(12 * s, 18 * s))
                .AddComponent<Image>().color = hair;
        }

        // === 目（正面・横で見える、背面では見えない） ===
        if (dir == 0) // 正面: 両目（SD風の大きな目）
        {
            DrawSdEye(parent, -9 * s, 14 * s, s, isFemale);
            DrawSdEye(parent, 9 * s, 14 * s, s, isFemale);
        }
        else if (dir == 2 || dir == 3) // 横向き: 1つの目
        {
            DrawSdEye(parent, 4 * xFlip * s, 14 * s, s, isFemale);
        }
        // dir==1 背面は目なし

        // === 口（正面のみ） ===
        if (dir == 0)
        {
            FacePart("Mouth", parent, new Vector2(0, 4 * s), new Vector2(8 * s, 3 * s))
                .AddComponent<Image>().color = new Color(0.82f, 0.55f, 0.55f);
        }

        // === ほっぺ（正面・横） ===
        if (dir == 0)
        {
            float cheekAlpha = isFemale ? 0.4f : 0.15f;
            Color cheekC = isFemale
                ? new Color(1f, 0.5f, 0.55f, cheekAlpha)
                : new Color(1f, 0.7f, 0.7f, cheekAlpha);
            FacePart("CheekL", parent, new Vector2(-12 * s, 8 * s), new Vector2(8 * s, 6 * s))
                .AddComponent<Image>().color = cheekC;
            FacePart("CheekR", parent, new Vector2(12 * s, 8 * s), new Vector2(8 * s, 6 * s))
                .AddComponent<Image>().color = cheekC;
        }
        else if ((dir == 2 || dir == 3) && isFemale)
        {
            FacePart("Cheek", parent, new Vector2(8 * xFlip * s, 8 * s), new Vector2(8 * s, 6 * s))
                .AddComponent<Image>().color = new Color(1f, 0.5f, 0.55f, 0.35f);
        }

        // === GODオーラ（半透明オーバーレイ、最前面） ===
        if (godBaby)
        {
            var glow = FacePart("GodGlow", parent, new Vector2(0, 4 * s), new Vector2(50 * s, 70 * s));
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
        float posX = (playerTileX - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (playerTileY - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
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
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        var btn = new UIE.Button();
        btn.AddToClassList("map-menu-btn");
        btn.focusable = false;
        btn.style.top = 24 + safeTop;
        btn.clicked += ToggleMenu;

        for (int i = 0; i < 3; i++)
        {
            var line = new UIE.VisualElement();
            line.AddToClassList("map-menu-line");
            btn.Add(line);
        }

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

        string nameColor = isGod ? "<color=#FFD700>" : "<color=#FFFFFF>";
        string godLabel = isGod ? " <color=#FFD700>GOD BABY</color>" : "";
        string poisonLabel = poisonTurns > 0 ? $" <color=#AA00FF>毒({poisonTurns})</color>" : "";

        statusLabel.text = $"{nameColor}{babyName}</color>  {Localization.GetAge(age)}{godLabel}\n" +
                           $"HP:<color=#00FF00>{currentHp}/{maxHp}</color>  ATK:<color=#FF6666>{atk}</color>  DEF:<color=#6699FF>{def}</color>{poisonLabel}";
    }

    void Update()
    {
        var kb = Keyboard.current;
        bool escPressed = (kb != null && kb.escapeKey.wasPressedThisFrame) || touchMenuPressed;

        // ESCでパネルを閉じる
        if (saveOverlayEl != null)
        {
            if (escPressed) { touchMenuPressed = false; CloseSavePanel(); }
            return;
        }
        if (statusDetailEl != null)
        {
            if (escPressed) { touchMenuPressed = false; CloseStatusPanel(); }
            return;
        }
        if (inventoryOverlayEl != null)
        {
            if (escPressed) { touchMenuPressed = false; CloseInventoryPanel(); }
            return;
        }

        if (menuOpen) return;

        HandleInput();
        UpdateMovement();
    }

    void HandleInput()
    {
        if (isMoving) return;

        int dx = 0, dy = 0;

        // キーボード入力
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.upArrowKey.isPressed || kb.wKey.isPressed)
                dy = 1;
            else if (kb.downArrowKey.isPressed || kb.sKey.isPressed)
                dy = -1;
            else if (kb.leftArrowKey.isPressed || kb.aKey.isPressed)
                dx = -1;
            else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed)
                dx = 1;
        }

        // タッチ入力（スワイプ）
        if (dx == 0 && dy == 0)
        {
            dx = touchDx;
            dy = touchDy;
        }

        if (dx != 0 || dy != 0)
        {
            TryMove(dx, dy);
        }

        // インタラクト
        bool kbInteract = kb != null && kb.spaceKey.wasPressedThisFrame;
        if (kbInteract || touchInteract)
        {
            touchInteract = false;
            Interact();
        }

        // メニュー
        bool kbMenu = kb != null && kb.escapeKey.wasPressedThisFrame;
        if (kbMenu || touchMenuPressed)
        {
            touchMenuPressed = false;
            ToggleMenu();
        }
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

        if (newX < 0 || newX >= MAP_WIDTH || newY < 0 || newY >= MAP_HEIGHT)
            return;

        // ボスの門に歩いて入ろうとした場合
        if (mapData[newX, newY] == TILE_BOSS_GATE)
        {
            StartCoroutine(EnterBossMansion());
            return;
        }
        // 悪魔村: 館に歩いて入ろうとした場合 → 館内（area 2）へ
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
                StartFixedEncounter(mercName);
                return;
            }
        }
        if (tileAtDest == TILE_BOSS_DOOR)
        {
            int areaForDoor = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (areaForDoor == 4)
            {
                // 109: 傭兵なし、直接ボス戦
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

        if (!walkable[newX, newY])
            return;

        playerTileX = newX;
        playerTileY = newY;

        float posX = (playerTileX - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (playerTileY - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
        targetPosition = new Vector2(posX, posY);
        isMoving = true;

        // 歩数カウント
        stepCount++;

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

        if (Vector2.Distance(playerRect.anchoredPosition, targetPosition) < 0.5f)
        {
            playerRect.anchoredPosition = targetPosition;
            isMoving = false;

            // 歩行毒ダメージ
            ApplyPoisonStep();

            // アイテム拾得判定
            CheckItemPickup();

            // ミルクポイント回復判定
            CheckMilkPoint();

            // 移動完了時にエンカウント判定
            CheckRandomEncounter();
        }
    }

    void CheckRandomEncounter()
    {
        // 館内 (area 2) ではランダムエンカウントなし
        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area == 2) return;

        // 草タイルの上でランダムエンカウント（移動完了時に判定）
        if (mapData[playerTileX, playerTileY] == TILE_GRASS ||
            mapData[playerTileX, playerTileY] == TILE_FLOWER)
        {
            float rate;
            if (area == 3)
            {
                // 小悪魔の森: 草5%, 花10%
                rate = mapData[playerTileX, playerTileY] == TILE_FLOWER ? 0.10f : 0.05f;
            }
            else if (area == 1)
            {
                // 悪魔村: 草4%, 花8%（村の2倍）
                rate = mapData[playerTileX, playerTileY] == TILE_FLOWER ? 0.08f : 0.04f;
            }
            else
            {
                // 村: 花畑は4%、草は2%
                rate = mapData[playerTileX, playerTileY] == TILE_FLOWER ? 0.04f : 0.02f;
            }

            if (Random.Range(0f, 1f) < rate)
            {
                StartCoroutine(StartBattle());
            }
        }
    }

    IEnumerator StartBattle()
    {
        menuOpen = true;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.backgroundColor = new Color(1, 1, 1, 0);
        overlayRoot.Add(overlay);

        // フラッシュエフェクト
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
        var label = UIHelper.CreateLabel(Localization.Get("map_encounter"), "map-encounter-text");
        overlay.Add(label);

        yield return new WaitForSeconds(1.0f);

        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.SaveData();
        }

        SceneManager.LoadScene("BattleScene");
    }

    void Interact()
    {
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
                // 悪魔村: 館内（area 2）に入る
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
        SceneManager.LoadScene("BattleScene");
    }

    // 悪魔村から館内（area 2）に入る
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
    void StartFixedEncounter(string enemyName)
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.fixedEncounterEnemy = enemyName;
            DataCarrier.Instance.cameFromMap = true;
            DataCarrier.Instance.isBossBattle = false;
            DataCarrier.Instance.SaveData();
        }
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
        SceneManager.LoadScene("BattleScene");
    }

    // 館から悪魔村に戻る
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
            DataCarrier.Instance.mapPlayerX = 5;
            DataCarrier.Instance.mapPlayerY = 17;
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
            if (nx >= 0 && nx < MAP_WIDTH && ny >= 0 && ny < MAP_HEIGHT)
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
            Localization.Get("map_menu_inventory"),
            Localization.Get("map_menu_home"),
            Localization.Get("map_menu_save"),
            Localization.Get("map_menu_title"),
            Localization.Get("map_menu_close")
        };
        System.Action[] actions = {
            () => { CloseMenu(); OpenStatusPanel(); },
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
        closeBtn.AddToClassList("pill-button");
        closeBtn.style.marginTop = 64;
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
        yesBtn.AddToClassList("pill-button");
        yesBtn.style.marginTop = 64;
        yesBtn.text = Localization.Get("map_save_overwrite");
        UIHelper.ApplyFont(yesBtn);
        yesBtn.clicked += () => DoSaveToSlot(slot);
        saveOverlayEl.Add(yesBtn);

        var cancelBtn = new UIE.Button();
        cancelBtn.AddToClassList("pill-button");
        cancelBtn.style.marginTop = 64;
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
        ShowMessage(Localization.Get("ui_saved"));
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
        float posX = (eggX - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (eggY - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
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
    }

    // ===== ミルクポイント（回復） =====

    void CreateMilkPoint()
    {
        if (tilesContainer == null) return;

        milkPointObj = new GameObject("MilkPoint");
        milkPointObj.transform.SetParent(tilesContainer.transform, false);

        var rect = milkPointObj.AddComponent<RectTransform>();
        float posX = (milkPointX - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (milkPointY - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(DISPLAY_TILE, DISPLAY_TILE);

        // タップ用の透明ボタン背景
        var btnImg = milkPointObj.AddComponent<Image>();
        btnImg.color = new Color(0, 0, 0, 0);  // 透明だがraycast受け取り可能
        var btn = milkPointObj.AddComponent<Button>();
        btn.transition = Selectable.Transition.None;
        btn.onClick.AddListener(() => OnMilkPointTapped());

        // 哺乳瓶の描画
        DrawBabyBottle(milkPointObj.transform, 0.8f);

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

        if (DataCarrier.Instance != null)
        {
            int currentHp = DataCarrier.Instance.babyCurrentHp;
            int maxHp = DataCarrier.Instance.babyHp;
            bool isPoisoned = DataCarrier.Instance.babyPoisonTurns > 0;
            if ((currentHp == -1 || currentHp >= maxHp) && !isPoisoned)
            {
                // 満タン＋毒なしの場合メッセージだけ表示
                ShowMessage(Localization.Get("map_milk_full"));
                return;
            }

            // HP全回復 + 毒治療
            DataCarrier.Instance.babyCurrentHp = -1;
            DataCarrier.Instance.babyPoisonTurns = 0;
            UpdateStatusText();
        }

        StartCoroutine(ShowMilkCutin());
    }

    IEnumerator ShowMilkCutin()
    {
        milkCutinActive = true;
        menuOpen = true;

        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("fill");
        overlay.style.alignItems = UIE.Align.Center;
        overlay.style.justifyContent = UIE.Justify.Center;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0);
        overlayRoot.Add(overlay);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.6f * (elapsed / 0.3f));
            yield return null;
        }
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.6f);

        // 背景円
        var circle = new UIE.VisualElement();
        circle.AddToClassList("milk-cutin-circle");
        overlay.Add(circle);

        // 哺乳瓶（Canvas FacePart で描画）
        var bottleHost = new GameObject("MilkBottleHost");
        bottleHost.transform.SetParent(canvas.transform, false);
        var hostRect = bottleHost.AddComponent<RectTransform>();
        hostRect.anchorMin = new Vector2(0.5f, 0.5f);
        hostRect.anchorMax = new Vector2(0.5f, 0.5f);
        hostRect.anchoredPosition = new Vector2(0, 50);
        hostRect.sizeDelta = new Vector2(300, 300);
        DrawBabyBottle(bottleHost.transform, 2.5f);

        // テキスト帯
        var textBg = new UIE.VisualElement();
        textBg.AddToClassList("milk-cutin-text-bg");
        var label = UIHelper.CreateLabel(Localization.Get("map_milk_heal"), "milk-cutin-text");
        textBg.Add(label);
        overlay.Add(textBg);

        yield return new WaitForSeconds(2.5f);

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.6f * (1 - elapsed / 0.3f));
            yield return null;
        }

        overlay.RemoveFromHierarchy();
        if (bottleHost != null) Destroy(bottleHost);
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

        string genderColor = dc.babyGender == "\u7537\u306e\u5b50" ? "#00BFFF" : "#FF69B4";
        string traitColor = dc.trait1 == "\u8987\u738b\u8272" ? "#FF4500" : "#FFA500";
        string godLabel = dc.isGodBaby ? "  <color=#FFD700>\u2605GOD BABY\u2605</color>" : "";

        string content = $"<b>{dc.babyName}</b>{godLabel}\n" +
            $"<color={genderColor}>{Localization.GetGender(dc.babyGender)}</color>\u3000\u3000{Localization.GetAge(dc.babyAge)}\n" +
            "\n" +
            $"{Localization.Get("map_status_hp")} {dc.babyHp}\u3000\u3000{Localization.Get("map_status_atk")} {dc.babyAtk}\u3000\u3000{Localization.Get("map_status_def")} {dc.babyDef}\n" +
            $"{Localization.Get("map_status_academic")} {dc.babyAcademic}\u3000\u3000{Localization.Get("map_status_athletic")} {dc.babyAthletic}\n" +
            $"{Localization.Get("map_status_height")} {dc.babyHeight} cm\u3000\u3000{Localization.Get("map_status_weight")} {dc.babyWeight} g\n" +
            "\n" +
            $"{Localization.Get("map_status_trait")} <color={traitColor}>{Localization.GetTrait(dc.trait1)}</color>\n" +
            "\n" +
            $"{Localization.Get("map_status_father")} {dc.fatherName}\u3000\u3000{Localization.Get("map_status_mother")} {dc.motherName}\n" +
            "\n" +
            $"{Localization.Get("map_status_exp")} {dc.babyExp} / {DataCarrier.ExpForNextAge(dc.babyAge)}\n" +
            $"{Localization.Get("map_status_enemies")} {dc.defeatedEnemies}";

        var contentLabel = UIHelper.CreateLabel(content, "map-detail-content");
        contentLabel.enableRichText = true;
        statusDetailEl.Add(contentLabel);

        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button-small");
        closeBtn.style.marginTop = 20;
        closeBtn.clicked += () => CloseStatusPanel();
        statusDetailEl.Add(closeBtn);

        overlay.Add(statusDetailEl);
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
                inventoryOverlayEl.Add(itemLabel);
            }
        }

        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button-small");
        closeBtn.style.marginTop = 20;
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
            touchStartPos = new Vector2(evt.position.x, evt.position.y);
            isTouchDragging = true;
            touchDx = 0; touchDy = 0;
        });

        touchSwipeEl.RegisterCallback<UIE.PointerMoveEvent>(evt =>
        {
            if (!isTouchDragging) return;
            Vector2 current = new Vector2(evt.position.x, evt.position.y);
            Vector2 delta = current - touchStartPos;
            if (delta.magnitude < SWIPE_THRESHOLD)
            {
                touchDx = 0; touchDy = 0;
                return;
            }
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                touchDx = delta.x > 0 ? 1 : -1;
                touchDy = 0;
            }
            else
            {
                touchDx = 0;
                // UI Toolkit Y is inverted vs screen coords
                touchDy = delta.y < 0 ? 1 : -1;
            }
        });

        touchSwipeEl.RegisterCallback<UIE.PointerUpEvent>(evt =>
        {
            isTouchDragging = false;
            touchDx = 0; touchDy = 0;
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
        if (villagePanelSettings != null)
            Destroy(villagePanelSettings);
        if (overlayPanelSettings != null)
            Destroy(overlayPanelSettings);
    }
}
