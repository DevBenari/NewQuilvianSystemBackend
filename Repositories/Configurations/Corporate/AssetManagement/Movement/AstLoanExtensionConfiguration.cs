using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.AssetManagement.Movement;

public sealed class AstLoanExtensionConfiguration : IEntityTypeConfiguration<AstLoanExtension>
{
    public void Configure(EntityTypeBuilder<AstLoanExtension> entity)
    {
        entity.ToTable("AstLoanExtension", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.PreviousReturnDate).HasColumnType("date");
        entity.Property(x => x.RequestedReturnDate).HasColumnType("date");
        entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Status).HasConversion<int>().IsRequired();
        entity.Property(x => x.RequestedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DecidedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DecisionReason).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Loan).WithMany(x => x.Extensions).HasForeignKey(x => x.LoanId).OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.LoanId)
            .IsUnique()
            .HasFilter("\"Status\" = 1 AND \"IsDelete\" = false")
            .HasDatabaseName("IX_AstLoanExtension_OpenPerLoan");

        entity.HasIndex(x => x.Status);
    }
}
