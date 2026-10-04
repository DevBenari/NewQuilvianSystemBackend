using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Readers;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// BE-FIN-081/082, FIN-DES-089/093: unggah, validasi, deklarasi saldo awal Accounting, persetujuan
/// (melahirkan item piutang/utang beserta mutasi pembukanya), dan penolakan batch migrasi tagihan
/// lama. Validasi bekerja di atas <see cref="OpeningItemRawRow"/> (baris terurai) — aturannya
/// **tunggal** untuk CSV maupun XLSX kelak (FIN-VAL-187..191), dan nol penguraian angka/tanggal
/// terjadi di <see cref="Readers"/> (02-backend-architecture.md M.8). Perpindahan ke APPROVED
/// **MUST** terjadi dalam satu transaksi beserta mutasi pembukanya (FIN-DEC-129, FIN-DEC-130) —
/// gagal berarti nol-duanya.
/// </summary>
public sealed class FinanceOpeningItemBatchService
{
    private const string LogCategory = "Corporate.FinanceManagement.AccountingIntegration.OpeningItemBatch";
    private const string XlsxMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string DateFormat = "yyyy-MM-dd";
    private const string FormatMismatchCode = "FIN-VAL-226";

    /// <summary>
    /// Batas baris data per berkas migrasi (BE-FIN-088, FIN-DEC-155, FIN-DES-096, FIN-VAL-229).
    /// SENGAJA konstanta kode, BUKAN konfigurasi — ia batas ketahanan transaksi (persetujuan batch
    /// melahirkan seluruh item dalam satu transaksi), bukan nilai operasional seperti batas ukuran
    /// berkas bukti. Menjadikannya konfigurasi membuka jalan seseorang menaikkannya tanpa memahami
    /// akibatnya pada ukuran transaksi persetujuan.
    /// </summary>
    private const int MaxUploadRowCount = 10_000;

    // Angka pada pesan galat ditulis dengan dua desimal dan pemisah Indonesia, bukan mengikuti budaya server.
    private static readonly CultureInfo IndonesianCulture = CultureInfo.GetCultureInfo("id-ID");

    private static readonly string[] ReceivableRequiredColumns =
    [
        "NomorDokumen", "JenisDebitur", "NamaDebitur", "TanggalDokumen", "TanggalJatuhTempo", "SisaTagihan"
    ];

    private static readonly string[] SupplierPayableRequiredColumns =
    [
        "KodeSupplier", "NomorInvoiceSupplier", "TanggalInvoiceSupplier", "TanggalJatuhTempo", "SisaTagihan"
    ];

    private readonly ApplicationDbContext _dbContext;
    private readonly IEnumerable<IOpeningItemFileReader> _readers;
    private readonly FinanceSubledgerMovementService _movementService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FinanceOpeningItemBatchService> _logger;

    public FinanceOpeningItemBatchService(
        ApplicationDbContext dbContext,
        IEnumerable<IOpeningItemFileReader> readers,
        FinanceSubledgerMovementService movementService,
        IWebHostEnvironment environment,
        ILogger<FinanceOpeningItemBatchService> logger)
    {
        _dbContext = dbContext;
        _readers = readers;
        _movementService = movementService;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Mengunggah spreadsheet migrasi dan membuat batch DRAFT (FIN-VAL-186, 225; state-transition-matrix.md F.2).
    /// Validasi baris (FIN-VAL-187..191, 226) **belum** dijalankan di sini — menyusul <see cref="ValidateAsync"/>.
    /// </summary>
    public async Task<FinOpeningItemBatch> UploadAsync(
        IFormFile? file, string? itemKind, Guid actorUserId, CancellationToken cancellationToken)
    {
        var (buffer, originalFileName, sourceFormat) = await ReadAndCheckUploadAsync(file, itemKind, cancellationToken);
        using var bufferLease = buffer;
        var normalizedItemKind = itemKind!.Trim().ToUpperInvariant();

        // FIN-DES-089: CutoverDate batch MUST sama dengan FinOpeningBalance (satu nilai dipakai
        // bersama seluruh kelompok saldo, BE-FIN-064/065). Nol baris berarti saldo awal cutover
        // belum pernah dicatat — migrasi tagihan lama belum dapat dimulai.
        var cutoverDate = await _dbContext.FinOpeningBalances
            .AsNoTracking()
            .Where(x => !x.IsDelete)
            .Select(x => x.CutoverDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (cutoverDate == default)
        {
            throw new OpeningItemBatchValidationException(
                "Saldo awal cutover belum ditetapkan. Catat saldo awal cutover terlebih dahulu sebelum mengunggah tagihan lama.");
        }

        var batchId = Guid.NewGuid();
        var physicalPath = ResolvePhysicalPath(batchId, sourceFormat);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        buffer.Position = 0;
        await using (var outputStream = new FileStream(
            physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await buffer.CopyToAsync(outputStream, cancellationToken);
        }

        var entity = new FinOpeningItemBatch
        {
            Id = batchId,
            BatchNumber = GenerateBatchNumber(),
            ItemKind = normalizedItemKind,
            Status = FinOpeningItemBatchStatuses.Draft,
            CutoverDate = cutoverDate,
            TotalItemCount = 0,
            TotalOutstandingAmount = 0m,
            // Belum dinyatakan petugas (BE-FIN-082, declare-accounting-opening) — kolom ini NOT NULL
            // tanpa bawaan pada skema (R14.6), sehingga diisi placeholder sampai dinyatakan sungguhan.
            DeclaredAccountingOpeningAmount = 0m,
            AccountingReferenceDocument = string.Empty,
            UploadedFileName = originalFileName,
            SourceFormat = sourceFormat,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow
        };

        try
        {
            _dbContext.FinOpeningItemBatches.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeleteIfExists(physicalPath);
            throw;
        }

        _logger.LogInformation(
            "Batch migrasi tagihan lama diunggah. Id: {Id}, BatchNumber: {BatchNumber}, ItemKind: {ItemKind}, SourceFormat: {SourceFormat}",
            entity.Id, entity.BatchNumber, entity.ItemKind, entity.SourceFormat);

        return entity;
    }

    /// <summary>
    /// Memeriksa berkas unggahan: terlampir dan tidak kosong, jenis item sah, bukan XLSX (pembaca belum ada),
    /// dapat dibaca, memuat baris data, dan memuat seluruh kolom templat jenis itu (FIN-VAL-186, 225).
    /// Dipakai bersama <see cref="UploadAsync"/> dan <see cref="ReuploadAsync"/> supaya aturan unggah
    /// **satu**, bukan dua salinan yang dapat berselisih. Penyimpanan ke disk dan basis data bukan tugasnya.
    /// Pemanggil MUST membuang <c>Buffer</c> yang dikembalikan; pada galat, buffer sudah dibuang di sini.
    /// </summary>
    private async Task<(MemoryStream Buffer, string OriginalFileName, string SourceFormat)> ReadAndCheckUploadAsync(
        IFormFile? file, string? itemKind, CancellationToken cancellationToken)
    {
        if (file == null || file.Length <= 0)
            throw new OpeningItemBatchBadRequestException("Berkas migrasi wajib dilampirkan dan tidak boleh kosong.");

        var normalizedItemKind = itemKind?.Trim().ToUpperInvariant();
        if (normalizedItemKind is not (FinOpeningItemBatchItemKinds.Receivable or FinOpeningItemBatchItemKinds.SupplierPayable))
            throw new OpeningItemBatchBadRequestException("Pilih jenis item: RECEIVABLE atau SUPPLIER_PAYABLE.");

        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var mediaType = file.ContentType?.Trim();

        // FIN-DES-093: format=XLSX kontraktual sah (FIN-VAL-225 tidak boleh menolaknya sebagai
        // "format salah"), tetapi pembacanya belum ada — BE-FIN-083, terblokir FIN-OQ-081. Pola
        // sama dengan GET /template (BE-FIN-080): nilai benar, sistem belum siap memenuhinya.
        if (extension == ".xlsx" || string.Equals(mediaType, XlsxMediaType, StringComparison.OrdinalIgnoreCase))
        {
            throw new OpeningItemBatchNotReadyException(
                "Berkas XLSX belum dapat diproses — pembaca XLSX menunggu wewenang paket pembaca (FIN-OQ-081). Gunakan format CSV.");
        }

        var reader = _readers.FirstOrDefault(r => r.CanRead(mediaType, extension))
            ?? throw new OpeningItemBatchBadRequestException("Berkas migrasi hanya dapat berupa CSV atau XLSX.");

        // KNOWN LIMITATION (BE-FIN-083 MUST meninjau ulang): SourceFormat ditetapkan "CSV" begitu
        // saja karena CsvOpeningItemFileReader adalah satu-satunya IOpeningItemFileReader terdaftar
        // saat ini — menebak dari ekstensi berkas di sini salah untuk berkas yang diterima pembaca
        // lewat sinyal tipe media saja (mis. ".txt" bertipe media "text/csv"). Begitu
        // XlsxOpeningItemFileReader ada, SourceFormat MUST ditentukan dari pembaca mana yang
        // sungguh-sungguh menerima berkasnya (mis. lewat properti nama format pada antarmuka),
        // bukan dari ekstensi secara terpisah.
        const string sourceFormat = "CSV";

        var buffer = new MemoryStream();
        try
        {
            await using (var inputStream = file.OpenReadStream())
            {
                await inputStream.CopyToAsync(buffer, cancellationToken);
            }

            List<OpeningItemRawRow> rows;
            try
            {
                buffer.Position = 0;
                rows = reader.Read(buffer);
            }
            catch (Exception)
            {
                // FIN-VAL-186: berkas tidak terbaca.
                throw new OpeningItemBatchBadRequestException("Berkas tidak dapat dibaca. Gunakan templat yang disediakan.");
            }

            // FIN-VAL-186 ("kolom templat tidak lengkap"): IOpeningItemFileReader hanya memulangkan
            // baris data (header dikonsumsi di dalam pembaca), sehingga kelengkapan kolom diperiksa
            // dari kunci Cells baris pertama. Berkas tanpa baris data sama sekali juga ditolak di sini
            // — templat kosong bukan unggahan yang bermakna.
            if (rows.Count == 0)
                throw new OpeningItemBatchBadRequestException("Berkas tidak dapat dibaca. Gunakan templat yang disediakan.");

            var requiredColumns = normalizedItemKind == FinOpeningItemBatchItemKinds.Receivable
                ? ReceivableRequiredColumns
                : SupplierPayableRequiredColumns;

            if (!requiredColumns.All(column => rows[0].Cells.ContainsKey(column)))
                throw new OpeningItemBatchBadRequestException("Berkas tidak dapat dibaca. Gunakan templat yang disediakan.");

            // FIN-VAL-229 (BE-FIN-088, FIN-DEC-155): batas baris data diperiksa SESUDAH berkas
            // berhasil diurai — jumlah baris sebenarnya hanya diketahui setelah penguraian, dan
            // menghitung dari ukuran berkas adalah terkaan yang dilarang FIN-DEC-140 — dan SEBELUM
            // berkas disimpan ke disk maupun basis data, supaya berkas yang ditolak meninggalkan
            // NOL jejak (nol berkas fisik, nol baris batch). `rows` sudah TIDAK memuat baris judul
            // (header dikonsumsi di dalam pembaca, lihat ringkasan class), sehingga `rows.Count`
            // adalah jumlah baris data persis. Batas INKLUSIF: tepat 10.000 diterima.
            if (rows.Count > MaxUploadRowCount)
            {
                throw new OpeningItemBatchBadRequestException(
                    "Berkas memuat lebih dari 10.000 baris. Pecah menjadi beberapa berkas, lalu unggah masing-masing sebagai batch tersendiri.");
            }

            return (buffer, originalFileName, sourceFormat);
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Mengunggah ulang berkas pada batch yang masih DRAFT (state-transition-matrix.md F.2: DRAFT -> DRAFT,
    /// hak `Update`). Hasil validasi sebelumnya dibuang karena tidak lagi berlaku untuk berkas yang baru; saldo awal
    /// Accounting yang sudah dinyatakan **dipertahankan** (itu pernyataan petugas, bukan hasil berkas). Jenis item
    /// tidak dapat diganti. Batch yang sudah VALIDATED, APPROVED, LOCKED, atau REJECTED ditolak `409`.
    /// </summary>
    public async Task<FinOpeningItemBatch> ReuploadAsync(
        Guid id, IFormFile? file, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.FinOpeningItemBatches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch migrasi tagihan lama tidak ditemukan.");

        if (entity.Status != FinOpeningItemBatchStatuses.Draft)
        {
            throw new OpeningItemBatchConflictException(
                "Berkas hanya dapat diunggah ulang selama batch berstatus DRAFT.");
        }

        EnsureCurrentRowVersion(entity.RowVersion, expectedRowVersion);

        var (buffer, originalFileName, sourceFormat) = await ReadAndCheckUploadAsync(file, entity.ItemKind, cancellationToken);
        using var bufferLease = buffer;

        // FIN-DES-089: CutoverDate batch MUST tetap sama dengan FinOpeningBalance; diambil ulang seperti saat unggah.
        var cutoverDate = await _dbContext.FinOpeningBalances
            .AsNoTracking()
            .Where(x => !x.IsDelete)
            .Select(x => x.CutoverDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (cutoverDate == default)
        {
            throw new OpeningItemBatchValidationException(
                "Saldo awal cutover belum ditetapkan. Catat saldo awal cutover terlebih dahulu sebelum mengunggah tagihan lama.");
        }

        var finalPath = ResolvePhysicalPath(entity.Id, sourceFormat);
        var previousPath = ResolvePhysicalPath(entity.Id, entity.SourceFormat);
        var tempPath = $"{finalPath}.{Guid.NewGuid():N}.tmp";
        Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);

        // Berkas baru ditulis ke jalur sementara dulu; berkas lama tidak disentuh sampai basis data berhasil disimpan.
        buffer.Position = 0;
        await using (var outputStream = new FileStream(
            tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await buffer.CopyToAsync(outputStream, cancellationToken);
        }

        entity.UploadedFileName = originalFileName;
        entity.SourceFormat = sourceFormat;
        entity.CutoverDate = cutoverDate;
        entity.TotalItemCount = 0;
        entity.TotalOutstandingAmount = 0m;
        entity.ValidationSummaryJson = null;
        entity.UpdateBy = actorUserId;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.RowVersion = Guid.NewGuid();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            DeleteIfExists(tempPath);
            throw;
        }

        // Basis data sudah menunjuk berkas baru; gantikan berkas fisik, lalu buang berkas lama bila lokasinya berbeda.
        File.Move(tempPath, finalPath, overwrite: true);
        if (!string.Equals(previousPath, finalPath, StringComparison.OrdinalIgnoreCase))
        {
            DeleteIfExists(previousPath);
        }

        _logger.LogInformation(
            "Berkas batch migrasi tagihan lama diunggah ulang. Id: {Id}, BatchNumber: {BatchNumber}, SourceFormat: {SourceFormat}",
            entity.Id, entity.BatchNumber, entity.SourceFormat);

        return entity;
    }

    /// <summary>
    /// Menjalankan validasi per baris (FIN-VAL-187..191, 226) di atas baris yang sudah terurai.
    /// Batch berpindah ke VALIDATED hanya bila nol baris bergalat; sebaliknya tetap DRAFT beserta
    /// daftar galatnya (state-transition-matrix.md F.2).
    /// </summary>
    public async Task<(FinOpeningItemBatch Entity, List<OpeningItemBatchRowValidationResult> Rows)> ValidateAsync(
        Guid id, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.FinOpeningItemBatches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch migrasi tagihan lama tidak ditemukan.");

        if (entity.Status != FinOpeningItemBatchStatuses.Draft)
        {
            throw new OpeningItemBatchConflictException(
                "Batch ini sudah tidak berstatus DRAFT, sehingga tidak dapat divalidasi ulang.");
        }

        var rows = await ReadRowsAsync(entity, cancellationToken);
        var results = entity.ItemKind == FinOpeningItemBatchItemKinds.Receivable
            ? ValidateReceivableRows(rows, entity.CutoverDate)
            : await ValidateSupplierPayableRowsAsync(rows, entity.CutoverDate, cancellationToken);

        var errorCount = results.Count(r => !r.IsValid);

        entity.TotalItemCount = results.Count(r => r.IsValid);
        entity.TotalOutstandingAmount = results.Where(r => r.IsValid).Sum(r => r.OutstandingAmount ?? 0m);
        entity.ValidationSummaryJson = JsonSerializer.Serialize(results);
        entity.Status = errorCount == 0 ? FinOpeningItemBatchStatuses.Validated : FinOpeningItemBatchStatuses.Draft;
        entity.UpdateBy = actorUserId;
        entity.UpdateDateTime = DateTime.UtcNow;
        // BE-FIN-082: diperbaiki — BE-FIN-081 tidak memutar RowVersion di sini, sehingga
        // ExpectedRowVersion sisi klien tetap "berlaku" setelah panggilan ini sukses, melemahkan
        // jaminan optimistic concurrency yang justru menjadi tujuan [ConcurrencyCheck] (pola
        // seharusnya sama dengan FinanceReceivableService/FinanceSupplierPayableService).
        entity.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Validasi batch migrasi tagihan lama selesai. Id: {Id}, TotalBaris: {TotalBaris}, BarisBergalat: {BarisBergalat}, StatusBaru: {StatusBaru}",
            entity.Id, results.Count, errorCount, entity.Status);

        return (entity, results);
    }

    /// <summary>BE-FIN-082, FIN-API-1.6 F.2: daftar batch migrasi, bersaring jenis dan status.</summary>
    public async Task<PagedResult<OpeningItemBatchResponse>> GetPagedAsync(
        OpeningItemBatchPagedQuery query, CancellationToken cancellationToken)
    {
        var q = _dbContext.FinOpeningItemBatches.AsNoTracking().Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.ItemKind))
            q = q.Where(x => x.ItemKind == query.ItemKind.Trim().ToUpperInvariant());

        if (!string.IsNullOrWhiteSpace(query.Status))
            q = q.Where(x => x.Status == query.Status.Trim().ToUpperInvariant());

        var totalData = await q.CountAsync(cancellationToken);

        q = (query.SortBy?.ToLowerInvariant(), query.SortDirection?.ToLowerInvariant()) switch
        {
            ("cutoverdate", "asc") => q.OrderBy(x => x.CutoverDate),
            ("cutoverdate", "desc") => q.OrderByDescending(x => x.CutoverDate),
            ("batchnumber", "asc") => q.OrderBy(x => x.BatchNumber),
            ("batchnumber", "desc") => q.OrderByDescending(x => x.BatchNumber),
            (_, "asc") => q.OrderBy(x => x.CreateDateTime),
            _ => q.OrderByDescending(x => x.CreateDateTime)
        };

        var items = await q.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToListAsync(cancellationToken);

        // BE-FIN-089, FIN-DES-095: nama seluruh penyetuju SATU HALAMAN diambil SEKALI di sini —
        // bukan satu kueri per baris. Pola sama dengan FinanceOpeningBalanceService.GetUserNamesAsync.
        var names = await GetUserNamesAsync(items.Select(x => x.ApprovedBy), cancellationToken);

        return new PagedResult<OpeningItemBatchResponse>
        {
            Items = items.Select(x => MapWithName(x, names)).ToList(),
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)query.PageSize)
        };
    }

    /// <summary>BE-FIN-082, FIN-API-1.6 F.2: rincian batch beserta hasil validasi per baris terakhir.</summary>
    public async Task<OpeningItemBatchDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.FinOpeningItemBatches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch migrasi tagihan lama tidak ditemukan.");

        var rows = string.IsNullOrWhiteSpace(entity.ValidationSummaryJson)
            ? []
            : JsonSerializer.Deserialize<List<OpeningItemBatchRowValidationResult>>(entity.ValidationSummaryJson) ?? [];

        return await MapDetailWithNameAsync(entity, rows, cancellationToken);
    }

    /// <summary>
    /// Menyatakan total saldo awal Accounting dan rujukan dokumennya (FIN-VAL-182-style: keduanya
    /// wajib). Boleh dipanggil dari DRAFT atau VALIDATED; status batch tidak berubah (state-transition-matrix.md F.2).
    /// </summary>
    public async Task<FinOpeningItemBatch> DeclareAccountingOpeningAsync(
        Guid id, Guid expectedRowVersion, decimal declaredAmount, string? referenceDocument,
        Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.FinOpeningItemBatches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch migrasi tagihan lama tidak ditemukan.");

        EnsureCurrentRowVersion(entity.RowVersion, expectedRowVersion);

        if (entity.Status is not (FinOpeningItemBatchStatuses.Draft or FinOpeningItemBatchStatuses.Validated))
        {
            throw new OpeningItemBatchConflictException(
                "Saldo awal Accounting hanya dapat dinyatakan selama batch berstatus DRAFT atau VALIDATED.");
        }

        var trimmedReferenceDocument = referenceDocument?.Trim() ?? string.Empty;
        if (declaredAmount <= 0m || string.IsNullOrWhiteSpace(trimmedReferenceDocument))
        {
            throw new OpeningItemBatchValidationException(
                "Nominal dan rujukan dokumen saldo awal Accounting wajib diisi.");
        }

        entity.DeclaredAccountingOpeningAmount = declaredAmount;
        entity.AccountingReferenceDocument = trimmedReferenceDocument;
        entity.UpdateBy = actorUserId;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Saldo awal Accounting dinyatakan untuk batch migrasi tagihan lama. Id: {Id}, DeclaredAmount: {DeclaredAmount}",
            entity.Id, entity.DeclaredAccountingOpeningAmount);

        return entity;
    }

    /// <summary>
    /// Menolak batch (FIN-DES-089). Boleh dipanggil dari DRAFT atau VALIDATED; alasan wajib.
    /// </summary>
    public async Task<FinOpeningItemBatch> RejectAsync(
        Guid id, Guid expectedRowVersion, string? reason, Guid actorUserId, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.FinOpeningItemBatches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch migrasi tagihan lama tidak ditemukan.");

        EnsureCurrentRowVersion(entity.RowVersion, expectedRowVersion);

        if (entity.Status is not (FinOpeningItemBatchStatuses.Draft or FinOpeningItemBatchStatuses.Validated))
        {
            throw new OpeningItemBatchConflictException("Batch hanya dapat ditolak selama berstatus DRAFT atau VALIDATED.");
        }

        var trimmedReason = reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedReason))
        {
            throw new OpeningItemBatchValidationException("Alasan penolakan wajib diisi.");
        }

        entity.Status = FinOpeningItemBatchStatuses.Rejected;
        entity.RejectionReason = trimmedReason;
        entity.UpdateBy = actorUserId;
        entity.UpdateDateTime = DateTime.UtcNow;
        entity.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Batch migrasi tagihan lama ditolak. Id: {Id}", entity.Id);

        return entity;
    }

    /// <summary>
    /// Menyetujui batch VALIDATED: melahirkan item piutang/utang beserta mutasi pembukanya dalam
    /// SATU transaksi (FIN-DEC-129, FIN-DEC-130) — gagal berarti nol-duanya. Batch berpindah ke
    /// APPROVED lalu otomatis LOCKED (state-transition-matrix.md F.2). Menerbitkan NOL kejadian
    /// akuntansi (FIN-DEC-129) — item migrasi tidak pernah menulis ke kotak keluar outbox.
    /// </summary>
    public async Task<FinOpeningItemBatch> ApproveAsync(
        Guid id, Guid expectedRowVersion, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        var committed = false;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_OPENING_ITEM_BATCH_{id:N}", cancellationToken);

            var entity = await _dbContext.FinOpeningItemBatches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Batch migrasi tagihan lama tidak ditemukan.");

            EnsureCurrentRowVersion(entity.RowVersion, expectedRowVersion);

            if (entity.Status != FinOpeningItemBatchStatuses.Validated)
                throw new OpeningItemBatchConflictException("Batch hanya dapat disetujui selama berstatus VALIDATED.");

            if (string.IsNullOrWhiteSpace(entity.AccountingReferenceDocument) || entity.DeclaredAccountingOpeningAmount <= 0m)
            {
                throw new OpeningItemBatchValidationException(
                    "Saldo awal Accounting beserta rujukan dokumennya wajib dinyatakan sebelum batch disetujui.");
            }

            // FIN-DEC-129: CutoverDate batch MUST tetap sama dengan FinOpeningBalance pada saat
            // persetujuan, bukan hanya saat unggah — keduanya bisa berselang lama.
            var currentCutoverDate = await _dbContext.FinOpeningBalances.AsNoTracking()
                .Where(x => !x.IsDelete).Select(x => x.CutoverDate).FirstOrDefaultAsync(cancellationToken);
            if (currentCutoverDate == default || currentCutoverDate != entity.CutoverDate)
            {
                throw new OpeningItemBatchValidationException(
                    "Tanggal cutover batch ini tidak lagi sama dengan saldo awal cutover saat ini. Validasi ulang batch sebelum menyetujui.");
            }

            // Pertahanan berlapis: validasi ulang baris dari berkas fisik, memastikan nol perubahan
            // sejak validasi terakhir sebelum melahirkan data finansial sungguhan.
            var rows = await ReadRowsAsync(entity, cancellationToken);
            var results = entity.ItemKind == FinOpeningItemBatchItemKinds.Receivable
                ? ValidateReceivableRows(rows, entity.CutoverDate)
                : await ValidateSupplierPayableRowsAsync(rows, entity.CutoverDate, cancellationToken);

            if (results.Any(r => !r.IsValid))
            {
                entity.Status = FinOpeningItemBatchStatuses.Draft;
                entity.ValidationSummaryJson = JsonSerializer.Serialize(results);
                entity.TotalItemCount = results.Count(r => r.IsValid);
                entity.TotalOutstandingAmount = results.Where(r => r.IsValid).Sum(r => r.OutstandingAmount ?? 0m);
                entity.UpdateBy = actorUserId;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.RowVersion = Guid.NewGuid();

                await _dbContext.SaveChangesAsync(cancellationToken);
                await CommitAsync(transaction, cancellationToken);
                committed = true;

                throw new OpeningItemBatchConflictException(
                    "Berkas batch ini berubah sejak validasi terakhir dan kini memuat baris bergalat. Batch dikembalikan ke DRAFT — validasi ulang sebelum menyetujui.");
            }

            var totalOutstanding = results.Where(r => r.IsValid).Sum(r => r.OutstandingAmount ?? 0m);
            if (totalOutstanding != entity.DeclaredAccountingOpeningAmount)
            {
                throw new OpeningItemBatchValidationException(
                    $"Total sisa tagihan (Rp {totalOutstanding.ToString("N2", IndonesianCulture)}) tidak sama dengan saldo awal Accounting yang dinyatakan (Rp {entity.DeclaredAccountingOpeningAmount.ToString("N2", IndonesianCulture)}). Batch tidak dapat disetujui.");
            }

            var now = DateTimeOffset.UtcNow;
            var correlationId = Guid.NewGuid();

            if (entity.ItemKind == FinOpeningItemBatchItemKinds.Receivable)
            {
                foreach (var (row, result) in rows.Zip(results, (row, result) => (row, result)))
                {
                    if (!result.IsValid) continue;
                    await CreateReceivableFromRowAsync(entity, row, result, correlationId, now, actorUserId, cancellationToken);
                }
            }
            else
            {
                var supplierCodeToId = await _dbContext.MstSuppliers.AsNoTracking()
                    .Where(s => !s.IsDelete)
                    .Select(s => new { s.SupplierCode, s.Id })
                    .ToDictionaryAsync(s => s.SupplierCode, s => s.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

                foreach (var (row, result) in rows.Zip(results, (row, result) => (row, result)))
                {
                    if (!result.IsValid) continue;
                    await CreateSupplierPayableFromRowAsync(entity, row, result, supplierCodeToId, correlationId, now, actorUserId, cancellationToken);
                }
            }

            entity.Status = FinOpeningItemBatchStatuses.Locked;
            entity.ApprovedBy = actorUserId;
            entity.ApprovedAt = now;
            entity.LockedAt = now;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            committed = true;

            _logger.LogInformation(
                "Batch migrasi tagihan lama disetujui dan dikunci. Id: {Id}, TotalItemCount: {TotalItemCount}, TotalOutstandingAmount: {TotalOutstandingAmount}",
                entity.Id, entity.TotalItemCount, entity.TotalOutstandingAmount);

            return entity;
        }
        catch
        {
            if (!committed) await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    private async Task CreateReceivableFromRowAsync(
        FinOpeningItemBatch batch, OpeningItemRawRow row, OpeningItemBatchRowValidationResult result,
        Guid correlationId, DateTimeOffset now, Guid actorUserId, CancellationToken cancellationToken)
    {
        var amount = result.OutstandingAmount!.Value;
        var documentNumber = GetCell(row, "NomorDokumen");
        var debtorType = GetCell(row, "JenisDebitur").ToUpperInvariant();
        var dueDate = TryParseDate(GetCell(row, "TanggalJatuhTempo"), out var parsedDueDate) ? parsedDueDate : batch.CutoverDate;
        var description = GetCell(row, "Keterangan");

        var receivable = new FinReceivable
        {
            Id = Guid.NewGuid(),
            ReceivableNumber = GenerateReceivableNumber(),
            OpeningItemBatchId = batch.Id,
            DebtorType = debtorType,
            OriginalAmount = amount,
            OutstandingAmount = amount,
            DueDate = dueDate,
            Status = FinReceivableStatuses.Outstanding,
            ClaimStatus = FinReceivableClaimStatuses.NotRequired,
            RecognizedAt = now,
            CorrelationId = correlationId,
            CausationId = batch.Id,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow
        };
        receivable.Items.Add(new FinReceivableItem
        {
            Id = Guid.NewGuid(),
            Description = string.IsNullOrWhiteSpace(description) ? documentNumber : description,
            Amount = amount,
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow
        });
        _dbContext.FinReceivables.Add(receivable);

        await _movementService.RecordReceivableMovementAsync(
            receivable: receivable,
            movementType: FinReceivableMovementTypes.PembukaanMigrasi,
            deltaAmount: amount,
            balanceBefore: 0m,
            occurredAt: now,
            actorUserId: actorUserId,
            correlationId: correlationId,
            causationId: batch.Id,
            referenceNumber: documentNumber,
            openingItemBatchId: batch.Id,
            cancellationToken: cancellationToken);
    }

    private async Task CreateSupplierPayableFromRowAsync(
        FinOpeningItemBatch batch, OpeningItemRawRow row, OpeningItemBatchRowValidationResult result,
        IReadOnlyDictionary<string, Guid> supplierCodeToId, Guid correlationId, DateTimeOffset now,
        Guid actorUserId, CancellationToken cancellationToken)
    {
        var amount = result.OutstandingAmount!.Value;
        var documentNumber = GetCell(row, "NomorInvoiceSupplier");
        var supplierCode = GetCell(row, "KodeSupplier");
        var documentDate = TryParseDate(GetCell(row, "TanggalInvoiceSupplier"), out var parsedDocDate) ? parsedDocDate : batch.CutoverDate;
        var dueDate = TryParseDate(GetCell(row, "TanggalJatuhTempo"), out var parsedDueDate) ? parsedDueDate : batch.CutoverDate;
        var description = GetCell(row, "Keterangan");

        if (!supplierCodeToId.TryGetValue(supplierCode, out var supplierId))
        {
            // Pertahanan berlapis — mustahil tercapai karena validasi ulang di ApproveAsync sudah
            // memastikan seluruh kode supplier pada baris yang lolos ada di master.
            throw new InvalidOperationException($"Supplier {supplierCode} hilang dari master antara validasi dan persetujuan.");
        }

        var payable = new FinSupplierPayable
        {
            Id = Guid.NewGuid(),
            PayableNumber = GeneratePayableNumber(),
            SupplierId = supplierId,
            SupplierInvoiceNumber = documentNumber,
            SupplierInvoiceDate = documentDate,
            OriginalAmount = amount,
            OutstandingAmount = amount,
            DueDate = dueDate,
            Status = FinSupplierPayableStatuses.Outstanding,
            OpeningItemBatchId = batch.Id,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow
        };
        payable.Items.Add(new FinSupplierPayableItem
        {
            Id = Guid.NewGuid(),
            Description = string.IsNullOrWhiteSpace(description) ? documentNumber : description,
            Quantity = 1m,
            UnitPrice = amount,
            Amount = amount,
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow
        });
        _dbContext.FinSupplierPayables.Add(payable);

        await _movementService.RecordSupplierPayableMovementAsync(
            payable: payable,
            movementType: FinSupplierPayableMovementTypes.PembukaanMigrasi,
            deltaAmount: amount,
            balanceBefore: 0m,
            occurredAt: now,
            actorUserId: actorUserId,
            correlationId: correlationId,
            causationId: batch.Id,
            referenceNumber: documentNumber,
            openingItemBatchId: batch.Id,
            cancellationToken: cancellationToken);
    }

    private static List<OpeningItemBatchRowValidationResult> ValidateReceivableRows(
        List<OpeningItemRawRow> rows, DateOnly cutoverDate)
    {
        var results = new List<OpeningItemBatchRowValidationResult>(rows.Count);
        var seenDocumentNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var documentNumber = GetCell(row, "NomorDokumen");
            var debtorName = GetCell(row, "NamaDebitur");

            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                results.Add(Invalid(row.RowNumber, debtorName, FormatMismatchCode, FormatMismatchMessage(row.RowNumber)));
                continue;
            }

            // FIN-VAL-191: diperiksa sebelum aturan lain supaya baris kembar tetap terdeteksi
            // walau baris pertamanya sendiri gagal aturan lain.
            if (seenDocumentNumbers.TryGetValue(documentNumber, out var firstRowNumber))
            {
                results.Add(Invalid(row.RowNumber, debtorName, "FIN-VAL-191",
                    $"Baris {row.RowNumber}: nomor dokumen kembar dengan baris {firstRowNumber}."));
                continue;
            }
            seenDocumentNumbers[documentNumber] = row.RowNumber;

            // BE-FIN-082: EMPLOYEE_BENEFIT SENGAJA tidak diterima jalur migrasi — CK_FinReceivable_BenefitOwner
            // mewajibkan BenefitOwnerId terisi untuk jenis ini, dan templat CSV tidak punya kolom untuk itu.
            // Konsisten dengan intake Billing yang juga tidak pernah menghasilkan EMPLOYEE_BENEFIT (FIN-DES-024,
            // OPEN DECISION) — ditemukan dan diperbaiki sebagai bagian BE-FIN-082, lihat laporan bagian 7.
            var debtorType = GetCell(row, "JenisDebitur").ToUpperInvariant();
            if (debtorType is not (FinReceivableDebtorTypes.Payer or FinReceivableDebtorTypes.PatientGuarantor))
            {
                results.Add(Invalid(row.RowNumber, debtorName, "FIN-VAL-187",
                    $"Baris {row.RowNumber}: jenis debitur tidak dikenal untuk migrasi tagihan lama (hanya PAYER atau PATIENT_GUARANTOR)."));
                continue;
            }

            if (!TryParseAmount(GetCell(row, "SisaTagihan"), out var outstandingAmount))
            {
                results.Add(Invalid(row.RowNumber, debtorName, FormatMismatchCode, FormatMismatchMessage(row.RowNumber)));
                continue;
            }

            if (outstandingAmount <= 0m)
            {
                results.Add(Invalid(row.RowNumber, debtorName, "FIN-VAL-189", $"Baris {row.RowNumber}: sisa tagihan harus lebih besar dari nol."));
                continue;
            }

            if (!TryParseDate(GetCell(row, "TanggalDokumen"), out var documentDate) ||
                !TryParseDate(GetCell(row, "TanggalJatuhTempo"), out _))
            {
                results.Add(Invalid(row.RowNumber, debtorName, FormatMismatchCode, FormatMismatchMessage(row.RowNumber)));
                continue;
            }

            if (documentDate > cutoverDate)
            {
                results.Add(Invalid(row.RowNumber, debtorName, "FIN-VAL-190",
                    $"Baris {row.RowNumber}: tanggal dokumen tidak boleh melewati tanggal cutover."));
                continue;
            }

            results.Add(Valid(row.RowNumber, debtorName, outstandingAmount));
        }

        return results;
    }

    private async Task<List<OpeningItemBatchRowValidationResult>> ValidateSupplierPayableRowsAsync(
        List<OpeningItemRawRow> rows, DateOnly cutoverDate, CancellationToken cancellationToken)
    {
        var supplierCodes = rows
            .Select(r => GetCell(r, "KodeSupplier"))
            .Where(code => code.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var knownSupplierCodes = await _dbContext.MstSuppliers
            .AsNoTracking()
            .Where(s => !s.IsDelete && supplierCodes.Contains(s.SupplierCode))
            .Select(s => s.SupplierCode)
            .ToListAsync(cancellationToken);
        var knownSupplierCodeSet = new HashSet<string>(knownSupplierCodes, StringComparer.OrdinalIgnoreCase);

        var results = new List<OpeningItemBatchRowValidationResult>(rows.Count);
        var seenDocumentNumbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var documentNumber = GetCell(row, "NomorInvoiceSupplier");
            var supplierCode = GetCell(row, "KodeSupplier");

            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                results.Add(Invalid(row.RowNumber, supplierCode, FormatMismatchCode, FormatMismatchMessage(row.RowNumber)));
                continue;
            }

            if (seenDocumentNumbers.TryGetValue(documentNumber, out var firstRowNumber))
            {
                results.Add(Invalid(row.RowNumber, supplierCode, "FIN-VAL-191",
                    $"Baris {row.RowNumber}: nomor dokumen kembar dengan baris {firstRowNumber}."));
                continue;
            }
            seenDocumentNumbers[documentNumber] = row.RowNumber;

            if (string.IsNullOrWhiteSpace(supplierCode) || !knownSupplierCodeSet.Contains(supplierCode))
            {
                results.Add(Invalid(row.RowNumber, supplierCode, "FIN-VAL-188", $"Baris {row.RowNumber}: supplier tidak ditemukan."));
                continue;
            }

            if (!TryParseAmount(GetCell(row, "SisaTagihan"), out var outstandingAmount))
            {
                results.Add(Invalid(row.RowNumber, supplierCode, FormatMismatchCode, FormatMismatchMessage(row.RowNumber)));
                continue;
            }

            if (outstandingAmount <= 0m)
            {
                results.Add(Invalid(row.RowNumber, supplierCode, "FIN-VAL-189", $"Baris {row.RowNumber}: sisa tagihan harus lebih besar dari nol."));
                continue;
            }

            if (!TryParseDate(GetCell(row, "TanggalInvoiceSupplier"), out var documentDate) ||
                !TryParseDate(GetCell(row, "TanggalJatuhTempo"), out _))
            {
                results.Add(Invalid(row.RowNumber, supplierCode, FormatMismatchCode, FormatMismatchMessage(row.RowNumber)));
                continue;
            }

            if (documentDate > cutoverDate)
            {
                results.Add(Invalid(row.RowNumber, supplierCode, "FIN-VAL-190",
                    $"Baris {row.RowNumber}: tanggal dokumen tidak boleh melewati tanggal cutover."));
                continue;
            }

            results.Add(Valid(row.RowNumber, supplierCode, outstandingAmount));
        }

        return results;
    }

    private static string GetCell(OpeningItemRawRow row, string column) =>
        row.Cells.TryGetValue(column, out var value) ? value.Trim() : string.Empty;

    private static bool TryParseAmount(string text, out decimal value) =>
        decimal.TryParse(text, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);

    private static bool TryParseDate(string text, out DateOnly value) =>
        DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);

    private static string FormatMismatchMessage(int rowNumber) => $"Baris {rowNumber}: format angka atau tanggal tidak sesuai templat.";

    private static OpeningItemBatchRowValidationResult Valid(int rowNumber, string identifier, decimal outstandingAmount) => new()
    {
        RowNumber = rowNumber,
        Identifier = identifier,
        IsValid = true,
        OutstandingAmount = outstandingAmount
    };

    private static OpeningItemBatchRowValidationResult Invalid(int rowNumber, string identifier, string errorCode, string message) => new()
    {
        RowNumber = rowNumber,
        Identifier = identifier,
        IsValid = false,
        ErrorCode = errorCode,
        Message = message
    };

    /// <summary>
    /// Membaca ulang berkas fisik tersimpan menjadi baris terurai. Dipakai <see cref="ValidateAsync"/>
    /// dan <see cref="ApproveAsync"/> — keduanya MUST membaca dari sumber yang sama, bukan dari
    /// salinan yang mungkin sudah basi.
    /// </summary>
    private async Task<List<OpeningItemRawRow>> ReadRowsAsync(FinOpeningItemBatch entity, CancellationToken cancellationToken)
    {
        var physicalPath = ResolvePhysicalPath(entity.Id, entity.SourceFormat);
        if (!File.Exists(physicalPath))
        {
            // Cacat sistem (berkas seharusnya selalu ada sejak UploadAsync), bukan kesalahan
            // pengguna — pola sama dengan FIN-VAL-165/167 (500, dibiarkan Exception bawaan).
            throw new InvalidOperationException($"Berkas batch migrasi {entity.Id} hilang dari penyimpanan.");
        }

        var extension = entity.SourceFormat.Equals("CSV", StringComparison.OrdinalIgnoreCase) ? ".csv" : ".xlsx";
        var reader = _readers.FirstOrDefault(r => r.CanRead(null, extension))
            ?? throw new OpeningItemBatchNotReadyException($"Nol pembaca terdaftar untuk format {entity.SourceFormat}.");

        await using var stream = File.OpenRead(physicalPath);
        return reader.Read(stream);
    }

    private static void EnsureCurrentRowVersion(Guid actual, Guid expected)
    {
        if (expected == Guid.Empty || actual != expected)
            throw new OpeningItemBatchConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");
    }

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private static string GenerateBatchNumber()
    {
        // KNOWN ISSUE (bersama seluruh generator nomor rumpun ini): belum memakai provider
        // number-series atomik (QBE-CODE-001..006).
        var candidate = $"OIB-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string GenerateReceivableNumber()
    {
        var candidate = $"AR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private static string GeneratePayableNumber()
    {
        var candidate = $"AP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private string ResolvePhysicalPath(Guid batchId, string sourceFormat)
    {
        var extension = sourceFormat.Equals("CSV", StringComparison.OrdinalIgnoreCase) ? ".csv" : ".xlsx";
        var directory = Path.Combine(_environment.ContentRootPath, "Storage", "uploads", "finance", "opening-item-batches");
        return Path.Combine(directory, $"{batchId:N}{extension}");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Nama tampilan sekumpulan pengguna, diambil SEKALI (BE-FIN-089, FIN-DES-095). Mengikuti pola
    /// <c>GetUserNamesAsync</c> pada <c>FinanceOpeningBalanceService</c>: ID yang tidak ditemukan
    /// tidak ikut dikembalikan. Dipakai <see cref="GetPagedAsync"/> supaya daftar berpaging tidak
    /// menimbulkan satu kueri per baris.
    /// </summary>
    private async Task<Dictionary<Guid, string?>> GetUserNamesAsync(
        IEnumerable<Guid?> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string?>();
        }

        return await _dbContext.Users.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
            .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
    }

    /// <summary>
    /// Nama tampilan satu pengguna (BE-FIN-089, FIN-DES-095). Mengikuti pola
    /// <c>DirectPaymentThresholdService.GetUserNameAsync</c> (BE-FIN-086): <c>null</c> bila ID
    /// kosong atau penggunanya tidak ditemukan. Dipakai respons tunggal (unggah, unggah ulang,
    /// validasi, deklarasi, setuju, tolak) — bukan daftar berpaging, yang memakai pencarian
    /// batch <see cref="GetUserNamesAsync"/> di atas.
    /// </summary>
    private async Task<string?> GetUserNameAsync(Guid? userId, CancellationToken cancellationToken)
    {
        if (userId is null || userId == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.Users.AsNoTracking()
            .Where(x => x.Id == userId.Value)
            .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Sisipan <see cref="ApprovedByName"/> dari kamus yang sudah diambil sekali (BE-FIN-089).</summary>
    private static OpeningItemBatchResponse MapWithName(FinOpeningItemBatch x, IReadOnlyDictionary<Guid, string?> names)
    {
        var response = Map(x);
        response.ApprovedByName = x.ApprovedBy.HasValue && names.TryGetValue(x.ApprovedBy.Value, out var name)
            ? name
            : null;
        return response;
    }

    /// <summary>Pembungkus <see cref="Map"/> yang menyertakan pencarian nama satu pengguna (BE-FIN-089).</summary>
    public async Task<OpeningItemBatchResponse> MapWithNameAsync(FinOpeningItemBatch x, CancellationToken cancellationToken)
    {
        var response = Map(x);
        response.ApprovedByName = await GetUserNameAsync(x.ApprovedBy, cancellationToken);
        return response;
    }

    /// <summary>Pembungkus <see cref="MapDetail"/> yang menyertakan pencarian nama satu pengguna (BE-FIN-089).</summary>
    public async Task<OpeningItemBatchDetailResponse> MapDetailWithNameAsync(
        FinOpeningItemBatch x, List<OpeningItemBatchRowValidationResult> rows, CancellationToken cancellationToken)
    {
        var response = MapDetail(x, rows);
        response.ApprovedByName = await GetUserNameAsync(x.ApprovedBy, cancellationToken);
        return response;
    }

    public static OpeningItemBatchResponse Map(FinOpeningItemBatch x) => new()
    {
        Id = x.Id,
        BatchNumber = x.BatchNumber,
        ItemKind = x.ItemKind,
        Status = x.Status,
        CutoverDate = x.CutoverDate,
        TotalItemCount = x.TotalItemCount,
        TotalOutstandingAmount = x.TotalOutstandingAmount,
        DeclaredAccountingOpeningAmount = x.DeclaredAccountingOpeningAmount,
        AccountingReferenceDocument = x.AccountingReferenceDocument,
        UploadedFileName = x.UploadedFileName,
        SourceFormat = x.SourceFormat,
        RejectionReason = x.RejectionReason,
        ApprovedBy = x.ApprovedBy,
        ApprovedAt = x.ApprovedAt,
        LockedAt = x.LockedAt,
        RowVersion = x.RowVersion
    };

    public static OpeningItemBatchDetailResponse MapDetail(FinOpeningItemBatch x, List<OpeningItemBatchRowValidationResult> rows)
    {
        var mapped = Map(x);
        return new OpeningItemBatchDetailResponse
        {
            Id = mapped.Id,
            BatchNumber = mapped.BatchNumber,
            ItemKind = mapped.ItemKind,
            Status = mapped.Status,
            CutoverDate = mapped.CutoverDate,
            TotalItemCount = mapped.TotalItemCount,
            TotalOutstandingAmount = mapped.TotalOutstandingAmount,
            DeclaredAccountingOpeningAmount = mapped.DeclaredAccountingOpeningAmount,
            AccountingReferenceDocument = mapped.AccountingReferenceDocument,
            UploadedFileName = mapped.UploadedFileName,
            SourceFormat = mapped.SourceFormat,
            RejectionReason = mapped.RejectionReason,
            ApprovedBy = mapped.ApprovedBy,
            ApprovedAt = mapped.ApprovedAt,
            LockedAt = mapped.LockedAt,
            RowVersion = mapped.RowVersion,
            Rows = rows
        };
    }
}
