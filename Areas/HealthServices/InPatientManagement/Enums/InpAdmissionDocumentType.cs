namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Jenis dokumen admisi Workspace PPRI — kontrak <c>episode-rawat-inap</c> <c>0.11.0</c>
    /// backend 13.9 (<c>BE-RWI-192</c>). General Consent, Gelang, Label, dan IPD sengaja tidak
    /// menjadi jenis dokumen: General Consent hanya dicetak selama <i>fail-closed</i>
    /// (<c>RWI-DEC-230</c>, <c>233</c>), sedangkan cetakan lain hanya meninggalkan log cetak.
    /// </summary>
    public enum InpAdmissionDocumentType
    {
        /// <summary>Ceklist Serah Terima Pasien Baru.</summary>
        NewPatientHandover = 1,

        /// <summary>Formulir Permintaan Privasi.</summary>
        PrivacyRequest = 2,

        /// <summary>Formulir Identifikasi Nilai-Nilai dan Kepercayaan Pasien.</summary>
        BeliefValues = 3,

        /// <summary>Surat Pernyataan Kesediaan Menanggung Biaya Sendiri (Selisih Biaya).</summary>
        CostDifferenceStatement = 4,

        /// <summary>Pernyataan Kesediaan Melunaskan Deposit.</summary>
        DepositSettlementStatement = 5,

        /// <summary>Estimasi Biaya Rekap — di luar gelombang sampai <c>DEC-INP-020</c> (<c>EPIC-RWA-09</c>).</summary>
        CostEstimate = 6
    }
}
