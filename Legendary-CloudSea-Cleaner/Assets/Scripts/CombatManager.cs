using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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

    int _currentAP;
    int _block;

    void Start()
    {
        _currentAP = maxAP;
        _block = 0;
        victoryPanel.SetActive(false);
        defeatPanel.SetActive(false);
        RefreshUI();
    }

    // ===== 统一打牌方法 =====

    public void PlayCard(CardData card)
    {
        if (enemyHP <= 0 || playerHP <= 0) return;
        if (_currentAP < card.cost) return;

        _currentAP -= card.cost;

        if (card.damage > 0)
        {
            enemyHP = Mathf.Max(0, enemyHP - card.damage);
        }

        if (card.block > 0)
        {
            _block += card.block;
        }

        RefreshUI();
        CheckEnemyDead();
    }

    // ===== 结束回合：敌人打你 =====

    public void OnClick_EndTurn()
    {
        if (enemyHP <= 0 || playerHP <= 0) return;

        int damage = enemyAttack;
        if (_block > 0)
        {
            int absorbed = Mathf.Min(_block, damage);
            _block -= absorbed;
            damage -= absorbed;
        }

        playerHP = Mathf.Max(0, playerHP - damage);
        _currentAP = maxAP;
        RefreshUI();
        CheckPlayerDead();
    }

    // ===== 胜负 =====

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
