using EGOIST.Domain.Abstracts;

namespace EGOIST.Domain.Entities;

public class SystemInfo : EntityBase
{
    private int _cpu;
    private int _ram;
    private int _gpu;
    private int _vram;
    private int _vramTotal;
    private int _vramUsed;

    public int CPU { get => _cpu; set => Notify(ref _cpu, value); }
    public int RAM { get => _ram; set => Notify(ref _ram, value); }
    public int GPU { get => _gpu; set => Notify(ref _gpu, value); }
    public int VRAM { get => _vram; set => Notify(ref _vram, value); }
    public int VRAMTotal { get => _vramTotal; set => Notify(ref _vramTotal, value); }
    public int VRAMUsed { get => _vramUsed; set => Notify(ref _vramUsed, value); }
    public int VRAMFree => VRAMTotal - VRAMUsed;
}
