using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprStatusHistoryConfiguration : IEntityTypeConfiguration<OprStatusHistory>
{
    public void Configure(EntityTypeBuilder<OprStatusHistory> builder)
    {
        builder.ToTable("OprStatusHistory", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.Source).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.HasIndex(x => new { x.OprCaseId, x.OccurredAt });
        builder.HasOne(x => x.OprCase).WithMany(x => x.StatusHistories).HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
    }
}
