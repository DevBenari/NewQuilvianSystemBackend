using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstPrescriptionTemplateConfiguration
        : IEntityTypeConfiguration<MstPrescriptionTemplate>
    {
        public void Configure(EntityTypeBuilder<MstPrescriptionTemplate> entity)
        {
            entity.ToTable("MstPrescriptionTemplate", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TemplateCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TemplateName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.TemplateCategory)
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsShared)
                .HasDefaultValue(false);

            entity.Property(x => x.IsFavorite)
                .HasDefaultValue(false);

            entity.Property(x => x.UsageCount)
                .HasDefaultValue(0);

            entity.Property(x => x.RegularItemCount)
                .HasDefaultValue(0);

            entity.Property(x => x.CompoundCount)
                .HasDefaultValue(0);

            entity.Property(x => x.CompoundIngredientCount)
                .HasDefaultValue(0);

            entity.Property(x => x.TotalItemCount)
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

            entity.HasOne(x => x.OwnerDoctor)
                .WithMany()
                .HasForeignKey(x => x.OwnerDoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TemplateCode)
                .IsUnique();

            entity.HasIndex(x => new
            {
                x.OwnerDoctorId,
                x.TemplateName,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsShared,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.IsFavorite,
                x.OwnerDoctorId,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.TemplateCategory,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
