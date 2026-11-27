using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    // LAB-API-v1 r51 bagian 46.3 — setelan penyerahan hasil kepada pasien (BE-LAB-104). DTO dokumen,
    // ringkasan, dan catatan kirim (46.2) menyusul di berkas yang sama pada BE-LAB-105..107 dan BE-LAB-41.

    /// <summary>Perubahan aktivasi penyerahan hasil untuk satu disiplin (jenis <i>Status</i>).</summary>
    public class ChangeLabPatientDeliveryActivationRequest
    {
        /// <summary>Keadaan yang diminta; harus berbeda dari keadaan kini (<c>VAL-178</c>).</summary>
        public bool Enabled { get; set; }

        /// <summary>Alasan perubahan, wajib (penjaga layanan, pola <c>CancelLabOrderRequest</c>).</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }

        /// <summary>Wajib <c>true</c> saat mengaktifkan: daftar pemeriksaan sensitif sudah ditetapkan, termasuk bila kosong.</summary>
        public bool SensitiveListConfirmed { get; set; }
    }

    /// <summary>Menandai satu jenis pemeriksaan sebagai sensitif (jenis <i>Create</i>).</summary>
    public class AddLabSensitiveProcedureRequest
    {
        [Required]
        public Guid ProcedureId { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    /// <summary>Mencabut penanda sensitif (jenis <i>Status</i>).</summary>
    public class RemoveLabSensitiveProcedureRequest
    {
        /// <summary>Alasan pencabutan, wajib (penjaga layanan).</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>Keadaan setelan penyerahan hasil: aktivasi PK dan Mikrobiologi beserta daftar sensitif.</summary>
    public class LabPatientDeliverySettingResponse
    {
        public List<LabPatientDeliveryDisciplineResponse> Disciplines { get; set; } = new();

        public List<LabSensitiveProcedureResponse> SensitiveProcedures { get; set; } = new();

        /// <summary>
        /// <c>LabPatientResult:VerificationBaseUrl</c> sudah diisi. Selama <c>false</c>, aktivasi ditolak
        /// (<c>VAL-178</c>, 34.10 no. 12).
        /// </summary>
        public bool VerificationBaseUrlConfigured { get; set; }
    }

    /// <summary>Keadaan aktivasi satu disiplin; perubahan terakhir kosong bila belum pernah diubah.</summary>
    public class LabPatientDeliveryDisciplineResponse
    {
        /// <summary>Nama enum, mis. <c>ClinicalPathology</c>.</summary>
        public string Discipline { get; set; } = string.Empty;

        public string DisciplineName { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        public DateTime? ChangedAt { get; set; }

        public string? ChangedByName { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>Satu baris jejak aktivasi.</summary>
    public class LabPatientDeliveryActivationResponse
    {
        public Guid Id { get; set; }

        public string Discipline { get; set; } = string.Empty;

        public bool Enabled { get; set; }

        public string Reason { get; set; } = string.Empty;

        public bool SensitiveListConfirmed { get; set; }

        public Guid ChangedByUserId { get; set; }

        public string? ChangedByName { get; set; }

        public DateTime ChangedAt { get; set; }
    }

    /// <summary>Satu jenis pemeriksaan yang ditandai sensitif.</summary>
    public class LabSensitiveProcedureResponse
    {
        public Guid Id { get; set; }

        public Guid ProcedureId { get; set; }

        public string ProcedureCode { get; set; } = string.Empty;

        public string ProcedureName { get; set; } = string.Empty;

        public string? Note { get; set; }

        public string? MarkedByName { get; set; }

        public DateTime MarkedAt { get; set; }
    }
}
