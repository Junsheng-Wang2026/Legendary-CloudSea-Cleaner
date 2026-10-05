using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Cleaner/Event Data")]
public class EventData : ScriptableObject
{
    public string eventName;        // 事件名
    [TextArea] public string description;  // 事件描述
    public enum EventType { Fight, Choice, Interaction }
    public EventType eventType;     // 战斗/选择/交互

    // 选项
    public string option1Text;      // 选项1文字
    public bool option1IsFight;     // 选项1是否进战斗
    public int option1TimeCost;     // 选项1时间消耗
    public int option1HPChange;     // 选项1血量变化
    public CardData option1Reward;  // 选项1奖励牌（可选）

    public string option2Text;      // 选项2文字
    public bool option2IsFight;     // 选项2是否进战斗
    public int option2TimeCost;     // 选项2时间消耗
    public int option2HPChange;     // 选项2血量变化
    public CardData option2Reward;  // 选项2奖励牌（可选）
}
