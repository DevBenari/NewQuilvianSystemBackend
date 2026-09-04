using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.LaboratoryManagement
{
    public class LabValueBoundChangeRequestConfiguration : IEntityTypeConfiguration<LabValueBoundChangeRequest>
    {
        public void Configure(EntityTypeBuilder<LabValueBoundChangeRequest> builder)
        {
            builder.ToTable("LabValueBoundChangeRequest", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ValueBoundId).IsRequired();

            builder.Property(x => x.RequestStatus).HasConversion<int>().IsRequired();

            builder.Property(x => x.ProposedCriticalLow).HasPrecision(18, 4);
            builder.Property(x => x.ProposedCriticalHigh).HasPrecision(18, 4);
            builder.Property(x => x.ProposedCriticalOptionCodes).HasMaxLength(500);

            builder.Property(x => x.RequestReason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.DecisionNote).HasMaxLength(1000);

            builder.Property(x => x.RequestedByUserId).IsRequired();

            // Dua pemutus yang menyetujui pengajuan yang sama secara bersamaan tidak boleh
            // sama-sama berhasil; keduanya akan menulis batas kritis berbeda ke batas nilai
            // yang sama. Pola yang sama sudah dipakai LabOrder dan LabSpecimen (CAP-17).
            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.HasIndex(x => x.ValueBoundId);
            builder.HasIndex(x => x.RequestStatus);

            // Restrict: batas nilai yang masih punya jejak pengajuan tidak boleh hilang begitu
            // saja dari bawahnya.
            builder.HasOne(x => x.ValueBound)
                .WithMany()
                .HasForeignKey(x => x.ValueBoundId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
