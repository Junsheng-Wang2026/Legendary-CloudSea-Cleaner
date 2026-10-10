using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardButton : MonoBehaviour, IPointerEnterHandler
{
    public CardData card;
    public CombatManager combatManager;
    public TMP_Text buttonText;   // 描述文字
    public TMP_Text nameText;     // 卡名（可选，没拖就不显示）
    public TMP_Text costText;     // 费用（可选，没拖就不显示）
    public Image cardImage;  // 卡牌图片
    public bool enableHoverRaise = true;  // 是否允许悬停置顶（奖励面板里关掉）

    public void SetCard(CardData newCard, CombatManager cm)
    {
        card = newCard;
        combatManager = cm;
        if (buttonText != null && card != null)
        {
            buttonText.text = card.description;
        }
        if (nameText != null && card != null)
        {
            nameText.text = card.cardName;
        }
        if (costText != null && card != null)
        {
            // 状态牌（呆滞等）不显示费用
            costText.text = card.cardType == CardType.Status ? "" : card.cost.ToString();
        }
        if (cardImage != null && card != null)
        {
            cardImage.sprite = card.cardImage;
            cardImage.enabled = card.cardImage != null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!enableHoverRaise) return;
        transform.SetAsLastSibling();  // 悬停时置顶，防止点到旁边的牌
    }

    public void OnClick()
    {
        if (combatManager != null && card != null)
        {
            combatManager.PlayCard(card);
        }
    }
}
