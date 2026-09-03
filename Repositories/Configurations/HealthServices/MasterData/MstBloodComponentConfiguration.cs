using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    /// <summary>
    /// Pemetaan katalog komponen darah. Index unik pada <c>ComponentCode</c> adalah penjaga
    /// terakhir ketika dua petugas menyimpan kode yang sama pada saat hampir bersamaan —
    /// pemeriksaan di service saja tidak cukup untuk itu.
    /// </summary>
    public class MstBloodComponentConfiguration : IEntityTypeConfiguration<MstBloodComponent>
    {
        public void Configure(EntityTypeBuilder<MstBloodComponent> builder)
        {
            builder.ToTable("MstBloodComponent", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComponentCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.ComponentName).HasMaxLength(100).IsRequired();

            builder.HasIndex(x => x.ComponentCode).IsUnique();
            builder.HasIndex(x => new { x.IsActive, x.ComponentName });
        }
    }
}
