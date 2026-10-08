using UnityEngine;

// 事件基类（可继承）。
// 普通战斗/二选一事件：直接建这个基类资产，用下面的敌人字段和选项字段配置，由导演处理。
// 有特殊行为的事件（商店/休息/结算等）：继承本类并 override Execute，见 ShopEventData 等。
[CreateAssetMenu(fileName = "NewEvent", menuName = "Cleaner/Event Data")]
public class EventData : ScriptableObject
{
    public string eventName;        // 事件名
    [TextArea] public string description;  // 事件描述
    public enum EventType { Fight, Choice, Interaction }
    public EventType eventType;     // 战斗/选择/交互

    [Header("战斗敌人（非战斗事件留空即可）")]
    public EnemyData enemy;         // 固定敌人：拖了就打它
    public EnemyPool enemyPool;     // 随机敌人：enemy 留空时从这个池子抽
    public bool forceFight = false; // true=无选项、到点强制直接开打（超时BOSS事件）
    public bool advanceDistance = true; // 打完是否推进下降距离（超时BOSS设 false）

    // 选项
    public string option1Text;      // 选项1文字
    public bool option1IsFight;     // 选项1是否进战斗
    public int option1TimeCost;     // 选项1时间消耗
    public int option1HPChange;     // 选项1血量变化
    public CardData option1Reward;  // 选项1奖励牌（可选）

    public string option2Text;      // 选项2文字
    public bool option2IsFight;     // 选项2是否进战斗
    public int option2TimeCost;     // 选项2时间消耗
    public int option2HPChange;     // 选项2血量变化
    public CardData option2Reward;  // 选项2奖励牌（可选）

    // 事件执行入口：特殊事件子类 override 它；普通战斗/选择事件默认空，由导演按上面字段处理
    public virtual void Execute(IEventContext ctx) { }
}
