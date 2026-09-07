using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.MasterData
{
    /// <summary>
    /// Bentuk tabel kebijakan batas waktu pengkajian — <c>BE-RWI-055</c>.
    /// </summary>
    /// <remarks>
    /// Index pada <c>(AssessmentType, ServiceUnitType, EffectiveFrom)</c> adalah jalan yang
    /// dipakai setiap pembuatan pengkajian untuk menemukan kebijakan yang berlaku. Tanpa index
    /// itu, setiap pengkajian baru memindai seluruh tabel kebijakan.
    /// </remarks>
    public class MstClinicalAssessmentPolicyConfiguration
        : IEntityTypeConfiguration<MstClinicalAssessmentPolicy>
    {
        public void Configure(EntityTypeBuilder<MstClinicalAssessmentPolicy> builder)
        {
            builder.ToTable("MstClinicalAssessmentPolicy", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.PolicyCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.PolicyName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.AssessmentType)
                .HasConversion<int>()
                .HasDefaultValue(PatientAssessmentType.Initial)
                .IsRequired();

            builder.Property(x => x.ServiceUnitType)
                .HasConversion<int?>()
                .IsRequired(false);

            builder.Property(x => x.DueWithinMinutes)
                .IsRequired();

            builder.Property(x => x.EffectiveFrom)
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.EffectiveTo)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            builder.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            builder.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            builder.HasIndex(x => x.PolicyCode)
                .IsUnique();

            builder.HasIndex(x => new
            {
                x.AssessmentType,
                x.ServiceUnitType,
                x.EffectiveFrom
            });

            builder.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
