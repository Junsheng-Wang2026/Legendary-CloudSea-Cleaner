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
    public HealthBar enemyHealthBar;    // 敌人血条
    public CardRewardPanel rewardPanel; // 三选一奖励面板

    [Header("敌人外观（按敌人资产自动更换）")]
    public SpriteRenderer enemySpriteRenderer;  // 世界空间的敌人立绘
    public Image enemyImage;                    // 如果敌人是 UI Image 就拖这个
    public TMP_Text enemyNameText;              // 可选：敌人名字
    public EnemyData enemyData;                 // 可选：单场景测试用敌人（没走事件入口时用）

    [Header("战斗数值")]
    public int enemyHP = 35;
    public int enemyAttack = 7;
    public int maxAP = 3;
    public int playerHP = 100;
    public int maxPlayerHP = 100;
    public int cardsPerTurn = 5;

    [Header("牌组")]
    public List<CardData> startingDeck = new List<CardData>();  // 拖初始牌组

    [Header("奖励牌池")]
    public List<CardData> rewardCards = new List<CardData>();  // 拖几张可奖励的牌

    [Header("手牌布局")]
    public HandLayout handLayout;  // 拖手牌父物体

    [Header("遗物")]
    public RelicData equippedRelic;  // 拖一个测试遗物

    [Header("状态牌")]
    public CardData statusCard;  // 敌人给的debuff牌

    public enum EnemyIntent { Attack, Defend, Buff, Idle }
    EnemyIntent _currentIntent;
    int _enemyBlock;
    int _buffAttack = 0;  // 本回合buff加的攻击，打完清零

    // 当前敌人的运行时数值（从 EnemyData 来，没资产时用下面默认值）
    int _enemyMaxHP = 35;
    int _defendAmount = 5;    // 防御意图加甲
    int _buffAmount = 7;      // 蓄力意图下次加攻
    float _statusChance = 0.3f; // 攻击塞状态牌概率
    int _timeCost = 30;       // 击杀耗时（分钟）

    List<CardData> _drawPile = new List<CardData>();
    List<CardData> _hand = new List<CardData>();
    List<CardData> _discardPile = new List<CardData>();

    int _currentAP;
    int _block;

    void Start()
    {
        _currentAP = maxAP;
        _block = 0;
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);

        // 从全局读血量
        if (GameManager.Instance != null)
        {
            playerHP = GameManager.Instance.currentHP;
            maxPlayerHP = GameManager.Instance.maxHP;
        }

        // 确定本场敌人：优先全局传入（事件/敌人池/超时BOSS），否则用 Inspector 上的测试敌人
        EnemyData data = null;
        if (GameManager.Instance != null && GameManager.Instance.pendingEnemy != null)
        {
            data = GameManager.Instance.pendingEnemy;
            GameManager.Instance.pendingEnemy = null;  // 消费掉，避免残留
        }
        else if (enemyData != null)
        {
            data = enemyData;
        }

        if (data != null)
        {
            ApplyEnemy(data);
        }
        else if (GameManager.Instance != null && GameManager.Instance.isBossFight)
        {
            // 兜底：没配敌人资产时的默认 BOSS 数值
            enemyHP = 300;
            enemyAttack = 15;
            _enemyMaxHP = 300;
            GameManager.Instance.isBossFight = false;
        }
        else
        {
            _enemyMaxHP = enemyHP;
        }

        // 初始化牌组：从全局牌组读
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.playerDeck.Count == 0)
            {
                // 第一次进战斗，把 startingDeck 复制到全局牌组
                GameManager.Instance.playerDeck = new List<CardData>(startingDeck);
            }
            _drawPile = new List<CardData>(GameManager.Instance.playerDeck);
        }
        else
        {
            _drawPile = new List<CardData>(startingDeck);
        }
        Shuffle(_drawPile);

        // 抽第一回合的牌
        int startDraw = cardsPerTurn;
        if (equippedRelic != null && equippedRelic.effect == RelicData.EffectType.ExtraDraw)
        {
            startDraw += equippedRelic.value;
        }
        DrawCards(startDraw);
        PickNewIntent();
        RefreshUI();
    }

    // 按敌人资产刷新立绘、数值、血条上限与耗时
    void ApplyEnemy(EnemyData d)
    {
        _enemyMaxHP = d.maxHP;
        enemyHP = d.maxHP;
        enemyAttack = d.attack;
        _defendAmount = d.defendAmount;
        _buffAmount = d.buffAmount;
        _statusChance = d.statusChance;
        _timeCost = d.timeCost;

        if (d.icon != null)
        {
            if (enemySpriteRenderer != null) enemySpriteRenderer.sprite = d.icon;
            if (enemyImage != null) enemyImage.sprite = d.icon;
        }
        if (enemyNameText != null) enemyNameText.text = d.enemyName;
    }

    // ===== 抽牌 =====

    void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // 抽牌堆空了，把弃牌堆洗回去
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

    // ===== 打牌 =====

    bool _lostHPThisTurn = false;  // 本回合是否损失过生命值

    public void PlayCard(CardData card)
    {
        if (enemyHP <= 0 || playerHP <= 0) return;
        if (card.cardType == CardType.Status) return;  // 状态牌不可打出
        if (_currentAP < card.cost) return;
        if (!_hand.Contains(card)) return;

        _currentAP -= card.cost;
        _hand.Remove(card);

        // 消耗牌不进弃牌堆
        if (!card.exhaust)
        {
            _discardPile.Add(card);
        }

        // 自损
        if (card.selfDamage > 0)
        {
            playerHP = Mathf.Max(0, playerHP - card.selfDamage);
            _lostHPThisTurn = true;
        }

        // 伤害（多次攻击）
        if (card.damage > 0)
        {
            for (int i = 0; i < card.attackTimes; i++)
            {
                int dmg = card.damage;
                if (!card.ignoreBlock && _enemyBlock > 0)
                {
                    int absorbed = Mathf.Min(_enemyBlock, dmg);
                    _enemyBlock -= absorbed;
                    dmg -= absorbed;
                }
                enemyHP = Mathf.Max(0, enemyHP - dmg);
            }
        }

        // 条件伤害（本回合损失过生命值）
        if (_lostHPThisTurn && card.conditionalDamage > 0)
        {
            int dmg = card.conditionalDamage;
            if (!card.ignoreBlock && _enemyBlock > 0)
            {
                int absorbed = Mathf.Min(_enemyBlock, dmg);
                _enemyBlock -= absorbed;
                dmg -= absorbed;
            }
            enemyHP = Mathf.Max(0, enemyHP - dmg);
        }

        // 格挡
        if (card.block > 0)
        {
            _block += card.block;
        }
        if (_lostHPThisTurn && card.conditionalBlock > 0)
        {
            _block += card.conditionalBlock;
        }

        // 抽牌
        if (card.drawCards > 0)
        {
            DrawCards(card.drawCards);
        }
        if (_lostHPThisTurn && card.conditionalDraw > 0)
        {
            DrawCards(card.conditionalDraw);
        }

        // 回能
        if (card.gainEnergy > 0)
        {
            _currentAP += card.gainEnergy;
        }

        UpdateHandUI();
        RefreshUI();
        CheckEnemyDead();
    }

    // ===== 结束回合 =====

    public void OnClick_EndTurn()
    {
        if (enemyHP <= 0 || playerHP <= 0) return;

        // 手牌全弃
        _discardPile.AddRange(_hand);
        _hand.Clear();

        _lostHPThisTurn = false;  // 重置本回合损失血量标记

        // 敌人按意图行动
        if (_currentIntent == EnemyIntent.Attack)
        {
            int damage = enemyAttack + _buffAttack;
            _buffAttack = 0;  // 打完清零
            if (_block > 0)
            {
                int absorbed = Mathf.Min(_block, damage);
                _block -= absorbed;
                damage -= absorbed;
            }
            playerHP = Mathf.Max(0, playerHP - damage);
            // 按敌人配置的概率给玩家塞一张状态牌
            if (statusCard != null && Random.value < _statusChance)
            {
                _discardPile.Add(statusCard);
            }
        }
        else if (_currentIntent == EnemyIntent.Defend)
        {
            _enemyBlock += _defendAmount;
        }
        else if (_currentIntent == EnemyIntent.Buff)
        {
            _buffAttack = _buffAmount;  // 下回合攻击强化
        }
        // Idle: 什么都不做

        // 回合结束清空玩家护甲
        _block = 0;

        // 回 AP + 抽新牌
        _currentAP = maxAP;
        int drawCount = cardsPerTurn;
        if (equippedRelic != null && equippedRelic.effect == RelicData.EffectType.ExtraDraw)
        {
            drawCount += equippedRelic.value;
        }
        DrawCards(drawCount);

        PickNewIntent();
        RefreshUI();
        CheckPlayerDead();
    }

    void PickNewIntent()
    {
        // 如果刚buff过，下回合一定攻击
        if (_buffAttack > 0)
        {
            _currentIntent = EnemyIntent.Attack;
            return;
        }
        // 随机选一个意图
        int r = Random.Range(0, 4);
        _currentIntent = (EnemyIntent)r;
    }

    public void OnClick_ReturnToBoot()
    {
        SceneManager.LoadScene("Boot");
    }

    void CheckEnemyDead()
    {
        if (enemyHP <= 0)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.SpendTime(_timeCost);
            }

            // 三选一奖励：优先从 主职+副职 奖励牌池按权重抽 3 张互不相同；
            // 没配职业、或职业池抽不到时，回退到 Inspector 上固定的 rewardCards 测试牌
            List<CardData> rewardOptions = null;
            if (GameManager.Instance != null)
            {
                rewardOptions = GameManager.Instance.DrawRewardCards(3);
            }
            if ((rewardOptions == null || rewardOptions.Count == 0) && rewardCards != null && rewardCards.Count > 0)
            {
                rewardOptions = rewardCards;
            }

            if (rewardPanel != null && rewardOptions != null && rewardOptions.Count > 0)
            {
                rewardPanel.ShowRewards(rewardOptions);
            }
            else
            {
                victoryPanel.SetActive(true);
            }
        }
    }

    // 奖励选完/跳过后显示胜利面板
    public void ShowVictoryAfterReward()
    {
        victoryPanel.SetActive(true);
    }

    void CheckPlayerDead()
    {
        if (playerHP <= 0)
        {
            defeatPanel.SetActive(true);
        }
    }

    void RefreshUI()
    {
        enemyHPText.text = "enemy HP:" + enemyHP;
        if (enemyHealthBar != null) enemyHealthBar.SetHealth(enemyHP, _enemyMaxHP);
        if (playerHealthBar != null) playerHealthBar.SetHealth(playerHP, maxPlayerHP);
        // 显示意图
        switch (_currentIntent)
        {
            case EnemyIntent.Attack:
                enemyIntentText.text = "Intent: Attack " + (enemyAttack + _buffAttack);
                break;
            case EnemyIntent.Defend:
                enemyIntentText.text = "Intent: Defend " + _enemyBlock;
                break;
            case EnemyIntent.Buff:
                enemyIntentText.text = "Intent: Buff -> next +" + _buffAmount;
                break;
            case EnemyIntent.Idle:
                enemyIntentText.text = "Intent: Idle";
                break;
        }
        apText.text = "AP:" + _currentAP;
        playerHPText.text = "PlayerHP:" + playerHP + "/" + maxPlayerHP;
        blockText.text = "Block:" + _block;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentHP = playerHP;
        }
    }
}
