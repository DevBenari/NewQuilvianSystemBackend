namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Enums
{
    /// <summary>
    /// Status siklus sinkronisasi kartu penjamin internal pasien (FIN-STATE-1.11 V.2, kamus data 12.2).
    /// </summary>
    public static class EmployeeBenefitCardSyncStatuses
    {
        public const string Synced = "SYNCED";
        public const string WaitingPatient = "WAITING_PATIENT";
        public const string OnHold = "ON_HOLD";
        public const string FailedRetryable = "FAILED_RETRYABLE";
        public const string Revoked = "REVOKED";
        public const string NotStarted = "NOT_STARTED";

        public static readonly string[] All = { Synced, WaitingPatient, OnHold, FailedRetryable, Revoked, NotStarted };
    }

    /// <summary>
    /// Pemicu proses sinkronisasi kartu pasien (FIN-INTEGRATION-1.11 V.2, kamus data 12.2).
    /// </summary>
    public static class EmployeeBenefitCardSyncTriggerSources
    {
        public const string Activation = "ACTIVATION";
        public const string Scheduler = "SCHEDULER";
        public const string ManualResync = "MANUAL_RESYNC";
        public const string Cancel = "CANCEL";

        public static readonly string[] All = { Activation, Scheduler, ManualResync, Cancel };
    }

    /// <summary>
    /// Kode alasan hasil sinkronisasi kartu penjamin pasien (kamus data 12.2, FIN-DES-116).
    /// Wajib diisi bila status != SYNCED, kosong/null bila status == SYNCED.
    /// </summary>
    public static class EmployeeBenefitCardSyncReasonCodes
    {
        // Alasan untuk status WAITING_PATIENT (diulang berkala oleh penjadwal)
        public const string NoMatch = "NO_MATCH";
        public const string IdentityTypeNotAllowed = "IDENTITY_TYPE_NOT_ALLOWED";
        public const string IdentityDataIncomplete = "IDENTITY_DATA_INCOMPLETE";

        // Alasan untuk status ON_HOLD (ditahan untuk tinjauan manual petugas Registrasi, tidak diulang otomatis)
        public const string MultipleMatch = "MULTIPLE_MATCH";
        public const string OtherInternalCardExists = "OTHER_INTERNAL_CARD_EXISTS";
        public const string LegacyUnlinkedCard = "LEGACY_UNLINKED_CARD";

        // Alasan untuk status FAILED_RETRYABLE (kegagalan sementara, diulang dengan backoff)
        public const string HrUnavailable = "HR_UNAVAILABLE";
        public const string InternalGuarantorUnavailable = "INTERNAL_GUARANTOR_UNAVAILABLE";
        public const string CardValidationRejected = "CARD_VALIDATION_REJECTED";
        public const string UnexpectedError = "UNEXPECTED_ERROR";

        // Alasan untuk status REVOKED (kartu dinonaktifkan / dicabut)
        public const string CardRevokedByStaff = "CARD_REVOKED_BY_STAFF";
        public const string CardDeleted = "CARD_DELETED";
    }
}
