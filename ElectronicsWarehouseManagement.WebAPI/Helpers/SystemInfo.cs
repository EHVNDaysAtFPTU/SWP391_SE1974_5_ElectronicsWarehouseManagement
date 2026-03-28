using ElectronicsWarehouseManagement.DTO;
using Hardware.Info;
using System.Diagnostics;

namespace ElectronicsWarehouseManagement.WebAPI.Helpers
{
    internal static class SystemInfo
    {
        public static double GetCpuPercent(Process p, int sampleMs = 500)
        {
            var cpu1 = p.TotalProcessorTime;
            var t1 = Stopwatch.GetTimestamp();
            Thread.Sleep(sampleMs);
            p.Refresh();
            var cpu2 = p.TotalProcessorTime;
            var t2 = Stopwatch.GetTimestamp();
            var cpuDeltaMs = (cpu2 - cpu1).TotalMilliseconds;
            var wallDeltaMs = (t2 - t1) * 1000.0 / Stopwatch.Frequency;
            var percent = cpuDeltaMs / (wallDeltaMs * Environment.ProcessorCount) * 100.0;
            if (percent < 0) percent = 0;
            return percent;
        }

        static HardwareInfo hw = new HardwareInfo();

        internal static SystemInfoResp Get()
        {
            hw.RefreshMemoryStatus();
            hw.RefreshOperatingSystem();
            hw.RefreshComputerSystemList();
            hw.RefreshCPUList();
            hw.RefreshMemoryList();
            CPU cpu = hw.CpuList.FirstOrDefault() ?? throw new Exception("No CPU?");
            Process process = Process.GetCurrentProcess();
            PerformanceCounter counter = new PerformanceCounter("Process", "Working Set - Private", process.ProcessName);
            double currentMem = counter.RawValue / 1024d / 1024d;
            counter.Dispose();

            return new SystemInfoResp
            {
                VendorID = cpu.Manufacturer,
                CPUModel = cpu.Name,
                PhysicalCores = cpu.NumberOfCores,
                LogicalCores = cpu.NumberOfLogicalProcessors,
                OSName = hw.OperatingSystem.Name,
                OSVersion = hw.OperatingSystem.VersionString,
                MachineMemoryUsageMB = (hw.MemoryStatus.TotalPhysical - hw.MemoryStatus.AvailablePhysical) / 1024 / 1024,
                MachineTotalMemoryMB = hw.MemoryStatus.TotalPhysical / 1024 / 1024,
                MachineMemoryUsagePagedMB = (hw.MemoryStatus.TotalPageFile - hw.MemoryStatus.AvailablePageFile) / 1024 / 1024,
                MachineMemoryTotalPagedMB = hw.MemoryStatus.TotalPageFile / 1024 / 1024,
                MachineCPUUsagePercent = cpu.PercentProcessorTime,
                CPUUsagePercent = GetCpuPercent(process),
                CurrentMemoryUsageMB = currentMem,
                CurrentMemoryUsagePagedMB = process.PrivateMemorySize64 / 1024f / 1024f,
                Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime()
            };
        }
    }
}
