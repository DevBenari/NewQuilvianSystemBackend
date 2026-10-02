using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>Penyaring daftar profil Mikrobiologi katalog.</summary>
    public class LabProcedureMicrobiologyProfilePagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public bool? IsActive { get; set; }

        /// <summary>Menyaring hanya yang memakai set bakteri, atau hanya yang tidak.</summary>
        public bool? UsesSusceptibilitySet { get; set; }

        /// <summary>Pencarian pada kode dan nama pemeriksaan.</summary>
        public string? Search { get; set; }
    }

    /// <summary>Memetakan satu pemeriksaan katalog.</summary>
    public class CreateLabProcedureMicrobiologyProfileRequest
    {
        public Guid ProcedureId { get; set; }

        public bool UsesSusceptibilitySet { get; set; } = true;

        public LabCultureType? DefaultCultureType { get; set; }

        public LabSusceptibilityMethod? DefaultSusceptibilityMethod { get; set; }
    }

    /// <summary>Mengubah pemetaan beserta status aktifnya.</summary>
    public class UpdateLabProcedureMicrobiologyProfileRequest
    {
        public bool UsesSusceptibilitySet { get; set; } = true;

        public LabCultureType? DefaultCultureType { get; set; }

        public LabSusceptibilityMethod? DefaultSusceptibilityMethod { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>Satu baris profil beserta identitas pemeriksaannya.</summary>
    public class LabProcedureMicrobiologyProfileResponse
    {
        public Guid Id { get; set; }

        public Guid ProcedureId { get; set; }

        public string? ProcedureCode { get; set; }

        public string? ProcedureName { get; set; }

        public bool UsesSusceptibilitySet { get; set; }

        public LabCultureType? DefaultCultureType { get; set; }

        public LabSusceptibilityMethod? DefaultSusceptibilityMethod { get; set; }

        public bool IsActive { get; set; }
    }
}
