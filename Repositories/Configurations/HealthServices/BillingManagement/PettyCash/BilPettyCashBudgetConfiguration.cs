using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Configurations;

public sealed class BilPettyCashBudgetConfiguration : IEntityTypeConfiguration<BilPettyCashBudget>
{
    public void Configure(EntityTypeBuilder<BilPettyCashBudget> entity)
    {
        entity.ToTable("BilPettyCashBudget", "public", table =>
        {
            table.HasCheckConstraint("CK_BilPettyCashBudget_CurrentBalance", "\"CurrentBalance\" >= 0");
            table.HasCheckConstraint("CK_BilPettyCashBudget_Status", "\"Status\" IN ('ACTIVE','INACTIVE')");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.PoolCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.PoolName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.CurrentBalance).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.TotalTopUpAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.TotalDisbursedAmount).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired().HasDefaultValue(PettyCashBudgetStatuses.Active);
        entity.Property(x => x.LastMovementAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasIndex(x => x.PoolCode)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_BilPettyCashBudget_PoolCode");

        // "Tepat satu kolam aktif" selama PC-DEC-010 masih berlaku (PC-DES-014).
        // Unique parsial pada kolom Status: dua baris aktif akan sama-sama bernilai
        // 'ACTIVE', sehingga baris kedua ditolak index ini.
        entity.HasIndex(x => x.Status)
            .IsUnique()
            .HasFilter("\"Status\" = 'ACTIVE' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_BilPettyCashBudget_ActiveSingleton");

        // Kolam tunggal HOSPITAL_MAIN — 02-backend-architecture.md § "Rencana data master awal".
        // Saldo awal sengaja 0 agar angka pertama di layar adalah angka yang
        // benar-benar dimasukkan Finance, bukan angka karangan migration.
        var seedTime = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);
        entity.HasData(new BilPettyCashBudget
        {
            Id = new Guid("b7c1f5a2-9d34-4e88-9a10-000000000001"),
            PoolCode = "HOSPITAL_MAIN",
            PoolName = "Kas Kecil Rumah Sakit",
            CurrentBalance = 0m,
            TotalTopUpAmount = 0m,
            TotalDisbursedAmount = 0m,
            Status = PettyCashBudgetStatuses.Active,
            LastMovementAt = null,
            RowVersion = new Guid("b7c1f5a2-9d34-4e88-9a10-0000000000f1"),
            CreateDateTime = seedTime,
            CreateBy = Guid.Empty,
            UpdateBy = Guid.Empty,
            DeleteBy = Guid.Empty,
            CancelBy = Guid.Empty,
            IsDelete = false,
            IsCancel = false
        });
    }
}
