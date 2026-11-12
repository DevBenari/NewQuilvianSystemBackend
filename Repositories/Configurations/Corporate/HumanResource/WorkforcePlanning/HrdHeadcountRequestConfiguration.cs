using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdHeadcountRequestConfiguration : IEntityTypeConfiguration<HrdHeadcountRequest>
    {
        public void Configure(EntityTypeBuilder<HrdHeadcountRequest> builder)
        {
            builder.ToTable("HrdHeadcountRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequiredStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.TemporaryEndDate).HasColumnType("date");
            builder.Property(x => x.Justification).HasMaxLength(2000);
            builder.Property(x => x.RequestedQuantity).HasPrecision(9, 2).HasDefaultValue(0m);
            builder.Property(x => x.ExternalSeatCount).HasDefaultValue(0);
            builder.Property(x => x.InternalSeatCount).HasDefaultValue(0);
            builder.Property(x => x.RemainingSeatsAtSubmission).HasPrecision(9, 2);
            builder.Property(x => x.EstimatedMonthlyRate).HasPrecision(18, 2);
            builder.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            builder.Property(x => x.FinanceAdjustedCost).HasPrecision(18, 2);
            builder.Property(x => x.FinanceAdjustmentReason).HasMaxLength(1000);
            builder.Property(x => x.FinanceAdjustedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsOverFormationAfterAddendum).HasDefaultValue(false);
            builder.Property(x => x.IsOverdue).HasDefaultValue(false);
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ReadyForRecruitmentAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.WithdrawnAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SalaryGrade)
                .WithMany()
                .HasForeignKey(x => x.SalaryGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TemporaryReasonWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.TemporaryReasonWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FormationVersionAtSubmission)
                .WithMany()
                .HasForeignKey(x => x.FormationVersionIdAtSubmission)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedFormation)
                .WithMany()
                .HasForeignKey(x => x.CreatedFormationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.RequestNumber)
                .IsUnique()
                .HasDatabaseName("IX_HrdHeadcountRequest_RequestNumber");

            builder.HasIndex(x => new { x.OrganizationUnitId, x.PositionId, x.RequestStatus })
                .HasDatabaseName("IX_HrdHeadcountRequest_Unit_Position_Status");

            builder.HasIndex(x => x.IsOverFormationAfterAddendum);
            builder.HasIndex(x => x.IsOverdue);
            builder.HasIndex(x => x.WorkflowInstanceId);
        }
    }
}
