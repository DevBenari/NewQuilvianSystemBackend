using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpHealthRecordConfiguration : IEntityTypeConfiguration<WfpHealthRecord>
    {
        public void Configure(EntityTypeBuilder<WfpHealthRecord> entity)
        {
            entity.ToTable("WfpHealthRecord", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.RequirementCode)
                .HasMaxLength(100);

            entity.Property(x => x.HealthRecordType)
                .HasConversion<int>()
                .HasDefaultValue(HealthRecordType.Unknown)
                .IsRequired();

            entity.Property(x => x.RecordDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.ResultStatus)
                .HasConversion<int>()
                .HasDefaultValue(HealthRecordResultStatus.Unknown)
                .IsRequired();

            entity.Property(x => x.ProviderName)
                .HasMaxLength(200);

            entity.Property(x => x.ExpiredDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.IsFitToWork)
                .IsRequired(false);

            entity.Property(x => x.FitToWorkRestrictionNote)
                .HasMaxLength(250);

            entity.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            entity.Property(x => x.VerifiedByUserId)
                .IsRequired(false);

            entity.Property(x => x.VerifiedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.VerificationNote)
                .HasMaxLength(250);

            entity.Property(x => x.FilePath)
                .HasMaxLength(500);

            entity.Property(x => x.FileContentType)
                .HasMaxLength(100);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

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
                .WithMany(x => x.HealthRecords)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.VerifiedByUser)
                .WithMany()
                .HasForeignKey(x => x.VerifiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.VerifiedByUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.HealthRecordType,
                x.RecordDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.HealthRecordType,
                x.ResultStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.IsVerified,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.IsFitToWork,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.HealthRecordType,
                x.ExpiredDate,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.ExpiredDate,
                x.IsVerified,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ExpiredDate,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
