using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandLayout : MonoBehaviour
{
    public GameObject cardButtonPrefab;  // 拖 Card1 预制体
    public float cardWidth = 150f;       // 卡牌宽度
    public float spacing = 110f;         // 卡牌间距
    public float arcHeight = 40f;        // 扇形弧度高度

    CombatManager _cm;

    public void Init(CombatManager cm)
    {
        _cm = cm;
    }

    // 根据手牌数量算最大角度
    float GetMaxAngle(int count)
    {
        if (count <= 1) return 0f;
        if (count <= 3) return 5f;
        if (count <= 4) return 12f;
        if (count <= 6) return 20f;
        return 25f;
    }

    // 根据手牌数量算弧度高度
    float GetArcHeight(int count)
    {
        if (count <= 1) return 0f;
        if (count <= 3) return 10f;
        if (count <= 4) return 25f;
        if (count <= 6) return 40f;
        return 50f;
    }

    public void LayoutHand(System.Collections.Generic.List<CardData> hand)
    {
        // 清空旧按钮
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        int count = hand.Count;
        if (count == 0) return;

        float maxAngle = GetMaxAngle(count);
        float currentArcHeight = GetArcHeight(count);

        // 总宽度
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        // 角度范围
        float startAngle = -maxAngle / 2f;
        float stepAngle = count > 1 ? maxAngle / (count - 1) : 0;

        for (int i = 0; i < count; i++)
        {
            GameObject cardObj = Instantiate(cardButtonPrefab, transform);
            CardButton btn = cardObj.GetComponent<CardButton>();

            float angle = startAngle + stepAngle * i;
            if (count == 1) angle = 0;

            // 水平位置
            float x = startX + i * spacing;
            // 扇形弧度（中间高，两边低，用二次函数）
            float normalizedAngle = count > 1 ? (angle / (maxAngle / 2f)) : 0;
            float y = -normalizedAngle * normalizedAngle * currentArcHeight;

            RectTransform rt = cardObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(x, y);
            rt.localRotation = Quaternion.Euler(0, 0, -angle);

            btn.SetCard(hand[i], _cm);
        }
    }
}
