using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AccountingManagement.AccountingEvent
{
    public class AccAccountingEventComponentConfiguration : IEntityTypeConfiguration<AccAccountingEventComponent>
    {
        public void Configure(EntityTypeBuilder<AccAccountingEventComponent> entity)
        {
            entity.ToTable("AccAccountingEventComponent", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AccountingEventId)
                .IsRequired();

            entity.Property(x => x.ComponentCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

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
                .WithMany(x => x.Components)
                .HasForeignKey(x => x.AccountingEventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.AccountingEventId, x.ComponentCode })
                .IsUnique();
        }
    }
}
