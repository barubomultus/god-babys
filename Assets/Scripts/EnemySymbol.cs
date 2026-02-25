using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// マップ上の敵シンボル。巡回 → 発見(!) → 追跡 の3状態AI。
/// MapManager が生成し、毎フレーム Tick() を呼ぶ。
/// </summary>
public class EnemySymbol
{
    // --- 識別 ---
    public string enemyName;
    public Color bodyColor;

    // --- タイル位置 ---
    public int tileX, tileY;
    Vector2 targetPosition;
    public bool isMoving;

    // --- AI ---
    enum State { Wander, Alert, Chase }
    State state = State.Wander;
    float wanderTimer;
    float alertTimer;
    bool encounterTriggered;

    // --- 描画 ---
    public GameObject obj;
    RectTransform rect;
    GameObject alertIconObj;

    // --- 参照（Initialize で受け取る） ---
    float displayTile;

    // --- 定数 ---
    const float MOVE_SPEED = 5f;
    const float WANDER_INTERVAL_MIN = 1.5f;
    const float WANDER_INTERVAL_MAX = 3.5f;
    const int DETECT_RANGE = 4;
    const float ALERT_DURATION = 0.6f;
    const float CHASE_INTERVAL = 0.4f;

    // ===== 初期化 =====

    public void Initialize(string enemyName, Color bodyColor,
                           int startX, int startY,
                           Transform parent, float displayTile,
                           int mapWidth, int mapHeight)
    {
        this.enemyName = enemyName;
        this.bodyColor = bodyColor;
        this.tileX = startX;
        this.tileY = startY;
        this.displayTile = displayTile;

        wanderTimer = Random.Range(WANDER_INTERVAL_MIN, WANDER_INTERVAL_MAX);

        // GameObject 作成
        obj = new GameObject("Enemy_" + enemyName);
        obj.transform.SetParent(parent, false);

        rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(displayTile - 4, displayTile - 4);
        UpdatePosition(mapWidth, mapHeight);
        targetPosition = rect.anchoredPosition;

        // 背景 Image（透明、レイキャスト用）
        var bgImg = obj.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0);

        DrawSymbol(obj.transform);
    }

    // ===== 描画 =====

    void DrawSymbol(Transform parent)
    {
        float s = displayTile * 0.35f; // 基本サイズ

        // 体
        var bodyObj = CreatePart("Body", parent, new Vector2(0, -s * 0.3f), new Vector2(s * 0.7f, s * 0.8f));
        bodyObj.AddComponent<Image>().color = bodyColor * 0.85f;

        // 頭
        var headObj = CreatePart("Head", parent, new Vector2(0, s * 0.3f), new Vector2(s, s));
        headObj.AddComponent<Image>().color = bodyColor;

        // 左目
        var eyeLObj = CreatePart("EyeL", parent, new Vector2(-s * 0.18f, s * 0.35f), new Vector2(s * 0.22f, s * 0.22f));
        eyeLObj.AddComponent<Image>().color = Color.white;
        var pupilLObj = CreatePart("PupilL", parent, new Vector2(-s * 0.18f, s * 0.32f), new Vector2(s * 0.12f, s * 0.12f));
        pupilLObj.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

        // 右目
        var eyeRObj = CreatePart("EyeR", parent, new Vector2(s * 0.18f, s * 0.35f), new Vector2(s * 0.22f, s * 0.22f));
        eyeRObj.AddComponent<Image>().color = Color.white;
        var pupilRObj = CreatePart("PupilR", parent, new Vector2(s * 0.18f, s * 0.32f), new Vector2(s * 0.12f, s * 0.12f));
        pupilRObj.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

        // "!" アラートアイコン（非表示）
        alertIconObj = CreatePart("Alert", parent, new Vector2(0, s * 0.95f), new Vector2(s * 0.6f, s * 0.6f));
        var alertImg = alertIconObj.AddComponent<Image>();
        alertImg.color = new Color(1f, 0.2f, 0.2f, 0.9f);

        var alertTextObj = new GameObject("AlertText");
        alertTextObj.transform.SetParent(alertIconObj.transform, false);
        var alertRt = alertTextObj.AddComponent<RectTransform>();
        alertRt.anchorMin = Vector2.zero;
        alertRt.anchorMax = Vector2.one;
        alertRt.offsetMin = Vector2.zero;
        alertRt.offsetMax = Vector2.zero;
        var alertTmp = alertTextObj.AddComponent<TextMeshProUGUI>();
        alertTmp.text = "!";
        alertTmp.fontSize = s * 0.45f;
        alertTmp.fontStyle = FontStyles.Bold;
        alertTmp.color = Color.white;
        alertTmp.alignment = TextAlignmentOptions.Center;

        alertIconObj.SetActive(false);
    }

    GameObject CreatePart(string name, Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    // ===== 毎フレーム更新 =====

    public void Tick(int playerTileX, int playerTileY,
                     bool[,] walkable, int mapWidth, int mapHeight)
    {
        if (obj == null || encounterTriggered) return;

        // 移動アニメーション
        UpdateMovement();

        // AI
        int dist = Mathf.Abs(tileX - playerTileX) + Mathf.Abs(tileY - playerTileY);

        switch (state)
        {
            case State.Wander:
                wanderTimer -= Time.deltaTime;
                if (wanderTimer <= 0f)
                {
                    WanderStep(walkable, mapWidth, mapHeight);
                    wanderTimer = Random.Range(WANDER_INTERVAL_MIN, WANDER_INTERVAL_MAX);
                }
                if (dist <= DETECT_RANGE)
                {
                    state = State.Alert;
                    alertTimer = ALERT_DURATION;
                    if (alertIconObj != null) alertIconObj.SetActive(true);
                }
                break;

            case State.Alert:
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0f)
                {
                    state = State.Chase;
                    wanderTimer = CHASE_INTERVAL;
                    if (alertIconObj != null) alertIconObj.SetActive(false);
                }
                break;

            case State.Chase:
                wanderTimer -= Time.deltaTime;
                if (wanderTimer <= 0f && !isMoving)
                {
                    ChaseStep(playerTileX, playerTileY, walkable, mapWidth, mapHeight);
                    wanderTimer = CHASE_INTERVAL;
                }
                if (dist > DETECT_RANGE * 2)
                {
                    state = State.Wander;
                    wanderTimer = Random.Range(WANDER_INTERVAL_MIN, WANDER_INTERVAL_MAX);
                }
                break;
        }
    }

    // ===== 移動ロジック =====

    void WanderStep(bool[,] walkable, int mapW, int mapH)
    {
        if (isMoving) return;

        // ランダム方向
        int[] ddx = { 0, 0, -1, 1 };
        int[] ddy = { 1, -1, 0, 0 };
        int start = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            int dir = (start + i) % 4;
            int nx = tileX + ddx[dir];
            int ny = tileY + ddy[dir];
            if (nx >= 0 && nx < mapW && ny >= 0 && ny < mapH && walkable[nx, ny])
            {
                MoveTo(nx, ny, mapW, mapH);
                return;
            }
        }
    }

    void ChaseStep(int playerX, int playerY, bool[,] walkable, int mapW, int mapH)
    {
        if (isMoving) return;

        int ddx = playerX - tileX;
        int ddy = playerY - tileY;

        // 主軸方向を優先（貪欲法）
        int dx1, dy1, dx2, dy2;
        if (Mathf.Abs(ddx) >= Mathf.Abs(ddy))
        {
            dx1 = ddx > 0 ? 1 : -1; dy1 = 0;
            dx2 = 0; dy2 = ddy > 0 ? 1 : (ddy < 0 ? -1 : 0);
        }
        else
        {
            dx1 = 0; dy1 = ddy > 0 ? 1 : -1;
            dx2 = ddx > 0 ? 1 : (ddx < 0 ? -1 : 0); dy2 = 0;
        }

        // 主軸方向
        int nx = tileX + dx1;
        int ny = tileY + dy1;
        if (nx >= 0 && nx < mapW && ny >= 0 && ny < mapH && walkable[nx, ny])
        {
            MoveTo(nx, ny, mapW, mapH);
            return;
        }

        // 副軸方向
        if (dx2 != 0 || dy2 != 0)
        {
            nx = tileX + dx2;
            ny = tileY + dy2;
            if (nx >= 0 && nx < mapW && ny >= 0 && ny < mapH && walkable[nx, ny])
            {
                MoveTo(nx, ny, mapW, mapH);
                return;
            }
        }
    }

    void MoveTo(int nx, int ny, int mapW, int mapH)
    {
        tileX = nx;
        tileY = ny;
        float posX = (tileX - mapW / 2f + 0.5f) * displayTile;
        float posY = (tileY - mapH / 2f + 0.5f) * displayTile;
        targetPosition = new Vector2(posX, posY);
        isMoving = true;
    }

    void UpdateMovement()
    {
        if (!isMoving || rect == null) return;

        rect.anchoredPosition = Vector2.MoveTowards(
            rect.anchoredPosition,
            targetPosition,
            MOVE_SPEED * displayTile * Time.deltaTime
        );

        if (Vector2.Distance(rect.anchoredPosition, targetPosition) < 0.5f)
        {
            rect.anchoredPosition = targetPosition;
            isMoving = false;
        }
    }

    void UpdatePosition(int mapW, int mapH)
    {
        float posX = (tileX - mapW / 2f + 0.5f) * displayTile;
        float posY = (tileY - mapH / 2f + 0.5f) * displayTile;
        rect.anchoredPosition = new Vector2(posX, posY);
    }

    // ===== 接触判定 =====

    public bool IsContactWith(int playerTileX, int playerTileY)
    {
        if (encounterTriggered) return false;
        return tileX == playerTileX && tileY == playerTileY;
    }

    // ===== 表示制御 =====

    public void SetActive(bool active)
    {
        if (obj != null) obj.SetActive(active);
        encounterTriggered = !active;
    }

    public void Destroy()
    {
        if (obj != null)
        {
            Object.Destroy(obj);
            obj = null;
        }
    }
}
