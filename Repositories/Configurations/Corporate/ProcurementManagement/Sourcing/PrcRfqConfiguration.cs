using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// RFQ — kamus data bagian 4. Nomor RFQ unik bila terisi (QBE-CODE-004); satu RFQ DRAFT/SENT per paket; alasan gagal
    /// wajib saat FAILED (PBJ-DEC-075).
    /// </summary>
    public class PrcRfqConfiguration : IEntityTypeConfiguration<PrcRfq>
    {
        public void Configure(EntityTypeBuilder<PrcRfq> builder)
        {
            builder.ToTable("PrcRfq", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcRfq_Status", "\"Status\" IN ('DRAFT', 'SENT', 'CLOSED', 'FAILED', 'CANCELLED')");
                table.HasCheckConstraint("CK_PrcRfq_FailureReason", "\"Status\" <> 'FAILED' OR length(btrim(coalesce(\"FailureReason\", ''))) > 0");
                table.HasCheckConstraint("CK_PrcRfq_ExtensionCount", "\"ExtensionCount\" >= 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RfqNumber).HasMaxLength(20);
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.FailureReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.RfqNumber)
                .IsUnique()
                .HasFilter("\"RfqNumber\" IS NOT NULL AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcRfq_RfqNumber");
            builder.HasIndex(x => x.PackageId)
                .IsUnique()
                .HasFilter("\"Status\" IN ('DRAFT', 'SENT') AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcRfq_OpenPerPackage");
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.Package).WithMany()
                .HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.InheritedFromRfq).WithMany()
                .HasForeignKey(x => x.InheritedFromRfqId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
