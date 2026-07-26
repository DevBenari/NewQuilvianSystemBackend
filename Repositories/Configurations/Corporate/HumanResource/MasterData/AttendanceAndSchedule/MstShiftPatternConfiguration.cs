using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.MasterData.AttendanceAndSchedule
{
    public class MstShiftPatternConfiguration : IEntityTypeConfiguration<MstShiftPattern>
    {
        public void Configure(EntityTypeBuilder<MstShiftPattern> entity)
        {
            entity.ToTable("MstShiftPattern", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ShiftGroupId).IsRequired();
            entity.Property(x => x.ShiftPatternCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ShiftPatternName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.CycleLengthDays).HasDefaultValue(1);
            entity.Property(x => x.PatternDefinitionJson).HasColumnType("jsonb").HasDefaultValue("[]").IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.IsDefault).HasDefaultValue(false);
            entity.Property(x => x.SortOrder).HasDefaultValue(0);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            ConfigureAuditFields(entity);

            entity.HasOne(x => x.ShiftGroup)
                .WithMany(x => x.ShiftPatterns)
                .HasForeignKey(x => x.ShiftGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ShiftPatternCode)
                .IsUnique()
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new { x.ShiftGroupId, x.ShiftPatternName });
            entity.HasIndex(x => new { x.ShiftGroupId, x.IsDefault, x.IsActive, x.IsDelete });
        }

        private static void ConfigureAuditFields(EntityTypeBuilder<MstShiftPattern> entity)
        {
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
