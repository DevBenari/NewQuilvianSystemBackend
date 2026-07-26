using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class TrxEmployeeRotationConfiguration : IEntityTypeConfiguration<TrxEmployeeRotation>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeRotation> builder)
        {
            builder.ToTable("TrxEmployeeRotation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RotationNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RotationStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectiveDate).HasColumnType("date");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReasonText).HasMaxLength(500);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TransferReason).WithMany().HasForeignKey(x => x.TransferReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromOrganizationUnit).WithMany().HasForeignKey(x => x.FromOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToOrganizationUnit).WithMany().HasForeignKey(x => x.ToOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromDepartment).WithMany().HasForeignKey(x => x.FromDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToDepartment).WithMany().HasForeignKey(x => x.ToDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromPosition).WithMany().HasForeignKey(x => x.FromPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToPosition).WithMany().HasForeignKey(x => x.ToPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromWorkLocation).WithMany().HasForeignKey(x => x.FromWorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToWorkLocation).WithMany().HasForeignKey(x => x.ToWorkLocationId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RotationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveDate, x.RotationStatus });
        }
    }
}
