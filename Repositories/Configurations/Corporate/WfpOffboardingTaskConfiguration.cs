using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpOffboardingTaskConfiguration : IEntityTypeConfiguration<WfpOffboardingTask>
    {
        public void Configure(EntityTypeBuilder<WfpOffboardingTask> entity)
        {
            entity.ToTable("WfpOffboardingTask", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.OffboardingChecklistId)
                .IsRequired();

            entity.Property(x => x.TaskCode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.TaskName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.TaskCategory)
                .HasConversion<int>()
                .HasDefaultValue(OffboardingTaskCategory.Other)
                .IsRequired();

            entity.Property(x => x.IsRequired)
                .HasDefaultValue(true);

            entity.Property(x => x.IsCompleted)
                .HasDefaultValue(false);

            entity.Property(x => x.CompletedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CompletedByUserId)
                .IsRequired(false);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.OffboardingChecklist)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.OffboardingChecklistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompletedByUser)
                .WithMany()
                .HasForeignKey(x => x.CompletedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.OffboardingChecklistId);

            entity.HasIndex(x => x.CompletedByUserId);

            entity.HasIndex(x => new
            {
                x.OffboardingChecklistId,
                x.TaskCategory,
                x.IsCompleted,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.OffboardingChecklistId,
                x.TaskCode
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.OffboardingChecklistId,
                x.SortOrder
            });

            entity.HasIndex(x => new
            {
                x.IsRequired,
                x.IsCompleted,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
