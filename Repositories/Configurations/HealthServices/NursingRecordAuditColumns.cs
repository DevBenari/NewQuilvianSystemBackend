using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Repositories.Configurations.HealthServices
{
    /// <summary>
    /// Kolom jejak <c>IdentityModel</c> yang sama pada seluruh tabel baru sub-modul keperawatan
    /// rawat inap revision 7 — <c>BE-RWI-107</c> s.d. <c>BE-RWI-123</c>.
    /// </summary>
    /// <remarks>
    /// Bentuknya sama persis dengan <c>SlidingScaleAuditColumns</c> milik <c>BE-RWI-102</c>, sehingga
    /// tabel keperawatan dan tabel sliding scale yang saling merujuk punya kolom jejak yang setara.
    /// Dipisah karena dipakai dua modul pemilik tabel sekaligus: <c>ClinicalManagement</c> dan
    /// <c>PharmacyManagement</c>.
    /// </remarks>
    internal static class NursingRecordAuditColumns
    {
        public static void Configure<T>(EntityTypeBuilder<T> builder) where T : IdentityModel
        {
            builder.Property(x => x.CreateDateTime).HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(x => x.UpdateDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.DeleteDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CancelDateTime).HasColumnType("timestamp with time zone");
            builder.Property(x => x.IsDelete).HasDefaultValue(false);
            builder.Property(x => x.IsCancel).HasDefaultValue(false);
        }
    }
}
