using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class HrdContractDecisionConfiguration : IEntityTypeConfiguration<HrdContractDecision>
    {
        public void Configure(EntityTypeBuilder<HrdContractDecision> builder)
        {
            builder.ToTable("HrdContractDecision", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.DecisionNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.DecisionType).HasConversion<int>().IsRequired();
            builder.Property(x => x.ProposedStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.ProposedEndDate).HasColumnType("date");
            builder.Property(x => x.ReasonText).HasMaxLength(2000);
            builder.Property(x => x.DecisionStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DecisionNote).HasMaxLength(1000);
            builder.Property(x => x.EmployeeNotifiedOn).HasColumnType("date");
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.ContractHistory).WithMany().HasForeignKey(x => x.ContractHistoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.DecidedByUser).WithMany().HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.FollowUpContractHistory).WithMany().HasForeignKey(x => x.FollowUpContractHistoryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.NotificationRecordedByUser).WithMany().HasForeignKey(x => x.NotificationRecordedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.DecisionNumber)
                .HasDatabaseName("IX_HrdContractDecision_DecisionNumber")
                .IsUnique();

            builder.HasIndex(x => x.ContractHistoryId)
                .HasDatabaseName("IX_HrdContractDecision_ContractHistoryId_Open")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"DecisionStatus\" IN (1, 2, 4)");
        }
    }
}
