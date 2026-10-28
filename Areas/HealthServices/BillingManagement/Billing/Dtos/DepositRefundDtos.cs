using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class DepositRefundEligibilityResponse
{
    public bool IsEligible { get; set; }
    public string? IneligibilityReason { get; set; }
    public Guid EncounterId { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public decimal TotalDepositReceived { get; set; }
    public decimal TotalDepositAllocated { get; set; }
    public decimal DepositAvailableBalance { get; set; }
    public decimal ReservedRefundAmount { get; set; }
    public decimal MaxRefundableAmount { get; set; }
    public Guid DepositRowVersion { get; set; }
}

public sealed class CreateDepositRefundRequest
{
    public Guid InvoiceId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal RequestedAmount { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>Selalu <c>TRANSFER</c>. Nilai lain (termasuk tunai) ditolak server.</summary>
    [MaxLength(30)]
    public string PaymentMethod { get; set; } = "TRANSFER";

    /// <summary>Rekening pengirim rumah sakit (master rekening bank). Wajib; diperiksa ulang server.</summary>
    public Guid? BankAccountId { get; set; }

    /// <summary>
    /// Bank pengirim yang dipilih kasir. Opsional; bila dikirim, server memastikan rekening pengirim
    /// memang milik bank ini. Tidak disimpan — bank diturunkan dari relasi master rekening.
    /// </summary>
    public Guid? SenderBankId { get; set; }

    /// <summary>Rekening tujuan pasien — data terpisah dari rekening pengirim.</summary>
    [MaxLength(100)]
    public string? PayeeBankName { get; set; }

    [MaxLength(50)]
    public string? PayeeAccountNumber { get; set; }

    [MaxLength(150)]
    public string? PayeeAccountName { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
    public Guid ExpectedDepositRowVersion { get; set; }
}

public sealed class DepositRefundResponse
{
    public Guid RefundRequestId { get; set; }
    public Guid RefundableCreditId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PayeeBankName { get; set; }
    public string? PayeeAccountNumber { get; set; }
    public string? PayeeAccountName { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal RemainingDepositBalance { get; set; }
    public bool IsReplay { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
}

