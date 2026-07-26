using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.HrServiceManagement
{
    public class TrxHrServiceRequestCommentConfiguration : IEntityTypeConfiguration<TrxHrServiceRequestComment>
    {
        public void Configure(EntityTypeBuilder<TrxHrServiceRequestComment> entity)
        {
            entity.ToTable("TrxHrServiceRequestComment", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CommentedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsInternalNote).HasDefaultValue(false);
            entity.Property(x => x.IsEmployeeVisible).HasDefaultValue(true);
            entity.Property(x => x.IsSystemGenerated).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.HrServiceRequest).WithMany(x => x.Comments).HasForeignKey(x => x.HrServiceRequestId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CommentByUser).WithMany().HasForeignKey(x => x.CommentByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.CommentByWorkforceProfile).WithMany().HasForeignKey(x => x.CommentByWorkforceProfileId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.HrServiceRequestId, x.CommentedAt });
            entity.HasIndex(x => new { x.CommentByUserId, x.IsInternalNote });

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<TrxHrServiceRequestComment> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
