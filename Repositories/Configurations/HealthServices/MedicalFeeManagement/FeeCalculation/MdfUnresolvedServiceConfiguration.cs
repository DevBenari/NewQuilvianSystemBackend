using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.FeeCalculation
{
    /// <summary>
    /// Konfigurasi EF Core layanan medis belum dapat dihitung — data-dictionary.md bagian 8, validation-matrix.md bagian 5 & 7.
    /// </summary>
    public class MdfUnresolvedServiceConfiguration : IEntityTypeConfiguration<MdfUnresolvedService>
    {
        public void Configure(EntityTypeBuilder<MdfUnresolvedService> builder)
        {
            builder.ToTable("MdfUnresolvedService", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfUnresolvedService_Status", "\"Status\" IN ('Open','Resolved','Waived')");
                table.HasCheckConstraint("CK_MdfUnresolvedService_Reason", "\"Reason\" IN ('PERFORMER_MISSING','AGREEMENT_MISSING','RULE_MISSING','CONTRACT_EXPIRED','ROLE_UNMAPPED','SOURCE_UNSUPPORTED')");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PeriodId).IsRequired();
            builder.Property(x => x.SourceDomain).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SourceDetailId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ServiceDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.BaseAmount).HasPrecision(18, 2);
            builder.Property(x => x.Reason).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ReasonDetail).HasMaxLength(500);
            builder.Property(x => x.DetectedAt).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(MdfUnresolvedStatuses.Open);
            builder.Property(x => x.ResolutionNote).HasMaxLength(500);

            // Satu layanan tidak boleh muncul dua kali pada periode yang sama (validation-matrix 5.12)
            builder.HasIndex(x => new { x.PeriodId, x.SourceDomain, x.SourceDetailId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Index pencarian status penyelesaian dan kode alasan
            builder.HasIndex(x => new { x.PeriodId, x.Status });
            builder.HasIndex(x => x.Reason);

            // Relasi ke MdfFeePeriod (Cascade)
            builder.HasOne(x => x.Period)
                .WithMany(x => x.UnresolvedServices)
                .HasForeignKey(x => x.PeriodId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relasi ke tarif layanan (Restrict)
            builder.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

