using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstReferralInstitutionConfiguration : IEntityTypeConfiguration<MstReferralInstitution>
    {
        public void Configure(EntityTypeBuilder<MstReferralInstitution> builder)
        {
            builder.ToTable("MstReferralInstitution", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.InstitutionCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.InstitutionName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.PhoneNumber).HasMaxLength(50);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.Property(x => x.IsPartner).HasDefaultValue(false);

            // DEC-FRJ-001. Baris lama tidak punya nilai untuk kolom-kolom ini, sehingga semuanya
            // nullable/berbawaan; kewajiban isiannya ditegakkan service saat create dan update.
            builder.Property(x => x.InstitutionType).HasConversion<int>().HasDefaultValue(ReferralInstitutionType.Unknown);
            builder.Property(x => x.Email).HasMaxLength(150);
            builder.Property(x => x.PicName).HasMaxLength(150);
            builder.Property(x => x.ExternalFacilityCode).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.Province)
                .WithMany()
                .HasForeignKey(x => x.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Keunikan kode ditegakkan database, bukan hanya pemeriksaan di service. Baris yang
            // sudah dihapus tidak ikut menghalangi, mengikuti pola MstProcedure.
            builder.HasIndex(x => x.InstitutionCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.InstitutionName);

            // Daftar master disaring dan diurutkan menurut tanggal dibuat (filter periode).
            builder.HasIndex(x => x.CreateDateTime);
        }
    }
}
