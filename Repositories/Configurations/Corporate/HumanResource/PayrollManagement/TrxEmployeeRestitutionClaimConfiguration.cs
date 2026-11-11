using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxEmployeeRestitutionClaimConfiguration : IEntityTypeConfiguration<TrxEmployeeRestitutionClaim>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeRestitutionClaim> builder)
        {
            builder.ToTable("TrxEmployeeRestitutionClaim", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ClaimNumber)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ClaimCategory)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.ClaimAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.ApprovedAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.ClaimStatus)
                .HasMaxLength(50)
                .IsRequired();

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Supervisor)
                .WithMany()
                .HasForeignKey(x => x.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.HrReviewer)
                .WithMany()
                .HasForeignKey(x => x.HrReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.FinalApprover)
                .WithMany()
                .HasForeignKey(x => x.FinalApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ClaimNumber)
                .IsUnique()
                .HasDatabaseName("uq_employee_restitution_claim_number");

            builder.HasIndex(x => new { x.EmployeeId, x.ExpenseDate })
                .HasDatabaseName("idx_restitution_claim_employee_date");

            builder.HasIndex(x => x.ClaimStatus)
                .HasDatabaseName("idx_restitution_claim_status");
        }
    }
}
