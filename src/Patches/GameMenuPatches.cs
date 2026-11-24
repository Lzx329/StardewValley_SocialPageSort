using System;
using HarmonyLib;
using SocialPageSort.Sorting;
using SocialPageSort.UI;
using StardewModdingAPI;
using StardewValley.Menus;

namespace SocialPageSort.Patches;

/// <summary>切换标签时关闭面板，并在进入社交标签时按当前配置重新排序。</summary>
[HarmonyPatch(typeof(GameMenu))]
internal static class GameMenuPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameMenu.changeTab))]
    private static void ChangeTab_Postfix(GameMenu __instance, int whichTab)
    {
        try
        {
            foreach (IClickableMenu page in __instance.pages)
            {
                if (page is SocialPage socialPage)
                {
                    SortOverlay.Close(socialPage);
                }
            }

            if (whichTab != GameMenu.socialTab || __instance.pages.Count <= GameMenu.socialTab)
            {
                return;
            }

            if (__instance.pages[GameMenu.socialTab] is SocialPage openedPage)
            {
                SocialPageSorter.ApplyToPage(openedPage);
            }
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                "change-tab",
                $"Could not apply the sort when switching to the social tab. {ex}",
                LogLevel.Warn);
        }
    }
}
