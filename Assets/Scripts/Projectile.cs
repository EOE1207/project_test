using UnityEngine;

// 魚要實作這個介面（程式 B 的魚生成器會用到）
public interface IDamageable
{
    void TakeDamage(int amount);   // 魚自己處理血量、翻肚、入背包
}

// 掛在魚叉 Prefab 上（需要 Rigidbody + Collider）
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public string waterTag = "Water";  // 水面 Collider 設 Is Trigger 並加這個 Tag

    Rigidbody rb;
    ToolData tool;
    ThrowController owner;
    float launchTime;
    bool resolved;

    void Awake() => rb = GetComponent<Rigidbody>();

    public void SetHeld(bool held)
    {
        rb.isKinematic = held;
        GetComponent<Collider>().enabled = !held; // 拿在手上時不撞東西
    }

    public void Launch(Vector3 velocity, ToolData data, ThrowController from)
    {
        tool = data; owner = from;
        SetHeld(false);
        rb.velocity = velocity; // 升級到 Unity 6 時改成 rb.linearVelocity
        launchTime = Time.time;
    }

    void Update()
    {
        if (owner == null || resolved) return;
        // 讓魚叉頭朝飛行方向
        if (rb.velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        if (Time.time - launchTime > tool.maxFlightTime) Resolve(false);
    }

    void OnCollisionEnter(Collision c) => HandleHit(c.collider);
    void OnTriggerEnter(Collider other) => HandleHit(other);

    void HandleHit(Collider other)
    {
        if (owner == null || resolved) return;

        if (tool.areaRadius > 0f) { Resolve(AreaDamage()); return; } // 漁網/炸彈：碰到任何東西就結算

        var target = other.GetComponentInParent<IDamageable>();
        if (target != null) { target.TakeDamage(tool.damage); Resolve(true); }
        else if (other.CompareTag(waterTag)) Resolve(false);
    }

    bool AreaDamage()
    {
        bool hitAny = false;
        foreach (var col in Physics.OverlapSphere(transform.position, tool.areaRadius))
        {
            var target = col.GetComponentInParent<IDamageable>();
            if (target != null) { target.TakeDamage(tool.damage); hitAny = true; }
        }
        return hitAny;
    }

    void Resolve(bool hit)
    {
        resolved = true;
        owner.ReportResult(hit);
        Destroy(gameObject, 0.5f); // 留一下讓水花/音效播完
    }
}
