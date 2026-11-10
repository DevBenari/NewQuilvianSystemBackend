using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptWarrantyClaim</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptWarrantyClaimConfiguration : IEntityTypeConfiguration<OptWarrantyClaim>
    {
        public void Configure(EntityTypeBuilder<OptWarrantyClaim> builder)
        {
            builder.ToTable("OptWarrantyClaim", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComplaintType).HasConversion<int>().IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.ClaimNumber).IsUnique().HasDatabaseName("UX_OptWarrantyClaim_ClaimNumber");
            builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_OptWarrantyClaim_OrderId");
            builder.HasIndex(x => x.EncounterId).HasDatabaseName("IX_OptWarrantyClaim_EncounterId");
            builder.HasIndex(x => x.RemakeOrderId).IsUnique().HasFilter("\"RemakeOrderId\" IS NOT NULL").HasDatabaseName("UX_OptWarrantyClaim_RemakeOrderId");
            builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL").HasDatabaseName("UX_OptWarrantyClaim_IdempotencyKey");

            builder.HasOne<OptOrder>()
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstWorkforceProfile>()
                .WithMany()
                .HasForeignKey(x => x.ExaminedByWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OptRefractionExam>()
                .WithMany()
                .HasForeignKey(x => x.ReexamRefractionExamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OptOrder>()
                .WithMany()
                .HasForeignKey(x => x.RemakeOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
