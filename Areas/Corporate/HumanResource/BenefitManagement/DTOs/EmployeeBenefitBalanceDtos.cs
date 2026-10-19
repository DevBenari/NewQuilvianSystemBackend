using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    // BE-FIN-104, FIN-INTEGRATION-1.9 R.3 (INT-BIL-HR-001). Tiga operasi pertama adalah kontrak
    // in-process untuk Billing; buku mutasi juga dibaca lewat GET /{id}/movements (FIN-API-1.10 R.1).

    public sealed class BenefitBalanceReadRequest
    {
        /// <summary>Identitas pemilik benefit (utama); EmployeeId lama diterjemahkan bila kosong.</summary>
        public Guid? WorkforceProfileId { get; set; }

        /// <summary>Kompatibilitas kontrak lama (MstEmployee.Id). Boleh kosong untuk dokter.</summary>
        public Guid EmployeeId { get; set; }
        public Guid? FamilyMemberId { get; set; }

        /// <summary>Tanggal layanan.</summary>
        public DateTime AsOfDate { get; set; }
    }

    public sealed class BenefitBalanceReadResult
    {
        public bool IsUsable { get; set; }

        /// <summary>OK bila dapat dipakai; selain itu kode R.2.3.</summary>
        public string ReasonCode { get; set; } = string.Empty;
        public string? Message { get; set; }
        public string? BenefitPersonKind { get; set; }
        public Guid? BenefitPersonId { get; set; }
        public Guid? EnrollmentId { get; set; }
        public string? RelationshipCode { get; set; }

        /// <summary>Pemilik benefit yang sah; terisi bila dapat dipakai.</summary>
        public Guid? WorkforceProfileId { get; set; }
        public string? OwnerType { get; set; }
        public string? OwnerName { get; set; }
        public decimal LimitAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? PlanCode { get; set; }
    }

    public sealed class BenefitBalanceUseRequest
    {
        /// <summary>HandoffKey baris serah terima PAYER; kunci idempotensi.</summary>
        public Guid SourceHandoffKey { get; set; }
        public Guid InvoiceId { get; set; }
        public string BenefitPersonKind { get; set; } = string.Empty;
        public Guid BenefitPersonId { get; set; }
        public decimal Amount { get; set; }
        public Guid? ActorUserId { get; set; }
    }

    public sealed class BenefitBalanceUseResult
    {
        /// <summary>APPLIED, ALREADY_APPLIED, KEY_CONFLICT, INSUFFICIENT, NOT_USABLE.</summary>
        public string Result { get; set; } = string.Empty;
        public string? Message { get; set; }
        public Guid? MovementId { get; set; }

        /// <summary>Sisa terkini. Pada INSUFFICIENT dipakai Billing untuk menghitung ulang (R.3.4 langkah 5).</summary>
        public decimal RemainingAmount { get; set; }
    }

    public sealed class BenefitBalanceReturnRequest
    {
        public Guid SourceHandoffKey { get; set; }
        [MaxLength(500)] public string? Reason { get; set; }
        public Guid? ActorUserId { get; set; }
    }

    public sealed class BenefitBalanceReturnResult
    {
        /// <summary>RETURNED, ALREADY_RETURNED, NOT_FOUND.</summary>
        public string Result { get; set; } = string.Empty;
        public Guid? MovementId { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingAmount { get; set; }
    }

    public sealed class BenefitMovementPagedQuery
    {
        [MaxLength(10)] public string? MovementType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
        [Range(1, 50)] public int PageSize { get; set; } = 10;
    }

    public sealed class BenefitMovementResponse
    {
        public Guid Id { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public string BenefitPersonKind { get; set; } = string.Empty;
        public Guid BenefitPersonId { get; set; }
        public string? PersonName { get; set; }

        /// <summary>Pemilik enrollment (karyawan atau dokter) beserta nomornya, untuk baris pemakaian pemilik sendiri.</summary>
        public string? OwnerType { get; set; }
        public string? OwnerStaffNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public Guid InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTimeOffset OccurredAt { get; set; }
        public Guid? ReversesMovementId { get; set; }
        public string? ReasonText { get; set; }
    }
}
