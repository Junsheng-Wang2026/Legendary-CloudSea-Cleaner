using UnityEngine;

// 休息事件：即时回一定血量，然后结束本事件、解锁下一节点。
// 回多少血在资产上配，同一种休息可建多份资产（小休息/大休息），不用再写类。
[CreateAssetMenu(fileName = "RestEvent", menuName = "CloudSea Cleaner/Events/Rest Event")]
public class RestEventData : EventData
{
    [Header("回复血量")]
    public int healAmount = 30;

    public override void Execute(IEventContext ctx)
    {
        if (ctx == null) return;
        ctx.Heal(healAmount);
        ctx.FinishEvent();
    }
}
