using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.OperatingRoomManagement;

public class OprCaseConfiguration : IEntityTypeConfiguration<OprCase>
{
    public void Configure(EntityTypeBuilder<OprCase> builder)
    {
        builder.ToTable("OprCase", "public", table =>
        {
            // BE-RWI-174 (kamus data 19.2): status Rejected (8) wajib membawa ketiga jejak penolakan.
            table.HasCheckConstraint("CK_OprCase_Rejected",
                "\"Status\" <> 8 OR (\"RejectedAt\" IS NOT NULL AND \"RejectedByUserId\" IS NOT NULL AND \"RejectionReason\" IS NOT NULL)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CaseNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Indication).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Laterality).HasMaxLength(30);
        builder.Property(x => x.Version).IsConcurrencyToken();

        // BE-RWI-174 / migration E5 bagian kasus. Kasus lama terbaca General (1) tanpa rencana anestesi.
        builder.Property(x => x.SurgicalServiceType).IsRequired().HasDefaultValue(OprSurgicalServiceType.General);
        builder.Property(x => x.PlannedAnesthesiaType);
        builder.Property(x => x.RejectedAt).HasColumnType("timestamp with time zone");
        builder.Property(x => x.RejectedByUserId);
        builder.Property(x => x.RejectionReason).HasMaxLength(500);
        builder.HasIndex(x => x.CaseNumber).IsUnique();
        builder.HasIndex(x => new { x.PatientId, x.RequestedAt });
        builder.HasIndex(x => new { x.EncounterId, x.Status });
        builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Encounter).WithMany().HasForeignKey(x => x.EncounterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RequesterDoctor).WithMany().HasForeignKey(x => x.RequesterDoctorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PrimarySurgeon).WithMany().HasForeignKey(x => x.PrimarySurgeonId).OnDelete(DeleteBehavior.Restrict);
    }
}
