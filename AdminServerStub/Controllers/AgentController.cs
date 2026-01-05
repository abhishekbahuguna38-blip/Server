using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdminServerStub.Infrastructure;
using AdminServerStub.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminServerStub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        // Registration payload sent by real agent
        public class RegisterRequest
        {
            public string? MachineName { get; set; }
            public string? IpAddress { get; set; }
            public string? MacAddress { get; set; }
            public string? OperatingSystem { get; set; }
            public string? Location { get; set; }
        }

        [HttpPost("register")]
        public ActionResult<object> Register([FromBody] RegisterRequest body)
        {
            // Read identity overrides from headers if present
            Request.Headers.TryGetValue("X-Mac-Address", out var macHeader);
            Request.Headers.TryGetValue("X-Ip-Address", out var ipHeader);
            Request.Headers.TryGetValue("X-Machine-Name", out var machineHeader);
            Request.Headers.TryGetValue("X-Location", out var locationHeader);

            var mac = string.IsNullOrWhiteSpace(macHeader) ? body?.MacAddress : macHeader.ToString();
            var ip = string.IsNullOrWhiteSpace(ipHeader) ? body?.IpAddress : ipHeader.ToString();
            var machine = string.IsNullOrWhiteSpace(machineHeader) ? body?.MachineName : machineHeader.ToString();
            var location = string.IsNullOrWhiteSpace(locationHeader) ? body?.Location : locationHeader.ToString();

            // Normalize zero MAC to null for de-duplication
            if (!string.IsNullOrWhiteSpace(mac) && mac.Replace(":", "").Replace("-", "").Trim('0').Length == 0)
                mac = null;

            // Try to reuse existing agent: by MAC first, then by MachineName
            var existingId = InMemoryStore.FindAgentIdByMac(mac, machine) ?? InMemoryStore.FindAgentIdByMachine(machine);
            var agentId = string.IsNullOrEmpty(existingId) ? Guid.NewGuid().ToString() : existingId;
            var identity = InMemoryStore.CreateOrUpdateAgent(
                agentId,
                machine ?? Environment.MachineName,
                ip ?? "127.0.0.1",
                mac ?? "00:00:00:00:00:00",
                body?.OperatingSystem ?? Environment.OSVersion.ToString(),
                location);

            return Ok(new { agentId, configuration = (object?)null, token = string.Empty });
        }

        [HttpPost("metrics")]
        public ActionResult SubmitMetrics([FromBody] SystemMetrics metrics)
        {
            if (metrics == null || string.IsNullOrWhiteSpace(metrics.AgentId))
                return BadRequest("agentId required");
            // Try to capture identity from headers
            Request.Headers.TryGetValue("X-Mac-Address", out var macHeader);
            Request.Headers.TryGetValue("X-Ip-Address", out var ipHeader);
            Request.Headers.TryGetValue("X-Machine-Name", out var machineHeader);
            Request.Headers.TryGetValue("X-Location", out var locationHeader);

            // Update identity fields (if present) and touch heartbeat
            InMemoryStore.UpdateIdentityFields(metrics.AgentId, machineHeader, ipHeader, macHeader, null, locationHeader);
            InMemoryStore.LatestMetrics[metrics.AgentId] = metrics;
            return Ok(new { success = true });
        }

        [HttpPost("data")]
        public ActionResult SubmitData([FromBody] object data)
        {
            return Ok(new { success = true });
        }

        [HttpPost("heartbeat/{agentId}")]
        public ActionResult Heartbeat([FromRoute] string agentId)
        {
            // Capture identity info from headers if available
            Request.Headers.TryGetValue("X-Mac-Address", out var macHeader);
            Request.Headers.TryGetValue("X-Ip-Address", out var ipHeader);
            Request.Headers.TryGetValue("X-Machine-Name", out var machineHeader);
            Request.Headers.TryGetValue("X-Location", out var locationHeader);

            InMemoryStore.UpdateIdentityFields(agentId, machineHeader, ipHeader, macHeader, null, locationHeader);
            return Ok(new { success = true, agentId });
        }
    }
}
