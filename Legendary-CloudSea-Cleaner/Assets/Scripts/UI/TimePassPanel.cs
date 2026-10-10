using UnityEngine;
using TMPro;

// 消磨时间三选一面板：点 SKIP 按钮打开，同时跳过 20 分钟
// 三个选项：随机回 3~8 体力 / 彩票点数 +1 / 删牌点数 +1；没有跳过按钮，必须选一个
public class TimePassPanel : MonoBehaviour
{
    [Header("面板与参数")]
    public GameObject panel;            // 三选一面板根物体（默认隐藏）
    public float minutesToSkip = 20f;   // 跳过的分钟数
    public int pointsNeeded = 3;        // 点数攒够多少可用（刮彩票/删牌界面暂不做）

    [Header("点数文字（可选，显示 x/3）")]
    public TMP_Text lotteryLabel;
    public TMP_Text removeLabel;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
        RefreshLabels();
    }

    // 绑 SKIP 按钮：打开面板并跳过时间
    public void OpenPanel()
    {
        if (panel != null) panel.SetActive(true);
        if (TimeManager.Instance != null)
            TimeManager.Instance.SpendTime(minutesToSkip);
        RefreshLabels();
    }

    // 选项一：随机回复 3~8 点体力
    public void ChooseHeal()
    {
        if (GameManager.Instance != null)
        {
            int heal = Random.Range(3, 9); // 3..8
            GameManager.Instance.currentHP = Mathf.Min(
                GameManager.Instance.maxHP,
                GameManager.Instance.currentHP + heal);
            Debug.Log("消磨时间：回复体力 " + heal);
        }
        Close();
    }

    // 选项二：彩票点数 +1（刮彩票功能暂不做）
    public void ChooseLottery()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.lotteryPoints++;
            Debug.Log("彩票点数：" + GameManager.Instance.lotteryPoints
                + "/" + pointsNeeded
                + (GameManager.Instance.lotteryPoints >= pointsNeeded ? "（已可刮彩票，界面待做）" : ""));
        }
        RefreshLabels();
        Close();
    }

    // 选项三：删牌点数 +1（删牌功能暂不做）
    public void ChooseRemove()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.removePoints++;
            Debug.Log("删牌点数：" + GameManager.Instance.removePoints
                + "/" + pointsNeeded
                + (GameManager.Instance.removePoints >= pointsNeeded ? "（已可删牌，界面待做）" : ""));
        }
        RefreshLabels();
        Close();
    }

    void Close()
    {
        if (panel != null) panel.SetActive(false);
    }

    void RefreshLabels()
    {
        if (GameManager.Instance == null) return;
        if (lotteryLabel != null)
            lotteryLabel.text = UIText.Lottery + GameManager.Instance.lotteryPoints + "/" + pointsNeeded;
        if (removeLabel != null)
            removeLabel.text = UIText.Remove + GameManager.Instance.removePoints + "/" + pointsNeeded;
    }
}
