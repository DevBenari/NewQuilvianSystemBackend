using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdPositionFormationConfiguration : IEntityTypeConfiguration<HrdPositionFormation>
    {
        public void Configure(EntityTypeBuilder<HrdPositionFormation> builder)
        {
            builder.ToTable("HrdPositionFormation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.FormationQuantity).HasPrecision(9, 2);
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.SourceDocumentNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Position)
                .WithMany()
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PreviousFormation)
                .WithMany()
                .HasForeignKey(x => x.PreviousFormationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.OrganizationUnitId, x.PositionId, x.VersionNumber })
                .IsUnique()
                .HasDatabaseName("IX_HrdPositionFormation_Unit_Position_Version");

            builder.HasIndex(x => new { x.OrganizationUnitId, x.PositionId, x.EffectiveStartDate })
                .IsUnique()
                .HasDatabaseName("IX_HrdPositionFormation_Unit_Position_Start");

            builder.HasIndex(x => x.HospitalSiteId);
            builder.HasIndex(x => x.SourceDocumentId);
        }
    }
}
