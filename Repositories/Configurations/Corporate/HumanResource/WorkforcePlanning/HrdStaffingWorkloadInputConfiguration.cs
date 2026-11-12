using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdStaffingWorkloadInputConfiguration : IEntityTypeConfiguration<HrdStaffingWorkloadInput>
    {
        public void Configure(EntityTypeBuilder<HrdStaffingWorkloadInput> builder)
        {
            builder.ToTable("HrdStaffingWorkloadInput", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.PeriodStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.PeriodEndDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.WorkloadValue).HasPrecision(12, 2);
            builder.Property(x => x.SourceNote).HasMaxLength(500).IsRequired();

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.OrganizationUnitId, x.PositionId, x.PeriodStartDate })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false")
                .HasDatabaseName("IX_HrdStaffingWorkloadInput_Unit_Position_Period");
        }
    }
}
