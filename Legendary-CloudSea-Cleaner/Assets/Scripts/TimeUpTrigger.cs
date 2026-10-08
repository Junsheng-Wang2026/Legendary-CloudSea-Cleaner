using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeUpTrigger : MonoBehaviour
{
    void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeUp += OnTimeUp;
        }
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeUp -= OnTimeUp;
        }
    }

    void OnTimeUp()
    {
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene("Combat");
            return;
        }

        // 打章节开始就确定好的本章 BOSS；无选项、不推进下降距离
        if (GameManager.Instance.chapterBoss != null)
        {
            GameManager.Instance.EnterCombat(GameManager.Instance.chapterBoss, false);
        }
        else
        {
            // 兜底：没配本章 BOSS 资产时，退回旧的标记方式（CombatManager 用默认 BOSS 数值）
            GameManager.Instance.isBossFight = true;
            GameManager.Instance.pendingDistanceAdvance = false;
            SceneManager.LoadScene("Combat");
        }
    }
}
