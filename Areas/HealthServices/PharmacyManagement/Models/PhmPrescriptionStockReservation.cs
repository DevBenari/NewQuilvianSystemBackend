using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Satu baris penahanan stok milik sebuah resep, pada satu batch di satu depo.
/// </summary>
/// <remarks>
/// <para>
/// <c>PHA-DEC-066</c> menetapkan reservasi terjadi saat dokter memfinalkan resep, dan reservasi
/// itu <b>milik resep tersebut</b> sampai dikonsumsi, dilepas, atau kedaluwarsa.
/// </para>
/// <para>
/// <b>Mengapa entitas ini perlu ada, padahal reservasi sudah "ada".</b> Sebelum keputusan ini,
/// satu-satunya jejak reservasi adalah kolom penghitung
/// <c>PhmDrugStockBalance.QuantityReserved</c>. Penghitung itu menjawab <i>berapa</i> yang
/// ditahan, tetapi tidak dapat menjawab <i>milik siapa</i>, <i>sampai kapan</i>, <i>sudah
/// dikonsumsi atau belum</i>, dan <i>siapa yang melepasnya dengan alasan apa</i>. Seluruh
/// kewajiban <c>PHA-DEC-066</c> — kepemilikan oleh resep, TTL rawat jalan, pelepasan oleh
/// sistem, konsumsi sekali saja, dan audit trail — tidak dapat dinyatakan oleh sebuah
/// penghitung. Karena itu entitas ini ditambahkan, dan <b>penghitungnya tetap dipakai apa
/// adanya</b> sebagai sumber kebenaran jumlah: entitas ini mencatat kepemilikan dan
/// lifecycle-nya, bukan menggandakan saldo.
/// </para>
/// <para>
/// Granularitasnya per <b>batch</b>, karena FEFO menyebar satu permintaan ke beberapa batch.
/// Satu baris resep karena itu dapat memiliki beberapa baris di sini.
/// </para>
/// </remarks>
public class PhmPrescriptionStockReservation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid PrescriptionId { get; set; }

    /// <summary>
    /// Baris resep pemilik penahanan. Kosong untuk bahan racikan, yang memakai
    /// <see cref="PrescriptionCompoundItemId"/>.
    /// </summary>
    public Guid? PrescriptionItemId { get; set; }

    public Guid? PrescriptionCompoundItemId { get; set; }

    [Required] public Guid DrugId { get; set; }

    [Required] public Guid DrugBatchId { get; set; }

    /// <summary>
    /// Depo pelayanan authoritative hasil <c>PHA-DEPOT-ROUTING-v1</c>. Disimpan supaya
    /// pelepasan dan konsumsi tidak perlu menghitung ulang routing, yang jawabannya dapat
    /// berubah bila master depo berubah.
    /// </summary>
    [Required] public Guid StorageLocationId { get; set; }

    public decimal Quantity { get; set; }

    public PrescriptionReservationState State { get; set; } = PrescriptionReservationState.Reserved;

    public DateTime ReservedAt { get; set; }

    /// <summary>
    /// Batas waktu penahanan sebelum pembayaran valid. <c>null</c> berarti tidak ada batas:
    /// rawat inap dan IGD (<c>PHA-DEC-066</c> butir 8), atau resep yang pembayarannya sudah
    /// valid (butir 7).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    public DateTime? ConsumedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    /// <summary>
    /// Alasan pelepasan memakai konstanta <c>PrescriptionReservationReleaseReasons</c>, bukan
    /// kalimat bebas, supaya dapat dihitung pada laporan.
    /// </summary>
    [MaxLength(64)] public string? ReleaseReasonCode { get; set; }

    /// <summary>Aktor yang menyebabkan perubahan keadaan terakhir; <c>null</c> bila sistem.</summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>Peristiwa atau proses pemicu, misalnya <c>CONSULTATION_FINALIZATION</c>.</summary>
    [MaxLength(64)] public string SourceEvent { get; set; } = string.Empty;

    /// <summary>
    /// Kunci idempotency tingkat resep. Finalisasi yang diulang memakai kunci yang sama,
    /// sehingga tidak melahirkan penahanan kedua.
    /// </summary>
    [MaxLength(128)] public string IdempotencyKey { get; set; } = string.Empty;

    public PhmPrescription? Prescription { get; set; }
    public PhmPrescriptionItem? PrescriptionItem { get; set; }
    public MstDrug? Drug { get; set; }
    public PhmDrugBatch? DrugBatch { get; set; }
    public MstDrugStorageLocation? StorageLocation { get; set; }
}
