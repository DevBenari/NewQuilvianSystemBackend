using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxWorkplaceInvestigationConfiguration : IEntityTypeConfiguration<TrxWorkplaceInvestigation>
    {
        public void Configure(EntityTypeBuilder<TrxWorkplaceInvestigation> entity)
        {
            entity.ToTable("TrxWorkplaceInvestigation", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StartDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.EndDate).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ClosedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsConfidential).HasDefaultValue(true);
            entity.Property(x => x.RequiresEnhancedAudit).HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.IncidentReport)
                .WithMany(x => x.Investigations)
                .HasForeignKey(x => x.IncidentReportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeGrievance)
                .WithMany(x => x.Investigations)
                .HasForeignKey(x => x.EmployeeGrievanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.LeadInvestigatorUser)
                .WithMany()
                .HasForeignKey(x => x.LeadInvestigatorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ClosedByUser)
                .WithMany()
                .HasForeignKey(x => x.ClosedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.InvestigationNumber)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.InvestigationStatus, x.StartDate });

            entity.HasIndex(x => new { x.LeadInvestigatorUserId, x.InvestigationStatus });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxWorkplaceInvestigation> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
