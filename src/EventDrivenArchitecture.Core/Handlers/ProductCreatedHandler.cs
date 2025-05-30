using EventDrivenArchitecture.src.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenArchitecture.src.Application.Handlers;
public class ProductCreatedHandler : INotificationHandler<ProductCreated>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductCreatedHandler> _logger;

    public ProductCreatedHandler(ILogger<ProductCreatedHandler> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public async Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"Message sent to Service Bus");

            await _publishEndpoint.Publish(notification, cancellationToken);
        }
        catch (Exception ex)
        {

        }
    }
}
