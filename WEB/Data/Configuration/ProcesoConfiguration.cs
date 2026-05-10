using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.Models;

namespace WEB.Data.Configuration;

public class ProcesoConfiguration : IEntityTypeConfiguration<ProcesoModel>
{
    public void Configure(EntityTypeBuilder<ProcesoModel> builder)
    {
        builder.HasOne(p => p.JefeProceso)
            .WithMany()
            .HasForeignKey(p => p.JefeProcesoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Indicadores)
            .WithOne(i => i.Proceso)
            .HasForeignKey(i => i.ProcesoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}