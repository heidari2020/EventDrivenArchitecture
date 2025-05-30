using EventDrivenArchitecture.src.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitecture.src.Infrastructure;
public class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProductCreated> context)
    {
        var @event = context.Message;
        _logger.LogInformation($"Received event {@event.Id} at: {@event.Createdat}");
        return Task.CompletedTask;

    }
}
