using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Pemetaan tiket antrean Admisi Rawat Jalan (blueprint Cetak Antrean Admisi v1.1 bagian 6
    /// dan 11).
    /// </summary>
    public class RegAdmissionTicketConfiguration : IEntityTypeConfiguration<RegAdmissionTicket>
    {
        public void Configure(EntityTypeBuilder<RegAdmissionTicket> builder)
        {
            builder.ToTable("RegAdmissionTicket", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ServiceDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.QueueGroupCode).HasMaxLength(30).IsRequired();
            builder.Property(x => x.QueueGroupName).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Prefix).HasMaxLength(10).IsRequired();
            builder.Property(x => x.DisplayNumber).HasMaxLength(20).IsRequired();
            builder.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.IdempotencyKey).HasMaxLength(100);
            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(AdmissionTicketStatus.Waiting)
                .IsRequired();
            builder.Property(x => x.PrintStatus)
                .HasConversion<int>()
                .HasDefaultValue(AdmissionTicketPrintStatus.Unconfirmed)
                .IsRequired();
            builder.Property(x => x.PrintAttemptCount).HasDefaultValue(0);
            builder.Property(x => x.LastPrintedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.PrintFailureReason).HasMaxLength(250);
            builder.Property(x => x.CounterCode).HasMaxLength(30);
            builder.Property(x => x.CounterName).HasMaxLength(100);
            builder.Property(x => x.CalledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CallAttemptCount).HasDefaultValue(0);
            builder.Property(x => x.ServiceStartedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ServiceEndedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.SkipCount).HasDefaultValue(0);
            builder.Property(x => x.SkippedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.SkipReason).HasMaxLength(250);
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelReason).HasMaxLength(250);
            builder.Property(x => x.CompletionNote).HasMaxLength(500);
            builder.Property(x => x.RegistrationLinkedAt).HasColumnType("timestamp with time zone");

            // Bagian 6: nomor unik per tanggal pelayanan dan kelompok antrean.
            builder.HasIndex(x => new { x.ServiceDate, x.QueueGroupCode, x.SequenceNumber })
                .IsUnique()
                .HasDatabaseName("UX_RegAdmissionTicket_ServiceDate_Group_Sequence");

            // Bagian 6/12: ketukan ganda atau request ulang dari kiosk yang sama tidak menerbitkan tiket kedua.
            builder.HasIndex(x => new { x.IssuedByUserId, x.IdempotencyKey })
                .IsUnique()
                .HasDatabaseName("UX_RegAdmissionTicket_Issuer_IdempotencyKey")
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");

            // Bagian 10 butir 2: satu tiket admisi untuk satu kunjungan.
            builder.HasIndex(x => x.EncounterId)
                .IsUnique()
                .HasFilter("\"EncounterId\" IS NOT NULL");

            builder.HasIndex(x => new { x.ServiceDate, x.QueueGroupCode, x.Status });

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
