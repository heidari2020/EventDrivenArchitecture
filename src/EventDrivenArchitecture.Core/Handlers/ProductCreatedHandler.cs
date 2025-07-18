using EventDrivenArchitecture.src.Domain.Events;
using MassTransit;
using MassTransit.Caching;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitecture.src.Application.Handlers;
public class ProductCreatedHandler : INotificationHandler<ProductCreated>
{
    private readonly ILogger<ProductCreatedHandler> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly AsyncPolicyWrap _policyWrap;
    private readonly IConnection _rabbitMqConnection;

    public ProductCreatedHandler(ILogger<ProductCreatedHandler> logger, 
        IConnection rabbitMqConnection)
    {
        _logger = logger;
        _rabbitMqConnection = rabbitMqConnection;
        _retryPolicy = Policy.Handle<Exception>()
             .WaitAndRetryAsync(
                 3,
                 retryAttempt => TimeSpan.FromSeconds(Math.Pow(5, retryAttempt)),
                 (exception, span, retryCount) => // Changed from retryAttempt to retryCount
                 {
                     _logger.LogWarning($"Retry {retryCount} after {span.TotalSeconds}s: {exception.Message}");
                 }
             );
        _circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                (exception, duration) =>
                {
                    _logger.LogError($"Circuit broken for {duration.TotalSeconds}s.");
                },
                () =>
                {
                    _logger.LogInformation("Circuit reset.");
                });

        _policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
    }

    public async Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        await _policyWrap.ExecuteAsync(async () =>
        {
             var channel = await _rabbitMqConnection.CreateChannelAsync();

           //await DeclareExchange(channel);
            await PublishMessage(channel, notification);

            _logger.LogInformation("Product created event published successfully");
        });
       
            _logger.LogInformation("Message published successfully"); 
      
    } 
    private async Task PublishMessage(IChannel channel, ProductCreated notification)
    {
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(notification));

        await channel.BasicPublishAsync(
            exchange: "product_exchange",
             routingKey: string.Empty,
            body: body);
    }
}
