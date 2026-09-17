using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    public class PrescriptionTemplateResponse
    {
        public Guid Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string? TemplateCategory { get; set; }
        public string? Description { get; set; }
        public Guid OwnerDoctorId { get; set; }
        public string OwnerDoctorName { get; set; } = string.Empty;
        public bool IsShared { get; set; }
        public bool IsFavorite { get; set; }
        public int UsageCount { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public int RegularItemCount { get; set; }
        public int CompoundCount { get; set; }
        public int CompoundIngredientCount { get; set; }
        public int TotalItemCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PrescriptionTemplateDetailResponse : PrescriptionTemplateResponse
    {
        public List<PrescriptionTemplateItemResponse> Items { get; set; } = new();
        public List<PrescriptionTemplateCompoundResponse> Compounds { get; set; } = new();
    }

    public class PrescriptionTemplateItemResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? DrugForm { get; set; }
        public decimal Dose { get; set; }
        public Guid? DoseUnitMeasurementId { get; set; }
        public string? DoseUnitName { get; set; }
        public string? FrequencyCode { get; set; }
        public string? FrequencyText { get; set; }
        public decimal? FrequencyPerDay { get; set; }
        public decimal? DurationValue { get; set; }
        public string? DurationUnit { get; set; }
        public bool IsAsNeeded { get; set; }
        public string? AdministrationTime { get; set; }
        public string? Signa { get; set; }
        public string? AdministrationInstruction { get; set; }
        public string? DoctorNote { get; set; }
        public decimal Quantity { get; set; }
        public Guid? DispenseUnitMeasurementId { get; set; }
        public string? DispenseUnitName { get; set; }
        public int SortOrder { get; set; }
    }

    public class PrescriptionTemplateCompoundResponse
    {
        public Guid Id { get; set; }
        public string CompoundName { get; set; } = string.Empty;
        public string? CompoundForm { get; set; }
        public decimal TotalPackage { get; set; }
        public Guid? PackageUnitMeasurementId { get; set; }
        public string? PackageUnitName { get; set; }
        public decimal DosePerUse { get; set; }
        public Guid? DoseUnitMeasurementId { get; set; }
        public string? DoseUnitName { get; set; }
        public string? FrequencyCode { get; set; }
        public string? FrequencyText { get; set; }
        public decimal? FrequencyPerDay { get; set; }
        public decimal? DurationValue { get; set; }
        public string? DurationUnit { get; set; }
        public bool IsAsNeeded { get; set; }
        public string? AdministrationTime { get; set; }
        public string? Signa { get; set; }
        public string? CompoundingInstruction { get; set; }
        public string? AdministrationInstruction { get; set; }
        public string? DoctorNote { get; set; }
        public int SortOrder { get; set; }
        public List<PrescriptionTemplateCompoundItemResponse> Items { get; set; } = new();
    }

    public class PrescriptionTemplateCompoundItemResponse
    {
        public Guid Id { get; set; }
        public Guid DrugId { get; set; }
        public string DrugCode { get; set; } = string.Empty;
        public string DrugName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public decimal AmountPerPackage { get; set; }
        public decimal TotalQuantity { get; set; }
        public Guid? QuantityUnitMeasurementId { get; set; }
        public string? QuantityUnitName { get; set; }
        public string? IngredientInstruction { get; set; }
        public int SortOrder { get; set; }
    }

    public class PrescriptionTemplateFilterMetadataResponse
    {
        public PrescriptionTemplateDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PrescriptionTemplateSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class PrescriptionTemplateDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? OwnerDoctorId { get; set; }
        public string? TemplateCategory { get; set; }
        public bool? IsShared { get; set; }
        public bool? IsFavorite { get; set; }
        public bool? IsActive { get; set; } = true;
        public string SortBy { get; set; } = "templateName";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PrescriptionTemplateSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePrescriptionTemplateRequest
    {
        [Required, MaxLength(200)] public string TemplateName { get; set; } = string.Empty;
        [MaxLength(100)] public string? TemplateCategory { get; set; }
        [MaxLength(500)] public string? Description { get; set; }

        /// <summary>
        /// BE-RWI-105 / FR-DOK-088. Pemilik selalu dokter akun login. Boleh kosong; bila diisi dengan
        /// dokter lain, permintaan ditolak 403 tanpa menyimpan apa pun.
        /// </summary>
        public Guid OwnerDoctorId { get; set; }

        public bool IsShared { get; set; }
        public bool IsFavorite { get; set; }

        /// <summary>
        /// BE-RWI-105 / RWI-DEC-135 butir (3). Isi <c>Inpatient</c> dari ruang kerja rawat inap: template
        /// tersimpan pribadi (<c>IsShared</c> dipaksa <c>false</c>). Kosong untuk poliklinik dan Farmasi.
        /// </summary>
        [MaxLength(30)] public string? ServiceContext { get; set; }

        public List<PrescriptionTemplateItemRequest> Items { get; set; } = new();
        public List<PrescriptionTemplateCompoundRequest> Compounds { get; set; } = new();
    }

    public class UpdatePrescriptionTemplateRequest : CreatePrescriptionTemplateRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class PrescriptionTemplateItemRequest
    {
        [Required] public Guid DrugId { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal Dose { get; set; } = 1;
        public Guid? DoseUnitMeasurementId { get; set; }
        [MaxLength(50)] public string? FrequencyCode { get; set; }
        [MaxLength(150)] public string? FrequencyText { get; set; }
        public decimal? FrequencyPerDay { get; set; }
        public decimal? DurationValue { get; set; }
        [MaxLength(30)] public string? DurationUnit { get; set; }
        public bool IsAsNeeded { get; set; }
        [MaxLength(250)] public string? AdministrationTime { get; set; }
        [MaxLength(500)] public string? Signa { get; set; }
        [MaxLength(500)] public string? AdministrationInstruction { get; set; }
        [MaxLength(500)] public string? DoctorNote { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal Quantity { get; set; } = 1;
        public Guid? DispenseUnitMeasurementId { get; set; }
        public int SortOrder { get; set; }
    }

    public class PrescriptionTemplateCompoundRequest
    {
        [Required, MaxLength(200)] public string CompoundName { get; set; } = string.Empty;
        [MaxLength(100)] public string? CompoundForm { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal TotalPackage { get; set; } = 1;
        public Guid? PackageUnitMeasurementId { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal DosePerUse { get; set; } = 1;
        public Guid? DoseUnitMeasurementId { get; set; }
        [MaxLength(50)] public string? FrequencyCode { get; set; }
        [MaxLength(150)] public string? FrequencyText { get; set; }
        public decimal? FrequencyPerDay { get; set; }
        public decimal? DurationValue { get; set; }
        [MaxLength(30)] public string? DurationUnit { get; set; }
        public bool IsAsNeeded { get; set; }
        [MaxLength(250)] public string? AdministrationTime { get; set; }
        [MaxLength(500)] public string? Signa { get; set; }
        [MaxLength(1000)] public string? CompoundingInstruction { get; set; }
        [MaxLength(500)] public string? AdministrationInstruction { get; set; }
        [MaxLength(500)] public string? DoctorNote { get; set; }
        public int SortOrder { get; set; }
        public List<PrescriptionTemplateCompoundItemRequest> Items { get; set; } = new();
    }

    public class PrescriptionTemplateCompoundItemRequest
    {
        [Required] public Guid DrugId { get; set; }
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal AmountPerPackage { get; set; } = 1;
        [Range(typeof(decimal), "0.0001", "999999999")] public decimal TotalQuantity { get; set; } = 1;
        public Guid? QuantityUnitMeasurementId { get; set; }
        [MaxLength(500)] public string? IngredientInstruction { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateTemplateFromPrescriptionRequest
    {
        [Required] public Guid PrescriptionId { get; set; }
        [Required, MaxLength(200)] public string TemplateName { get; set; } = string.Empty;
        [MaxLength(100)] public string? TemplateCategory { get; set; }
        [MaxLength(500)] public string? Description { get; set; }
        public bool IsShared { get; set; }
        public bool IsFavorite { get; set; }

        /// <summary>BE-RWI-105. Resep berkonteks rawat inap selalu menghasilkan template pribadi.</summary>
        [MaxLength(30)] public string? ServiceContext { get; set; }
    }

    public class ApplyPrescriptionTemplateRequest
    {
        [Required] public Guid PrescriptionId { get; set; }
        public bool ReplaceExisting { get; set; }
    }

    public class ApplyPrescriptionTemplateResponse
    {
        public Guid TemplateId { get; set; }
        public Guid PrescriptionId { get; set; }
        public int AddedRegularItemCount { get; set; }
        public int AddedCompoundCount { get; set; }
        public int AddedCompoundIngredientCount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal CoveredAmount { get; set; }
        public decimal PatientPayAmount { get; set; }
        public bool IsNeedApproval { get; set; }
        public bool IsApproved { get; set; }

        /// <summary>BE-RWI-105 / FR-DOK-090. Benar bila ada butir bertanda bentrok alergi atau tidak tersedia.</summary>
        public bool HasFlaggedItems { get; set; }

        /// <summary>Hasil per butir template beserta penandanya.</summary>
        public List<ApplyPrescriptionTemplateItemResult> Items { get; set; } = new();
    }

    /// <summary>
    /// Hasil pemakaian satu butir template — BE-RWI-105, api-contract 0.6.0 bagian 12.7.
    /// </summary>
    /// <remarks>
    /// <c>Flags</c> berisi <c>AllergyConflict</c> dan/atau <c>Unavailable</c>. Butir bentrok alergi
    /// tetap masuk draft (penunjuk butir terisi); butir tidak tersedia tidak masuk draft (penunjuk
    /// butir kosong) dan wajib diganti dokter.
    /// </remarks>
    public class ApplyPrescriptionTemplateItemResult
    {
        public Guid? PrescriptionItemId { get; set; }
        public Guid? PrescriptionCompoundId { get; set; }
        public Guid? PrescriptionCompoundItemId { get; set; }
        public Guid DrugId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public bool IsCompoundIngredient { get; set; }
        public string? CompoundName { get; set; }
        public List<string> Flags { get; set; } = new();
    }

    public class CancelPrescriptionTemplateRequest
    {
        [Required, MaxLength(250)] public string Reason { get; set; } = string.Empty;
    }
}
