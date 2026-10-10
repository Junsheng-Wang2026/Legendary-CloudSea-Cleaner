using System.Collections.Generic;
using UnityEngine;

// 事件的一个选项（按钮）。一个事件可以配任意多个选项。
[System.Serializable]
public class EventOption
{
    [Tooltip("按钮上显示的文字")]
    public string buttonText;

    [Header("显示条件（C4 用，先留空=始终显示）")]
    [Tooltip("需要检查的标记名，留空=始终显示这个选项")]
    public string requireFlag;
    [Tooltip("true=有该标记才显示；false=没有该标记才显示（用于互斥，选过一次后隐藏）")]
    public bool requireFlagSet = true;

    [Tooltip("点下这个选项后按顺序执行的效果")]
    public List<EventEffect> effects = new List<EventEffect>();

    // 这个选项当前是否应显示（C5 动态生成按钮时调用）。
    // 没填 requireFlag = 始终显示；填了就按“需要有/需要没有该标记”判断。
    public bool IsVisible(GameManager gm)
    {
        if (string.IsNullOrEmpty(requireFlag)) return true;
        bool has = gm != null && gm.HasFlag(requireFlag);
        return has == requireFlagSet;
    }
}
