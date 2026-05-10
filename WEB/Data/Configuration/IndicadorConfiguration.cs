using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.Models;

namespace WEB.Data.Configuration;

public class IndicadorConfiguration : IEntityTypeConfiguration<IndicadorModel>
{
    public void Configure(EntityTypeBuilder<IndicadorModel> builder)
    {
     
        builder.Property(e => e.MetaCumplirDecimal).HasPrecision(18, 4);
        builder.Property(e => e.MetaRealDecimal).HasPrecision(18, 4);
        builder.Property(e => e.ValorRealAcumulado).HasPrecision(18, 4);
        builder.Property(e => e.ValorTotalAcumulado).HasPrecision(18, 4);

       
        builder.HasMany(i => i.Objetivos)
            .WithMany(o => o.Indicadores)
            .UsingEntity(j => j.ToTable("IndicadorObjetivos"));
        
    }
}