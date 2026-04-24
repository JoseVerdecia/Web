using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WEB.Data.Configuration;
using WEB.Interfaces;
using WEB.Models;

namespace WEB.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<IndicadorModel> Indicador { get; set; }
    public DbSet<IndicadorDeAreaModel> IndicadorDeArea { get; set; }
    public DbSet<ProcesoModel> Proceso { get; set; }
    public DbSet<AreaModel> Area { get; set; }
    public DbSet<ObjetivoModel> Objetivo { get; set; }
    public DbSet<NotificacionModel> Notificacion { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new NotificacionConfiguration());
        
        
        modelBuilder.Entity<IndicadorModel>()
            .HasMany(i => i.Objetivos)
            .WithMany(o => o.Indicadores)
            .UsingEntity(j => j.ToTable("IndicadorObjetivos"));
        
        modelBuilder.Entity<ProcesoModel>()
            .HasOne(p => p.JefeProceso)          
            .WithMany()                         
            .HasForeignKey(p => p.JefeProcesoId) 
            .OnDelete(DeleteBehavior.SetNull);   

        modelBuilder.Entity<ProcesoModel>()
            .HasMany(p => p.Indicadores)
            .WithOne(i => i.Proceso)
            .HasForeignKey(i => i.ProcesoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<IndicadorDeAreaModel>()
            .HasOne(i => i.Indicador)
            .WithMany(i => i.IndicadoresDeArea)
            .HasForeignKey(i => i.IndicadorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<IndicadorDeAreaModel>()
            .HasOne(i => i.Area)
            .WithMany(a => a.IndicadoresDeArea)
            .HasForeignKey(i => i.AreaId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<AreaModel>()
            .HasOne(a => a.JefeArea)
            .WithMany()
            .HasForeignKey(a => a.JefeAreaId)
            .OnDelete(DeleteBehavior.SetNull);
        
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var propertyAccess = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var condition = Expression.Equal(propertyAccess, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}