using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Paket pengadaan — kamus data pemilihan-penyedia bagian 3. Nomor paket dan nomor PO unik bila terisi (QBE-CODE-004,
    /// PBJ-DEC-011); status dibatasi CHECK; alasan metode di luar rentang wajib bila ditandai (PBJ-DEC-068).
    /// </summary>
    public class PrcPackageConfiguration : IEntityTypeConfiguration<PrcPackage>
    {
        public void Configure(EntityTypeBuilder<PrcPackage> builder)
        {
            builder.ToTable("PrcPackage", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcPackage_Status", "\"Status\" IN ('DRAFT', 'PREPARATION_PENDING_APPROVAL', 'PREPARATION_RETURNED', 'READY_FOR_RFQ', 'RFQ_OPEN', 'EVALUATION', 'AWARD_PENDING_APPROVAL', 'AWARDED', 'ORDERED', 'REJECTED', 'CANCELLED')");
                table.HasCheckConstraint("CK_PrcPackage_MethodDeviationReason", "NOT \"IsMethodOutOfRange\" OR length(btrim(coalesce(\"MethodDeviationReason\", ''))) > 0");
                table.HasCheckConstraint("CK_PrcPackage_HpsTotal", "\"HpsTotal\" >= 0");
                table.HasCheckConstraint("CK_PrcPackage_PreparationSubmissionCount", "\"PreparationSubmissionCount\" >= 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PackageNumber).HasMaxLength(20);
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.MethodCode).HasMaxLength(30);
            builder.Property(x => x.MethodName).HasMaxLength(150);
            builder.Property(x => x.MethodMinHpsAmount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.MethodMaxHpsAmount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.HpsTotal).HasColumnType("numeric(18,2)");
            builder.Property(x => x.ApprovedHpsTotal).HasColumnType("numeric(18,2)");
            builder.Property(x => x.MethodDeviationReason).HasMaxLength(500);
            builder.Property(x => x.PurchaseOrderNumber).HasMaxLength(50);
            builder.Property(x => x.CancelReason).HasMaxLength(500);

            // PBJ-VAL-090: dua petugas menyimpan paket yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.PackageNumber)
                .IsUnique()
                .HasFilter("\"PackageNumber\" IS NOT NULL AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcPackage_PackageNumber");
            builder.HasIndex(x => x.PurchaseOrderNumber)
                .IsUnique()
                .HasFilter("\"PurchaseOrderNumber\" IS NOT NULL AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcPackage_PurchaseOrderNumber");
            builder.HasIndex(x => x.ProcurementTypeId);
            builder.HasIndex(x => x.FiscalYear);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PicUserId);
            builder.HasIndex(x => x.PreparationWorkflowInstanceId);

            builder.HasOne(x => x.ProcurementType).WithMany()
                .HasForeignKey(x => x.ProcurementTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PackagePlan).WithMany()
                .HasForeignKey(x => x.PackagePlanId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SplitFromPackage).WithMany()
                .HasForeignKey(x => x.SplitFromPackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Method).WithMany()
                .HasForeignKey(x => x.MethodId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
