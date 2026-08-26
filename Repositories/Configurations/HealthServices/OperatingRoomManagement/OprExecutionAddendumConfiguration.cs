using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprExecutionAddendumConfiguration : IEntityTypeConfiguration<OprExecutionAddendum>
{
    public void Configure(EntityTypeBuilder<OprExecutionAddendum> builder)
    {
        builder.ToTable("OprExecutionAddendum", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Content).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.ExecutionRecordId, x.AuthoredAt });
        builder.HasOne(x => x.ExecutionRecord).WithMany(x => x.Addenda).HasForeignKey(x => x.ExecutionRecordId).OnDelete(DeleteBehavior.Restrict);
    }
}
