using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdInitialFormationBatchRowConfiguration : IEntityTypeConfiguration<HrdInitialFormationBatchRow>
    {
        public void Configure(EntityTypeBuilder<HrdInitialFormationBatchRow> builder)
        {
            builder.ToTable("HrdInitialFormationBatchRow", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.FormationQuantity).HasPrecision(9, 2);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.IsValid).HasDefaultValue(false);
            builder.Property(x => x.ValidationMessage).HasMaxLength(500);

            builder.HasOne(x => x.Batch)
                .WithMany(x => x.Rows)
                .HasForeignKey(x => x.BatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedFormation)
                .WithMany()
                .HasForeignKey(x => x.CreatedFormationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.BatchId, x.OrganizationUnitId, x.PositionId })
                .IsUnique()
                .HasDatabaseName("IX_HrdInitialFormationBatchRow_Batch_Unit_Position");

            builder.HasIndex(x => new { x.OrganizationUnitId, x.PositionId });
        }
    }
}
