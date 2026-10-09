using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 全局时间管理器。挂在 Boot 场景的 GameManager 物体上，DontDestroyOnLoad。
/// 时间单位：分钟（策划案里的 12 小时 = 720 分钟）。
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

 [Header("初始时间（分钟）")]
    public float startMinutes = 720f;   // 12 小时

    [Header("当前剩余时间（只读，运行时看）")]
    public float currentMinutes;

    // 时间变化时广播，参数是"新的剩余分钟数"
    public event System.Action<float> OnTimeChanged;

    // 时间耗尽时广播（用来触发 BOSS）
    public event System.Action OnTimeUp;

    private bool _isTimeUp = false;

    void Awake()
    {
        // 单例：防止重复创建
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentMinutes = startMinutes;
    }

    void Start()
    {
        // 开局广播一次，让 HUD 刷新到正确的数字
        OnTimeChanged?.Invoke(currentMinutes);
    }

    /// <summary>
    /// 扣时间。minutes 传正数。
    /// </summary>
    public void SpendTime(float minutes)
    {
        if (_isTimeUp) return;

        currentMinutes = Mathf.Max(0f, currentMinutes - minutes);
        OnTimeChanged?.Invoke(currentMinutes);

        if (currentMinutes <= 0f)
        {
            _isTimeUp = true;
            OnTimeUp?.Invoke();
        }
    }

    /// <summary>
    /// 把分钟格式化成 "HH:MM" 显示。例如 720 -> "12:00"，719 -> "11:59"。
    /// </summary>
    public static string Format(float minutes)
    {
        int total = Mathf.CeilToInt(minutes);   // 向上取整，显示成 11:59 而不是 11:59.3
        int h = total / 60;
        int m = total % 60;
        return $"{h:00}:{m:00}";
    }
}
