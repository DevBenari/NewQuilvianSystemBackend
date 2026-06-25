using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstNurseStationClusterStaffConfiguration : IEntityTypeConfiguration<MstNurseStationClusterStaff>
    {
        public void Configure(EntityTypeBuilder<MstNurseStationClusterStaff> entity)
        {
            entity.ToTable("MstNurseStationClusterStaff", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.NurseStationClusterId)
                .IsRequired();

            entity.Property(x => x.EmployeeId)
                .IsRequired();

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.CanCallQueue)
                .HasDefaultValue(true);

            entity.Property(x => x.CanStartScreening)
                .HasDefaultValue(true);

            entity.Property(x => x.CanTransferQueue)
                .HasDefaultValue(true);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.UpdateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.NurseStationCluster)
                .WithMany()
                .HasForeignKey(x => x.NurseStationClusterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.NurseStationClusterId);

            entity.HasIndex(x => x.EmployeeId);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => new
            {
                x.NurseStationClusterId,
                x.EmployeeId,
                x.IsDelete
            }).IsUnique();

            entity.HasIndex(x => new
            {
                x.NurseStationClusterId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EmployeeId,
                x.IsActive,
                x.IsDelete
            });
        }
    }

}
