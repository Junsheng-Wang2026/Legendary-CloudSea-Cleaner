using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 主场景距离进度条（竖向蓝色，Fill 从底部匀速增长）
// 文本显示“距离本章检查点 XXM”；完成一个事件调用 AdvanceEvent() 推进一段。
// 总距离与各段距离由 ChapterDirector 在章节开始时通过 ConfigureByChapter 写入。
public class DistanceBar : MonoBehaviour
{
    [Header("章节数据（一般留空，由 ChapterDirector 喂；单场景测试可手动拖）")]
    public ChapterData chapterData;

    [Header("距离配置（米，一般由章节数据自动写入，也可手填兜底）")]
    public int checkpointDistance = 100;          // 到本章检查点总距离
    public int[] eventDistances = { 20, 20, 20, 20, 20 }; // 每个节点段的距离，按节点序号依次取
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

        if (chapterData != null)
            ConfigureByChapter(chapterData);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.checkpointDepth = checkpointDistance;

            // 章节导演流程：距离在下降动画段已推进，战斗/商店返回后只恢复显示，不再补推
            _displayedDepth = GameManager.Instance.currentDepth;
            _targetDepth = GameManager.Instance.currentDepth;
        }

        UpdateBar(_displayedDepth);
    }

    // 按章节资产配置：总距离=各节点段距之和，分段=各节点段距
    [ContextMenu("Apply Chapter Data")]
    public void ConfigureByChapter(ChapterData chapter)
    {
        if (chapter == null) return;

        checkpointDistance = Mathf.Max(1, Mathf.RoundToInt(chapter.GetTotalDistance()));

        float[] segs = chapter.GetSegmentDistances();
        eventDistances = new int[segs.Length];
        for (int i = 0; i < segs.Length; i++)
            eventDistances[i] = Mathf.RoundToInt(segs[i]);

        if (GameManager.Instance != null)
            GameManager.Instance.checkpointDepth = checkpointDistance;
    }

    // 切场景返回后：按 GameManager 当前深度立即同步显示（不做动画、不推进）
    public void RefreshToCurrentDepth()
    {
        if (GameManager.Instance == null) return;
        if (fill != null && _fullHeight <= 0)
            _fullHeight = GetComponent<RectTransform>().rect.height;

        _displayedDepth = GameManager.Instance.currentDepth;
        _targetDepth = GameManager.Instance.currentDepth;
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

    // 开始前往下一节点时调用：按当前节点序号推进对应距离（与楼层下降动画同时）
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
