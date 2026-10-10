// 所有由代码拼出来的界面文字都集中在这里，想改措辞只改这个文件
// （这个文件是 UTF-8 编码，中文字符串放这里不会乱码）
public static class UIText
{
    // ===== 战斗界面 =====
    public const string EnemyHP      = "敌人生命：";
    public const string IntentAttack = "意图：攻击 ";
    public const string IntentDefend = "意图：防御 +";
    public const string IntentBuff   = "意图：蓄力，下次攻击 +";
    public const string IntentIdle   = "意图：待机";
    public const string AP           = "行动点：";
    public const string PlayerHP     = "生命：";
    public const string Block        = "格挡：";

    // ===== Boot 顶部状态栏 =====
    public const string Descending = "下降中...";
    public const string Gold       = "金币：";
    public static string Chapter(int chapter) { return "第" + chapter + "章"; }
    public static string ChapterEvent(int chapter, int eventIndex) { return "第" + chapter + "章 第" + eventIndex + "节"; }

    // ===== 消磨时间面板 =====
    public const string Lottery = "彩票 ";
    public const string Remove  = "删牌 ";

    // ===== 左侧距离条 =====
    public static string DistanceToCheckpoint(int remain) { return "距离本章检查点 " + remain + "米"; }
}
