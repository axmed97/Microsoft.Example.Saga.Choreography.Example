using Microsoft.EntityFrameworkCore;
using Order.API.Models;

namespace Order.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OrderEntity> OrderEntities { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
