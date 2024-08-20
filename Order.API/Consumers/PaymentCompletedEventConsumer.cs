using MassTransit;
using Order.API.Data;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer(AppDbContext _context) : IConsumer<PaymentCompletedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
        {
            var orderEntity = await _context.OrderEntities.FindAsync(context.Message.OrderId);
            orderEntity.OrderStatus = Enums.OrderStatus.Completed;
            await _context.SaveChangesAsync();
        }
    }
}
