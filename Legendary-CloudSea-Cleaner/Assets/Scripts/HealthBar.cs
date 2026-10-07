using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image fill;           // 拖 Fill 的 Image
    public TMP_Text hpText;      // 拖文字

    [Header("地图场景：自动读取全局玩家血量")]
    public bool bindGameManager = false;  // Boot 主场景勾选，战斗场景不勾

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

        if (bindGameManager && GameManager.Instance != null)
        {
            SetHealth(GameManager.Instance.currentHP, GameManager.Instance.maxHP);
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
