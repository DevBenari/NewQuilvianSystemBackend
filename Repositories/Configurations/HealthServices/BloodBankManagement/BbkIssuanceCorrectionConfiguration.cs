using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BloodBankManagement
{
    /// <summary>
    /// Pemetaan catatan koreksi pencatatan pemberian (BD-DOM-23 / BE-BD-010).
    /// </summary>
    public class BbkIssuanceCorrectionConfiguration
        : IEntityTypeConfiguration<BbkIssuanceCorrection>
    {
        public void Configure(EntityTypeBuilder<BbkIssuanceCorrection> builder)
        {
            builder.ToTable("BbkIssuanceCorrection", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.BloodUnitId)
                .IsRequired();

            builder.Property(x => x.WhatWasWrong)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.WhatIsCorrect)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(x => x.ReasonCode)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.SupportingEvidenceNote)
                .HasMaxLength(1000)
                .IsRequired();

            // Token konkurensi keputusan: UPDATE hanya berhasil bila status masih seperti saat
            // dibaca, sehingga dua pemutus serentak tidak dapat sama-sama memutuskan (VAL-BD-075).
            builder.Property(x => x.CorrectionStatus)
                .HasConversion<int>()
                .IsRequired()
                .IsConcurrencyToken();

            builder.Property(x => x.RequestedByUserId)
                .IsRequired();

            builder.Property(x => x.RequestedAt)
                .IsRequired();

            builder.Property(x => x.DecisionNote)
                .HasMaxLength(500);

            builder.HasIndex(
                x => x.BloodUnitId,
                "IX_BbkIssuanceCorrection_BloodUnitId");

            // Ringkasan pemenuhan menyaring koreksi Approved (INV-BD-033).
            builder.HasIndex(
                x => x.CorrectionStatus,
                "IX_BbkIssuanceCorrection_CorrectionStatus");

            builder.HasIndex(
                x => x.RequestedByUserId,
                "IX_BbkIssuanceCorrection_RequestedByUserId");

            builder.HasOne(x => x.BloodUnit)
                .WithMany()
                .HasForeignKey(x => x.BloodUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
