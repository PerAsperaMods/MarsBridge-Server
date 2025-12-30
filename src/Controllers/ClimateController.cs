using Microsoft.AspNetCore.Mvc;
using MarsBridge.Server.Models;
using MarsBridge.Server.Services;
using Serilog;

namespace MarsBridge.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClimateController : ControllerBase
{
    private readonly ClimateService _climateService;

    public ClimateController(ClimateService climateService)
    {
        _climateService = climateService;
    }

    [HttpGet]
    [Route("/")]
    public IActionResult Index()
    {
        var html = @"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>🌍 MarsBridge Server</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background: #f5f5f5; }
        .container { max-width: 800px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; text-align: center; }
        .status { background: #27ae60; color: white; padding: 10px; border-radius: 5px; text-align: center; margin: 20px 0; }
        .endpoints { background: #ecf0f1; padding: 20px; border-radius: 5px; margin: 20px 0; }
        .endpoint { margin: 10px 0; }
        .endpoint a { color: #3498db; text-decoration: none; }
        .endpoint a:hover { text-decoration: underline; }
        .games { display: flex; justify-content: space-around; margin: 20px 0; }
        .game { background: #3498db; color: white; padding: 15px; border-radius: 5px; text-align: center; flex: 1; margin: 0 10px; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🌍 MarsBridge Server</h1>
        <p><strong>Cross-Game Climate Communication System</strong></p>
        
        <div class='status'>
            ✅ Server Status: RUNNING<br>
            📅 Last Update: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @" UTC
        </div>
        
        <div class='games'>
            <div class='game'>
                <h3>🚀 Per Aspera</h3>
                <p>Mars Terraforming</p>
            </div>
            <div class='game'>
                <h3>🏭 Satisfactory</h3>
                <p>Factory Automation</p>
            </div>
        </div>
        
        <h2>📡 Available Endpoints</h2>
        <div class='endpoints'>
            <div class='endpoint'><strong>Health Check:</strong> <a href='/health'>/health</a></div>
            <div class='endpoint'><strong>Climate Status:</strong> <a href='/api/climate/status'>/api/climate/status</a></div>
            <div class='endpoint'><strong>Atmosphere Data:</strong> <a href='/api/climate/atmosphere'>/api/climate/atmosphere</a></div>
            <div class='endpoint'><strong>SignalR Hub:</strong> /climatehub (WebSocket)</div>
            <div class='endpoint'><strong>Prometheus Metrics:</strong> <a href='/metrics'>/metrics</a></div>
        </div>
        
        <p><em>Real-time climate data exchange between Per Aspera and Satisfactory games.</em></p>
    </div>
</body>
</html>";
        
        return Content(html, "text/html");
    }
    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse<ClimateStatus>>> GetClimateStatus()
    {
        try
        {
            var status = await _climateService.GetCurrentStatusAsync();
            return Ok(new ApiResponse<ClimateStatus>
            {
                Success = true,
                Message = "Climate status retrieved successfully",
                Data = status
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error getting climate status");
            return StatusCode(500, new ApiResponse<ClimateStatus>
            {
                Success = false,
                Message = $"Error getting climate status: {ex.Message}"
            });
        }
    }

    [HttpGet("atmosphere")]
    public async Task<ActionResult<ApiResponse<AtmosphericComposition>>> GetAtmosphere()
    {
        try
        {
            var atmosphere = await _climateService.GetAtmosphereAsync();
            return Ok(new ApiResponse<AtmosphericComposition>
            {
                Success = true,
                Message = "Atmospheric data retrieved successfully",
                Data = atmosphere
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error getting atmospheric data");
            return StatusCode(500, new ApiResponse<AtmosphericComposition>
            {
                Success = false,
                Message = $"Error getting atmospheric data: {ex.Message}"
            });
        }
    }

    [HttpGet("atmosphere/cell/{cellId}")]
    public async Task<ActionResult<ApiResponse<AtmosphericComposition>>> GetCellAtmosphere(string cellId)
    {
        try
        {
            var atmosphere = await _climateService.GetCellAtmosphereAsync(cellId);
            return Ok(new ApiResponse<AtmosphericComposition>
            {
                Success = true,
                Message = $"Cell {cellId} atmospheric data retrieved successfully",
                Data = atmosphere
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error getting cell atmospheric data for {CellId}", cellId);
            return StatusCode(500, new ApiResponse<AtmosphericComposition>
            {
                Success = false,
                Message = $"Error getting cell atmospheric data: {ex.Message}"
            });
        }
    }

    [HttpPost("command")]
    public async Task<ActionResult<ApiResponse<string>>> ProcessClimateCommand([FromBody] ClimateCommand command)
    {
        try
        {
            if (command == null)
            {
                return BadRequest(new ApiResponse<string>
                {
                    Success = false,
                    Message = "Invalid command data"
                });
            }

            var result = await _climateService.ProcessCommandAsync(command);
            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = $"Climate command '{command.Action}' processed successfully",
                Data = result
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error processing climate command: {Action}", command?.Action);
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = $"Error processing climate command: {ex.Message}"
            });
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<ClimateHistoryEntry>>>> GetClimateHistory(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
            var toDate = to ?? DateTime.UtcNow;

            var history = await _climateService.GetHistoryAsync(fromDate, toDate, limit);
            return Ok(new ApiResponse<List<ClimateHistoryEntry>>
            {
                Success = true,
                Message = "Climate history retrieved successfully",
                Data = history
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error getting climate history");
            return StatusCode(500, new ApiResponse<List<ClimateHistoryEntry>>
            {
                Success = false,
                Message = $"Error getting climate history: {ex.Message}"
            });
        }
    }

    [HttpGet("ticktime")]
    public async Task<ActionResult<ApiResponse<float>>> GetTickTime()
    {
        try
        {
            var multiplier = await _climateService.GetTickTimeMultiplierAsync();
            return Ok(new ApiResponse<float>
            {
                Success = true,
                Message = "TickTime multiplier retrieved successfully",
                Data = multiplier
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error getting tickTime multiplier");
            return StatusCode(500, new ApiResponse<float>
            {
                Success = false,
                Message = $"Error getting tickTime multiplier: {ex.Message}"
            });
        }
    }

    [HttpPost("ticktime")]
    public async Task<ActionResult<ApiResponse<bool>>> SetTickTime([FromBody] TickTimeRequest request)
    {
        try
        {
            if (!request.IsValid)
            {
                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Invalid tickTime multiplier. Must be between 0.1 and 10.0"
                });
            }

            var success = await _climateService.SetTickTimeMultiplierAsync(request.Multiplier);
            if (success)
            {
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = $"TickTime multiplier set to {request.Multiplier}x successfully",
                    Data = true
                });
            }
            else
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to set tickTime multiplier"
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error setting tickTime multiplier");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = $"Error setting tickTime multiplier: {ex.Message}"
            });
        }
    }
}