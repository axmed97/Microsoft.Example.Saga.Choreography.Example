using MassTransit;
using Payment.API.Consumers;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<StockReservedEventConsumer>();
    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("amqps://fkwmludd:CW1PU7v3m_il5wTInG-ttMN6UvTEJZ8G@hawk.rmq.cloudamqp.com/fkwmludd");
        cfg.ReceiveEndpoint(RabbitMQSettings.Payment_StockReservedEventQueue, 
            e => e.ConfigureConsumer<StockReservedEventConsumer>(ctx));
    });
});

var app = builder.Build();


app.Run();
