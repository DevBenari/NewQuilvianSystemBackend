using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Pemilik instrumen dan formulir klinis berversi — <c>BE-RWI-107</c> (konfigurasi berversi),
    /// <c>BE-RWI-108</c> (pengesahan empat mata), <c>INT-KEP-16</c> (resolusi bagi formulir).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Proses bisnisnya.</b> Admin konfigurasi klinis (Andi) menyimpan draft "Morse Dewasa v2" Senin
    /// 09.00. Ns. Wati dari komite keperawatan mengesahkannya Selasa 10.00. Pada transaksi pengesahan
    /// yang sama, "Morse Dewasa v1" yang tadinya sah menjadi <c>Retired</c>. Pengkajian sesudahnya
    /// menyimpan v2; pengkajian lama tetap menyimpan v1 dan tetap terbaca.
    /// </para>
    /// <para>
    /// <b>Jalur tidak normal.</b> Andi mencoba mengesahkan versi yang terakhir ia ubah → <c>403</c>
    /// (<c>VAL-KEP-20a</c>), walaupun ia memegang butir <c>Approve</c>. Wati mengesahkan dari layar yang
    /// masih memuat definisi lama sementara Andi baru saja mengubahnya → hash berbeda → <c>409</c>
    /// (<c>VAL-KEP-20c</c>). Mengubah versi yang sudah sah → <c>409</c> (<c>VAL-KEP-20b</c>).
    /// </para>
    /// <para>
    /// <b>Tidak ada nama peran di sini.</b> Siapa yang boleh mengubah dan mengesahkan diatur layar Akses
    /// Role lewat butir <c>ClinicalInstrumentConfiguration : Update</c> dan <c>: Approve</c>; yang dijaga
    /// kode hanya kewenangan yang melekat pada baris — pengesah bukan pengubah terakhir.
    /// </para>
    /// </remarks>
    public class ClinicalInstrumentService
    {
        private const string LogCategory = "HealthServices.Clinical.Instrument";

        /// <summary>Kunci pengaturan lingkungan uji — <c>RWI-DEC-124</c> butir 5; wajib <c>false</c> di produksi.</summary>
        public const string AllowDraftSettingKey = "ClinicalConfiguration:AllowDraftVersionsForTesting";

        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly LoggerService _loggerService;

        public ClinicalInstrumentService(
            ApplicationDbContext dbContext,
            IConfiguration configuration,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _loggerService = loggerService;
        }

        /// <summary><c>true</c> hanya di lingkungan yang menyalakan pengaturan uji — <c>FR-KEP-042</c>.</summary>
        public bool IsDraftAllowedInThisEnvironment =>
            _configuration.GetValue<bool>(AllowDraftSettingKey, false);

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<PagedResult<ClinicalInstrumentListItem>> ListAsync(
            ClinicalInstrumentKind? kind,
            bool? isActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);

            var query = _dbContext.Set<CliClinicalInstrument>().AsNoTracking().Where(x => !x.IsDelete);

            if (kind.HasValue)
                query = query.Where(x => x.InstrumentKind == kind.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var total = await query.CountAsync(cancellationToken);

            var baris = await query
                .OrderBy(x => x.InstrumentKind).ThenBy(x => x.TargetMinAgeMonths).ThenBy(x => x.Code)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var ids = baris.Select(x => x.Id).ToList();

            var versi = await _dbContext.Set<CliClinicalInstrumentVersion>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.InstrumentId) && !x.IsDelete)
                .Select(x => new { x.Id, x.InstrumentId, x.VersionNumber, x.VersionStatus })
                .ToListAsync(cancellationToken);

            return new PagedResult<ClinicalInstrumentListItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = baris.Select(x =>
                {
                    var sah = versi.FirstOrDefault(v => v.InstrumentId == x.Id && v.VersionStatus == ClinicalInstrumentVersionStatus.Approved);
                    return new ClinicalInstrumentListItem
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name,
                        InstrumentKind = x.InstrumentKind,
                        TargetMinAgeMonths = x.TargetMinAgeMonths,
                        TargetMaxAgeMonths = x.TargetMaxAgeMonths,
                        IsActive = x.IsActive,
                        ApprovedVersionId = sah?.Id,
                        ApprovedVersionNumber = sah?.VersionNumber,
                        LatestDraftVersionNumber = versi
                            .Where(v => v.InstrumentId == x.Id && v.VersionStatus == ClinicalInstrumentVersionStatus.Draft)
                            .Select(v => (int?)v.VersionNumber)
                            .Max()
                    };
                }).ToList()
            };
        }

        public async Task<NursingResult<ClinicalInstrumentResponse>> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var instrumen = await _dbContext.Set<CliClinicalInstrument>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (instrumen == null)
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status404NotFound, "Instrumen klinis tidak ditemukan.");

            var versi = await _dbContext.Set<CliClinicalInstrumentVersion>()
                .AsNoTracking()
                .Where(x => x.InstrumentId == id && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .ToListAsync(cancellationToken);

            var nama = await UserNamesAsync(versi.SelectMany(v => new[] { v.LastModifiedByUserId, v.ApprovedByUserId ?? Guid.Empty }), cancellationToken);
            var versiResponse = versi.Select(v => ToVersionResponse(v, nama)).ToList();
            var sah = versi.FirstOrDefault(v => v.VersionStatus == ClinicalInstrumentVersionStatus.Approved);

            return NursingResult<ClinicalInstrumentResponse>.Ok(new ClinicalInstrumentResponse
            {
                Id = instrumen.Id,
                Code = instrumen.Code,
                Name = instrumen.Name,
                InstrumentKind = instrumen.InstrumentKind,
                TargetMinAgeMonths = instrumen.TargetMinAgeMonths,
                TargetMaxAgeMonths = instrumen.TargetMaxAgeMonths,
                Description = instrumen.Description,
                IsActive = instrumen.IsActive,
                ApprovedVersionId = sah?.Id,
                ApprovedVersionNumber = sah?.VersionNumber,
                LatestDraftVersionNumber = versi.Where(v => v.VersionStatus == ClinicalInstrumentVersionStatus.Draft).Select(v => (int?)v.VersionNumber).Max(),
                Versions = versiResponse
            }, "Instrumen klinis berhasil diambil.");
        }

        public async Task<NursingResult<ClinicalInstrumentVersionResponse>> GetVersionAsync(Guid versionId, CancellationToken cancellationToken = default)
        {
            var versi = await _dbContext.Set<CliClinicalInstrumentVersion>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == versionId && !x.IsDelete, cancellationToken);

            if (versi == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status404NotFound, "Versi instrumen tidak ditemukan.");

            var nama = await UserNamesAsync(new[] { versi.LastModifiedByUserId, versi.ApprovedByUserId ?? Guid.Empty }, cancellationToken);
            return NursingResult<ClinicalInstrumentVersionResponse>.Ok(ToVersionResponse(versi, nama), "Versi instrumen berhasil diambil.");
        }

        // =====================================================================
        // Instrumen
        // =====================================================================

        public async Task<NursingResult<ClinicalInstrumentResponse>> CreateInstrumentAsync(
            CreateClinicalInstrumentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kode = request.Code?.Trim().ToUpperInvariant() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(kode) || string.IsNullOrWhiteSpace(request.Name))
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status400BadRequest, "Kode dan nama instrumen wajib diisi.");

            if (!Enum.IsDefined(request.InstrumentKind))
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status400BadRequest, "Jenis instrumen tidak dikenal.");

            var usia = ValidateAgeRange(request.TargetMinAgeMonths, request.TargetMaxAgeMonths);

            if (usia != null)
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status400BadRequest, usia);

            if (await _dbContext.Set<CliClinicalInstrument>().AnyAsync(x => x.Code == kode && !x.IsDelete, cancellationToken))
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status409Conflict, $"Kode instrumen {kode} sudah dipakai.");

            var bertumpuk = await FindOverlappingInstrumentAsync(null, request.InstrumentKind, request.TargetMinAgeMonths, request.TargetMaxAgeMonths, cancellationToken);

            if (bertumpuk != null)
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status409Conflict, $"Rentang usia bertumpuk dengan instrumen {bertumpuk}.");

            var now = DateTime.UtcNow;
            var instrumen = new CliClinicalInstrument
            {
                Id = Guid.NewGuid(),
                Code = kode,
                Name = request.Name.Trim(),
                InstrumentKind = request.InstrumentKind,
                TargetMinAgeMonths = request.TargetMinAgeMonths,
                TargetMaxAgeMonths = request.TargetMaxAgeMonths,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<CliClinicalInstrument>().Add(instrumen);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status409Conflict, $"Kode instrumen {kode} sudah dipakai.");
            }

            var hasil = await GetAsync(instrumen.Id, cancellationToken);
            return NursingResult<ClinicalInstrumentResponse>.Ok(hasil.Value!, "Instrumen klinis berhasil dibuat.", StatusCodes.Status201Created);
        }

        public async Task<NursingResult<ClinicalInstrumentResponse>> UpdateInstrumentAsync(
            Guid id,
            UpdateClinicalInstrumentRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var instrumen = await _dbContext.Set<CliClinicalInstrument>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (instrumen == null)
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status404NotFound, "Instrumen klinis tidak ditemukan.");

            if (string.IsNullOrWhiteSpace(request.Name))
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status400BadRequest, "Nama instrumen wajib diisi.");

            var usia = ValidateAgeRange(request.TargetMinAgeMonths, request.TargetMaxAgeMonths);

            if (usia != null)
                return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status400BadRequest, usia);

            // VAL-KEP-19c — hanya instrumen yang aktif yang ikut diperiksa tumpang tindihnya.
            if (request.IsActive)
            {
                var bertumpuk = await FindOverlappingInstrumentAsync(id, instrumen.InstrumentKind, request.TargetMinAgeMonths, request.TargetMaxAgeMonths, cancellationToken);

                if (bertumpuk != null)
                    return NursingResult<ClinicalInstrumentResponse>.Fail(StatusCodes.Status409Conflict, $"Rentang usia bertumpuk dengan instrumen {bertumpuk}.");
            }

            instrumen.Name = request.Name.Trim();
            instrumen.TargetMinAgeMonths = request.TargetMinAgeMonths;
            instrumen.TargetMaxAgeMonths = request.TargetMaxAgeMonths;
            instrumen.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            instrumen.IsActive = request.IsActive;
            instrumen.UpdateDateTime = DateTime.UtcNow;
            instrumen.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var hasil = await GetAsync(id, cancellationToken);
            return NursingResult<ClinicalInstrumentResponse>.Ok(hasil.Value!, "Instrumen klinis berhasil diubah.");
        }

        // =====================================================================
        // Versi — BE-RWI-107, BE-RWI-108
        // =====================================================================

        /// <summary>Membuat versi <c>Draft</c> baru; tanpa definisi, disalin dari versi terakhir.</summary>
        public async Task<NursingResult<ClinicalInstrumentVersionResponse>> CreateVersionAsync(
            Guid instrumentId,
            CreateClinicalInstrumentVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var instrumen = await _dbContext.Set<CliClinicalInstrument>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == instrumentId && !x.IsDelete, cancellationToken);

            if (instrumen == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status404NotFound, "Instrumen klinis tidak ditemukan.");

            var terakhir = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                .Where(x => x.InstrumentId == instrumentId && !x.IsDelete)
                .OrderByDescending(x => x.VersionNumber)
                .FirstOrDefaultAsync(cancellationToken);

            ClinicalInstrumentDefinition? definisi;
            string? galat;

            if (request.Definition.HasValue && request.Definition.Value.ValueKind == JsonValueKind.Object)
            {
                definisi = ClinicalInstrumentDefinitionEngine.Parse(request.Definition.Value.GetRawText(), out galat);
            }
            else if (terakhir != null)
            {
                definisi = ClinicalInstrumentDefinitionEngine.Parse(terakhir.DefinitionJson, out galat);
            }
            else
            {
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest,
                    "Versi pertama instrumen wajib membawa definisi.");
            }

            if (definisi == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, galat ?? "Definisi formulir tidak sah.");

            var kesalahan = ClinicalInstrumentDefinitionEngine.Validate(definisi);

            if (kesalahan.Count > 0)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, string.Join(" ", kesalahan.Distinct()));

            var now = DateTime.UtcNow;
            var json = ClinicalInstrumentDefinitionEngine.Normalize(definisi);
            var versi = new CliClinicalInstrumentVersion
            {
                Id = Guid.NewGuid(),
                InstrumentId = instrumentId,
                VersionNumber = (terakhir?.VersionNumber ?? 0) + 1,
                VersionStatus = ClinicalInstrumentVersionStatus.Draft,
                DefinitionJson = json,
                DefinitionHash = ClinicalInstrumentDefinitionEngine.Hash(json),
                LastModifiedByUserId = actorUserId,
                LastModifiedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<CliClinicalInstrumentVersion>().Add(versi);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Dua pengubah membuat versi bersamaan: unique (InstrumentId, VersionNumber) menolak yang kedua.
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status409Conflict,
                    "Versi baru baru saja dibuat pengguna lain. Muat ulang sebelum melanjutkan.");
            }

            return await VersionResultAsync(versi.Id, "Versi konsep instrumen berhasil dibuat.", StatusCodes.Status201Created, cancellationToken);
        }

        /// <summary>Mengubah definisi versi <c>Draft</c> — <c>VAL-KEP-19</c>, <c>VAL-KEP-20b</c>, <c>VAL-KEP-20c</c>.</summary>
        public async Task<NursingResult<ClinicalInstrumentVersionResponse>> UpdateVersionAsync(
            Guid versionId,
            UpdateClinicalInstrumentVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var versi = await _dbContext.Set<CliClinicalInstrumentVersion>().FirstOrDefaultAsync(x => x.Id == versionId && !x.IsDelete, cancellationToken);

            if (versi == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status404NotFound, "Versi instrumen tidak ditemukan.");

            if (versi.VersionStatus != ClinicalInstrumentVersionStatus.Draft)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status409Conflict,
                    "Versi yang sudah disahkan tidak dapat diubah. Buat versi baru.", "VERSION_NOT_DRAFT");

            if (!string.Equals(versi.DefinitionHash, request.ExpectedDefinitionHash?.Trim(), StringComparison.OrdinalIgnoreCase))
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status409Conflict,
                    "Definisi sudah diubah orang lain. Muat ulang sebelum melanjutkan.", "STALE_DEFINITION");

            if (request.Definition.ValueKind != JsonValueKind.Object)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, "Definisi formulir wajib diisi.");

            var definisi = ClinicalInstrumentDefinitionEngine.Parse(request.Definition.GetRawText(), out var galat);

            if (definisi == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, galat ?? "Definisi formulir tidak sah.");

            var kesalahan = ClinicalInstrumentDefinitionEngine.Validate(definisi);

            if (kesalahan.Count > 0)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, string.Join(" ", kesalahan.Distinct()));

            var now = DateTime.UtcNow;
            var json = ClinicalInstrumentDefinitionEngine.Normalize(definisi);

            versi.DefinitionJson = json;
            versi.DefinitionHash = ClinicalInstrumentDefinitionEngine.Hash(json);
            versi.LastModifiedByUserId = actorUserId;
            versi.LastModifiedAt = now;
            versi.UpdateDateTime = now;
            versi.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await VersionResultAsync(versi.Id, "Versi konsep instrumen berhasil diubah.", StatusCodes.Status200OK, cancellationToken);
        }

        /// <summary>
        /// Mengesahkan versi <c>Draft</c>; versi sah sebelumnya dipensiunkan pada transaksi yang sama —
        /// <c>BE-RWI-108</c> kriteria 1 s.d. 4.
        /// </summary>
        public async Task<NursingResult<ClinicalInstrumentVersionResponse>> ApproveVersionAsync(
            Guid versionId,
            ApproveClinicalInstrumentVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var versi = await _dbContext.Set<CliClinicalInstrumentVersion>().FirstOrDefaultAsync(x => x.Id == versionId && !x.IsDelete, cancellationToken);

            if (versi == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status404NotFound, "Versi instrumen tidak ditemukan.");

            if (versi.VersionStatus != ClinicalInstrumentVersionStatus.Draft)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status409Conflict,
                    versi.VersionStatus == ClinicalInstrumentVersionStatus.Approved
                        ? "Versi ini sudah disahkan."
                        : "Versi yang sudah dipensiunkan tidak dapat disahkan kembali. Buat versi baru dari salinannya.",
                    "VERSION_NOT_DRAFT");

            if (!string.Equals(versi.DefinitionHash, request.ExpectedDefinitionHash?.Trim(), StringComparison.OrdinalIgnoreCase))
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status409Conflict,
                    "Definisi sudah diubah orang lain. Muat ulang sebelum melanjutkan.", "STALE_DEFINITION");

            // VAL-KEP-20a / RWI-AC-196. Dijaga juga check constraint CK_CliClinicalInstrumentVersion_ApproverDiffers.
            if (versi.LastModifiedByUserId == actorUserId)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status403Forbidden,
                    "Versi ini terakhir diubah oleh Anda. Pengesahan harus dilakukan orang lain.", "APPROVER_IS_LAST_MODIFIER");

            var definisi = ClinicalInstrumentDefinitionEngine.Parse(versi.DefinitionJson, out var galat);

            if (definisi == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, galat ?? "Definisi formulir tidak sah.");

            if (definisi.ReviewFlags.Count > 0)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest,
                    "Versi ini masih punya penanda tinjauan: " + string.Join(" | ", definisi.ReviewFlags) +
                    " Tinjau dan kosongkan penandanya lewat perubahan versi sebelum mengesahkan.", "REVIEW_FLAGS_OPEN");

            var kesalahan = ClinicalInstrumentDefinitionEngine.Validate(definisi);

            if (kesalahan.Count > 0)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, string.Join(" ", kesalahan.Distinct()));

            var now = DateTime.UtcNow;

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Kriteria 3. Dua SaveChanges di dalam satu transaksi: index unik parsial versi Approved
                // diperiksa per pernyataan, sehingga versi lama wajib pensiun lebih dulu.
                var sahLama = await _dbContext.Set<CliClinicalInstrumentVersion>()
                    .Where(x => x.InstrumentId == versi.InstrumentId && x.Id != versi.Id && !x.IsDelete &&
                                x.VersionStatus == ClinicalInstrumentVersionStatus.Approved)
                    .ToListAsync(cancellationToken);

                foreach (var lama in sahLama)
                {
                    lama.VersionStatus = ClinicalInstrumentVersionStatus.Retired;
                    lama.RetiredAt = now;
                    lama.RetiredByUserId = actorUserId;
                    lama.RetireReason = $"Digantikan versi {versi.VersionNumber}.";
                    lama.UpdateDateTime = now;
                    lama.UpdateBy = actorUserId;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                versi.VersionStatus = ClinicalInstrumentVersionStatus.Approved;
                versi.ApprovedByUserId = actorUserId;
                versi.ApprovedAt = now;
                versi.ApprovalNote = string.IsNullOrWhiteSpace(request.ApprovalNote) ? null : request.ApprovalNote.Trim();
                versi.UpdateDateTime = now;
                versi.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaksi.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                await transaksi.RollbackAsync(cancellationToken);
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status409Conflict,
                    "Versi lain baru saja disahkan untuk instrumen ini. Muat ulang sebelum melanjutkan.");
            }

            await _loggerService.InfoAsync(LogCategory, "ClinicalInstrument.ApproveVersion",
                "Mengesahkan versi instrumen klinis.",
                new { versi.Id, versi.InstrumentId, versi.VersionNumber, versi.DefinitionHash, ApprovedBy = actorUserId });

            return await VersionResultAsync(versi.Id, "Versi instrumen berhasil disahkan.", StatusCodes.Status200OK, cancellationToken);
        }

        /// <summary>Memensiunkan versi sah tanpa pengganti; alasan wajib.</summary>
        public async Task<NursingResult<ClinicalInstrumentVersionResponse>> RetireVersionAsync(
            Guid versionId,
            RetireClinicalInstrumentVersionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan pemensiunan wajib diisi.");

            var versi = await _dbContext.Set<CliClinicalInstrumentVersion>().FirstOrDefaultAsync(x => x.Id == versionId && !x.IsDelete, cancellationToken);

            if (versi == null)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status404NotFound, "Versi instrumen tidak ditemukan.");

            if (versi.VersionStatus != ClinicalInstrumentVersionStatus.Approved)
                return NursingResult<ClinicalInstrumentVersionResponse>.Fail(StatusCodes.Status409Conflict,
                    "Hanya versi yang sedang sah yang dapat dipensiunkan.", "VERSION_NOT_APPROVED");

            var now = DateTime.UtcNow;
            versi.VersionStatus = ClinicalInstrumentVersionStatus.Retired;
            versi.RetiredAt = now;
            versi.RetiredByUserId = actorUserId;
            versi.RetireReason = request.Reason.Trim();
            versi.UpdateDateTime = now;
            versi.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await VersionResultAsync(versi.Id, "Versi instrumen berhasil dipensiunkan.", StatusCodes.Status200OK, cancellationToken);
        }

        /// <summary>Uji hitung tanpa menyimpan — pengesah memeriksa pita sebelum mengesahkan.</summary>
        public async Task<NursingResult<InstrumentScoreResult>> ScorePreviewAsync(
            Guid versionId,
            ScorePreviewRequest request,
            CancellationToken cancellationToken = default)
        {
            var versi = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == versionId && !x.IsDelete, cancellationToken);

            if (versi == null)
                return NursingResult<InstrumentScoreResult>.Fail(StatusCodes.Status404NotFound, "Versi instrumen tidak ditemukan.");

            var definisi = ClinicalInstrumentDefinitionEngine.Parse(versi.DefinitionJson, out var galat);

            if (definisi == null)
                return NursingResult<InstrumentScoreResult>.Fail(StatusCodes.Status400BadRequest, galat ?? "Definisi formulir tidak sah.");

            var hasil = ClinicalInstrumentDefinitionEngine.Score(definisi, request.Responses ?? new());
            return NursingResult<InstrumentScoreResult>.Ok(hasil, "Uji hitung instrumen berhasil.");
        }

        // =====================================================================
        // Resolusi bagi formulir — INT-KEP-16
        // =====================================================================

        /// <summary>
        /// Versi yang berlaku bagi pasien satu episode dan satu jenis instrumen. Versi sah dipilih lebih
        /// dulu; bila belum ada, konsep terakhir dikembalikan dengan <c>IsApproved = false</c>, sehingga
        /// dokumen tetap dapat disimpan sebagai konsep tetapi tidak dapat diselesaikan di produksi.
        /// </summary>
        public async Task<NursingResult<ResolvedInstrumentResponse>> ResolveForEpisodeAsync(
            ClinicalInstrumentKind kind,
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var pasien = await _dbContext.Set<InpEpisode>().AsNoTracking()
                .Where(x => x.Id == episodeId && !x.IsDelete)
                .Select(x => (Guid?)x.PatientId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!pasien.HasValue)
                return NursingResult<ResolvedInstrumentResponse>.Fail(StatusCodes.Status404NotFound, "Perawatan rawat inap tidak ditemukan.");

            return await ResolveForPatientAsync(kind, pasien.Value, DateTime.UtcNow, cancellationToken);
        }

        public async Task<NursingResult<ResolvedInstrumentResponse>> ResolveForPatientAsync(
            ClinicalInstrumentKind kind,
            Guid patientId,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            var tanggalLahir = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => x.Id == patientId)
                .Select(x => x.BirthDate)
                .FirstOrDefaultAsync(cancellationToken);

            int? usiaBulan = tanggalLahir.HasValue ? AgeInMonths(tanggalLahir.Value, atUtc) : null;

            var kandidat = await _dbContext.Set<CliClinicalInstrument>().AsNoTracking()
                .Where(x => x.InstrumentKind == kind && x.IsActive && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var instrumen = kandidat.FirstOrDefault(x =>
                usiaBulan.HasValue
                    ? (!x.TargetMinAgeMonths.HasValue || usiaBulan.Value >= x.TargetMinAgeMonths.Value) &&
                      (!x.TargetMaxAgeMonths.HasValue || usiaBulan.Value < x.TargetMaxAgeMonths.Value)
                    : !x.TargetMinAgeMonths.HasValue && !x.TargetMaxAgeMonths.HasValue);

            if (instrumen == null)
            {
                return NursingResult<ResolvedInstrumentResponse>.Fail(StatusCodes.Status404NotFound,
                    usiaBulan.HasValue
                        ? "Tidak ada instrumen untuk usia pasien ini."
                        : "Tanggal lahir pasien belum tercatat, sehingga instrumen menurut usia tidak dapat dipilih.",
                    "INSTRUMENT_NOT_FOUND");
            }

            var versiSemua = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                .Where(x => x.InstrumentId == instrumen.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var versi = versiSemua.FirstOrDefault(x => x.VersionStatus == ClinicalInstrumentVersionStatus.Approved)
                        ?? versiSemua.Where(x => x.VersionStatus == ClinicalInstrumentVersionStatus.Draft)
                            .OrderByDescending(x => x.VersionNumber)
                            .FirstOrDefault();

            if (versi == null)
                return NursingResult<ResolvedInstrumentResponse>.Fail(StatusCodes.Status404NotFound,
                    $"Instrumen {instrumen.Name} belum punya versi yang dapat dipakai.", "INSTRUMENT_VERSION_NOT_FOUND");

            using var dokumen = JsonDocument.Parse(versi.DefinitionJson);

            return NursingResult<ResolvedInstrumentResponse>.Ok(new ResolvedInstrumentResponse
            {
                InstrumentId = instrumen.Id,
                InstrumentCode = instrumen.Code,
                InstrumentName = instrumen.Name,
                InstrumentKind = instrumen.InstrumentKind,
                VersionId = versi.Id,
                VersionNumber = versi.VersionNumber,
                Definition = dokumen.RootElement.Clone(),
                DefinitionHash = versi.DefinitionHash,
                IsApproved = versi.VersionStatus == ClinicalInstrumentVersionStatus.Approved,
                ApprovedAt = versi.ApprovedAt,
                IsDraftAllowedInThisEnvironment = IsDraftAllowedInThisEnvironment,
                PatientAgeMonths = usiaBulan
            }, versi.VersionStatus == ClinicalInstrumentVersionStatus.Approved
                ? "Instrumen yang berlaku berhasil diambil."
                : "Instrumen belum disahkan. Dokumen dapat disimpan sebagai konsep.");
        }

        /// <summary>Usia dalam bulan penuh pada saat tertentu. Lahir 15 Maret 1959, dinilai 16 September 2026 → 810 bulan.</summary>
        public static int AgeInMonths(DateTime birthDate, DateTime atUtc)
        {
            var bulan = (atUtc.Year - birthDate.Year) * 12 + atUtc.Month - birthDate.Month;

            if (atUtc.Day < birthDate.Day)
                bulan--;

            return Math.Max(0, bulan);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private static string? ValidateAgeRange(int? min, int? max)
        {
            if (min.HasValue && min.Value < 0)
                return "Batas usia bawah tidak boleh negatif.";

            if (min.HasValue && max.HasValue && min.Value >= max.Value)
                return "Batas usia bawah harus lebih kecil dari batas usia atas.";

            return null;
        }

        private async Task<string?> FindOverlappingInstrumentAsync(
            Guid? exceptId,
            ClinicalInstrumentKind kind,
            int? min,
            int? max,
            CancellationToken cancellationToken)
        {
            var lain = await _dbContext.Set<CliClinicalInstrument>().AsNoTracking()
                .Where(x => x.InstrumentKind == kind && x.IsActive && !x.IsDelete && (!exceptId.HasValue || x.Id != exceptId.Value))
                .ToListAsync(cancellationToken);

            var bawahBaru = min ?? int.MinValue;
            var atasBaru = max ?? int.MaxValue;

            return lain.FirstOrDefault(x => bawahBaru < (x.TargetMaxAgeMonths ?? int.MaxValue) &&
                                            (x.TargetMinAgeMonths ?? int.MinValue) < atasBaru)?.Name;
        }

        private async Task<NursingResult<ClinicalInstrumentVersionResponse>> VersionResultAsync(
            Guid versionId,
            string message,
            int statusCode,
            CancellationToken cancellationToken)
        {
            var hasil = await GetVersionAsync(versionId, cancellationToken);
            return hasil.IsSuccess
                ? NursingResult<ClinicalInstrumentVersionResponse>.Ok(hasil.Value!, message, statusCode)
                : hasil;
        }

        private async Task<Dictionary<Guid, string>> UserNamesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            var daftar = ids.Where(x => x != Guid.Empty).Distinct().ToList();

            if (daftar.Count == 0)
                return new Dictionary<Guid, string>();

            return await _dbContext.Users.AsNoTracking()
                .Where(x => daftar.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        private static ClinicalInstrumentVersionResponse ToVersionResponse(CliClinicalInstrumentVersion v, IReadOnlyDictionary<Guid, string> nama)
        {
            var definisi = ClinicalInstrumentDefinitionEngine.Parse(v.DefinitionJson, out var galat);
            using var dokumen = JsonDocument.Parse(string.IsNullOrWhiteSpace(v.DefinitionJson) ? "{}" : v.DefinitionJson);

            return new ClinicalInstrumentVersionResponse
            {
                Id = v.Id,
                InstrumentId = v.InstrumentId,
                VersionNumber = v.VersionNumber,
                VersionStatus = v.VersionStatus,
                Definition = dokumen.RootElement.Clone(),
                DefinitionHash = v.DefinitionHash,
                LastModifiedByUserId = v.LastModifiedByUserId,
                LastModifiedByName = nama.GetValueOrDefault(v.LastModifiedByUserId),
                LastModifiedAt = v.LastModifiedAt,
                ApprovedByUserId = v.ApprovedByUserId,
                ApprovedByName = v.ApprovedByUserId.HasValue ? nama.GetValueOrDefault(v.ApprovedByUserId.Value) : null,
                ApprovedAt = v.ApprovedAt,
                ApprovalNote = v.ApprovalNote,
                RetiredAt = v.RetiredAt,
                RetireReason = v.RetireReason,
                ReviewFlags = definisi?.ReviewFlags ?? new List<string>(),
                ValidationErrors = definisi == null
                    ? new List<string> { galat ?? "Definisi formulir tidak dapat dibaca." }
                    : ClinicalInstrumentDefinitionEngine.Validate(definisi)
            };
        }
    }
}
