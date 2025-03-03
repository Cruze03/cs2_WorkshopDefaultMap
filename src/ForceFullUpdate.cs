using System.Runtime.InteropServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;


[StructLayout(LayoutKind.Sequential)]
struct CUtlMemory
{
    public unsafe nint* m_pMemory;
    public int m_nAllocationCount;
    public int m_nGrowSize;
}

[StructLayout(LayoutKind.Sequential)]
struct CUtlVector
{
    public unsafe nint this[int index]
    {
        get => this.m_Memory.m_pMemory[index];
        set => this.m_Memory.m_pMemory[index] = value;
    }

    public int m_iSize;
    public CUtlMemory m_Memory;
    
    public nint Element(int index) => this[index];
}

class INetworkServerService : NativeObject
{
    private readonly VirtualFunctionWithReturn<nint, nint> GetIGameServerFunc;

    public INetworkServerService() : base(NativeAPI.GetValveInterface(0, "NetworkServerService_001"))
    {
        this.GetIGameServerFunc = new VirtualFunctionWithReturn<nint, nint>(this.Handle, GameData.GetOffset("INetworkServerService_GetIGameServer"));
    }

    public INetworkGameServer GetIGameServer()
    {
        return new INetworkGameServer(this.GetIGameServerFunc.Invoke(this.Handle));
    }
}

public class INetworkGameServer : NativeObject
{
    private static int SlotsOffset = GameData.GetOffset("INetworkGameServer_Slots");

    private CUtlVector Slots;

    public INetworkGameServer(nint ptr) : base(ptr)
    {
        this.Slots = Marshal.PtrToStructure<CUtlVector>(base.Handle + SlotsOffset);
    }
}