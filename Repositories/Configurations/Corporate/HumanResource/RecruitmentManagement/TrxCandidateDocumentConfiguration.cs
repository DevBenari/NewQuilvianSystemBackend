using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class TrxCandidateDocumentConfiguration : IEntityTypeConfiguration<TrxCandidateDocument>
    {
        public void Configure(EntityTypeBuilder<TrxCandidateDocument> builder)
        {
            builder.ToTable("TrxCandidateDocument", "public");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);

            builder.Property(x => x.DocumentTypeCode).HasMaxLength(50).IsRequired();
            builder.Property(x => x.DocumentName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.DocumentNumber).HasMaxLength(100);
            builder.Property(x => x.IssueDate).HasColumnType("date");
            builder.Property(x => x.ExpiryDate).HasColumnType("date");
            builder.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            builder.Property(x => x.OriginalFileName).HasMaxLength(255);
            builder.Property(x => x.MimeType).HasMaxLength(150);
            builder.Property(x => x.FileChecksum).HasMaxLength(128);
            builder.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone");
            builder.Property(x => x.VerificationNotes).HasMaxLength(1000);
            builder.Property(x => x.IsActive).HasDefaultValue(true);

            builder.HasOne(x => x.Candidate).WithMany().HasForeignKey(x => x.CandidateId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.CandidateApplication).WithMany().HasForeignKey(x => x.CandidateApplicationId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.CandidateId, x.DocumentTypeCode, x.IsActive });
            builder.HasIndex(x => x.FileChecksum).HasFilter("\"FileChecksum\" IS NOT NULL AND \"IsDelete\" = false");
        }
    }
}
