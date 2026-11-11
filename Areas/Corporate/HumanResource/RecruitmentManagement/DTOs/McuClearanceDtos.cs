using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class RegisterMcuRequest
    {
        [Required(ErrorMessage = "ID Surat Penawaran Kerja (OfferId) wajib diisi.")]
        public Guid OfferId { get; set; }

        [Required(ErrorMessage = "Fasilitas/Klinik rujukan MCU wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama fasilitas/klinik maksimal 150 karakter.")]
        public string ClinicOrFacility { get; set; } = string.Empty;

        public DateTime? ScheduledCheckDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class RecordMcuResultRequest
    {
        [Required(ErrorMessage = "Hasil kelaikan kerja (ClearanceResult) wajib ditentukan.")]
        public McuClearanceResult ClearanceResult { get; set; }

        [Required(ErrorMessage = "Nama Dokter Pemeriksa K3RS wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama dokter pemeriksa maksimal 150 karakter.")]
        public string ExaminingDoctorName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tanggal pemeriksaan wajib diisi.")]
        public DateTime CheckDate { get; set; }

        [MaxLength(1000)]
        public string? DoctorNotes { get; set; }

        [MaxLength(500)]
        public string? CertificateFilePath { get; set; }
    }

    public sealed class McuClearanceResponse
    {
        public Guid Id { get; set; }
        public Guid OfferId { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateNik { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public string ReferralNumber { get; set; } = string.Empty;
        public DateTime ReferralDate { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ClinicOrFacility { get; set; } = string.Empty;
        public McuClearanceResult ClearanceResult { get; set; }
        public string ClearanceResultName => ClearanceResult.ToString();
        public string? DoctorNotes { get; set; }
        public bool IsDoctorNotesMasked { get; set; }
        public string? CertificateFilePath { get; set; }
        public string? ExaminingDoctorName { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class McuClearanceListItemResponse
    {
        public Guid Id { get; set; }
        public Guid OfferId { get; set; }
        public string OfferNumber { get; set; } = string.Empty;
        public string CandidateName { get; set; } = string.Empty;
        public string VacancyTitle { get; set; } = string.Empty;
        public string ReferralNumber { get; set; } = string.Empty;
        public DateTime ReferralDate { get; set; }
        public DateTime? CheckDate { get; set; }
        public string ClinicOrFacility { get; set; } = string.Empty;
        public McuClearanceResult ClearanceResult { get; set; }
        public string ClearanceResultName => ClearanceResult.ToString();
        public string? ExaminingDoctorName { get; set; }
    }

    public sealed class McuClearanceQuery
    {
        public Guid? VacancyId { get; set; }
        public McuClearanceResult? ClearanceResult { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
