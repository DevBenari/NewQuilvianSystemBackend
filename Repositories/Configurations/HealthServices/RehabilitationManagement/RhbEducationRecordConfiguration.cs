using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RehabilitationManagement
{
    /// <summary>Pemetaan edukasi dan persetujuan — <c>data-dictionary.md</c> bagian 2.11.</summary>
    public class RhbEducationRecordConfiguration : IEntityTypeConfiguration<RhbEducationRecord>
    {
        public void Configure(EntityTypeBuilder<RhbEducationRecord> builder)
        {
            builder.ToTable("RhbEducationRecord", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.MaterialText).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.RecipientName).HasMaxLength(150).IsRequired();
            builder.Property(x => x.RecipientRelation).HasMaxLength(50).IsRequired();
            builder.Property(x => x.MethodText).HasMaxLength(200).IsRequired();
            builder.Property(x => x.UnderstandingLevel).HasConversion<int>().IsRequired();
            builder.Property(x => x.BarrierText).HasMaxLength(500);
            builder.Property(x => x.ConsentStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ConsentNote).HasMaxLength(500);
            builder.Property(x => x.RecordStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.Version).HasDefaultValue(0).IsConcurrencyToken();

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => new { x.ProgramId, x.EducatedAt });
            builder.HasIndex(x => x.ConsentStatus);

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.EducationRecords)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Program)
                .WithMany()
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<RegPatientEncounter>()
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
