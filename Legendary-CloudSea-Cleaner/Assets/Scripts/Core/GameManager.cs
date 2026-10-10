using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("全局玩家状态")]
    public int maxHP = 100;
    public int currentHP = 100;
    public int gold = 0;          // 金币
    public int chapter = 1;       // 当前章节（第几章）

    [Header("消磨时间点数")]
    public int lotteryPoints = 0;  // 彩票点数（攒够可刮彩票，刮奖界面暂不做）
    public int removePoints = 0;   // 删牌点数（攒够可删一张牌，删牌界面暂不做）

    [Header("战斗标记")]
    public bool isBossFight = false;

    [Header("下一场战斗的敌人（由地图/敌人池在进战斗前设置，Combat 场景读取）")]
    public EnemyData pendingEnemy;

    [Header("下一场战斗的遭遇（多敌人组合；和 pendingEnemy 二选一，遭遇优先）")]
    public EncounterData pendingEncounter;

    [Header("本章 BOSS（章节开始时固定或随机确定，左侧面板提前显示）")]
    public EnemyData chapterBoss;

    [Header("全局牌组")]
    public List<CardData> playerDeck = new List<CardData>();

    [Header("职业（主职必填、副职可空；配了主职就会在开局按职业牌池自动组牌）")]
    public JobData mainJob;
    public JobData subJob;

    [Header("中立奖励兜底牌（职业奖励牌池凑不满 3 张时补，可空）")]
    public List<CardData> fallbackRewardCards = new List<CardData>();

    [Header("高度进度")]
    public int currentDepth = 0;
    public int checkpointDepth = 100;  // 到检查点需要的高度
    public int currentEventIndex = 0;  // 当前是本章第几起事件（用于取每段距离）
    public bool pendingDistanceAdvance = false;  // 战斗胜利返回后是否推进一次距离（旧流程保留）
    public int activeChapterNumber = 0;  // 已初始化、正在进行的章节号（切场景返回后用于恢复而非重开）
    public WeatherData currentWeather;   // 本章已抽定的天气（切场景返回后恢复左侧栏，避免重抽）

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 配了主职、且牌组还是空的：开局按 主职+副职 的初始牌池自动组一次牌
        if (mainJob != null && (playerDeck == null || playerDeck.Count == 0))
        {
            BuildStartingDeckFromJobs();
        }
    }

    /// <summary>按 主职+副职 的初始牌池重新展开生成开局牌组（会清空并替换当前 playerDeck）。</summary>
    public void BuildStartingDeckFromJobs()
    {
        playerDeck = CardPool.BuildStartingCards(JobData.CollectStartingPools(mainJob, subJob));
    }

    /// <summary>战斗胜利三选一：从 主职+副职 的奖励牌池按权重抽 count 张互不相同的牌；
    /// 同一张牌在多个池出现时权重累加，凑不够用 fallbackRewardCards 补。
    /// 没配主职时返回 null，调用方回退到自己的测试奖励牌。</summary>
    public List<CardData> DrawRewardCards(int count = 3)
    {
        if (mainJob == null) return null;
        return CardPool.DrawRewards(JobData.CollectRewardPools(mainJob, subJob), count, fallbackRewardCards);
    }

    /// <summary>三选一选中后把牌加入玩家牌组。</summary>
    public void AddCardToDeck(CardData card)
    {
        if (card != null) playerDeck.Add(card);
    }

    // 统一进入战斗入口：传入单个敌人（可为 null，Combat 会回退默认值）和这次战斗是否推进距离
    public void EnterCombat(EnemyData enemy, bool advanceDistance)
    {
        pendingEnemy = enemy;
        pendingEncounter = null;
        isBossFight = enemy != null && enemy.kind == EnemyData.EnemyKind.Boss;
        pendingDistanceAdvance = advanceDistance;
        SceneManager.LoadScene("Combat");
    }

    // 统一进入战斗入口：传入一整个遭遇（多敌人组合）
    public void EnterCombat(EncounterData encounter, bool advanceDistance)
    {
        pendingEncounter = encounter;
        pendingEnemy = null;
        isBossFight = false;
        if (encounter != null)
        {
            foreach (EncounterData.Member m in encounter.members)
            {
                if (m != null && m.enemy != null && m.enemy.kind == EnemyData.EnemyKind.Boss)
                {
                    isBossFight = true;
                    break;
                }
            }
        }
        pendingDistanceAdvance = advanceDistance;
        SceneManager.LoadScene("Combat");
    }

    [Header("事件标记 flag（DontDestroyOnLoad，存活整个 run；用于事件互斥/选项显隐）")]
    public HashSet<string> flags = new HashSet<string>();

    public void SetFlag(string f)
    {
        if (!string.IsNullOrEmpty(f)) flags.Add(f);
    }

    public bool HasFlag(string f)
    {
        return !string.IsNullOrEmpty(f) && flags.Contains(f);
    }

    public void RemoveFlag(string f)
    {
        if (!string.IsNullOrEmpty(f)) flags.Remove(f);
    }

    // 开始新的一局时清空（回主菜单/重开可调用）
    public void ResetFlags()
    {
        flags.Clear();
    }
}
