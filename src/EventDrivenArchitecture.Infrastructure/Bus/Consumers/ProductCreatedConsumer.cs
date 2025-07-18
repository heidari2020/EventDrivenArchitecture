using EventDrivenArchitecture.src.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Threading.Channels;
using MassTransit.Caching;

namespace EventDrivenArchitecture.src.Infrastructure;
public class ProductCreatedConsumer 
{
    private readonly ILogger<ProductCreatedConsumer> _logger;
    private readonly IConnection _rabbitMqConnection; 

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger, IConnection rabbitMqConnection)
    {
        _logger = logger;
        _rabbitMqConnection = rabbitMqConnection;

    }

    public async Task HandleMessage(ProductCreated message)
    {

            Console.WriteLine("Started consuming from queue"); 
            _logger.LogInformation($"Received event {message.Id} at: {message.Createdat}");
            await Task.CompletedTask;
    }

    public async Task Subscribe()
    {
         var channel = await _rabbitMqConnection.CreateChannelAsync();


       await channel.QueueDeclareAsync("product_queue", true, false, false, null);
        await channel.ExchangeDeclareAsync("product_exchange", ExchangeType.Fanout, true);
        await channel.QueueBindAsync("product_queue", "product_exchange", string.Empty);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var productCreated = JsonConvert.DeserializeObject<ProductCreated>(
                Encoding.UTF8.GetString(body));
            await HandleMessage(productCreated);

            await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };
        await channel.BasicConsumeAsync(
                queue: "product_queue",
                autoAck: false,
                consumer: consumer); 
    }
    public async Task Stop()
    {
        if (_rabbitMqConnection != null)
        {
            await _rabbitMqConnection.DisposeAsync();
        }
    }
}
