using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants
{
    /// <summary>
    /// Peta kewenangan per jenis tindakan finansial — <c>RJ-BIL-GATE-DEC-006</c> butir <c>1</c>.
    ///
    /// Keputusan itu menuntut kemampuan yang <b>terpisah</b> untuk charge create/finalize,
    /// adjustment, void create/approve, reversal create/approve, refund create/approve/execute,
    /// waiver create/approve, write-off create/approve, financial-review resolve/approve, manual
    /// override, serta folio close/reopen.
    ///
    /// <para><b>Mengapa dipetakan, bukan ditempel sebagai atribut.</b></para>
    ///
    /// Endpoint pengajuan dan persetujuan bersifat umum: jenis tindakannya baru diketahui dari
    /// isi permintaan atau dari baris yang sudah tersimpan. Atribut permission hanya menjaga
    /// pintu masuk. Kewenangan sebenarnya karena itu diperiksa saat berjalan, terhadap jenis
    /// tindakan yang benar-benar dituju. Tanpa ini, seseorang yang hanya berhak membuat
    /// adjustment dapat menyetujui refund lewat endpoint yang sama.
    ///
    /// <para><b>Nama jabatan tidak pernah muncul di sini.</b></para>
    ///
    /// Butir <c>5</c> keputusan yang sama melarang organizational title di-hardcode. Yang ada di
    /// bawah hanyalah nama kemampuan; pemetaan role ke kemampuan itu urusan administrasi
    /// otorisasi, bukan urusan code ini.
    /// </summary>
    public static class BillingFinancialCapabilities
    {
        /// <summary>
        /// Nama controller yang dipakai administrasi otorisasi untuk seluruh kemampuan di bawah.
        /// </summary>
        public const string ControllerName = "BillingFinancialAction";

        public const string Read = "Read";

        /// <summary>
        /// Menutup folio. Dipisahkan dari <see cref="FolioReopen"/> karena keduanya jauh berbeda
        /// bobotnya: menutup folio yang sudah bersih adalah pekerjaan harian, sedangkan membuka
        /// kembali folio yang sudah tertutup selalu high-risk.
        /// </summary>
        public const string FolioClose = "FolioClose";

        public const string FolioReopen = "FolioReopenCreate";

        public static string CreateCapability(BillingFinancialActionType actionType) =>
            actionType switch
            {
                BillingFinancialActionType.Void => "VoidCreate",
                BillingFinancialActionType.Adjustment => "AdjustmentCreate",
                BillingFinancialActionType.Reversal => "ReversalCreate",
                BillingFinancialActionType.Refund => "RefundCreate",
                BillingFinancialActionType.Waiver => "WaiverCreate",
                BillingFinancialActionType.WriteOff => "WriteOffCreate",
                BillingFinancialActionType.ManualOverride => "ManualOverrideCreate",
                BillingFinancialActionType.FolioReopen => FolioReopen,
                _ => throw new ArgumentOutOfRangeException(nameof(actionType))
            };

        public static string ApproveCapability(BillingFinancialActionType actionType) =>
            actionType switch
            {
                BillingFinancialActionType.Void => "VoidApprove",
                BillingFinancialActionType.Adjustment => "AdjustmentApprove",
                BillingFinancialActionType.Reversal => "ReversalApprove",
                BillingFinancialActionType.Refund => "RefundApprove",
                BillingFinancialActionType.Waiver => "WaiverApprove",
                BillingFinancialActionType.WriteOff => "WriteOffApprove",
                BillingFinancialActionType.ManualOverride => "ManualOverrideApprove",
                BillingFinancialActionType.FolioReopen => "FolioReopenApprove",
                _ => throw new ArgumentOutOfRangeException(nameof(actionType))
            };

        /// <summary>
        /// Kewenangan yang dibutuhkan untuk <i>menjalankan</i> tindakan yang sudah disetujui.
        ///
        /// Hanya refund yang memiliki kemampuan eksekusi tersendiri, persis seperti yang
        /// dituliskan <c>RJ-BIL-GATE-DEC-006</c>: <i>"refund create/approve/execute"</i> —
        /// sementara jenis lain hanya disebut create/approve. Alasannya masuk akal: refund adalah
        /// satu-satunya yang mengeluarkan uang dari rumah sakit, sehingga menyetujuinya dan
        /// benar-benar mengeluarkannya sengaja dijadikan dua kemampuan.
        ///
        /// Untuk jenis lain, pelaksanaan mengikuti kemampuan pembuatnya. Ini tidak melonggarkan
        /// apa pun: eksekusi baru mungkin setelah checker yang berbeda orang menyetujuinya.
        /// </summary>
        public static string ExecuteCapability(BillingFinancialActionType actionType) =>
            actionType == BillingFinancialActionType.Refund
                ? "RefundExecute"
                : CreateCapability(actionType);
    }
}
