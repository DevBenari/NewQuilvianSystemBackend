using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpDoctorAssignmentConfiguration : IEntityTypeConfiguration<InpDoctorAssignment>
    {
        public void Configure(EntityTypeBuilder<InpDoctorAssignment> builder)
        {
            builder.ToTable("InpDoctorAssignment", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.HandoverReason).HasMaxLength(500);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.DoctorId);
            builder.HasIndex(x => x.StartDateTime);
            builder.HasIndex(x => x.EndDateTime);
            builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();

            // INV-INP-03 — satu episode paling banyak satu DPJP aktif.
            builder.HasIndex(x => x.EpisodeId, "IX_InpDoctorAssignment_EpisodeId_Active")
                .IsUnique()
                .HasFilter("\"EndDateTime\" IS NULL");

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.DoctorAssignments)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Doctor)
                .WithMany()
                .HasForeignKey(x => x.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.AssignedByUser)
                .WithMany()
                .HasForeignKey(x => x.AssignedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
