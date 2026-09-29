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
    public int enemyAttack = 8;     // 敌人每回合打你多少
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

    // ===== 三张牌 =====

    public void OnClick_MengShua()
    {
        if (_currentAP < 2 || enemyHP <= 0 || playerHP <= 0) return;
        _currentAP -= 2;
        enemyHP = Mathf.Max(0, enemyHP - 14);
        RefreshUI();
        CheckEnemyDead();
    }

    public void OnClick_TouXiMaBu()
    {
        if (enemyHP <= 0 || playerHP <= 0) return;
        _block += 3;
        RefreshUI();
    }

    public void OnClick_JianTingYaoBan()
    {
        if (_currentAP < 1 || enemyHP <= 0 || playerHP <= 0) return;
        _currentAP -= 1;
        _block += 8;
        RefreshUI();
    }

    // ===== 结束回合：敌人打你 =====

    public void OnClick_EndTurn()
    {
        if (enemyHP <= 0 || playerHP <= 0) return;

        // 敌人攻击，先扣格挡
        int damage = enemyAttack;
        if (_block > 0)
        {
            int absorbed = Mathf.Min(_block, damage);
            _block -= absorbed;
            damage -= absorbed;
        }

        // 剩下的伤害扣血
        playerHP = Mathf.Max(0, playerHP - damage);

        // 回行动点
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
            // 战斗结束扣 30 分钟
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
