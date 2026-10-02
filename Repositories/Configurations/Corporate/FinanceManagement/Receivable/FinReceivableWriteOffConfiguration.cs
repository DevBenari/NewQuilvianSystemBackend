using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

/// <summary>
/// Bentuknya sama persis dengan FinReceivableAdjustmentConfiguration kecuali tanpa Direction dan
/// SourceHandoffAdjustmentId (data-dictionary.md §2.5). Check constraint Status/Amount/maker-checker
/// dan nama index UK dibentuk dengan pola yang identik, karena dokumen sumber menyatakan
/// keduanya "memakai bentuk yang sama" — bukan diciptakan sepihak.
/// </summary>
public sealed class FinReceivableWriteOffConfiguration : IEntityTypeConfiguration<FinReceivableWriteOff>
{
    public void Configure(EntityTypeBuilder<FinReceivableWriteOff> entity)
    {
        entity.ToTable("FinReceivableWriteOff", "public", table =>
        {
            table.HasCheckConstraint("CK_FinReceivableWriteOff_Status", "\"Status\" IN ('REQUESTED','APPROVED','REJECTED')");
            table.HasCheckConstraint("CK_FinReceivableWriteOff_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_FinReceivableWriteOff_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
        });
        entity.HasKey(x => x.Id);

        entity.Property(x => x.WriteOffNumber).HasMaxLength(50).IsRequired();
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
            .WithMany(x => x.WriteOffs)
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.WriteOffNumber).IsUnique().HasFilter("\"IsDelete\" = false").HasDatabaseName("IX_FinReceivableWriteOff_Number");
    }
}
