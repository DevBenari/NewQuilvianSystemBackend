using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan komplikasi intradialisis — <c>data-dictionary.md</c> bagian 3.13.</summary>
    public class HmdSessionComplicationConfiguration : IEntityTypeConfiguration<HmdSessionComplication>
    {
        public void Configure(EntityTypeBuilder<HmdSessionComplication> builder)
        {
            builder.ToTable("HmdSessionComplication", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.ComplicationType).HasConversion<int>().IsRequired();
            builder.Property(x => x.Outcome).HasConversion<int>().IsRequired();
            builder.Property(x => x.SessionImpact).HasConversion<int>().IsRequired();
            builder.Property(x => x.Severity).HasMaxLength(50);
            builder.Property(x => x.SignsAndSymptoms).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Intervention).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.ClinicianInstruction).HasMaxLength(1000);
            builder.Property(x => x.TransferDestination).HasMaxLength(200);

            builder.HasIndex(x => x.SessionId);
            builder.HasIndex(x => x.ComplicationType);
            builder.HasIndex(x => x.DetectedAt);
            builder.HasIndex(x => x.DetectedByUserId);
            builder.HasIndex(x => x.Outcome);

            builder.HasOne(x => x.Session)
                .WithMany(x => x.Complications)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
