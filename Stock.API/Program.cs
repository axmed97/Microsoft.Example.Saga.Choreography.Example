using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Consumers;
using Stock.API.Models;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("amqps://fkwmludd:CW1PU7v3m_il5wTInG-ttMN6UvTEJZ8G@hawk.rmq.cloudamqp.com/fkwmludd");
        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e =>
        {
            e.ConfigureConsumer<OrderCreatedEventConsumer>(ctx);
        });

        cfg.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventQueue, e =>
        {
            e.ConfigureConsumer<PaymentFailedEventConsumer>(ctx);
        });
    });
});

builder.Services.AddSingleton<MongoDbService>();

var app = builder.Build();


using IServiceScope scope = app.Services.CreateScope();
var mongoDbService = scope.ServiceProvider.GetService<MongoDbService>();

var stockCollection = mongoDbService.GetCollection<StockEntity>();

if(!stockCollection.FindSync(x => true).Any())
{
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 100 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 200 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 300 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 400 });
    await stockCollection.InsertOneAsync(new() { ProductId = Guid.NewGuid().ToString(), Count = 500 });
}

app.Run();
