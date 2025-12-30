using MarsBridge.Server.Models;
using Serilog;

namespace MarsBridge.Server.Services;

public class ClimateService
{
    private static ClimateData _currentClimate = new()
    {
        Temperature = -60f,  // Mars baseline
        Pressure = 0.006f,   // Mars baseline
        AtmosphericComposition = new List<ChemicalElement>
        {
            new("CO2", "gas", 95.0f, 0.0057f),
            new("N2", "gas", 2.7f, 0.000162f),
            new("Ar", "gas", 1.6f, 0.000096f),
            new("O2", "gas", 0.13f, 0.0000078f),
            new("CO", "gas", 0.08f, 0.0000048f),
            new("H2O", "gas", 0.03f, 0.0000018f),
            new("He", "gas", 0.01f, 0.0000006f)
        },
        TerraformingProgress = 0f
    };

    private readonly List<ClimateHistoryEntry> _history = new();

    public async Task<ClimateStatus> GetCurrentStatusAsync()
    {
        await Task.Delay(1); // Simulate async operation

        return new ClimateStatus
        {
            Temperature = _currentClimate.Temperature,
            Pressure = _currentClimate.Pressure,
            TerraformingProgress = _currentClimate.TerraformingProgress,
            PlayersOnline = GetConnectedPlayersCount(), // TODO: Implement from Hub
            LastUpdate = _currentClimate.Timestamp
        };
    }

    public async Task<AtmosphericComposition> GetAtmosphereAsync()
    {
        await Task.Delay(1); // Simulate async operation

        var atmosphere = new AtmosphericComposition
        {
            TotalPressure = _currentClimate.Pressure,
            Temperature = _currentClimate.Temperature,
            Gases = new Dictionary<string, GasInfo>()
        };

        foreach (var element in _currentClimate.AtmosphericComposition.Where(e => e.State == "gas"))
        {
            atmosphere.Gases[element.Symbol] = new GasInfo
            {
                Percentage = element.Percentage,
                PartialPressure = element.PartialPressure
            };
        }

        return atmosphere;
    }

    public async Task<AtmosphericComposition> GetCellAtmosphereAsync(string cellId)
    {
        await Task.Delay(1); // Simulate async operation

        // Parse cell coordinates (format: "latIndex_lonIndex")
        var parts = cellId.Split('_');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int latIndex) || !int.TryParse(parts[1], out int lonIndex))
        {
            throw new ArgumentException($"Invalid cell ID format: {cellId}. Expected format: latIndex_lonIndex");
        }

        // Calculate latitude from index (assuming 18 latitude bands)
        float latitude = (latIndex - 9) * 10f; // -90 to +90 degrees

        // Simulate atmospheric variation by latitude
        float tempVariation = Math.Abs(latitude) * 0.1f; // Cooler at poles
        float adjustedTemperature = _currentClimate.Temperature - tempVariation;

        var cellAtmosphere = new AtmosphericComposition
        {
            TotalPressure = _currentClimate.Pressure,
            Temperature = adjustedTemperature,
            Gases = new Dictionary<string, GasInfo>()
        };

        // Slight gas composition variations by latitude
        var co2Percentage = 95.0f + (latitude / 180f) * 2f; // More CO2 at poles
        var o2Percentage = 0.13f + (latitude / 180f) * 0.05f; // More O2 at equator

        cellAtmosphere.Gases["CO2"] = new GasInfo
        {
            Percentage = co2Percentage,
            PartialPressure = _currentClimate.Pressure * (co2Percentage / 100f)
        };

        cellAtmosphere.Gases["N2"] = new GasInfo
        {
            Percentage = 2.7f,
            PartialPressure = _currentClimate.Pressure * 0.027f
        };

        cellAtmosphere.Gases["Ar"] = new GasInfo
        {
            Percentage = 1.6f,
            PartialPressure = _currentClimate.Pressure * 0.016f
        };

        cellAtmosphere.Gases["O2"] = new GasInfo
        {
            Percentage = o2Percentage,
            PartialPressure = _currentClimate.Pressure * (o2Percentage / 100f)
        };

        return cellAtmosphere;
    }

    public async Task<string> ProcessCommandAsync(ClimateCommand command)
    {
        await Task.Delay(10); // Simulate processing

        Log.Information("🎯 Processing climate command: {Action} by {PlayerId}", command.Action, command.PlayerId);

        var oldClimate = new ClimateData
        {
            Temperature = _currentClimate.Temperature,
            Pressure = _currentClimate.Pressure,
            TerraformingProgress = _currentClimate.TerraformingProgress
        };

        // Process different command types
        switch (command.Action.ToUpper())
        {
            case "INCREASE_TEMPERATURE":
                _currentClimate.Temperature = Math.Min(_currentClimate.Temperature + (command.TargetValue * 0.1f), 30f);
                break;
                
            case "DECREASE_TEMPERATURE":
                _currentClimate.Temperature = Math.Max(_currentClimate.Temperature - (command.TargetValue * 0.1f), -80f);
                break;
                
            case "INCREASE_PRESSURE":
                _currentClimate.Pressure = Math.Min(_currentClimate.Pressure + (command.TargetValue * 0.01f), 1.0f);
                break;
                
            case "CONVERT_CO2":
                // Simulate CO2 conversion to O2
                var co2Element = _currentClimate.GetElement("CO2", "gas");
                var o2Element = _currentClimate.GetElement("O2", "gas");
                if (co2Element != null && o2Element != null)
                {
                    var co2Reduction = command.TargetValue * 0.01f;
                    var newCo2Percentage = Math.Max(co2Element.Percentage - co2Reduction, 10f);
                    var newO2Percentage = Math.Min(o2Element.Percentage + (co2Reduction * 0.5f), 25f);
                    
                    _currentClimate.SetElement("CO2", "gas", newCo2Percentage);
                    _currentClimate.SetElement("O2", "gas", newO2Percentage);
                }
                break;
                
            default:
                Log.Warning("⚠️ Unknown climate command: {Action}", command.Action);
                break;
        }

        // Update terraforming progress based on changes
        _currentClimate.TerraformingProgress = CalculateTerraformingProgress();
        _currentClimate.Timestamp = DateTime.UtcNow;

        // Add to history
        _history.Add(new ClimateHistoryEntry
        {
            Id = _history.Count + 1,
            Temperature = _currentClimate.Temperature,
            Pressure = _currentClimate.Pressure,
            TerraformingProgress = _currentClimate.TerraformingProgress,
            TriggerEvent = command.Action,
            PlayerId = command.PlayerId,
            Timestamp = DateTime.UtcNow
        });

        Log.Information("🌡️ Climate updated: Temp={Temperature}°C, Pressure={Pressure}atm, Progress={Progress}%",
            _currentClimate.Temperature, _currentClimate.Pressure, _currentClimate.TerraformingProgress);

        return "Command processed successfully";
    }

    public async Task<List<ClimateHistoryEntry>> GetHistoryAsync(DateTime from, DateTime to, int limit)
    {
        await Task.Delay(1); // Simulate async operation

        return _history
            .Where(h => h.Timestamp >= from && h.Timestamp <= to)
            .OrderByDescending(h => h.Timestamp)
            .Take(limit)
            .ToList();
    }

    public void UpdateClimateFromGame(ClimateData climateData)
    {
        _currentClimate = climateData;
        Log.Information("🎮 Climate data updated from game: Temp={Temperature}°C, Pressure={Pressure}atm",
            climateData.Temperature, climateData.Pressure);
    }

    private float CalculateTerraformingProgress()
    {
        // Simple terraforming calculation based on temperature, pressure, and O2
        float tempProgress = Math.Max(0, (_currentClimate.Temperature + 60) / 80f); // -60°C to +20°C
        float pressureProgress = Math.Min(1, _currentClimate.Pressure / 0.2f); // 0 to 0.2 atm
        float oxygenProgress = _currentClimate.GetElement("O2", "gas")?.Percentage ?? 0 / 20f; // 0% to 20%

        return Math.Min(100f, (tempProgress + pressureProgress + oxygenProgress) / 3f * 100f);
    }

    private int GetConnectedPlayersCount()
    {
        // TODO: Implement connection to SignalR Hub to get actual count
        return 0;
    }
}