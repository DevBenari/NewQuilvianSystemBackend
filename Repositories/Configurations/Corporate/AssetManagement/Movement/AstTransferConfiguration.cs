using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Movement;

public sealed class AstTransferConfiguration : IEntityTypeConfiguration<AstTransfer>
{
    public void Configure(EntityTypeBuilder<AstTransfer> entity)
    {
        entity.ToTable("AstTransfer", "public", table =>
        {
            table.HasCheckConstraint("CK_AstTransfer_DifferentUnits", "\"SourceOrganizationUnitId\" <> \"TargetOrganizationUnitId\"");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.TransferNumber).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasConversion<int>().IsRequired();
        entity.Property(x => x.ReasonNote).HasMaxLength(1000).IsRequired();
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.UnitApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsUnitApprovalByRequester).HasDefaultValue(false);
        entity.Property(x => x.AssetHeadApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ReceivedAt).HasColumnType("timestamp with time zone");
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

        entity.HasOne(x => x.SourceOrganizationUnit).WithMany().HasForeignKey(x => x.SourceOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.TargetOrganizationUnit).WithMany().HasForeignKey(x => x.TargetOrganizationUnitId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.TargetLocation).WithMany().HasForeignKey(x => x.TargetLocationId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.TargetCustodianEmployee).WithMany().HasForeignKey(x => x.TargetCustodianEmployeeId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.CorrectsTransfer).WithMany().HasForeignKey(x => x.CorrectsTransferId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.TransferNumber)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_AstTransfer_TransferNumber");

        entity.HasIndex(x => new { x.Status, x.SourceOrganizationUnitId, x.TargetOrganizationUnitId })
            .HasDatabaseName("IX_AstTransfer_Status_Units");

        entity.HasIndex(x => x.SourceOrganizationUnitId);
        entity.HasIndex(x => x.TargetOrganizationUnitId);
        entity.HasIndex(x => x.Reason);
        entity.HasIndex(x => x.CorrectsTransferId);
        entity.HasIndex(x => x.SourceStockTakeResultId);
        entity.HasIndex(x => x.RequestedBy);
    }
}
