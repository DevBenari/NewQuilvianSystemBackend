using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class HrdExitClearanceProcessConfiguration : IEntityTypeConfiguration<HrdExitClearanceProcess>
    {
        public void Configure(EntityTypeBuilder<HrdExitClearanceProcess> entity)
        {
            entity.ToTable("HrdExitClearanceProcess", "public");
            entity.HasKey(x => x.ClearanceProcessId);

            entity.Property(x => x.ClearanceProcessId).HasColumnName("clearance_process_id");
            entity.Property(x => x.SeparationId).HasColumnName("separation_id").IsRequired();
            entity.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(x => x.InitiatedDate).HasColumnName("initiated_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.TargetCompletionDate).HasColumnName("target_completion_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.ActualCompletionDate).HasColumnName("actual_completion_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.OverallClearanceStatus).HasColumnName("overall_clearance_status").HasMaxLength(32).HasDefaultValue("InProgress").IsRequired();
            entity.Property(x => x.IsFinalPayrollUnlocked).HasColumnName("is_final_payroll_unlocked").HasDefaultValue(false);
            entity.Property(x => x.FinalizationNotes).HasColumnName("finalization_notes");

            entity.HasIndex(x => x.SeparationId);
            entity.HasIndex(x => x.EmployeeId);
            entity.HasIndex(x => x.OverallClearanceStatus);
        }
    }
}
