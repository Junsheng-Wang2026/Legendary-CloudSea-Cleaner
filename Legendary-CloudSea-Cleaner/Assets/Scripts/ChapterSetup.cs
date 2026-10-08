using UnityEngine;
using System.Collections.Generic;

// 一章的配置：天气 + BOSS，各自都支持“固定一个”或“从池子随机”
[System.Serializable]
public class ChapterConfig
{
    public WeatherData weather;        // 拖了 = 本章固定这个天气
    public WeatherPool weatherPool;    // weather 留空 = 从这个天气池随机抽
    public EnemyData fixedBoss;        // 拖了 = 本章固定这个 BOSS
    public EnemyPool bossPool;         // fixedBoss 留空 = 从这个 BOSS 池随机抽
}

// 章节初始化：挂在 Boot 场景一个始终激活的物体上。
// Chapter Configs 列表第 1 项=第1章、第2项=第2章……每章天气/BOSS 各自配置。
// 章节切换后回到 Boot 场景会自动按 GameManager.chapter 读取对应配置。
public class ChapterSetup : MonoBehaviour
{
    [Header("各章配置（Element 0=第1章，Element 1=第2章，依次类推）")]
    public List<ChapterConfig> chapterConfigs = new List<ChapterConfig>();

    [Header("兜底：章节数超过上面列表时用这套")]
    public WeatherData fallbackWeather;
    public WeatherPool fallbackWeatherPool;
    public EnemyData fallbackFixedBoss;
    public EnemyPool fallbackBossPool;

    [Header("左侧章节状态栏（拖场景里的 ChapterStatusBar）")]
    public ChapterStatusBar statusBar;

    void Start()
    {
        int chapter = GameManager.Instance != null ? GameManager.Instance.chapter : 1;
        BeginChapter(chapter);
    }

    // chapter 从 1 开始；章节切换时也可手动调这个方法
    public void BeginChapter(int chapter)
    {
        ChapterConfig cfg = null;
        int idx = chapter - 1;
        if (idx >= 0 && idx < chapterConfigs.Count)
            cfg = chapterConfigs[idx];

        // 天气：固定优先，否则从天气池随机
        WeatherData weather = cfg != null ? cfg.weather : fallbackWeather;
        WeatherPool wPool = cfg != null ? cfg.weatherPool : fallbackWeatherPool;
        if (weather == null && wPool != null)
            weather = wPool.GetRandom();

        // BOSS：固定优先，否则从 BOSS 池随机
        EnemyData boss = cfg != null ? cfg.fixedBoss : fallbackFixedBoss;
        EnemyPool bPool = cfg != null ? cfg.bossPool : fallbackBossPool;
        if (boss == null && bPool != null)
            boss = bPool.GetRandom();

        if (GameManager.Instance != null)
            GameManager.Instance.chapterBoss = boss;

        if (statusBar != null)
            statusBar.SetChapter(weather, boss);
    }
}
