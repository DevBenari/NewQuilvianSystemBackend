using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Globalization;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Koreksi Informasi Specimen dari halaman hasil, beserta jejaknya
    /// (<c>LAB-DEC-107</c>, <c>LAB-DEC-112</c>, <c>BE-LAB-57</c>).
    ///
    /// <b>Berdiri terpisah dari <c>LabSpecimenService</c> yang sudah lebih dari 2.100 baris.</b>
    /// Controller-nya tetap satu — yang dipisah implementasinya, bukan permukaannya.
    ///
    /// <b>Kenapa koreksi dibuka sama sekali.</b> Menutupnya mengulang jalan buntu yang persis
    /// dihindari <c>LAB-DEC-040</c>: satu salah pilih saat penerimaan menahan pengisian hasil
    /// sampai petugas penerimaan tersedia — dan pada shift malam ia mungkin tidak ada. Yang
    /// tertahan bukan formulir, melainkan pekerjaan atas bahan yang sudah terlanjur diambil
    /// dari tubuh pasien.
    /// </summary>
    public class LabSpecimenCorrectionService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";
        private const string EntityName = nameof(LabSpecimen);

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;
        private readonly LabFieldChangeRecorder _recorder;

        public LabSpecimenCorrectionService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService,
            LabFieldChangeRecorder recorder)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
            _recorder = recorder;
        }

        /// <summary>
        /// Menerapkan koreksi. <b>Utuh atau nol</b> — seluruhnya di dalam satu transaksi.
        /// </summary>
        public async Task<int> ApplyCorrectionAsync(
            Guid specimenId,
            LabSpecimenCorrectionRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var specimen = await _dbContext.LabSpecimens
                .Include(x => x.SpecimenType)
                .Include(x => x.VolumeUnit)
                .FirstOrDefaultAsync(x => x.Id == specimenId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Wadah specimen tidak ditemukan.");

            // VAL-109. Sesudah hasil dinyatakan selesai ditulis, Informasi Specimen menjadi
            // baca-saja (LAB-DEC-107 butir 3). Yang menahan adalah FinalizedAt pada salah satu
            // pemeriksaan yang memakai wadah ini.
            var adaHasilFinal = await _dbContext.LabExaminations
                .AsNoTracking()
                .AnyAsync(x => x.SpecimenId == specimenId && !x.IsDelete && x.FinalizedAt != null,
                    cancellationToken);

            if (adaHasilFinal)
            {
                throw new LabSpecimenCorrectionValidationException(
                    "Hasil sudah dinyatakan selesai; buka kembali hasilnya lebih dulu sebelum mengoreksi specimen.");
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var perubahan = new List<(string, string?, string?)>();

                await ApplySpecimenTypeAsync(specimen, request, perubahan, cancellationToken);
                await ApplyVolumeAsync(specimen, request, perubahan, cancellationToken);
                ApplyDescription(specimen, request, perubahan);
                ApplyPhysicallyReceivedAt(specimen, request, perubahan, now);

                var jumlahRincian = await ApplyDetailsAsync(
                    specimen, request, perubahan, now, actorUserId, cancellationToken);

                var jumlahRuas = _recorder.Record(EntityName, specimen.Id, perubahan, actorUserId, now);

                if (jumlahRuas > 0 || jumlahRincian > 0)
                {
                    specimen.UpdateDateTime = now;
                    specimen.UpdateBy = actorUserId;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // Jejaknya ditulis ke LabFieldChangeLog, BUKAN ke LabTransitionHistory —
                // LAB-DEC-112 memisahkan keduanya, dan AC-175 mengujinya langsung.
                await _loggerService.AuditAsync(
                    LogCategory,
                    "LabSpecimen.ApplyCorrection",
                    "Mengoreksi Informasi Specimen dari halaman hasil.",
                    new { specimen.Id, JumlahRuasBerubah = jumlahRuas, JumlahRincian = jumlahRincian });

                return jumlahRuas;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<List<LabFieldChangeResponse>> GetFieldChangesAsync(
            Guid specimenId,
            CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.LabSpecimens
                .AsNoTracking()
                .AnyAsync(x => x.Id == specimenId && !x.IsDelete, cancellationToken);

            if (!ada)
                throw new KeyNotFoundException("Wadah specimen tidak ditemukan.");

            var rows = await _dbContext.LabFieldChangeLogs
                .AsNoTracking()
                .Where(x => x.EntityName == EntityName && x.EntityId == specimenId && !x.IsDelete)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync(cancellationToken);

            return [.. rows.Select(x => new LabFieldChangeResponse
            {
                FieldName = x.FieldName,
                FieldLabel = ResolveLabel(x.FieldName),
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                ChangedByUserId = x.ChangedByUserId,
                ChangedAt = x.ChangedAt
            })];
        }

        private async Task ApplySpecimenTypeAsync(
            LabSpecimen specimen,
            LabSpecimenCorrectionRequest request,
            List<(string, string?, string?)> perubahan,
            CancellationToken cancellationToken)
        {
            if (!request.SpecimenTypeId.HasValue)
                return;

            var jenis = await _dbContext.LabSpecimenTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.SpecimenTypeId.Value && !x.IsDelete && x.IsActive,
                    cancellationToken)
                ?? throw new LabSpecimenCorrectionValidationException(
                    "Jenis specimen yang dipilih tidak berlaku.");

            var keterangan = Normalize(request.SpecimenTypeOtherNote);

            // VAL-104. Menegakkan LAB-DEC-040 butir 3: sampel yang jenisnya belum terdaftar
            // tidak boleh menghalangi penerimaan, TETAPI jalan keluarnya wajib berketerangan
            // supaya daftar pantau kepala instalasi punya isi.
            if (jenis.IsOtherBucket && string.IsNullOrEmpty(keterangan))
            {
                throw new LabSpecimenCorrectionValidationException(
                    "Jenis specimen Lainnya wajib disertai keterangan.");
            }

            perubahan.Add(("SpecimenTypeId",
                specimen.SpecimenType?.SpecimenTypeName,
                jenis.SpecimenTypeName));

            perubahan.Add(("SpecimenTypeOtherNote", specimen.SpecimenTypeOtherNote, keterangan));

            specimen.SpecimenTypeId = jenis.Id;
            specimen.SpecimenTypeOtherNote = jenis.IsOtherBucket ? keterangan : null;
        }

        private async Task ApplyVolumeAsync(
            LabSpecimen specimen,
            LabSpecimenCorrectionRequest request,
            List<(string, string?, string?)> perubahan,
            CancellationToken cancellationToken)
        {
            if (request.VolumeAmount.HasValue)
            {
                perubahan.Add(("VolumeAmount",
                    FormatVolume(specimen.VolumeAmount),
                    FormatVolume(request.VolumeAmount)));

                specimen.VolumeAmount = request.VolumeAmount;
            }

            if (!request.VolumeUnitId.HasValue)
                return;

            // VAL-110.
            var satuan = await _dbContext.MstMeasurements
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.VolumeUnitId.Value && !x.IsDelete && x.IsForLaboratory,
                    cancellationToken)
                ?? throw new LabSpecimenCorrectionValidationException(
                    "Satuan itu tidak berlaku untuk specimen laboratorium.");

            perubahan.Add(("VolumeUnitId",
                specimen.VolumeUnit?.MeasurementSymbol ?? specimen.VolumeUnit?.MeasurementName,
                satuan.MeasurementSymbol ?? satuan.MeasurementName));

            specimen.VolumeUnitId = satuan.Id;
        }

        private static void ApplyDescription(
            LabSpecimen specimen,
            LabSpecimenCorrectionRequest request,
            List<(string, string?, string?)> perubahan)
        {
            if (request.SpecimenDescription is null)
                return;

            var baru = Normalize(request.SpecimenDescription);

            if (baru is { Length: > 500 })
            {
                throw new LabSpecimenCorrectionValidationException(
                    "Keterangan specimen paling panjang 500 karakter.");
            }

            perubahan.Add(("SpecimenDescription", specimen.SpecimenDescription, baru));

            specimen.SpecimenDescription = baru;
        }

        private static void ApplyPhysicallyReceivedAt(
            LabSpecimen specimen,
            LabSpecimenCorrectionRequest request,
            List<(string, string?, string?)> perubahan,
            DateTime now)
        {
            if (!request.PhysicallyReceivedAt.HasValue)
                return;

            // BR-37. Waktu nyata tidak boleh berada di masa depan, dan tidak boleh mendahului
            // waktu pengambilan.
            if (request.PhysicallyReceivedAt.Value > now)
            {
                throw new LabSpecimenCorrectionValidationException(
                    "Waktu penerimaan fisik tidak boleh melewati waktu sekarang.");
            }

            if (specimen.CollectedAt.HasValue && request.PhysicallyReceivedAt.Value < specimen.CollectedAt.Value)
            {
                throw new LabSpecimenCorrectionValidationException(
                    "Waktu penerimaan fisik tidak boleh mendahului waktu pengambilan specimen.");
            }

            perubahan.Add(("PhysicallyReceivedAt",
                specimen.PhysicallyReceivedAt?.ToString("O", CultureInfo.InvariantCulture),
                request.PhysicallyReceivedAt.Value.ToString("O", CultureInfo.InvariantCulture)));

            specimen.PhysicallyReceivedAt = request.PhysicallyReceivedAt;
        }

        /// <summary>
        /// Mengganti <b>seluruh</b> Spesifik Specimen wadah ini. Perubahannya dicatat sebagai
        /// satu baris jejak berisi daftar nama lama dan baru — bukan satu baris per rincian,
        /// sebab yang berubah adalah <i>pilihannya sebagai satu kesatuan</i>.
        /// </summary>
        private async Task<int> ApplyDetailsAsync(
            LabSpecimen specimen,
            LabSpecimenCorrectionRequest request,
            List<(string, string?, string?)> perubahan,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (request.DetailTypeIds is null)
                return 0;

            var lama = await _dbContext.LabSpecimenDetails
                .Where(x => x.LabSpecimenId == specimen.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var namaLama = string.Join(", ", lama.Select(x => x.DetailNameSnapshot).OrderBy(x => x));

            _dbContext.LabSpecimenDetails.RemoveRange(lama);

            var ids = request.DetailTypeIds.Distinct().ToList();

            // VAL-105 ditegakkan lewat Distinct di atas beserta index unik parsial pada
            // tabelnya — dua lapis, sebab pengiriman ganda tidak boleh lolos ke database.
            if (ids.Count != request.DetailTypeIds.Count)
            {
                throw new LabSpecimenCorrectionValidationException(
                    "Rincian specimen yang sama tidak boleh dipilih dua kali.");
            }

            var namaBaruList = new List<string>();

            if (ids.Count > 0)
            {
                var rincian = await _dbContext.LabSpecimenDetailTypes
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.Id) && !x.IsDelete && x.IsActive)
                    .ToListAsync(cancellationToken);

                if (rincian.Count != ids.Count)
                {
                    throw new LabSpecimenCorrectionValidationException(
                        "Sebagian rincian specimen yang dipilih sudah tidak dipakai lagi.");
                }

                foreach (var r in rincian)
                {
                    var nama = string.IsNullOrWhiteSpace(r.DetailTypeNameId)
                        ? r.DetailTypeNameEn
                        : r.DetailTypeNameId;

                    namaBaruList.Add(nama);

                    _dbContext.LabSpecimenDetails.Add(new LabSpecimenDetail
                    {
                        LabSpecimenId = specimen.Id,
                        LabSpecimenDetailTypeId = r.Id,
                        DetailNameSnapshot = nama,
                        CreateDateTime = now,
                        CreateBy = actorUserId
                    });
                }
            }

            var namaBaru = string.Join(", ", namaBaruList.OrderBy(x => x));

            perubahan.Add(("SpecimenDetails", namaLama, namaBaru));

            return ids.Count;
        }

        private static string ResolveLabel(string fieldName) => fieldName switch
        {
            "SpecimenTypeId" => "Jenis Specimen",
            "SpecimenTypeOtherNote" => "Keterangan Jenis Lainnya",
            "SpecimenDetails" => "Spesifik Specimen",
            "VolumeAmount" => "Volume",
            "VolumeUnitId" => "Satuan Volume",
            "SpecimenDescription" => "Keterangan Specimen",
            "PhysicallyReceivedAt" => "Waktu Penerimaan Fisik",
            _ => fieldName
        };

        /// <summary>
        /// Membuang nol di belakang koma sebelum nilai volume dibandingkan.
        ///
        /// <b>Kolomnya <c>numeric(12,3)</c>, sehingga 5 yang tersimpan terbaca kembali sebagai
        /// 5,000.</b> Dibandingkan sebagai teks apa adanya, keduanya berbeda — dan mengirim
        /// ulang volume yang sama akan mencatat perubahan yang tidak pernah terjadi pada setiap
        /// penyimpanan. Justru jejak semacam itu yang dihindari <see cref="LabFieldChangeRecorder"/>.
        /// </summary>
        private static string? FormatVolume(decimal? value)
            => value?.ToString("0.############", CultureInfo.InvariantCulture);

        private static string? Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }

    /// <summary>Pelanggaran aturan koreksi specimen. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabSpecimenCorrectionValidationException(string message) : Exception(message);
}
