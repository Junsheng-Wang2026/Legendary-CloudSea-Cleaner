using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    [Header("玩家 UI")]
    public TMP_Text apText;
    public TMP_Text playerHPText;
    public TMP_Text blockText;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public HealthBar playerHealthBar;   // 玩家血条
    public CardRewardPanel rewardPanel; // 胜利三选一奖励面板

    [Header("多敌人（必配）")]
    public EnemySlot enemySlotPrefab;   // 敌人槽预制体
    public Transform enemyContainer;    // 敌人槽父物体（摆到敌人站立区）
    public EncounterData debugEncounter;// 测试用：直开 Combat 场景就打这个遭遇（正式由事件设置，可空）

    [Header("敌人排列（代码自动横排居中，不用 LayoutGroup）")]
    public float enemySpacing = 220f;   // 相邻敌人槽中心的水平间距
    public float enemyRowY = 0f;        // 整排相对敌人容器中心的上下偏移

    [Header("玩家数值")]
    public int maxAP = 3;
    public int playerHP = 100;
    public int maxPlayerHP = 100;
    public int cardsPerTurn = 5;

    [Header("手牌与遗物")]
    public HandLayout handLayout;
    public RelicData equippedRelic;

    // 多敌人运行时
    readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();
    EnemyRuntime _selected;
    int _encounterTimeCost = 30;

    List<CardData> _drawPile = new List<CardData>();
    List<CardData> _hand = new List<CardData>();
    List<CardData> _discardPile = new List<CardData>();

    int _currentAP;
    int _block;
    bool _lostHPThisTurn;

    void Start()
    {
        _currentAP = maxAP;
        _block = 0;
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            playerHP = GameManager.Instance.currentHP;
            maxPlayerHP = GameManager.Instance.maxHP;
        }

        BuildEnemies();
        SpawnEnemyUI();
        AutoSelectTarget();

        // 牌组统一来自 GameManager（开局由主职/副职牌池组好）
        if (GameManager.Instance != null && GameManager.Instance.playerDeck != null)
            _drawPile = new List<CardData>(GameManager.Instance.playerDeck);
        Shuffle(_drawPile);

        int startDraw = cardsPerTurn;
        if (equippedRelic != null && equippedRelic.effect == RelicData.EffectType.ExtraDraw)
            startDraw += equippedRelic.value;
        DrawCards(startDraw);

        RefreshUI();
    }

    // ===== 进场：决定这场打哪些怪（只认遭遇）=====

    void BuildEnemies()
    {
        EncounterData enc = null;
        if (GameManager.Instance != null)
        {
            enc = GameManager.Instance.pendingEncounter;
            if (enc != null) GameManager.Instance.pendingEncounter = null;
        }
        if (enc == null) enc = debugEncounter;  // 没走事件入口时，用 Inspector 上的测试遭遇

        if (enc == null)
        {
            Debug.LogError("[CombatManager] 没有战斗遭遇：请让事件设置遭遇，或在 Inspector 拖 Debug Encounter");
            return;
        }

        foreach (EncounterData.Member m in enc.members)
            if (m != null && m.enemy != null) AddEnemy(m.enemy, Mathf.Max(0, m.startPhase));
        _encounterTimeCost = enc.GetTimeCost();
    }

    void AddEnemy(EnemyData data, int startPhase)
    {
        _enemies.Add(new EnemyRuntime
        {
            data = data,
            maxHP = data.maxHP,
            hp = data.maxHP,
            phase = startPhase
        });
    }

    void SpawnEnemyUI()
    {
        if (enemySlotPrefab == null || enemyContainer == null)
        {
            Debug.LogError("[CombatManager] 必须配置 Enemy Slot Prefab 和 Enemy Container，否则战斗里看不到敌人");
            return;
        }

        int n = _enemies.Count;
        for (int i = 0; i < n; i++)
        {
            EnemyRuntime rt = _enemies[i];
            EnemySlot slot = Instantiate(enemySlotPrefab, enemyContainer);
            rt.slot = slot;
            slot.Setup(rt, OnClickEnemy);

            // 以容器中心为准，按序号水平居中横排（不用 LayoutGroup，不锁子物体）
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            if (slotRect != null)
            {
                slotRect.anchorMin = new Vector2(0.5f, 0.5f);
                slotRect.anchorMax = new Vector2(0.5f, 0.5f);
                slotRect.pivot = new Vector2(0.5f, 0.5f);
                float x = (i - (n - 1) / 2f) * enemySpacing;
                slotRect.anchoredPosition = new Vector2(x, enemyRowY);
                slotRect.localScale = Vector3.one;
            }
        }
    }

    void OnClickEnemy(EnemyRuntime rt)
    {
        if (rt == null || rt.dead) return;
        _selected = rt;
        RefreshUI();
    }

    void AutoSelectTarget()
    {
        foreach (EnemyRuntime e in _enemies)
            if (!e.dead) { _selected = e; return; }
        _selected = null;
    }

    // ===== 出招表 / 意图 =====

    EnemyData.EnemyAction CurrentAction(EnemyRuntime rt)
    {
        if (rt.data == null || rt.data.actionCycle == null || rt.data.actionCycle.Count == 0)
            return null;
        return rt.data.actionCycle[rt.phase % rt.data.actionCycle.Count];
    }

    string IntentText(EnemyRuntime rt)
    {
        EnemyData.EnemyAction a = CurrentAction(rt);
        if (a == null) return "Intent: Idle";  // 没配出招表的怪不行动

        var parts = new List<string>();

        if (a.damage > 0)
        {
            int perHit = a.damage + rt.perm;
            int total = perHit * Mathf.Max(1, a.hits) + rt.charge;
            string atk = "Attack " + total;
            if (a.hits > 1) atk += "x" + a.hits;
            parts.Add(atk);
        }

        if (a.block > 0)
        {
            if (a.target == EnemyData.ActionTarget.AllAllies) parts.Add("Protect all " + a.block);
            else if (a.target == EnemyData.ActionTarget.LowestHPAlly) parts.Add("Protect ally " + a.block);
            else parts.Add("Block " + a.block);
        }

        if (a.heal > 0 || a.healPercentMaxHP > 0)
            parts.Add(a.target == EnemyData.ActionTarget.AllAllies ? "Heal all" : "Heal ally");

        if (a.charge > 0) parts.Add("Charge +" + a.charge);
        if (a.permanentAttackGrowth > 0) parts.Add("Empower");
        if (a.inflictCards != null && a.inflictCards.Count > 0) parts.Add("Debuff");

        if (parts.Count == 0) return "Intent: Idle";
        return "Intent: " + string.Join(" + ", parts.ToArray());
    }

    // 敌人按出招表出一招
    void ExecuteAction(EnemyRuntime rt, EnemyData.EnemyAction a)
    {
        if (a.inflictCards != null)
            foreach (CardData c in a.inflictCards)
                if (c != null) _discardPile.Add(c);

        // 蓄力 / 永久成长作用于自己
        if (a.permanentAttackGrowth > 0) rt.perm += a.permanentAttackGrowth;
        if (a.charge > 0) rt.charge += a.charge;

        // 格挡 / 治疗：按“目标”给自己或友方
        ApplySupport(rt, a);

        if (a.damage > 0)
        {
            int perHit = a.damage + rt.perm;
            int dmg = perHit * Mathf.Max(1, a.hits) + rt.charge;
            rt.charge = 0;
            DamagePlayer(dmg);
        }
    }

    // 友方辅助：格挡 / 治疗
    void ApplySupport(EnemyRuntime caster, EnemyData.EnemyAction a)
    {
        if (a.block <= 0 && a.heal <= 0 && a.healPercentMaxHP <= 0) return;

        foreach (EnemyRuntime t in GetSupportTargets(caster, a.target))
        {
            if (t == null || t.dead) continue;

            if (a.block > 0) t.block += a.block;

            int amount = a.heal;
            if (a.healPercentMaxHP > 0)
                amount += Mathf.RoundToInt(t.maxHP * a.healPercentMaxHP / 100f);
            if (amount > 0) t.hp = Mathf.Min(t.maxHP, t.hp + amount);
        }
    }

    List<EnemyRuntime> GetSupportTargets(EnemyRuntime caster, EnemyData.ActionTarget target)
    {
        var list = new List<EnemyRuntime>();

        if (target == EnemyData.ActionTarget.AllAllies)
        {
            foreach (EnemyRuntime e in _enemies) if (!e.dead) list.Add(e);
            return list;
        }

        if (target == EnemyData.ActionTarget.LowestHPAlly)
        {
            EnemyRuntime best = null;
            float bestRatio = float.MaxValue;
            foreach (EnemyRuntime e in _enemies)
            {
                if (e.dead) continue;
                float ratio = e.maxHP > 0 ? (float)e.hp / e.maxHP : 0f;
                if (ratio < bestRatio) { bestRatio = ratio; best = e; }
            }
            if (best != null) list.Add(best);
            return list;
        }

        // Self
        if (!caster.dead) list.Add(caster);
        return list;
    }

    void DamagePlayer(int dmg)
    {
        if (dmg <= 0) return;
        if (_block > 0)
        {
            int absorbed = Mathf.Min(_block, dmg);
            _block -= absorbed;
            dmg -= absorbed;
        }
        if (dmg > 0) playerHP = Mathf.Max(0, playerHP - dmg);
    }

    // ===== 抽牌 =====

    void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_drawPile.Count == 0)
            {
                _drawPile = new List<CardData>(_discardPile);
                _discardPile.Clear();
                Shuffle(_drawPile);
            }
            if (_drawPile.Count == 0) break;
            CardData card = _drawPile[0];
            _drawPile.RemoveAt(0);
            _hand.Add(card);
        }
        UpdateHandUI();
    }

    void UpdateHandUI()
    {
        if (handLayout != null)
        {
            handLayout.Init(this);
            handLayout.LayoutHand(_hand);
        }
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }

    // ===== 打牌（打当前选中的敌人）=====

    public void PlayCard(CardData card)
    {
        if (playerHP <= 0) return;
        if (card.cardType == CardType.Status) return;
        if (_currentAP < card.cost) return;
        if (!_hand.Contains(card)) return;

        AutoSelectTarget();
        if (_selected == null) return;

        _currentAP -= card.cost;
        _hand.Remove(card);

        if (!card.exhaust) _discardPile.Add(card);

        if (card.selfDamage > 0)
        {
            playerHP = Mathf.Max(0, playerHP - card.selfDamage);
            _lostHPThisTurn = true;
        }

        if (card.damage > 0)
            for (int i = 0; i < card.attackTimes; i++)
                DamageEnemy(_selected, card.damage, card.ignoreBlock);

        if (_lostHPThisTurn && card.conditionalDamage > 0)
            DamageEnemy(_selected, card.conditionalDamage, card.ignoreBlock);

        if (card.block > 0) _block += card.block;
        if (_lostHPThisTurn && card.conditionalBlock > 0) _block += card.conditionalBlock;

        if (card.drawCards > 0) DrawCards(card.drawCards);
        if (_lostHPThisTurn && card.conditionalDraw > 0) DrawCards(card.conditionalDraw);
        if (card.gainEnergy > 0) _currentAP += card.gainEnergy;

        UpdateHandUI();
        RefreshUI();
        CheckEnemiesDead();
    }

    void DamageEnemy(EnemyRuntime rt, int dmg, bool ignoreBlock)
    {
        if (rt == null || rt.dead || dmg <= 0) return;
        if (!ignoreBlock && rt.block > 0)
        {
            int absorbed = Mathf.Min(rt.block, dmg);
            rt.block -= absorbed;
            dmg -= absorbed;
        }
        if (dmg > 0) rt.hp = Mathf.Max(0, rt.hp - dmg);
        if (rt.hp <= 0) KillEnemy(rt);
    }

    void KillEnemy(EnemyRuntime rt)
    {
        if (rt.dead) return;
        rt.dead = true;
        if (rt.slot != null) Destroy(rt.slot.gameObject);
        if (_selected == rt) AutoSelectTarget();
    }

    bool AllEnemiesDead()
    {
        if (_enemies.Count == 0) return false;
        foreach (EnemyRuntime e in _enemies) if (!e.dead) return false;
        return true;
    }

    // ===== 结束回合：所有存活敌人依次按出招表行动 =====

    public void OnClick_EndTurn()
    {
        if (playerHP <= 0 || AllEnemiesDead()) return;

        _discardPile.AddRange(_hand);
        _hand.Clear();
        _lostHPThisTurn = false;

        foreach (EnemyRuntime rt in _enemies)
        {
            if (rt.dead) continue;
            EnemyData.EnemyAction act = CurrentAction(rt);
            if (act != null)
            {
                ExecuteAction(rt, act);
                rt.phase++;
            }
            // 没配出招表的怪：本轮不行动
        }

        _block = 0;  // 玩家护甲回合结束清空

        _currentAP = maxAP;
        int drawCount = cardsPerTurn;
        if (equippedRelic != null && equippedRelic.effect == RelicData.EffectType.ExtraDraw)
            drawCount += equippedRelic.value;
        DrawCards(drawCount);

        RefreshUI();
        CheckPlayerDead();
    }

    public void OnClick_ReturnToBoot()
    {
        SceneManager.LoadScene("Boot");
    }

    void CheckEnemiesDead()
    {
        if (!AllEnemiesDead()) return;

        if (TimeManager.Instance != null)
            TimeManager.Instance.SpendTime(_encounterTimeCost);

        // 胜利三选一：从主职/副职奖励牌池抽；没配职业或凑不满则直接显示胜利面板
        List<CardData> rewardOptions = null;
        if (GameManager.Instance != null)
            rewardOptions = GameManager.Instance.DrawRewardCards(3);

        if (rewardPanel != null && rewardOptions != null && rewardOptions.Count > 0)
            rewardPanel.ShowRewards(rewardOptions);
        else
            victoryPanel.SetActive(true);
    }

    public void ShowVictoryAfterReward()
    {
        victoryPanel.SetActive(true);
    }

    void CheckPlayerDead()
    {
        if (playerHP <= 0) defeatPanel.SetActive(true);
    }

    void RefreshUI()
    {
        if (playerHealthBar != null) playerHealthBar.SetHealth(playerHP, maxPlayerHP);
        if (apText != null) apText.text = "AP:" + _currentAP;
        if (playerHPText != null) playerHPText.text = "PlayerHP:" + playerHP + "/" + maxPlayerHP;
        if (blockText != null) blockText.text = "Block:" + _block;

        foreach (EnemyRuntime rt in _enemies)
        {
            if (rt.dead) continue;
            if (rt.slot != null) rt.slot.Refresh(IntentText(rt), rt == _selected);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.currentHP = playerHP;
    }
}
