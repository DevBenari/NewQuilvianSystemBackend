using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Movement;

public sealed class AstLoanConfiguration : IEntityTypeConfiguration<AstLoan>
{
    public void Configure(EntityTypeBuilder<AstLoan> entity)
    {
        entity.ToTable("AstLoan", "public", table =>
        {
            table.HasCheckConstraint("CK_AstLoan_DifferentUnits", "\"OwnerOrganizationUnitId\" <> \"BorrowerOrganizationUnitId\"");
            table.HasCheckConstraint("CK_AstLoan_ReturnAfterStart", "\"PlannedReturnDate\" >= \"StartDate\"");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.LoanNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Purpose).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.StartDate).HasColumnType("date");
        entity.Property(x => x.PlannedReturnDate).HasColumnType("date");
        entity.Property(x => x.OriginalPlannedReturnDate).HasColumnType("date");
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsApprovalByRequester).HasDefaultValue(false);
        entity.Property(x => x.PickedUpAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReturnedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReturnConfirmedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DecisionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.OwnerOrganizationUnit).WithMany().HasForeignKey(x => x.OwnerOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.BorrowerOrganizationUnit).WithMany().HasForeignKey(x => x.BorrowerOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.LoanNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstLoan_LoanNumber");

        entity.HasIndex(x => new { x.Status, x.PlannedReturnDate })
            .HasDatabaseName("IX_AstLoan_Status_PlannedReturnDate");

        entity.HasIndex(x => x.OwnerOrganizationUnitId);
        entity.HasIndex(x => x.BorrowerOrganizationUnitId);
        entity.HasIndex(x => x.RequestedBy);
    }
}
