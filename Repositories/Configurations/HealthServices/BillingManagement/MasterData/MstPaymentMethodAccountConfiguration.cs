using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.BillingManagement.MasterData
{
    public sealed class MstPaymentMethodAccountConfiguration : IEntityTypeConfiguration<MstPaymentMethodAccount>
    {
        public void Configure(EntityTypeBuilder<MstPaymentMethodAccount> entity)
        {
            entity.ToTable("MstPaymentMethodAccount", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PaymentMethodId)
                .IsRequired();

            entity.Property(x => x.BankName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.AccountNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.AccountHolderName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Purpose)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone");

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.PaymentMethod)
                .WithMany(x => x.Accounts)
                .HasForeignKey(x => x.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PaymentMethodId);

            entity.HasIndex(x => x.AccountNumber);

            entity.HasIndex(x => new
            {
                x.PaymentMethodId,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
