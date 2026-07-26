using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.PayrollAndBenefit
{
    public class MstSalaryGradeConfiguration : IEntityTypeConfiguration<MstSalaryGrade>
    {
        public void Configure(EntityTypeBuilder<MstSalaryGrade> entity)
        {
            entity.ToTable("MstSalaryGrade", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.SalaryGradeCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SalaryGradeName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.GradeLevel).HasDefaultValue(0);
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.MinimumSalary).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.MidpointSalary).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.MaximumSalary).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(x => x.AnnualIncrementPercentage).HasPrecision(9, 4).HasDefaultValue(0m);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.EmployeeGrade)
                .WithMany()
                .HasForeignKey(x => x.EmployeeGradeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SalaryGradeCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.SalaryGradeName);
            entity.HasIndex(x => x.EmployeeGradeId);
            entity.HasIndex(x => new { x.GradeLevel, x.CurrencyCode, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
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
