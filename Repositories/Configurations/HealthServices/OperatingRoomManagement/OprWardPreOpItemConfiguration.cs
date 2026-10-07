using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

/// <summary><c>OprWardPreOpItem</c> — kamus data 19.5 dan DDL 19.12 (<c>BE-RWI-176</c>).</summary>
public class OprWardPreOpItemConfiguration : IEntityTypeConfiguration<OprWardPreOpItem>
{
    public void Configure(EntityTypeBuilder<OprWardPreOpItem> builder)
    {
        builder.ToTable("OprWardPreOpItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsMandatorySnapshot).IsRequired();
        builder.Property(x => x.SenderConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ReceiverConfirmed).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.ReceiverConfirmedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => new { x.NoteId, x.PreparationItemId }, "UX_OprWardPreOpItem_Note_Item").IsUnique();
        builder.HasIndex(x => x.PreparationItemId);

        // Butir ikut versinya; definisi master tidak boleh dihapus selama dipakai riwayat.
        builder.HasOne(x => x.PreOpNote).WithMany(x => x.Items).HasForeignKey(x => x.NoteId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PreparationItem).WithMany().HasForeignKey(x => x.PreparationItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
