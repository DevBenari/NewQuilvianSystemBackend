using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    [Table("MstProcedure", Schema = "public")]
    public class MstProcedure : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ProcedureCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ProcedureName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ProcedureGroupName { get; set; }

        [MaxLength(100)]
        public string? ProcedureCategoryName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProcedureType { get; set; } = "General";
        // General, Nursing, DoctorAction, Surgery, Laboratory, Radiology, Therapy, Other

        public bool IsDoctorAction { get; set; } = true;

        public bool IsNursingAction { get; set; } = false;

        public bool IsSurgery { get; set; } = false;

        public bool IsLaboratory { get; set; } = false;

        /// <summary>
        /// Disiplin laboratorium yang menaungi jenis pemeriksaan ini — Patologi Klinik,
        /// Patologi Anatomi, atau Mikrobiologi (<c>LAB-DEC-036</c>, <c>BE-EXT-01</c>).
        ///
        /// <b>Klasifikasi, bukan atribut operasional.</b> Kolom ini sejenis dengan
        /// <see cref="IsLaboratory"/> dan <see cref="IsRadiology"/> di atas: ia menggolongkan
        /// jenis tindakan. Satuan hasil, batas nilai, dan jenis wadah <b>tetap dilarang</b>
        /// masuk ke sini — seluruhnya berada di tabel milik Laboratorium.
        ///
        /// Hanya bermakna bila <see cref="IsLaboratory"/> bernilai benar. Boleh kosong karena
        /// tindakan non-laboratorium memang tidak punya disiplin, dan karena katalog yang sudah
        /// ada belum digolongkan.
        /// </summary>
        public LabDiscipline? LabDiscipline { get; set; }

        public bool IsRadiology { get; set; } = false;

        public bool IsTherapy { get; set; } = false;

        public bool IsNeedDoctor { get; set; } = true;

        public bool IsNeedApproval { get; set; } = false;

        public bool IsCoveredByInsuranceDefault { get; set; } = true;

        public bool IsAvailableForOutpatient { get; set; } = true;

        public bool IsAvailableForInpatient { get; set; } = true;

        public bool IsAvailableForEmergency { get; set; } = true;

        public int EstimatedDurationMinutes { get; set; } = 0;

        [MaxLength(50)]
        public string? ExternalProcedureCode { get; set; }

        [MaxLength(50)]
        public string? IntegrationCode { get; set; }

        public int SortOrder { get; set; } = 0;

        [MaxLength(500)]
        public string? ClinicalNoteTemplate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}