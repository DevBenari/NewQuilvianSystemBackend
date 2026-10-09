namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Status satu versi dokumen admisi — state matrix 10.1 (<c>BE-RWI-192</c>,
    /// <c>RWI-DEC-240</c>). Hanya <see cref="Draft"/>, <see cref="AwaitingSignature"/>, dan
    /// <see cref="Completed"/> yang dihitung aktif (<c>UX_InpAdmissionDocument_Episode_Type_Active</c>).
    /// </summary>
    public enum InpAdmissionDocumentStatus
    {
        /// <summary>Konsep; isi masih boleh diubah.</summary>
        Draft = 1,

        /// <summary>Dikunci; isi dan angka sudah dibekukan, menunggu tanda tangan.</summary>
        AwaitingSignature = 2,

        /// <summary>Seluruh slot wajib terisi; isi tidak berubah lagi.</summary>
        Completed = 3,

        /// <summary>Digantikan versi koreksi; tetap terbaca di Riwayat.</summary>
        Superseded = 4,

        /// <summary>Konsep dibuang pembuatnya, atau dokumen dibatalkan beralasan.</summary>
        Cancelled = 5
    }
}
