using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.FeeCalculation
{
    /// <summary>
    /// Konfigurasi EF Core hasil perhitungan jasa medis — data-dictionary.md bagian 5, validation-matrix.md bagian 6.
    /// </summary>
    public class MdfServiceFeeConfiguration : IEntityTypeConfiguration<MdfServiceFee>
    {
        public void Configure(EntityTypeBuilder<MdfServiceFee> builder)
        {
            builder.ToTable("MdfServiceFee", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfServiceFee_Status", "\"Status\" IN ('Calculated','Verified','Approved','HandedOff')");
                table.HasCheckConstraint("CK_MdfServiceFee_Total", "\"FinalAmount\" = \"GrossAmount\" + \"AdjustmentAmount\"");
                table.HasCheckConstraint("CK_MdfServiceFee_Gross", "\"GrossAmount\" >= 0");
                table.HasCheckConstraint("CK_MdfServiceFee_Maker", "\"VerifiedBy\" IS NULL OR \"ApprovedBy\" IS NULL OR \"VerifiedBy\" <> \"ApprovedBy\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FeeNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PeriodId).IsRequired();
            builder.Property(x => x.PayeeType).HasMaxLength(30).IsRequired().HasDefaultValue(MdfPayeeTypes.Doctor);
            builder.Property(x => x.PayeeReferenceId).IsRequired();
            builder.Property(x => x.GrossAmount).HasPrecision(18, 2).IsRequired().HasDefaultValue(0m);
            builder.Property(x => x.AdjustmentAmount).HasPrecision(18, 2).IsRequired().HasDefaultValue(0m);
            builder.Property(x => x.FinalAmount).HasPrecision(18, 2).IsRequired().HasDefaultValue(0m);
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(MdfServiceFeeStatuses.Calculated);
            builder.Property(x => x.CalculatedAt).IsRequired();
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // FeeNumber unik di antara baris aktif
            builder.HasIndex(x => x.FeeNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Satu penerima satu hasil jasa per periode (validation-matrix 6.3)
            builder.HasIndex(x => new { x.PeriodId, x.PayeeType, x.PayeeReferenceId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Index pencarian penerima dan status
            builder.HasIndex(x => new { x.PayeeType, x.PayeeReferenceId, x.Status });
            builder.HasIndex(x => new { x.PeriodId, x.Status });

            // Relasi ke MdfFeePeriod (Restrict)
            builder.HasOne(x => x.Period)
                .WithMany(x => x.ServiceFees)
                .HasForeignKey(x => x.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi ke MdfServiceFeeDetail (Cascade)
            builder.HasMany(x => x.Details)
                .WithOne(x => x.ServiceFee)
                .HasForeignKey(x => x.ServiceFeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relasi ke MdfServiceFeeAdjustment (Restrict jika masih ada riwayat koreksi)
            builder.HasMany(x => x.Adjustments)
                .WithOne(x => x.ServiceFee)
                .HasForeignKey(x => x.ServiceFeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

