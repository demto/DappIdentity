using DApp.API.Data.Configs;
using DApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options)
            :base(options) {}

        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder){
            modelBuilder.ApplyConfiguration(new ValueConfigs());
            modelBuilder.ApplyConfiguration(new LikeConfigs());
            modelBuilder.ApplyConfiguration(new MessageConfigs());
        }
    }
}