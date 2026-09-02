using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.RadiologyManagement
{
    public class RadOrderConfiguration : IEntityTypeConfiguration<RadOrder>
    {
        public void Configure(EntityTypeBuilder<RadOrder> builder)
        {
            builder.ToTable("RadOrder", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrderStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.StatusBeforeHold).HasConversion<int>();
            builder.Property(x => x.ClinicalIndication).HasMaxLength(1000);
            builder.Property(x => x.ClosureReason).HasMaxLength(1000);
            builder.Property(x => x.Version).IsConcurrencyToken();

            builder.HasIndex(x => x.EncounterId);
            builder.HasIndex(x => x.OrderStatus);
            builder.HasIndex(x => new { x.ModalityId, x.OrderStatus });

            builder.HasOne(x => x.Encounter)
                .WithMany()
                .HasForeignKey(x => x.EncounterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                .WithMany()
                .HasForeignKey(x => x.ProcedureId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Modality)
                .WithMany()
                .HasForeignKey(x => x.ModalityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Studies)
                .WithOne(x => x.RadOrder)
                .HasForeignKey(x => x.RadOrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
