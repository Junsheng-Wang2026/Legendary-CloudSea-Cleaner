using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 章节导演（块5b：章节初始化 + 楼层/距离联动 + 到位执行事件 + 切场景返回恢复）
// 挂在 Boot 场景一个始终激活的物体上；Next Event 按钮绑 OnNextEvent。
public class ChapterDirector : MonoBehaviour, IEventContext
{
    public static ChapterDirector Instance { get; private set; }

    [Header("章节（按顺序拖入，Element 0=第1章；多章都放这里）")]
    public List<ChapterData> chapters = new List<ChapterData>();

    [Header("测试用：直接指定一章（拖了就用它，忽略上面列表）")]
    public ChapterData overrideChapter;

    [Header("场景引用")]
    public DistanceBar distanceBar;
    public FloorScroller floorScroller;
    public ChapterStatusBar statusBar;     // 左侧章节栏（天气/BOSS）
    public TopStatusBar topStatusBar;      // 顶部状态栏（章-节 / Descending）
    [Tooltip("挂 MapNode、且接好 EventPanel 与两个选项按钮的物体，用来弹战斗/选择事件面板")]
    public MapNode eventPanelHost;

    ChapterData _chapter;
    int _currentNodeIndex;
    bool _moving;       // 楼层下降动画播放中
    bool _eventActive;  // 到位后事件尚未处理完，锁住 Next Event

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (floorScroller != null)
            floorScroller.onArrived.AddListener(OnArrivedAtNode);

        BeginChapter();
    }

    ChapterData ResolveChapterData(int ch)
    {
        int idx = ch - 1;
        if (overrideChapter != null) return overrideChapter;
        if (idx >= 0 && idx < chapters.Count) return chapters[idx];
        if (chapters.Count > 0) return chapters[0];
        return null;
    }

    // 进入/恢复一章
    public void BeginChapter()
    {
        int ch = GameManager.Instance != null ? GameManager.Instance.chapter : 1;
        _chapter = ResolveChapterData(ch);

        if (_chapter == null)
        {
            Debug.LogWarning("[ChapterDirector] 没有配置任何章节数据");
            return;
        }

        // 从战斗/商店场景返回 Boot：本章已初始化过，只恢复，不重抽天气/BOSS、不重置进度
        bool returning = GameManager.Instance != null
                      && GameManager.Instance.activeChapterNumber == ch;

        if (!returning)
        {
            WeatherData weather = _chapter.ResolveWeather();
            EnemyData boss = _chapter.ResolveBoss();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentWeather = weather;
                GameManager.Instance.chapterBoss = boss;
                GameManager.Instance.currentDepth = 0;
                GameManager.Instance.currentEventIndex = 0;
                GameManager.Instance.pendingDistanceAdvance = false;
                GameManager.Instance.activeChapterNumber = ch;
            }

            if (statusBar != null)
                statusBar.SetChapter(weather, boss);

            if (distanceBar != null)
            {
                distanceBar.ConfigureByChapter(_chapter);
                distanceBar.ResetChapter();
            }

            ResetEventPools(_chapter);
            _currentNodeIndex = 0;

            Debug.Log("[ChapterDirector] 新章节开始，总距离 " + _chapter.GetTotalDistance()
                + "m，节点数 " + _chapter.nodes.Count);
        }
        else
        {
            // 恢复：距离条按本章配置并同步到已下降深度，天气/BOSS 用已抽定的
            if (distanceBar != null)
            {
                distanceBar.ConfigureByChapter(_chapter);
                distanceBar.RefreshToCurrentDepth();
            }
            if (statusBar != null && GameManager.Instance != null)
                statusBar.SetChapter(GameManager.Instance.currentWeather, GameManager.Instance.chapterBoss);

            _currentNodeIndex = GameManager.Instance != null ? GameManager.Instance.currentEventIndex : 0;
            Debug.Log("[ChapterDirector] 从战斗/商店返回，恢复到节点进度 " + _currentNodeIndex);
        }

        _moving = false;
        _eventActive = false;

        if (topStatusBar != null)
            topStatusBar.ShowIdle();
    }

    void ResetEventPools(ChapterData c)
    {
        if (c.nodes == null) return;
        foreach (ChapterNode n in c.nodes)
        {
            if (n != null && n.eventPool != null)
                n.eventPool.ResetPool();
        }
    }

    // 绑 Next Event 按钮：开始下降 + 距离条推进当前段
    public void OnNextEvent()
    {
        if (_chapter == null || _moving || _eventActive) return;
        if (GameManager.Instance == null) return;

        int nodeIdx = GameManager.Instance.currentEventIndex;
        if (nodeIdx >= _chapter.nodes.Count)
        {
            Debug.Log("[ChapterDirector] 本章节点已走完");
            return;
        }

        _currentNodeIndex = nodeIdx;
        _moving = true;

        if (topStatusBar != null)
            topStatusBar.SetDescending();

        if (floorScroller != null)
            floorScroller.StartDescent();

        if (distanceBar != null)
            distanceBar.AdvanceEvent();
    }

    // 楼层到位：解析本节点事件并执行
    void OnArrivedAtNode()
    {
        _moving = false;
        if (topStatusBar != null)
            topStatusBar.SetEvent();

        EventData ev = _chapter.ResolveNodeEvent(_currentNodeIndex);

        if (ev == null)
        {
            Debug.Log("[ChapterDirector] 节点 " + (_currentNodeIndex + 1) + " 无事件，直接通过");
            FinishEvent();
            return;
        }

        Debug.Log("[ChapterDirector] 到达节点 " + (_currentNodeIndex + 1)
            + "，事件：" + ev.eventName + "（类型 " + ev.GetType().Name + "）");

        if (ev.GetType() == typeof(EventData))
        {
            // 基类事件：战斗 / 二选一，走事件面板
            _eventActive = true;
            if (eventPanelHost != null)
                eventPanelHost.ShowEvent(ev);
            else
            {
                Debug.LogError("[ChapterDirector] 没有拖 Event Panel Host，无法弹事件面板");
                FinishEvent();
            }
        }
        else
        {
            // 子类事件（商店/休息/结算…）：多态执行，由事件自身决定何时 FinishEvent
            _eventActive = true;
            ev.Execute(this);
        }
    }

    // ===== IEventContext 实现 =====

    public void Heal(int amount)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.currentHP = Mathf.Max(
            0, Mathf.Min(GameManager.Instance.maxHP, GameManager.Instance.currentHP + amount));

        HealthBar hb = FindObjectOfType<HealthBar>();
        if (hb != null)
            hb.SetHealth(GameManager.Instance.currentHP, GameManager.Instance.maxHP);
    }

    public void GainGold(int amount)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.gold = Mathf.Max(0, GameManager.Instance.gold + amount);
        if (topStatusBar != null) topStatusBar.RefreshGold();
    }

    public void GainCard(CardData card)
    {
        if (GameManager.Instance == null) return;
        if (card != null)
        {
            GameManager.Instance.playerDeck.Add(card);
            Debug.Log("[ChapterDirector] 获得卡牌：" + card.name);
        }
        else
        {
            // Boot 场景暂无随机牌池；需要随机奖励时在事件资产上直接指定奖励牌
            Debug.Log("[ChapterDirector] 请求随机奖励牌，但 Boot 未配置牌池，暂不发牌");
        }
    }

    public void EnterFight(EnemyData enemy, bool advanceDistance)
    {
        // 章节导演流程距离已在下降段推进，统一不再补推
        if (GameManager.Instance != null)
            GameManager.Instance.EnterCombat(enemy, false);
        else
            SceneManager.LoadScene("Combat");
    }

    public void GoToScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        bool exists = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (SceneUtility.GetScenePathByBuildIndex(i).Contains(sceneName))
            {
                exists = true;
                break;
            }
        }

        if (exists)
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("[ChapterDirector] 场景未加入 Build Settings，无法跳转：" + sceneName);
    }

    public void SettleChapter()
    {
        // 块6实现：章节结算（回血/奖励/结算面板/进入下一章）。现在先锁在章末并打日志。
        _eventActive = true;
        Debug.Log("[ChapterDirector] 触发章节结算（块6实现：回血/奖励/下一章）");
    }

    public void FinishEvent()
    {
        _eventActive = false;
        Debug.Log("[ChapterDirector] 本节点事件结束，可继续下一节点");
    }
}
