using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Penetapan penyedia — kamus data bagian 5. Satu penetapan hidup per paket (PENDING_APPROVAL/RETURNED/APPROVED); status
    /// dibatasi CHECK; alasan wajib dijaga CHECK (PBJ-VAL-141, 142, 131; PBJ-DEC-092, 093).
    /// </summary>
    public class PrcPackageAwardConfiguration : IEntityTypeConfiguration<PrcPackageAward>
    {
        public void Configure(EntityTypeBuilder<PrcPackageAward> builder)
        {
            builder.ToTable("PrcPackageAward", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcPackageAward_Status", "\"Status\" IN ('PENDING_APPROVAL', 'RETURNED', 'APPROVED', 'REJECTED', 'WITHDRAWN', 'REVOKED', 'NOT_APPLIED')");
                table.HasCheckConstraint("CK_PrcPackageAward_OverHpsReason", "NOT \"IsOverHps\" OR length(btrim(coalesce(\"OverHpsReason\", ''))) > 0");
                table.HasCheckConstraint("CK_PrcPackageAward_NonLowestReason", "\"IsLowest\" OR length(btrim(coalesce(\"NonLowestReason\", ''))) > 0");
                table.HasCheckConstraint("CK_PrcPackageAward_RestrictedReason", "NOT \"IsRestrictedVendor\" OR length(btrim(coalesce(\"RestrictedReason\", ''))) > 0");
                table.HasCheckConstraint("CK_PrcPackageAward_RevokeReason", "\"Status\" <> 'REVOKED' OR length(btrim(coalesce(\"RevokeReason\", ''))) > 0");
                table.HasCheckConstraint("CK_PrcPackageAward_NotAppliedReason", "\"Status\" <> 'NOT_APPLIED' OR length(btrim(coalesce(\"NotAppliedReason\", ''))) > 0");
                table.HasCheckConstraint("CK_PrcPackageAward_SubmissionCount", "\"SubmissionCount\" >= 1");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.AwardTotal).HasColumnType("numeric(18,2)");
            builder.Property(x => x.HpsTotalSnapshot).HasColumnType("numeric(18,2)");
            builder.Property(x => x.OverHpsReason).HasMaxLength(500);
            builder.Property(x => x.NonLowestReason).HasMaxLength(500);
            builder.Property(x => x.RestrictedReason).HasMaxLength(500);
            builder.Property(x => x.RankingSnapshotJson).HasColumnType("jsonb").IsRequired();
            builder.Property(x => x.RevokeReason).HasMaxLength(500);
            builder.Property(x => x.NotAppliedReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.PackageId)
                .IsUnique()
                .HasFilter("\"Status\" IN ('PENDING_APPROVAL', 'RETURNED', 'APPROVED') AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcPackageAward_LivePerPackage");
            builder.HasIndex(x => x.QuotationId);
            builder.HasIndex(x => x.SupplierId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.WorkflowInstanceId);

            builder.HasOne(x => x.Package).WithMany()
                .HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Quotation).WithMany()
                .HasForeignKey(x => x.QuotationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
