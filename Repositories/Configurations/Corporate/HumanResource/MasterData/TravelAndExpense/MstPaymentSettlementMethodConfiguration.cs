using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.TravelAndExpense.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.TravelAndExpense
{
    public class MstPaymentSettlementMethodConfiguration : IEntityTypeConfiguration<MstPaymentSettlementMethod>
    {
        public void Configure(EntityTypeBuilder<MstPaymentSettlementMethod> entity)
        {
            entity.ToTable("MstPaymentSettlementMethod", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SettlementMethodCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SettlementMethodName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.SettlementType).HasMaxLength(50).HasDefaultValue("BankTransfer").IsRequired();
            entity.Property(x => x.IsForTravelAdvance).HasDefaultValue(true);
            entity.Property(x => x.IsForTravelSettlement).HasDefaultValue(true);
            entity.Property(x => x.IsForExpenseReimbursement).HasDefaultValue(true);
            entity.Property(x => x.IsForEmployeeRefund).HasDefaultValue(true);
            entity.Property(x => x.RequiresEmployeeBankAccount).HasDefaultValue(true);
            entity.Property(x => x.RequiresPayrollCycle).HasDefaultValue(false);
            entity.Property(x => x.RequiresFinanceVerification).HasDefaultValue(true);
            entity.Property(x => x.MaximumSettlementAmount).HasPrecision(18, 2);
            entity.Property(x => x.ProcessingDays).HasDefaultValue(0);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasIndex(x => x.SettlementMethodCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.SettlementMethodName);
            entity.HasIndex(x => new { x.SettlementType, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.IsForTravelAdvance, x.IsForTravelSettlement, x.IsForExpenseReimbursement });
            entity.HasIndex(x => new { x.IsDefault, x.SortOrder, x.IsActive, x.IsDelete });
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
