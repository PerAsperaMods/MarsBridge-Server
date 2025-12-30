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
}