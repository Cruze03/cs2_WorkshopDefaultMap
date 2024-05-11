namespace WorkshopDefaultMap;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

public class WorkshopDefaultMapConfig : BasePluginConfig
{
    public override int Version { get; set; } = 1;

    [JsonPropertyName("Map")]
    public string Map { get; set; } = "Awp_arena_vlgbeta_gg2";
}

public class WorkshopDefaultMap : BasePlugin, IPluginConfig<WorkshopDefaultMapConfig>
{
    public override string ModuleName => "Workshop Collection Default Map";
    public override string ModuleVersion => "0.4";
    public override string ModuleAuthor => "Cruze";
    public override string ModuleDescription => "Sets default map after server restart";

    private bool g_bServerStarted = true;
    private ulong g_uOldMapId;

    private Timer? g_TimerForceReset = null;
    private Timer? g_TimerChangeMap = null;

    public required WorkshopDefaultMapConfig Config { get; set; } = new();

    public void OnConfigParsed(WorkshopDefaultMapConfig config)
    {
        Config = config;

        if(string.IsNullOrEmpty(Config.Map))
            Logger.LogError("Map specified in config is blank. Plugin will not work as intended.");
    }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        RegisterListener<Listeners.OnMapStart>(OnMapStart);

        if(hotReload)
        {
            Logger.LogInformation($"Plugin needs server restart to work.");
        }
    }

    private void OnMapStart(string mapName)
    {
        if(string.IsNullOrEmpty(Config.Map)) return;

        if(g_bServerStarted || g_TimerForceReset != null)
        {
            g_bServerStarted = false;
            g_TimerForceReset?.Kill();
            g_TimerForceReset = AddTimer(10.0f, ResetTimer);
            
            g_TimerChangeMap?.Kill();
            g_TimerChangeMap = AddTimer(1.0f, ChangeMap);
            Logger.LogInformation($"Changing map to {Config.Map}...");
        }
    }

    private void ChangeMap()
    {
        g_TimerChangeMap = null;
        
        if(string.IsNullOrEmpty(Config.Map)) return;
        
        if(Server.MapName.Equals(Config.Map, StringComparison.OrdinalIgnoreCase)) return;

        if(!ulong.TryParse(Config.Map, out ulong mapid))
        {
            Server.ExecuteCommand($"ds_workshop_changelevel {Config.Map}");
            Logger.LogInformation($"Changed map to {Config.Map}.");
            return;
        }

        if(g_uOldMapId == mapid) return; // Hacky fix till there is a way to find workshop id of a map.
        
        Server.ExecuteCommand($"host_workshop_map {mapid}");
        Logger.LogInformation($"Changed map to mapid {mapid}.");
        g_uOldMapId = mapid;
    }

    private void ResetTimer()
    {
        g_TimerForceReset = null;
        g_TimerChangeMap?.Kill();
        g_TimerChangeMap = null;
    }
}
