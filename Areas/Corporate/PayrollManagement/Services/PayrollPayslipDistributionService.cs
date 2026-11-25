using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    /// <summary>
    /// Layanan distribusi dan proteksi slip gaji mandiri ESS ber-PIN sekunder 6-digit (VR-PAY-22 s.d. VR-PAY-24, BE-HRD-159, HRD-DEC-127, HRD-ASM-30).
    /// </summary>
    public class PayrollPayslipDistributionService : IPayrollPayslipDistributionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<PayrollPayslipDistributionService> _logger;

        // Remediasi 10 November 2026 (BE-HRD-159): penghitung salah-PIN, penguncian 30 menit, dan sesi 15 menit kini disimpan
        // di tabel HrdPayslipPinLockout dan HrdPayslipPinSession, bukan di memori proses yang hilang saat restart.
        private const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan SessionDuration = TimeSpan.FromMinutes(15);

        public PayrollPayslipDistributionService(
            ApplicationDbContext dbContext,
            ILogger<PayrollPayslipDistributionService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<SelfServicePayslipSummaryDto>> GetMyPayslipsAsync(
            Guid employeeId,
            int? year,
            CancellationToken cancellationToken = default)
        {
            // Query slip gaji milik pegawai yang sudah dirilis (IsEmployeeVisible = true, PayslipStatus = Published/Released)
            // MSL-04 (18 Nov 2026): employeeId adalah kunci pegawai dari token (workforce_profile_id/employee_id), bukan ID akun.
            var (ownerProfileId, ownerEmployeeId) = await ResolveOwnerKeysAsync(employeeId, cancellationToken);
            var query = _dbContext.PrlPayrollPayslips
                .Include(p => p.PayrollRun)
                .Include(p => p.PayrollRunEmployee)
                .Where(p => p.IsEmployeeVisible &&
                            p.IsActive &&
                            (p.PayslipStatus == "Published" || p.PayslipStatus == "Released" || p.PayslipStatus == "Downloaded") &&
                            ((ownerProfileId != null && p.PayrollRunEmployee!.WorkforceProfileId == ownerProfileId) ||
                             (ownerEmployeeId != null && p.PayrollRunEmployee!.EmployeeId == ownerEmployeeId)));

            if (year.HasValue)
            {
                query = query.Where(p => p.PeriodStartDateSnapshot.Year == year.Value);
            }

            var list = await query
                .OrderByDescending(p => p.PeriodStartDateSnapshot)
                .ToListAsync(cancellationToken);

            return list.Select(p => new SelfServicePayslipSummaryDto
            {
                PayslipId = p.Id,
                PayrollRunId = p.PayrollRunId,
                PayslipNumber = p.PayslipNumber,
                PeriodYear = p.PeriodStartDateSnapshot.Year,
                PeriodMonth = p.PeriodStartDateSnapshot.Month,
                PeriodName = $"{p.PeriodStartDateSnapshot:MMMM yyyy}",
                NetPay = p.NetPay,
                GrossPay = p.GrossPay,
                PayslipStatus = p.PayslipStatus,
                PublishedAt = p.PublishedAt,
                IsDownloaded = p.DownloadCount > 0,
                DownloadCount = p.DownloadCount
            }).ToList();
        }

        public async Task<PayslipPinVerificationResultDto> VerifyPinAsync(
            Guid employeeId,
            string pin,
            CancellationToken cancellationToken = default)
        {
            // 1. Validasi format PIN 6 digit angka numerik
            if (string.IsNullOrWhiteSpace(pin) || !Regex.IsMatch(pin.Trim(), @"^\d{6}$"))
            {
                return new PayslipPinVerificationResultDto
                {
                    IsSuccess = false,
                    Message = "PIN sekunder slip gaji wajib terdiri tepat dari 6 digit numerik."
                };
            }

            var cleanPin = pin.Trim();

            // 2. Evaluasi status lockout akun (VR-PAY-22, PAY-B6) — dibaca dari database, berlaku lintas restart dan instance
            var lockout = await _dbContext.HrdPayslipPinLockouts
                .FirstOrDefaultAsync(l => l.EmployeeKey == employeeId && !l.IsDelete, cancellationToken);

            if (lockout != null && lockout.LockoutExpiresAt.HasValue)
            {
                if (lockout.LockoutExpiresAt.Value > DateTime.UtcNow)
                {
                    var minutesRemaining = (int)Math.Ceiling((lockout.LockoutExpiresAt.Value - DateTime.UtcNow).TotalMinutes);
                    return new PayslipPinVerificationResultDto
                    {
                        IsSuccess = false,
                        LockoutMinutesRemaining = minutesRemaining,
                        RemainingAttempts = 0,
                        Message = $"Akses menu slip gaji terkunci selama {minutesRemaining} menit karena 3 kali kesalahan PIN berturut-turut (VR-PAY-22, PAY-B6)."
                    };
                }
                else
                {
                    // Periode lockout telah berakhir, reset state
                    _dbContext.HrdPayslipPinLockouts.Remove(lockout);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    lockout = null;
                }
            }

            // 3. Verifikasi keabsahan PIN terhadap data pegawai
            // Standar RS: PIN bawaan adalah kombinasi tanggal lahir DDMMYY (HRD-ASM-30).
            var isValidPin = false;

            var employee = await _dbContext.MstEmployees
                .FirstOrDefaultAsync(e => e.WorkforceProfileId == employeeId || e.Id == employeeId, cancellationToken);

            if (employee != null && employee.BirthDate != default)
            {
                var dobPin = employee.BirthDate.ToString("ddMMyy");
                if (cleanPin == dobPin)
                {
                    isValidPin = true;
                }
            }

            // 4. Penanganan hasil verifikasi
            if (isValidPin)
            {
                // Reset counter kegagalan
                if (lockout != null)
                {
                    _dbContext.HrdPayslipPinLockouts.Remove(lockout);
                }

                // Terbitkan token sesi rahasia valid selama 15 menit; hanya hash-nya yang disimpan
                var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
                var issuedAt = DateTime.UtcNow;
                _dbContext.HrdPayslipPinSessions.Add(new HrdPayslipPinSession
                {
                    Id = Guid.NewGuid(),
                    TokenHash = HashToken(token),
                    EmployeeKey = employeeId,
                    IssuedAt = issuedAt,
                    ExpiresAt = issuedAt.Add(SessionDuration),
                    CreateDateTime = issuedAt
                });
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Verifikasi PIN sekunder berhasil untuk Pegawai ID {EmpId}", employeeId);

                return new PayslipPinVerificationResultDto
                {
                    IsSuccess = true,
                    PinToken = token,
                    RemainingAttempts = 3,
                    Message = "Autentikasi PIN sekunder berhasil. Sesi tampilan slip gaji aktif selama 15 menit."
                };
            }
            else
            {
                // Catat kegagalan dan evaluasi penalti lockout
                var now = DateTime.UtcNow;
                if (lockout == null)
                {
                    lockout = new HrdPayslipPinLockout
                    {
                        Id = Guid.NewGuid(),
                        EmployeeKey = employeeId,
                        FailedAttempts = 0,
                        CreateDateTime = now
                    };
                    _dbContext.HrdPayslipPinLockouts.Add(lockout);
                }

                var currentFailures = lockout.FailedAttempts + 1;
                lockout.FailedAttempts = currentFailures;
                lockout.LastFailedAt = now;
                lockout.UpdateDateTime = now;

                if (currentFailures >= MaxFailedAttempts)
                {
                    lockout.LockoutExpiresAt = now.Add(LockoutDuration);
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning("Pegawai ID {EmpId} terkena lockout akses slip gaji 30 menit akibat 3x salah PIN.", employeeId);

                    return new PayslipPinVerificationResultDto
                    {
                        IsSuccess = false,
                        LockoutMinutesRemaining = 30,
                        RemainingAttempts = 0,
                        Message = "Akses menu slip gaji terkunci selama 30 menit karena 3 kali kesalahan PIN berturut-turut (VR-PAY-22, PAY-B6)."
                    };
                }
                else
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    var remaining = MaxFailedAttempts - currentFailures;

                    return new PayslipPinVerificationResultDto
                    {
                        IsSuccess = false,
                        RemainingAttempts = remaining,
                        Message = $"PIN sekunder yang dimasukkan tidak cocok. Sisa kesempatan: {remaining} kali percobaan."
                    };
                }
            }
        }

        public async Task<SelfServicePayslipDetailDto> GetPayslipDetailAsync(
            Guid payslipId,
            Guid employeeId,
            string pinToken,
            CancellationToken cancellationToken = default)
        {
            await ValidatePinSessionAsync(employeeId, pinToken, cancellationToken);

            var slip = await _dbContext.PrlPayrollPayslips
                .Include(p => p.PayrollRun)
                .Include(p => p.PayrollRunEmployee)
                .FirstOrDefaultAsync(p => p.Id == payslipId, cancellationToken);

            if (slip == null)
            {
                throw new KeyNotFoundException("Dokumen slip gaji tidak ditemukan.");
            }

            var emp = slip.PayrollRunEmployee;
            await EnsureOwnedVisiblePayslipAsync(slip, employeeId, cancellationToken);

            // Ambil rincian komponen slip gaji jika ada di PrlPayrollEmployeeComponent
            var components = emp != null
                ? await _dbContext.PrlPayrollEmployeeComponents
                    .Where(c => c.PayrollRunEmployeeId == emp.Id && c.IsActive)
                    .ToListAsync(cancellationToken)
                : new List<PrlPayrollEmployeeComponent>();

            var earnings = new List<PayslipItemLineDto>();
            var deductions = new List<PayslipItemLineDto>();

            if (components.Any())
            {
                foreach (var comp in components)
                {
                    var line = new PayslipItemLineDto
                    {
                        ComponentCode = comp.ComponentCodeSnapshot ?? string.Empty,
                        ComponentName = comp.ComponentNameSnapshot ?? comp.ComponentCodeSnapshot ?? string.Empty,
                        ComponentCategory = comp.CalculationMethodSnapshot ?? "Standard",
                        Amount = comp.Amount
                    };

                    if (string.Equals(comp.ComponentTypeSnapshot, "Earning", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(comp.ComponentTypeSnapshot, "Allowance", StringComparison.OrdinalIgnoreCase))
                    {
                        earnings.Add(line);
                    }
                    else
                    {
                        deductions.Add(line);
                    }
                }
            }
            else
            {
                // Fallback komponen sintetis berbasis snapshot PrlPayrollRunEmployee
                earnings.Add(new PayslipItemLineDto
                {
                    ComponentCode = "BASE_SALARY",
                    ComponentName = "Gaji Pokok",
                    ComponentCategory = "Basic",
                    Amount = slip.BaseSalary
                });

                if (emp != null && emp.TotalRecurringEarning > 0)
                {
                    earnings.Add(new PayslipItemLineDto
                    {
                        ComponentCode = "ALLOWANCE_RECURRING",
                        ComponentName = "Tunjangan Tetap / Profesi",
                        ComponentCategory = "Allowance",
                        Amount = emp.TotalRecurringEarning
                    });
                }

                if (emp != null && emp.TotalOvertimeAmount > 0)
                {
                    earnings.Add(new PayslipItemLineDto
                    {
                        ComponentCode = "OVERTIME_PAY",
                        ComponentName = "Upah Lembur Resmi PP 35",
                        ComponentCategory = "Overtime",
                        Amount = emp.TotalOvertimeAmount
                    });
                }

                if (emp != null && emp.TotalVariableEarning > 0)
                {
                    earnings.Add(new PayslipItemLineDto
                    {
                        ComponentCode = "SHIFT_NIGHT_PREMIUM",
                        ComponentName = "Premi Shift Malam & Variabel",
                        ComponentCategory = "ShiftPremium",
                        Amount = emp.TotalVariableEarning
                    });
                }

                // Potongan iuran hanya dari nilai hasil hitung penerima; tidak ada perkiraan dari total potongan. Tanpa baris penerima,
                // yang tersedia hanya total potongan dan ditampilkan apa adanya.
                if (emp != null && emp.TotalEmployeeInsuranceContribution > 0)
                {
                    deductions.Add(new PayslipItemLineDto
                    {
                        ComponentCode = "INSURANCE_CONTRIBUTION",
                        ComponentName = "Iuran Karyawan (asuransi)",
                        ComponentCategory = "Deduction",
                        Amount = emp.TotalEmployeeInsuranceContribution
                    });
                }
                else if (emp == null && slip.TotalDeduction > 0)
                {
                    deductions.Add(new PayslipItemLineDto
                    {
                        ComponentCode = "DEDUCTION_TOTAL",
                        ComponentName = "Total Potongan (rincian tidak tersedia)",
                        ComponentCategory = "Deduction",
                        Amount = slip.TotalDeduction
                    });
                }

                // Label pajak diturunkan dari jenis, skema, cara hitung, dan versi kebijakan yang dipakai pada hasil hitung.
                if (slip.TotalTax > 0)
                {
                    deductions.Add(new PayslipItemLineDto
                    {
                        ComponentCode = "INCOME_TAX",
                        ComponentName = PayrollTaxLabels.BuildFromSnapshot(ReadTaxSnapshot(emp)),
                        ComponentCategory = "Tax",
                        Amount = slip.TotalTax
                    });
                }
            }

            return new SelfServicePayslipDetailDto
            {
                PayslipId = slip.Id,
                PayslipNumber = slip.PayslipNumber,
                EmployeeNik = slip.EmployeeNumberSnapshot ?? "EMP-001",
                EmployeeName = slip.EmployeeNameSnapshot,
                DepartmentName = slip.DepartmentNameSnapshot ?? "Umum",
                PositionName = slip.PositionNameSnapshot ?? "Staf",
                PeriodYear = slip.PeriodStartDateSnapshot.Year,
                PeriodMonth = slip.PeriodStartDateSnapshot.Month,
                CurrencyCode = slip.CurrencyCode ?? "IDR",
                BaseSalary = slip.BaseSalary,
                TotalRecurringEarning = emp?.TotalRecurringEarning ?? 0m,
                TotalVariableEarning = emp?.TotalVariableEarning ?? 0m,
                TotalOvertimeAmount = emp?.TotalOvertimeAmount ?? 0m,
                GrossPay = slip.GrossPay,
                TotalDeduction = slip.TotalDeduction,
                TotalTax = slip.TotalTax,
                NetPay = slip.NetPay,
                PublishedAt = slip.PublishedAt,
                Earnings = earnings,
                Deductions = deductions
            };
        }

        public async Task<(byte[] FileBytes, string FileName, string ContentType)> DownloadPayslipPdfAsync(
            Guid payslipId,
            Guid employeeId,
            string pinToken,
            CancellationToken cancellationToken = default)
        {
            await ValidatePinSessionAsync(employeeId, pinToken, cancellationToken);

            var slip = await _dbContext.PrlPayrollPayslips
                .Include(p => p.PayrollRun)
                .Include(p => p.PayrollRunEmployee)
                .FirstOrDefaultAsync(p => p.Id == payslipId, cancellationToken);

            if (slip == null)
            {
                throw new KeyNotFoundException("Dokumen slip gaji tidak ditemukan.");
            }

            var emp = slip.PayrollRunEmployee;
            await EnsureOwnedVisiblePayslipAsync(slip, employeeId, cancellationToken);

            // Update jejak audit pengunduhan slip gaji
            slip.DownloadCount++;
            slip.LastDownloadedAt = DateTime.UtcNow;
            if (!slip.FirstDownloadedAt.HasValue)
            {
                slip.FirstDownloadedAt = DateTime.UtcNow;
            }
            if (slip.PayslipStatus == "Published")
            {
                slip.PayslipStatus = "Downloaded";
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // MSL-09 (18 Nov 2026): PDF ber-watermark dan berkata sandi AES-256 (FR-PAY-25, HRD-DEC-127 butir 3).
            // Kata sandi pembuka = tanggal lahir pegawai DDMMYYYY (8 digit). Tanpa tanggal lahir, PDF tidak dibuat (fail-closed).
            var (_, ownerEmployeeId) = await ResolveOwnerKeysAsync(employeeId, cancellationToken);
            var birthDate = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(e => (ownerEmployeeId != null && e.Id == ownerEmployeeId) || e.WorkforceProfileId == employeeId)
                .Select(e => e.BirthDate)
                .FirstOrDefaultAsync(cancellationToken);
            if (birthDate == default)
            {
                throw new InvalidOperationException("PDF slip gaji belum dapat dibuat karena tanggal lahir Anda belum tercatat di data kepegawaian. Hubungi bagian SDM.");
            }

            var pdfBytes = EncryptPdf(GeneratePayslipPdfDocument(slip, emp), birthDate.ToString("ddMMyyyy"));
            var safeNumber = slip.PayslipNumber.Replace("/", "-");
            var fileName = $"SlipGaji_{safeNumber}.pdf";

            return (pdfBytes, fileName, "application/pdf");
        }

        private async Task ValidatePinSessionAsync(Guid employeeId, string pinToken, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(pinToken))
            {
                throw new UnauthorizedAccessException("Sesi autentikasi PIN slip gaji tidak valid atau telah kedaluwarsa. Silakan masukkan PIN 6-digit Anda kembali.");
            }

            var tokenHash = HashToken(pinToken.Trim());
            var now = DateTime.UtcNow;
            var isValid = await _dbContext.HrdPayslipPinSessions
                .AsNoTracking()
                .AnyAsync(s => s.TokenHash == tokenHash
                            && s.EmployeeKey == employeeId
                            && s.RevokedAt == null
                            && !s.IsDelete
                            && s.ExpiresAt > now, cancellationToken);

            if (!isValid)
            {
                throw new UnauthorizedAccessException("Sesi autentikasi PIN slip gaji tidak valid atau telah kedaluwarsa. Silakan masukkan PIN 6-digit Anda kembali.");
            }
        }

        private static readonly JsonSerializerOptions SnapshotJsonOptions = new() { PropertyNameCaseInsensitive = true };

        /// <summary>Snapshot pajak penerima pada run; null bila tidak ada atau tidak terbaca.</summary>
        private static PayrollTaxSnapshot? ReadTaxSnapshot(PrlPayrollRunEmployee? emp)
        {
            if (emp == null || string.IsNullOrWhiteSpace(emp.TaxSnapshotJson)) return null;
            try { return JsonSerializer.Deserialize<PayrollTaxSnapshot>(emp.TaxSnapshotJson, SnapshotJsonOptions); }
            catch (JsonException) { return null; }
        }

        private static string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
        }

        /// <summary>
        /// Membentuk PDF slip gaji resmi dengan watermark nama dan NIK pegawai (VR-PAY-24, HRD-DEC-127).
        /// Sejak 18 Nov 2026 (MSL-09) dibangun dengan QuestPDF; sebelumnya PDF ditulis manual tanpa tabel xref yang sah.
        /// Nilai yang tidak tercatat ditampilkan "-", tidak lagi diperkirakan dari total potongan.
        /// </summary>
        private static byte[] GeneratePayslipPdfDocument(PrlPayrollPayslip slip, PrlPayrollRunEmployee? emp)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("id-ID");
            string Rupiah(decimal? value) => value.HasValue ? "Rp " + value.Value.ToString("N2", culture) : "-";

            var employeeName = slip.EmployeeNameSnapshot ?? "-";
            var employeeNumber = slip.EmployeeNumberSnapshot ?? "-";
            var watermark = $"RAHASIA - {employeeName} - {employeeNumber}";
            var allowances = slip.GrossPay - slip.BaseSalary;
            var insurance = emp?.TotalEmployeeInsuranceContribution;
            var taxLabel = PayrollTaxLabels.BuildFromSnapshot(ReadTaxSnapshot(emp));

            var rows = new List<(string Label, string Value, bool Strong)>
            {
                ("Gaji pokok", Rupiah(slip.BaseSalary), false),
                ("Tunjangan dan lembur", Rupiah(allowances), false),
                ("Total penghasilan bruto", Rupiah(slip.GrossPay), true),
                ("Potongan iuran pegawai (asuransi)", Rupiah(insurance), false),
                (taxLabel, Rupiah(slip.TotalTax), false),
                ("Total potongan", Rupiah(slip.TotalDeduction), true),
                ("Gaji bersih (take home pay)", Rupiah(slip.NetPay), true)
            };

            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Background().AlignCenter().AlignMiddle().Rotate(-30)
                        .Text(watermark).FontSize(26).FontColor(QuestPDF.Helpers.Colors.Grey.Lighten2);

                    page.Header().Column(column =>
                    {
                        column.Item().Text("SLIP GAJI PEGAWAI").FontSize(16).Bold();
                        column.Item().Text($"Nomor dokumen: {slip.PayslipNumber}");
                        column.Item().Text($"Periode: {slip.PeriodStartDateSnapshot.ToString("dd MMMM yyyy", culture)} s.d. {slip.PeriodEndDateSnapshot.ToString("dd MMMM yyyy", culture)}");
                    });

                    page.Content().PaddingVertical(12).Column(column =>
                    {
                        column.Spacing(6);
                        column.Item().Text($"Nama pegawai: {employeeName}");
                        column.Item().Text($"NIK pegawai: {employeeNumber}");
                        column.Item().Text($"Unit kerja: {slip.DepartmentNameSnapshot ?? "-"}  |  Jabatan: {slip.PositionNameSnapshot ?? "-"}");
                        column.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            foreach (var (label, value, strong) in rows)
                            {
                                var labelText = table.Cell().BorderBottom(0.5f).PaddingVertical(4).Text(label);
                                var valueText = table.Cell().BorderBottom(0.5f).PaddingVertical(4).AlignRight().Text(value);
                                if (strong)
                                {
                                    labelText.Bold();
                                    valueText.Bold();
                                }
                            }
                        });
                    });

                    page.Footer().Column(column =>
                    {
                        column.Item().Text("Dokumen rahasia. Dilarang menyebarluaskan tanpa izin pegawai yang bersangkutan.").FontSize(8);
                        column.Item().Text($"Dicetak secara digital pada {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8);
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Mengunci PDF dengan enkripsi AES-256 lewat <c>QuestPDF.Fluent.DocumentOperation</c> (qpdf bawaan paket QuestPDF).
        /// Sandi pemilik dibuat acak per dokumen, sehingga izin cetak/salin tidak dapat dilonggarkan oleh penerima.
        /// </summary>
        private static byte[] EncryptPdf(byte[] pdf, string userPassword)
        {
            var workDir = Path.Combine(Path.GetTempPath(), "quilvian-payslip");
            Directory.CreateDirectory(workDir);
            var input = Path.Combine(workDir, $"{Guid.NewGuid():N}.pdf");
            var output = Path.Combine(workDir, $"{Guid.NewGuid():N}.pdf");
            try
            {
                File.WriteAllBytes(input, pdf);
                QuestPDF.Fluent.DocumentOperation
                    .LoadFile(input)
                    .Encrypt(new QuestPDF.Fluent.DocumentOperation.Encryption256Bit
                    {
                        UserPassword = userPassword,
                        OwnerPassword = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)),
                        AllowPrinting = true,
                        AllowContentExtraction = false,
                        AllowAnnotation = false,
                        AllowAssembly = false,
                        AllowFillingForms = false,
                        EncryptMetadata = true
                    })
                    .Save(output);
                return File.ReadAllBytes(output);
            }
            finally
            {
                TryDelete(input);
                TryDelete(output);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (IOException)
            {
                // Berkas sementara di folder temp; kegagalan hapus tidak boleh menggagalkan unduhan.
            }
        }
        private async Task<(Guid? ProfileId, Guid? EmployeeId)> ResolveOwnerKeysAsync(Guid ownerKey, CancellationToken cancellationToken)
        {
            if (ownerKey == Guid.Empty)
            {
                return (null, null);
            }

            var employee = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(e => e.WorkforceProfileId == ownerKey || e.Id == ownerKey)
                .Select(e => new { e.Id, e.WorkforceProfileId })
                .FirstOrDefaultAsync(cancellationToken);

            return employee == null
                ? (ownerKey, null)
                : (employee.WorkforceProfileId != Guid.Empty ? employee.WorkforceProfileId : ownerKey, employee.Id);
        }

        /// <summary>
        /// MSL-04: slip hanya terbuka bagi pemiliknya dan hanya bila sudah dirilis ke pegawai. Slip milik orang lain
        /// dijawab "tidak ditemukan" agar keberadaannya tidak bocor.
        /// </summary>
        private async Task EnsureOwnedVisiblePayslipAsync(PrlPayrollPayslip slip, Guid ownerKey, CancellationToken cancellationToken)
        {
            var (ownerProfileId, ownerEmployeeId) = await ResolveOwnerKeysAsync(ownerKey, cancellationToken);
            var emp = slip.PayrollRunEmployee;
            var owned = emp != null &&
                        ((ownerProfileId != null && emp.WorkforceProfileId == ownerProfileId) ||
                         (ownerEmployeeId != null && emp.EmployeeId == ownerEmployeeId));
            var visible = slip.IsEmployeeVisible && slip.IsActive &&
                          (slip.PayslipStatus == "Published" || slip.PayslipStatus == "Released" || slip.PayslipStatus == "Downloaded");
            if (!owned || !visible)
            {
                throw new KeyNotFoundException("Dokumen slip gaji tidak ditemukan.");
            }
        }
    }
}
