using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services
{
    /// <summary>
    /// Kebijakan murni pembatasan data lapis service untuk tenaga medis (MDF-PERM-1.0 §5, MF-DEC-010, BE-MDF-017).
    /// Mengatur aturan row-level security:
    /// - T-80: Dokter A meminta hasil jasa Dokter B -> menerima kosong.
    /// - T-81: Dokter A meminta daftar tanpa filter -> hanya barisnya sendiri yang muncul.
    /// - T-82: Dokter A membuka rincian milik B lewat ID langsung -> 404 Not Found (bukan 403 Forbidden).
    /// - T-84: Staf Medical Fee tanpa pembatasan -> seluruh 218 dokter terlihat.
    /// </summary>
    public static class MedicalFeeDataRestrictionPolicy
    {
        /// <summary>
        /// Menerapkan filter pembatasan data pada kueri daftar terpaginasi hasil jasa medis atau kesepakatan tarif.
        /// </summary>
        /// <param name="actor">Konteks pengguna pemanggil layanan.</param>
        /// <param name="requestedPayeeReferenceId">Rujukan ID penerima yang diminta dalam query filter.</param>
        /// <param name="shouldReturnEmpty">True jika permintaan ditolak menghasilkan 0 baris (contoh T-80).</param>
        /// <returns>True jika aturan pembatasan aktif diterapkan.</returns>
        public static bool ApplyListFilter(
            MedicalFeeActorContext? actor,
            ref Guid? requestedPayeeReferenceId,
            out bool shouldReturnEmpty)
        {
            shouldReturnEmpty = false;

            if (actor == null || actor.IsUnrestricted)
            {
                return false; // Staf/Admin berhak melihat seluruh penerima (T-84)
            }

            var myPayeeId = actor.RestrictedPayeeReferenceId ?? Guid.Empty;

            // T-80: Jika Dokter A meminta data penerima lain secara eksplisit (Dokter B),
            // sistem mengembalikan hasil kosong tanpa membocorkan data target.
            if (requestedPayeeReferenceId.HasValue && requestedPayeeReferenceId.Value != myPayeeId)
            {
                shouldReturnEmpty = true;
                return true;
            }

            // T-81: Jika Dokter A meminta daftar tanpa menyertakan filter ID,
            // query secara otomatis dipaksa menyaring hanya baris miliknya sendiri.
            requestedPayeeReferenceId = myPayeeId;
            return true;
        }

        /// <summary>
        /// Memvalidasi otorisasi akses ke satu rekaman hasil jasa medis (MdfServiceFee) tertentu.
        /// Melempar KeyNotFoundException jika pengguna dibatasi dan rekaman bukan miliknya,
        /// sehingga HTTP response menghasilkan 404 Not Found bukan 403 Forbidden (T-82).
        /// </summary>
        public static void AssertCanAccessServiceFee(
            MedicalFeeActorContext? actor,
            Guid feePayeeReferenceId,
            Guid serviceFeeId)
        {
            if (actor == null || actor.IsUnrestricted)
            {
                return; // Staf/Admin berhak membuka hasil jasa siapa pun (T-84)
            }

            var myPayeeId = actor.RestrictedPayeeReferenceId ?? Guid.Empty;

            // T-82: 404 dipakai secara sengaja untuk mencegah user enumeration / data leakage.
            // Respons 403 atas ID tertentu membocorkan bahwa hasil jasa dengan ID itu benar-benar ada.
            if (feePayeeReferenceId != myPayeeId)
            {
                throw new KeyNotFoundException($"Hasil jasa medis dengan ID '{serviceFeeId}' tidak ditemukan.");
            }
        }

        /// <summary>
        /// Memvalidasi otorisasi akses ke satu rekaman kesepakatan tarif sharing (MdfSharingAgreement).
        /// Melempar KeyNotFoundException jika pengguna dibatasi dan kesepakatan bukan miliknya (404 Not Found).
        /// </summary>
        public static void AssertCanAccessSharingAgreement(
            MedicalFeeActorContext? actor,
            Guid agreementPayeeReferenceId,
            Guid agreementId)
        {
            if (actor == null || actor.IsUnrestricted)
            {
                return;
            }

            var myPayeeId = actor.RestrictedPayeeReferenceId ?? Guid.Empty;

            if (agreementPayeeReferenceId != myPayeeId)
            {
                throw new KeyNotFoundException($"Kesepakatan tarif dengan ID '{agreementId}' tidak ditemukan.");
            }
        }
    }
}

