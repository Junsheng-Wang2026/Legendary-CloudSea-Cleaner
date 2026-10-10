using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 事件里的三选一面板（Boot 场景用）。和战斗胜利的 CardRewardPanel 区别：
// 不绑 CombatManager，选完通过回调把牌交回事件效果执行器，让后续效果继续跑。
// 搭法：一个面板物体挂本脚本，下挂卡牌容器、卡牌预制体（同 Card1，带 CardButton + Button），
// 可选一个“跳过”按钮（不拖就必须三选一，不能跳过）。
public class EventCardRewardPanel : MonoBehaviour
{
    public GameObject cardPrefab;     // 拖卡牌预制体（Card1）
    public Transform cardContainer;   // 拖卡牌容器
    public Button skipButton;         // 可选：跳过按钮，不拖则必须选一张

    List<CardData> _options = new List<CardData>();
    List<GameObject> _spawned = new List<GameObject>();
    Action<CardData> _onChosen;
    Action _onClosed;
    bool _decided;

    void Awake()
    {
        if (skipButton != null) skipButton.onClick.AddListener(Skip);
        gameObject.SetActive(false);
    }

    // 弹出候选牌；onChosen=选了哪张，onClosed=跳过/关闭（都只触发一次）
    public void Show(List<CardData> options, Action<CardData> onChosen, Action onClosed)
    {
        _onChosen = onChosen;
        _onClosed = onClosed;
        _decided = false;

        ClearSpawned();
        _options.Clear();

        gameObject.SetActive(true);

        if (options == null) return;
        foreach (CardData c in options)
        {
            if (c != null) _options.Add(c);
        }

        for (int i = 0; i < _options.Count; i++)
        {
            GameObject obj = Instantiate(cardPrefab, cardContainer);
            CardButton cb = obj.GetComponent<CardButton>();
            if (cb != null)
            {
                cb.enableHoverRaise = false;
                cb.SetCard(_options[i], null); // combatManager 传 null，点击不会打出
            }

            Button btn = obj.GetComponent<Button>();
            int index = i;
            if (btn != null) btn.onClick.AddListener(() => Choose(index));

            _spawned.Add(obj);
        }
    }

    void Choose(int index)
    {
        if (_decided) return;
        _decided = true;

        CardData chosen = (index >= 0 && index < _options.Count) ? _options[index] : null;
        Hide();
        _onChosen?.Invoke(chosen);
    }

    public void Skip()
    {
        if (_decided) return;
        _decided = true;
        Hide();
        _onClosed?.Invoke();
    }

    void Hide()
    {
        ClearSpawned();
        gameObject.SetActive(false);
    }

    void ClearSpawned()
    {
        foreach (GameObject o in _spawned)
        {
            if (o != null) Destroy(o);
        }
        _spawned.Clear();
    }
}
