using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services
{
    /// <summary>
    /// Adapter penarik dan penormalisasi data layanan medis dari berbagai modul sumber (Kamar Operasi, Poliklinik/Rawat Inap,
    /// Laboratorium, dan Billing) ke dalam struktur seragam AdaptedServiceItem (MDF-DES-011, MF-DEC-003, MF-DEC-015, MDF-INTEGRATION-1.0 §2).
    /// Mengisolasi seluruh perbedaan teknis antar sumber layanan agar tidak merembes ke mesin perhitungan jasa medis.
    /// </summary>
    public class MedicalFeeServiceSourceAdapter
    {
        private static readonly TimeZoneInfo AsiaJakartaTimeZone = GetAsiaJakartaTimeZone();

        private static TimeZoneInfo GetAsiaJakartaTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<MedicalFeeServiceSourceAdapter> _logger;

        public MedicalFeeServiceSourceAdapter(
            ApplicationDbContext dbContext,
            ILogger<MedicalFeeServiceSourceAdapter> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Menarik seluruh transaksi layanan medis yang relevan pada rentang periode kalkulasi,
        /// memetakan pelaksana dan peran, serta mencocokkannya dengan nilai kotor tagihan di Billing.
        /// </summary>
        public async Task<List<AdaptedServiceItem>> ExtractServicesForPeriodAsync(
            DateOnly periodStart,
            DateOnly periodEnd,
            CancellationToken ct = default)
        {
            var startUtc = periodStart.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endUtc = periodEnd.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            _logger.LogInformation(
                "Memulai penarikan layanan medis untuk periode {Start} s.d. {End}",
                periodStart, periodEnd);

            // 1. Pemuatan referensi peran jasa medis dan pemetaannya
            var roles = await _dbContext.MstMedicalFeeRoles
                .AsNoTracking()
                .Where(r => !r.IsDelete && r.IsActive)
                .ToListAsync(ct);

            var primaryRole = roles.FirstOrDefault(r => r.IsPrimaryRole);
            var oprRoleMap = roles
                .Where(r => !string.IsNullOrWhiteSpace(r.OprTeamRoleMapping))
                .ToDictionary(r => r.OprTeamRoleMapping!.Trim(), r => r, StringComparer.OrdinalIgnoreCase);

            var roleByCodeMap = roles
                .Where(r => !string.IsNullOrWhiteSpace(r.RoleCode))
                .ToDictionary(r => r.RoleCode.Trim(), r => r, StringComparer.OrdinalIgnoreCase);

            // 2. Pemuatan pemetaan WorkforceProfileId -> DoctorId dan DoctorId -> DoctorName
            var doctors = await _dbContext.MstDoctors
                .AsNoTracking()
                .Where(d => !d.IsDelete)
                .Select(d => new { d.Id, DoctorName = d.FullName, d.WorkforceProfileId })
                .ToListAsync(ct);

            var doctorByWorkforceMap = doctors
                .Where(d => d.WorkforceProfileId != Guid.Empty)
                .GroupBy(d => d.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => (Id: g.First().Id, DoctorName: g.First().DoctorName));

            var doctorByIdMap = doctors
                .ToDictionary(d => d.Id, d => d.DoctorName);

            // 3. Pemuatan baris tagihan Billing yang sudah final/closed pada rentang tanggal
            var invoiceItems = await _dbContext.BilInvoiceItems
                .AsNoTracking()
                .Include(i => i.Invoice)
                .Where(i => !i.IsDelete &&
                            i.Status == BillingInvoiceItemStatuses.Active &&
                            (i.Invoice.Status == BillingInvoiceStatuses.Final || i.Invoice.Status == BillingInvoiceStatuses.Closed) &&
                            i.Invoice.InvoiceDate.HasValue &&
                            i.Invoice.InvoiceDate.Value.UtcDateTime >= startUtc &&
                            i.Invoice.InvoiceDate.Value.UtcDateTime <= endUtc)
                .ToListAsync(ct);

            // Indeks pencarian cepat Billing item: key = "{SourceDomain}_{SourceDetailId}"
            var invoiceMap = invoiceItems
                .GroupBy(i => $"{i.SourceDomain}_{i.SourceDetailId}")
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var result = new List<AdaptedServiceItem>();

            // 4. Adaptasi Kamar Operasi (OPERATING_ROOM) — Dukungan tim penuh (T-25)
            var oprServices = await ExtractOperatingRoomServicesAsync(
                startUtc, endUtc, invoiceMap, oprRoleMap, doctorByWorkforceMap, ct);
            result.AddRange(oprServices);

            // 5. Adaptasi Tindakan Klinis (PROCEDURE) — Pelaksana tunggal
            var procedureServices = await ExtractProcedureServicesAsync(
                startUtc, endUtc, invoiceMap, primaryRole, ct);
            result.AddRange(procedureServices);

            // 6. Adaptasi Laboratorium (LABORATORY) — Pelaksana tunggal (T-39)
            var labServices = await ExtractLaboratoryServicesAsync(
                startUtc, endUtc, invoiceMap, primaryRole, ct);
            result.AddRange(labServices);

            // 7. Adaptasi Entri Bebas Kasir (ADHOC & ADHOC_CATALOG) — Terkunci MDF-INTEGRATION-1.1 §5 (BE-MDF-015)
            var adhocServices = ExtractAdhocBillingServices(
                invoiceItems, primaryRole, roleByCodeMap, doctorByIdMap, periodStart, periodEnd);
            result.AddRange(adhocServices);

            // CATATAN PENTING (T-38, MF-DEC-015):
            // Layanan Radiologi DIABAIKAN SEPENUHNYA — tidak ditarik, tidak dihitung, dan tidak dicatat ke unresolved.

            _logger.LogInformation(
                "Penarikan layanan selesai. Total {Count} item diadaptasi (Kamar Operasi: {Opr}, Klinis: {Proc}, Lab: {Lab}, Adhoc: {Adhoc})",
                result.Count, oprServices.Count, procedureServices.Count, labServices.Count, adhocServices.Count);

            return result;
        }

        // =====================================================================
        // 4. Sumber Kamar Operasi (OPERATING_ROOM)
        // =====================================================================

        /// <summary>
        /// Menarik kasus operasi beserta seluruh anggota timnya (OprTeamMember).
        /// Satu operasi dengan 3 anggota tim menghasilkan tepat 3 rincian (T-25, MDF-DES-013).
        /// </summary>
        private async Task<List<AdaptedServiceItem>> ExtractOperatingRoomServicesAsync(
            DateTime startUtc,
            DateTime endUtc,
            Dictionary<string, BilInvoiceItem> invoiceMap,
            Dictionary<string, MstMedicalFeeRole> oprRoleMap,
            Dictionary<Guid, (Guid Id, string DoctorName)> doctorByWorkforceMap,
            CancellationToken ct)
        {
            var items = new List<AdaptedServiceItem>();

            var oprCases = await _dbContext.Set<OprCase>()
                .AsNoTracking()
                .Include(c => c.TeamMembers.Where(m => !m.IsDelete && m.IsCurrent))
                .ThenInclude(m => m.Workforce)
                .Include(c => c.Procedures.Where(p => !p.IsDelete))
                .ThenInclude(p => p.PatientProcedure)
                .Where(c => !c.IsDelete &&
                            c.Status != OprCaseStatus.Cancelled &&
                            c.Status != OprCaseStatus.Rejected &&
                            c.RequestedAt >= startUtc &&
                            c.RequestedAt <= endUtc)
                .ToListAsync(ct);

            foreach (var oprCase in oprCases)
            {
                var serviceDate = DateOnly.FromDateTime(oprCase.RequestedAt);

                // Mencari tagihan billing yang berkaitan dengan OprCase
                BilInvoiceItem? invoiceItem = null;
                if (!invoiceMap.TryGetValue($"OPERATING_ROOM_{oprCase.Id}", out invoiceItem))
                {
                    // Fallback bila ditagihkan per prosedur kasus
                    foreach (var proc in oprCase.Procedures)
                    {
                        if (invoiceMap.TryGetValue($"OPERATING_ROOM_{proc.Id}", out invoiceItem) ||
                            invoiceMap.TryGetValue($"PROCEDURE_{proc.PatientProcedureId}", out invoiceItem))
                        {
                            break;
                        }
                    }
                }

                // Nilai kotor dasar baris tagihan sebelum diskon (MDF-DES-012)
                var baseAmount = invoiceItem != null
                    ? (invoiceItem.Quantity * invoiceItem.UnitPrice)
                    : 0m;

                var tariffId = invoiceItem?.TariffId;
                var tariffCategoryId = invoiceItem?.CategoryId;

                // Bila belum ada anggota tim yang terdaftar pada kasus operasi
                if (!oprCase.TeamMembers.Any())
                {
                    items.Add(new AdaptedServiceItem
                    {
                        SourceDomain = MdfServiceSourceDomains.OperatingRoom,
                        SourceDetailId = oprCase.Id.ToString(),
                        InvoiceItemId = invoiceItem?.Id,
                        TariffId = tariffId,
                        TariffCategoryId = tariffCategoryId,
                        ServiceDescription = $"Operasi {oprCase.CaseNumber}: {oprCase.Indication}",
                        ServiceDate = serviceDate,
                        BaseAmount = baseAmount,
                        PayeeType = MdfPayeeTypes.Doctor,
                        PayeeReferenceId = null,
                        PayeeName = string.Empty,
                        RoleId = null,
                        UnresolvedReason = MdfUnresolvedReasons.PerformerMissing,
                        UnresolvedReasonDetail = $"Kasus operasi {oprCase.CaseNumber} belum memiliki anggota tim operasi terdaftar."
                    });
                    continue;
                }

                // T-25: Setiap anggota tim menghasilkan tepat satu rincian mandiri
                foreach (var member in oprCase.TeamMembers)
                {
                    var sourceDetailId = $"{oprCase.Id}/{member.Id}";

                    // Pemetaan peran OprTeamRole ke MstMedicalFeeRole (MDF-VAL 5.5)
                    var roleMappingKey = member.Role.ToString();
                    oprRoleMap.TryGetValue(roleMappingKey, out var feeRole);

                    // Pemetaan pelaksana (WorkforceId -> MstDoctor atau MstWorkforceProfile)
                    Guid? payeeReferenceId = null;
                    var payeeType = MdfPayeeTypes.Doctor;
                    var payeeName = string.Empty;

                    if (member.WorkforceId != Guid.Empty)
                    {
                        if (doctorByWorkforceMap.TryGetValue(member.WorkforceId, out var doc))
                        {
                            payeeType = MdfPayeeTypes.Doctor;
                            payeeReferenceId = doc.Id;
                            payeeName = doc.DoctorName;
                        }
                        else
                        {
                            payeeType = MdfPayeeTypes.Workforce;
                            payeeReferenceId = member.WorkforceId;
                            payeeName = member.Workforce?.DisplayName ?? "Tenaga Medis Kamar Operasi";
                        }
                    }

                    string? unresolvedReason = null;
                    string? unresolvedReasonDetail = null;

                    if (!payeeReferenceId.HasValue || payeeReferenceId.Value == Guid.Empty)
                    {
                        unresolvedReason = MdfUnresolvedReasons.PerformerMissing;
                        unresolvedReasonDetail = $"Anggota tim operasi ({member.Role}) belum memiliki data tenaga kerja/dokter sah.";
                    }
                    else if (feeRole == null)
                    {
                        unresolvedReason = MdfUnresolvedReasons.RoleUnmapped;
                        unresolvedReasonDetail = $"Peran kamar operasi '{member.Role}' belum dipetakan ke MstMedicalFeeRole.";
                    }

                    items.Add(new AdaptedServiceItem
                    {
                        SourceDomain = MdfServiceSourceDomains.OperatingRoom,
                        SourceDetailId = sourceDetailId,
                        InvoiceItemId = invoiceItem?.Id,
                        TariffId = tariffId,
                        TariffCategoryId = tariffCategoryId,
                        ServiceDescription = $"Operasi {oprCase.CaseNumber} - {member.Role}",
                        ServiceDate = serviceDate,
                        BaseAmount = baseAmount,
                        PayeeType = payeeType,
                        PayeeReferenceId = payeeReferenceId,
                        PayeeName = payeeName,
                        RoleId = feeRole?.Id,
                        RoleCode = feeRole?.RoleCode,
                        RoleName = feeRole?.RoleName,
                        IsPrimaryRole = member.IsLead || (feeRole?.IsPrimaryRole ?? false),
                        UnresolvedReason = unresolvedReason,
                        UnresolvedReasonDetail = unresolvedReasonDetail
                    });
                }
            }

            return items;
        }

        // =====================================================================
        // 5. Sumber Tindakan Klinis (PROCEDURE)
        // =====================================================================

        /// <summary>
        /// Menarik tindakan poliklinik dan rawat inap dari TrxPatientProcedure.
        /// Pelaksana tunggal (DoctorId) dengan peran utama (IsPrimaryRole = true) (MDF-INTEGRATION-1.0 §2.2).
        /// </summary>
        private async Task<List<AdaptedServiceItem>> ExtractProcedureServicesAsync(
            DateTime startUtc,
            DateTime endUtc,
            Dictionary<string, BilInvoiceItem> invoiceMap,
            MstMedicalFeeRole? primaryRole,
            CancellationToken ct)
        {
            var items = new List<AdaptedServiceItem>();

            var procedures = await _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Include(p => p.Doctor)
                .Include(p => p.Tariff)
                .Where(p => !p.IsDelete &&
                            !p.IsSurgeryRelated && // Tindakan operasi ditangani oleh modul Kamar Operasi
                            p.ProcedureDateTime >= startUtc &&
                            p.ProcedureDateTime <= endUtc)
                .ToListAsync(ct);

            foreach (var proc in procedures)
            {
                var sourceDetailId = proc.Id.ToString();
                var serviceDate = DateOnly.FromDateTime(proc.ProcedureDateTime);

                invoiceMap.TryGetValue($"PROCEDURE_{sourceDetailId}", out var invoiceItem);

                var baseAmount = invoiceItem != null
                    ? (invoiceItem.Quantity * invoiceItem.UnitPrice)
                    : (proc.Quantity * (proc.Tariff?.NormalPrice ?? 0m));

                var tariffId = invoiceItem?.TariffId ?? proc.TariffId;
                var tariffCategoryId = invoiceItem?.CategoryId;

                string? unresolvedReason = null;
                string? unresolvedReasonDetail = null;

                Guid? payeeReferenceId = proc.DoctorId != Guid.Empty ? proc.DoctorId : null;

                if (!payeeReferenceId.HasValue)
                {
                    unresolvedReason = MdfUnresolvedReasons.PerformerMissing;
                    unresolvedReasonDetail = $"Tindakan medis '{proc.ProcedureNameSnapshot}' belum memiliki dokter pelaksana (DoctorId).";
                }
                else if (primaryRole == null)
                {
                    unresolvedReason = MdfUnresolvedReasons.RoleUnmapped;
                    unresolvedReasonDetail = "Peran utama (IsPrimaryRole = true) belum dikonfigurasi pada data induk peran jasa medis.";
                }

                items.Add(new AdaptedServiceItem
                {
                    SourceDomain = MdfServiceSourceDomains.Procedure,
                    SourceDetailId = sourceDetailId,
                    InvoiceItemId = invoiceItem?.Id,
                    TariffId = tariffId,
                    TariffCategoryId = tariffCategoryId,
                    ServiceDescription = proc.ProcedureNameSnapshot,
                    ServiceDate = serviceDate,
                    BaseAmount = baseAmount,
                    PayeeType = MdfPayeeTypes.Doctor,
                    PayeeReferenceId = payeeReferenceId,
                    PayeeName = proc.Doctor?.FullName ?? string.Empty,
                    RoleId = primaryRole?.Id,
                    RoleCode = primaryRole?.RoleCode,
                    RoleName = primaryRole?.RoleName,
                    IsPrimaryRole = true,
                    UnresolvedReason = unresolvedReason,
                    UnresolvedReasonDetail = unresolvedReasonDetail
                });
            }

            return items;
        }

        // =====================================================================
        // 6. Sumber Laboratorium (LABORATORY)
        // =====================================================================

        /// <summary>
        /// Menarik pesanan laboratorium dari LabOrder.
        /// Pelaksana adalah ExaminerDoctorId. Bila kosong, dicatat sebagai PERFORMER_MISSING (T-39, MDF-INTEGRATION-1.0 §2.3).
        /// </summary>
        private async Task<List<AdaptedServiceItem>> ExtractLaboratoryServicesAsync(
            DateTime startUtc,
            DateTime endUtc,
            Dictionary<string, BilInvoiceItem> invoiceMap,
            MstMedicalFeeRole? primaryRole,
            CancellationToken ct)
        {
            var items = new List<AdaptedServiceItem>();

            var labOrders = await _dbContext.Set<LabOrder>()
                .AsNoTracking()
                .Where(l => !l.IsDelete &&
                            l.OrderStatus != LabOrderStatus.Cancelled &&
                            l.RequestedAt.HasValue &&
                            l.RequestedAt.Value >= startUtc &&
                            l.RequestedAt.Value <= endUtc)
                .ToListAsync(ct);

            // Memuat nama dokter pemeriksa bila ada
            var examinerDoctorIds = labOrders
                .Where(l => l.ExaminerDoctorId.HasValue && l.ExaminerDoctorId.Value != Guid.Empty)
                .Select(l => l.ExaminerDoctorId!.Value)
                .Distinct()
                .ToList();

            var examinerDoctorMap = await _dbContext.MstDoctors
                .AsNoTracking()
                .Where(d => examinerDoctorIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.FullName, ct);

            foreach (var lab in labOrders)
            {
                var sourceDetailId = lab.Id.ToString();
                var serviceDate = DateOnly.FromDateTime(lab.CompletedAt ?? lab.RequestedAt ?? DateTime.UtcNow);

                invoiceMap.TryGetValue($"LABORATORY_{sourceDetailId}", out var invoiceItem);

                var baseAmount = invoiceItem != null
                    ? (invoiceItem.Quantity * invoiceItem.UnitPrice)
                    : 0m;

                var tariffId = invoiceItem?.TariffId;
                var tariffCategoryId = invoiceItem?.CategoryId;

                string? unresolvedReason = null;
                string? unresolvedReasonDetail = null;

                // T-39: LabOrder tanpa ExaminerDoctorId ditandai PERFORMER_MISSING
                Guid? payeeReferenceId = lab.ExaminerDoctorId.HasValue && lab.ExaminerDoctorId.Value != Guid.Empty
                    ? lab.ExaminerDoctorId.Value
                    : null;

                var payeeName = string.Empty;
                if (payeeReferenceId.HasValue && examinerDoctorMap.TryGetValue(payeeReferenceId.Value, out var name))
                {
                    payeeName = name;
                }

                if (!payeeReferenceId.HasValue)
                {
                    unresolvedReason = MdfUnresolvedReasons.PerformerMissing;
                    unresolvedReasonDetail = $"Pesanan laboratorium {lab.OrderNumber} belum memiliki dokter pemeriksa (ExaminerDoctorId).";
                }
                else if (primaryRole == null)
                {
                    unresolvedReason = MdfUnresolvedReasons.RoleUnmapped;
                    unresolvedReasonDetail = "Peran utama (IsPrimaryRole = true) belum dikonfigurasi pada data induk peran jasa medis.";
                }

                items.Add(new AdaptedServiceItem
                {
                    SourceDomain = MdfServiceSourceDomains.Laboratory,
                    SourceDetailId = sourceDetailId,
                    InvoiceItemId = invoiceItem?.Id,
                    TariffId = tariffId,
                    TariffCategoryId = tariffCategoryId,
                    ServiceDescription = $"Pemeriksaan Lab {lab.OrderNumber}",
                    ServiceDate = serviceDate,
                    BaseAmount = baseAmount,
                    PayeeType = MdfPayeeTypes.Doctor,
                    PayeeReferenceId = payeeReferenceId,
                    PayeeName = payeeName,
                    RoleId = primaryRole?.Id,
                    RoleCode = primaryRole?.RoleCode,
                    RoleName = primaryRole?.RoleName,
                    IsPrimaryRole = true,
                    UnresolvedReason = unresolvedReason,
                    UnresolvedReasonDetail = unresolvedReasonDetail
                });
            }

            return items;
        }

        // =====================================================================
        // 7. Sumber Adhoc Billing (ADHOC & ADHOC_CATALOG)
        // =====================================================================

        /// <summary>
        /// Menarik entri bebas kasir yang mengandung hak jasa (IsFeeBearing == true)
        /// sesuai kontrak terkunci MDF-INTEGRATION-1.1 §5 (locked 27 Oktober 2026, MDF-DES-021, BE-MDF-015).
        /// Menutup celah G1–G4:
        /// - G1 & T-104: Memetakan PerformerType mentah (DOCTOR/WORKFORCE) ke MdfPayeeTypes (Doctor/Workforce). Nilai lain -> PERFORMER_MISSING.
        /// - G2 & T-105: Memakai PerformerRoleCode jika terisi; tidak dikenal/nonaktif -> ROLE_UNMAPPED; kosong -> peran utama aktif.
        /// - G3 & T-106: Mengonversi SourceOccurredAt ke zona waktu Asia/Jakarta (WIB) sehingga layanan pukul 23.30 WIB jatuh pada tanggal WIB.
        /// - G4: Komentar kontrak §5 disesuaikan dengan versi 1.1 yang terkunci.
        /// - T-103: Baris IsFeeBearing == false tidak dihitung dan TIDAK dicatat ke daftar belum dapat dihitung (unresolved).
        /// - T-107: Entri bebas ADHOC tanpa tarif (TariffId == null) dihitung memakai baris tarif bawaan peran (MF-OQ-012 pilihan A).
        /// </summary>
        private List<AdaptedServiceItem> ExtractAdhocBillingServices(
            List<BilInvoiceItem> invoiceItems,
            MstMedicalFeeRole? primaryRole,
            Dictionary<string, MstMedicalFeeRole> roleByCodeMap,
            Dictionary<Guid, string> doctorByIdMap,
            DateOnly periodStart,
            DateOnly periodEnd)
        {
            var items = new List<AdaptedServiceItem>();

            // T-103, Rule 5.3: Baris dengan IsFeeBearing == false tidak dihitung dan TIDAK dicatat PERFORMER_MISSING.
            var feeBearingItems = invoiceItems
                .Where(i => i.IsFeeBearing &&
                            (i.SourceDomain == MdfServiceSourceDomains.Adhoc ||
                             i.SourceDomain == MdfServiceSourceDomains.AdhocCatalog))
                .ToList();

            foreach (var item in feeBearingItems)
            {
                // G3 & T-106: Tanggal layanan dari SourceOccurredAt dikonversi ke tanggal Asia/Jakarta (WIB).
                // Layanan pukul 23.30 WIB jatuh pada tanggal WIB, bukan tanggal UTC.
                DateTimeOffset sourceTime = item.SourceOccurredAt != default && item.SourceOccurredAt > DateTimeOffset.MinValue
                    ? item.SourceOccurredAt
                    : (item.Invoice.InvoiceDate ?? DateTimeOffset.UtcNow);

                var localTime = TimeZoneInfo.ConvertTime(sourceTime, AsiaJakartaTimeZone);
                var serviceDate = DateOnly.FromDateTime(localTime.DateTime);

                // Pastikan tanggal layanan berada dalam rentang periode kalkulasi
                if (serviceDate < periodStart || serviceDate > periodEnd)
                {
                    continue;
                }

                var baseAmount = item.Quantity * item.UnitPrice; // Nilai kotor sebelum diskon (MDF-DES-012)

                string? unresolvedReason = null;
                string? unresolvedReasonDetail = null;

                // G1 & T-104: Pemetaan PerformerType mentah ke MdfPayeeTypes (Doctor / Workforce).
                // Nilai kosong atau lain TIDAK menebak Doctor, melainkan ditandai PERFORMER_MISSING (MDF-VAL-1.0 §5.3).
                string payeeType;
                if (string.Equals(item.PerformerType, BillingPerformerTypes.Doctor, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.PerformerType, "Doctor", StringComparison.OrdinalIgnoreCase))
                {
                    payeeType = MdfPayeeTypes.Doctor;
                }
                else if (string.Equals(item.PerformerType, BillingPerformerTypes.Workforce, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(item.PerformerType, "Workforce", StringComparison.OrdinalIgnoreCase))
                {
                    payeeType = MdfPayeeTypes.Workforce;
                }
                else
                {
                    payeeType = string.IsNullOrWhiteSpace(item.PerformerType) ? "UNKNOWN" : item.PerformerType.Trim();
                    unresolvedReason = MdfUnresolvedReasons.PerformerMissing;
                    unresolvedReasonDetail = $"Jenis pelaksana jasa ('{item.PerformerType}') pada entri kasir '{item.DescriptionSnapshot}' tidak valid atau tidak dikenal (wajib DOCTOR atau WORKFORCE).";
                }

                if (!item.PerformerReferenceId.HasValue || item.PerformerReferenceId.Value == Guid.Empty)
                {
                    unresolvedReason = MdfUnresolvedReasons.PerformerMissing;
                    unresolvedReasonDetail = $"Item tagihan bebas '{item.DescriptionSnapshot}' belum memiliki rujukan pelaksana jasa (PerformerReferenceId kosong).";
                }

                // G2 & T-105: Pemilihan peran berdasarkan PerformerRoleCode
                MstMedicalFeeRole? targetRole = null;
                if (string.IsNullOrWhiteSpace(item.PerformerRoleCode))
                {
                    // PerformerRoleCode kosong -> gunakan peran utama aktif
                    if (primaryRole != null)
                    {
                        targetRole = primaryRole;
                    }
                    else if (unresolvedReason == null)
                    {
                        unresolvedReason = MdfUnresolvedReasons.RoleUnmapped;
                        unresolvedReasonDetail = "Peran utama (IsPrimaryRole = true) belum dikonfigurasi pada data induk peran jasa medis.";
                    }
                }
                else
                {
                    // PerformerRoleCode terisi -> cari peran aktif yang cocok pada data induk peran
                    var roleCode = item.PerformerRoleCode.Trim();
                    if (roleByCodeMap.TryGetValue(roleCode, out var matchedRole) && matchedRole.IsActive)
                    {
                        targetRole = matchedRole;
                    }
                    else if (unresolvedReason == null)
                    {
                        unresolvedReason = MdfUnresolvedReasons.RoleUnmapped;
                        unresolvedReasonDetail = $"Kode peran pelaksana '{roleCode}' tidak ditemukan atau nonaktif pada data induk peran jasa medis.";
                    }
                }

                // Nama penerima jika bertipe Doctor dan terdaftar di master dokter
                var payeeName = string.Empty;
                if (payeeType == MdfPayeeTypes.Doctor && item.PerformerReferenceId.HasValue &&
                    doctorByIdMap.TryGetValue(item.PerformerReferenceId.Value, out var docName))
                {
                    payeeName = docName;
                }

                items.Add(new AdaptedServiceItem
                {
                    SourceDomain = item.SourceDomain,
                    SourceDetailId = item.SourceDetailId,
                    InvoiceItemId = item.Id,
                    TariffId = item.TariffId,
                    TariffCategoryId = item.CategoryId,
                    ServiceDescription = item.DescriptionSnapshot,
                    ServiceDate = serviceDate,
                    BaseAmount = baseAmount,
                    PayeeType = payeeType,
                    PayeeReferenceId = item.PerformerReferenceId,
                    PayeeName = payeeName,
                    RoleId = targetRole?.Id,
                    RoleCode = targetRole?.RoleCode,
                    RoleName = targetRole?.RoleName,
                    IsPrimaryRole = targetRole?.IsPrimaryRole ?? false,
                    UnresolvedReason = unresolvedReason,
                    UnresolvedReasonDetail = unresolvedReasonDetail
                });
            }

            return items;
        }
    }
}

