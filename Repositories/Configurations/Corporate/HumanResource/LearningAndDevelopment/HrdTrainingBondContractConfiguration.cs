using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class HrdTrainingBondContractConfiguration : IEntityTypeConfiguration<HrdTrainingBondContract>
    {
        public void Configure(EntityTypeBuilder<HrdTrainingBondContract> builder)
        {
            builder.ToTable("hrd_training_bond_contracts", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.ContractNumber).HasMaxLength(60).IsRequired();
            builder.Property(x => x.TrainingProgramName).HasMaxLength(250).IsRequired();
            builder.Property(x => x.TotalTrainingCost).HasColumnType("decimal(15,2)").HasDefaultValue(0m);
            builder.Property(x => x.HospitalFinancedAmount).HasColumnType("decimal(15,2)").HasDefaultValue(0m);
            builder.Property(x => x.RetentionPeriodMonths).HasDefaultValue(12);
            builder.Property(x => x.BondStartDate).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.BondEndDate).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.BondStatus).HasMaxLength(40).HasDefaultValue("ACTIVE").IsRequired();
            builder.Property(x => x.IsBreached).HasDefaultValue(false);
            builder.Property(x => x.BreachRecordedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CalculatedPenaltyAmount).HasColumnType("decimal(15,2)");

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.ContractNumber).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
