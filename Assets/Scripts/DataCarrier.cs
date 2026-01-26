using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    public static DataCarrier Instance;

    [Header("Baby Stats")]
    public int babyAtk;
    public int babyDef;      // これが足りなかった！
    public int babyHp;
    public int babyAcademic; // これが足りなかった！
    public int babyWeight;   // これが足りなかった！

    void Awake()
    {
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