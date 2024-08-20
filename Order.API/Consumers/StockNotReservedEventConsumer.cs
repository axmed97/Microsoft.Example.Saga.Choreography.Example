using MassTransit;
using Order.API.Data;
using Shared.Events;

namespace Order.API.Consumers
{
    public class StockNotReservedEventConsumer(AppDbContext _context) : IConsumer<StockNotReservedEvent>
    {
        public async Task Consume(ConsumeContext<StockNotReservedEvent> context)
        {
            var orderEntity = await _context.OrderEntities.FindAsync(context.Message.OrderId);
            orderEntity.OrderStatus = Enums.OrderStatus.Fail;
            await _context.SaveChangesAsync();
        }
    }
}
