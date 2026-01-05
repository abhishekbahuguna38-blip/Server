using System;
using System.Collections.Generic;
using System.Linq;
using AdminServerStub.Infrastructure;
using AdminServerStub.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminServerStub.Controllers
{
    [ApiController]
    [Route("api/Admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("agents")]
        public ActionResult<IEnumerable<AgentIdentity>> GetAllAgents([FromQuery] bool onlineOnly = false, [FromQuery] int minutes = 5)
        {
            var list = InMemoryStore.Agents.Values.ToList();
            if (onlineOnly)
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-Math.Abs(minutes));
                list = list.Where(a => a.LastHeartbeat.HasValue && a.LastHeartbeat.Value >= cutoff).ToList();
            }
            return Ok(list);
        }

        [HttpGet("agents/{agentId}")]
        public ActionResult<AgentIdentity> GetAgent(string agentId)
        {
            if (InMemoryStore.Agents.TryGetValue(agentId, out var agent))
                return Ok(agent);
            return NotFound();
        }

        [HttpGet("agents/{agentId}/metrics")]
        public ActionResult<SystemMetrics> GetAgentMetrics(string agentId)
        {
            if (InMemoryStore.LatestMetrics.TryGetValue(agentId, out var metrics))
                return Ok(metrics);
            // Return empty array sometimes as AdminApiService handles both array/object
            return Ok(Array.Empty<SystemMetrics>());
        }

        [HttpGet("agents/{agentId}/metrics/aggregated")]
        public ActionResult<object> GetAgentMetricsAggregated(string agentId)
        {
            return Ok(new { agentId, avgCpu = 0.0, avgMemory = 0.0 });
        }

        [HttpGet("agents/{agentId}/metrics/average")]
        public ActionResult<object> GetAgentMetricsAverage(string agentId)
        {
            return Ok(new { agentId, avgCpu = 0.0, avgMemory = 0.0 });
        }

        [HttpGet("agents/{agentId}/metrics/trend")]
        public ActionResult<object> GetAgentMetricsTrend(string agentId)
        {
            return Ok(new { agentId, points = new object[0] });
        }

        [HttpGet("agents/{agentId}/ports")]
        public ActionResult<IEnumerable<NetworkPortSnapshot>> GetAgentPorts(string agentId)
        {
            var snapshots = InMemoryStore.GetNetworkPortSnapshots(agentId);
            if (snapshots.Count == 0)
                return Ok(Array.Empty<NetworkPortSnapshot>());
            return Ok(snapshots);
        }

        [HttpGet("agents/{agentId}/ports/latest")]
        public ActionResult<NetworkPortSnapshot> GetAgentPortsLatest(string agentId)
        {
            var snapshot = InMemoryStore.GetLatestNetworkPortSnapshot(agentId);
            if (snapshot == null)
                return NotFound();
            return Ok(snapshot);
        }

        [HttpGet("ports")]
        public ActionResult<object> GetAllAgentPorts()
        {
            // Return latest port information for all agents
            var allPorts = InMemoryStore.LatestNetworkPortSnapshots.Values
                .GroupBy(s => s.AgentId)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        agentId = group.Key,
                        snapshot = group.OrderByDescending(s => s.Timestamp).FirstOrDefault()
                    });

            return Ok(allPorts);
        }

        [HttpGet("ports/summary")]
        public ActionResult<object> GetAllAgentPortsSummary()
        {
            // Return summary of port information for all agents with agent details
            var summary = InMemoryStore.Agents.Values.Select(agent =>
            {
                var latestSnapshot = InMemoryStore.GetLatestNetworkPortSnapshot(agent.Id);
                return new
                {
                    agentId = agent.Id,
                    machineName = agent.MachineName,
                    ipAddress = agent.IpAddress,
                    portCount = latestSnapshot?.Connections.Count ?? 0,
                    connections = latestSnapshot?.Connections ?? new List<NetworkPortConnection>()
                };
            }).ToList();

            return Ok(summary);
        }
    }
}
