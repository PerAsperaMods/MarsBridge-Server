using Microsoft.AspNetCore.Mvc;
using MarsBridge.Server.Services;
using MarsBridge.Server.Hubs;
using System.Diagnostics;

namespace MarsBridge.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly ClimateService _climateService;
    private static readonly PerformanceCounter? _cpuCounter;
    private static readonly PerformanceCounter? _ramCounter;

    static MonitoringController()
    {
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }
        catch
        {
            // Performance counters might not be available in containerized environments
        }
    }

    public MonitoringController(ClimateService climateService)
    {
        _climateService = climateService;
    }

    [HttpGet]
    [Route("/")]
    [Route("/monitor")]
    public IActionResult Index()
    {
        var html = GenerateMonitoringPage();
        return Content(html, "text/html");
    }

    [HttpGet("api")]
    public async Task<IActionResult> GetMonitoringData()
    {
        var process = Process.GetCurrentProcess();
        var atmosphere = await _climateService.GetAtmosphereAsync();

        var data = new
        {
            timestamp = DateTime.UtcNow,
            server = new
            {
                uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime(),
                cpuUsage = _cpuCounter?.NextValue() ?? 0,
                memoryUsage = process.WorkingSet64 / 1024 / 1024, // MB
                availableMemory = _ramCounter?.NextValue() ?? 0,
                threadCount = process.Threads.Count,
                handleCount = process.HandleCount
            },
            climate = new
            {
                temperature = atmosphere.Temperature,
                pressure = atmosphere.TotalPressure,
                gasCount = atmosphere.Gases.Count,
                lastUpdate = DateTime.UtcNow
            },
            connections = new
            {
                activeConnections = ClimateHub.ActiveConnections,
                totalMessages = ClimateHub.TotalMessages
            }
        };

        return Ok(data);
    }

    private string GenerateMonitoringPage()
    {
        return @"<!DOCTYPE html>
<html lang=""fr"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>🪐 MarsBridge Server Monitor</title>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%);
            color: #e8e8e8;
            min-height: 100vh;
        }

        .container {
            max-width: 1200px;
            margin: 0 auto;
            padding: 20px;
        }

        .header {
            text-align: center;
            margin-bottom: 30px;
            padding: 20px;
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            backdrop-filter: blur(10px);
        }

        .header h1 {
            font-size: 2.5em;
            margin-bottom: 10px;
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4, #45b7d1);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
        }

        .status-indicator {
            display: inline-block;
            width: 12px;
            height: 12px;
            border-radius: 50%;
            background: #4ecdc4;
            margin-left: 10px;
            animation: pulse 2s infinite;
        }

        .metrics-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
            gap: 20px;
            margin-bottom: 30px;
        }

        .metric-card {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            padding: 20px;
            backdrop-filter: blur(10px);
            border: 1px solid rgba(255, 255, 255, 0.2);
            transition: transform 0.3s ease;
        }

        .metric-card:hover {
            transform: translateY(-5px);
        }

        .metric-card h3 {
            color: #4ecdc4;
            margin-bottom: 15px;
            font-size: 1.2em;
            border-bottom: 2px solid #4ecdc4;
            padding-bottom: 5px;
        }

        .metric-item {
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
            padding: 8px 0;
            border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        }

        .metric-item:last-child {
            border-bottom: none;
        }

        .metric-label {
            font-weight: 500;
        }

        .metric-value {
            font-weight: bold;
            color: #ff6b6b;
        }

        .atmosphere-section {
            background: rgba(255, 255, 255, 0.1);
            border-radius: 15px;
            padding: 20px;
            margin-bottom: 20px;
            backdrop-filter: blur(10px);
        }

        .gas-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
            gap: 15px;
            margin-top: 15px;
        }

        .gas-item {
            background: rgba(255, 255, 255, 0.05);
            padding: 10px;
            border-radius: 8px;
            text-align: center;
        }

        .gas-symbol {
            font-size: 1.5em;
            font-weight: bold;
            color: #45b7d1;
        }

        .gas-percentage {
            font-size: 0.9em;
            color: #ff6b6b;
        }

        .gas-pressure {
            font-size: 0.8em;
            color: #a8a8a8;
        }

        .footer {
            text-align: center;
            margin-top: 30px;
            padding: 20px;
            background: rgba(255, 255, 255, 0.05);
            border-radius: 15px;
        }

        .refresh-btn {
            background: linear-gradient(45deg, #ff6b6b, #4ecdc4);
            border: none;
            color: white;
            padding: 12px 24px;
            border-radius: 25px;
            cursor: pointer;
            font-size: 1em;
            margin: 10px;
            transition: transform 0.2s ease;
        }

        .refresh-btn:hover {
            transform: scale(1.05);
        }

        @keyframes pulse {
            0% { opacity: 1; }
            50% { opacity: 0.5; }
            100% { opacity: 1; }
        }

        .loading {
            text-align: center;
            padding: 20px;
            color: #a8a8a8;
        }

        .error {
            background: rgba(255, 107, 107, 0.2);
            border: 1px solid #ff6b6b;
            border-radius: 8px;
            padding: 15px;
            margin: 10px 0;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>🪐 MarsBridge Server Monitor</h1>
            <p>Surveillance temps réel du serveur MarsBridge</p>
            <div class=""status-indicator"" id=""status-indicator""></div>
        </div>

        <div class=""metrics-grid"">
            <div class=""metric-card"">
                <h3>🖥️ Serveur</h3>
                <div id=""server-metrics"" class=""loading"">Chargement...</div>
            </div>

            <div class=""metric-card"">
                <h3>🌡️ Climat</h3>
                <div id=""climate-metrics"" class=""loading"">Chargement...</div>
            </div>

            <div class=""metric-card"">
                <h3>🔗 Connexions</h3>
                <div id=""connection-metrics"" class=""loading"">Chargement...</div>
            </div>
        </div>

        <div class=""atmosphere-section"">
            <h3>☁️ Composition Atmosphérique</h3>
            <div id=""atmosphere-composition"" class=""loading"">Chargement...</div>
        </div>

        <div class=""footer"">
            <button class=""refresh-btn"" onclick=""refreshData()"">🔄 Actualiser</button>
            <p>MarsBridge Server v1.0.0 | <a href=""/swagger"" style=""color: #4ecdc4;"">API Docs</a> | <a href=""/metrics"" style=""color: #4ecdc4;"">Prometheus</a></p>
            <div id=""last-update"" style=""margin-top: 10px; color: #a8a8a8; font-size: 0.9em;""></div>
        </div>
    </div>

    <script>
        let lastUpdate = Date.now();

        async function fetchMonitoringData() {
            try {
                const response = await fetch('/monitoring/api');
                if (!response.ok) throw new Error('Erreur HTTP: ' + response.status);
                return await response.json();
            } catch (error) {
                console.error('Erreur lors de la récupération des données:', error);
                return null;
            }
        }

        function formatBytes(bytes) {
            if (bytes === 0) return '0 B';
            const k = 1024;
            const sizes = ['B', 'KB', 'MB', 'GB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
        }

        function formatUptime(seconds) {
            const days = Math.floor(seconds / 86400);
            const hours = Math.floor((seconds % 86400) / 3600);
            const minutes = Math.floor((seconds % 3600) / 60);
            const secs = Math.floor(seconds % 60);

            if (days > 0) return `${days}j ${hours}h ${minutes}m`;
            if (hours > 0) return `${hours}h ${minutes}m ${secs}s`;
            if (minutes > 0) return `${minutes}m ${secs}s`;
            return `${secs}s`;
        }

        function updateServerMetrics(data) {
            const server = data.server;
            document.getElementById('server-metrics').innerHTML = `
                <div class='metric-item'>
                    <span class='metric-label'>⏱️ Uptime</span>
                    <span class='metric-value'>${formatUptime(server.uptime.totalSeconds)}</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>🖥️ CPU</span>
                    <span class='metric-value'>${server.cpuUsage.toFixed(1)}%</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>💾 Mémoire</span>
                    <span class='metric-value'>${server.memoryUsage} MB</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>🧵 Threads</span>
                    <span class='metric-value'>${server.threadCount}</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>🔗 Handles</span>
                    <span class='metric-value'>${server.handleCount}</span>
                </div>
            `;
        }

        function updateClimateMetrics(data) {
            const climate = data.climate;
            document.getElementById('climate-metrics').innerHTML = `
                <div class='metric-item'>
                    <span class='metric-label'>🌡️ Température</span>
                    <span class='metric-value'>${climate.temperature}°C</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>📊 Pression</span>
                    <span class='metric-value'>${climate.pressure.toFixed(4)} atm</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>☁️ Gaz détectés</span>
                    <span class='metric-value'>${climate.gasCount}</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>🕒 Dernière MAJ</span>
                    <span class='metric-value'>${new Date(climate.lastUpdate).toLocaleTimeString()}</span>
                </div>
            `;
        }

        function updateConnectionMetrics(data) {
            const connections = data.connections;
            document.getElementById('connection-metrics').innerHTML = `
                <div class='metric-item'>
                    <span class='metric-label'>🔗 Connexions actives</span>
                    <span class='metric-value'>${connections.activeConnections}</span>
                </div>
                <div class='metric-item'>
                    <span class='metric-label'>💬 Messages totaux</span>
                    <span class='metric-value'>${connections.totalMessages}</span>
                </div>
            `;
        }

        function updateAtmosphereComposition(data) {
            // Pour l'instant, on utilise des données statiques car l'API ne retourne pas encore la composition détaillée
            const gases = [
                { symbol: 'CO₂', percentage: 95.0, pressure: 0.0057 },
                { symbol: 'N₂', percentage: 2.7, pressure: 0.000162 },
                { symbol: 'Ar', percentage: 1.6, pressure: 0.000096 },
                { symbol: 'O₂', percentage: 0.13, pressure: 0.0000078 }
            ];

            document.getElementById('atmosphere-composition').innerHTML = `
                <div class='gas-grid'>
                    ${gases.map(gas => `
                        <div class='gas-item'>
                            <div class='gas-symbol'>${gas.symbol}</div>
                            <div class='gas-percentage'>${gas.percentage}%</div>
                            <div class='gas-pressure'>${gas.pressure.toFixed(7)} atm</div>
                        </div>
                    `).join('')}
                </div>
            `;
        }

        async function updateDashboard() {
            const data = await fetchMonitoringData();
            if (!data) {
                document.querySelectorAll('.loading').forEach(el => {
                    el.innerHTML = '<div class=""error"">❌ Erreur de connexion au serveur</div>';
                });
                document.getElementById('status-indicator').style.background = '#ff6b6b';
                return;
            }

            document.getElementById('status-indicator').style.background = '#4ecdc4';

            updateServerMetrics(data);
            updateClimateMetrics(data);
            updateConnectionMetrics(data);
            updateAtmosphereComposition(data);

            lastUpdate = Date.now();
            document.getElementById('last-update').textContent = 'Dernière mise à jour: ' + new Date().toLocaleString();
        }

        function refreshData() {
            updateDashboard();
        }

        // Mise à jour automatique toutes les 30 secondes
        setInterval(updateDashboard, 30000);

        // Chargement initial
        updateDashboard();
    </script>
</body>
</html>";
    }
}