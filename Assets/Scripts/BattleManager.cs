using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI statusText;
    public RectTransform babyFace;
    public Canvas canvas;

    void Start()
    {
        if (DataCarrier.Instance != null)
        {
            int hp  = DataCarrier.Instance.babyHp;
            int atk = DataCarrier.Instance.babyAtk;
            int def = DataCarrier.Instance.babyDef;
            int weight = DataCarrier.Instance.babyWeight;

            statusText.text = $"<color=red><b>BATTLE START!</b></color>\nHP: {hp}  ATK: {atk}  DEF: {def}";

            float faceWidth = 1.0f + (weight - 3000) * 0.0002f;
            babyFace.localScale = new Vector3(faceWidth, 1.0f, 1.0f);
        }
        else
        {
            statusText.text = "データがありません\nBirthSceneから始めてください";
        }

        CreateMenuBar();
    }

    // ===== メニューバー（右上） =====

    void CreateMenuBar()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // メニューバーパネル（右上）
        var bar = new GameObject("MenuBar");
        bar.transform.SetParent(canvas.transform, false);

        var barRect = bar.AddComponent<RectTransform>();
        barRect.anchorMin = new Vector2(1, 1);
        barRect.anchorMax = new Vector2(1, 1);
        barRect.pivot = new Vector2(1, 1);
        barRect.anchoredPosition = new Vector2(-20, -20);
        barRect.sizeDelta = new Vector2(360, 60);

        var barBg = bar.AddComponent<Image>();
        barBg.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
        barBg.raycastTarget = false;

        var layout = bar.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 12;
        layout.padding = new RectOffset(12, 12, 6, 6);
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        // セーブボタン
        CreateMenuButton(bar.transform, "セーブ", OnSave);
        // トップへ戻るボタン
        CreateMenuButton(bar.transform, "トップへ", OnGoTop);
    }

    void CreateMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction action)
    {
        var btnObj = new GameObject(label);
        btnObj.transform.SetParent(parent, false);

        var btnImage = btnObj.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.45f, 1f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        var colors = btn.colors;
        colors.highlightedColor = new Color(0.45f, 0.45f, 0.65f, 1f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.35f, 1f);
        btn.colors = colors;

        btn.onClick.AddListener(action);

        // テキスト
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

    // ===== セーブ =====

    void OnSave()
    {
        if (DataCarrier.Instance == null)
        {
            Debug.LogWarning("DataCarrierが見つかりません");
            return;
        }

        var dc = DataCarrier.Instance;
        PlayerPrefs.SetInt("babyAtk", dc.babyAtk);
        PlayerPrefs.SetInt("babyDef", dc.babyDef);
        PlayerPrefs.SetInt("babyHp", dc.babyHp);
        PlayerPrefs.SetInt("babyAcademic", dc.babyAcademic);
        PlayerPrefs.SetInt("babyWeight", dc.babyWeight);
        PlayerPrefs.SetInt("babyAthletic", dc.babyAthletic);
        PlayerPrefs.SetInt("babyHeight", dc.babyHeight);
        PlayerPrefs.SetString("trait1", dc.trait1 ?? "");
        PlayerPrefs.SetString("trait2", dc.trait2 ?? "");
        PlayerPrefs.SetString("fatherName", dc.fatherName ?? "");
        PlayerPrefs.SetString("motherName", dc.motherName ?? "");
        PlayerPrefs.SetInt("hasSaveData", 1);
        PlayerPrefs.Save();

        Debug.Log("セーブ完了！");
        StartCoroutine(ShowSaveMessage());
    }

    System.Collections.IEnumerator ShowSaveMessage()
    {
        string original = statusText.text;
        statusText.text = "<color=green><b>セーブしました！</b></color>";
        yield return new WaitForSeconds(1.2f);
        statusText.text = original;
    }

    // ===== トップへ戻る =====

    void OnGoTop()
    {
        SceneManager.LoadScene("BirthScene");
    }
}
