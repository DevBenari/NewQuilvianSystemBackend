using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableAdjustmentConfiguration : IEntityTypeConfiguration<FinReceivableAdjustment>
{
    public void Configure(EntityTypeBuilder<FinReceivableAdjustment> entity)
    {
        entity.ToTable("FinReceivableAdjustment", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceivableAdjustment_Direction", "\"Direction\" IN ('DEBIT','CREDIT')");
            table.HasCheckConstraint("CK_FinReceivableAdjustment_Status", "\"Status\" IN ('REQUESTED','APPROVED','REJECTED')");
            table.HasCheckConstraint("CK_FinReceivableAdjustment_Amount", "\"Amount\" > 0");
            // Maker-checker: pengaju tidak boleh menyetujui permohonannya sendiri (FIN-DEC-012).
            table.HasCheckConstraint("CK_FinReceivableAdjustment_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.AdjustmentNumber).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Direction).HasMaxLength(10).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(FinReceivableApprovalStatuses.Requested);
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RejectionReason).HasMaxLength(500);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Receivable)
            .WithMany(x => x.Adjustments)
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.AdjustmentNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinReceivableAdjustment_Number");
    }
}
