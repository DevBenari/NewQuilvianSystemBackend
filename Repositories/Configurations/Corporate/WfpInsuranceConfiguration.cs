using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpInsuranceConfiguration : IEntityTypeConfiguration<WfpInsurance>
    {
        public void Configure(EntityTypeBuilder<WfpInsurance> entity)
        {
            entity.ToTable("WfpInsurance", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.IsBpjsKesehatanEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.BpjsKesehatanNumber)
                .HasMaxLength(50);

            entity.Property(x => x.IsBpjsKetenagakerjaanEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.BpjsKetenagakerjaanNumber)
                .HasMaxLength(50);

            entity.Property(x => x.IsPrivateInsuranceEnabled)
                .HasDefaultValue(false);

            entity.Property(x => x.PrivateInsuranceProvider)
                .HasMaxLength(100);

            entity.Property(x => x.PrivateInsuranceNumber)
                .HasMaxLength(100);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

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
                .WithOne(x => x.Insurance)
                .HasForeignKey<WfpInsurance>(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.BpjsKesehatanNumber);

            entity.HasIndex(x => x.BpjsKetenagakerjaanNumber);

            entity.HasIndex(x => new
            {
                x.IsBpjsKesehatanEnabled,
                x.IsBpjsKetenagakerjaanEnabled,
                x.IsPrivateInsuranceEnabled,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}