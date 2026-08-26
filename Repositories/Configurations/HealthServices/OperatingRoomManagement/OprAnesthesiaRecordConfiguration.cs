using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprAnesthesiaRecordConfiguration : IEntityTypeConfiguration<OprAnesthesiaRecord>
{
    public void Configure(EntityTypeBuilder<OprAnesthesiaRecord> builder)
    {
        builder.ToTable("OprAnesthesiaRecord", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssessmentSummary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Technique).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.MedicationFluidSummary).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.AirwaySummary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.MonitoringSummary).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.EventSummary).HasMaxLength(4000);
        builder.Property(x => x.FinalCondition).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.OprCaseId).IsUnique();
        builder.HasOne(x => x.OprCase).WithOne().HasForeignKey<OprAnesthesiaRecord>(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
