using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.Models;

namespace WEB.Data.Configuration;

public class IndicadorDeAreaConfiguration : IEntityTypeConfiguration<IndicadorDeAreaModel>
{
    public void Configure(EntityTypeBuilder<IndicadorDeAreaModel> builder)
    {
        builder.Property(e => e.MetaCumplirDecimal).HasPrecision(18, 4);
        builder.Property(e => e.MetaRealDecimal).HasPrecision(18, 4);
        builder.Property(e => e.ValorReal).HasPrecision(18, 4);
        builder.Property(e => e.ValorTotal).HasPrecision(18, 4);

        builder.HasOne(i => i.Indicador)
            .WithMany(i => i.IndicadoresDeArea)
            .HasForeignKey(i => i.IndicadorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Area)
            .WithMany(a => a.IndicadoresDeArea)
            .HasForeignKey(i => i.AreaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}