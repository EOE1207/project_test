using System.Collections.Generic;
using UnityEngine;

// 全部魚種 + 全部稀有度的總表，整個遊戲只需要一份
// Project 視窗右鍵 > Create > FishHunter > Fish Database
[CreateAssetMenu(menuName = "FishHunter/Fish Database", fileName = "FishDatabase")]
public class FishDatabase : ScriptableObject
{
    public List<RarityData> rarities = new();   // 拖入 4 份 RarityData
    public List<FishData> fishes = new();       // 拖入所有 FishData

    // GDD 4.4 第 1 步：魚生成時先抽稀有度（依 spawnWeight 權重）
    public RarityData RollRarity()
    {
        float total = 0f;
        foreach (var r in rarities) total += r.spawnWeight;

        float pick = Random.Range(0f, total);
        foreach (var r in rarities)
        {
            if (pick < r.spawnWeight) return r;
            pick -= r.spawnWeight;
        }
        return rarities[rarities.Count - 1];
    }

    // GDD 4.4 第 2 步：血量歸零時，從「該稀有度」的魚種池抽出實際魚種
    public FishData RollSpecies(Rarity rarity)
    {
        var pool = fishes.FindAll(f => f.rarity == rarity);
        if (pool.Count == 0)
        {
            Debug.LogWarning($"[FishDatabase] 沒有任何 {rarity} 稀有度的魚種資料！");
            return null;
        }
        return pool[Random.Range(0, pool.Count)];
    }
}
