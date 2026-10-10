using System.Collections.Generic;
using UnityEngine;

// 遭遇池：随机抽“一整场遭遇”。用于高层污渍这种“雨渍/青苔/霉菌随机组合”。
// Project 里右键 Create -> CloudSea Cleaner -> Encounter Pool 创建。
[CreateAssetMenu(fileName = "EncounterPool", menuName = "CloudSea Cleaner/Encounter Pool", order = 3)]
public class EncounterPool : ScriptableObject
{
    [Tooltip("这个池子包含的遭遇（每场是一整个 EncounterData）")]
    public List<EncounterData> encounters = new List<EncounterData>();

    [Tooltip("勾选后不重复抽取，一轮抽完再重新洗入")]
    public bool noRepeatUntilEmpty = true;

    private List<EncounterData> _remaining = new List<EncounterData>();

    // 从池子里随机取一场遭遇
    public EncounterData GetRandom()
    {
        if (encounters == null || encounters.Count == 0) return null;

        if (!noRepeatUntilEmpty)
            return encounters[Random.Range(0, encounters.Count)];

        if (_remaining.Count == 0)
            _remaining = new List<EncounterData>(encounters);

        int i = Random.Range(0, _remaining.Count);
        EncounterData picked = _remaining[i];
        _remaining.RemoveAt(i);
        return picked;
    }

    // 需要重置抽取记录时调用（进入新一章）
    public void ResetPool()
    {
        _remaining.Clear();
    }
}
