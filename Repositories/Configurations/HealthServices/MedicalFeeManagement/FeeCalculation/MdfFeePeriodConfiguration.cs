using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.FeeCalculation
{
    /// <summary>
    /// Konfigurasi EF Core periode kalkulasi jasa medis — data-dictionary.md bagian 4, validation-matrix.md bagian 4.
    /// </summary>
    public class MdfFeePeriodConfiguration : IEntityTypeConfiguration<MdfFeePeriod>
    {
        public void Configure(EntityTypeBuilder<MdfFeePeriod> builder)
        {
            builder.ToTable("MdfFeePeriod", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfFeePeriod_Status", "\"Status\" IN ('Open','Calculated','Verified','Approved','Closed')");
                table.HasCheckConstraint("CK_MdfFeePeriod_Range", "\"PeriodEnd\" >= \"PeriodStart\"");
                table.HasCheckConstraint("CK_MdfFeePeriod_Maker", "\"VerifiedBy\" IS NULL OR \"ApprovedBy\" IS NULL OR \"VerifiedBy\" <> \"ApprovedBy\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PeriodCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.PeriodStart).HasColumnType("date").IsRequired();
            builder.Property(x => x.PeriodEnd).HasColumnType("date").IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(MdfFeePeriodStatuses.Open);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // PeriodCode unik di antara baris aktif
            builder.HasIndex(x => x.PeriodCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Index pencarian status dan tanggal awal periode
            builder.HasIndex(x => new { x.Status, x.PeriodStart });

            // Relasi ke MdfServiceFee (Restrict jika periode masih memiliki rekap hasil jasa)
            builder.HasMany(x => x.ServiceFees)
                .WithOne(x => x.Period)
                .HasForeignKey(x => x.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi ke MdfUnresolvedService (Cascade jika periode dihapus)
            builder.HasMany(x => x.UnresolvedServices)
                .WithOne(x => x.Period)
                .HasForeignKey(x => x.PeriodId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

