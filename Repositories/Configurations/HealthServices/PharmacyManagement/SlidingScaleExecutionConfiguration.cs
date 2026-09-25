using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PharmacyManagement
{
    /// <summary>
    /// Bentuk tabel <c>PhmSlidingScaleExecution</c> — migration <c>K7</c>.
    /// </summary>
    public class PhmSlidingScaleExecutionConfiguration : IEntityTypeConfiguration<PhmSlidingScaleExecution>
    {
        public void Configure(EntityTypeBuilder<PhmSlidingScaleExecution> builder)
        {
            builder.ToTable("PhmSlidingScaleExecution", "public", table =>
            {
                table.HasCheckConstraint("CK_PhmSlidingScaleExecution_ComputedDose", "\"ComputedDoseUnits\" >= 0");
                table.HasCheckConstraint("CK_PhmSlidingScaleExecution_ExceptionReason", "\"IsException\" = false OR \"ExceptionReason\" IS NOT NULL");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExecutionNumber)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.OrderId)
                .IsRequired();

            builder.Property(x => x.OrderVersionId)
                .IsRequired();

            builder.Property(x => x.InpEpisodeId)
                .IsRequired();

            builder.Property(x => x.BloodGlucoseReadingId)
                .IsRequired();

            builder.Property(x => x.GlucoseValueSnapshot)
                .HasColumnType("numeric(7,2)")
                .IsRequired();

            builder.Property(x => x.GlucoseUnitSnapshot)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.MatchedRangeId)
                .IsRequired();

            builder.Property(x => x.ComputedDoseUnits)
                .HasColumnType("numeric(6,2)")
                .IsRequired();

            builder.Property(x => x.MedicationAdministrationId)
                .IsRequired();

            builder.Property(x => x.IsException)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.ExceptionReason)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(x => x.ReadingCorrectedAfterExecution)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(x => x.ExecutedByEmployeeId)
                .IsRequired();

            builder.Property(x => x.ExecutedByUserId)
                .IsRequired();

            builder.Property(x => x.ExecutedAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.ExecutionStatus)
                .HasConversion<int>()
                .HasDefaultValue(SlidingScaleExecutionStatus.Recorded)
                .IsRequired();

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(100)
                .IsRequired();

            NursingRecordAuditColumns.Configure(builder);

            builder.HasOne<PhmSlidingScaleOrder>()
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_OrderId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PhmSlidingScaleOrderVersion>()
                .WithMany()
                .HasForeignKey(x => x.OrderVersionId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_OrderVersionId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<InpEpisode>()
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_InpEpisodeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<CliBloodGlucoseReading>()
                .WithMany()
                .HasForeignKey(x => x.BloodGlucoseReadingId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_BloodGlucoseReadingId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PhmSlidingScaleRange>()
                .WithMany()
                .HasForeignKey(x => x.MatchedRangeId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_MatchedRangeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PhmMedicationAdministration>()
                .WithMany()
                .HasForeignKey(x => x.MedicationAdministrationId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_MedicationAdministrationId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.ExecutedByEmployeeId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_ExecutedByEmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ExecutedByUserId)
                .HasConstraintName("FK_PhmSlidingScaleExecution_ExecutedByUserId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ExecutionNumber, "UX_PhmSlidingScaleExecution_Number")
                .IsUnique();

            builder.HasIndex(x => new { x.OrderId, x.ExecutedAt }, "IX_PhmSlidingScaleExecution_Order_ExecutedAt");

            builder.HasIndex(x => x.BloodGlucoseReadingId, "UX_PhmSlidingScaleExecution_Reading_Recorded")
                .IsUnique()
                .HasFilter("\"ExecutionStatus\" = 1");

            builder.HasIndex(x => x.MedicationAdministrationId, "UX_PhmSlidingScaleExecution_MedicationAdministration")
                .IsUnique();

            builder.HasIndex(x => x.IdempotencyKey, "UX_PhmSlidingScaleExecution_IdempotencyKey")
                .IsUnique();

            builder.HasIndex(x => x.OrderVersionId, "IX_PhmSlidingScaleExecution_OrderVersionId");

            builder.HasIndex(x => x.InpEpisodeId, "IX_PhmSlidingScaleExecution_InpEpisodeId");

            builder.HasIndex(x => x.MatchedRangeId, "IX_PhmSlidingScaleExecution_MatchedRangeId");

            builder.HasIndex(x => x.ExecutedByEmployeeId, "IX_PhmSlidingScaleExecution_ExecutedByEmployeeId");

            builder.HasIndex(x => x.ExecutedByUserId, "IX_PhmSlidingScaleExecution_ExecutedByUserId");
        }
    }
}
