namespace Order.API.DTOs
{
    public class CreateOrderDto
    {
        public string BuyerId { get; set; }
        public List<OrderItemDto> OrderItemDtos { get; set; }
    }
}
