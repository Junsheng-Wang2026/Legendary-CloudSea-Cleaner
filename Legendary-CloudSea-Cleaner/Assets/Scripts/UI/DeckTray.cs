using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 底部折叠卡组栏（工具箱）：左=主职颜色块，中=玩家牌组卡牌数量，右=副职颜色块。
/// 颜色自动取 GameManager.mainJob/subJob 的 jobColor；数量取 playerDeck.Count。
/// 没有主职/副职时对应色块显示灰色；副职为空时右块保持灰色（不隐藏，维持左右对称）。
/// 点击展开浏览牌组属于后续功能，本脚本只负责显示。
/// </summary>
public class DeckTray : MonoBehaviour
{
    [Header("职业颜色块（拖 Image）")]
    [Tooltip("左侧：主职颜色")]
    public Image mainJobColor;
    [Tooltip("右侧：副职颜色（无副职时显示灰色）")]
    public Image subJobColor;

    [Header("卡牌数量（拖 TextMeshPro 文本）")]
    public TMP_Text countText;

    [Tooltip("没有职业/副职时色块显示的灰色")]
    public Color emptyColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    int _lastCount = -1;
    JobData _lastMain;
    JobData _lastSub;

    void OnEnable()
    {
        Refresh();  // 从 Combat 返回 Boot 重新激活时刷新
    }

    void Update()
    {
        // 牌组数量或职业变化时才刷新（开局组牌、战斗拿牌后都能自动跟上）
        GameManager gm = GameManager.Instance;
        int n = (gm != null && gm.playerDeck != null) ? gm.playerDeck.Count : 0;
        JobData m = gm != null ? gm.mainJob : null;
        JobData s = gm != null ? gm.subJob : null;
        if (n != _lastCount || m != _lastMain || s != _lastSub)
        {
            Refresh();
        }
    }

    /// <summary>按当前 GameManager 状态重绘折叠栏（也可在外部手动调用）。</summary>
    public void Refresh()
    {
        GameManager gm = GameManager.Instance;
        int n = (gm != null && gm.playerDeck != null) ? gm.playerDeck.Count : 0;
        JobData m = gm != null ? gm.mainJob : null;
        JobData s = gm != null ? gm.subJob : null;

        if (countText != null) countText.text = n.ToString();
        if (mainJobColor != null) mainJobColor.color = m != null ? m.jobColor : emptyColor;
        if (subJobColor != null) subJobColor.color = s != null ? s.jobColor : emptyColor;

        _lastCount = n;
        _lastMain = m;
        _lastSub = s;
    }
}
