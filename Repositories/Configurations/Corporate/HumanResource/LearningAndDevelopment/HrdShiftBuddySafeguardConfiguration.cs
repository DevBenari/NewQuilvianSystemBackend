using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.LearningAndDevelopment
{
    public class HrdShiftBuddySafeguardConfiguration : IEntityTypeConfiguration<HrdShiftBuddySafeguard>
    {
        public void Configure(EntityTypeBuilder<HrdShiftBuddySafeguard> entity)
        {
            entity.ToTable("HrdShiftBuddySafeguard", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.StaffNameSnapshot).HasMaxLength(200);
            entity.Property(x => x.Message).HasMaxLength(1000);
            entity.Property(x => x.ActivatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReleasedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsOverdueFlagged).HasDefaultValue(true);
            entity.Property(x => x.IsBuddySystemEnforced).HasDefaultValue(true);
            entity.Property(x => x.IsRkkRestrictedLevel3).HasDefaultValue(false);
            entity.Property(x => x.IsActive).HasDefaultValue(true);

            entity.HasOne(x => x.WorkforceProfile)
                .WithMany()
                .HasForeignKey(x => x.WorkforceProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Satu pengamanan aktif per staf.
            entity.HasIndex(x => x.WorkforceProfileId)
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDelete\" = false");

            ConfigureIdentity(entity);
        }

        private static void ConfigureIdentity(EntityTypeBuilder<HrdShiftBuddySafeguard> entity)
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
