using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class WfpTaxConfiguration : IEntityTypeConfiguration<WfpTax>
    {
        public void Configure(EntityTypeBuilder<WfpTax> entity)
        {

            entity.ToTable("WfpTax", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.NpwpNumber).HasMaxLength(50);
            entity.Property(x => x.TaxStatus).HasMaxLength(30).HasDefaultValue("TK/0").IsRequired();
            entity.Property(x => x.TaxMethod).HasMaxLength(30).HasDefaultValue("Gross").IsRequired();
            entity.Property(x => x.TaxCountryCode).HasMaxLength(3).HasDefaultValue("ID").IsRequired();
            entity.Property(x => x.TaxOfficeCode).HasMaxLength(50);
            entity.Property(x => x.PreviousEmployerTaxableIncome).HasPrecision(18, 2);
            entity.Property(x => x.PreviousEmployerTaxPaid).HasPrecision(18, 2);
            entity.Property(x => x.AnnualNonTaxableIncome).HasPrecision(18, 2);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Ignore(x => x.TaxIdentificationNumber);
            entity.Ignore(x => x.PtkpStatus);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.NpwpNumber).HasFilter("\"NpwpNumber\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.TaxStatus, x.TaxMethod, x.IsActive, x.IsDelete });
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
