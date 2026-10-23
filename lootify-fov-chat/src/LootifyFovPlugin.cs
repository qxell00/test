using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Config;
using CounterStrikeSharp.API.Modules.Extensions;

namespace LootifyFov;

public sealed class LootifyFovConfig : BasePluginConfig
{
    [JsonPropertyName("DefaultFov")]
    public int DefaultFov { get; set; } = 90;
}

// Plugin separat doar pentru FOV (nu modifica setarile BHOP).
public sealed class LootifyFovPlugin : BasePlugin, IPluginConfig<LootifyFovConfig>
{
    public override string ModuleName => "Lootify FOV HOST";
    public override string ModuleVersion => "3.0.0";
    public override string ModuleAuthor => "Lootify Community";
    public override string ModuleDescription => "FOV individual prin chat";

    private readonly Dictionary<ulong, int> _personalFov = new();
    public LootifyFovConfig Config { get; set; } = new();

    public void OnConfigParsed(LootifyFovConfig config)
    {
        config.DefaultFov = Math.Clamp(config.DefaultFov, 60, 130);
        Config = config;
    }

    [ConsoleCommand("css_fov", "Seteaza FOV personal: !fov 110")]
    [CommandHelper(minArgs: 0, usage: "[60-130 | reset]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void SetPersonalFov(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null || !player.IsValid) return;
        if (info.ArgCount > 1 && info.GetArg(1).Equals("reset", StringComparison.OrdinalIgnoreCase))
        {
            ResetPersonalFov(player, info);
            return;
        }
        if (!ParseFov(info.ArgCount > 1 ? info.GetArg(1) : "120", out int fov))
        {
            info.ReplyToCommand("[Lootify FOV] Valoare valida: 60-130. Exemplu: !fov 110");
            return;
        }

        _personalFov[player.SteamID] = fov;
        bool applied = Apply(player, fov);
        info.ReplyToCommand(applied
            ? $"[Lootify FOV] FOV-ul tau: {fov}."
            : $"[Lootify FOV] FOV {fov} memorat; se aplica la urmatorul spawn.");
    }

    [ConsoleCommand("css_fovreset", "Reseteaza FOV-ul personal la valoarea implicita a hostului")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void ResetPersonalFov(CCSPlayerController? player, CommandInfo info)
    {
        if (player is null || !player.IsValid) return;
        _personalFov.Remove(player.SteamID);
        bool applied = Apply(player, Config.DefaultFov);
        info.ReplyToCommand(applied
            ? $"[Lootify FOV] FOV resetat la {Config.DefaultFov}."
            : $"[Lootify FOV] FOV {Config.DefaultFov} se aplica la urmatorul spawn.");
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn e, GameEventInfo info)
    {
        var player = e.Userid;
        if (player is null || !player.IsValid) return HookResult.Continue;

        // Dupa ce CS2 initializeaza pawn-ul, aplica valoarea pentru acest jucator.
        AddTimer(0.35f, () =>
        {
            if (player.IsValid)
            {
                int fov = _personalFov.TryGetValue(player.SteamID, out int value)
                    ? value : Config.DefaultFov;
                Apply(player, fov);
            }
        });
        return HookResult.Continue;
    }

    private static bool ParseFov(string value, out int fov) =>
        int.TryParse(value, out fov) && fov >= 60 && fov <= 130;

    private static bool Apply(CCSPlayerController? player, int fov)
    {
        if (player is null || !player.IsValid || !player.PlayerPawn.IsValid) return false;
        player.DesiredFOV = (uint)fov;
        Utilities.SetStateChanged(player, "CBasePlayerController", "m_iDesiredFOV");
        return true;
    }
}
