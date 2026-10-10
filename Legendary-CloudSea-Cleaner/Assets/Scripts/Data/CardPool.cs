using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 牌池（一盒牌）。每张牌带两个数：
/// startingCount = 开局牌组里给几张（0 = 开局不带这张牌）；
/// rewardWeight = 三选一奖励里的权重，越大越容易刷到（0 = 不会作为随机奖励出现）。
/// 一个职业可以挂 1~N 个牌池；主职/副职的牌池在使用时会被合并。
/// </summary>
[CreateAssetMenu(fileName = "CardPool", menuName = "CloudSea Cleaner/Card Pool", order = 4)]
public class CardPool : ScriptableObject
{
    [System.Serializable]
    public class CardEntry
    {
        [Tooltip("这张牌（CardData 资产）")]
        public CardData card;

        [Tooltip("开局牌组里给几张，0 = 开局不带这张牌")]
        [Min(0)] public int startingCount = 1;

        [Tooltip("三选一奖励的权重，越大越容易刷到，0 = 不会作为随机奖励出现")]
        [Min(0)] public int rewardWeight = 1;
    }

    [Tooltip("这盒牌里包含的牌")]
    public List<CardEntry> cards = new List<CardEntry>();

    private struct WeightedCard
    {
        public CardData card;
        public int weight;
    }

    // ================= 开局组牌 =================

    /// <summary>把本盒按 startingCount 展开成实际卡牌（同一张牌按数量重复引用）。</summary>
    public List<CardData> BuildStartingCards()
    {
        var result = new List<CardData>();
        foreach (CardEntry e in cards)
        {
            if (e == null || e.card == null) continue;
            int n = Mathf.Max(0, e.startingCount);
            for (int i = 0; i < n; i++)
            {
                result.Add(e.card);
            }
        }
        return result;
    }

    /// <summary>把多个牌池的开局牌合并展开（主职 + 副职用）。</summary>
    public static List<CardData> BuildStartingCards(List<CardPool> pools)
    {
        var result = new List<CardData>();
        if (pools == null) return result;
        foreach (CardPool p in pools)
        {
            if (p != null) result.AddRange(p.BuildStartingCards());
        }
        return result;
    }

    // ================= 奖励抽牌（按权重、不重复） =================

    /// <summary>
    /// 从多个奖励牌池里按权重抽 count 张互不相同的牌（不放回）。
    /// 同一张牌在多个池里出现时，权重累加。
    /// 牌池凑不够 count 张时，用 fallbackCards（等概率，通常是中立/测试用 Reward Cards）补齐；
    /// 兜底也补不齐时，有多少返回多少。
    /// </summary>
    public static List<CardData> DrawRewards(List<CardPool> rewardPools, int count,
        List<CardData> fallbackCards = null)
    {
        var picked = new List<CardData>();
        if (count <= 0) return picked;

        // 1) 合并所有池，按卡牌聚合权重（同卡权重累加）
        var agg = new List<WeightedCard>();
        if (rewardPools != null)
        {
            foreach (CardPool pool in rewardPools)
            {
                if (pool == null) continue;
                foreach (CardEntry e in pool.cards)
                {
                    if (e == null || e.card == null || e.rewardWeight <= 0) continue;
                    int idx = agg.FindIndex(w => w.card == e.card);
                    if (idx >= 0)
                    {
                        WeightedCard w = agg[idx];
                        w.weight += e.rewardWeight;
                        agg[idx] = w;
                    }
                    else
                    {
                        agg.Add(new WeightedCard { card = e.card, weight = e.rewardWeight });
                    }
                }
            }
        }

        // 2) 加权不放回抽牌，直到抽满或没有可抽
        while (picked.Count < count)
        {
            int total = 0;
            foreach (WeightedCard w in agg)
            {
                if (!picked.Contains(w.card)) total += w.weight;
            }
            if (total <= 0) break;

            int roll = Random.Range(0, total); // [0, total-1]
            int acc = 0;
            CardData chosen = null;
            foreach (WeightedCard w in agg)
            {
                if (picked.Contains(w.card)) continue;
                acc += w.weight;
                if (roll < acc)
                {
                    chosen = w.card;
                    break;
                }
            }
            if (chosen == null) break;
            picked.Add(chosen);
        }

        // 3) 池里凑不够，用兜底卡等概率补（仍保证不重复）
        if (picked.Count < count && fallbackCards != null)
        {
            var remaining = new List<CardData>(fallbackCards);
            while (picked.Count < count && remaining.Count > 0)
            {
                int i = Random.Range(0, remaining.Count);
                CardData c = remaining[i];
                remaining.RemoveAt(i);
                if (c != null && !picked.Contains(c)) picked.Add(c);
            }
        }

        return picked;
    }
}
