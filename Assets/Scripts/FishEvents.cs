using System;

// 全域的「捕獲」廣播站：魚被抓到時在這裡喊一聲，誰需要就自己來聽
// 背包、圖鑑、炸彈解鎖、提示 UI 都訂閱這裡，不用互相知道對方存在
public static class FishEvents
{
    // 參數：抓到的魚種資料
    public static event Action<FishData> OnFishCaught;

    public static void RaiseCaught(FishData species)
    {
        UnityEngine.Debug.Log($"[FishEvents] 捕獲：{species.displayName}（{species.rarity}，賣價 {species.sellPrice}）");
        OnFishCaught?.Invoke(species);
    }
}
