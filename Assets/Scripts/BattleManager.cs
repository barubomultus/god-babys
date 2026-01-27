using UnityEngine;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI statusText;
    public RectTransform babyFace;

    void Start()
    {
        // 1. DataCarrier（運び屋）が存在するか確認
        if (DataCarrier.Instance != null)
        {
            // 2. データを抽出
            int hp = DataCarrier.Instance.babyHp;
            int atk = DataCarrier.Instance.babyAtk;
            int weight = DataCarrier.Instance.babyWeight;

            // 3. 画面に表示
            statusText.text = $"<color=red><b>BATTLE START!</b></color>\nHP: {hp}\nATK: {atk}";

            // 4. 見た目のサイズもデータから再現
            float faceWidth = 1.0f + (weight - 3000) * 0.0002f;
            babyFace.localScale = new Vector3(faceWidth, 1.0f, 1.0f);
            
            Debug.Log("運び屋から赤ちゃんのデータを受け取りました！");
        }
        else
        {
            statusText.text = "データがありません\nBirthSceneから始めてください";
        }
    }
}