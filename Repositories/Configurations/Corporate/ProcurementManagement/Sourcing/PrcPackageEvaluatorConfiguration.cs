using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.Sourcing
{
    /// <summary>
    /// Evaluator paket — kamus data bagian 3. Satu penugasan aktif per evaluator per paket (PBJ-DEC-074).
    /// </summary>
    public class PrcPackageEvaluatorConfiguration : IEntityTypeConfiguration<PrcPackageEvaluator>
    {
        public void Configure(EntityTypeBuilder<PrcPackageEvaluator> builder)
        {
            builder.ToTable("PrcPackageEvaluator", "public");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.PackageId, x.EvaluatorUserId })
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false")
                .HasDatabaseName("IX_PrcPackageEvaluator_ActivePackageEvaluator");
            builder.HasIndex(x => x.EvaluatorUserId);

            builder.HasOne(x => x.Package).WithMany(x => x.Evaluators)
                .HasForeignKey(x => x.PackageId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
