using EventDrivenArchitecture.Core.Interfaces;
using EventDrivenArchitecture.src.Domain.Events;
using EventDrivenArchitecture.src.Infrastructure;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitecture;
    public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMediator _mediator;
    private readonly IEventConsumer _consumer;
    public Worker(ILogger<Worker> logger, IMediator mediator, IEventConsumer consumer)
    {
        _logger = logger;
        _mediator = mediator;
        _consumer = consumer;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _consumer.StartConsumingAsync();
        await base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting event hub");

        while (!stoppingToken.IsCancellationRequested)
        {
            await  _mediator.Publish(new ProductCreatedEvent(Guid.NewGuid(), DateTime.UtcNow)); ;

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.StopConsumingAsync();
        await base.StopAsync(cancellationToken);
    }
}
