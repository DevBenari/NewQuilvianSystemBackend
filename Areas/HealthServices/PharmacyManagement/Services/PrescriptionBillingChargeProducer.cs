using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Mengirimkan tagihan obat satu resep ke invoice Billing pada saat resep difinalkan dokter
/// (tahap pemenuhan 1 → 2), sehingga tagihannya sudah berdiri ketika pasien ke kasir.
/// </summary>
/// <remarks>
/// <para>
/// Sebelum ini tidak ada satu pun kode di Farmasi yang mengirim charge obat ke
/// <c>BilInvoiceItems</c>. Fakta klinis memang sudah diterbitkan
/// <c>ClinicalMilestoneFactProducer</c>, tetapi fakta itu bermuara ke ledger folio
/// (<c>BilFolio</c>/<c>BilChargeLine</c>) — ledger yang berbeda dari invoice. Surat financial
/// clearance (<c>BilPrescriptionClearanceHandoff</c>) membaca <c>BilInvoiceItems</c>, jadi
/// selama item itu tidak pernah ada, surat clearance tidak pernah terbit dan resep tertahan
/// selamanya pada tahap 2. Produser inilah yang menutup celah itu.
/// </para>
/// <para>
/// Satu resep menjadi <b>satu</b> baris invoice, dengan <c>SourceDetailId</c> = PrescriptionId.
/// Bentuk itu wajib, bukan pilihan gaya: penargetan surat clearance di
/// <c>BilConsumerHandoffService</c> menerjemahkan <c>SourceDetailId</c> kembali menjadi
/// PrescriptionId. Memecahnya per obat akan membuat surat tidak menemukan resepnya.
/// </para>
/// <para>
/// Kegagalan pengiriman <b>tidak</b> membatalkan finalisasi konsultasi. Konsultasi yang sudah sah
/// secara klinis tidak boleh runtuh karena Billing sedang bermasalah; yang terjadi hanyalah
/// tagihannya belum berdiri, dan itu dilaporkan sebagai keluhan agar dapat diulang.
/// </para>
/// </remarks>
public sealed class PrescriptionBillingChargeProducer
{
    /// <summary>Status sumber yang dipakai saat resep baru difinalkan, belum diserahkan.</summary>
    public const string SubmittedSourceStatus = "SUBMITTED";

    /// <summary>Versi kontrak charge yang masih diterima <c>ContractBillingChargeSourceAdapter</c>.</summary>
    public const string ContractVersion = "BIL-INTEGRATION-1.2";

    /// <summary>
    /// Revisi pertama baris tagihan. Finalisasi hanya terjadi sekali per resep —
    /// <see cref="PrescriptionWorkflowService.FinalizeFromConsultationAsync"/> menolak yang kedua —
    /// sehingga pengiriman dari sini selalu revisi 1. Perubahan jumlah saat penyerahan kelak
    /// mengirim revisi yang lebih tinggi pada baris yang sama.
    /// </summary>
    private const long InitialSourceVersion = 1;

    private readonly ApplicationDbContext _dbContext;
    private readonly BillingInvoiceService _invoiceService;

    public PrescriptionBillingChargeProducer(
        ApplicationDbContext dbContext,
        BillingInvoiceService invoiceService)
    {
        _dbContext = dbContext;
        _invoiceService = invoiceService;
    }

    /// <summary>
    /// Kunci idempotensi yang diturunkan dari identitas resep dan revisinya.
    /// </summary>
    /// <remarks>
    /// Deterministik dengan sengaja. Bila finalisasi yang sama terkirim dua kali — permintaan
    /// diulang, jaringan terputus di tengah, pengguna menekan dua kali — kunci yang dihasilkan
    /// sama, sehingga <c>BilChargeReceipts</c> mengenalinya sebagai pengiriman ulang dan tidak
    /// ada baris tagihan kedua yang lahir.
    /// </remarks>
    public static Guid DeterministicIdempotencyKey(Guid prescriptionId, long sourceVersion) =>
        new(SHA256.HashData(
            Encoding.UTF8.GetBytes($"phm-charge|{prescriptionId:D}|{sourceVersion}"))[..16]);

    /// <summary>
    /// Mengirim tagihan satu resep yang baru difinalkan. Mengembalikan alasan kegagalan, atau
    /// <c>null</c> bila tagihannya berhasil berdiri.
    /// </summary>
    public async Task<string?> SendForFinalizedPrescriptionAsync(
        PhmPrescription prescription,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prescription);

        if (prescription.TotalItemCount <= 0)
            return "PHM_CHARGE_NO_ITEM";

        // Harga resep dikirim apa adanya sebagai satu baris. Angkanya sudah dihitung
        // PrescriptionAggregateService dari harga tiap obat sebelum finalisasi; produser ini
        // tidak menghitung ulang dan tidak menambah komponen apa pun.
        if (prescription.TotalPrice <= 0)
            return "PHM_CHARGE_NO_PRICE";

        var categoryId = await ResolvePharmacyCategoryIdAsync(cancellationToken);
        if (categoryId == null)
            return "PHM_CHARGE_NO_PHARMACY_CATEGORY";

        var request = new UpsertChargeRequest
        {
            EncounterId = prescription.EncounterId,
            SourceDomain = "PHARMACY",
            SourceDetailId = prescription.Id.ToString("D"),
            SourceVersion = InitialSourceVersion,
            SourceStatus = SubmittedSourceStatus,
            OccurredAt = occurredAt,
            CategoryId = categoryId.Value,
            DescriptionSnapshot = BuildDescription(prescription),
            Quantity = 1,
            UnitPrice = prescription.TotalPrice,
            DoctorShare = 0,
            ContractVersion = ContractVersion,
            CorrelationId = correlationId,
            CausationId = prescription.Id
        };

        try
        {
            await _invoiceService.UpsertChargeAsync(
                request,
                DeterministicIdempotencyKey(prescription.Id, InitialSourceVersion),
                actorUserId,
                cancellationToken);

            return null;
        }
        catch (Exception exception)
        {
            // Sebabnya dibawa apa adanya ke pemanggil supaya terbaca petugas, bukan ditelan.
            return $"PHM_CHARGE_FAILED: {exception.Message}";
        }
    }

    /// <summary>
    /// Mencari kategori tarif yang ditandai farmasi. Tidak pernah menebak: bila tidak ada satu
    /// pun kategori bertanda <c>IsPharmacy</c>, pengiriman dilaporkan gagal, karena memilih
    /// kategori yang salah berarti obat tertagih pada kelompok tagihan yang salah.
    /// </summary>
    private async Task<Guid?> ResolvePharmacyCategoryIdAsync(CancellationToken cancellationToken)
    {
        var category = await _dbContext.MstTariffCategories.AsNoTracking()
            .Where(x => x.IsPharmacy && x.IsActive && !x.IsDelete && !x.IsCancel)
            .OrderBy(x => x.SortOrder)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return category;
    }

    private static string BuildDescription(PhmPrescription prescription)
    {
        var nomor = string.IsNullOrWhiteSpace(prescription.PrescriptionNumber)
            ? prescription.Id.ToString("D")
            : prescription.PrescriptionNumber.Trim();

        var keterangan = $"Obat resep {nomor} ({prescription.TotalItemCount} item)";

        // Kolom deskripsi dibatasi 250 karakter oleh kontraknya.
        return keterangan.Length <= 250 ? keterangan : keterangan[..250];
    }
}
