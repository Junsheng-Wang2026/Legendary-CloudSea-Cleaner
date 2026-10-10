using System.Collections.Generic;
using UnityEngine;

// 敌人数据资产：普通敌人、精英、BOSS 都用同一类，用 kind 区分
// Project 里右键 Create -> CloudSea Cleaner -> Enemy Data 创建
[CreateAssetMenu(fileName = "EnemyData", menuName = "CloudSea Cleaner/Enemy Data", order = 0)]
public class EnemyData : ScriptableObject
{
    public enum EnemyKind { Normal, Elite, Boss }

    // 格挡 / 治疗 的目标（攻击和塞状态牌永远打玩家，不受这个影响）
    public enum ActionTarget
    {
        Self,           // 自己（注意隐私 / 凝固 / 增殖）
        LowestHPAlly,   // 血量百分比最低的一名友方，可能就是自己（庇护 / 我们在谈工作）
        AllAllies       // 全体存活友方，含自己（威逼利诱）
    }

    // 一招（一个行动）。一个行动可以同时带多个效果，例如：
    //   我简单讲两句 = 伤害6 + 塞1张呆滞
    //   增殖         = 自己加8格挡 + 永久每段攻击+1
    //   盘旋         = 不攻击，蓄力让下一次攻击更痛
    //   庇护         = 给血量最低友方 8 格挡
    //   威逼利诱     = 全体存活友方各 10 格挡
    //   我们在谈工作 = 奶血量最低友方，其最大生命的 40%
    [System.Serializable]
    public class EnemyAction
    {
        [Tooltip("行动名，仅备注用（如：啄击 / 盘旋 / 俯冲），不直接显示到游戏")]
        public string actionName;

        [Tooltip("伤害；0 = 本回合不攻击（伤害永远打玩家）")]
        public int damage = 0;

        [Tooltip("攻击段数，默认1；俯冲/飞溅/连击这类填 2 或 3")]
        public int hits = 1;

        [Tooltip("格挡；0 = 不加。加给谁看下面“格挡/治疗目标”（凝固/增殖给自己，庇护给最低血友方，威逼利诱给全体）")]
        public int block = 0;

        [Tooltip("格挡和治疗的目标：自己 / 血量%最低友方 / 全体友方。不影响攻击目标")]
        public ActionTarget target = ActionTarget.Self;

        [Tooltip("治疗固定值；0 = 不奶（可和百分比叠加）")]
        public int heal = 0;

        [Tooltip("按目标最大生命百分比治疗，0~100；公关“奶市长40%最大生命”填 40")]
        [Range(0, 100)] public int healPercentMaxHP = 0;

        [Tooltip("蓄力：让下一次攻击额外 +X，那次攻击打完后清空（盘旋，作用于自己）")]
        public int charge = 0;

        [Tooltip("永久成长：之后每段攻击永久 +X（霉菌增殖，作用于自己）")]
        public int permanentAttackGrowth = 0;

        [Tooltip("塞给玩家弃牌堆的状态牌（呆滞 / 晕眩 / 瘙痒 / 心情舒畅等），直接拖状态牌资产，可多张")]
        public List<CardData> inflictCards = new List<CardData>();
    }

    [Header("基础信息")]
    public string enemyName = "Enemy";
    public Sprite icon;                 // 敌人立绘/图标（战斗场景里按它更换）
    public EnemyKind kind = EnemyKind.Normal;

    [Header("战斗数值")]
    public int maxHP = 35;
    public int timeCost = 30;           // 被击杀消耗的时间（分钟，精英可填 45，BOSS 可填 45）

    [Header("出招表（按顺序循环；必须配置，留空的怪在战斗里不会行动）")]
    [Tooltip("从第几招开始，0 = 第一招。多只怪的不同起手在“遭遇”里单独覆盖")]
    public int startPhase = 0;
    [Tooltip("例如雨渍：浸湿→飞溅→凝固，配 3 个元素即可循环")]
    public List<EnemyAction> actionCycle = new List<EnemyAction>();
}
