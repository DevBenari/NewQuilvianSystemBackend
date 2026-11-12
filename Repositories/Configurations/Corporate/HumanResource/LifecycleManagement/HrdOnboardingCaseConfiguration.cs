using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdOnboardingCaseConfiguration : IEntityTypeConfiguration<HrdOnboardingCase>
    {
        public void Configure(EntityTypeBuilder<HrdOnboardingCase> builder)
        {
            builder.ToTable("HrdOnboardingCase", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.CaseNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.PlannedStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.CaseStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.FirstDayAttendedOn).HasColumnType("date");
            builder.Property(x => x.ReadyForWorkAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelReason).HasConversion<int>();
            builder.Property(x => x.CancelNote).HasMaxLength(1000);
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.HasGeneralTemplate).HasDefaultValue(true);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HiringHandoff).WithMany().HasForeignKey(x => x.HiringHandoffId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.HospitalSite).WithMany().HasForeignKey(x => x.HospitalSiteId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.OrganizationUnit).WithMany().HasForeignKey(x => x.OrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Position).WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CoordinatorUser).WithMany().HasForeignKey(x => x.CoordinatorUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FirstDayRecordedByUser).WithMany().HasForeignKey(x => x.FirstDayRecordedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReadyForWorkByUser).WithMany().HasForeignKey(x => x.ReadyForWorkByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CaseNumber).IsUnique();
            builder.HasIndex(x => x.HiringHandoffId).HasDatabaseName("IX_HrdOnboardingCase_HiringHandoffId").IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.OrganizationUnitId, x.CaseStatus }).HasDatabaseName("IX_HrdOnboardingCase_OrganizationUnitId_CaseStatus");
            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.HospitalSiteId);
            builder.HasIndex(x => x.PlannedStartDate);
            builder.HasIndex(x => x.CoordinatorUserId);
        }
    }
}
