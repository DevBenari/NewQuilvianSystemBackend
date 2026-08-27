using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstRegisterConfiguration : IEntityTypeConfiguration<MstRegister>
    {
        public void Configure(EntityTypeBuilder<MstRegister> entity)
        {
            entity.ToTable("MstRegister", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RegisterCode)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.RegisterName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Location)
                .HasMaxLength(150);

            entity.Property(x => x.Description)
                .HasMaxLength(250);

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

            entity.HasIndex(x => x.RegisterCode)
                .IsUnique();

            entity.HasIndex(x => x.RegisterName);

            entity.HasIndex(x => new
            {
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
