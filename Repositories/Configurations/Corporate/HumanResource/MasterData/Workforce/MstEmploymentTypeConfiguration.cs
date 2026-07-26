using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstEmploymentTypeConfiguration : IEntityTypeConfiguration<MstEmploymentType>
    {
        public void Configure(EntityTypeBuilder<MstEmploymentType> entity)
        {
            entity.ToTable("MstEmploymentType", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.EmploymentTypeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EmploymentTypeName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsPermanent)
                .HasDefaultValue(false);

            entity.Property(x => x.IsContractBased)
                .HasDefaultValue(false);

            entity.Property(x => x.RequiresContractEndDate)
                .HasDefaultValue(false);

            entity.Property(x => x.IsPayrollEligible)
                .HasDefaultValue(true);

            entity.Property(x => x.IsBenefitEligible)
                .HasDefaultValue(true);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

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

            entity.HasIndex(x => x.EmploymentTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EmploymentTypeName);

            entity.HasIndex(x => new
            {
                x.IsPermanent,
                x.IsContractBased,
                x.IsPayrollEligible,
                x.IsBenefitEligible,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
