using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan kesiapan unit — <c>data-dictionary.md</c> bagian 3.20.</summary>
    /// <remarks>
    /// Unique <c>(ServiceUnitId, ReadinessDate, Shift)</c> untuk baris yang belum ditandai terhapus
    /// menjaga satu lembar per tanggal dan shift (<c>HMD-VAL-100</c>).
    /// </remarks>
    public class HmdUnitReadinessConfiguration : IEntityTypeConfiguration<HmdUnitReadiness>
    {
        public void Configure(EntityTypeBuilder<HmdUnitReadiness> builder)
        {
            builder.ToTable("HmdUnitReadiness", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Shift).HasConversion<int>().IsRequired();
            builder.Property(x => x.ReadinessStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.NotReadyReason).HasMaxLength(1000);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => new { x.ServiceUnitId, x.ReadinessDate, x.Shift })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.ReadinessStatus);
            builder.HasIndex(x => x.DeclaredByUserId);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
