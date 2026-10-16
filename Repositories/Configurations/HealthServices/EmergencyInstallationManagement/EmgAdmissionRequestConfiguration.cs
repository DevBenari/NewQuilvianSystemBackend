using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.EmergencyInstallationManagement
{
    public class EmgAdmissionRequestConfiguration : IEntityTypeConfiguration<EmgAdmissionRequest>
    {
        public void Configure(EntityTypeBuilder<EmgAdmissionRequest> builder)
        {
            builder.ToTable("EmgAdmissionRequest", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_EmgAdmissionRequest_State",
                    "(\"Status\" <> 3 OR (\"SignedAt\" IS NOT NULL AND \"SignedByUserId\" IS NOT NULL AND \"SignedByDoctorId\" IS NOT NULL AND \"SnapshotJson\" IS NOT NULL)) AND (\"Status\" <> 4 OR \"CancellationReason\" IS NOT NULL)");
                table.HasCheckConstraint(
                    "CK_EmgAdmissionRequest_Los",
                    "\"EstimatedLengthOfStayDays\" BETWEEN 1 AND 90");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(EmergencyAdmissionRequestStatus.Draft);
            builder.Property(x => x.EstimatedLengthOfStayDays)
                .IsRequired()
                .HasDefaultValue(3);
            builder.Property(x => x.PatientDependencyLevel)
                .HasConversion<int>()
                .IsRequired(false);
            builder.Property(x => x.ClinicalIndication).HasMaxLength(2000);
            builder.Property(x => x.CarePlanInstruction).HasMaxLength(2000);
            builder.Property(x => x.SignatureRequestedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.SignedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.SignerNameSnapshot).HasMaxLength(200);
            builder.Property(x => x.SignerPositionSnapshot).HasMaxLength(200);
            builder.Property(x => x.SignatureDeviceInfo).HasMaxLength(250);
            builder.Property(x => x.SignatureIpAddress).HasMaxLength(64);
            builder.Property(x => x.SnapshotJson).HasColumnType("jsonb");
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancellationReason).HasMaxLength(500);
            builder.Property(x => x.RowVersion).IsConcurrencyToken();

            builder.HasIndex(x => x.EmergencyVisitId, "UX_EmgAdmissionRequest_Visit_Open")
                .IsUnique()
                .HasFilter("\"Status\" IN (1, 2) AND NOT \"IsDelete\"");
            builder.HasIndex(x => x.DispositionId);
            builder.HasIndex(x => x.Status);

            builder.HasOne(x => x.EmergencyVisit)
                .WithMany()
                .HasForeignKey(x => x.EmergencyVisitId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Disposition)
                .WithMany()
                .HasForeignKey(x => x.DispositionId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestedServiceUnit)
                .WithMany()
                .HasForeignKey(x => x.RequestedServiceUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequestedPatientClass)
                .WithMany()
                .HasForeignKey(x => x.RequestedPatientClassId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ProposedInpatientDoctor)
                .WithMany()
                .HasForeignKey(x => x.ProposedInpatientDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.SignedByDoctor)
                .WithMany()
                .HasForeignKey(x => x.SignedByDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Events)
                .WithOne(x => x.AdmissionRequest)
                .HasForeignKey(x => x.AdmissionRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
