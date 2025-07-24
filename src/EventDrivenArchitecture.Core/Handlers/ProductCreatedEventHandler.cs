using EventDrivenArchitecture.Core.Interfaces;
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
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;
    private readonly IEventPublisher _eventPublisher;


    public ProductCreatedEventHandler(
        ILogger<ProductCreatedEventHandler> logger,
        IEventPublisher eventPublisher)
    {
        _logger = logger;
        _eventPublisher = eventPublisher;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _eventPublisher.PublishAsync(notification);
            _logger.LogInformation("Published event {EventId}", notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventId}", notification.Id);
            throw;
        } 
    }
}
