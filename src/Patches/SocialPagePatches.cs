using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using SocialPageSort.Sorting;
using SocialPageSort.UI;
using StardewModdingAPI;
using StardewValley.Menus;

namespace SocialPageSort.Patches;

/// <summary>社交页的排序、绘制与输入补丁（全部为前缀/后缀，NFR-6）。</summary>
[HarmonyPatch(typeof(SocialPage))]
internal static class SocialPagePatches
{
    /// <summary>构造函数中 FindSocialCharacters 早于 CreateComponents，在这里排序即可让组件与数据一致。</summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(SocialPage.FindSocialCharacters))]
    private static void FindSocialCharacters_Postfix(List<SocialPage.SocialEntry> __result)
    {
        try
        {
            SocialPageSorter.SortInPlace(__result, ModEntry.Config);
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                "sort-entries",
                $"Sorting failed; keeping the game's original order. {ex}",
                LogLevel.Warn);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SocialPage.draw))]
    private static void Draw_Postfix(SocialPage __instance, SpriteBatch b)
    {
        try
        {
            SortOverlay.Draw(__instance, b);
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                "draw-overlay",
                $"Could not draw the sort button/panel. {ex}",
                LogLevel.Warn);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(SocialPage.receiveLeftClick))]
    private static bool ReceiveLeftClick_Prefix(SocialPage __instance, int x, int y)
    {
        try
        {
            return SortOverlay.HandleLeftClick(__instance, x, y);
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                "handle-click",
                $"Could not handle a sort panel click. {ex}",
                LogLevel.Warn);
            return true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(SocialPage.performHoverAction))]
    private static void PerformHoverAction_Postfix(SocialPage __instance, int x, int y)
    {
        try
        {
            SortOverlay.HandleHover(__instance, x, y);
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                "handle-hover",
                $"Could not handle hovering the sort button/panel. {ex}",
                LogLevel.Warn);
        }
    }
}
