using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Collection;

public sealed class FinCrossEntryConfiguration : IEntityTypeConfiguration<FinCrossEntry>
{
    public void Configure(EntityTypeBuilder<FinCrossEntry> builder)
    {
        builder.ToTable("FinCrossEntry", "public", t =>
        {
            t.HasCheckConstraint("CK_FinCrossEntry_OriginalAmount", "\"OriginalAmount\" > 0");
            t.HasCheckConstraint("CK_FinCrossEntry_Status", "\"Status\" IN ('OPEN', 'PARTIALLY_USED', 'FULLY_USED', 'CANCELLED')");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CrossEntryNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.OriginalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TransactionDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .HasMaxLength(30)
            .HasDefaultValue(FinCrossEntryStatuses.Open)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsConcurrencyToken()
            .IsRequired();

        // Relasi
        builder.HasOne(x => x.InsuranceProvider)
            .WithMany()
            .HasForeignKey(x => x.InsuranceProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Receipt)
            .WithOne()
            .HasForeignKey<FinCrossEntry>(x => x.ReceiptId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Transactions)
            .WithOne(t => t.CrossEntry)
            .HasForeignKey(t => t.CrossEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Documents)
            .WithOne(d => d.CrossEntry)
            .HasForeignKey(d => d.CrossEntryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(x => x.CrossEntryNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinCrossEntry_CrossEntryNumber_Active");

        builder.HasIndex(x => x.ReceiptId)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinCrossEntry_ReceiptId_Active");

        builder.HasIndex(x => new { x.InsuranceProviderId, x.TransactionDate })
            .HasDatabaseName("IX_FinCrossEntry_InsuranceProvider_Date");

        builder.HasIndex(x => new { x.BankAccountId, x.TransactionDate })
            .HasDatabaseName("IX_FinCrossEntry_BankAccount_Date");

        builder.HasIndex(x => new { x.Status, x.TransactionDate })
            .HasDatabaseName("IX_FinCrossEntry_Status_Date");

        builder.HasIndex(x => x.ReferenceNumber)
            .HasDatabaseName("IX_FinCrossEntry_ReferenceNumber");
    }
}
