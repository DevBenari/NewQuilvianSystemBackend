using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstTravelAllowanceRateConfiguration : IEntityTypeConfiguration<MstTravelAllowanceRate>
    {
        public void Configure(EntityTypeBuilder<MstTravelAllowanceRate> entity)
        {
            entity.ToTable("MstTravelAllowanceRate", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TravelPolicyId).IsRequired();
            entity.Property(x => x.TravelExpenseCategoryId).IsRequired();
            entity.Property(x => x.AllowanceRateCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AllowanceRateName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.RateType).HasMaxLength(50).HasDefaultValue("Fixed").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.RateAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.MinimumAmount).HasPrecision(18, 2);
            entity.Property(x => x.MaximumAmount).HasPrecision(18, 2);
            entity.Property(x => x.Percentage).HasPrecision(5, 2);
            entity.Property(x => x.RequiresReceipt).HasDefaultValue(false);
            entity.Property(x => x.IsTaxable).HasDefaultValue(false);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.TravelPolicy)
                .WithMany(x => x.AllowanceRates)
                .HasForeignKey(x => x.TravelPolicyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TravelExpenseCategory)
                .WithMany(x => x.AllowanceRates)
                .HasForeignKey(x => x.TravelExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TravelClass)
                .WithMany(x => x.AllowanceRates)
                .HasForeignKey(x => x.TravelClassId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DestinationZone)
                .WithMany(x => x.AllowanceRates)
                .HasForeignKey(x => x.DestinationZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeGrade)
                .WithMany()
                .HasForeignKey(x => x.EmployeeGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AllowanceRateCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.AllowanceRateName);
            entity.HasIndex(x => x.TravelPolicyId);
            entity.HasIndex(x => x.TravelExpenseCategoryId);
            entity.HasIndex(x => new { x.TravelClassId, x.DestinationZoneId, x.EmployeeGradeId });
            entity.HasIndex(x => new { x.RateType, x.CurrencyCode, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.Priority, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields<T>(EntityTypeBuilder<T> entity)
            where T : QuilvianSystemBackend.Models.IdentityModel
        {
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
        }
    }
}
