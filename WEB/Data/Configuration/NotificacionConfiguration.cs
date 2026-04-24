using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.Models;

namespace WEB.Data.Configuration;

public class NotificacionConfiguration : IEntityTypeConfiguration<NotificacionModel>
{
    public void Configure(EntityTypeBuilder<NotificacionModel> builder)
    {
        builder.ToTable("Notificaciones");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Cabecera)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Cuerpo)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.MensajePersonalizado)
            .HasMaxLength(500);
        
        builder.HasIndex(n => n.DestinatarioId);
        builder.HasIndex(n => n.RemitenteId);
        builder.HasIndex(n => n.Estado);
        builder.HasIndex(n => n.Leida);
        builder.HasIndex(n => new { n.DestinatarioId, n.Leida }); 
        
        builder.HasOne(n => n.Destinatario)
            .WithMany()
            .HasForeignKey(n => n.DestinatarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Remitente)
            .WithMany()
            .HasForeignKey(n => n.RemitenteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.IndicadorDeArea)
            .WithMany(i => i.Notificaciones)
            .HasForeignKey(n => n.IndicadorDeAreaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.SolicitudOriginal)
            .WithMany()
            .HasForeignKey(n => n.SolicitudOriginalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(n => !n.IsDeleted);
    }
}