using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToCombat : MonoBehaviour
{
    [Header("可选：这个入口固定打某个敌人")]
    public EnemyData overrideEnemy;

    [Header("可选：这个入口从某个敌人池随机抽（优先用固定敌人）")]
    public EnemyPool overridePool;

    [Header("这场战斗是否推进下降距离")]
    public bool advanceDistance = true;

    public void LoadCombat()
    {
        EnemyData chosen = overrideEnemy;
        if (chosen == null && overridePool != null)
            chosen = overridePool.GetRandom();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnterCombat(chosen, advanceDistance);
        }
        else
        {
            SceneManager.LoadScene("Combat");
        }
    }

    // 由别的脚本指定敌人后调用
    public void LoadCombatWithEnemy(EnemyData enemy)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.EnterCombat(enemy, advanceDistance);
        else
            SceneManager.LoadScene("Combat");
    }
}
