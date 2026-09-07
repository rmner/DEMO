using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int unlockedLevel = 0;   // 0=只解锁第1关, 1=解锁第2关, 2=全部
    public int bestScore = 0;
    public int coins = 0;           // 金币钱包（可花，用来买商城东西）
    public List<string> ownedItems = new List<string>();   // 已拥有商品 id
    public string equippedSkin = "";       // 当前装备的皮肤 id
    public string equippedAbility = "";    // 当前装备的能力 id
}

public static class SaveManager
{
    const string Key = "MetroidvaniaSave";

    public static void Save(SaveData d)
    {
        if (d.ownedItems == null) d.ownedItems = new List<string>();
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(d));
        PlayerPrefs.Save();
    }

    public static SaveData Load()
    {
        SaveData d;
        if (PlayerPrefs.HasKey(Key))
            d = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key));
        else
            d = new SaveData();

        // 老存档没有这些字段时补上默认值，避免 null 崩溃
        if (d.ownedItems == null) d.ownedItems = new List<string>();
        if (d.coins < 0) d.coins = 0;
        return d;
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
