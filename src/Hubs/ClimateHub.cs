using Microsoft.AspNetCore.SignalR;
using MarsBridge.Server.Models;
using Serilog;

namespace MarsBridge.Server.Hubs;

public class ClimateHub : Hub
{
    private static readonly Dictionary<string, PlayerInfo> ConnectedPlayers = new();
    
    // Propriétés statiques pour le monitoring
    public static int ActiveConnections => ConnectedPlayers.Count;
    public static long TotalMessages { get; private set; } = 0;

    public async Task RegisterPlayer(string playerId, string playerName, string gameType)
    {
        var playerInfo = new PlayerInfo
        {
            PlayerId = playerId,
            PlayerName = playerName,
            GameType = gameType, // "PerAspera" or "Satisfactory"
            ConnectionId = Context.ConnectionId,
            ConnectedAt = DateTime.UtcNow
        };

        ConnectedPlayers[Context.ConnectionId] = playerInfo;

        await Groups.AddToGroupAsync(Context.ConnectionId, gameType);
        
        Log.Information("🎮 Player {PlayerName} ({GameType}) connected with ID: {PlayerId}", 
            playerName, gameType, playerId);

        // Notify all clients about new player
        await Clients.All.SendAsync("PlayerConnected", playerInfo);
        TotalMessages++;
    }

    public async Task SendClimateData(ClimateData data)
    {
        if (ConnectedPlayers.TryGetValue(Context.ConnectionId, out var player))
        {
            Log.Information("🌡️ Climate data from {PlayerName}: Temp={Temperature}°C, Pressure={Pressure}atm", 
                player.PlayerName, data.Temperature, data.Pressure);

            // Forward to all Satisfactory clients
            await Clients.Group("Satisfactory").SendAsync("MarsClimateUpdate", data);
            TotalMessages++;
        }
    }

    public async Task SendResourceData(ResourceData data)
    {
        if (ConnectedPlayers.TryGetValue(Context.ConnectionId, out var player))
        {
            Log.Information("⚡ Resource data from {PlayerName}: Energy={Energy}, Iron={Iron}", 
                player.PlayerName, data.EnergyProduced, data.Resources.GetValueOrDefault("Iron", 0));

            // Forward to Per Aspera clients
            await Clients.Group("PerAspera").SendAsync("FactoryResourceUpdate", data);
            TotalMessages++;
        }
    }

    public async Task SendClimateCommand(ClimateCommand command)
    {
        if (ConnectedPlayers.TryGetValue(Context.ConnectionId, out var player))
        {
            Log.Information("🎯 Climate command from {PlayerName}: {Action} = {Value}", 
                player.PlayerName, command.Action, command.TargetValue);

            // Forward to Per Aspera clients
            await Clients.Group("PerAspera").SendAsync("ClimateCommandReceived", command);
            TotalMessages++;
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectedPlayers.TryGetValue(Context.ConnectionId, out var player))
        {
            ConnectedPlayers.Remove(Context.ConnectionId);
            
            Log.Information("👋 Player {PlayerName} ({GameType}) disconnected", 
                player.PlayerName, player.GameType);

            await Clients.All.SendAsync("PlayerDisconnected", player);
            TotalMessages++;
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Get connected players info
    public async Task GetConnectedPlayers()
    {
        await Clients.Caller.SendAsync("ConnectedPlayersInfo", ConnectedPlayers.Values);
        TotalMessages++;
    }
}