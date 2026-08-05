using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.OvertimeManagement
{
    public class TrxOvertimePlanConfiguration : IEntityTypeConfiguration<TrxOvertimePlan>
    {
        public void Configure(EntityTypeBuilder<TrxOvertimePlan> entity)
        {
            entity.ToTable("TrxOvertimePlan", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PlanNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PlanTitle)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.PlanStartDate)
                .HasColumnType("date");

            entity.Property(x => x.PlanEndDate)
                .HasColumnType("date");

            entity.Property(x => x.Reason)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(2000);

            entity.Property(x => x.PlanStatus)
                .HasMaxLength(40)
                .HasDefaultValue(OvertimeValueConstants.PlanStatus.Draft)
                .IsRequired();

            entity.Property(x => x.ValidatedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.PublishedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.ClosedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

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
                .WithMany()
                .HasForeignKey(x => x.LegalEntityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CostCenter)
                .WithMany()
                .HasForeignKey(x => x.CostCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.WorkLocation)
                .WithMany()
                .HasForeignKey(x => x.WorkLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RosterPeriod)
                .WithMany()
                .HasForeignKey(x => x.RosterPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ValidatedByUser)
                .WithMany()
                .HasForeignKey(x => x.ValidatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PublishedByUser)
                .WithMany()
                .HasForeignKey(x => x.PublishedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Details)
                .WithOne(x => x.OvertimePlan)
                .HasForeignKey(x => x.OvertimePlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PlanNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.HospitalSiteId,
                x.DepartmentId,
                x.PlanStartDate,
                x.PlanEndDate,
                x.PlanStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.RosterPeriodId,
                x.PlanStatus,
                x.IsDelete
            });
        }
    }
}
