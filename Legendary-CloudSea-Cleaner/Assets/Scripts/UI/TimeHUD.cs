using UnityEngine;
using TMPro;

/// <summary>
/// 挂在 Canvas 下的时间文本上。订阅 TimeManager 的事件，时间一变就刷新显示。
/// </summary>
public class TimeHUD : MonoBehaviour
{
    [Header("拖入你建的 TextMeshPro 文本")]
    public TMP_Text timeText;

    void OnEnable()
    {
        // OnEnable 可能比 TimeManager.Awake 先跑，这里必须判空
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged += Refresh;
        }
    }

    void OnDisable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= Refresh;
        }
    }

    void Start()
    {
        // Start 一定在所有 Awake 之后，这里再订阅一次，兜底
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= Refresh; // 先退订一次防重复
            TimeManager.Instance.OnTimeChanged += Refresh;
            Refresh(TimeManager.Instance.currentMinutes);
        }
        else
        {
            Debug.LogError("TimeManager.Instance 还是空！确认 Boot 场景里挂了 TimeManager 脚本的 GameManager 物体存在。");
        }
    }

    void Refresh(float minutes)
    {
        if (timeText != null)
        {
            timeText.text = TimeManager.Format(minutes);
        }
    }
}
