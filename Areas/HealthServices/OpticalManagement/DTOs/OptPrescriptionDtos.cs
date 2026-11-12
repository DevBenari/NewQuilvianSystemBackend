using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs
{
    // OPT-API-v1 grup "Health Services / Optical Management / Opt Prescription" (BE-OPT-012).
    // Seluruh nilai ukur bertanda Sensitif di kamus data: tidak pernah ditulis ke logger.

    /// <summary>
    /// Ukuran kacamata satu mata. SPH −20,00..+20,00 dan CYL −6,00..0,00, kelipatan 0,25; AXIS 0–180, wajib bila
    /// CYL ≠ 0 dan kosong bila CYL = 0; ADD +0,75..+3,50 kelipatan 0,25 (<c>OPT-DEC-045</c>, <c>OPT-VAL-010</c>..<c>014</c>).
    /// Mata yang tidak diresepkan dikirim kosong; mata yang diresepkan wajib ber-SPH (tulis 0 untuk plano).
    /// </summary>
    public class OptLensPowerDto
    {
        public decimal? Sph { get; set; }
        public decimal? Cyl { get; set; }
        public int? Axis { get; set; }
        public decimal? Add { get; set; }
    }

    /// <summary>
    /// Lensa kontak satu mata. Power memakai aturan SPH; CYL/AXIS torik memakai aturan kacamata; BC dan DIA mengikuti
    /// rentang pengaturan Optik (<c>OPT-VAL-018</c>). Lensa kosmetik tanpa ukuran ditulis power 0.
    /// </summary>
    public class OptContactLensEyeDto
    {
        public decimal? Power { get; set; }
        public decimal? Cyl { get; set; }
        public int? Axis { get; set; }
        public decimal? BaseCurve { get; set; }
        public decimal? Diameter { get; set; }
    }

    /// <summary>
    /// Isi resep. Resep kacamata memakai <see cref="Od"/>/<see cref="Os"/> dan PD; resep lensa kontak memakai
    /// <see cref="ContactLensOd"/>/<see cref="ContactLensOs"/> serta merk/jenis. Isian milik jenis lain diabaikan.
    /// </summary>
    public class OptPrescriptionValues
    {
        public OptLensPowerDto? Od { get; set; }
        public OptLensPowerDto? Os { get; set; }

        /// <summary>Jarak pupil (mm) 50..80 (<c>OPT-VAL-015</c>).</summary>
        public decimal? PdDistance { get; set; }
        public decimal? PdNear { get; set; }

        public OptContactLensEyeDto? ContactLensOd { get; set; }
        public OptContactLensEyeDto? ContactLensOs { get; set; }

        /// <summary>Wajib untuk resep lensa kontak (<c>OPT-VAL-017</c>).</summary>
        [MaxLength(150)]
        public string? ContactLensBrandOrType { get; set; }

        /// <summary>Contoh "Bulanan".</summary>
        [MaxLength(50)]
        public string? ContactLensReplacement { get; set; }

        /// <summary>
        /// Diisi Sp.M ber-hak <c>OptPrescription : OverrideRange</c> bila ada SPH/CYL/ADD/power di luar rentang
        /// (<c>OPT-VAL-004</c>, <c>005</c>). Diabaikan bila semua isian dalam rentang.
        /// </summary>
        [MaxLength(500)]
        public string? RangeOverrideReason { get; set; }
    }

    /// <summary>Resep internal dari pemeriksaan refraksi yang sudah final.</summary>
    public class OptPrescriptionCreateRequest : OptPrescriptionValues
    {
        [Required]
        public Guid? RefractionExamId { get; set; }

        [Required]
        public OptPrescriptionType? PrescriptionType { get; set; }
    }

    /// <summary>
    /// Salin resep dari luar RS (<c>OPT-DEC-017</c>). Silinder plus ditransposisi ke notasi minus; teks aslinya
    /// tersimpan (<c>OPT-VAL-016</c>). Contoh: +1,00 / +0,50 × 90 → +1,50 / −0,50 × 180.
    /// </summary>
    public class OptPrescriptionCopyExternalRequest : OptPrescriptionValues
    {
        /// <summary>Kunjungan Optik tempat resep disalin.</summary>
        [Required]
        public Guid? EncounterId { get; set; }

        [Required]
        public OptPrescriptionType? PrescriptionType { get; set; }

        [MaxLength(150)]
        public string? ExternalAuthorName { get; set; }

        [MaxLength(150)]
        public string? ExternalFacilityName { get; set; }

        /// <summary>Tanggal di kertas resep; tidak boleh melewati hari ini (<c>OPT-VAL-020</c>).</summary>
        public DateOnly? IssuedDate { get; set; }
    }

    /// <summary>
    /// Revisi resep yang belum terkunci (<c>OPT-DEC-007</c>, <c>011</c>): menambah versi baru berisi isi lengkap.
    /// Silinder wajib sudah bernotasi minus.
    /// </summary>
    public class OptPrescriptionReviseRequest : OptPrescriptionValues
    {
        [Required]
        public int? ExpectedVersion { get; set; }

        /// <summary>Wajib (<c>OPT-VAL-021</c>), contoh "verifikasi ulang dengan trial frame".</summary>
        [MaxLength(500)]
        public string? RevisionReason { get; set; }
    }

    /// <summary>Penyaring daftar resep.</summary>
    public class OptPrescriptionListQuery
    {
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public OptPrescriptionType? PrescriptionType { get; set; }

        /// <summary>Benar = hanya resep yang masih berlaku hari ini (tanggal WIB).</summary>
        public bool? ValidOnly { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Hak pengguna saat ini untuk mengonfirmasi ukuran di luar rentang.</summary>
    public class OptRangeOverrideEligibilityResponse
    {
        /// <summary>Benar bila pengguna Sp.M dengan SIP dan STR berlaku.</summary>
        public bool CanOverrideRange { get; set; }

        /// <summary>Alasan bila tidak bisa, contoh "Hanya Sp.M yang dapat mengonfirmasi ukuran di luar rentang."</summary>
        public string? Reason { get; set; }
    }

    /// <summary>Satu versi isi resep.</summary>
    public class OptPrescriptionVersionResponse
    {
        public Guid Id { get; set; }
        public int VersionNo { get; set; }
        public OptLensPowerDto? Od { get; set; }
        public OptLensPowerDto? Os { get; set; }
        public decimal? PdDistance { get; set; }
        public decimal? PdNear { get; set; }
        public OptContactLensEyeDto? ContactLensOd { get; set; }
        public OptContactLensEyeDto? ContactLensOs { get; set; }
        public string? ContactLensBrandOrType { get; set; }
        public string? ContactLensReplacement { get; set; }

        /// <summary>Benar bila resep luar bernotasi plus diubah ke minus; teks aslinya di <see cref="OriginalNotationText"/>.</summary>
        public bool IsTransposed { get; set; }
        public string? OriginalNotationText { get; set; }
        public string? RangeOverrideReason { get; set; }

        /// <summary>Ringkas, contoh "OD −1,50 / −0,50 × 180; OS −1,75".</summary>
        public string Summary { get; set; } = string.Empty;

        public Guid AuthorWorkforceProfileId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorLicenseNumberSnapshot { get; set; } = string.Empty;
        public DateTime AuthoredAt { get; set; }
        public string? RevisionReason { get; set; }
    }

    /// <summary>Resep beserta versinya.</summary>
    public class OptPrescriptionResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid EncounterId { get; set; }
        public Guid? RefractionExamId { get; set; }
        public OptPrescriptionType PrescriptionType { get; set; }
        public string PrescriptionTypeLabel { get; set; } = string.Empty;
        public OptPrescriptionOrigin Origin { get; set; }
        public string OriginLabel { get; set; } = string.Empty;
        public string? ExternalAuthorName { get; set; }
        public string? ExternalFacilityName { get; set; }
        public DateOnly IssuedDate { get; set; }
        public int ValidityMonthsSnapshot { get; set; }
        public DateOnly ValidUntil { get; set; }

        /// <summary>Benar bila <see cref="ValidUntil"/> sudah lewat (tanggal WIB hari ini).</summary>
        public bool IsExpired { get; set; }

        /// <summary>Benar bila lensa sudah dipesan; revisi ditolak (<c>OPT-VAL-022</c>).</summary>
        public bool IsLocked { get; set; }
        public DateTime? LockedAt { get; set; }
        public Guid? LockedByOrderId { get; set; }
        public int CurrentVersionNo { get; set; }

        /// <summary>Kirim kembali sebagai <c>ExpectedVersion</c> saat revisi.</summary>
        public int Version { get; set; }

        public OptPrescriptionVersionResponse? Current { get; set; }

        /// <summary>Benar bila jawaban ini berasal dari kunci idempoten yang sudah dipakai (tombol Simpan ditekan dua kali). Tidak disimpan.</summary>
        public bool IsReplay { get; set; }

        /// <summary>Seluruh versi, terlama dulu. Hanya diisi pada detail.</summary>
        public List<OptPrescriptionVersionResponse> Versions { get; set; } = new();
    }

    /// <summary>Resep berlaku untuk dipilih di pesanan.</summary>
    public class OptPrescriptionOptionResponse
    {
        public Guid Id { get; set; }
        public string PrescriptionNumber { get; set; } = string.Empty;
        public OptPrescriptionType PrescriptionType { get; set; }
        public OptPrescriptionOrigin Origin { get; set; }
        public DateOnly IssuedDate { get; set; }
        public DateOnly ValidUntil { get; set; }
        public bool IsLocked { get; set; }
        public int CurrentVersionNo { get; set; }
        public string Summary { get; set; } = string.Empty;
    }
}
