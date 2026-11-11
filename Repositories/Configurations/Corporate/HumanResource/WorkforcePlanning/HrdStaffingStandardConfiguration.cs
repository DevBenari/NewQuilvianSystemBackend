using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdStaffingStandardConfiguration : IEntityTypeConfiguration<HrdStaffingStandard>
    {
        public void Configure(EntityTypeBuilder<HrdStaffingStandard> builder)
        {
            builder.ToTable("HrdStaffingStandard", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.StandardNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.EffectiveStartDate).HasColumnType("date").IsRequired();
            builder.Property(x => x.EffectiveEndDate).HasColumnType("date");
            builder.Property(x => x.Justification).HasMaxLength(1000);
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ActivatedAt).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasOne(x => x.HospitalSite)
                .WithMany()
                .HasForeignKey(x => x.HospitalSiteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.OrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.OrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SupersedesStandard)
                .WithMany()
                .HasForeignKey(x => x.SupersedesStandardId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkflowDefinition)
                .WithMany()
                .HasForeignKey(x => x.WorkflowDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.StandardNumber)
                .IsUnique()
                .HasDatabaseName("IX_HrdStaffingStandard_StandardNumber");

            builder.HasIndex(x => new { x.OrganizationUnitId, x.VersionNumber })
                .IsUnique()
                .HasDatabaseName("IX_HrdStaffingStandard_Unit_Version");

            builder.HasIndex(x => new { x.OrganizationUnitId, x.EffectiveStartDate })
                .HasDatabaseName("IX_HrdStaffingStandard_Unit_StartDate");

            builder.HasIndex(x => x.HospitalSiteId);
            builder.HasIndex(x => x.StandardStatus);
            builder.HasIndex(x => x.WorkflowInstanceId);
        }
    }
}
