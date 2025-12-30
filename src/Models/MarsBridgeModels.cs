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

public class ChemicalElement
{
    public string Symbol { get; set; } = string.Empty; // "CO2", "H2O", "N2", "O2", etc.
    public string State { get; set; } = "gas"; // "gas", "liquid", "solid", "plasma"
    public float Percentage { get; set; } // 0-100
    public float PartialPressure { get; set; } // atm
    
    // Clé unique pour identifier l'élément dans les collections
    public string UniqueKey => $"{Symbol}_{State}";
    
    // Constructeur pratique
    public ChemicalElement(string symbol, string state, float percentage, float partialPressure = 0)
    {
        Symbol = symbol;
        State = state;
        Percentage = percentage;
        PartialPressure = partialPressure;
    }
}

public class ClimateData
{
    public float Temperature { get; set; }
    public float Pressure { get; set; }
    // Remplacement du Dictionary<string, float> par une liste d'éléments chimiques
    public List<ChemicalElement> AtmosphericComposition { get; set; } = new();
    public float TerraformingProgress { get; set; }
    public float TickTimeMultiplier { get; set; } = 1.0f; // Game speed multiplier
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Méthode helper pour obtenir un élément par symbole et état
    public ChemicalElement? GetElement(string symbol, string state = "gas")
    {
        return AtmosphericComposition.FirstOrDefault(e => e.Symbol == symbol && e.State == state);
    }
    
    // Méthode helper pour ajouter/modifier un élément
    public void SetElement(string symbol, string state, float percentage, float partialPressure = 0)
    {
        var existing = GetElement(symbol, state);
        if (existing != null)
        {
            existing.Percentage = percentage;
            existing.PartialPressure = partialPressure;
        }
        else
        {
            AtmosphericComposition.Add(new ChemicalElement(symbol, state, percentage, partialPressure));
        }
    }
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

public class TickTimeRequest
{
    public float Multiplier { get; set; }
    
    // Validation
    public bool IsValid => Multiplier >= 0.1f && Multiplier <= 10.0f;
}