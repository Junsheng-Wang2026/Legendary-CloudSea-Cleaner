
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardButton : MonoBehaviour
{
    public CardData card;
    public CombatManager combatManager;
    public TMP_Text buttonText;
    public Image frameImage;   // 留空的话会自动用自己身上的 Image

    public void SetCard(CardData newCard, CombatManager cm)
    {
        card = newCard;
        combatManager = cm;

        if (buttonText != null && card != null)
        {
            buttonText.text = card.description;
        }

        if (frameImage == null)
        {
            frameImage = GetComponent<Image>();
        }
        if (frameImage != null && card != null && card.frame != null)
        {
            frameImage.sprite = card.frame;
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