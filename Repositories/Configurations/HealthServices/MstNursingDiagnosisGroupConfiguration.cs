using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstNursingDiagnosisGroupConfiguration : IEntityTypeConfiguration<MstNursingDiagnosisGroup>
    {
        public void Configure(EntityTypeBuilder<MstNursingDiagnosisGroup> entity)
        {
            entity.ToTable("MstNursingDiagnosisGroup", "public");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.GroupCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.GroupName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired(false);

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

            entity.HasIndex(x => x.GroupCode)
                .IsUnique();

            entity.HasMany(x => x.Diagnoses)
                .WithOne(x => x.Group)
                .HasForeignKey(x => x.GroupId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
