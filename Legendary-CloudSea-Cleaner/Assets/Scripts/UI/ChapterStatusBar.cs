using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 左侧章节状态栏：天气图标 + 天气名称 + “当前天气/章节BOSS极端天气名”文字
// 天气由 WeatherData 资产驱动；章节 BOSS 由 EnemyData 资产驱动（章节开始时确定，提前显示）。
public class ChapterStatusBar : MonoBehaviour
{
    [Header("当前天气（拖 WeatherData 资产，也可由 ChapterDirector 设置）")]
    public WeatherData defaultWeather;
    private WeatherData _currentWeather;

    [Header("天气 UI")]
    public Image weatherIcon;          // 天气图标
    public TMP_Text weatherText;       // 天气名称（如 Clear）

    [Header("章节 BOSS")]
    public TMP_Text bossText;          // 显示“天气/BOSS名”，没配 BOSS 时只显示天气名
    public string bossNameFallback = ""; // 没有 BOSS 敌人资产时的兜底文字（默认空，不显示假名字）
    private EnemyData _currentBoss;

    void Start()
    {
        if (defaultWeather != null)
            _currentWeather = defaultWeather;
        Refresh();
    }

    // 切换天气
    public void SetWeather(WeatherData data)
    {
        _currentWeather = data;
        Refresh();
    }

    // 设置本章 BOSS（名字从敌人资产读取）
    public void SetBoss(EnemyData boss)
    {
        _currentBoss = boss;
        Refresh();
    }

    // 一次性设置本章天气 + BOSS
    public void SetChapter(WeatherData weather, EnemyData boss)
    {
        _currentWeather = weather;
        _currentBoss = boss;
        Refresh();
    }

    public void Refresh()
    {
        string weatherName = _currentWeather != null ? _currentWeather.weatherName : "";

        if (weatherIcon != null && _currentWeather != null && _currentWeather.icon != null)
            weatherIcon.sprite = _currentWeather.icon;

        if (weatherText != null)
            weatherText.text = weatherName;

        if (bossText != null)
        {
            string bossName = _currentBoss != null ? _currentBoss.enemyName : bossNameFallback;
            bossText.text = string.IsNullOrEmpty(bossName) ? weatherName : weatherName + "/" + bossName;
        }
    }
}
