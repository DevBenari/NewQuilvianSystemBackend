using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class MstOnboardingTemplateTaskConfiguration : IEntityTypeConfiguration<MstOnboardingTemplateTask>
    {
        public void Configure(EntityTypeBuilder<MstOnboardingTemplateTask> builder)
        {
            builder.ToTable("MstOnboardingTemplateTask", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.TaskCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TaskName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.TaskCategory).HasMaxLength(50);
            builder.Property(x => x.ResponsiblePartyType).HasMaxLength(50);
            builder.Property(x => x.CompletionSource).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.OnboardingTemplate).WithMany(x => x.Tasks).HasForeignKey(x => x.OnboardingTemplateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ResponsibleOrganizationUnit).WithMany().HasForeignKey(x => x.ResponsibleOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ResponsiblePosition).WithMany().HasForeignKey(x => x.ResponsiblePositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.OnboardingTemplateId, x.TaskCode }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.OnboardingTemplateId, x.SortOrder, x.IsActive });
        }
    }
}
