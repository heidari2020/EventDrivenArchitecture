
using EventDrivenArchitecture;
using EventDrivenArchitecture.Core.Interfaces;
using EventDrivenArchitecture.Infrastructure.Bus.Config;
using EventDrivenArchitecture.Infrastructure.Bus.Publisher;
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
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProductCreatedEvent).Assembly));


var rabbitMqConfig =  new RabbitMqConfiguration
{
    HostName = "localhost",     
    Port = 5672,              
    Username = "guest",         
    Password = "guest"           
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
builder.Services.AddSingleton<IEventPublisher, RabbitMqPublisher>();
builder.Services.AddSingleton<IEventConsumer, RabbitMqConsumer>();


var host = builder.Build();
host.Run();