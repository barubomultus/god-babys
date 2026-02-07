using UnityEngine;

public class DataCarrier : MonoBehaviour
{
    public static DataCarrier Instance;
    public const int MAX_SAVE_SLOTS = 5;

    [Header("Baby Stats")]
    public int babyAtk;
    public int babyDef;
    public int babyHp;
    public int babyAcademic;
    public int babyWeight;
    public int babyAthletic;
    public int babyHeight;

    [Header("Age & Experience")]
    public int babyAge = 0;
    public int babyExp = 0;
    public int defeatedEnemies = 0;

    [Header("Traits & Parents")]
    public string babyName;
    public string trait1;
    public string trait2;
    public string fatherName;
    public string motherName;
    public string babyGender;
    public bool isGodBaby;
    public bool cameFromMap = false;
    public bool isBossBattle = false;

    [Header("Inventory")]
    public string inventory = "";

    // マップ上のプレイヤー位置（復帰用）
    public int mapPlayerX = 10;
    public int mapPlayerY = 7;

    // 現在のセーブスロット
    public int currentSlot = -1;

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

    // ===== スロット指定セーブ/ロード =====

    string SlotKey(int slot, string key) => $"slot{slot}_{key}";

    public void SaveToSlot(int slot)
    {
        currentSlot = slot;
        string p = $"slot{slot}_";
        PlayerPrefs.SetInt(p + "babyAtk", babyAtk);
        PlayerPrefs.SetInt(p + "babyDef", babyDef);
        PlayerPrefs.SetInt(p + "babyHp", babyHp);
        PlayerPrefs.SetInt(p + "babyAcademic", babyAcademic);
        PlayerPrefs.SetInt(p + "babyWeight", babyWeight);
        PlayerPrefs.SetInt(p + "babyAthletic", babyAthletic);
        PlayerPrefs.SetInt(p + "babyHeight", babyHeight);
        PlayerPrefs.SetInt(p + "babyAge", babyAge);
        PlayerPrefs.SetInt(p + "babyExp", babyExp);
        PlayerPrefs.SetInt(p + "defeatedEnemies", defeatedEnemies);
        PlayerPrefs.SetString(p + "babyName", babyName);
        PlayerPrefs.SetString(p + "trait1", trait1);
        PlayerPrefs.SetString(p + "trait2", trait2);
        PlayerPrefs.SetString(p + "fatherName", fatherName);
        PlayerPrefs.SetString(p + "motherName", motherName);
        PlayerPrefs.SetString(p + "babyGender", babyGender);
        PlayerPrefs.SetInt(p + "isGodBaby", isGodBaby ? 1 : 0);
        PlayerPrefs.SetInt(p + "cameFromMap", cameFromMap ? 1 : 0);
        PlayerPrefs.SetInt(p + "isBossBattle", isBossBattle ? 1 : 0);
        PlayerPrefs.SetInt(p + "mapPlayerX", mapPlayerX);
        PlayerPrefs.SetInt(p + "mapPlayerY", mapPlayerY);
        PlayerPrefs.SetString(p + "inventory", inventory);
        PlayerPrefs.SetInt(p + "exists", 1);
        PlayerPrefs.Save();
    }

    public void LoadFromSlot(int slot)
    {
        currentSlot = slot;
        string p = $"slot{slot}_";
        babyAtk = PlayerPrefs.GetInt(p + "babyAtk", 0);
        babyDef = PlayerPrefs.GetInt(p + "babyDef", 0);
        babyHp = PlayerPrefs.GetInt(p + "babyHp", 0);
        babyAcademic = PlayerPrefs.GetInt(p + "babyAcademic", 0);
        babyWeight = PlayerPrefs.GetInt(p + "babyWeight", 0);
        babyAthletic = PlayerPrefs.GetInt(p + "babyAthletic", 0);
        babyHeight = PlayerPrefs.GetInt(p + "babyHeight", 0);
        babyAge = PlayerPrefs.GetInt(p + "babyAge", 0);
        babyExp = PlayerPrefs.GetInt(p + "babyExp", 0);
        defeatedEnemies = PlayerPrefs.GetInt(p + "defeatedEnemies", 0);
        babyName = PlayerPrefs.GetString(p + "babyName", "");
        trait1 = PlayerPrefs.GetString(p + "trait1", "");
        trait2 = PlayerPrefs.GetString(p + "trait2", "");
        fatherName = PlayerPrefs.GetString(p + "fatherName", "");
        motherName = PlayerPrefs.GetString(p + "motherName", "");
        babyGender = PlayerPrefs.GetString(p + "babyGender", "");
        isGodBaby = PlayerPrefs.GetInt(p + "isGodBaby", 0) == 1;
        cameFromMap = PlayerPrefs.GetInt(p + "cameFromMap", 0) == 1;
        isBossBattle = PlayerPrefs.GetInt(p + "isBossBattle", 0) == 1;
        mapPlayerX = PlayerPrefs.GetInt(p + "mapPlayerX", 10);
        mapPlayerY = PlayerPrefs.GetInt(p + "mapPlayerY", 7);
        inventory = PlayerPrefs.GetString(p + "inventory", "");
    }

    // 現在のスロットにセーブ（スロット未設定なら空きスロットを探す）
    public void SaveData()
    {
        if (currentSlot < 0)
        {
            currentSlot = FindEmptySlot();
            if (currentSlot < 0) currentSlot = 0; // 空きがなければスロット0に上書き
        }
        SaveToSlot(currentSlot);
    }

    public void LoadData()
    {
        if (currentSlot >= 0)
            LoadFromSlot(currentSlot);
    }

    // 空きスロットを探す
    public static int FindEmptySlot()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            if (!SlotExists(i)) return i;
        }
        return -1;
    }

    // スロット削除
    public static void DeleteSlot(int slot)
    {
        string p = $"slot{slot}_";
        PlayerPrefs.DeleteKey(p + "babyAtk");
        PlayerPrefs.DeleteKey(p + "babyDef");
        PlayerPrefs.DeleteKey(p + "babyHp");
        PlayerPrefs.DeleteKey(p + "babyAcademic");
        PlayerPrefs.DeleteKey(p + "babyWeight");
        PlayerPrefs.DeleteKey(p + "babyAthletic");
        PlayerPrefs.DeleteKey(p + "babyHeight");
        PlayerPrefs.DeleteKey(p + "babyAge");
        PlayerPrefs.DeleteKey(p + "babyExp");
        PlayerPrefs.DeleteKey(p + "defeatedEnemies");
        PlayerPrefs.DeleteKey(p + "babyName");
        PlayerPrefs.DeleteKey(p + "trait1");
        PlayerPrefs.DeleteKey(p + "trait2");
        PlayerPrefs.DeleteKey(p + "fatherName");
        PlayerPrefs.DeleteKey(p + "motherName");
        PlayerPrefs.DeleteKey(p + "babyGender");
        PlayerPrefs.DeleteKey(p + "isGodBaby");
        PlayerPrefs.DeleteKey(p + "cameFromMap");
        PlayerPrefs.DeleteKey(p + "isBossBattle");
        PlayerPrefs.DeleteKey(p + "mapPlayerX");
        PlayerPrefs.DeleteKey(p + "mapPlayerY");
        PlayerPrefs.DeleteKey(p + "inventory");
        PlayerPrefs.DeleteKey(p + "exists");
        PlayerPrefs.Save();
    }

    // ===== 持ち物管理 =====

    public void AddItem(string item)
    {
        if (string.IsNullOrEmpty(inventory))
            inventory = item;
        else
            inventory += "," + item;
    }

    public bool HasItem(string item)
    {
        if (string.IsNullOrEmpty(inventory)) return false;
        foreach (var i in inventory.Split(','))
        {
            if (i == item) return true;
        }
        return false;
    }

    public string[] GetItemList()
    {
        if (string.IsNullOrEmpty(inventory)) return new string[0];
        return inventory.Split(',');
    }

    // 次の年齢に必要な累計経験値
    public static int ExpForNextAge(int age)
    {
        return 300 + age * 250;
    }

    // 年齢アップ時のステータス成長
    public void AgeUp()
    {
        babyAge++;
        float growthRate = 1.0f + (babyAge * 0.1f);
        babyAtk += Mathf.RoundToInt(5 * growthRate);
        babyDef += Mathf.RoundToInt(3 * growthRate);
        babyHp += Mathf.RoundToInt(20 * growthRate);
        babyAcademic += Mathf.RoundToInt(4 * growthRate);
        babyAthletic += Mathf.RoundToInt(4 * growthRate);
        babyHeight += Mathf.RoundToInt(8 + Random.Range(0, 5));
        babyWeight += Mathf.RoundToInt(2000 + Random.Range(0, 1000));
    }

    // ===== スロット情報取得（静的メソッド） =====

    public static bool SlotExists(int slot)
    {
        return PlayerPrefs.GetInt($"slot{slot}_exists", 0) == 1;
    }

    public static string GetSlotBabyName(int slot)
    {
        return PlayerPrefs.GetString($"slot{slot}_babyName", "");
    }

    public static int GetSlotAge(int slot)
    {
        return PlayerPrefs.GetInt($"slot{slot}_babyAge", 0);
    }

    public static string GetSlotFatherName(int slot)
    {
        return PlayerPrefs.GetString($"slot{slot}_fatherName", "");
    }

    public static string GetSlotMotherName(int slot)
    {
        return PlayerPrefs.GetString($"slot{slot}_motherName", "");
    }

    public static bool GetSlotIsGodBaby(int slot)
    {
        return PlayerPrefs.GetInt($"slot{slot}_isGodBaby", 0) == 1;
    }

    public static bool HasAnySaveData()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            if (SlotExists(i)) return true;
        }
        return false;
    }
}