using UnityEngine;

// 稀有度：GDD 4.2，普通 7 種、稀有 5 種、史詩 2 種、傳說 1 種
public enum Rarity
{
    Common,     // 普通
    Rare,       // 稀有
    Epic,       // 史詩
    Legendary   // 傳說
}

// 目前只做湖邊小屋一個場景，所以不需要「出現場景」欄位
// 一種魚一份資料：Project 視窗右鍵 > Create > FishHunter > Fish Data
[CreateAssetMenu(menuName = "FishHunter/Fish Data", fileName = "NewFish")]
public class FishData : ScriptableObject
{
    [Header("基本資料")]
    public string fishId = "fish_01";       // 程式用的代號，不要重複（存檔、圖鑑會用）
    public string displayName = "小丑魚";    // 顯示給玩家看的名字
    public Rarity rarity = Rarity.Common;

    [Header("外觀（同稀有度共用模型，靠材質區分）")]
    public Material material;
    public Sprite icon;                      // 背包、圖鑑用的圖示

    [Header("經濟與圖鑑")]
    public int sellPrice = 10;
    [TextArea(2, 4)] public string description = "圖鑑介紹文字";
}
