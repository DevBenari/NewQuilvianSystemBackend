using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.AccountingEvent
{
    public class AccAccountingEventAttemptConfiguration : IEntityTypeConfiguration<AccAccountingEventAttempt>
    {
        public void Configure(EntityTypeBuilder<AccAccountingEventAttempt> entity)
        {
            entity.ToTable("AccAccountingEventAttempt", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AccountingEventId)
                .IsRequired();

            entity.Property(x => x.AttemptNumber)
                .IsRequired();

            entity.Property(x => x.AttemptedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            entity.Property(x => x.IsSuccess)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(x => x.FailureMessage)
                .HasMaxLength(1000);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.AccountingEvent)
                .WithMany(x => x.Attempts)
                .HasForeignKey(x => x.AccountingEventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.AccountingEventId, x.AttemptNumber })
                .IsUnique();
        }
    }
}
