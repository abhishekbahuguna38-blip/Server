using System;
using System.Collections.Generic;
using AdminServerStub.Infrastructure;
using AdminServerStub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdminServerStub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NetworkPortController : ControllerBase
    {
        private readonly ILogger<NetworkPortController> _logger;

        public NetworkPortController(ILogger<NetworkPortController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult Submit([FromBody] NetworkPortData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.AgentId))
            {
                _logger.LogWarning("Network port submission rejected because agentId was missing");
                return BadRequest("agentId required");
            }

            var snapshot = new NetworkPortSnapshot
            {
                AgentId = data.AgentId,
                Timestamp = data.Timestamp == default ? DateTime.UtcNow : data.Timestamp,
                Connections = data.Connections ?? new List<NetworkPortConnection>()
            };

            InMemoryStore.SaveNetworkPortSnapshot(snapshot);
            InMemoryStore.UpdateIdentityFields(snapshot.AgentId, null, null, null, null, null);

            _logger.LogInformation("Stored network port snapshot for agent {AgentId} with {ConnectionCount} connections at {Timestamp}",
                snapshot.AgentId,
                snapshot.Connections?.Count ?? 0,
                snapshot.Timestamp);

            return Ok(new { success = true });
        }

        [HttpGet("{agentId}/latest")]
        public ActionResult<NetworkPortSnapshot> GetLatest(string agentId)
        {
            var snapshot = InMemoryStore.GetLatestNetworkPortSnapshot(agentId);
            if (snapshot == null)
            {
                _logger.LogDebug("No network port snapshot found for agent {AgentId}", agentId);
                return NotFound();
            }

            return Ok(snapshot);
        }

        [HttpGet("{agentId}")]
        public ActionResult<IEnumerable<NetworkPortSnapshot>> GetHistory(string agentId)
        {
            var snapshots = InMemoryStore.GetNetworkPortSnapshots(agentId);
            if (snapshots.Count == 0)
            {
                _logger.LogDebug("No network port history stored for agent {AgentId}", agentId);
                return Ok(Array.Empty<NetworkPortSnapshot>());
            }

            return Ok(snapshots);
        }
    }
}
