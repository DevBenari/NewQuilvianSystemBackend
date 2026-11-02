using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan nilai isian hasil Pemeriksaan MCU — <c>data-dictionary.md</c> bagian 3.</summary>
    /// <remarks>Satu isian paling banyak satu nilai per hasil (index unik).</remarks>
    public class McuExaminationResultValueConfiguration : IEntityTypeConfiguration<McuExaminationResultValue>
    {
        public void Configure(EntityTypeBuilder<McuExaminationResultValue> builder)
        {
            builder.ToTable("McuExaminationResultValue", "public");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ExaminationResultId, x.ExaminationFieldId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_McuExaminationResultValue_Result_Field");

            builder.HasOne(x => x.ExaminationResult)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.ExaminationResultId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ExaminationField)
                .WithMany()
                .HasForeignKey(x => x.ExaminationFieldId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
