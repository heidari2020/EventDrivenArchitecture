
using EventDrivenArchitecture;
using EventDrivenArchitecture.Infrastructure;
using EventDrivenArchitecture.src.Infrastructure;
using MassTransit;
using MassTransit.Configuration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Registry;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddMassTransit(configurator =>
{
    configurator.SetKebabCaseEndpointNameFormatter();

    configurator.AddConsumer<ProductCreatedConsumer>();
    // Configure RabbitMQ endpoints
    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri($"rabbitmq://localhost/"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        cfg.ReceiveEndpoint("message_queue", e =>
        {
            e.ConfigureConsumer<ProductCreatedConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
        });
        // Configure the message endpoint
        //cfg.ReceiveEndpoint("message_queue", e =>
        //{
        //    e.UseMessageRetry(r => r.Intervals(100, 500, 1000)); // Retry intervals
        //});
    });

   
});

// Configure the policy registry
//var services = builder.Services;
//services.AddPolicyRegistry<string>(registry =>
//{
//    registry.AddAsyncPolicy("MessagePolicy",
//        Policy.Handle<Exception>()
//            .WaitAndRetryAsync(
//                _settings.MaxRetries,
//                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
//                (exception, span) => _logger.LogWarning($"Retry {retryAttempt} after {span.TotalSeconds}s")
//            )
//            .Wrap(Policy.Handle<Exception>()
//                .CircuitBreakerAsync(
//                    exceptionsAllowedBeforeBreaking: _settings.CircuitBreakerFailureThreshold,
//                    durationOfBreak: TimeSpan.FromSeconds(_settings.CircuitBreakerDurationSeconds),
//                    onBreak: (ex, ts) => _logger.LogError($"Circuit broken for {ts.TotalSeconds}s"),
//                    onReset: () => _logger.LogInformation("Circuit reset")
//                )
//            )
//    );
//});
var host = builder.Build();
host.Run();