namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos
{
    // =========================================================================
    // REQUEST DTOS
    // =========================================================================

    /// <summary>
    /// Permintaan pratinjau perbandingan perhitungan dengan payer kandidat (POST /{id}/payer-comparison-preview).
    /// Murni membaca data (100% read-only), tidak membuat versi perhitungan baru,
    /// tidak menyentuh kunjungan, dan tidak meninggalkan jejak apa pun (BIL-API-1.0).
    /// </summary>
    public sealed class PayerComparisonPreviewRequest
    {
        public string CandidatePaymentType { get; set; } = string.Empty;
        public Guid? CandidatePatientInsuranceId { get; set; }
        public Guid? CandidatePatientCompanyGuarantorId { get; set; }
        public Guid? CandidatePaymentMethodId { get; set; }
    }

    /// <summary>
    /// Permintaan penggantian payer kunjungan yang berlaku (PUT /{id}/payment-source).
    /// Satu bentuk untuk ketiga jenis target (MPY-DES-001, BIL-API-1.0).
    /// </summary>
    public sealed class SwitchPaymentSourceRequest
    {
        public string PaymentType { get; set; } = string.Empty;
        public Guid? PaymentMethodId { get; set; }
        public Guid? PatientInsuranceId { get; set; }
        public Guid? PatientCompanyGuarantorId { get; set; }
        public string ExpectedRowVersion { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
        public string? CausationId { get; set; }
    }

    /// <summary>
    /// Permintaan penandaan penanggung tiap baris biaya (PUT /{id}/item-payer-assignments, BE-BKC-048, MPY-DES-008, BIL-API-1.0).
    /// </summary>
    public sealed class UpdateItemPayerAssignmentsRequest
    {
        public List<ItemPayerAssignmentItemRequest> Assignments { get; set; } = [];
        public string ExpectedRowVersion { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
        public string? CausationId { get; set; }
    }

    public sealed class ItemPayerAssignmentItemRequest
    {
        public Guid InvoiceItemId { get; set; }
        public string PayerKind { get; set; } = string.Empty;
    }

    /// <summary>
    /// Permintaan pengaturan disposisi penebusan obat tagihan (PUT /{id}/drug-billing-disposition, BE-BKC-049, MPY-DES-010, BIL-API-1.0).
    /// Mode: ALL_REDEEMED, PARTIAL_REDEEMED, atau NOT_REDEEMED.
    /// </summary>
    public sealed class UpdateDrugBillingDispositionRequest
    {
        public string Mode { get; set; } = string.Empty;
        public List<Guid> IncludedInvoiceItemIds { get; set; } = [];
        public string ExpectedRowVersion { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
        public string? CausationId { get; set; }
    }

    // =========================================================================
    // RESPONSE DTOS
    // =========================================================================

    /// <summary>
    /// Seluruh bahan layar Edit Tagihan dalam satu panggilan (GET /{id}/edit-context, MPY-DES-015, BIL-API-1.0).
    /// </summary>
    public sealed class InvoiceEditContextResponse
    {
        public InvoiceEditHeaderResponse Invoice { get; set; } = new();
        public CalculationResponse Calculation { get; set; } = new();
        public CurrentPayerResponse CurrentPayer { get; set; } = new();
        public List<AvailablePayerOptionResponse> AvailablePayerOptions { get; set; } = [];
        public List<ItemPayerAssignmentResponse> ItemPayerAssignments { get; set; } = [];
        public List<DrugBillingDispositionItemResponse> DrugBillingDisposition { get; set; } = [];
        public List<Guid> EligibleDrugInvoiceItemIds { get; set; } = [];
        public InvoiceEditCapabilitiesResponse Capabilities { get; set; } = new();
    }

    public sealed class InvoiceEditHeaderResponse
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public Guid RowVersion { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public DateTime EncounterDate { get; set; }
    }

    public sealed class CurrentPayerResponse
    {
        public string PaymentType { get; set; } = "CASH";
        public string? PaymentSourceName { get; set; }
        public string? CardNumberMasked { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsEligible { get; set; } = true;
        public bool IsPolicyActive { get; set; } = false;
        public Guid? PatientInsuranceId { get; set; }
        public Guid? PatientCompanyGuarantorId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public Guid? InsuranceProviderId { get; set; }
        public Guid? CompanyGuarantorId { get; set; }
    }

    public sealed class AvailablePayerOptionResponse
    {
        public string PayerType { get; set; } = "CASH";
        public string PayerName { get; set; } = string.Empty;
        public Guid? PatientInsuranceId { get; set; }
        public Guid? PatientCompanyGuarantorId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public string? CardNumberMasked { get; set; }
        public string? PolicyNumber { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? BenefitPlanCode { get; set; }
        public string? BenefitPlanName { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsEligible { get; set; } = true;
        public bool IsCurrentlyAvailable { get; set; } = true;
        public string? ReasonIfNotAvailable { get; set; }
    }

    public sealed class ItemPayerAssignmentResponse
    {
        public Guid InvoiceItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string PayerKind { get; set; } = "CASH";
        public string AssignmentSource { get; set; } = "AUTO";
        public decimal Amount { get; set; }
    }

    public sealed class DrugBillingDispositionItemResponse
    {
        public Guid InvoiceItemId { get; set; }
        public string DrugName { get; set; } = string.Empty;
        public string Disposition { get; set; } = "INCLUDED";
        public string DecisionSource { get; set; } = "AUTO";
        public decimal Amount { get; set; }
    }

    public sealed class InvoiceEditCapabilitiesResponse
    {
        public bool CanEditPaymentSource { get; set; }
        public string? PaymentSourceBlockReason { get; set; }
        public bool CanEditItemPayer { get; set; }
        public string? ItemPayerBlockReason { get; set; }
        public bool CanEditDrugBilling { get; set; }
        public string? DrugBillingBlockReason { get; set; }
    }

    /// <summary>
    /// Hasil pratinjau perbandingan perhitungan antara payer yang sedang berlaku dengan payer kandidat
    /// (POST /{id}/payer-comparison-preview, BIL-API-1.0).
    /// </summary>
    public sealed class PayerComparisonPreviewResponse
    {
        public PayerComparisonSummaryResponse Current { get; set; } = new();
        public PayerComparisonSummaryResponse Candidate { get; set; } = new();
        public List<PayerComparisonItemResponse> PerItemComparison { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
        public bool CanApply { get; set; } = true;
        public string? BlockReason { get; set; }
    }

    public sealed class PayerComparisonSummaryResponse
    {
        public string PayerKind { get; set; } = "CASH";
        public string PayerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PrimaryAmount { get; set; }
        public decimal PatientAmount { get; set; }
        public decimal ExcessAmount { get; set; }
        public decimal NonBillableResidualAmount { get; set; }
        public decimal DataAnomalyAmount { get; set; }
    }

    public sealed class PayerComparisonItemResponse
    {
        public Guid InvoiceItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public decimal CurrentCovered { get; set; }
        public decimal CurrentPatient { get; set; }
        public decimal CandidateCovered { get; set; }
        public decimal CandidatePatient { get; set; }
        public decimal Difference { get; set; }
    }

    /// <summary>
    /// Hasil eksekusi perubahan payer / penanggung tagihan (PUT /{id}/payment-source, BIL-API-1.0).
    /// </summary>
    public sealed class InvoiceEditResultResponse
    {
        public CalculationResponse Calculation { get; set; } = new();
        public Guid RowVersion { get; set; }
        public int ResetAssignmentCount { get; set; }
        public List<string> Warnings { get; set; } = [];
    }
}
