using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TitleManager : MonoBehaviour
{
    private readonly string[] introLines = new string[]
    {
        "あなたは──「神」である。",
        "この世界では、選ばれし父と母の遺伝子が交わり\n新たな命が誕生する。",
        "生まれてくる赤ちゃんの能力は\n両親のDNAと、運命のルーレットで決まる。",
        "最強の赤ちゃんを生み出し\nバトルに勝利せよ。"
    };

    private const float slideDuration = 1.5f;
    private const float lineInterval = 1.0f;
    private const float endWaitTime = 2.0f;
    private const float startOffsetX = -800f;

    public void StartGame()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        // キャンバスを探すか、なければ生成
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("IntroCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 暗い背景パネルを動的生成
        GameObject panel = new GameObject("IntroPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0f);

        // パネルのフェードイン
        float fadeInDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.9f, elapsed / fadeInDuration);
            panelImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);

        // 各行を順番にスライドイン表示
        float verticalStart = 100f;
        float lineSpacing = 120f;

        for (int i = 0; i < introLines.Length; i++)
        {
            GameObject textObj = new GameObject("IntroText_" + i);
            textObj.transform.SetParent(panel.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();

            tmp.text = introLines[i];
            tmp.fontSize = 36;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;

            float yPos = verticalStart - (i * lineSpacing);
            textRect.sizeDelta = new Vector2(800f, 100f);
            textRect.anchoredPosition = new Vector2(startOffsetX, yPos);

            // 左からスライドイン (Lerp)
            float slideElapsed = 0f;
            Vector2 startPos = new Vector2(startOffsetX, yPos);
            Vector2 endPos = new Vector2(0f, yPos);

            while (slideElapsed < slideDuration)
            {
                slideElapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, slideElapsed / slideDuration);
                textRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }
            textRect.anchoredPosition = endPos;

            // 次の行までの間隔
            if (i < introLines.Length - 1)
            {
                yield return new WaitForSeconds(lineInterval);
            }
        }

        // 全行表示後、少し待つ
        yield return new WaitForSeconds(endWaitTime);

        // フェードアウトしてシーン遷移
        float fadeOutDuration = 1.0f;
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        SceneManager.LoadScene("BirthScene");
    }
}
