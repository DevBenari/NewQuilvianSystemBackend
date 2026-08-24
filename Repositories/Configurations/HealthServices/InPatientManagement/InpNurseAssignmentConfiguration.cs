using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpNurseAssignmentConfiguration : IEntityTypeConfiguration<InpNurseAssignment>
    {
        public void Configure(EntityTypeBuilder<InpNurseAssignment> builder)
        {
            builder.ToTable("InpNurseAssignment", "public");

            builder.HasKey(x => x.Id);


            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.EmployeeId);
            builder.HasIndex(x => x.StartDateTime);
            builder.HasIndex(x => x.EndDateTime);
            builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();

            builder.HasIndex(x => x.EpisodeId, "IX_InpNurseAssignment_EpisodeId_Active")
                .IsUnique()
                .HasFilter("\"EndDateTime\" IS NULL");

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.NurseAssignments)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Employee)
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
