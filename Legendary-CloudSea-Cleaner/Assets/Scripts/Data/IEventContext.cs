using System.Collections.Generic;

// 事件执行上下文：由章节导演（ChapterDirector）实现。
// 事件资产 / 事件效果（EventEffectRunner）通过它影响游戏，避免资产自己去找场景物体。
public interface IEventContext
{
    void Heal(int amount);                                  // 回血（传负数=扣血），自动夹在 0~最大血量
    void GainGold(int amount);                              // 加金币（传负数=花金币，不会低于 0）
    void GainCard(CardData card);                           // 获得一张牌；传 null = 随机奖励
    void GrantCards(List<CardData> cards);                  // 一次获得多张牌（固定牌/状态牌）
    void SpendTime(int minutes);                            // 消耗时间（分钟，传正数）
    void RefundTime(int minutes);                           // 返还时间（分钟，传正数）
    void EnterFight(EnemyData enemy, bool advanceDistance); // 进入单怪战斗（敌人可空，是否推进距离）
    void EnterEncounter(EncounterData encounter);           // 进入一整场多敌人遭遇战斗
    void GoToScene(string sceneName);                       // 切到其他场景（如 Shop）
    void SettleChapter();                                   // 触发章节结算
    void FinishEvent();                                     // 本事件处理完，解锁下一次 Next Event

    void SetFlag(string flag);                              // 打一个事件标记（跨场景存活整个 run）
    bool HasFlag(string flag);                              // 是否已有某标记
    void ChangeWeather(WeatherData weather);                // 改变当前天气并刷新左侧栏
}
