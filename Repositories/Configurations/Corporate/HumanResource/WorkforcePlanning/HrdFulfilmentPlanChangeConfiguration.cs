using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdFulfilmentPlanChangeConfiguration : IEntityTypeConfiguration<HrdFulfilmentPlanChange>
    {
        public void Configure(EntityTypeBuilder<HrdFulfilmentPlanChange> builder)
        {
            builder.ToTable("HrdFulfilmentPlanChange", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.ChangedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.HeadcountRequest)
                .WithMany()
                .HasForeignKey(x => x.HeadcountRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Seat)
                .WithMany()
                .HasForeignKey(x => x.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.HeadcountRequestId);
        }
    }
}
