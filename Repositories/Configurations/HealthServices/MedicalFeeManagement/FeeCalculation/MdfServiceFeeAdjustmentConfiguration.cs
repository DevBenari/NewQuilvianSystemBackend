using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.FeeCalculation
{
    /// <summary>
    /// Konfigurasi EF Core koreksi penyesuaian jasa medis — data-dictionary.md bagian 7, validation-matrix.md bagian 6.
    /// </summary>
    public class MdfServiceFeeAdjustmentConfiguration : IEntityTypeConfiguration<MdfServiceFeeAdjustment>
    {
        public void Configure(EntityTypeBuilder<MdfServiceFeeAdjustment> builder)
        {
            builder.ToTable("MdfServiceFeeAdjustment", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfServiceFeeAdjustment_Status", "\"Status\" IN ('Pending','Approved','Rejected')");
                table.HasCheckConstraint("CK_MdfServiceFeeAdjustment_Direction", "\"Direction\" IN ('Addition','Deduction')");
                table.HasCheckConstraint("CK_MdfServiceFeeAdjustment_Amount", "\"Amount\" > 0");
                table.HasCheckConstraint("CK_MdfServiceFeeAdjustment_Maker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AdjustmentNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ServiceFeeId).IsRequired();
            builder.Property(x => x.Direction).HasMaxLength(20).IsRequired().HasDefaultValue(MdfAdjustmentDirections.Addition);
            builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(MdfAdjustmentStatuses.Pending);
            builder.Property(x => x.RequestedBy).IsRequired();
            builder.Property(x => x.RequestedAt).IsRequired();
            builder.Property(x => x.RejectionReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // AdjustmentNumber unik di antara baris aktif
            builder.HasIndex(x => x.AdjustmentNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Index pencarian koreksi per hasil jasa dan status
            builder.HasIndex(x => new { x.ServiceFeeId, x.Status });

            // Relasi ke MdfServiceFee (Restrict)
            builder.HasOne(x => x.ServiceFee)
                .WithMany(x => x.Adjustments)
                .HasForeignKey(x => x.ServiceFeeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

