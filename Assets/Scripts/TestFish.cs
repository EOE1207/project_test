using UnityEngine;

// 測試用的假魚：掛在任何有 Collider 的物件上即可被魚叉打中
public class TestFish : MonoBehaviour, IDamageable
{
    public int hp = 2;

    public void TakeDamage(int amount)
    {
        hp -= amount;
        Debug.Log($"{name} 被打中，剩 {hp} 血");
        if (hp <= 0)
        {
            Debug.Log("擊殺！");
            Destroy(gameObject);
        }
    }
}
