using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 事件面板：节点到位后由 ChapterDirector 调 ShowEvent。
//   - 选项事件：按 EventData.options 动态生成选项按钮（用预制体，手动代码排布，不用 LayoutGroup）；
//   - 纯战斗事件（无选项、配了遭遇）：到点直接进战斗；
//   - 特殊事件（商店/休息/结算子类）：由 ChapterDirector 直接 Execute，不经过这里。
public class MapNode : MonoBehaviour
{
    [Header("面板")]
    public GameObject eventPanel;            // 事件描述+选项面板根物体（默认隐藏）
    public TMP_Text eventDescriptionText;     // 事件描述文字

    [Header("动态选项按钮")]
    public GameObject optionButtonPrefab;     // 选项按钮预制体（带 Button，子物体带 TMP 文字）
    public RectTransform optionButtonContainer; // 选项按钮父容器
    public float optionSpacing = 70f;         // 选项按钮纵向间距
    public float optionTopOffset = -20f;      // 第一个按钮相对容器顶部的下偏
    public float optionXOffset = 0f;          // 按钮水平偏移

    readonly List<GameObject> _spawnedOptions = new List<GameObject>();

    // 节点到位，章节导演调用
    public void ShowEvent(EventData ev)
    {
        if (ev == null)
        {
            ChapterDirector.Instance?.FinishEvent();
            return;
        }

        eventPanel.SetActive(true);
        if (eventDescriptionText != null)
            eventDescriptionText.text = ev.description;

        ClearSpawnedOptions();

        if (ev.HasNewOptions())
        {
            ShowDynamicOptions(ev);
            return;
        }

        // 无选项：配了遭遇就是纯战斗事件，到点直接开打
        EncounterData enc = ev.ResolveEncounter();
        if (enc != null)
        {
            HideEventPanel();
            if (ChapterDirector.Instance != null)
                ChapterDirector.Instance.EnterEncounter(enc);
            else
                Debug.LogError("[MapNode] 没有 ChapterDirector，无法进入战斗");
        }
        else
        {
            // 既没选项也没遭遇：交互事件忘配内容，直接结束避免卡死
            Debug.LogWarning("[MapNode] 事件「" + ev.eventName + "」没有选项也没有遭遇，直接通过");
            HideEventPanel();
            ChapterDirector.Instance?.FinishEvent();
        }
    }

    // 按 options 动态生成按钮，过滤掉不满足 flag 条件的选项
    void ShowDynamicOptions(EventData ev)
    {
        GameManager gm = GameManager.Instance;

        foreach (EventOption option in ev.options)
        {
            if (option == null) continue;
            if (!option.IsVisible(gm)) continue;

            GameObject obj = Instantiate(optionButtonPrefab, optionButtonContainer);

            TMP_Text label = obj.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = option.buttonText;

            Button btn = obj.GetComponent<Button>();
            if (btn != null)
            {
                EventOption captured = option;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnClickDynamicOption(captured));
            }

            // 手动纵向排列（容器锚点顶部居中；不用 LayoutGroup，不锁子物体 RectTransform）
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                int i = _spawnedOptions.Count;
                rt.anchorMin = new Vector2(0.5f, 1f);
                rt.anchorMax = new Vector2(0.5f, 1f);
                rt.pivot = new Vector2(0.5f, 1f);
                rt.anchoredPosition = new Vector2(optionXOffset, optionTopOffset - i * optionSpacing);
            }

            _spawnedOptions.Add(obj);
        }
    }

    void OnClickDynamicOption(EventOption option)
    {
        HideEventPanel();
        if (ChapterDirector.Instance != null)
            ChapterDirector.Instance.RunOptionEffects(option);
    }

    public void HideEventPanel()
    {
        ClearSpawnedOptions();
        if (eventPanel != null) eventPanel.SetActive(false);
    }

    void ClearSpawnedOptions()
    {
        foreach (GameObject o in _spawnedOptions)
        {
            if (o != null) Destroy(o);
        }
        _spawnedOptions.Clear();
    }
}
