using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Serilog;

namespace MarsBridge.Server.Services;

public class RabbitMQService : IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly string _connectionString;

    public RabbitMQService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672/";
        
        try
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_connectionString),
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchanges and queues
            SetupExchangesAndQueues();

            Log.Information("✅ RabbitMQ connected successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Failed to connect to RabbitMQ: {ConnectionString}", _connectionString);
        }
    }

    private void SetupExchangesAndQueues()
    {
        if (_channel == null) return;

        // Climate data exchange (Per Aspera → MarsBridge → Satisfactory)
        _channel.ExchangeDeclare("climate.data", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("climate.data.queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("climate.data.queue", "climate.data", "mars.climate.*");

        // Climate commands exchange (Satisfactory → MarsBridge → Per Aspera)  
        _channel.ExchangeDeclare("climate.commands", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("climate.commands.queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("climate.commands.queue", "climate.commands", "terraform.*");

        // Resource data exchange (Satisfactory → MarsBridge)
        _channel.ExchangeDeclare("resource.data", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("resource.data.queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("resource.data.queue", "resource.data", "factory.*");

        // System monitoring exchange
        _channel.ExchangeDeclare("system.monitoring", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("system.monitoring.queue", durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind("system.monitoring.queue", "system.monitoring", "health.*");

        Log.Information("🐰 RabbitMQ exchanges and queues configured");
    }

    public async Task PublishClimateDataAsync<T>(string routingKey, T data)
    {
        if (_channel == null)
        {
            Log.Warning("⚠️ RabbitMQ channel not available for publishing");
            return;
        }

        try
        {
            var message = JsonSerializer.Serialize(data);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.MessageId = Guid.NewGuid().ToString();

            _channel.BasicPublish(
                exchange: "climate.data",
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );

            Log.Debug("📤 Published message to climate.data: {RoutingKey}", routingKey);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Failed to publish climate data: {RoutingKey}", routingKey);
        }
    }

    public async Task PublishClimateCommandAsync<T>(string routingKey, T command)
    {
        if (_channel == null)
        {
            Log.Warning("⚠️ RabbitMQ channel not available for publishing");
            return;
        }

        try
        {
            var message = JsonSerializer.Serialize(command);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.MessageId = Guid.NewGuid().ToString();

            _channel.BasicPublish(
                exchange: "climate.commands",
                routingKey: routingKey,
                basicProperties: properties,
                body: body
            );

            Log.Debug("📤 Published command to climate.commands: {RoutingKey}", routingKey);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Failed to publish climate command: {RoutingKey}", routingKey);
        }
    }

    public void StartConsumingClimateData(Func<string, string, Task> onMessageReceived)
    {
        if (_channel == null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                Log.Debug("📥 Received climate data: {RoutingKey}", routingKey);

                await onMessageReceived(routingKey, message);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error processing climate data message");
                _channel.BasicReject(ea.DeliveryTag, true); // Requeue on error
            }
        };

        _channel.BasicConsume(
            queue: "climate.data.queue",
            autoAck: false,
            consumer: consumer
        );

        Log.Information("👂 Started consuming climate data messages");
    }

    public void StartConsumingClimateCommands(Func<string, string, Task> onMessageReceived)
    {
        if (_channel == null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                Log.Debug("📥 Received climate command: {RoutingKey}", routingKey);

                await onMessageReceived(routingKey, message);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "❌ Error processing climate command message");
                _channel.BasicReject(ea.DeliveryTag, true); // Requeue on error
            }
        };

        _channel.BasicConsume(
            queue: "climate.commands.queue",
            autoAck: false,
            consumer: consumer
        );

        Log.Information("👂 Started consuming climate command messages");
    }

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            Log.Information("🛑 RabbitMQ connection closed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "❌ Error closing RabbitMQ connection");
        }
    }
}