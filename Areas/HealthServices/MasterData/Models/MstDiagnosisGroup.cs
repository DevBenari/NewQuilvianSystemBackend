using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Kelompok ICD Diagnosa — Daftar Tabulasi Dasar (DTD) — RJ-DOC-REV-BE-006.
    /// Satu kelompok menaungi satu atau beberapa rentang kode ICD-10, contoh
    /// <c>001.0 Kolera (A00)</c>, <c>087.0 Leukemia (C91 - C95)</c>. Dipakai mengelompokkan
    /// hasil pencarian diagnosa ICD-10 pada SOAP dokter.
    /// </summary>
    public class MstDiagnosisGroup : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>ID kelompok pada sumber data rumah sakit (<c>icddid</c>), kunci impor ulang.</summary>
        public string SourceCode { get; set; } = string.Empty;

        /// <summary>Nomor DTD, contoh <c>001.0</c>.</summary>
        public string DtdNumber { get; set; } = string.Empty;

        /// <summary>Rentang kode ICD-10 terperinci apa adanya dari sumber, contoh <c>C 91 - C 95</c>.</summary>
        public string? CodeRangeText { get; set; }

        public string GroupName { get; set; } = string.Empty;

        public bool IsImmunization { get; set; }

        public bool IsAccidentCause { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MstDiagnosis> Diagnoses { get; set; } = new List<MstDiagnosis>();
    }
}
