using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs
{
    /// <summary>Satu baris pada daftar kantong darah.</summary>
    public class BloodUnitListDto
    {
        public Guid Id { get; set; }

        /// <summary>Nomor kantong dari PMI. <b>Sensitif</b> — tidak pernah masuk log.</summary>
        public string PmiBagNumber { get; set; } = string.Empty;

        public Guid BloodComponentId { get; set; }
        public string? BloodComponentCode { get; set; }
        public string? BloodComponentName { get; set; }

        public BbkBloodUnitStatus UnitStatus { get; set; }
        public string UnitStatusLabel { get; set; } = string.Empty;

        /// <summary>Kantong datang melebihi jumlah yang diminta (<c>DEC-BD-025</c>).</summary>
        public bool IsExcess { get; set; }

        public Guid ProviderRequestId { get; set; }
        public string? RequestNumber { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        public Guid ReceiptId { get; set; }
        public DateTime? ReceivedAt { get; set; }

        public int Version { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>Detail satu kantong beserta asal dan riwayat perpindahan statusnya.</summary>
    public class BloodUnitDetailDto
    {
        public Guid Id { get; set; }
        public string PmiBagNumber { get; set; } = string.Empty;

        public Guid BloodComponentId { get; set; }
        public string? BloodComponentCode { get; set; }
        public string? BloodComponentName { get; set; }

        public BbkBloodUnitStatus UnitStatus { get; set; }
        public string UnitStatusLabel { get; set; } = string.Empty;
        public bool IsExcess { get; set; }

        /// <summary>Asal kantong — tidak pernah putus.</summary>
        public Guid ProviderRequestId { get; set; }
        public string? RequestNumber { get; set; }
        public BbkProviderRequestStatus? RequestStatus { get; set; }
        public string? RequestStatusLabel { get; set; }
        public Guid? BloodOrderId { get; set; }
        public string? OrderNumber { get; set; }
        public Guid? PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }

        public Guid ReceiptId { get; set; }
        public int? ReceiptSequence { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public Guid? ReceivedByUserId { get; set; }

        public Guid? IssuedToPatientId { get; set; }
        public DateTime? IssuedAt { get; set; }
        public Guid? IssuedByUserId { get; set; }
        public bool IssuedViaEmergency { get; set; }

        public int Version { get; set; }

        public List<BloodBankTransitionDto> Transitions { get; set; } = new();

        /// <summary>
        /// Aksi yang layak dicoba. <b>Kosong pada slice <c>BE-BD-004</c></b>: tindakan pertama atas
        /// kantong <c>Received</c> adalah penyimpanan, yang lahir pada <c>BE-BD-015</c>.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();

        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    /// <summary>Angka ringkasan daftar kantong darah per status.</summary>
    public class BloodUnitSummaryResponse
    {
        public int TotalUnit { get; set; }
        public int ReceivedUnit { get; set; }
        public int StoredUnit { get; set; }
        public int AvailableUnit { get; set; }
        public int AllocatedUnit { get; set; }
        public int IssuedUnit { get; set; }
        public int PendingReviewUnit { get; set; }
        public int ReallocatedUnit { get; set; }
        public int ReturnedToProviderUnit { get; set; }
        public int NotUsableUnit { get; set; }

        /// <summary>Kantong berlebih, di status apa pun.</summary>
        public int ExcessUnit { get; set; }
    }

    public class BloodUnitFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";

        public BloodUnitDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BloodBankOptionItemResponse> UnitStatusOptions { get; set; } = new();
        public List<BloodBankSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    public class BloodUnitDefaultFilterResponse
    {
        public string? Search { get; set; }
        public BbkBloodUnitStatus? UnitStatus { get; set; }
        public bool? IsExcess { get; set; }
        public Guid? ProviderRequestId { get; set; }
        public Guid? BloodComponentId { get; set; }
        public string SortBy { get; set; } = "createDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
