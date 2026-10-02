using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>Penyaring daftar aturan kritis.</summary>
    public class LabMicrobiologyCriticalRulePagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public bool? IsActive { get; set; }

        public Guid? LabOrganismId { get; set; }

        public Guid? LabAntibioticId { get; set; }
    }

    /// <summary>
    /// Menambah aturan kritis (<c>LAB-API-v1</c> <c>r26</c> bagian 21.6).
    ///
    /// <b>Ketiga ruas penilainya boleh kosong, dan kosong berarti "apa saja".</b> Tetapi
    /// ketiganya <b>tidak boleh kosong bersamaan</b> (<c>VAL-106</c>).
    /// </summary>
    public class CreateLabMicrobiologyCriticalRuleRequest
    {
        public Guid? LabOrganismId { get; set; }

        public Guid? LabAntibioticId { get; set; }

        public LabSusceptibilityResult? SusceptibilityResult { get; set; }

        public string? RuleNote { get; set; }
    }

    /// <summary>Mengubah aturan kritis beserta status aktifnya.</summary>
    public class UpdateLabMicrobiologyCriticalRuleRequest
    {
        public Guid? LabOrganismId { get; set; }

        public Guid? LabAntibioticId { get; set; }

        public LabSusceptibilityResult? SusceptibilityResult { get; set; }

        public string? RuleNote { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>Satu baris aturan kritis beserta nama data induknya.</summary>
    public class LabMicrobiologyCriticalRuleResponse
    {
        public Guid Id { get; set; }

        public Guid? LabOrganismId { get; set; }

        /// <summary>Kosong berarti berlaku bagi <b>kuman apa saja</b>.</summary>
        public string? OrganismName { get; set; }

        public Guid? LabAntibioticId { get; set; }

        /// <summary>Kosong berarti berlaku bagi <b>antibiotik apa saja</b>.</summary>
        public string? AntibioticName { get; set; }

        public LabSusceptibilityResult? SusceptibilityResult { get; set; }

        public string? RuleNote { get; set; }

        /// <summary>Bunyi aturannya dalam satu kalimat yang dapat dibaca orang.</summary>
        public string RuleSummary { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
