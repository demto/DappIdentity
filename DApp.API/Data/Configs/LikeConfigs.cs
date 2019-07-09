using DApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DApp.API.Data.Configs
{
    public class LikeConfigs : IEntityTypeConfiguration<Like>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Like> builder)
        {
            // table

            builder.ToTable("Likes");

            // key

            builder.HasKey(k => new {k.LikeeId, k.LikerId});

            // property

            //relationship

            builder.HasOne(l => l.Likee)
                .WithMany(u => u.Likers)
                .HasForeignKey(l => l.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(l => l.Liker)
                .WithMany(u => u.Likees)
                .HasForeignKey(l => l.LikerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}