using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Configurations;

public sealed class BilPettyCashVoucherCommandConfiguration : IEntityTypeConfiguration<BilPettyCashVoucherCommand>
{
    public void Configure(EntityTypeBuilder<BilPettyCashVoucherCommand> entity)
    {
        entity.ToTable("BilPettyCashVoucherCommand", "public", table =>
        {
            // RETURN dan REVERSAL ditambahkan BE-BKC-057 (PC-DES-019, PC-DES-020) — delta
            // kontrak terhadap data-dictionary.md, yang masih mencatat tabel ini "Nol
            // perubahan bentuk" untuk gelombang 15 September 2026. Wajib diperbarui supaya
            // jejak perintah ReturnAsync/ReverseAsync tidak ditolak constraint ini.
            table.HasCheckConstraint("CK_BilPettyCashVoucherCommand_CommandType", "\"CommandType\" IN ('SUBMIT','APPROVE','REJECT','CANCEL','DISBURSE','ATTACH_PROOF','PROOF_CORRECTED','RETURN','REVERSAL')");
            // Reason wajib terisi untuk REJECT, CANCEL, RETURN, dan REVERSAL (data-dictionary.md).
            table.HasCheckConstraint("CK_BilPettyCashVoucherCommand_Reason", "\"CommandType\" NOT IN ('REJECT','CANCEL','RETURN','REVERSAL') OR \"Reason\" IS NOT NULL");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.CommandType).HasMaxLength(40).IsRequired();
        entity.Property(x => x.ActorRole).HasMaxLength(150).IsRequired();
        entity.Property(x => x.StatusBefore).HasMaxLength(30);
        entity.Property(x => x.StatusAfter).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.Property(x => x.Reason).HasMaxLength(500);
        entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
        // data-dictionary.md § "Skema dalam bentuk DDL" menetapkan text, bukan jsonb.
        entity.Property(x => x.ResponseJson).HasColumnType("text").IsRequired().HasDefaultValue("{}");

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => new { x.VoucherId, x.OccurredAt })
            .HasDatabaseName("IX_BilPettyCashVoucherCommand_VoucherId_OccurredAt");

        entity.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL")
            .HasDatabaseName("IX_BilPettyCashVoucherCommand_IdempotencyKey");

        entity.HasOne(x => x.Voucher)
            .WithMany(x => x.Commands)
            .HasForeignKey(x => x.VoucherId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
