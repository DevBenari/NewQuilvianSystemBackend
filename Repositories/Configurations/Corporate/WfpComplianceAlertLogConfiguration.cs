using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpComplianceAlertLogConfiguration : IEntityTypeConfiguration<WfpComplianceAlertLog>
    {
        public void Configure(EntityTypeBuilder<WfpComplianceAlertLog> entity)
        {
            entity.ToTable("WfpComplianceAlertLog", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ComplianceAlertId)
                .IsRequired();

            entity.Property(x => x.LogType)
                .HasConversion<int>()
                .HasDefaultValue(ComplianceAlertLogType.Created)
                .IsRequired();

            entity.Property(x => x.OldStatus)
                .HasConversion<int>()
                .IsRequired(false);

            entity.Property(x => x.NewStatus)
                .HasConversion<int>()
                .IsRequired(false);

            entity.Property(x => x.LogMessage)
                .HasMaxLength(1000);

            entity.Property(x => x.PerformedByUserId)
                .IsRequired(false);

            entity.Property(x => x.PerformedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

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

            entity.HasOne(x => x.ComplianceAlert)
                .WithMany(x => x.Logs)
                .HasForeignKey(x => x.ComplianceAlertId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformedByUser)
                .WithMany()
                .HasForeignKey(x => x.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ComplianceAlertId);

            entity.HasIndex(x => x.PerformedByUserId);

            entity.HasIndex(x => x.LogType);

            entity.HasIndex(x => x.PerformedAt);

            entity.HasIndex(x => new
            {
                x.ComplianceAlertId,
                x.LogType,
                x.PerformedAt,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PerformedByUserId,
                x.PerformedAt,
                x.IsDelete
            });
        }
    }
}
