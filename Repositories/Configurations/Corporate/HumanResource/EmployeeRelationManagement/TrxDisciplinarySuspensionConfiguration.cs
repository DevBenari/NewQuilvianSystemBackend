using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.EmployeeRelationManagement
{
    public class TrxDisciplinarySuspensionConfiguration : IEntityTypeConfiguration<TrxDisciplinarySuspension>
    {
        public void Configure(EntityTypeBuilder<TrxDisciplinarySuspension> entity)
        {
            entity.ToTable("trx_disciplinary_suspensions", "public");
            entity.HasKey(x => x.SuspensionId);

            entity.Property(x => x.SuspensionId).HasColumnName("suspension_id");
            entity.Property(x => x.InvestigationId).HasColumnName("investigation_id").IsRequired();
            entity.Property(x => x.EmployeeId).HasColumnName("employee_id").IsRequired();
            entity.Property(x => x.SuspensionNumber).HasColumnName("suspension_number").HasMaxLength(64).IsRequired();
            entity.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpectedEndDate).HasColumnName("expected_end_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.ActualEndDate).HasColumnName("actual_end_date").HasColumnType("timestamp with time zone");
            entity.Property(x => x.WagePaymentStatus).HasColumnName("wage_payment_status").HasMaxLength(32).HasDefaultValue("PaidFull").IsRequired();
            entity.Property(x => x.IsSystemAccessRevoked).HasColumnName("is_system_access_revoked").HasDefaultValue(true);
            entity.Property(x => x.IsPhysicalAccessRevoked).HasColumnName("is_physical_access_revoked").HasDefaultValue(true);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).HasDefaultValue("Active").IsRequired();
            entity.Property(x => x.ReinstatementNotes).HasColumnName("reinstatement_notes");

            entity.HasIndex(x => x.SuspensionNumber).IsUnique();
            entity.HasIndex(x => x.EmployeeId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ExpectedEndDate);

            entity.HasOne(x => x.Investigation)
                .WithMany(x => x.Suspensions)
                .HasForeignKey(x => x.InvestigationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
