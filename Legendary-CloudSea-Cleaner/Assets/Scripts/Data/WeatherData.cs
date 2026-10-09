using UnityEngine;

// 天气数据资产：Project 里右键 Create -> CloudSea Cleaner -> Weather Data 创建
// 每种天气一个资产，配好名称、图标、效果描述；状态栏读它自动刷新图标和文字
[CreateAssetMenu(fileName = "WeatherData", menuName = "CloudSea Cleaner/Weather Data", order = 0)]
public class WeatherData : ScriptableObject
{
    [Header("基础信息")]
    public string weatherName = "Clear";   // 显示名（英文，如 Clear / Cloudy / Rain）
    public Sprite icon;                    // 天气图标

    [Header("天气效果说明（悬停 tooltip 用，可留空）")]
    [TextArea] public string weatherEffect = "";   // 例：每场战斗开始时，将一张“瘙痒”洗入抽牌堆
}
