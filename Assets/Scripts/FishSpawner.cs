using System.Collections.Generic;
using UnityEngine;

// 掛在場景裡一個空物件上，物件的位置 = 生魚範圍的中心（放在水面上）
// 測試用簡單版，正式版由程式 B 接手
public class FishSpawner : MonoBehaviour
{
    [Header("資料")]
    public FishDatabase database;
    public Fish fishPrefab;                  // 只掛 Fish 元件的空物件 Prefab

    [Header("生成規則")]
    public int maxFish = 5;                  // 場上最多幾條魚
    public float spawnInterval = 2f;         // 每隔幾秒檢查一次要不要補魚
    public float spawnRadius = 4f;           // 以這個物件為中心的生成半徑（公尺）

    readonly List<Fish> alive = new();
    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        if (alive.Count < maxFish) SpawnOne();
    }

    void SpawnOne()
    {
        // GDD 4.4 第 1 步：生成時先抽稀有度
        RarityData rarity = database.RollRarity();

        Vector2 r = Random.insideUnitCircle * spawnRadius;
        Vector3 pos = transform.position + new Vector3(r.x, 0f, r.y);

        Fish fish = Instantiate(fishPrefab, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        fish.database = database;            // 擊殺時要從這裡抽魚種
        fish.Init(rarity);

        // 魚死掉或到期消失時，從清單移除，空出位置讓下一條補上
        alive.Add(fish);
        fish.OnKilled += Remove;
        fish.OnDespawned += Remove;

        Debug.Log($"[Spawner] 生成 {rarity.rarity} 魚（場上 {alive.Count}/{maxFish} 條）");
    }

    void Remove(Fish fish) => alive.Remove(fish);

    // 在 Scene 視窗畫出生成範圍，方便擺位置（只有選取這個物件時會顯示）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
