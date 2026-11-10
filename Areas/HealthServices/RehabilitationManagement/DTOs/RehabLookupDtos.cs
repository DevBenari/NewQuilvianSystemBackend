using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Rehab Lookup — contracts/api-contract.md v1, BE-RHB-006. Seluruhnya baca saja.
    // Rincian field tidak diatur kontrak (DEV_DISCRETION); dicatat di laporan task.
    // =====================================================================

    public class RehabPatientLookupQuery
    {
        /// <summary>Nama atau nomor rekam medis, minimal 3 karakter.</summary>
        public string? Search { get; set; }

        /// <summary>Banyaknya hasil, bawaan dan maksimum 20.</summary>
        public int PageSize { get; set; } = 20;
    }

    public class RehabPatientOption
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
    }

    public class RehabEncounterOption
    {
        public Guid Id { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public EncounterType EncounterType { get; set; }
        public string EncounterTypeName { get; set; } = string.Empty;
        public EncounterStatus EncounterStatus { get; set; }
        public DateTime EncounterDate { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
    }

    public class RehabReferralLetterOption
    {
        public Guid Id { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public string? DoctorName { get; set; }
        public Guid SourceEncounterId { get; set; }
        public string? SourceEncounterNumber { get; set; }
        public string? SourceClinicName { get; set; }
        public Guid? TargetClinicId { get; set; }
        public string? TargetName { get; set; }
    }

    public class RehabDoctorOption
    {
        public Guid Id { get; set; }
        public string DoctorCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfessionName { get; set; }
    }

    public class RehabDisciplineLookupOption
    {
        public Guid Id { get; set; }
        public string DisciplineCode { get; set; } = string.Empty;
        public string DisciplineName { get; set; } = string.Empty;
        public Guid ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
    }

    public class RehabProcedureOption
    {
        public Guid ProcedureId { get; set; }
        public string ProcedureCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
    }

    public class RehabInstrumentLookupQuery
    {
        public Guid? PatientId { get; set; }

        /// <summary>Kosong berarti instrumen asesmen medis Sp.KFR.</summary>
        public Guid? DisciplineId { get; set; }
    }

    /// <summary>
    /// Satu jenis instrumen yang dipetakan ke disiplin (atau asesmen medis), beserta versi yang
    /// berlaku bagi pasien. <see cref="Instrument"/> hanya terisi bila versinya sudah disahkan, atau
    /// konsep pada lingkungan uji yang mengizinkannya; selain itu layar mencatat pengukuran
    /// terstruktur saja (RHB-DEC-017).
    /// </summary>
    public class RehabResolvedInstrument
    {
        public ClinicalInstrumentKind InstrumentKind { get; set; }
        public string InstrumentKindName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? UnavailableReason { get; set; }
        public ResolvedInstrumentResponse? Instrument { get; set; }
    }
}
