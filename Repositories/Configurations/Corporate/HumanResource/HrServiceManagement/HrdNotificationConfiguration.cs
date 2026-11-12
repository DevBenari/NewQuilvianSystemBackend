using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class HrdNotificationConfiguration : IEntityTypeConfiguration<HrdNotification>
    {
        public void Configure(EntityTypeBuilder<HrdNotification> entity)
        {
            entity.ToTable("HrdNotification", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Body).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(60).IsRequired();
            entity.Property(x => x.Priority).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Channel).HasMaxLength(30).IsRequired();
            entity.Property(x => x.RequestedChannel).HasMaxLength(30);
            entity.Property(x => x.SourceModule).HasMaxLength(80);
            entity.Property(x => x.ReferenceType).HasMaxLength(100);
            entity.Property(x => x.DeepLinkUrl).HasMaxLength(300);
            entity.Property(x => x.DedupKey).HasMaxLength(250);

            entity.Property(x => x.ReadAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeliveredAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            entity.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.DeliveredAt })
                .HasDatabaseName("IX_HrdNotification_Recipient_IsRead_DeliveredAt");
            entity.HasIndex(x => x.DedupKey)
                .IsUnique()
                .HasFilter("\"DedupKey\" IS NOT NULL")
                .HasDatabaseName("uq_hrd_notification_dedup_key");
            entity.HasIndex(x => new { x.ReferenceType, x.ReferenceId })
                .HasDatabaseName("IX_HrdNotification_Reference");
        }
    }
}
