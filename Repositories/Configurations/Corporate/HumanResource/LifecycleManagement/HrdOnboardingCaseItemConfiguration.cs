using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdOnboardingCaseItemConfiguration : IEntityTypeConfiguration<HrdOnboardingCaseItem>
    {
        public void Configure(EntityTypeBuilder<HrdOnboardingCaseItem> builder)
        {
            builder.ToTable("HrdOnboardingCaseItem", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ItemName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.TaskCategory).HasMaxLength(50);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.SystemItemCode).HasMaxLength(40);
            builder.Property(x => x.TemplateLayer).HasConversion<int>().IsRequired();
            builder.Property(x => x.ResponsibleRole).HasConversion<int>().IsRequired();
            builder.Property(x => x.ItemKind).HasConversion<int>().IsRequired();
            builder.Property(x => x.AutomaticSource).HasConversion<int>();
            builder.Property(x => x.IsRequired).HasDefaultValue(true);
            builder.Property(x => x.DueDayOffset).HasDefaultValue(0);
            builder.Property(x => x.DueDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.ItemStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletionNote).HasMaxLength(1000);
            builder.Property(x => x.EvidenceStoragePath).HasMaxLength(500);
            builder.Property(x => x.EvidenceFileName).HasMaxLength(250);
            builder.Property(x => x.EvidenceContentType).HasMaxLength(150);
            builder.Property(x => x.AutomaticCheckedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AutomaticResultNote).HasMaxLength(250);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.OnboardingCase).WithMany(c => c.Items).HasForeignKey(x => x.OnboardingCaseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SourceTemplateTask).WithMany().HasForeignKey(x => x.SourceTemplateTaskId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CertificationType).WithMany().HasForeignKey(x => x.CertificationTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SelectedWorkforceProfile).WithMany().HasForeignKey(x => x.SelectedWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.OnboardingCaseId, x.ItemCode })
                .HasDatabaseName("IX_HrdOnboardingCaseItem_OnboardingCaseId_ItemCode")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.SystemItemCode);
        }
    }
}
