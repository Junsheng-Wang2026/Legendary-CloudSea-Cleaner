using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// 战斗管理器：挂在 Combat 场景根级别的空物体 CombatManager 上。
/// Day2 极简版：木桩敌人不还手，3 个按钮代替 3 张测试牌。
/// </summary>
public class CombatManager : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text enemyHPText;
    public TMP_Text apText;
    public TMP_Text playerHPText;
    public TMP_Text blockText;
    public GameObject victoryPanel;   // 胜利按钮，默认隐藏

    [Header("战斗数值")]
    public int enemyHP = 100;
    public int maxAP = 3;
    public int playerHP = 100;

    int _currentAP;
    int _block;

    void Start()
    {
        _currentAP = maxAP;
        _block = 0;
        victoryPanel.SetActive(false);
        RefreshUI();
    }

    // ===== 三个卡牌按钮 =====

    // 猛刷：2 费打 14
    public void OnClick_MengShua()
    {
        if (_currentAP < 2 || enemyHP <= 0) return;
        _currentAP -= 2;
        enemyHP = Mathf.Max(0, enemyHP - 14);
        RefreshUI();
        CheckEnemyDead();
    }

    // 投洗抹布：0 费加 3 格挡
    public void OnClick_TouXiMaBu()
    {
        if (enemyHP <= 0) return;
        _block += 3;
        RefreshUI();
    }

    // 坚挺腰板：1 费加 8 格挡
    public void OnClick_JianTingYaoBan()
    {
        if (_currentAP < 1 || enemyHP <= 0) return;
        _currentAP -= 1;
        _block += 8;
        RefreshUI();
    }

    // 结束回合：回行动点、清格挡
    public void OnClick_EndTurn()
    {
        if (enemyHP <= 0) return;
        _block = 0;
        _currentAP = maxAP;
        RefreshUI();
    }

    // 胜利后回 Boot
    public void OnClick_ReturnToBoot()
    {
        SceneManager.LoadScene("Boot");
    }

    // ===== 内部 =====

    void CheckEnemyDead()
    {
        if (enemyHP <= 0)
        {
            victoryPanel.SetActive(true);
            GameEvents.RaiseCombatVictory();
        }
    }

    void RefreshUI()
    {
        enemyHPText.text = "敌人  " + enemyHP;
        apText.text = "行动点  " + _currentAP;
        playerHPText.text = "体力  " + playerHP;
        blockText.text = "格挡  " + _block;
    }
}
