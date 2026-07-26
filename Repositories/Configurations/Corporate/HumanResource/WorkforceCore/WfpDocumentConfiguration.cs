using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.WorkforceCore
{
    public class WfpDocumentConfiguration : IEntityTypeConfiguration<WfpDocument>
    {
        public void Configure(EntityTypeBuilder<WfpDocument> builder)
        {
            builder.ToTable("WfpDocument", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");

            builder.Property(x => x.RequirementCode).HasMaxLength(100);
            builder.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DocumentName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.DocumentNumber).HasMaxLength(150);
            builder.Property(x => x.IssueDate).HasColumnType("date");
            builder.Property(x => x.ExpiredDate).HasColumnType("date");
            builder.Property(x => x.IssuingAuthority).HasMaxLength(200);
            builder.Property(x => x.FilePath).HasMaxLength(500);
            builder.Property(x => x.FileContentType).HasMaxLength(150);
            builder.Property(x => x.OriginalFileName).HasMaxLength(255);
            builder.Property(x => x.StoredFileName).HasMaxLength(255);
            builder.Property(x => x.FileChecksum).HasMaxLength(128);
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerificationNote).HasMaxLength(500);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.HasOne(x => x.WorkforceProfile).WithMany().HasForeignKey(x => x.WorkforceProfileId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.WorkforceProfileId, x.DocumentType, x.DocumentNumber });
            builder.HasIndex(x => new { x.ExpiredDate, x.IsActive, x.IsDelete });
        }
    }
}
