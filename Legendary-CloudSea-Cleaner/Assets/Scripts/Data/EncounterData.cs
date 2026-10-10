using System.Collections.Generic;
using UnityEngine;

// 遭遇（一场战斗上哪几只怪）= “怪物投放”表里的一行。
// 一个遭遇里放多个成员，每个成员指定敌人资产 + 从出招表第几招开始（起始相位）。
// 例：飞鸟A 从啄击开始(startPhase 0)，飞鸟B 从盘旋蓄力开始(startPhase 1)。
// Project 里右键 Create -> CloudSea Cleaner -> Encounter Data 创建。
[CreateAssetMenu(fileName = "EncounterData", menuName = "CloudSea Cleaner/Encounter Data", order = 2)]
public class EncounterData : ScriptableObject
{
    [System.Serializable]
    public class Member
    {
        [Tooltip("这只怪用哪个敌人资产")]
        public EnemyData enemy;

        [Tooltip("从它出招表第几招开始，0=第一招。同一种怪起手不同就改这里（飞鸟A=0啄击、飞鸟B=1盘旋）")]
        public int startPhase = 0;
    }

    [Tooltip("这一场一起上场的怪（1~3只）")]
    public List<Member> members = new List<Member>();

    [Tooltip("打完总耗时（分钟）。填0=自动取成员里最大的 Time Cost（精英45/普通30）")]
    public int timeCostOverride = 0;

    // 取本场耗时
    public int GetTimeCost()
    {
        if (timeCostOverride > 0) return timeCostOverride;
        int max = 0;
        foreach (Member m in members)
        {
            if (m != null && m.enemy != null && m.enemy.timeCost > max)
                max = m.enemy.timeCost;
        }
        return max;
    }
}
