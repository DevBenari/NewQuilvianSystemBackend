using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprTeamMemberConfiguration : IEntityTypeConfiguration<OprTeamMember>
{
    public void Configure(EntityTypeBuilder<OprTeamMember> builder)
    {
        builder.ToTable("OprTeamMember", "public");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ScheduleId, x.WorkforceId, x.Role }).IsUnique();
        builder.HasIndex(x => new { x.WorkforceId, x.IsCurrent });
        builder.HasOne(x => x.OprCase).WithMany(x => x.TeamMembers).HasForeignKey(x => x.OprCaseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Schedule).WithMany(x => x.TeamMembers).HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Workforce).WithMany().HasForeignKey(x => x.WorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}
