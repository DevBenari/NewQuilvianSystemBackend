using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstEmploymentStatusConfiguration : IEntityTypeConfiguration<MstEmploymentStatus>
    {
        public void Configure(EntityTypeBuilder<MstEmploymentStatus> entity)
        {
            entity.ToTable("MstEmploymentStatus", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.EmploymentStatusCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.EmploymentStatusName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsActiveEmployment)
                .HasDefaultValue(true);

            entity.Property(x => x.IsSchedulable)
                .HasDefaultValue(true);

            entity.Property(x => x.IsPayrollEligible)
                .HasDefaultValue(true);

            entity.Property(x => x.IsTerminalStatus)
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

            entity.HasIndex(x => x.EmploymentStatusCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.EmploymentStatusName);

            entity.HasIndex(x => new
            {
                x.IsActiveEmployment,
                x.IsSchedulable,
                x.IsPayrollEligible,
                x.IsTerminalStatus,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
