using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptOrder</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptOrderConfiguration : IEntityTypeConfiguration<OptOrder>
    {
        public void Configure(EntityTypeBuilder<OptOrder> builder)
        {
            builder.ToTable("OptOrder", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderKind).HasConversion<int>().IsRequired();
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.Property(x => x.DownPaymentMinPercentSnapshot).HasPrecision(5, 2);
            builder.Property(x => x.WorkshopType).HasConversion<int?>();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.OrderNumber).IsUnique().HasDatabaseName("UX_OptOrder_OrderNumber");
            builder.HasIndex(x => new { x.EncounterId, x.Status }).HasDatabaseName("IX_OptOrder_EncounterId_Status");
            builder.HasIndex(x => new { x.Status, x.ReadyAt }).HasDatabaseName("IX_OptOrder_Status_ReadyAt");
            builder.HasIndex(x => x.PatientId).HasDatabaseName("IX_OptOrder_PatientId");
            builder.HasIndex(x => x.PrescriptionId).HasDatabaseName("IX_OptOrder_PrescriptionId");
            builder.HasIndex(x => x.ParentOrderId).HasDatabaseName("IX_OptOrder_ParentOrderId");
            builder.HasIndex(x => x.ExternalLabSupplierId).HasDatabaseName("IX_OptOrder_ExternalLabSupplierId");
            builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("\"IdempotencyKey\" IS NOT NULL").HasDatabaseName("UX_OptOrder_IdempotencyKey");

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OptPrescription>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OptPrescriptionVersion>()
                .WithMany()
                .HasForeignKey(x => x.PrescriptionVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstSupplier>()
                .WithMany()
                .HasForeignKey(x => x.ExternalLabSupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<OptOrder>()
                .WithMany()
                .HasForeignKey(x => x.ParentOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
