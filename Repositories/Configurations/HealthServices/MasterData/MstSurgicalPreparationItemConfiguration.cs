using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    /// <summary>
    /// Konfigurasi master butir persiapan bedah — <c>episode-rawat-inap</c> kontrak <c>0.10.0</c>
    /// kamus data 19.7 dan DDL 19.12 (<c>BE-RWI-172</c>, migration <c>E4</c>).
    /// </summary>
    public class MstSurgicalPreparationItemConfiguration : IEntityTypeConfiguration<MstSurgicalPreparationItem>
    {
        public void Configure(EntityTypeBuilder<MstSurgicalPreparationItem> builder)
        {
            builder.ToTable("MstSurgicalPreparationItem", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.GroupName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.IsMandatory).HasDefaultValue(true);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            // Kode unik di antara baris yang belum dihapus (UX_MstSurgicalPreparationItem_Code).
            builder.HasIndex(x => x.Code, "UX_MstSurgicalPreparationItem_Code")
                .IsUnique()
                .HasFilter("NOT \"IsDelete\"");

            builder.HasIndex(x => x.GroupName);
            builder.HasIndex(x => x.IsActive);
        }
    }
}
