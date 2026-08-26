using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprHandoverConfiguration : IEntityTypeConfiguration<OprHandover>
{
    public void Configure(EntityTypeBuilder<OprHandover> builder)
    {
        builder.ToTable("OprHandover", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConditionSummary).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.DeviceTherapySummary).HasMaxLength(4000);
        builder.Property(x => x.RiskSummary).HasMaxLength(4000);
        builder.Property(x => x.InstructionSummary).HasMaxLength(4000);
        builder.Property(x => x.RejectionReason).HasMaxLength(2000);
        builder.HasIndex(x => new { x.OprCaseId, x.Revision }).IsUnique();
        builder.HasIndex(x => new { x.DestinationUnitId, x.Status });
        builder.HasOne(x => x.OprCase).WithMany().HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
