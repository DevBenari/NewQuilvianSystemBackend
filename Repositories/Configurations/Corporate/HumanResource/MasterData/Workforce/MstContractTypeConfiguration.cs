using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstContractTypeConfiguration : IEntityTypeConfiguration<MstContractType>
    {
        public void Configure(EntityTypeBuilder<MstContractType> entity)
        {
            entity.ToTable("MstContractType", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.ContractTypeCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.ContractTypeName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.DefaultDurationMonths)
                .IsRequired(false);

            entity.Property(x => x.IsRenewable)
                .HasDefaultValue(true);

            entity.Property(x => x.RequiresEndDate)
                .HasDefaultValue(true);

            entity.Property(x => x.IsProbationApplicable)
                .HasDefaultValue(false);

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

            entity.HasIndex(x => x.ContractTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ContractTypeName);

            entity.HasIndex(x => new
            {
                x.IsRenewable,
                x.RequiresEndDate,
                x.IsProbationApplicable,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
