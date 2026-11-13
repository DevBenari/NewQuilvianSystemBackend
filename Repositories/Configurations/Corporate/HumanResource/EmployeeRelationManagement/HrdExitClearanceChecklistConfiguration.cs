using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class HrdExitClearanceChecklistConfiguration : IEntityTypeConfiguration<HrdExitClearanceChecklist>
    {
        public void Configure(EntityTypeBuilder<HrdExitClearanceChecklist> entity)
        {
            entity.ToTable("HrdExitClearanceChecklist", "public");
            entity.HasKey(x => x.ChecklistId);

            entity.Property(x => x.ChecklistId).HasColumnName("checklist_id");
            entity.Property(x => x.ClearanceProcessId).HasColumnName("clearance_process_id").IsRequired();
            entity.Property(x => x.DepartmentType).HasColumnName("department_type").HasMaxLength(64).IsRequired();
            entity.Property(x => x.ItemName).HasColumnName("item_name").HasMaxLength(255).IsRequired();
            entity.Property(x => x.IsReturned).HasColumnName("is_returned").HasDefaultValue(false);
            entity.Property(x => x.OutstandingLiabilityAmount).HasColumnName("outstanding_liability_amount").HasColumnType("numeric(14,2)").HasDefaultValue(0m);
            entity.Property(x => x.ApproverNotes).HasColumnName("approver_notes");
            entity.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id");
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at").HasColumnType("timestamp with time zone");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).HasDefaultValue("Pending").IsRequired();

            entity.HasIndex(x => x.ClearanceProcessId);
            entity.HasIndex(x => x.DepartmentType);
            entity.HasIndex(x => x.Status);

            entity.HasOne(x => x.ClearanceProcess)
                .WithMany(x => x.Checklists)
                .HasForeignKey(x => x.ClearanceProcessId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
