using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.FeeCalculation
{
    /// <summary>
    /// Konfigurasi EF Core penyerahan hasil jasa medis ke Finance AP — data-dictionary.md bagian 9, validation-matrix.md bagian 7.
    /// </summary>
    public class MdfFinanceHandoffConfiguration : IEntityTypeConfiguration<MdfFinanceHandoff>
    {
        public void Configure(EntityTypeBuilder<MdfFinanceHandoff> builder)
        {
            builder.ToTable("MdfFinanceHandoff", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfFinanceHandoff_Status", "\"Status\" IN ('Created','Acknowledged','Failed')");
                table.HasCheckConstraint("CK_MdfFinanceHandoff_Amount", "\"GrossAmount\" >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ServiceFeeId).IsRequired();
            builder.Property(x => x.HandoffKey).IsRequired();
            builder.Property(x => x.PayeeType).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PayeeReferenceId).IsRequired();
            builder.Property(x => x.PeriodCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.GrossAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(MdfFinanceHandoffStatuses.Created);
            builder.Property(x => x.CorrelationId).IsRequired();
            builder.Property(x => x.CausationId).IsRequired(false);
            builder.Property(x => x.AcknowledgedAt).IsRequired(false);
            builder.Property(x => x.FailureReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // Satu hasil jasa paling banyak satu penyerahan aktif (validation-matrix 7.5, T-73)
            builder.HasIndex(x => x.ServiceFeeId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // HandoffKey unik di antara baris aktif untuk idempoten Finance (validation-matrix 7.6, T-76)
            builder.HasIndex(x => x.HandoffKey)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Index pencarian status dan riwayat pembuatan
            builder.HasIndex(x => new { x.Status, x.CreateDateTime });

            // Index pencarian penyerahan per periode dan penerima
            builder.HasIndex(x => new { x.PeriodCode, x.PayeeType });

            // Relasi ke MdfServiceFee (1-to-1, Restrict)
            builder.HasOne(x => x.ServiceFee)
                .WithOne(x => x.FinanceHandoff)
                .HasForeignKey<MdfFinanceHandoff>(x => x.ServiceFeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

