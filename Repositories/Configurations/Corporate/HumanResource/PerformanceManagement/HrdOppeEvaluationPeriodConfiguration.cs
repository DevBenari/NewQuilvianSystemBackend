using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdOppeEvaluationPeriodConfiguration : IEntityTypeConfiguration<HrdOppeEvaluationPeriod>
    {
        public void Configure(EntityTypeBuilder<HrdOppeEvaluationPeriod> builder)
        {
            builder.ToTable("HrdOppeEvaluationPeriod", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.PeriodCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.PeriodName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.Semester).HasDefaultValue(1);
            builder.Property(x => x.StartDate).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.EndDate).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("OPEN").IsRequired();
            builder.Property(x => x.QuantitativeWeightPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(60.00m);
            builder.Property(x => x.QualitativeWeightPercentage).HasColumnType("decimal(5,2)").HasDefaultValue(40.00m);

            builder.HasIndex(x => x.PeriodCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
