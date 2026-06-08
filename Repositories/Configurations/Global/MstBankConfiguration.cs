using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.Workforce
{
    public class WfpBankAccountConfiguration : IEntityTypeConfiguration<WfpBankAccount>
    {
        public void Configure(EntityTypeBuilder<WfpBankAccount> entity)
        {
            entity.ToTable("WfpBankAccount", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.BankId)
                .IsRequired();

            entity.Property(x => x.AccountNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.AccountHolderName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.BankBranch)
                .HasMaxLength(100);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.CreateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.UpdateBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelBy)
                .HasDefaultValue(Guid.Empty);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Bank)
                .WithMany()
                .HasForeignKey(x => x.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.BankId);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.BankId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.AccountNumber,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.IsPrimary,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}