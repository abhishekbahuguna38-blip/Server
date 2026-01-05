using System;
using System.Collections.Generic;
using System.Linq;
using AdminServerStub.Controllers;
using AdminServerStub.Infrastructure;
using AdminServerStub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AdminServerStub.Tests.Controllers;

public class NetworkPortAndCommandControllerTests
{
    [Fact]
    public void GetLatest_WhenNoSnapshot_ReturnsNotFound()
    {
        ResetInMemoryStore();
        var controller = new NetworkPortController(NullLogger<NetworkPortController>.Instance);

        var result = controller.GetLatest("missing-agent");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void SubmitNetworkPortData_StoresSnapshotAndReturnsLatest()
    {
        ResetInMemoryStore();
        var controller = new NetworkPortController(NullLogger<NetworkPortController>.Instance);
        var timestamp = new DateTime(2024, 12, 5, 10, 30, 0, DateTimeKind.Utc);

        var submitResult = controller.Submit(new NetworkPortData
        {
            AgentId = "agent-1",
            Timestamp = timestamp,
            Connections =
            {
                new NetworkPortConnection
                {
                    LocalEndpoint = "127.0.0.1",
                    LocalPort = 8080,
                    RemoteEndpoint = "192.168.1.10",
                    RemotePort = 9000,
                    ProcessId = 1234,
                    ProcessName = "dotnet",
                    Protocol = "TCP",
                    State = "Established"
                }
            }
        });

        var okSubmit = Assert.IsType<OkObjectResult>(submitResult);
        Assert.True(GetProperty<bool>(okSubmit.Value!, "success"));

        var latestResult = controller.GetLatest("agent-1");
        var latestSnapshot = Assert.IsType<NetworkPortSnapshot>(latestResult.Value);
        Assert.Equal(timestamp, latestSnapshot.Timestamp);
        var connection = Assert.Single(latestSnapshot.Connections);
        Assert.Equal(1234, connection.ProcessId);
        Assert.Equal("dotnet", connection.ProcessName);
        Assert.Equal(8080, connection.LocalPort);

        var historyResult = controller.GetHistory("agent-1");
        var history = Assert.IsAssignableFrom<IEnumerable<NetworkPortSnapshot>>(historyResult.Value);
        Assert.Single(history);
    }

    [Fact]
    public void CommandLifecycle_SupportsKillProcessFlow()
    {
        ResetInMemoryStore();
        var controller = new CommandController(NullLogger<CommandController>.Instance);

        var command = new CommandRequest
        {
            CommandId = Guid.NewGuid().ToString(),
            TargetAgentId = "agent-9",
            CommandType = "KillProcess",
            Parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["processId"] = "4711",
                ["command"] = "taskkill /PID 4711"
            }
        };

        var queueResult = controller.Queue(command);
        var okQueue = Assert.IsType<OkObjectResult>(queueResult.Result);
        Assert.Equal(command.CommandId, GetProperty<string>(okQueue.Value!, "commandId"));

        var pendingLookup = controller.GetById(command.CommandId);
        var accepted = Assert.IsType<AcceptedResult>(pendingLookup.Result);
        Assert.Equal("Pending", GetProperty<string>(accepted.Value!, "status"));

        var pendingForAgent = controller.GetPending(command.TargetAgentId);
        var pendingCommands = Assert.IsAssignableFrom<IEnumerable<CommandRequest>>(pendingForAgent.Value);
        var deliveredCommand = Assert.Single(pendingCommands);
        Assert.Equal("KillProcess", deliveredCommand.CommandType);
        Assert.Equal("4711", deliveredCommand.Parameters["processId"]);
        Assert.Equal("taskkill /PID 4711", deliveredCommand.Parameters["command"]);

        var response = new CommandResponse
        {
            CommandId = command.CommandId,
            AgentId = command.TargetAgentId,
            Status = "Completed",
            Output = "Process terminated",
            ErrorOutput = string.Empty,
            ExecutionTimeMs = 150
        };

        var submitResult = controller.SubmitResult(response);
        var okSubmit = Assert.IsType<OkObjectResult>(submitResult);
        Assert.True(GetProperty<bool>(okSubmit.Value!, "success"));

        var storedResult = controller.GetById(command.CommandId);
        var okStored = Assert.IsType<OkObjectResult>(storedResult.Result);
        var storedResponse = Assert.IsType<CommandResponse>(okStored.Value);
        Assert.Equal("Completed", storedResponse.Status);
        Assert.Equal("Process terminated", storedResponse.Output);
        Assert.Equal(150, storedResponse.ExecutionTimeMs);
    }

    private static void ResetInMemoryStore()
    {
        InMemoryStore.Agents.Clear();
        InMemoryStore.LatestMetrics.Clear();
        InMemoryStore.PendingCommands.Clear();
        InMemoryStore.CommandResults.Clear();
        InMemoryStore.EnhancedData.Clear();
        InMemoryStore.LatestNetworkPortSnapshots.Clear();
        InMemoryStore.NetworkPortHistory.Clear();
        InMemoryStore.InstalledSoftware.Clear();
    }

    private static T GetProperty<T>(object instance, string propertyName)
    {
        var value = instance.GetType().GetProperty(propertyName)?.GetValue(instance, null);
        return value is T typed ? typed : default!;
    }
}
