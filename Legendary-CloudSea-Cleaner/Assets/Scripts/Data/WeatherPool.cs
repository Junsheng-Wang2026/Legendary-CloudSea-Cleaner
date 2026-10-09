using UnityEngine;
using System.Collections.Generic;

// 天气池子：往里拖 WeatherData，章节开始时随机抽一个天气
// Project 里右键 Create -> CloudSea Cleaner -> Weather Pool 创建
[CreateAssetMenu(fileName = "WeatherPool", menuName = "CloudSea Cleaner/Weather Pool", order = 2)]
public class WeatherPool : ScriptableObject
{
    [Tooltip("这个池子包含的天气")]
    public List<WeatherData> weathers = new List<WeatherData>();

    [Tooltip("勾选后不重复抽取，一轮抽完再重新洗入")]
    public bool noRepeatUntilEmpty = true;

    private List<WeatherData> _remaining = new List<WeatherData>();

    // 从池子里随机取一个天气
    public WeatherData GetRandom()
    {
        if (weathers == null || weathers.Count == 0) return null;

        if (!noRepeatUntilEmpty)
            return weathers[Random.Range(0, weathers.Count)];

        if (_remaining.Count == 0)
            _remaining = new List<WeatherData>(weathers);

        int i = Random.Range(0, _remaining.Count);
        WeatherData picked = _remaining[i];
        _remaining.RemoveAt(i);
        return picked;
    }

    // 需要重置抽取记录时调用（比如进入新一章）
    public void ResetPool()
    {
        _remaining.Clear();
    }
}
