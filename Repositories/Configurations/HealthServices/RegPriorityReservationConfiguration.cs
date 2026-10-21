using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Pemetaan Reservasi Prioritas — rawat-jalan <c>data/data-dictionary.md</c> RP-2 dan RP-5.
    /// </summary>
    public class RegPriorityReservationConfiguration : IEntityTypeConfiguration<RegPriorityReservation>
    {
        public void Configure(EntityTypeBuilder<RegPriorityReservation> builder)
        {
            builder.ToTable("RegPriorityReservation", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReservationNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PatientId).IsRequired();
            builder.Property(x => x.VisitDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(PriorityReservationStatus.AwaitingArrival)
                .IsRequired();
            builder.Property(x => x.ArrivalCodeHash).HasMaxLength(64).IsRequired();
            builder.Property(x => x.ArrivalCodeIssuedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.ArrivedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ArrivalSource).HasConversion<int?>();
            builder.Property(x => x.ScreenedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ClosureReason).HasMaxLength(500);
            builder.Property(x => x.IsPrioritySnapshot).HasDefaultValue(false);
            builder.Property(x => x.MembershipTierCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.QueuePriorityLevelSnapshot).HasDefaultValue(0);
            builder.Property(x => x.QueueAudienceSnapshot)
                .HasConversion<int>()
                .HasDefaultValue(QueueAudience.Regular)
                .IsRequired();
            builder.Property(x => x.PublicDisplayModeSnapshot)
                .HasConversion<int>()
                .HasDefaultValue(PublicDisplayMode.Default)
                .IsRequired();
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.ReservationNumber).IsUnique();
            builder.HasIndex(x => x.ArrivalCodeHash).IsUnique();
            builder.HasIndex(x => x.EncounterId)
                .IsUnique()
                .HasFilter("\"EncounterId\" IS NOT NULL");

            // INV-RP-01: satu reservasi terbuka (Menunggu Kedatangan / Sudah Tiba) per pasien per tanggal.
            builder.HasIndex(x => new { x.PatientId, x.VisitDate })
                .IsUnique()
                .HasDatabaseName("UX_RegPriorityReservation_Open_Patient_VisitDate")
                .HasFilter("\"Status\" IN (1, 2) AND NOT \"IsDelete\"");

            builder.HasIndex(x => new { x.VisitDate, x.Status });
            builder.HasIndex(x => x.PatientMembershipIdSnapshot);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PatientMembershipSnapshot)
                .WithMany()
                .HasForeignKey(x => x.PatientMembershipIdSnapshot)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
