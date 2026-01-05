using System;
using System.Collections.Generic;
using System.Linq;
using AdminServerStub.Infrastructure;
using AdminServerStub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AdminServerStub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly ILogger<CommandController> _logger;

        public CommandController(ILogger<CommandController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<object> Execute([FromBody] CommandRequest command)
        {
            return QueueInternal(command);
        }

        [HttpPost("queue")]
        public ActionResult<object> Queue([FromBody] CommandRequest command)
        {
            return QueueInternal(command);
        }

        private ActionResult<object> QueueInternal(CommandRequest? command)
        {
            if (command == null)
            {
                _logger.LogWarning("Command queue request rejected because payload was null");
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(command.CommandId))
                command.CommandId = Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(command.TargetAgentId))
            {
                _logger.LogWarning("Command queue request rejected because targetAgentId was missing");
                return BadRequest("targetAgentId required");
            }

            command.Timestamp = command.Timestamp == default ? DateTime.UtcNow : command.Timestamp.ToUniversalTime();

            var queue = InMemoryStore.PendingCommands.GetOrAdd(command.TargetAgentId, _ => new());
            queue.Enqueue(command);

            InMemoryStore.CommandResults.AddOrUpdate(command.CommandId,
                _ => new CommandResponse
                {
                    CommandId = command.CommandId,
                    AgentId = command.TargetAgentId,
                    Status = "Pending",
                    StartTime = command.Timestamp,
                    EndTime = command.Timestamp,
                    ExecutionTimeMs = 0,
                    Output = string.Empty,
                    ErrorOutput = string.Empty,
                    ExitCode = 0
                },
                (_, existing) =>
                {
                    existing.AgentId = command.TargetAgentId;
                    existing.Status = "Pending";
                    existing.StartTime = command.Timestamp;
                    existing.EndTime = command.Timestamp;
                    existing.ExecutionTimeMs = 0;
                    existing.Output = string.Empty;
                    existing.ErrorOutput = string.Empty;
                    existing.ExitCode = 0;
                    return existing;
                });

            _logger.LogInformation("Queued command {CommandId} of type {CommandType} for agent {AgentId}",
                command.CommandId,
                string.IsNullOrWhiteSpace(command.CommandType) ? "(unspecified)" : command.CommandType,
                command.TargetAgentId);

            // Additional debug logging for logical port management commands
            var ct = command.CommandType;
            if (!string.IsNullOrWhiteSpace(ct) &&
                (string.Equals(ct, "OpenPort", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(ct, "ClosePort", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var paramSummary = command.Parameters != null && command.Parameters.Count > 0
                        ? string.Join(", ", command.Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"))
                        : "(no parameters)";
                    _logger.LogInformation("Port command {CommandId} ({CommandType}) parameters: {Parameters}",
                        command.CommandId,
                        ct,
                        paramSummary);
                }
                catch
                {
                    // Best-effort logging only
                }
            }

            return Ok(new { commandId = command.CommandId });
        }

        [HttpGet("pending/{agentId}")]
        public ActionResult<IEnumerable<CommandRequest>> GetPending(string agentId)
        {
            if (InMemoryStore.PendingCommands.TryGetValue(agentId, out var queue))
            {
                var list = new List<CommandRequest>();
                while (queue.TryDequeue(out var cmd))
                {
                    list.Add(cmd);
                }
                if (list.Count > 0)
                {
                    _logger.LogInformation("Delivered {CommandCount} pending commands to agent {AgentId}", list.Count, agentId);
                }
                return Ok(list);
            }
            return Ok(Array.Empty<CommandRequest>());
        }

        [HttpGet("{commandId}")]
        public ActionResult<CommandResponse> GetById(string commandId)
        {
            if (InMemoryStore.CommandResults.TryGetValue(commandId, out var result))
            {
                _logger.LogDebug("Returning stored result for command {CommandId} with status {Status}", commandId, result.Status);
                return Ok(result);
            }

            // No result yet - indicate pending if the command is still queued
            var isPending = InMemoryStore.PendingCommands.Values.Any(queue => queue.Any(cmd => string.Equals(cmd.CommandId, commandId, StringComparison.OrdinalIgnoreCase)));
            if (isPending)
                return Accepted(new { commandId, status = "Pending" });

            return NotFound();
        }

        [HttpPost("result")]
        public ActionResult SubmitResult([FromBody] CommandResponse response)
        {
            if (response == null || string.IsNullOrWhiteSpace(response.CommandId))
            {
                _logger.LogWarning("Command result submission rejected because commandId was missing");
                return BadRequest("commandId required");
            }

            if (string.IsNullOrWhiteSpace(response.AgentId))
            {
                _logger.LogWarning("Command result submission rejected because agentId was missing for command {CommandId}", response.CommandId);
                return BadRequest("agentId required");
            }

            if (string.IsNullOrWhiteSpace(response.Status))
            {
                // Derive status from exit code when not explicitly provided by the agent
                response.Status = response.ExitCode == 0 ? "Completed" : "Failed";
            }

            var endTime = response.EndTime == default ? DateTime.UtcNow : response.EndTime.ToUniversalTime();
            var startTime = response.StartTime == default ? endTime : response.StartTime.ToUniversalTime();
            response.StartTime = startTime;
            response.EndTime = endTime;
            if (response.ExecutionTimeMs <= 0)
            {
                response.ExecutionTimeMs = (long)Math.Max(0, (endTime - startTime).TotalMilliseconds);
            }

            InMemoryStore.CommandResults[response.CommandId] = response;

            _logger.LogInformation("Stored command result for {CommandId} with status {Status}", response.CommandId, response.Status);

            return Ok(new { success = true });
        }
    }
}
