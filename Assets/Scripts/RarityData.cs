using UnityEngine;

// 一個稀有度一份資料（共 4 份）：Project 視窗右鍵 > Create > FishHunter > Rarity Data
// GDD 4.4：魚生成時先抽稀有度，由這份資料決定外觀模型、血量、游速
[CreateAssetMenu(menuName = "FishHunter/Rarity Data", fileName = "NewRarity")]
public class RarityData : ScriptableObject
{
    public Rarity rarity = Rarity.Common;

    [Header("外觀")]
    public GameObject modelPrefab;          // 這個稀有度共用的魚模型（建模做好前先用方塊）
    public float bodyScale = 1f;            // 體型：越稀有越大

    [Header("戰鬥")]
    [Min(1)] public int maxHp = 1;          // GDD 暫定：普通 1、稀有 2、史詩 3、傳說 4

    [Header("游動")]
    public float swimSpeed = 1f;            // 公尺/秒

    [Header("生成")]
    [Min(0)] public float spawnWeight = 60f; // 抽中機率的權重，四份加起來不用剛好是 100
}
