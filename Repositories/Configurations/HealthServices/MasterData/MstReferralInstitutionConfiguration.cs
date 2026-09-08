using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

            // Keunikan kode ditegakkan database, bukan hanya pemeriksaan di service. Baris yang
            // sudah dihapus tidak ikut menghalangi, mengikuti pola MstProcedure.
            builder.HasIndex(x => x.InstitutionCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.InstitutionName);
        }
    }
}
