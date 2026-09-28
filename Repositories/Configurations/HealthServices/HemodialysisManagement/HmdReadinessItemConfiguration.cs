using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan master butir kesiapan unit — <c>data-dictionary.md</c> bagian 3.19.</summary>
    public class HmdReadinessItemConfiguration : IEntityTypeConfiguration<HmdReadinessItem>
    {
        public void Configure(EntityTypeBuilder<HmdReadinessItem> builder)
        {
            builder.ToTable("HmdReadinessItem", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Category).HasConversion<int>().IsRequired();

            builder.HasIndex(x => x.ItemCode).IsUnique();
            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.IsMandatory);
            builder.HasIndex(x => x.IsActive);
        }
    }
}
