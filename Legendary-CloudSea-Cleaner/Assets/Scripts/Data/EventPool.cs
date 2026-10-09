using UnityEngine;
using System.Collections.Generic;

// 事件池子：往里拖 EventData（含商店/休息/结算/战斗等各种事件子类资产）
// 章节节点需要随机事件时，从这个池子抽一个
// Project 里右键 Create -> CloudSea Cleaner -> Event Pool 创建
[CreateAssetMenu(fileName = "EventPool", menuName = "CloudSea Cleaner/Event Pool", order = 3)]
public class EventPool : ScriptableObject
{
    [Tooltip("这个池子包含的事件")]
    public List<EventData> events = new List<EventData>();

    [Tooltip("勾选后不重复抽取，一轮抽完再重新洗入")]
    public bool noRepeatUntilEmpty = true;

    private List<EventData> _remaining = new List<EventData>();

    // 从池子里随机取一个事件
    public EventData GetRandom()
    {
        if (events == null || events.Count == 0) return null;

        if (!noRepeatUntilEmpty)
            return events[Random.Range(0, events.Count)];

        if (_remaining.Count == 0)
            _remaining = new List<EventData>(events);

        int i = Random.Range(0, _remaining.Count);
        EventData picked = _remaining[i];
        _remaining.RemoveAt(i);
        return picked;
    }

    // 需要重置抽取记录时调用（比如进入新一章）
    public void ResetPool()
    {
        _remaining.Clear();
    }
}
