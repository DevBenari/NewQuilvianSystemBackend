using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class MstOperatingRoomStockSourceConfiguration : IEntityTypeConfiguration<MstOperatingRoomStockSource>
{
    public void Configure(EntityTypeBuilder<MstOperatingRoomStockSource> builder)
    {
        builder.ToTable("MstOperatingRoomStockSource", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(500);

        // Satu kamar, satu sumber aktif. Ditegakkan di basis data karena akibat pelanggarannya
        // adalah stok yang berkurang di depo yang salah — kesalahan yang baru terlihat saat
        // stok opname, jauh setelah operasinya selesai.
        builder.HasIndex(x => x.RoomId)
            .IsUnique()
            .HasFilter("\"IsActive\" = TRUE AND \"IsDelete\" = FALSE");

        builder.HasIndex(x => x.StorageLocationId);

        builder.HasOne(x => x.Room)
            .WithMany()
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StorageLocation)
            .WithMany()
            .HasForeignKey(x => x.StorageLocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
