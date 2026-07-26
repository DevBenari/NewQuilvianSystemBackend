using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.HumanResource.PayrollManagement
{
    public class TrxPayrollReversalConfiguration : IEntityTypeConfiguration<TrxPayrollReversal>
    {
        public void Configure(EntityTypeBuilder<TrxPayrollReversal> entity)
        {

            entity.ToTable("TrxPayrollReversal", "public");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReversalNumber).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReversalType).HasMaxLength(30).HasDefaultValue("Full").IsRequired();
            entity.Property(x => x.ReversalStatus).HasMaxLength(30).HasDefaultValue("Draft").IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("IDR").IsRequired();
            entity.Property(x => x.ReversalAmount).HasPrecision(18, 2);
            entity.Property(x => x.ReversalReason).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ReversedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.PostedAt).HasColumnType("timestamp with time zone").IsRequired(false);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            ConfigureIdentity(entity);

            entity.HasOne(x => x.PayrollRun).WithMany(x => x.Reversals).HasForeignKey(x => x.PayrollRunId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollRunEmployee).WithMany().HasForeignKey(x => x.PayrollRunEmployeeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PayrollPayment).WithMany(x => x.Reversals).HasForeignKey(x => x.PayrollPaymentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ReversedByUser).WithMany().HasForeignKey(x => x.ReversedByUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PostedByUser).WithMany().HasForeignKey(x => x.PostedByUserId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.ReversalNumber).IsUnique().HasFilter("\"IsDelete\" = false");
            entity.HasIndex(x => new { x.PayrollRunId, x.ReversalStatus, x.IsDelete });
            entity.HasIndex(x => new { x.PayrollPaymentId, x.ReversalStatus, x.IsDelete });
            entity.HasIndex(x => new { x.OriginalGlHeaderId, x.ReversalGlHeaderId });
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
