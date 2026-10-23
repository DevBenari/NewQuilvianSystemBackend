using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Service read-only untuk Settlement AR V2 (Finance - Transaksi A/R - Pembayaran dan Pelunasan - Settlement AR).
/// Menampilkan piutang dan riwayat mutasi per pasien serta invoice, dengan filter tiga kategori (Perusahaan,
/// Pasien Umum, Karyawan), pagination 10 baris, perhitungan Beginning dan Ending Balance historis yang akurat,
/// serta ekspor Excel dan PDF.
///
/// HANYA MEMBACA. Service ini tidak mengubah status transaksi, tidak membuat mutasi, dan tidak menimbulkan side effect.
/// </summary>
public sealed class FinanceSettlementService
{
    private static readonly CultureInfo IdCulture = new("id-ID");
    private readonly ApplicationDbContext _dbContext;

    public FinanceSettlementService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Mengambil daftar Settlement AR terpaginasi (maksimum 10 baris) beserta ringkasan global sesuai filter.
    /// </summary>
    public async Task<FinanceSettlementPagedResponse> GetSettlementPagedAsync(
        FinanceSettlementQuery query, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 10); // Enforce strictly max 10 server-side

        var category = NormalizeCategory(query.Category);
        var queryable = BuildFilteredQueryable(query, category);

        var totalData = await queryable.CountAsync(cancellationToken);
        var totalPage = (int)Math.Ceiling(totalData / (double)pageSize);

        if (totalData == 0)
        {
            return new FinanceSettlementPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = 0,
                TotalPage = 1,
                Items = new List<FinanceSettlementRowResponse>(),
                Summary = new FinanceSettlementSummaryResponse
                {
                    Category = category,
                    TotalReceivables = 0,
                    GrandTotalPiutang = 0m,
                    GrandTotalBeginningBalance = 0m,
                    GrandTotalEndingBalance = 0m,
                    GrandTotalOutstanding = 0m,
                    PeriodStart = query.StartDate,
                    PeriodEnd = query.EndDate,
                    BalanceAsOfDate = query.EndDate ?? FinanceBusinessDate.Today(),
                    PeriodLabel = FormatPeriodLabel(query.StartDate, query.EndDate)
                }
            };
        }

        // Sorting
        queryable = ApplySorting(queryable, query.SortBy, query.SortDirection);

        var pagedList = await queryable
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var receivableIds = pagedList.Select(x => x.r.Id).Distinct().ToList();

        // Batch query movements untuk baris yang tampil pada halaman aktif
        var movements = receivableIds.Count > 0
            ? await _dbContext.FinReceivableMovements.AsNoTracking()
                .Where(m => receivableIds.Contains(m.ReceivableId) && !m.IsDelete)
                .OrderBy(m => m.BusinessDate)
                .ThenBy(m => m.OccurredAt)
                .ThenBy(m => m.CreateDateTime)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableMovement>();

        var movementLookup = movements.GroupBy(m => m.ReceivableId).ToDictionary(g => g.Key, g => g.ToList());

        var items = new List<FinanceSettlementRowResponse>(pagedList.Count);
        foreach (var row in pagedList)
        {
            movementLookup.TryGetValue(row.r.Id, out var recMovements);
            recMovements ??= new List<FinReceivableMovement>();

            var (begBal, endBal) = CalculateBalances(row.r, recMovements, query.StartDate, query.EndDate);
            var lastMovDate = recMovements.LastOrDefault()?.BusinessDate;

            items.Add(new FinanceSettlementRowResponse
            {
                ReceivableId = row.r.Id,
                ReceivableNumber = row.r.ReceivableNumber,
                InvoiceId = row.r.InvoiceId,
                InvoiceNumber = !string.IsNullOrWhiteSpace(row.inv?.InvoiceNumber)
                    ? row.inv.InvoiceNumber
                    : (!string.IsNullOrWhiteSpace(row.batchNumber) ? row.batchNumber : row.r.ReceivableNumber),
                PatientId = row.pat?.Id ?? row.r.DebtorReferenceId,
                PatientName = row.pat?.FullName ?? (!string.IsNullOrWhiteSpace(row.debtorResolvedName) ? row.debtorResolvedName : "Pasien Umum"),
                MedicalRecordNumber = row.pat?.MedicalRecordNumber ?? "-",
                EncounterNumber = row.encounterNumber,
                DebtorType = row.r.DebtorType,
                DebtorReferenceId = row.r.DebtorReferenceId,
                DebtorName = row.debtorResolvedName ?? "-",
                BenefitOwnerId = row.r.BenefitOwnerId,
                BenefitOwnerWorkforceProfileId = row.r.BenefitOwnerWorkforceProfileId,
                BenefitOwnerName = row.emp?.FullName ?? row.doc?.FullName,
                BenefitRelationship = row.r.BenefitRelationship,
                TotalPiutang = row.r.OriginalAmount,
                BeginningBalance = begBal,
                EndingBalance = endBal,
                OutstandingAmount = row.r.OutstandingAmount,
                AllocatedAmount = row.r.AllocatedAmount,
                AdjustedAmount = row.r.AdjustedAmount,
                WrittenOffAmount = row.r.WrittenOffAmount,
                DueDate = row.r.DueDate,
                InvoiceDate = row.inv?.InvoiceDate ?? row.r.RecognizedAt,
                Status = row.r.Status,
                LastMovementDate = lastMovDate
            });
        }

        // Hitung Summary global sesuai filter
        var summary = await GetSettlementSummaryInternalAsync(query, category, totalData, cancellationToken);

        return new FinanceSettlementPagedResponse
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = totalPage,
            Items = items,
            Summary = summary
        };
    }

    /// <summary>
    /// Mengambil ringkasan global Settlement AR (3 summary cards) untuk seluruh data yang cocok dengan filter.
    /// </summary>
    public async Task<FinanceSettlementSummaryResponse> GetSettlementSummaryAsync(
        FinanceSettlementQuery query, CancellationToken cancellationToken)
    {
        var category = NormalizeCategory(query.Category);
        var queryable = BuildFilteredQueryable(query, category);
        var totalCount = await queryable.CountAsync(cancellationToken);

        return await GetSettlementSummaryInternalAsync(query, category, totalCount, cancellationToken);
    }

    /// <summary>
    /// Mengambil detail satu Settlement AR untuk header tampilan Detail Settlement AR.
    /// </summary>
    public async Task<FinanceSettlementDetailResponse> GetSettlementDetailAsync(
        Guid receivableId, DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
    {
        var row = await (from r in _dbContext.FinReceivables.AsNoTracking().Where(x => x.Id == receivableId && !x.IsDelete)
                         join inv in _dbContext.BilInvoices.AsNoTracking() on r.InvoiceId equals inv.Id into invJoin
                         from inv in invJoin.DefaultIfEmpty()
                         join enc in _dbContext.RegPatientEncounters.AsNoTracking() on inv.EncounterId equals enc.Id into encJoin
                         from enc in encJoin.DefaultIfEmpty()
                         join pat in _dbContext.MstPatients.AsNoTracking() on enc.PatientId equals pat.Id into patJoin
                         from pat in patJoin.DefaultIfEmpty()
                         join emp in _dbContext.MstEmployees.AsNoTracking() on r.BenefitOwnerId equals emp.Id into empJoin
                         from emp in empJoin.DefaultIfEmpty()
                         join doc in _dbContext.MstDoctors.AsNoTracking() on r.BenefitOwnerWorkforceProfileId equals doc.WorkforceProfileId into docJoin
                         from doc in docJoin.DefaultIfEmpty()
                         join comp in _dbContext.MstCompanyGuarantors.AsNoTracking() on r.DebtorReferenceId equals comp.Id into compJoin
                         from comp in compJoin.DefaultIfEmpty()
                         join ins in _dbContext.MstInsuranceProviders.AsNoTracking() on r.DebtorReferenceId equals ins.Id into insJoin
                         from ins in insJoin.DefaultIfEmpty()
                         select new
                         {
                             r,
                             inv,
                             encounterNumber = enc != null ? enc.EncounterNumber : null,
                             pat,
                             emp,
                             doc,
                             debtorResolvedName = comp != null ? comp.CompanyGuarantorName
                                                : ins != null ? ins.InsuranceProviderName
                                                : emp != null ? emp.FullName
                                                : doc != null ? doc.FullName
                                                : pat != null ? pat.FullName : null
                         }).FirstOrDefaultAsync(cancellationToken);

        if (row == null)
            throw new KeyNotFoundException($"Piutang dengan ID '{receivableId}' tidak ditemukan.");

        var movements = await _dbContext.FinReceivableMovements.AsNoTracking()
            .Where(m => m.ReceivableId == receivableId && !m.IsDelete)
            .OrderBy(m => m.BusinessDate)
            .ThenBy(m => m.OccurredAt)
            .ThenBy(m => m.CreateDateTime)
            .ToListAsync(cancellationToken);

        var (begBal, endBal) = CalculateBalances(row.r, movements, startDate, endDate);

        return new FinanceSettlementDetailResponse
        {
            ReceivableId = row.r.Id,
            ReceivableNumber = row.r.ReceivableNumber,
            InvoiceId = row.r.InvoiceId,
            InvoiceNumber = row.inv?.InvoiceNumber ?? row.r.ReceivableNumber,
            PatientId = row.pat?.Id ?? row.r.DebtorReferenceId,
            PatientName = row.pat?.FullName ?? row.debtorResolvedName ?? "Pasien",
            MedicalRecordNumber = row.pat?.MedicalRecordNumber ?? "-",
            EncounterNumber = row.encounterNumber,
            DebtorType = row.r.DebtorType,
            DebtorReferenceId = row.r.DebtorReferenceId,
            DebtorName = row.debtorResolvedName ?? "-",
            BenefitOwnerId = row.r.BenefitOwnerId,
            BenefitOwnerWorkforceProfileId = row.r.BenefitOwnerWorkforceProfileId,
            BenefitOwnerName = row.emp?.FullName ?? row.doc?.FullName,
            BenefitRelationship = row.r.BenefitRelationship,
            InvoiceDate = row.inv?.InvoiceDate ?? row.r.RecognizedAt,
            DueDate = row.r.DueDate,
            Status = row.r.Status,
            OriginalAmount = row.r.OriginalAmount,
            BeginningBalance = begBal,
            EndingBalance = endBal,
            OutstandingAmount = row.r.OutstandingAmount,
            AllocatedAmount = row.r.AllocatedAmount,
            AdjustedAmount = row.r.AdjustedAmount,
            WrittenOffAmount = row.r.WrittenOffAmount,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };
    }

    /// <summary>
    /// Ekspor seluruh hasil filter Settlement AR ke format Excel (.xlsx).
    /// </summary>
    public async Task<byte[]> ExportExcelAsync(FinanceSettlementQuery query, CancellationToken cancellationToken)
    {
        var category = NormalizeCategory(query.Category);
        var queryable = BuildFilteredQueryable(query, category);
        queryable = ApplySorting(queryable, query.SortBy, query.SortDirection);

        var allRows = await queryable.ToListAsync(cancellationToken);
        var allIds = allRows.Select(x => x.r.Id).Distinct().ToList();

        var movements = allIds.Count > 0
            ? await _dbContext.FinReceivableMovements.AsNoTracking()
                .Where(m => allIds.Contains(m.ReceivableId) && !m.IsDelete)
                .OrderBy(m => m.BusinessDate)
                .ThenBy(m => m.OccurredAt)
                .ThenBy(m => m.CreateDateTime)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableMovement>();

        var movementLookup = movements.GroupBy(m => m.ReceivableId).ToDictionary(g => g.Key, g => g.ToList());

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Settlement AR");

        // Judul Laporan
        ws.Cell(1, 1).Value = "SETTLEMENT AR / PELUNASAN AR";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#0d7f89");

        var periodLabel = FormatPeriodLabel(query.StartDate, query.EndDate);
        var categoryLabel = FormatCategoryLabel(category);
        ws.Cell(2, 1).Value = $"Kategori: {categoryLabel}  |  Periode: {periodLabel}";
        ws.Cell(2, 1).Style.Font.FontSize = 10;
        ws.Cell(2, 1).Style.Font.FontColor = XLColor.FromHtml("#4b5563");

        var printTimeWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, FinanceBusinessDate.BusinessTimeZone);
        ws.Cell(3, 1).Value = $"Dicetak pada: {printTimeWib:dd/MM/yyyy HH:mm} WIB  |  Total Data: {allRows.Count} Baris";
        ws.Cell(3, 1).Style.Font.FontSize = 9;
        ws.Cell(3, 1).Style.Font.Italic = true;
        ws.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml("#6b7280");

        // Table Header di baris 5
        var headerRow = 5;
        string[] headers = [
            "NO",
            "NAMA PASIEN",
            "NO. RM",
            "NO. INVOICE",
            "PENJAMIN / DEBITUR",
            "TOTAL BEGINNING BALANCE",
            "TOTAL ENDING BALANCE",
            "TOTAL PIUTANG"
        ];

        for (var c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(headerRow, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 10;
            cell.Style.Font.FontColor = XLColor.FromHtml("#0d7f89");
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#dff8fb");
            cell.Style.Alignment.Horizontal = c == 0 ? XLAlignmentHorizontalValues.Center
                : (c >= 5 ? XLAlignmentHorizontalValues.Right : XLAlignmentHorizontalValues.Left);
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.FromHtml("#8adce4");
        }

        var r = headerRow + 1;
        var globalIndex = 1;

        decimal sumBeginning = 0m;
        decimal sumEnding = 0m;
        decimal sumPiutang = 0m;

        foreach (var row in allRows)
        {
            movementLookup.TryGetValue(row.r.Id, out var recMovements);
            recMovements ??= new List<FinReceivableMovement>();

            var (begBal, endBal) = CalculateBalances(row.r, recMovements, query.StartDate, query.EndDate);

            sumBeginning += begBal;
            sumEnding += endBal;
            sumPiutang += row.r.OriginalAmount;

            var invNumber = !string.IsNullOrWhiteSpace(row.inv?.InvoiceNumber)
                ? row.inv.InvoiceNumber
                : (!string.IsNullOrWhiteSpace(row.batchNumber) ? row.batchNumber : row.r.ReceivableNumber);

            var patName = row.pat?.FullName ?? row.debtorResolvedName ?? "Pasien Umum";
            var mrn = row.pat?.MedicalRecordNumber ?? "-";
            var debtorName = row.debtorResolvedName ?? "-";

            ws.Cell(r, 1).Value = globalIndex++;
            ws.Cell(r, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Cell(r, 2).Value = patName;
            ws.Cell(r, 3).Value = mrn;
            ws.Cell(r, 4).Value = invNumber;
            ws.Cell(r, 5).Value = debtorName;

            ws.Cell(r, 6).Value = begBal;
            ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Cell(r, 7).Value = endBal;
            ws.Cell(r, 7).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            if (endBal > 0)
            {
                ws.Cell(r, 7).Style.Font.FontColor = XLColor.FromHtml("#dc2626"); // Merah
            }
            else
            {
                ws.Cell(r, 7).Style.Font.FontColor = XLColor.FromHtml("#16a34a"); // Hijau
            }

            ws.Cell(r, 8).Value = row.r.OriginalAmount;
            ws.Cell(r, 8).Style.NumberFormat.Format = "#,##0";
            ws.Cell(r, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            for (var c = 1; c <= 8; c++)
            {
                ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(r, c).Style.Border.OutsideBorderColor = XLColor.FromHtml("#e5e7eb");
            }
            r++;
        }

        // Baris Grand Total di akhir tabel
        var totalLabelRange = ws.Range(r, 1, r, 5);
        totalLabelRange.Merge();
        totalLabelRange.Value = "GRAND TOTAL :";
        totalLabelRange.Style.Font.Bold = true;
        totalLabelRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        totalLabelRange.Style.Font.FontColor = XLColor.FromHtml("#0d7f89");
        totalLabelRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#dff8fb");

        ws.Cell(r, 6).Value = sumBeginning;
        ws.Cell(r, 6).Style.Font.Bold = true;
        ws.Cell(r, 6).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Cell(r, 6).Style.Font.FontColor = XLColor.FromHtml("#0d7f89");
        ws.Cell(r, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#dff8fb");

        ws.Cell(r, 7).Value = sumEnding;
        ws.Cell(r, 7).Style.Font.Bold = true;
        ws.Cell(r, 7).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Cell(r, 7).Style.Font.FontColor = sumEnding > 0 ? XLColor.FromHtml("#dc2626") : XLColor.FromHtml("#16a34a");
        ws.Cell(r, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#dff8fb");

        ws.Cell(r, 8).Value = sumPiutang;
        ws.Cell(r, 8).Style.Font.Bold = true;
        ws.Cell(r, 8).Style.NumberFormat.Format = "#,##0";
        ws.Cell(r, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        ws.Cell(r, 8).Style.Font.FontColor = XLColor.FromHtml("#0d7f89");
        ws.Cell(r, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#dff8fb");

        for (var c = 1; c <= 8; c++)
        {
            ws.Cell(r, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Cell(r, c).Style.Border.OutsideBorderColor = XLColor.FromHtml("#8adce4");
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Ekspor seluruh hasil filter Settlement AR ke format PDF dokumen resmi.
    /// </summary>
    public async Task<byte[]> ExportPdfAsync(FinanceSettlementQuery query, CancellationToken cancellationToken)
    {
        var category = NormalizeCategory(query.Category);
        var queryable = BuildFilteredQueryable(query, category);
        queryable = ApplySorting(queryable, query.SortBy, query.SortDirection);

        var allRows = await queryable.ToListAsync(cancellationToken);
        var allIds = allRows.Select(x => x.r.Id).Distinct().ToList();

        var movements = allIds.Count > 0
            ? await _dbContext.FinReceivableMovements.AsNoTracking()
                .Where(m => allIds.Contains(m.ReceivableId) && !m.IsDelete)
                .OrderBy(m => m.BusinessDate)
                .ThenBy(m => m.OccurredAt)
                .ThenBy(m => m.CreateDateTime)
                .ToListAsync(cancellationToken)
            : new List<FinReceivableMovement>();

        var movementLookup = movements.GroupBy(m => m.ReceivableId).ToDictionary(g => g.Key, g => g.ToList());

        var periodLabel = FormatPeriodLabel(query.StartDate, query.EndDate);
        var categoryLabel = FormatCategoryLabel(category);
        var printTimeWib = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, FinanceBusinessDate.BusinessTimeZone);

        decimal sumBeginning = 0m;
        decimal sumEnding = 0m;
        decimal sumPiutang = 0m;

        var items = new List<FinanceSettlementRowResponse>(allRows.Count);
        foreach (var row in allRows)
        {
            movementLookup.TryGetValue(row.r.Id, out var recMovements);
            recMovements ??= new List<FinReceivableMovement>();

            var (begBal, endBal) = CalculateBalances(row.r, recMovements, query.StartDate, query.EndDate);

            sumBeginning += begBal;
            sumEnding += endBal;
            sumPiutang += row.r.OriginalAmount;

            var invNumber = !string.IsNullOrWhiteSpace(row.inv?.InvoiceNumber)
                ? row.inv.InvoiceNumber
                : (!string.IsNullOrWhiteSpace(row.batchNumber) ? row.batchNumber : row.r.ReceivableNumber);

            items.Add(new FinanceSettlementRowResponse
            {
                ReceivableId = row.r.Id,
                ReceivableNumber = row.r.ReceivableNumber,
                InvoiceNumber = invNumber,
                PatientName = row.pat?.FullName ?? row.debtorResolvedName ?? "Pasien Umum",
                MedicalRecordNumber = row.pat?.MedicalRecordNumber ?? "-",
                DebtorName = row.debtorResolvedName ?? "-",
                BeginningBalance = begBal,
                EndingBalance = endBal,
                TotalPiutang = row.r.OriginalAmount
            });
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                // Header
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("QUILVIAN HOSPITAL SYSTEM").Bold().FontSize(14).FontColor("#0d7f89");
                            c.Item().Text("LAPORAN SETTLEMENT AR (PELUNASAN & MUTASI PIUTANG)").Bold().FontSize(12);
                            c.Item().Text($"Kategori: {categoryLabel}   |   Periode: {periodLabel}").FontSize(9).FontColor("#4b5563");
                        });

                        row.ConstantItem(200).AlignRight().Column(c =>
                        {
                            c.Item().Text($"Dicetak: {printTimeWib:dd/MM/yyyy HH:mm} WIB").FontSize(8).FontColor("#6b7280");
                            c.Item().Text($"Total: {items.Count} Data Piutang").FontSize(8).FontColor("#6b7280");
                        });
                    });

                    col.Item().PaddingVertical(6).LineHorizontal(1).LineColor("#0d7f89");

                    // Summary Strip
                    col.Item().PaddingBottom(8).Row(summaryRow =>
                    {
                        summaryRow.RelativeItem().Border(1).BorderColor("#8adce4").Background("#dff8fb").Padding(6).Column(c =>
                        {
                            c.Item().Text("Grandtotal Piutang").FontSize(8).FontColor("#0d7f89").Bold();
                            c.Item().Text(sumPiutang.ToString("C0", IdCulture)).FontSize(11).Bold();
                        });
                        summaryRow.ConstantItem(10);
                        summaryRow.RelativeItem().Border(1).BorderColor("#8adce4").Background("#dff8fb").Padding(6).Column(c =>
                        {
                            c.Item().Text("Grandtotal Beginning Balance").FontSize(8).FontColor("#0d7f89").Bold();
                            c.Item().Text(sumBeginning.ToString("C0", IdCulture)).FontSize(11).Bold();
                        });
                        summaryRow.ConstantItem(10);
                        summaryRow.RelativeItem().Border(1).BorderColor("#8adce4").Background("#dff8fb").Padding(6).Column(c =>
                        {
                            c.Item().Text("Grandtotal Ending Balance").FontSize(8).FontColor("#0d7f89").Bold();
                            c.Item().Text(sumEnding.ToString("C0", IdCulture)).FontSize(11).Bold().FontColor(sumEnding > 0 ? "#dc2626" : "#16a34a");
                        });
                    });
                });

                // Content Table
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(30);  // No
                        columns.RelativeColumn(3);   // Nama Pasien & RM
                        columns.RelativeColumn(2);   // No Invoice
                        columns.RelativeColumn(2.5f); // Penjamin / Debitur
                        columns.RelativeColumn(2);   // Beginning Balance
                        columns.RelativeColumn(2);   // Ending Balance
                        columns.RelativeColumn(2);   // Total Piutang
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background("#0d7f89").Padding(5).AlignCenter().Text("NO").Bold().FontColor("#ffffff").FontSize(8);
                        h.Cell().Background("#0d7f89").Padding(5).Text("NAMA PASIEN / NO. RM").Bold().FontColor("#ffffff").FontSize(8);
                        h.Cell().Background("#0d7f89").Padding(5).Text("NO. INVOICE").Bold().FontColor("#ffffff").FontSize(8);
                        h.Cell().Background("#0d7f89").Padding(5).Text("PENJAMIN / DEBITUR").Bold().FontColor("#ffffff").FontSize(8);
                        h.Cell().Background("#0d7f89").Padding(5).AlignRight().Text("BEGINNING BALANCE").Bold().FontColor("#ffffff").FontSize(8);
                        h.Cell().Background("#0d7f89").Padding(5).AlignRight().Text("ENDING BALANCE").Bold().FontColor("#ffffff").FontSize(8);
                        h.Cell().Background("#0d7f89").Padding(5).AlignRight().Text("TOTAL PIUTANG").Bold().FontColor("#ffffff").FontSize(8);
                    });

                    var idx = 1;
                    foreach (var item in items)
                    {
                        var bgColor = idx % 2 == 0 ? "#f8fafc" : "#ffffff";
                        table.Cell().Background(bgColor).Padding(5).AlignCenter().Text(idx.ToString()).FontSize(8);
                        table.Cell().Background(bgColor).Padding(5).Column(c =>
                        {
                            c.Item().Text(item.PatientName).Bold().FontSize(8.5f);
                            c.Item().Text($"No. RM: {item.MedicalRecordNumber}").FontSize(7.5f).FontColor("#64748b");
                        });
                        table.Cell().Background(bgColor).Padding(5).Text(item.InvoiceNumber).FontColor("#0d7f89").Bold().FontSize(8);
                        table.Cell().Background(bgColor).Padding(5).Text(item.DebtorName).FontSize(8);
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text(item.BeginningBalance.ToString("C0", IdCulture)).FontSize(8);
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text(item.EndingBalance.ToString("C0", IdCulture)).FontSize(8).FontColor(item.EndingBalance > 0 ? "#dc2626" : "#16a34a").Bold();
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text(item.TotalPiutang.ToString("C0", IdCulture)).FontSize(8);
                        idx++;
                    }

                    // Total Row
                    table.Cell().ColumnSpan(4).Background("#e6f7f8").Padding(6).AlignRight().Text("GRAND TOTAL :").Bold().FontColor("#0d7f89").FontSize(9);
                    table.Cell().Background("#e6f7f8").Padding(6).AlignRight().Text(sumBeginning.ToString("C0", IdCulture)).Bold().FontColor("#0d7f89").FontSize(9);
                    table.Cell().Background("#e6f7f8").Padding(6).AlignRight().Text(sumEnding.ToString("C0", IdCulture)).Bold().FontColor(sumEnding > 0 ? "#dc2626" : "#16a34a").FontSize(9);
                    table.Cell().Background("#e6f7f8").Padding(6).AlignRight().Text(sumPiutang.ToString("C0", IdCulture)).Bold().FontColor("#0d7f89").FontSize(9);
                });

                // Footer
                page.Footer().Row(r =>
                {
                    r.RelativeItem().Text("Dokumen ini dihasilkan secara otomatis oleh modul Finance Settlement AR.").FontSize(7.5f).FontColor("#9ca3af");
                    r.ConstantItem(150).AlignRight().Text(x =>
                    {
                        x.Span("Halaman ");
                        x.CurrentPageNumber();
                        x.Span(" dari ");
                        x.TotalPages();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    // ============================================================================================
    // PRIVATE HELPER METHODS
    // ============================================================================================

    private static string NormalizeCategory(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return FinanceSettlementCategories.Company;
        var upper = raw.Trim().ToUpperInvariant();
        return upper switch
        {
            "PASIEN" or "PASIEN_UMUM" or "PATIENT" => FinanceSettlementCategories.Patient,
            "KARYAWAN" or "EMPLOYEE" => FinanceSettlementCategories.Employee,
            _ => FinanceSettlementCategories.Company
        };
    }

    private static string FormatCategoryLabel(string category) =>
        category switch
        {
            FinanceSettlementCategories.Patient => "Pasien Umum",
            FinanceSettlementCategories.Employee => "Karyawan",
            _ => "Perusahaan / Asuransi"
        };

    private static string FormatPeriodLabel(DateOnly? start, DateOnly? end)
    {
        if (start.HasValue && end.HasValue)
            return $"{start.Value:dd/MM/yyyy} s/d {end.Value:dd/MM/yyyy}";
        if (start.HasValue)
            return $"Mulai {start.Value:dd/MM/yyyy}";
        if (end.HasValue)
            return $"Sampai {end.Value:dd/MM/yyyy}";
        return "Semua Periode (Posisi Terkini)";
    }

    private static (decimal beginningBalance, decimal endingBalance) CalculateBalances(
        FinReceivable r,
        List<FinReceivableMovement> recMovements,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        if (!startDate.HasValue && !endDate.HasValue)
        {
            return (r.OriginalAmount, r.OutstandingAmount);
        }

        decimal beginningBalance;
        decimal endingBalance;

        // 1. Beginning Balance: saldo tepat sebelum awal periode (BusinessDate < StartDate)
        if (startDate.HasValue)
        {
            var priorMovements = recMovements.Where(m => m.BusinessDate < startDate.Value).ToList();
            if (priorMovements.Count > 0)
            {
                beginningBalance = priorMovements.Last().BalanceAfter;
            }
            else
            {
                var startDtUtc = ToUtcStartOfDay(startDate.Value);
                if (r.RecognizedAt < startDtUtc)
                {
                    var firstMovement = recMovements.FirstOrDefault();
                    beginningBalance = firstMovement != null ? firstMovement.BalanceBefore : r.OriginalAmount;
                }
                else
                {
                    beginningBalance = 0m;
                }
            }
        }
        else
        {
            beginningBalance = r.OriginalAmount;
        }

        // 2. Ending Balance: saldo setelah seluruh mutasi sampai akhir periode (BusinessDate <= EndDate)
        if (endDate.HasValue)
        {
            var effectiveMovements = recMovements.Where(m => m.BusinessDate <= endDate.Value).ToList();
            if (effectiveMovements.Count > 0)
            {
                endingBalance = effectiveMovements.Last().BalanceAfter;
            }
            else
            {
                var endDtUtc = ToUtcEndOfDay(endDate.Value);
                if (r.RecognizedAt <= endDtUtc)
                {
                    var firstMovement = recMovements.FirstOrDefault();
                    endingBalance = firstMovement != null ? firstMovement.BalanceBefore : r.OriginalAmount;
                }
                else
                {
                    endingBalance = 0m;
                }
            }
        }
        else
        {
            endingBalance = r.OutstandingAmount;
        }

        return (beginningBalance, endingBalance);
    }

    private static DateTime ToUtcStartOfDay(DateOnly date)
    {
        var localDateTime = date.ToDateTime(TimeOnly.MinValue);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, FinanceBusinessDate.BusinessTimeZone);
    }

    private static DateTime ToUtcEndOfDay(DateOnly date)
    {
        var localDateTime = date.ToDateTime(TimeOnly.MaxValue);
        return TimeZoneInfo.ConvertTimeToUtc(localDateTime, FinanceBusinessDate.BusinessTimeZone);
    }

    /// <summary>
    /// Baris gabungan internal untuk query Settlement AR.
    /// Menggunakan class dengan parameterless constructor dan properti getter/setter
    /// (bukan positional record constructor) agar EF Core Npgsql dapat menerjemahkan
    /// MemberInitExpression ke SQL PostgreSQL tanpa memicu System.InvalidOperationException
    /// saat OrderBy, Where, CountAsync, atau Select dijalankan setelah join.
    /// </summary>
    internal sealed class SettlementQueryRow
    {
        public FinReceivable r { get; set; } = null!;
        public BilInvoice? inv { get; set; }
        public string? encounterNumber { get; set; }
        public MstPatient? pat { get; set; }
        public MstEmployee? emp { get; set; }

        /// <summary>Dokter tetap pemilik manfaat (tanpa MstEmployee), dihubungkan lewat BenefitOwnerWorkforceProfileId.</summary>
        public MstDoctor? doc { get; set; }
        public MstCompanyGuarantor? comp { get; set; }
        public MstInsuranceProvider? ins { get; set; }
        public string? batchNumber { get; set; }

        /// <summary>
        /// Nama debitur teresolusi di memori setelah data termaterialisasi.
        /// </summary>
        public string? debtorResolvedName =>
            comp != null ? comp.CompanyGuarantorName
            : ins != null ? ins.InsuranceProviderName
            : emp != null ? emp.FullName
            : doc != null ? doc.FullName
            : pat != null ? pat.FullName : null;
    }

    private IQueryable<SettlementQueryRow> BuildFilteredQueryable(FinanceSettlementQuery query, string category)
    {
        IQueryable<FinReceivable> baseRecs = _dbContext.FinReceivables.AsNoTracking().Where(r => !r.IsDelete);

        // Filter berdasarkan kategori debitur
        if (category == FinanceSettlementCategories.Company)
        {
            baseRecs = baseRecs.Where(r => r.DebtorType == FinReceivableDebtorTypes.Payer || r.DebtorType == "COMPANY");
            if (query.PayerId.HasValue && query.PayerId.Value != Guid.Empty)
            {
                baseRecs = baseRecs.Where(r => r.DebtorReferenceId == query.PayerId.Value);
            }
        }
        else if (category == FinanceSettlementCategories.Patient)
        {
            baseRecs = baseRecs.Where(r => r.DebtorType == FinReceivableDebtorTypes.PatientGuarantor || r.DebtorType == "PATIENT");
        }
        else if (category == FinanceSettlementCategories.Employee)
        {
            baseRecs = baseRecs.Where(r => r.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit || r.DebtorType == "EMPLOYEE");
            if (query.EmployeeId.HasValue && query.EmployeeId.Value != Guid.Empty)
            {
                baseRecs = baseRecs.Where(r => r.BenefitOwnerId == query.EmployeeId.Value || r.DebtorReferenceId == query.EmployeeId.Value);
            }
        }

        // Filter periode tanggal jika EndDate ada: piutang harus sudah ada sebelum/pada EndDate
        if (query.EndDate.HasValue)
        {
            var endUtc = ToUtcEndOfDay(query.EndDate.Value);
            baseRecs = baseRecs.Where(r => r.RecognizedAt <= endUtc || r.CreateDateTime <= endUtc);
        }

        // Subquery active batch number (hanya batch yang tidak dibatalkan dan item membership aktif, satu nomor batch per piutang)
        var batchSubquery = from b in _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
                            join batch in _dbContext.FinReceivableInvoiceBatches.AsNoTracking() on b.BatchId equals batch.Id
                            where !b.IsDelete && b.IsActiveMembership && !batch.IsDelete && batch.Status != FinReceivableInvoiceBatchStatuses.Cancelled
                            group batch.BatchNumber by b.ReceivableId into g
                            select new { ReceivableId = g.Key, BatchNumber = g.Max() };

        var queryable = from r in baseRecs
                        join inv in _dbContext.BilInvoices.AsNoTracking() on r.InvoiceId equals inv.Id into invJoin
                        from inv in invJoin.DefaultIfEmpty()
                        join enc in _dbContext.RegPatientEncounters.AsNoTracking() on inv.EncounterId equals enc.Id into encJoin
                        from enc in encJoin.DefaultIfEmpty()
                        join pat in _dbContext.MstPatients.AsNoTracking() on enc.PatientId equals pat.Id into patJoin
                        from pat in patJoin.DefaultIfEmpty()
                        join emp in _dbContext.MstEmployees.AsNoTracking() on r.BenefitOwnerId equals emp.Id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        join doc in _dbContext.MstDoctors.AsNoTracking() on r.BenefitOwnerWorkforceProfileId equals doc.WorkforceProfileId into docJoin
                        from doc in docJoin.DefaultIfEmpty()
                        join comp in _dbContext.MstCompanyGuarantors.AsNoTracking() on r.DebtorReferenceId equals comp.Id into compJoin
                        from comp in compJoin.DefaultIfEmpty()
                        join ins in _dbContext.MstInsuranceProviders.AsNoTracking() on r.DebtorReferenceId equals ins.Id into insJoin
                        from ins in insJoin.DefaultIfEmpty()
                        join bItem in batchSubquery on r.Id equals bItem.ReceivableId into bJoin
                        from bItem in bJoin.DefaultIfEmpty()
                        select new SettlementQueryRow
                        {
                            r = r,
                            inv = inv,
                            encounterNumber = enc != null ? enc.EncounterNumber : null,
                            pat = pat,
                            emp = emp,
                            doc = doc,
                            comp = comp,
                            ins = ins,
                            batchNumber = bItem != null ? bItem.BatchNumber : null
                        };

        // Filter spesifik PatientId jika kategori Pasien Umum
        if (category == FinanceSettlementCategories.Patient && query.PatientId.HasValue && query.PatientId.Value != Guid.Empty)
        {
            queryable = queryable.Where(x => (x.pat != null && x.pat.Id == query.PatientId.Value) || x.r.DebtorReferenceId == query.PatientId.Value);
        }

        // Filter No Invoice
        if (!string.IsNullOrWhiteSpace(query.InvoiceNumber))
        {
            var invNum = query.InvoiceNumber.Trim().ToUpper();
            queryable = queryable.Where(x =>
                (x.inv != null && x.inv.InvoiceNumber.ToUpper().Contains(invNum)) ||
                (x.batchNumber != null && x.batchNumber.ToUpper().Contains(invNum)) ||
                x.r.ReceivableNumber.ToUpper().Contains(invNum));
        }

        // Filter Search teks bebas
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToUpper();
            queryable = queryable.Where(x =>
                x.r.ReceivableNumber.ToUpper().Contains(search) ||
                (x.inv != null && x.inv.InvoiceNumber.ToUpper().Contains(search)) ||
                (x.batchNumber != null && x.batchNumber.ToUpper().Contains(search)) ||
                (x.pat != null && x.pat.FullName.ToUpper().Contains(search)) ||
                (x.pat != null && x.pat.MedicalRecordNumber.ToUpper().Contains(search)) ||
                (x.encounterNumber != null && x.encounterNumber.ToUpper().Contains(search)) ||
                (x.comp != null && x.comp.CompanyGuarantorName.ToUpper().Contains(search)) ||
                (x.ins != null && x.ins.InsuranceProviderName.ToUpper().Contains(search)) ||
                (x.emp != null && x.emp.FullName.ToUpper().Contains(search)) ||
                (x.doc != null && x.doc.FullName.ToUpper().Contains(search)) ||
                (x.doc != null && x.doc.DoctorNumber.ToUpper().Contains(search)));
        }

        return queryable;
    }

    private static IQueryable<SettlementQueryRow> ApplySorting(
        IQueryable<SettlementQueryRow> query, string sortBy, string sortDirection)
    {
        var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var normSort = string.IsNullOrWhiteSpace(sortBy) ? "patientname" : sortBy.Trim().ToLowerInvariant();

        return normSort switch
        {
            "invoicenumber" => desc
                ? query.OrderByDescending(x => x.inv != null ? x.inv.InvoiceNumber : (x.batchNumber ?? x.r.ReceivableNumber)).ThenBy(x => x.r.Id)
                : query.OrderBy(x => x.inv != null ? x.inv.InvoiceNumber : (x.batchNumber ?? x.r.ReceivableNumber)).ThenBy(x => x.r.Id),

            "duedate" => desc
                ? query.OrderByDescending(x => x.r.DueDate).ThenBy(x => x.r.Id)
                : query.OrderBy(x => x.r.DueDate).ThenBy(x => x.r.Id),

            "outstandingamount" or "endingbalance" => desc
                ? query.OrderByDescending(x => x.r.OutstandingAmount).ThenBy(x => x.r.Id)
                : query.OrderBy(x => x.r.OutstandingAmount).ThenBy(x => x.r.Id),

            "totalpiutang" or "beginningbalance" => desc
                ? query.OrderByDescending(x => x.r.OriginalAmount).ThenBy(x => x.r.Id)
                : query.OrderBy(x => x.r.OriginalAmount).ThenBy(x => x.r.Id),

            // default: patientName ASC
            _ => desc
                ? query.OrderByDescending(x => x.pat != null ? x.pat.FullName
                    : x.comp != null ? x.comp.CompanyGuarantorName
                    : x.ins != null ? x.ins.InsuranceProviderName
                    : x.emp != null ? x.emp.FullName
                    : x.doc != null ? x.doc.FullName : null).ThenBy(x => x.r.Id)
                : query.OrderBy(x => x.pat != null ? x.pat.FullName
                    : x.comp != null ? x.comp.CompanyGuarantorName
                    : x.ins != null ? x.ins.InsuranceProviderName
                    : x.emp != null ? x.emp.FullName
                    : x.doc != null ? x.doc.FullName : null).ThenBy(x => x.r.Id)
        };
    }

    private async Task<FinanceSettlementSummaryResponse> GetSettlementSummaryInternalAsync(
        FinanceSettlementQuery query, string category, int totalCount, CancellationToken cancellationToken)
    {
        var periodLabel = FormatPeriodLabel(query.StartDate, query.EndDate);

        if (totalCount == 0)
        {
            return new FinanceSettlementSummaryResponse
            {
                Category = category,
                TotalReceivables = 0,
                GrandTotalPiutang = 0m,
                GrandTotalBeginningBalance = 0m,
                GrandTotalEndingBalance = 0m,
                GrandTotalOutstanding = 0m,
                PeriodStart = query.StartDate,
                PeriodEnd = query.EndDate,
                BalanceAsOfDate = query.EndDate ?? FinanceBusinessDate.Today(),
                PeriodLabel = periodLabel
            };
        }

        var queryable = BuildFilteredQueryable(query, category);

        // Agregasi dasar
        var baseAggregates = await queryable.Select(x => new
        {
            x.r.Id,
            x.r.OriginalAmount,
            x.r.OutstandingAmount,
            x.r.RecognizedAt
        }).ToListAsync(cancellationToken);

        var grandTotalPiutang = baseAggregates.Sum(x => x.OriginalAmount);
        var grandTotalOutstanding = baseAggregates.Sum(x => x.OutstandingAmount);

        decimal grandTotalBeginning = 0m;
        decimal grandTotalEnding = 0m;

        if (!query.StartDate.HasValue && !query.EndDate.HasValue)
        {
            grandTotalBeginning = grandTotalPiutang;
            grandTotalEnding = grandTotalOutstanding;
        }
        else
        {
            // Ambil semua movements untuk seluruh ID data tersaring
            var allIds = baseAggregates.Select(x => x.Id).Distinct().ToList();
            var allMovements = await _dbContext.FinReceivableMovements.AsNoTracking()
                .Where(m => allIds.Contains(m.ReceivableId) && !m.IsDelete)
                .OrderBy(m => m.BusinessDate)
                .ThenBy(m => m.OccurredAt)
                .ThenBy(m => m.CreateDateTime)
                .ToListAsync(cancellationToken);

            var movementLookup = allMovements.GroupBy(m => m.ReceivableId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in baseAggregates)
            {
                movementLookup.TryGetValue(item.Id, out var recMovements);
                recMovements ??= new List<FinReceivableMovement>();

                // Dummy model untuk hitung balance
                var dummyRec = new FinReceivable
                {
                    Id = item.Id,
                    OriginalAmount = item.OriginalAmount,
                    OutstandingAmount = item.OutstandingAmount,
                    RecognizedAt = item.RecognizedAt
                };

                var (beg, end) = CalculateBalances(dummyRec, recMovements, query.StartDate, query.EndDate);
                grandTotalBeginning += beg;
                grandTotalEnding += end;
            }
        }

        return new FinanceSettlementSummaryResponse
        {
            Category = category,
            TotalReceivables = totalCount,
            GrandTotalPiutang = grandTotalPiutang,
            GrandTotalBeginningBalance = grandTotalBeginning,
            GrandTotalEndingBalance = grandTotalEnding,
            GrandTotalOutstanding = grandTotalOutstanding,
            PeriodStart = query.StartDate,
            PeriodEnd = query.EndDate,
            BalanceAsOfDate = query.EndDate ?? FinanceBusinessDate.Today(),
            PeriodLabel = periodLabel
        };
    }
}
