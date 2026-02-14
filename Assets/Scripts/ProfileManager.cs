using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UIElements;
using UIE = UnityEngine.UIElements;

public class ProfileManager : MonoBehaviour
{
    private UIE.PanelSettings panelSettings;
    private UIE.VisualElement root;

    private int selectedIcon = 0;

    private static readonly string[] iconNames = new string[]
    {
        "kayo", "ikemen", "inteli", "matcho", "old-women", "sexy-lady"
    };

    private static readonly Color[] iconColors = new Color[]
    {
        new Color(0.90f, 0.30f, 0.30f),
        new Color(0.30f, 0.50f, 0.90f),
        new Color(0.30f, 0.80f, 0.40f),
        new Color(0.60f, 0.35f, 0.85f),
        new Color(0.95f, 0.60f, 0.20f),
        new Color(0.95f, 0.45f, 0.65f),
    };

    private UIE.VisualElement avatarImage;
    private UIE.VisualElement iconPickerOverlay;
    private UIE.TextField nameInputField;
    private UIE.Button startButton;
    private UIE.Label startButtonLabel;
    private bool isEditing;

    void Start()
    {
        panelSettings = UIHelper.CreatePanelSettings(0f);
        root = UIHelper.SetupUIDocument(gameObject,
            new[] { "UI/CommonStyle", "UI/ProfileStyle" }, panelSettings);

        isEditing = DataCarrier.HasProfile();
        BuildUI();

        if (isEditing)
        {
            string existingName = DataCarrier.GetProfileName();
            int existingIcon = DataCarrier.GetProfileIcon();
            if (nameInputField != null && !string.IsNullOrEmpty(existingName))
                nameInputField.value = existingName;
            SelectIcon(existingIcon);
        }
        else
        {
            SelectIcon(0);
        }
        UpdateStartButton();
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

        // Main content column
        var content = new UIE.VisualElement();
        content.style.position = UIE.Position.Absolute;
        content.style.left = 0;
        content.style.right = 0;
        content.style.top = 0;
        content.style.bottom = 0;
        content.style.alignItems = UIE.Align.Center;
        root.Add(content);

        // Avatar display
        CreateAvatarDisplay(content);

        // Name label
        var nameLabel = UIHelper.CreateLabel(Localization.Get("profile_name_label"), "name-label");
        content.Add(nameLabel);

        // Name input
        nameInputField = new UIE.TextField();
        nameInputField.AddToClassList("name-input");
        nameInputField.maxLength = 12;
        UIHelper.ApplyFont(nameInputField);
        // Apply font to inner input element
        var inputEl = nameInputField.Q<UIE.VisualElement>(className: "unity-text-field__input");
        if (inputEl != null) UIHelper.ApplyFont(inputEl);
        nameInputField.RegisterValueChangedCallback(evt => UpdateStartButton());
        content.Add(nameInputField);

        // Start button
        CreateStartButton(content);
    }

    void CreateAvatarDisplay(UIE.VisualElement parent)
    {
        // Position avatar area
        var avatarArea = new UIE.VisualElement();
        avatarArea.style.marginTop = 300;
        avatarArea.style.alignItems = UIE.Align.Center;

        var avatarBtn = new UIE.Button();
        avatarBtn.AddToClassList("avatar-btn");
        avatarBtn.clicked += ToggleIconPicker;

        // Circle mask for avatar
        var circle = new UIE.VisualElement();
        circle.AddToClassList("avatar-circle");

        avatarImage = new UIE.VisualElement();
        avatarImage.AddToClassList("avatar-image");
        ApplyAvatarSprite(0);
        circle.Add(avatarImage);
        avatarBtn.Add(circle);

        // Edit badge
        var badge = new UIE.VisualElement();
        badge.AddToClassList("edit-badge");
        var badgeText = UIHelper.CreateLabel("\u270E", "edit-badge-text");
        badge.Add(badgeText);
        avatarBtn.Add(badge);

        avatarArea.Add(avatarBtn);
        parent.Add(avatarArea);
    }

    void ApplyAvatarSprite(int index)
    {
        string name = (index >= 0 && index < iconNames.Length) ? iconNames[index] : iconNames[0];
        Sprite spr = Resources.Load<Sprite>($"Icons/{name}");

        if (spr != null)
        {
            avatarImage.style.backgroundImage = new UIE.StyleBackground(spr);
            avatarImage.style.backgroundColor = UIE.StyleKeyword.None;
        }
        else
        {
            avatarImage.style.backgroundImage = UIE.StyleKeyword.None;
            Color c = iconColors[Mathf.Clamp(index, 0, iconColors.Length - 1)];
            avatarImage.style.backgroundColor = c;
        }
    }

    void ToggleIconPicker()
    {
        if (iconPickerOverlay != null)
        {
            iconPickerOverlay.RemoveFromHierarchy();
            iconPickerOverlay = null;
            return;
        }
        ShowIconPicker();
    }

    void ShowIconPicker()
    {
        iconPickerOverlay = new UIE.VisualElement();
        iconPickerOverlay.AddToClassList("overlay-dark");

        // Card
        var card = new UIE.VisualElement();
        card.AddToClassList("icon-picker-card");

        var title = UIHelper.CreateLabel(Localization.Get("profile_icon_select"), "picker-title");
        card.Add(title);

        // Icon grid
        var grid = new UIE.VisualElement();
        grid.AddToClassList("icon-grid");

        for (int i = 0; i < iconNames.Length; i++)
            CreatePickerIcon(grid, i);

        card.Add(grid);

        // Button row
        var btnRow = new UIE.VisualElement();
        btnRow.AddToClassList("picker-btn-row");

        // Save button
        var saveBtn = UIHelper.CreatePillButton(Localization.Get("profile_save"), "pill-button-medium");
        saveBtn.style.width = 350;
        saveBtn.style.height = 90;
        saveBtn.style.marginRight = 15;
        saveBtn.clicked += () =>
        {
            if (DataCarrier.Instance != null)
            {
                DataCarrier.Instance.playerIcon = selectedIcon;
                DataCarrier.Instance.SaveProfile();
            }
            iconPickerOverlay.RemoveFromHierarchy();
            iconPickerOverlay = null;
            StartCoroutine(ShowToast(Localization.Get("profile_icon_saved")));
        };
        btnRow.Add(saveBtn);

        // Close button
        var closeBtn = UIHelper.CreatePillButton(Localization.Get("ui_close"), "pill-button-medium");
        closeBtn.style.width = 350;
        closeBtn.style.height = 90;
        closeBtn.style.marginLeft = 15;
        closeBtn.clicked += () =>
        {
            iconPickerOverlay.RemoveFromHierarchy();
            iconPickerOverlay = null;
        };
        btnRow.Add(closeBtn);

        card.Add(btnRow);
        iconPickerOverlay.Add(card);
        root.Add(iconPickerOverlay);
    }

    void CreatePickerIcon(UIE.VisualElement grid, int index)
    {
        var container = new UIE.Button();
        container.AddToClassList("picker-icon-container");
        container.AddToClassList(index == selectedIcon ? "picker-icon-selected" : "picker-icon-unselected");

        string iconName = (index >= 0 && index < iconNames.Length) ? iconNames[index] : iconNames[0];
        Sprite spr = Resources.Load<Sprite>($"Icons/{iconName}");

        if (spr != null)
        {
            var img = new UIE.VisualElement();
            img.AddToClassList("picker-icon-image");
            img.style.backgroundImage = new UIE.StyleBackground(spr);
            container.Add(img);
        }
        else
        {
            Color c = iconColors[Mathf.Clamp(index, 0, iconColors.Length - 1)];
            container.style.backgroundColor = c;
            var numLabel = UIHelper.CreateLabel((index + 1).ToString());
            numLabel.style.fontSize = 36;
            numLabel.style.color = Color.white;
            numLabel.AddToClassList("text-title");
            container.Add(numLabel);
        }

        int idx = index;
        container.clicked += () =>
        {
            SelectIcon(idx);
            iconPickerOverlay.RemoveFromHierarchy();
            iconPickerOverlay = null;
            ShowIconPicker();
        };

        grid.Add(container);
    }

    void SelectIcon(int index)
    {
        selectedIcon = index;
        ApplyAvatarSprite(index);
    }

    void CreateStartButton(UIE.VisualElement parent)
    {
        var wrapper = new UIE.VisualElement();
        wrapper.style.marginTop = 80;
        wrapper.style.alignItems = UIE.Align.Center;

        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("shadow-layer");
        shadow.style.borderTopLeftRadius = 60;
        shadow.style.borderTopRightRadius = 60;
        shadow.style.borderBottomLeftRadius = 60;
        shadow.style.borderBottomRightRadius = 60;

        startButton = UIHelper.CreatePillButton(
            isEditing ? Localization.Get("profile_save") : Localization.Get("profile_start"));
        startButton.clicked += OnStartPressed;

        wrapper.Add(shadow);
        wrapper.Add(startButton);
        parent.Add(wrapper);
    }

    void UpdateStartButton()
    {
        bool hasName = nameInputField != null && !string.IsNullOrEmpty(nameInputField.value.Trim());
        if (startButton != null)
        {
            startButton.SetEnabled(hasName);
            startButton.style.opacity = hasName ? 1f : 0.5f;
        }
    }

    void OnStartPressed()
    {
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.playerName = nameInputField.value.Trim();
            DataCarrier.Instance.playerIcon = selectedIcon;
            DataCarrier.Instance.SaveProfile();
        }

        if (isEditing)
            SceneManager.LoadScene("HomeScene");
        else
            StartCoroutine(CutsceneSequence());
    }

    IEnumerator ShowToast(string message)
    {
        var toast = new UIE.VisualElement();
        toast.AddToClassList("toast");

        var shadow = new UIE.VisualElement();
        shadow.AddToClassList("toast-shadow");
        toast.Add(shadow);

        var text = UIHelper.CreateLabel(message, "toast-text");
        toast.Add(text);

        root.Add(toast);

        yield return new WaitForSeconds(2f);

        toast.style.opacity = 0f;
        yield return new WaitForSeconds(0.5f);
        toast.RemoveFromHierarchy();
    }

    IEnumerator CutsceneSequence()
    {
        if (startButton != null)
            startButton.SetEnabled(false);

        string[] lines = new string[]
        {
            Localization.Get("cutscene_line1"),
            Localization.Get("cutscene_line2"),
        };

        var panel = new UIE.VisualElement();
        panel.AddToClassList("cutscene-panel");
        root.Add(panel);

        yield return null;
        panel.AddToClassList("cutscene-panel-visible");

        yield return new WaitForSeconds(1.0f);

        float lineSpacing = 140f;
        float startY = -((lines.Length - 1) * lineSpacing) / 2f;

        for (int i = 0; i < lines.Length; i++)
        {
            var textEl = new UIE.Label(lines[i]);
            textEl.AddToClassList("cutscene-text");
            UIHelper.ApplyFont(textEl);
            textEl.style.top = new UIE.StyleLength(new UIE.Length(50, UIE.LengthUnit.Percent));
            float yPos = startY + i * lineSpacing;
            textEl.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(-600, yPos));
            panel.Add(textEl);

            yield return null;
            textEl.AddToClassList("cutscene-text-visible");
            textEl.style.translate = new UIE.StyleTranslate(
                new UIE.Translate(new UIE.Length(-50, UIE.LengthUnit.Percent), yPos));

            if (i < lines.Length - 1)
                yield return new WaitForSeconds(2.0f);
        }

        yield return new WaitForSeconds(2.0f);

        // Fade out
        panel.style.opacity = 0f;
        panel.style.transitionProperty = new System.Collections.Generic.List<UIE.StylePropertyName>
            { new UIE.StylePropertyName("opacity") };
        panel.style.transitionDuration = new System.Collections.Generic.List<UIE.TimeValue>
            { new UIE.TimeValue(0.8f) };
        panel.style.opacity = 0f;

        yield return new WaitForSeconds(1.0f);

        SceneManager.LoadScene("BirthScene");
    }
}
