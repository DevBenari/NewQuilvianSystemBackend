using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstReferralDoctorConfiguration : IEntityTypeConfiguration<MstReferralDoctor>
    {
        public void Configure(EntityTypeBuilder<MstReferralDoctor> builder)
        {
            builder.ToTable("MstReferralDoctor", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DoctorName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasIndex(x => x.ReferralInstitutionId);
            builder.HasIndex(x => x.DoctorName);

            // Restrict: instansi yang masih menaungi dokter tidak dapat dihapus. Menghapusnya
            // akan meninggalkan dokter tanpa asal-usul, dan kunjungan lama yang menunjuk dokter
            // itu kehilangan konteksnya.
            builder.HasOne(x => x.ReferralInstitution)
                .WithMany(x => x.Doctors)
                .HasForeignKey(x => x.ReferralInstitutionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
