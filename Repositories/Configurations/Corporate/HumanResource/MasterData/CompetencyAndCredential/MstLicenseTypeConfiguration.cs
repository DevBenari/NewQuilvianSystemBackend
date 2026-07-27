using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.CompetencyAndCredential
{
    public class MstLicenseTypeConfiguration : IEntityTypeConfiguration<MstLicenseType>
    {
        public void Configure(EntityTypeBuilder<MstLicenseType> entity)
        {
            entity.ToTable("MstLicenseType", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LicenseTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LicenseTypeName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IssuingAuthority).HasMaxLength(200);
            entity.Property(x => x.RegulatoryBody).HasMaxLength(200);
            entity.Property(x => x.DefaultValidityMonths).IsRequired(false);
            entity.Property(x => x.RequiresExpiryDate).HasDefaultValue(true);
            entity.Property(x => x.IsRenewable).HasDefaultValue(true);
            entity.Property(x => x.RequiresDocument).HasDefaultValue(true);
            entity.Property(x => x.RequiresVerification).HasDefaultValue(true);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.Profession)
                .WithMany(x => x.LicenseTypes)
                .HasForeignKey(x => x.ProfessionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.LicenseTypeCode).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.LicenseTypeName);
            entity.HasIndex(x => new { x.ProfessionId, x.IsActive, x.IsDelete });
        }
    }
}
