using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan pengaturan unit HD — <c>data-dictionary.md</c> bagian 3.22.</summary>
    /// <remarks>Unique <c>ServiceUnitId</c>: satu baris pengaturan per unit HD.</remarks>
    public class HmdSettingConfiguration : IEntityTypeConfiguration<HmdSetting>
    {
        public void Configure(EntityTypeBuilder<HmdSetting> builder)
        {
            builder.ToTable("HmdSetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.ServiceUnitId).IsUnique();
            builder.HasIndex(x => x.ProcedureId);

            builder.HasOne(x => x.ServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.ServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
