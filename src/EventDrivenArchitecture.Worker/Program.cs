
using EventDrivenArchitecture;
using EventDrivenArchitecture.Infrastructure.Bus.Config;
using EventDrivenArchitecture.src.Application.Handlers;
using EventDrivenArchitecture.src.Domain.Events;
using EventDrivenArchitecture.src.Infrastructure;
using MassTransit;
using MassTransit.Configuration;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProductCreated).Assembly));


var rabbitMqConfig =  new RabbitMqConfiguration
{
    HostName = "localhost",     // Replace with your host
    Port = 5672,               // Default RabbitMQ port
    Username = "guest",         // Replace with your username
    Password = "guest"          // Replace with your password
}; ;

var connectionFactory = new ConnectionFactory
{
    HostName = rabbitMqConfig.HostName,
    Port = rabbitMqConfig.Port,
    UserName = rabbitMqConfig.Username,
    Password = rabbitMqConfig.Password
};

var connection =await connectionFactory.CreateConnectionAsync();
builder.Services.AddSingleton<IConnection>(connection);
builder.Services.AddScoped<ProductCreatedHandler>();
builder.Services.AddScoped<ProductCreatedConsumer>();

//builder.Services.AddScoped<IConsumer<ProductCreated>, ProductCreatedConsumer>();
//builder.Services.AddScoped<INotificationHandler<ProductCreated>, ProductCreatedHandler>();


//builder.Services.AddMassTransit(configurator =>
//{
//    configurator.SetKebabCaseEndpointNameFormatter();

//    configurator.AddConsumer<ProductCreatedConsumer>();
//    // Configure RabbitMQ endpoints
//    configurator.UsingRabbitMq((context, cfg) =>
//    {
//        cfg.Host(new Uri($"rabbitmq://localhost/"), h =>
//        {
//            h.Username("guest");
//            h.Password("guest");
//        });
        
//        cfg.ReceiveEndpoint("message_queue", e =>
//        {
//            e.ConfigureConsumer<ProductCreatedConsumer>(context);
//            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
//        });
//        // Configure the message endpoint
//        //cfg.ReceiveEndpoint("message_queue", e =>
//        //{
//        //    e.UseMessageRetry(r => r.Intervals(100, 500, 1000)); // Retry intervals
//        //});
//    });

   
//});

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