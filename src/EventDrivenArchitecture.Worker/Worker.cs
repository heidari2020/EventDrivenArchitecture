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
    private readonly ProductCreatedConsumer _consumer;
    public Worker(ILogger<Worker> logger, IMediator mediator, ProductCreatedConsumer consumer)
    {
        _logger = logger;
        _mediator = mediator;
        _consumer = consumer;
    }
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
       await _consumer.Subscribe();
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Starting event hub");

        while (!stoppingToken.IsCancellationRequested)
        {
            await  _mediator.Publish(new ProductCreated(Guid.NewGuid(), DateTime.UtcNow)); ;

            await Task.Delay(10000, stoppingToken);
        }
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _consumer.Stop();
        await base.StopAsync(cancellationToken);
    }
}
