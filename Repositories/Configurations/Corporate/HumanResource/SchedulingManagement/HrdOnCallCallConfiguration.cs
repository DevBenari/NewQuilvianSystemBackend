using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.SchedulingManagement
{
    public class HrdOnCallCallConfiguration : IEntityTypeConfiguration<HrdOnCallCall>
    {
        public void Configure(EntityTypeBuilder<HrdOnCallCall> entity)
        {
            entity.ToTable("HrdOnCallCall", "public", table =>
                table.HasCheckConstraint("CK_HrdOnCallCall_BackupSequence", "\"BackupSequence\" >= 0"));
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BackupSequence).HasDefaultValue(0).IsRequired();
            entity.Property(x => x.CallReason).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CallStatus).HasMaxLength(20).HasDefaultValue("Sent").IsRequired();
            entity.Property(x => x.CalledAt).HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(x => x.AnsweredAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeclineReason).HasMaxLength(500);
            entity.Property(x => x.ExpiredAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.ArrivedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CompletedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.RowVersion).IsConcurrencyToken();

            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.OnCallAssignment)
                .WithMany()
                .HasForeignKey(x => x.OnCallAssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CalledWorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.CalledWorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CallingOrganizationUnit)
                .WithMany()
                .HasForeignKey(x => x.CallingOrganizationUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.OnCallAssignmentId, x.CalledAt });
        }
    }
}
