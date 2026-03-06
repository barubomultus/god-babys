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
    public int babyHeight;
    public int babyWeight;

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
    public int currentArea = 0;     // 0=よちよちの里, 1=ゴージャス・ヴィレッジ, 2=デヴィル夫人のやかた, 3=小悪魔の街, 4=109, 5=実家, 6=武器屋, 7=塾, 8=ステラ・オリジン
    public int shopEntryArea = 1;    // おあそびどうぐやに入った元エリア（戻り先）
    public string fixedEncounterEnemy = "";  // 固定エンカウント敵名（空=ランダム）
    public int babyCurrentHp = -1;  // おあそび間HP持越し（-1=maxHP）
    public int babyPoisonTurns = 0;  // 毒残りターン
    public string customBabyImagePath = ""; // persistentDataPath内のカスタム画像ファイル名
    public string synthBabyImagePath = ""; // BabySynthesizer合成結果の画像ファイル名
    [System.NonSerialized] public Texture2D battleBgTexture; // マップスクショ（バトル背景用、保存不要）

    [Header("Player Profile")]
    public string playerName = "";
    public int playerIcon = 0;

    [Header("Inventory")]
    public string inventory = "";

    [Header("Equipment")]
    public string equipment = "";       // 所持装備（購入済み全て）
    public string equippedSlots = "";   // 装備中（最大3つ）
    public bool motherGaveItem = false;
    public int goldenEggOldManState = 0; // 0=待機, 1=変身済み
    public int kaguyaMetCount = 0;   // 0=未遭遇, 1=1回目済, 2=2回目済, 3=バトル前
    public bool kaguyaLover = false; // 相思相愛
    public int stellaOriginProgress = 0; // ビットフラグ: 1=DEFバフ済, 2=ATKバフ済, 4=ゆりかご済
    public string stellaOriginTitle = ""; // ゆりかごで選んだ称号

    [Header("Currency")]
    public int milk = 0;

    [Header("Defeated Enemies (縁の書)")]
    public string defeatedEnemyList = "";

    [Header("Met NPCs (縁の書)")]
    public string metNpcList = "";

    // マップ上のプレイヤー位置（復帰用）
    public int mapPlayerX = 10;
    public int mapPlayerY = 7;

    // シーン遷移演出フラグ（セーブ不要・一時的）
    [System.NonSerialized] public bool pendingWipeIn = false;

    // 現在のセーブスロット
    public int currentSlot = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        Debug.Log("[STARTUP] ★ RuntimeInitialize BeforeSceneLoad — C# scripts initialized");
    }

    void Awake()
    {
        Debug.Log("[STARTUP] DataCarrier.Awake() called");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProfile();
            Debug.Log("[STARTUP] DataCarrier initialized, profile loaded");
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
        PlayerPrefs.SetInt(p + "babyHeight", babyHeight);
        PlayerPrefs.SetInt(p + "babyWeight", babyWeight);
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
        PlayerPrefs.SetInt(p + "shopEntryArea", shopEntryArea);
        PlayerPrefs.SetInt(p + "babyCurrentHp", babyCurrentHp);
        PlayerPrefs.SetInt(p + "babyPoisonTurns", babyPoisonTurns);
        PlayerPrefs.SetString(p + "inventory", inventory);
        PlayerPrefs.SetString(p + "equipment", equipment);
        PlayerPrefs.SetString(p + "equippedSlots", equippedSlots);
        PlayerPrefs.SetInt(p + "motherGaveItem", motherGaveItem ? 1 : 0);
        PlayerPrefs.SetInt(p + "goldenEggOldManState", goldenEggOldManState);
        PlayerPrefs.SetInt(p + "kaguyaMetCount", kaguyaMetCount);
        PlayerPrefs.SetInt(p + "kaguyaLover", kaguyaLover ? 1 : 0);
        PlayerPrefs.SetInt(p + "stellaOriginProgress", stellaOriginProgress);
        PlayerPrefs.SetString(p + "stellaOriginTitle", stellaOriginTitle);
        PlayerPrefs.SetString(p + "defeatedEnemyList", defeatedEnemyList);
        PlayerPrefs.SetString(p + "metNpcList", metNpcList);
        PlayerPrefs.SetString(p + "fixedEncounterEnemy", fixedEncounterEnemy);
        PlayerPrefs.SetString(p + "customBabyImagePath", customBabyImagePath);
        PlayerPrefs.SetString(p + "synthBabyImagePath", synthBabyImagePath);
        PlayerPrefs.SetInt(p + "milk", milk);
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
        babyHeight = PlayerPrefs.GetInt(p + "babyHeight", 0);
        babyWeight = PlayerPrefs.GetInt(p + "babyWeight", 0);
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
        shopEntryArea = PlayerPrefs.GetInt(p + "shopEntryArea", 1);
        babyCurrentHp = PlayerPrefs.GetInt(p + "babyCurrentHp", -1);
        babyPoisonTurns = PlayerPrefs.GetInt(p + "babyPoisonTurns", 0);
        inventory = PlayerPrefs.GetString(p + "inventory", "");
        equipment = PlayerPrefs.GetString(p + "equipment", "");
        equippedSlots = PlayerPrefs.GetString(p + "equippedSlots", "");
        // マイグレーション: 旧セーブで equippedSlots が未設定の場合、所持装備を最大3つ自動装備
        if (string.IsNullOrEmpty(equippedSlots) && !string.IsNullOrEmpty(equipment))
        {
            var items = equipment.Split(',');
            int count = Mathf.Min(items.Length, MAX_EQUIP_SLOTS);
            equippedSlots = string.Join(",", items, 0, count);
        }
        motherGaveItem = PlayerPrefs.GetInt(p + "motherGaveItem", 0) == 1;
        goldenEggOldManState = PlayerPrefs.GetInt(p + "goldenEggOldManState", 0);
        kaguyaMetCount = PlayerPrefs.GetInt(p + "kaguyaMetCount", 0);
        kaguyaLover = PlayerPrefs.GetInt(p + "kaguyaLover", 0) == 1;
        stellaOriginProgress = PlayerPrefs.GetInt(p + "stellaOriginProgress", 0);
        stellaOriginTitle = PlayerPrefs.GetString(p + "stellaOriginTitle", "");
        defeatedEnemyList = PlayerPrefs.GetString(p + "defeatedEnemyList", "");
        metNpcList = PlayerPrefs.GetString(p + "metNpcList", "");
        fixedEncounterEnemy = PlayerPrefs.GetString(p + "fixedEncounterEnemy", "");
        customBabyImagePath = PlayerPrefs.GetString(p + "customBabyImagePath", "");
        synthBabyImagePath = PlayerPrefs.GetString(p + "synthBabyImagePath", "");
        milk = PlayerPrefs.GetInt(p + "milk", 0);
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
        PlayerPrefs.DeleteKey(p + "babyHeight");
        PlayerPrefs.DeleteKey(p + "babyWeight");
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
        PlayerPrefs.DeleteKey(p + "equipment");
        PlayerPrefs.DeleteKey(p + "equippedSlots");
        PlayerPrefs.DeleteKey(p + "motherGaveItem");
        PlayerPrefs.DeleteKey(p + "goldenEggOldManState");
        PlayerPrefs.DeleteKey(p + "kaguyaMetCount");
        PlayerPrefs.DeleteKey(p + "kaguyaLover");
        PlayerPrefs.DeleteKey(p + "stellaOriginProgress");
        PlayerPrefs.DeleteKey(p + "stellaOriginTitle");
        PlayerPrefs.DeleteKey(p + "defeatedEnemyList");
        PlayerPrefs.DeleteKey(p + "metNpcList");
        PlayerPrefs.DeleteKey(p + "fixedEncounterEnemy");
        PlayerPrefs.DeleteKey(p + "customBabyImagePath");
        PlayerPrefs.DeleteKey(p + "synthBabyImagePath");
        PlayerPrefs.DeleteKey(p + "milk");
        PlayerPrefs.DeleteKey(p + "exists");
        PlayerPrefs.Save();
    }

    // 新しい赤ちゃんのためにゲーム進行データをリセット（ステータスと名前は別途設定）
    public void ResetForNewBaby()
    {
        currentSlot = -1;
        babyAge = 0;
        babyExp = 0;
        defeatedEnemies = 0;
        cameFromMap = false;
        isBossBattle = false;
        currentArea = 0;
        shopEntryArea = 1;
        fixedEncounterEnemy = "";
        babyCurrentHp = -1;
        babyPoisonTurns = 0;
        inventory = "";
        equipment = "";
        equippedSlots = "";
        motherGaveItem = false;
        goldenEggOldManState = 0;
        kaguyaMetCount = 0;
        kaguyaLover = false;
        stellaOriginProgress = 0;
        stellaOriginTitle = "";
        milk = 0;
        defeatedEnemyList = "";
        metNpcList = "";
        mapPlayerX = 10;
        mapPlayerY = 7;
    }

    // ===== 装備管理 =====

    public const int MAX_EQUIP_SLOTS = 3;

    public void AddEquipment(string item)
    {
        if (string.IsNullOrEmpty(equipment))
            equipment = item;
        else
            equipment += "," + item;
    }

    // 所持しているか（購入済み全て）
    public bool HasEquipment(string item)
    {
        if (string.IsNullOrEmpty(equipment)) return false;
        foreach (var i in equipment.Split(','))
        {
            if (i == item) return true;
        }
        return false;
    }

    // 所持装備一覧
    public string[] GetEquipmentList()
    {
        if (string.IsNullOrEmpty(equipment)) return new string[0];
        return equipment.Split(',');
    }

    // 装備中か（スロットに入っているか）
    public bool IsEquipped(string item)
    {
        if (string.IsNullOrEmpty(equippedSlots)) return false;
        foreach (var i in equippedSlots.Split(','))
        {
            if (i == item) return true;
        }
        return false;
    }

    // 装備中一覧
    public string[] GetEquippedList()
    {
        if (string.IsNullOrEmpty(equippedSlots)) return new string[0];
        return equippedSlots.Split(',');
    }

    // 装備中の数
    public int GetEquippedCount()
    {
        if (string.IsNullOrEmpty(equippedSlots)) return 0;
        return equippedSlots.Split(',').Length;
    }

    // スロットに装備する
    public bool EquipItem(string item)
    {
        if (!HasEquipment(item)) return false;
        if (IsEquipped(item)) return false;
        if (GetEquippedCount() >= MAX_EQUIP_SLOTS) return false;
        if (string.IsNullOrEmpty(equippedSlots))
            equippedSlots = item;
        else
            equippedSlots += "," + item;
        return true;
    }

    // スロットから外す
    public void UnequipItem(string item)
    {
        if (string.IsNullOrEmpty(equippedSlots)) return;
        var list = new System.Collections.Generic.List<string>(equippedSlots.Split(','));
        list.Remove(item);
        equippedSlots = string.Join(",", list);
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

    public void RemoveItem(string item)
    {
        if (string.IsNullOrEmpty(inventory)) return;
        var items = new System.Collections.Generic.List<string>(inventory.Split(','));
        items.Remove(item);
        inventory = string.Join(",", items);
    }

    // ===== 縁の書（あそんだおともだち管理） =====

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

    // ===== NPC遭遇管理 =====

    public bool HasMetNpc(string npcName)
    {
        if (string.IsNullOrEmpty(metNpcList)) return false;
        foreach (var n in metNpcList.Split(','))
        {
            if (n == npcName) return true;
        }
        return false;
    }

    public void AddMetNpc(string npcName)
    {
        if (HasMetNpc(npcName)) return;
        if (string.IsNullOrEmpty(metNpcList))
            metNpcList = npcName;
        else
            metNpcList += "," + npcName;
    }

    public string[] GetMetNpcList()
    {
        if (string.IsNullOrEmpty(metNpcList)) return new string[0];
        return metNpcList.Split(',');
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

    // 合成赤ちゃん画像の読み込み（バトル/マップ用：透過版優先）
    public static Sprite LoadSynthBabySprite()
    {
        if (Instance == null) return null;
        string path = Instance.synthBabyImagePath;
        if (string.IsNullOrEmpty(path)) return null;

        // 透過版があればそちらを使用
        string transPath = Path.Combine(Application.persistentDataPath, "synth_baby_transparent.png");
        if (File.Exists(transPath))
        {
            byte[] data = File.ReadAllBytes(transPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(data);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        // フォールバック：背景あり版
        string fullPath = Path.Combine(Application.persistentDataPath, path);
        if (!File.Exists(fullPath)) return null;

        byte[] fallbackData = File.ReadAllBytes(fullPath);
        var fallbackTex = new Texture2D(2, 2);
        fallbackTex.LoadImage(fallbackData);
        return Sprite.Create(fallbackTex, new Rect(0, 0, fallbackTex.width, fallbackTex.height), new Vector2(0.5f, 0.5f));
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