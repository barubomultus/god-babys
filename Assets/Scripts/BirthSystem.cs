using UnityEngine;
using TMPro;
using UnityEngine.UI; // Imageを操作するために必要です

public class BirthSystem : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI childStatusText;

    [Header("Baby Face Visuals")]
    public RectTransform babyFace;  // 顔の土台 (Image)
    public Image babyEyes;         // 目 (Image)

    public void SpinRoulette()
    {
        // --- 1. 両親のステータス生成 ---
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

        // --- 3. 見た目の反映 (ここが新規追加！) ---
        ApplyVisuals(c_weight, c_academic);

        // --- 4. 文字列の組み立て ---
        string result = "<color=yellow><b>[ A NEW LIFE IS BORN! ]</b></color>\n\n";
        result += $"<b>Father:</b> Ht:{f_height} / Acad:{f_academic} / Atk:{f_atk} / Def:{f_def}\n";
        result += $"<b>Mother:</b> Ht:{m_height} / Acad:{m_academic} / Atk:{m_atk} / Def:{m_def}\n";
        result += "--------------------------------------------------\n";
        result += $"<size=120%><b>NEWBORN STATS:</b></size>\n";
        result += $"<b>Height:</b> {c_height} cm  /  <b>Weight:</b> {c_weight} g\n";
        result += $"<b>HP:</b> {c_hp} / <b>ATK:</b> {c_atk} / <b>DEF:</b> {c_def}\n";
        result += $"<b>Academic:</b> {c_academic} / <b>Athletic:</b> {c_athletic}";

        childStatusText.text = result;
        Debug.Log("New life and visuals generated!");
    }

    // 見た目を変更するための専用関数
    void ApplyVisuals(int weight, int academic)
    {
        // 1. 体重で顔の横幅を変える
        // 3000gを基準(1.0)として、1gごとに0.0002倍変化させる
        float faceWidth = 1.0f + (weight - 3000) * 0.0002f;
        babyFace.localScale = new Vector3(faceWidth, 1.0f, 1.0f);

        // 2. 学力で目の色を変える (高いと水色、低いと赤)
        float iqFactor = Mathf.Clamp01(academic / 100f);
        babyEyes.color = Color.Lerp(Color.red, Color.cyan, iqFactor);

        // 3. 学力が高いと目が少し大きくなる
        float eyeScale = 0.8f + (iqFactor * 0.4f);
        babyEyes.rectTransform.localScale = new Vector3(eyeScale, eyeScale, 1f);
    }
}