using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollAdjustmentConfiguration : IEntityTypeConfiguration<TrxPayrollAdjustment>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollAdjustment> entity)
        {

            entity.ToTable("TrxPayrollAdjustment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AdjustmentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AdjustmentType).HasMaxLength(30).HasDefaultValue("Add").IsRequired();
            entity.Property(x => x.AdjustmentStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Rate).HasPrecision(18, 2);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.SourceType).HasMaxLength(50);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovalNotes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRun).WithMany(x => x.Adjustments).HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollRunEmployee).WithMany(x => x.Adjustments).HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AppliedByUser).WithMany().HasForeignKey(x => x.AppliedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AdjustmentNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollRunId, x.AdjustmentStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.PayrollComponentId, x.IsDelete });
            entity.HasIndex(x => new { x.SourceType, x.SourceId, x.IsDelete });
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
