using EventDrivenArchitecture.Core.Interfaces;
using EventDrivenArchitecture.src.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitecture.Infrastructure.Bus.Publisher;
    public sealed class RabbitMqPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IAsyncPolicy _resiliencyPolicy;
    private IChannel _channel;
    private const string ExchangeName = "product_events";

    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        IConnection rabbitMqConnection)
    {
        _logger = logger;
        _connection = rabbitMqConnection;
        _resiliencyPolicy = CreateResiliencyPolicy();
    }

    private IAsyncPolicy CreateResiliencyPolicy()
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, attempt =>
                TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (ex, delay) =>
                    _logger.LogWarning(ex, "Retrying after {Delay}s", delay.TotalSeconds));

        var circuitBreaker = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
            onBreak: (ex, duration) =>
                _logger.LogError(ex, "Circuit broken for {Duration}s", duration.TotalSeconds),
            onReset: () =>
                _logger.LogInformation("Circuit reset"));

        return Policy.WrapAsync(retryPolicy, circuitBreaker);
    }

    public async Task InitializeAsync()
    {
        _channel = await _connection.CreateChannelAsync(); 
    }

    public async Task PublishAsync(ProductCreatedEvent @event)
    {
        await _resiliencyPolicy.ExecuteAsync(async () =>
        {
            if (_channel?.IsOpen != true)
            {
                await InitializeAsync();
            } 
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: string.Empty,
                body: body);
        });
    }
     
    public void Dispose() => _channel?.Dispose();
}
