using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyVisitResponse
    {
        public Guid Id { get; set; }
        public string EmergencyVisitNumber { get; set; } = string.Empty;
        public Guid? EncounterId { get; set; }
        public Guid? PatientId { get; set; }

        /// <summary>
        /// Nama pasien untuk ditampilkan. Untuk pasien yang belum teridentifikasi, diisi
        /// alias sementara kunjungan supaya layar tidak pernah menampilkan kolom kosong.
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        public string? MedicalRecordNumber { get; set; }

        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid? ArrivalModeId { get; set; }
        public string? ArrivalModeName { get; set; }
        public Guid? CaseTypeId { get; set; }
        public string? CaseTypeName { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public EmergencyArrivalTimeSource ArrivalTimeSource { get; set; }
        public string? ArrivalConfirmedByName { get; set; }
        public DateTime? ArrivalConfirmedAt { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? ArrivalLocation { get; set; }
        public string? FoundLocation { get; set; }
        public string? TraumaLocation { get; set; }
        public DateTime? TraumaDateTime { get; set; }
        public bool IsUnknownPatient { get; set; }
        public string? TemporaryPatientAlias { get; set; }
        public bool IsImmediateCareAllowed { get; set; }
        public EmergencyRegistrationStatus RegistrationStatus { get; set; }
        public EmergencyVisitStatus VisitStatus { get; set; }
        public DateTime? RegistrationCompletedAt { get; set; }
        public Guid? RegistrationCompletedByUserId { get; set; }
        public DateTime? TreatmentStartedAt { get; set; }
        public DateTime? VisitCompletedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Terisi hanya bila kunjungan ini dibuat dengan menembus penjagaan satu pasien satu
        /// episode IGD aktif. Layar wajib menandainya, bukan menyembunyikannya.
        /// </summary>
        public string? DuplicateEpisodeOverrideReason { get; set; }

        public Guid? DuplicateEpisodeOverrideByUserId { get; set; }
        public DateTime? DuplicateEpisodeOverrideAt { get; set; }
        public Guid? DuplicateEpisodeOverrideOfVisitId { get; set; }

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>
    /// Hasil pra-cek episode IGD berjalan — <c>BE-IGD-050</c>, <c>IGD-DEC-138</c>. Dipanggil
    /// layar <b>sebelum</b> encounter dibuat, supaya penolakan episode ganda tidak lagi
    /// meninggalkan encounter tanpa kunjungan IGD. Baca-saja; tidak menulis apa pun.
    /// </summary>
    public class EmergencyActiveEpisodeResponse
    {
        /// <summary>Benar bila pasien masih punya kunjungan IGD yang episodenya berjalan.</summary>
        public bool HasActiveEpisode { get; set; }

        /// <summary>Kunjungan yang sudah ada; <c>null</c> bila <see cref="HasActiveEpisode"/> salah.</summary>
        public EmergencyActiveEpisodeVisitSummary? Visit { get; set; }
    }

    /// <summary>
    /// Ringkasan kunjungan yang menahan pendaftaran. Sengaja hanya tujuh ruas — cukup untuk
    /// menampilkan nomor dan status serta membuka kunjungannya, tanpa menyalin seluruh
    /// <see cref="EmergencyVisitResponse"/>.
    /// </summary>
    public class EmergencyActiveEpisodeVisitSummary
    {
        public Guid Id { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid PatientId { get; set; }

        /// <summary>
        /// Nama yang sama dengan <see cref="EmergencyVisitResponse.PatientName"/>: nama pasien,
        /// lalu alias sementara, lalu keterangan bawaan. Tidak pernah kosong.
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        public string EmergencyVisitNumber { get; set; } = string.Empty;
        public EmergencyVisitStatus VisitStatus { get; set; }
        public DateTime ArrivalDateTime { get; set; }
    }

    public class CreateEmergencyVisitRequest
    {
        [MaxLength(50)]
        public string? EmergencyVisitNumber { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ArrivalModeId { get; set; }

        public Guid? CaseTypeId { get; set; }

        public DateTime ArrivalDateTime { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(250)]
        public string? ArrivalLocation { get; set; }

        [MaxLength(250)]
        public string? FoundLocation { get; set; }

        [MaxLength(250)]
        public string? TraumaLocation { get; set; }

        public DateTime? TraumaDateTime { get; set; }

        public bool IsUnknownPatient { get; set; }

        [MaxLength(100)]
        public string? TemporaryPatientAlias { get; set; }

        public bool IsImmediateCareAllowed { get; set; }

        public EmergencyRegistrationStatus RegistrationStatus { get; set; } = EmergencyRegistrationStatus.Pending;

        public EmergencyVisitStatus VisitStatus { get; set; } = EmergencyVisitStatus.Arrived;

        public DateTime? RegistrationCompletedAt { get; set; }

        public Guid? RegistrationCompletedByUserId { get; set; }

        public DateTime? TreatmentStartedAt { get; set; }

        public DateTime? VisitCompletedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Alasan menembus penjagaan satu pasien satu episode IGD aktif — <c>IGD-DEC-084</c>.
        /// Diisi <b>hanya</b> ketika petugas sengaja mendaftarkan kunjungan kedua untuk pasien
        /// yang episode IGD sebelumnya belum ditutup, misalnya karena pasien benar-benar datang
        /// lagi dengan keluhan baru.
        /// </summary>
        [MaxLength(1000)]
        public string? DuplicateEpisodeOverrideReason { get; set; }
    }

    public class UpdateEmergencyVisitRequest : CreateEmergencyVisitRequest
    {
    }

    public class StartEmergencyVisitRequest
    {
        public Guid? EncounterId { get; set; }

        public string? Mode { get; set; }

        public DateTime? ArrivalDateTime { get; set; }

        public Guid? ArrivalModeId { get; set; }

        public Guid? CaseTypeId { get; set; }

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        public bool IsUnknownPatient { get; set; }

        [MaxLength(100)]
        public string? TemporaryPatientAlias { get; set; }
    }

    public class UpdateEmergencyVisitRegistrationStatusRequest
    {
        [Required]
        public EmergencyRegistrationStatus RegistrationStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Saringan daftar <i>Menunggu Triage</i> terpadu — <c>BE-IGD-054</c>, API <c>0.11.0</c> §8.3.1.
    /// </summary>
    /// <remarks>
    /// Memakai <c>page</c>, bukan <c>pageNumber</c> seperti endpoint lain modul ini, karena bentuk
    /// itulah yang dikunci kontrak §8.3.1 dan yang dibaca <c>FE-IGD-035</c>.
    /// </remarks>
    public class EmergencyTriageQueueQuery
    {
        /// <summary>Nomor halaman, mulai dari 1. Nilai di bawah 1 ditolak <c>400</c>.</summary>
        public int Page { get; set; } = 1;

        /// <summary>Jumlah baris per halaman, 1 sampai 100. Di luar rentang itu ditolak <c>400</c>.</summary>
        public int PageSize { get; set; } = 20;

        /// <summary>Nama pasien, nomor rekam medis, nomor encounter, atau nomor kunjungan.</summary>
        public string? Search { get; set; }

        /// <summary>
        /// <c>WaitingForTriage</c> menyaring baris tanpa kunjungan sekaligus kunjungan yang
        /// berstatus sama; nilai <see cref="EmergencyVisitStatus"/> lain menyaring baris
        /// kunjungan saja. Nilai yang tidak dikenal ditolak <c>400</c>.
        /// </summary>
        public string? QueueStatus { get; set; }
    }

    /// <summary>
    /// Satu baris daftar <i>Menunggu Triage</i> terpadu — <c>BE-IGD-054</c>, <c>FR-IGD-070</c>,
    /// keputusan <c>IGD-DEC-139</c> butir 2.
    /// </summary>
    /// <remarks>
    /// Dua asal baris memakai bentuk yang sama supaya layar tidak menggabungkan dua sumber
    /// sendiri: encounter IGD yang belum berakhir dan belum punya kunjungan, serta kunjungan
    /// yang episodenya masih terbuka. Asal baris tidak diekspos sebagai ruas tersendiri;
    /// yang membedakannya adalah <see cref="EmergencyVisitId"/>, <see cref="ArrivalDateTime"/>,
    /// dan <see cref="AvailableActions"/>.
    /// </remarks>
    public class EmergencyTriageQueueRowResponse
    {
        /// <summary>
        /// Kunci stabil baris untuk layar: <c>enc:{encounterId}</c> atau <c>visit:{visitId}</c>.
        /// </summary>
        public string RowKey { get; set; } = string.Empty;

        public Guid? EncounterId { get; set; }

        /// <summary>Kosong selama kunjungan belum lahir.</summary>
        public Guid? EmergencyVisitId { get; set; }

        public Guid? PatientId { get; set; }

        /// <summary>
        /// Nama pasien; untuk rekam pengganti diisi alias sementara apa adanya
        /// (<c>IGD-DEC-151</c>), dan tidak pernah kosong.
        /// </summary>
        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        /// <summary>Selalu <c>false</c> untuk baris tanpa kunjungan.</summary>
        public bool IsUnknownPatient { get; set; }

        public string? TemporaryPatientAlias { get; set; }

        public string? EncounterNumber { get; set; }

        public string? EmergencyVisitNumber { get; set; }

        /// <summary>
        /// <c>WaitingForTriage</c> untuk baris tanpa kunjungan; nama
        /// <see cref="EmergencyVisitStatus"/> untuk baris kunjungan.
        /// </summary>
        public string QueueStatus { get; set; } = string.Empty;

        /// <summary>Kosong selama kunjungan belum lahir.</summary>
        public EmergencyVisitStatus? VisitStatus { get; set; }

        /// <summary>
        /// Waktu terdaftar. Layar menampilkannya berlabel <b>Terdaftar</b> dan tidak pernah
        /// sebagai waktu tiba.
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>Hanya terisi untuk baris kunjungan.</summary>
        public DateTime? ArrivalDateTime { get; set; }

        /// <summary>
        /// Baris tanpa kunjungan: <c>StartTriage</c>, <c>ImmediateCare</c>, <c>NoShow</c>.
        /// Baris kunjungan <c>Arrived</c>/<c>WaitingForTriage</c>: <c>FillTriage</c>,
        /// <c>ImmediateCare</c> (<c>IGD-DEC-128</c>). Status kunjungan lain: kosong.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class UpdateEmergencyVisitVisitStatusRequest
    {
        [Required]
        public EmergencyVisitStatus VisitStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Permintaan menyelesaikan kunjungan secara klinis. Waktu selesai tidak diterima dari
    /// pemanggil melainkan diisi waktu server, supaya penutupan tidak dapat dimundurkan.
    /// </summary>
    public class CompleteVisitRequest
    {
        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
