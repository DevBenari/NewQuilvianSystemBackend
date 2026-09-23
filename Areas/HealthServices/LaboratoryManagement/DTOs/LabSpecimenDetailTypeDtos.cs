namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>Penyaring daftar Spesifik Specimen (<c>LAB-API-v1</c> <c>r28</c> bagian 23.3).</summary>
    public class LabSpecimenDetailTypePagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>Menyaring per kelompok induk.</summary>
        public Guid? LabSpecimenTypeId { get; set; }

        /// <summary>Menyaring per subjenis — untuk pelaporan.</summary>
        public string? SubTypeName { get; set; }

        /// <summary>
        /// Hanya baris yang <b>nama Indonesianya kosong</b>.
        ///
        /// <b>Bukan penyaring hiasan.</b> Tanpa ini, pekerjaan menerjemahkan 1.767 baris nol
        /// punya cara diketahui kemajuannya, dan ia akan berhenti di tengah tanpa ada yang
        /// menyadari (<c>LAB-DEC-131</c>).
        /// </summary>
        public bool? UntranslatedOnly { get; set; }

        /// <summary>Menyertakan baris nonaktif — ke-166 berkonfidensi rendah.</summary>
        public bool? IncludeInactive { get; set; }

        /// <summary>Pencarian pada nama Indonesia <b>dan</b> Inggris (<c>RULE-007</c>).</summary>
        public string? Search { get; set; }
    }

    /// <summary>Menambah Spesifik Specimen — hanya kepala instalasi (<c>LAB-DEC-098</c> butir 5).</summary>
    public class CreateLabSpecimenDetailTypeRequest
    {
        public Guid LabSpecimenTypeId { get; set; }

        public string? DetailTypeCode { get; set; }

        public string? DetailTypeNameId { get; set; }

        /// <summary><b>Wajib</b> — seluruh baris punya nama Inggris.</summary>
        public string? DetailTypeNameEn { get; set; }

        public string? SubTypeName { get; set; }

        /// <summary>Boleh kosong: baris lokal nol punya kode SNOMED.</summary>
        public string? SnomedCode { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>Mengubah Spesifik Specimen, termasuk mengisi terjemahan Indonesianya.</summary>
    public class UpdateLabSpecimenDetailTypeRequest
    {
        public string? DetailTypeNameId { get; set; }

        public string? DetailTypeNameEn { get; set; }

        public string? SubTypeName { get; set; }

        public string? SnomedCode { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>Satu baris Spesifik Specimen.</summary>
    public class LabSpecimenDetailTypeResponse
    {
        public Guid Id { get; set; }

        public Guid LabSpecimenTypeId { get; set; }

        public string? SpecimenTypeCode { get; set; }

        public string? SpecimenTypeName { get; set; }

        public string DetailTypeCode { get; set; } = string.Empty;

        public string? DetailTypeNameId { get; set; }

        public string DetailTypeNameEn { get; set; } = string.Empty;

        /// <summary>Nama yang ditampilkan: Indonesia bila ada, Inggris bila belum.</summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>Benar ketika nama Indonesianya masih kosong.</summary>
        public bool IsUntranslated { get; set; }

        public string? SubTypeName { get; set; }

        public string? SnomedCode { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }

    /// <summary>Pilihan ringkas untuk layar pengisian.</summary>
    public class LabSpecimenDetailTypeOptionResponse
    {
        public Guid Id { get; set; }

        public string DetailTypeCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? SubTypeName { get; set; }
    }
}
