using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 主场景距离进度条（竖向蓝色，Fill 从底部匀速增长）
// 文本显示“距离本章检查点 XXM”；完成一个事件调用 AdvanceEvent() 推进一段
public class DistanceBar : MonoBehaviour
{
    [Header("距离配置（米）")]
    public int checkpointDistance = 100;          // 到本章检查点总距离
    public int[] eventDistances = { 20, 20, 20, 20, 20 }; // 每起事件之间的距离，按事件序号依次取
    public int fallbackDistance = 20;             // 数组取完后的默认每段距离

    [Header("动画")]
    public float fillDuration = 2.86f;  // 每段距离的填充时长（秒），速度会按本段距离自动适配

    [Header("UI 引用")]
    public Image fill;                  // 蓝色 Fill（锚点贴底，Pivot (0.5,0)）
    public TMP_Text distanceText;       // 显示“距离本章检查点 XXM”

    RectTransform _fillRect;
    float _fullHeight;
    float _displayedDepth;   // 当前动画显示到的深度
    float _targetDepth;      // 目标深度
    float _animSpeed;       // 本段动画速度（米/秒），由距离/时长算出

    void Start()
    {
        if (fill != null)
        {
            _fillRect = fill.rectTransform;
            _fullHeight = _fillRect.rect.height;
            if (_fullHeight <= 0)
                _fullHeight = GetComponent<RectTransform>().rect.height;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.checkpointDepth = checkpointDistance;

            // 从战斗胜利返回：先结算一次待推进距离
            if (GameManager.Instance.pendingDistanceAdvance)
            {
                GameManager.Instance.pendingDistanceAdvance = false;
                AdvanceEvent();
            }

            _displayedDepth = GameManager.Instance.currentDepth;
            _targetDepth = GameManager.Instance.currentDepth;
        }

        UpdateBar(_displayedDepth);
    }

    void Update()
    {
        if (_displayedDepth < _targetDepth)
        {
            _displayedDepth = Mathf.MoveTowards(
                _displayedDepth, _targetDepth, _animSpeed * Time.deltaTime);
            UpdateBar(_displayedDepth);
        }
    }

    // 完成一起事件后调用：按当前事件序号推进对应距离
    public void AdvanceEvent()
    {
        if (GameManager.Instance == null) return;

        int idx = GameManager.Instance.currentEventIndex;
        int d = fallbackDistance;
        if (eventDistances != null && eventDistances.Length > 0)
            d = eventDistances[Mathf.Min(idx, eventDistances.Length - 1)];

        GameManager.Instance.currentEventIndex = idx + 1;
        GameManager.Instance.currentDepth = Mathf.Min(
            checkpointDistance,
            GameManager.Instance.currentDepth + d);

        _targetDepth = GameManager.Instance.currentDepth;
        // 速度按本段距离自适应：保证无论距离多少，填充时长都约为 fillDuration
        _animSpeed = Mathf.Max(1f, d / Mathf.Max(0.01f, fillDuration));

        if (GameManager.Instance.currentDepth >= checkpointDistance)
            Debug.Log("到达本章检查点！");
    }

    // 按给定深度刷新 Fill 高度和文字
    void UpdateBar(float depth)
    {
        float ratio = Mathf.Clamp01(depth / checkpointDistance);
        if (_fillRect != null)
        {
            if (_fullHeight <= 0) _fullHeight = GetComponent<RectTransform>().rect.height;
            _fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _fullHeight * ratio);
        }

        int remain = Mathf.Max(0, Mathf.CeilToInt(checkpointDistance - depth));
        if (distanceText != null)
            distanceText.text = "距离本章检查点 " + remain + "M";
    }

    // 新章节重置
    public void ResetChapter()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.currentDepth = 0;
        GameManager.Instance.currentEventIndex = 0;
        GameManager.Instance.pendingDistanceAdvance = false;
        _displayedDepth = 0f;
        _targetDepth = 0f;
        UpdateBar(0f);
    }
}
