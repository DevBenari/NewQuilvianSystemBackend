using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstDeductionTypeConfiguration : IEntityTypeConfiguration<MstDeductionType>
    {
        public void Configure(EntityTypeBuilder<MstDeductionType> entity)
        {
            entity.ToTable("MstDeductionType", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.DeductionTypeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DeductionTypeName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.DeductionCategory).HasMaxLength(50).HasDefaultValue("General").IsRequired();
            entity.Property(x => x.CalculationMethod).HasMaxLength(50).HasDefaultValue("Fixed").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.DefaultAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.DefaultPercentage).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.MaximumAmount).HasPrecision(18, 2);
            entity.Property(x => x.IsRecurring).HasDefaultValue(true);
            entity.Property(x => x.IsStatutory).HasDefaultValue(false);
            entity.Property(x => x.IsPreTax).HasDefaultValue(false);
            entity.Property(x => x.RequiresApproval).HasDefaultValue(false);
            entity.Property(x => x.AllowPartialDeduction).HasDefaultValue(true);
            entity.Property(x => x.Priority).HasDefaultValue(0);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.PayrollComponent)
                .WithMany(x => x.DeductionTypes)
                .HasForeignKey(x => x.PayrollComponentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.DeductionTypeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.DeductionTypeName);
            entity.HasIndex(x => x.PayrollComponentId);
            entity.HasIndex(x => new { x.DeductionCategory, x.CalculationMethod, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.Priority, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
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
