using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WEB.Models;

namespace WEB.Data.Configuration;

public class AreaConfiguration : IEntityTypeConfiguration<AreaModel>
{
    public void Configure(EntityTypeBuilder<AreaModel> builder)
    {
        builder.HasOne(a => a.JefeArea)
            .WithMany()
            .HasForeignKey(a => a.JefeAreaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}