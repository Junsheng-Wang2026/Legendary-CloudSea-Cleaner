using UnityEngine;

// 敌人数据资产：普通敌人、精英、BOSS 都用同一类，用 kind 区分
// Project 里右键 Create -> CloudSea Cleaner -> Enemy Data 创建
[CreateAssetMenu(fileName = "EnemyData", menuName = "CloudSea Cleaner/Enemy Data", order = 0)]
public class EnemyData : ScriptableObject
{
    public enum EnemyKind { Normal, Elite, Boss }

    [Header("基础信息")]
    public string enemyName = "Enemy";
    public Sprite icon;                 // 敌人立绘/图标（战斗场景里按它更换）
    public EnemyKind kind = EnemyKind.Normal;

    [Header("战斗数值")]
    public int maxHP = 35;
    public int attack = 7;              // 攻击意图伤害
    public int defendAmount = 5;        // 防御意图加的护甲
    public int buffAmount = 7;          // 蓄力意图：下次攻击 +多少
    [Range(0f, 1f)] public float statusChance = 0.3f;  // 攻击时塞状态牌概率
    public int timeCost = 30;           // 被击杀消耗的时间（分钟，精英可填 45）
}
