using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.CredentialingManagement
{
    public class TrxSpkRkkIssuanceConfiguration : IEntityTypeConfiguration<TrxSpkRkkIssuance>
    {
        public void Configure(EntityTypeBuilder<TrxSpkRkkIssuance> builder)
        {
            builder.ToTable("TrxSpkRkkIssuance", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.SpkNumber).HasMaxLength(80).IsRequired();
            builder.Property(x => x.RkkNumber).HasMaxLength(80).IsRequired();
            builder.Property(x => x.IssuedAt).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.ValidFrom).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.ValidUntil).HasColumnType("timestamp with time zone").IsRequired();
            builder.Property(x => x.ClinicalPrivilegeType).HasMaxLength(50).HasDefaultValue("Regular");
            builder.Property(x => x.DirectorGeneralName).HasMaxLength(150);
            builder.Property(x => x.IsSignedByDirectorGeneral).HasDefaultValue(false);
            builder.Property(x => x.SignedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerificationHash).HasMaxLength(64);
            builder.Property(x => x.QrCodeImagePath).HasMaxLength(500);
            builder.Property(x => x.SpkPdfFilePath).HasMaxLength(500);
            builder.Property(x => x.RkkPdfFilePath).HasMaxLength(500);
            builder.Property(x => x.IssuanceStatus).HasMaxLength(30).HasDefaultValue("Active");

            builder.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Committee)
                .WithMany(c => c.SpkRkkIssuances)
                .HasForeignKey(x => x.CommitteeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.WorkforceProfileId);
            builder.HasIndex(x => x.SpkNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.RkkNumber).IsUnique().HasFilter("\"IsDelete\" = false");
        }
    }
}
