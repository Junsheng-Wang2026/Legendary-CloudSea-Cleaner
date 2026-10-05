using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MapNode : MonoBehaviour
{
    public GameObject eventPanel;   // 拖 EventPanel
    public TMP_Text eventDescriptionText;  // 事件描述文字
    public TMP_Text option1ButtonText;     // 选项1按钮文字
    public TMP_Text option2ButtonText;     // 选项2按钮文字
    public EventData eventData;    // 拖事件数据

    EventData _currentEvent;

    public void OnClickNode()
    {
        if (eventData == null) return;
        _currentEvent = eventData;

        eventDescriptionText.text = _currentEvent.description;
        option1ButtonText.text = _currentEvent.option1Text;
        option2ButtonText.text = _currentEvent.option2Text;

        eventPanel.SetActive(true);
    }

    public void OnClickOption1()
    {
        if (_currentEvent == null) return;

        // 选项1进战斗
        if (_currentEvent.option1IsFight)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.SpendTime(_currentEvent.option1TimeCost);
            }
            SceneManager.LoadScene("Combat");
            return;
        }

        // 应用选项1效果
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SpendTime(_currentEvent.option1TimeCost);
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHP += _currentEvent.option1HPChange;
            if (_currentEvent.option1Reward != null)
            {
                GameManager.Instance.playerDeck.Add(_currentEvent.option1Reward);
            }
        }

        eventPanel.SetActive(false);
    }

    public void OnClickOption2()
    {
        if (_currentEvent == null) return;

        // 选项2进战斗
        if (_currentEvent.option2IsFight)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.SpendTime(_currentEvent.option2TimeCost);
            }
            SceneManager.LoadScene("Combat");
            return;
        }

        // 应用选项2效果
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SpendTime(_currentEvent.option2TimeCost);
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHP += _currentEvent.option2HPChange;
            if (_currentEvent.option2Reward != null)
            {
                GameManager.Instance.playerDeck.Add(_currentEvent.option2Reward);
            }
        }

        eventPanel.SetActive(false);
    }
}
