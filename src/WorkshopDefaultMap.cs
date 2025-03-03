namespace WorkshopDefaultMap;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
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
    public override string ModuleVersion => "0.5";
    public override string ModuleAuthor => "Cruze";
    public override string ModuleDescription => "Sets default map after server restart";

    private bool g_bServerStarted = true;
    // private ulong g_uOldMapId;

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
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);

        RemoveListener<Listeners.OnMapStart>(OnMapStart);
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
            g_TimerChangeMap = AddTimer(5.0f, ChangeMap);
        }
    }

    private void ChangeMap()
    {
        g_TimerChangeMap = null;

        if(string.IsNullOrEmpty(Config.Map)) return;

        if(Server.MapName.Equals(Config.Map, StringComparison.OrdinalIgnoreCase)) return;

        if(IsDefaultMap(Server.MapName))
        {
            Server.ExecuteCommand($"changelevel {Config.Map}");
            Logger.LogInformation($"Changed map to {Config.Map}.");
            return;
        }

        if(!ulong.TryParse(Config.Map, out ulong mapid))
        {
            Server.ExecuteCommand($"ds_workshop_changelevel {Config.Map}");
            Logger.LogInformation($"Changed map to {Config.Map}.");
            return;
        }

        if(GetCurrentWorkshopMapID() == mapid) return;

        // if(g_uOldMapId == mapid) return; // Hacky fix till there is a way to find workshop id of a map.

        Server.ExecuteCommand($"host_workshop_map {mapid}");
        Logger.LogInformation($"Changed map to mapid {mapid}.");
        // g_uOldMapId = mapid;
    }

    private void ResetTimer()
    {
        g_TimerForceReset = null;
        g_TimerChangeMap?.Kill();
        g_TimerChangeMap = null;
    }

    private bool IsDefaultMap(string map)
    {
        List<string> defaultMapPool = ["ar_baggage","ar_pool_day","ar_shoots","de_anubis","de_ancient","de_dust2","de_inferno",
        "de_mirage","de_nuke","de_overpass","de_vertigo","de_basalt","de_palais","de_train",
        "de_whistle","cs_italy","cs_office"];

        return defaultMapPool.Contains(map);
    }

    private delegate IntPtr GetAddonNameDelegate(IntPtr thisPtr);
    private readonly INetworkServerService networkServerService = new();
    public ulong GetCurrentWorkshopMapID()
    {
        IntPtr networkGameServer = networkServerService.GetIGameServer().Handle;
        IntPtr vtablePtr = Marshal.ReadIntPtr(networkGameServer);
        IntPtr functionPtr = Marshal.ReadIntPtr(vtablePtr + (25 * IntPtr.Size));
        var getAddonName = Marshal.GetDelegateForFunctionPointer<GetAddonNameDelegate>(functionPtr);
        IntPtr result = getAddonName(networkGameServer);
        return ulong.Parse(Marshal.PtrToStringAnsi(result)!.Split(',')[0]);
    }
}
