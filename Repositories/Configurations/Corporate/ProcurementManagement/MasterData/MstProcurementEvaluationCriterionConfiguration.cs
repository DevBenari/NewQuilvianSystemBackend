using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    /// <summary>
    /// Kriteria penilaian vendor — kamus data manajemen-vendor bagian 2. Nama unik per kategori di antara baris
    /// aktif; pemeriksaan tanpa beda huruf dilakukan service sebelum simpan. Bobot 0 &lt; w ≤ 100 dijaga CHECK
    /// (PBJ-VAL-113).
    /// </summary>
    public class MstProcurementEvaluationCriterionConfiguration : IEntityTypeConfiguration<MstProcurementEvaluationCriterion>
    {
        public void Configure(EntityTypeBuilder<MstProcurementEvaluationCriterion> builder)
        {
            builder.ToTable("MstProcurementEvaluationCriterion", "public", table =>
                table.HasCheckConstraint(
                    "CK_MstProcurementEvaluationCriterion_WeightPercent",
                    "\"WeightPercent\" > 0 AND \"WeightPercent\" <= 100"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CriterionName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.WeightPercent).HasColumnType("numeric(5,2)");
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasIndex(x => new { x.ItemCategoryId, x.CriterionName })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasOne(x => x.ItemCategory).WithMany()
                .HasForeignKey(x => x.ItemCategoryId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
