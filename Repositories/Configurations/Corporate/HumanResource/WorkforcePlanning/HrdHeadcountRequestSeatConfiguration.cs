using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdHeadcountRequestSeatConfiguration : IEntityTypeConfiguration<HrdHeadcountRequestSeat>
    {
        public void Configure(EntityTypeBuilder<HrdHeadcountRequestSeat> builder)
        {
            builder.ToTable("HrdHeadcountRequestSeat", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.SeatFte).HasPrecision(4, 2).HasDefaultValue(1.00m);
            builder.Property(x => x.LinkedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.FulfilledAt).HasColumnType("date");
            builder.Property(x => x.CancelledAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.HeadcountRequest)
                .WithMany(x => x.Seats)
                .HasForeignKey(x => x.HeadcountRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ReplacedWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.ReplacedWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LinkedPositionAssignment)
                .WithMany()
                .HasForeignKey(x => x.LinkedPositionAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.LinkedPersonnelActionOrder)
                .WithMany()
                .HasForeignKey(x => x.LinkedPersonnelActionOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Cancellation)
                .WithMany()
                .HasForeignKey(x => x.CancellationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.HeadcountRequestId, x.SeatNumber })
                .IsUnique()
                .HasDatabaseName("IX_HrdHeadcountRequestSeat_Request_Seat");

            builder.HasIndex(x => x.LinkedPositionAssignmentId)
                .IsUnique()
                .HasFilter("\"LinkedPositionAssignmentId\" IS NOT NULL AND \"SeatStatus\" <> 5 AND \"IsDelete\" = false")
                .HasDatabaseName("IX_HrdHeadcountRequestSeat_LinkedPositionAssignment");

            builder.HasIndex(x => x.LinkedPersonnelActionOrderId)
                .IsUnique()
                .HasFilter("\"LinkedPersonnelActionOrderId\" IS NOT NULL AND \"SeatStatus\" <> 5 AND \"IsDelete\" = false")
                .HasDatabaseName("IX_HrdHeadcountRequestSeat_LinkedPersonnelActionOrder");

            builder.HasIndex(x => x.SeatStatus);
            builder.HasIndex(x => x.CancellationId);
        }
    }
}
