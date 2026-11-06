namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public enum NvwResultKind
    {
        Success = 1,
        Created = 2,
        Validation = 3,
        Forbidden = 4,
        NotFound = 5,
        Conflict = 6,
        BusinessRule = 7
    }

    public sealed class NvwResult<T>
    {
        private NvwResult(NvwResultKind kind)
        {
            Kind = kind;
        }

        public NvwResultKind Kind { get; private init; }

        public T? Value { get; private init; }

        public string? Code { get; private init; }

        public string? Message { get; private init; }

        public bool IsSuccess => Kind is NvwResultKind.Success or NvwResultKind.Created;

        public static NvwResult<T> Ok(T value) => new(NvwResultKind.Success) { Value = value };

        public static NvwResult<T> Created(T value) => new(NvwResultKind.Created) { Value = value };

        public static NvwResult<T> Invalid(string code, string message) =>
            new(NvwResultKind.Validation) { Code = code, Message = message };

        public static NvwResult<T> Forbidden(string code, string message) =>
            new(NvwResultKind.Forbidden) { Code = code, Message = message };

        public static NvwResult<T> NotFound(string message) =>
            new(NvwResultKind.NotFound) { Code = NvwErrorCodes.NotFound, Message = message };

        public static NvwResult<T> Conflict(string code, string message) =>
            new(NvwResultKind.Conflict) { Code = code, Message = message };

        public static NvwResult<T> Rule(string code, string message) =>
            new(NvwResultKind.BusinessRule) { Code = code, Message = message };
    }

    public static class NvwErrorCodes
    {
        public const string NotFound = "NV-NOT-FOUND";
        public const string FloorUnknown = "VAL-NV-02";
        public const string VersionMismatch = "VAL-NV-04";
        public const string LoginAlreadyExists = "VAL-NV-05";
        public const string LoginNotCreated = "VAL-NV-06";
        public const string DeactivationReason = "VAL-NV-07";
        public const string AlreadyInactive = "VAL-NV-08";
        public const string CodeAllocationFailed = "NV-CODE-ALLOCATION";
        public const string LoginAccountConflict = "NV-LOGIN-ACCOUNT";
        public const string EwsLevelUnknown = "VAL-NV-09";
        public const string EwsLevelDuplicate = "VAL-NV-10";
        public const string IntervalOutOfRange = "VAL-NV-11";
        public const string RatificationReference = "VAL-NV-12";
        public const string RatifiedOn = "VAL-NV-13";
        public const string IntervalItemsRequired = "NV-INTERVAL-ITEMS";
        public const string NotDisplayAccount = "VAL-NV-14";
        public const string DeviceInactive = "VAL-NV-15";
        public const string SessionStale = "NV-SESSION-STALE";
        public const string TabletNameRequired = "VAL-NV-17";
        public const string BedInvalid = "VAL-NV-18";
        public const string BedHasTablet = "VAL-NV-19";
        public const string BedHasActiveCall = "VAL-NV-20";
        public const string TabletVersionMismatch = "VAL-NV-21";
        public const string TabletLoginAlreadyExists = "VAL-NV-22";
        public const string TabletLoginNotCreated = "VAL-NV-23";
        public const string TabletDeactivationReason = "VAL-NV-24";
        public const string TabletAlreadyInactive = "VAL-NV-25";
        public const string WaitThresholdInvalid = "VAL-NV-40";
    }

    public static class NvwMessages
    {
        public const string DeviceNameRequired = "Nama perangkat wajib diisi, paling banyak 100 karakter.";
        public const string FloorUnknown = "Lantai tidak dikenal. Pilih lantai yang terdaftar pada data kamar.";
        public const string TooLong = "Isian terlalu panjang.";
        public const string VersionMismatch = "Data perangkat sudah diubah orang lain. Muat ulang lalu coba lagi.";
        public const string LoginAlreadyExists = "Perangkat ini sudah punya login. Gunakan Atur Ulang Login.";
        public const string LoginNotCreated = "Perangkat belum punya login. Gunakan Buat Login.";
        public const string DeactivationReason = "Alasan menonaktifkan wajib diisi (5–500 karakter).";
        public const string AlreadyInactive = "Perangkat sudah nonaktif.";
        public const string DeviceNotFound = "Perangkat Nurse View tidak ditemukan.";
        public const string CodeAllocationFailed = "Kode perangkat gagal diterbitkan. Coba lagi.";
        public const string LoginAccountConflict = "Akun login untuk kode perangkat ini sudah ada. Hubungi tim IT.";
        public const string EwsLevelUnknown = "Tingkat risiko EWS tidak dikenal.";
        public const string EwsLevelDuplicate = "Setiap tingkat risiko hanya boleh diisi satu kali.";
        public const string IntervalOutOfRange = "Interval harus antara 1 dan 1440 menit (24 jam).";
        public const string RatificationReference = "Nomor surat/notulen pengesahan Komite Keperawatan wajib diisi.";
        public const string RatifiedOnRequired = "Tanggal pengesahan wajib diisi.";
        public const string RatifiedOnFuture = "Tanggal pengesahan tidak boleh melewati hari ini.";
        public const string IntervalItemsRequired = "Daftar interval wajib dikirim. Kirim daftar kosong untuk mencabut seluruh interval.";
        public const string NotDisplayAccount = "Akun ini bukan akun display Nurse View.";
        public const string DeviceInactive = "Perangkat display dinonaktifkan. Hubungi IT.";
        public const string SessionStale = "Sesi perangkat sudah tidak berlaku. Silakan login ulang.";
        public const string TabletNameRequired = "Nama tablet wajib diisi, paling banyak 100 karakter.";
        public const string BedInvalid = "Bed tidak dikenal atau kamarnya belum punya lantai. Pilih bed dari daftar.";
        public const string BedHasTablet = "Bed ini sudah punya tablet. Nonaktifkan tablet lama lebih dulu.";
        public const string BedHasActiveCall = "Bed ini masih punya panggilan aktif. Selesaikan panggilannya dulu.";
        public const string TabletVersionMismatch = "Data tablet sudah diubah orang lain. Muat ulang lalu coba lagi.";
        public const string TabletLoginAlreadyExists = "Tablet ini sudah punya login. Gunakan Atur Ulang Login.";
        public const string TabletLoginNotCreated = "Tablet belum punya login. Gunakan Buat Login.";
        public const string TabletDeactivationReason = "Alasan menonaktifkan wajib diisi (5–500 karakter).";
        public const string TabletAlreadyInactive = "Tablet sudah nonaktif.";
        public const string TabletNotFound = "Tablet bed tidak ditemukan.";
        public const string WaitThresholdRange = "Ambang kuning 1–240 menit dan ambang merah harus lebih besar, paling banyak 480 menit.";
    }
}
