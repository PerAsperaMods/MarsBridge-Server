namespace MarsBridge.Server.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class PlayerInfo
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty; // "PerAspera" or "Satisfactory"
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
}

public class ClimateData
{
    public float Temperature { get; set; }
    public float Pressure { get; set; }
    public Dictionary<string, float> AtmosphericComposition { get; set; } = new();
    public float TerraformingProgress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ResourceData
{
    public string PlayerId { get; set; } = string.Empty;
    public Dictionary<string, long> Resources { get; set; } = new();
    public long EnergyProduced { get; set; }
    public long EnergyConsumed { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ClimateCommand
{
    public string Action { get; set; } = string.Empty; // "INCREASE_TEMPERATURE", "CONVERT_CO2", etc.
    public float TargetValue { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public long EnergyCost { get; set; }
    public Dictionary<string, long> ResourceCost { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ClimateStatus
{
    public float Temperature { get; set; }
    public float Pressure { get; set; }
    public float TerraformingProgress { get; set; }
    public int PlayersOnline { get; set; }
    public DateTime LastUpdate { get; set; }
}

public class AtmosphericComposition
{
    public float TotalPressure { get; set; }
    public float Temperature { get; set; }
    public Dictionary<string, GasInfo> Gases { get; set; } = new();
}

public class GasInfo
{
    public float Percentage { get; set; }
    public float PartialPressure { get; set; }
}

public class ClimateHistoryEntry
{
    public int Id { get; set; }
    public float Temperature { get; set; }
    public float Pressure { get; set; }
    public float TerraformingProgress { get; set; }
    public string? TriggerEvent { get; set; }
    public string? PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
}