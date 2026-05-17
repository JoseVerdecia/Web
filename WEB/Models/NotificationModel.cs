using System.ComponentModel.DataAnnotations;
using WEB.Data;
using WEB.Enums;
using WEB.Core.Interfaces;

namespace WEB.Models;

public class NotificacionModel : ISoftDeletable, ICreatedInterfaces
{
    [Key]
    public int Id { get; set; }

   
    [Required]
    public string DestinatarioId { get; set; } = null!;
    public ApplicationUser? Destinatario { get; set; }

    [Required]
    public string RemitenteId { get; set; } = null!;
    public ApplicationUser? Remitente { get; set; }

    [Required]
    [MaxLength(200)]
    public string Cabecera { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public string Cuerpo { get; set; } = null!;

    [Required]
    public TipoNotificacion Tipo { get; set; }

    public EstadoNotificacion Estado { get; set; } = EstadoNotificacion.Pendiente;
    
    public bool Leida { get; set; } = false;
    public DateTime? LeidaAt { get; set; }

    // Motivo de Cambio
    [MaxLength(500)]
    public string? MensajePersonalizado { get; set; }

    // Propiedades para la solicitud de cambio de meta a cumplir ==========
    public int? IndicadorDeAreaId { get; set; }
    public IndicadorDeAreaModel? IndicadorDeArea { get; set; }
    
    public string? MetaAnterior { get; set; }              
    public decimal? MetaAnteriorDecimal { get; set; }      
    public bool? IsMetaAnteriorPorcentaje { get; set; }    

  
    public string? NuevaMetaPropuesta { get; set; }        
    public decimal? NuevaMetaPropuestaDecimal { get; set; } 
    public bool? IsNuevaMetaPorcentaje { get; set; }       

    //  REFERENCIA A NOTIFICACIÓN ORIGINAL (para respuestas)
    public int? SolicitudOriginalId { get; set; }
    public NotificacionModel? SolicitudOriginal { get; set; }
    
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}