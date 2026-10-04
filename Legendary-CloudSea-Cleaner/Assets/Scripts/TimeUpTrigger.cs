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
        // 标记为 BOSS 战，CombatManager 会读这个值
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isBossFight = true;
        }
        SceneManager.LoadScene("Combat");
    }
}
