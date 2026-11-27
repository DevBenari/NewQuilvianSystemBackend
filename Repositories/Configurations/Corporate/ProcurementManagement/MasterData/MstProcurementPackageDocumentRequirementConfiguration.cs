using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.MasterData.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.Corporate.ProcurementManagement.MasterData
{
    /// <summary>
    /// Dokumen wajib paket — kamus data pemilihan-penyedia bagian 2. CHECK memaksa tepat satu dari metode atau jenis
    /// pengadaan terisi.
    /// </summary>
    /// <remarks>
    /// Keunikan (<c>DocumentTypeId</c>, <c>MethodId</c>, <c>ProcurementTypeId</c>) pada kamus data diwujudkan sebagai dua
    /// index unik parsial, karena PostgreSQL menganggap dua NULL berbeda: satu index untuk baris per metode dan satu
    /// untuk baris per jenis pengadaan. Contoh: Dasar HPS × Penunjukan Langsung hanya boleh ada satu baris hidup.
    /// Nama index ditulis eksplisit dan tidak lebih dari 63 karakter (batas identifier PostgreSQL), supaya nama di model
    /// sama dengan nama di basis data.
    /// </remarks>
    public class MstProcurementPackageDocumentRequirementConfiguration : IEntityTypeConfiguration<MstProcurementPackageDocumentRequirement>
    {
        public void Configure(EntityTypeBuilder<MstProcurementPackageDocumentRequirement> builder)
        {
            builder.ToTable("MstProcurementPackageDocumentRequirement", "public", table =>
                table.HasCheckConstraint(
                    "CK_MstProcurementPackageDocumentRequirement_ExactlyOneScope",
                    "(\"MethodId\" IS NOT NULL AND \"ProcurementTypeId\" IS NULL) OR (\"MethodId\" IS NULL AND \"ProcurementTypeId\" IS NOT NULL)"));
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.DocumentTypeId, x.MethodId })
                .HasDatabaseName("IX_MstProcurementPackageDocumentRequirement_DocumentTypeId_Meth")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"MethodId\" IS NOT NULL");
            builder.HasIndex(x => new { x.DocumentTypeId, x.ProcurementTypeId })
                .HasDatabaseName("IX_MstProcurementPackageDocumentRequirement_DocumentTypeId_Proc")
                .IsUnique()
                .HasFilter("\"IsDelete\" = false AND \"ProcurementTypeId\" IS NOT NULL");
            builder.HasIndex(x => x.MethodId);
            builder.HasIndex(x => x.ProcurementTypeId);

            builder.HasOne(x => x.DocumentType).WithMany()
                .HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Method).WithMany()
                .HasForeignKey(x => x.MethodId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ProcurementType).WithMany()
                .HasForeignKey(x => x.ProcurementTypeId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
