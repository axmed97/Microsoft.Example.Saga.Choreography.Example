using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Consumers;
using Order.API.Data;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<PaymentCompletedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.AddConsumer<StockNotReservedEventConsumer>();
    configurator.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host("amqps://fkwmludd:CW1PU7v3m_il5wTInG-ttMN6UvTEJZ8G@hawk.rmq.cloudamqp.com/fkwmludd");
        cfg.ReceiveEndpoint(RabbitMQSettings.Order_PaymentCompletedEventQueue,
            e => e.ConfigureConsumer<PaymentCompletedEventConsumer>(ctx));

        cfg.ReceiveEndpoint(RabbitMQSettings.Order_PaymentFailedEventQueue,
            e => e.ConfigureConsumer<PaymentFailedEventConsumer>(ctx));

        cfg.ReceiveEndpoint(RabbitMQSettings.Order_StockNotReservedEventQueue,
            e => e.ConfigureConsumer<StockNotReservedEventConsumer>(ctx));
    });
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderDto model, AppDbContext _context, IPublishEndpoint _publishEndpoint) =>
{
    OrderEntity orderEntity = new()
    {
        CreatedDate = DateTime.Now,
        BuyerId = Guid.Parse(model.BuyerId),
        OrderStatus = Order.API.Enums.OrderStatus.Suspend,
        TotalPrice = model.OrderItemDtos.Sum(x => x.Price * x.Count),
        OrderItems = model.OrderItemDtos.Select(x => new OrderItem
        {
            Count = x.Count,
            Price = x.Price,
            ProductId = Guid.Parse(x.ProductId)
        }).ToList()
    };

    await _context.AddAsync(orderEntity);
    await _context.SaveChangesAsync();

    OrderCreatedEvent orderCreatedEvent = new()
    {
        BuyerId = orderEntity.BuyerId,
        OrderId = orderEntity.Id,
        TotalPrice = orderEntity.TotalPrice,
        OrderItemMessages = orderEntity.OrderItems.Select(x => new Shared.Messages.OrderItemMessage
        {
            Count = x.Count,
            Price = x.Price,
            ProductId = x.ProductId
        }).ToList()
    };

    await _publishEndpoint.Publish(orderCreatedEvent);
});


app.Run();
