using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    /// <summary>
    /// Angka-angka kebijakan rekonsiliasi, satu baris untuk setiap jenis case.
    ///
    /// Ketiga angka di sini — ambang materialitas, durasi SLA, dan prioritas bawaan — tidak
    /// disebutkan nilainya di mana pun pada <c>RJ-BIL-GATE-DEC-008</c>. Keputusan
    /// <c>RJ-BIL-DEC-010</c> menetapkan bahwa ketiganya menjadi master data yang dapat diubah
    /// admin tanpa rilis, bukan angka yang ditanam di kode.
    ///
    /// Alasannya sama dengan alasan pada alasan penolakan sampel <c>RJ-BIL-BE-003</c>: bila
    /// rumah sakit kelak memutuskan kegagalan di bawah Rp50.000 tidak perlu menahan penutupan
    /// folio, perubahan itu seharusnya satu baris yang diubah admin, bukan perubahan kode,
    /// build ulang, dan penurunan aplikasi.
    ///
    /// Nilai awal <see cref="MaterialityThresholdAmount"/> adalah <c>0</c>, yang berarti setiap
    /// kegagalan menahan penutupan folio. Nol dipilih karena merupakan perilaku paling aman dan
    /// bukan angka karangan; angka sebenarnya masih menunggu keputusan pemilik.
    /// </summary>
    public class MstBillingReconciliationPolicy : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public BillingReconciliationCaseType CaseType { get; set; }

        /// <summary>
        /// Batas nilai rupiah sebuah kegagalan dianggap <i>financially material</i> sehingga
        /// menahan penutupan folio. Case dengan dampak <b>lebih besar dari</b> ambang ini
        /// menahan penutupan. Ambang <c>0</c> berarti setiap dampak di atas nol menahan.
        /// </summary>
        public decimal MaterialityThresholdAmount { get; set; }

        /// <summary>
        /// Umur case sebelum dinyatakan melampaui SLA. Pelampauan hanya memicu peringatan dan
        /// eskalasi, tidak pernah menyelesaikan case dengan sendirinya.
        /// </summary>
        public int SlaMinutes { get; set; }

        public BillingReconciliationPriority DefaultPriority { get; set; } =
            BillingReconciliationPriority.Normal;

        /// <summary>
        /// Apakah duplikat deterministik tanpa dampak boleh diselesaikan otomatis.
        /// <c>RJ-BIL-GATE-DEC-008</c> mengizinkannya, tetapi hanya untuk kasus yang benar-benar
        /// deterministik dan tanpa dampak finansial.
        /// </summary>
        public bool AllowAutoResolveDeterministicDuplicate { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }
    }
}
