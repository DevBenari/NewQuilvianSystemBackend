using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabPatientResultApprovalConfiguration : IEntityTypeConfiguration<LabPatientResultApproval>
    {
        public void Configure(EntityTypeBuilder<LabPatientResultApproval> builder)
        {
            builder.ToTable("LabPatientResultApproval", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Decision).HasConversion<int>().IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(1000);
            builder.Property(x => x.DecidedByUserId).IsRequired();
            builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone").IsRequired();

            // Keputusan terbaru per versi.
            builder.HasIndex(x => new { x.VersionId, x.DecidedAt });

            builder.HasOne(x => x.ResultVersion)
                .WithMany(x => x.Approvals)
                .HasForeignKey(x => x.VersionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Penyetuju selalu orang (02-backend-architecture.md 13.7).
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.DecidedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
