using UnityEngine;

// 章节结算事件：固定挂在本章最后一个节点。
// 执行后调用章节结算（块6完善：回血/奖励/进下一章），现在先打通接口。
[CreateAssetMenu(fileName = "SettlementEvent", menuName = "CloudSea Cleaner/Events/Settlement Event")]
public class SettlementEventData : EventData
{
    public override void Execute(IEventContext ctx)
    {
        if (ctx == null) return;
        ctx.SettleChapter();
    }
}
