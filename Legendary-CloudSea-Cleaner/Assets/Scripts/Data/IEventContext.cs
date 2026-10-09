// 事件执行上下文：由章节导演（ChapterDirector，块5）实现。
// 事件资产（EventData 子类）通过它影响游戏，避免资产自己去找场景物体。
public interface IEventContext
{
    void Heal(int amount);                                  // 回血（传负数=扣血），自动夹在 0~最大血量
    void GainGold(int amount);                              // 加金币
    void GainCard(CardData card);                           // 获得一张牌；传 null = 随机奖励
    void EnterFight(EnemyData enemy, bool advanceDistance); // 进入战斗（敌人可空，是否推进距离）
    void GoToScene(string sceneName);                       // 切到其他场景（如 Shop）
    void SettleChapter();                                   // 触发章节结算
    void FinishEvent();                                     // 本事件处理完，解锁下一次 Next Event
}
