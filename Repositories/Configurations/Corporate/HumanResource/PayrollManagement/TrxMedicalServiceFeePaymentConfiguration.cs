using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxMedicalServiceFeePaymentConfiguration : IEntityTypeConfiguration<TrxMedicalServiceFeePayment>
    {
        public void Configure(EntityTypeBuilder<TrxMedicalServiceFeePayment> entity)
        {

            entity.ToTable("TrxMedicalServiceFeePayment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.PaymentNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.PaymentAmount).HasPrecision(18, 2);
            entity.Property(x => x.PaymentReferenceNumber).HasMaxLength(100);
            entity.Property(x => x.ScheduledPaymentAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PaidAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.MedicalServiceFeeCalculation).WithMany(x => x.Payments).HasForeignKey(x => x.MedicalServiceFeeCalculationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollRunEmployee).WithMany().HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPayment).WithMany(x => x.MedicalServiceFeePayments).HasForeignKey(x => x.PayrollPaymentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaymentSettlementMethod).WithMany().HasForeignKey(x => x.PaymentSettlementMethodId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PaidByUser).WithMany().HasForeignKey(x => x.PaidByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PaymentNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.MedicalServiceFeeCalculationId, x.PaymentStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPaymentId, x.FinancePaymentId, x.GlHeaderId });
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
