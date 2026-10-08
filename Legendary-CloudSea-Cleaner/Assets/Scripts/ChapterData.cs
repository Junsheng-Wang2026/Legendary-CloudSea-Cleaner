using UnityEngine;
using System.Collections.Generic;

// 章节里的一个节点 = 下降路线上的一个里程标记 + 一个事件
[System.Serializable]
public class ChapterNode
{
    [Tooltip("到达本节点前要下降的距离（米），即上一节点/起点到本节点的间距")]
    public float segmentDistance = 20f;

    [Tooltip("固定事件：拖了就用它（章节最后一个结算节点拖 Settlement Event）")]
    public EventData fixedEvent;

    [Tooltip("随机事件：fixedEvent 留空时，从这个事件池抽一个")]
    public EventPool eventPool;
}

// 一章的完整定义：天气 + BOSS + 一串有序节点
// Project 里右键 Create -> CloudSea Cleaner -> Chapter Data 创建
[CreateAssetMenu(fileName = "NewChapter", menuName = "CloudSea Cleaner/Chapter Data")]
public class ChapterData : ScriptableObject
{
    [Header("本章天气（二选一：拖 Weather 固定，或留空用 Weather Pool 随机）")]
    public WeatherData weather;
    public WeatherPool weatherPool;

    [Header("本章 BOSS（二选一：拖 Fixed Boss 固定，或留空用 Boss Pool 随机）")]
    public EnemyData fixedBoss;
    public EnemyPool bossPool;

    [Header("本章节点（按下降顺序排列；最后一个建议放章节结算事件）")]
    public List<ChapterNode> nodes = new List<ChapterNode>();

    [Header("本章总距离（米，自动计算，请勿手改）")]
    [SerializeField] private float totalDistance = 0f;

    void OnValidate()
    {
        totalDistance = GetTotalDistance();
    }

    // 本章总距离 = 各节点段距之和
    public float GetTotalDistance()
    {
        float sum = 0f;
        if (nodes != null)
        {
            foreach (ChapterNode n in nodes)
            {
                if (n != null) sum += Mathf.Max(0f, n.segmentDistance);
            }
        }
        return sum;
    }

    // 各段距离（按节点顺序），给距离条用
    public float[] GetSegmentDistances()
    {
        if (nodes == null) return new float[0];
        float[] arr = new float[nodes.Count];
        for (int i = 0; i < nodes.Count; i++)
        {
            arr[i] = nodes[i] != null ? Mathf.Max(0f, nodes[i].segmentDistance) : 0f;
        }
        return arr;
    }

    // 取某节点要执行的事件：固定优先，否则从事件池抽，都没有返回 null
    public EventData ResolveNodeEvent(int index)
    {
        if (nodes == null || index < 0 || index >= nodes.Count) return null;
        ChapterNode node = nodes[index];
        if (node.fixedEvent != null) return node.fixedEvent;
        if (node.eventPool != null) return node.eventPool.GetRandom();
        return null;
    }

    public WeatherData ResolveWeather()
    {
        if (weather != null) return weather;
        if (weatherPool != null) return weatherPool.GetRandom();
        return null;
    }

    public EnemyData ResolveBoss()
    {
        if (fixedBoss != null) return fixedBoss;
        if (bossPool != null) return bossPool.GetRandom();
        return null;
    }
}
