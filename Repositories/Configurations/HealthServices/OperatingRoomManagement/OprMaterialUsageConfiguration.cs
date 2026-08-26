using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprMaterialUsageConfiguration : IEntityTypeConfiguration<OprMaterialUsage>
{
    public void Configure(EntityTypeBuilder<OprMaterialUsage> builder)
    {
        builder.ToTable("OprMaterialUsage", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitCode).HasMaxLength(30).IsRequired();
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.SerialNumber).HasMaxLength(150);
        builder.Property(x => x.CorrectionReason).HasMaxLength(2000);
        builder.HasIndex(x => new { x.OprCaseId, x.ExternalItemId });
        builder.HasIndex(x => new { x.BatchNumber, x.SerialNumber });
        builder.HasIndex(x => new { x.OprCaseId, x.Id, x.Revision }).IsUnique();
        builder.HasOne(x => x.OprCase).WithMany().HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
