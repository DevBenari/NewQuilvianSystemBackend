using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan penugasan petugas sesi — <c>data-dictionary.md</c> bagian 3.14.</summary>
    /// <remarks>
    /// Unique <c>(SessionId, WorkforceProfileId)</c> berlaku untuk baris yang belum ditandai terhapus,
    /// supaya petugas yang pernah dilepas dari sesi dapat ditugaskan kembali tanpa kehilangan jejak
    /// penugasan lamanya.
    /// </remarks>
    public class HmdSessionStaffAssignmentConfiguration : IEntityTypeConfiguration<HmdSessionStaffAssignment>
    {
        public void Configure(EntityTypeBuilder<HmdSessionStaffAssignment> builder)
        {
            builder.ToTable("HmdSessionStaffAssignment", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StaffRole).HasConversion<int>().IsRequired();
            builder.Property(x => x.CompetencyVerificationStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.CompetencySourceReference).HasMaxLength(200);

            builder.HasIndex(x => new { x.SessionId, x.WorkforceProfileId })
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.StaffRole);
            builder.HasIndex(x => x.CompetencyVerificationStatus);

            builder.HasOne(x => x.Session)
                .WithMany(x => x.StaffAssignments)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
