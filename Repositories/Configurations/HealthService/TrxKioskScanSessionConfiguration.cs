using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class TrxKioskScanSessionConfiguration : IEntityTypeConfiguration<TrxKioskScanSession>
    {
        public void Configure(EntityTypeBuilder<TrxKioskScanSession> entity)
        {
            entity.ToTable("TrxKioskScanSession", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SessionCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ScanSource)
                .HasConversion<int>()
                .HasDefaultValue(KioskScanSource.Unknown)
                .IsRequired();

            entity.Property(x => x.ScanStatus)
                .HasConversion<int>()
                .HasDefaultValue(KioskScanSessionStatus.Started)
                .IsRequired();

            entity.Property(x => x.KioskDeviceId)
                .IsRequired(false);

            entity.Property(x => x.IdentityScannerProfileId)
                .IsRequired(false);

            entity.Property(x => x.PatientId)
                .IsRequired(false);

            entity.Property(x => x.IdentityType)
                .HasMaxLength(100);

            entity.Property(x => x.IdentityNumber)
                .HasMaxLength(100);

            entity.Property(x => x.CardNumber)
                .HasMaxLength(100);

            entity.Property(x => x.MemberNumber)
                .HasMaxLength(100);

            entity.Property(x => x.InsuranceCardNumber)
                .HasMaxLength(100);

            entity.Property(x => x.FullName)
                .HasMaxLength(200);

            entity.Property(x => x.BirthDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.GenderText)
                .HasMaxLength(50);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.RawScanText)
                .HasMaxLength(12000);

            entity.Property(x => x.ParsedJson)
                .HasMaxLength(12000);

            entity.Property(x => x.FailureReason)
                .HasMaxLength(250);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.UserAgent)
                .HasMaxLength(500);

            entity.Property(x => x.StartedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsPatientFound)
                .HasDefaultValue(false);

            entity.Property(x => x.IsManualInput)
                .HasDefaultValue(false);

            entity.Property(x => x.IsUsedForRegistration)
                .HasDefaultValue(false);

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

            entity.HasOne(x => x.KioskDevice)
                .WithMany()
                .HasForeignKey(x => x.KioskDeviceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.IdentityScannerProfile)
                .WithMany()
                .HasForeignKey(x => x.IdentityScannerProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SessionCode)
                .IsUnique();

            entity.HasIndex(x => x.KioskDeviceId);

            entity.HasIndex(x => x.IdentityScannerProfileId);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.IdentityNumber);

            entity.HasIndex(x => x.CardNumber);

            entity.HasIndex(x => x.MemberNumber);

            entity.HasIndex(x => new
            {
                x.ScanStatus,
                x.ScanSource,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IsUsedForRegistration,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.StartedAt,
                x.ScanStatus,
                x.IsDelete
            });
        }
    }
}
