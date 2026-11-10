using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan episode rehab — <c>data-dictionary.md</c> bagian 2.3.</summary>
    /// <remarks>Satu episode per permintaan, dijaga unique index pada <c>RequestId</c>.</remarks>
    public class RhbEpisodeConfiguration : IEntityTypeConfiguration<RhbEpisode>
    {
        public void Configure(EntityTypeBuilder<RhbEpisode> builder)
        {
            builder.ToTable("RhbEpisode", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EpisodeNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.EpisodeStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ClosureReason).HasConversion<int?>();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.EpisodeNumber).IsUnique();
            builder.HasIndex(x => x.RequestId).IsUnique();
            builder.HasIndex(x => x.PatientId);
            builder.HasIndex(x => x.ResponsibleDoctorId);
            builder.HasIndex(x => x.EpisodeStatus);
            builder.HasIndex(x => x.OpenedAt);

            builder.HasOne(x => x.Request)
                .WithOne(x => x.Episode)
                .HasForeignKey<RhbEpisode>(x => x.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstPatient>()
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<MstDoctor>()
                .WithMany()
                .HasForeignKey(x => x.ResponsibleDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
