using System.Net.NetworkInformation;

namespace FortnitePerformanceOptimizer.Services;

/// <summary>
/// Service for measuring network performance (Ping, Jitter, Packet Loss)
/// </summary>
public class NetworkMonitoringService
{
    private static readonly Lazy<NetworkMonitoringService> _instance = new(() => new NetworkMonitoringService());
    public static NetworkMonitoringService Instance => _instance.Value;

    private readonly ILogger _logger = LoggingService.Instance;
    private readonly string[] _testServers = 
    {
        "1.1.1.1",      // Cloudflare DNS
        "8.8.8.8",      // Google DNS
        "208.67.222.222" // OpenDNS
    };

    public async Task<NetworkMetrics> MeasureNetworkAsync(CancellationToken cancellationToken = default)
    {
        var metrics = new NetworkMetrics
        {
            MeasurementTime = DateTime.Now
        };

        try
        {
            var pings = new List<long>();
            var packetLossCount = 0;
            const int pingsPerServer = 4;

            foreach (var server in _testServers)
            {
                try
                {
                    using var ping = new Ping();
                    for (int i = 0; i < pingsPerServer; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        try
                        {
                            var reply = await ping.SendPingAsync(server, timeout: 2000);
                            if (reply.Status == IPStatus.Success)
                            {
                                pings.Add(reply.RoundtripTime);
                            }
                            else
                            {
                                packetLossCount++;
                            }
                        }
                        catch
                        {
                            packetLossCount++;
                        }

                        await Task.Delay(50, cancellationToken); // Space out pings
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error pinging {server}: {ex.Message}");
                    packetLossCount += pingsPerServer;
                }
            }

            if (pings.Count > 0)
            {
                metrics.PingMs = pings.Average();
                metrics.PingMinMs = pings.Min();
                metrics.PingMaxMs = pings.Max();

                // Calculate jitter (standard deviation of ping times)
                var mean = pings.Average();
                var variance = pings.Sum(x => Math.Pow(x - mean, 2)) / pings.Count;
                metrics.JitterMs = Math.Sqrt(variance);

                var totalAttempts = _testServers.Length * pingsPerServer;
                metrics.PacketLossPercent = (packetLossCount * 100.0) / totalAttempts;
                metrics.IsValid = true;
            }
            else
            {
                _logger.LogWarning("No successful pings - network may be offline");
                metrics.IsValid = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Network measurement failed: {ex.Message}");
            metrics.IsValid = false;
        }

        return metrics;
    }
}

public class NetworkMetrics
{
    public double PingMs { get; set; }
    public double PingMinMs { get; set; }
    public double PingMaxMs { get; set; }
    public double JitterMs { get; set; }
    public double PacketLossPercent { get; set; }
    public bool IsValid { get; set; }
    public DateTime MeasurementTime { get; set; }
}