using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprSafetyChecklistConfiguration : IEntityTypeConfiguration<OprSafetyChecklist>
{
    public void Configure(EntityTypeBuilder<OprSafetyChecklist> builder)
    {
        builder.ToTable("OprSafetyChecklist", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TemplateVersion).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ItemsJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.BypassReason).HasMaxLength(2000);
        builder.HasIndex(x => new { x.OprCaseId, x.Phase, x.Revision }).IsUnique();
        builder.HasOne(x => x.OprCase).WithMany(x => x.SafetyChecklists).HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
