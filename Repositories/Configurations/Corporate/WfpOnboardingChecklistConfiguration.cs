using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpOnboardingChecklistConfiguration : IEntityTypeConfiguration<WfpOnboardingChecklist>
    {
        public void Configure(EntityTypeBuilder<WfpOnboardingChecklist> entity)
        {
            entity.ToTable("WfpOnboardingChecklist", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.OnboardingType)
                .HasConversion<int>()
                .HasDefaultValue(OnboardingType.Unknown)
                .IsRequired();

            entity.Property(x => x.StartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.TargetCompletionDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.CompletedDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Status)
                .HasConversion<int>()
                .HasDefaultValue(OnboardingStatus.Draft)
                .IsRequired();

            entity.Property(x => x.AssignedToUserId)
                .IsRequired(false);

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

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

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.OnboardingChecklists)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedToUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.AssignedToUserId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.Status,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.OnboardingType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.Status,
                x.TargetCompletionDate,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.Status
            })
            .HasFilter("\"IsDelete\" = false");
        }
    }
}
