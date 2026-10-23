namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services
{
    /// <summary>
    /// Jenis hasil satu tindakan MCU, dipetakan satu-satu ke kode status umum
    /// <c>MCU-API-v1</c>: <c>200</c>, <c>400</c>, <c>403</c>, <c>404</c>, <c>409</c>.
    /// </summary>
    /// <remarks>
    /// Kontrak MCU tidak mengenal <c>201</c>, <c>422</c>, maupun <c>423</c>; pembuatan data pun
    /// menjawab <c>200</c>. Syarat yang belum terpenuhi (belum lunas, kuota penuh, status tidak
    /// sesuai) dijawab <see cref="Conflict"/>.
    /// </remarks>
    public enum McuResultKind
    {
        Success = 1,
        Validation = 2,
        Forbidden = 3,
        NotFound = 4,
        Conflict = 5
    }

    /// <summary>Hasil satu tindakan beserta kode aturan dan pesannya.</summary>
    public sealed class McuResult<T>
    {
        private McuResult(McuResultKind kind)
        {
            Kind = kind;
        }

        public McuResultKind Kind { get; private init; }

        public T? Value { get; private init; }

        /// <summary>Kode aturan <c>validation-matrix.md</c>, misalnya <c>VAL-MCU-04</c>.</summary>
        public string? Code { get; private init; }

        public string? Message { get; private init; }

        public bool IsSuccess => Kind == McuResultKind.Success;

        public static McuResult<T> Ok(T value) => new(McuResultKind.Success) { Value = value };

        public static McuResult<T> Invalid(string code, string message) =>
            new(McuResultKind.Validation) { Code = code, Message = message };

        public static McuResult<T> Forbidden(string code, string message) =>
            new(McuResultKind.Forbidden) { Code = code, Message = message };

        public static McuResult<T> NotFound(string message) =>
            new(McuResultKind.NotFound) { Code = McuErrorCodes.NotFound, Message = message };

        public static McuResult<T> Conflict(string code, string message) =>
            new(McuResultKind.Conflict) { Code = code, Message = message };
    }

    /// <summary>
    /// Kode penolakan modul MCU. Kode bernomor sama dengan ID baris <c>validation-matrix.md</c>;
    /// kalimat pesannya hidup di service yang menegakkan aturan itu, disalin dari matriks.
    /// </summary>
    /// <remarks>
    /// Kode tanpa nomor matriks — <see cref="InvalidRequest"/> — dipakai untuk isian wajib, panjang
    /// isian, dan rentang tanggal yang matriksnya tidak sebut, dan dicatat sebagai delta pada
    /// laporan task.
    /// </remarks>
    public static class McuErrorCodes
    {
        public const string NotFound = "MCU-NOT-FOUND";
        public const string InvalidRequest = "MCU-INVALID-REQUEST";

        public const string Val01 = "VAL-MCU-01";
        public const string Val02 = "VAL-MCU-02";
        public const string Val03 = "VAL-MCU-03";
        public const string Val04 = "VAL-MCU-04";
        public const string Val05 = "VAL-MCU-05";
        public const string Val06 = "VAL-MCU-06";
        public const string Val07 = "VAL-MCU-07";
        public const string Val08 = "VAL-MCU-08";
        public const string Val09 = "VAL-MCU-09";
        public const string Val10 = "VAL-MCU-10";
        public const string Val11 = "VAL-MCU-11";
        public const string Val12 = "VAL-MCU-12";
        public const string Val13 = "VAL-MCU-13";
        public const string Val14 = "VAL-MCU-14";
        public const string Val15 = "VAL-MCU-15";
        public const string Val16 = "VAL-MCU-16";
        public const string Val17 = "VAL-MCU-17";
        public const string Val18 = "VAL-MCU-18";
        public const string Val20 = "VAL-MCU-20";
        public const string Val21 = "VAL-MCU-21";
        public const string Val23 = "VAL-MCU-23";
        public const string Val24 = "VAL-MCU-24";
        public const string Val25 = "VAL-MCU-25";
        public const string Val26 = "VAL-MCU-26";
        public const string Val27 = "VAL-MCU-27";
        public const string Val28 = "VAL-MCU-28";
        public const string Val29 = "VAL-MCU-29";
        public const string Val34 = "VAL-MCU-34";
        public const string Val35 = "VAL-MCU-35";
        public const string Val36 = "VAL-MCU-36";
        public const string Val37 = "VAL-MCU-37";
        public const string Val38 = "VAL-MCU-38";
        public const string Val39 = "VAL-MCU-39";
        public const string Val40 = "VAL-MCU-40";
        public const string Val41 = "VAL-MCU-41";
        public const string Val42 = "VAL-MCU-42";
        public const string Val43 = "VAL-MCU-43";
        public const string Val44 = "VAL-MCU-44";
        public const string Val45 = "VAL-MCU-45";
        public const string Val46 = "VAL-MCU-46";

        /// <summary>Peserta perusahaan belum punya kartu penjamin yang berlaku pada tanggal check-in.</summary>
        public const string CompanyCardMissing = "MCU-COMPANY-CARD-MISSING";

        /// <summary>Klinik bertipe Medical Check-Up belum ada di master data.</summary>
        public const string ClinicMissing = "MCU-CLINIC-MISSING";

        /// <summary>Langkah modul mitra (Registrasi/Billing) menolak; pesannya diteruskan apa adanya.</summary>
        public const string PartnerRejected = "MCU-PARTNER-REJECTED";

        /// <summary>Perpindahan status yang tidak sah (<c>MCU-STATE-v1</c> bagian 1.2).</summary>
        public const string InvalidTransition = "MCU-INVALID-TRANSITION";

        /// <summary>Nomor order tidak dapat diterbitkan alokator Platform.</summary>
        public const string NumberAllocationFailed = "MCU-NUMBER-ALLOCATION-FAILED";

        /// <summary>Baris diubah petugas lain (<c>Version</c> berbeda) di luar pemakaian kuota.</summary>
        public const string ConcurrencyConflict = "MCU-CONCURRENCY";

        /// <summary>Isian formulir yang sudah dipakai hasil pemeriksaan tidak boleh berganti jenis.</summary>
        public const string FieldInUse = "MCU-FIELD-IN-USE";
    }
}
