using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class HrdRestitutionSettingConfiguration : IEntityTypeConfiguration<HrdRestitutionSetting>
    {
        public void Configure(EntityTypeBuilder<HrdRestitutionSetting> builder)
        {
            builder.ToTable("HrdRestitutionSetting", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.GradeLevelCode)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.BenefitCategory)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.AnnualLimitAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.MaxDeductionPercentageOfGross)
                .HasPrecision(5, 2);

            builder.HasIndex(x => new { x.GradeLevelCode, x.BenefitCategory })
                .IsUnique()
                .HasDatabaseName("uq_restitution_setting_grade_category");
        }
    }
}
