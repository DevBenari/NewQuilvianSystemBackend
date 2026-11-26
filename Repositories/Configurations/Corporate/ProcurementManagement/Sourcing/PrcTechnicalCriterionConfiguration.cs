using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Kriteria teknis — kamus data bagian 4. Nomor butir positif dan unik per item paket.
    /// </summary>
    public class PrcTechnicalCriterionConfiguration : IEntityTypeConfiguration<PrcTechnicalCriterion>
    {
        public void Configure(EntityTypeBuilder<PrcTechnicalCriterion> builder)
        {
            builder.ToTable("PrcTechnicalCriterion", "public", table =>
            {
                table.HasCheckConstraint("CK_PrcTechnicalCriterion_CriterionNo", "\"CriterionNo\" >= 1");
            });
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CriterionText).HasMaxLength(500).IsRequired();

            builder.HasIndex(x => new { x.PackageItemId, x.CriterionNo })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_PrcTechnicalCriterion_PackageItemId_CriterionNo");

            builder.HasOne(x => x.PackageItem).WithMany(x => x.Criteria)
                .HasForeignKey(x => x.PackageItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
