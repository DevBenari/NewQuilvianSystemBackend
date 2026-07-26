using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkflowManagement
{
    public class TrxWorkflowCommentConfiguration : IEntityTypeConfiguration<TrxWorkflowComment>
    {
        public void Configure(EntityTypeBuilder<TrxWorkflowComment> entity)
        {
            entity.ToTable("TrxWorkflowComment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CommentedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsRequesterVisible).HasDefaultValue(true);
            entity.Property(x => x.IsInternalComment).HasDefaultValue(false);
            entity.Property(x => x.IsSystemGenerated).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkflowInstance).WithMany(x => x.Comments).HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowStepInstance).WithMany(x => x.Comments).HasForeignKey(x => x.WorkflowStepInstanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CommentByUser).WithMany().HasForeignKey(x => x.CommentByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CommentByWorkforceProfile).WithMany().HasForeignKey(x => x.CommentByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.WorkflowInstanceId, x.CommentedAt });
            entity.HasIndex(x => new { x.WorkflowStepInstanceId, x.IsInternalComment });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxWorkflowComment> entity)
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
