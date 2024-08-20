namespace Order.API.DTOs
{
    public class OrderItemDto
    {
        public string ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
