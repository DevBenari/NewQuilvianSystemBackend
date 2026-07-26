using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxBusinessTravelParticipantConfiguration : IEntityTypeConfiguration<TrxBusinessTravelParticipant>
    {
        public void Configure(EntityTypeBuilder<TrxBusinessTravelParticipant> entity)
        {
            entity.ToTable("TrxBusinessTravelParticipant", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ParticipantRole).HasMaxLength(30).HasDefaultValue("Participant").IsRequired();
            entity.Property(x => x.EstimatedAllowanceAmount).HasPrecision(18, 2);
            entity.Property(x => x.ApprovedAllowanceAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.ParticipantStatus).HasMaxLength(30).HasDefaultValue("Proposed").IsRequired();
            entity.Property(x => x.ConfirmedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.Participants).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.OrganizationAssignment).WithMany().HasForeignKey(x => x.OrganizationAssignmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelClass).WithMany().HasForeignKey(x => x.TravelClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelAllowanceRate).WithMany().HasForeignKey(x => x.TravelAllowanceRateId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ConfirmedByUser).WithMany().HasForeignKey(x => x.ConfirmedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.WorkforceProfileId }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.WorkforceProfileId, x.ParticipantStatus, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxBusinessTravelParticipant> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
