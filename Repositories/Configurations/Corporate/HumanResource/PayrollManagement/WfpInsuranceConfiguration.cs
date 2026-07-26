using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class WfpInsuranceConfiguration : IEntityTypeConfiguration<WfpInsurance>
    {
        public void Configure(EntityTypeBuilder<WfpInsurance> entity)
        {

            entity.ToTable("WfpInsurance", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BpjsKesehatanNumber).HasMaxLength(50);
            entity.Property(x => x.BpjsKetenagakerjaanNumber).HasMaxLength(50);
            entity.Property(x => x.PrivateInsuranceProvider).HasMaxLength(200);
            entity.Property(x => x.PrivateInsuranceNumber).HasMaxLength(100);
            entity.Property(x => x.BpjsHealthEmployeeRate).HasPrecision(9, 4);
            entity.Property(x => x.BpjsHealthEmployerRate).HasPrecision(9, 4);
            entity.Property(x => x.BpjsEmploymentEmployeeRate).HasPrecision(9, 4);
            entity.Property(x => x.BpjsEmploymentEmployerRate).HasPrecision(9, 4);
            entity.Property(x => x.PrivateInsuranceEmployeeContribution).HasPrecision(18, 2);
            entity.Property(x => x.PrivateInsuranceEmployerContribution).HasPrecision(18, 2);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => x.BpjsKesehatanNumber).HasFilter("\"BpjsKesehatanNumber\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => x.BpjsKetenagakerjaanNumber).HasFilter("\"BpjsKetenagakerjaanNumber\" IS NOT NULL AND \"IsDelete\" = false");
            entity.HasIndex(x => new { x.IsBpjsKesehatanEnabled, x.IsBpjsKetenagakerjaanEnabled, x.IsPrivateInsuranceEnabled, x.IsActive });
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
