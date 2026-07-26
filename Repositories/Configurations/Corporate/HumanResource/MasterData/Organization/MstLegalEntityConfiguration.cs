using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstLegalEntityConfiguration : IEntityTypeConfiguration<MstLegalEntity>
    {
        public void Configure(EntityTypeBuilder<MstLegalEntity> entity)
        {
            entity.ToTable("MstLegalEntity", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.LegalEntityName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ShortName)
                .HasMaxLength(100);

            entity.Property(x => x.TaxIdentificationNumber)
                .HasMaxLength(100);

            entity.Property(x => x.BusinessRegistrationNumber)
                .HasMaxLength(100);

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.Address)
                .HasMaxLength(500);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasIndex(x => x.LegalEntityCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.LegalEntityName);

            entity.HasIndex(x => x.TaxIdentificationNumber)
                .IsUnique()
                .HasFilter("\"TaxIdentificationNumber\" IS NOT NULL AND \"IsDelete\" = false");

            entity.HasIndex(x => x.BusinessRegistrationNumber)
                .HasFilter("\"BusinessRegistrationNumber\" IS NOT NULL");

            entity.HasIndex(x => new { x.IsDefault, x.IsActive, x.IsDelete });

            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }
    }
}
