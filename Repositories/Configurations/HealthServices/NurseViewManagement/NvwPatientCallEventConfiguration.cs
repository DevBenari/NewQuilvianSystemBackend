using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.NurseViewManagement
{
    public class NvwPatientCallEventConfiguration : IEntityTypeConfiguration<NvwPatientCallEvent>
    {
        public void Configure(EntityTypeBuilder<NvwPatientCallEvent> builder)
        {
            builder.ToTable("NvwPatientCallEvent", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventType).HasConversion<int>().IsRequired();
            builder.Property(x => x.ActorKind).HasConversion<int>().IsRequired();
            builder.Property(x => x.CloseReason).HasConversion<int?>();
            builder.Property(x => x.Note).HasMaxLength(250);
            builder.Property(x => x.OccurredAt).IsRequired();

            builder.HasOne(x => x.PatientCall)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.PatientCallId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.PatientCallId, x.OccurredAt });
            builder.HasIndex(x => x.IdempotencyKey)
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");
        }
    }
}
