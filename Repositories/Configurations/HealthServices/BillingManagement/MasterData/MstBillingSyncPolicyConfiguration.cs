using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Configurations;

public sealed class MstBillingSyncPolicyConfiguration : IEntityTypeConfiguration<MstBillingSyncPolicy>
{
    public void Configure(EntityTypeBuilder<MstBillingSyncPolicy> entity)
    {
        // Batas angka sama dengan validation matrix V2 RJE-VAL-030, dijaga juga di database
        // supaya perubahan lewat jalur apa pun tidak dapat menyimpan kebijakan yang tidak sah.
        entity.ToTable("MstBillingSyncPolicy", "public", table =>
        {
            table.HasCheckConstraint("CK_MstBillingSyncPolicy_MaxAttemptCount", "\"MaxAttemptCount\" BETWEEN 0 AND 20");
            table.HasCheckConstraint("CK_MstBillingSyncPolicy_BaseDelaySeconds", "\"BaseDelaySeconds\" BETWEEN 10 AND 3600");
            table.HasCheckConstraint("CK_MstBillingSyncPolicy_MaxDelaySeconds", "\"MaxDelaySeconds\" >= \"BaseDelaySeconds\" AND \"MaxDelaySeconds\" <= 86400");
        });

        entity.HasKey(x => x.Id);
        entity.Property(x => x.PolicyCode).HasMaxLength(50).IsRequired();
        entity.Property(x => x.PolicyName).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Description).HasMaxLength(500);
        entity.Property(x => x.IsActive).HasDefaultValue(true);
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.PolicyCode).IsUnique().HasFilter("\"IsDelete\" = false");

        // Nilai usulan awal (02-backend-architecture.md V2.11), bukan kebijakan rumah sakit.
        // Dapat diubah admin Billing tanpa rilis.
        var seedTime = new DateTime(2026, 9, 28, 0, 0, 0, DateTimeKind.Utc);
        entity.HasData(
            Seed(new Guid("5d0c2b61-7f3a-4e8e-9b1c-2a6f0e3d9a01"), BillingSyncPolicyCodes.FactDispatch,
                "Kirim ulang fakta klinis ke folio",
                "Penyerahan fakta klinis yang hasilnya belum pasti dikirim ulang dengan identitas dan kunci yang sama.",
                new Guid("9b3e7c42-1d58-4f06-a2b7-6c1e8d4f0a11"), seedTime),
            Seed(new Guid("5d0c2b61-7f3a-4e8e-9b1c-2a6f0e3d9a02"), BillingSyncPolicyCodes.InvoiceSync,
                "Kirim ulang efek folio ke invoice",
                "Efek folio yang gagal diteruskan ke invoice canonical dikirim ulang dengan kunci idempotency yang sama.",
                new Guid("9b3e7c42-1d58-4f06-a2b7-6c1e8d4f0a12"), seedTime));
    }

    private static MstBillingSyncPolicy Seed(Guid id, string code, string name, string description, Guid rowVersion, DateTime createdAt) => new()
    {
        Id = id,
        PolicyCode = code,
        PolicyName = name,
        MaxAttemptCount = 5,
        BaseDelaySeconds = 60,
        MaxDelaySeconds = 3600,
        IsActive = true,
        Description = description,
        RowVersion = rowVersion,
        CreateDateTime = createdAt,
        CreateBy = Guid.Empty,
        UpdateBy = Guid.Empty,
        DeleteBy = Guid.Empty,
        CancelBy = Guid.Empty,
        IsDelete = false,
        IsCancel = false
    };
}
