using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class WfpComplianceAlertLogConfiguration : IEntityTypeConfiguration<WfpComplianceAlertLog>
    {
        public void Configure(EntityTypeBuilder<WfpComplianceAlertLog> entity)
        {
            entity.ToTable("WfpComplianceAlertLog", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PerformedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.ComplianceAlert)
                .WithMany(x => x.Logs)
                .HasForeignKey(x => x.ComplianceAlertId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PerformedByUser)
                .WithMany()
                .HasForeignKey(x => x.PerformedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ComplianceAlertId, x.PerformedAt });

            entity.HasIndex(x => new { x.LogType, x.PerformedAt });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<WfpComplianceAlertLog> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
