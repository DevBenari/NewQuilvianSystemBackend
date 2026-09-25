using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan sesi HD — <c>data-dictionary.md</c> bagian 3.8.</summary>
    /// <remarks>
    /// <para>
    /// <b>Tiga index pencegahan tabrakan bukan unique</b>, karena tumpang tindih waktu tidak dapat
    /// dinyatakan sebagai keunikan sederhana. Pemeriksaannya dijalankan <c>HmdScheduleService</c>
    /// di dalam transaksi berkunci, dan index ini membuat pemeriksaan itu cepat.
    /// </para>
    /// <para>
    /// <b>Dua index unik bersyarat</b> menjaga dari sisi basis data: satu sesi tidak pernah punya
    /// dua tindakan pasien, dan satu kunci idempotency tombol Mulai tidak pernah dipakai dua sesi.
    /// </para>
    /// </remarks>
    public class HmdSessionConfiguration : IEntityTypeConfiguration<HmdSession>
    {
        public void Configure(EntityTypeBuilder<HmdSession> builder)
        {
            builder.ToTable("HmdSession", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.SessionNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Shift).HasConversion<int>().IsRequired();
            builder.Property(x => x.SessionStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StopReason).HasConversion<int?>();
            builder.Property(x => x.Disposition).HasConversion<int?>();
            builder.Property(x => x.BillingHandoffStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.HoldReason).HasMaxLength(500);
            builder.Property(x => x.StopNote).HasMaxLength(1000);
            builder.Property(x => x.DeviationNote).HasMaxLength(1000);
            builder.Property(x => x.DispositionNote).HasMaxLength(1000);
            builder.Property(x => x.CancelReason).HasMaxLength(500);
            builder.Property(x => x.ReturnReason).HasMaxLength(500);
            builder.Property(x => x.RecordHash).HasMaxLength(64);
            builder.Property(x => x.IdempotencyKey).HasMaxLength(100);
            builder.Property(x => x.BillingHandoffError).HasMaxLength(1000);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.SessionNumber).IsUnique();
            builder.HasIndex(x => x.PatientProcedureId)
                .IsUnique()
                .HasFilter("\"PatientProcedureId\" IS NOT NULL");
            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            builder.HasIndex(x => new { x.MachineId, x.ScheduledStartAt, x.ScheduledEndAt })
                .HasDatabaseName("IX_HmdSession_Machine_Window");
            builder.HasIndex(x => new { x.StationId, x.ScheduledStartAt, x.ScheduledEndAt })
                .HasDatabaseName("IX_HmdSession_Station_Window");
            builder.HasIndex(x => new { x.EpisodeId, x.ScheduledStartAt })
                .HasDatabaseName("IX_HmdSession_Episode_Window");

            builder.HasIndex(x => new { x.ScheduledDate, x.Shift });
            builder.HasIndex(x => x.PrescriptionId);
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.InpEpisodeId);
            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(x => x.ResponsibleDoctorId);
            builder.HasIndex(x => x.SessionStatus);
            builder.HasIndex(x => x.StartedAt);
            builder.HasIndex(x => x.StopReason);
            builder.HasIndex(x => x.BillingHandoffStatus);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.Sessions)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Prescription)
                .WithMany()
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InpEpisode)
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Machine)
                .WithMany()
                .HasForeignKey(x => x.MachineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Station)
                .WithMany()
                .HasForeignKey(x => x.StationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ResponsibleDoctor)
                .WithMany()
                .HasForeignKey(x => x.ResponsibleDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientProcedure)
                .WithMany()
                .HasForeignKey(x => x.PatientProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
