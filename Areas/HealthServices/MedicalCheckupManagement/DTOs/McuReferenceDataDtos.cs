using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs
{
    // =====================================================================
    // MCU-API-v1 — grup Mcu Reference Data (BE-MCU-006)
    // =====================================================================

    public class McuExaminationTypePagedQuery
    {
        /// <summary>Mencari kode atau nama jenis pemeriksaan.</summary>
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class McuExaminationTypeResponse
    {
        public Guid Id { get; set; }
        public string ExaminationTypeCode { get; set; } = string.Empty;
        public string ExaminationTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int FieldCount { get; set; }
    }

    public class McuExaminationTypeDetailResponse : McuExaminationTypeResponse
    {
        /// <summary>Urut nomor butir formulir.</summary>
        public List<McuExaminationFieldResponse> Fields { get; set; } = new();
    }

    public class McuExaminationFieldResponse
    {
        public Guid Id { get; set; }
        public int FieldNumber { get; set; }
        public string FieldCode { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public McuExaminationFieldType FieldType { get; set; }
        public string FieldTypeLabel { get; set; } = string.Empty;
        public string? UnitName { get; set; }

        /// <summary>Bentuk tersimpan, dipisah <c>|</c>.</summary>
        public string? OptionValues { get; set; }

        /// <summary><see cref="OptionValues"/> yang sudah dipecah, siap untuk pilihan di layar.</summary>
        public List<string> Options { get; set; } = new();
        public bool IsRequired { get; set; }
    }

    public class CreateMcuExaminationTypeRequest
    {
        /// <summary>Huruf besar tanpa spasi, contoh <c>VISUS</c>; disimpan dalam huruf besar.</summary>
        public string ExaminationTypeCode { get; set; } = string.Empty;
        public string ExaminationTypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<McuExaminationFieldRequest> Fields { get; set; } = new();
    }

    /// <summary>
    /// Isi jenis pemeriksaan lengkap. Isian yang tidak dikirim lagi dihapus dari formulir; hasil
    /// pemeriksaan yang sudah memakainya tetap terbaca karena penghapusannya berupa penandaan.
    /// </summary>
    public class UpdateMcuExaminationTypeRequest : CreateMcuExaminationTypeRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class McuExaminationFieldRequest
    {
        /// <summary>Isian tersimpan yang dipertahankan. Kosong = dipasangkan menurut kode isian, atau isian baru.</summary>
        public Guid? Id { get; set; }

        /// <summary>Nomor butir pada formulir; ≥ 1, unik per jenis.</summary>
        public int FieldNumber { get; set; }

        /// <summary>Contoh <c>TEKANAN_SISTOLIK</c>; disimpan dalam huruf besar.</summary>
        public string FieldCode { get; set; } = string.Empty;
        public string FieldLabel { get; set; } = string.Empty;
        public McuExaminationFieldType FieldType { get; set; }
        public string? UnitName { get; set; }

        /// <summary>Wajib untuk isian Pilihan (<c>VAL-MCU-09</c>); dipisah <c>|</c>, contoh <c>Normal|Parsial|Total</c>.</summary>
        public string? OptionValues { get; set; }
        public bool IsRequired { get; set; }
    }

    public class McuFitnessCategoryQuery
    {
        public bool? IsActive { get; set; }
    }

    public class McuFitnessCategoryResponse
    {
        public Guid Id { get; set; }
        public string FitnessCategoryCode { get; set; } = string.Empty;
        public string FitnessCategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateMcuFitnessCategoryRequest
    {
        /// <summary>Huruf besar tanpa spasi, contoh <c>UNFIT_SEMENTARA</c>; disimpan dalam huruf besar.</summary>
        public string FitnessCategoryCode { get; set; } = string.Empty;
        public string FitnessCategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    /// <summary>Kategori dinonaktifkan lewat <see cref="IsActive"/>; tidak pernah dihapus.</summary>
    public class UpdateMcuFitnessCategoryRequest : CreateMcuFitnessCategoryRequest
    {
        public bool IsActive { get; set; } = true;
    }
}
