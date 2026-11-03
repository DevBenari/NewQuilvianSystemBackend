using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforcePlanning
{
    public class HrdHeadcountRequestCancellationConfiguration : IEntityTypeConfiguration<HrdHeadcountRequestCancellation>
    {
        public void Configure(EntityTypeBuilder<HrdHeadcountRequestCancellation> builder)
        {
            builder.ToTable("HrdHeadcountRequestCancellation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.CancellationNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");

            builder.HasOne(x => x.HeadcountRequest)
                .WithMany()
                .HasForeignKey(x => x.HeadcountRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedFormation)
                .WithMany()
                .HasForeignKey(x => x.CreatedFormationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CancellationNumber)
                .IsUnique()
                .HasDatabaseName("IX_HrdHeadcountRequestCancellation_Number");

            builder.HasIndex(x => x.HeadcountRequestId);
        }
    }
}
