using EGOIST.Domain.Entities;
using LibreHardwareMonitor.Hardware;
using SharpDX.DXGI;

namespace EGOIST.Application.Services.Utilities
{
    public class SystemInfoService
    {
        private Computer? _computer;
        public SystemInfo systemInfo = new SystemInfo();

        private static SystemInfoService? _instance;

        public static SystemInfoService Instance => _instance ??= new SystemInfoService();

        /// <summary>
        /// Starts monitoring system hardware information.
        /// </summary>
        /// <returns>A task that completes when the monitoring process is started.</returns>
        public async Task Monitor()
        {
            if (_computer == null)
            {
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = true
                };
                _computer.Open();

                while (true)
                {
                    await Task.Run(() =>
                    {
                        foreach (var hardware in _computer.Hardware)
                        {
                            hardware.Update();

                            switch (hardware.HardwareType)
                            {
                                case HardwareType.Cpu:
                                    systemInfo.CPU = GetSensorValue(hardware, "CPU Total");
                                    break;
                                case HardwareType.Memory:
                                    systemInfo.RAM = GetSensorValue(hardware, "Memory");
                                    break;
                                case HardwareType.GpuNvidia:
                                case HardwareType.GpuAmd:
                                case HardwareType.GpuIntel:
                                    systemInfo.GPU = GetSensorValue(hardware, "GPU Core");
                                    systemInfo.VRAM = GetSensorValue(hardware, "GPU Memory");
                                    systemInfo.VRAMUsed = GetSensorValue(hardware, "D3D Dedicated Memory Used");
                                    break;
                            }
                        }
                    });

                    await Task.Delay(1000);
                }
            }
        }

        /// <summary>
        /// Sets the system information and initializes the VRAM if it's not set.
        /// </summary>
        /// <param name="info">The system information to set.</param>
        public void SetSystemInfo(ref SystemInfo info)
        {
            systemInfo = info;

            // Initialize VRAM if it's not set
            if (systemInfo.VRAM > 0) return;
            using var factory = new Factory4();
            using var adapter = new Adapter4(factory.GetAdapter1(0).NativePointer);
            systemInfo.VRAM = adapter.Description.DedicatedVideoMemory / (1024 * 1024);
        }

        /// <summary>
        /// Gets the value of a sensor with the specified name.
        /// </summary>
        /// <param name="hardware">The hardware containing the sensor.</param>
        /// <param name="sensorName">The name of the sensor.</param>
        /// <returns>The sensor value, or 0 if the sensor is not found.</returns>
        private static int GetSensorValue(IHardware hardware, string sensorName)
        {
            var sensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name == sensorName);
            return sensor != null ? (int)sensor.Value! : 0;
        }
    }
}