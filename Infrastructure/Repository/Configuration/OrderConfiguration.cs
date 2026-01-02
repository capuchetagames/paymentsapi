using Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Repository.Configuration;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("INT").ValueGeneratedNever().UseIdentityColumn();
        builder.Property(x=>x.UserId).HasColumnType("INT").IsRequired();
        builder.Property(x=>x.GameId).HasColumnType("INT").IsRequired();
        builder.Property(x => x.Price).HasColumnType("DECIMAL(7,4)").IsRequired();
        builder.Property(x => x.Status).HasColumnType("VARCHAR(100)").IsRequired();
        builder.Property(x=> x.CreatedAt).HasColumnType("DATETIME").IsRequired();
        
        // builder.HasOne(u=>u.User).WithMany(u=>u.Orders).HasPrincipalKey(u=>u.Id);
        // builder.HasOne(g=>g.Game).WithMany(g=>g.Orders).HasPrincipalKey(g=>g.Id);
        
    }
}