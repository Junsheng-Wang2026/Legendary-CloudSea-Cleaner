using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// 一只敌人在战斗里的运行时状态（不继承 MonoBehaviour，由 CombatManager 持有）
public class EnemyRuntime
{
    public EnemyData data;
    public int maxHP = 35;
    public int hp = 35;
    public int block;          // 自己当前格挡
    public int phase;          // 出招表打到第几招
    public int charge;         // 蓄力：下次攻击额外伤害（一次性）
    public int perm;           // 永久成长：每段攻击的永久加成
    public int buffAttack;     // 旧随机路径：蓄力下回合攻击加成
    public int intent = 3;     // 旧随机路径意图：0攻 1防 2蓄力 3发呆
    public bool dead;
    public EnemySlot slot;     // 对应的 UI 槽
}

// 敌人槽 UI：一个预制体挂这个脚本，里面拖 立绘/名字/血条/意图/选中框/按钮。
// 战斗开始时 CombatManager 会按遭遇里的怪数量 Instantiate 出多个。
public class EnemySlot : MonoBehaviour
{
    [Header("外观")]
    public Image icon;                 // UI 立绘
    public SpriteRenderer worldIcon;   // 若立绘是世界空间 SpriteRenderer 就拖这个
    public TMP_Text nameText;

    [Header("信息")]
    public TMP_Text intentText;        // 意图（Attack 9 / Block 6 ...）
    public TMP_Text hpText;            // 血量数字（可选，血条自带文字也行）
    public TMP_Text blockText;         // 格挡数字（可选）
    public HealthBar healthBar;        // 这只怪的小血条

    [Header("点选")]
    public GameObject selectedHighlight;  // 当前被选中的高亮框
    public Button button;                 // 根物体上的按钮（点这只怪=选目标）

    EnemyRuntime _rt;
    Action<EnemyRuntime> _onClick;

    public void Setup(EnemyRuntime rt, Action<EnemyRuntime> onClick)
    {
        _rt = rt;
        _onClick = onClick;

        if (rt.data != null)
        {
            if (rt.data.icon != null)
            {
                if (icon != null) icon.sprite = rt.data.icon;
                if (worldIcon != null) worldIcon.sprite = rt.data.icon;
            }
            if (nameText != null) nameText.text = rt.data.enemyName;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (_rt != null && !_rt.dead) _onClick?.Invoke(_rt);
            });
        }

        if (selectedHighlight != null) selectedHighlight.SetActive(false);
    }

    // intent 文本由 CombatManager 算好传进来
    public void Refresh(string intent, bool selected)
    {
        if (_rt == null) return;

        if (healthBar != null) healthBar.SetHealth(_rt.hp, _rt.maxHP);
        if (hpText != null) hpText.text = _rt.hp + "/" + _rt.maxHP;
        if (intentText != null) intentText.text = intent;
        if (blockText != null) blockText.text = _rt.block > 0 ? ("Block:" + _rt.block) : "";
        if (selectedHighlight != null) selectedHighlight.SetActive(selected);
    }
}
