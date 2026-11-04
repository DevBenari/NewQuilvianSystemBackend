using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptItemProfile</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptItemProfileConfiguration : IEntityTypeConfiguration<OptItemProfile>
    {
        public void Configure(EntityTypeBuilder<OptItemProfile> builder)
        {
            builder.ToTable("OptItemProfile", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ItemCategory).HasConversion<int>().IsRequired();
            builder.Property(x => x.LensIndex).HasPrecision(4, 2);
            builder.Property(x => x.LensDesign).HasConversion<int?>();
            builder.Property(x => x.ContactLensBaseCurve).HasPrecision(4, 2);
            builder.Property(x => x.ContactLensDiameter).HasPrecision(4, 2);

            builder.HasIndex(x => x.DrugId).IsUnique().HasDatabaseName("UX_OptItemProfile_DrugId");
            builder.HasIndex(x => x.ItemCategory).HasDatabaseName("IX_OptItemProfile_ItemCategory");

            builder.HasOne<MstDrug>()
                .WithMany()
                .HasForeignKey(x => x.DrugId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
