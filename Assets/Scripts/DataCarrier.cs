using UnityEngine;
using System.IO;

public class DataCarrier : MonoBehaviour
{
    public static DataCarrier Instance;
    public const int MAX_SAVE_SLOTS = 5;

    [Header("Baby Stats")]
    public int babyAtk;
    public int babyDef;
    public int babyHp;
    public int babyIntelligence;
    public int babyAthletic;
    public int babyLuck;
    public int babyFortune;

    [Header("Age & Experience")]
    public int babyAge = 0;
    public int babyExp = 0;
    public int defeatedEnemies = 0;

    [Header("Traits & Parents")]
    public string babyName;
    public string trait1;
    public string fatherName;
    public string motherName;
    public string babyGender;
    public bool isGodBaby;
    public bool cameFromMap = false;
    public bool isBossBattle = false;
    public int currentArea = 0;     // 0=村, 1=悪魔村, 2=デヴィル夫人のやかた, 3=小悪魔の街, 4=109
    public string fixedEncounterEnemy = "";  // 固定エンカウント敵名（空=ランダム）
    public int babyCurrentHp = -1;  // 戦闘間HP持越し（-1=maxHP）
    public int babyPoisonTurns = 0;  // 毒残りターン
    public string customBabyImagePath = ""; // persistentDataPath内のカスタム画像ファイル名

    [Header("Player Profile")]
    public string playerName = "";
    public int playerIcon = 0;

    [Header("Inventory")]
    public string inventory = "";

    [Header("Defeated Enemies (縁の書)")]
    public string defeatedEnemyList = "";

    // マップ上のプレイヤー位置（復帰用）
    public int mapPlayerX = 10;
    public int mapPlayerY = 7;

    // シーン遷移演出フラグ（セーブ不要・一時的）
    [System.NonSerialized] public bool pendingWipeIn = false;

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
        PlayerPrefs.SetInt(p + "babyIntelligence", babyIntelligence);
        PlayerPrefs.SetInt(p + "babyAthletic", babyAthletic);
        PlayerPrefs.SetInt(p + "babyLuck", babyLuck);
        PlayerPrefs.SetInt(p + "babyFortune", babyFortune);
        PlayerPrefs.SetInt(p + "babyAge", babyAge);
        PlayerPrefs.SetInt(p + "babyExp", babyExp);
        PlayerPrefs.SetInt(p + "defeatedEnemies", defeatedEnemies);
        PlayerPrefs.SetString(p + "babyName", babyName);
        PlayerPrefs.SetString(p + "trait1", trait1);
        PlayerPrefs.SetString(p + "fatherName", fatherName);
        PlayerPrefs.SetString(p + "motherName", motherName);
        PlayerPrefs.SetString(p + "babyGender", babyGender);
        PlayerPrefs.SetInt(p + "isGodBaby", isGodBaby ? 1 : 0);
        PlayerPrefs.SetInt(p + "cameFromMap", cameFromMap ? 1 : 0);
        PlayerPrefs.SetInt(p + "isBossBattle", isBossBattle ? 1 : 0);
        PlayerPrefs.SetInt(p + "mapPlayerX", mapPlayerX);
        PlayerPrefs.SetInt(p + "mapPlayerY", mapPlayerY);
        PlayerPrefs.SetInt(p + "currentArea", currentArea);
        PlayerPrefs.SetInt(p + "babyCurrentHp", babyCurrentHp);
        PlayerPrefs.SetInt(p + "babyPoisonTurns", babyPoisonTurns);
        PlayerPrefs.SetString(p + "inventory", inventory);
        PlayerPrefs.SetString(p + "defeatedEnemyList", defeatedEnemyList);
        PlayerPrefs.SetString(p + "fixedEncounterEnemy", fixedEncounterEnemy);
        PlayerPrefs.SetString(p + "customBabyImagePath", customBabyImagePath);
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
        babyIntelligence = PlayerPrefs.GetInt(p + "babyIntelligence", 0);
        babyAthletic = PlayerPrefs.GetInt(p + "babyAthletic", 0);
        babyLuck = PlayerPrefs.GetInt(p + "babyLuck", 0);
        babyFortune = PlayerPrefs.GetInt(p + "babyFortune", 0);
        babyAge = PlayerPrefs.GetInt(p + "babyAge", 0);
        babyExp = PlayerPrefs.GetInt(p + "babyExp", 0);
        defeatedEnemies = PlayerPrefs.GetInt(p + "defeatedEnemies", 0);
        babyName = PlayerPrefs.GetString(p + "babyName", "");
        trait1 = PlayerPrefs.GetString(p + "trait1", "");
        fatherName = PlayerPrefs.GetString(p + "fatherName", "");
        motherName = PlayerPrefs.GetString(p + "motherName", "");
        babyGender = PlayerPrefs.GetString(p + "babyGender", "");
        isGodBaby = PlayerPrefs.GetInt(p + "isGodBaby", 0) == 1;
        cameFromMap = PlayerPrefs.GetInt(p + "cameFromMap", 0) == 1;
        isBossBattle = PlayerPrefs.GetInt(p + "isBossBattle", 0) == 1;
        mapPlayerX = PlayerPrefs.GetInt(p + "mapPlayerX", 10);
        mapPlayerY = PlayerPrefs.GetInt(p + "mapPlayerY", 7);
        currentArea = PlayerPrefs.GetInt(p + "currentArea", 0);
        babyCurrentHp = PlayerPrefs.GetInt(p + "babyCurrentHp", -1);
        babyPoisonTurns = PlayerPrefs.GetInt(p + "babyPoisonTurns", 0);
        inventory = PlayerPrefs.GetString(p + "inventory", "");
        defeatedEnemyList = PlayerPrefs.GetString(p + "defeatedEnemyList", "");
        fixedEncounterEnemy = PlayerPrefs.GetString(p + "fixedEncounterEnemy", "");
        customBabyImagePath = PlayerPrefs.GetString(p + "customBabyImagePath", "");
        // playerName/playerIcon はグローバルプロフィールから読む
        LoadProfile();
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
        PlayerPrefs.DeleteKey(p + "babyIntelligence");
        PlayerPrefs.DeleteKey(p + "babyAthletic");
        PlayerPrefs.DeleteKey(p + "babyLuck");
        PlayerPrefs.DeleteKey(p + "babyFortune");
        PlayerPrefs.DeleteKey(p + "babyAge");
        PlayerPrefs.DeleteKey(p + "babyExp");
        PlayerPrefs.DeleteKey(p + "defeatedEnemies");
        PlayerPrefs.DeleteKey(p + "babyName");
        PlayerPrefs.DeleteKey(p + "trait1");
        PlayerPrefs.DeleteKey(p + "fatherName");
        PlayerPrefs.DeleteKey(p + "motherName");
        PlayerPrefs.DeleteKey(p + "babyGender");
        PlayerPrefs.DeleteKey(p + "isGodBaby");
        PlayerPrefs.DeleteKey(p + "cameFromMap");
        PlayerPrefs.DeleteKey(p + "isBossBattle");
        PlayerPrefs.DeleteKey(p + "mapPlayerX");
        PlayerPrefs.DeleteKey(p + "mapPlayerY");
        PlayerPrefs.DeleteKey(p + "currentArea");
        PlayerPrefs.DeleteKey(p + "babyCurrentHp");
        PlayerPrefs.DeleteKey(p + "babyPoisonTurns");
        PlayerPrefs.DeleteKey(p + "inventory");
        PlayerPrefs.DeleteKey(p + "defeatedEnemyList");
        PlayerPrefs.DeleteKey(p + "fixedEncounterEnemy");
        PlayerPrefs.DeleteKey(p + "customBabyImagePath");
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

    // ===== 縁の書（倒した敵管理） =====

    public bool HasDefeatedEnemy(string enemyName)
    {
        if (string.IsNullOrEmpty(defeatedEnemyList)) return false;
        foreach (var e in defeatedEnemyList.Split(','))
        {
            if (e == enemyName) return true;
        }
        return false;
    }

    /// <summary>
    /// Returns true if this enemy was newly added (first defeat)
    /// </summary>
    public bool AddDefeatedEnemy(string enemyName)
    {
        if (HasDefeatedEnemy(enemyName)) return false;
        if (string.IsNullOrEmpty(defeatedEnemyList))
            defeatedEnemyList = enemyName;
        else
            defeatedEnemyList += "," + enemyName;
        return true;
    }

    public string[] GetDefeatedEnemyList()
    {
        if (string.IsNullOrEmpty(defeatedEnemyList)) return new string[0];
        return defeatedEnemyList.Split(',');
    }

    // カスタム赤ちゃん画像の読み込み
    public static Sprite LoadCustomBabySprite()
    {
        if (Instance == null) return null;
        string path = Instance.customBabyImagePath;
        if (string.IsNullOrEmpty(path)) return null;

        string fullPath = Path.Combine(Application.persistentDataPath, path);
        if (!File.Exists(fullPath)) return null;

        byte[] data = File.ReadAllBytes(fullPath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(data);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    // 次の月齢に必要な経験値
    public static int ExpForNextAge(int age)
    {
        return 120 + age * 70;
    }

    // 月齢アップ時のステータス成長
    public void AgeUp()
    {
        babyAge++;
        float growthRate = 1.0f + (babyAge * 0.05f);
        babyAtk += Mathf.RoundToInt(2 * growthRate);
        babyDef += Mathf.RoundToInt(1 * growthRate);
        babyHp += Mathf.RoundToInt(8 * growthRate);
        // babyIntelligence は生まれつきの値で変化しない
        babyAthletic += Mathf.RoundToInt(2 * growthRate);
    }

    // ===== グローバルプロフィール =====

    public void SaveProfile()
    {
        PlayerPrefs.SetString("global_playerName", playerName);
        PlayerPrefs.SetInt("global_playerIcon", playerIcon);
        PlayerPrefs.SetInt("global_profileExists", 1);
        PlayerPrefs.Save();
    }

    public void LoadProfile()
    {
        playerName = PlayerPrefs.GetString("global_playerName", "");
        playerIcon = PlayerPrefs.GetInt("global_playerIcon", 0);
    }

    public static bool HasProfile()
    {
        return PlayerPrefs.GetInt("global_profileExists", 0) == 1;
    }

    public static void DeleteProfile()
    {
        PlayerPrefs.DeleteKey("global_playerName");
        PlayerPrefs.DeleteKey("global_playerIcon");
        PlayerPrefs.DeleteKey("global_profileExists");
        PlayerPrefs.Save();
    }

    public static string GetProfileName()
    {
        return PlayerPrefs.GetString("global_playerName", "");
    }

    public static int GetProfileIcon()
    {
        return PlayerPrefs.GetInt("global_playerIcon", 0);
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

    public static string GetSlotBabyGender(int slot)
    {
        return PlayerPrefs.GetString($"slot{slot}_babyGender", "");
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