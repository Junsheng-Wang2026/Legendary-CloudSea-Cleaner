using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// 一键把场景里写死的英文界面文字、以及测试数据里的英文名字换成中文
// 用法：菜单栏 Cleaner -> 界面文字换成中文
// 只替换下面字典里"完全一致"的文字，其他文字一律不动；可以反复点，已经是中文的不会再改
public static class UiTextLocalizer
{
    static readonly string[] ScenePaths =
    {
        "Assets/Scenes/Menu.unity",
        "Assets/Scenes/Boot.unity",
        "Assets/Scenes/Combat.unity",
    };

    // 场景里按钮、标题等写死的文字
    static readonly Dictionary<string, string> SceneText = new Dictionary<string, string>
    {
        // Menu
        { "Start Game", "开始游戏" },
        // Boot
        { "Fight", "战斗" },
        { "Run", "逃跑" },
        { "Checkpoint! HP restored.", "到达检查点！生命已恢复。" },
        { "Checkpoint", "检查点" },
        { "Back to Menu", "返回主菜单" },
        { "You Win!", "胜利！" },
        { "pass 20 minutes", "消磨20分钟" },
        { "Next Event", "下一个事件" },
        { "Node1", "节点1" },
        // Combat
        { "Choose a Card.", "选择一张卡牌" },
        { "Victory return", "胜利，返回" },
        { "Default return", "失败，返回主菜单" },
        { "Skip", "跳过" },
        { "End Return", "结束回合" },
        { "Block:0", "格挡：0" },
        { "AP:3", "行动点：3" },
    };

    // 敌人、天气、事件、遗物这些测试数据里会显示到界面上的名字
    static readonly Dictionary<string, string> DataText = new Dictionary<string, string>
    {
        { "Bird", "麻雀" },
        { "Scorch Sun", "烈日" },
        { "Clear", "晴" },
        { "Meeting f*cking bird.How to do?", "遇到了一只麻雀，怎么办？" },
        { "Fight", "战斗" },
        { "Run", "逃跑" },
        { "Sponge", "海绵" },
        { "every return 1 more card", "每回合多抽1张牌" },
    };

    [MenuItem("Cleaner/界面文字换成中文")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("先停止运行", "请先退出 Play 模式再执行。", "好");
            return;
        }
        // 当前场景有没保存的改动时，先问要不要保存（选取消就整个中止）
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string originalScene = SceneManager.GetActiveScene().path;
        var log = new List<string>();

        foreach (string path in ScenePaths)
        {
            if (!File.Exists(path)) { log.Add("找不到场景：" + path); continue; }
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int n = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    string key = t.text == null ? "" : t.text.Trim();
                    string zh;
                    if (SceneText.TryGetValue(key, out zh))
                    {
                        Undo.RecordObject(t, "界面文字换成中文");
                        t.text = zh;
                        EditorUtility.SetDirty(t);
                        log.Add(Path.GetFileNameWithoutExtension(path) + " / " + t.gameObject.name + "：" + key + " → " + zh);
                        n++;
                    }
                }
                foreach (ChapterStatusBar bar in root.GetComponentsInChildren<ChapterStatusBar>(true))
                {
                    if (Translate(ref bar.bossNameFallback))
                    {
                        EditorUtility.SetDirty(bar);
                        log.Add(Path.GetFileNameWithoutExtension(path) + " / BOSS 兜底名字 → " + bar.bossNameFallback);
                        n++;
                    }
                }
            }
            if (n > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        int dataCount = 0;
        foreach (EnemyData a in LoadAll<EnemyData>())
            if (Translate(ref a.enemyName)) { Mark(a, log); dataCount++; }
        foreach (WeatherData a in LoadAll<WeatherData>())
            if (Translate(ref a.weatherName)) { Mark(a, log); dataCount++; }
        foreach (EventData a in LoadAll<EventData>())
        {
            bool c = Translate(ref a.description);
          //  c |= Translate(ref a.option1Text);
          //  c |= Translate(ref a.option2Text);
            if (c) { Mark(a, log); dataCount++; }
        }
        foreach (RelicData a in LoadAll<RelicData>())
        {
            bool c = Translate(ref a.relicName);
            c |= Translate(ref a.description);
            if (c) { Mark(a, log); dataCount++; }
        }
        AssetDatabase.SaveAssets();

        if (!string.IsNullOrEmpty(originalScene) && File.Exists(originalScene))
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);

        foreach (string l in log) Debug.Log("[界面汉化] " + l);
        EditorUtility.DisplayDialog("界面文字换成中文", log.Count == 0 ? "没有需要替换的文字（可能已经换过了）。" : "共修改 " + log.Count + " 处，详见 Console。", "好");
    }

    static bool Translate(ref string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        string zh;
        if (!DataText.TryGetValue(s.Trim(), out zh)) return false;
        s = zh;
        return true;
    }

    static void Mark(Object a, List<string> log)
    {
        EditorUtility.SetDirty(a);
        log.Add("数据 / " + a.name);
    }

    static IEnumerable<T> LoadAll<T>() where T : Object
    {
        foreach (string guid in AssetDatabase.FindAssets("t:" + typeof(T).Name))
        {
            T a = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            if (a != null) yield return a;
        }
    }
}
