using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollVariableInputConfiguration : IEntityTypeConfiguration<TrxPayrollVariableInput>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollVariableInput> entity)
        {

            entity.ToTable("TrxPayrollVariableInput", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.InputNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.InputDate).HasColumnType("date");
            entity.Property(x => x.InputType).HasMaxLength(30).HasDefaultValue("Manual").IsRequired();
            entity.Property(x => x.InputStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.Quantity).HasPrecision(18, 4);
            entity.Property(x => x.Rate).HasPrecision(18, 2);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.SourceType).HasMaxLength(50);
            entity.Property(x => x.AttachmentPath).HasMaxLength(500);
            entity.Property(x => x.SubmittedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.VerifiedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(1000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRunEmployee).WithMany(x => x.VariableInputs).HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollComponent).WithMany().HasForeignKey(x => x.PayrollComponentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubmittedByUser).WithMany().HasForeignKey(x => x.SubmittedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.VerifiedByUser).WithMany().HasForeignKey(x => x.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.InputNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollRunEmployeeId, x.PayrollComponentId, x.InputStatus, x.IsDelete });
            entity.HasIndex(x => new { x.SourceType, x.SourceId, x.IsDelete });
        }

        private static void ConfigureIdentity<T>(EntityTypeBuilder<T> entity)
            where T : IdentityModel
        {
            entity.Property(x => x.CreateDateTime)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.IsDelete).HasDefaultValue(false);
            entity.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
