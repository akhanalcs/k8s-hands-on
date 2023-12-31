using System.Net;
using System.Runtime.InteropServices;

namespace TestApp.Api;

public readonly struct EnvironmentInfo
{
    public EnvironmentInfo()
    {
        var gcInfo = GC.GetGCMemoryInfo();
        TotalAvailableMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        string[] memoryLimitPaths =
        {
            "/sys/fs/cgroup/memory.max",
            "/sys/fs/cgroup/memory.high",
            "/sys/fs/cgroup/memory.low",
            "/sys/fs/cgroup/memory/memory.limit_in_bytes"
        };

        string[] currentMemoryPaths =
        {
            "/sys/fs/cgroup/memory.current",
            "/sys/fs/cgroup/memory/memory.usage_in_bytes"
        };

        MemoryLimit = GetBestValue(memoryLimitPaths);
        MemoryUsage = GetBestValue(currentMemoryPaths);
    }
    
    public long TotalAvailableMemoryBytes { get; }
    public long MemoryLimit { get; }
    public long MemoryUsage { get; }
    public string RuntimeVersion => RuntimeInformation.FrameworkDescription;
    public string OSVersion => RuntimeInformation.OSDescription;
    public string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();
    public string User => Environment.UserName;
    public int ProcessorCount => Environment.ProcessorCount;
    // This is set in values.yaml which which makes it to deployment.yaml
    public string? PodIpAddress => Environment.GetEnvironmentVariable("Runtime__PodIpAddress");
    public string? HostIpAddress => Environment.GetEnvironmentVariable("Runtime__HostIpAddress");
    public string? AspNetCoreEnvironment => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    public string HostName => Dns.GetHostName();
    
    private static long GetBestValue(string[] paths)
    {
        foreach (var path in paths)
        {
            if (Path.Exists(path) &&
                long.TryParse(File.ReadAllText(path), out long result))
            {
                return result;
            }
        }

        return 0;
    }
}