using UnityEngine;

// 商店事件：执行后切到商店场景；商店内部买卖由 Shop 场景里的管理器负责，
// 玩家点“离开”回 Boot 后再由导演 FinishEvent 继续下一节点。
[CreateAssetMenu(fileName = "ShopEvent", menuName = "CloudSea Cleaner/Events/Shop Event")]
public class ShopEventData : EventData
{
    [Header("商店场景名（Build Settings 里的场景名）")]
    public string shopSceneName = "Shop";

    public override void Execute(IEventContext ctx)
    {
        if (ctx == null) return;
        ctx.GoToScene(shopSceneName);
        // 注意：这里不调 FinishEvent，等玩家从商店返回 Boot 后再继续
    }
}
