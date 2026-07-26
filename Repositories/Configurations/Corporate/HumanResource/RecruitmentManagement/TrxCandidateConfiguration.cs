using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxCandidateConfiguration : IEntityTypeConfiguration<TrxCandidate>
    {
        public void Configure(EntityTypeBuilder<TrxCandidate> builder)
        {
            builder.ToTable("TrxCandidate", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.CandidateNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Gender).HasConversion<int?>();
            builder.Property(x => x.BirthDate).HasColumnType("date");
            builder.Property(x => x.BirthPlace).HasMaxLength(100);
            builder.Property(x => x.Nationality).HasMaxLength(100);
            builder.Property(x => x.IdentityNumber).HasMaxLength(100);
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.Property(x => x.PhoneNumber).HasMaxLength(50);
            builder.Property(x => x.WhatsAppNumber).HasMaxLength(50);
            builder.Property(x => x.Address).HasMaxLength(1000);
            builder.Property(x => x.LinkedInUrl).HasMaxLength(500);
            builder.Property(x => x.PortfolioUrl).HasMaxLength(500);
            builder.Property(x => x.CvFilePath).HasMaxLength(500);
            builder.Property(x => x.ConsentAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.BlacklistReason).HasMaxLength(1000);
            builder.Property(x => x.SourceChannel).HasMaxLength(30).IsRequired();
            builder.Property(x => x.AdditionalDataJson).HasColumnType("jsonb");
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Province).WithMany().HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.City).WithMany().HasForeignKey(x => x.CityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.PostalCode).WithMany().HasForeignKey(x => x.PostalCodeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RecruitmentSource).WithMany().HasForeignKey(x => x.RecruitmentSourceId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CandidateStatus).WithMany().HasForeignKey(x => x.CandidateStatusId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ReferredByWorkforceProfile).WithMany().HasForeignKey(x => x.ReferredByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.CandidateNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            builder.HasIndex(x => x.Email).HasFilter("\"Email\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => x.IdentityNumber).HasFilter("\"IdentityNumber\" IS NOT NULL AND \"IsDelete\" = false");
            builder.HasIndex(x => new { x.CandidateStatusId, x.IsBlacklisted, x.IsActive });
        }
    }
}
