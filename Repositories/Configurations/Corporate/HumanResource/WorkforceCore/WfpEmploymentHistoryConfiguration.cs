using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpEmploymentHistoryConfiguration : IEntityTypeConfiguration<WfpEmploymentHistory>
    {
        public void Configure(EntityTypeBuilder<WfpEmploymentHistory> builder)
        {
            builder.ToTable("WfpEmploymentHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.HistoryType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.OldStatus).HasMaxLength(100);
            builder.Property(x => x.NewStatus).HasMaxLength(100);
            builder.Property(x => x.EffectiveDate).HasColumnType("date");
            builder.Property(x => x.Reason).HasMaxLength(250);
            builder.Property(x => x.ReferenceType).HasMaxLength(100);
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OldEmploymentStatus).WithMany().HasForeignKey(x => x.OldEmploymentStatusId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NewEmploymentStatus).WithMany().HasForeignKey(x => x.NewEmploymentStatusId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OldEmploymentType).WithMany().HasForeignKey(x => x.OldEmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NewEmploymentType).WithMany().HasForeignKey(x => x.NewEmploymentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OldDepartment).WithMany().HasForeignKey(x => x.OldDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NewDepartment).WithMany().HasForeignKey(x => x.NewDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OldPosition).WithMany().HasForeignKey(x => x.OldPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NewPosition).WithMany().HasForeignKey(x => x.NewPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OldOrganizationUnit).WithMany().HasForeignKey(x => x.OldOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NewOrganizationUnit).WithMany().HasForeignKey(x => x.NewOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OldEmployeeGrade).WithMany().HasForeignKey(x => x.OldEmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NewEmployeeGrade).WithMany().HasForeignKey(x => x.NewEmployeeGradeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveDate, x.HistoryType });
        }
    }
}
