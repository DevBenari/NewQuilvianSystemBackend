using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workflow
{
    public class MstRequestReasonConfiguration : IEntityTypeConfiguration<MstRequestReason>
    {
        public void Configure(EntityTypeBuilder<MstRequestReason> entity)
        {
            entity.ToTable("MstRequestReason", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.RequestType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ReasonCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReasonName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ReasonCategory).HasMaxLength(100);
            entity.Property(x => x.IsCommentRequired).HasDefaultValue(false);
            entity.Property(x => x.IsAttachmentRequired).HasDefaultValue(false);
            entity.Property(x => x.IsEmployeeSelectable).HasDefaultValue(true);
            entity.Property(x => x.IsManagerSelectable).HasDefaultValue(true);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.WorkflowDefinition)
                .WithMany(x => x.RequestReasons)
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.RequestType, x.WorkflowDefinitionId, x.ReasonCode })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ReasonName);
            entity.HasIndex(x => new { x.RequestType, x.ReasonCategory, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.SortOrder, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate });
        }
    }
}
