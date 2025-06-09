using EventDrivenArchitecture.src.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Wrap;
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
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private readonly AsyncPolicyWrap _policyWrap;

    public ProductCreatedHandler(ILogger<ProductCreatedHandler> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        //_retryPolicy = Policy.Handle<Exception>()
        //     .WaitAndRetryAsync(
        //         3,
        //         retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        //         (exception, span, retryCount) => // Changed from retryAttempt to retryCount
        //         {
        //             _logger.LogWarning($"Retry {retryCount} after {span.TotalSeconds}s: {exception.Message}");
        //         }
        //     );
        //_circuitBreakerPolicy = Policy
        //    .Handle<Exception>()
        //    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
        //        (exception, duration) =>
        //        {
        //            _logger.LogError($"Circuit broken for {duration.TotalSeconds}s.");
        //        },
        //        () =>
        //        {
        //            _logger.LogInformation("Circuit reset.");
        //        });

        //_policyWrap = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
    }

    public async Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
      

        //await _policyWrap.ExecuteAsync(async () =>
        //{
            await _publishEndpoint.Publish(notification, cancellationToken);
            _logger.LogInformation("Message published successfully");
       // });
      
    }
}
