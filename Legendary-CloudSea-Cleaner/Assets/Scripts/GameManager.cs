using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("全局玩家状态")]
    public int maxHP = 100;
    public int currentHP = 100;
    public int gold = 0;          // 金币
    public int chapter = 1;       // 当前章节（第几章）

    [Header("战斗标记")]
    public bool isBossFight = false;

    [Header("全局牌组")]
    public List<CardData> playerDeck = new List<CardData>();

    [Header("高度进度")]
    public int currentDepth = 0;
    public int checkpointDepth = 100;  // 到检查点需要的高度
    public int currentEventIndex = 0;  // 当前是本章第几起事件（用于取每段距离）
    public bool pendingDistanceAdvance = false;  // 战斗胜利返回后待推进一次距离

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
