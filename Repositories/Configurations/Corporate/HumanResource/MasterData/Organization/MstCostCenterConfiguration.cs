using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Organization
{
    public class MstCostCenterConfiguration : IEntityTypeConfiguration<MstCostCenter>
    {
        public void Configure(EntityTypeBuilder<MstCostCenter> entity)
        {
            entity.ToTable("MstCostCenter", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.LegalEntityId)
                .IsRequired();

            entity.Property(x => x.CostCenterCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.CostCenterName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.AccountingCode)
                .HasMaxLength(100);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.LegalEntity)
                .WithMany(x => x.CostCenters)
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany(x => x.CostCenters)
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany(x => x.CostCenters)
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.LegalEntityId, x.CostCenterCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.LegalEntityId, x.CostCenterName });

            entity.HasIndex(x => x.AccountingCode)
                .HasFilter("\"AccountingCode\" IS NOT NULL");

            entity.HasIndex(x => new
            {
                x.HospitalSiteId,
                x.OrganizationUnitId,
                x.DepartmentId,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
