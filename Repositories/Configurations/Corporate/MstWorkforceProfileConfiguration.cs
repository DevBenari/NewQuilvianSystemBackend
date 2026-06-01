using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate
{
    public class MstWorkforceProfileConfiguration : IEntityTypeConfiguration<MstWorkforceProfile>
    {
        public void Configure(EntityTypeBuilder<MstWorkforceProfile> entity)
        {
            entity.ToTable("MstWorkforceProfile", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProfileCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.UserType)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(200);

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(30);

            entity.Property(x => x.WhatsAppNumber)
                .HasMaxLength(30);

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

            entity.HasOne(x => x.PrimaryDepartment)
                .WithMany()
                .HasForeignKey(x => x.PrimaryDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PrimaryPosition)
                .WithMany()
                .HasForeignKey(x => x.PrimaryPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ProfileCode)
                .IsUnique();

            entity.HasIndex(x => x.UserType);

            entity.HasIndex(x => x.DisplayName);

            entity.HasIndex(x => x.Email);

            entity.HasIndex(x => new
            {
                x.UserType,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
