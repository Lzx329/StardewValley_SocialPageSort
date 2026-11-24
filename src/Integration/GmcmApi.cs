using System;
using StardewModdingAPI;

namespace SocialPageSort.Integration;

/// <summary>Generic Mod Config Menu 的软依赖接口（只声明用到的成员，签名与 GMCM 1.16 一致）。</summary>
public interface IGenericModConfigMenuApi
{
    void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

    void AddTextOption(
        IManifest mod,
        Func<string> getValue,
        Action<string> setValue,
        Func<string> name,
        Func<string>? tooltip = null,
        string[]? allowedValues = null,
        Func<string, string>? formatAllowedValue = null,
        string? fieldId = null);

    void AddBoolOption(
        IManifest mod,
        Func<bool> getValue,
        Action<bool> setValue,
        Func<string> name,
        Func<string>? tooltip = null,
        string? fieldId = null);
}
