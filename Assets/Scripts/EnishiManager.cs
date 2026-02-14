using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UIE = UnityEngine.UIElements;

public class EnishiManager : MonoBehaviour
{
    private UIE.PanelSettings panelSettings;
    private UIE.VisualElement root;

    private static readonly Dictionary<string, string> EnemySpriteMap = new Dictionary<string, string>
    {
        { "やんちゃベイビー", "EnemyBabys/common-yantya" },
        { "いじわるベイビー", "EnemyBabys/frist-enemy" },
        { "なきむしベイビー", "EnemyBabys/common-nakimushi" },
        { "あばれんぼうベイビー", "EnemyBabys/common-abarennbou" },
        { "わがままベイビー", "EnemyBabys/common-wagamama" },
        { "村の王シバ", "EnemyBabys/boss/first-boss-shiba" },
        { "わるいベイビー", "EnemyBabys/frist-enemy" },
        { "どくベイビー", "EnemyBabys/poison/doku-baby" },
        { "のろいベイビー", "EnemyBabys/poison/noroi-baby" },
        { "あくまベイビー", "EnemyBabys/poison/akuma-baby" },
        { "やみベイビー", "EnemyBabys/poison/yami-baby" },
        { "じゃあくベイビー", "EnemyBabys/poison/jyaaku-baby" },
        { "まおうベイビー", "EnemyBabys/poison/maou-baby" },
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

    void Start()
    {
        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/EnishiStyle" }, panelSettings);

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

        // Content layer
        var content = new UIE.VisualElement();
        content.AddToClassList("fill");
        content.style.alignItems = UIE.Align.Center;
        root.Add(content);

        // Safe area
        var (safeTop, _, _, _) = UIHelper.GetSafeMargins();

        // Header row
        var headerRow = new UIE.VisualElement();
        headerRow.style.flexDirection = UIE.FlexDirection.Row;
        headerRow.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        headerRow.style.alignItems = UIE.Align.Center;
        headerRow.style.marginTop = 120 + safeTop;
        headerRow.style.marginBottom = 10;

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

        content.Add(headerRow);

        // ScrollView
        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.flexGrow = 1;
        scrollView.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        content.Add(scrollView);

        var scrollContent = scrollView.contentContainer;

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
                AddGridItem(grid, sprite, jaName, () => ShowParentDetail(name));
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
                AddGridItem(grid, sprite, jaName, () => ShowParentDetail(name));
            }
            scrollContent.Add(grid);
        }
    }

    UIE.VisualElement CreateGrid()
    {
        var grid = new UIE.VisualElement();
        grid.AddToClassList("enishi-grid");
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

        grid.Add(item);
    }

    // === Detail panels ===

    void ShowEnemyDetail(string jaName)
    {
        Sprite sprite = LoadEnemySprite(jaName);
        string displayName = Localization.GetEnemy(jaName);
        string bio = Localization.GetEnemyBio(jaName);
        ShowDetailPanel(sprite, displayName, bio);
    }

    void ShowParentDetail(string jaName)
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
        ShowDetailPanel(sprite, jaName, description);
    }

    void ShowDetailPanel(Sprite sprite, string name, string description)
    {
        var overlay = new UIE.VisualElement();
        overlay.AddToClassList("overlay-dark");

        // Tap overlay to close
        overlay.RegisterCallback<UIE.ClickEvent>(evt =>
        {
            if (evt.target == overlay)
                overlay.RemoveFromHierarchy();
        });

        // ScrollView for the detail content (fills the overlay)
        var scrollView = new UIE.ScrollView(UIE.ScrollViewMode.Vertical);
        scrollView.style.width = new UIE.StyleLength(new UIE.Length(100, UIE.LengthUnit.Percent));
        scrollView.style.maxHeight = new UIE.StyleLength(new UIE.Length(85, UIE.LengthUnit.Percent));

        var panel = new UIE.VisualElement();
        panel.AddToClassList("detail-panel");

        // Image
        var imgBorder = new UIE.VisualElement();
        imgBorder.AddToClassList("detail-image-border");

        var imgMask = new UIE.VisualElement();
        imgMask.AddToClassList("detail-image-mask");

        if (sprite != null)
        {
            var img = new UIE.VisualElement();
            img.AddToClassList("detail-image");
            img.style.backgroundImage = new UIE.StyleBackground(sprite);
            imgMask.Add(img);
        }
        else
        {
            imgMask.style.backgroundColor = new Color(0.85f, 0.85f, 0.85f);
        }

        imgBorder.Add(imgMask);
        panel.Add(imgBorder);

        // Name
        var nameLabel = UIHelper.CreateLabel(name, "detail-name");
        panel.Add(nameLabel);

        // Description
        if (!string.IsNullOrEmpty(description))
        {
            var descLabel = UIHelper.CreateLabel(description, "detail-description");
            panel.Add(descLabel);
        }

        // Close button
        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button-medium");
        closeBtn.style.marginTop = 30;
        closeBtn.style.marginBottom = 20;
        closeBtn.clicked += () => overlay.RemoveFromHierarchy();
        panel.Add(closeBtn);

        scrollView.Add(panel);
        overlay.Add(scrollView);
        root.Add(overlay);
    }

    // === Helpers ===

    Sprite LoadEnemySprite(string jaName)
    {
        if (EnemySpriteMap.TryGetValue(jaName, out string path))
            return Resources.Load<Sprite>(path);
        return null;
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
            default: return japaneseName != null ? japaneseName.ToLower() : "";
        }
    }
}
