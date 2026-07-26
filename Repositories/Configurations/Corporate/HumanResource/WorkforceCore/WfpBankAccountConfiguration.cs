using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpBankAccountConfiguration : IEntityTypeConfiguration<WfpBankAccount>
    {
        public void Configure(EntityTypeBuilder<WfpBankAccount> builder)
        {
            builder.ToTable("WfpBankAccount", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.BankName).HasMaxLength(200);
            builder.Property(x => x.AccountNumber).HasMaxLength(100).IsRequired();
            builder.Property(x => x.AccountHolderName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.BankBranch).HasMaxLength(150);
            builder.Property(x => x.AccountType).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date");
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Bank).WithMany().HasForeignKey(x => x.BankId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.AccountNumber }).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.WorkforceProfileId).IsUnique().HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
        }
    }
}
