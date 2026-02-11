using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Canvas canvas;

    // タイルサイズ
    const int TILE_SIZE = 32;
    const int MAP_WIDTH = 12;
    const int MAP_HEIGHT = 20;

    // 表示スケール（UI上でのタイルの大きさ）- 9:16縦画面用
    const float DISPLAY_SCALE = 2.8f;
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
    TextMeshProUGUI statusText;
    Texture2D tilesetTexture;
    Dictionary<int, Sprite> tileSprites = new Dictionary<int, Sprite>();

    // プレイヤースプライト
    Image playerImage;
    int playerDirection = 0; // 0=下(正面), 1=上(背面), 2=左, 3=右

    // メニュー
    GameObject menuPanel;
    bool menuOpen = false;

    // エンカウント
    float encounterCooldown = 0f;
    int stepCount = 0;

    // 金のたまご
    GameObject goldenEggObj;
    const int GOLDEN_EGG_X = 9;
    const int GOLDEN_EGG_Y = 14;

    // ミルクポイント（回復）
    GameObject milkPointObj;
    int milkPointX = 3;
    int milkPointY = 10;
    bool milkCutinActive = false;

    // 持ち物パネル
    GameObject inventoryPanel;
    GameObject statusPanel;
    GameObject savePanel;


    // タッチ操作
    GameObject touchControlsObj;
    int touchDx, touchDy;
    bool touchInteract;
    bool touchMenuPressed;

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
        if (area == 2)
            GenerateMansionMapData();
        else if (area == 1)
            GenerateDevilMapData();
        else
            GenerateMapData();

        CreateMapUI();
        CreatePlayer();
        CreateStatusUI();
        CreateMenuButton();

        if (area == 2)
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

        if (SafeAreaHelper.IsTouchDevice())
            CreateTouchControls();

        // プレイヤーを最前面に
        if (playerObj != null)
            playerObj.transform.SetAsLastSibling();
    }

    void LoadTileset()
    {
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

        // マップパネル（画面全体の背景）
        mapPanel = new GameObject("MapPanel");
        mapPanel.transform.SetParent(canvas.transform, false);
        var mapRect = mapPanel.AddComponent<RectTransform>();
        mapRect.anchorMin = Vector2.zero;
        mapRect.anchorMax = Vector2.one;
        mapRect.offsetMin = Vector2.zero;
        mapRect.offsetMax = Vector2.zero;

        var bg = mapPanel.AddComponent<Image>();
        int areaForColor = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (areaForColor == 2)
            bg.color = new Color(0.1f, 0.08f, 0.08f);   // 館内: 暗灰
        else if (areaForColor == 1)
            bg.color = new Color(0.2f, 0.08f, 0.15f);   // 悪魔村: 暗い赤紫
        else
            bg.color = new Color(0.15f, 0.35f, 0.15f);  // 村: 緑
        bg.raycastTarget = false;

        // タイルコンテナ（9:16縦画面: マップを上寄りに配置、下はタッチ操作エリア）
        tilesContainer = new GameObject("TilesContainer");
        tilesContainer.transform.SetParent(mapPanel.transform, false);
        tilesContainerRect = tilesContainer.AddComponent<RectTransform>();
        tilesContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        tilesContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        tilesContainerRect.anchoredPosition = new Vector2(0, 40);
        tilesContainerRect.sizeDelta = new Vector2(MAP_WIDTH * DISPLAY_TILE, MAP_HEIGHT * DISPLAY_TILE);

        // タイルを配置
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            for (int y = 0; y < MAP_HEIGHT; y++)
            {
                CreateTile(x, y, mapData[x, y]);
            }
        }

        // ボスの館オーバーレイ（タイルの上に大きな館を描画）
        CreateBossMansionOverlay();
    }

    void CreateBossMansionOverlay()
    {
        if (tilesContainer == null) return;

        int area = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
        if (area == 2) return; // 館内ではオーバーレイ不要

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
        signText.text = (area == 1)
            ? Localization.Get("map_devil_boss_sign")
            : Localization.Get("map_boss_sign");
        signText.fontSize = 18;
        signText.alignment = TextAlignmentOptions.Center;
        signText.color = Color.white;
        signText.fontStyle = FontStyles.Bold;
        signText.raycastTarget = false;
    }

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

        var img = tileObj.AddComponent<Image>();
        img.raycastTarget = false;

        // 草タイルはモダンな見た目をプログラムで生成
        if (tileType == TILE_GRASS)
        {
            CreateModernGrassTile(tileObj.transform, img, x, y);
        }
        else if (tileSprites.ContainsKey(tileType) && tileSprites[tileType] != null)
        {
            img.sprite = tileSprites[tileType];
            // 悪魔村の水タイルは赤系（溶岩池）
            int areaForTile = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (areaForTile == 1 && tileType == TILE_WATER)
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
        if (area == 1)
            baseColor = new Color(0.22f + hueShift, 0.15f + brightShift, 0.28f + hueShift); // 暗い紫灰
        else
            baseColor = new Color(0.28f + hueShift, 0.62f + brightShift, 0.25f + hueShift);
        baseImg.color = baseColor;

        // グラデーションオーバーレイ（上部を少し明るく）
        var gradTop = FacePart("GradTop", parent, new Vector2(0, DISPLAY_TILE * 0.2f),
            new Vector2(DISPLAY_TILE, DISPLAY_TILE * 0.5f));
        var gradImg = gradTop.AddComponent<Image>();
        gradImg.color = (area == 1)
            ? new Color(0.3f, 0.2f, 0.4f, 0.25f)
            : new Color(0.4f, 0.75f, 0.35f, 0.25f);
        gradImg.raycastTarget = false;

        // 下部の影
        var gradBot = FacePart("GradBot", parent, new Vector2(0, -DISPLAY_TILE * 0.25f),
            new Vector2(DISPLAY_TILE, DISPLAY_TILE * 0.4f));
        var gradBotImg = gradBot.AddComponent<Image>();
        gradBotImg.color = (area == 1)
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
            bladeImg.color = (area == 1)
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
        if (canvas == null) return;

        var (safeLeft, safeRight, safeTop, safeBottom) = SafeAreaHelper.GetSafeAreaInsets(canvas);

        // ステータスパネル（上部）
        var statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(canvas.transform, false);
        var statusRect = statusPanel.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(0.5f, 1);
        statusRect.anchoredPosition = new Vector2(0, -safeTop);
        statusRect.sizeDelta = new Vector2(0, 80);

        var statusBg = statusPanel.AddComponent<Image>();
        statusBg.color = new Color(0.05f, 0.05f, 0.15f, 0.85f);
        statusBg.raycastTarget = false;

        var textObj = new GameObject("StatusText");
        textObj.transform.SetParent(statusPanel.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(15, 5);
        textRect.offsetMax = new Vector2(-100, -5);

        statusText = textObj.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 28;
        statusText.alignment = TextAlignmentOptions.Left;
        statusText.color = Color.white;
        statusText.raycastTarget = false;
        statusText.richText = true;

        UpdateStatusText();
    }

    void CreateMenuButton()
    {
        if (canvas == null) return;

        var (safeLeft, safeRight, safeTop, safeBottom) = SafeAreaHelper.GetSafeAreaInsets(canvas);

        float size = 70f;

        // 右上のメニューボタン（円形＋ハンバーガーアイコン）
        var btnObj = new GameObject("MenuButton");
        btnObj.transform.SetParent(canvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-15 - safeRight, -15 - safeTop);
        btnRect.sizeDelta = new Vector2(size, size);

        // 円形背景
        var btnImg = btnObj.AddComponent<Image>();
        btnImg.sprite = CreateCircleSprite(64);
        btnImg.type = Image.Type.Simple;
        btnImg.color = new Color(0.25f, 0.25f, 0.4f, 0.85f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(ToggleMenu);

        // ハンバーガー3本線
        float lineW = 30f;
        float lineH = 4f;
        float gap = 8f;
        for (int i = -1; i <= 1; i++)
        {
            var line = new GameObject("Line" + (i + 2));
            line.transform.SetParent(btnObj.transform, false);
            var lr = line.AddComponent<RectTransform>();
            lr.anchorMin = new Vector2(0.5f, 0.5f);
            lr.anchorMax = new Vector2(0.5f, 0.5f);
            lr.anchoredPosition = new Vector2(0, i * gap);
            lr.sizeDelta = new Vector2(lineW, lineH);
            var lineImg = line.AddComponent<Image>();
            lineImg.color = Color.white;
            lineImg.raycastTarget = false;
        }
    }

    Sprite CreateCircleSprite(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius - dist);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }

    void UpdateStatusText()
    {
        if (statusText == null) return;

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

        statusText.text = $"{nameColor}{babyName}</color>  {Localization.GetAge(age)}{godLabel}\n" +
                          $"HP:<color=#00FF00>{currentHp}/{maxHp}</color>  ATK:<color=#FF6666>{atk}</color>  DEF:<color=#6699FF>{def}</color>{poisonLabel}";
    }

    void Update()
    {
        var kb = Keyboard.current;
        bool escPressed = (kb != null && kb.escapeKey.wasPressedThisFrame) || touchMenuPressed;

        // ESCでパネルを閉じる
        if (savePanel != null)
        {
            if (escPressed) { touchMenuPressed = false; CloseSavePanel(); }
            return;
        }
        if (statusPanel != null)
        {
            if (escPressed) { touchMenuPressed = false; CloseStatusPanel(); }
            return;
        }
        if (inventoryPanel != null)
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

        // タッチ入力（D-padホールド）
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
        if (mapData[newX, newY] == TILE_BOSS_MANSION)
        {
            int areaForGate = DataCarrier.Instance != null ? DataCarrier.Instance.currentArea : 0;
            if (areaForGate == 1)
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
            if (area == 1)
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
        // 入力を無効化
        menuOpen = true;

        // エンカウント演出（フラッシュ）
        var flashObj = new GameObject("EncounterFlash");
        flashObj.transform.SetParent(canvas.transform, false);
        var flashRect = flashObj.AddComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;

        var flashImg = flashObj.AddComponent<Image>();
        flashImg.color = new Color(1, 1, 1, 0);
        flashImg.raycastTarget = false;

        // フラッシュエフェクト
        for (int i = 0; i < 3; i++)
        {
            float flashTime = 0.15f;
            float elapsed = 0f;
            while (elapsed < flashTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashTime;
                flashImg.color = new Color(1, 1, 1, 1f - t);
                yield return null;
            }
            yield return new WaitForSeconds(0.05f);
        }

        // テキスト演出
        var textObj = new GameObject("EncounterText");
        textObj.transform.SetParent(flashObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(800, 100);

        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = Localization.Get("map_encounter");
        tmpText.fontSize = 42;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;

        flashImg.color = new Color(0, 0, 0, 0.7f);

        yield return new WaitForSeconds(1.0f);

        // セーブしてからバトルシーンへ
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
        menuOpen = true; // 入力無効化

        // 演出
        var overlay = new GameObject("BossOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0);
        overlayImg.raycastTarget = false;

        // 暗転
        float fadeTime = 0.8f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            overlayImg.color = new Color(0, 0, 0, elapsed / fadeTime);
            yield return null;
        }

        // テキスト
        var textObj = new GameObject("BossText");
        textObj.transform.SetParent(overlay.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(800, 200);

        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = Localization.Get("map_boss_enter");
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;

        yield return new WaitForSeconds(2.0f);

        // ボス戦へ
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
        menuOpen = true;

        var overlay = new GameObject("MansionOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0);
        overlayImg.raycastTarget = false;

        float fadeTime = 0.8f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            overlayImg.color = new Color(0, 0, 0, elapsed / fadeTime);
            yield return null;
        }

        var textObj = new GameObject("MansionText");
        textObj.transform.SetParent(overlay.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(800, 200);

        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = Localization.Get("map_mansion_enter");
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;

        yield return new WaitForSeconds(2.0f);

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
        menuOpen = true;

        var overlay = new GameObject("DevilLadyOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        var overlayImg = overlay.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0);
        overlayImg.raycastTarget = false;

        float fadeTime = 0.8f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            overlayImg.color = new Color(0, 0, 0, elapsed / fadeTime);
            yield return null;
        }

        var textObj = new GameObject("BossText");
        textObj.transform.SetParent(overlay.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(800, 200);

        var tmpText = textObj.AddComponent<TextMeshProUGUI>();
        tmpText.text = Localization.Get("map_mansion_boss_enter");
        tmpText.fontSize = 36;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.fontStyle = FontStyles.Bold;
        tmpText.raycastTarget = false;

        yield return new WaitForSeconds(2.0f);

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
        if (menuOpen && menuPanel != null)
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

        menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(canvas.transform, false);
        var panelRect = menuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(350, 480);

        var panelBg = menuPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.18f, 0.95f);

        // タイトル
        var titleObj = new GameObject("MenuTitle");
        titleObj.transform.SetParent(menuPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -25);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "MENU";
        titleText.fontSize = 30;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.9f, 0.9f, 0.5f);
        titleText.raycastTarget = false;

        // ボタン群
        float btnY = -70;
        CreateMenuItemButton(menuPanel.transform, Localization.Get("map_menu_status"), btnY, () => { CloseMenu(); OpenStatusPanel(); });
        btnY -= 60;
        CreateMenuItemButton(menuPanel.transform, Localization.Get("map_menu_inventory"), btnY, () => { CloseMenu(); OpenInventoryPanel(); });
        btnY -= 60;
        CreateMenuItemButton(menuPanel.transform, Localization.Get("map_menu_home"), btnY, OnGoHome);
        btnY -= 60;
        CreateMenuItemButton(menuPanel.transform, Localization.Get("map_menu_save"), btnY, OnSave);
        btnY -= 60;
        CreateMenuItemButton(menuPanel.transform, Localization.Get("map_menu_title"), btnY, OnGoTitle);
        btnY -= 60;
        CreateMenuItemButton(menuPanel.transform, Localization.Get("map_menu_close"), btnY, () => CloseMenu());
    }

    void CreateMenuItemButton(Transform parent, string label, float yPos, UnityEngine.Events.UnityAction action)
    {
        var btnObj = new GameObject(label + "Button");
        btnObj.transform.SetParent(parent, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 1);
        btnRect.anchorMax = new Vector2(0.5f, 1);
        btnRect.anchoredPosition = new Vector2(0, yPos);
        btnRect.sizeDelta = new Vector2(260, 50);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.25f, 0.4f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.6f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.3f);
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

    void CloseMenu()
    {
        menuOpen = false;
        if (menuPanel != null)
        {
            Destroy(menuPanel);
            menuPanel = null;
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
        if (savePanel != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        savePanel = new GameObject("SavePanel");
        savePanel.transform.SetParent(canvas.transform, false);
        var panelRect = savePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 480);

        var panelBg = savePanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.18f, 0.95f);

        // タイトル
        var titleObj = new GameObject("SaveTitle");
        titleObj.transform.SetParent(savePanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -25);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = Localization.Get("map_save_title");
        titleText.fontSize = 24;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.9f, 0.9f, 0.5f);
        titleText.raycastTarget = false;

        // スロット一覧
        float slotY = -70;
        for (int i = 0; i < DataCarrier.MAX_SAVE_SLOTS; i++)
        {
            CreateSaveSlotEntry(i, slotY);
            slotY -= 70;
        }

        // とじる
        CreateMenuItemButton(savePanel.transform, Localization.Get("ui_close"), slotY - 10, () => CloseSavePanel());
    }

    void CreateSaveSlotEntry(int slot, float yPos)
    {
        bool exists = DataCarrier.SlotExists(slot);

        var slotObj = new GameObject($"SaveSlot{slot}");
        slotObj.transform.SetParent(savePanel.transform, false);
        var slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0.5f, 1);
        slotRect.anchorMax = new Vector2(0.5f, 1);
        slotRect.anchoredPosition = new Vector2(0, yPos);
        slotRect.sizeDelta = new Vector2(340, 60);

        var slotBg = slotObj.AddComponent<Image>();
        slotBg.color = exists ? new Color(0.2f, 0.25f, 0.35f) : new Color(0.15f, 0.15f, 0.2f);

        // スロット情報テキスト
        var infoObj = new GameObject("Info");
        infoObj.transform.SetParent(slotObj.transform, false);
        var infoRect = infoObj.AddComponent<RectTransform>();
        infoRect.anchorMin = Vector2.zero;
        infoRect.anchorMax = Vector2.one;
        infoRect.offsetMin = new Vector2(10, 5);
        infoRect.offsetMax = new Vector2(-10, -5);
        var infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.fontSize = 18;
        infoText.alignment = TextAlignmentOptions.MidlineLeft;
        infoText.color = Color.white;
        infoText.richText = true;
        infoText.raycastTarget = false;

        if (exists)
        {
            string babyName = DataCarrier.GetSlotBabyName(slot);
            int age = DataCarrier.GetSlotAge(slot);
            bool isGod = DataCarrier.GetSlotIsGodBaby(slot);
            string godMark = isGod ? " <color=#FFD700>★</color>" : "";
            infoText.text = $"{Localization.Get("map_save_slot", slot + 1)}{babyName}{godMark}  ({Localization.GetAge(age)})";
        }
        else
        {
            infoText.text = $"{Localization.Get("map_save_slot", slot + 1)}{Localization.Get("map_save_slot_empty")}";
        }

        // スロット全体をボタンにする
        var btn = slotObj.AddComponent<Button>();
        btn.targetGraphic = slotBg;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.55f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.3f);
        btn.colors = colors;

        int idx = slot;
        btn.onClick.AddListener(() => OnSaveSlotSelected(idx));
    }

    void OnSaveSlotSelected(int slot)
    {
        if (DataCarrier.SlotExists(slot))
        {
            // 上書き確認
            ShowOverwriteConfirm(slot);
        }
        else
        {
            // 空きスロット → そのままセーブ
            DoSaveToSlot(slot);
        }
    }

    void ShowOverwriteConfirm(int slot)
    {
        // セーブパネルの中身を消して確認UIに差し替え
        foreach (Transform child in savePanel.transform)
            Destroy(child.gameObject);

        string babyName = DataCarrier.GetSlotBabyName(slot);

        // 確認メッセージ
        var msgObj = new GameObject("ConfirmMsg");
        msgObj.transform.SetParent(savePanel.transform, false);
        var msgRect = msgObj.AddComponent<RectTransform>();
        msgRect.anchorMin = new Vector2(0, 0.5f);
        msgRect.anchorMax = new Vector2(1, 0.5f);
        msgRect.anchoredPosition = new Vector2(0, 40);
        msgRect.sizeDelta = new Vector2(-40, 100);
        var msgText = msgObj.AddComponent<TextMeshProUGUI>();
        msgText.text = Localization.Get("map_save_overwrite_msg", slot + 1, babyName);
        msgText.fontSize = 22;
        msgText.alignment = TextAlignmentOptions.Center;
        msgText.color = Color.white;
        msgText.richText = true;
        msgText.raycastTarget = false;

        // はい
        CreateMenuItemButton(savePanel.transform, Localization.Get("map_save_overwrite"), -30, () => DoSaveToSlot(slot));
        CreateMenuItemButton(savePanel.transform, Localization.Get("map_save_cancel"), -90, () => { CloseSavePanel(); OpenSavePanel(); });
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
        if (savePanel != null)
        {
            Destroy(savePanel);
            savePanel = null;
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
        // 既に持っていたら生成しない
        if (DataCarrier.Instance != null && DataCarrier.Instance.HasItem("金のたまご")) return;

        goldenEggObj = new GameObject("GoldenEgg");
        goldenEggObj.transform.SetParent(tilesContainer.transform, false);

        var rect = goldenEggObj.AddComponent<RectTransform>();
        float posX = (GOLDEN_EGG_X - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float posY = (GOLDEN_EGG_Y - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
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
        if (playerTileX != GOLDEN_EGG_X || playerTileY != GOLDEN_EGG_Y) return;

        // 金のたまごを取得
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.AddItem("金のたまご");
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
        menuOpen = true; // 入力無効化

        // 半透明黒背景
        var overlay = new GameObject("MilkCutinOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        var overlayBg = overlay.AddComponent<Image>();
        overlayBg.color = new Color(0, 0, 0, 0f);

        // フェードイン
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.3f;
            overlayBg.color = new Color(0, 0, 0, 0.6f * t);
            yield return null;
        }
        overlayBg.color = new Color(0, 0, 0, 0.6f);

        // カットインパネル（中央）
        var cutinPanel = new GameObject("CutinPanel");
        cutinPanel.transform.SetParent(overlay.transform, false);
        var cutinRect = cutinPanel.AddComponent<RectTransform>();
        cutinRect.anchorMin = new Vector2(0.5f, 0.5f);
        cutinRect.anchorMax = new Vector2(0.5f, 0.5f);
        cutinRect.anchoredPosition = new Vector2(0, 50);
        cutinRect.sizeDelta = new Vector2(500, 500);

        // 背景円（柔らかいピンク）
        var circle = FacePart("Circle", cutinPanel.transform, Vector2.zero, new Vector2(350, 350));
        circle.AddComponent<Image>().color = new Color(1f, 0.9f, 0.92f, 0.9f);

        // 大きい哺乳瓶
        var bottleContainer = new GameObject("BigBottle");
        bottleContainer.transform.SetParent(cutinPanel.transform, false);
        var bottleRect = bottleContainer.AddComponent<RectTransform>();
        bottleRect.anchoredPosition = Vector2.zero;
        bottleRect.sizeDelta = new Vector2(300, 300);
        DrawBabyBottle(bottleContainer.transform, 2.5f);

        // キラキラエフェクト
        Vector2[] sparklePositions = {
            new Vector2(-120, 100), new Vector2(130, 80), new Vector2(-80, -90),
            new Vector2(100, -60), new Vector2(0, 140)
        };
        foreach (var sp in sparklePositions)
        {
            var sparkle = FacePart("Sparkle", cutinPanel.transform, sp, new Vector2(16, 16));
            sparkle.AddComponent<Image>().color = new Color(1f, 1f, 0.7f, 0.8f);
        }

        // テキスト
        var textObj = new GameObject("MilkText");
        textObj.transform.SetParent(overlay.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0, -200);
        textRect.sizeDelta = new Vector2(800, 80);

        var textBg = new GameObject("TextBg");
        textBg.transform.SetParent(textObj.transform, false);
        var textBgRect = textBg.AddComponent<RectTransform>();
        textBgRect.anchorMin = Vector2.zero;
        textBgRect.anchorMax = Vector2.one;
        textBgRect.offsetMin = Vector2.zero;
        textBgRect.offsetMax = Vector2.zero;
        textBg.AddComponent<Image>().color = new Color(1f, 0.85f, 0.9f, 0.9f);

        var milkText = new GameObject("Text");
        milkText.transform.SetParent(textObj.transform, false);
        var milkTextRect = milkText.AddComponent<RectTransform>();
        milkTextRect.anchorMin = Vector2.zero;
        milkTextRect.anchorMax = Vector2.one;
        milkTextRect.offsetMin = new Vector2(20, 5);
        milkTextRect.offsetMax = new Vector2(-20, -5);

        var tmp = milkText.AddComponent<TextMeshProUGUI>();
        tmp.text = Localization.Get("map_milk_heal");
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.6f, 0.2f, 0.3f);
        tmp.fontStyle = FontStyles.Bold;

        // 2.5秒表示
        yield return new WaitForSeconds(2.5f);

        // フェードアウト
        elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.3f;
            overlayBg.color = new Color(0, 0, 0, 0.6f * (1 - t));
            yield return null;
        }

        Destroy(overlay);
        menuOpen = false;
        milkCutinActive = false;

        // ステータス更新
        UpdateStatusText();
    }

    // ===== ステータスパネル =====

    void OpenStatusPanel()
    {
        if (statusPanel != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        var dc = DataCarrier.Instance;
        if (dc == null) return;

        statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(canvas.transform, false);
        var panelRect = statusPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(400, 480);

        var panelBg = statusPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.18f, 0.95f);

        // タイトル
        var titleObj = new GameObject("StatusTitle");
        titleObj.transform.SetParent(statusPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -25);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = Localization.Get("map_status_title");
        titleText.fontSize = 28;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.9f, 0.9f, 0.5f);
        titleText.raycastTarget = false;

        // ステータス内容
        string genderColor = dc.babyGender == "男の子" ? "#00BFFF" : "#FF69B4";
        string traitColor = dc.trait1 == "覇王色" ? "#FF4500" : "#FFA500";
        string godLabel = dc.isGodBaby ? "  <color=#FFD700>★GOD BABY★</color>" : "";

        string content = $"<b>{dc.babyName}</b>{godLabel}\n" +
            $"<color={genderColor}>{Localization.GetGender(dc.babyGender)}</color>　　{Localization.GetAge(dc.babyAge)}\n" +
            "\n" +
            $"{Localization.Get("map_status_hp")} {dc.babyHp}　　{Localization.Get("map_status_atk")} {dc.babyAtk}　　{Localization.Get("map_status_def")} {dc.babyDef}\n" +
            $"{Localization.Get("map_status_academic")} {dc.babyAcademic}　　{Localization.Get("map_status_athletic")} {dc.babyAthletic}\n" +
            $"{Localization.Get("map_status_height")} {dc.babyHeight} cm　　{Localization.Get("map_status_weight")} {dc.babyWeight} g\n" +
            "\n" +
            $"{Localization.Get("map_status_trait")} <color={traitColor}>{Localization.GetTrait(dc.trait1)}</color>\n" +
            "\n" +
            $"{Localization.Get("map_status_father")} {dc.fatherName}　　{Localization.Get("map_status_mother")} {dc.motherName}\n" +
            "\n" +
            $"{Localization.Get("map_status_exp")} {dc.babyExp} / {DataCarrier.ExpForNextAge(dc.babyAge)}\n" +
            $"{Localization.Get("map_status_enemies")} {dc.defeatedEnemies}";

        var contentObj = new GameObject("StatusContent");
        contentObj.transform.SetParent(statusPanel.transform, false);
        var contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.anchoredPosition = new Vector2(0, -80);
        contentRect.sizeDelta = new Vector2(-40, 300);
        contentRect.pivot = new Vector2(0.5f, 1);
        var contentText = contentObj.AddComponent<TextMeshProUGUI>();
        contentText.text = content;
        contentText.fontSize = 22;
        contentText.alignment = TextAlignmentOptions.TopLeft;
        contentText.color = Color.white;
        contentText.richText = true;
        contentText.lineSpacing = 8;
        contentText.raycastTarget = false;

        // とじるボタン
        CreateMenuItemButton(statusPanel.transform, Localization.Get("ui_close"), -430, () => CloseStatusPanel());
    }

    void CloseStatusPanel()
    {
        if (statusPanel != null)
        {
            Destroy(statusPanel);
            statusPanel = null;
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
        if (inventoryPanel != null) return;
        menuOpen = true;
        SetTouchControlsVisible(false);

        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform, false);
        var panelRect = inventoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(350, 300);

        var panelBg = inventoryPanel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.08f, 0.18f, 0.95f);

        // タイトル
        var titleObj = new GameObject("InvTitle");
        titleObj.transform.SetParent(inventoryPanel.transform, false);
        var titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.anchoredPosition = new Vector2(0, -25);
        titleRect.sizeDelta = new Vector2(0, 50);
        var titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = Localization.Get("map_inventory_title");
        titleText.fontSize = 28;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(0.9f, 0.9f, 0.5f);
        titleText.raycastTarget = false;

        // アイテム一覧
        string[] items = DataCarrier.Instance != null ? DataCarrier.Instance.GetItemList() : new string[0];

        float itemY = -70;
        if (items.Length == 0)
        {
            var emptyObj = new GameObject("Empty");
            emptyObj.transform.SetParent(inventoryPanel.transform, false);
            var emptyRect = emptyObj.AddComponent<RectTransform>();
            emptyRect.anchorMin = new Vector2(0, 1);
            emptyRect.anchorMax = new Vector2(1, 1);
            emptyRect.anchoredPosition = new Vector2(0, itemY);
            emptyRect.sizeDelta = new Vector2(0, 40);
            var emptyText = emptyObj.AddComponent<TextMeshProUGUI>();
            emptyText.text = Localization.Get("map_inventory_empty");
            emptyText.fontSize = 22;
            emptyText.alignment = TextAlignmentOptions.Center;
            emptyText.color = new Color(0.6f, 0.6f, 0.6f);
            emptyText.raycastTarget = false;
        }
        else
        {
            foreach (var item in items)
            {
                var itemObj = new GameObject("Item_" + item);
                itemObj.transform.SetParent(inventoryPanel.transform, false);
                var itemRect = itemObj.AddComponent<RectTransform>();
                itemRect.anchorMin = new Vector2(0, 1);
                itemRect.anchorMax = new Vector2(1, 1);
                itemRect.anchoredPosition = new Vector2(0, itemY);
                itemRect.sizeDelta = new Vector2(0, 40);
                var itemText = itemObj.AddComponent<TextMeshProUGUI>();
                itemText.text = item == "金のたまご" ? "<color=#FFD700>★ " + Localization.Get("map_item_golden_egg") + "</color>" : item;
                itemText.fontSize = 22;
                itemText.alignment = TextAlignmentOptions.Center;
                itemText.color = Color.white;
                itemText.richText = true;
                itemText.raycastTarget = false;
                itemY -= 40;
            }
        }

        // とじるボタン
        CreateMenuItemButton(inventoryPanel.transform, Localization.Get("ui_close"), -240, () => CloseInventoryPanel());
    }

    void CloseInventoryPanel()
    {
        if (inventoryPanel != null)
        {
            Destroy(inventoryPanel);
            inventoryPanel = null;
            menuOpen = false;
            SetTouchControlsVisible(true);
        }
    }

    // ===== メッセージ表示 =====

    Coroutine messageCoroutine;
    void ShowMessage(string msg)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(msg));
    }

    IEnumerator ShowMessageCoroutine(string msg)
    {
        var msgBox = new GameObject("MessageBox");
        msgBox.transform.SetParent(canvas.transform, false);
        var boxRect = msgBox.AddComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0.5f, 0.5f);
        boxRect.anchorMax = new Vector2(0.5f, 0.5f);
        boxRect.anchoredPosition = new Vector2(0, -200);
        boxRect.sizeDelta = new Vector2(800, 90);

        var boxBg = msgBox.AddComponent<Image>();
        boxBg.color = new Color(0, 0, 0, 0.85f);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(msgBox.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 10);
        textRect.offsetMax = new Vector2(-20, -10);

        var text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = msg;
        text.fontSize = 24;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.richText = true;

        yield return new WaitForSeconds(2f);

        Destroy(msgBox);
    }

    // ===== タッチコントロール =====

    void CreateTouchControls()
    {
        if (canvas == null) return;

        var (safeLeft, safeRight, safeTop, safeBottom) = SafeAreaHelper.GetSafeAreaInsets(canvas);

        touchControlsObj = new GameObject("TouchControls");
        touchControlsObj.transform.SetParent(canvas.transform, false);
        var touchRect = touchControlsObj.AddComponent<RectTransform>();
        touchRect.anchorMin = Vector2.zero;
        touchRect.anchorMax = Vector2.one;
        touchRect.offsetMin = Vector2.zero;
        touchRect.offsetMax = Vector2.zero;

        // 縦画面9:16 (1080x1920): D-padを左下、アクションボタンを右下
        float dpadCenterX = -300 + safeLeft;
        float dpadCenterY = 180 + safeBottom;
        float dpadBtnSize = 100f;
        float dpadSpacing = 105f;

        // 上
        CreateDpadButton(touchControlsObj.transform, new Vector2(dpadCenterX, dpadCenterY + dpadSpacing),
            new Vector2(dpadBtnSize, dpadBtnSize), "\u25B2", 0, 1);
        // 下
        CreateDpadButton(touchControlsObj.transform, new Vector2(dpadCenterX, dpadCenterY - dpadSpacing),
            new Vector2(dpadBtnSize, dpadBtnSize), "\u25BC", 0, -1);
        // 左
        CreateDpadButton(touchControlsObj.transform, new Vector2(dpadCenterX - dpadSpacing, dpadCenterY),
            new Vector2(dpadBtnSize, dpadBtnSize), "\u25C0", -1, 0);
        // 右
        CreateDpadButton(touchControlsObj.transform, new Vector2(dpadCenterX + dpadSpacing, dpadCenterY),
            new Vector2(dpadBtnSize, dpadBtnSize), "\u25B6", 1, 0);

        // === 調べるボタン（右下） ===
        float rightBtnX = 300 - safeRight;
        float rightBtnBaseY = 140 + safeBottom;

        var interactBtn = CreateTouchButton(touchControlsObj.transform,
            new Vector2(rightBtnX, rightBtnBaseY),
            new Vector2(180, 90),
            Localization.Get("touch_interact"),
            new Color(0.3f, 0.6f, 0.4f));
        interactBtn.GetComponent<Button>().onClick.AddListener(() => { touchInteract = true; });

        // === メニューボタン（右下、調べるの上） ===
        var menuBtn = CreateTouchButton(touchControlsObj.transform,
            new Vector2(rightBtnX, rightBtnBaseY + 110),
            new Vector2(180, 90),
            Localization.Get("touch_menu"),
            new Color(0.4f, 0.35f, 0.55f));
        menuBtn.GetComponent<Button>().onClick.AddListener(() => { touchMenuPressed = true; });
    }

    void CreateDpadButton(Transform parent, Vector2 pos, Vector2 size, string label, int dx, int dy)
    {
        var btnObj = new GameObject($"Dpad_{label}");
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        var img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.3f, 0.7f);

        // EventTrigger でホールド移動対応
        var trigger = btnObj.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        pointerDown.callback.AddListener((_) => { touchDx = dx; touchDy = dy; });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        pointerUp.callback.AddListener((_) => { if (touchDx == dx && touchDy == dy) { touchDx = 0; touchDy = 0; } });
        trigger.triggers.Add(pointerUp);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }

    GameObject CreateTouchButton(Transform parent, Vector2 pos, Vector2 size, string label, Color bgColor)
    {
        var btnObj = new GameObject($"Touch_{label}");
        btnObj.transform.SetParent(parent, false);
        var rect = btnObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0);
        rect.anchorMax = new Vector2(0.5f, 0);
        rect.anchoredPosition = pos;
        rect.sizeDelta = size;

        var img = btnObj.AddComponent<Image>();
        img.color = new Color(bgColor.r, bgColor.g, bgColor.b, 0.7f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return btnObj;
    }

    void SetTouchControlsVisible(bool visible)
    {
        if (touchControlsObj != null)
            touchControlsObj.SetActive(visible);
    }
}
