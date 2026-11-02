using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan hasil Pemeriksaan MCU — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>Satu komponen paling banyak satu hasil (index unik).</remarks>
    public class McuExaminationResultConfiguration : IEntityTypeConfiguration<McuExaminationResult>
    {
        public void Configure(EntityTypeBuilder<McuExaminationResult> builder)
        {
            builder.ToTable("McuExaminationResult", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.McuOrderComponentId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_McuExaminationResult_Component");

            builder.HasOne(x => x.McuOrderComponent)
                .WithOne(x => x.ExaminationResult)
                .HasForeignKey<McuExaminationResult>(x => x.McuOrderComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.FilledByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
