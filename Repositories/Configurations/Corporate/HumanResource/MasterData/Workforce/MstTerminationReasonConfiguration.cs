using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.Workforce
{
    public class MstTerminationReasonConfiguration : IEntityTypeConfiguration<MstTerminationReason>
    {
        public void Configure(EntityTypeBuilder<MstTerminationReason> entity)
        {
            entity.ToTable("MstTerminationReason", "public");

            entity.HasKey(x => x.Id);


            entity.Property(x => x.TerminationReasonCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TerminationReasonName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.TerminationType)
                .HasMaxLength(50)
                .HasDefaultValue("Other")
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsVoluntary)
                .HasDefaultValue(false);

            entity.Property(x => x.RequiresExitClearance)
                .HasDefaultValue(true);

            entity.Property(x => x.DefaultRehireEligible)
                .HasDefaultValue(true);

            entity.Property(x => x.SortOrder)
                .HasDefaultValue(0);

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

            entity.HasIndex(x => x.TerminationReasonCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => x.TerminationReasonName);

            entity.HasIndex(x => new
            {
                x.TerminationType,
                x.IsVoluntary,
                x.RequiresExitClearance,
                x.IsActive,
                x.IsDelete
            });

        }
    }
}
