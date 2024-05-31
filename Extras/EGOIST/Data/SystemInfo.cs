using EGOIST.Domain.Entities;
using LibreHardwareMonitor.Hardware;
using NetFabric.Hyperlinq;

namespace EGOIST.Data;

public partial class SystemInfo : ObservableObject
{
<<<<<<< Updated upstream:EGOIST/Data/SystemInfo.cs
    [ObservableProperty]
    public int _CPU;
    [ObservableProperty]
    public int _RAM;
    [ObservableProperty]
    public int _GPU;
    [ObservableProperty]
    public int _VRAM;

    private Computer? computer;
=======
    private Computer? computer;
    public SystemInfo? systemInfo;

    public void SetSystemInfo(ref SystemInfo info)
    {
        systemInfo = info;
        if (systemInfo.VRAM <= 0)
        {
            var factory = new Factory4();
            var adapter = new Adapter4(factory.GetAdapter1(0).NativePointer);
            var memoryInfo = adapter.Description.DedicatedVideoMemory;
            systemInfo.VRAM = (int)(memoryInfo / (1024f * 1024f));
            factory.Dispose();
            adapter.Dispose();
        }
    }
>>>>>>> Stashed changes:EGOIST.Application/Services/Utilities/SystemInfoService.cs

    public async Task Montitor()
    {
        if (computer != null)
            return;

        computer = new()
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true
        };
        computer.Open();

        while (true)
        {
            await Task.Run(() =>
            {
                foreach (var hardware in computer.Hardware)
                {
                    hardware.Update();
                    switch (hardware.HardwareType)
                    {
                        case HardwareType.Cpu:
                            systemInfo.CPU = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "CPU Total")?.Value ?? 0);
                            break;
                        case HardwareType.Memory:
                            systemInfo.RAM = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "Memory")?.Value ?? 0);
                            break;
                        case HardwareType.GpuNvidia:
                        case HardwareType.GpuAmd:
                        case HardwareType.GpuIntel:
<<<<<<< Updated upstream:EGOIST/Data/SystemInfo.cs
                            GPU = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "GPU Core")?.Value ?? 0);
                            VRAM = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "GPU Memory")?.Value ?? 0);
=======
                            systemInfo.GPU = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "GPU Core")?.Value ?? 0);
                            systemInfo.VRAM = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "GPU Memory")?.Value ?? 0);
                            systemInfo.VRAMUsed = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.Name == "D3D Dedicated Memory Used")?.Value ?? 0);
>>>>>>> Stashed changes:EGOIST.Application/Services/Utilities/SystemInfoService.cs
                            break;
                    }
                }
            });

            await Task.Delay(1000);
        }
    }

    private static SystemInfo? instance;
    public static SystemInfo Instance => instance ??= new SystemInfo();
}
