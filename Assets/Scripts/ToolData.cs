using UnityEngine;

// 每種工具一筆資料：Project 視窗右鍵 > Create > FishHunter > Tool Data
[CreateAssetMenu(menuName = "FishHunter/Tool Data", fileName = "NewTool")]
public class ToolData : ScriptableObject
{
    public string toolName = "木製魚叉";
    public GameObject projectilePrefab;   // 需掛 Projectile + Rigidbody + Collider

    [Header("投擲")]
    public float forceMultiplier = 1.5f;  // 力道倍率（三叉戟調高 = 飛更遠）
    public float maxFlightTime = 4f;      // 超過這個時間還沒結果就算未命中

    [Header("傷害")]
    public int damage = 1;
    public float areaRadius = 0f;         // 0 = 單體；>0 = 範圍（漁網、炸彈）
}
