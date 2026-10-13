using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    public class MstPatientCompanyGuarantorConfiguration : IEntityTypeConfiguration<MstPatientCompanyGuarantor>
    {
        public void Configure(EntityTypeBuilder<MstPatientCompanyGuarantor> entity)
        {
            entity.ToTable("MstPatientCompanyGuarantor", "public", table =>
            {
                // BE-FIN-105, kamus data 9.4: ketiga kolom tautan HR kosong semua (kartu biasa),
                // atau EmployeeId dan BenefitRelationship terisi dengan SELF <=> FamilyMemberId kosong.
                table.HasCheckConstraint(
                    "CK_MstPatientCompanyGuarantor_BenefitLink",
                    "(\"EmployeeId\" IS NULL AND \"FamilyMemberId\" IS NULL AND \"BenefitRelationship\" IS NULL) " +
                    "OR (\"EmployeeId\" IS NOT NULL AND \"BenefitRelationship\" IS NOT NULL " +
                    "AND ((\"BenefitRelationship\" = 'SELF') = (\"FamilyMemberId\" IS NULL)))");
            });

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientId)
                .IsRequired();

            entity.Property(x => x.CompanyGuarantorId)
                .IsRequired();

            entity.Property(x => x.EmployeeNumber)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.EmployeeName)
                .HasMaxLength(200);

            entity.Property(x => x.DepartmentName)
                .HasMaxLength(100);

            entity.Property(x => x.PositionName)
                .HasMaxLength(100);

            entity.Property(x => x.GradeLevel)
                .HasMaxLength(100);

            entity.Property(x => x.BenefitPlanCode)
                .HasMaxLength(100);

            entity.Property(x => x.BenefitPlanName)
                .HasMaxLength(150);

            entity.Property(x => x.ClassName)
                .HasMaxLength(100);

            entity.Property(x => x.EffectiveStartDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.EffectiveEndDate)
                .HasColumnType("date")
                .IsRequired(false);

            entity.Property(x => x.IsPrimary)
                .HasDefaultValue(false);

            entity.Property(x => x.IsEligible)
                .HasDefaultValue(true);

            entity.Property(x => x.LastEligibilityCheckAt)
                .HasColumnType("timestamp with time zone")
                .IsRequired(false);

            entity.Property(x => x.LastEligibilityReferenceNumber)
                .HasMaxLength(100);

            entity.Property(x => x.EligibilityNote)
                .HasMaxLength(250);

            entity.Property(x => x.AnnualLimitAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.RemainingLimitAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.CoPaymentPercent)
                .HasColumnType("numeric(5,2)")
                .IsRequired(false);

            entity.Property(x => x.CoPaymentAmount)
                .HasColumnType("numeric(18,2)")
                .IsRequired(false);

            entity.Property(x => x.IsNeedGuaranteeLetter)
                .HasDefaultValue(true);

            entity.Property(x => x.IsNeedEmployeeVerification)
                .HasDefaultValue(true);

            entity.Property(x => x.IsAllowExcessPaymentByPatient)
                .HasDefaultValue(true);

            entity.Property(x => x.GuaranteeDocumentPath)
                .HasMaxLength(500);

            entity.Property(x => x.CardImagePath)
                .HasMaxLength(500);

            entity.Property(x => x.Notes)
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.EmployeeId)
                .IsRequired(false);

            entity.Property(x => x.FamilyMemberId)
                .IsRequired(false);

            entity.Property(x => x.BenefitRelationship)
                .HasMaxLength(50)
                .IsRequired(false);

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

            entity.HasOne(x => x.Patient)
                .WithMany()
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.CompanyGuarantor)
                .WithMany()
                .HasForeignKey(x => x.CompanyGuarantorId)
                .OnDelete(DeleteBehavior.Restrict);

            // BE-FIN-105: FK lintas area ke HR tanpa navigation property, mengikuti pola
            // SlidingScaleExecutionConfiguration (HealthServices -> MstEmployee).
            entity.HasOne<MstEmployee>()
                .WithMany()
                .HasForeignKey(x => x.EmployeeId)
                .HasConstraintName("FK_MstPatientCompanyGuarantor_EmployeeId")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<WfpFamilyMember>()
                .WithMany()
                .HasForeignKey(x => x.FamilyMemberId)
                .HasConstraintName("FK_MstPatientCompanyGuarantor_FamilyMemberId")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PatientId);

            entity.HasIndex(x => x.CompanyGuarantorId);

            entity.HasIndex(x => x.EmployeeId)
                .HasFilter("\"EmployeeId\" IS NOT NULL")
                .HasDatabaseName("IX_MstPatientCompanyGuarantor_EmployeeId");

            entity.HasIndex(x => x.FamilyMemberId)
                .HasFilter("\"FamilyMemberId\" IS NOT NULL")
                .HasDatabaseName("IX_MstPatientCompanyGuarantor_FamilyMemberId");

            entity.HasIndex(x => x.EmployeeNumber)
                .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.CompanyGuarantorId,
                x.EmployeeNumber
            })
            .IsUnique()
            .HasFilter("\"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IsPrimary
            })
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            entity.HasIndex(x => new
            {
                x.PatientId,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.CompanyGuarantorId,
                x.IsEligible,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                x.IsActive,
                x.IsDelete
            });

            entity.HasIndex(x => new
            {
                x.BenefitPlanCode,
                x.ClassName,
                x.IsActive,
                x.IsDelete
            });
        }
    }
}
