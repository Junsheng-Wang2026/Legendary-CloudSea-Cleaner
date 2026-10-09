using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

// 关卡间闲置画面：右侧手动铺满的楼层块，点击“下一事件”后循环向上滚动（模拟玩家下降）
// 循环步长按你手动摆放的实际间距自动计算；事件楼层升到 stopMarker 高度时停止并触发 onArrived
public class FloorScroller : MonoBehaviour
{
    [Header("容器与楼层")]
    public RectTransform segmentContainer; // 铺满楼层的父容器
    public RectTransform eventFloor;       // 事件楼层（手动摆在画面下方外）
    public RectTransform stopMarker;       // 停止标记：放在玩家平行高度（容器下建空物体拖入）

    [Header("循环参数（滑块可调）")]
    public float fallbackStep = 200f;      // 只有一块楼时用的备用步长
    public float scrollSpeed = 600f;       // 最高滚动速度（像素/秒）
    public float startDelay = 0.3f;        // 点按钮后延迟多久开始滚动（秒）

    [Header("加减速（数值越小越猛）")]
    [Range(0.05f, 1f)] public float accelerateRange = 0.25f; // 加速路程占比，越小起步加速越猛
    [Range(0f, 1f)] public float startSpeedFactor = 0.15f;   // 起步速度比例，越小起步越慢
    [Range(0.05f, 1f)] public float decelerateRange = 0.2f;  // 减速路程占比，越小刹车越急
    [Range(0f, 1f)] public float endSpeedFactor = 0.2f;      // 接近停止时速度比例

    [Header("停止高度（没有 stopMarker 时用）")]
    public float playerY = 0f;

    [Header("到位回调")]
    public UnityEvent onArrived;

    readonly List<RectTransform> _segments = new List<RectTransform>();
    readonly List<float> _initSegY = new List<float>();
    float _initEventY;

    float _step;
    float _recycleY;
    float _loopSpan;
    float _targetY;

    enum State { Idle, Wait, Moving, Arrived }
    State _state = State.Idle;
    float _waitTimer = 0f;
    bool _arrivedFired = false;

    float _totalDist;
    float _traveled;

    void Start()
    {
        CollectFloors();
    }

    void CollectFloors()
    {
        _segments.Clear();
        _initSegY.Clear();

        foreach (RectTransform child in segmentContainer)
        {
            if (child == eventFloor) continue;
            if (child == stopMarker) continue;
            _segments.Add(child);
            _initSegY.Add(child.anchoredPosition.y);
            DisableRaycast(child);  // 楼层是纯视觉，不拦截点击
        }

        int n = _segments.Count;
        float maxY = float.MinValue, minY = float.MaxValue;
        foreach (float y in _initSegY)
        {
            if (y > maxY) maxY = y;
            if (y < minY) minY = y;
        }

        // 用实际摆放的相邻间距算步长
        if (n > 1) _step = (maxY - minY) / (n - 1);
        else _step = fallbackStep;
        if (_step <= 0f) _step = fallbackStep;

        _loopSpan = n * _step;          // 整个楼层列的循环周期
        _recycleY = maxY + _step;       // 超过这条线就回收到最底部

        if (eventFloor != null)
        {
            _initEventY = eventFloor.anchoredPosition.y;
            DisableRaycast(eventFloor);
        }
        _targetY = stopMarker != null ? stopMarker.anchoredPosition.y : playerY;
    }

    // 关闭物体及所有子物体 Image 的射线检测，避免挡住 UI 按钮
    void DisableRaycast(RectTransform root)
    {
        foreach (Image img in root.GetComponentsInChildren<Image>(true))
        {
            img.raycastTarget = false;
        }
    }

    // 绑“下一事件”按钮
    public void StartDescent()
    {
        ResetFloors();  // 每次开始前复位到初始铺满状态，避免卡在 Arrived 导致再点无效

        _state = State.Wait;
        _waitTimer = 0f;
        _arrivedFired = false;
        _targetY = stopMarker != null ? stopMarker.anchoredPosition.y : playerY;

        _traveled = 0f;
        if (eventFloor != null)
            _totalDist = Mathf.Max(1f, _targetY - eventFloor.anchoredPosition.y);
        else
            _totalDist = 1f;
    }

    void Update()
    {
        if (_state == State.Wait)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= startDelay) _state = State.Moving;
            return;
        }

        if (_state != State.Moving) return;

        // 进度 0→1，起步加速、中段最快、接近减速
        float p = Mathf.Clamp01(_traveled / _totalDist);
        float decelStart = 1f - decelerateRange;
        float speedFactor;
        if (p < accelerateRange)
            speedFactor = Mathf.Lerp(startSpeedFactor, 1f, p / accelerateRange); // 起步加速
        else if (p > decelStart)
            speedFactor = Mathf.Lerp(1f, endSpeedFactor, (p - decelStart) / decelerateRange); // 到位前减速
        else
            speedFactor = 1f;

        float delta = scrollSpeed * speedFactor * Time.deltaTime;
        _traveled += delta;

        // 普通楼层循环上移
        foreach (RectTransform seg in _segments)
        {
            Vector2 pos = seg.anchoredPosition;
            pos.y += delta;
            if (pos.y > _recycleY) pos.y -= _loopSpan;
            seg.anchoredPosition = pos;
        }

        // 事件楼层上移，到目标高度停止
        if (eventFloor != null)
        {
            Vector2 pos = eventFloor.anchoredPosition;
            pos.y += delta;
            if (pos.y >= _targetY)
            {
                pos.y = _targetY;
                eventFloor.anchoredPosition = pos;
                _state = State.Arrived;
                if (!_arrivedFired)
                {
                    _arrivedFired = true;
                    onArrived?.Invoke();
                }
                return;
            }
            eventFloor.anchoredPosition = pos;
        }
    }

    // 事件处理完/战斗返回后调用，复位到初始铺满状态
    public void ResetFloors()
    {
        _state = State.Idle;
        _waitTimer = 0f;

        for (int i = 0; i < _segments.Count; i++)
        {
            Vector2 pos = _segments[i].anchoredPosition;
            pos.y = _initSegY[i];
            _segments[i].anchoredPosition = pos;
        }

        if (eventFloor != null)
        {
            Vector2 pos = eventFloor.anchoredPosition;
            pos.y = _initEventY;
            eventFloor.anchoredPosition = pos;
        }
    }
}
