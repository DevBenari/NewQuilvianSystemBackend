using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BusinessTravelManagement
{
    public class TrxTravelDocumentConfiguration : IEntityTypeConfiguration<TrxTravelDocument>
    {
        public void Configure(EntityTypeBuilder<TrxTravelDocument> entity)
        {
            entity.ToTable("TrxTravelDocument", "public");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentType).HasMaxLength(50).HasDefaultValue("Other").IsRequired();
            entity.Property(x => x.DocumentName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(255);
            entity.Property(x => x.ContentType).HasMaxLength(150);
            entity.Property(x => x.FileChecksum).HasMaxLength(128);
            entity.Property(x => x.IssueDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.ExpiryDate).HasColumnType("date").IsRequired(false);
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DocumentStatus).HasMaxLength(30).HasDefaultValue("Uploaded").IsRequired();
            entity.Property(x => x.VerificationNotes).HasMaxLength(1000);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.BusinessTravelRequest).WithMany(x => x.Documents).HasForeignKey(x => x.BusinessTravelRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.BusinessTravelParticipant).WithMany(x => x.Documents).HasForeignKey(x => x.BusinessTravelParticipantId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelExpenseClaim).WithMany(x => x.Documents).HasForeignKey(x => x.TravelExpenseClaimId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.TravelExpenseItem).WithMany(x => x.Documents).HasForeignKey(x => x.TravelExpenseItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.BusinessTravelRequestId, x.DocumentType, x.DocumentStatus, x.IsDelete });
            entity.HasIndex(x => new { x.TravelExpenseClaimId, x.TravelExpenseItemId, x.IsDelete });
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxTravelDocument> entity)
        {
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
