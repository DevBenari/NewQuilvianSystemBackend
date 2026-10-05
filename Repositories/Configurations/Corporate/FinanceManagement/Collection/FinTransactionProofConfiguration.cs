using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.FinanceManagement.Collection;

public sealed class FinTransactionProofConfiguration : IEntityTypeConfiguration<FinTransactionProof>
{
    public void Configure(EntityTypeBuilder<FinTransactionProof> entity)
    {
        entity.ToTable("FinTransactionProof", "public");
        entity.HasKey(x => x.Id);

        entity.Property(x => x.ProofType).HasMaxLength(30).IsRequired();
        entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
        entity.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
        entity.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
        entity.Property(x => x.MediaType).HasMaxLength(100).IsRequired();
        entity.Property(x => x.UploadedAt).HasColumnType("timestamp with time zone").IsRequired();

        entity.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
        entity.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
        entity.Property(x => x.IsDelete).HasDefaultValue(false);
        entity.Property(x => x.IsCancel).HasDefaultValue(false);

        // FIN-DES-087: satu bukti tidak dapat dipakai dua pembayaran — dijaga unique index pada
        // ProofId di kedua tabel mutasi (FinReceivableMovement/FinSupplierPayableMovement, BE-FIN-058).
        // Di sini, nama berkas tersimpan sendiri juga MUST unik supaya tidak pernah bertabrakan.
        entity.HasIndex(x => x.StoredFileName)
            .IsUnique()
            .HasFilter("\"IsDelete\" = false")
            .HasDatabaseName("IX_FinTransactionProof_StoredFileName");

        entity.HasIndex(x => x.ProofType).HasDatabaseName("IX_FinTransactionProof_ProofType");
        entity.HasIndex(x => x.UploadedBy).HasDatabaseName("IX_FinTransactionProof_UploadedBy");
    }
}
