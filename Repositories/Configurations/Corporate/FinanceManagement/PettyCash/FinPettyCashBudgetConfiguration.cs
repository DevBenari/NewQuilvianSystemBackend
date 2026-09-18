using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.PettyCash;

public sealed class FinPettyCashBudgetConfiguration : IEntityTypeConfiguration<FinPettyCashBudget>
{
    public void Configure(EntityTypeBuilder<FinPettyCashBudget> entity)
    {
        entity.ToTable("FinPettyCashBudget", "public", table =>
        {
            table.HasCheckConstraint("CK_FinPettyCashBudget_CurrentBalance", "\"CurrentBalance\" >= 0");
            table.HasCheckConstraint("CK_FinPettyCashBudget_Status", "\"Status\" IN ('ACTIVE','INACTIVE','DRAFT','CLOSED')");
            table.HasCheckConstraint("CK_FinPettyCashBudget_BudgetAmount", "\"BudgetAmount\" >= 0");
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

        entity.HasIndex(x => new { x.PoolCode, x.PeriodStart })
            .HasDatabaseName("IX_FinPettyCashBudget_PoolCode_PeriodStart");

        entity.HasIndex(x => x.PoolCode)
            .IsUnique()
            .HasFilter("\"Status\" = 'ACTIVE' AND \"IsDelete\" = false")
            .HasDatabaseName("IX_FinPettyCashBudget_ActivePerPool");

        entity.HasOne<FinPettyCashBudget>()
            .WithMany()
            .HasForeignKey(x => x.SupersededByBudgetId)
            .OnDelete(DeleteBehavior.Restrict);

        var seedTime = new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc);
        entity.HasData(new FinPettyCashBudget
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

