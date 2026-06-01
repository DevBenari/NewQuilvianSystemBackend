using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPatientClassConfiguration : IEntityTypeConfiguration<MstPatientClass>
    {
        public void Configure(EntityTypeBuilder<MstPatientClass> entity)
        {
            entity.ToTable("MstPatientClass", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientClassCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PatientClassName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.PatientClassType)
                .HasConversion<int>()
                .HasDefaultValue(PatientClassType.Unknown)
                .IsRequired();

            entity.Property(x => x.ExternalClassCode)
                .HasMaxLength(50);

            entity.Property(x => x.ClassAlias)
                .HasMaxLength(100);

            entity.Property(x => x.ClassLevel)
                .HasDefaultValue(0);

            entity.Property(x => x.IsForOutpatient)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForInpatient)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForEmergency)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForIntensiveCare)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForNewborn)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForRoomCharge)
                .HasDefaultValue(false);

            entity.Property(x => x.IsForTariffMapping)
                .HasDefaultValue(true);

            entity.Property(x => x.IsDefault)
                .HasDefaultValue(false);

            entity.Property(x => x.DefaultDailyRoomRate)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.DefaultRegistrationFee)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.DefaultConsultationFee)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.PatientClassCode)
                .IsUnique();

            entity.HasIndex(x => x.PatientClassName);

            entity.HasIndex(x => x.ExternalClassCode);

            entity.HasIndex(x => x.PatientClassType);

            entity.HasIndex(x => new
            {
                x.ExternalClassCode,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.PatientClassType,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsForRoomCharge,
                x.IsForTariffMapping,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
