using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprExecutionRecordConfiguration : IEntityTypeConfiguration<OprExecutionRecord>
{
    public void Configure(EntityTypeBuilder<OprExecutionRecord> builder)
    {
        builder.ToTable("OprExecutionRecord", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PreDiagnosis).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.PostDiagnosis).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Findings).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Technique).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Complications).HasMaxLength(4000);
        builder.Property(x => x.SpecimenNote).HasMaxLength(2000);
        builder.Property(x => x.ImplantDrainNote).HasMaxLength(2000);
        builder.Property(x => x.PostPlan).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.OprCaseId).IsUnique();
        builder.HasOne(x => x.OprCase).WithOne().HasForeignKey<OprExecutionRecord>(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
