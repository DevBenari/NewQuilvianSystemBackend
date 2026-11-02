using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalCheckupManagement
{
    /// <summary>Pemetaan isian formulir Pemeriksaan MCU — <c>data-dictionary.md</c> bagian 1.</summary>
    /// <remarks>Kode isian dan nomor butir masing-masing unik per jenis pemeriksaan.</remarks>
    public class McuExaminationFieldConfiguration : IEntityTypeConfiguration<McuExaminationField>
    {
        public void Configure(EntityTypeBuilder<McuExaminationField> builder)
        {
            builder.ToTable("McuExaminationField", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FieldType).HasConversion<int>().IsRequired();
            builder.Property(x => x.IsRequired).HasDefaultValue(false);

            builder.HasIndex(x => new { x.ExaminationTypeId, x.FieldCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_McuExaminationField_Type_FieldCode");

            builder.HasIndex(x => new { x.ExaminationTypeId, x.FieldNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_McuExaminationField_Type_FieldNumber");

            builder.HasOne(x => x.ExaminationType)
                .WithMany(x => x.Fields)
                .HasForeignKey(x => x.ExaminationTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
