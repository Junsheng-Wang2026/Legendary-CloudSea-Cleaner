/*using UnityEngine;

// 时间归零：打本章开始就确定好的 BOSS（无选项、不推进下降距离）。
// 挂在 Boot 场景一个始终激活的物体上。
public class TimeUpTrigger : MonoBehaviour
{
    void Start()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnTimeUp += OnTimeUp;
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.OnTimeUp -= OnTimeUp;
    }

    void OnTimeUp()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[TimeUpTrigger] 时间到，但场景里没有 GameManager");
            return;
        }

        if (GameManager.Instance.chapterBoss != null)
        {
            GameManager.Instance.EnterCombat(GameManager.Instance.chapterBoss);
        }
        else
        {
            // 没配本章 BOSS：不进战斗，直接报错（请在 ChapterData 的 Fixed Boss / Boss Pool 配置）
            Debug.LogError("[TimeUpTrigger] 时间到，但本章没有配置 BOSS（ChapterData 的 Fixed Boss 或 Boss Pool 为空）");
        }
    }
}
*/