using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.BenefitManagement
{
    /// <summary>BE-FIN-104, kamus data 9.2. Buku mutasi plafon, hanya menambah.</summary>
    public class HrdBenefitBalanceMovementConfiguration : IEntityTypeConfiguration<HrdBenefitBalanceMovement>
    {
        public void Configure(EntityTypeBuilder<HrdBenefitBalanceMovement> entity)
        {
            entity.ToTable("HrdBenefitBalanceMovement", "public", table =>
            {
                table.HasCheckConstraint(
                    "CK_HrdBenefitBalanceMovement_Type",
                    "\"MovementType\" IN ('USE','RETURN')");
                table.HasCheckConstraint(
                    "CK_HrdBenefitBalanceMovement_Kind",
                    "(\"BenefitPersonKind\" = 'ENROLLMENT' AND \"EmployeeBenefitDependentId\" IS NULL) " +
                    "OR (\"BenefitPersonKind\" = 'DEPENDENT' AND \"EmployeeBenefitDependentId\" IS NOT NULL)");
                table.HasCheckConstraint(
                    "CK_HrdBenefitBalanceMovement_Amount",
                    "\"Amount\" > 0");
                table.HasCheckConstraint(
                    "CK_HrdBenefitBalanceMovement_Balance",
                    "((\"MovementType\" = 'USE' AND \"BalanceAfter\" = \"BalanceBefore\" - \"Amount\") " +
                    "OR (\"MovementType\" = 'RETURN' AND \"BalanceAfter\" = \"BalanceBefore\" + \"Amount\")) " +
                    "AND \"BalanceAfter\" >= 0");
                table.HasCheckConstraint(
                    "CK_HrdBenefitBalanceMovement_Reverses",
                    "(\"MovementType\" = 'RETURN') = (\"ReversesMovementId\" IS NOT NULL)");
            });
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);

            entity.Property(x => x.BenefitPersonKind).HasMaxLength(20).IsRequired();
            entity.Property(x => x.MovementType).HasMaxLength(10).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.BalanceBefore).HasPrecision(18, 2);
            entity.Property(x => x.BalanceAfter).HasPrecision(18, 2);
            entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ReasonText).HasMaxLength(500);

            entity.HasIndex(x => new { x.SourceHandoffKey, x.MovementType })
                .IsUnique()
                .HasDatabaseName("UX_HrdBenefitBalanceMovement_Key");

            entity.HasIndex(x => x.ReversesMovementId)
                .IsUnique()
                .HasFilter("\"ReversesMovementId\" IS NOT NULL")
                .HasDatabaseName("UX_HrdBenefitBalanceMovement_Reverses");

            entity.HasIndex(x => new { x.EmployeeBenefitEnrollmentId, x.OccurredAt })
                .HasDatabaseName("IX_HrdBenefitBalanceMovement_Enrollment_OccurredAt");

            entity.HasIndex(x => x.EmployeeBenefitDependentId)
                .HasDatabaseName("IX_HrdBenefitBalanceMovement_Dependent");

            entity.HasIndex(x => x.InvoiceId)
                .HasDatabaseName("IX_HrdBenefitBalanceMovement_InvoiceId");

            entity.HasOne(x => x.EmployeeBenefitEnrollment)
                .WithMany()
                .HasForeignKey(x => x.EmployeeBenefitEnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.EmployeeBenefitDependent)
                .WithMany()
                .HasForeignKey(x => x.EmployeeBenefitDependentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReversesMovement)
                .WithMany()
                .HasForeignKey(x => x.ReversesMovementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
