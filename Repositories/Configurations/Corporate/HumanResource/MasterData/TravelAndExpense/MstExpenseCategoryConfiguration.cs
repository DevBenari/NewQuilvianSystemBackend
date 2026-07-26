using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstExpenseCategoryConfiguration : IEntityTypeConfiguration<MstExpenseCategory>
    {
        public void Configure(EntityTypeBuilder<MstExpenseCategory> entity)
        {
            entity.ToTable("MstExpenseCategory", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ExpenseCategoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ExpenseCategoryName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CategoryType).HasMaxLength(50).HasDefaultValue("General").IsRequired();
            entity.Property(x => x.IsTravelRelated).HasDefaultValue(false);
            entity.Property(x => x.IsMedicalBenefitRelated).HasDefaultValue(false);
            entity.Property(x => x.IsTrainingRelated).HasDefaultValue(false);
            entity.Property(x => x.RequiresReceipt).HasDefaultValue(true);
            entity.Property(x => x.AllowWithoutReceipt).HasDefaultValue(false);
            entity.Property(x => x.IsReimbursable).HasDefaultValue(true);
            entity.Property(x => x.IsTaxable).HasDefaultValue(false);
            entity.Property(x => x.RequireCostCenter).HasDefaultValue(true);
            entity.Property(x => x.AllowSplitAllocation).HasDefaultValue(false);
            entity.Property(x => x.DefaultMaximumAmount).HasPrecision(18, 2);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.ParentExpenseCategory)
                .WithMany(x => x.ChildExpenseCategories)
                .HasForeignKey(x => x.ParentExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ExpenseCategoryCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.ExpenseCategoryName);
            entity.HasIndex(x => x.ParentExpenseCategoryId);
            entity.HasIndex(x => new { x.CategoryType, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsTravelRelated, x.IsReimbursable, x.IsActive, x.IsDelete });
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
