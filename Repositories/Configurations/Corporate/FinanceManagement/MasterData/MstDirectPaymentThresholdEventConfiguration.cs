using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.MasterData;

public sealed class MstDirectPaymentThresholdEventConfiguration : IEntityTypeConfiguration<MstDirectPaymentThresholdEvent>
{
    public void Configure(EntityTypeBuilder<MstDirectPaymentThresholdEvent> entity)
    {
        entity.ToTable("MstDirectPaymentThresholdEvent", "public", table =>
        {
            table.HasCheckConstraint(
                "CK_MstDirectPaymentThresholdEvent_EventType",
                "\"EventType\" IN ('PROPOSAL_CREATED', 'PROPOSAL_APPROVED', 'PROPOSAL_REJECTED', 'THRESHOLD_ACTIVATED', 'THRESHOLD_SUPERSEDED')");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.EventType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.PreviousAmount).HasPrecision(18, 2);
        entity.Property(x => x.NewAmount).HasPrecision(18, 2);
        entity.Property(x => x.StatusBefore).HasMaxLength(40);
        entity.Property(x => x.StatusAfter).HasMaxLength(40).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500);
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ActorIsSuperAdmin).HasDefaultValue(false);
        entity.Property(x => x.IsSelfApproval).HasDefaultValue(false);
        entity.Property(x => x.IsLegacyBaseline).HasDefaultValue(false);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // Restrict: kejadian tidak boleh ikut hilang bersama pengajuan, dan pengajuan tidak dapat dihapus selama punya kejadian.
        entity.HasOne(x => x.Proposal)
            .WithMany()
            .HasForeignKey(x => x.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.ProposalId).HasDatabaseName("IX_MstDirectPaymentThresholdEvent_ProposalId");
        entity.HasIndex(x => x.OccurredAt).HasDatabaseName("IX_MstDirectPaymentThresholdEvent_OccurredAt");
        entity.HasIndex(x => x.EventType).HasDatabaseName("IX_MstDirectPaymentThresholdEvent_EventType");
    }
}
