using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // シーン移動用

public class BirthSystem : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI childStatusText;

    [Header("Baby Face Visuals")]
    public RectTransform babyFace;
    public Image babyEyes;

    [Header("Visual Resources")]
    public Sprite[] eyeSprites;
    public Sprite[] mouthSprites;

    [Header("Body Part References")]
    public Image babyEyeImage;
    public Image babyMouthImage;

    public void SpinRoulette()
    {
        // --- 1. 両親のステータス生成 (省略なし) ---
        int f_atk = Random.Range(20, 80);
        int f_def = Random.Range(20, 80);
        int f_hp = Random.Range(100, 200);
        int f_height = Random.Range(160, 190);
        int f_academic = Random.Range(30, 95);
        int f_weight = Random.Range(55, 90);
        int f_athletic = Random.Range(30, 90);

        int m_atk = Random.Range(15, 70);
        int m_def = Random.Range(25, 85);
        int m_hp = Random.Range(80, 170);
        int m_height = Random.Range(145, 175);
        int m_academic = Random.Range(40, 100);
        int m_weight = Random.Range(45, 70);
        int m_athletic = Random.Range(30, 90);

        // --- 2. 赤ちゃんのステータス計算 ---
        int c_atk = (f_atk + m_atk) / 2 + Random.Range(-5, 6);
        int c_def = (f_def + m_def) / 2 + Random.Range(-5, 6);
        int c_hp = (f_hp + m_hp) / 2 + Random.Range(-10, 11);
        int c_academic = (f_academic + m_academic) / 2 + Random.Range(-5, 6);
        int c_athletic = (f_athletic + m_athletic) / 2 + Random.Range(-5, 6);
        int c_height = 50 + Random.Range(-3, 4);
        int c_weight = 3000 + Random.Range(-500, 501);

        // --- 3. 見た目の反映 ---
        ApplyVisuals(c_weight, c_academic);

        // --- 4. UIテキスト表示 ---
        string result = "<color=yellow><b>[ A NEW LIFE IS BORN! ]</b></color>\n\n";
        result += $"<b>Father:</b> Ht:{f_height} / Acad:{f_academic} / Atk:{f_atk} / Def:{f_def}\n";
        result += $"<b>Mother:</b> Ht:{m_height} / Acad:{m_academic} / Atk:{m_atk} / Def:{m_def}\n";
        result += "--------------------------------------------------\n";
        result += $"<size=120%><b>NEWBORN STATS:</b></size>\n";
        result += $"<b>Height:</b> {c_height} cm  /  <b>Weight:</b> {c_weight} g\n";
        result += $"<b>HP:</b> {c_hp} / <b>ATK:</b> {c_atk} / <b>DEF:</b> {c_def}\n";
        result += $"<b>Academic:</b> {c_academic} / <b>Athletic:</b> {c_athletic}";

        childStatusText.text = result;

        // ★ここが「データを書き込む」重要な処理です
        if (DataCarrier.Instance != null)
        {
            DataCarrier.Instance.babyAtk = c_atk;
            DataCarrier.Instance.babyDef = c_def;
            DataCarrier.Instance.babyHp = c_hp;
            DataCarrier.Instance.babyAcademic = c_academic;
            DataCarrier.Instance.babyWeight = c_weight;
            
            Debug.Log($"Data saved to Carrier: Atk={c_atk}, Hp={c_hp}");
        }
        else
        {
            Debug.LogError("DataCarrier.Instance is null! Make sure DataCarrier is in the Hierarchy.");
        }
    }

    // シーン移動用
    public void GoToBattle()
    {
        SceneManager.LoadScene("BattleScene");
    }

    void ApplyVisuals(int weight, int academic)
    {
        // 体重で顔の横幅を変える
        float faceWidth = 1.0f + (weight - 3000) * 0.0002f;
        babyFace.localScale = new Vector3(faceWidth, 1.0f, 1.0f);

        // 学力で目の色を変える
        float iqFactor = Mathf.Clamp01(academic / 100f);
        babyEyes.color = Color.Lerp(Color.red, Color.cyan, iqFactor);

        // ランダムな目の画像差し替え (配列に画像がある場合)
        if (eyeSprites.Length > 0 && babyEyeImage != null)
        {
            babyEyeImage.sprite = eyeSprites[Random.Range(0, eyeSprites.Length)];
        }
    }
}