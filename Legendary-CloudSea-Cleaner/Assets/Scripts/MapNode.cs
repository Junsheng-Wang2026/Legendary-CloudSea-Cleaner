using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapNode : MonoBehaviour
{
    public GameObject eventPanel;   // 拖 EventPanel
    public TMP_Text eventDescriptionText;  // 事件描述文字
    public TMP_Text option1ButtonText;     // 选项1按钮文字
    public TMP_Text option2ButtonText;     // 选项2按钮文字
    public EventData eventData;    // 旧测试入口：节点按钮自带事件

    EventData _currentEvent;

    // 旧的节点按钮测试入口
    public void OnClickNode()
    {
        if (eventData != null)
            ShowEvent(eventData);
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

        // 强制战斗事件：不弹选项面板，直接开打
        if (ev.forceFight)
        {
            StartFight(ev, 0);
            return;
        }

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

        eventDescriptionText.text = ev.description;
        option1ButtonText.text = ev.option1Text;
        option2ButtonText.text = ev.option2Text;

        eventPanel.SetActive(true);
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
