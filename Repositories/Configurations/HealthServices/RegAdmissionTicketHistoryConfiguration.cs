using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Pemetaan riwayat tiket antrean admisi (blueprint Cetak Antrean Admisi v1.1 bagian 11,
    /// baris Audit perubahan).
    /// </summary>
    public class RegAdmissionTicketHistoryConfiguration : IEntityTypeConfiguration<RegAdmissionTicketHistory>
    {
        public void Configure(EntityTypeBuilder<RegAdmissionTicketHistory> builder)
        {
            builder.ToTable("RegAdmissionTicketHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventCode).HasMaxLength(40).IsRequired();
            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.CounterCode).HasMaxLength(30);
            builder.Property(x => x.Reason).HasMaxLength(500);
            builder.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();

            builder.HasIndex(x => new { x.AdmissionTicketId, x.OccurredAt });

            builder.HasOne(x => x.AdmissionTicket)
                .WithMany(x => x.Histories)
                .HasForeignKey(x => x.AdmissionTicketId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
