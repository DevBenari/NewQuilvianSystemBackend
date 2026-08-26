using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprCaseConfiguration : IEntityTypeConfiguration<OprCase>
{
    public void Configure(EntityTypeBuilder<OprCase> builder)
    {
        builder.ToTable("OprCase", "public");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CaseNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Indication).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Laterality).HasMaxLength(30);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.HasIndex(x => x.CaseNumber).IsUnique();
        builder.HasIndex(x => new { x.PatientId, x.RequestedAt });
        builder.HasIndex(x => new { x.EncounterId, x.Status });
        builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Encounter).WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RequesterDoctor).WithMany().HasForeignKey(x => x.RequesterDoctorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PrimarySurgeon).WithMany().HasForeignKey(x => x.PrimarySurgeonId).OnDelete(DeleteBehavior.Restrict);
    }
}
