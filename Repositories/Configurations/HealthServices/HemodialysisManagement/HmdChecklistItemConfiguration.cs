using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan master butir checklist Pra-HD — <c>data-dictionary.md</c> bagian 3.18.</summary>
    /// <remarks>
    /// Nilai bawaan database <c>IsOverridable = false</c> disengaja (<c>HMD-ASM-001</c>): butir yang
    /// disisipkan tanpa menyebut kolom itu tetap tidak dapat dilewati — fail-closed.
    /// </remarks>
    public class HmdChecklistItemConfiguration : IEntityTypeConfiguration<HmdChecklistItem>
    {
        public void Configure(EntityTypeBuilder<HmdChecklistItem> builder)
        {
            builder.ToTable("HmdChecklistItem", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Category).HasConversion<int>().IsRequired();
            builder.Property(x => x.IsOverridable).HasDefaultValue(false);
            builder.Property(x => x.OverridableDecisionNote).HasMaxLength(1000);

            builder.HasIndex(x => x.ItemCode).IsUnique();
            builder.HasIndex(x => x.Category);
            builder.HasIndex(x => x.IsMandatory);
            builder.HasIndex(x => x.IsOverridable);
            builder.HasIndex(x => x.IsActive);
        }
    }
}
