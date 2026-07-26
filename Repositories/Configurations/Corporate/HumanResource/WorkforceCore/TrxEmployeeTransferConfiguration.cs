using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class TrxEmployeeTransferConfiguration : IEntityTypeConfiguration<TrxEmployeeTransfer>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeTransfer> builder)
        {
            builder.ToTable("TrxEmployeeTransfer", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.TransferNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TransferStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectiveDate).HasColumnType("date");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReasonText).HasMaxLength(500);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TransferReason).WithMany().HasForeignKey(x => x.TransferReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromOrganizationAssignment).WithMany().HasForeignKey(x => x.FromOrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToOrganizationAssignment).WithMany().HasForeignKey(x => x.ToOrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromLegalEntity).WithMany().HasForeignKey(x => x.FromLegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToLegalEntity).WithMany().HasForeignKey(x => x.ToLegalEntityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromHospitalSite).WithMany().HasForeignKey(x => x.FromHospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToHospitalSite).WithMany().HasForeignKey(x => x.ToHospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromOrganizationUnit).WithMany().HasForeignKey(x => x.FromOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToOrganizationUnit).WithMany().HasForeignKey(x => x.ToOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromDepartment).WithMany().HasForeignKey(x => x.FromDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToDepartment).WithMany().HasForeignKey(x => x.ToDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromPosition).WithMany().HasForeignKey(x => x.FromPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToPosition).WithMany().HasForeignKey(x => x.ToPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromCostCenter).WithMany().HasForeignKey(x => x.FromCostCenterId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToCostCenter).WithMany().HasForeignKey(x => x.ToCostCenterId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FromWorkLocation).WithMany().HasForeignKey(x => x.FromWorkLocationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ToWorkLocation).WithMany().HasForeignKey(x => x.ToWorkLocationId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TransferNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveDate, x.TransferStatus });
        }
    }
}
