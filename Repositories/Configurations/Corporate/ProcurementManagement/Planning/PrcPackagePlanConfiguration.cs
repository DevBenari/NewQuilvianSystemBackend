using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Planning
{
    /// <summary>Rencana paket — kamus data bagian 3. Nomor rencana paket unik bila terisi (QBE-CODE-004).</summary>
    public class PrcPackagePlanConfiguration : IEntityTypeConfiguration<PrcPackagePlan>
    {
        public void Configure(EntityTypeBuilder<PrcPackagePlan> builder)
        {
            builder.ToTable("PrcPackagePlan", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PackagePlanNumber).HasMaxLength(30);
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.IndicativeMethod).HasMaxLength(100);
            builder.Property(x => x.EstimatedValue).HasColumnType("numeric(18,2)");
            builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
            builder.Property(x => x.CancelReason).HasMaxLength(1000);

            // PBJ-VAL-090: dua orang menyunting draf yang sama → simpanan kedua ditolak 409.
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.PackagePlanNumber)
                .IsUnique()
                .HasFilter("\"PackagePlanNumber\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => x.FiscalYear);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.PicUserId);
            builder.HasIndex(x => x.WorkflowInstanceId);

            builder.HasOne(x => x.ProcurementType).WithMany()
                .HasForeignKey(x => x.ProcurementTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Lines).WithOne(x => x.PackagePlan)
                .HasForeignKey(x => x.PackagePlanId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
