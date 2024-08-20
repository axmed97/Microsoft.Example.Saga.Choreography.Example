using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Models;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer(MongoDbService mongoDbService) : IConsumer<PaymentFailedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            IMongoCollection<StockEntity> stocks = mongoDbService.GetCollection<StockEntity>();

            foreach (var item in context.Message.OrderItemMessages)
            {
                var stock = await (await stocks.FindAsync(x => x.ProductId == item.ProductId.ToString())).FirstOrDefaultAsync();
                stock.Count += item.Count;
                await stocks.FindOneAndReplaceAsync(x => x.ProductId == item.ProductId.ToString(), stock);
            }
        }
    }
}
