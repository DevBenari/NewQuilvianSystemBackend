using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Collection;

public sealed class FinCrossEntryTransactionConfiguration : IEntityTypeConfiguration<FinCrossEntryTransaction>
{
    public void Configure(EntityTypeBuilder<FinCrossEntryTransaction> builder)
    {
        builder.ToTable("FinCrossEntryTransaction", "public", t =>
        {
            t.HasCheckConstraint("CK_FinCrossEntryTransaction_Direction", "\"Direction\" IN ('CREDIT', 'DEBIT')");
            t.HasCheckConstraint("CK_FinCrossEntryTransaction_Amount", "\"Amount\" > 0");
            t.HasCheckConstraint("CK_FinCrossEntryTransaction_Balance", "\"BalanceAfterTransaction\" >= 0");
            t.HasCheckConstraint("CK_FinCrossEntryTransaction_Type", "\"TransactionType\" IN ('INITIAL_RECEIPT', 'AR_ALLOCATION', 'AR_ALLOCATION_REVERSAL')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionType)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Direction)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.BalanceAfterTransaction)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.HasOne(x => x.CrossEntry)
            .WithMany(c => c.Transactions)
            .HasForeignKey(x => x.CrossEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReceiptAllocation)
            .WithMany()
            .HasForeignKey(x => x.ReceiptAllocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ReversalOfTransaction)
            .WithMany()
            .HasForeignKey(x => x.ReversalOfTransactionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CrossEntryId, x.OccurredAt })
            .HasDatabaseName("IX_FinCrossEntryTransaction_CrossEntry_OccurredAt");

        builder.HasIndex(x => x.ReceiptAllocationId)
            .HasFilter("\"ReceiptAllocationId\" IS NOT NULL")
            .HasDatabaseName("IX_FinCrossEntryTransaction_ReceiptAllocationId");
    }
}
