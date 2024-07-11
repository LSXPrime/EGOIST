using LibreHardwareMonitor.Hardware;
using SharpDX.DXGI;
using System.Timers;
using EGOIST.Domain.Entities;
using Timer = System.Timers.Timer;

namespace EGOIST.Application.Services.Utilities;

public class SystemInfoService : IDisposable
{
    private const string CpuTotalSensorName = "CPU Total";
    private const string MemorySensorName = "Memory";
    private const string GpuCoreSensorName = "GPU Core";
    private const string DedicatedMemoryUsedSensorName = "D3D Dedicated Memory Used";

    private readonly Computer _computer;
    private readonly Timer _timer;

    public SystemInfo Info { get; } = new();

    public event EventHandler<SystemInfo>? OnUpdate; 

    
    private static readonly Lazy<SystemInfoService> LazyInstance = new(() => new SystemInfoService());
    public static SystemInfoService Instance => LazyInstance.Value;
    
    public SystemInfoService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };
        _computer.Open();

        InitializeVramTotalAsync(); 

        _timer = new Timer(1000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.Start();
    }

    private async void InitializeVramTotalAsync()
    {
        Info.VRAMTotal = await Task.Run(() =>
        {
            using var factory = new Factory4();
            using var adapter = new Adapter4(factory.GetAdapter1(0).NativePointer);
            return adapter.Description.DedicatedVideoMemory / (1024 * 1024);
        });
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        UpdateSystemInfo();
        OnUpdate?.Invoke(this, Info);
    }

    private void UpdateSystemInfo()
    {
        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            switch (hardware.HardwareType)
            {
                case HardwareType.Cpu:
                    Info.CPU = GetSensorValue(hardware, CpuTotalSensorName, SensorType.Load);
                    break;
                case HardwareType.Memory:
                    Info.RAM = GetSensorValue(hardware, MemorySensorName, SensorType.Load);
                    break;
                case HardwareType.GpuNvidia:
                case HardwareType.GpuAmd:
                case HardwareType.GpuIntel:
                    Info.GPU = GetSensorValue(hardware, GpuCoreSensorName, SensorType.Load);
                    Info.VRAMUsed = GetSensorValue(hardware, DedicatedMemoryUsedSensorName, SensorType.SmallData);
                    Info.VRAM = Info.VRAMTotal > 0 ? Info.VRAMUsed * 100 / Info.VRAMTotal : 0;
                    break;
            }
        }
    }

    private static int GetSensorValue(IHardware hardware, string sensorName, SensorType sensorType)
    {
        var sensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == sensorType && s.Name == sensorName);
        return sensor != null ? (int)sensor.Value! : 0;
    }

    public void Dispose()
    {
        _timer.Dispose();
        _computer.Close(); 
    }
}