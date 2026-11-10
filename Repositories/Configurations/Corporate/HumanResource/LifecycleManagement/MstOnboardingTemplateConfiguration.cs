using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class MstOnboardingTemplateConfiguration : IEntityTypeConfiguration<MstOnboardingTemplate>
    {
        public void Configure(EntityTypeBuilder<MstOnboardingTemplate> builder)
        {
            builder.ToTable("MstOnboardingTemplate", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.TemplateCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.TemplateName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.TemplateLayer).HasConversion<int>().IsRequired();
            builder.Property(x => x.TargetCompletionDays).HasDefaultValue(30);
            builder.Property(x => x.Version).HasDefaultValue(1);
            builder.Property(x => x.Description).HasMaxLength(1500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Tasks)
                .WithOne(t => t.OnboardingTemplate)
                .HasForeignKey(t => t.OnboardingTemplateId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TemplateCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.TemplateLayer)
                .HasDatabaseName("IX_MstOnboardingTemplate_TemplateLayer_Active")
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false AND \"TemplateLayer\" IN (1, 2)");

            builder.HasIndex(x => x.OrganizationUnitId)
                .HasDatabaseName("IX_MstOnboardingTemplate_OrganizationUnitId_Active")
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false AND \"TemplateLayer\" = 3");
        }
    }
}
