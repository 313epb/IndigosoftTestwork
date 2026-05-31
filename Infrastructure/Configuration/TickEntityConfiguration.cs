using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class TickEntityConfiguration: IEntityTypeConfiguration<TickEntity>
{
    public void Configure(EntityTypeBuilder<TickEntity> builder)
    {
        builder.ToTable("ticks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id");

        builder.Property(x => x.Source)
               .HasColumnName("source")
               .HasMaxLength(50);

        builder.Property(x => x.Symbol)
               .HasColumnName("symbol")
               .HasMaxLength(50);

        builder.Property(x => x.Price)
               .HasColumnName("price")
               .HasPrecision(18, 8);

        builder.Property(x => x.Volume)
               .HasColumnName("volume")
               .HasPrecision(18, 8);

        builder.Property(x => x.Timestamp)
               .HasColumnName("timestamp");
    }
}