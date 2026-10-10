/*using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 测试用：往当前打开的 Combat 场景里的 CombatManager 加卡
// 直接改的是 Unity 里打开着的场景，改完按 Ctrl+S 保存即可
public static class TestDeckTools
{
    // 现在的战斗代码就能完整结算的卡（不含衍生卡、状态卡和基础防护）
    static readonly string[] PlayableIds =
    {
        "STR_L01A", "STR_L03B", "STR_L04B", "STR_L05B", "STR_L07B", "STR_L10B", "STR_L11B",
        "AGI_L01B", "AGI_L03A", "AGI_L08B",
        "INT_L08B",
    };

    [MenuItem("Cleaner/测试牌组/奖励池加入可正常打的卡")]
    static void AddPlayableToReward() { Add(FindByIds(PlayableIds), false); }

    [MenuItem("Cleaner/测试牌组/把选中的卡加入奖励池")]
    static void AddSelectedToReward() { Add(Selected(), false); }

    [MenuItem("Cleaner/测试牌组/把选中的卡加入初始牌组")]
    static void AddSelectedToDeck() { Add(Selected(), true); }

    static void Add(List<CardData> cards, bool toStartingDeck)
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("先停止运行", "请先退出 Play 模式，否则改动会在停止运行时丢失。", "好");
            return;
        }
        CombatManager cm = Object.FindObjectOfType<CombatManager>(true);
        if (cm == null)
        {
            EditorUtility.DisplayDialog("找不到 CombatManager", "请先打开 Combat 场景再执行。", "好");
            return;
        }
        if (cards.Count == 0)
        {
            EditorUtility.DisplayDialog("没有卡牌", "没找到要加的卡。如果是\"选中的卡\"，请先在 Project 窗口选中卡牌资产。", "好");
            return;
        }

        Undo.RecordObject(cm, "加入测试卡牌");
        List<CardData> list = toStartingDeck ? cm.startingDeck : cm.rewardCards;
        int added = 0;
        foreach (CardData c in cards)
        {
            if (!toStartingDeck && list.Contains(c)) continue;  // 奖励池不放重复的，初始牌组允许重复
            list.Add(c);
            added++;
        }
        EditorUtility.SetDirty(cm);
        EditorSceneManager.MarkSceneDirty(cm.gameObject.scene);

        string target = toStartingDeck ? "初始牌组" : "奖励池";
        EditorUtility.DisplayDialog("加入完成", "已加入 " + added + " 张到" + target + "（现在共 " + list.Count + " 张）。\n记得按 Ctrl+S 保存场景。", "好");
    }

    static List<CardData> Selected()
    {
        return Selection.GetFiltered<CardData>(SelectionMode.Assets).ToList();
    }

    // 按资产名前缀找卡，例如 "STR_L01A" 找到 STR_L01A_抹除
    static List<CardData> FindByIds(string[] ids)
    {
        List<CardData> all = AssetDatabase.FindAssets("t:CardData")
            .Select(g => AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(c => c != null)
            .ToList();
        var result = new List<CardData>();
        foreach (string id in ids)
        {
            CardData c = all.FirstOrDefault(x => x.name.StartsWith(id + "_"));
            if (c != null) result.Add(c);
        }
        return result;
    }
}
*/