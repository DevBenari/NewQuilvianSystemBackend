using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Configurations;

public sealed class BilCashierShiftConfiguration : IEntityTypeConfiguration<BilCashierShift>
{
    public void Configure(EntityTypeBuilder<BilCashierShift> entity)
    {
        entity.ToTable("BilCashierShift", "public", table =>
        {
            table.HasCheckConstraint("CK_BilCashierShift_OpeningCash", "\"OpeningCash\" >= 0");
            table.HasCheckConstraint("CK_BilCashierShift_SystemCash", "\"SystemCash\" >= 0");
            table.HasCheckConstraint("CK_BilCashierShift_PhysicalCash", "\"PhysicalCash\" >= 0");
            table.HasCheckConstraint(
                "CK_BilCashierShift_Status",
                "\"Status\" IN ('OPEN','HANDED_OVER','CLOSED','CLOSED_WITH_VARIANCE','REVIEWED','REOPENED')");
        });
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ShiftNumber).HasMaxLength(40).IsRequired();
        entity.Property(x => x.OpeningCash).HasPrecision(18, 2);
        entity.Property(x => x.SystemCash).HasPrecision(18, 2);
        entity.Property(x => x.PhysicalCash).HasPrecision(18, 2);
        entity.Property(x => x.Variance).HasPrecision(18, 2);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.OpenedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        ConfigureIdentity(entity);

        entity.HasIndex(x => x.ShiftNumber).IsUnique();
        entity.HasIndex(x => new { x.CashierId, x.Status });
        entity.HasIndex(x => new { x.RegisterId, x.Status });
        entity.HasIndex(x => x.CashierId).IsUnique()
            .HasFilter("\"Status\" IN ('OPEN','REOPENED') AND \"IsDelete\" = FALSE");
        entity.HasIndex(x => x.RegisterId).IsUnique()
            .HasFilter("\"Status\" IN ('OPEN','REOPENED') AND \"IsDelete\" = FALSE");
        entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureIdentity(EntityTypeBuilder<BilCashierShift> entity)
    {
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
    }
}
