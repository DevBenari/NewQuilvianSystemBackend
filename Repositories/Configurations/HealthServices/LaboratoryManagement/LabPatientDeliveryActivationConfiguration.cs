using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    // Data dictionary 25.5. Tabel peristiwa: keadaan kini = baris terbaru per disiplin.
    public class LabPatientDeliveryActivationConfiguration : IEntityTypeConfiguration<LabPatientDeliveryActivation>
    {
        public void Configure(EntityTypeBuilder<LabPatientDeliveryActivation> builder)
        {
            builder.ToTable("LabPatientDeliveryActivation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Discipline).HasConversion<int>().IsRequired();
            builder.Property(x => x.Enabled).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            builder.Property(x => x.SensitiveListConfirmed).IsRequired().HasDefaultValue(false);
            builder.Property(x => x.ChangedByUserId).IsRequired();
            builder.Property(x => x.ChangedAt).HasColumnType("timestamp with time zone").IsRequired();

            builder.HasIndex(x => new { x.Discipline, x.ChangedAt });

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
