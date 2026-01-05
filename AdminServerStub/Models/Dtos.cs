using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AdminServerStub.Models
{
    public class AgentIdentity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MachineName { get; set; } = Environment.MachineName;
        public string IpAddress { get; set; } = "127.0.0.1";
        public string MacAddress { get; set; } = "00:00:00:00:00:00";
        public string OperatingSystem { get; set; } = Environment.OSVersion.ToString();
        public DateTime? LastHeartbeat { get; set; } = DateTime.UtcNow;
        public string? Location { get; set; }
    }

    public class ProcessInfo
    {
        public required string Name { get; set; }
        public int Id { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
    }

    public class SystemMetrics
    {
        [JsonPropertyName("agentId")] public required string AgentId { get; set; }
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
        [JsonPropertyName("cpuUsage")] public double CpuUsage { get; set; }
        [JsonPropertyName("memoryUsage")] public double MemoryUsage { get; set; }
        [JsonPropertyName("diskUsage")] public double DiskUsage { get; set; }
        [JsonPropertyName("networkSent")] public long NetworkSent { get; set; }
        [JsonPropertyName("networkReceived")] public long NetworkReceived { get; set; }
        [JsonPropertyName("topProcesses")] public List<ProcessInfo> TopProcesses { get; set; } = new();
    }

    public class CommandRequest
    {
        [JsonPropertyName("commandId")] public required string CommandId { get; set; }
        [JsonPropertyName("targetAgentId")] public required string TargetAgentId { get; set; }
        [JsonPropertyName("commandType")] public string CommandType { get; set; } = string.Empty;
        [JsonPropertyName("parameters")] public Dictionary<string, string> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("priority")] public int Priority { get; set; }
        [JsonPropertyName("timeoutSeconds")] public int TimeoutSeconds { get; set; }
        [JsonPropertyName("requireConfirmation")] public bool RequireConfirmation { get; set; }
    }

    public class CommandResponse
    {
        [JsonPropertyName("commandId")] public required string CommandId { get; set; }
        [JsonPropertyName("agentId")] public required string AgentId { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; } = "Completed";
        [JsonPropertyName("startTime")] public DateTime StartTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("endTime")] public DateTime EndTime { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("executionTimeMs")] public long ExecutionTimeMs { get; set; }
        [JsonPropertyName("output")] public string Output { get; set; } = string.Empty;
        [JsonPropertyName("errorOutput")] public string ErrorOutput { get; set; } = string.Empty;
        [JsonPropertyName("exitCode")] public int ExitCode { get; set; } = 0;
    }

    public class InstalledSoftwareData
    {
        public required string AgentId { get; set; }
        // Timestamp when the data was collected (provided by agent)
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<object> SoftwareList { get; set; } = new();
    }

    /// <summary>
    /// DTO returned by the AdminServerStub for latest installed software.
    /// Matches what the Admin client expects: agentId, timestamp, softwareList.
    /// </summary>
    public class InstalledSoftwareInfoDto
    {
        [JsonPropertyName("agentId")] public required string AgentId { get; set; }
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("softwareList")] public List<object> SoftwareList { get; set; } = new();
    }

    public class NetworkPortConnection
    {
        [JsonPropertyName("localEndpoint")] public string? LocalEndpoint { get; set; }
        [JsonPropertyName("remoteEndpoint")] public string? RemoteEndpoint { get; set; }
        [JsonPropertyName("localPort")] public int? LocalPort { get; set; }
        [JsonPropertyName("remotePort")] public int? RemotePort { get; set; }
        [JsonPropertyName("processId")] public int? ProcessId { get; set; }
        [JsonPropertyName("processName")] public string? ProcessName { get; set; }
        [JsonPropertyName("state")] public string? State { get; set; }
        [JsonPropertyName("protocol")] public string? Protocol { get; set; }
    }

    public class NetworkPortData
    {
        [JsonPropertyName("agentId")] public required string AgentId { get; set; }
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        [JsonPropertyName("connections")] public List<NetworkPortConnection> Connections { get; set; } = new();
    }

    public class NetworkPortSnapshot
    {
        [JsonPropertyName("agentId")] public required string AgentId { get; set; }
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
        [JsonPropertyName("connections")] public List<NetworkPortConnection> Connections { get; set; } = new();
    }
}
