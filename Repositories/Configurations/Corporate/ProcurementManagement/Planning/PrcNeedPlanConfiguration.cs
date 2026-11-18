using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Planning
{
    /// <summary>
    /// Rencana kebutuhan — kamus data bagian 3. Satu versi berlaku per unit-tahun dijaga index unik
    /// parsial; nomor rencana unik per versi (QBE-CODE-004).
    /// </summary>
    public class PrcNeedPlanConfiguration : IEntityTypeConfiguration<PrcNeedPlan>
    {
        public void Configure(EntityTypeBuilder<PrcNeedPlan> builder)
        {
            builder.ToTable("PrcNeedPlan", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PlanNumber).HasMaxLength(30);
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
            builder.Property(x => x.RevisionReason).HasMaxLength(1000);
            builder.Property(x => x.EstimatedTotal).HasColumnType("numeric(18,2)");

            // PBJ-VAL-090: dua orang menyunting draf yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // Versi revisi memakai nomor yang sama dengan versi 1, jadi keunikan pada nomor + versi.
            builder.HasIndex(x => new { x.PlanNumber, x.VersionNo })
                .IsUnique()
                .HasFilter("\"PlanNumber\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.DepartmentId, x.FiscalYear })
                .IsUnique()
                .HasFilter("\"IsCurrentVersion\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcNeedPlan_CurrentVersion");
            builder.HasIndex(x => x.FiscalYear);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PreviousVersionId);
            builder.HasIndex(x => x.WorkflowInstanceId);

            builder.HasOne(x => x.Department).WithMany()
                .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PreviousVersion).WithMany()
                .HasForeignKey(x => x.PreviousVersionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Lines).WithOne(x => x.NeedPlan)
                .HasForeignKey(x => x.NeedPlanId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
