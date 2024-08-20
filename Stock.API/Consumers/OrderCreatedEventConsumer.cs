using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.Events;
using Stock.API.Models;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer(MongoDbService _mongoDbService, 
        ISendEndpointProvider _sendEndpointProvider,
        IPublishEndpoint _publishEndpoint) : IConsumer<OrderCreatedEvent>
    {

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> stockResult = new();
            IMongoCollection<StockEntity> collection = _mongoDbService.GetCollection<StockEntity>();

            foreach (var item in context.Message.OrderItemMessages)
            {
                stockResult.Add(await (await collection.FindAsync(x => x.ProductId == item.ProductId.ToString() 
                    && x.Count > item.Count))
                    .AnyAsync());
            }

            if(stockResult.TrueForAll(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItemMessages)
                {
                    StockEntity stockEntity = await (await collection.FindAsync(x => x.ProductId == item.ProductId.ToString())).FirstOrDefaultAsync();
                    stockEntity.Count -= item.Count;
                    await collection.FindOneAndReplaceAsync(x => x.ProductId == item.ProductId.ToString(), stockEntity);
                }
                var sendEndpoint = await _sendEndpointProvider
                    .GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.Payment_StockReservedEventQueue}"));

                StockReservedEvent stockReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    TotalPrice = context.Message.TotalPrice,
                    OrderItemMessages = context.Message.OrderItemMessages,
                };
                await sendEndpoint.Send(stockReservedEvent);

            }
            else
            {
                StockNotReservedEvent stockNotReservedEvent = new()
                {
                    BuyerId = context.Message.BuyerId,
                    Message = "Fail",
                    OrderId = context.Message.OrderId,
                };
                await _publishEndpoint.Publish(stockNotReservedEvent);
            }
        }
    }
}
