using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelAdvancePaymentConfiguration : IEntityTypeConfiguration<TrxTravelAdvancePayment>
    {
        public void Configure(EntityTypeBuilder<TrxTravelAdvancePayment> entity)
        {
            entity.ToTable("TrxTravelAdvancePayment", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PaymentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentDate).HasColumnType("date");
            entity.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.BankReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.FinanceReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).HasDefaultValue("Pending").IsRequired();
            entity.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.TravelAdvanceRequest).WithMany(x => x.Payments).HasForeignKey(x => x.TravelAdvanceRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentSettlementMethod).WithMany().HasForeignKey(x => x.PaymentSettlementMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ProcessedByUser).WithMany().HasForeignKey(x => x.ProcessedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PaymentNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.TravelAdvanceRequestId, x.PaymentStatus, x.IsDelete });
            entity.HasIndex(x => new { x.FinancePaymentId, x.GlHeaderId });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelAdvancePayment> entity)
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
