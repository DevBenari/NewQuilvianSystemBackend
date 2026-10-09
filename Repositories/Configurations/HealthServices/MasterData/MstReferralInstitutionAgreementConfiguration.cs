using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    public class MstReferralInstitutionAgreementConfiguration : IEntityTypeConfiguration<MstReferralInstitutionAgreement>
    {
        public void Configure(EntityTypeBuilder<MstReferralInstitutionAgreement> builder)
        {
            builder.ToTable("MstReferralInstitutionAgreement", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.AgreementNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_MstReferralInstitutionAgreement_DateRange",
                "\"EndDate\" >= \"StartDate\""));

            builder.HasOne(x => x.ReferralInstitution)
                .WithMany(x => x.Agreements)
                .HasForeignKey(x => x.ReferralInstitutionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu nomor PKS tidak boleh tercatat dua kali pada fasilitas yang sama.
            builder.HasIndex(x => new { x.ReferralInstitutionId, x.AgreementNumber })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            // Pencarian kelayakan: perjanjian yang mencakup satu tanggal layanan.
            builder.HasIndex(x => new { x.ReferralInstitutionId, x.StartDate, x.EndDate });
        }
    }
}
