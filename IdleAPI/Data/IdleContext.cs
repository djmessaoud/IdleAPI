using IdleAPI.Models;
using Microsoft.EntityFrameworkCore;
namespace IdleAPI.Data
{
    public class IdleContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<BalanceLog> BalanceHistory { get; set; }

        public DbSet<Config> IdleConfig { get; set; }
        public IdleContext(DbContextOptions<IdleContext> options) : base(options)
        {
        }
    }
}
