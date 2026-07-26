using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelAttendanceLinkConfiguration : IEntityTypeConfiguration<TrxTravelAttendanceLink>
    {
        public void Configure(EntityTypeBuilder<TrxTravelAttendanceLink> entity)
        {
            entity.ToTable("TrxTravelAttendanceLink", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AttendanceDate).HasColumnType("date");
            entity.Property(x => x.AttendanceLinkStatus).HasMaxLength(30).HasDefaultValue("Planned").IsRequired();
            entity.Property(x => x.SyncedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.SyncMessage).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.AttendanceLinks).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTravelParticipant).WithMany(x => x.AttendanceLinks).HasForeignKey(x => x.BusinessTravelParticipantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTripAttendance).WithMany().HasForeignKey(x => x.BusinessTripAttendanceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.AttendanceDaily).WithMany().HasForeignKey(x => x.AttendanceDailyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SyncedByUser).WithMany().HasForeignKey(x => x.SyncedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.WorkforceProfileId, x.AttendanceDate }).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.AttendanceLinkStatus, x.IsAttendanceGenerated, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelAttendanceLink> entity)
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
