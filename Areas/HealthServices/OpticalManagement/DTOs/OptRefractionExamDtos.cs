using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs
{
    // OPT-API-v1 grup "Health Services / Optical Management / Opt Refraction Exam" (BE-OPT-011).
    // Seluruh nilai ukur bertanda Sensitif di kamus data: tidak pernah ditulis ke logger.

    /// <summary>Penyaring daftar pemeriksaan refraksi.</summary>
    public class OptRefractionExamListQuery
    {
        /// <summary>Kunjungan Optik tertentu.</summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Seluruh pemeriksaan pasien tertentu (riwayat).</summary>
        public Guid? PatientId { get; set; }

        /// <summary>Kosong = semua status.</summary>
        public OptRefractionExamStatus? Status { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Mulai pemeriksaan pada satu kunjungan Optik yang belum ditutup.</summary>
    public class OptRefractionExamStartRequest
    {
        [Required]
        public Guid? EncounterId { get; set; }
    }

    /// <summary>
    /// Satu set lengkap hasil ukur. Setiap simpan mengganti seluruh set, bukan sebagian: isian yang dikirim kosong
    /// tersimpan kosong.
    /// </summary>
    public class OptRefractionMeasurementValues
    {
        /// <summary>
        /// Visus ditulis notasi Snellen <c>n/n</c> (boleh berdesimal, contoh <c>6/12</c>, <c>6/7.5</c>, <c>1/60</c>,
        /// <c>1/300</c>), atau <c>1/~</c> (hanya persepsi cahaya), atau <c>0</c> (tanpa persepsi cahaya) —
        /// <c>OPT-VAL-034</c>, <c>OPT-DEC-059</c>. Simpan final butuh visus jauh mata kanan dan kiri, dengan atau
        /// tanpa koreksi (<c>OPT-VAL-031</c>).
        /// </summary>
        [MaxLength(10)]
        public string? VisusDistanceUncorrectedOd { get; set; }

        [MaxLength(10)]
        public string? VisusDistanceUncorrectedOs { get; set; }

        [MaxLength(10)]
        public string? VisusDistanceCorrectedOd { get; set; }

        [MaxLength(10)]
        public string? VisusDistanceCorrectedOs { get; set; }

        [MaxLength(10)]
        public string? VisusNearUncorrectedOd { get; set; }

        [MaxLength(10)]
        public string? VisusNearUncorrectedOs { get; set; }

        [MaxLength(10)]
        public string? VisusNearCorrectedOd { get; set; }

        [MaxLength(10)]
        public string? VisusNearCorrectedOs { get; set; }

        /// <summary>Autorefraktometer. CYL wajib notasi minus (≤ 0); AXIS 0–180 dan hanya bila ada silinder.</summary>
        public decimal? AutorefOdSph { get; set; }
        public decimal? AutorefOdCyl { get; set; }
        public int? AutorefOdAxis { get; set; }
        public decimal? AutorefOsSph { get; set; }
        public decimal? AutorefOsCyl { get; set; }
        public int? AutorefOsAxis { get; set; }

        /// <summary>Koreksi subjektif, aturan CYL/AXIS sama dengan autorefraktometer. ADD bernilai positif.</summary>
        public decimal? SubjectiveOdSph { get; set; }
        public decimal? SubjectiveOdCyl { get; set; }
        public int? SubjectiveOdAxis { get; set; }
        public decimal? SubjectiveOdAdd { get; set; }
        public decimal? SubjectiveOsSph { get; set; }
        public decimal? SubjectiveOsCyl { get; set; }
        public int? SubjectiveOsAxis { get; set; }
        public decimal? SubjectiveOsAdd { get; set; }

        /// <summary>Jarak pupil dalam mm, satu desimal.</summary>
        public decimal? PdDistance { get; set; }
        public decimal? PdNear { get; set; }

        [MaxLength(200)]
        public string? KeratometryNote { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }
    }

    /// <summary>Isi atau ubah hasil ukur selama pemeriksaan masih <c>Draft</c>.</summary>
    public class OptRefractionExamMeasurementRequest : OptRefractionMeasurementValues
    {
        /// <summary>Nilai <c>Version</c> terakhir yang dibaca; tertinggal → <c>409</c>.</summary>
        [Required]
        public int? ExpectedVersion { get; set; }
    }

    /// <summary>Simpan final. Hasil ukur yang dipakai adalah isian draf terakhir.</summary>
    public class OptRefractionExamFinalizeRequest
    {
        [Required]
        public int? ExpectedVersion { get; set; }
    }

    /// <summary>
    /// Revisi sesudah final (<c>OPT-DEC-047</c>): menambah versi baru berisi set hasil ukur lengkap. Versi lama tetap
    /// tersimpan, dan tagihan pemeriksaan tidak berubah.
    /// </summary>
    public class OptRefractionExamReviseRequest : OptRefractionMeasurementValues
    {
        [Required]
        public int? ExpectedVersion { get; set; }

        /// <summary>Wajib, contoh "salah ketik visus OS".</summary>
        [MaxLength(500)]
        public string? RevisionReason { get; set; }
    }

    /// <summary>Batalkan pemeriksaan <c>Draft</c>. Pemeriksaan final tidak dapat dibatalkan (<c>OPT-VAL-033</c>).</summary>
    public class OptRefractionExamCancelRequest
    {
        [Required]
        public int? ExpectedVersion { get; set; }

        /// <summary>Wajib, contoh "pasien menolak diperiksa".</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>Satu versi hasil ukur.</summary>
    public class OptRefractionExamRevisionResponse : OptRefractionMeasurementValues
    {
        public Guid Id { get; set; }
        public int RevisionNo { get; set; }
        public Guid RevisedByWorkforceProfileId { get; set; }
        public string RevisedByName { get; set; } = string.Empty;
        public DateTime RevisedAt { get; set; }
        public string? RevisionReason { get; set; }
    }

    /// <summary>Pemeriksaan refraksi beserta versi hasil ukur.</summary>
    public class OptRefractionExamResponse
    {
        public Guid Id { get; set; }
        public string ExamNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public Guid PatientId { get; set; }
        public Guid ExaminerWorkforceProfileId { get; set; }
        public string ExaminerName { get; set; } = string.Empty;
        public string ExaminerLicenseNumberSnapshot { get; set; } = string.Empty;
        public OptRefractionExamStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public int CurrentRevisionNo { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public string? CancelReason { get; set; }

        /// <summary>Kirim kembali sebagai <c>ExpectedVersion</c> pada tindakan berikutnya.</summary>
        public int Version { get; set; }

        /// <summary>Versi hasil ukur yang berlaku.</summary>
        public OptRefractionExamRevisionResponse? Current { get; set; }

        /// <summary>Seluruh versi, terlama dulu. Hanya diisi pada detail; kosong pada daftar.</summary>
        public List<OptRefractionExamRevisionResponse> Revisions { get; set; } = new();
    }
}
