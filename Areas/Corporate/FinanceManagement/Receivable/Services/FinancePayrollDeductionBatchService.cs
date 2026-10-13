using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Layanan jembatan manual potong gaji dan jadwal potongan cicilan per periode
/// (BE-FIN-116, BE-FIN-117, FIN-DEC-219..221, FR-FIN-253..256, FIN-INTEGRATION-1.10 §S.1..S.2, FIN-API-1.11 §S.1, FIN-VAL-272..286).
/// </summary>
public sealed class FinancePayrollDeductionBatchService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.PayrollDeductionBridge";
    private static readonly Regex PeriodRegex = new(@"^\d{4}-(0[1-9]|1[0-2])$", RegexOptions.Compiled);
    private static readonly CultureInfo IdCulture = new("id-ID");
    private static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;
    private readonly FinanceReceivablePayrollSyncService _payrollSyncService;
    private readonly NumberSeriesAllocator _numberSeriesAllocator;

    public FinancePayrollDeductionBatchService(
        ApplicationDbContext dbContext,
        LoggerService loggerService,
        FinanceReceivablePayrollSyncService payrollSyncService,
        NumberSeriesAllocator numberSeriesAllocator)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
        _payrollSyncService = payrollSyncService;
        _numberSeriesAllocator = numberSeriesAllocator;
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-116: Pembacaan Jadwal dan Ekspor (Excel / PDF)
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengambil jadwal potongan cicilan satu periode beserta total dan rincian per pegawai (AC 1, AC 2).
    /// </summary>
    public async Task<DeductionScheduleResponse> GetScheduleAsync(string period, CancellationToken cancellationToken)
    {
        ValidatePeriodFormat(period);

        // Cari master periode payroll HR bila kalendernya sudah terdaftar
        var year = int.Parse(period[..4]);
        var month = int.Parse(period[5..7]);
        var payrollPeriod = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .Where(x => !x.IsDelete && x.FiscalYear == year && x.PeriodNumber == month && x.PeriodType == "Monthly")
            .OrderByDescending(x => x.CreateDateTime)
            .FirstOrDefaultAsync(cancellationToken);

        // Kueri angsuran:
        // 1. Status angsuran DIJADWALKAN pada periode ini (AC 1: baris TERTUNGGAK dari bulan sebelumnya TIDAK muncul lagi).
        // 2. Status perjanjian induk DISETUJUI (AC 2: perjanjian MENUNGGU / status lain TIDAK muncul).
        var items = await (
            from inst in _dbContext.FinReceivableInstallments.AsNoTracking()
            join plan in _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
                on inst.PlanId equals plan.Id
            join rec in _dbContext.FinReceivables.AsNoTracking()
                on plan.ReceivableId equals rec.Id
            where !inst.IsDelete
                  && inst.DeductionPeriod == period
                  && inst.Status == FinReceivableInstallmentStatuses.Dijadwalkan
                  && !plan.IsDelete
                  && plan.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                  && !rec.IsDelete
            select new
            {
                Installment = inst,
                Plan = plan,
                Receivable = rec
            }
        ).ToListAsync(cancellationToken);

        // Muat profil pegawai untuk seluruh BenefitOwnerId unik
        var employeeIds = items
            .Where(x => x.Receivable.BenefitOwnerId.HasValue)
            .Select(x => x.Receivable.BenefitOwnerId!.Value)
            .Distinct()
            .ToList();

        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Include(e => e.PrimaryDepartment)
            .Where(e => employeeIds.Contains(e.Id) && !e.IsDelete)
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        // Susun baris respons terurut NIP lalu nomor angsuran
        var rows = items.Select(x =>
        {
            MstEmployee? emp = null;
            if (x.Receivable.BenefitOwnerId.HasValue)
            {
                employees.TryGetValue(x.Receivable.BenefitOwnerId.Value, out emp);
            }

            return new DeductionScheduleRowResponse
            {
                InstallmentId = x.Installment.Id,
                InstallmentCode = $"{x.Plan.PlanNumber}-{x.Installment.InstallmentNumber}",
                EmployeeNumber = emp?.EmployeeNumber ?? string.Empty,
                EmployeeName = emp?.FullName ?? string.Empty,
                OrganizationUnitName = emp?.PrimaryDepartment?.DepartmentName ?? string.Empty,
                ReceivableNumber = x.Receivable.ReceivableNumber,
                InstallmentNumber = x.Installment.InstallmentNumber,
                ScheduledAmount = x.Installment.ScheduledAmount,
                CarriedOverAmount = x.Installment.CarriedOverAmount,
                // DueAmount = ScheduledAmount + CarriedOverAmount - PaidAmount (AC 1)
                DueAmount = x.Installment.OutstandingAmount
            };
        })
        .OrderBy(x => x.EmployeeNumber)
        .ThenBy(x => x.InstallmentNumber)
        .ToList();

        return new DeductionScheduleResponse
        {
            Period = period,
            PayrollPeriodId = payrollPeriod?.Id,
            TotalDueAmount = rows.Sum(x => x.DueAmount),
            RowCount = rows.Count,
            Rows = rows
        };
    }

    /// <summary>
    /// Ekspor jadwal ke Excel; sekaligus menjadi templat unggah hasil batch (AC 3, FIN-DEC-219).
    /// Kolom Status Potongan dan Nominal Terpotong SENGAJA dibiarkan KOSONG untuk diisi tim penggajian.
    /// </summary>
    public async Task<byte[]> ExportScheduleExcelAsync(string period, CancellationToken cancellationToken)
    {
        var schedule = await GetScheduleAsync(period, cancellationToken);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add($"Jadwal Potong {period}");

        string[] headers =
        [
            "No",
            "Kode Angsuran",
            "NIP",
            "Nama Pegawai",
            "Unit Kerja",
            "No. Piutang",
            "Angsuran Ke",
            "Nominal Terjadwal",
            "Tunggakan Terbawa",
            "Nominal Terutang",
            "Status Potongan",
            "Nominal Terpotong"
        ];

        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        var headerRange = ws.Range(1, 1, 1, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        var r = 2;
        foreach (var row in schedule.Rows)
        {
            ws.Cell(r, 1).Value = r - 1;
            ws.Cell(r, 2).Value = row.InstallmentCode;
            ws.Cell(r, 3).Value = row.EmployeeNumber;
            ws.Cell(r, 4).Value = row.EmployeeName;
            ws.Cell(r, 5).Value = row.OrganizationUnitName;
            ws.Cell(r, 6).Value = row.ReceivableNumber;
            ws.Cell(r, 7).Value = row.InstallmentNumber;
            ws.Cell(r, 8).Value = row.ScheduledAmount;
            ws.Cell(r, 9).Value = row.CarriedOverAmount;
            ws.Cell(r, 10).Value = row.DueAmount;

            // AC 3: Kolom status dan nominal terpotong KOSONG untuk diisi oleh tim penggajian (HR)
            ws.Cell(r, 11).Value = string.Empty;
            ws.Cell(r, 12).Value = string.Empty;
            r++;
        }

        if (r > 2)
        {
            ws.Range(2, 8, r - 1, 10).Style.NumberFormat.Format = "#,##0";
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Ekspor jadwal ke PDF untuk keperluan serah terima arsip fisik / cetak (FIN-DEC-219).
    /// </summary>
    public async Task<byte[]> ExportSchedulePdfAsync(string period, CancellationToken cancellationToken)
    {
        var schedule = await GetScheduleAsync(period, cancellationToken);
        return DeductionScheduleReportDocuments.BuildSchedulePdf(schedule);
    }

    // ------------------------------------------------------------------------------------
    // BE-FIN-117: Unggah Berkas Batch, Validasi Baris, Pengajuan, dan Pengesahan Atomik
    // ------------------------------------------------------------------------------------

    /// <summary>
    /// Mengunggah berkas hasil potongan penggajian, memvalidasi per baris, dan membuat batch DRAFT (AC 1, AC 7, AC 8).
    /// </summary>
    public async Task<DeductionBatchResponse> UploadBatchAsync(
        UploadDeductionBatchRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        ValidatePeriodFormat(request.Period);

        // FIN-VAL-272: Periode MUST berformat YYYY-MM dan PayrollPeriodId MUST menunjuk master periode aktif pada bulan yang sama
        var year = int.Parse(request.Period[..4]);
        var month = int.Parse(request.Period[5..7]);
        var payrollPeriod = await _dbContext.MstPayrollPeriods.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PayrollPeriodId && !x.IsDelete, cancellationToken);

        if (payrollPeriod is null || payrollPeriod.FiscalYear != year || payrollPeriod.PeriodNumber != month)
        {
            throw new FinancePayrollDeductionBatchValidationException("Periode payroll tidak cocok dengan bulan yang dipilih.");
        }

        // FIN-VAL-286: Batch manual untuk periode >= AutomaticFromPeriod ditolak (AC 8)
        var cutover = await _dbContext.MstPayrollDeductionCutovers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsActive && !x.IsDelete, cancellationToken);
        if (cutover is not null && string.CompareOrdinal(request.Period, cutover.AutomaticFromPeriod) >= 0)
        {
            throw new FinancePayrollDeductionBatchValidationException(
                $"Periode {request.Period} sudah ditangani mesin payroll; pencatatan manual tidak diizinkan.");
        }

        var batchType = string.IsNullOrWhiteSpace(request.BatchType)
            ? FinPayrollDeductionBatchTypes.Reguler
            : request.BatchType.Trim().ToUpperInvariant();

        if (batchType != FinPayrollDeductionBatchTypes.Reguler && batchType != FinPayrollDeductionBatchTypes.Koreksi)
        {
            throw new FinancePayrollDeductionBatchBadRequestException("BatchType harus REGULER atau KOREKSI.");
        }

        // FIN-VAL-282: Hanya satu batch REGULER aktif (DRAFT, DIAJUKAN, DISAHKAN) per periode (AC 7)
        if (batchType == FinPayrollDeductionBatchTypes.Reguler)
        {
            var hasActiveRegular = await _dbContext.FinPayrollDeductionBatches.AsNoTracking()
                .AnyAsync(x => !x.IsDelete
                               && x.DeductionPeriod == request.Period
                               && x.BatchType == FinPayrollDeductionBatchTypes.Reguler
                               && (x.Status == FinPayrollDeductionBatchStatuses.Draft
                                   || x.Status == FinPayrollDeductionBatchStatuses.Diajukan
                                   || x.Status == FinPayrollDeductionBatchStatuses.Disahkan),
                    cancellationToken);

            if (hasActiveRegular)
            {
                throw new FinancePayrollDeductionBatchConflictException(
                    $"Sudah ada batch hasil potongan untuk periode {request.Period}.");
            }
        }
        else
        {
            // FIN-VAL-285: Batch KOREKSI MUST punya alasan dan menunjuk periode yang sudah punya batch REGULER DISAHKAN
            if (string.IsNullOrWhiteSpace(request.CorrectionReason))
            {
                throw new FinancePayrollDeductionBatchValidationException("Alasan koreksi wajib diisi.");
            }

            var hasApprovedRegular = await _dbContext.FinPayrollDeductionBatches.AsNoTracking()
                .AnyAsync(x => !x.IsDelete
                               && x.DeductionPeriod == request.Period
                               && x.BatchType == FinPayrollDeductionBatchTypes.Reguler
                               && x.Status == FinPayrollDeductionBatchStatuses.Disahkan,
                    cancellationToken);

            if (!hasApprovedRegular)
            {
                throw new FinancePayrollDeductionBatchValidationException(
                    "Koreksi hanya dapat dilakukan untuk periode yang sudah memiliki batch reguler berstatus DISAHKAN.");
            }
        }

        // FIN-VAL-273: Berkas MUST .xlsx/.csv, memuat kolom wajib, dan maksimal 2.000 baris
        if (request.File is null || request.File.Length == 0)
        {
            throw new FinancePayrollDeductionBatchBadRequestException("Berkas hasil potongan tidak boleh kosong.");
        }

        var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".csv")
        {
            throw new FinancePayrollDeductionBatchBadRequestException(
                "Berkas tidak sesuai templat atau melebihi 2.000 baris. Unduh ulang templat dari jadwal potongan.");
        }

        var rawRows = ext == ".xlsx"
            ? ParseXlsx(request.File)
            : await ParseCsvAsync(request.File, cancellationToken);

        if (rawRows.Count > 2000)
        {
            throw new FinancePayrollDeductionBatchBadRequestException(
                "Berkas tidak sesuai templat atau melebihi 2.000 baris. Unduh ulang templat dari jadwal potongan.");
        }

        // Ambil data referensi seluruh angsuran dan karyawan untuk validasi
        var allInstallmentData = await (
            from inst in _dbContext.FinReceivableInstallments.AsNoTracking()
            join plan in _dbContext.FinReceivableInstallmentPlans.AsNoTracking() on inst.PlanId equals plan.Id
            join rec in _dbContext.FinReceivables.AsNoTracking() on plan.ReceivableId equals rec.Id
            where !inst.IsDelete && !plan.IsDelete && !rec.IsDelete
            select new
            {
                Installment = inst,
                Plan = plan,
                Receivable = rec
            }
        ).ToListAsync(cancellationToken);

        var employeeIds = allInstallmentData
            .Where(x => x.Receivable.BenefitOwnerId.HasValue)
            .Select(x => x.Receivable.BenefitOwnerId!.Value)
            .Distinct()
            .ToList();

        var employees = await _dbContext.MstEmployees.AsNoTracking()
            .Where(e => employeeIds.Contains(e.Id) && !e.IsDelete)
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        // Buat lookup kode angsuran: Code -> Detail
        var codeLookup = allInstallmentData.ToDictionary(
            x => $"{x.Plan.PlanNumber}-{x.Installment.InstallmentNumber}".Trim().ToUpperInvariant(),
            x =>
            {
                MstEmployee? emp = null;
                if (x.Receivable.BenefitOwnerId.HasValue)
                {
                    employees.TryGetValue(x.Receivable.BenefitOwnerId.Value, out emp);
                }

                return new
                {
                    x.Installment,
                    x.Plan,
                    x.Receivable,
                    Employee = emp
                };
            },
            StringComparer.OrdinalIgnoreCase);

        var batchId = Guid.NewGuid();
        var seenCodes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var batchItems = new List<FinPayrollDeductionBatchItem>();

        for (var i = 0; i < rawRows.Count; i++)
        {
            var raw = rawRows[i];
            var rowNum = i + 1;

            var code = raw.GetValue("Kode Angsuran", "KodeAngsuran", "InstallmentCode")?.Trim() ?? string.Empty;
            var nip = raw.GetValue("NIP", "EmployeeNumber")?.Trim() ?? string.Empty;
            var statusStr = raw.GetValue("Status Potongan", "StatusPotongan", "ResultStatus", "Status")?.Trim().ToUpperInvariant() ?? string.Empty;
            var amountStr = raw.GetValue("Nominal Terpotong", "NominalTerpotong", "DeductedAmount", "Nominal")?.Trim() ?? "0";

            var item = new FinPayrollDeductionBatchItem
            {
                Id = Guid.NewGuid(),
                BatchId = batchId,
                RowNumber = rowNum,
                InstallmentCode = code,
                EmployeeNumber = nip,
                ResultStatus = string.IsNullOrWhiteSpace(statusStr) ? FinPayrollDeductionItemResultStatuses.Gagal : statusStr,
                ValidationStatus = FinPayrollDeductionItemValidationStatuses.Valid,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            // Parse nominal
            if (!decimal.TryParse(amountStr, NumberStyles.Number | NumberStyles.AllowCurrencySymbol, IdCulture, out var parsedAmount)
                && !decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedAmount))
            {
                item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                item.ValidationMessage = $"Baris {rowNum}: nominal terpotong '{amountStr}' tidak valid.";
                batchItems.Add(item);
                continue;
            }

            item.DeductedAmount = parsedAmount;

            // FIN-VAL-277: Satu angsuran MUST NOT muncul dua kali dalam satu berkas
            if (!string.IsNullOrWhiteSpace(code) && seenCodes.TryGetValue(code, out var firstSeenRow))
            {
                item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                item.ValidationMessage = $"Baris {rowNum}: angsuran {code} sudah ada di baris {firstSeenRow}.";
                batchItems.Add(item);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(code))
            {
                seenCodes[code] = rowNum;
            }

            // FIN-VAL-274: Kode angsuran MUST ada dan NIP MUST sama dengan pemilik manfaat angsuran itu
            if (string.IsNullOrWhiteSpace(code) || !codeLookup.TryGetValue(code, out var target))
            {
                item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                item.ValidationMessage = $"Baris {rowNum}: kode angsuran {code} tidak cocok dengan NIP {nip}.";
                batchItems.Add(item);
                continue;
            }

            item.InstallmentId = target.Installment.Id;

            // Untuk KOREKSI, due adalah total kewajiban angsuran (ScheduledAmount + CarriedOverAmount)
            // Untuk REGULER, due adalah OutstandingAmount saat ini
            var due = batchType == FinPayrollDeductionBatchTypes.Koreksi
                ? (target.Installment.ScheduledAmount + target.Installment.CarriedOverAmount)
                : target.Installment.OutstandingAmount;

            item.DueAmount = due;
            if (batchType == FinPayrollDeductionBatchTypes.Koreksi)
            {
                item.PreviousDeductedAmount = target.Installment.PaidAmount;
                item.DeltaAmount = item.DeductedAmount - target.Installment.PaidAmount;
            }

            var targetNip = target.Employee?.EmployeeNumber ?? string.Empty;
            if (!string.Equals(targetNip, nip, StringComparison.OrdinalIgnoreCase))
            {
                item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                item.ValidationMessage = $"Baris {rowNum}: kode angsuran {code} tidak cocok dengan NIP {nip}.";
                batchItems.Add(item);
                continue;
            }

            // FIN-VAL-275: Angsuran MUST milik perjanjian DISETUJUI dan periode potongannya sama dengan periode batch
            if (target.Plan.Status != FinReceivableInstallmentPlanStatuses.Disetujui
                || !string.Equals(target.Installment.DeductionPeriod, request.Period, StringComparison.OrdinalIgnoreCase))
            {
                item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                item.ValidationMessage = $"Baris {rowNum}: angsuran {code} bukan jadwal periode {request.Period}.";
                batchItems.Add(item);
                continue;
            }

            // FIN-VAL-276: Status vs nominal:
            // BERHASIL => nominal = terutang
            // SEBAGIAN => 0 < nominal < terutang
            // GAGAL => nominal = 0
            if (item.ResultStatus == FinPayrollDeductionItemResultStatuses.Berhasil)
            {
                if (item.DeductedAmount != due)
                {
                    item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                    item.ValidationMessage = $"Baris {rowNum}: status BERHASIL tidak sesuai nominal Rp {item.DeductedAmount:N0} (terutang Rp {due:N0}).";
                }
            }
            else if (item.ResultStatus == FinPayrollDeductionItemResultStatuses.Sebagian)
            {
                if (item.DeductedAmount <= 0 || item.DeductedAmount >= due)
                {
                    item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                    item.ValidationMessage = $"Baris {rowNum}: status SEBAGIAN tidak sesuai nominal Rp {item.DeductedAmount:N0} (terutang Rp {due:N0}).";
                }
            }
            else if (item.ResultStatus == FinPayrollDeductionItemResultStatuses.Gagal)
            {
                if (item.DeductedAmount != 0)
                {
                    item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                    item.ValidationMessage = $"Baris {rowNum}: status GAGAL tidak sesuai nominal Rp {item.DeductedAmount:N0} (terutang Rp {due:N0}).";
                }
            }
            else
            {
                item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                item.ValidationMessage = $"Baris {rowNum}: status {item.ResultStatus} tidak sesuai nominal Rp {item.DeductedAmount:N0} (terutang Rp {due:N0}).";
            }

            // FIN-VAL-285: Khusus KOREKSI, angsuran berikutnya MUST belum menerima hasil (AC 2)
            if (batchType == FinPayrollDeductionBatchTypes.Koreksi && item.ValidationStatus == FinPayrollDeductionItemValidationStatuses.Valid)
            {
                var nextInst = allInstallmentData.FirstOrDefault(
                    x => x.Installment.PlanId == target.Installment.PlanId
                         && x.Installment.InstallmentNumber == target.Installment.InstallmentNumber + 1);

                if (nextInst is not null && (nextInst.Installment.Status != FinReceivableInstallmentStatuses.Dijadwalkan || nextInst.Installment.PaidAmount > 0))
                {
                    item.ValidationStatus = FinPayrollDeductionItemValidationStatuses.Invalid;
                    item.ValidationMessage = $"Koreksi angsuran {code} tidak dapat dilakukan karena angsuran berikutnya sudah menerima hasil. Gunakan penyesuaian piutang.";
                }
            }

            batchItems.Add(item);
        }

        var totalRows = batchItems.Count;
        var invalidRows = batchItems.Count(x => x.ValidationStatus == FinPayrollDeductionItemValidationStatuses.Invalid);
        var totalDeducted = batchItems
            .Where(x => x.ValidationStatus == FinPayrollDeductionItemValidationStatuses.Valid)
            .Sum(x => x.DeductedAmount);

        var batchNumber = await GenerateBatchNumberAsync(actorUserId, cancellationToken);

        var batch = new FinPayrollDeductionBatch
        {
            Id = batchId,
            BatchNumber = batchNumber,
            DeductionPeriod = request.Period,
            PayrollPeriodId = request.PayrollPeriodId,
            BatchType = batchType,
            Status = FinPayrollDeductionBatchStatuses.Draft,
            ControlTotalAmount = request.ControlTotalAmount,
            TotalDeductedAmount = totalDeducted,
            TotalRows = totalRows,
            InvalidRows = invalidRows,
            CorrectionReason = batchType == FinPayrollDeductionBatchTypes.Koreksi ? request.CorrectionReason : null,
            SourceFileName = request.File.FileName,
            SubmittedBy = actorUserId,
            RowVersion = Guid.NewGuid(),
            CreateBy = actorUserId,
            CreateDateTime = DateTime.UtcNow,
            Items = batchItems
        };

        _dbContext.FinPayrollDeductionBatches.Add(batch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "Batch.Uploaded",
            $"Batch hasil potongan diunggah. BatchNumber={batch.BatchNumber} Period={batch.DeductionPeriod} TotalRows={totalRows} InvalidRows={invalidRows}",
            new { batch.Id, batch.BatchNumber, batch.DeductionPeriod, batch.BatchType, totalRows, invalidRows, ActorUserId = actorUserId });

        return await GetBatchDetailAsync(batch.Id, cancellationToken);
    }

    /// <summary>
    /// Mengajukan batch dengan nomor laporan penggajian dan bukti dokumen jenis LAPORAN-PENGGAJIAN (AC 2, AC 3).
    /// </summary>
    public async Task<DeductionBatchResponse> SubmitBatchAsync(
        Guid id, SubmitDeductionBatchRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinPayrollDeductionBatches
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new FinancePayrollDeductionBatchNotFoundException("Batch hasil potongan tidak ditemukan.");

        if (batch.RowVersion != request.RowVersion)
        {
            throw new FinancePayrollDeductionBatchConflictException("Data telah berubah oleh pengguna lain. Silakan muat ulang.");
        }

        if (batch.Status != FinPayrollDeductionBatchStatuses.Draft)
        {
            throw new FinancePayrollDeductionBatchConflictException($"Batch berstatus {batch.Status} tidak dapat diajukan.");
        }

        // FIN-VAL-283: Batch dengan baris tidak valid MUST NOT diajukan
        if (batch.InvalidRows > 0)
        {
            throw new FinancePayrollDeductionBatchValidationException($"Perbaiki {batch.InvalidRows} baris tidak valid lalu unggah ulang.");
        }

        // FIN-VAL-280: Nomor laporan penggajian dan bukti jenis LAPORAN-PENGGAJIAN MUST ada saat diajukan
        if (string.IsNullOrWhiteSpace(request.PayrollReportNumber) || request.ProofId == Guid.Empty)
        {
            throw new FinancePayrollDeductionBatchValidationException("Nomor dan lampiran laporan penggajian wajib diisi sebelum diajukan.");
        }

        var proof = await _dbContext.FinTransactionProofs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ProofId && !x.IsDelete, cancellationToken);

        if (proof is null || proof.ProofType != FinTransactionProofTypes.LaporanPenggajian)
        {
            throw new FinancePayrollDeductionBatchValidationException("Nomor dan lampiran laporan penggajian wajib diisi sebelum diajukan.");
        }

        // FIN-VAL-278: Batch REGULER MUST memuat setiap angsuran pada jadwal periode itu (AC 2)
        if (batch.BatchType == FinPayrollDeductionBatchTypes.Reguler)
        {
            var schedule = await GetScheduleAsync(batch.DeductionPeriod, cancellationToken);
            var batchInstallmentIds = batch.Items
                .Where(x => x.InstallmentId.HasValue)
                .Select(x => x.InstallmentId!.Value)
                .ToHashSet();

            var missingCount = schedule.Rows.Count(r => !batchInstallmentIds.Contains(r.InstallmentId));
            if (missingCount > 0)
            {
                throw new FinancePayrollDeductionBatchValidationException(
                    $"{missingCount} angsuran terjadwal belum ada di berkas. Tulis GAGAL untuk yang tidak dipotong.");
            }
        }

        // FIN-VAL-279: Total kontrol MUST sama dengan jumlah nominal terpotong (AC 3)
        if (batch.ControlTotalAmount != batch.TotalDeductedAmount)
        {
            throw new FinancePayrollDeductionBatchValidationException(
                $"Total berkas Rp {batch.TotalDeductedAmount:N0} tidak sama dengan total laporan penggajian Rp {batch.ControlTotalAmount:N0}.");
        }

        batch.Status = FinPayrollDeductionBatchStatuses.Diajukan;
        batch.PayrollReportNumber = request.PayrollReportNumber.Trim();
        batch.ProofId = request.ProofId;
        batch.SubmittedBy = actorUserId;
        batch.SubmittedAt = DateTimeOffset.UtcNow;
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "Batch.Submitted",
            $"Batch diajukan. BatchNumber={batch.BatchNumber} ReportNumber={batch.PayrollReportNumber}",
            new { batch.Id, batch.BatchNumber, ActorUserId = actorUserId });

        return await GetBatchDetailAsync(batch.Id, cancellationToken);
    }

    /// <summary>
    /// Mengesahkan batch REGULER dan menerapkan seluruh baris dalam satu transaksi atomik (AC 4, AC 5, AC 6).
    /// </summary>
    public async Task<DeductionBatchResponse> ApproveBatchAsync(
        Guid id, DeductionBatchTransitionRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinPayrollDeductionBatches
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new FinancePayrollDeductionBatchNotFoundException("Batch hasil potongan tidak ditemukan.");

        if (batch.RowVersion != request.RowVersion)
        {
            throw new FinancePayrollDeductionBatchConflictException("Data telah berubah oleh pengguna lain. Silakan muat ulang.");
        }

        if (batch.Status != FinPayrollDeductionBatchStatuses.Diajukan)
        {
            throw new FinancePayrollDeductionBatchConflictException($"Batch berstatus {batch.Status} tidak dapat disahkan.");
        }

        if (batch.BatchType != FinPayrollDeductionBatchTypes.Reguler)
        {
            throw new FinancePayrollDeductionBatchBadRequestException(
                "Endpoint ini hanya untuk batch REGULER. Gunakan endpoint approve-correction untuk batch KOREKSI.");
        }

        // FIN-VAL-281: Pengesah MUST berbeda dari pengaju (AC 4)
        if (batch.SubmittedBy.HasValue && batch.SubmittedBy.Value == actorUserId)
        {
            throw new FinancePayrollDeductionBatchForbiddenException("Pengaju tidak boleh mengesahkan batchnya sendiri.");
        }

        // FIN-VAL-284: Pengesahan MUST menerapkan seluruh baris dalam satu transaksi; satu gagal, semua batal (AC 5)
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            }

            // Kunci per angsuran berurutan menurut InstallmentId dalam satu transaksi untuk mencegah deadlock (FIN-DES-106)
            var sortedItems = batch.Items
                .Where(x => x.InstallmentId.HasValue)
                .OrderBy(x => x.InstallmentId!.Value)
                .ToList();

            var now = DateTimeOffset.UtcNow;
            foreach (var item in sortedItems)
            {
                if (_dbContext.Database.IsRelational())
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock(hashtext({0}));",
                        [$"FIN_INSTALLMENT_PAYROLL_RESULT_{item.InstallmentId!.Value:N}"],
                        cancellationToken);
                }

                var syncRequest = new PayrollResultRequest
                {
                    InstallmentId = item.InstallmentId!.Value,
                    PayrollPeriodId = batch.PayrollPeriodId,
                    Status = item.ResultStatus,
                    DeductedAmount = item.DeductedAmount,
                    ExecutionTimestamp = now,
                    PayrollRunId = null, // Manual batch (sengaja null per FIN-DES-105)
                    Notes = $"Batch {batch.BatchNumber} baris ke-{item.RowNumber}"
                };

                // Terapkan hasil potongan ke angsuran, mutasi subledger, dan pelimpahan tunggakan (AC 6)
                await _payrollSyncService.ApplyResultCoreAsync(syncRequest, actorUserId, cancellationToken);

                item.AppliedAt = now;
                item.UpdateDateTime = DateTime.UtcNow;
                item.UpdateBy = actorUserId;
            }

            batch.Status = FinPayrollDeductionBatchStatuses.Disahkan;
            batch.ApprovedBy = actorUserId;
            batch.ApprovedAt = now;
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorUserId;
            batch.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }

            await _loggerService.AuditAsync(LogCategory, "Batch.ApproveFailed",
                $"Pengesahan batch {batch.BatchNumber} gagal: {ex.Message}. Transaksi dibatalkan.",
                new { batch.Id, batch.BatchNumber, Error = ex.Message, ActorUserId = actorUserId });

            throw new FinancePayrollDeductionBatchValidationException(
                $"Batch gagal disahkan: {ex.Message}. Tidak ada saldo yang berubah.");
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }

        await _loggerService.AuditAsync(LogCategory, "Batch.Approved",
            $"Batch disahkan secara atomik. BatchNumber={batch.BatchNumber}",
            new { batch.Id, batch.BatchNumber, ActorUserId = actorUserId });

        return await GetBatchDetailAsync(batch.Id, cancellationToken);
    }

    /// <summary>
    /// Mengesahkan batch KOREKSI (FIN-API-1.11 §S.1, BE-FIN-117/BE-FIN-118).
    /// </summary>
    public async Task<DeductionBatchResponse> ApproveCorrectionBatchAsync(
        Guid id, DeductionBatchTransitionRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinPayrollDeductionBatches
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new FinancePayrollDeductionBatchNotFoundException("Batch hasil potongan tidak ditemukan.");

        if (batch.RowVersion != request.RowVersion)
        {
            throw new FinancePayrollDeductionBatchConflictException("Data telah berubah oleh pengguna lain. Silakan muat ulang.");
        }

        if (batch.Status != FinPayrollDeductionBatchStatuses.Diajukan)
        {
            throw new FinancePayrollDeductionBatchConflictException($"Batch berstatus {batch.Status} tidak dapat disahkan.");
        }

        if (batch.BatchType != FinPayrollDeductionBatchTypes.Koreksi)
        {
            throw new FinancePayrollDeductionBatchBadRequestException("Endpoint ini hanya untuk batch KOREKSI.");
        }

        // FIN-VAL-281: Pengesah MUST berbeda dari pengaju
        if (batch.SubmittedBy.HasValue && batch.SubmittedBy.Value == actorUserId)
        {
            throw new FinancePayrollDeductionBatchForbiddenException("Pengaju tidak boleh mengesahkan batchnya sendiri.");
        }

        // Pengesahan koreksi atomik (FIN-DES-106, FIN-DES-107, AC 1, AC 2)
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            }

            var sortedItems = batch.Items
                .Where(x => x.InstallmentId.HasValue)
                .OrderBy(x => x.InstallmentId!.Value)
                .ToList();

            var now = DateTimeOffset.UtcNow;
            foreach (var item in sortedItems)
            {
                if (_dbContext.Database.IsRelational())
                {
                    await _dbContext.Database.ExecuteSqlRawAsync(
                        "SELECT pg_advisory_xact_lock(hashtext({0}));",
                        [$"FIN_INSTALLMENT_PAYROLL_RESULT_{item.InstallmentId!.Value:N}"],
                        cancellationToken);
                }

                // Terapkan selisih koreksi ke kartu piutang, mutasi PEMBALIKAN-POTONGAN-GAJI / POTONGAN-GAJI,
                // saldo angsuran, dan penyesuaian tunggakan periode berikutnya (AC 1, AC 2)
                await _payrollSyncService.ApplyCorrectionCoreAsync(
                    installmentId: item.InstallmentId!.Value,
                    newDeductedAmount: item.DeductedAmount,
                    newResultStatus: item.ResultStatus,
                    actorUserId: actorUserId,
                    notes: batch.CorrectionReason ?? request.Notes,
                    cancellationToken: cancellationToken);

                item.AppliedAt = now;
                item.UpdateDateTime = DateTime.UtcNow;
                item.UpdateBy = actorUserId;
            }

            batch.Status = FinPayrollDeductionBatchStatuses.Disahkan;
            batch.ApprovedBy = actorUserId;
            batch.ApprovedAt = now;
            batch.UpdateDateTime = DateTime.UtcNow;
            batch.UpdateBy = actorUserId;
            batch.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }

            await _loggerService.AuditAsync(LogCategory, "Batch.CorrectionApproveFailed",
                $"Pengesahan batch koreksi {batch.BatchNumber} gagal: {ex.Message}. Transaksi dibatalkan.",
                new { batch.Id, batch.BatchNumber, Error = ex.Message, ActorUserId = actorUserId });

            throw new FinancePayrollDeductionBatchValidationException(
                $"Batch gagal disahkan: {ex.Message}. Tidak ada saldo yang berubah.");
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }

        await _loggerService.AuditAsync(LogCategory, "Batch.CorrectionApproved",
            $"Batch koreksi disahkan. BatchNumber={batch.BatchNumber}",
            new { batch.Id, batch.BatchNumber, ActorUserId = actorUserId });

        return await GetBatchDetailAsync(batch.Id, cancellationToken);
    }

    /// <summary>
    /// Menolak batch yang diajukan; alasan wajib (FIN-API-1.11 §S.1).
    /// </summary>
    public async Task<DeductionBatchResponse> RejectBatchAsync(
        Guid id, RejectDeductionBatchRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinPayrollDeductionBatches
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new FinancePayrollDeductionBatchNotFoundException("Batch hasil potongan tidak ditemukan.");

        if (batch.RowVersion != request.RowVersion)
        {
            throw new FinancePayrollDeductionBatchConflictException("Data telah berubah oleh pengguna lain. Silakan muat ulang.");
        }

        if (batch.Status != FinPayrollDeductionBatchStatuses.Diajukan)
        {
            throw new FinancePayrollDeductionBatchConflictException($"Batch berstatus {batch.Status} tidak dapat ditolak.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new FinancePayrollDeductionBatchValidationException("Alasan penolakan wajib diisi.");
        }

        batch.Status = FinPayrollDeductionBatchStatuses.Ditolak;
        batch.RejectedBy = actorUserId;
        batch.RejectedAt = DateTimeOffset.UtcNow;
        batch.RejectionReason = request.Reason.Trim();
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "Batch.Rejected",
            $"Batch ditolak. BatchNumber={batch.BatchNumber} Reason={batch.RejectionReason}",
            new { batch.Id, batch.BatchNumber, ActorUserId = actorUserId });

        return await GetBatchDetailAsync(batch.Id, cancellationToken);
    }

    /// <summary>
    /// Membatalkan batch DRAFT atau DIAJUKAN; alasan wajib (FIN-API-1.11 §S.1).
    /// </summary>
    public async Task<DeductionBatchResponse> CancelBatchAsync(
        Guid id, CancelDeductionBatchRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinPayrollDeductionBatches
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new FinancePayrollDeductionBatchNotFoundException("Batch hasil potongan tidak ditemukan.");

        if (batch.RowVersion != request.RowVersion)
        {
            throw new FinancePayrollDeductionBatchConflictException("Data telah berubah oleh pengguna lain. Silakan muat ulang.");
        }

        if (batch.Status != FinPayrollDeductionBatchStatuses.Draft && batch.Status != FinPayrollDeductionBatchStatuses.Diajukan)
        {
            throw new FinancePayrollDeductionBatchConflictException($"Batch berstatus {batch.Status} tidak dapat dibatalkan.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new FinancePayrollDeductionBatchValidationException("Alasan pembatalan wajib diisi.");
        }

        batch.Status = FinPayrollDeductionBatchStatuses.Dibatalkan;
        batch.RejectionReason = request.Reason.Trim();
        batch.UpdateDateTime = DateTime.UtcNow;
        batch.UpdateBy = actorUserId;
        batch.RowVersion = Guid.NewGuid();

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "Batch.Canceled",
            $"Batch dibatalkan. BatchNumber={batch.BatchNumber} Reason={batch.RejectionReason}",
            new { batch.Id, batch.BatchNumber, ActorUserId = actorUserId });

        return await GetBatchDetailAsync(batch.Id, cancellationToken);
    }

    /// <summary>
    /// Mengambil daftar batch berpaging (FIN-API-1.11 §S.1).
    /// </summary>
    public async Task<PagedResult<DeductionBatchListResponse>> GetBatchesAsync(
        DeductionBatchPagedQuery query, CancellationToken cancellationToken)
    {
        var q = _dbContext.FinPayrollDeductionBatches.AsNoTracking().Where(x => !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(query.Period))
        {
            q = q.Where(x => x.DeductionPeriod == query.Period.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.BatchType))
        {
            q = q.Where(x => x.BatchType == query.BatchType.Trim().ToUpperInvariant());
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            q = q.Where(x => x.Status == query.Status.Trim().ToUpperInvariant());
        }

        var totalData = await q.CountAsync(cancellationToken);
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize is < 1 or > 50 ? 10 : query.PageSize;

        var items = await q
            .OrderByDescending(x => x.CreateDateTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DeductionBatchListResponse
            {
                Id = x.Id,
                BatchNumber = x.BatchNumber,
                Period = x.DeductionPeriod,
                BatchType = x.BatchType,
                Status = x.Status,
                ControlTotalAmount = x.ControlTotalAmount,
                TotalDeductedAmount = x.TotalDeductedAmount,
                TotalRows = x.TotalRows,
                InvalidRows = x.InvalidRows,
                SubmittedAt = x.SubmittedAt,
                ApprovedAt = x.ApprovedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DeductionBatchListResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)pageSize)
        };
    }

    /// <summary>
    /// Mengambil rincian batch beserta baris item dan angsuran yang belum ada di berkas (FIN-API-1.11 §S.1).
    /// </summary>
    public async Task<DeductionBatchResponse> GetBatchDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var batch = await _dbContext.FinPayrollDeductionBatches
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new FinancePayrollDeductionBatchNotFoundException("Batch hasil potongan tidak ditemukan.");

        // Ambil jadwal periode bersangkutan untuk mendeteksi MissingScheduledInstallments
        var schedule = await GetScheduleAsync(batch.DeductionPeriod, cancellationToken);
        var batchInstallmentIds = batch.Items
            .Where(x => x.InstallmentId.HasValue)
            .Select(x => x.InstallmentId!.Value)
            .ToHashSet();

        var missing = schedule.Rows
            .Where(r => !batchInstallmentIds.Contains(r.InstallmentId))
            .ToList();

        // Cari nama pegawai untuk baris-baris item
        var employeeNips = batch.Items.Select(x => x.EmployeeNumber).Distinct().ToList();
        var employeeMap = await _dbContext.MstEmployees.AsNoTracking()
            .Where(e => employeeNips.Contains(e.EmployeeNumber) && !e.IsDelete)
            .ToDictionaryAsync(e => e.EmployeeNumber, e => e.FullName, StringComparer.OrdinalIgnoreCase);

        var itemResponses = batch.Items
            .OrderBy(x => x.RowNumber)
            .Select(x =>
            {
                employeeMap.TryGetValue(x.EmployeeNumber, out var empName);
                return new DeductionBatchItemResponse
                {
                    RowNumber = x.RowNumber,
                    InstallmentCode = x.InstallmentCode,
                    EmployeeNumber = x.EmployeeNumber,
                    EmployeeName = empName ?? string.Empty,
                    ResultStatus = x.ResultStatus,
                    DeductedAmount = x.DeductedAmount,
                    DueAmount = x.DueAmount,
                    PreviousDeductedAmount = x.PreviousDeductedAmount ?? 0m,
                    DeltaAmount = x.DeltaAmount ?? 0m,
                    ValidationStatus = x.ValidationStatus,
                    ValidationMessage = x.ValidationMessage,
                    AppliedAt = x.AppliedAt
                };
            })
            .ToList();

        return new DeductionBatchResponse
        {
            Id = batch.Id,
            BatchNumber = batch.BatchNumber,
            Period = batch.DeductionPeriod,
            BatchType = batch.BatchType,
            Status = batch.Status,
            ControlTotalAmount = batch.ControlTotalAmount,
            TotalDeductedAmount = batch.TotalDeductedAmount,
            TotalRows = batch.TotalRows,
            InvalidRows = batch.InvalidRows,
            SubmittedAt = batch.SubmittedAt,
            ApprovedAt = batch.ApprovedAt,
            PayrollPeriodId = batch.PayrollPeriodId,
            PayrollReportNumber = batch.PayrollReportNumber,
            ProofId = batch.ProofId,
            CorrectionReason = batch.CorrectionReason,
            RejectionReason = batch.RejectionReason,
            RowVersion = batch.RowVersion,
            Items = itemResponses,
            MissingScheduledInstallments = missing
        };
    }

    // ------------------------------------------------------------------------------------
    // Helper Privat
    // ------------------------------------------------------------------------------------

    private async Task<string> GenerateBatchNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var effectiveUserId = actorUserId == Guid.Empty ? SystemFallbackUserId : actorUserId;
        var request = new NumberAllocationRequest(
            SequenceKey: "FIN_PAYROLL_DEDUCTION_BATCH",
            Prefix: "PDB",
            ResetPolicy: NumberSeriesResetPolicies.Monthly,
            SequenceDigits: 4,
            ActorUserId: effectiveUserId,
            Instant: DateTimeOffset.UtcNow,
            MinimumValue: 0,
            CompactScopeFormat: true,
            AllowSequenceOverflow: true);

        return await _numberSeriesAllocator.AllocateAsync(request, cancellationToken);
    }

    private static void ValidatePeriodFormat(string period)
    {
        if (string.IsNullOrWhiteSpace(period) || !PeriodRegex.IsMatch(period.Trim()))
        {
            throw new FinancePayrollDeductionBatchBadRequestException(
                "Format parameter period harus YYYY-MM dengan bulan 01 sampai 12 (contoh: 2026-09).");
        }
    }

    private static List<RawRow> ParseXlsx(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault()
            ?? throw new FinancePayrollDeductionBatchBadRequestException(
                "Berkas tidak sesuai templat atau melebihi 2.000 baris. Unduh ulang templat dari jadwal potongan.");

        var headerRow = worksheet.Row(1);
        var lastColumn = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        var headers = Enumerable.Range(1, lastColumn).Select(i => headerRow.Cell(i).GetString().Trim()).ToArray();

        var rows = new List<RawRow>();
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var r = 2; r <= lastRow; r++)
        {
            var excelRow = worksheet.Row(r);
            if (excelRow.IsEmpty()) continue;
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++)
            {
                values[headers[i]] = excelRow.Cell(i + 1).GetString().Trim();
            }
            rows.Add(new RawRow(r, values));
        }

        return rows;
    }

    private static async Task<List<RawRow>> ParseCsvAsync(IFormFile file, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine)) return [];
        var headers = headerLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();

        var rows = new List<RawRow>();
        var rowNumber = 1;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            rowNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cells = line.Split(',').Select(c => c.Trim().Trim('"')).ToArray();
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length && i < cells.Length; i++)
            {
                values[headers[i]] = cells[i];
            }
            rows.Add(new RawRow(rowNumber, values));
        }

        return rows;
    }

    private sealed record RawRow(int RowNumber, Dictionary<string, string> Values)
    {
        public string? GetValue(params string[] candidateKeys)
        {
            foreach (var key in candidateKeys)
            {
                if (Values.TryGetValue(key, out var val)) return val;
            }
            return null;
        }
    }
}
