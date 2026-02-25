using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UIE = UnityEngine.UIElements;

public class EnishiManager : MonoBehaviour
{
    private UIE.PanelSettings panelSettings;
    private UIE.VisualElement root;

    private static readonly Dictionary<string, string> NpcSpriteMap = new Dictionary<string, string>
    {
        { "ミルク母さん", "MapCharacters/Milk_Mother" },
        { "長老", "MapCharacters/Old_Men" },
    };

    private static readonly Dictionary<string, string> EnemySpriteMap = new Dictionary<string, string>
    {
        { "うずうずベイビー", "EnemyBabys/common-yantya" },
        { "ぷんぷんベイビー", "EnemyBabys/frist-enemy" },
        { "えんえんベイビー", "EnemyBabys/common-nakimushi" },
        { "どたばたベイビー", "EnemyBabys/common-abarennbou" },
        { "いやだいやだベイビー", "EnemyBabys/common-wagamama" },
        { "青年のシバ", "EnemyBabys/boss/first-boss-shiba" },
        { "わるいベイビー", "EnemyBabys/frist-enemy" },
        { "にがにがベイビー", "EnemyBabys/poison/doku-baby" },
        { "ぐちぐちベイビー", "EnemyBabys/poison/noroi-baby" },
        { "つんつんベイビー", "EnemyBabys/poison/akuma-baby" },
        { "どよよんベイビー", "EnemyBabys/poison/yami-baby" },
        { "いじいじベイビー", "EnemyBabys/poison/jyaaku-baby" },
        { "ごーじゃすベイビー", "EnemyBabys/poison/maou-baby" },
        { "デヴィル傭兵A", "EnemyBabys/poison/katchu-a" },
        { "デヴィル傭兵B", "EnemyBabys/poison/katchu-b" },
        { "デヴィル夫人", "EnemyBabys/boss/devil-wife" },
        { "小悪魔ひとみ", "EnemyBabys/cute/hitomi" },
        { "小悪魔あやか", "EnemyBabys/cute/ayaka" },
        { "小悪魔りん", "EnemyBabys/cute/rin" },
        { "小悪魔みく", "EnemyBabys/cute/miku" },
        { "小悪魔なな", "EnemyBabys/cute/nana" },
        { "小悪魔れい", "EnemyBabys/cute/rei" },
        { "メロディアス女王", "EnemyBabys/boss/melodias" },
    };

    private enum DetailType { Enemy, Npc, Father, Mother }

    void Start()
    {
        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/EnishiStyle" }, panelSettings);
        UIHelper.RegisterTapSE(root);

        BuildUI();
    }

    void OnDestroy()
    {
        if (panelSettings != null)
            Destroy(panelSettings);
    }

    void BuildUI()
    {
        // Background
        var bg = new UIE.VisualElement();
        bg.AddToClassList("bg-screen");
        root.Add(bg);

        // Safe area
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        // ヘッダーの高さを計算
        float headerHeight = safeTop + 40 + 60 + 16; // safeTop + marginTop + row概算高 + paddingBottom

        // ScrollView (ヘッダーの下から画面下端まで) — 先に追加してヘッダーの背面に配置
        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.position = UIE.Position.Absolute;
        scrollView.style.left = 0;
        scrollView.style.right = 0;
        scrollView.style.top = headerHeight;
        scrollView.style.bottom = 0;
        root.Add(scrollView);

        // Header (固定: ScrollView より後に追加 → 前面に描画)
        var headerWrapper = new UIE.VisualElement();
        headerWrapper.style.position = UIE.Position.Absolute;
        headerWrapper.style.left = 0;
        headerWrapper.style.top = 0;
        headerWrapper.style.right = 0;
        headerWrapper.style.backgroundColor = new Color(0.953f, 0.969f, 0.973f); // bg-light
        headerWrapper.style.paddingTop = safeTop;
        headerWrapper.style.paddingBottom = 16;

        var headerRow = new UIE.VisualElement();
        headerRow.style.flexDirection = UIE.FlexDirection.Row;
        headerRow.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        headerRow.style.alignItems = UIE.Align.Center;
        headerRow.style.marginTop = 40;

        // Back button
        var backBtn = new UIE.Button();
        backBtn.AddToClassList("back-button");
        UIHelper.ApplyFont(backBtn);
        backBtn.text = "< " + Localization.Get("ui_back");
        backBtn.style.position = UIE.Position.Relative;
        backBtn.clicked += () => SceneManager.LoadScene("HomeScene");
        headerRow.Add(backBtn);

        // Title (centered)
        var title = UIHelper.CreateLabel(Localization.Get("enishi_title"), "enishi-title");
        title.style.flexGrow = 1;
        headerRow.Add(title);

        // Spacer to balance back button
        var spacer = new UIE.VisualElement();
        spacer.style.width = 100;
        headerRow.Add(spacer);

        headerWrapper.Add(headerRow);
        root.Add(headerWrapper);

        var scrollContent = scrollView.contentContainer;
        scrollContent.style.flexShrink = 0;

        // === Collect data ===
        var uniqueEnemies = new List<string>();
        var seen = new HashSet<string>();
        var dc = DataCarrier.Instance;

        if (dc != null)
        {
            foreach (var e in dc.GetDefeatedEnemyList())
                if (!seen.Contains(e)) { seen.Add(e); uniqueEnemies.Add(e); }
        }

        for (int slot = 0; slot < DataCarrier.MAX_SAVE_SLOTS; slot++)
        {
            if (!DataCarrier.SlotExists(slot)) continue;
            string list = PlayerPrefs.GetString($"slot{slot}_defeatedEnemyList", "");
            if (string.IsNullOrEmpty(list)) continue;
            foreach (var e in list.Split(','))
                if (!string.IsNullOrEmpty(e) && !seen.Contains(e))
                { seen.Add(e); uniqueEnemies.Add(e); }
        }

        // === Section: Enemies ===
        AddSectionHeader(scrollContent, Localization.Get("enishi_section_enemies"));

        if (uniqueEnemies.Count == 0)
        {
            AddEmptyText(scrollContent, Localization.Get("enishi_empty"));
        }
        else
        {
            var grid = CreateGrid();
            foreach (var jaName in uniqueEnemies)
            {
                Sprite sprite = LoadEnemySprite(jaName);
                string displayName = Localization.GetEnemy(jaName);
                string name = jaName;
                AddGridItem(grid, sprite, displayName, () => ShowEnemyDetail(name));
            }
            scrollContent.Add(grid);
        }

        // === Collect NPCs ===
        var metNpcs = new List<string>();
        var seenNpcs = new HashSet<string>();

        if (dc != null)
        {
            foreach (var n in dc.GetMetNpcList())
                if (!string.IsNullOrEmpty(n) && !seenNpcs.Contains(n))
                { seenNpcs.Add(n); metNpcs.Add(n); }
        }

        for (int slot = 0; slot < DataCarrier.MAX_SAVE_SLOTS; slot++)
        {
            if (!DataCarrier.SlotExists(slot)) continue;
            string list = PlayerPrefs.GetString($"slot{slot}_metNpcList", "");
            if (string.IsNullOrEmpty(list)) continue;
            foreach (var n in list.Split(','))
                if (!string.IsNullOrEmpty(n) && !seenNpcs.Contains(n))
                { seenNpcs.Add(n); metNpcs.Add(n); }
        }

        // === Section: NPCs ===
        if (metNpcs.Count > 0)
        {
            AddSectionHeader(scrollContent, Localization.Get("enishi_section_npcs"));
            var grid = CreateGrid();
            foreach (var jaName in metNpcs)
            {
                Sprite sprite = LoadNpcSprite(jaName);
                string name = jaName;
                AddGridItem(grid, sprite, jaName, () => ShowNpcDetail(name));
            }
            scrollContent.Add(grid);
        }

        // === Collect parents ===
        var encounteredFathers = new List<string>();
        var encounteredMothers = new List<string>();
        var seenFathers = new HashSet<string>();
        var seenMothers = new HashSet<string>();

        if (dc != null)
        {
            if (!string.IsNullOrEmpty(dc.fatherName) && !seenFathers.Contains(dc.fatherName))
            { seenFathers.Add(dc.fatherName); encounteredFathers.Add(dc.fatherName); }
            if (!string.IsNullOrEmpty(dc.motherName) && !seenMothers.Contains(dc.motherName))
            { seenMothers.Add(dc.motherName); encounteredMothers.Add(dc.motherName); }
        }

        for (int slot = 0; slot < DataCarrier.MAX_SAVE_SLOTS; slot++)
        {
            if (!DataCarrier.SlotExists(slot)) continue;
            string f = PlayerPrefs.GetString($"slot{slot}_fatherName", "");
            string m = PlayerPrefs.GetString($"slot{slot}_motherName", "");
            if (!string.IsNullOrEmpty(f) && !seenFathers.Contains(f))
            { seenFathers.Add(f); encounteredFathers.Add(f); }
            if (!string.IsNullOrEmpty(m) && !seenMothers.Contains(m))
            { seenMothers.Add(m); encounteredMothers.Add(m); }
        }

        // === Section: Fathers ===
        AddSectionHeader(scrollContent, Localization.Get("enishi_section_fathers"));
        if (encounteredFathers.Count == 0)
        {
            AddEmptyText(scrollContent, Localization.Get("enishi_empty"));
        }
        else
        {
            var grid = CreateGrid();
            foreach (var jaName in encounteredFathers)
            {
                string imgKey = GetParentImageName(jaName);
                Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);
                string name = jaName;
                AddGridItem(grid, sprite, jaName, () => ShowFatherDetail(name));
            }
            scrollContent.Add(grid);
        }

        // === Section: Mothers ===
        AddSectionHeader(scrollContent, Localization.Get("enishi_section_mothers"));
        if (encounteredMothers.Count == 0)
        {
            AddEmptyText(scrollContent, Localization.Get("enishi_empty"));
        }
        else
        {
            var grid = CreateGrid();
            foreach (var jaName in encounteredMothers)
            {
                string imgKey = GetParentImageName(jaName);
                Sprite sprite = Resources.Load<Sprite>("Parents/" + imgKey);
                string name = jaName;
                AddGridItem(grid, sprite, jaName, () => ShowMotherDetail(name));
            }
            scrollContent.Add(grid);
        }
    }

    const int GRID_COLUMNS = 3;
    int gridItemCount;

    UIE.VisualElement CreateGrid()
    {
        var grid = new UIE.VisualElement();
        grid.AddToClassList("enishi-grid");
        gridItemCount = 0;
        return grid;
    }

    void AddSectionHeader(UIE.VisualElement parent, string text)
    {
        var header = UIHelper.CreateLabel(text, "section-header");
        parent.Add(header);

        var sep = new UIE.VisualElement();
        sep.AddToClassList("section-separator");
        parent.Add(sep);
    }

    void AddEmptyText(UIE.VisualElement parent, string text)
    {
        var label = UIHelper.CreateLabel(text, "empty-text");
        parent.Add(label);
    }

    void AddGridItem(UIE.VisualElement grid, Sprite sprite, string name, System.Action onTap)
    {
        // 3アイテムごとに新しい行を作成
        if (gridItemCount % GRID_COLUMNS == 0)
        {
            var row = new UIE.VisualElement();
            row.AddToClassList("enishi-grid-row");
            grid.Add(row);
        }

        // 最後の行を取得
        var currentRow = grid[grid.childCount - 1];

        var item = new UIE.VisualElement();
        item.AddToClassList("grid-item");

        var border = new UIE.VisualElement();
        border.AddToClassList("grid-circle-border");

        var maskBtn = new UIE.Button();
        maskBtn.AddToClassList("grid-circle-mask");
        maskBtn.clicked += () => onTap();

        if (sprite != null)
        {
            var img = new UIE.VisualElement();
            img.AddToClassList("grid-image");
            img.style.backgroundImage = new UIE.StyleBackground(sprite);
            maskBtn.Add(img);
        }
        else
        {
            var placeholder = new UIE.VisualElement();
            placeholder.AddToClassList("grid-image-placeholder");
            maskBtn.Add(placeholder);
        }

        border.Add(maskBtn);
        item.Add(border);

        var nameLabel = UIHelper.CreateLabel(name, "grid-name");
        item.Add(nameLabel);

        currentRow.Add(item);
        gridItemCount++;
    }

    // === Detail panels ===

    void ShowEnemyDetail(string jaName)
    {
        Sprite sprite = LoadEnemySprite(jaName);
        string displayName = Localization.GetEnemy(jaName);
        string bio = Localization.GetEnemyBio(jaName);
        ShowDetailPanel(sprite, displayName, bio, DetailType.Enemy);
    }

    void ShowFatherDetail(string jaName)
    {
        string imgKey = GetParentImageName(jaName);
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
        ShowDetailPanel(sprite, jaName, description, DetailType.Father);
    }

    void ShowMotherDetail(string jaName)
    {
        string imgKey = GetParentImageName(jaName);
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
        ShowDetailPanel(sprite, jaName, description, DetailType.Mother);
    }

    void ShowDetailPanel(Sprite sprite, string name, string description, DetailType type)
    {
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("overlay-dark");

        // Tap overlay to close
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay)
                overlay.RemoveFromHierarchy();
        });

        // ScrollView for the detail content
        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        scrollView.style.maxHeight = new UIE.StyleLength(new UIE.Length(90, UIE.LengthUnit.Percent));

        // Album panel (sepia paper background)
        var panel = new UIE.VisualElement();
        panel.AddToClassList("album-panel");

        // album-content: 2-column row
        var content = new UIE.VisualElement();
        content.AddToClassList("album-content");

        // === Left column: polaroid + corner seals ===
        var left = new UIE.VisualElement();
        left.AddToClassList("album-left");

        var polaroidFrame = new UIE.VisualElement();
        polaroidFrame.AddToClassList("polaroid-frame");
        polaroidFrame.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(-3f, UIE.AngleUnit.Degree)));

        var polaroidInner = new UIE.VisualElement();
        polaroidInner.AddToClassList("polaroid-inner");

        if (sprite != null)
        {
            var img = new UIE.VisualElement();
            img.AddToClassList("polaroid-image");
            img.style.backgroundImage = new UIE.StyleBackground(sprite);
            polaroidInner.Add(img);
        }
        else
        {
            polaroidInner.style.backgroundColor = new Color(0.85f, 0.85f, 0.85f);
        }

        polaroidFrame.Add(polaroidInner);

        // Caption under the polaroid photo
        var caption = UIHelper.CreateLabel(name, "polaroid-caption");
        polaroidFrame.Add(caption);

        left.Add(polaroidFrame);

        // Corner seals (4 corners)
        AddCornerSeal(left, 20, 10);    // top-left
        AddCornerSeal(left, 356, 10);   // top-right
        AddCornerSeal(left, 20, 350);   // bottom-left
        AddCornerSeal(left, 356, 350);  // bottom-right

        content.Add(left);

        // === Right column: name + divider + description ===
        var right = new UIE.VisualElement();
        right.AddToClassList("album-right");

        var nameLabel = UIHelper.CreateLabel(name, "album-name");
        right.Add(nameLabel);

        var divider = new UIE.VisualElement();
        divider.AddToClassList("album-divider");
        right.Add(divider);

        if (!string.IsNullOrEmpty(description))
        {
            var descLabel = UIHelper.CreateLabel(description, "album-description");
            right.Add(descLabel);
        }

        content.Add(right);
        panel.Add(content);

        // === Footer ===
        var footer = new UIE.VisualElement();
        footer.AddToClassList("album-footer");

        // "愛を育む" button — only for Father / Mother
        if (type == DetailType.Father || type == DetailType.Mother)
        {
            var loveBtn = new UIE.Button();
            loveBtn.AddToClassList("album-love-button");
            UIHelper.ApplyFont(loveBtn);
            loveBtn.text = Localization.Get("birth_nurture_love");
            loveBtn.clicked += () =>
            {
                UIHelper.PlayTapSE();
                SceneManager.LoadScene("BirthScene");
            };
            footer.Add(loveBtn);
        }

        // Close button
        var closeBtn = new UIE.Button();
        closeBtn.AddToClassList("album-close-button");
        UIHelper.ApplyFont(closeBtn);
        closeBtn.text = Localization.Get("ui_close");
        closeBtn.clicked += () => { UIHelper.PlayTapSE(); overlay.RemoveFromHierarchy(); };
        footer.Add(closeBtn);

        panel.Add(footer);
        scrollView.Add(panel);
        overlay.Add(scrollView);
        root.Add(overlay);
    }

    void AddCornerSeal(UIE.VisualElement parent, float x, float y)
    {
        var seal = new UIE.VisualElement();
        seal.AddToClassList("corner-seal");
        seal.style.left = x;
        seal.style.top = y;
        seal.style.rotate = new UIE.StyleRotate(new UIE.Rotate(new UIE.Angle(45f, UIE.AngleUnit.Degree)));
        parent.Add(seal);
    }

    // === Helpers ===

    Sprite LoadEnemySprite(string jaName)
    {
        if (EnemySpriteMap.TryGetValue(jaName, out string path))
            return Resources.Load<Sprite>(path);
        return null;
    }

    Sprite LoadNpcSprite(string jaName)
    {
        if (NpcSpriteMap.TryGetValue(jaName, out string path))
            return Resources.Load<Sprite>(path);
        // 親キャラ（母親NPC等）のスプライトをフォールバック
        string imgKey = GetParentImageName(jaName);
        Sprite parentSprite = Resources.Load<Sprite>("Parents/" + imgKey);
        if (parentSprite != null) return parentSprite;
        return null;
    }

    void ShowNpcDetail(string jaName)
    {
        Sprite sprite = LoadNpcSprite(jaName);
        string bio = Localization.GetNpcBio(jaName);
        ShowDetailPanel(sprite, jaName, bio, DetailType.Npc);
    }

    static string GetParentImageName(string japaneseName)
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
            default: return japaneseName != null ? japaneseName.ToLower() : "";
        }
    }
}
