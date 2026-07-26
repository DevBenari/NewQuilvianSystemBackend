using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class TrxEmployeeProfileChangeRequestConfiguration : IEntityTypeConfiguration<TrxEmployeeProfileChangeRequest>
    {
        public void Configure(EntityTypeBuilder<TrxEmployeeProfileChangeRequest> builder)
        {
            builder.ToTable("TrxEmployeeProfileChangeRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RequestNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequestCategory).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequestStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.RequestReasonText).HasMaxLength(500);
            builder.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.ApprovedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.AppliedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.WorkflowDefinition).WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestReason).WithMany().HasForeignKey(x => x.RequestReasonId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => x.RequestNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => new { x.WorkforceProfileId, x.RequestStatus, x.CreateDateTime });
        }
    }
}
