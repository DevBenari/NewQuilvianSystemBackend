using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelAdvanceRequestConfiguration : IEntityTypeConfiguration<TrxTravelAdvanceRequest>
    {
        public void Configure(EntityTypeBuilder<TrxTravelAdvanceRequest> entity)
        {
            entity.ToTable("TrxTravelAdvanceRequest", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AdvanceRequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RequestDate).HasColumnType("date");
            entity.Property(x => x.RequiredPaymentDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.RequestedAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaidAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.AdvanceBreakdownJson).HasColumnType("jsonb");
            entity.Property(x => x.AdvanceStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.SettledAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.AdvanceRequests).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTravelParticipant).WithMany(x => x.AdvanceRequests).HasForeignKey(x => x.BusinessTravelParticipantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentSettlementMethod).WithMany().HasForeignKey(x => x.PaymentSettlementMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPeriod).WithMany().HasForeignKey(x => x.PayrollPeriodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RejectedByUser).WithMany().HasForeignKey(x => x.RejectedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AdvanceRequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.WorkforceProfileId, x.AdvanceStatus, x.IsDelete });
            entity.HasIndex(x => new { x.FinancePaymentId, x.GlHeaderId });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelAdvanceRequest> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
