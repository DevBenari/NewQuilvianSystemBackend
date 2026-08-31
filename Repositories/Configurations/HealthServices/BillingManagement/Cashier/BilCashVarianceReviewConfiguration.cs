using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Configurations;

public sealed class BilCashVarianceReviewConfiguration : IEntityTypeConfiguration<BilCashVarianceReview>
{
    public void Configure(EntityTypeBuilder<BilCashVarianceReview> entity)
    {
        entity.ToTable("BilCashVarianceReview", "public");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Variance).HasPrecision(18, 2);
        entity.Property(x => x.Resolution).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.ReviewedAt).HasColumnType("timestamp with time zone");
        ConfigureIdentity(entity);

        entity.HasIndex(x => new { x.ShiftId, x.ReviewedAt });
        entity.HasOne(x => x.Shift).WithMany(x => x.VarianceReviews)
            .HasForeignKey(x => x.ShiftId).OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
        entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.ReopenAuthorizedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureIdentity(EntityTypeBuilder<BilCashVarianceReview> entity)
    {
        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);
    }
}
