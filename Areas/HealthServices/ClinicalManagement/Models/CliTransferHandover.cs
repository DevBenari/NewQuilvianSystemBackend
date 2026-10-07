using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Dokumen serah terima klinis saat transfer antarunit, sembilan bagian V1 (<c>BE-RWI-183</c>,
    /// kamus data 19.9, <c>RWI-DEC-182</c>, <c>RWI-DEC-189</c>, <c>P2</c>).
    /// </summary>
    /// <remarks>
    /// Satu dokumen per penempatan tujuan (<c>UX_CliTransferHandover_ToPlacement</c>). Contoh
    /// <c>UAT-RWF-14</c>: Budi dipindah dari Melati (bangsal) ke ICU → satu dokumen "Belum dikirim"
    /// lahir sesudah transfer tersimpan; perpindahan bed di dalam Melati tidak membuat dokumen.
    /// GCS, tanda vital, nyeri, risiko jatuh, dan balance cairan tidak diketik — dibekukan dari
    /// pencatatan terakhir saat dikirim.
    /// </remarks>
    public class CliTransferHandover : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid InpEpisodeId { get; set; }

        public Guid FromPlacementId { get; set; }

        /// <summary>Unik: satu dokumen per penempatan tujuan (idempotensi <c>INT-RWF-26</c>).</summary>
        public Guid ToPlacementId { get; set; }

        public Guid FromServiceUnitId { get; set; }

        public Guid ToServiceUnitId { get; set; }

        public CliTransferHandoverStatus Status { get; set; } = CliTransferHandoverStatus.NotSent;

        /// <summary>Kondisi pasien. SENSITIF.</summary>
        public string? SoapSummary { get; set; }

        /// <summary>Barang yang diserahkan. SENSITIF.</summary>
        public string? HandedItems { get; set; }

        /// <summary>SENSITIF.</summary>
        public string? SpecialInstructions { get; set; }

        /// <summary>Potret klinis yang dibekukan saat dikirim beserta rujukan catatan sumbernya. SENSITIF.</summary>
        public string? SnapshotJson { get; set; }

        public Guid? SentByUserId { get; set; }

        public DateTime? SentAt { get; set; }

        /// <summary>Penerima atau penolak; selalu berbeda dari pengirim (<c>CK_CliTransferHandover_TwoAccounts</c>).</summary>
        public Guid? ReceivedByUserId { get; set; }

        public DateTime? ReceivedAt { get; set; }

        /// <summary>Alasan penolakan terakhir. SENSITIF.</summary>
        public string? RejectionReason { get; set; }

        /// <summary>Konkurensi optimistis.</summary>
        public int Version { get; set; }

        public InpEpisode? Episode { get; set; }

        public InpBedPlacement? FromPlacement { get; set; }

        public InpBedPlacement? ToPlacement { get; set; }

        public MstServiceUnit? FromServiceUnit { get; set; }

        public MstServiceUnit? ToServiceUnit { get; set; }
    }
}
