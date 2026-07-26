using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstTravelExpenseCategoryConfiguration : IEntityTypeConfiguration<MstTravelExpenseCategory>
    {
        public void Configure(EntityTypeBuilder<MstTravelExpenseCategory> entity)
        {
            entity.ToTable("MstTravelExpenseCategory", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TravelExpenseCategoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TravelExpenseCategoryName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.ExpenseType).HasMaxLength(50).HasDefaultValue("Other").IsRequired();
            entity.Property(x => x.UnitType).HasMaxLength(50).HasDefaultValue("Actual").IsRequired();
            entity.Property(x => x.RequiresReceipt).HasDefaultValue(true);
            entity.Property(x => x.AllowWithoutReceipt).HasDefaultValue(false);
            entity.Property(x => x.IsAdvanceEligible).HasDefaultValue(true);
            entity.Property(x => x.IsReimbursable).HasDefaultValue(true);
            entity.Property(x => x.IsTaxable).HasDefaultValue(false);
            entity.Property(x => x.DefaultDailyLimit).HasPrecision(18, 2);
            entity.Property(x => x.DefaultTransactionLimit).HasPrecision(18, 2);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.ExpenseCategory)
                .WithMany(x => x.TravelExpenseCategories)
                .HasForeignKey(x => x.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TravelExpenseCategoryCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TravelExpenseCategoryName);
            entity.HasIndex(x => x.ExpenseCategoryId);
            entity.HasIndex(x => new { x.ExpenseType, x.UnitType, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsAdvanceEligible, x.IsReimbursable, x.IsActive, x.IsDelete });
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
