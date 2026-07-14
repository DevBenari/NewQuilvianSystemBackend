using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Global
{
    public class MstNurseStationClusterStaffClinicConfiguration : IEntityTypeConfiguration<MstNurseStationClusterStaffClinic>
    {
        public void Configure(EntityTypeBuilder<MstNurseStationClusterStaffClinic> entity)
        {
            entity.ToTable("MstNurseStationClusterStaffClinic", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.NurseStationClusterStaffId)
                .IsRequired();

            entity.Property(x => x.ClinicId)
                .IsRequired();

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

            entity.HasOne(x => x.NurseStationClusterStaff)
                .WithMany(x => x.StaffClinics)
                .HasForeignKey(x => x.NurseStationClusterStaffId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Clinic)
                .WithMany()
                .HasForeignKey(x => x.ClinicId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.NurseStationClusterStaffId);

            entity.HasIndex(x => x.ClinicId);

            entity.HasIndex(x => new
            {
                x.NurseStationClusterStaffId,
                x.ClinicId
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.NurseStationClusterStaffId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ClinicId,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
