using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OpticalManagement
{
    /// <summary>Pemetaan <c>OptSettingRevision</c> — <c>erd/data-dictionary.md</c> r1 (<c>BE-OPT-002</c>). Seluruh FK <c>Restrict</c>: rekam layanan dan klinis tidak boleh terhapus berantai.</summary>
    public class OptSettingRevisionConfiguration : IEntityTypeConfiguration<OptSettingRevision>
    {
        public void Configure(EntityTypeBuilder<OptSettingRevision> builder)
        {
            builder.ToTable("OptSettingRevision", "public");
            builder.HasKey(x => x.Id);


            builder.HasIndex(x => new { x.SettingId, x.ChangedAt }).HasDatabaseName("IX_OptSettingRevision_SettingId_ChangedAt");

            builder.HasOne(x => x.Setting)
                .WithMany(x => x.Revisions)
                .HasForeignKey(x => x.SettingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
