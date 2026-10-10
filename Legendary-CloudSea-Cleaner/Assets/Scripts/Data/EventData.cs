using System.Collections.Generic;
using UnityEngine;

// 事件资产：节点到位后执行。
// 两种用法：
//   1) 纯战斗事件：Options 留空，在下面拖一个遭遇（Encounter）或遭遇池，到点直接开打；
//   2) 选项事件：在 Options 里配若干选项（每选项一串效果），到点弹选项面板。
// 商店/休息/结算等特殊事件用对应子类（Shop/Rest/Settlement Event Data）。
// Project 里右键 Create -> Cleaner -> Event Data 创建。
[CreateAssetMenu(fileName = "EventData", menuName = "Cleaner/Event Data")]
public class EventData : ScriptableObject
{
    public enum EventType { Fight, Choice, Interaction }

    [Header("描述")]
    public string eventName;
    [TextArea(3, 8)] public string description;
    public EventType eventType = EventType.Choice;

    [Header("纯战斗事件（Options 留空时用）：二选一，拖固定遭遇或留空用遭遇池随机")]
    public EncounterData encounter;
    public EncounterPool encounterPool;

    [Header("多选项（配了就弹选项面板；纯战斗事件留空，到点直接打上面的遭遇）")]
    public List<EventOption> options = new List<EventOption>();

    // 解析这场战斗的遭遇：固定优先，否则从遭遇池随机，都没有返回 null
    public EncounterData ResolveEncounter()
    {
        if (encounter != null) return encounter;
        if (encounterPool != null) return encounterPool.GetRandom();
        return null;
    }

    // 是否配了新的多选项
    public bool HasNewOptions()
    {
        return options != null && options.Count > 0;
    }

    // 子类事件（商店/休息/结算）重写它，由章节导演直接调用
    public virtual void Execute(IEventContext ctx) { }
}
