using EventDrivenArchitecture.src.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Threading.Channels;
using MassTransit.Caching;
using EventDrivenArchitecture.Core.Interfaces;
using MassTransit.Middleware;

namespace EventDrivenArchitecture.src.Infrastructure;
public sealed class RabbitMqConsumer : IEventConsumer, IDisposable
{
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IConnection _connection;
    private IChannel _channel;
    private AsyncEventingBasicConsumer _consumer;
    private const string ExchangeName = "product_events";
    private const string QueueName = "product_events_queue";
    public RabbitMqConsumer(
       ILogger<RabbitMqConsumer> logger,
       IConnection rabbitMqConnection)
    {
        _logger = logger;
        _connection = rabbitMqConnection;
    }
    public async Task StartConsumingAsync()
    {
        _channel = await _connection.CreateChannelAsync();
        await ConfigureTopologyAsync(_channel);
        _consumer = CreateConsumer(_channel);

        await _channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: _consumer);
    }

    private async Task ConfigureTopologyAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: string.Empty);
    }

    private AsyncEventingBasicConsumer CreateConsumer(IChannel channel)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;
        return consumer;
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        try
        {
             var body = eventArgs.Body.ToArray();
            var message = JsonConvert.DeserializeObject<ProductCreatedEvent>(
                Encoding.UTF8.GetString(body)); ;
            try
            {
                await ProcessMessageAsync(message);
                await _channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message processing failed");
                await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
            } 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message processing failed");
            await _channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private Task ProcessMessageAsync(ProductCreatedEvent message)
    {
        _logger.LogInformation(
            "Received event {EventId} at {Timestamp}",
            message.Id, message.Createdat);

        return Task.CompletedTask;
    }
    public async Task StopConsumingAsync()
    {
        if (_consumer != null)
        {
            _consumer.ReceivedAsync -= OnMessageReceivedAsync;
        }

        if (_channel?.IsOpen == true)
        {
            await _channel.CloseAsync();
        }
    }

    public void Dispose() => _channel?.Dispose();
}

 
