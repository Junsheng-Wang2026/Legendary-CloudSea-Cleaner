using System.Collections.Generic;
using UnityEngine;

// 事件三选一的“抽牌器”：根据一个 DrawCardChoice 效果的配置，产出供选择的候选牌。
// 三种来源：职业奖励池 / 事件指定的固定牌池 / 红绿蓝（力量·敏捷·智慧）各一张。
public static class EventRewardPicker
{
    private struct WeightedCard
    {
        public CardData card;
        public int weight;
    }

    public static List<CardData> Pick(EventEffect fx)
    {
        var empty = new List<CardData>();
        if (fx == null) return empty;

        int count = Mathf.Max(1, fx.choiceCount);
        GameManager gm = GameManager.Instance;

        switch (fx.rewardSource)
        {
            case EventEffect.RewardSource.JobPools:
                if (gm == null || gm.mainJob == null)
                {
                    Debug.LogWarning("[EventRewardPicker] 职业奖励池来源：没配主职，抽不到牌");
                    return empty;
                }
                return gm.DrawRewardCards(count) ?? empty;

            case EventEffect.RewardSource.EventPool:
                if (fx.rewardPool == null)
                {
                    Debug.LogWarning("[EventRewardPicker] 事件固定牌池来源：Reward Pool 没拖 CardPool");
                    return empty;
                }
                var pools = new List<CardPool> { fx.rewardPool };
                List<CardData> fb = gm != null ? gm.fallbackRewardCards : null;
                return CardPool.DrawRewards(pools, count, fb);

            case EventEffect.RewardSource.ByTypeRGB:
                return PickOnePerType(gm);
        }
        return empty;
    }

    // 力量 / 敏捷 / 智慧 各随机一张（加权、互不重复）；某色没牌就用兜底牌补，再没有就少一张
    private static List<CardData> PickOnePerType(GameManager gm)
    {
        var result = new List<CardData>();
        var picked = new List<CardData>();

        var agg = new List<WeightedCard>();
        if (gm != null && gm.mainJob != null)
        {
            List<CardPool> pools = JobData.CollectRewardPools(gm.mainJob, gm.subJob);
            if (pools != null)
            {
                foreach (CardPool pool in pools)
                {
                    if (pool == null) continue;
                    foreach (CardPool.CardEntry e in pool.cards)
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
        }

        CardType[] types = { CardType.Strength, CardType.Agility, CardType.Wisdom };
        foreach (CardType t in types)
        {
            CardData c = WeightedPickOfType(agg, t, picked);

            if (c == null && gm != null && gm.fallbackRewardCards != null)
            {
                var fbOfType = new List<CardData>();
                foreach (CardData fc in gm.fallbackRewardCards)
                {
                    if (fc != null && fc.cardType == t && !picked.Contains(fc))
                        fbOfType.Add(fc);
                }
                if (fbOfType.Count > 0)
                    c = fbOfType[Random.Range(0, fbOfType.Count)];
            }

            if (c != null)
            {
                picked.Add(c);
                result.Add(c);
            }
        }

        return result;
    }

    private static CardData WeightedPickOfType(List<WeightedCard> agg, CardType type, List<CardData> exclude)
    {
        int total = 0;
        foreach (WeightedCard w in agg)
        {
            if (w.card.cardType == type && !exclude.Contains(w.card)) total += w.weight;
        }
        if (total <= 0) return null;

        int roll = Random.Range(0, total);
        int acc = 0;
        foreach (WeightedCard w in agg)
        {
            if (w.card.cardType != type || exclude.Contains(w.card)) continue;
            acc += w.weight;
            if (roll < acc) return w.card;
        }
        return null;
    }
}
