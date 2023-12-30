using LibreHardwareMonitor.Hardware;
using NetFabric.Hyperlinq;

namespace EGOIST.Data;

public partial class SystemInfo : ObservableObject
{
    [ObservableProperty]
    public int _CPU;
    [ObservableProperty]
    public int _RAM;
    [ObservableProperty]
    public int _GPU;
    [ObservableProperty]
    public float _VRAM;

    private Computer? computer;

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
                            CPU = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "CPU Total")?.Value ?? 0);
                            break;
                        case HardwareType.Memory:
                            RAM = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "Memory")?.Value ?? 0);
                            break;
                        case HardwareType.GpuNvidia:
                        case HardwareType.GpuAmd:
                        case HardwareType.GpuIntel:
                            GPU = (int)(hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "GPU Core")?.Value ?? 0);
                            var vramUsed = hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name == "GPU Memory")?.Value ?? 0;
                            var vramTotal = hardware.Sensors.AsValueEnumerable().FirstOrDefault(s => s.SensorType == SensorType.SmallData && s.Name == "GPU Memory Total")?.Value ?? 0;
                            VRAM = vramUsed;
                            //VRAM = (int)(vramUsed / vramTotal * 100);
                            break;
                    }
                }
            });

            await Task.Delay(1000);
        }
    }

    private static SystemInfo? instance;
    public static SystemInfo Instance => instance ?? (instance = new SystemInfo());
}
