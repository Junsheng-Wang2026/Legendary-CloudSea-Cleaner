using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


// 事件效果执行器：把一个选项挂的一串 EventEffect 按顺序逐个跑掉。
// 纯逻辑、不挂物体；C5 的选项按钮点击后调 ChapterDirector.RunOptionEffects(...)，内部走这里的协程。
public static class EventEffectRunner
{
    // 协程内部状态（记录是否已被战斗/切场景/结算接管）
    class RoutineState
    {
        public bool taken;
    }

    // ===== 同步入口（不含等待玩家的三选一；给测试用）=====
    public static bool RunOption(EventOption option, IEventContext ctx)
    {
        if (option == null) return false;
        return Run(option.effects, ctx);
    }

    public static bool Run(IEnumerable<EventEffect> effects, IEventContext ctx)
    {
        if (effects == null || ctx == null) return false;
        bool takenOver = false;
        foreach (EventEffect fx in effects)
        {
            if (ApplyImmediate(fx, ctx)) takenOver = true;
        }
        return takenOver;
    }

    // ===== 协程入口（推荐；支持三选一等玩家、概率分支嵌套）=====
    // onDone(true)=中途切场景/战斗/结算，流程被接管；onDone(false)=一串效果跑完，调用方关面板并 FinishEvent。
    public static IEnumerator RunRoutine(EventCardRewardPanel rewardPanel,
                                          IEnumerable<EventEffect> effects,
                                          IEventContext ctx,
                                          Action<bool> onDone)
    {
        var st = new RoutineState();
        yield return ProcessList(rewardPanel, effects, ctx, st);
        onDone?.Invoke(st.taken);
    }

    // 逐效果执行；概率分支会递归进它选中的那组效果
    static IEnumerator ProcessList(EventCardRewardPanel rewardPanel,
                                   IEnumerable<EventEffect> effects,
                                   IEventContext ctx,
                                   RoutineState st)
    {
        if (effects == null) yield break;

        foreach (EventEffect fx in effects)
        {
            if (fx == null || fx.type == EventEffect.EffectType.None) continue;

            // 三选一：弹面板等玩家选完再继续
            if (fx.type == EventEffect.EffectType.DrawCardChoice)
            {
                List<CardData> candidates = EventRewardPicker.Pick(fx);
                if (candidates == null || candidates.Count == 0)
                {
                    Debug.LogWarning("[EventEffectRunner] 三选一无候选牌，跳过");
                    continue;
                }
                if (rewardPanel == null)
                {
                    Debug.LogError("[EventEffectRunner] 要弹三选一，但 ChapterDirector 没拖 Event Reward Panel");
                    continue;
                }

                bool decided = false;
                CardData chosen = null;
                rewardPanel.Show(candidates,
                    c => { chosen = c; decided = true; },
                    () => { decided = true; });
                while (!decided) yield return null;

                if (chosen != null && GameManager.Instance != null)
                {
                    GameManager.Instance.AddCardToDeck(chosen);
                    Debug.Log("[EventEffectRunner] 事件三选一获得：" + chosen.cardName);
                }
                continue;
            }

            // 概率分支：选中一组效果，递归执行
            if (fx.type == EventEffect.EffectType.RandomBranch)
            {
                EventEffect.RandomOutcome outcome = PickOutcome(fx);
                if (outcome != null && outcome.effects != null)
                {
                    yield return ProcessList(rewardPanel, outcome.effects, ctx, st);
                    if (st.taken) yield break;
                }
                continue;
            }

            // 其余立即执行；战斗/切场景/结算会接管流程，协程结束
            if (ApplyImmediate(fx, ctx))
            {
                st.taken = true;
                yield break;
            }
        }
    }

    // 按权重随机选一个概率分支结果
    static EventEffect.RandomOutcome PickOutcome(EventEffect fx)
    {
        if (fx.branches == null || fx.branches.Count == 0)
        {
            Debug.LogWarning("[EventEffectRunner] RandomBranch 没有配任何分支");
            return null;
        }

        int total = 0;
        foreach (EventEffect.RandomOutcome o in fx.branches)
        {
            if (o != null && o.weight > 0) total += o.weight;
        }
        if (total <= 0)
        {
            Debug.LogWarning("[EventEffectRunner] RandomBranch 所有分支权重都是 0");
            return null;
        }

        int roll = Random.Range(0, total);
        int acc = 0;
        foreach (EventEffect.RandomOutcome o in fx.branches)
        {
            if (o == null || o.weight <= 0) continue;
            acc += o.weight;
            if (roll < acc) return o;
        }
        return null;
    }

    // 立即执行单个效果；返回 true=该效果接管了流程（战斗/商店/切场景/结算）
    public static bool ApplyImmediate(EventEffect fx, IEventContext ctx)
    {
        if (fx == null || ctx == null) return false;

        switch (fx.type)
        {
            case EventEffect.EffectType.SpendTime:
                ctx.SpendTime(fx.amount);
                return false;

            case EventEffect.EffectType.RefundTime:
                ctx.RefundTime(fx.amount);
                return false;

            case EventEffect.EffectType.ChangeHP:
                ctx.Heal(fx.amount);
                return false;

            case EventEffect.EffectType.ChangeGold:
                ctx.GainGold(fx.amount);
                return false;

            case EventEffect.EffectType.GrantFixedCards:
                ctx.GrantCards(fx.fixedCards);
                // cardsNonRemovable / cardsTemp 需要“卡牌实例系统”，当前牌组直接引用资产，暂不生效
                return false;

            case EventEffect.EffectType.AddStatusCard:
                ctx.GrantCards(fx.fixedCards); // 先按进牌组处理；后续若要塞弃牌堆/战斗后移除再细化
                return false;

            case EventEffect.EffectType.StartEncounter:
                EncounterData enc = fx.encounter;
                if (enc == null && fx.encounterPool != null)
                    enc = fx.encounterPool.GetRandom();
                if (enc != null)
                {
                    ctx.EnterEncounter(enc);
                    return true; // 进战斗切场景，应是本选项最后一个效果
                }
                Debug.LogWarning("[EventEffectRunner] StartEncounter 没配遭遇，也没配遭遇池，跳过");
                return false;

            case EventEffect.EffectType.OpenShop:
                ctx.GoToScene(string.IsNullOrEmpty(fx.sceneName) ? "Shop" : fx.sceneName);
                return true;

            case EventEffect.EffectType.GoToScene:
                ctx.GoToScene(fx.sceneName);
                return true;

            case EventEffect.EffectType.SettleChapter:
                ctx.SettleChapter();
                return true;

            case EventEffect.EffectType.FinishEvent:
                ctx.FinishEvent();
                return false;

            case EventEffect.EffectType.SetFlag:
                ctx.SetFlag(fx.flag);
                return false;

            case EventEffect.EffectType.ChangeWeather:
                ctx.ChangeWeather(fx.weather);
                return false;

            case EventEffect.EffectType.RandomBranch:
                // 同步入口：选中后立即跑这组（组内若有三选一，同步下不会弹面板，会打警告）
                EventEffect.RandomOutcome outcome = PickOutcome(fx);
                return outcome != null && Run(outcome.effects, ctx);

            case EventEffect.EffectType.DrawCardChoice:
                Debug.LogWarning("[EventEffectRunner] 三选一应走 RunRoutine 协程，同步 Run 不会弹面板");
                return false;

            case EventEffect.EffectType.RemoveCard:
                Debug.Log("[EventEffectRunner] RemoveCard（删牌）需要删牌界面，后续实现");
                return false;

            case EventEffect.EffectType.UpgradeCard:
                Debug.Log("[EventEffectRunner] UpgradeCard（升级牌）需要升级界面，后续实现");
                return false;
        }
        return false;
    }
}
