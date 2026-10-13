using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableWriteOffProposalItemConfiguration : IEntityTypeConfiguration<FinReceivableWriteOffProposalItem>
{
    public void Configure(EntityTypeBuilder<FinReceivableWriteOffProposalItem> entity)
    {
        entity.ToTable("FinReceivableWriteOffProposalItem", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceivableWriteOffProposalItem_Amounts",
                "\"ProposedAmount\" > 0 AND \"ApprovedAmount\" >= 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.OutstandingSnapshot).HasPrecision(18, 2);
        entity.Property(x => x.ProposedAmount).HasPrecision(18, 2);
        entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
        entity.Property(x => x.Note).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Proposal)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.InvoiceBatch)
            .WithMany()
            .HasForeignKey(x => x.InvoiceBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Receivable)
            .WithMany()
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.WriteOff)
            .WithMany()
            .HasForeignKey(x => x.WriteOffId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasIndex(x => new { x.ProposalId, x.ReceivableId }).HasDatabaseName("IX_FinReceivableWriteOffProposalItem_Proposal_Receivable");
        entity.HasIndex(x => x.InvoiceBatchId).HasDatabaseName("IX_FinReceivableWriteOffProposalItem_InvoiceBatchId");
        entity.HasIndex(x => x.ReceivableId).HasDatabaseName("IX_FinReceivableWriteOffProposalItem_ReceivableId");
        entity.HasIndex(x => x.WriteOffId).HasDatabaseName("IX_FinReceivableWriteOffProposalItem_WriteOffId");
    }
}
