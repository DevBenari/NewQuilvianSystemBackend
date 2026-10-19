using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

/// <summary>
/// Hasil operasi penahanan stok resep (<c>PHA-DEC-066</c>).
/// </summary>
/// <remarks>
/// Kegagalan dinyatakan sebagai hasil, bukan exception, karena kekurangan stok dan penahanan
/// yang kedaluwarsa adalah <b>keadaan bisnis normal</b>. <c>PHA-DEC-066</c> menuntut keadaan
/// seperti itu menghasilkan status eksplisit, bukan <c>HTTP 500</c> generik.
/// </remarks>
public sealed class PrescriptionReservationResult
{
    public bool IsSuccess { get; private init; }

    /// <summary>
    /// Benar bila penahanan sudah ada sebelum pemanggilan ini — retry finalisasi atau callback
    /// pembayaran yang diulang. Pemanggil tidak perlu membedakannya untuk menentukan lanjut
    /// atau tidak, tetapi membutuhkannya untuk tidak mencatat peristiwa dua kali.
    /// </summary>
    public bool WasAlreadyReserved { get; private init; }

    public string Code { get; private init; } = string.Empty;

    public string Message { get; private init; } = string.Empty;

    public Guid? StorageLocationId { get; private init; }

    public DateTime? ExpiresAt { get; private init; }

    public IReadOnlyList<PhmPrescriptionStockReservation> Reservations { get; private init; }
        = Array.Empty<PhmPrescriptionStockReservation>();

    public decimal TotalQuantity => Reservations.Sum(x => x.Quantity);

    public static PrescriptionReservationResult Reserved(
        IReadOnlyList<PhmPrescriptionStockReservation> reservations,
        Guid? storageLocationId,
        DateTime? expiresAt)
        => new()
        {
            IsSuccess = true,
            Code = "PHA_RSV_RESERVED",
            Message = "Stok resep berhasil ditahan.",
            Reservations = reservations,
            StorageLocationId = storageLocationId,
            ExpiresAt = expiresAt
        };

    public static PrescriptionReservationResult AlreadyReserved(
        IReadOnlyList<PhmPrescriptionStockReservation> reservations)
        => new()
        {
            IsSuccess = true,
            WasAlreadyReserved = true,
            Code = "PHA_RSV_ALREADY_RESERVED",
            Message = "Stok resep sudah ditahan sebelumnya.",
            Reservations = reservations,
            StorageLocationId = reservations.Count > 0 ? reservations[0].StorageLocationId : null,
            ExpiresAt = reservations.Count > 0 ? reservations[0].ExpiresAt : null
        };

    public static PrescriptionReservationResult Failure(string code, string message)
        => new() { IsSuccess = false, Code = code, Message = message };
}

/// <summary>
/// Keadaan penahanan stok sebuah resep sebagaimana dibaca layar Farmasi
/// (<c>PHA-FE-001</c>).
/// </summary>
/// <remarks>
/// Hanya keterangan. Layar tidak diberi satu pun jalan mengubah penahanan, karena
/// <c>PHA-DEC-066</c> butir 5 menetapkan pelepasan dilakukan sistem dan butir 1 menetapkan
/// penahanan dibuat saat finalisasi oleh dokter.
/// </remarks>
public sealed class PrescriptionStockReservationStateResponse
{
    public Guid PrescriptionId { get; set; }

    /// <summary>Depo pelayanan authoritative hasil <c>PHA-DEPOT-ROUTING-v1</c>.</summary>
    public Guid? StorageLocationId { get; set; }

    public string StorageLocationName { get; set; } = string.Empty;

    /// <summary>
    /// Keadaan ringkas resep: <c>RESERVED</c>, <c>CONSUMED</c>, <c>RELEASED</c>,
    /// <c>EXPIRED</c>, atau <c>NOT_RESERVED</c> bila resep tidak pernah memegang penahanan.
    /// </summary>
    public string State { get; set; } = "NOT_RESERVED";

    public decimal ReservedQuantity { get; set; }

    /// <summary>Batas waktu sebelum pembayaran valid; <c>null</c> bila tidak berlaku.</summary>
    public DateTime? ExpiresAt { get; set; }

    public DateTime? ReservedAt { get; set; }

    public DateTime? ConsumedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    public string ReleaseReasonCode { get; set; } = string.Empty;

    /// <summary>
    /// Kode dan kalimat alasan ketika pekerjaan Farmasi tidak dapat dilanjutkan. Kosong ketika
    /// alur normal, supaya layar tidak memunculkan peringatan palsu.
    /// </summary>
    public string BlockReasonCode { get; set; } = string.Empty;

    public string BlockReason { get; set; } = string.Empty;

    public bool CanProceed { get; set; }

    public List<PrescriptionStockReservationLineResponse> Lines { get; set; } = [];
}

public sealed class PrescriptionStockReservationLineResponse
{
    public Guid DrugId { get; set; }
    public string DrugName { get; set; } = string.Empty;
    public Guid DrugBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public PrescriptionReservationState State { get; set; }
    public string StateLabel { get; set; } = string.Empty;
}
