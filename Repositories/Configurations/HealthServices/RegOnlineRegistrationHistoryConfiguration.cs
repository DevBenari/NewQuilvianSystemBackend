using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Pemetaan riwayat pendaftaran online — kiosk <c>data/data-dictionary.md</c> PO-D.4.
    /// </summary>
    public class RegOnlineRegistrationHistoryConfiguration : IEntityTypeConfiguration<RegOnlineRegistrationHistory>
    {
        public void Configure(EntityTypeBuilder<RegOnlineRegistrationHistory> builder)
        {
            builder.ToTable("RegOnlineRegistrationHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OnlineRegistrationId).IsRequired();
            builder.Property(x => x.EventCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Source).HasConversion<int>().IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500);
            builder.Property(x => x.FromVisitDate).HasColumnType("date");
            builder.Property(x => x.ToVisitDate).HasColumnType("date");
            builder.Property(x => x.FromContact).HasMaxLength(150);
            builder.Property(x => x.ToContact).HasMaxLength(150);
            builder.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();

            builder.HasIndex(x => new { x.OnlineRegistrationId, x.OccurredAt });

            builder.HasOne(x => x.OnlineRegistration)
                .WithMany(x => x.Histories)
                .HasForeignKey(x => x.OnlineRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
