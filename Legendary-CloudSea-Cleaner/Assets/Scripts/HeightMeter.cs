using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeightMeter : MonoBehaviour
{
    public Image fillBar;  // 竖条填充
    public TMP_Text depthText;  // 显示还剩多少米

    void Update()
    {
        if (GameManager.Instance == null) return;

        float pct = (float)GameManager.Instance.currentDepth / GameManager.Instance.checkpointDepth;
        fillBar.fillAmount = pct;

        int remaining = GameManager.Instance.checkpointDepth - GameManager.Instance.currentDepth;
        depthText.text = "还剩 " + remaining + "m";
    }

    // 过一个节点扣高度
    public void AddDepth(int amount)
    {
        GameManager.Instance.currentDepth += amount;
        if (GameManager.Instance.currentDepth >= GameManager.Instance.checkpointDepth)
        {
            Debug.Log("到达检查点！");
        }
    }
}
