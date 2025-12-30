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
            <div class='endpoint'><strong>Time Control UI:</strong> <a href='/time-control'>/time-control</a></div>
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
                    Message = "Invalid tickTime multiplier. Must be between 0.0 and 10.0"
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

    [HttpGet]
    [Route("/time-control")]
    public IActionResult TimeControl()
    {
        var html = @"
<!DOCTYPE html>
<html lang='fr'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>⏰ MarsBridge - Contrôle Temporel</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1e3c72 0%, #2a5298 100%);
            color: #333;
            min-height: 100vh;
        }
        .container {
            max-width: 1000px;
            margin: 0 auto;
            padding: 20px;
        }
        .header {
            text-align: center;
            background: rgba(255, 255, 255, 0.95);
            padding: 30px;
            border-radius: 15px;
            margin-bottom: 30px;
            box-shadow: 0 8px 32px rgba(0,0,0,0.1);
        }
        .header h1 {
            color: #2c3e50;
            font-size: 2.5em;
            margin-bottom: 10px;
        }
        .header p {
            color: #7f8c8d;
            font-size: 1.1em;
        }

        .control-panel {
            background: rgba(255, 255, 255, 0.95);
            padding: 30px;
            border-radius: 15px;
            margin-bottom: 30px;
            box-shadow: 0 8px 32px rgba(0,0,0,0.1);
        }

        .current-speed {
            text-align: center;
            font-size: 1.5em;
            margin-bottom: 30px;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 10px;
            border: 2px solid #3498db;
        }

        .speed-display {
            font-size: 2em;
            font-weight: bold;
            color: #e74c3c;
        }

        .slider-container {
            margin: 30px 0;
        }

        .slider {
            width: 100%;
            height: 10px;
            border-radius: 5px;
            background: #ddd;
            outline: none;
            appearance: none;
            margin: 20px 0;
        }

        .slider::-webkit-slider-thumb {
            appearance: none;
            width: 25px;
            height: 25px;
            border-radius: 50%;
            background: #3498db;
            cursor: pointer;
            border: 3px solid #fff;
            box-shadow: 0 2px 6px rgba(0,0,0,0.2);
        }

        .slider::-moz-range-thumb {
            width: 25px;
            height: 25px;
            border-radius: 50%;
            background: #3498db;
            cursor: pointer;
            border: 3px solid #fff;
            box-shadow: 0 2px 6px rgba(0,0,0,0.2);
        }

        .presets {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 15px;
            margin: 30px 0;
        }

        .preset-btn {
            padding: 15px;
            border: none;
            border-radius: 8px;
            font-size: 1em;
            font-weight: bold;
            cursor: pointer;
            transition: all 0.3s ease;
            text-align: center;
        }

        .preset-btn:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(0,0,0,0.2);
        }

        .preset-pause { background: #e74c3c; color: white; }
        .preset-realistic { background: #27ae60; color: white; }
        .preset-normal { background: #3498db; color: white; }
        .preset-fast { background: #f39c12; color: white; }
        .preset-max { background: #9b59b6; color: white; }

        .custom-input {
            display: flex;
            gap: 10px;
            align-items: center;
            margin: 20px 0;
        }

        .custom-input input {
            flex: 1;
            padding: 10px;
            border: 2px solid #ddd;
            border-radius: 5px;
            font-size: 1em;
        }

        .custom-input button {
            padding: 10px 20px;
            background: #2ecc71;
            color: white;
            border: none;
            border-radius: 5px;
            cursor: pointer;
            font-weight: bold;
        }

        .status {
            padding: 15px;
            border-radius: 8px;
            margin: 20px 0;
            font-weight: bold;
        }

        .status.success { background: #d4edda; color: #155724; border: 1px solid #c3e6cb; }
        .status.error { background: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; }
        .status.info { background: #cce7ff; color: #004085; border: 1px solid #b3d7ff; }

        .info-panel {
            background: rgba(255, 255, 255, 0.9);
            padding: 20px;
            border-radius: 10px;
            margin-top: 20px;
        }

        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin: 20px 0;
        }

        .info-item {
            background: #f8f9fa;
            padding: 15px;
            border-radius: 8px;
            border-left: 4px solid #3498db;
        }

        .info-item h4 {
            margin-bottom: 8px;
            color: #2c3e50;
        }

        .info-item p {
            color: #7f8c8d;
            font-size: 0.9em;
        }

        .footer {
            text-align: center;
            margin-top: 30px;
            color: rgba(255, 255, 255, 0.8);
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏰ MarsBridge - Contrôle Temporel</h1>
            <p>Système de contrôle de la vitesse temporelle Per Aspera</p>
        </div>

        <div class='control-panel'>
            <div class='current-speed'>
                Vitesse actuelle: <span class='speed-display' id='currentSpeed'>Chargement...</span>
            </div>

            <div class='slider-container'>
                <label for='speedSlider'>Contrôle continu (0.0x - 10.0x):</label>
                <input type='range' id='speedSlider' class='slider' min='0' max='10' step='0.001' value='1'>
                <div style='display: flex; justify-content: space-between; font-size: 0.8em; color: #666;'>
                    <span>0.0x (Pause)</span>
                    <span id='sliderValue'>1.0x</span>
                    <span>10.0x (Ultra-rapide)</span>
                </div>
            </div>

            <div class='presets'>
                <button class='preset-btn preset-pause' onclick='setSpeed(0.0)'>⏸️ Pause</button>
                <button class='preset-btn preset-realistic' onclick='setSpeed(0.000277)'>🌍 Réaliste<br><small>1 sol/heure</small></button>
                <button class='preset-btn preset-normal' onclick='setSpeed(0.016)'>🎮 Normal<br><small>1 sol/minute</small></button>
                <button class='preset-btn preset-fast' onclick='setSpeed(1.0)'>🚀 Rapide<br><small>1 sol/seconde</small></button>
                <button class='preset-btn preset-max' onclick='setSpeed(10.0)'>⚡ Maximum<br><small>10 sols/seconde</small></button>
            </div>

            <div class='custom-input'>
                <input type='number' id='customSpeed' step='0.000001' min='0' max='10' placeholder='Valeur personnalisée (0.0 - 10.0)'>
                <button onclick='setCustomSpeed()'>🚀 Appliquer</button>
            </div>

            <div id='statusMessage'></div>
        </div>

        <div class='info-panel'>
            <h3>📚 Informations sur le système</h3>

            <div class='info-grid'>
                <div class='info-item'>
                    <h4>🌌 Base temporelle</h4>
                    <p>1x = 1 sol martien par seconde<br>1 sol = ~24h terrestres</p>
                </div>
                <div class='info-item'>
                    <h4>🎯 Contrôle principal</h4>
                    <p>Universe.gameSpeed<br>Priorité maximale dans le jeu</p>
                </div>
                <div class='info-item'>
                    <h4>📡 Communication</h4>
                    <p>SignalR temps réel<br>Mise à jour instantanée</p>
                </div>
                <div class='info-item'>
                    <h4>🔧 Validation</h4>
                    <p>0.0x - 10.0x<br>Protection contre les erreurs</p>
                </div>
            </div>

            <h4 style='margin-top: 20px;'>🎮 Préréglages utiles :</h4>
            <ul style='margin: 10px 0; padding-left: 20px;'>
                <li><strong>0.0x</strong> - Pause complète (planification)</li>
                <li><strong>0.000277x</strong> - Simulation réaliste (1 sol/heure)</li>
                <li><strong>0.016x</strong> - Jeu normal (1 sol/minute)</li>
                <li><strong>1.0x</strong> - Développement rapide</li>
                <li><strong>10.0x</strong> - Tests accélérés</li>
            </ul>
        </div>
    </div>

    <div class='footer'>
        <p>MarsBridge Server - Contrôle Temporel Per Aspera | " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @" UTC</p>
    </div>

    <script>
        let currentSpeed = 1.0;

        // Charger la vitesse actuelle au démarrage
        async function loadCurrentSpeed() {
            try {
                const response = await fetch('/api/climate/ticktime');
                const data = await response.json();
                if (data.success) {
                    currentSpeed = data.data;
                    updateDisplay();
                }
            } catch (error) {
                console.error('Erreur chargement vitesse:', error);
            }
        }

        // Mettre à jour l'affichage
        function updateDisplay() {
            document.getElementById('currentSpeed').textContent = currentSpeed.toFixed(6) + 'x';
            document.getElementById('speedSlider').value = currentSpeed;
            document.getElementById('sliderValue').textContent = currentSpeed.toFixed(3) + 'x';
        }

        // Définir une vitesse
        async function setSpeed(speed) {
            try {
                const response = await fetch('/api/climate/ticktime', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ multiplier: speed })
                });

                const data = await response.json();

                if (data.success) {
                    currentSpeed = speed;
                    updateDisplay();
                    showStatus('✅ Vitesse définie à ' + speed.toFixed(6) + 'x', 'success');
                } else {
                    showStatus('❌ Erreur: ' + data.message, 'error');
                }
            } catch (error) {
                showStatus('❌ Erreur de connexion', 'error');
                console.error('Erreur:', error);
            }
        }

        // Appliquer une vitesse personnalisée
        function setCustomSpeed() {
            const input = document.getElementById('customSpeed');
            const speed = parseFloat(input.value);
            if (isNaN(speed) || speed < 0 || speed > 10) {
                showStatus('❌ Valeur invalide (0.0 - 10.0)', 'error');
                return;
            }
            setSpeed(speed);
            input.value = '';
        }

        // Afficher un message de statut
        function showStatus(message, type) {
            const statusDiv = document.getElementById('statusMessage');
            statusDiv.className = 'status ' + type;
            statusDiv.textContent = message;
            setTimeout(() => {
                statusDiv.textContent = '';
                statusDiv.className = 'status';
            }, 5000);
        }

        // Gestionnaire du slider
        document.getElementById('speedSlider').addEventListener('input', function(e) {
            const speed = parseFloat(e.target.value);
            document.getElementById('sliderValue').textContent = speed.toFixed(3) + 'x';
        });

        document.getElementById('speedSlider').addEventListener('change', function(e) {
            const speed = parseFloat(e.target.value);
            setSpeed(speed);
        });

        // Gestionnaire pour la touche Entrée dans l'input personnalisé
        document.getElementById('customSpeed').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                setCustomSpeed();
            }
        });

        // Charger la vitesse au démarrage
        loadCurrentSpeed();

        // Actualiser périodiquement
        setInterval(loadCurrentSpeed, 5000);
    </script>
</body>
</html>";
        
        return Content(html, "text/html");
    }
}