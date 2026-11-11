using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs
{
    /// <summary>
    /// DTO Rapor Komposit 3 Tahunan Rekredensialing & Klasifikasi Tier Kelayakan (INT-HR-75, VR-OPP-15..18)
    /// </summary>
    public class RecredentialingScorecardDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorCode { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        /// <summary>
        /// Skor OPPE Tahun ke-1 (Bobot 25%)
        /// </summary>
        public double Year1Score { get; set; }

        /// <summary>
        /// Skor OPPE Tahun ke-2 (Bobot 35%)
        /// </summary>
        public double Year2Score { get; set; }

        /// <summary>
        /// Skor OPPE Tahun ke-3 (Bobot 40%)
        /// </summary>
        public double Year3Score { get; set; }

        /// <summary>
        /// Formula Deterministik 25%-35%-40%: (0.25 * Y1) + (0.35 * Y2) + (0.40 * Y3)
        /// </summary>
        public double FinalCompositeScore { get; set; }

        /// <summary>
        /// <c>true</c> bila ketiga skor tahunan tersedia (dari riwayat OPPE atau masukan simulasi). Bila <c>false</c>,
        /// skor komposit dan tier belum dapat dihitung; tahun yang kosong tidak diisi angka karangan (remediasi 10 November 2026).
        /// </summary>
        public bool IsDataSufficient { get; set; } = true;

        /// <summary>Jumlah rapor tahunan yang tersedia, 0 s.d. 3.</summary>
        public int AvailableEvaluationYears { get; set; } = 3;

        /// <summary>Nomor tahun (1, 2, 3) yang skornya belum tersedia.</summary>
        public List<int> MissingEvaluationYears { get; set; } = new();

        /// <summary>Penjelasan untuk pengguna bila data belum cukup; <c>null</c> bila lengkap.</summary>
        public string? DataSufficiencyMessage { get; set; }

        /// <summary>
        /// GreenFastTrack, YellowClarificationReview, RedPlenaryInvestigation, atau InsufficientData bila riwayat OPPE belum cukup
        /// </summary>
        public string CalculatedTier { get; set; } = "GreenFastTrack";

        /// <summary>
        /// Tier hasil penetapan aktif (bisa sama dengan CalculatedTier atau hasil diskresi override)
        /// </summary>
        public string AssignedTier { get; set; } = "GreenFastTrack";

        public string TierDescription => AssignedTier switch
        {
            "GreenFastTrack" => "Tier Hijau - Rekomendasi Jalur Cepat (Fast-Track) Perpanjangan SPK Tanpa Hambatan",
            "YellowClarificationReview" => "Tier Kuning - Wajib Wawancara Klarifikasi KSM & Rencana Pembinaan",
            "RedPlenaryInvestigation" => "Tier Merah - Wajib Sidang Khusus Pleno Komite Terkait Pembatasan/Pencabutan Wewenang",
            "InsufficientData" => "Data Belum Cukup - Rapor OPPE 3 tahun belum lengkap, tier belum dapat ditetapkan",
            _ => AssignedTier
        };

        public bool IsOverridden { get; set; } = false;
        public string? TierOverrideReason { get; set; }
        public Guid? OverriddenBy { get; set; }
        public string? OverriddenByName { get; set; }
        public DateTime? OverriddenAt { get; set; }

        public bool HasSentinelIncident { get; set; } = false;
        public bool HasEmrDelinquency { get; set; } = false;
        public string? FppeHistorySummary { get; set; }

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO Request Simulasi / Kalkulasi Manual Rapor Komposit 3 Tahun
    /// </summary>
    public class CalculateRecredScorecardRequestDto
    {
        [Range(0, 100, ErrorMessage = "Skor Tahun ke-1 wajib bernilai 0 s.d. 100.")]
        public double? Year1Score { get; set; }

        [Range(0, 100, ErrorMessage = "Skor Tahun ke-2 wajib bernilai 0 s.d. 100.")]
        public double? Year2Score { get; set; }

        [Range(0, 100, ErrorMessage = "Skor Tahun ke-3 wajib bernilai 0 s.d. 100.")]
        public double? Year3Score { get; set; }

        public bool SimulateSentinelIncident { get; set; } = false;
        public bool SimulateEmrDelinquency { get; set; } = false;
    }

    /// <summary>
    /// DTO Request Penyesuaian Diskresi Tier oleh Ketua Komite Medik (VR-OPP-19)
    /// </summary>
    public class OverrideRecredTierRequestDto
    {
        /// <summary>
        /// GreenFastTrack, YellowClarificationReview, RedPlenaryInvestigation
        /// </summary>
        [Required(ErrorMessage = "Tier baru wajib ditentukan.")]
        public string NewTier { get; set; } = "GreenFastTrack";

        [Required(ErrorMessage = "Nota justifikasi klinis tertulis wajib diisi.")]
        [MinLength(20, ErrorMessage = "Dasar justifikasi klinis tertulis minimal 20 karakter (VR-OPP-19).")]
        public string JustificationReason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ketua Komite Medik pengesah diskresi wajib diisi.")]
        public Guid ApprovedByCommitteeChairmanId { get; set; }
    }

    /// <summary>
    /// DTO Request Penerbitan 4 Putusan Sah Sidang Rekredensialing (VR-OPP-20)
    /// </summary>
    public class CreateRecredVerdictRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId dokter target wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        /// <summary>
        /// 4 Putusan Baku: RenewAsIs, Upgrade, Restricted, Revoked
        /// </summary>
        [Required(ErrorMessage = "Putusan sidang rekredensialing wajib ditentukan (VR-OPP-20).")]
        public string Verdict { get; set; } = "RenewAsIs";

        [Required(ErrorMessage = "Catatan berita acara sidang pleno Komite Medik wajib diisi.")]
        [MinLength(15, ErrorMessage = "Catatan sidang minimal 15 karakter.")]
        public string CommitteeNotes { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ketua Komite Medik pengesah putusan wajib diisi.")]
        public Guid ApprovedByCommitteeChairmanId { get; set; }
    }

    /// <summary>
    /// DTO Response Berita Acara Putusan Sidang Rekredensialing
    /// </summary>
    public class RecredVerdictResponseDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorCode { get; set; } = string.Empty;
        public string Verdict { get; set; } = "RenewAsIs";
        public string VerdictDescription => Verdict switch
        {
            "RenewAsIs" => "Perpanjang As-Is: Kewenangan Klinis Mandiri Level 4 Diperpanjang Penuh",
            "Upgrade" => "Upgrade: Peningkatan Rincian Kewenangan Klinis Mandiri Level 4",
            "Restricted" => "Restricted: Pembatasan Wewenang Klinis Menjadi Level 3 Supervisi (Wajib Co-Signature 24 Jam)",
            "Revoked" => "Revoked: Pencabutan Kewenangan Klinis Mutlak & Pemblokiran Hak Order SIMRS (Hard-Stop)",
            _ => Verdict
        };
        public string CommitteeNotes { get; set; } = string.Empty;
        public Guid ApprovedByCommitteeChairmanId { get; set; }
        public DateTime DecidedAt { get; set; }
        public string SignatureHash { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO Request Transmisi Sinkronisasi Wewenang ke Modul EMR SIMRS (INT-HR-76, VR-OPP-25)
    /// </summary>
    public class DispatchSimrsPrivilegeRequestDto
    {
        [Required(ErrorMessage = "VerdictId wajib disertakan.")]
        public Guid VerdictId { get; set; }

        [Required(ErrorMessage = "Kode dokter SIMRS wajib diisi.")]
        [MaxLength(50)]
        public string DoctorCode { get; set; } = string.Empty;

        public Guid? TindakanId { get; set; }

        /// <summary>
        /// Level 1..4
        /// </summary>
        [Range(1, 4, ErrorMessage = "Level kewenangan SIMRS wajib antara 1 s.d. 4.")]
        public int Level { get; set; } = 4;

        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO Audit Log Transmisi Sinkronisasi SIMRS EMR
    /// </summary>
    public class SimrsPrivilegeSyncAuditLogDto
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid VerdictId { get; set; }
        public string DoctorCode { get; set; } = string.Empty;
        public int TargetLevel { get; set; }
        public bool IsHardStopBlocked { get; set; }
        public string PayloadJson { get; set; } = string.Empty;
        public int ResponseStatusCode { get; set; } = 200;
        public long LatencyMs { get; set; } = 45;
        public DateTime DispatchedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "SUCCESS";
        public string Message { get; set; } = string.Empty;
    }
}
