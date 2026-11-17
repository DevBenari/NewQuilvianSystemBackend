using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Planning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Planning
{
    /// <summary>
    /// Baris rencana paket — kamus data bagian 3. Satu baris rencana hanya sekali per rencana paket
    /// (index unik untuk baris hidup); alokasi positif dijaga basis data.
    /// </summary>
    public class PrcPackagePlanLineConfiguration : IEntityTypeConfiguration<PrcPackagePlanLine>
    {
        public void Configure(EntityTypeBuilder<PrcPackagePlanLine> builder)
        {
            builder.ToTable("PrcPackagePlanLine", "public", table =>
                table.HasCheckConstraint("CK_PrcPackagePlanLine_AllocatedQuantity", "\"AllocatedQuantity\" > 0"));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AllocatedQuantity).HasColumnType("numeric(18,4)");

            builder.HasIndex(x => new { x.PackagePlanId, x.NeedPlanLineKey })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.NeedPlanLineKey);

            builder.HasOne(x => x.Department).WithMany()
                .HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Item).WithMany()
                .HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
