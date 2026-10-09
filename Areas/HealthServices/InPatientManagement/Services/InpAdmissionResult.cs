namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Kode alasan Workspace PPRI — <c>contracts/validation-matrix.md</c> bagian 15
    /// (<c>BE-RWI-193</c>). Kalimat pesannya ditulis di service, sama persis dengan matriks.
    /// </summary>
    public static class InpAdmissionCodes
    {
        public const string EpisodeNotWritable = "INP-ADM-DOC-001";
        public const string EpisodeNotAdmitted = "INP-ADM-DOC-002";
        public const string ActiveDocumentExists = "INP-ADM-DOC-003";
        public const string StaleRowVersion = "INP-ADM-DOC-004";
        public const string InvalidDocumentState = "INP-ADM-DOC-005";
        public const string UnlockWithSignature = "INP-ADM-DOC-006";
        public const string DiscardNotCreator = "INP-ADM-DOC-008";
        public const string CostDifferencePayer = "INP-ADM-DOC-010";
        public const string NoDepositShortfall = "INP-ADM-DOC-011";
        public const string DepositUnavailable = "INP-ADM-DOC-012";
        public const string PartySourceNotFound = "INP-ADM-DOC-018";
        public const string DueDateOutOfRange = "INP-ADM-DOC-019";
        public const string HandoverItemNotChosen = "INP-ADM-DOC-020";
        public const string HandoverNoteRequired = "INP-ADM-DOC-021";
        public const string PrivacyIncomplete = "INP-ADM-DOC-022";
        public const string BeliefIncomplete = "INP-ADM-DOC-023";
        public const string CostDifferenceIncomplete = "INP-ADM-DOC-024";
        public const string DepositIncomplete = "INP-ADM-DOC-025";
        public const string SnapshotSourceUnavailable = "INP-ADM-DOC-027";
        public const string SignatureNotLocked = "INP-ADM-DOC-030";
        public const string SlotNotRequired = "INP-ADM-DOC-031";
        public const string SlotAlreadySigned = "INP-ADM-DOC-032";
        public const string SameAccountTwoSlots = "INP-ADM-DOC-033";
        public const string PatientNotInBed = "INP-ADM-DOC-034";

        public const string ReprintReasonRequired = "INP-ADM-PRT-001";
        public const string WrongPrintPath = "INP-ADM-PRT-002";
        public const string AmountPrintNeedsPrint = "INP-ADM-PRT-003";
        public const string BaseDataPrintBlocked = "INP-ADM-PRT-004";
        public const string WristbandKindMismatch = "INP-ADM-PRT-005";
    }

    /// <summary>Satu alasan penolakan beserta kodenya, misalnya butir serah terima yang belum dipilih.</summary>
    public sealed record InpAdmissionError(string Code, string Message);

    /// <summary>
    /// Hasil perintah atau bacaan Workspace PPRI beserta kode status HTTP-nya (<c>BE-RWI-193</c>).
    /// </summary>
    /// <remarks>
    /// Penolakan kunci memuat <b>seluruh</b> isian yang kurang dalam satu hasil (validation 15.4),
    /// sehingga petugas tidak mengunci berulang kali. Contoh: butir 5 belum dipilih dan butir 11
    /// Belum tanpa keterangan → satu penolakan 422 dengan dua <see cref="Errors"/>.
    /// </remarks>
    public sealed class InpAdmissionResult<T>
    {
        public int StatusCode { get; private init; }

        public string Message { get; private init; } = string.Empty;

        public string? Code { get; private init; }

        public IReadOnlyList<InpAdmissionError>? Errors { get; private init; }

        public T? Data { get; private init; }

        public bool IsSuccess => StatusCode is >= 200 and < 300;

        public static InpAdmissionResult<T> Ok(T data, string message)
            => new() { StatusCode = StatusCodes.Status200OK, Data = data, Message = message };

        public static InpAdmissionResult<T> Fail(
            int statusCode,
            string message,
            string? code = null,
            IReadOnlyList<InpAdmissionError>? errors = null)
            => new() { StatusCode = statusCode, Message = message, Code = code, Errors = errors };

        /// <summary>Meneruskan kegagalan ke hasil berjenis lain.</summary>
        public InpAdmissionResult<TOther> AsFailure<TOther>()
            => InpAdmissionResult<TOther>.Fail(StatusCode, Message, Code, Errors);
    }
}
