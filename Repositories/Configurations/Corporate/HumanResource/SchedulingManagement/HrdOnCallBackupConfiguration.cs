using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class HrdOnCallBackupConfiguration : IEntityTypeConfiguration<HrdOnCallBackup>
    {
        public void Configure(EntityTypeBuilder<HrdOnCallBackup> entity)
        {
            entity.ToTable("HrdOnCallBackup", "public", table =>
                table.HasCheckConstraint("CK_HrdOnCallBackup_BackupSequence", "\"BackupSequence\" >= 1"));
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BackupSequence).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.OnCallAssignment)
                .WithMany()
                .HasForeignKey(x => x.OnCallAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.OnCallAssignmentId, x.BackupSequence })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.OnCallAssignmentId, x.WorkforceProfileId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
