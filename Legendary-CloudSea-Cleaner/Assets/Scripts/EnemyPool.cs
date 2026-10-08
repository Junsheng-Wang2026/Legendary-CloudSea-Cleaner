using UnityEngine;
using System.Collections.Generic;

// 敌人池子：普通池 / 精英池 / BOSS 池各建一个资产，往里拖 EnemyData
// Project 里右键 Create -> CloudSea Cleaner -> Enemy Pool 创建
[CreateAssetMenu(fileName = "EnemyPool", menuName = "CloudSea Cleaner/Enemy Pool", order = 1)]
public class EnemyPool : ScriptableObject
{
    [Tooltip("这个池子包含的敌人")]
    public List<EnemyData> enemies = new List<EnemyData>();

    [Tooltip("勾选后不重复抽取，一轮抽完再重新洗入")]
    public bool noRepeatUntilEmpty = true;

    private List<EnemyData> _remaining = new List<EnemyData>();

    // 从池子里随机取一个敌人
    public EnemyData GetRandom()
    {
        if (enemies == null || enemies.Count == 0) return null;

        if (!noRepeatUntilEmpty)
            return enemies[Random.Range(0, enemies.Count)];

        if (_remaining.Count == 0)
            _remaining = new List<EnemyData>(enemies);

        int i = Random.Range(0, _remaining.Count);
        EnemyData picked = _remaining[i];
        _remaining.RemoveAt(i);
        return picked;
    }

    // 需要重置抽取记录时调用（比如进入新一章）
    public void ResetPool()
    {
        _remaining.Clear();
    }
}
