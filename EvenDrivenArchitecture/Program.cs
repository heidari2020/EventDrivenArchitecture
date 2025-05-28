using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
    {

        cfg.Host(new Uri($"rabbitmq://localhost/"), h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

    }));
});
var serviceProvider = builder.Services.BuildServiceProvider();
var bus = serviceProvider.GetRequiredService<IBusControl>();
await bus.StartAsync();
