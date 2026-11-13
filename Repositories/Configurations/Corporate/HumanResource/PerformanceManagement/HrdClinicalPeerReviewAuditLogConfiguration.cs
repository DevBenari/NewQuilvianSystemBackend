using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdClinicalPeerReviewAuditLogConfiguration : IEntityTypeConfiguration<HrdClinicalPeerReviewAuditLog>
    {
        public void Configure(EntityTypeBuilder<HrdClinicalPeerReviewAuditLog> builder)
        {
            builder.ToTable("HrdClinicalPeerReviewAuditLog", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RecordType).HasMaxLength(50).HasDefaultValue("OPPE_REPORT").IsRequired();
            builder.Property(x => x.AccessReason).HasMaxLength(250).IsRequired();
            builder.Property(x => x.AccessedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.IpAddress).HasMaxLength(50).HasDefaultValue("127.0.0.1").IsRequired();

            builder.HasIndex(x => x.AccessedWorkforceProfileId);
            builder.HasIndex(x => new { x.RecordType, x.RecordId });
        }
    }
}
