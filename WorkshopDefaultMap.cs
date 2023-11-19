namespace WorkshopDefaultMap;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;

public class WorkshopDefaultMap : BasePlugin
{
    public override string ModuleName => "Workshop Collection Default Map";
    public override string ModuleVersion => "0.2";
    public override string ModuleAuthor => "Cruze";
    public override string ModuleDescription => "Sets default map after server restart";

    private string FilePath => Path.Join(ModuleDirectory, "WorkshopDefaultMap.txt"); 

    private string MapName = "";

    private bool g_bChangeMap = true;

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        MapName = File.ReadAllText(FilePath);

        g_bChangeMap = true;

        if(string.IsNullOrEmpty(MapName))
        {
            LogError("WorkshopDefaultMap.txt is blank. Plugin will not work as intended.");
        }

        Log($"MapName found: {MapName}");

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    private void OnMapStart(string mapName)
    {
        if (!g_bChangeMap || string.IsNullOrEmpty(MapName)) return;
        
        AddTimer(7.0f, () => ChangeMap(), TimerFlags.STOP_ON_MAPCHANGE);
        Log($"Changing map to {MapName}...");
    }

    private void ChangeMap()
    {
        if (!g_bChangeMap || string.IsNullOrEmpty(MapName))
        {
            LogError($"[ChangeMap] Error. {g_bChangeMap} & \"{MapName}\"");
            return;
        }

        Server.ExecuteCommand($"ds_workshop_changelevel {MapName}");
        NativeAPI.IssueServerCommand($"ds_workshop_changelevel {MapName}");
        Log($"Changed map to {MapName}.");
        
        g_bChangeMap = false;
    }

    private static void LogError(string message)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void Log(string message)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
