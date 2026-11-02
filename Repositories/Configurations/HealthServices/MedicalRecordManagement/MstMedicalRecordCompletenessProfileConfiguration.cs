using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MedicalRecordManagement
{
    /// <summary>
    /// Ditaruh di folder rekam medis mengikuti preseden
    /// <see cref="MstMedicalRecordAccessPurposeConfiguration"/>.
    /// </summary>
    public class MstMedicalRecordCompletenessProfileConfiguration
        : IEntityTypeConfiguration<MstMedicalRecordCompletenessProfile>
    {
        public void Configure(EntityTypeBuilder<MstMedicalRecordCompletenessProfile> builder)
        {
            builder.ToTable("MstMedicalRecordCompletenessProfile", "public", t =>
            {
                // VAL-RM-A04: profil tanpa kondisi wajib menyebut jenis kunjungan.
                t.HasCheckConstraint(
                    "CK_MstMedicalRecordCompletenessProfile_EncounterTypeWajib",
                    "\"Condition\" <> 0 OR \"EncounterType\" IS NOT NULL");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(150).IsRequired();

            builder.HasIndex(x => x.Code)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // VAL-RM-A05: satu profil per kunci. Kolom kosong dibandingkan sebagai sama
            // (NULLS NOT DISTINCT, PostgreSQL 15+; devYoga 15.15).
            builder.HasIndex(x => new { x.EncounterType, x.ServiceUnitType, x.Condition })
                .IsUnique()
                .AreNullsDistinct(false)
                .HasFilter("\"IsDelete\" = false");

            builder.HasMany(x => x.Items)
                .WithOne(x => x.Profile)
                .HasForeignKey(x => x.ProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
