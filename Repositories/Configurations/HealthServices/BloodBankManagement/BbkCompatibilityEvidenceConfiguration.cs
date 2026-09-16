using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>
    /// Pemetaan bukti kecocokan kantong darah (BD-DOM-07 / BE-BD-007).
    /// </summary>
    public class BbkCompatibilityEvidenceConfiguration
        : IEntityTypeConfiguration<BbkCompatibilityEvidence>
    {
        public void Configure(EntityTypeBuilder<BbkCompatibilityEvidence> builder)
        {
            builder.ToTable("BbkCompatibilityEvidence", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BloodUnitId)
                .IsRequired();

            builder.Property(x => x.PatientId)
                .IsRequired();

            builder.Property(x => x.EvidenceResult)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ValidatedByUserId)
                .IsRequired();

            builder.Property(x => x.CheckedAt)
                .IsRequired();

            builder.Property(x => x.IsSuperseded)
                .IsRequired();

            // Riwayat seluruh evidence satu kantong.
            builder.HasIndex(
                x => x.BloodUnitId,
                "IX_BbkCompatibilityEvidence_BloodUnitId");

            // Gerbang issuance mencari evidence per pasangan kantong+pasien.
            builder.HasIndex(
                x => new { x.BloodUnitId, x.PatientId },
                "IX_BbkCompatibilityEvidence_BloodUnitId_PatientId");

            builder.HasIndex(
                x => new
                {
                    x.BloodUnitId,
                    x.PatientId,
                    x.IsSuperseded,
                    x.CheckedAt
                },
                "IX_BbkCompatibilityEvidence_IssuanceLookup");

            builder.HasOne(x => x.BloodUnit)
                .WithMany()
                .HasForeignKey(x => x.BloodUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}