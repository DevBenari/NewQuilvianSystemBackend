using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    // Konfigurasi LabOrder sendiri sudah ada sejak sebelum RJ-BIL-BE-003 pada
    // Repositories/Configurations/HealthServices/LabOrderConfiguration.cs dan diperluas di
    // sana, bukan diduplikasi di sini. ApplyConfigurationsFromAssembly menerapkan seluruh
    // IEntityTypeConfiguration yang ditemukannya, sehingga dua konfigurasi untuk entity yang
    // sama hanya akan menyulitkan penelusuran.

    public class LabSpecimenConfiguration : IEntityTypeConfiguration<LabSpecimen>
    {
        public void Configure(EntityTypeBuilder<LabSpecimen> builder)
        {
            builder.ToTable("LabSpecimen", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SpecimenBarcode).HasMaxLength(64).IsRequired();
            builder.Property(x => x.SpecimenDescription).HasMaxLength(200);
            builder.Property(x => x.ProcedureCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.ProcedureNameSnapshot).HasMaxLength(200);
            builder.Property(x => x.TariffCodeSnapshot).HasMaxLength(50);
            builder.Property(x => x.UnitPriceSnapshot).HasPrecision(18, 2);
            builder.Property(x => x.SpecimenStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.RejectionReasonCode).HasMaxLength(50);
            builder.Property(x => x.RejectionNote).HasMaxLength(1000);
            builder.Property(x => x.RecollectionCause).HasConversion<int>();
            builder.Property(x => x.RecollectionReason).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            // Barcode wajib unik. Keunikan ditegakkan database, bukan hanya pemeriksaan di
            // service, agar dua permintaan bersamaan tidak dapat menghasilkan barcode kembar.
            builder.HasIndex(x => x.SpecimenBarcode).IsUnique();

            builder.HasIndex(x => new { x.LabOrderId, x.SpecimenSequence });
            builder.HasIndex(x => x.SpecimenStatus);

            // Relasi ke LabOrder dideklarasikan dari sisi LabOrderConfiguration agar hanya ada
            // satu tempat yang mendefinisikannya.

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RejectionReason)
                .WithMany()
                .HasForeignKey(x => x.RejectionReasonId)
                .OnDelete(DeleteBehavior.Restrict);

            // Rantai pengambilan ulang. Restrict memastikan sampel yang ditolak tidak dapat
            // dihapus selama masih menjadi asal-usul sampel penggantinya.
            builder.HasOne(x => x.SupersededSpecimen)
                .WithMany()
                .HasForeignKey(x => x.SupersededSpecimenId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
