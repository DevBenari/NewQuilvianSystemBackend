using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstInpatientClearanceItemConfiguration : IEntityTypeConfiguration<MstInpatientClearanceItem>
    {
        public void Configure(EntityTypeBuilder<MstInpatientClearanceItem> builder)
        {
            builder.ToTable("MstInpatientClearanceItem", "public", table =>
            {
                // BE-RWI-185 / E9 (kamus data 20.15, DDL 20.18): butir penutupan tidak punya induk
                // maupun sumber saran.
                table.HasCheckConstraint("CK_MstInpatientClearanceItem_Type",
                    "\"ChecklistType\" <> 1 OR (\"ParentItemId\" IS NULL AND \"HandoverSuggestionSource\" = 0)");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description).HasMaxLength(500);

            // Baris lama mendapat EpisodeClosure (1) dan None (0) dari database, sehingga daftar
            // periksa penutupan episode sama persis sebelum dan sesudah migration E9.
            builder.Property(x => x.ChecklistType)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(MstClearanceChecklistType.EpisodeClosure);

            builder.Property(x => x.HandoverSuggestionSource)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(MstHandoverSuggestionSource.None);

            builder.HasIndex(x => x.ItemCode).IsUnique();
            builder.HasIndex(x => x.IsMandatory);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => new { x.ChecklistType, x.IsActive });
            builder.HasIndex(x => x.ParentItemId);

            builder.HasOne(x => x.ParentItem)
                .WithMany()
                .HasForeignKey(x => x.ParentItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
