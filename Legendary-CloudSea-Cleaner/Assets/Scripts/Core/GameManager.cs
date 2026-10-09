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

    [Header("本章 BOSS（章节开始时固定或随机确定，左侧面板提前显示）")]
    public EnemyData chapterBoss;

    [Header("全局牌组")]
    public List<CardData> playerDeck = new List<CardData>();

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

    // 统一进入战斗入口：传入敌人（可为 null，Combat 会回退默认值）和这次战斗是否推进距离
    public void EnterCombat(EnemyData enemy, bool advanceDistance)
    {
        pendingEnemy = enemy;
        isBossFight = enemy != null && enemy.kind == EnemyData.EnemyKind.Boss;
        pendingDistanceAdvance = advanceDistance;
        SceneManager.LoadScene("Combat");
    }
}
