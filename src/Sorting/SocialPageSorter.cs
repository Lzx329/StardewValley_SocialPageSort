using System;
using System.Collections.Generic;
using System.Globalization;
using StardewValley;
using StardewValley.Menus;

namespace SocialPageSort.Sorting;

/// <summary>集中存放所有对社交页内部成员的访问与排序逻辑（NFR-3）。</summary>
internal static class SocialPageSorter
{
    /// <summary>名字比较使用的文化（zh-CN ICU，汉字按拼音）。</summary>
    private static readonly CompareInfo? NameCompareInfo = CreateNameCompareInfo();

    /// <summary>忽略大小写、全半角与变音符号，避免仅显示差异导致顺序抖动。</summary>
    private const CompareOptions NameCompareOptions =
        CompareOptions.IgnoreCase | CompareOptions.IgnoreWidth | CompareOptions.IgnoreNonSpace;

    /// <summary>对条目列表中的 NPC 段排序，玩家段保持原位与相对顺序。</summary>
    public static void SortInPlace(List<SocialPage.SocialEntry> entries, ModConfig config)
    {
        int firstNpc = 0;
        while (firstNpc < entries.Count && entries[firstNpc].IsPlayer)
        {
            firstNpc++;
        }

        int count = entries.Count - firstNpc;
        if (count < 2)
        {
            return;
        }

        var npcEntries = new List<SocialPage.SocialEntry>(count);
        for (int i = 0; i < count; i++)
        {
            npcEntries.Add(entries[firstNpc + i]);
        }

        Comparison<SocialPage.SocialEntry> comparison = BuildComparison(config);
        var order = new int[count];
        for (int i = 0; i < count; i++)
        {
            order[i] = i;
        }

        // 用原始下标作为最终比较键，保证「数据缺失」的条目保持原相对顺序（稳定排序）。
        Array.Sort(order, (a, b) =>
        {
            int result = comparison(npcEntries[a], npcEntries[b]);
            return result != 0 ? result : a.CompareTo(b);
        });

        for (int i = 0; i < count; i++)
        {
            entries[firstNpc + i] = npcEntries[order[i]];
        }
    }

    /// <summary>在已打开的社交页上重新排序并重建组件。</summary>
    public static void ApplyToPage(SocialPage page)
    {
        List<SocialPage.SocialEntry> entries = page.SocialEntries;
        SortInPlace(entries, ModEntry.Config);

        page.CreateComponents();

        int firstNpc = 0;
        while (firstNpc < entries.Count && entries[firstNpc].IsPlayer)
        {
            firstNpc++;
        }

        page.slotPosition = firstNpc;
        PositionScrollBar(page);
        page.updateSlots();
    }

    private static Comparison<SocialPage.SocialEntry> BuildComparison(ModConfig config)
    {
        SortMode mode = config.SortMode;
        bool descending = config.GetDescending(mode);

        return (a, b) =>
        {
            // 未见面角色一律排在已见面角色之后，彼此保持原顺序（FR-2.6）。
            if (a.IsMet != b.IsMet)
            {
                return a.IsMet ? -1 : 1;
            }

            if (!a.IsMet)
            {
                return 0;
            }

            switch (mode)
            {
                case SortMode.Name:
                    return CompareName(a, b, descending);

                case SortMode.Birthday:
                {
                    int? aDays = GetDaysUntilBirthday(a);
                    int? bDays = GetDaysUntilBirthday(b);
                    if (aDays.HasValue != bDays.HasValue)
                    {
                        return aDays.HasValue ? -1 : 1;
                    }

                    if (!aDays.HasValue)
                    {
                        return 0;
                    }

                    int birthdayResult = CompareInt(aDays.Value, bDays!.Value, descending);
                    return birthdayResult != 0 ? birthdayResult : CompareName(a, b, false);
                }

                default:
                {
                    bool aHasData = a.Friendship != null;
                    bool bHasData = b.Friendship != null;
                    if (aHasData != bHasData)
                    {
                        return aHasData ? -1 : 1;
                    }

                    if (!aHasData)
                    {
                        return 0;
                    }

                    int heartResult = CompareInt(a.HeartLevel, b.HeartLevel, descending);
                    if (heartResult != 0)
                    {
                        return heartResult;
                    }

                    int pointResult = CompareInt(a.Friendship!.Points, b.Friendship!.Points, descending);
                    return pointResult != 0 ? pointResult : CompareName(a, b, false);
                }
            }
        };
    }

    private static int CompareName(SocialPage.SocialEntry a, SocialPage.SocialEntry b, bool descending)
    {
        // 名字分两段：以汉字开头的名字在前（按拼音），非汉字名（拉丁字母等）整体在后（按字母序）。
        int aSegment = IsHanName(a.DisplayName) ? 0 : 1;
        int bSegment = IsHanName(b.DisplayName) ? 0 : 1;
        if (aSegment != bSegment)
        {
            return aSegment.CompareTo(bSegment);
        }

        int result = CompareDisplayName(a.DisplayName, b.DisplayName);
        if (result != 0)
        {
            return descending ? -result : result;
        }

        // 显示名相同（含仅大小写差异）时用 InternalName 兜底，保证多次打开顺序一致。
        return string.CompareOrdinal(a.InternalName ?? string.Empty, b.InternalName ?? string.Empty);
    }

    private static int CompareDisplayName(string? a, string? b)
    {
        string left = a ?? string.Empty;
        string right = b ?? string.Empty;
        return NameCompareInfo != null
            ? NameCompareInfo.Compare(left, right, NameCompareOptions)
            : string.CompareOrdinal(left, right);
    }

    /// <summary>显示名是否以汉字开头（含扩展 A/B 与兼容区）。</summary>
    private static bool IsHanName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        int codePoint = char.IsSurrogatePair(name, 0) ? char.ConvertToUtf32(name, 0) : name[0];
        return (codePoint >= 0x3400 && codePoint <= 0x4DBF)
            || (codePoint >= 0x4E00 && codePoint <= 0x9FFF)
            || (codePoint >= 0xF900 && codePoint <= 0xFAFF)
            || (codePoint >= 0x20000 && codePoint <= 0x2FA1F);
    }

    private static int CompareInt(int a, int b, bool descending)
    {
        return descending ? b.CompareTo(a) : a.CompareTo(b);
    }

    /// <summary>距离下一个生日的天数；当天生日为 0。无生日数据返回 null。</summary>
    private static int? GetDaysUntilBirthday(SocialPage.SocialEntry entry)
    {
        if (entry.IsChild || entry.Data == null || entry.Character is not NPC npc)
        {
            return null;
        }

        string season = npc.Birthday_Season;
        int day = npc.Birthday_Day;
        if (string.IsNullOrEmpty(season) || day < 1 || day > WorldDate.DaysPerMonth)
        {
            return null;
        }

        int seasonIndex = Utility.getSeasonNumber(season);
        if (seasonIndex < 0)
        {
            return null;
        }

        WorldDate today = Game1.Date;
        int todayDay = today.SeasonIndex * WorldDate.DaysPerMonth + (today.DayOfMonth - 1);
        int birthdayDay = seasonIndex * WorldDate.DaysPerMonth + (day - 1);
        int days = birthdayDay - todayDay;
        if (days < 0)
        {
            days += WorldDate.DaysPerYear;
        }

        return days;
    }

    private static CompareInfo? CreateNameCompareInfo()
    {
        try
        {
            return CultureInfo.GetCultureInfo("zh-CN").CompareInfo;
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                "pinyin-culture",
                $"Could not load the zh-CN comparer, falling back to ordinal sorting. {ex.GetType().Name}: {ex.Message}",
                StardewModdingAPI.LogLevel.Warn);
            return null;
        }
    }

    /// <summary>复刻原版 setScrollBarToCurrentIndex 的公式（该方法为 private）。</summary>
    private static void PositionScrollBar(SocialPage page)
    {
        if (page.scrollBar == null || page.upButton == null || page.downButton == null)
        {
            return;
        }

        int count = page.sprites.Count;
        if (count <= 0)
        {
            return;
        }

        int divisor = Math.Max(1, count - 4);
        page.scrollBar.bounds.Y =
            page.scrollBarRunner.Height / divisor * page.slotPosition + page.upButton.bounds.Bottom + 4;

        if (page.slotPosition == count - 5)
        {
            page.scrollBar.bounds.Y = page.downButton.bounds.Y - page.scrollBar.bounds.Height - 4;
        }
    }
}
