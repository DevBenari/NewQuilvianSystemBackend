using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpManagerAssignmentConfiguration : IEntityTypeConfiguration<WfpManagerAssignment>
    {
        public void Configure(EntityTypeBuilder<WfpManagerAssignment> builder)
        {
            builder.ToTable("WfpManagerAssignment", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.ManagerType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ManagerWorkforceProfile).WithMany().HasForeignKey(x => x.ManagerWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Department).WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ManagerPosition).WithMany().HasForeignKey(x => x.ManagerPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.ManagerWorkforceProfileId, x.IsActive });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate });
            builder.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsPrimaryManager\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
            builder.HasCheckConstraint("CK_WfpManagerAssignment_NotSelf", "\"WorkforceProfileId\" <> \"ManagerWorkforceProfileId\"");
        }
    }
}
