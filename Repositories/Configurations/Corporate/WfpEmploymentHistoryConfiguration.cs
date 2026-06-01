using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpEmploymentHistoryConfiguration : IEntityTypeConfiguration<WfpEmploymentHistory>
    {
        public void Configure(EntityTypeBuilder<WfpEmploymentHistory> entity)
        {
            entity.ToTable("WfpEmploymentHistory", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.HistoryType)
                .HasConversion<int>()
                .HasDefaultValue(EmploymentHistoryType.Unknown)
                .IsRequired();

            entity.Property(x => x.OldStatus)
                .HasMaxLength(100);

            entity.Property(x => x.NewStatus)
                .HasMaxLength(100);

            entity.Property(x => x.EffectiveDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.ApprovedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.FilePath)
                .HasMaxLength(500);

            entity.Property(x => x.FileContentType)
                .HasMaxLength(100);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.EmploymentHistories)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OldDepartment)
                .WithMany()
                .HasForeignKey(x => x.OldDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NewDepartment)
                .WithMany()
                .HasForeignKey(x => x.NewDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OldPosition)
                .WithMany()
                .HasForeignKey(x => x.OldPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.NewPosition)
                .WithMany()
                .HasForeignKey(x => x.NewPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ApprovedByUser)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.HistoryType);

            entity.HasIndex(x => x.EffectiveDate);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.HistoryType,
                x.EffectiveDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.NewDepartmentId,
                x.NewPositionId
            });

            entity.HasIndex(x => x.ApprovedByUserId);
        }
    }
}
