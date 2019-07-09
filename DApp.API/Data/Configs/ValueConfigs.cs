using DApp.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DApp.API.Data.Configs
{
    public class ValueConfigs : IEntityTypeConfiguration<Value>
    {
        public void Configure(EntityTypeBuilder<Value> builder)
        {
            // table
                // builder.ToTable("ValueTable");
            // key

            // property
                // builder.Property(p => p.Name)
                // .IsRequired()
                // .HasMaxLength(10);
            // relationship
        }
    }
}