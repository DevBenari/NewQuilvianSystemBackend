using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstPromotionReasonConfiguration : IEntityTypeConfiguration<MstPromotionReason>
    {
        public void Configure(EntityTypeBuilder<MstPromotionReason> entity)
        {
            entity.ToTable("MstPromotionReason", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.PromotionReasonCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PromotionReasonName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.RequiresPerformanceReview)
                .HasDefaultValue(true);

            entity.Property(x => x.RequiresSalaryReview)
                .HasDefaultValue(true);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasIndex(x => x.PromotionReasonCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.PromotionReasonName);

            entity.HasIndex(x => new
            {
                x.RequiresPerformanceReview,
                x.RequiresSalaryReview,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
