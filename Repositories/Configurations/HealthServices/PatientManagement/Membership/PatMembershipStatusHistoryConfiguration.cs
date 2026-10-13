using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices.PatientManagement.Membership
{
    /// <summary>Pemetaan riwayat status membership — <c>crm-membership/erd/data-dictionary.md</c>.</summary>
    public class PatMembershipStatusHistoryConfiguration : IEntityTypeConfiguration<PatMembershipStatusHistory>
    {
        public void Configure(EntityTypeBuilder<PatMembershipStatusHistory> builder)
        {
            builder.ToTable("PatMembershipStatusHistory", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FromStatus).HasConversion<int?>();
            builder.Property(x => x.ToStatus).HasConversion<int>().IsRequired();
            builder.Property(x => x.ActionCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Source).HasConversion<int>().IsRequired();
            builder.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.Reason).HasMaxLength(500);
            builder.Property(x => x.OldNotes).HasMaxLength(250);
            builder.Property(x => x.NewNotes).HasMaxLength(250);

            builder.HasIndex(x => new { x.PatientMembershipId, x.OccurredAt });

            builder.HasOne(x => x.PatientMembership)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.PatientMembershipId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
