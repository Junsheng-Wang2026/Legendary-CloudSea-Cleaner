using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("战斗数值")]
    public int enemyHP = 100;
    public int enemyAttack = 8;
    public int maxAP = 3;
    public int playerHP = 100;
    public int maxPlayerHP = 100;
    public int cardsPerTurn = 3;

    [Header("牌组")]
    public List<CardData> startingDeck = new List<CardData>();  // 拖初始牌组

    [Header("奖励牌池")]
    public List<CardData> rewardCards = new List<CardData>();  // 拖几张可奖励的牌

    [Header("手牌按钮")]
    public CardButton[] handButtons;  // 拖 Card1, Card2, Card3

    [Header("遗物")]
    public RelicData equippedRelic;  // 拖一个测试遗物

    public enum EnemyIntent { Attack, Defend, Buff, Idle }
    EnemyIntent _currentIntent;
    int _enemyBlock;
    int _buffAttack = 0;  // 本回合buff加的攻击，打完清零

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

        // BOSS 战：敌人 300 血，攻击 15
        if (GameManager.Instance != null && GameManager.Instance.isBossFight)
        {
            enemyHP = 300;
            enemyAttack = 15;
            GameManager.Instance.isBossFight = false;  // 打完重置标记
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
        for (int i = 0; i < handButtons.Length; i++)
        {
            if (i < _hand.Count)
            {
                handButtons[i].gameObject.SetActive(true);
                handButtons[i].SetCard(_hand[i], this);
            }
            else
            {
                handButtons[i].gameObject.SetActive(false);
            }
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

    public void PlayCard(CardData card)
    {
        if (enemyHP <= 0 || playerHP <= 0) return;
        if (_currentAP < card.cost) return;
        if (!_hand.Contains(card)) return;

        _currentAP -= card.cost;
        _hand.Remove(card);
        _discardPile.Add(card);

        if (card.damage > 0)
        {
            int dmg = card.damage;
            if (_enemyBlock > 0)
            {
                int absorbed = Mathf.Min(_enemyBlock, dmg);
                _enemyBlock -= absorbed;
                dmg -= absorbed;
            }
            enemyHP = Mathf.Max(0, enemyHP - dmg);
        }
        if (card.block > 0)
        {
            _block += card.block;
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
        }
        else if (_currentIntent == EnemyIntent.Defend)
        {
            _enemyBlock += 5;
        }
        else if (_currentIntent == EnemyIntent.Buff)
        {
            _buffAttack = 7;  // 下回合攻击+7
        }
        // Idle: 什么都不做

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
            victoryPanel.SetActive(true);
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.SpendTime(30f);
            }

            // 给一张奖励牌加到全局牌组
            if (rewardCards.Count > 0 && GameManager.Instance != null)
            {
                CardData reward = rewardCards[Random.Range(0, rewardCards.Count)];
                GameManager.Instance.playerDeck.Add(reward);
                Debug.Log("获得奖励牌: " + reward.cardName);
            }
        }
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
                enemyIntentText.text = "Intent: Buff -> next +7";
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
