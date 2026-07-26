using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpEducationConfiguration : IEntityTypeConfiguration<WfpEducation>
    {
        public void Configure(EntityTypeBuilder<WfpEducation> builder)
        {
            builder.ToTable("WfpEducation", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RequirementCode).HasMaxLength(100);
            builder.Property(x => x.EducationLevel).HasMaxLength(100).IsRequired();
            builder.Property(x => x.InstitutionName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Major).HasMaxLength(200);
            builder.Property(x => x.StartDate).HasColumnType("date");
            builder.Property(x => x.EndDate).HasColumnType("date");
            builder.Property(x => x.CertificateNumber).HasMaxLength(150);
            builder.Property(x => x.FilePath).HasMaxLength(500);
            builder.Property(x => x.FileContentType).HasMaxLength(150);
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Country).WithMany().HasForeignKey(x => x.CountryId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.EducationLevel, x.InstitutionName });
            builder.HasIndex(x => new { x.WorkforceProfileId, x.CertificateNumber }).HasFilter("\"CertificateNumber\" IS NOT NULL AND \"IsDelete\" = false");
        }
    }
}
