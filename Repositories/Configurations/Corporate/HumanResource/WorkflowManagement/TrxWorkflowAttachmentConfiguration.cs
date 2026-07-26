using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxWorkflowAttachmentConfiguration : IEntityTypeConfiguration<TrxWorkflowAttachment>
    {
        public void Configure(EntityTypeBuilder<TrxWorkflowAttachment> entity)
        {
            entity.ToTable("TrxWorkflowAttachment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UploadedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsRequesterVisible).HasDefaultValue(true);
            entity.Property(x => x.IsConfidential).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowInstance).WithMany(x => x.Attachments).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStepInstance).WithMany(x => x.Attachments).HasForeignKey(x => x.WorkflowStepInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovalAction).WithMany(x => x.Attachments).HasForeignKey(x => x.ApprovalActionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowComment).WithMany(x => x.Attachments).HasForeignKey(x => x.WorkflowCommentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.UploadedByWorkforceProfile).WithMany().HasForeignKey(x => x.UploadedByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkflowInstanceId, x.UploadedAt });
            entity.HasIndex(x => x.FileChecksum);

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxWorkflowAttachment> entity)
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
