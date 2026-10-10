using System.Collections.Generic;
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

    [Header("战斗敌人（旧单怪，非战斗事件留空）")]
    public EnemyData enemy;         // 固定敌人：拖了就打它
    public EnemyPool enemyPool;     // 随机敌人：enemy 留空时从这个池子抽
    public bool forceFight = false; // true=无选项、到点强制直接开打（超时BOSS事件）
    public bool advanceDistance = true; // 打完是否推进下降距离（超时BOSS设 false）

    [Header("战斗遭遇（新·多敌人，和上面 enemy/enemyPool 二选一；遭遇优先）")]
    public EncounterData encounter;     // 固定遭遇：拖了就打这一整场
    public EncounterPool encounterPool; // 随机遭遇：encounter 留空时从池子抽一整场

    // ===== 旧 2 选项（向后兼容，新事件可不填，改用下面 Options）=====
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

    [Header("多选项（新；配了任意一项就用这套，数量不限）")]
    [Tooltip("每个选项挂一串效果，按顺序执行")]
    public List<EventOption> options = new List<EventOption>();

    // 解析本事件要打的遭遇：固定优先，其次遭遇池；都没有返回 null（调用方再回退旧 enemy/enemyPool）
    public EncounterData ResolveEncounter()
    {
        if (encounter != null) return encounter;
        if (encounterPool != null) return encounterPool.GetRandom();
        return null;
    }

    // 是否使用新的多选项系统
    public bool HasNewOptions()
    {
        return options != null && options.Count > 0;
    }

    // 事件执行入口：特殊事件子类 override 它；普通战斗/选择事件默认空，由导演按上面字段处理
    public virtual void Execute(IEventContext ctx) { }
}
