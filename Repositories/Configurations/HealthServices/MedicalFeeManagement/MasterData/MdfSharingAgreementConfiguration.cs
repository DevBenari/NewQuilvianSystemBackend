using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalFeeManagement.MasterData
{
    /// <summary>
    /// Konfigurasi EF Core kesepakatan tarif sharing — data-dictionary.md bagian 2, validation-matrix.md bagian 2.
    /// </summary>
    public class MdfSharingAgreementConfiguration : IEntityTypeConfiguration<MdfSharingAgreement>
    {
        public void Configure(EntityTypeBuilder<MdfSharingAgreement> builder)
        {
            builder.ToTable("MdfSharingAgreement", "public", table =>
            {
                table.HasCheckConstraint("CK_MdfSharingAgreement_Status", "\"Status\" IN ('Draft','Active','Expired','Terminated')");
                table.HasCheckConstraint("CK_MdfSharingAgreement_PayeeType", "\"PayeeType\" IN ('Doctor','Workforce')");
                table.HasCheckConstraint("CK_MdfSharingAgreement_Period", "\"EffectiveEnd\" IS NULL OR \"EffectiveEnd\" >= \"EffectiveStart\"");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AgreementNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.SourceContractHistoryId).IsRequired();
            builder.Property(x => x.PayeeType).HasMaxLength(30).IsRequired().HasDefaultValue(MdfPayeeTypes.Doctor);
            builder.Property(x => x.PayeeReferenceId).IsRequired();
            builder.Property(x => x.EffectiveStart).HasColumnType("date").IsRequired();
            builder.Property(x => x.EffectiveEnd).HasColumnType("date");
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(MdfSharingAgreementStatuses.Draft);
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // AgreementNumber unik di antara baris yang belum dihapus (validation-matrix 2.1).
            builder.HasIndex(x => x.AgreementNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Index pencarian relasi kontrak dan filter penerima/status
            builder.HasIndex(x => x.SourceContractHistoryId);
            builder.HasIndex(x => new { x.PayeeType, x.PayeeReferenceId, x.Status });

            // FK Restrict ke WfpContractHistory (kontrak HR yang dirujuk tidak boleh dihapus)
            builder.HasOne(x => x.SourceContractHistory)
                .WithMany()
                .HasForeignKey(x => x.SourceContractHistoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relasi ke MdfSharingRule (Cascade jika aggregate root kesepakatan dihapus)
            builder.HasMany(x => x.SharingRules)
                .WithOne(x => x.Agreement)
                .HasForeignKey(x => x.AgreementId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

