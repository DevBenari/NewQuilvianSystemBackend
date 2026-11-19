namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs
{
    /// <summary>
    /// Filter daftar lintas pegawai K3RS (MSL-14, DEC-FER-05 opsi A, 18 Nov 2026).
    /// <c>Search</c> mencocokkan nama pegawai, kode profil, atau nomor dokumen; <c>Status</c> mencocokkan status dokumen persis.
    /// </summary>
    public class OccupationalHealthListQuery
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public bool? ActiveOnly { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>Baris daftar pembatasan kerja. Tanpa dasar klinis (hanya instruksi administratif).</summary>
    public class WorkRestrictionListItemDto
    {
        public Guid Id { get; set; }
        public string RestrictionNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        /// <summary>MstEmployee.Id untuk endpoint per pegawai (cek jadwal, ringkasan kelaikan).</summary>
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string RestrictionType { get; set; } = string.Empty;
        public string RestrictionStatus { get; set; } = string.Empty;
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? AdministrativeInstruction { get; set; }
        public int? MaximumWorkHoursPerDay { get; set; }
        public bool NoNightShift { get; set; }
        public bool NoOnCall { get; set; }
        public bool NoHeavyLifting { get; set; }
        public bool IsSchedulingBlocked { get; set; }
        public bool IsClinicalServiceBlocked { get; set; }
    }

    /// <summary>Baris ringkasan kelaikan kerja (fitness to work). Tanpa kolom klinis.</summary>
    public class FitnessSummaryListItemDto
    {
        public Guid Id { get; set; }
        public string AssessmentNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        /// <summary>MstEmployee.Id untuk endpoint per pegawai (ringkasan kelaikan, rekam klinis).</summary>
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string FitnessStatus { get; set; } = string.Empty;
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool WorkRestrictionRequired { get; set; }
        public bool IsSchedulingAllowed { get; set; }
        public bool IsClinicalDutyAllowed { get; set; }
        public DateTime? ReviewDate { get; set; }
    }

    /// <summary>Baris daftar kecelakaan kerja. Catatan klinis rahasia tidak ikut.</summary>
    public class InjuryListItemDto
    {
        public Guid Id { get; set; }
        public string InjuryNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public DateTime InjuryDateTime { get; set; }
        public string InjuryType { get; set; } = string.Empty;
        public string? BodyPart { get; set; }
        public string SeverityLevel { get; set; } = string.Empty;
        public bool IsLostTimeInjury { get; set; }
        public int LostWorkDays { get; set; }
        public bool IsReportableIncident { get; set; }
        public string? WorkCompensationClaimNumber { get; set; }
        public DateTime? ReturnToWorkDate { get; set; }
        public string InjuryStatus { get; set; } = string.Empty;
        /// <summary>Batas lapor BPJS JKK tahap I: waktu kejadian + 48 jam (VR-OHS-18, INT-HR-90).</summary>
        public DateTime BpjsStage1ReportDeadline { get; set; }
        /// <summary>True bila laporan dicatat sebelum batas 2×24 jam (rumus sama dengan OccupationalInjuryService).</summary>
        public bool BpjsReportedWithin48Hours { get; set; }
    }

    /// <summary>Baris daftar vaksinasi pegawai.</summary>
    public class VaccinationListItemDto
    {
        public Guid Id { get; set; }
        public string VaccinationNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string VaccineType { get; set; } = string.Empty;
        public string VaccineName { get; set; } = string.Empty;
        public int DoseNumber { get; set; }
        public DateTime? AdministrationDate { get; set; }
        public DateTime? NextDoseDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string VaccinationStatus { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public bool IsCompliant { get; set; }
        public bool IsVerified { get; set; }
    }
}
