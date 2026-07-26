using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LifecycleManagement
{
    public class TrxTerminationConfiguration : IEntityTypeConfiguration<TrxTermination>
    {
        public void Configure(EntityTypeBuilder<TrxTermination> builder)
        {
            builder.ToTable("TrxTermination", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
            builder.Property(x => x.TerminationNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.IncidentDate).HasColumnType("date");
            builder.Property(x => x.TerminationDate).HasColumnType("date");
            builder.Property(x => x.SeveranceAmount).HasPrecision(18, 2);
            builder.Property(x => x.FinalPayAmount).HasPrecision(18, 2);
            builder.Property(x => x.TerminationReasonText).HasMaxLength(2500).IsRequired();
            builder.Property(x => x.InvestigationSummary).HasMaxLength(2500);
            builder.Property(x => x.TerminationStatus).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Notes).HasMaxLength(1500);
            builder.Property(x => x.IsActive).HasDefaultValue(true);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.EmployeeSeparation).WithMany().HasForeignKey(x => x.EmployeeSeparationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.TerminationReason).WithMany().HasForeignKey(x => x.TerminationReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RejectionReason).WithMany().HasForeignKey(x => x.RejectionReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ApprovedByUser).WithMany().HasForeignKey(x => x.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.TerminationNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.TerminationDate, x.TerminationStatus });
        }
    }
}
