using System.Collections.Generic;
using UnityEngine;

// 一个事件效果：选项被点下后，按列表顺序逐个执行。
// 常用效果只用选“类型 + 填参数”，不用再写脚本；只有商店/云海神/章节结算才写子类。
[System.Serializable]
public class EventEffect
{
    public enum EffectType
    {
        None = 0,
        SpendTime = 1,        // 消耗时间（分钟）：观察、等待、离开+30min 等；amount 填正数
        RefundTime = 2,       // 返还时间（分钟）：amount 填正数
        ChangeHP = 3,         // 加减血：amount 正=回血，负=扣血，自动夹在 0~最大血量
        ChangeGold = 4,       // 加减金币：amount 正=获得，负=花费，不会低于 0
        GrantFixedCards = 5,  // 直接给固定牌（往 fixedCards 拖 CardData，可多张）
        DrawCardChoice = 6,   // 弹三选一（牌从哪来看 Reward Source）
        AddStatusCard = 7,    // 往玩家牌组塞状态牌（呆滞/遮沙壁风等，拖到 fixedCards）
        StartEncounter = 8,   // 开始一场战斗（拖 Encounter 固定，或 Encounter Pool 随机）
        ChangeWeather = 9,    // 改变当前天气
        OpenShop = 10,        // 进入商店场景
        GoToScene = 11,       // 切到指定场景（Scene Name 填场景名）
        SettleChapter = 12,   // 触发章节结算
        SetFlag = 13,         // 打一个标记（用于互斥/后续选项显隐，C4 用）
        FinishEvent = 14,     // 结束本事件、解锁下一节点（一串效果的最后通常自动结束）

        // 下面两类先占位，需要对应 UI，后续块再实现
        RandomBranch = 20,    // 概率分支：按权重在几组效果里随机（C4）
        RemoveCard = 21,      // 删一张牌（需删牌界面，后续）
        UpgradeCard = 22      // 升级一张牌（需升级界面，后续）
    }

    // 三选一的牌从哪来
    public enum RewardSource
    {
        JobPools,     // 按玩家主职+副职的奖励牌池（默认）
        EventPool,    // 用本效果指定的固定 CardPool（与职业无关，制作者定死）
        ByTypeRGB     // 力量/敏捷/智慧（红绿蓝）各随机一张
    }

    [Tooltip("效果类型")]
    public EffectType type = EffectType.None;

    [Tooltip("通用数值：耗时分钟 / 血量(正回负扣) / 金币(正得负花)")]
    public int amount = 0;

    [Header("给牌（GrantFixedCards / AddStatusCard 用）")]
    [Tooltip("直接给的牌，可多张")]
    public List<CardData> fixedCards = new List<CardData>();
    [Tooltip("这些牌标记为不可删除（如心情舒畅）。需要卡牌实例系统支持，暂先记录")]
    public bool cardsNonRemovable = false;
    [Tooltip("这些牌标记为临时（战斗结束移除）。需要卡牌实例系统支持，暂先记录")]
    public bool cardsTemp = false;

    [Header("三选一（DrawCardChoice 用）")]
    public RewardSource rewardSource = RewardSource.JobPools;
    [Tooltip("Reward Source = EventPool 时，拖这个与职业无关的固定牌池")]
    public CardPool rewardPool;
    [Tooltip("抽几张供选择，默认 3")]
    public int choiceCount = 3;

    [Header("战斗（StartEncounter 用，二选一）")]
    public EncounterData encounter;       // 固定遭遇
    public EncounterPool encounterPool;   // 或从遭遇池随机

    [Header("天气（ChangeWeather 用）")]
    public WeatherData weather;

    [Header("场景（GoToScene / OpenShop 用）")]
    public string sceneName = "Shop";

    [Header("标记（SetFlag 用）")]
    [Tooltip("标记名，如 gotSecret / foughtMayor；同事件用它做互斥")]
    public string flag;

    [Header("概率分支（RandomBranch 用）：按权重随机选其中一组效果执行")]
    [Tooltip("每个结果一个权重 + 这组情况下要执行的效果；权重越大越容易中")]
    public List<RandomOutcome> branches = new List<RandomOutcome>();

    // RandomBranch 的一个随机结果
    [System.Serializable]
    public class RandomOutcome
    {
        [Tooltip("权重，越大越容易被抽中（0=不会抽中）")]
        [Min(0)] public int weight = 1;

        [Tooltip("抽中这一结果后按顺序执行的效果（也可以再嵌套概率分支/三选一）")]
        public List<EventEffect> effects = new List<EventEffect>();
    }
}
