namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class ArHandoffResponse
{
    public Guid Id { get; set; }
    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? HandoffKey { get; set; }
    public Guid? BenefitOwnerId { get; set; }

    /// <summary>Identitas canonical pemilik manfaat (karyawan atau dokter tetap); kosong pada baris lama sebelum backfill.</summary>
    public Guid? BenefitOwnerWorkforceProfileId { get; set; }
    public string? BenefitRelationship { get; set; }
    public decimal DeferredPatientAmount { get; set; }
    public string? DeferredConsentGivenBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }

    /// <summary>
    /// True untuk baris PAYER porsi Benefit Karyawan RS atas penjamin internal (biaya RS sendiri, nol kas masuk). Bukan
    /// utang pribadi karyawan dan bukan piutang pihak luar.
    /// </summary>
    public bool IsInternalBenefit { get; set; }

    /// <summary>Asal piutang karyawan (hanya EMPLOYEE_BENEFIT): OVER_ALLOWANCE, PATIENT_PORTION, atau MIXED.</summary>
    public string? EmployeeArOrigin { get; set; }

    /// <summary>
    /// Tahap serah terima ke Finance, diturunkan dari data sungguhan (lihat ArHandoffFinanceStages): handoff Billing
    /// berstatus CREATED BUKAN berarti piutang sudah diakui Finance.
    /// </summary>
    public string FinanceStage { get; set; } = ArHandoffFinanceStages.HandoffCreated;

    /// <summary>Status fakta masuk Finance (NEW, CONSUMED, ACKNOWLEDGED, ERROR); kosong bila intake belum terbentuk.</summary>
    public string? FinanceIntakeStatus { get; set; }
    public string? FinanceIntakeError { get; set; }

    /// <summary>Piutang Finance hasil pengolahan handoff ini; kosong bila belum diakui.</summary>
    public Guid? FinReceivableId { get; set; }
    public string? FinReceivableNumber { get; set; }
    public string? FinReceivableStatus { get; set; }
}

/// <summary>
/// Kode tahap serah terima handoff AR dari Billing ke Finance. Kode program, TIDAK diterjemahkan.
/// </summary>
public static class ArHandoffFinanceStages
{
    /// <summary>Handoff Billing dibuat; Finance belum membuat fakta masuk (intake).</summary>
    public const string HandoffCreated = "HANDOFF_CREATED";

    /// <summary>Fakta masuk Finance sudah ada (NEW) tetapi belum diolah.</summary>
    public const string IntakePending = "INTAKE_PENDING";

    /// <summary>Pengolahan fakta masuk gagal; perlu ditinjau dan diulang di Finance.</summary>
    public const string IntakeError = "INTAKE_ERROR";

    /// <summary>Piutang Finance (FinReceivable) sudah terbentuk dari handoff ini.</summary>
    public const string ReceivableRecognized = "RECEIVABLE_RECOGNIZED";

    /// <summary>Intake tercatat selesai diolah tetapi tidak ada piutang yang cocok: data tidak sinkron.</summary>
    public const string ReconciliationRequired = "RECONCILIATION_REQUIRED";

    /// <summary>
    /// Menurunkan tahap dari tiga fakta: status intake (null = belum ada), apakah piutang Finance dengan
    /// SourceHandoffKey yang sama sudah ada, dan status intake itu sendiri. Piutang yang ada menang atas status intake,
    /// karena piutang adalah bukti pengakuan.
    /// </summary>
    public static string Derive(string? intakeStatus, bool receivableExists)
    {
        if (receivableExists) return ReceivableRecognized;
        if (string.IsNullOrWhiteSpace(intakeStatus)) return HandoffCreated;

        return intakeStatus.Trim().ToUpperInvariant() switch
        {
            "NEW" => IntakePending,
            "ERROR" => IntakeError,
            "CONSUMED" or "ACKNOWLEDGED" => ReconciliationRequired,
            _ => IntakePending
        };
    }
}

public sealed class ApHandoffResponse
{
    public Guid Id { get; set; }
    public Guid DoctorId { get; set; }
    public decimal Amount { get; set; }
    public string ReadinessStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? HandoffKey { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ReadyAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
}

public sealed class HandoffAdjustmentResponse
{
    public Guid Id { get; set; }
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class HandoffStatusResponse
{
    public Guid InvoiceId { get; set; }
    public Guid FinalizationRecordId { get; set; }
    public IReadOnlyList<ArHandoffResponse> ArHandoffs { get; set; } = [];
    public IReadOnlyList<ApHandoffResponse> ApHandoffs { get; set; } = [];
    public IReadOnlyList<HandoffAdjustmentResponse> Adjustments { get; set; } = [];
}
