using MarsBridge.Server.Models;

namespace MarsBridge.Server.Services;

public class PlayerService
{
    private readonly List<PlayerInfo> _registeredPlayers = new();
    private readonly Dictionary<string, DateTime> _lastActivity = new();

    public async Task<PlayerInfo> RegisterPlayerAsync(string playerId, string playerName, string gameType)
    {
        await Task.Delay(1); // Simulate async operation

        var existingPlayer = _registeredPlayers.FirstOrDefault(p => p.PlayerId == playerId);
        if (existingPlayer != null)
        {
            existingPlayer.ConnectedAt = DateTime.UtcNow;
            _lastActivity[playerId] = DateTime.UtcNow;
            return existingPlayer;
        }

        var newPlayer = new PlayerInfo
        {
            PlayerId = playerId,
            PlayerName = playerName,
            GameType = gameType,
            ConnectedAt = DateTime.UtcNow
        };

        _registeredPlayers.Add(newPlayer);
        _lastActivity[playerId] = DateTime.UtcNow;

        return newPlayer;
    }

    public async Task<List<PlayerInfo>> GetActivePlayersAsync()
    {
        await Task.Delay(1); // Simulate async operation

        var cutoff = DateTime.UtcNow.AddMinutes(-5); // Consider players inactive after 5 minutes
        return _registeredPlayers
            .Where(p => _lastActivity.GetValueOrDefault(p.PlayerId, DateTime.MinValue) > cutoff)
            .ToList();
    }

    public async Task UpdatePlayerActivityAsync(string playerId)
    {
        await Task.Delay(1); // Simulate async operation
        _lastActivity[playerId] = DateTime.UtcNow;
    }

    public async Task<PlayerInfo?> GetPlayerAsync(string playerId)
    {
        await Task.Delay(1); // Simulate async operation
        return _registeredPlayers.FirstOrDefault(p => p.PlayerId == playerId);
    }

    public async Task RemovePlayerAsync(string playerId)
    {
        await Task.Delay(1); // Simulate async operation
        
        var player = _registeredPlayers.FirstOrDefault(p => p.PlayerId == playerId);
        if (player != null)
        {
            _registeredPlayers.Remove(player);
            _lastActivity.Remove(playerId);
        }
    }

    public async Task<Dictionary<string, int>> GetPlayerStatsByGameAsync()
    {
        await Task.Delay(1); // Simulate async operation

        var activePlayersCutoff = DateTime.UtcNow.AddMinutes(-5);
        var activePlayers = _registeredPlayers
            .Where(p => _lastActivity.GetValueOrDefault(p.PlayerId, DateTime.MinValue) > activePlayersCutoff);

        return activePlayers
            .GroupBy(p => p.GameType)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}