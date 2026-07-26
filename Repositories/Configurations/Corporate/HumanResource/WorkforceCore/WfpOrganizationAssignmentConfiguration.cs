using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpOrganizationAssignmentConfiguration : IEntityTypeConfiguration<WfpOrganizationAssignment>
    {
        public void Configure(EntityTypeBuilder<WfpOrganizationAssignment> builder)
        {
            builder.ToTable("WfpOrganizationAssignment", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.AssignmentType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.AssignmentNumber).HasMaxLength(100);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LegalEntity).WithMany().HasForeignKey(x => x.LegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkLocation).WithMany().HasForeignKey(x => x.WorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeGrade).WithMany().HasForeignKey(x => x.EmployeeGradeId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate });
            builder.HasIndex(x => new { x.OrganizationUnitId, x.IsActive });
            builder.HasIndex(x => new { x.DepartmentId, x.PositionId, x.IsActive });
            builder.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
        }
    }
}
