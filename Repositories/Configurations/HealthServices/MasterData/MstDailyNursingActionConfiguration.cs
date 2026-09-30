using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    /// <summary>
    /// Konfigurasi pemetaan entitas Master Data Tindakan Harian Keperawatan (MstDailyNursingAction).
    /// </summary>
    public class MstDailyNursingActionConfiguration : IEntityTypeConfiguration<MstDailyNursingAction>
    {
        public void Configure(EntityTypeBuilder<MstDailyNursingAction> builder)
        {
            builder.ToTable("MstDailyNursingAction", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ActionCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ActionName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DefaultNotes).HasMaxLength(500);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.ActionCode).IsUnique();
            builder.HasIndex(x => new { x.Category, x.IsActive, x.IsDelete });
            builder.HasIndex(x => new { x.SortOrder, x.ActionName });
        }
    }
}
