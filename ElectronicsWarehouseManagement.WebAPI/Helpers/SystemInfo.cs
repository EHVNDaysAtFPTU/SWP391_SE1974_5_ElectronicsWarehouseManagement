using ElectronicsWarehouseManagement.DTO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;

namespace ElectronicsWarehouseManagement.WebAPI.Helpers
{
    internal static class SystemInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        struct PsApiPerformanceInformation
        {
            public int Size;
            public nint CommitTotal;
            public nint CommitLimit;
            public nint CommitPeak;
            public nint PhysicalTotal;
            public nint PhysicalAvailable;
            public nint SystemCache;
            public nint KernelTotal;
            public nint KernelPaged;
            public nint KernelNonPaged;
            public nint PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        class PerfomanceInfoData
        {
            public long CommitTotalPages;
            public long CommitLimitPages;
            public long CommitPeakPages;
            public long PhysicalTotalBytes;
            public long PhysicalAvailableBytes;
            public long SystemCacheBytes;
            public long KernelTotalBytes;
            public long KernelPagedBytes;
            public long KernelNonPagedBytes;
            public long PageSizeBytes;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetLogicalProcessorInformationEx(int relationshipType, nint buffer, out uint returnedLength);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPerformanceInfo([Out] out PsApiPerformanceInformation PerformanceInformation, [In] int Size);

        static PerfomanceInfoData GetPerformanceInfo()
        {
            PerfomanceInfoData data = new PerfomanceInfoData();
            PsApiPerformanceInformation perfInfo = new PsApiPerformanceInformation();
            if (GetPerformanceInfo(out perfInfo, Marshal.SizeOf(perfInfo)))
            {
                /// data in pages
                data.CommitTotalPages = perfInfo.CommitTotal.ToInt64();
                data.CommitLimitPages = perfInfo.CommitLimit.ToInt64();
                data.CommitPeakPages = perfInfo.CommitPeak.ToInt64();
                /// data in bytes
                long pageSize = perfInfo.PageSize.ToInt64();
                data.PhysicalTotalBytes = perfInfo.PhysicalTotal.ToInt64() * pageSize;
                data.PhysicalAvailableBytes = perfInfo.PhysicalAvailable.ToInt64() * pageSize;
                data.SystemCacheBytes = perfInfo.SystemCache.ToInt64() * pageSize;
                data.KernelTotalBytes = perfInfo.KernelTotal.ToInt64() * pageSize;
                data.KernelPagedBytes = perfInfo.KernelPaged.ToInt64() * pageSize;
                data.KernelNonPagedBytes = perfInfo.KernelNonPaged.ToInt64() * pageSize;
                data.PageSizeBytes = pageSize;
                /// counters
                data.HandlesCount = perfInfo.HandlesCount;
                data.ProcessCount = perfInfo.ProcessCount;
                data.ThreadCount = perfInfo.ThreadCount;
            }
            return data;
        }

        static string GetVendorId()
        {
            var cpuInfo = X86Base.CpuId(0, 0);
            return (ConvertToString(cpuInfo.Ebx) + ConvertToString(cpuInfo.Edx) + ConvertToString(cpuInfo.Ecx)).Trim();
        }

        static int GetPhysicalCores()
        {
            GetLogicalProcessorInformationEx(0, nint.Zero, out uint returnedLength);
            nint bufferPtr = Marshal.AllocHGlobal((int)returnedLength);
            GetLogicalProcessorInformationEx(0, bufferPtr, out returnedLength);
            Marshal.FreeHGlobal(bufferPtr);
            return (int)returnedLength / 48;
        }

        static Dictionary<string, string> RunSystemInfo()
        {
            var systemInfo = new Dictionary<string, string>();
            using var process = new Process();
            process.StartInfo.FileName = "systeminfo";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            using var reader = new StreamReader(process.StandardOutput.BaseStream);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] parts = line.Split(":  ", 2);
                if (parts.Length == 2)
                    systemInfo[parts[0].Trim()] = parts[1].Trim();
                else if (systemInfo.Count > 0 && !line.EndsWith(':'))
                    systemInfo[systemInfo.Last().Key] += Environment.NewLine + line.Trim();
                else if (!string.IsNullOrWhiteSpace(line))
                    systemInfo.Add(parts[0].Trim(), "");
            }
            process.WaitForExit();
            return systemInfo;
        }

        static string GetCpuModel() => (ConvertToString(X86Base.CpuId(unchecked((int)0x80000002), 0)) + ConvertToString(X86Base.CpuId(unchecked((int)0x80000003), 0)) + ConvertToString(X86Base.CpuId(unchecked((int)0x80000004), 0))).Trim().TrimEnd('\0');

        static int GetLogicalCores() => X86Base.CpuId(0xB, 1).Ebx & 0xFF;

        static string ConvertToString((int Eax, int Ebx, int Ecx, int Edx) value) => ConvertToString(value.Eax) + ConvertToString(value.Ebx) + ConvertToString(value.Ecx) + ConvertToString(value.Edx);

        static string ConvertToString(int value) => new([.. BitConverter.GetBytes(value).Select(i => (char)i)]);

        internal static SystemInfoResp Get()
        {
            SystemInfoResp result = new SystemInfoResp();
            result.PhysicalCores = GetPhysicalCores();
            if (X86Base.IsSupported)
            {
                var dict = RunSystemInfo();
                int lCores = GetLogicalCores();
                result.VendorID = GetVendorId();
                result.CPUModel = GetCpuModel();
                result.LogicalCores = Math.Max(result.PhysicalCores, lCores);
                result.OSName = dict["OS Name"];
                result.OSVersion = dict["OS Version"].Split("Build ").Last();
            }
            var pInfo = GetPerformanceInfo();
            GetPhysicallyInstalledSystemMemory(out long totalMem);
            totalMem /= 1024;
            double totalMemPaged = pInfo.CommitTotalPages * pInfo.PageSizeBytes / 1024f / 1024f + totalMem;
            double usedMem = (pInfo.PhysicalTotalBytes - pInfo.PhysicalAvailableBytes) / 1024f / 1024f;
            result.MachineMemoryUsageMB = usedMem;
            result.MachineTotalMemoryMB = totalMem;
            result.MachineMemoryUsagePagedMB = totalMemPaged;
            Process process = Process.GetCurrentProcess();
            PerformanceCounter counter = new PerformanceCounter("Process", "Working Set - Private", process.ProcessName);
            double currentMem = counter.RawValue / 1024d / 1024d;
            counter.Dispose();
            double currentMemPaged = process.PrivateMemorySize64 / 1024f / 1024f;
            counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
            counter.NextValue();
            Thread.Sleep(200);
            double cpuUsage = counter.NextValue();
            counter.Dispose();
            TimeSpan uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
            result.CurrentMemoryUsageMB = currentMem;
            result.CurrentMemoryUsagePagedMB = currentMemPaged;
            result.CPUUsagePercent = cpuUsage;
            result.Uptime = uptime;
            return result;
        }
    }
}
