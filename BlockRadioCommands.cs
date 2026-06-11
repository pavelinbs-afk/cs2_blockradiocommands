namespace BlockRadioCommands;

using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Core.Attributes;

[MinimumApiVersion(100)]
public class BlockRCommands : BasePlugin
{
    public override string ModuleName => "Block Radio Commands";
    public override string ModuleVersion => "0.91";
    public override string ModuleAuthor => "Cruze & pRfect";
    public override string ModuleDescription =>
        "";
    /// <summary>
    /// Blocks radio + chatwheel; map pings enabled. Optional brc_ping_token_cooldown sets engine player_ping_token_cooldown.

    /// If &gt;= 0, applied to engine <c>player_ping_token_cooldown</c> (seconds). Use <c>-1</c> to leave the engine default (20).
    /// </summary>
    public FakeConVar<float> BrcPingTokenCooldown = new(
        "brc_ping_token_cooldown",
        "If >= 0, sets player_ping_token_cooldown (sec). -1 = do not change.",
        0.1f);

    // player_ping / chatwheel_ping omitted so tactical map markers and pings still work
    private string[] Commands = new string[] { "coverme", "takepoint", "holdpos", "regroup", "followme", "takingfire", "go", "fallback", "sticktog","getinpos", "stormfront", "report", "roger", "enemyspot", "needbackup", "sectorclear", "inposition", "reportingin","getout", "negative", "enemydown", "compliment", "thanks", "cheer", "go_a", "go_b", "sorry", "needrop", "playerradio", "playerchatwheel"};


    public override void Load(bool hotReload)
    {
        RegisterFakeConVars(this);
        BrcPingTokenCooldown.ValueChanged += (_, _) => ApplyPingTokenCooldownOverride();

        for (int i = 0; i < Commands.Length; i++)
        {
            AddCommandListener(Commands[i], CommandListener_RadioCommands);
        }

        RegisterListener<Listeners.OnMapStart>(_ => ApplyPingTokenCooldownOverride());

        ApplyPingTokenCooldownOverride();

        Console.WriteLine("BlockRadioCommands is loaded");
    }

    private void ApplyPingTokenCooldownOverride()
    {
        var want = BrcPingTokenCooldown.Value;
        if (want < 0f)
        {
            return;
        }

        var cv = ConVar.Find("player_ping_token_cooldown");
        if (cv == null)
        {
            Console.WriteLine("[BlockRadioCommands] player_ping_token_cooldown: ConVar not found.");
            return;
        }

        try
        {
            switch (cv.Type)
            {
                case ConVarType.Float32:
                case ConVarType.Float64:
                    cv.SetValue(want);
                    break;
                default:
                    Console.WriteLine($"[BlockRadioCommands] player_ping_token_cooldown: unexpected type {cv.Type}.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BlockRadioCommands] Could not set player_ping_token_cooldown: {ex.Message}");
        }
    }

    private HookResult CommandListener_RadioCommands(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        return HookResult.Handled;
    }
}
