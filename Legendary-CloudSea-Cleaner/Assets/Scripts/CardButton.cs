using UnityEngine;
using TMPro;

public class CardButton : MonoBehaviour
{
    public CardData card;
    public CombatManager combatManager;
    public TMP_Text buttonText;

    public void SetCard(CardData newCard, CombatManager cm)
    {
        card = newCard;
        combatManager = cm;
        if (buttonText != null && card != null)
        {
            buttonText.text = card.description;
        }
    }

    public void OnClick()
    {
        if (combatManager != null && card != null)
        {
            combatManager.PlayCard(card);
        }
    }
}
