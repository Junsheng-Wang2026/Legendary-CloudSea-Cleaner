using UnityEngine;
using TMPro;

// 顶部玩家状态栏
// 静止（开局/事件中）显示“章 - 节”，只有楼层下降动画播放时显示 Descending
public class TopStatusBar : MonoBehaviour
{
    public TMP_Text statusText;    // 关卡状态文字
    public TMP_Text goldText;      // 金币文字（Gold: 0）

    void Start()
    {
        ShowIdle();
        RefreshGold();
    }

    // 静止状态：显示章与节（开局还没事件时只显示章）
    public void ShowIdle()
    {
        if (statusText == null || GameManager.Instance == null) return;

        if (GameManager.Instance.currentEventIndex <= 0)
            statusText.text = UIText.Chapter(GameManager.Instance.chapter);
        else
            statusText.text = UIText.ChapterEvent(GameManager.Instance.chapter,
                GameManager.Instance.currentEventIndex);
    }

    // 楼层下降动画播放时调用：显示“下降中”
    public void SetDescending()
    {
        if (statusText != null && GameManager.Instance != null)
            statusText.text = UIText.Descending;
    }

    // 事件楼层到位时调用：回到章与节显示
    public void SetEvent()
    {
        ShowIdle();
    }

    public void RefreshGold()
    {
        if (goldText != null && GameManager.Instance != null)
            goldText.text = UIText.Gold + GameManager.Instance.gold;
    }

    // 事件/战斗获得或花费金币时调用
    public void AddGold(int amount)
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.gold = Mathf.Max(0, GameManager.Instance.gold + amount);
        RefreshGold();
    }
}
