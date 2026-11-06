using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs
{
    /// <summary>
    /// Parameter kueri halaman daftar kerja sinkronisasi kartu penjamin pasien (BE-FIN-139, FIN-API-1.12 V.3, FIN-DES-120).
    /// </summary>
    public sealed class EmployeeBenefitCardSyncPagedQuery
    {
        /// <summary>
        /// Status sinkronisasi (bisa satu atau beberapa dipisahkan koma, mis. "WAITING_PATIENT,ON_HOLD").
        /// Bawaan bila kosong: "WAITING_PATIENT,ON_HOLD" (FIN-DES-120, AC 1).
        /// </summary>
        [MaxLength(100)]
        public string? Status { get; set; }

        /// <summary>
        /// Kata kunci pencarian: nomor enrollment, nama staf / dokter, atau nomor pegawai / dokter.
        /// </summary>
        [MaxLength(100)]
        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 50)]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Baris item daftar kerja sinkronisasi kartu penjamin pasien untuk petugas Registrasi (FIN-API-1.12 V.3).
    /// TIDAK memuat field data pasien (seperti patientId, patientName, NIK, nomor RM) demi perlindungan privasi medis (AC 4, FIN-DEC-255).
    /// </summary>
    public sealed class EmployeeBenefitCardSyncListResponse
    {
        public Guid SyncId { get; set; }
        public Guid EnrollmentId { get; set; }
        public string EnrollmentNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerStaffNumber { get; set; } = string.Empty;
        public string OwnerType { get; set; } = string.Empty; // EMPLOYEE / DOCTOR
        public string PlanName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ReasonCode { get; set; }
        public string? ReasonLabel { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public DateTime? NextAttemptAt { get; set; }
        public DateTime? SyncedAt { get; set; }
    }

    /// <summary>
    /// Ringkasan jumlah baris sinkronisasi per status (FIN-API-1.12 V.3).
    /// </summary>
    public sealed class EmployeeBenefitCardSyncStatusCountResponse
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    /// <summary>
    /// Respons ringkasan jumlah status sinkronisasi kartu (GET /summary, FIN-API-1.12 V.3).
    /// </summary>
    public sealed class EmployeeBenefitCardSyncSummaryResponse
    {
        public int Total { get; set; }
        public List<EmployeeBenefitCardSyncStatusCountResponse> ByStatus { get; set; } = new();
    }
}

