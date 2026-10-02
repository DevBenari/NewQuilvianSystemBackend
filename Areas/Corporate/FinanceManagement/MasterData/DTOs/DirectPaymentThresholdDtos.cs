using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;

/// <summary>Mengubah (atau menetapkan untuk pertama kali) ambang pembayaran langsung (BE-FIN-076, FIN-DEC-134).</summary>
public sealed class UpdateDirectPaymentThresholdRequest
{
    public decimal Amount { get; set; }

    /// <summary>
    /// Wajib setiap kali diubah (FIN-DEC-134, FIN-VAL-205). SENGAJA TIDAK diberi <c>[Required]</c> —
    /// atribut itu akan membuat pipeline validasi otomatis [ApiController] memotong permintaan
    /// dengan 400 generik sebelum sempat dijawab 422 + pesan kontrak yang benar. Pemeriksaan
    /// kosong/spasi dilakukan manual di DirectPaymentThresholdService.
    /// </summary>
    [MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;

    public DateOnly EffectiveFrom { get; set; }
}

/// <summary>Ambang aktif beserta alasan dan jejak perubahan terakhirnya (FIN-API-1.6 F.4).</summary>
public sealed class DirectPaymentThresholdResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public Guid LastChangedBy { get; set; }
    public DateTime LastChangedAt { get; set; }
}

/// <summary>FIN-VAL-205: ChangeReason kosong saat mengubah ambang — MUST dijawab 422.</summary>
public sealed class DirectPaymentThresholdValidationException(string message) : Exception(message);

/// <summary>FIN-VAL-206: nilai ambang nol atau negatif — MUST dijawab 400.</summary>
public sealed class DirectPaymentThresholdBadRequestException(string message) : Exception(message);
