using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Receivable;

public sealed class FinReceivableDocumentConfiguration : IEntityTypeConfiguration<FinReceivableDocument>
{
    public void Configure(EntityTypeBuilder<FinReceivableDocument> entity)
    {
        entity.ToTable("FinReceivableDocument", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
        entity.Property(x => x.DocumentNumber).HasMaxLength(100);
        entity.Property(x => x.IsReceived).HasDefaultValue(false);
        entity.Property(x => x.ReceivedAt).HasColumnType("timestamp with time zone");
        entity.Property(x => x.Notes).HasMaxLength(500);

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        entity.HasOne(x => x.Receivable)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.ReceivableId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
