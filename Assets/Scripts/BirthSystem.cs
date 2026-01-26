using UnityEngine;
using TMPro; // Required for TextMeshPro

public class BirthSystem : MonoBehaviour
{
    // Link your StatusText object here in the Inspector
    public TextMeshProUGUI childStatusText;

    // This function is called when the GenerateButton is clicked
    public void SpinRoulette()
    {
        // 1. Generate Father's Stats
        int f_atk = Random.Range(20, 80);
        int f_def = Random.Range(20, 80);
        int f_hp = Random.Range(100, 200);
        int f_height = Random.Range(160, 190);
        int f_academic = Random.Range(30, 95);
        int f_weight = Random.Range(55, 90);
        int f_athletic = Random.Range(30, 90);

        // 2. Generate Mother's Stats
        int m_atk = Random.Range(15, 70);
        int m_def = Random.Range(25, 85);
        int m_hp = Random.Range(80, 170);
        int m_height = Random.Range(145, 175);
        int m_academic = Random.Range(40, 100);
        int m_weight = Random.Range(45, 70);
        int m_athletic = Random.Range(30, 90);

        // 3. Inheritance Calculation for the Baby (Average + Random Variation)
        int c_atk = (f_atk + m_atk) / 2 + Random.Range(-5, 6);
        int c_def = (f_def + m_def) / 2 + Random.Range(-5, 6);
        int c_hp = (f_hp + m_hp) / 2 + Random.Range(-10, 11);
        int c_academic = (f_academic + m_academic) / 2 + Random.Range(-5, 6);
        int c_athletic = (f_athletic + m_athletic) / 2 + Random.Range(-5, 6);
        
        // Typical newborn sizes
        int c_height = 50 + Random.Range(-3, 4); // Around 50cm
        int c_weight = 3000 + Random.Range(-500, 501); // Around 3000g

        // 4. Build the Display String
        string result = "<color=yellow><b>[ A NEW LIFE IS BORN! ]</b></color>\n\n";
        
        result += $"<b>Father:</b> Ht:{f_height} / Acad:{f_academic} / Atk:{f_atk} / Def:{f_def}\n";
        result += $"<b>Mother:</b> Ht:{m_height} / Acad:{m_academic} / Atk:{m_atk} / Def:{m_def}\n";
        
        result += "--------------------------------------------------\n";
        
        result += $"<size=120%><b>NEWBORN STATS:</b></size>\n";
        result += $"<b>Height:</b> {c_height} cm  /  <b>Weight:</b> {c_weight} g\n";
        result += $"<b>HP:</b> {c_hp} / <b>ATK:</b> {c_atk} / <b>DEF:</b> {c_def}\n";
        result += $"<b>Academic:</b> {c_academic} / <b>Athletic:</b> {c_athletic}";

        // 5. Update the UI
        childStatusText.text = result;

        // Log to console for debugging
        Debug.Log("New life generated successfully!");
    }
}