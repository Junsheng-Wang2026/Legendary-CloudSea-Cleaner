using UnityEngine;

/// <summary>
/// 全局入口。挂在 Boot 场景里那个叫 GameManager 的空物体上。
/// 现在只负责保证自己 DontDestroyOnLoad，以后要加音频管理器、存档管理器都往这儿挂。
/// 注意：TimeManager 自己也是单例，挂在同一个物体上就行，不要重复建。
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
