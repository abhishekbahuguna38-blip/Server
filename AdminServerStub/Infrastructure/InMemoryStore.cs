using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AdminServerStub.Models;

namespace AdminServerStub.Infrastructure
{
    public static class InMemoryStore
    {
        public static ConcurrentDictionary<string, AgentIdentity> Agents { get; } = new();
        public static ConcurrentDictionary<string, SystemMetrics> LatestMetrics { get; } = new();
        public static ConcurrentDictionary<string, ConcurrentQueue<CommandRequest>> PendingCommands { get; } = new();
        public static ConcurrentDictionary<string, CommandResponse> CommandResults { get; } = new();
        public static ConcurrentDictionary<string, object> EnhancedData { get; } = new();
        public static ConcurrentDictionary<string, NetworkPortSnapshot> LatestNetworkPortSnapshots { get; } = new();
        public static ConcurrentDictionary<string, ConcurrentQueue<NetworkPortSnapshot>> NetworkPortHistory { get; } = new();
        public static ConcurrentDictionary<string, List<object>> InstalledSoftware { get; } = new();

        public static AgentIdentity GetOrAddAgent(string agentId)
        {
            return Agents.GetOrAdd(agentId, id => new AgentIdentity { Id = id, LastHeartbeat = DateTime.UtcNow });
        }

        private const int MaxSnapshotsPerAgent = 50;

        public static NetworkPortSnapshot SaveNetworkPortSnapshot(NetworkPortSnapshot snapshot)
        {
            LatestNetworkPortSnapshots.AddOrUpdate(snapshot.AgentId, snapshot,
                (_, existing) => snapshot.Timestamp >= existing.Timestamp ? snapshot : existing);

            var history = NetworkPortHistory.GetOrAdd(snapshot.AgentId, _ => new ConcurrentQueue<NetworkPortSnapshot>());
            history.Enqueue(snapshot);

            while (history.Count > MaxSnapshotsPerAgent && history.TryDequeue(out _))
            {
                // Trim oldest snapshots to prevent unbounded growth
            }

            return snapshot;
        }

        public static NetworkPortSnapshot? GetLatestNetworkPortSnapshot(string agentId)
        {
            return LatestNetworkPortSnapshots.TryGetValue(agentId, out var snapshot) ? snapshot : null;
        }

        public static IReadOnlyCollection<NetworkPortSnapshot> GetNetworkPortSnapshots(string agentId)
        {
            if (NetworkPortHistory.TryGetValue(agentId, out var history))
            {
                var items = history.ToArray();
                Array.Sort(items, (a, b) => b.Timestamp.CompareTo(a.Timestamp));
                return items;
            }

            return Array.Empty<NetworkPortSnapshot>();
        }

        /// <summary>
        /// Attempts to find an existing agent ID by MAC address (and optionally machine name)
        /// </summary>
        public static string? FindAgentIdByMac(string? macAddress, string? machineName = null)
        {
            if (string.IsNullOrWhiteSpace(macAddress)) return null;
            foreach (var kvp in Agents)
            {
                var a = kvp.Value;
                if (string.Equals(a.MacAddress, macAddress, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(machineName) || string.Equals(a.MachineName, machineName, StringComparison.OrdinalIgnoreCase))
                        return kvp.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Attempts to find an existing agent ID by machine name only (useful when MAC is missing/zero)
        /// </summary>
        public static string? FindAgentIdByMachine(string? machineName)
        {
            if (string.IsNullOrWhiteSpace(machineName)) return null;
            foreach (var kvp in Agents)
            {
                var a = kvp.Value;
                if (string.Equals(a.MachineName, machineName, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>
        /// Updates identity fields if the provided values are not null/empty
        /// </summary>
        public static void UpdateIdentityFields(string agentId, string? machineName, string? ipAddress, string? macAddress, string? operatingSystem, string? location = null)
        {
            Agents.AddOrUpdate(agentId,
                id => new AgentIdentity
                {
                    Id = id,
                    MachineName = machineName ?? Environment.MachineName,
                    IpAddress = ipAddress ?? "127.0.0.1",
                    MacAddress = macAddress ?? "00:00:00:00:00:00",
                    OperatingSystem = operatingSystem ?? Environment.OSVersion.ToString(),
                    LastHeartbeat = DateTime.UtcNow,
                    Location = location
                },
                (id, existing) =>
                {
                    if (!string.IsNullOrWhiteSpace(machineName)) existing.MachineName = machineName;
                    if (!string.IsNullOrWhiteSpace(ipAddress)) existing.IpAddress = ipAddress;
                    if (!string.IsNullOrWhiteSpace(macAddress)) existing.MacAddress = macAddress;
                    if (!string.IsNullOrWhiteSpace(operatingSystem)) existing.OperatingSystem = operatingSystem;
                    if (!string.IsNullOrWhiteSpace(location)) existing.Location = location;
                    existing.LastHeartbeat = DateTime.UtcNow;
                    return existing;
                });
        }

        // Create or update an agent identity with provided fields
        public static AgentIdentity CreateOrUpdateAgent(string agentId, string? machineName, string? ipAddress, string? macAddress, string? operatingSystem, string? location = null)
        {
            return Agents.AddOrUpdate(
                agentId,
                id => new AgentIdentity
                {
                    Id = id,
                    MachineName = machineName ?? Environment.MachineName,
                    IpAddress = ipAddress ?? "127.0.0.1",
                    MacAddress = macAddress ?? "00:00:00:00:00:00",
                    OperatingSystem = operatingSystem ?? Environment.OSVersion.ToString(),
                    LastHeartbeat = DateTime.UtcNow,
                    Location = location
                },
                (id, existing) =>
                {
                    if (!string.IsNullOrWhiteSpace(machineName)) existing.MachineName = machineName;
                    if (!string.IsNullOrWhiteSpace(ipAddress)) existing.IpAddress = ipAddress;
                    if (!string.IsNullOrWhiteSpace(macAddress)) existing.MacAddress = macAddress;
                    if (!string.IsNullOrWhiteSpace(operatingSystem)) existing.OperatingSystem = operatingSystem;
                    if (!string.IsNullOrWhiteSpace(location)) existing.Location = location;
                    existing.LastHeartbeat = DateTime.UtcNow;
                    return existing;
                }
            );
        }
    }
}
