using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PerformanceManagement
{
    public class HrdOppeStaffEvaluationConfiguration : IEntityTypeConfiguration<HrdOppeStaffEvaluation>
    {
        public void Configure(EntityTypeBuilder<HrdOppeStaffEvaluation> builder)
        {
            builder.ToTable("HrdOppeStaffEvaluation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.QuantitativeScore).HasColumnType("decimal(5,2)").HasDefaultValue(0.00m);
            builder.Property(x => x.QualitativeScore).HasColumnType("decimal(5,2)").HasDefaultValue(0.00m);
            builder.Property(x => x.FinalCompositeScore).HasColumnType("decimal(5,2)").HasDefaultValue(0.00m);
            builder.Property(x => x.PerformanceGrade).HasMaxLength(20).HasDefaultValue("C").IsRequired();
            builder.Property(x => x.IsAcknowledgedByStaff).HasDefaultValue(false);
            builder.Property(x => x.AcknowledgedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.Period)
                .WithMany()
                .HasForeignKey(x => x.PeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.PeriodId);
            builder.HasIndex(x => x.WorkforceProfileId);
        }
    }
}
