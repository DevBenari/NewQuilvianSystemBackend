using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models
{
    /// <summary>
    /// Riwayat penutupan dan pembukaan kembali sebuah folio — <c>RJ-BIL-BE-006</c>.
    ///
    /// <c>RJ-BIL-GATE-DEC-006</c>: <i>"Reopen selalu controlled high-risk request dan
    /// mempertahankan closing history."</i> Kalimat terakhir itulah alasan tabel ini ada.
    /// Membuka kembali folio tidak menghapus fakta bahwa ia pernah ditutup, oleh siapa, kapan,
    /// dan atas dasar apa — baris penutupan tetap berdiri, dan pembukaan kembali menjadi baris
    /// berikutnya, bukan penghapusan baris sebelumnya.
    ///
    /// Setiap penutupan juga membekukan daftar penghalang yang <i>tidak</i> ada saat itu, lewat
    /// <see cref="ClosureEvidence"/>. Tanpa itu, pertanyaan <i>"kok waktu itu boleh ditutup?"</i>
    /// hanya bisa dijawab dengan tebakan.
    /// </summary>
    public class BilFolioClosureHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid FolioId { get; set; }

        public Guid EncounterId { get; set; }

        public BillingFolioClosureAction Action { get; set; }

        public BillingFolioStatus PriorStatus { get; set; }

        public BillingFolioStatus NewStatus { get; set; }

        public Guid PerformedByUserId { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }

        /// <summary>
        /// Permintaan tindakan finansial yang menjadi dasar pembukaan kembali.
        ///
        /// Wajib terisi untuk <see cref="BillingFolioClosureAction.Reopen"/> dan selalu kosong
        /// untuk penutupan: menutup folio yang memang sudah bersih bukan tindakan high-risk,
        /// sedangkan membukanya kembali selalu high-risk.
        /// </summary>
        public Guid? FinancialActionRequestId { get; set; }

        /// <summary>
        /// Ringkasan keadaan gerbang penutupan pada saat tindakan dilakukan, dibekukan sebagai
        /// teks. Bukan sumber kebenaran, melainkan bukti.
        /// </summary>
        public string? ClosureEvidence { get; set; }

        public Guid? CorrelationId { get; set; }

        public BilFolio Folio { get; set; } = null!;
    }
}
