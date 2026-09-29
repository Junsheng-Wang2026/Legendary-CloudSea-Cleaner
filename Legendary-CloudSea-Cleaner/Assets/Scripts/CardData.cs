using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cleaner/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;        // 牌名
    public int cost;               // 费用
    public int damage;             // 打多少血，0 = 不打
    public int block;              // 加多少格挡，0 = 不加
    [TextArea] public string description;  // 按钮上显示的字
}
