using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class MstOffboardingTemplateTaskConfiguration : IEntityTypeConfiguration<MstOffboardingTemplateTask>
    {
        public void Configure(EntityTypeBuilder<MstOffboardingTemplateTask> builder)
        {
            builder.ToTable("MstOffboardingTemplateTask", "public");
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
            builder.HasOne(x => x.OffboardingTemplate).WithMany(x => x.Tasks).HasForeignKey(x => x.OffboardingTemplateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ResponsibleOrganizationUnit).WithMany().HasForeignKey(x => x.ResponsibleOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ResponsiblePosition).WithMany().HasForeignKey(x => x.ResponsiblePositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.OffboardingTemplateId, x.TaskCode }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.OffboardingTemplateId, x.SortOrder, x.IsActive });
        }
    }
}
