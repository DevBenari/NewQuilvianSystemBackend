using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class MstCredentialingCommitteeConfiguration : IEntityTypeConfiguration<MstCredentialingCommittee>
    {
        public void Configure(EntityTypeBuilder<MstCredentialingCommittee> builder)
        {
            builder.ToTable("MstCredentialingCommittee", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.CommitteeCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.CommitteeName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.SubCommitteeCredentialName).HasMaxLength(100).HasDefaultValue("Sub-Komite Kredensial");
            builder.Property(x => x.MinimumPeerReviewersCount).HasDefaultValue(2);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.ChairpersonWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.ChairpersonWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SecretaryWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.SecretaryWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CommitteeCode).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
