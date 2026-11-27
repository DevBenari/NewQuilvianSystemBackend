using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Services;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.GeneralLedger.Controllers
{
    /// <summary>
    /// Buku besar, neraca saldo, dan saldo per akun. Seluruhnya baca saja.
    /// </summary>
    /// <remarks>
    /// Ketiga endpoint hanya menghitung jurnal berstatus disahkan. Laporan tidak pernah mencampur
    /// yang sudah dan belum disahkan.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/accounting/general-ledger")]
    [AccessController(
        moduleCode: "ACCOUNTING_GENERAL_LEDGER",
        moduleName: "Accounting General Ledger",
        displayName: "General Ledger",
        AreaName = "Corporate",
        ControllerName = "GeneralLedger",
        Description = "Corporate accounting general ledger and trial balance",
        SortOrder = 1)]
    [Tags("Corporate / Accounting / General Ledger")]
    public class GeneralLedgerController : ControllerBase
    {
        private const string LogCategory = "Corporate.Accounting.GeneralLedger";

        private readonly AccGeneralLedgerService _service;
        private readonly LoggerService _loggerService;

        public GeneralLedgerController(
            AccGeneralLedgerService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("movements")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat mutasi buku besar", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetMovements([FromQuery] LedgerMovementQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetMovementsAsync(query, ct));

        [HttpGet("movements/summary")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat ringkasan saldo buku besar", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetMovementSummary([FromQuery] LedgerMovementQuery query, CancellationToken ct)
            => ToActionResult(await _service.GetMovementSummaryAsync(query, ct));

        [HttpGet("movements/export")]
        [AccessAction("Read", "Read General Ledger", Description = "Mengunduh buku besar sebagai CSV", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> ExportMovements([FromQuery] LedgerMovementQuery query, CancellationToken ct)
        {
            var hasil = await _service.BuildMovementCsvAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "BukuBesar",
                query.LegalEntityId,
                query.AccountId,
                query.CostCenterId,
                query.DateFrom,
                query.DateTo,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.RowCount
            };

            if (!hasil.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);
                return ToActionResult(hasil);
            }

            await _loggerService.InfoAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);

            return File(hasil.Data!.Content, AccLedgerCsv.ContentType, hasil.Data.FileName);
        }

        /// <remarks>
        /// <b>Satu-satunya endpoint grup ini yang dicatat logger</b>, sesuai `ACC-PERMISSION-0.3`
        /// bagian 3. Neraca saldo adalah laporan yang dipakai menutup buku, sehingga siapa
        /// membacanya dan kapan menjadi pertanyaan audit. Dua endpoint lain adalah penelusuran
        /// sehari-hari; mencatat semuanya menghasilkan ratusan baris yang justru menyulitkan
        /// penelusuran saat benar-benar dibutuhkan.
        ///
        /// Muatan log sengaja tidak memuat satu pun angka — `ACC-PERMISSION-0.3` bagian 4
        /// melarang nilai uang masuk payload log.
        /// </remarks>
        [HttpGet("trial-balance")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat neraca saldo", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetTrialBalance([FromQuery] TrialBalanceQuery query, CancellationToken ct)
        {
            var hasil = await _service.GetTrialBalanceAsync(query, ct);

            var muatan = new
            {
                query.LegalEntityId,
                query.PeriodCode,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.Rows.Count
            };

            if (hasil.Success)
            {
                await _loggerService.InfoAsync(
                    LogCategory, "GeneralLedger.TrialBalance", hasil.Message, muatan);
            }
            else
            {
                await _loggerService.WarningAsync(
                    LogCategory, "GeneralLedger.TrialBalance", hasil.Message, muatan);
            }

            return ToActionResult(hasil);
        }

        [HttpGet("trial-balance/export")]
        [AccessAction("Read", "Read General Ledger", Description = "Mengunduh neraca saldo sebagai CSV", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> ExportTrialBalance([FromQuery] TrialBalanceQuery query, CancellationToken ct)
        {
            var hasil = await _service.BuildTrialBalanceCsvAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "NeracaSaldo",
                query.LegalEntityId,
                query.PeriodCode,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.RowCount
            };

            if (!hasil.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);
                return ToActionResult(hasil);
            }

            await _loggerService.InfoAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);

            return File(hasil.Data!.Content, AccLedgerCsv.ContentType, hasil.Data.FileName);
        }

        [HttpGet("income-statement")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat laba rugi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetIncomeStatement([FromQuery] IncomeStatementQuery query, CancellationToken ct)
        {
            var hasil = await _service.GetIncomeStatementAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "LabaRugi",
                query.LegalEntityId,
                query.PeriodCode,
                query.CostCenterId,
                query.IncludeZeroAccounts,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.Sections.Sum(x => x.Rows.Count)
            };

            if (hasil.Success)
            {
                await _loggerService.InfoAsync(
                    LogCategory, "FinancialStatement.IncomeStatement", hasil.Message, muatan);
            }
            else
            {
                await _loggerService.WarningAsync(
                    LogCategory, "FinancialStatement.IncomeStatement", hasil.Message, muatan);
            }

            return ToActionResult(hasil);
        }

        [HttpGet("income-statement/export")]
        [AccessAction("Read", "Read General Ledger", Description = "Mengunduh laba rugi sebagai CSV", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> ExportIncomeStatement([FromQuery] IncomeStatementQuery query, CancellationToken ct)
        {
            var hasil = await _service.BuildIncomeStatementCsvAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "LabaRugi",
                query.LegalEntityId,
                query.PeriodCode,
                query.CostCenterId,
                query.IncludeZeroAccounts,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.RowCount
            };

            if (!hasil.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);
                return ToActionResult(hasil);
            }

            await _loggerService.InfoAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);

            return File(hasil.Data!.Content, AccLedgerCsv.ContentType, hasil.Data.FileName);
        }

        [HttpGet("balance-sheet")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat neraca", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetBalanceSheet([FromQuery] BalanceSheetQuery query, CancellationToken ct)
        {
            var hasil = await _service.GetBalanceSheetAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "Neraca",
                query.LegalEntityId,
                query.PeriodCode,
                query.IncludeZeroAccounts,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.Sections.Sum(x => x.Rows.Count),
                UnclassifiedAccountCount = hasil.Data?.UnclassifiedAccountCount
            };

            if (hasil.Success)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "FinancialStatement.BalanceSheet",
                    $"{hasil.Message} Akun bersaldo dengan klasifikasi belum lengkap: {hasil.Data?.UnclassifiedAccountCount ?? 0}.",
                    muatan);
            }
            else
            {
                await _loggerService.WarningAsync(
                    LogCategory, "FinancialStatement.BalanceSheet", hasil.Message, muatan);
            }

            return ToActionResult(hasil);
        }

        [HttpGet("balance-sheet/export")]
        [AccessAction("Read", "Read General Ledger", Description = "Mengunduh neraca sebagai CSV", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> ExportBalanceSheet([FromQuery] BalanceSheetQuery query, CancellationToken ct)
        {
            var hasil = await _service.BuildBalanceSheetCsvAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "Neraca",
                query.LegalEntityId,
                query.PeriodCode,
                query.IncludeZeroAccounts,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.RowCount
            };

            if (!hasil.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);
                return ToActionResult(hasil);
            }

            await _loggerService.InfoAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);

            return File(hasil.Data!.Content, AccLedgerCsv.ContentType, hasil.Data.FileName);
        }

        [HttpGet("financial-ratios")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat rasio keuangan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetFinancialRatios([FromQuery] FinancialRatioQuery query, CancellationToken ct)
        {
            var hasil = await _service.GetFinancialRatiosAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "RasioKeuangan",
                query.LegalEntityId,
                query.PeriodCode,
                hasil.StatusCode
            };

            if (hasil.Success)
            {
                var kini = hasil.Data!.Ratios.Select(x => x.Current.Status).ToList();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "FinancialStatement.Ratio",
                    $"{hasil.Message} Kolom kini: dihitung {kini.Count(x => x == FinancialRatioStatus.Calculated)}, " +
                    $"N/A {kini.Count(x => x == FinancialRatioStatus.NotApplicable)}, " +
                    $"tidak dapat dihitung {kini.Count(x => x == FinancialRatioStatus.Unavailable)}, " +
                    $"tidak sah {kini.Count(x => x == FinancialRatioStatus.Invalid)}.",
                    muatan);
            }
            else
            {
                await _loggerService.WarningAsync(
                    LogCategory, "FinancialStatement.Ratio", hasil.Message, muatan);
            }

            return ToActionResult(hasil);
        }

        [HttpGet("financial-ratios/export")]
        [AccessAction("Read", "Read General Ledger", Description = "Mengunduh rasio keuangan sebagai CSV", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> ExportFinancialRatios([FromQuery] FinancialRatioQuery query, CancellationToken ct)
        {
            var hasil = await _service.BuildFinancialRatioCsvAsync(query, ct);

            var muatan = new
            {
                JenisLaporan = "RasioKeuangan",
                query.LegalEntityId,
                query.PeriodCode,
                hasil.StatusCode,
                JumlahBaris = hasil.Data?.RowCount
            };

            if (!hasil.Success)
            {
                await _loggerService.WarningAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);
                return ToActionResult(hasil);
            }

            await _loggerService.InfoAsync(LogCategory, "GeneralLedger.Export", hasil.Message, muatan);

            return File(hasil.Data!.Content, AccLedgerCsv.ContentType, hasil.Data.FileName);
        }

        [HttpGet("account-balance/{accountId:guid}")]
        [AccessAction("Read", "Read General Ledger", Description = "Melihat saldo satu akun", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("GeneralLedger", "Read")]
        public async Task<IActionResult> GetAccountBalance(
            Guid accountId,
            [FromQuery] string? periodCode,
            CancellationToken ct)
            => ToActionResult(await _service.GetAccountBalanceAsync(accountId, periodCode, ct));

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private IActionResult ToActionResult<T>(AccountingServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
