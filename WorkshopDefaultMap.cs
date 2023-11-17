namespace WorkshopDefaultMap;

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

public class WorkshopDefaultMap : BasePlugin
{
    public override string ModuleName => "Workshop Collection Default Map";
    public override string ModuleVersion => "0.1";
    public override string ModuleAuthor => "Cruze";
    public override string ModuleDescription => "Sets default map after server restart";

    private string FilePath => Path.Join(ModuleDirectory, "WorkshopDefaultMap.txt"); 

    private string MapName = "";

    private bool g_bChangeMap;

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        MapName = File.ReadAllText(FilePath);

        if(string.IsNullOrEmpty(MapName))
        {
            Log("WorkshopDefaultMap.txt is blank. Plugin will not work as intended.");
        }

        g_bChangeMap = true;

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
    }

    private void OnMapStart(string mapName)
    {
        if (!g_bChangeMap || string.IsNullOrEmpty(MapName)) return;
        
        base.AddTimer(2.0f, Timer_OnMapStart);
    }

    private void Timer_OnMapStart()
    {
        if (!g_bChangeMap) return;
        
        Server.ExecuteCommand($"ds_workshop_changelevel {MapName}");
        NativeAPI.IssueServerCommand($"ds_workshop_changelevel {MapName}");

        g_bChangeMap = false;
    }

    private static void Log(string message)
    {
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}