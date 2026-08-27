using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class TrxEmergencyVisitConfiguration : IEntityTypeConfiguration<TrxEmergencyVisit>
    {
        public void Configure(EntityTypeBuilder<TrxEmergencyVisit> builder)
        {
            builder.ToTable("TrxEmergencyVisit", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EmergencyVisitNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ChiefComplaint).HasMaxLength(1000);
            builder.Property(x => x.ArrivalLocation).HasMaxLength(250);
            builder.Property(x => x.FoundLocation).HasMaxLength(250);
            builder.Property(x => x.TraumaLocation).HasMaxLength(250);
            builder.Property(x => x.TemporaryPatientAlias).HasMaxLength(100);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.DuplicateEpisodeOverrideReason).HasMaxLength(1000);

            builder.Property(x => x.RegistrationStatus).HasConversion<int>();
            builder.Property(x => x.VisitStatus).HasConversion<int>();

            builder.HasIndex(x => x.EmergencyVisitNumber).IsUnique();
            builder.HasIndex(x => x.EncounterId).IsUnique();
            builder.HasIndex(x => new { x.PatientId, x.ArrivalDateTime });
            builder.HasIndex(x => new { x.ServiceUnitId, x.VisitStatus, x.ArrivalDateTime });
            builder.HasIndex(x => new { x.RegistrationStatus, x.ArrivalDateTime });

            // BE-IGD-025 - daftar pantau pemakaian jalan keluar episode ganda menyaring kolom
            // ini, dan jumlah baris yang terisi selalu jauh lebih kecil daripada seluruh
            // kunjungan, sehingga index parsial yang tepat.
            builder.HasIndex(x => x.DuplicateEpisodeOverrideAt)
                .HasFilter("\"DuplicateEpisodeOverrideAt\" IS NOT NULL");

            // Satu kunjungan hanya boleh menembus satu kunjungan lain, dan penembusan yang
            // sama tidak boleh tercatat dua kali.
            builder.HasIndex(x => x.DuplicateEpisodeOverrideOfVisitId)
                .HasFilter("\"DuplicateEpisodeOverrideOfVisitId\" IS NOT NULL");

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ArrivalMode)
                .WithMany(x => x.EmergencyVisits)
                .HasForeignKey(x => x.ArrivalModeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CaseType)
                .WithMany(x => x.EmergencyVisits)
                .HasForeignKey(x => x.CaseTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RegistrationCompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.RegistrationCompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Triages)
                .WithOne(x => x.EmergencyVisit)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Resuscitations)
                .WithOne(x => x.EmergencyVisit)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Observations)
                .WithOne(x => x.EmergencyVisit)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Dispositions)
                .WithOne(x => x.EmergencyVisit)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Departures)
                .WithOne(x => x.EmergencyVisit)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ProcedureDetails)
                .WithOne(x => x.EmergencyVisit)
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
