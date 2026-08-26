using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.InPatientManagement
{
    public class InpFinancialClearanceConfiguration : IEntityTypeConfiguration<InpFinancialClearance>
    {
        public void Configure(EntityTypeBuilder<InpFinancialClearance> builder)
        {
            builder.ToTable("InpFinancialClearance", "public");

            builder.HasKey(x => x.Id);


            builder.Property(x => x.ClearanceStatus).HasConversion<int>();
            builder.Property(x => x.Note).IsRequired().HasMaxLength(500);

            builder.HasIndex(x => x.EpisodeId);
            builder.HasIndex(x => x.ClearanceStatus);
            builder.HasIndex(x => x.MarkedAt);
            builder.HasIndex(x => new { x.EpisodeId, x.SequenceNumber }).IsUnique();

            builder.HasOne(x => x.Episode)
                .WithMany(x => x.FinancialClearances)
                .HasForeignKey(x => x.EpisodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.MarkedByUser)
                .WithMany()
                .HasForeignKey(x => x.MarkedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
