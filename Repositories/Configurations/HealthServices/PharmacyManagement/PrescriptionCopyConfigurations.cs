using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement;

public class PhmPrescriptionCopyConfiguration : IEntityTypeConfiguration<PhmPrescriptionCopy>
{
    public void Configure(EntityTypeBuilder<PhmPrescriptionCopy> builder)
    {
        builder.ToTable("PhmPrescriptionCopy", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CopyNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SiteNameSnapshot).HasMaxLength(200);
        builder.Property(x => x.SiteAddressSnapshot).HasMaxLength(500);
        builder.Property(x => x.SitePhoneSnapshot).HasMaxLength(50);
        builder.Property(x => x.PharmacistNameSnapshot).HasMaxLength(200);
        builder.Property(x => x.PharmacistLicenseNumberSnapshot).HasMaxLength(100);
        builder.Property(x => x.PharmacistLicenseTypeSnapshot).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.Property(x => x.RevokeReason).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();

        // Nomor dokumen tidak boleh kembar: dua lembar bernomor sama membuat penelusuran
        // "lembar mana yang dipegang pasien" tidak terjawab.
        builder.HasIndex(x => x.CopyNumber).IsUnique();

        // Pertanyaan yang paling sering diajukan: lembar apa saja yang pernah terbit untuk
        // resep ini.
        builder.HasIndex(x => new { x.PrescriptionId, x.IssuedAt });

        builder.HasOne(x => x.Prescription).WithMany()
            .HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.HospitalSite).WithMany()
            .HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PharmacistWorkforce).WithMany()
            .HasForeignKey(x => x.PharmacistWorkforceId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PhmPrescriptionCopyItemConfiguration : IEntityTypeConfiguration<PhmPrescriptionCopyItem>
{
    public void Configure(EntityTypeBuilder<PhmPrescriptionCopyItem> builder)
    {
        builder.ToTable("PhmPrescriptionCopyItem", "public");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DrugNameSnapshot).HasMaxLength(200).IsRequired();
        builder.Property(x => x.GenericNameSnapshot).HasMaxLength(200);
        builder.Property(x => x.StrengthSnapshot).HasMaxLength(100);
        builder.Property(x => x.DrugFormSnapshot).HasMaxLength(100);
        builder.Property(x => x.DispenseUnitSnapshot).HasMaxLength(50);
        builder.Property(x => x.SignaSnapshot).HasMaxLength(500);
        builder.Property(x => x.AdministrationInstructionSnapshot).HasMaxLength(500);
        builder.Property(x => x.QuantityPrescribed).HasColumnType("numeric(18,3)");
        builder.Property(x => x.QuantityDispensed).HasColumnType("numeric(18,3)");
        builder.Property(x => x.QuantityRemaining).HasColumnType("numeric(18,3)");

        // Satu baris resep muncul tepat sekali pada satu lembar.
        builder.HasIndex(x => new { x.PrescriptionCopyId, x.PrescriptionItemId }).IsUnique();

        builder.HasOne(x => x.PrescriptionCopy).WithMany(x => x.Items)
            .HasForeignKey(x => x.PrescriptionCopyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PrescriptionItem).WithMany()
            .HasForeignKey(x => x.PrescriptionItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
