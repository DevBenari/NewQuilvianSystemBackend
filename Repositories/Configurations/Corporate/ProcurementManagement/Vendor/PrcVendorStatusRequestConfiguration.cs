using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Vendor
{
    /// <summary>
    /// Usulan blokir/buka blokir — kamus data manajemen-vendor bagian 4. Nomor UBV unik bila terisi (QBE-CODE-004);
    /// satu usulan terbuka per vendor dijaga index unik parsial (PBJ-VAL-107). Alasan wajib dijaga CHECK.
    /// </summary>
    public class PrcVendorStatusRequestConfiguration : IEntityTypeConfiguration<PrcVendorStatusRequest>
    {
        public void Configure(EntityTypeBuilder<PrcVendorStatusRequest> builder)
        {
            builder.ToTable("PrcVendorStatusRequest", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_PrcVendorStatusRequest_RequestType",
                    "\"RequestType\" IN ('BLOCK', 'UNBLOCK')");
                table.HasCheckConstraint(
                    "CK_PrcVendorStatusRequest_Reason",
                    "length(btrim(\"Reason\")) > 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RequestNumber).HasMaxLength(30);
            builder.Property(x => x.RequestType).HasMaxLength(10).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();

            // PBJ-VAL-090: dua orang memproses usulan yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.RequestNumber)
                .IsUnique()
                .HasFilter("\"RequestNumber\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => x.VendorProfileId)
                .IsUnique()
                .HasFilter("\"Status\" IN ('DRAFT', 'PENDING_APPROVAL', 'RETURNED') AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcVendorStatusRequest_OpenPerVendor");
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.WorkflowInstanceId);
            builder.HasIndex(x => x.EvidenceEvaluationId);

            builder.HasOne(x => x.VendorProfile).WithMany()
                .HasForeignKey(x => x.VendorProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EvidenceEvaluation).WithMany()
                .HasForeignKey(x => x.EvidenceEvaluationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
