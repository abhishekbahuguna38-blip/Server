using System;
using System.Collections.Generic;
using AdminServerStub.Infrastructure;
using AdminServerStub.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminServerStub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstalledSoftwareController : ControllerBase
    {
        [HttpPost]
        public ActionResult Submit([FromBody] InstalledSoftwareData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.AgentId))
                return BadRequest("agentId required");

            // Persist a normalized DTO the Admin client can consume
            var dto = new InstalledSoftwareInfoDto
            {
                AgentId = data.AgentId,
                Timestamp = data.Timestamp == default ? DateTime.UtcNow : data.Timestamp,
                SoftwareList = data.SoftwareList ?? new List<object>()
            };

            // Append to history per agent
            if (!InMemoryStore.InstalledSoftware.TryGetValue(data.AgentId, out var existing))
            {
                existing = new List<object>();
                InMemoryStore.InstalledSoftware[data.AgentId] = existing;
            }
            existing.Add(dto);
            InMemoryStore.GetOrAddAgent(data.AgentId).LastHeartbeat = DateTime.UtcNow;
            return Ok(new { success = true });
        }

        [HttpGet("{agentId}")]
        public ActionResult<IEnumerable<object>> Get(string agentId)
        {
            if (InMemoryStore.InstalledSoftware.TryGetValue(agentId, out var list))
                return Ok(list);
            return Ok(Array.Empty<object>());
        }

        [HttpGet("{agentId}/latest")]
        public ActionResult<IEnumerable<object>> GetLatest(string agentId)
        {
            if (InMemoryStore.InstalledSoftware.TryGetValue(agentId, out var list))
            {
                // Return the last item if we have multiple, else the only item
                if (list.Count > 0)
                    return Ok(list[^1]);
            }
            return Ok(Array.Empty<object>());
        }
    }
}
