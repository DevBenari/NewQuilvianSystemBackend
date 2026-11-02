using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models
{
    /// <summary>
    /// Hasil penilaian kelengkapan terakhir satu kunjungan (`AGG-RM-03`, `RM-DEC-045`–`046`, `BE-39`).
    ///
    /// Kesimpulan rekam medis sendiri, BUKAN salinan isi dokumen modul lain (`RM-DEC-038`).
    /// Disimpan supaya daftar kerja dapat disaring dan diurutkan tanpa memanggil belasan sumber
    /// untuk setiap kunjungan; dinilai ulang pada pemicu tertentu dan pemeriksaan berkala.
    /// </summary>
    [Table("MrcEncounterCompletenessAssessment", Schema = "public")]
    public class MrcEncounterCompletenessAssessment : IdentityModel
    {
        public Guid Id { get; set; }

        /// <summary>Unik — satu kunjungan, satu penilaian.</summary>
        public Guid EncounterId { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>Unik — penilaian hanya ada sesudah tutup klinis.</summary>
        public Guid ClinicalClosureId { get; set; }

        public CompletenessState State { get; set; }

        public int DeficiencyCount { get; set; }

        /// <summary>
        /// Larik kekurangan dalam JSON. Memuat id penulis — SENSITIF, tidak boleh masuk payload
        /// logger. Tidak memuat isi klinis.
        /// </summary>
        public string Deficiencies { get; set; } = "[]";

        public DateTime EvaluatedAt { get; set; }

        /// <summary>Sidik profil yang dipakai, untuk mengenali penilaian beraturan lama.</summary>
        public string ProfileFingerprint { get; set; } = string.Empty;

        /// <summary>Diisi sekali saat pertama kali lengkap; tidak dihapus bila keadaan turun.</summary>
        public DateTime? FirstCompletedAt { get; set; }

        public DateTime? NextCheckAt { get; set; }

        /// <summary>True bila kunjungan kemudian batal; baris tidak dihapus.</summary>
        public bool IsExcluded { get; set; }

        /// <summary>Concurrency token; naik setiap simpan (`NFR-003`).</summary>
        public int Version { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public MrcEncounterClinicalClosure? ClinicalClosure { get; set; }
    }
}
