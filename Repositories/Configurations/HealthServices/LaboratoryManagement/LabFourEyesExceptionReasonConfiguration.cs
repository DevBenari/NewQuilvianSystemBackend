using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabFourEyesExceptionReasonConfiguration : IEntityTypeConfiguration<LabFourEyesExceptionReason>
    {
        public void Configure(EntityTypeBuilder<LabFourEyesExceptionReason> builder)
        {
            builder.ToTable("LabFourEyesExceptionReason", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ReasonCode).HasMaxLength(32).IsRequired();
            builder.Property(x => x.ReasonName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(256);

            builder.Property(x => x.RequiresNote).HasDefaultValue(false);
            builder.Property(x => x.SortOrder).HasDefaultValue(0);

            // Sama dengan LabResultCorrectionReasonConfiguration: DEFAULT true di skema, tetapi
            // nilai false yang dikirim aplikasi tidak boleh ditimpa default database.
            builder.Property(x => x.IsActive).HasDefaultValue(true).ValueGeneratedNever();

            // PARSIAL — pelajaran LAB-CONFLICT-005.
            builder.HasIndex(x => x.ReasonCode)
                .IsUnique()
                .HasDatabaseName("IX_LabFourEyesExceptionReason_ReasonCode")
                .HasFilter("\"IsDelete\" = false");

            builder.HasIndex(x => new { x.IsActive, x.SortOrder });
        }
    }
}
