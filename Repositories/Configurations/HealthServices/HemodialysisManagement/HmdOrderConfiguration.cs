using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.HemodialysisManagement
{
    /// <summary>Pemetaan permintaan HD masuk — <c>data-dictionary.md</c> bagian 3.1.</summary>
    /// <remarks>
    /// Index unik <c>OrderNumber</c> adalah penjaga terakhir keunikan nomor bisnis
    /// (<c>QBE-CODE-004</c>). Seluruh FK <c>Restrict</c>: permintaan adalah rekam klinis.
    /// </remarks>
    public class HmdOrderConfiguration : IEntityTypeConfiguration<HmdOrder>
    {
        public void Configure(EntityTypeBuilder<HmdOrder> builder)
        {
            builder.ToTable("HmdOrder", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderNumber).HasMaxLength(30).IsRequired();
            builder.Property(x => x.ClinicalReason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.DecisionReason).HasMaxLength(1000);
            builder.Property(x => x.Priority).HasConversion<int>().IsRequired();
            builder.Property(x => x.OrderStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int?>();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.OrderNumber).IsUnique();
            builder.HasIndex(x => new { x.PatientId, x.OrderStatus });
            builder.HasIndex(x => new { x.RequestedDate, x.Priority });
            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.InpEpisodeId);
            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.RequestedByUserId);
            builder.HasIndex(x => x.RequestingDoctorId);
            builder.HasIndex(x => x.RequestedAt);
            builder.HasIndex(x => x.Priority);
            builder.HasIndex(x => x.OrderStatus);

            builder.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InpEpisode)
                .WithMany()
                .HasForeignKey(x => x.InpEpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Episode)
                .WithMany()
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestingDoctor)
                .WithMany()
                .HasForeignKey(x => x.RequestingDoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
