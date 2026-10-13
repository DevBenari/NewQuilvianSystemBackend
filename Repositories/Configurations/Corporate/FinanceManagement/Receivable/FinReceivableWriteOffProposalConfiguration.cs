using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableWriteOffProposalConfiguration : IEntityTypeConfiguration<FinReceivableWriteOffProposal>
{
    public void Configure(EntityTypeBuilder<FinReceivableWriteOffProposal> entity)
    {
        entity.ToTable("FinReceivableWriteOffProposal", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceivableWriteOffProposal_Status",
                "\"Status\" IN ('DRAFT','SUBMITTED','APPROVED','POSTED','REJECTED')");
            table.HasCheckConstraint("CK_FinReceivableWriteOffProposal_Amounts",
                "\"TotalProposedAmount\" >= 0 AND \"TotalApprovedAmount\" >= 0");
            table.HasCheckConstraint("CK_FinReceivableWriteOffProposal_MakerChecker",
                "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.ProposalNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ProposalDate).IsRequired();
        entity.Property(x => x.ProposalType).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableWriteOffProposalTypes.PerInvoice);
        entity.Property(x => x.ServiceType).HasMaxLength(30);
        entity.Property(x => x.DebtorReferenceId);
        entity.Property(x => x.ReasonCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.ReasonDescription).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableWriteOffProposalStatuses.Draft);

        entity.Property(x => x.TotalInvoiceAmount).HasPrecision(18, 2);
        entity.Property(x => x.TotalOutstandingAmount).HasPrecision(18, 2);
        entity.Property(x => x.TotalProposedAmount).HasPrecision(18, 2);
        entity.Property(x => x.TotalApprovedAmount).HasPrecision(18, 2);

        entity.Property(x => x.RequestedBy).IsRequired();
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone").IsRequired();
        entity.Property(x => x.ApprovedBy);
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.PostedBy);
        entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectedBy);
        entity.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectionReason).HasMaxLength(500);

        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasMany(x => x.Items)
            .WithOne(x => x.Proposal)
            .HasForeignKey(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => x.ProposalNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinReceivableWriteOffProposal_Number");
        entity.HasIndex(x => x.Status).HasDatabaseName("IX_FinReceivableWriteOffProposal_Status");
        entity.HasIndex(x => x.ProposalDate).HasDatabaseName("IX_FinReceivableWriteOffProposal_Date");
        entity.HasIndex(x => x.DebtorReferenceId).HasDatabaseName("IX_FinReceivableWriteOffProposal_DebtorReferenceId");
    }
}
