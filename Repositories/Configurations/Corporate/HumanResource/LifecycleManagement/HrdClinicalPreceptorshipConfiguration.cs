using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdClinicalPreceptorshipConfiguration : IEntityTypeConfiguration<HrdClinicalPreceptorship>
    {
        public void Configure(EntityTypeBuilder<HrdClinicalPreceptorship> builder)
        {
            builder.ToTable("HrdClinicalPreceptorship", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.PreceptorshipStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.AccompaniedSince).HasColumnType("date");
            builder.Property(x => x.IndependentOn).HasColumnType("date");
            builder.Property(x => x.EvidenceStoragePath).HasMaxLength(500);
            builder.Property(x => x.EvidenceFileName).HasMaxLength(250);
            builder.Property(x => x.EvidenceContentType).HasMaxLength(150);
            builder.Property(x => x.AssessmentNote).HasMaxLength(2000);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.OnboardingCase)
                .WithOne(c => c.ClinicalPreceptorship)
                .HasForeignKey<HrdClinicalPreceptorship>(x => x.OnboardingCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DeclaredByUser).WithMany().HasForeignKey(x => x.DeclaredByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OnboardingCaseId)
                .HasDatabaseName("IX_HrdClinicalPreceptorship_OnboardingCaseId")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.OrganizationUnitId);
            builder.HasIndex(x => x.PreceptorshipStatus);
        }
    }
}
