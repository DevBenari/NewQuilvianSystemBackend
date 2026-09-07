using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabValueOptionConfiguration : IEntityTypeConfiguration<LabValueOption>
    {
        public void Configure(EntityTypeBuilder<LabValueOption> builder)
        {
            builder.ToTable("LabValueOption", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ValueBoundId).IsRequired();
            builder.Property(x => x.OptionCode).HasMaxLength(20).IsRequired();
            builder.Property(x => x.OptionName).HasMaxLength(100).IsRequired();

            // AC-28: analis memilih dari daftar yang sah. Kode pilihan wajib unik dalam satu
            // batas nilai supaya satu kode tidak pernah menunjuk dua arti pada pemeriksaan yang
            // sama.
            builder.HasIndex(x => new { x.ValueBoundId, x.OptionCode })
                .IsUnique()
                .HasDatabaseName("IX_LabValueOption_ValueBoundId_OptionCode")
                .HasFilter("\"IsDelete\" = false");

            // Relasi ke LabValueBound dideklarasikan dari sisi LabValueBoundConfiguration agar
            // hanya ada satu tempat yang mendefinisikannya, mengikuti pola yang sudah dipakai
            // LabOrder dan LabSpecimen pada modul ini.
        }
    }
}
