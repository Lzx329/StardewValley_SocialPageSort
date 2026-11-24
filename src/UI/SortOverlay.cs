using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SocialPageSort.Sorting;
using StardewValley;
using StardewValley.Menus;

namespace SocialPageSort.UI;

/// <summary>标签条左侧空白格的排序按钮与展开面板。</summary>
internal static class SortOverlay
{
    private const int ButtonHeight = 44;
    private const int PanelWidth = 264;
    private const int RowHeight = 46;
    private const int PanelPadding = 12;
    private const int ArrowWidth = 22;
    private const int ArrowHeight = 24;

    private static readonly SortMode[] Modes = { SortMode.Friendship, SortMode.Name, SortMode.Birthday };

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<SocialPage, PageState> States =
        new System.Runtime.CompilerServices.ConditionalWeakTable<SocialPage, PageState>();

    private sealed class PageState
    {
        public bool PanelOpen;
    }

    /// <summary>点击排序项后关闭面板（切换标签/关闭菜单时调用）。</summary>
    public static void Close(SocialPage page)
    {
        GetState(page).PanelOpen = false;
    }

    /// <summary>绘制按钮与面板。</summary>
    public static void Draw(SocialPage page, SpriteBatch b)
    {
        Rectangle button = GetButtonBounds(page);
        DrawBox(b, button, Color.White);
        DrawButtonLabel(b, button);

        if (!GetState(page).PanelOpen)
        {
            return;
        }

        Rectangle panel = GetPanelBounds(page);
        b.Draw(Game1.staminaRect, new Rectangle(panel.X + 8, panel.Y + 8, panel.Width, panel.Height), Color.Black * 0.25f);
        DrawBox(b, panel, Color.White);

        for (int i = 0; i < Modes.Length; i++)
        {
            SortMode mode = Modes[i];
            Rectangle row = GetRowBounds(page, i);

            if (ModEntry.Config.SortMode == mode)
            {
                b.Draw(Game1.staminaRect, row, Color.Wheat * 0.55f);
                b.Draw(
                    Game1.staminaRect,
                    new Rectangle(row.X, row.Y, 6, row.Height),
                    Color.SaddleBrown);
            }

            string label = Translate("mode." + mode.ToString().ToLowerInvariant());
            Vector2 labelSize = Game1.smallFont.MeasureString(label);
            b.DrawString(
                Game1.smallFont,
                label,
                new Vector2(row.X + 10, row.Center.Y - labelSize.Y / 2f),
                Game1.textColor);

            Rectangle arrow = GetArrowBounds(page, i);
            Rectangle arrowSource = ModEntry.Config.GetDescending(mode)
                ? new Rectangle(421, 472, 11, 12)
                : new Rectangle(421, 459, 11, 12);
            b.Draw(Game1.mouseCursors, arrow, arrowSource, Color.White);
        }
    }

    /// <summary>处理左键点击；返回 false 表示拦截原版的点击处理。</summary>
    public static bool HandleLeftClick(SocialPage page, int x, int y)
    {
        PageState state = GetState(page);

        if (GetButtonBounds(page).Contains(x, y))
        {
            state.PanelOpen = !state.PanelOpen;
            Game1.playSound("shwip");
            return false;
        }

        if (!state.PanelOpen)
        {
            return true;
        }

        Rectangle panel = GetPanelBounds(page);
        if (panel.Contains(x, y))
        {
            for (int i = 0; i < Modes.Length; i++)
            {
                if (GetArrowBounds(page, i).Contains(x, y))
                {
                    ApplyMode(page, Modes[i], flipDirection: true);
                    return false;
                }

                if (GetRowBounds(page, i).Contains(x, y))
                {
                    ApplyMode(page, Modes[i], flipDirection: false);
                    return false;
                }
            }

            return false;
        }

        state.PanelOpen = false;

        // 点击页面内部时吞掉这次点击，避免误触角色条目；滚动条在页面右侧外部，仍交给原版。
        bool insidePage = new Rectangle(
            page.xPositionOnScreen,
            page.yPositionOnScreen,
            page.width,
            page.height).Contains(x, y);

        return !insidePage;
    }

    /// <summary>悬停提示。</summary>
    public static void HandleHover(SocialPage page, int x, int y)
    {
        PageState state = GetState(page);

        if (!state.PanelOpen)
        {
            if (GetButtonBounds(page).Contains(x, y))
            {
                page.hoverText = Translate("button.tooltip");
            }

            return;
        }

        if (GetPanelBounds(page).Contains(x, y))
        {
            page.hoverText = Translate("panel.tooltip");
        }
    }

    private static void ApplyMode(SocialPage page, SortMode mode, bool flipDirection)
    {
        ModConfig config = ModEntry.Config;
        config.SortMode = mode;
        if (flipDirection)
        {
            config.FlipDescending(mode);
        }
        else
        {
            config.SetDescending(mode, ModConfig.DefaultDescending(mode));
        }

        try
        {
            ModEntry.Instance.Helper.WriteConfig(config);
        }
        catch (Exception ex)
        {
            ModEntry.LogOnce(
                "write-config",
                $"Could not write config.json; the choice will apply for this session only. {ex.GetType().Name}: {ex.Message}",
                StardewModdingAPI.LogLevel.Warn);
        }

        SocialPageSorter.ApplyToPage(page);
        Game1.playSound("shwip");
    }

    private static PageState GetState(SocialPage page)
    {
        return States.GetValue(page, _ => new PageState());
    }

    private static Rectangle GetButtonBounds(SocialPage page)
    {
        // 右上角无法可靠避让：UI Info Suite 2 等 mod 会在那里自绘按钮，且不在 GameMenu.tabs 里，
        // 拿不到它的边界。改为放在标签条最左侧的空白格：1.6 原版第一个标签的 bounds 从 x+64 开始，
        // 左侧一格始终为空，且没有已知常用 mod 占用。
        int left = page.xPositionOnScreen + 4;
        int right = page.xPositionOnScreen + 60;
        int top = page.yPositionOnScreen + 26;

        if (Game1.activeClickableMenu is GameMenu menu)
        {
            int firstTabLeft = int.MaxValue;
            int firstTabTop = int.MaxValue;
            foreach (ClickableComponent tab in menu.tabs)
            {
                if (tab != null && tab.bounds.Width > 0)
                {
                    firstTabLeft = Math.Min(firstTabLeft, tab.bounds.Left);
                    firstTabTop = Math.Min(firstTabTop, tab.bounds.Y);
                }
            }

            if (firstTabLeft != int.MaxValue)
            {
                // 标签贴图会比 bounds 向左多画约 16px，这里再留 6px 余量。
                right = firstTabLeft - 22;
                top = firstTabTop + 10;
            }
        }

        int width = Math.Clamp(right - left, 32, 56);
        return new Rectangle(left, top, width, ButtonHeight);
    }

    private static void DrawButtonLabel(SpriteBatch b, Rectangle button)
    {
        string text = Translate("button.name");
        Vector2 size = Game1.smallFont.MeasureString(text);
        float scale = size.X + 10f > button.Width ? (button.Width - 8f) / size.X : 1f;

        if (scale >= 0.6f)
        {
            b.DrawString(
                Game1.smallFont,
                text,
                new Vector2(
                    button.Center.X - size.X * scale / 2f,
                    button.Center.Y - size.Y * scale / 2f),
                Game1.textColor,
                0f,
                Vector2.Zero,
                scale,
                SpriteEffects.None,
                1f);
            return;
        }

        // 空间太窄时画三条横线作为排序/菜单图标。
        int barWidth = button.Width - 16;
        int barHeight = 3;
        int gap = 4;
        int totalHeight = barHeight * 3 + gap * 2;
        int top = button.Center.Y - totalHeight / 2;
        for (int i = 0; i < 3; i++)
        {
            b.Draw(
                Game1.staminaRect,
                new Rectangle(button.X + 8, top + i * (barHeight + gap), barWidth, barHeight),
                Game1.textColor);
        }
    }

    private static Rectangle GetPanelBounds(SocialPage page)
    {
        Rectangle button = GetButtonBounds(page);
        int height = PanelPadding * 2 + RowHeight * Modes.Length;
        int top = Math.Max(button.Bottom + 6, page.yPositionOnScreen + 84);
        int x = Math.Max(
            page.xPositionOnScreen + 4,
            Math.Min(button.X, page.xPositionOnScreen + page.width - 36 - PanelWidth));
        return new Rectangle(x, top, PanelWidth, height);
    }

    private static Rectangle GetRowBounds(SocialPage page, int index)
    {
        Rectangle panel = GetPanelBounds(page);
        return new Rectangle(
            panel.X + PanelPadding,
            panel.Y + PanelPadding + index * RowHeight,
            PanelWidth - PanelPadding * 2,
            RowHeight - 4);
    }

    private static Rectangle GetArrowBounds(SocialPage page, int index)
    {
        Rectangle row = GetRowBounds(page, index);
        return new Rectangle(
            row.Right - ArrowWidth - 4,
            row.Center.Y - ArrowHeight / 2,
            ArrowWidth,
            ArrowHeight);
    }

    private static void DrawBox(SpriteBatch b, Rectangle bounds, Color color)
    {
        IClickableMenu.drawTextureBox(
            b,
            Game1.menuTexture,
            new Rectangle(0, 256, 60, 60),
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            color,
            1f,
            true,
            -1f);
    }

    private static string Translate(string key)
    {
        return ModEntry.Instance.Helper.Translation.Get(key).ToString();
    }
}
