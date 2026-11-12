using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdOnboardingAccessRequestConfiguration : IEntityTypeConfiguration<HrdOnboardingAccessRequest>
    {
        public void Configure(EntityTypeBuilder<HrdOnboardingAccessRequest> builder)
        {
            builder.ToTable("HrdOnboardingAccessRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.RequestType).HasConversion<int>().IsRequired();
            builder.Property(x => x.RequestStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.FailureMessage).HasMaxLength(500);
            builder.Property(x => x.SelfServiceActivatedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletionNote).HasMaxLength(500);

            builder.HasOne(x => x.OnboardingCase).WithMany(c => c.AccessRequests).HasForeignKey(x => x.OnboardingCaseId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApplicationUser).WithMany().HasForeignKey(x => x.ApplicationUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.OnboardingCaseId, x.RequestType })
                .HasDatabaseName("IX_HrdOnboardingAccessRequest_Case_Type")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
        }
    }
}
