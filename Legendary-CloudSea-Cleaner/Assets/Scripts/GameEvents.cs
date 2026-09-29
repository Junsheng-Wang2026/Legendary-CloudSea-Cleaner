/// <summary>
/// 全局事件总线。所有跨模块通信走这里，不要互相 FindObjectOfType。
/// 用法：
///   发：GameEvents.OnCombatVictory?.Invoke();
///   收：void OnEnable() { GameEvents.OnCombatVictory += OnWin; }
///       void OnDisable() { GameEvents.OnCombatVictory -= OnWin; }
/// </summary>
public static class GameEvents
{
    // ===== 资源类 =====
    /// <summary>时间变了，参数是新的剩余分钟数</summary>
    public static event System.Action<float> OnTimeChangedMin;

    /// <summary>体力变了，参数是 (当前, 最大)</summary>
    public static event System.Action<int, int> OnHPChanged;

    /// <summary>时间归零，该触发超时 BOSS 了</summary>
    public static event System.Action OnTimeUpBoss;

    // ===== 战斗类 =====
    public static event System.Action OnPlayerTurnStart;
    public static event System.Action OnEnemyTurnStart;
    public static event System.Action OnCombatVictory;
    public static event System.Action OnCombatDefeat;

    // ===== 地图/流程类 =====
    public static event System.Action OnEventNodeEntered;
    public static event System.Action OnCheckpointReached;
    public static event System.Action<int> OnChapterChanged;  // 参数是新章节 1/2/3

    // ===== 下面是给外部调用的触发方法（静态类里 event 不能在外面 Invoke，要用包装方法）=====
    public static void RaiseTimeChangedMin(float min) => OnTimeChangedMin?.Invoke(min);
    public static void RaiseHPChanged(int cur, int max) => OnHPChanged?.Invoke(cur, max);
    public static void RaiseTimeUpBoss() => OnTimeUpBoss?.Invoke();
    public static void RaisePlayerTurnStart() => OnPlayerTurnStart?.Invoke();
    public static void RaiseEnemyTurnStart() => OnEnemyTurnStart?.Invoke();
    public static void RaiseCombatVictory() => OnCombatVictory?.Invoke();
    public static void RaiseCombatDefeat() => OnCombatDefeat?.Invoke();
    public static void RaiseEventNodeEntered() => OnEventNodeEntered?.Invoke();
    public static void RaiseCheckpointReached() => OnCheckpointReached?.Invoke();
    public static void RaiseChapterChanged(int act) => OnChapterChanged?.Invoke(act);
}
