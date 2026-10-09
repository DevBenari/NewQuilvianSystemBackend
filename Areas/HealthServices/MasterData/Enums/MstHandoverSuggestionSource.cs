namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums
{
    /// <summary>
    /// Sumber saran sistem "Sudah" pada butir Serah Terima Pasien Baru — kontrak
    /// <c>episode-rawat-inap</c> <c>0.11.0</c> backend 13.9 (<c>BE-RWI-185</c>, <c>RWI-DEC-241</c>
    /// butir 2, <c>RWI-DEC-262</c>).
    /// </summary>
    /// <remarks>
    /// Saran hanya membantu petugas memilih; ia tidak pernah memilih otomatis dan tidak disimpan.
    /// Saran dibaca dari kolom ini, bukan dari nomor butir, supaya admin bebas mengurutkan ulang
    /// butirnya. Hanya berlaku untuk butir jenis serah terima; butir penutupan selalu
    /// <see cref="None"/> (<c>CK_MstInpatientClearanceItem_Type</c>).
    /// </remarks>
    public enum MstHandoverSuggestionSource
    {
        /// <summary>Tanpa saran.</summary>
        None = 0,

        /// <summary>Surat Pengantar Rawat Inap berstatus terbit pada kunjungan episode.</summary>
        ReferralLetter = 1,

        /// <summary>Estimasi Biaya lengkap — diam sampai <c>EPIC-RWA-09</c> dikirim.</summary>
        CostEstimateCompleted = 2,

        /// <summary>Pernyataan Pelunasan Deposit lengkap.</summary>
        DepositStatementCompleted = 3,

        /// <summary>IPD (Data Dasar Rawat Inap) sudah dicetak.</summary>
        BaseDataPrinted = 4,

        /// <summary>
        /// Label, stiker, dan General Consent. Tidak memberi saran selama General Consent
        /// <i>fail-closed</i> (<c>RWI-DEC-230</c>), karena cetak persetujuan tidak meninggalkan rekaman.
        /// </summary>
        LabelAndGeneralConsent = 5,

        /// <summary>Gelang identitas sudah dicetak.</summary>
        WristbandPrinted = 6
    }
}
