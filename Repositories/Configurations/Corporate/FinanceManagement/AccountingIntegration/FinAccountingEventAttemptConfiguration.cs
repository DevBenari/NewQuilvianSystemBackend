using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.AccountingIntegration;

public sealed class FinAccountingEventAttemptConfiguration : IEntityTypeConfiguration<FinAccountingEventAttempt>
{
    public void Configure(EntityTypeBuilder<FinAccountingEventAttempt> entity)
    {
        entity.ToTable("FinAccountingEventAttempt", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.AttemptedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.ResponseBody).HasMaxLength(2000);
        entity.Property(x => x.ErrorMessage).HasMaxLength(1000);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Outbox)
            .WithMany(x => x.Attempts)
            .HasForeignKey(x => x.OutboxId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => new { x.OutboxId, x.AttemptNumber })
            .IsUnique()
            .HasDatabaseName("IX_FinAccountingEventAttempt_Outbox_Number");
    }
}
