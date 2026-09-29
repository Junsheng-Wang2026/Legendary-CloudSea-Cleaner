using UnityEngine;
using TMPro;

public class CardButton : MonoBehaviour
{
    public CardData card;                  // 拖卡牌资产
    public CombatManager combatManager;    // 拖 CombatManager
    public TMP_Text buttonText;            // 拖按钮上的文字

    void Start()
    {
        RefreshDisplay();
    }

    public void OnClick()
    {
        combatManager.PlayCard(card);
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (card != null && buttonText != null)
        {
            buttonText.text = card.description;
        }
    }
}
