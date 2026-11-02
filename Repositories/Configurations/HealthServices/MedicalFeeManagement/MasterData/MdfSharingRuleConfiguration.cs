using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.MasterData
{
    /// <summary>
    /// Konfigurasi EF Core baris aturan tarif sharing — data-dictionary.md bagian 3, validation-matrix.md bagian 3.
    /// </summary>
    public class MdfSharingRuleConfiguration : IEntityTypeConfiguration<MdfSharingRule>
    {
        public void Configure(EntityTypeBuilder<MdfSharingRule> builder)
        {
            builder.ToTable("MdfSharingRule", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfSharingRule_Percentage", "\"SharingPercentage\" >= 0 AND \"SharingPercentage\" <= 100");
                table.HasCheckConstraint("CK_MdfSharingRule_Period", "\"EffectiveEnd\" IS NULL OR \"EffectiveEnd\" >= \"EffectiveStart\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AgreementId).IsRequired();
            builder.Property(x => x.RoleId).IsRequired();
            builder.Property(x => x.SharingPercentage).HasPrecision(5, 2).IsRequired();
            builder.Property(x => x.EffectiveStart).HasColumnType("date").IsRequired();
            builder.Property(x => x.EffectiveEnd).HasColumnType("date");
            builder.Property(x => x.Notes).HasMaxLength(500);

            // Index pencarian aturan berdasarkan kesepakatan dan rentang waktu
            builder.HasIndex(x => new { x.AgreementId, x.EffectiveStart, x.EffectiveEnd });
            builder.HasIndex(x => x.TariffId);
            builder.HasIndex(x => x.TariffCategoryId);
            builder.HasIndex(x => x.RoleId);

            // Relasi ke kesepakatan induk (Cascade)
            builder.HasOne(x => x.Agreement)
                .WithMany(x => x.SharingRules)
                .HasForeignKey(x => x.AgreementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relasi ke tarif layanan (Restrict)
            builder.HasOne(x => x.Tariff)
                .WithMany()
                .HasForeignKey(x => x.TariffId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi ke kategori tarif (Restrict)
            builder.HasOne(x => x.TariffCategory)
                .WithMany()
                .HasForeignKey(x => x.TariffCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi ke peran pelaksana jasa medis (Restrict)
            builder.HasOne(x => x.Role)
                .WithMany(x => x.SharingRules)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi self-referencing versi pengganti (Restrict)
            builder.HasOne(x => x.SupersededByRule)
                .WithMany()
                .HasForeignKey(x => x.SupersededByRuleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

