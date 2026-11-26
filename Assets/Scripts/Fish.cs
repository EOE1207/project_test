using System;
using System.Collections;
using UnityEngine;

// 掛在一個空物件上做成「魚」Prefab；模型會在 Init 時依稀有度生成成子物件
public class Fish : MonoBehaviour, IDamageable
{
    [Header("GDD 4.4：場上存留時間")]
    public float lifetime = 30f;

    [Header("游動範圍（以出生點為中心）")]
    public float wanderRadius = 3f;

    [Header("魚種總表（擊殺時從這裡抽出實際魚種）")]
    public FishDatabase database;

    [Header("測試用：有填就會在 Start 自己初始化，不用生魚器")]
    public RarityData testRarity;

    // 事件：之後步驟 4 / 程式 B 的生魚器、背包會訂閱
    public event Action<Fish> OnKilled;      // 被打死
    public event Action<Fish> OnDespawned;   // 30 秒到期消失

    public RarityData Rarity { get; private set; }
    public int CurrentHp { get; private set; }
    public bool IsDead { get; private set; }
    public FishData CaughtSpecies { get; private set; }   // 擊殺後才會有值

    Vector3 home;            // 出生點
    Vector3 swimTarget;      // 目前要游去的點
    float age;               // 已經活了幾秒
    bool gone;               // 已經在消失中（死掉或到期）
    Renderer[] renderers;

    void Start()
    {
        if (Rarity == null && testRarity != null) Init(testRarity);
    }

    // 生魚器呼叫這個：決定稀有度後才知道要長什麼樣、有幾滴血
    public void Init(RarityData rarity)
    {
        Rarity = rarity;
        CurrentHp = rarity.maxHp;
        home = transform.position;
        PickNewTarget();

        if (rarity.modelPrefab != null)
        {
            var model = Instantiate(rarity.modelPrefab, transform);
            model.transform.localPosition = Vector3.zero;
        }
        transform.localScale = Vector3.one * rarity.bodyScale;
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        if (Rarity == null || gone) return;

        age += Time.deltaTime;
        if (age >= lifetime) { Despawn(); return; }

        Swim();
    }

    // ── 游動：在出生點附近隨便找一個點游過去，到了再換下一個點 ──
    void Swim()
    {
        Vector3 toTarget = swimTarget - transform.position;
        if (toTarget.magnitude < 0.3f) { PickNewTarget(); return; }

        Vector3 dir = toTarget.normalized;
        transform.position += dir * Rarity.swimSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(dir), Time.deltaTime * 3f);
    }

    void PickNewTarget()
    {
        Vector2 r = UnityEngine.Random.insideUnitCircle * wanderRadius;
        swimTarget = home + new Vector3(r.x, 0f, r.y);   // 只在水平面上游，高度不變
    }

    // ── 被魚叉打中（Projectile 會呼叫） ──
    public void TakeDamage(int amount)
    {
        if (IsDead || gone) return;

        CurrentHp -= amount;
        Debug.Log($"[Fish] {Rarity.rarity} 魚被打中，剩 {CurrentHp}/{Rarity.maxHp} 血");

        if (CurrentHp <= 0) Die();
        else StartCoroutine(FlashRed());
    }

    void Die()
    {
        IsDead = true;
        gone = true;
        Debug.Log($"[Fish] {Rarity.rarity} 魚被擊殺！");

        // GDD 4.4：血量歸零時才決定實際魚種
        if (database != null)
        {
            CaughtSpecies = database.RollSpecies(Rarity.rarity);
            if (CaughtSpecies != null) FishEvents.RaiseCaught(CaughtSpecies);
        }
        else Debug.LogWarning("[Fish] 沒有指定 Database，無法抽出魚種");

        OnKilled?.Invoke(this);
        StartCoroutine(BellyUp());
    }

    // GDD 4.5：擊殺 → 翻肚
    IEnumerator BellyUp()
    {
        Quaternion from = transform.rotation;
        Quaternion to = from * Quaternion.Euler(0f, 0f, 180f);   // 繞身體長軸翻 180 度
        Vector3 startPos = transform.position;

        for (float t = 0; t < 1f; t += Time.deltaTime / 0.6f)
        {
            transform.rotation = Quaternion.Slerp(from, to, t);
            transform.position = startPos + Vector3.up * (0.2f * t);   // 稍微浮起來
            yield return null;
        }
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    // GDD 4.4：存留 30 秒到期 → 消失並丟失進度（包含已受的傷）
    void Despawn()
    {
        gone = true;
        Debug.Log($"[Fish] {Rarity.rarity} 魚存留時間到，消失");
        OnDespawned?.Invoke(this);
        StartCoroutine(ShrinkAway());
    }

    IEnumerator ShrinkAway()
    {
        Vector3 start = transform.localScale;
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.5f)
        {
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }

    // 受傷但沒死：閃紅色 0.15 秒，讓玩家知道有打到
    IEnumerator FlashRed()
    {
        foreach (var r in renderers) r.material.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        foreach (var r in renderers) r.material.color = Color.white;
    }
}
