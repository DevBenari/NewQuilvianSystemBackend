namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs
{
    /// <summary>
    /// Response rekapitulasi penetapan gaji lintas seluruh pegawai.
    /// [DECISION] HRD-DEC-033: Nominal gaji (BaseSalary) MUST NOT disertakan pada response ini demi perlindungan kerahasiaan.
    /// Nominal dibaca terpisah lewat endpoint detail dengan wewenang ViewAmount.
    /// </summary>
    public class WfpSalaryAssignmentCrossEmployeeResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public Guid SalaryStructureId { get; set; }
        public string SalaryStructureName { get; set; } = string.Empty;
        public Guid SalaryGradeId { get; set; }
        public string SalaryGradeName { get; set; } = string.Empty;
        public Guid? EmployeeGradeId { get; set; }
        public string? EmployeeGradeName { get; set; }
        public Guid? PayrollPeriodId { get; set; }
        public string? PayrollPeriodName { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string PaymentFrequency { get; set; } = "Monthly";
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsConfidential { get; set; }
        public bool IsActive { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public bool IsApproved { get; set; }
        public string? Description { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    /// <summary>
    /// Response pembacaan nominal gaji spesifik satu penetapan bagi pengguna yang memiliki wewenang ViewAmount.
    /// [DECISION] HRD-DEC-033.
    /// </summary>
    public class WfpSalaryAmountResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string WorkforceProfileCode { get; set; } = string.Empty;
        public string WorkforceDisplayName { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public string CurrencyCode { get; set; } = "IDR";
        public string PaymentFrequency { get; set; } = "Monthly";
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }
}
