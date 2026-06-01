using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpComplianceAlertConfiguration : IEntityTypeConfiguration<WfpComplianceAlert>
    {
        public void Configure(EntityTypeBuilder<WfpComplianceAlert> entity)
        {
            entity.ToTable("WfpComplianceAlert", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.SourceEntityName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.SourceEntityId)
                .IsRequired();

            entity.Property(x => x.AlertType)
                .HasConversion<int>()
                .HasDefaultValue(ComplianceAlertType.Unknown)
                .IsRequired();

            entity.Property(x => x.AlertTitle)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.AlertMessage)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.DueDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.AlertStatus)
                .HasConversion<int>()
                .HasDefaultValue(ComplianceAlertStatus.Open)
                .IsRequired();

            entity.Property(x => x.SeverityLevel)
                .HasConversion<int>()
                .HasDefaultValue(ComplianceAlertSeverityLevel.Low)
                .IsRequired();

            entity.Property(x => x.IsResolved)
                .HasDefaultValue(false);

            entity.Property(x => x.ResolvedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ResolvedByUserId)
                .IsRequired(false);

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
                .WithMany(x => x.ComplianceAlerts)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ResolvedByUser)
                .WithMany()
                .HasForeignKey(x => x.ResolvedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.ResolvedByUserId);

            entity.HasIndex(x => x.SourceEntityName);

            entity.HasIndex(x => x.SourceEntityId);

            entity.HasIndex(x => x.AlertType);

            entity.HasIndex(x => x.AlertStatus);

            entity.HasIndex(x => x.SeverityLevel);

            entity.HasIndex(x => x.DueDate);

            entity.HasIndex(x => new
            {
                x.SourceEntityName,
                x.SourceEntityId,
                x.AlertType,
                x.DueDate
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.AlertStatus,
                x.IsResolved,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.DueDate,
                x.AlertStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.DueDate,
                x.SeverityLevel,
                x.AlertStatus,
                x.IsResolved,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.SourceEntityName,
                x.SourceEntityId,
                x.IsDelete
            });
        }
    }
}
