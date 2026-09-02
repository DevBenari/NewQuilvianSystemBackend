using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadTransitionHistoryConfiguration : IEntityTypeConfiguration<RadTransitionHistory>
    {
        public void Configure(EntityTypeBuilder<RadTransitionHistory> builder)
        {
            builder.ToTable("RadTransitionHistory", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Scope).HasConversion<int>().IsRequired();
            builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
            builder.Property(x => x.FromStatus).HasMaxLength(50);
            builder.Property(x => x.ToStatus).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ReasonCode).HasMaxLength(50);
            builder.Property(x => x.ReasonNote).HasMaxLength(1000);

            builder.HasIndex(x => new { x.RadOrderId, x.OccurredAt });
            builder.HasIndex(x => new { x.RadStudyId, x.OccurredAt });
            builder.HasIndex(x => x.EncounterId);

            builder.HasOne(x => x.RadOrder)
                .WithMany()
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RadStudy)
                .WithMany()
                .HasForeignKey(x => x.RadStudyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
