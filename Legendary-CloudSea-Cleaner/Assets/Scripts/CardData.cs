using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cleaner/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;        // 牌名
    public enum CardType { Strength, Agility, Wisdom, Status }
    public CardType cardType;      // 力量/敏捷/智慧/状态
    public enum Rarity { Common, Uncommon, Rare }
    public Rarity rarity;          // 白/蓝/紫
    public int cost;               // 费用
    public int damage;             // 打多少血，0 = 不打
    public int block;              // 加多少格挡，0 = 不加
    public Sprite cardImage;        // 卡牌图片
    [TextArea] public string description;  // 按钮上显示的字
}
