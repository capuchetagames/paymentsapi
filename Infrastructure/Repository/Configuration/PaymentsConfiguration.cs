using Core.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Repository.Configuration;

public class PaymentsConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("INT").ValueGeneratedNever().UseIdentityColumn();
        builder.Property(x=>x.UserId).HasColumnType("INT").IsRequired();
        builder.Property(x=>x.GameId).HasColumnType("INT").IsRequired();
        builder.Property(x => x.Price).HasColumnType("DECIMAL(7,4)").IsRequired();
        builder.Property(x => x.Status).HasColumnType("VARCHAR(100)").IsRequired();
        builder.Property(x=> x.CreatedAt).HasColumnType("DATETIME").IsRequired();
        
        // builder.HasOne(u=>u.User).WithMany(u=>u.Payments).HasPrincipalKey(u=>u.Id);
        // builder.HasOne(g=>g.Game).WithMany(g=>g.Payments).HasPrincipalKey(g=>g.Id);
        
    }
}