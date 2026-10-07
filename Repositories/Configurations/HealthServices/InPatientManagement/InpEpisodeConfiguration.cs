using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpEpisodeConfiguration : IEntityTypeConfiguration<InpEpisode>
    {
        public void Configure(EntityTypeBuilder<InpEpisode> builder)
        {
            builder.ToTable("InpEpisode", "public");

            builder.HasKey(x => x.Id);
            // Versi untuk ExpectedVersion penutupan (integrasi-billing API 3.2). Bukan concurrency
            // token: penutupan memeriksa versi di bawah kunci FOR UPDATE, sehingga alur lama yang
            // menyunting episode tidak berubah perilaku saat terjadi tulis bersamaan.
            builder.Property(x => x.Version).HasDefaultValue(1);


            builder.Property(x => x.EpisodeNumber).IsRequired().HasMaxLength(50);
            builder.Property(x => x.EpisodeStatus).HasConversion<int>();
            builder.Property(x => x.DischargeType).HasConversion<int>();
            builder.Property(x => x.IsolationSource).HasConversion<int>();

            // Kontrak integrasi-billing 1.1.0 kamus data 6.3 — jejak status kasir yang diamati.
            builder.Property(x => x.DepartureClearanceObserved).HasConversion<int>();
            builder.Property(x => x.ClosureClearanceObserved).HasConversion<int>();
            builder.Property(x => x.DepartureClearanceWarningAcknowledged).HasDefaultValue(false);
            builder.HasIndex(x => x.DepartureClearanceObserved, "IX_InpEpisode_DepartureClearanceObserved");
            builder.Property(x => x.IsolationNote).HasMaxLength(500);
            builder.Property(x => x.ClosedWithoutClearanceReason).HasMaxLength(500);
            builder.Property(x => x.CancelReason).HasMaxLength(500);
            builder.Property(x => x.Notes).HasMaxLength(1000);

            builder.HasIndex(x => x.EpisodeNumber).IsUnique();
            builder.HasIndex(x => x.EncounterId).IsUnique();
            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.ServiceUnitId);
            builder.HasIndex(x => x.PatientClassId);
            builder.HasIndex(x => x.EpisodeStatus);
            builder.HasIndex(x => x.AdmittedAt);
            builder.HasIndex(x => x.DischargeDecidedAt);
            builder.HasIndex(x => x.PhysicallyLeftAt);
            builder.HasIndex(x => x.MotherEpisodeId);
            builder.HasIndex(x => x.RequiresIsolation);
            builder.HasIndex(x => x.IsolationSetByDoctorId);
            builder.HasIndex(x => x.ClosedAt);
            builder.HasIndex(x => x.IsClosedWithoutFinancialClearance);

            // INV-INP-10 — satu pasien paling banyak satu episode yang benar-benar hadir.
            // 1 = Admitted, 2 = DischargePending yang kepergiannya belum dicatat.
            builder.HasIndex(x => x.PatientId, "IX_InpEpisode_PatientId_Present")
                .IsUnique()
                .HasFilter("\"EpisodeStatus\" = 1 OR (\"EpisodeStatus\" = 2 AND \"PhysicallyLeftAt\" IS NULL)");

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

            builder.HasOne(x => x.PatientClass)
                .WithMany()
                .HasForeignKey(x => x.PatientClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PhysicallyLeftByUser)
                .WithMany()
                .HasForeignKey(x => x.PhysicallyLeftByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MotherEpisode)
                .WithMany()
                .HasForeignKey(x => x.MotherEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.IsolationSetByUser)
                .WithMany()
                .HasForeignKey(x => x.IsolationSetByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.IsolationSetByDoctor)
                .WithMany()
                .HasForeignKey(x => x.IsolationSetByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
