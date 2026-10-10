using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MapNode : MonoBehaviour
{
    public GameObject eventPanel;          // 拖 EventPanel
    public TMP_Text eventDescriptionText;  // 事件描述文字
    public TMP_Text option1ButtonText;     // 旧选项1按钮文字
    public TMP_Text option2ButtonText;     // 旧选项2按钮文字
    public EventData eventData;            // 旧测试入口：节点按钮自带事件

    [Header("新多选项动态按钮（C5）")]
    [Tooltip("选项按钮预制体：根节点是 Button、下面带一个 TMP 文字。可把现有选项按钮拖成预制体")]
    public Button optionButtonPrefab;

    [Tooltip("动态按钮的父容器（一个 RectTransform）；不拖就直接生成在 EventPanel 下")]
    public Transform optionButtonContainer;

    [Tooltip("相邻选项按钮的纵向间距")]
    public float optionSpacing = 70f;

    [Tooltip("第一个按钮距容器顶部的距离（往下为正）")]
    public float optionTopOffset = 20f;

    [Tooltip("按钮横向偏移（一般 0 居中）")]
    public float optionXOffset = 0f;

    [Tooltip("旧的两个选项按钮物体（Element0=选项1，Element1=选项2）。新多选项事件会隐藏它们，旧事件仍显示")]
    public GameObject[] legacyOptionButtons;

    EventData _currentEvent;
    readonly List<GameObject> _spawnedOptions = new List<GameObject>();

    // 旧的节点按钮测试入口
    public void OnClickNode()
    {
        if (eventData != null)
            ShowEvent(eventData);
    }

    // 供章节导演在新多选项效果跑完后关闭事件面板
    public void HideEventPanel()
    {
        ClearSpawnedOptions();
        if (eventPanel != null) eventPanel.SetActive(false);
    }

    // 显示指定事件（章节导演到位后调用）
    public void ShowEvent(EventData ev)
    {
        if (ev == null)
        {
            ChapterDirector.Instance?.FinishEvent();
            return;
        }
        _currentEvent = ev;
        ClearSpawnedOptions();

        // 强制战斗事件：不弹选项面板，直接开打
        if (ev.forceFight)
        {
            StartFight(ev, 0);
            return;
        }

        // 新的多选项事件：动态生成按钮
        if (ev.HasNewOptions())
        {
            ShowDynamicOptions(ev);
            return;
        }

        // ===== 以下为旧两选项事件，逻辑保持不变 =====
        bool hasOption = !string.IsNullOrEmpty(ev.option1Text)
                      || !string.IsNullOrEmpty(ev.option2Text);

        // 没有选项文字：有敌人就直接打，没有则视为空事件直接结束
        if (!hasOption)
        {
            if (ResolveEnemy(ev) != null)
            {
                StartFight(ev, 0);
                return;
            }
            eventPanel.SetActive(false);
            ChapterDirector.Instance?.FinishEvent();
            return;
        }

        eventPanel.SetActive(true);
        if (eventDescriptionText != null) eventDescriptionText.text = ev.description;
        if (option1ButtonText != null) option1ButtonText.text = ev.option1Text;
        if (option2ButtonText != null) option2ButtonText.text = ev.option2Text;

        SetLegacyButtons(true);
        // 旧第二个按钮没有文字就隐藏
        if (legacyOptionButtons != null && legacyOptionButtons.Length >= 2 && legacyOptionButtons[1] != null)
            legacyOptionButtons[1].SetActive(!string.IsNullOrEmpty(ev.option2Text));
    }

    // 新多选项事件：按 options 动态生成按钮，纵向排列，按 flag 决定显隐
    void ShowDynamicOptions(EventData ev)
    {
        eventPanel.SetActive(true);
        if (eventDescriptionText != null) eventDescriptionText.text = ev.description;
        SetLegacyButtons(false); // 隐藏旧的两个固定按钮

        if (optionButtonPrefab == null)
        {
            Debug.LogError("[MapNode] 这是新多选项事件，但没拖 Option Button Prefab（选项按钮预制体）");
            return;
        }

        Transform container = optionButtonContainer != null ? optionButtonContainer : eventPanel.transform;
        GameManager gm = GameManager.Instance;

        int shown = 0;
        foreach (EventOption opt in ev.options)
        {
            if (opt == null) continue;
            if (!opt.IsVisible(gm)) continue; // flag 条件不满足，不显示

            Button btn = Instantiate(optionButtonPrefab, container);
            GameObject obj = btn.gameObject;
            obj.SetActive(true);
            obj.name = "Option_" + shown + "_" + opt.buttonText;

            // 手动纵向排列（锚点顶部居中、pivot 顶部居中），不使用 LayoutGroup
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(optionXOffset, -optionTopOffset - shown * optionSpacing);
            }

            TMP_Text label = obj.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = opt.buttonText;

            EventOption captured = opt;
            btn.onClick.RemoveAllListeners(); // 清掉预制体上可能带的旧测试回调
            btn.onClick.AddListener(() => OnClickDynamicOption(captured));

            _spawnedOptions.Add(obj);
            shown++;
        }

        if (shown == 0)
            Debug.LogWarning("[MapNode] 事件 " + ev.eventName + " 没有任何满足 flag 条件的可见选项");
    }

    void OnClickDynamicOption(EventOption option)
    {
        if (ChapterDirector.Instance != null)
            ChapterDirector.Instance.RunOptionEffects(option);
        else
            Debug.LogError("[MapNode] 找不到 ChapterDirector，无法执行选项效果");
    }

    void SetLegacyButtons(bool visible)
    {
        if (legacyOptionButtons == null) return;
        foreach (GameObject b in legacyOptionButtons)
        {
            if (b != null) b.SetActive(visible);
        }
    }

    void ClearSpawnedOptions()
    {
        foreach (GameObject o in _spawnedOptions)
        {
            if (o != null) Destroy(o);
        }
        _spawnedOptions.Clear();
    }

    // 解析事件要打的敌人：固定敌人优先，否则从池子随机，都没有返回 null
    EnemyData ResolveEnemy(EventData ev)
    {
        if (ev.enemy != null) return ev.enemy;
        if (ev.enemyPool != null) return ev.enemyPool.GetRandom();
        return null;
    }

    // 进入战斗：扣选项耗时 -> 带敌人进 Combat
    // 章节导演流程下距离已在下降动画段推进，这里统一不再补推距离（advanceDistance=false）
    void StartFight(EventData ev, int optionTimeCost)
    {
        if (TimeManager.Instance != null && optionTimeCost > 0)
            TimeManager.Instance.SpendTime(optionTimeCost);

        EnemyData enemy = ResolveEnemy(ev);

        if (GameManager.Instance != null)
            GameManager.Instance.EnterCombat(enemy, false);
        else
            SceneManager.LoadScene("Combat");
    }

    public void OnClickOption1()
    {
        if (_currentEvent == null) return;

        if (_currentEvent.option1IsFight)
        {
            StartFight(_currentEvent, _currentEvent.option1TimeCost);
            return;
        }

        ApplyNonFight(_currentEvent.option1TimeCost, _currentEvent.option1HPChange, _currentEvent.option1Reward);
    }

    public void OnClickOption2()
    {
        if (_currentEvent == null) return;

        if (_currentEvent.option2IsFight)
        {
            StartFight(_currentEvent, _currentEvent.option2TimeCost);
            return;
        }

        ApplyNonFight(_currentEvent.option2TimeCost, _currentEvent.option2HPChange, _currentEvent.option2Reward);
    }

    // 非战斗选项：扣时间、改血量、给奖励牌，然后通知导演本事件结束
    void ApplyNonFight(int timeCost, int hpChange, CardData reward)
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.SpendTime(timeCost);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHP = Mathf.Max(
                0,
                Mathf.Min(GameManager.Instance.maxHP, GameManager.Instance.currentHP + hpChange));
            if (reward != null)
                GameManager.Instance.playerDeck.Add(reward);
        }

        eventPanel.SetActive(false);
        ChapterDirector.Instance?.FinishEvent();
    }
}
