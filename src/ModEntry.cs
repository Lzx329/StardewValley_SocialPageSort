using System;
using System.Collections.Generic;
using HarmonyLib;
using SocialPageSort.Integration;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace SocialPageSort;

/// <summary>mod 入口：读取配置、打补丁、按需注册 GMCM。</summary>
public sealed class ModEntry : Mod
{
    internal static ModEntry Instance { get; private set; } = null!;

    internal static ModConfig Config { get; set; } = new ModConfig();

    private static readonly HashSet<string> LoggedOnce = new HashSet<string>();

    private Harmony? harmony;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        Config = this.LoadConfig();

        this.harmony = new Harmony(this.ModManifest.UniqueID);
        this.harmony.PatchAll();

        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <summary>同一问题只记录一次日志，避免逐帧刷屏。</summary>
    internal static void LogOnce(string key, string message, LogLevel level)
    {
        if (Instance == null || !LoggedOnce.Add(key))
        {
            return;
        }

        Instance.Monitor.Log(message, level);
    }

    private ModConfig LoadConfig()
    {
        try
        {
            ModConfig? config = this.Helper.ReadConfig<ModConfig>();
            if (config == null)
            {
                this.Monitor.Log("config.json was empty; using defaults (friendship descending, name ascending, birthday ascending).", LogLevel.Warn);
                return new ModConfig();
            }

            return config;
        }
        catch (Exception ex)
        {
            this.Monitor.Log(
                $"Could not read config.json ({ex.GetType().Name}: {ex.Message}); using defaults (friendship descending, name ascending, birthday ascending).",
                LogLevel.Warn);
            return new ModConfig();
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.RegisterGenericModConfigMenu();
    }

    private void RegisterGenericModConfigMenu()
    {
        IGenericModConfigMenuApi? api =
            this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
        if (api == null)
        {
            return;
        }

        api.Register(
            this.ModManifest,
            reset: () => Config = new ModConfig(),
            save: () => this.Helper.WriteConfig(Config),
            titleScreenOnly: false);

        api.AddTextOption(
            this.ModManifest,
            getValue: () => Config.SortMode.ToString(),
            setValue: value =>
            {
                if (Enum.TryParse(value, out SortMode mode) && Enum.IsDefined(typeof(SortMode), mode))
                {
                    Config.SortMode = mode;
                }
            },
            name: () => this.Translate("gmcm.mode.name"),
            tooltip: () => this.Translate("gmcm.mode.tooltip"),
            allowedValues: new[]
            {
                nameof(SortMode.Friendship),
                nameof(SortMode.Name),
                nameof(SortMode.Birthday)
            },
            formatAllowedValue: value => this.Translate("mode." + value.ToLowerInvariant()),
            fieldId: null);

        this.AddDirectionOption(
            api,
            "friendship",
            () => Config.FriendshipDescending,
            value => Config.FriendshipDescending = value);

        this.AddDirectionOption(
            api,
            "name",
            () => Config.NameDescending,
            value => Config.NameDescending = value);

        this.AddDirectionOption(
            api,
            "birthday",
            () => Config.BirthdayDescending,
            value => Config.BirthdayDescending = value);
    }

    private void AddDirectionOption(IGenericModConfigMenuApi api, string modeKey, Func<bool> getValue, Action<bool> setValue)
    {
        api.AddBoolOption(
            this.ModManifest,
            getValue: getValue,
            setValue: setValue,
            name: () => this.Translate($"gmcm.{modeKey}-desc.name"),
            tooltip: () => this.Translate($"gmcm.{modeKey}-desc.tooltip"),
            fieldId: null);
    }

    private string Translate(string key)
    {
        return this.Helper.Translation.Get(key).ToString();
    }
}
