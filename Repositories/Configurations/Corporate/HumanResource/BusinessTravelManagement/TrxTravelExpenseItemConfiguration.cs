using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelExpenseItemConfiguration : IEntityTypeConfiguration<TrxTravelExpenseItem>
    {
        public void Configure(EntityTypeBuilder<TrxTravelExpenseItem> entity)
        {
            entity.ToTable("TrxTravelExpenseItem", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExpenseDate).HasColumnType("date");
            entity.Property(x => x.ExpenseDescription).HasMaxLength(500).IsRequired();
            entity.Property(x => x.MerchantName).HasMaxLength(200);
            entity.Property(x => x.ReceiptNumber).HasMaxLength(100);
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.UnitAmount).HasPrecision(18, 2);
            entity.Property(x => x.ClaimedAmount).HasPrecision(18, 2);
            entity.Property(x => x.EligibleAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAmount).HasPrecision(18, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.ExchangeRate).HasPrecision(18, 6);
            entity.Property(x => x.BaseCurrencyAmount).HasPrecision(18, 2);
            entity.Property(x => x.ReceiptFilePath).HasMaxLength(500);
            entity.Property(x => x.ReceiptFileName).HasMaxLength(255);
            entity.Property(x => x.ReceiptContentType).HasMaxLength(150);
            entity.Property(x => x.ReceiptChecksum).HasMaxLength(128);
            entity.Property(x => x.ItemStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.VerificationNotes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.TravelExpenseClaim).WithMany(x => x.Items).HasForeignKey(x => x.TravelExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelExpenseCategory).WithMany().HasForeignKey(x => x.TravelExpenseCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ExpenseCategory).WithMany().HasForeignKey(x => x.ExpenseCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CostCenter).WithMany().HasForeignKey(x => x.CostCenterId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelItinerary).WithMany(x => x.ExpenseItems).HasForeignKey(x => x.TravelItineraryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelTransportation).WithMany(x => x.ExpenseItems).HasForeignKey(x => x.TravelTransportationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelAccommodation).WithMany(x => x.ExpenseItems).HasForeignKey(x => x.TravelAccommodationId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.TravelExpenseClaimId, x.ExpenseDate, x.ItemStatus, x.IsDelete });
            entity.HasIndex(x => new { x.TravelExpenseCategoryId, x.CostCenterId, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelExpenseItem> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
