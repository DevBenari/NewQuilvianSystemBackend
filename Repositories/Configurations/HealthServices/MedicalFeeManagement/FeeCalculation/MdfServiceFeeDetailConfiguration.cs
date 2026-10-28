using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.FeeCalculation
{
    /// <summary>
    /// Konfigurasi EF Core rincian perhitungan jasa per layanan — data-dictionary.md bagian 6, validation-matrix.md bagian 5.
    /// </summary>
    public class MdfServiceFeeDetailConfiguration : IEntityTypeConfiguration<MdfServiceFeeDetail>
    {
        public void Configure(EntityTypeBuilder<MdfServiceFeeDetail> builder)
        {
            builder.ToTable("MdfServiceFeeDetail", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfServiceFeeDetail_Percentage", "\"SharingPercentage\" >= 0 AND \"SharingPercentage\" <= 100");
                table.HasCheckConstraint("CK_MdfServiceFeeDetail_Base", "\"BaseAmount\" >= 0");
                table.HasCheckConstraint("CK_MdfServiceFeeDetail_Source", "\"SourceDomain\" IN ('OPERATING_ROOM','PROCEDURE','LABORATORY','ADHOC','ADHOC_CATALOG')");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ServiceFeeId).IsRequired();
            builder.Property(x => x.SourceDomain).HasMaxLength(30).IsRequired();
            builder.Property(x => x.SourceDetailId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RoleId).IsRequired();
            builder.Property(x => x.BaseAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.SharingPercentage).HasPrecision(5, 2).IsRequired();
            builder.Property(x => x.CalculatedAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.SharingRuleId).IsRequired();
            builder.Property(x => x.ServiceDate).HasColumnType("date").IsRequired();

            // Index pencarian rincian berdasarkan induk hasil jasa dan sumber layanan
            builder.HasIndex(x => x.ServiceFeeId);
            builder.HasIndex(x => new { x.SourceDomain, x.SourceDetailId });
            builder.HasIndex(x => x.InvoiceItemId);
            builder.HasIndex(x => x.RoleId);
            builder.HasIndex(x => x.SharingRuleId);

            // Relasi ke MdfServiceFee (Cascade)
            builder.HasOne(x => x.ServiceFee)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.ServiceFeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relasi ke tarif layanan (Restrict)
            builder.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi ke peran jasa medis (Restrict)
            builder.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi ke baris aturan tarif sharing asal (Restrict) — DoD: SharingRuleId wajib
            builder.HasOne(x => x.SharingRule)
                .WithMany()
                .HasForeignKey(x => x.SharingRuleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

