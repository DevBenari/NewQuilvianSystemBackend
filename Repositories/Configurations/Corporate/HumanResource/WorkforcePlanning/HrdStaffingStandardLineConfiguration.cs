using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdStaffingStandardLineConfiguration : IEntityTypeConfiguration<HrdStaffingStandardLine>
    {
        public void Configure(EntityTypeBuilder<HrdStaffingStandardLine> builder)
        {
            builder.ToTable("HrdStaffingStandardLine", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.MinimumHeadcount).HasPrecision(9, 2);
            builder.Property(x => x.RatioStaffQuantity).HasPrecision(9, 2);
            builder.Property(x => x.RatioWorkloadQuantity).HasPrecision(9, 2);
            builder.Property(x => x.WorkloadUnit).HasMaxLength(50);
            builder.Property(x => x.StaffPerShift).HasPrecision(9, 2);
            builder.Property(x => x.ServiceHoursPerDay).HasPrecision(5, 2);
            builder.Property(x => x.ReliefFactor).HasPrecision(5, 2);
            builder.Property(x => x.CountsOutsourced).HasDefaultValue(false);
            builder.Property(x => x.Notes).HasMaxLength(500);

            builder.HasOne(x => x.StaffingStandard)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.StaffingStandardId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Profession)
                .WithMany()
                .HasForeignKey(x => x.ProfessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StaffingStandardId, x.PositionId })
                .IsUnique()
                .HasDatabaseName("IX_HrdStaffingStandardLine_Standard_Position");

            builder.HasIndex(x => x.PositionId);
        }
    }
}
