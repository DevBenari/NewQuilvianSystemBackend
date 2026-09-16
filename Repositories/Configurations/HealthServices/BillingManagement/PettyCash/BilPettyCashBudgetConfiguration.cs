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
            // Union nilai lama (ACTIVE/INACTIVE, masih ditulis PettyCashBudgetService yang
            // belum disentuh task ini) dan nilai baru (DRAFT/CLOSED, PC-DES-017). Dipersempit
            // ke empat nilai final saat BE-BKC-054 menyentuh service tersebut.
            table.HasCheckConstraint("CK_BilPettyCashBudget_Status", "\"Status\" IN ('ACTIVE','INACTIVE','DRAFT','CLOSED')");
            table.HasCheckConstraint("CK_BilPettyCashBudget_BudgetAmount", "\"BudgetAmount\" >= 0");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.PoolCode).HasMaxLength(30).IsRequired();
        entity.Property(x => x.PoolName).HasMaxLength(100).IsRequired();
        entity.Property(x => x.PeriodStart).HasColumnType("date");
        entity.Property(x => x.PeriodEnd).HasColumnType("date");
        entity.Property(x => x.BudgetAmount).HasPrecision(18, 2).HasDefaultValue(0m);
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

        // PC-DES-017 SUPERSEDES catatan lama pada berkas ini: sebelum revisi 15 September
        // 2026, index ini unik secara GLOBAL pada PoolCode (satu PoolCode paling banyak satu
        // baris selamanya) — sengaja benar untuk kolam statis PC-DES-014. Model periode
        // butuh BANYAK baris berbagi PoolCode yang sama dari waktu ke waktu (satu per
        // periode), sehingga index lama itu akan menolak pembuatan periode kedua. Diganti
        // index non-unique murni untuk pencarian/pengurutan per kolam beserta periodenya.
        entity.HasIndex(x => new { x.PoolCode, x.PeriodStart })
            .HasDatabaseName("IX_BilPettyCashBudget_PoolCode_PeriodStart");

        // "Paling banyak satu periode aktif per kolam" (PC-DES-017), menggantikan singleton
        // global lama yang tidak mengenal konsep periode. PoolCode ikut masuk index supaya
        // desain ini tetap benar bila PC-DEC-010 (multi-kolam) kelak dicabut — untuk MVP ini,
        // dengan hanya satu PoolCode yang pernah ada, efeknya identik dengan singleton lama.
        entity.HasIndex(x => x.PoolCode)
            .IsUnique()
            .HasFilter("\"Status\" = 'ACTIVE' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_BilPettyCashBudget_ActivePerPool");

        entity.HasOne<BilPettyCashBudget>()
            .WithMany()
            .HasForeignKey(x => x.SupersededByBudgetId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kolam tunggal HOSPITAL_MAIN — 02-backend-architecture.md § "Rencana data master awal".
        // Saldo awal sengaja 0 agar angka pertama di layar adalah angka yang
        // benar-benar dimasukkan Finance, bukan angka karangan migration.
        // PeriodStart/BudgetAmount ikut disemai (PC-DES-017) supaya baris kompile-time ini
        // konsisten dengan pemetaan yang migration terapkan ke baris runtime yang sudah ada.
        var seedTime = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);
        entity.HasData(new BilPettyCashBudget
        {
            Id = new Guid("b7c1f5a2-9d34-4e88-9a10-000000000001"),
            PoolCode = "HOSPITAL_MAIN",
            PoolName = "Kas Kecil Rumah Sakit",
            PeriodStart = DateOnly.FromDateTime(seedTime),
            PeriodEnd = null,
            BudgetAmount = 0m,
            CurrentBalance = 0m,
            TotalTopUpAmount = 0m,
            TotalDisbursedAmount = 0m,
            Status = PettyCashBudgetStatuses.Active,
            SupersededByBudgetId = null,
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
