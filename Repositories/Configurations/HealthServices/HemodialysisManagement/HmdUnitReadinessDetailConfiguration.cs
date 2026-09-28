using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan hasil butir kesiapan unit — <c>data-dictionary.md</c> bagian 3.21.</summary>
    public class HmdUnitReadinessDetailConfiguration : IEntityTypeConfiguration<HmdUnitReadinessDetail>
    {
        public void Configure(EntityTypeBuilder<HmdUnitReadinessDetail> builder)
        {
            builder.ToTable("HmdUnitReadinessDetail", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Result).HasConversion<int>().IsRequired();
            builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
            builder.Property(x => x.Note).HasMaxLength(500);

            builder.HasIndex(x => new { x.UnitReadinessId, x.ReadinessItemId }).IsUnique();
            builder.HasIndex(x => x.ReadinessItemId);
            builder.HasIndex(x => x.Result);

            builder.HasOne(x => x.UnitReadiness)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.UnitReadinessId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ReadinessItem)
                .WithMany()
                .HasForeignKey(x => x.ReadinessItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
