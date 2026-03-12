using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// マップ上のプレイヤー移動を制御する。
/// キーボード / スワイプ / タップ（A*パスファインディング）に対応。
/// MapManager が生成し、Update() から Tick() を呼ぶ。
/// </summary>
public class PlayerMovementController
{
    // --- 外部参照（Initialize で受け取る） ---
    System.Func<int, int, bool> tryMove;       // MapManager.TryMove → 移動成功なら true
    System.Func<int, int, bool> isWalkable;    // walkable チェック
    System.Func<Vector2Int> getPlayerTile;     // 現在のタイル座標
    System.Func<bool> getIsMoving;             // isMoving 状態
    int mapWidth, mapHeight;
    float displayTile;
    RectTransform tilesContainerRect;
    Transform tilesContainerParent;            // マーカー親

    // --- ロック ---
    public bool IsLocked { get; set; }

    // --- スワイプ状態 ---
    Vector2 touchStartPos;
    bool isTouchDragging;
    bool touchIsSwipe;
    float touchDownTime;
    int swipeDx, swipeDy;

    // --- タップ移動（パスキュー） ---
    Queue<Vector2Int> pathQueue = new Queue<Vector2Int>();

    // --- 慣性 ---
    int inertiaDx, inertiaDy;
    int inertiaSteps;

    // --- マーカー ---
    GameObject markerObj;
    Image markerImage;
    Vector2Int markerTargetTile;
    float markerTimer;

    // --- 定数 ---
    const float SWIPE_THRESHOLD = 30f;
    const float TAP_TIME_LIMIT = 0.25f;
    const float TAP_DIST_LIMIT = 20f;
    const float MARKER_LIFETIME = 2.5f;
    const int INERTIA_TILES = 1;

    bool initialized;

    // ===== 初期化 =====

    public void Initialize(
        System.Func<int, int, bool> tryMove,
        System.Func<int, int, bool> isWalkable,
        System.Func<Vector2Int> getPlayerTile,
        System.Func<bool> getIsMoving,
        int mapWidth, int mapHeight, float displayTile,
        RectTransform tilesContainerRect,
        Transform tilesContainerParent)
    {
        this.tryMove = tryMove;
        this.isWalkable = isWalkable;
        this.getPlayerTile = getPlayerTile;
        this.getIsMoving = getIsMoving;
        this.mapWidth = mapWidth;
        this.mapHeight = mapHeight;
        this.displayTile = displayTile;
        this.tilesContainerRect = tilesContainerRect;
        this.tilesContainerParent = tilesContainerParent;
        initialized = true;
    }

    // ===== 毎フレーム呼び出し =====

    public void Tick()
    {
        if (!initialized) return;

        UpdateMarker();

        if (IsLocked)
        {
            swipeDx = 0;
            swipeDy = 0;
            pathQueue.Clear();
            inertiaSteps = 0;
            return;
        }

        if (getIsMoving()) return;

        // 1. キーボード入力
        int dx = 0, dy = 0;
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

        if (dx != 0 || dy != 0)
        {
            pathQueue.Clear();
            inertiaSteps = 0;
            tryMove(dx, dy);
            return;
        }

        // 2. スワイプ入力
        if (swipeDx != 0 || swipeDy != 0)
        {
            pathQueue.Clear();
            inertiaSteps = 0;
            tryMove(swipeDx, swipeDy);
            return;
        }

        // 3. 慣性
        if (inertiaSteps > 0)
        {
            inertiaSteps--;
            tryMove(inertiaDx, inertiaDy);
            return;
        }

        // 4. パスキュー（タップ移動）
        if (pathQueue.Count > 0)
        {
            var tile = getPlayerTile();
            var next = pathQueue.Dequeue();
            int pdx = next.x - tile.x;
            int pdy = next.y - tile.y;

            if (!isWalkable(next.x, next.y))
            {
                pathQueue.Clear();
                HideMarker();
                return;
            }

            bool moved = tryMove(pdx, pdy);
            if (!moved)
            {
                pathQueue.Clear();
                HideMarker();
            }
            else if (pathQueue.Count == 0)
            {
                HideMarker();
            }
        }
    }

    // ===== ポインタイベント（MapManager から呼ばれる） =====

    public void OnPointerDown(Vector2 pos)
    {
        touchStartPos = pos;
        touchDownTime = Time.time;
        isTouchDragging = true;
        touchIsSwipe = false;
        swipeDx = 0;
        swipeDy = 0;
    }

    public void OnPointerMove(Vector2 pos)
    {
        if (!isTouchDragging) return;

        Vector2 delta = pos - touchStartPos;
        if (delta.magnitude < SWIPE_THRESHOLD)
        {
            swipeDx = 0;
            swipeDy = 0;
            return;
        }

        touchIsSwipe = true;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            swipeDx = delta.x > 0 ? 1 : -1;
            swipeDy = 0;
        }
        else
        {
            swipeDx = 0;
            // UI Toolkit Y は画面座標と逆
            swipeDy = delta.y < 0 ? 1 : -1;
        }
    }

    public void OnPointerUp(Vector2 pos)
    {
        if (!isTouchDragging) return;

        float elapsed = Time.time - touchDownTime;
        Vector2 delta = pos - touchStartPos;

        if (!touchIsSwipe && elapsed < TAP_TIME_LIMIT && delta.magnitude < TAP_DIST_LIMIT)
        {
            // タップ → タップ移動
            StartTapToMove(pos);
        }
        else if (touchIsSwipe)
        {
            // スワイプ終了 → 慣性
            inertiaDx = swipeDx;
            inertiaDy = swipeDy;
            inertiaSteps = INERTIA_TILES;
        }

        swipeDx = 0;
        swipeDy = 0;
        isTouchDragging = false;
        touchIsSwipe = false;
    }

    // ===== タップ移動 =====

    void StartTapToMove(Vector2 screenPos)
    {
        Vector2Int tapTile = ScreenToTile(screenPos);
        if (tapTile.x < 0 || tapTile.x >= mapWidth || tapTile.y < 0 || tapTile.y >= mapHeight)
            return;

        Vector2Int targetTile = tapTile;

        // walkableでないタイルをタップした場合、隣接する最も近いwalkableタイルを探す
        if (!isWalkable(tapTile.x, tapTile.y))
        {
            var from = getPlayerTile();
            Vector2Int best = new Vector2Int(-1, -1);
            float bestDist = float.MaxValue;
            int[] ddx = { 0, 0, -1, 1 };
            int[] ddy = { 1, -1, 0, 0 };
            for (int i = 0; i < 4; i++)
            {
                int nx = tapTile.x + ddx[i];
                int ny = tapTile.y + ddy[i];
                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight && isWalkable(nx, ny))
                {
                    float d = Mathf.Abs(from.x - nx) + Mathf.Abs(from.y - ny);
                    if (d < bestDist) { bestDist = d; best = new Vector2Int(nx, ny); }
                }
            }
            if (best.x < 0) return;
            targetTile = best;
        }

        var start = getPlayerTile();
        if (start.x == targetTile.x && start.y == targetTile.y)
            return;

        var path = FindPath(start, targetTile);
        if (path == null || path.Count == 0)
            return;

        pathQueue.Clear();
        foreach (var step in path)
            pathQueue.Enqueue(step);

        ShowMarker(targetTile.x, targetTile.y);
    }

    Vector2Int ScreenToTile(Vector2 screenPos)
    {
        Vector2 offset = tilesContainerRect.anchoredPosition;
        // UI Toolkit 座標系: 左上原点, canvas 1080x1920
        float cx = screenPos.x - 540f - offset.x;
        float cy = -(screenPos.y - 960f) - offset.y + 40f; // +40f カメラY補正
        int tileX = Mathf.FloorToInt(cx / displayTile + mapWidth / 2f);
        int tileY = Mathf.FloorToInt(cy / displayTile + mapHeight / 2f);
        return new Vector2Int(tileX, tileY);
    }

    // ===== A* パスファインディング =====

    List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
    {
        if (from == to) return null;
        if (!isWalkable(to.x, to.y)) return null;

        var openList = new List<AStarNode>();
        var gScore = new Dictionary<Vector2Int, float>();
        var parent = new Dictionary<Vector2Int, Vector2Int>();
        var closed = new HashSet<Vector2Int>();

        gScore[from] = 0;
        openList.Add(new AStarNode(from, Heuristic(from, to)));

        int[] ddx = { 0, 0, -1, 1 };
        int[] ddy = { 1, -1, 0, 0 };

        while (openList.Count > 0)
        {
            // 最小 fScore を探す
            int bestIdx = 0;
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fScore < openList[bestIdx].fScore)
                    bestIdx = i;
            }
            var current = openList[bestIdx];
            openList.RemoveAt(bestIdx);

            if (current.pos == to)
                return ReconstructPath(parent, from, to);

            if (closed.Contains(current.pos)) continue;
            closed.Add(current.pos);

            float g = gScore[current.pos];

            for (int i = 0; i < 4; i++)
            {
                int nx = current.pos.x + ddx[i];
                int ny = current.pos.y + ddy[i];
                var nb = new Vector2Int(nx, ny);

                if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight) continue;
                if (closed.Contains(nb)) continue;
                if (!isWalkable(nx, ny)) continue;

                float ng = g + 1f;
                if (!gScore.ContainsKey(nb) || ng < gScore[nb])
                {
                    gScore[nb] = ng;
                    parent[nb] = current.pos;
                    openList.Add(new AStarNode(nb, ng + Heuristic(nb, to)));
                }
            }
        }

        return null; // パスなし
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> parent,
                                     Vector2Int from, Vector2Int to)
    {
        var path = new List<Vector2Int>();
        var cur = to;
        while (cur != from)
        {
            path.Add(cur);
            cur = parent[cur];
        }
        path.Reverse();
        return path;
    }

    struct AStarNode
    {
        public Vector2Int pos;
        public float fScore;

        public AStarNode(Vector2Int pos, float fScore)
        {
            this.pos = pos;
            this.fScore = fScore;
        }
    }

    // ===== タップマーカー =====

    void ShowMarker(int tx, int ty)
    {
        HideMarker();

        markerObj = new GameObject("TapMarker");
        markerObj.transform.SetParent(tilesContainerParent, false);

        var rt = markerObj.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        float px = (tx - mapWidth / 2f + 0.5f) * displayTile;
        float py = (ty - mapHeight / 2f + 0.5f) * displayTile;
        rt.anchoredPosition = new Vector2(px, py);
        float markerSize = displayTile * 0.7f;
        rt.sizeDelta = new Vector2(markerSize, markerSize);

        // リング外枠
        var ringObj = new GameObject("Ring");
        ringObj.transform.SetParent(markerObj.transform, false);
        var ringRt = ringObj.AddComponent<RectTransform>();
        ringRt.anchorMin = Vector2.zero;
        ringRt.anchorMax = Vector2.one;
        ringRt.offsetMin = Vector2.zero;
        ringRt.offsetMax = Vector2.zero;
        markerImage = ringObj.AddComponent<Image>();
        // accent色 #AAF0D1 の半透明
        markerImage.color = new Color(170f / 255f, 240f / 255f, 209f / 255f, 0.7f);

        // 内側を透明にする（中心に小さい透明パネル）
        var innerObj = new GameObject("Inner");
        innerObj.transform.SetParent(ringObj.transform, false);
        var innerRt = innerObj.AddComponent<RectTransform>();
        innerRt.anchorMin = new Vector2(0.2f, 0.2f);
        innerRt.anchorMax = new Vector2(0.8f, 0.8f);
        innerRt.offsetMin = Vector2.zero;
        innerRt.offsetMax = Vector2.zero;
        var innerImg = innerObj.AddComponent<Image>();
        innerImg.color = new Color(170f / 255f, 240f / 255f, 209f / 255f, 0.3f);

        markerTargetTile = new Vector2Int(tx, ty);
        markerTimer = MARKER_LIFETIME;
    }

    void UpdateMarker()
    {
        if (markerObj == null) return;

        markerTimer -= Time.deltaTime;

        // 脈動
        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.08f;
        markerObj.transform.localScale = Vector3.one * pulse;

        // フェードアウト
        if (markerImage != null)
        {
            float alpha = Mathf.Clamp01(markerTimer / MARKER_LIFETIME) * 0.7f;
            var c = markerImage.color;
            markerImage.color = new Color(c.r, c.g, c.b, alpha);
        }

        // 到着 or タイマー切れで消去
        var tile = getPlayerTile();
        if (markerTimer <= 0f || (tile.x == markerTargetTile.x && tile.y == markerTargetTile.y))
            HideMarker();
    }

    public void HideMarker()
    {
        if (markerObj != null)
        {
            Object.Destroy(markerObj);
            markerObj = null;
        }
    }

    // ===== クリーンアップ =====

    public void ClearPath()
    {
        pathQueue.Clear();
        inertiaSteps = 0;
        HideMarker();
    }

    public void Destroy()
    {
        HideMarker();
    }
}
