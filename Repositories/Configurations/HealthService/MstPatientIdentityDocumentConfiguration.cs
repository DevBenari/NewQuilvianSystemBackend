using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPatientIdentityDocumentConfiguration : IEntityTypeConfiguration<MstPatientIdentityDocument>
    {
        public void Configure(EntityTypeBuilder<MstPatientIdentityDocument> entity)
        {
            entity.ToTable("MstPatientIdentityDocument", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.IdentityType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.IdentityNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.DocumentName)
                .HasMaxLength(200);

            entity.Property(x => x.FilePath)
                .HasMaxLength(500);

            entity.Property(x => x.FileContentType)
                .HasMaxLength(100);

            entity.Property(x => x.IssuedBy)
                .HasMaxLength(100);

            entity.Property(x => x.IssueDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.ExpiredDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsVerified)
                .HasDefaultValue(false);

            entity.Property(x => x.VerifiedByUserId)
                .IsRequired(false);

            entity.Property(x => x.VerifiedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.VerificationNote)
                .HasMaxLength(250);

            entity.Property(x => x.IsFromKioskScan)
                .HasDefaultValue(false);

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

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.IdentityNumber);

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IdentityType,
                x.IdentityNumber
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IsPrimary
            })
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IsVerified,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsFromKioskScan,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
