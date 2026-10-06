using UnityEngine;

public enum CardType { Strength, Agility, Wisdom, Status }
public enum Rarity { Common, Uncommon, Rare, Gold }
public enum StatusEffect { None, Dazed, Itch, Stun, GoodMood, Energized }

[CreateAssetMenu(fileName = "NewCard", menuName = "Cleaner/Card Data")]
public class CardData : ScriptableObject
{
    [Header("基础信息")]
    public string cardName;
    public CardType cardType;
    public Rarity rarity;
    public int cost;
    public Sprite cardImage;
    [TextArea] public string description;

    [Header("基础效果")]
    public int damage;             // 单次伤害
    public int attackTimes = 1;    // 攻击次数
    public int block;              // 格挡
    public int drawCards;          // 抽牌数
    public int gainEnergy;         // 回能
    public int selfDamage;         // 自损血量

    [Header("特殊标记")]
    public bool exhaust;           // 消耗（打出后移除本局）
    public bool isTemp;            // 临时牌（战斗结束移除）
    public bool ignoreBlock;       // 无视敌方格挡

    [Header("状态效果")]
    public int applyWeak;          // 施加虚弱回合数
    public int applyVulnerable;    // 施加易伤回合数

    [Header("条件效果（本回合损失过生命值触发）")]
    public int conditionalDamage;  // 条件额外伤害
    public int conditionalBlock;   // 条件额外格挡
    public int conditionalDraw;    // 条件额外抽牌

    [Header("状态牌专属")]
    public StatusEffect statusEffect;  // 状态牌效果类型
}
