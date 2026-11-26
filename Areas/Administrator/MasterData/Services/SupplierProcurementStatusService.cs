using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Services
{
    /// <summary>
    /// Satu-satunya penulis status blokir supplier (<c>MstSupplier.IsBlacklisted</c>, <c>BlacklistReason</c>) sejak
    /// PBJ-DEC-060 (BE-PBJ-VD-007; PBJ-VD-INT 1.0 §1). Master Data Supplier tidak lagi mengubah kedua kolom itu.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dipanggil hanya oleh penerapan keputusan usulan blokir/buka blokir Pengadaan yang sudah disetujui. Service ini
    /// <b>tidak menyimpan</b>: perubahan masuk change tracker dan tersimpan bersama status usulan dalam transaksi pemanggil,
    /// sehingga bila salah satu gagal keduanya batal.
    /// </para>
    /// <para>
    /// Contoh: usulan UBV-000003 "blokir PT Alkes Maju — 3 NCR kritis" disetujui Direktur →
    /// <c>ApplyAsync(supplierId, true, "3 NCR kritis…", usulanId, direktur)</c>. Supplier yang diblokir otomatis bukan lagi
    /// supplier pilihan (<c>IsPreferredSupplier = false</c>), sama dengan aturan Master Data sebelumnya.
    /// </para>
    /// </remarks>
    public sealed class SupplierProcurementStatusService
    {
        private readonly ApplicationDbContext _dbContext;

        public SupplierProcurementStatusService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menetapkan status blokir. <paramref name="reason"/> disalin ke <c>BlacklistReason</c> saat diblokir dan
        /// dikosongkan saat dibuka. <paramref name="statusRequestId"/> hanya untuk jejak audit pemanggil.
        /// </summary>
        public async Task ApplyAsync(
            Guid supplierId,
            bool isBlacklisted,
            string? reason,
            Guid statusRequestId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (statusRequestId == Guid.Empty)
                throw new ArgumentException("Usulan status vendor wajib disebut.", nameof(statusRequestId));

            var supplier = await _dbContext.MstSuppliers
                .FirstOrDefaultAsync(x => x.Id == supplierId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Supplier tidak ditemukan atau sudah dihapus di Master Data.");

            supplier.IsBlacklisted = isBlacklisted;
            supplier.BlacklistReason = isBlacklisted
                ? (string.IsNullOrWhiteSpace(reason) ? null : reason.Trim())
                : null;
            if (isBlacklisted) supplier.IsPreferredSupplier = false;
            supplier.UpdateDateTime = DateTime.UtcNow;
            supplier.UpdateBy = actorUserId;
        }
    }
}
