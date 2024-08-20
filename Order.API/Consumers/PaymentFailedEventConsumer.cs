using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Shared.Events;

namespace Order.API.Consumers
{
    public class PaymentFailedEventConsumer(AppDbContext _context) : IConsumer<PaymentFailedEvent>
    {
        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var orderEntity = await _context.OrderEntities.FindAsync(context.Message.OrderId);
            orderEntity.OrderStatus = Enums.OrderStatus.Fail;
            await _context.SaveChangesAsync();
        }
    }
}
