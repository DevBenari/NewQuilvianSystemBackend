using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableMovementConfiguration : IEntityTypeConfiguration<FinReceivableMovement>
{
    public void Configure(EntityTypeBuilder<FinReceivableMovement> entity)
    {
        entity.ToTable("FinReceivableMovement", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceivableMovement_Balance", "\"BalanceAfter\" = \"BalanceBefore\" + \"Amount\"");
            table.HasCheckConstraint("CK_FinReceivableMovement_FundingSource", "\"PaymentMethodCode\" IS NULL OR \"FundingSourceType\" IS NOT NULL");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.MovementType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
        entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
        entity.Property(x => x.BusinessDate).HasColumnType("date").IsRequired();
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();
        entity.Property(x => x.PaymentMethodCode).HasMaxLength(30);
        entity.Property(x => x.FundingSourceType).HasMaxLength(30);
        entity.Property(x => x.ReferenceNumber).HasMaxLength(100);
        entity.Property(x => x.Notes).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Receivable)
            .WithMany(x => x.Movements)
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_FinReceivableMovement_FinReceivable_ReceivableId");

        entity.HasIndex(x => new { x.ReceivableId, x.BusinessDate }).HasDatabaseName("IX_FinReceivableMovement_ReceivableId_BusinessDate");
        entity.HasIndex(x => x.BusinessDate).HasDatabaseName("IX_FinReceivableMovement_BusinessDate");
        entity.HasIndex(x => x.SourceAllocationId).HasDatabaseName("IX_FinReceivableMovement_SourceAllocationId");
        entity.HasIndex(x => x.PaymentMethodCode).HasDatabaseName("IX_FinReceivableMovement_PaymentMethodCode");
        entity.HasIndex(x => x.OpeningItemBatchId).HasDatabaseName("IX_FinReceivableMovement_OpeningItemBatchId");
        entity.HasIndex(x => x.CorrelationId).HasDatabaseName("IX_FinReceivableMovement_CorrelationId");

        entity.HasIndex(x => x.ProofId)
            .IsUnique()
            .HasFilter("\"ProofId\" IS NOT NULL AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinReceivableMovement_ProofId");
    }
}
