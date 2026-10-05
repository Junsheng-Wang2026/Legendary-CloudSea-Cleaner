using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardButton : MonoBehaviour, IPointerEnterHandler
{
    public CardData card;
    public CombatManager combatManager;
    public TMP_Text buttonText;
    public Image cardImage;  // 卡牌图片

    public void SetCard(CardData newCard, CombatManager cm)
    {
        card = newCard;
        combatManager = cm;
        if (buttonText != null && card != null)
        {
            buttonText.text = card.description;
        }
        if (cardImage != null && card != null)
        {
            cardImage.sprite = card.cardImage;
            cardImage.enabled = card.cardImage != null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
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
