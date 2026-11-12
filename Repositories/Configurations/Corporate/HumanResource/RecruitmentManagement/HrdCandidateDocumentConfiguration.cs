using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.RecruitmentManagement
{
    public class HrdCandidateDocumentConfiguration : IEntityTypeConfiguration<HrdCandidateDocument>
    {
        public void Configure(EntityTypeBuilder<HrdCandidateDocument> builder)
        {
            builder.ToTable("HrdCandidateDocument", "public");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DocumentTitle)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(x => x.CandidateId);
            builder.HasIndex(x => x.ApplicationId);
            builder.HasIndex(x => x.DocumentType);

            builder.HasOne(x => x.Candidate)
                .WithMany(c => c.Documents)
                .HasForeignKey(x => x.CandidateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Application)
                .WithMany()
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
