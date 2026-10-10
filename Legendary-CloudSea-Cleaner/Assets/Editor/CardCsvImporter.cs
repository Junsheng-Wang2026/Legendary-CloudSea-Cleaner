using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

// 卡牌表批量导入工具
// 用法：菜单栏 Cleaner -> 导入卡牌表 (CSV)
// 规则：
//   1. 第一行是表头，表头名 = CardData 里的字段名（cardName、damage、block...），新增字段后加一列同名表头即可
//   2. 以 # 开头的行是注释行（第二行中文说明），会被跳过
//   3. id 列是唯一键：资产命名为 "id_卡名"，再次导入时按 id 找到旧资产并覆盖数值，不会重复创建，也不会删除任何资产
//   4. cardImage 列填 Sprite 名字，留空则保留资产上已经手动拖好的图
//   5. 资产按 cardType 放进 Assets/Data/Cards/<cardType>/ 子文件夹
public static class CardCsvImporter
{
    const string DefaultCsvPath = "Assets/Data/Cards/CardTable.csv";
    const string OutputRoot = "Assets/Data/Cards";

    // 只做参考、不写入资产的列
    static readonly HashSet<string> ReferenceColumns = new HashSet<string> { "id", "目标", "原表", "待实现", "备注", "批注" };

    // 允许表里直接写中文
    static readonly Dictionary<string, string> EnumAlias = new Dictionary<string, string>
    {
        { "力量", "Strength" }, { "敏捷", "Agility" }, { "智力", "Wisdom" }, { "状态", "Status" },
        { "白", "Common" }, { "蓝", "Uncommon" }, { "紫", "Rare" }, { "金", "Gold" },
        { "无", "None" }, { "呆滞", "Dazed" }, { "瘙痒", "Itch" }, { "晕眩", "Stun" },
        { "心情舒畅", "GoodMood" }, { "干劲十足", "Energized" },
    };

    [MenuItem("Cleaner/导入卡牌表 (CSV)")]
    public static void ImportDefault()
    {
        if (!File.Exists(DefaultCsvPath))
        {
            ImportPick();
            return;
        }
        Import(DefaultCsvPath);
    }

    [MenuItem("Cleaner/选择 CSV 导入卡牌...")]
    public static void ImportPick()
    {
        string path = EditorUtility.OpenFilePanel("选择卡牌表 CSV", Application.dataPath, "csv");
        if (!string.IsNullOrEmpty(path)) Import(path);
    }

    public static void Import(string csvPath)
    {
        string text = ReadCsvText(csvPath);
        if (text == null)
        {
            EditorUtility.DisplayDialog("导入失败", "读不出这个 CSV 的编码。\n请在 Excel/WPS 里另存为「CSV UTF-8」后再导入。", "好");
            return;
        }

        List<List<string>> rows = ParseCsv(text);
        if (rows.Count < 2)
        {
            EditorUtility.DisplayDialog("导入失败", "CSV 里没有数据行。", "好");
            return;
        }

        // ---- 表头 -> 字段 ----
        List<string> header = rows[0];
        int idCol = -1;
        var columnFields = new FieldInfo[header.Count];
        var unknownColumns = new List<string>();
        for (int c = 0; c < header.Count; c++)
        {
            string h = header[c].Trim();
            header[c] = h;
            if (h == "id") { idCol = c; continue; }
            if (h.Length == 0 || ReferenceColumns.Contains(h)) continue;
            FieldInfo f = typeof(CardData).GetField(h, BindingFlags.Public | BindingFlags.Instance);
            if (f == null) unknownColumns.Add(h);
            else columnFields[c] = f;
        }
        if (idCol < 0)
        {
            EditorUtility.DisplayDialog("导入失败", "表头里找不到 id 列。", "好");
            return;
        }
        if (unknownColumns.Count > 0)
            Debug.LogWarning("[卡牌导入] 这些列在 CardData 里没有同名字段，已忽略：" + string.Join("，", unknownColumns));

        List<CardData> existing = FindExistingCards();

        int created = 0, updated = 0, skipped = 0;
        var seenIds = new HashSet<string>();
        var problems = new List<string>();

        try
        {
            for (int r = 1; r < rows.Count; r++)
            {
                List<string> row = rows[r];
                string id = Cell(row, idCol);
                if (id.Length == 0 || id.StartsWith("#")) continue;

                if (!seenIds.Add(id))
                {
                    problems.Add($"第{r + 1}行 id 重复：{id}，已跳过");
                    skipped++;
                    continue;
                }

                CardData card = FindById(existing, id);
                bool isNew = card == null;
                if (isNew) card = ScriptableObject.CreateInstance<CardData>();

                for (int c = 0; c < header.Count; c++)
                {
                    FieldInfo f = columnFields[c];
                    if (f == null) continue;
                    string raw = Cell(row, c);
                    string err = SetField(card, f, raw);
                    if (err != null) problems.Add($"第{r + 1}行 {id} 的 {f.Name}：{err}");
                }

                string assetName = id + "_" + SafeFileName(string.IsNullOrEmpty(card.cardName) ? "未命名" : card.cardName);
                if (isNew)
                {
                    string folder = EnsureFolder(OutputRoot + "/" + card.cardType);
                    string path = AssetDatabase.GenerateUniqueAssetPath(folder + "/" + assetName + ".asset");
                    AssetDatabase.CreateAsset(card, path);
                    created++;
                }
                else
                {
                    string path = AssetDatabase.GetAssetPath(card);
                    if (Path.GetFileNameWithoutExtension(path) != assetName)
                    {
                        string err = AssetDatabase.RenameAsset(path, assetName);
                        if (!string.IsNullOrEmpty(err)) problems.Add($"{id} 改名失败：{err}");
                    }
                    EditorUtility.SetDirty(card);
                    updated++;
                }
            }
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        foreach (string p in problems) Debug.LogWarning("[卡牌导入] " + p);
        string summary = $"新建 {created} 张，更新 {updated} 张，跳过 {skipped} 行。";
        if (problems.Count > 0) summary += $"\n有 {problems.Count} 条提示，详见 Console。";
        Debug.Log("[卡牌导入] " + summary);
        EditorUtility.DisplayDialog("卡牌导入完成", summary, "好");
    }

    static List<CardData> FindExistingCards()
    {
        var list = new List<CardData>();
        foreach (string guid in AssetDatabase.FindAssets("t:CardData", new[] { OutputRoot }))
        {
            CardData card = AssetDatabase.LoadAssetAtPath<CardData>(AssetDatabase.GUIDToAssetPath(guid));
            if (card != null) list.Add(card);
        }
        return list;
    }

    // 资产名形如 "STR_L01A_抹除"：以 "id_" 开头就算同一张牌（改了卡名也能找回来）
    // 旧的 Card_Wash 之类不会被匹配到，导入不会动它们
    static CardData FindById(List<CardData> cards, string id)
    {
        string prefix = id + "_";
        foreach (CardData c in cards)
        {
            string name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(c));
            if (name == id || name.StartsWith(prefix, StringComparison.Ordinal)) return c;
        }
        return null;
    }

    static string SetField(CardData card, FieldInfo f, string raw)
    {
        Type t = f.FieldType;
        if (t == typeof(string))
        {
            f.SetValue(card, raw.Replace("\\n", "\n"));
        }
        else if (t == typeof(int))
        {
            if (raw.Length == 0) { f.SetValue(card, 0); return null; }
            int v;
            if (!int.TryParse(raw, out v)) return $"\"{raw}\" 不是整数";
            f.SetValue(card, v);
        }
        else if (t == typeof(float))
        {
            if (raw.Length == 0) { f.SetValue(card, 0f); return null; }
            float v;
            if (!float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v))
                return $"\"{raw}\" 不是数字";
            f.SetValue(card, v);
        }
        else if (t == typeof(bool))
        {
            string s = raw.ToLowerInvariant();
            f.SetValue(card, s == "1" || s == "true" || s == "y" || s == "yes" || s == "是" || s == "√");
        }
        else if (t.IsEnum)
        {
            if (raw.Length == 0) return null;  // 留空保持原值
            string name = EnumAlias.TryGetValue(raw, out string alias) ? alias : raw;
            try { f.SetValue(card, Enum.Parse(t, name, true)); }
            catch { return $"\"{raw}\" 不是 {t.Name} 里的选项（可选：{string.Join("/", Enum.GetNames(t))}）"; }
        }
        else if (t == typeof(Sprite))
        {
            if (raw.Length == 0) return null;  // 留空不覆盖手动拖的图
            Sprite s = FindSprite(raw);
            if (s == null) return $"找不到名为 \"{raw}\" 的 Sprite";
            f.SetValue(card, s);
        }
        else
        {
            return $"暂不支持 {t.Name} 类型的列";
        }
        return null;
    }

    static Sprite FindSprite(string name)
    {
        foreach (string guid in AssetDatabase.FindAssets(name + " t:Sprite"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            foreach (UnityEngine.Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                Sprite s = o as Sprite;
                if (s != null && s.name == name) return s;
            }
        }
        return null;
    }

    static string EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return folder;
        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        return folder;
    }

    static string SafeFileName(string s)
    {
        foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "");
        return s.Replace("\"", "").Replace(".", "").Trim();
    }

    static string Cell(List<string> row, int col)
    {
        return col < row.Count ? row[col].Replace("\r", "").Trim() : "";
    }

    // 先按 UTF-8 读，不合法再按 GBK 读（WPS/旧版 Excel 默认存 GBK）
    static string ReadCsvText(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        try
        {
            return new UTF8Encoding(false, true).GetString(bytes).TrimStart('﻿');
        }
        catch (DecoderFallbackException) { }
        try
        {
            return Encoding.GetEncoding(936).GetString(bytes);
        }
        catch (Exception) { }
        return null;
    }

    // 支持引号包裹、字段内逗号/换行、"" 转义
    static List<List<string>> ParseCsv(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];
            if (inQuotes)
            {
                if (ch == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(ch);
            }
            else if (ch == '"') inQuotes = true;
            else if (ch == ',') { row.Add(sb.ToString()); sb.Length = 0; }
            else if (ch == '\n')
            {
                row.Add(sb.ToString()); sb.Length = 0;
                rows.Add(row); row = new List<string>();
            }
            else if (ch != '\r') sb.Append(ch);
        }
        if (sb.Length > 0 || row.Count > 0) { row.Add(sb.ToString()); rows.Add(row); }
        return rows;
    }
}
