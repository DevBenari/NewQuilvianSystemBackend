namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Kontrak baca indikator ketepatan ringkasan pulang Rawat Inap (`INT-RM-18`, `BE-49`).
    /// Implementasinya berada di folder pemilik data (Rawat Inap, `BE-50`), mengikuti pola
    /// <see cref="Sources.IMedicalRecordDocumentSource"/> (`DC-RM-03`).
    /// </summary>
    /// <remarks>
    /// Satu baris per episode Rawat Inap yang tidak dihapus dan tidak batal, yang pasiennya sudah
    /// pulang (keluar ruangan atau episode ditutup), dan yang jam mulainya jatuh di
    /// <c>[startFromUtc, startToUtcExclusive)</c> — aturan lengkap `02-backend-architecture.md`
    /// A6.9.1. Baris <b>tidak pernah</b> memuat identitas pasien, nomor rekam medis, kunjungan,
    /// maupun isi ringkasan (`RM-DEC-101`).
    /// </remarks>
    public interface IDischargeSummaryTimelinessSource
    {
        Task<IReadOnlyList<DischargeSummaryTimelinessRow>> ReadAsync(
            DateTime startFromUtc,
            DateTime startToUtcExclusive,
            CancellationToken cancellationToken);
    }

    /// <summary>Satu episode Rawat Inap yang dinilai indikator (A6.9.1).</summary>
    /// <param name="EpisodeId">Kunci internal untuk menghitung; tidak pernah keluar dari service.</param>
    /// <param name="StartAtUtc">Jam mulai: <c>PhysicallyLeftAt</c>, cadangan <c>ClosedAt</c> (`RM-DEC-107`).</param>
    /// <param name="StartedFromClosedAt">Benar bila jam mulai memakai cadangan waktu tutup episode.</param>
    /// <param name="FirstSignedAtUtc">Tanda tangan pertama yang terlacak; kosong bila belum pernah ditandatangani (`RM-DEC-105`).</param>
    /// <param name="FirstSignerDoctorId">Dokter penanda tangan pertama (`DS-R6-04`).</param>
    /// <param name="ActiveDpjpDoctorId">DPJP aktif; hanya diisi bila belum ditandatangani (`RM-DEC-106`).</param>
    public sealed record DischargeSummaryTimelinessRow(
        Guid EpisodeId,
        DateTime StartAtUtc,
        bool StartedFromClosedAt,
        DateTime? FirstSignedAtUtc,
        Guid? FirstSignerDoctorId,
        Guid? ActiveDpjpDoctorId);
}
