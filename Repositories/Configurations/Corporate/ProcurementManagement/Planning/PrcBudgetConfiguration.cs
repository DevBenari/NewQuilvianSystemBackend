using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Planning
{
    /// <summary>
    /// Pagu — kamus data bagian 3. Satu versi berlaku per tahun × unit × kategori × sumber dana dijaga
    /// index unik parsial (PBJ-VAL-060); nilai pagu positif dijaga basis data (PBJ-VAL-061).
    /// </summary>
    public class PrcBudgetConfiguration : IEntityTypeConfiguration<PrcBudget>
    {
        public void Configure(EntityTypeBuilder<PrcBudget> builder)
        {
            builder.ToTable("PrcBudget", "public", table =>
                table.HasCheckConstraint("CK_PrcBudget_Amount", "\"Amount\" > 0"));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Amount).HasColumnType("numeric(18,2)");
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
            builder.Property(x => x.RevisionReason).HasMaxLength(1000);

            // PBJ-VAL-090: dua orang menyunting draf yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => new { x.FiscalYear, x.DepartmentId, x.ItemCategoryId, x.FundSourceId })
                .IsUnique()
                .HasFilter("\"IsCurrentVersion\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcBudget_CurrentVersion");
            builder.HasIndex(x => new { x.LineageKey, x.VersionNo })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.FiscalYear);
            builder.HasIndex(x => x.DepartmentId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.WorkflowInstanceId);

            builder.HasOne(x => x.Department).WithMany()
                .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ItemCategory).WithMany()
                .HasForeignKey(x => x.ItemCategoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FundSource).WithMany()
                .HasForeignKey(x => x.FundSourceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PreviousVersion).WithMany()
                .HasForeignKey(x => x.PreviousVersionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
