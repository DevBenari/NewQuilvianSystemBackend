using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan hasil checklist Pra-HD — <c>data-dictionary.md</c> bagian 3.9.</summary>
    /// <remarks>Unique <c>(SessionId, ChecklistItemId)</c>: satu butir hanya punya satu hasil per sesi.</remarks>
    public class HmdSessionChecklistConfiguration : IEntityTypeConfiguration<HmdSessionChecklist>
    {
        public void Configure(EntityTypeBuilder<HmdSessionChecklist> builder)
        {
            builder.ToTable("HmdSessionChecklist", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Result).HasConversion<int>().IsRequired();
            builder.Property(x => x.Note).HasMaxLength(500);
            builder.Property(x => x.OverrideReason).HasMaxLength(1000);

            builder.HasIndex(x => new { x.SessionId, x.ChecklistItemId }).IsUnique();
            builder.HasIndex(x => x.ChecklistItemId);
            builder.HasIndex(x => x.Result);
            builder.HasIndex(x => x.IsOverridden);

            builder.HasOne(x => x.Session)
                .WithMany(x => x.Checklists)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ChecklistItem)
                .WithMany()
                .HasForeignKey(x => x.ChecklistItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
