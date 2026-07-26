using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class TrxEmployeeDocumentIssuanceConfiguration : IEntityTypeConfiguration<TrxEmployeeDocumentIssuance>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeDocumentIssuance> entity)
        {
            entity.ToTable("TrxEmployeeDocumentIssuance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ValidFrom).HasColumnType("date");
            entity.Property(x => x.ValidUntil).HasColumnType("date");
            entity.Property(x => x.DigitallySignedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.VersionNumber).HasDefaultValue(1);
            entity.Property(x => x.IsDigitallySigned).HasDefaultValue(false);
            entity.Property(x => x.IsEmployeeDownloadAllowed).HasDefaultValue(true);
            entity.Property(x => x.IsRevoked).HasDefaultValue(false);
            entity.Property(x => x.DocumentSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.EmployeeDocumentRequest).WithMany(x => x.Issuances).HasForeignKey(x => x.EmployeeDocumentRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceDocument).WithMany().HasForeignKey(x => x.WorkforceDocumentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.IssuedByUser).WithMany().HasForeignKey(x => x.IssuedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.DigitallySignedByUser).WithMany().HasForeignKey(x => x.DigitallySignedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RevokedByUser).WithMany().HasForeignKey(x => x.RevokedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.IssuanceNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.EmployeeDocumentRequestId, x.VersionNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.IssuedAt, x.IsRevoked });
            entity.HasIndex(x => x.FileChecksum);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxEmployeeDocumentIssuance> entity)
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
