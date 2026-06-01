using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpTaxConfiguration : IEntityTypeConfiguration<WfpTax>
    {
        public void Configure(EntityTypeBuilder<WfpTax> entity)
        {
            entity.ToTable("WfpTax", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.NpwpNumber)
                .HasMaxLength(30);

            entity.Property(x => x.TaxStatus)
                .HasMaxLength(50)
                .HasDefaultValue("TK0");

            entity.Property(x => x.IsTaxed)
                .HasDefaultValue(true);

            entity.Property(x => x.TaxCalculationMethod)
                .HasMaxLength(50)
                .HasDefaultValue("Gross");

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

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

            entity.HasOne(x => x.WorkforceProfile)
                .WithOne(x => x.Tax)
                .HasForeignKey<WfpTax>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.NpwpNumber);

            entity.HasIndex(x => new
            {
                x.TaxStatus,
                x.IsTaxed,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
