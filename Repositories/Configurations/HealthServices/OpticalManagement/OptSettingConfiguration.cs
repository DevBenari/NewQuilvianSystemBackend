using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptSetting</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptSettingConfiguration : IEntityTypeConfiguration<OptSetting>
    {
        public void Configure(EntityTypeBuilder<OptSetting> builder)
        {
            builder.ToTable("OptSetting", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DownPaymentMinPercent).HasPrecision(5, 2);
            builder.Property(x => x.ContactLensBaseCurveMin).HasPrecision(4, 2);
            builder.Property(x => x.ContactLensBaseCurveMax).HasPrecision(4, 2);
            builder.Property(x => x.ContactLensDiameterMin).HasPrecision(4, 2);
            builder.Property(x => x.ContactLensDiameterMax).HasPrecision(4, 2);
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.ServiceUnitId).IsUnique().HasDatabaseName("UX_OptSetting_ServiceUnitId");

            builder.HasOne<MstServiceUnit>()
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
