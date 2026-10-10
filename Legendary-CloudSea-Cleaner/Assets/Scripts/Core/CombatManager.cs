using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text enemyHPText;
    public TMP_Text enemyIntentText;
    public TMP_Text apText;
    public TMP_Text playerHPText;
    public TMP_Text blockText;
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public HealthBar playerHealthBar;   // 玩家血条
    public HealthBar enemyHealthBar;    // 旧单敌人血条（没做多敌人槽时兜底）
    public CardRewardPanel rewardPanel; // 三选一奖励面板

    [Header("敌人外观（旧单敌人 UI，没做多敌人槽时用）")]
    public SpriteRenderer enemySpriteRenderer;
    public Image enemyImage;
    public TMP_Text enemyNameText;
    public EnemyData enemyData;         // 单场景测试用敌人（没走事件入口时用）

    [Header("多敌人（拖了就按遭遇生成多只；不拖则用上面单敌人 UI 兜底）")]
    public EnemySlot enemySlotPrefab;   // 敌人槽预制体
    public Transform enemyContainer;    // 敌人槽父物体
    public EncounterData debugEncounter;// 测试用：直开 Combat 场景就打这个遭遇（正式进战斗由事件设置，可空）

    [Header("战斗数值（无敌人资产时的兜底默认怪）")]
    public int enemyHP = 35;
    public int enemyAttack = 7;
    public int maxAP = 3;
    public int playerHP = 100;
    public int maxPlayerHP = 100;
    public int cardsPerTurn = 5;

    [Header("牌组")]
    public List<CardData> startingDeck = new List<CardData>();

    [Header("奖励牌池")]
    public List<CardData> rewardCards = new List<CardData>();

    [Header("手牌布局")]
    public HandLayout handLayout;

    [Header("遗物")]
    public RelicData equippedRelic;

    [Header("状态牌（旧随机兜底用）")]
    public CardData statusCard;

    // 多敌人运行时
    readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();
    EnemyRuntime _selected;
    int _encounterTimeCost = 30;

    // 无敌人资产兜底怪的随机出招默认值
    const int FallbackDefend = 5;
    const int FallbackBuff = 7;
    const float FallbackStatusChance = 0.3f;

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

        // 初始化牌组
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerDeck.Count == 0)
                GameManager.Instance.playerDeck = new List<CardData>(startingDeck);
            _drawPile = new List<CardData>(GameManager.Instance.playerDeck);
        }
        else
        {
            _drawPile = new List<CardData>(startingDeck);
        }
        Shuffle(_drawPile);

        int startDraw = cardsPerTurn;
        if (equippedRelic != null && equippedRelic.effect == RelicData.EffectType.ExtraDraw)
            startDraw += equippedRelic.value;
        DrawCards(startDraw);

        foreach (EnemyRuntime e in _enemies) RefreshIntent(e);
        RefreshUI();
    }

    // ===== 进场：决定这场打哪些怪 =====

    void BuildEnemies()
    {
        EncounterData enc = null;
        if (GameManager.Instance != null)
        {
            enc = GameManager.Instance.pendingEncounter;
            if (enc != null) GameManager.Instance.pendingEncounter = null;
        }
        if (enc == null) enc = debugEncounter;  // 没走事件入口时，用 Inspector 上的测试遭遇

        if (enc != null)
        {
            foreach (EncounterData.Member m in enc.members)
                if (m != null && m.enemy != null) AddEnemy(m.enemy, Mathf.Max(0, m.startPhase));
            _encounterTimeCost = enc.GetTimeCost();
            return;
        }

        EnemyData data = null;
        if (GameManager.Instance != null && GameManager.Instance.pendingEnemy != null)
        {
            data = GameManager.Instance.pendingEnemy;
            GameManager.Instance.pendingEnemy = null;
        }
        else if (enemyData != null)
        {
            data = enemyData;
        }

        if (data != null)
        {
            AddEnemy(data, Mathf.Max(0, data.startPhase));
            _encounterTimeCost = data.timeCost;
        }
        else if (GameManager.Instance != null && GameManager.Instance.isBossFight)
        {
            EnemyRuntime rt = MakeRuntime(null);
            rt.maxHP = rt.hp = 300;
            _enemies.Add(rt);
            _encounterTimeCost = 45;
            GameManager.Instance.isBossFight = false;
        }
        else
        {
            EnemyRuntime rt = MakeRuntime(null);
            rt.maxHP = rt.hp = enemyHP;
            _enemies.Add(rt);
            _encounterTimeCost = 30;
        }
    }

    EnemyRuntime MakeRuntime(EnemyData data)
    {
        return new EnemyRuntime { data = data, maxHP = 35, hp = 35, intent = 3 };
    }

    void AddEnemy(EnemyData data, int startPhase)
    {
        EnemyRuntime rt = MakeRuntime(data);
        rt.maxHP = data.maxHP;
        rt.hp = data.maxHP;
        rt.phase = startPhase;
        _enemies.Add(rt);
    }

    void SpawnEnemyUI()
    {
        bool useSlots = enemySlotPrefab != null && enemyContainer != null;
        foreach (EnemyRuntime rt in _enemies)
        {
            if (useSlots)
            {
                EnemySlot slot = Instantiate(enemySlotPrefab, enemyContainer);
                rt.slot = slot;
                slot.Setup(rt, OnClickEnemy);
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

    // 没配出招表的怪才随机选意图
    void RefreshIntent(EnemyRuntime rt)
    {
        if (CurrentAction(rt) != null) return;
        if (rt.buffAttack > 0) rt.intent = 0;
        else rt.intent = Random.Range(0, 4);
    }

    string IntentText(EnemyRuntime rt)
    {
        EnemyData.EnemyAction a = CurrentAction(rt);
        if (a != null)
        {
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

        int baseAttack = rt.data != null ? rt.data.attack : enemyAttack;
        int buff = rt.data != null ? rt.data.buffAmount : FallbackBuff;
        switch (rt.intent)
        {
            case 0: return "Intent: Attack " + (baseAttack + rt.buffAttack);
            case 1: return "Intent: Defend";
            case 2: return "Intent: Buff -> next +" + buff;
            default: return "Intent: Idle";
        }
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

    // 没出招表的怪：随机意图结算
    void ExecuteRandomIntent(EnemyRuntime rt)
    {
        int baseAttack = rt.data != null ? rt.data.attack : enemyAttack;
        int defend = rt.data != null ? rt.data.defendAmount : FallbackDefend;
        int buff = rt.data != null ? rt.data.buffAmount : FallbackBuff;
        float chance = rt.data != null ? rt.data.statusChance : FallbackStatusChance;

        switch (rt.intent)
        {
            case 0:
                DamagePlayer(baseAttack + rt.buffAttack);
                rt.buffAttack = 0;
                if (statusCard != null && Random.value < chance) _discardPile.Add(statusCard);
                break;
            case 1:
                rt.block += defend;
                break;
            case 2:
                rt.buffAttack = buff;
                break;
        }
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

    // ===== 结束回合：所有存活敌人依次行动 =====

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
            else
            {
                ExecuteRandomIntent(rt);
            }
        }

        _block = 0;  // 玩家护甲回合结束清空

        _currentAP = maxAP;
        int drawCount = cardsPerTurn;
        if (equippedRelic != null && equippedRelic.effect == RelicData.EffectType.ExtraDraw)
            drawCount += equippedRelic.value;
        DrawCards(drawCount);

        foreach (EnemyRuntime e in _enemies) RefreshIntent(e);
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

        // 三选一：优先职业奖励池，凑不够回退 Inspector 固定 rewardCards
        List<CardData> rewardOptions = null;
        if (GameManager.Instance != null)
            rewardOptions = GameManager.Instance.DrawRewardCards(3);
        if ((rewardOptions == null || rewardOptions.Count == 0) && rewardCards != null && rewardCards.Count > 0)
            rewardOptions = rewardCards;

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

        bool useSlots = enemySlotPrefab != null && enemyContainer != null;
        string legacyIntent = null;

        foreach (EnemyRuntime rt in _enemies)
        {
            if (rt.dead) continue;
            string intent = IntentText(rt);
            if (rt.slot != null) rt.slot.Refresh(intent, rt == _selected);
            if (rt == _selected) legacyIntent = intent;
        }

        // 没做多敌人槽：旧单敌人 UI 显示当前选中那只
        if (!useSlots && _selected != null)
        {
            if (enemyHPText != null) enemyHPText.text = "enemy HP:" + _selected.hp;
            if (enemyHealthBar != null) enemyHealthBar.SetHealth(_selected.hp, _selected.maxHP);
            if (enemyIntentText != null) enemyIntentText.text = legacyIntent;
            if (_selected.data != null)
            {
                if (_selected.data.icon != null)
                {
                    if (enemySpriteRenderer != null) enemySpriteRenderer.sprite = _selected.data.icon;
                    if (enemyImage != null) enemyImage.sprite = _selected.data.icon;
                }
                if (enemyNameText != null) enemyNameText.text = _selected.data.enemyName;
            }
        }

        if (GameManager.Instance != null)
            GameManager.Instance.currentHP = playerHP;
    }
}
