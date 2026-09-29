using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text enemyHPText;
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

    [Header("手牌按钮")]
    public CardButton[] handButtons;  // 拖 Card1, Card2, Card3

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

        // 初始化牌组
        _drawPile = new List<CardData>(startingDeck);
        Shuffle(_drawPile);

        // 抽第一回合的牌
        DrawCards(cardsPerTurn);
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
            enemyHP = Mathf.Max(0, enemyHP - card.damage);
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

        // 敌人打你
        int damage = enemyAttack;
        if (_block > 0)
        {
            int absorbed = Mathf.Min(_block, damage);
            _block -= absorbed;
            damage -= absorbed;
        }
        playerHP = Mathf.Max(0, playerHP - damage);

        // 回 AP + 抽新牌
        _currentAP = maxAP;
        DrawCards(cardsPerTurn);
        RefreshUI();
        CheckPlayerDead();
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
        apText.text = "AP:" + _currentAP;
        playerHPText.text = "PlayerHP:" + playerHP + "/" + maxPlayerHP;
        blockText.text = "Block:" + _block;
    }
}
