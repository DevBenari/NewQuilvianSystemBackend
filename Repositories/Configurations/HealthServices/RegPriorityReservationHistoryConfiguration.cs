using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Pemetaan riwayat Reservasi Prioritas — rawat-jalan <c>data/data-dictionary.md</c> RP-3.
    /// </summary>
    public class RegPriorityReservationHistoryConfiguration : IEntityTypeConfiguration<RegPriorityReservationHistory>
    {
        public void Configure(EntityTypeBuilder<RegPriorityReservationHistory> builder)
        {
            builder.ToTable("RegPriorityReservationHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventCode).HasMaxLength(40).IsRequired();
            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Source).HasConversion<int>().IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500);
            builder.Property(x => x.FromVisitDate).HasColumnType("date");
            builder.Property(x => x.ToVisitDate).HasColumnType("date");
            builder.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();

            builder.HasIndex(x => new { x.PriorityReservationId, x.OccurredAt });

            builder.HasOne(x => x.PriorityReservation)
                .WithMany(x => x.Histories)
                .HasForeignKey(x => x.PriorityReservationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
