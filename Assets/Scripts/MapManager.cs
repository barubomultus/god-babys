using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("UI Reference")]
    public Canvas canvas;

    // タイルサイズ
    const int TILE_SIZE = 32;
    const int MAP_WIDTH = 20;
    const int MAP_HEIGHT = 15;

    // 表示スケール（UI上でのタイルの大きさ）
    const float DISPLAY_SCALE = 2.5f;
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

    // マップデータ
    int[,] mapData;
    bool[,] walkable;

    // プレイヤー
    GameObject playerObj;
    RectTransform playerRect;
    int playerTileX = 10;
    int playerTileY = 7;
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

    // メニュー
    GameObject menuPanel;
    bool menuOpen = false;

    // エンカウント
    float encounterCooldown = 0f;
    int stepCount = 0;

    // 金のたまご
    GameObject goldenEggObj;
    const int GOLDEN_EGG_X = 12;
    const int GOLDEN_EGG_Y = 11;

    // 持ち物パネル
    GameObject inventoryPanel;
    GameObject statusPanel;
    GameObject savePanel;

    void Start()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        // DataCarrierからマップ位置を復元
        if (DataCarrier.Instance != null)
        {
            playerTileX = DataCarrier.Instance.mapPlayerX;
            playerTileY = DataCarrier.Instance.mapPlayerY;
        }

        LoadTileset();
        GenerateMapData();
        CreateMapUI();
        CreatePlayer();
        CreateStatusUI();
        CreateMenuButton();
        CreateGoldenEgg();

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

        // === 道（メインストリート + サブ） ===
        // 横道（メインストリート）
        for (int x = 1; x < MAP_WIDTH - 1; x++)
        {
            mapData[x, 7] = TILE_PATH;
            mapData[x, 8] = TILE_PATH;
        }
        // 縦道（村の中央）
        for (int y = 2; y < MAP_HEIGHT - 1; y++)
        {
            mapData[10, y] = TILE_PATH;
        }
        // サブ通路
        for (int y = 3; y <= 6; y++)
        {
            mapData[5, y] = TILE_PATH;
        }
        // 館への接続道
        for (int y = 8; y <= 9; y++)
        {
            mapData[16, y] = TILE_PATH;
        }

        // === 池（左上エリア） ===
        for (int x = 2; x <= 4; x++)
        {
            for (int y = 10; y <= 12; y++)
            {
                mapData[x, y] = TILE_WATER;
                walkable[x, y] = false;
            }
        }

        // === 家を配置 ===
        // 赤い家（左側）
        mapData[3, 5] = TILE_HOUSE_RED;
        walkable[3, 5] = false;
        mapData[4, 5] = TILE_HOUSE_RED;
        walkable[4, 5] = false;
        mapData[3, 6] = TILE_HOUSE_RED;
        walkable[3, 6] = false;
        mapData[4, 6] = TILE_HOUSE_RED;
        walkable[4, 6] = false;

        // 緑の家（右上）
        mapData[13, 4] = TILE_HOUSE_GREEN;
        walkable[13, 4] = false;
        mapData[14, 4] = TILE_HOUSE_GREEN;
        walkable[14, 4] = false;
        mapData[13, 5] = TILE_HOUSE_GREEN;
        walkable[13, 5] = false;
        mapData[14, 5] = TILE_HOUSE_GREEN;
        walkable[14, 5] = false;

        // 青い家（左下）
        mapData[7, 3] = TILE_HOUSE_BLUE;
        walkable[7, 3] = false;
        mapData[8, 3] = TILE_HOUSE_BLUE;
        walkable[8, 3] = false;
        mapData[7, 2] = TILE_HOUSE_BLUE;
        walkable[7, 2] = false;
        mapData[8, 2] = TILE_HOUSE_BLUE;
        walkable[8, 2] = false;

        // === 森（上端、下端の装飾） ===
        for (int x = 0; x < MAP_WIDTH; x++)
        {
            mapData[x, 0] = TILE_TREE;
            walkable[x, 0] = false;
            mapData[x, MAP_HEIGHT - 1] = TILE_TREE;
            walkable[x, MAP_HEIGHT - 1] = false;
        }
        // 左右の端
        for (int y = 0; y < MAP_HEIGHT; y++)
        {
            mapData[0, y] = TILE_TREE;
            walkable[0, y] = false;
            mapData[MAP_WIDTH - 1, y] = TILE_TREE;
            walkable[MAP_WIDTH - 1, y] = false;
        }

        // === ボスの館（右奥） ===
        // 館の周辺を暗い土で囲む
        for (int x = 14; x <= 18; x++)
        {
            for (int y = 10; y <= 13; y++)
            {
                mapData[x, y] = TILE_DARK_DIRT;
            }
        }
        // 館本体（3x3）
        for (int x = 15; x <= 17; x++)
        {
            for (int y = 11; y <= 13; y++)
            {
                mapData[x, y] = TILE_BOSS_MANSION;
                walkable[x, y] = false;
            }
        }
        // 館の門（正面下、入口タイル）
        mapData[16, 10] = TILE_BOSS_GATE;
        walkable[16, 10] = false;
        // 周辺の柵
        mapData[14, 10] = TILE_FENCE;
        walkable[14, 10] = false;
        mapData[18, 10] = TILE_FENCE;
        walkable[18, 10] = false;
        mapData[14, 11] = TILE_FENCE;
        walkable[14, 11] = false;
        mapData[14, 12] = TILE_FENCE;
        walkable[14, 12] = false;
        mapData[14, 13] = TILE_FENCE;
        walkable[14, 13] = false;
        // === 岩 ===
        mapData[3, 9] = TILE_ROCK;
        walkable[3, 9] = false;

        // === 花畑 ===
        mapData[6, 10] = TILE_FLOWER;
        mapData[7, 10] = TILE_FLOWER;
        mapData[6, 11] = TILE_FLOWER;
        mapData[7, 11] = TILE_FLOWER;
        mapData[8, 10] = TILE_FLOWER;
        mapData[8, 11] = TILE_FLOWER;

        // 点在する花
        mapData[2, 3] = TILE_FLOWER;
        mapData[11, 2] = TILE_FLOWER;
        mapData[14, 9] = TILE_FLOWER;

        // === 柵（家の庭） ===
        for (int x = 2; x <= 5; x++)
        {
            mapData[x, 4] = TILE_FENCE;
            walkable[x, 4] = false;
        }
        for (int x = 12; x <= 15; x++)
        {
            mapData[x, 3] = TILE_FENCE;
            walkable[x, 3] = false;
        }

        // プレイヤーの初期位置は必ず歩けるようにする
        walkable[playerTileX, playerTileY] = true;
        if (mapData[playerTileX, playerTileY] != TILE_PATH && mapData[playerTileX, playerTileY] != TILE_GRASS)
        {
            mapData[playerTileX, playerTileY] = TILE_PATH;
        }
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
        bg.color = new Color(0.15f, 0.35f, 0.15f);
        bg.raycastTarget = false;

        // タイルコンテナ
        tilesContainer = new GameObject("TilesContainer");
        tilesContainer.transform.SetParent(mapPanel.transform, false);
        tilesContainerRect = tilesContainer.AddComponent<RectTransform>();
        tilesContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        tilesContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        tilesContainerRect.anchoredPosition = Vector2.zero;
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

        // 館本体（3x3タイル分の大きさ）中央: (16, 12)
        float centerX = (16 - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float centerY = (12 - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;

        var mansion = new GameObject("BossMansion");
        mansion.transform.SetParent(tilesContainer.transform, false);
        var mansionRect = mansion.AddComponent<RectTransform>();
        mansionRect.anchoredPosition = new Vector2(centerX, centerY);
        mansionRect.sizeDelta = new Vector2(DISPLAY_TILE * 3, DISPLAY_TILE * 3);

        // 館の壁
        var wall = FacePart("Wall", mansion.transform, Vector2.zero, new Vector2(DISPLAY_TILE * 2.8f, DISPLAY_TILE * 2.6f));
        wall.AddComponent<Image>().color = new Color(0.2f, 0.07f, 0.1f);

        // 屋根（三角形風）
        var roof = FacePart("Roof", mansion.transform, new Vector2(0, DISPLAY_TILE * 1.0f), new Vector2(DISPLAY_TILE * 3.0f, DISPLAY_TILE * 0.8f));
        roof.AddComponent<Image>().color = new Color(0.35f, 0.05f, 0.08f);

        // 屋根の先端
        var roofTop = FacePart("RoofTop", mansion.transform, new Vector2(0, DISPLAY_TILE * 1.45f), new Vector2(DISPLAY_TILE * 1.8f, DISPLAY_TILE * 0.5f));
        roofTop.AddComponent<Image>().color = new Color(0.4f, 0.05f, 0.05f);

        // 窓（左）
        var winL = FacePart("WinL", mansion.transform, new Vector2(-DISPLAY_TILE * 0.55f, DISPLAY_TILE * 0.15f), new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.5f));
        winL.AddComponent<Image>().color = new Color(0.9f, 0.7f, 0.2f, 0.8f);

        // 窓（右）
        var winR = FacePart("WinR", mansion.transform, new Vector2(DISPLAY_TILE * 0.55f, DISPLAY_TILE * 0.15f), new Vector2(DISPLAY_TILE * 0.5f, DISPLAY_TILE * 0.5f));
        winR.AddComponent<Image>().color = new Color(0.9f, 0.7f, 0.2f, 0.8f);

        // ドア
        var door = FacePart("Door", mansion.transform, new Vector2(0, -DISPLAY_TILE * 0.7f), new Vector2(DISPLAY_TILE * 0.6f, DISPLAY_TILE * 0.9f));
        door.AddComponent<Image>().color = new Color(0.35f, 0.15f, 0.1f);

        // ドアノブ
        var knob = FacePart("Knob", mansion.transform, new Vector2(DISPLAY_TILE * 0.12f, -DISPLAY_TILE * 0.7f), new Vector2(8, 8));
        knob.AddComponent<Image>().color = new Color(0.8f, 0.65f, 0.2f);

        // 「ボスのやかた」看板
        var signObj = new GameObject("BossSign");
        signObj.transform.SetParent(tilesContainer.transform, false);
        var signRect = signObj.AddComponent<RectTransform>();
        float signX = (16 - MAP_WIDTH / 2f + 0.5f) * DISPLAY_TILE;
        float signY = (10 - MAP_HEIGHT / 2f + 0.5f) * DISPLAY_TILE;
        signRect.anchoredPosition = new Vector2(signX, signY - DISPLAY_TILE * 0.1f);
        signRect.sizeDelta = new Vector2(DISPLAY_TILE * 2.2f, DISPLAY_TILE * 0.55f);

        var signBg = signObj.AddComponent<Image>();
        signBg.color = new Color(0.15f, 0.05f, 0.05f, 0.9f);
        signBg.raycastTarget = false;

        var signTextObj = new GameObject("SignText");
        signTextObj.transform.SetParent(signObj.transform, false);
        var signTextRect = signTextObj.AddComponent<RectTransform>();
        signTextRect.anchorMin = Vector2.zero;
        signTextRect.anchorMax = Vector2.one;
        signTextRect.offsetMin = Vector2.zero;
        signTextRect.offsetMax = Vector2.zero;

        var signText = signTextObj.AddComponent<TextMeshProUGUI>();
        signText.text = Localization.Get("map_boss_sign");
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

        // ベースカラー: 深めの緑にタイルごとの微妙な色ムラ
        float hueShift = Random.Range(-0.02f, 0.02f);
        float brightShift = Random.Range(-0.04f, 0.04f);
        Color baseColor = new Color(0.28f + hueShift, 0.62f + brightShift, 0.25f + hueShift);
        baseImg.color = baseColor;

        // グラデーションオーバーレイ（上部を少し明るく）
        var gradTop = FacePart("GradTop", parent, new Vector2(0, DISPLAY_TILE * 0.2f),
            new Vector2(DISPLAY_TILE, DISPLAY_TILE * 0.5f));
        var gradImg = gradTop.AddComponent<Image>();
        gradImg.color = new Color(0.4f, 0.75f, 0.35f, 0.25f);
        gradImg.raycastTarget = false;

        // 下部の影
        var gradBot = FacePart("GradBot", parent, new Vector2(0, -DISPLAY_TILE * 0.25f),
            new Vector2(DISPLAY_TILE, DISPLAY_TILE * 0.4f));
        var gradBotImg = gradBot.AddComponent<Image>();
        gradBotImg.color = new Color(0.1f, 0.3f, 0.1f, 0.15f);
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
            bladeImg.color = new Color(0.45f, 0.82f, 0.38f, bladeAlpha);
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
        playerRect.sizeDelta = new Vector2(DISPLAY_TILE - 8, DISPLAY_TILE - 8);

        playerImage = playerObj.AddComponent<Image>();
        playerImage.raycastTarget = false;

        // BattleManagerのパターンを再利用：親名からスプライト取得
        if (!TryShowBabySprite())
        {
            // フォールバック: 手描き風の赤ちゃん顔を生成
            playerImage.enabled = false;
            GenerateBabyFace(playerObj.transform);
        }

        playerObj.transform.SetAsLastSibling();
        targetPosition = playerRect.anchoredPosition;
    }

    bool TryShowBabySprite()
    {
        if (DataCarrier.Instance == null) return false;

        string fatherName = GetParentImageName(DataCarrier.Instance.fatherName);
        string motherName = GetParentImageName(DataCarrier.Instance.motherName);
        string genderKey = DataCarrier.Instance.babyGender == "男の子" ? "male" : "female";

        string babyImagePath = $"babys/{fatherName}_{motherName}_{genderKey}";
        Sprite babySprite = Resources.Load<Sprite>(babyImagePath);

        if (babySprite != null)
        {
            playerImage.sprite = babySprite;
            playerImage.color = Color.white;
            playerImage.preserveAspect = true;
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
            default: return japaneseName != null ? japaneseName.ToLower() : "";
        }
    }

    // マップ用の赤ちゃん顔生成（小さいタイル向け）
    void GenerateBabyFace(Transform parent)
    {
        string gender = "男の子";
        bool godBaby = false;
        int academic = 60;
        int athletic = 60;
        int atk = 50;

        if (DataCarrier.Instance != null)
        {
            gender = DataCarrier.Instance.babyGender;
            godBaby = DataCarrier.Instance.isGodBaby;
            academic = DataCarrier.Instance.babyAcademic;
            athletic = DataCarrier.Instance.babyAthletic;
            atk = DataCarrier.Instance.babyAtk;
        }

        bool isFemale = gender == "女の子";
        float s = 0.38f; // マップ用の小さいスケール

        // 肌色
        Color skin = new Color(0.98f, 0.89f, 0.82f);
        Color skinShadow = new Color(skin.r * 0.85f, skin.g * 0.82f, skin.b * 0.8f);

        // 髪色
        Color[] hairTones = { new Color(0.08f, 0.06f, 0.05f), new Color(0.2f, 0.12f, 0.08f), new Color(0.35f, 0.22f, 0.12f), new Color(0.55f, 0.38f, 0.2f) };
        Color hair = hairTones[Mathf.Clamp(academic / 25, 0, 3)];

        // GOD BABYオーラ
        if (godBaby)
        {
            var aura = FacePart("Aura", parent, Vector2.zero, new Vector2(140 * s, 160 * s));
            aura.AddComponent<Image>().color = new Color(1f, 0.85f, 0.2f, 0.15f);
        }

        // 顔ベース
        FacePart("Face", parent, Vector2.zero, new Vector2(100 * s, 120 * s)).AddComponent<Image>().color = skin;

        // 髪
        FacePart("Hair", parent, new Vector2(0, 45 * s), new Vector2(108 * s, 48 * s)).AddComponent<Image>().color = hair;
        FacePart("TopHair", parent, new Vector2(0, 62 * s), new Vector2(85 * s, 25 * s)).AddComponent<Image>().color = hair;

        // 目
        CreateMapEye(parent, -16 * s, 6 * s, s, isFemale);
        CreateMapEye(parent, 16 * s, 6 * s, s, isFemale);

        // 眉
        float browAngle = (atk - 50) * 0.15f;
        var browL = FacePart("BrowL", parent, new Vector2(-18 * s, 24 * s), new Vector2(20 * s, 4 * s));
        browL.transform.localRotation = Quaternion.Euler(0, 0, browAngle);
        browL.AddComponent<Image>().color = new Color(hair.r * 0.7f, hair.g * 0.7f, hair.b * 0.7f);
        var browR = FacePart("BrowR", parent, new Vector2(18 * s, 24 * s), new Vector2(20 * s, 4 * s));
        browR.transform.localRotation = Quaternion.Euler(0, 0, -browAngle);
        browR.AddComponent<Image>().color = new Color(hair.r * 0.7f, hair.g * 0.7f, hair.b * 0.7f);

        // 口
        float smile = athletic / 100f;
        Color lip = godBaby ? new Color(0.85f, 0.35f, 0.4f) : new Color(0.82f, 0.55f, 0.55f);
        float mw = (18 + smile * 10) * s;
        FacePart("Mouth", parent, new Vector2(0, -26 * s), new Vector2(mw, 5 * s)).AddComponent<Image>().color = lip;

        // ほっぺ
        float cheekAlpha = isFemale ? 0.35f : 0.15f;
        Color cheekC = isFemale ? new Color(1f, 0.5f, 0.55f, cheekAlpha) : new Color(1f, 0.7f, 0.7f, cheekAlpha);
        FacePart("CheekL", parent, new Vector2(-25 * s, -10 * s), new Vector2(18 * s, 14 * s)).AddComponent<Image>().color = cheekC;
        FacePart("CheekR", parent, new Vector2(25 * s, -10 * s), new Vector2(18 * s, 14 * s)).AddComponent<Image>().color = cheekC;

        // 耳
        FacePart("EarL", parent, new Vector2(-48 * s, 4 * s), new Vector2(10 * s, 18 * s)).AddComponent<Image>().color = skin;
        FacePart("EarR", parent, new Vector2(48 * s, 4 * s), new Vector2(10 * s, 18 * s)).AddComponent<Image>().color = skin;
    }

    void CreateMapEye(Transform parent, float x, float y, float s, bool isFemale)
    {
        float sz = 8 * s;
        // 白目
        FacePart("EyeW", parent, new Vector2(x, y), new Vector2(sz * 1.4f, sz)).AddComponent<Image>().color = new Color(0.95f, 0.95f, 0.97f);
        // 虹彩
        Color iris = new Color(0.18f, 0.12f, 0.08f);
        FacePart("Iris", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.8f, sz * 0.8f)).AddComponent<Image>().color = iris;
        // 瞳孔
        FacePart("Pupil", parent, new Vector2(x, y - 0.5f * s), new Vector2(sz * 0.3f, sz * 0.3f)).AddComponent<Image>().color = new Color(0.02f, 0.02f, 0.02f);
        // ハイライト
        FacePart("HL", parent, new Vector2(x - sz * 0.1f, y + sz * 0.1f), new Vector2(sz * 0.2f, sz * 0.2f)).AddComponent<Image>().color = Color.white;

        // まつげ（女の子）
        if (isFemale)
        {
            var lash = FacePart("Lash", parent, new Vector2(x, y + sz * 0.5f), new Vector2(sz * 1.2f, 2f * s));
            lash.AddComponent<Image>().color = new Color(0.1f, 0.08f, 0.06f);
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

        // ステータスパネル（上部）
        var statusPanel = new GameObject("StatusPanel");
        statusPanel.transform.SetParent(canvas.transform, false);
        var statusRect = statusPanel.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 1);
        statusRect.anchorMax = new Vector2(1, 1);
        statusRect.pivot = new Vector2(0.5f, 1);
        statusRect.anchoredPosition = Vector2.zero;
        statusRect.sizeDelta = new Vector2(0, 70);

        var statusBg = statusPanel.AddComponent<Image>();
        statusBg.color = new Color(0.05f, 0.05f, 0.15f, 0.85f);
        statusBg.raycastTarget = false;

        var textObj = new GameObject("StatusText");
        textObj.transform.SetParent(statusPanel.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20, 5);
        textRect.offsetMax = new Vector2(-120, -5);

        statusText = textObj.AddComponent<TextMeshProUGUI>();
        statusText.fontSize = 24;
        statusText.alignment = TextAlignmentOptions.Left;
        statusText.color = Color.white;
        statusText.raycastTarget = false;
        statusText.richText = true;

        UpdateStatusText();

        // 操作説明（下部）
        var helpPanel = new GameObject("HelpPanel");
        helpPanel.transform.SetParent(canvas.transform, false);
        var helpRect = helpPanel.AddComponent<RectTransform>();
        helpRect.anchorMin = new Vector2(0, 0);
        helpRect.anchorMax = new Vector2(1, 0);
        helpRect.pivot = new Vector2(0.5f, 0);
        helpRect.anchoredPosition = Vector2.zero;
        helpRect.sizeDelta = new Vector2(0, 40);

        var helpBg = helpPanel.AddComponent<Image>();
        helpBg.color = new Color(0, 0, 0, 0.5f);
        helpBg.raycastTarget = false;

        var helpTextObj = new GameObject("HelpText");
        helpTextObj.transform.SetParent(helpPanel.transform, false);
        var helpTextRect = helpTextObj.AddComponent<RectTransform>();
        helpTextRect.anchorMin = Vector2.zero;
        helpTextRect.anchorMax = Vector2.one;
        helpTextRect.offsetMin = new Vector2(10, 5);
        helpTextRect.offsetMax = new Vector2(-10, -5);

        var helpText = helpTextObj.AddComponent<TextMeshProUGUI>();
        helpText.text = Localization.Get("map_help_text");
        helpText.fontSize = 18;
        helpText.alignment = TextAlignmentOptions.Center;
        helpText.color = new Color(0.8f, 0.8f, 0.8f);
        helpText.raycastTarget = false;
    }

    void CreateMenuButton()
    {
        if (canvas == null) return;

        // 右上のメニューボタン
        var btnObj = new GameObject("MenuButton");
        btnObj.transform.SetParent(canvas.transform, false);
        var btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-15, -12);
        btnRect.sizeDelta = new Vector2(90, 46);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.3f, 0.3f, 0.45f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(ToggleMenu);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        var textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "MENU";
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.raycastTarget = false;
    }

    void UpdateStatusText()
    {
        if (statusText == null) return;

        string babyName = DataCarrier.Instance?.babyName ?? "???";
        int age = DataCarrier.Instance?.babyAge ?? 0;
        int hp = DataCarrier.Instance?.babyHp ?? 100;
        int atk = DataCarrier.Instance?.babyAtk ?? 10;
        int def = DataCarrier.Instance?.babyDef ?? 5;
        bool isGod = DataCarrier.Instance != null && DataCarrier.Instance.isGodBaby;

        string nameColor = isGod ? "<color=#FFD700>" : "<color=#FFFFFF>";
        string godLabel = isGod ? " <color=#FFD700>GOD BABY</color>" : "";

        statusText.text = $"{nameColor}{babyName}</color>  {Localization.GetAge(age)}{godLabel}\n" +
                          $"HP:<color=#00FF00>{hp}</color>  ATK:<color=#FF6666>{atk}</color>  DEF:<color=#6699FF>{def}</color>";
    }

    void Update()
    {
        // ESCでパネルを閉じる
        if (savePanel != null)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
                CloseSavePanel();
            return;
        }
        if (statusPanel != null)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
                CloseStatusPanel();
            return;
        }
        if (inventoryPanel != null)
        {
            var kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
                CloseInventoryPanel();
            return;
        }

        if (menuOpen) return;

        HandleInput();
        UpdateMovement();
    }

    void HandleInput()
    {
        if (isMoving) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        int dx = 0, dy = 0;

        if (kb.upArrowKey.isPressed || kb.wKey.isPressed)
            dy = 1;
        else if (kb.downArrowKey.isPressed || kb.sKey.isPressed)
            dy = -1;
        else if (kb.leftArrowKey.isPressed || kb.aKey.isPressed)
            dx = -1;
        else if (kb.rightArrowKey.isPressed || kb.dKey.isPressed)
            dx = 1;

        if (dx != 0 || dy != 0)
        {
            TryMove(dx, dy);
        }

        if (kb.spaceKey.wasPressedThisFrame)
        {
            Interact();
        }

        if (kb.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    void TryMove(int dx, int dy)
    {
        int newX = playerTileX + dx;
        int newY = playerTileY + dy;

        if (newX < 0 || newX >= MAP_WIDTH || newY < 0 || newY >= MAP_HEIGHT)
            return;

        // ボスの門に歩いて入ろうとした場合 → ボス戦へ
        if (mapData[newX, newY] == TILE_BOSS_GATE)
        {
            StartCoroutine(EnterBossMansion());
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

            // アイテム拾得判定
            CheckItemPickup();

            // 移動完了時にエンカウント判定
            CheckRandomEncounter();
        }
    }

    void CheckRandomEncounter()
    {
        // 草タイルの上でランダムエンカウント（移動完了時に判定）
        if (mapData[playerTileX, playerTileY] == TILE_GRASS ||
            mapData[playerTileX, playerTileY] == TILE_FLOWER)
        {
            // 花畑は4%、草は2%
            float rate = mapData[playerTileX, playerTileY] == TILE_FLOWER ? 0.04f : 0.02f;

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
        textRect.sizeDelta = new Vector2(600, 100);

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
        // ボスの館の門に隣接している場合 → ボス戦へ
        if (IsAdjacentTo(TILE_BOSS_GATE) || IsAdjacentTo(TILE_BOSS_MANSION))
        {
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
        textRect.sizeDelta = new Vector2(700, 200);

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

        menuPanel = new GameObject("MenuPanel");
        menuPanel.transform.SetParent(canvas.transform, false);
        var panelRect = menuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(350, 420);

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
        var eggBody = FacePart("EggBody", goldenEggObj.transform, new Vector2(0, -2), new Vector2(30, 38));
        eggBody.AddComponent<Image>().color = new Color(1f, 0.84f, 0f);

        // 上部ハイライト
        var highlight = FacePart("Highlight", goldenEggObj.transform, new Vector2(-4, 6), new Vector2(10, 14));
        highlight.AddComponent<Image>().color = new Color(1f, 1f, 0.7f, 0.7f);

        // 輝きエフェクト（小さい星）
        var sparkle = FacePart("Sparkle", goldenEggObj.transform, new Vector2(8, 12), new Vector2(6, 6));
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

    // ===== ステータスパネル =====

    void OpenStatusPanel()
    {
        if (statusPanel != null) return;
        menuOpen = true;

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
        }
    }

    // ===== 持ち物パネル =====

    void OpenInventoryPanel()
    {
        if (inventoryPanel != null) return;
        menuOpen = true;

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
        boxRect.anchorMin = new Vector2(0.5f, 0.15f);
        boxRect.anchorMax = new Vector2(0.5f, 0.15f);
        boxRect.anchoredPosition = Vector2.zero;
        boxRect.sizeDelta = new Vector2(500, 80);

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
}
