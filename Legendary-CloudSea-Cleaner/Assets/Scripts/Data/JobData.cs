using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 职业（主职、副职都用这个资产）。
/// 一个职业挂两排牌池，每排都能挂 1~N 个 CardPool：
/// startingPools 决定开局牌组（按每张牌的 startingCount 展开）；
/// rewardPools  决定战斗胜利三选一能刷出什么牌（按 rewardWeight 加权、不重复抽）。
/// jobColor 用于底部折叠卡组栏的主职/副职色块。
/// </summary>
[CreateAssetMenu(fileName = "JobData", menuName = "CloudSea Cleaner/Job Data", order = 5)]
public class JobData : ScriptableObject
{
    [Header("职业信息")]
    [Tooltip("职业名称（游戏内文字目前用英文/拼音）")]
    public string jobName = "New Job";

    [Tooltip("职业颜色，用于底部折叠卡组栏的职业色块")]
    public Color jobColor = Color.white;

    [Header("牌池（每排都可以挂 1~N 个）")]
    [Tooltip("初始牌池：开局牌组由这些牌池按“开局几张”展开合并")]
    public List<CardPool> startingPools = new List<CardPool>();

    [Tooltip("奖励牌池：战斗胜利三选一从这些牌池按权重抽牌")]
    public List<CardPool> rewardPools = new List<CardPool>();

    /// <summary>展开本职业自己的开局牌（只合并本职业的初始牌池）。</summary>
    public List<CardData> BuildStartingDeck()
    {
        return CardPool.BuildStartingCards(startingPools);
    }

    /// <summary>把主职 + 副职的初始牌池合并成一个列表（副职可为 null；同一牌池不会重复计入）。</summary>
    public static List<CardPool> CollectStartingPools(JobData mainJob, JobData subJob)
    {
        var pools = new List<CardPool>();
        AppendPools(pools, mainJob, true);
        AppendPools(pools, subJob, true);
        return pools;
    }

    /// <summary>把主职 + 副职的奖励牌池合并成一个列表（副职可为 null；同一牌池不会重复计入）。</summary>
    public static List<CardPool> CollectRewardPools(JobData mainJob, JobData subJob)
    {
        var pools = new List<CardPool>();
        AppendPools(pools, mainJob, false);
        AppendPools(pools, subJob, false);
        return pools;
    }

    private static void AppendPools(List<CardPool> target, JobData job, bool starting)
    {
        if (job == null) return;
        List<CardPool> source = starting ? job.startingPools : job.rewardPools;
        if (source == null) return;
        foreach (CardPool p in source)
        {
            if (p != null && !target.Contains(p)) target.Add(p);
        }
    }
}
