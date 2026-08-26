using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprScheduleConfiguration : IEntityTypeConfiguration<OprSchedule>
{
    public void Configure(EntityTypeBuilder<OprSchedule> builder)
    {
        builder.ToTable("OprSchedule", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChangeReason).HasMaxLength(500);
        builder.HasIndex(x => new { x.OprCaseId, x.Revision }).IsUnique();
        builder.HasIndex(x => x.OprCaseId).IsUnique().HasFilter("\"IsCurrent\" = TRUE AND \"IsDelete\" = FALSE");
        builder.HasIndex(x => new { x.RoomId, x.StartAt, x.EndAt, x.IsCurrent });
        builder.HasOne(x => x.OprCase).WithMany(x => x.Schedules).HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
    }
}
