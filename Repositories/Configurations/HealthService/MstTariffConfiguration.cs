using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthService
{
    public class MstTariffConfiguration : IEntityTypeConfiguration<MstTariff>
    {
        public void Configure(EntityTypeBuilder<MstTariff> entity)
        {
            entity.ToTable("MstTariff", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TariffCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TariffName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.TariffCategoryId).IsRequired();
            entity.Property(x => x.ExternalServiceCode).HasMaxLength(50);
            entity.Property(x => x.ExternalClassCode).HasMaxLength(50);
            entity.Property(x => x.NormalPrice).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            entity.Property(x => x.EffectiveStartDate).HasColumnType("date");
            entity.Property(x => x.EffectiveEndDate).HasColumnType("date");
            entity.Property(x => x.Description).HasMaxLength(250);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.HasOne(x => x.TariffCategory).WithMany().HasForeignKey(x => x.TariffCategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ServiceUnit).WithMany().HasForeignKey(x => x.ServiceUnitId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Clinic).WithMany().HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PatientClass).WithMany().HasForeignKey(x => x.PatientClassId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Procedure).WithMany().HasForeignKey(x => x.ProcedureId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Drug).WithMany().HasForeignKey(x => x.DrugId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.TariffCode).IsUnique();
            entity.HasIndex(x => x.TariffName);
            entity.HasIndex(x => x.TariffCategoryId);
            entity.HasIndex(x => x.ServiceUnitId);
            entity.HasIndex(x => x.ClinicId);
            entity.HasIndex(x => x.PatientClassId);
            entity.HasIndex(x => x.ProcedureId);
            entity.HasIndex(x => x.DrugId);
            entity.HasIndex(x => new { x.DrugId, x.PatientClassId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.ProcedureId, x.PatientClassId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.TariffCategoryId, x.PatientClassId, x.IsActive, x.IsDelete });
            entity.HasIndex(x => new { x.EffectiveStartDate, x.EffectiveEndDate, x.IsActive, x.IsDelete });
        }
    }
}
