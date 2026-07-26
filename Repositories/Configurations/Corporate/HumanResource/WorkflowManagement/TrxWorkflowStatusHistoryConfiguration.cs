using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxWorkflowStatusHistoryConfiguration : IEntityTypeConfiguration<TrxWorkflowStatusHistory>
    {
        public void Configure(EntityTypeBuilder<TrxWorkflowStatusHistory> entity)
        {
            entity.ToTable("TrxWorkflowStatusHistory", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ChangedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsSystemGenerated).HasDefaultValue(false);
            entity.Property(x => x.StatusSnapshotJson).HasColumnType("jsonb");
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowInstance).WithMany(x => x.StatusHistories).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStepInstance).WithMany(x => x.StatusHistories).HasForeignKey(x => x.WorkflowStepInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ChangedByUser).WithMany().HasForeignKey(x => x.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ChangedByWorkforceProfile).WithMany().HasForeignKey(x => x.ChangedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkflowInstanceId, x.SequenceNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkflowInstanceId, x.ChangedAt });
            entity.HasIndex(x => new { x.WorkflowStepInstanceId, x.ActionType });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxWorkflowStatusHistory> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
