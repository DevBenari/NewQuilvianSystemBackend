using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class WfpDisciplinaryActionConfiguration : IEntityTypeConfiguration<WfpDisciplinaryAction>
    {
        public void Configure(EntityTypeBuilder<WfpDisciplinaryAction> entity)
        {
            entity.ToTable("WfpDisciplinaryAction", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.WorkforceProfileId)
                .IsRequired();

            entity.Property(x => x.ActionType)
                .HasConversion<int>()
                .HasDefaultValue(DisciplinaryActionType.Unknown)
                .IsRequired();

            entity.Property(x => x.IncidentDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.IssuedDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(x => x.SeverityLevel)
                .HasConversion<int>()
                .HasDefaultValue(DisciplinarySeverityLevel.Low)
                .IsRequired();

            entity.Property(x => x.Reason)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.IssuedByUserId)
                .IsRequired();

            entity.Property(x => x.EffectiveUntil)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.FilePath)
                .HasMaxLength(500);

            entity.Property(x => x.ActionStatus)
                .HasConversion<int>()
                .HasDefaultValue(DisciplinaryActionStatus.Draft)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(x => x.UpdateDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.DeleteDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.CancelDateTime)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.IsDelete)
                .HasDefaultValue(false);

            entity.Property(x => x.IsCancel)
                .HasDefaultValue(false);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany(x => x.DisciplinaryActions)
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.IssuedByUser)
                .WithMany()
                .HasForeignKey(x => x.IssuedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.WorkforceProfileId);

            entity.HasIndex(x => x.IssuedByUserId);

            entity.HasIndex(x => x.ActionType);

            entity.HasIndex(x => x.SeverityLevel);

            entity.HasIndex(x => x.ActionStatus);

            entity.HasIndex(x => x.IncidentDate);

            entity.HasIndex(x => x.IssuedDate);

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ActionType,
                x.IssuedDate,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.ActionStatus,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.WorkforceProfileId,
                x.SeverityLevel,
                x.ActionStatus,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EffectiveUntil,
                x.ActionStatus,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
