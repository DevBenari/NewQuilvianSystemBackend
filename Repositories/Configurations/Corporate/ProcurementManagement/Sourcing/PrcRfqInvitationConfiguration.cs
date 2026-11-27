using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Undangan RFQ — kamus data bagian 4. Satu undangan per vendor per RFQ; vendor Dibatasi wajib beralasan (PBJ-VAL-131).
    /// </summary>
    public class PrcRfqInvitationConfiguration : IEntityTypeConfiguration<PrcRfqInvitation>
    {
        public void Configure(EntityTypeBuilder<PrcRfqInvitation> builder)
        {
            builder.ToTable("PrcRfqInvitation", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcRfqInvitation_RestrictedReason", "NOT \"WasRestricted\" OR length(btrim(coalesce(\"RestrictedReason\", ''))) > 0");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.RestrictedReason).HasMaxLength(500);

            builder.HasIndex(x => new { x.RfqId, x.SupplierId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcRfqInvitation_RfqId_SupplierId");
            builder.HasIndex(x => x.SupplierId);

            builder.HasOne(x => x.Rfq).WithMany(x => x.Invitations)
                .HasForeignKey(x => x.RfqId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Supplier).WithMany()
                .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
