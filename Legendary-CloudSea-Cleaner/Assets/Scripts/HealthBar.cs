using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image fill;           // мо Fill ╣д Image
    public TMP_Text hpText;      // мондвж
    RectTransform _fillRect;
    float _fullWidth;

    void Start()
    {
        if (fill != null)
        {
            _fillRect = fill.rectTransform;
            _fullWidth = _fillRect.rect.width;
            if (_fullWidth <= 0)
                _fullWidth = GetComponent<RectTransform>().rect.width;
        }
    }

    public void SetHealth(int current, int max)
    {
        if (fill != null)
        {
            if (_fillRect == null)
            {
                _fillRect = fill.rectTransform;
                _fullWidth = GetComponent<RectTransform>().rect.width;
            }
            float ratio = Mathf.Clamp01((float)current / max);
            _fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _fullWidth * ratio);
        }
        if (hpText != null)
        {
            hpText.text = current + "/" + max;
        }
    }
}
