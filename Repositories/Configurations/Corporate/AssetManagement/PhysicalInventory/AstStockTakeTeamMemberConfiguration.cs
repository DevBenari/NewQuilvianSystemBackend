using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.PhysicalInventory;

public sealed class AstStockTakeTeamMemberConfiguration : IEntityTypeConfiguration<AstStockTakeTeamMember>
{
    public void Configure(EntityTypeBuilder<AstStockTakeTeamMember> entity)
    {
        entity.ToTable("AstStockTakeTeamMember", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.IsLeader).HasDefaultValue(false);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Session).WithMany(x => x.TeamMembers).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.SessionId, x.EmployeeId })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstStockTakeTeamMember_Session_Employee");

        entity.HasIndex(x => x.SessionId)
            .IsUnique()
            .HasFilter("\"IsLeader\" = true AND \"IsDelete\" = false")
            .HasDatabaseName("IX_AstStockTakeTeamMember_LeaderPerSession");

        entity.HasIndex(x => x.EmployeeId);
    }
}
