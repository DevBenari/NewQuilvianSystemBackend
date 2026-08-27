using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Configurations;

public sealed class BilCashierShiftHandoverConfiguration : IEntityTypeConfiguration<BilCashierShiftHandover>
{
    public void Configure(EntityTypeBuilder<BilCashierShiftHandover> entity)
    {
        entity.ToTable("BilCashierShiftHandover", "public", table =>
            table.HasCheckConstraint(
                "CK_BilCashierShiftHandover_Status",
                "\"Status\" IN ('PENDING','CONFIRMED')"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.InitiatedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ConfirmedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.RowVersion).IsConcurrencyToken();
        ConfigureIdentity(entity);

        entity.HasIndex(x => x.SourceShiftId).IsUnique()
            .HasFilter("\"Status\" = 'PENDING' AND \"IsDelete\" = FALSE");
        entity.HasIndex(x => x.ReceivingShiftId).IsUnique()
            .HasFilter("\"ReceivingShiftId\" IS NOT NULL");
        entity.HasOne(x => x.SourceShift).WithMany().HasForeignKey(x => x.SourceShiftId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne(x => x.ReceivingShift).WithMany().HasForeignKey(x => x.ReceivingShiftId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.OutgoingCashierId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.IncomingCashierId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureIdentity(EntityTypeBuilder<BilCashierShiftHandover> entity)
    {
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
    }
}
