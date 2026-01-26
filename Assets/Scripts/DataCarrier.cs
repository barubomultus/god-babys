using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    public static DataCarrier Instance;

    // 保存したい赤ちゃんのデータ
    public int babyAtk;
    public int babyHp;
    public string babyName = "Baby Hero";
    // ...他に必要なステータス

    void Awake()
    {
        // シーンを切り替えてもこのオブジェクトを消さない設定（超重要！）
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}