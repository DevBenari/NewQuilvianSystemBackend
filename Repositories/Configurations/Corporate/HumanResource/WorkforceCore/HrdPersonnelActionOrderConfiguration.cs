using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class HrdPersonnelActionOrderConfiguration : IEntityTypeConfiguration<HrdPersonnelActionOrder>
    {
        public void Configure(EntityTypeBuilder<HrdPersonnelActionOrder> builder)
        {
            builder.ToTable("HrdPersonnelActionOrder", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.ActionOrderNumber).HasMaxLength(60).IsRequired();
            builder.Property(x => x.ActionCategory).HasMaxLength(40).IsRequired();
            builder.Property(x => x.EffectiveDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.PostingDate).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.OrderStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ReasonText).HasMaxLength(1500).IsRequired();
            builder.Property(x => x.SignedDocumentPath).HasMaxLength(500);
            builder.Property(x => x.SignedDocumentFileName).HasMaxLength(255);
            builder.Property(x => x.RejectionReason).HasMaxLength(500);
            builder.Property(x => x.CancellationReason).HasMaxLength(500);
            builder.Property(x => x.CounterOrderNumber).HasMaxLength(60);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.TargetBaseSalary).HasColumnType("decimal(18,2)");

            // Relationships
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TargetOrganizationUnit).WithMany().HasForeignKey(x => x.TargetOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TargetDepartment).WithMany().HasForeignKey(x => x.TargetDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TargetPosition).WithMany().HasForeignKey(x => x.TargetPositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TargetManagerProfile).WithMany().HasForeignKey(x => x.TargetManagerProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.LinkedSalaryAssignment).WithMany().HasForeignKey(x => x.LinkedSalaryAssignmentId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowInstance).WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.ActionOrderNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EffectiveDate });
            builder.HasIndex(x => new { x.OrderStatus, x.EffectiveDate });
            builder.HasIndex(x => x.WorkflowInstanceId).IsUnique().HasFilter("\"WorkflowInstanceId\" IS NOT NULL AND \"IsDelete\" = false");
        }
    }
}
