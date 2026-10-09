using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    /// <summary>
    /// Jatuh tempo dan angka deposit beku pada Pelunasan Deposit — kamus data 20.10
    /// (<c>BE-RWI-192</c>, <c>RWI-DEC-231</c>, <c>248</c>, <c>263</c>).
    /// </summary>
    /// <remarks>
    /// Angka kosong selama <c>Draft</c> dan terisi dari Billing saat dikunci; angka tidak pernah
    /// diketik petugas (<c>INV-RWA-06</c>). Contoh: dikunci saat kurang Rp 3.000.000; kasir menerima
    /// Rp 1.000.000 sesudahnya; dokumen dan cetakannya tetap Rp 3.000.000.
    /// </remarks>
    public class InpAdmissionDepositStatement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        /// <summary>Tanggal pilihan petugas pukul 11.00 waktu rumah sakit, disimpan UTC.</summary>
        public DateTime? DueAt { get; set; }

        /// <summary>Interval kebijakan deposit yang membatasi jatuh tempo; dibekukan saat kunci.</summary>
        public int? PolicyFollowUpIntervalDays { get; set; }

        /// <summary>Dari Billing, dibekukan saat kunci. SENSITIF.</summary>
        public decimal? MinimumPolicyAmount { get; set; }

        /// <summary>SENSITIF.</summary>
        public decimal? ReceivedAmount { get; set; }

        /// <summary>Harus lebih dari 0 saat kunci. SENSITIF.</summary>
        public decimal? ShortfallAmount { get; set; }

        /// <summary>Waktu angka dibaca dari Billing.</summary>
        public DateTime? AmountsReadAt { get; set; }

        public InpAdmissionDocument? Document { get; set; }
    }
}
