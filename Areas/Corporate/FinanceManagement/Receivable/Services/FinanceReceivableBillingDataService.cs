using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Data Tagihan (Finance &gt; Transaksi A/R &gt; Tagihan/Billing): daftar tagihan Billing yang sudah menjadi piutang,
/// beserta saringan kategori, pencarian, status pembuatan tagihan, jenis pasien, dan periode.
///
/// HANYA MEMBACA. Service ini tidak menulis apa pun dan tidak membuat piutang atau batch; pembuatan tagihan
/// tetap lewat <see cref="FinanceReceivableInvoiceBatchService"/>. Satu query dasar dipakai untuk daftar dan
/// ringkasan, sehingga angka ringkasan selalu mengikuti saringan yang sama dengan tabel.
///
/// Dulu membaca daftar ini juga menyinkronkan dan mengolah fakta AR dari Billing menjadi piutang (efek samping pada
/// GET, termasuk untuk pengguna dengan hak Read saja, dengan <c>catch</c> kosong). Itu dicabut: pemrosesan kini hanya
/// lewat POST billing-intake/sync dan POST billing-intake/process-pending-ar (hak Sync / Process). Respons memuat
/// <see cref="BillingDataPagedResponse.PendingFacts"/> — hitungan baca-saja — supaya fakta yang belum menjadi piutang
/// tetap terlihat dan punya jalur pemrosesan.
///
/// Pemetaan kategori: company → PAYER pihak luar, internalBenefit → PAYER IsInternalBenefit, generalPatient →
/// PATIENT_GUARANTOR, employee → EMPLOYEE_BENEFIT.
/// "Sudah Dibuat" (billingStatus) = piutang menjadi anggota Batch Tagihan yang belum CANCELLED (predikat yang sama dengan
/// <see cref="FinanceReceivableInvoiceBatchService.GetEligibleReceivablesAsync"/>) dan hanya bermakna untuk company.
/// Kategori lain memakai <c>resolutionStatus</c>: status penutupan benefit / perjanjian angsuran dari data sumbernya.
/// </summary>
public sealed class FinanceReceivableBillingDataService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly FinanceBillingIntakeService _intakeService;

    public FinanceReceivableBillingDataService(
        ApplicationDbContext dbContext,
        FinanceBillingIntakeService intakeService)
    {
        _dbContext = dbContext;
        _intakeService = intakeService;
    }

    public async Task<BillingDataPagedResponse> GetAsync(
        BillingDataQuery request,
        CancellationToken cancellationToken)
    {
        var filter = ResolveFilter(request);
        var entityName = await ResolveEntityNameAsync(filter, cancellationToken);

        var rows = BuildQuery(filter);

        // Ringkasan dihitung atas seluruh hasil saringan, bukan halaman aktif.
        var total = await rows.CountAsync(cancellationToken);
        var totalAmount = await rows.SumAsync(x => (decimal?)x.Item.Amount, cancellationToken) ?? 0m;
        var patientCount = await rows
            .Where(x => x.Item.PatientId != null)
            .Select(x => x.Item.PatientId)
            .Distinct()
            .CountAsync(cancellationToken);

        var pageRows = total == 0
            ? new List<BillingDataItemResponse>()
            : await LoadPageAsync(rows, filter, cancellationToken);

        var groups = await BuildGroupsAsync(rows, pageRows, filter, total, totalAmount, cancellationToken);

        return new BillingDataPagedResponse
        {
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)filter.PageSize),
            Items = pageRows,
            Groups = groups,
            Summary = new BillingDataSummaryResponse
            {
                TotalAmount = totalAmount,
                PatientCount = patientCount,
                EntityName = entityName,
                PeriodLabel = filter.PeriodLabel
            },
            Notice = total == 0 ? EmptyStateNotice(filter.Category) : null,
            // Hitungan baca-saja; TIDAK memproses apa pun.
            PendingFacts = await _intakeService.GetPendingArFactsAsync(cancellationToken)
        };
    }

    // Keadaan kosong yang akurat per kategori. Kosong BUKAN berarti jalur tidak tersedia: tiap kategori hanya terisi
    // setelah kejadian bisnis tertentu dan setelah Finance mengolah serah terima dari Billing.
    private static string? EmptyStateNotice(string category) => category switch
    {
        BillingDataCategories.Employee =>
            "Belum ada piutang karyawan. Piutang karyawan (EMPLOYEE_BENEFIT) hanya terbentuk bila kasir menangguhkan " +
            "kewajiban pegawai menjadi AR Karyawan saat finalisasi Benefit RS. Porsi yang ditanggung Benefit RS tidak " +
            "termasuk di sini; lihat kategori Benefit RS Internal.",
        BillingDataCategories.InternalBenefit =>
            "Belum ada piutang Benefit RS Internal. Porsi ini terbentuk setelah kasir mengonfirmasi finalisasi Benefit RS " +
            "dan Finance mengolah serah terima dari Billing.",
        _ => null
    };

    /// <summary>
    /// Isi field pilihan asuransi/perusahaan: gabungan asuransi dan perusahaan penjamin yang aktif, diurutkan
    /// menurut nama. Ringan: hanya Id, nama, jenis; dibatasi <c>Limit</c> dan dipersempit lewat <c>Search</c>.
    /// </summary>
    public async Task<List<BillingDataPayerOptionResponse>> GetPayerOptionsAsync(
        BillingDataPayerOptionQuery request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, 50);
        var search = request.Search?.Trim();

        var companies = _dbContext.MstCompanyGuarantors.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
        if (!string.IsNullOrEmpty(search))
        {
            var pattern = BuildContainsPattern(search);
            companies = companies.Where(x => EF.Functions.ILike(x.CompanyGuarantorName, pattern, "\\"));
        }

        // Untuk Benefit RS Internal: penjamin adalah penjamin internal (MstCompanyGuarantor), bukan asuransi pihak luar.
        if (string.Equals(request.Category, BillingDataCategories.InternalBenefit, StringComparison.OrdinalIgnoreCase))
        {
            var internalGuarantorIds = _dbContext.FinReceivables.AsNoTracking()
                .Where(r => !r.IsDelete && r.IsInternalBenefit && r.DebtorReferenceId.HasValue)
                .Select(r => r.DebtorReferenceId!.Value);

            var internalCompanies = companies.Where(x => x.IsEmployeeBenefitGuarantor || internalGuarantorIds.Contains(x.Id));
            var hasSpecific = await internalCompanies.AnyAsync(cancellationToken);
            var query = hasSpecific ? internalCompanies : companies;

            return await query
                .OrderBy(x => x.CompanyGuarantorName).ThenBy(x => x.Id)
                .Take(limit)
                .Select(x => new BillingDataPayerOptionResponse
                {
                    Id = x.Id,
                    Name = x.CompanyGuarantorName,
                    Kind = BillingDataPayerKinds.Company
                })
                .ToListAsync(cancellationToken);
        }

        var insurance = _dbContext.MstInsuranceProviders.AsNoTracking().Where(x => !x.IsDelete && x.IsActive);
        if (!string.IsNullOrEmpty(search))
        {
            var pattern = BuildContainsPattern(search);
            insurance = insurance.Where(x => EF.Functions.ILike(x.InsuranceProviderName, pattern, "\\"));
        }

        // Masing-masing diambil sebanyak Limit lalu digabung dan dipotong lagi, sehingga hasil akhir benar
        // (Limit nama pertama menurut urutan gabungan) tanpa memuat seluruh master.
        var insuranceOptions = await insurance
            .OrderBy(x => x.InsuranceProviderName).ThenBy(x => x.Id)
            .Take(limit)
            .Select(x => new BillingDataPayerOptionResponse
            {
                Id = x.Id,
                Name = x.InsuranceProviderName,
                Kind = BillingDataPayerKinds.Insurance
            })
            .ToListAsync(cancellationToken);
        var companyOptions = await companies
            .OrderBy(x => x.CompanyGuarantorName).ThenBy(x => x.Id)
            .Take(limit)
            .Select(x => new BillingDataPayerOptionResponse
            {
                Id = x.Id,
                Name = x.CompanyGuarantorName,
                Kind = BillingDataPayerKinds.Company
            })
            .ToListAsync(cancellationToken);

        return insuranceOptions.Concat(companyOptions)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Kind, StringComparer.Ordinal)
            .Take(limit)
            .ToList();
    }

    /// <summary>
    /// Isi field pilihan karyawan pada Data Tagihan kategori employee.
    /// Berisi hanya karyawan yang terkait dengan piutang EMPLOYEE_BENEFIT yang masuk dalam cakupan Data Tagihan.
    /// Pasien umum TIDAK dimuat. Mendukung pencarian nama karyawan dan No. RM karyawan, serta paginasi.
    /// </summary>
    public async Task<PagedResult<BillingDataEmployeeOptionResponse>> GetEmployeeOptionsAsync(
        BillingDataEmployeeOptionQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.Limit.HasValue && request.Limit.Value > 0
            ? Math.Clamp(request.Limit.Value, 1, 100)
            : Math.Clamp(request.PageSize, 1, 100);
        var pageNumber = Math.Max(1, request.PageNumber);
        var search = request.Search?.Trim();

        var activeBatchedIds = ActiveBatchedReceivableIds();
        var debtorType = FinReceivableDebtorTypes.EmployeeBenefit;

        // 1. Ambil seluruh piutang EMPLOYEE_BENEFIT yang masuk cakupan Data Tagihan
        var receivablesQuery = _dbContext.FinReceivables.AsNoTracking()
            .Where(r => !r.IsDelete
                && r.DebtorType == debtorType
                && r.Status != FinReceivableStatuses.Cancelled
                && (activeBatchedIds.Contains(r.Id)
                    || r.Status == FinReceivableStatuses.Outstanding
                    || r.Status == FinReceivableStatuses.Partial
                    || r.Status == FinReceivableStatuses.Settled));

        var distinctOwners = await receivablesQuery
            .Select(r => new { r.BenefitOwnerWorkforceProfileId, r.BenefitOwnerId })
            .Distinct()
            .ToListAsync(cancellationToken);

        if (distinctOwners.Count == 0)
        {
            return new PagedResult<BillingDataEmployeeOptionResponse>
            {
                Items = new List<BillingDataEmployeeOptionResponse>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = 0,
                TotalPage = 1
            };
        }

        // 2. Terjemahkan BenefitOwnerId historis ke WorkforceProfileId
        var legacyEmployeeIds = distinctOwners
            .Where(x => !x.BenefitOwnerWorkforceProfileId.HasValue && x.BenefitOwnerId.HasValue)
            .Select(x => x.BenefitOwnerId!.Value)
            .Distinct()
            .ToList();

        var profileByEmployee = legacyEmployeeIds.Count > 0
            ? await _dbContext.MstEmployees.AsNoTracking()
                .Where(e => legacyEmployeeIds.Contains(e.Id))
                .ToDictionaryAsync(e => e.Id, e => e.WorkforceProfileId, cancellationToken)
            : new Dictionary<Guid, Guid>();

        var allProfileIds = distinctOwners
            .Select(x => x.BenefitOwnerWorkforceProfileId
                ?? (x.BenefitOwnerId.HasValue && profileByEmployee.TryGetValue(x.BenefitOwnerId.Value, out var pId) ? pId : (Guid?)null))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        // 3. Muat profil karyawan/dokter dari MstWorkforceProfile
        var profiles = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .Where(w => allProfileIds.Contains(w.Id) && !w.IsDelete)
            .Select(w => new
            {
                w.Id,
                w.DisplayName,
                EmployeeFullName = w.Employee != null ? w.Employee.FullName : null,
                DoctorFullName = w.Doctor != null ? w.Doctor.FullName : null,
                IdentityNumber = w.Employee != null ? w.Employee.IdentityNumber : null
            })
            .ToListAsync(cancellationToken);

        // 4. Telusuri No. RM karyawan yang terverifikasi (TIDAK memakai No. RM anggota keluarga)
        // 4a. Dari piutang dengan BenefitRelationship == "SELF"
        var selfPatientMrns = await (
            from item in _dbContext.FinReceivableItems.AsNoTracking()
            join r in receivablesQuery on item.ReceivableId equals r.Id
            join p in _dbContext.MstPatients.AsNoTracking() on item.PatientId equals p.Id
            where !item.IsDelete && !p.IsDelete
                && r.BenefitRelationship != null && EF.Functions.ILike(r.BenefitRelationship, "SELF")
            select new
            {
                ProfileId = r.BenefitOwnerWorkforceProfileId
                    ?? (r.BenefitOwnerId.HasValue && profileByEmployee.ContainsKey(r.BenefitOwnerId.Value) ? profileByEmployee[r.BenefitOwnerId.Value] : (Guid?)null),
                p.MedicalRecordNumber
            }
        ).Where(x => x.ProfileId.HasValue).Distinct().ToListAsync(cancellationToken);

        var mrnByProfile = new Dictionary<Guid, string>();
        foreach (var row in selfPatientMrns.Where(x => x.ProfileId.HasValue && !string.IsNullOrWhiteSpace(x.MedicalRecordNumber)))
        {
            mrnByProfile.TryAdd(row.ProfileId!.Value, row.MedicalRecordNumber);
        }

        // 4b. Dari kartu penjamin internal MstPatientCompanyGuarantor dengan BenefitRelationship == "SELF"
        var unmappedProfileIds = allProfileIds.Where(id => !mrnByProfile.ContainsKey(id)).ToList();
        if (unmappedProfileIds.Count > 0)
        {
            var cardMrns = await (
                from cg in _dbContext.MstPatientCompanyGuarantors.AsNoTracking()
                join p in _dbContext.MstPatients.AsNoTracking() on cg.PatientId equals p.Id
                where !cg.IsDelete && !cg.IsCancel && cg.IsActive && !p.IsDelete
                    && cg.BenefitRelationship != null && EF.Functions.ILike(cg.BenefitRelationship, "SELF")
                    && cg.BenefitOwnerWorkforceProfileId != null
                    && unmappedProfileIds.Contains(cg.BenefitOwnerWorkforceProfileId.Value)
                select new
                {
                    ProfileId = cg.BenefitOwnerWorkforceProfileId!.Value,
                    p.MedicalRecordNumber
                }
            ).Distinct().ToListAsync(cancellationToken);

            foreach (var card in cardMrns.Where(x => !string.IsNullOrWhiteSpace(x.MedicalRecordNumber)))
            {
                mrnByProfile.TryAdd(card.ProfileId, card.MedicalRecordNumber);
            }
        }

        // 4c. Dari pencocokan NIK terverifikasi (bila ada NIK valid pada MstEmployee yang cocok ke MstPatient)
        unmappedProfileIds = allProfileIds.Where(id => !mrnByProfile.ContainsKey(id)).ToList();
        if (unmappedProfileIds.Count > 0)
        {
            var nikPairs = profiles
                .Where(p => unmappedProfileIds.Contains(p.Id) && !string.IsNullOrWhiteSpace(p.IdentityNumber) && p.IdentityNumber.Length >= 8)
                .Select(p => new { p.Id, p.IdentityNumber })
                .ToList();

            if (nikPairs.Count > 0)
            {
                var niks = nikPairs.Select(x => x.IdentityNumber).Distinct().ToList();
                var patientNiks = await _dbContext.MstPatients.AsNoTracking()
                    .Where(p => !p.IsDelete && p.IdentityNumber != null && niks.Contains(p.IdentityNumber))
                    .Select(p => new { p.IdentityNumber, p.MedicalRecordNumber })
                    .ToListAsync(cancellationToken);

                var mrnByNik = patientNiks
                    .Where(x => !string.IsNullOrWhiteSpace(x.IdentityNumber) && !string.IsNullOrWhiteSpace(x.MedicalRecordNumber))
                    .ToDictionary(x => x.IdentityNumber!, x => x.MedicalRecordNumber);

                foreach (var pair in nikPairs)
                {
                    if (mrnByNik.TryGetValue(pair.IdentityNumber!, out var mrn))
                    {
                        mrnByProfile.TryAdd(pair.Id, mrn);
                    }
                }
            }
        }

        // 5. Rakit daftar opsi
        var candidates = new List<BillingDataEmployeeOptionResponse>();
        foreach (var profile in profiles)
        {
            var name = !string.IsNullOrWhiteSpace(profile.EmployeeFullName)
                ? profile.EmployeeFullName
                : (!string.IsNullOrWhiteSpace(profile.DoctorFullName) ? profile.DoctorFullName : profile.DisplayName);

            mrnByProfile.TryGetValue(profile.Id, out var mrn);

            candidates.Add(new BillingDataEmployeeOptionResponse
            {
                Id = profile.Id,
                Name = name,
                MedicalRecordNumber = mrn
            });
        }

        // 6. Saring berdasarkan Search (nama karyawan ATAU No. RM karyawan)
        if (!string.IsNullOrWhiteSpace(search))
        {
            candidates = candidates
                .Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (c.MedicalRecordNumber != null && c.MedicalRecordNumber.Contains(search, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        // 7. Urutkan nama karyawan ASC, lalu Id ASC
        candidates = candidates
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.Id)
            .ToList();

        var totalData = candidates.Count;
        var totalPage = Math.Max(1, (int)Math.Ceiling((double)totalData / pageSize));
        var items = candidates.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<BillingDataEmployeeOptionResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = totalData,
            TotalPage = totalPage
        };
    }

    // ------------------------------------------------------------------------------------
    // Query dasar + penerapan saringan. Urutan sama dengan rancangan: kategori → pencarian → penjamin/pasien
    // → status → jenis pasien → tanggal. Semua diterjemahkan menjadi SQL; tidak ada saringan di memori.
    // ------------------------------------------------------------------------------------

    private IQueryable<BillingDataRow> BuildQuery(BillingDataFilter filter)
    {
        var activeBatchedIds = ActiveBatchedReceivableIds();

        // Satu baris per rincian piutang. Join ke invoice, kunjungan, pasien, dan penjamin bersifat LEFT JOIN
        // pada kunci utama sehingga tidak menambah atau mengurangi baris; item migrasi lama (tanpa invoice atau
        // kunjungan) tetap muncul, sama seperti pada GET eligible-receivables.
        var debtorType = filter.DebtorType;
        var rows =
            from item in _dbContext.FinReceivableItems.AsNoTracking()
            join receivable in _dbContext.FinReceivables.AsNoTracking()
                on item.ReceivableId equals receivable.Id
            join invoiceJoin in _dbContext.BilInvoices.AsNoTracking()
                on item.InvoiceId equals (Guid?)invoiceJoin.Id into invoiceGroup
            from invoice in invoiceGroup.DefaultIfEmpty()
            join encounterJoin in _dbContext.RegPatientEncounters.AsNoTracking()
                on item.EncounterId equals (Guid?)encounterJoin.Id into encounterGroup
            from encounter in encounterGroup.DefaultIfEmpty()
            join patientJoin in _dbContext.MstPatients.AsNoTracking()
                on item.PatientId equals (Guid?)patientJoin.Id into patientGroup
            from patient in patientGroup.DefaultIfEmpty()
            join providerJoin in _dbContext.MstInsuranceProviders.AsNoTracking()
                on receivable.DebtorReferenceId equals (Guid?)providerJoin.Id into providerGroup
            from provider in providerGroup.DefaultIfEmpty()
            where !item.IsDelete
                && !receivable.IsDelete
                && receivable.DebtorType == debtorType
            select new BillingDataRow
            {
                Item = item,
                Receivable = receivable,
                Invoice = invoice,
                Encounter = encounter,
                Patient = patient,
                Provider = provider,
                // Perusahaan penjamin dibaca dari penjamin aktif pada kunjungan (yang utama dulu), karena Billing
                // hanya mengisi FinReceivable.DebtorReferenceId untuk asuransi. Dipakai hanya bila DebtorReferenceId kosong.
                CompanyGuarantorId = _dbContext.RegPatientEncounterGuarantors
                    .Where(g => g.EncounterId == item.EncounterId && g.IsActive && !g.IsDelete && g.CompanyGuarantorId != null)
                    .OrderByDescending(g => g.IsPrimary)
                    .ThenBy(g => g.Priority)
                    .Select(g => g.CompanyGuarantorId)
                    .FirstOrDefault(),
                EmployeeNumber = _dbContext.RegPatientEncounterGuarantors
                    .Where(g => g.EncounterId == item.EncounterId && g.IsActive && !g.IsDelete && g.EmployeeNumberSnapshot != null)
                    .OrderByDescending(g => g.IsPrimary)
                    .ThenBy(g => g.Priority)
                    .Select(g => g.EmployeeNumberSnapshot)
                    .FirstOrDefault()
            };

        // Porsi Benefit Karyawan RS (PAYER dengan IsInternalBenefit) dipisah dari piutang pihak luar: itu biaya rumah
        // sakit sendiri yang diselesaikan lewat Penutupan Benefit, bukan tagihan ke perusahaan/asuransi.
        if (filter.Category == BillingDataCategories.Company)
            rows = rows.Where(x => !x.Receivable.IsInternalBenefit);
        else if (filter.Category == BillingDataCategories.InternalBenefit)
            rows = rows.Where(x => x.Receivable.IsInternalBenefit);

        // Cakupan Data Tagihan: piutang yang tidak CANCELLED, dan yang masih bisa ditagihkan (OUTSTANDING/PARTIAL/
        // SETTLED) atau sudah tergabung dalam batch aktif. Status pembayaran (SETTLED/Lunas) dan status pembuatan
        // AR/Invoice (keanggotaan batch) adalah dua hal terpisah: piutang yang sudah lunas dibayar (mis. gabungan
        // asuransi + excess tunai) tapi belum pernah digabung ke Batch Tagihan tetap perlu dibuatkan AR/Invoice-nya
        // untuk keperluan dokumentasi/klaim, sehingga tetap tampil sebagai "Belum Dibuat". Hanya WRITTEN_OFF
        // (dihapus-buku, keputusan akuntansi terpisah) yang tidak pernah digabung yang tetap tidak ditampilkan.
        // Dengan cakupan ini setiap baris pasti tepat salah satu dari "Belum Dibuat" atau "Sudah Dibuat".
        rows = rows.Where(x => x.Receivable.Status != FinReceivableStatuses.Cancelled
            && (activeBatchedIds.Contains(x.Receivable.Id)
                || x.Receivable.Status == FinReceivableStatuses.Outstanding
                || x.Receivable.Status == FinReceivableStatuses.Partial
                || x.Receivable.Status == FinReceivableStatuses.Settled));

        rows = ApplyEntity(rows, filter);
        rows = ApplyBillingStatus(rows, filter, activeBatchedIds);
        rows = ApplyResolutionStatus(rows, filter);
        rows = ApplyPatientType(rows, filter);
        rows = ApplyDateRange(rows, filter);
        rows = ApplySearch(rows, filter);
        return rows;
    }

    private IQueryable<Guid> ActiveBatchedReceivableIds() =>
        _dbContext.FinReceivableInvoiceBatchItems
            .Where(i => !i.IsDelete && i.IsActiveMembership)
            .Select(i => i.ReceivableId);

    private IQueryable<BillingDataRow> ApplyEntity(IQueryable<BillingDataRow> rows, BillingDataFilter filter)
    {
        if (!filter.EntityId.HasValue) return rows;
        var entityId = filter.EntityId.Value;

        // Jangan menyaring dengan nama: kunci stabilnya Id penjamin (company & internalBenefit), Id karyawan / pemilik benefit (employee),
        // atau Id pasien (generalPatient).
        // Penjamin = asuransi/perusahaan (DebtorReferenceId) ATAU perusahaan penjamin pada kunjungan.
        if (filter.Category == BillingDataCategories.Company || filter.Category == BillingDataCategories.InternalBenefit)
        {
            return rows.Where(x => x.Receivable.DebtorReferenceId == entityId
                || (x.Receivable.DebtorReferenceId == null && x.CompanyGuarantorId == entityId));
        }

        if (filter.Category == BillingDataCategories.Employee)
        {
            return rows.Where(x =>
                x.Receivable.BenefitOwnerWorkforceProfileId == entityId
                || x.Receivable.BenefitOwnerId == entityId
                || _dbContext.MstEmployees.Any(e => e.Id == x.Receivable.BenefitOwnerId && e.WorkforceProfileId == entityId));
        }

        return rows.Where(x => x.Item.PatientId == entityId);
    }

    private static IQueryable<BillingDataRow> ApplyBillingStatus(
        IQueryable<BillingDataRow> rows, BillingDataFilter filter, IQueryable<Guid> activeBatchedIds) =>
        filter.BillingStatus switch
        {
            BillingDataStatuses.Created => rows.Where(x => activeBatchedIds.Contains(x.Receivable.Id)),
            BillingDataStatuses.NotCreated => rows.Where(x => !activeBatchedIds.Contains(x.Receivable.Id)),
            _ => rows
        };

    // Status penyelesaian menurut DATA SUMBER. Urutan prioritas sama dengan penentuan ResolutionStatus pada respons
    // (ResolveResolutionStatus): piutang Lunas mengalahkan segalanya; untuk Karyawan, perjanjian DISETUJUI mengalahkan MENUNGGU.
    // Perjanjian DITOLAK / DIBATALKAN dan Penutupan Benefit DIBATALKAN tidak dihitung: kembali ke "belum ada".
    private IQueryable<BillingDataRow> ApplyResolutionStatus(IQueryable<BillingDataRow> rows, BillingDataFilter filter)
    {
        if (filter.ResolutionStatus == BillingDataResolutionStatuses.All) return rows;

        var settledStatus = FinReceivableStatuses.Settled;
        var draftSettlementReceivableIds = _dbContext.FinBenefitSettlementItems
            .Where(i => !i.IsDelete
                && i.Settlement != null
                && !i.Settlement.IsDelete
                && i.Settlement.Status == FinBenefitSettlementStatuses.Draf)
            .Select(i => i.ReceivableId);
        var approvedPlanReceivableIds = _dbContext.FinReceivableInstallmentPlans
            .Where(p => !p.IsDelete && p.Status == FinReceivableInstallmentPlanStatuses.Disetujui)
            .Select(p => p.ReceivableId);
        var pendingPlanReceivableIds = _dbContext.FinReceivableInstallmentPlans
            .Where(p => !p.IsDelete && p.Status == FinReceivableInstallmentPlanStatuses.Menunggu)
            .Select(p => p.ReceivableId);

        return filter.ResolutionStatus switch
        {
            BillingDataResolutionStatuses.Settled or BillingDataResolutionStatuses.Paid =>
                rows.Where(x => x.Receivable.Status == settledStatus),
            BillingDataResolutionStatuses.SettlementDraft =>
                rows.Where(x => x.Receivable.Status != settledStatus && draftSettlementReceivableIds.Contains(x.Receivable.Id)),
            BillingDataResolutionStatuses.AwaitingSettlement =>
                rows.Where(x => x.Receivable.Status != settledStatus && !draftSettlementReceivableIds.Contains(x.Receivable.Id)),
            BillingDataResolutionStatuses.PlanApproved =>
                rows.Where(x => x.Receivable.Status != settledStatus && approvedPlanReceivableIds.Contains(x.Receivable.Id)),
            BillingDataResolutionStatuses.PlanPending =>
                rows.Where(x => x.Receivable.Status != settledStatus
                    && pendingPlanReceivableIds.Contains(x.Receivable.Id)
                    && !approvedPlanReceivableIds.Contains(x.Receivable.Id)),
            BillingDataResolutionStatuses.NoPlan =>
                rows.Where(x => x.Receivable.Status != settledStatus
                    && !pendingPlanReceivableIds.Contains(x.Receivable.Id)
                    && !approvedPlanReceivableIds.Contains(x.Receivable.Id)),
            _ => rows
        };
    }

    private static IQueryable<BillingDataRow> ApplyPatientType(IQueryable<BillingDataRow> rows, BillingDataFilter filter)
    {
        if (!filter.EncounterType.HasValue) return rows;
        var encounterType = filter.EncounterType.Value;

        // Memakai tipe kunjungan (enum), bukan nama ruangan.
        return rows.Where(x => x.Encounter != null && x.Encounter.EncounterType == encounterType);
    }

    private static IQueryable<BillingDataRow> ApplyDateRange(IQueryable<BillingDataRow> rows, BillingDataFilter filter)
    {
        // Tanggal yang dipakai = tanggal kunjungan; bila kunjungan tidak terbaca (item migrasi lama), tanggal
        // pengakuan piutang. Batas bawah inklusif, batas atas eksklusif (awal hari berikutnya WIB), supaya
        // transaksi pukul 23:59:59 tidak hilang.
        if (filter.From.HasValue)
        {
            var fromOffset = filter.From.Value;
            var fromUtc = fromOffset.UtcDateTime;
            rows = rows.Where(x =>
                (x.Encounter != null && x.Encounter.EncounterDate >= fromUtc)
                || (x.Encounter == null && x.Receivable.RecognizedAt >= fromOffset));
        }

        if (filter.ToExclusive.HasValue)
        {
            var toOffset = filter.ToExclusive.Value;
            var toUtc = toOffset.UtcDateTime;
            rows = rows.Where(x =>
                (x.Encounter != null && x.Encounter.EncounterDate < toUtc)
                || (x.Encounter == null && x.Receivable.RecognizedAt < toOffset));
        }

        return rows;
    }

    private IQueryable<BillingDataRow> ApplySearch(IQueryable<BillingDataRow> rows, BillingDataFilter filter)
    {
        if (filter.SearchPattern is null) return rows;
        var pattern = filter.SearchPattern;

        // Kategori company dan internalBenefit juga dicari lewat nama asuransi, nama perusahaan penjamin, dan nama penjamin yang
        // tersimpan pada kunjungan.
        if (filter.Category == BillingDataCategories.Company || filter.Category == BillingDataCategories.InternalBenefit)
        {
            return rows.Where(x =>
                (x.Invoice != null && EF.Functions.ILike(x.Invoice.InvoiceNumber, pattern, "\\"))
                || (x.Encounter != null && EF.Functions.ILike(x.Encounter.EncounterNumber, pattern, "\\"))
                || (x.Patient != null && EF.Functions.ILike(x.Patient.FullName, pattern, "\\"))
                || (x.Patient != null && EF.Functions.ILike(x.Patient.MedicalRecordNumber, pattern, "\\"))
                || (x.Provider != null && EF.Functions.ILike(x.Provider.InsuranceProviderName, pattern, "\\"))
                || _dbContext.MstCompanyGuarantors.Any(c =>
                    !c.IsDelete
                    && (c.Id == x.CompanyGuarantorId || c.Id == x.Receivable.DebtorReferenceId)
                    && EF.Functions.ILike(c.CompanyGuarantorName, pattern, "\\"))
                || _dbContext.RegPatientEncounterGuarantors.Any(g =>
                    g.EncounterId == x.Item.EncounterId
                    && g.IsActive
                    && !g.IsDelete
                    && g.PaymentSourceNameSnapshot != null
                    && EF.Functions.ILike(g.PaymentSourceNameSnapshot, pattern, "\\")));
        }

        if (filter.Category == BillingDataCategories.Employee)
        {
            // Batasi HANYA pada nama karyawan dan No. RM karyawan (bukan No. Bill, No. Reg, atau nama/No. RM pasien keluarga).
            return rows.Where(x =>
                // 1. Nama karyawan dari MstWorkforceProfile / MstEmployee / MstDoctor
                _dbContext.MstWorkforceProfiles.Any(w =>
                    !w.IsDelete
                    && (w.Id == x.Receivable.BenefitOwnerWorkforceProfileId
                        || (x.Receivable.BenefitOwnerId != null && w.Employee != null && w.Employee.Id == x.Receivable.BenefitOwnerId))
                    && (EF.Functions.ILike(w.DisplayName, pattern, "\\")
                        || (w.Employee != null && EF.Functions.ILike(w.Employee.FullName, pattern, "\\"))
                        || (w.Doctor != null && EF.Functions.ILike(w.Doctor.FullName, pattern, "\\"))))
                // Bila piutang SELF: nama pasien adalah nama karyawan
                || ((x.Receivable.BenefitRelationship != null && EF.Functions.ILike(x.Receivable.BenefitRelationship, "SELF"))
                    && x.Patient != null && EF.Functions.ILike(x.Patient.FullName, pattern, "\\"))
                // 2. No. RM karyawan:
                // a. Bila piutang SELF: No. RM pasien pada piutang ini adalah No. RM karyawan
                || ((x.Receivable.BenefitRelationship != null && EF.Functions.ILike(x.Receivable.BenefitRelationship, "SELF"))
                    && x.Patient != null && EF.Functions.ILike(x.Patient.MedicalRecordNumber, pattern, "\\"))
                // b. Kartu SELF karyawan di MstPatientCompanyGuarantor
                || _dbContext.MstPatientCompanyGuarantors.Any(cg =>
                    !cg.IsDelete && !cg.IsCancel && cg.IsActive
                    && cg.BenefitRelationship != null && EF.Functions.ILike(cg.BenefitRelationship, "SELF")
                    && (cg.BenefitOwnerWorkforceProfileId == x.Receivable.BenefitOwnerWorkforceProfileId
                        || (cg.EmployeeId != null && cg.EmployeeId == x.Receivable.BenefitOwnerId))
                    && cg.Patient != null && !cg.Patient.IsDelete
                    && EF.Functions.ILike(cg.Patient.MedicalRecordNumber, pattern, "\\")));
        }

        return rows.Where(x =>
            (x.Invoice != null && EF.Functions.ILike(x.Invoice.InvoiceNumber, pattern, "\\"))
            || (x.Encounter != null && EF.Functions.ILike(x.Encounter.EncounterNumber, pattern, "\\"))
            || (x.Patient != null && EF.Functions.ILike(x.Patient.FullName, pattern, "\\"))
            || (x.Patient != null && EF.Functions.ILike(x.Patient.MedicalRecordNumber, pattern, "\\")));
    }

    // ------------------------------------------------------------------------------------
    // Halaman data
    // ------------------------------------------------------------------------------------

    private async Task<List<BillingDataItemResponse>> LoadPageAsync(
        IQueryable<BillingDataRow> rows, BillingDataFilter filter, CancellationToken cancellationToken)
    {
        var ordered = ApplySort(rows, filter);

        // Proyeksi langsung ke kolom yang dibutuhkan; informasi batch diambil lewat subquery skalar yang hanya
        // dijalankan untuk baris pada halaman ini (maksimal 100), memakai indeks unik ReceivableId.
        var page = await ordered
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new
            {
                ReceivableId = x.Receivable.Id,
                ReceivableItemId = x.Item.Id,
                x.Receivable.ReceivableNumber,
                x.Receivable.RecognizedAt,
                ReceivableStatus = x.Receivable.Status,
                x.Receivable.DebtorType,
                x.Receivable.DebtorReferenceId,
                x.Receivable.IsInternalBenefit,
                x.Receivable.EmployeeArOrigin,
                x.Receivable.BenefitOwnerId,
                x.Receivable.BenefitOwnerWorkforceProfileId,
                x.Receivable.OriginalAmount,
                x.Receivable.OutstandingAmount,
                DueDate = (DateOnly?)x.Receivable.DueDate,
                BillingAmount = x.Item.Amount,
                x.Item.InvoiceId,
                x.Item.EncounterId,
                x.Item.PatientId,
                InvoiceNumber = x.Invoice != null ? x.Invoice.InvoiceNumber : null,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                EncounterDate = x.Encounter != null ? (DateTime?)x.Encounter.EncounterDate : null,
                EncounterCompletedAt = x.Encounter != null ? (DateTime?)x.Encounter.CompletedAt : null,
                EncounterKind = x.Encounter != null ? (EncounterType?)x.Encounter.EncounterType : null,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
                PatientName = x.Patient != null ? x.Patient.FullName : null,
                ProviderName = x.Provider != null ? x.Provider.InsuranceProviderName : null,
                x.CompanyGuarantorId,
                x.EmployeeNumber,
                CompanyName = _dbContext.MstCompanyGuarantors
                    .Where(c => !c.IsDelete && (c.Id == x.CompanyGuarantorId || c.Id == x.Receivable.DebtorReferenceId))
                    .Select(c => c.CompanyGuarantorName)
                    .FirstOrDefault(),
                GuarantorName = _dbContext.RegPatientEncounterGuarantors
                    .Where(g => g.EncounterId == x.Item.EncounterId && g.IsActive && !g.IsDelete)
                    .Select(g => g.PaymentSourceNameSnapshot)
                    .FirstOrDefault(),
                ActiveBatchId = _dbContext.FinReceivableInvoiceBatchItems
                    .Where(i => !i.IsDelete && i.ReceivableId == x.Receivable.Id && i.IsActiveMembership)
                    .Select(i => (Guid?)i.BatchId)
                    .FirstOrDefault(),
                ActiveBatchNumber = _dbContext.FinReceivableInvoiceBatchItems
                    .Where(i => !i.IsDelete && i.ReceivableId == x.Receivable.Id && i.IsActiveMembership)
                    .Select(i => i.Batch!.BatchNumber)
                    .FirstOrDefault(),
                ActiveBatchStatus = _dbContext.FinReceivableInvoiceBatchItems
                    .Where(i => !i.IsDelete && i.ReceivableId == x.Receivable.Id && i.IsActiveMembership)
                    .Select(i => i.Batch!.Status)
                    .FirstOrDefault(),
                AnyBatchMembership = _dbContext.FinReceivableInvoiceBatchItems
                    .Any(i => !i.IsDelete && i.ReceivableId == x.Receivable.Id)
            })
            .ToListAsync(cancellationToken);

        var isCompany = filter.Category == BillingDataCategories.Company;
        var isInternalBenefit = filter.Category == BillingDataCategories.InternalBenefit;

        // Pemilik benefit (karyawan) dibaca lewat resolver HR bersama untuk seluruh halaman sekaligus (tanpa N+1);
        // hanya untuk baris yang memang punya pemilik benefit. Menampilkan pemilik, bukan hanya nama pasien.
        var ownerLookup = await FinanceBenefitOwnerDirectory.LoadAsync(
            _dbContext,
            page.Where(x => x.BenefitOwnerId.HasValue || x.BenefitOwnerWorkforceProfileId.HasValue)
                .Select(x => (x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId)),
            cancellationToken);

        // Status penyelesaian dari DATA SUMBER, dimuat sekali untuk seluruh halaman (tanpa N+1) dan hanya untuk jenis
        // piutang yang relevan: Penutupan Benefit untuk Benefit RS Internal, perjanjian angsuran untuk Karyawan.
        var benefitReceivableIds = page.Where(x => x.IsInternalBenefit).Select(x => x.ReceivableId).Distinct().ToList();
        var settlementByReceivable = new Dictionary<Guid, (Guid SettlementId, string Number, string Status)>();
        if (benefitReceivableIds.Count > 0)
        {
            var settlementRows = await _dbContext.FinBenefitSettlementItems.AsNoTracking()
                .Where(i => !i.IsDelete
                    && benefitReceivableIds.Contains(i.ReceivableId)
                    && i.Settlement != null
                    && !i.Settlement.IsDelete
                    && i.Settlement.Status != FinBenefitSettlementStatuses.Dibatalkan)
                .Select(i => new
                {
                    i.ReceivableId,
                    i.SettlementId,
                    i.Settlement!.SettlementNumber,
                    i.Settlement.Status
                })
                .ToListAsync(cancellationToken);

            // Satu piutang paling banyak masuk satu Penutupan yang tidak dibatalkan (ditegakkan service penutupan);
            // bila data menyimpang, yang DITERBITKAN didahulukan atas DRAF.
            foreach (var row in settlementRows
                         .OrderBy(r => r.Status == FinBenefitSettlementStatuses.Diterbitkan ? 0 : 1))
            {
                settlementByReceivable.TryAdd(row.ReceivableId, (row.SettlementId, row.SettlementNumber, row.Status));
            }
        }

        var employeeReceivableIds = page
            .Where(x => x.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit)
            .Select(x => x.ReceivableId).Distinct().ToList();
        var planByReceivable = new Dictionary<Guid, (Guid PlanId, string Number, string Status)>();
        if (employeeReceivableIds.Count > 0)
        {
            var planRows = await _dbContext.FinReceivableInstallmentPlans.AsNoTracking()
                .Where(p => !p.IsDelete
                    && employeeReceivableIds.Contains(p.ReceivableId)
                    && (p.Status == FinReceivableInstallmentPlanStatuses.Menunggu
                        || p.Status == FinReceivableInstallmentPlanStatuses.Disetujui
                        || p.Status == FinReceivableInstallmentPlanStatuses.Selesai))
                .Select(p => new { p.ReceivableId, p.Id, p.PlanNumber, p.Status, p.RequestedAt })
                .ToListAsync(cancellationToken);

            // DISETUJUI mengalahkan MENUNGGU, lalu SELESAI; sesama status, yang terbaru.
            foreach (var row in planRows
                         .OrderBy(r => PlanPriority(r.Status))
                         .ThenByDescending(r => r.RequestedAt))
            {
                planByReceivable.TryAdd(row.ReceivableId, (row.Id, row.PlanNumber, row.Status));
            }
        }

        return page.Select(x =>
        {
            var owner = ownerLookup.Find(x.BenefitOwnerId, x.BenefitOwnerWorkforceProfileId);
            settlementByReceivable.TryGetValue(x.ReceivableId, out var settlement);
            planByReceivable.TryGetValue(x.ReceivableId, out var plan);
            var hasSettlement = settlementByReceivable.ContainsKey(x.ReceivableId);
            var hasPlan = planByReceivable.ContainsKey(x.ReceivableId);
            var billingDate = x.EncounterDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(x.EncounterDate.Value, DateTimeKind.Utc))
                : x.RecognizedAt;

            var encounterStartAt = x.EncounterDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(x.EncounterDate.Value, DateTimeKind.Utc))
                : (DateTimeOffset?)x.RecognizedAt;

            var encounterEndAt = x.EncounterCompletedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(x.EncounterCompletedAt.Value, DateTimeKind.Utc))
                : (DateTimeOffset?)null;

            var blockedReason = ResolveCreateBlockedReason(
                x.DebtorType, x.DebtorReferenceId, x.ActiveBatchId.HasValue, x.AnyBatchMembership, x.ReceivableStatus,
                x.IsInternalBenefit, hasSettlement);

            return new BillingDataItemResponse
            {
                Id = x.ReceivableId,
                ReceivableItemId = x.ReceivableItemId,
                ReceivableNumber = x.ReceivableNumber,
                BillingDate = billingDate,
                EncounterStartAt = encounterStartAt,
                EncounterEndAt = encounterEndAt,
                OriginalAmount = x.OriginalAmount,
                OutstandingAmount = x.OutstandingAmount,
                DueDate = x.DueDate,
                InvoiceNumber = x.InvoiceNumber,
                EncounterNumber = x.EncounterNumber,
                MedicalRecordNumber = x.MedicalRecordNumber,
                PatientName = x.PatientName,
                EmployeeId = x.EmployeeNumber,
                PatientType = MapPatientType(x.EncounterKind),
                InvoiceId = x.InvoiceId,
                EncounterId = x.EncounterId,
                PatientId = x.PatientId,
                DebtorType = x.DebtorType,
                DebtorReferenceId = x.DebtorReferenceId,
                IsInternalBenefit = x.IsInternalBenefit,
                EmployeeArOrigin = x.EmployeeArOrigin,
                BenefitOwnerName = owner?.StaffName,
                BenefitOwnerStaffNumber = owner?.StaffNumber,
                PayerName = (isCompany || isInternalBenefit) ? (x.ProviderName ?? x.CompanyName ?? x.GuarantorName ?? (isInternalBenefit ? "Benefit RS Internal" : null)) : x.PatientName,
                PayerId = (isCompany || isInternalBenefit) ? (x.DebtorReferenceId ?? x.CompanyGuarantorId) : null,
                PayerKind = (!isCompany && !isInternalBenefit)
                    ? null
                    : x.ProviderName != null
                        ? BillingDataPayerKinds.Insurance
                        : x.CompanyName != null ? BillingDataPayerKinds.Company : null,
                BillingAmount = x.BillingAmount,
                ReceivableStatus = x.ReceivableStatus,
                BillingStatus = x.ActiveBatchId.HasValue ? BillingDataStatuses.Created : BillingDataStatuses.NotCreated,
                // "Belum Dibuat/Sudah Dibuat" berlaku untuk PAYER (baik Perusahaan/Asuransi maupun Benefit RS Internal).
                BillingStatusApplicable = x.DebtorType == FinReceivableDebtorTypes.Payer,
                ResolutionStatus = ResolveResolutionStatus(
                    x.DebtorType, x.IsInternalBenefit, x.ReceivableStatus, x.ActiveBatchId.HasValue,
                    hasSettlement ? settlement.Status : null, hasPlan ? plan.Status : null),
                RecognizedAt = x.RecognizedAt,
                SettlementId = hasSettlement ? settlement.SettlementId : null,
                SettlementNumber = hasSettlement ? settlement.Number : null,
                SettlementStatus = hasSettlement ? settlement.Status : null,
                // Batas bulan UTC, sama dengan FinanceBenefitSettlementService.ParsePeriod. Jangan diganti WIB atau tanggal invoice.
                SettlementPeriodCode = x.IsInternalBenefit
                    ? x.RecognizedAt.UtcDateTime.ToString("yyyy-MM", CultureInfo.InvariantCulture)
                    : null,
                InstallmentPlanId = hasPlan ? plan.PlanId : null,
                InstallmentPlanNumber = hasPlan ? plan.Number : null,
                InstallmentPlanStatus = hasPlan ? plan.Status : null,
                InvoiceBatchId = x.ActiveBatchId,
                InvoiceBatchNumber = x.ActiveBatchNumber,
                InvoiceBatchStatus = x.ActiveBatchStatus,
                CanCreateInvoice = blockedReason is null,
                CreateBlockedReason = blockedReason
            };
        }).ToList();
    }

    private static int PlanPriority(string status) => status switch
    {
        FinReceivableInstallmentPlanStatuses.Disetujui => 0,
        FinReceivableInstallmentPlanStatuses.Menunggu => 1,
        _ => 2
    };

    // Status penyelesaian menurut data sumber. Urutan prioritasnya HARUS sama dengan ApplyResolutionStatus (saringan).
    private static string ResolveResolutionStatus(
        string debtorType, bool isInternalBenefit, string receivableStatus, bool inActiveBatch,
        string? settlementStatus, string? planStatus)
    {
        var settled = receivableStatus == FinReceivableStatuses.Settled;

        if (debtorType == FinReceivableDebtorTypes.EmployeeBenefit)
        {
            if (settled) return BillingDataResolutionStatuses.Paid;
            if (planStatus == FinReceivableInstallmentPlanStatuses.Disetujui) return BillingDataResolutionStatuses.PlanApproved;
            if (planStatus == FinReceivableInstallmentPlanStatuses.Menunggu) return BillingDataResolutionStatuses.PlanPending;
            return BillingDataResolutionStatuses.NoPlan;
        }

        if (debtorType == FinReceivableDebtorTypes.Payer && isInternalBenefit)
        {
            if (settled) return BillingDataResolutionStatuses.Settled;
            return settlementStatus == FinBenefitSettlementStatuses.Draf
                ? BillingDataResolutionStatuses.SettlementDraft
                : BillingDataResolutionStatuses.AwaitingSettlement;
        }

        if (debtorType == FinReceivableDebtorTypes.Payer)
            return inActiveBatch ? BillingDataResolutionStatuses.Invoiced : BillingDataResolutionStatuses.AwaitingInvoice;

        return settled ? BillingDataResolutionStatuses.Paid : BillingDataResolutionStatuses.Outstanding;
    }

    private static IQueryable<BillingDataRow> ApplySort(IQueryable<BillingDataRow> rows, BillingDataFilter filter)
    {
        var descending = filter.Descending;
        return filter.SortBy switch
        {
            "invoicenumber" => descending
                ? rows.OrderByDescending(x => x.Invoice != null ? x.Invoice.InvoiceNumber : "").ThenBy(x => x.Item.Id)
                : rows.OrderBy(x => x.Invoice != null ? x.Invoice.InvoiceNumber : "").ThenBy(x => x.Item.Id),
            "encounternumber" => descending
                ? rows.OrderByDescending(x => x.Encounter != null ? x.Encounter.EncounterNumber : "").ThenBy(x => x.Item.Id)
                : rows.OrderBy(x => x.Encounter != null ? x.Encounter.EncounterNumber : "").ThenBy(x => x.Item.Id),
            "medicalrecordnumber" => descending
                ? rows.OrderByDescending(x => x.Patient != null ? x.Patient.MedicalRecordNumber : "").ThenBy(x => x.Item.Id)
                : rows.OrderBy(x => x.Patient != null ? x.Patient.MedicalRecordNumber : "").ThenBy(x => x.Item.Id),
            "patientname" => descending
                ? rows.OrderByDescending(x => x.Patient != null ? x.Patient.FullName : "").ThenBy(x => x.Item.Id)
                : rows.OrderBy(x => x.Patient != null ? x.Patient.FullName : "").ThenBy(x => x.Item.Id),
            "amount" => descending
                ? rows.OrderByDescending(x => x.Item.Amount).ThenBy(x => x.Item.Id)
                : rows.OrderBy(x => x.Item.Amount).ThenBy(x => x.Item.Id),
            // Bawaan ("grouped" / "date"): Urutan stabil seperti V1.
            // 1. Group alphabetical (Payer untuk Company, Patient untuk Employee dan GeneralPatient)
            // 2. Dalam group: Tanggal DESC (kunjungan / diakui)
            // 3. NoBill ASC
            // 4. Item.Id ASC
            _ => (filter.Category == BillingDataCategories.Company || filter.Category == BillingDataCategories.InternalBenefit)
                ? rows.OrderBy(x => x.Provider != null ? x.Provider.InsuranceProviderName : (x.CompanyName ?? ""))
                    .ThenBy(x => x.Encounter == null)
                    .ThenByDescending(x => x.Encounter != null ? (DateTime?)x.Encounter.EncounterDate : null)
                    .ThenByDescending(x => x.Receivable.RecognizedAt)
                    .ThenBy(x => x.Invoice != null ? x.Invoice.InvoiceNumber : "")
                    .ThenBy(x => x.Item.Id)
                : rows.OrderBy(x => x.Patient != null ? x.Patient.FullName : "")
                    .ThenBy(x => x.Encounter == null)
                    .ThenByDescending(x => x.Encounter != null ? (DateTime?)x.Encounter.EncounterDate : null)
                    .ThenByDescending(x => x.Receivable.RecognizedAt)
                    .ThenBy(x => x.Invoice != null ? x.Invoice.InvoiceNumber : "")
                    .ThenBy(x => x.Item.Id)
        };
    }

    /// <summary>
    /// Membangun kelompok tagihan (BillingDataGroupResponse) untuk halaman aktif, menghitung subtotal per tanggal,
    /// dan menghitung groupTotal final dari SELURUH DATA FILTERED di database.
    /// </summary>
    private async Task<List<BillingDataGroupResponse>> BuildGroupsAsync(
        IQueryable<BillingDataRow> rows,
        List<BillingDataItemResponse> pageRows,
        BillingDataFilter filter,
        int totalFilteredCount,
        decimal totalFilteredAmount,
        CancellationToken cancellationToken)
    {
        if (pageRows.Count == 0) return new List<BillingDataGroupResponse>();

        // Tentukan kunci group untuk setiap baris di halaman aktif
        var groupedItems = new List<(string GroupId, string GroupName, BillingDataGroupMeta Meta, BillingDataItemResponse Item)>();

        foreach (var item in pageRows)
        {
            string groupId;
            string groupName;
            BillingDataGroupMeta meta;

            switch (filter.Category)
            {
                case BillingDataCategories.Employee:
                    groupId = !string.IsNullOrWhiteSpace(item.EmployeeId)
                        ? item.EmployeeId.Trim()
                        : (item.PatientId.HasValue
                            ? item.PatientId.Value.ToString()
                            : (!string.IsNullOrWhiteSpace(item.MedicalRecordNumber)
                                ? item.MedicalRecordNumber.Trim()
                                : (!string.IsNullOrWhiteSpace(item.PatientName) ? item.PatientName.Trim() : "-")));
                    groupName = !string.IsNullOrWhiteSpace(item.PatientName) ? item.PatientName.Trim() : "Karyawan";
                    meta = new BillingDataGroupMeta
                    {
                        NoRM = item.MedicalRecordNumber,
                        EmployeeId = item.EmployeeId,
                        BenefitOwnerName = item.BenefitOwnerName,
                        BenefitOwnerStaffNumber = item.BenefitOwnerStaffNumber
                    };
                    break;

                case BillingDataCategories.GeneralPatient:
                    groupId = item.PatientId.HasValue
                        ? item.PatientId.Value.ToString()
                        : (!string.IsNullOrWhiteSpace(item.MedicalRecordNumber)
                            ? item.MedicalRecordNumber.Trim()
                            : (!string.IsNullOrWhiteSpace(item.PatientName) ? item.PatientName.Trim() : "-"));
                    groupName = !string.IsNullOrWhiteSpace(item.PatientName) ? item.PatientName.Trim() : "Pasien";
                    meta = new BillingDataGroupMeta
                    {
                        NoRM = item.MedicalRecordNumber,
                        EmployeeId = null,
                        BenefitOwnerName = null,
                        BenefitOwnerStaffNumber = null
                    };
                    break;

                case BillingDataCategories.InternalBenefit:
                    groupId = item.PayerId.HasValue
                        ? item.PayerId.Value.ToString()
                        : (!string.IsNullOrWhiteSpace(item.PayerName) ? item.PayerName.Trim() : "-");
                    groupName = !string.IsNullOrWhiteSpace(item.PayerName) ? item.PayerName.Trim() : "Benefit RS Internal";
                    meta = new BillingDataGroupMeta
                    {
                        NoRM = null,
                        EmployeeId = null,
                        BenefitOwnerName = item.BenefitOwnerName,
                        BenefitOwnerStaffNumber = item.BenefitOwnerStaffNumber
                    };
                    break;

                default: // Company
                    groupId = item.PayerId.HasValue
                        ? item.PayerId.Value.ToString()
                        : (!string.IsNullOrWhiteSpace(item.PayerName) ? item.PayerName.Trim() : "-");
                    groupName = !string.IsNullOrWhiteSpace(item.PayerName) ? item.PayerName.Trim() : "-";
                    meta = new BillingDataGroupMeta
                    {
                        NoRM = null,
                        EmployeeId = null
                    };
                    break;
            }

            groupedItems.Add((groupId, groupName, meta, item));
        }

        // Urutkan grup secara alfabetis sesuai aturan V1 (locale id / OrdinalIgnoreCase)
        var groupBuckets = groupedItems
            .GroupBy(x => x.GroupId)
            .OrderBy(g => g.First().GroupName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Hitung groupTotal, itemCount, dan patientCount dari SELURUH DATA FILTERED (bukan hanya halaman aktif)
        var isSinglePageAllData = totalFilteredCount <= pageRows.Count;
        var isSingleEntitySelected = filter.EntityId.HasValue;

        var fullGroupTotals = new Dictionary<string, (decimal TotalAmount, int ItemCount, int PatientCount)>(StringComparer.OrdinalIgnoreCase);

        if (isSinglePageAllData)
        {
            // Seluruh data filtered sudah ada di pageRows, hitung langsung tanpa query tambahan
            foreach (var g in groupBuckets)
            {
                var totalAmount = g.Sum(x => x.Item.BillingAmount);
                var itemCount = g.Count();
                var patientCount = g.Select(x => x.Item.PatientId).Where(p => p != null).Distinct().Count();
                fullGroupTotals[g.Key] = (totalAmount, itemCount, patientCount);
            }
        }
        else if (isSingleEntitySelected && groupBuckets.Count == 1)
        {
            // Satu entitas terpilih dan hanya 1 grup: total group sama dengan summary grand total
            fullGroupTotals[groupBuckets[0].Key] = (totalFilteredAmount, totalFilteredCount,
                groupBuckets[0].Select(x => x.Item.PatientId).Where(p => p != null).Distinct().Count());
        }
        else
        {
            // Multiple groups across pages: hitung agregasi dari database tanpa N+1 query
            if (filter.Category == BillingDataCategories.Company || filter.Category == BillingDataCategories.InternalBenefit)
            {
                var payerGuids = pageRows
                    .Where(x => x.PayerId.HasValue)
                    .Select(x => x.PayerId!.Value)
                    .Distinct()
                    .ToList();

                if (payerGuids.Count > 0)
                {
                    var payerAggs = await rows
                        .Where(x => (x.Receivable.DebtorReferenceId != null && payerGuids.Contains(x.Receivable.DebtorReferenceId.Value))
                            || (x.Receivable.DebtorReferenceId == null && x.CompanyGuarantorId != null && payerGuids.Contains(x.CompanyGuarantorId.Value)))
                        .GroupBy(x => (Guid?)(x.Receivable.DebtorReferenceId ?? x.CompanyGuarantorId))
                        .Select(g => new
                        {
                            PayerId = g.Key,
                            TotalAmount = g.Sum(x => x.Item.Amount),
                            ItemCount = g.Count(),
                            PatientCount = g.Where(x => x.Item.PatientId != null).Select(x => x.Item.PatientId).Distinct().Count()
                        })
                        .ToListAsync(cancellationToken);

                    foreach (var agg in payerAggs)
                    {
                        if (agg.PayerId.HasValue)
                        {
                            fullGroupTotals[agg.PayerId.Value.ToString()] = (agg.TotalAmount, agg.ItemCount, agg.PatientCount);
                        }
                    }
                }
            }
            else
            {
                // Employee atau GeneralPatient: agregasi berbasis PatientId
                var patientGuids = pageRows
                    .Where(x => x.PatientId.HasValue)
                    .Select(x => x.PatientId!.Value)
                    .Distinct()
                    .ToList();

                if (patientGuids.Count > 0)
                {
                    var patientAggs = await rows
                        .Where(x => x.Item.PatientId != null && patientGuids.Contains(x.Item.PatientId.Value))
                        .GroupBy(x => x.Item.PatientId)
                        .Select(g => new
                        {
                            PatientId = g.Key,
                            TotalAmount = g.Sum(x => x.Item.Amount),
                            ItemCount = g.Count(),
                            PatientCount = 1
                        })
                        .ToListAsync(cancellationToken);

                    foreach (var agg in patientAggs)
                    {
                        if (agg.PatientId.HasValue)
                        {
                            fullGroupTotals[agg.PatientId.Value.ToString()] = (agg.TotalAmount, agg.ItemCount, agg.PatientCount);
                        }
                    }
                }
            }
        }

        var resultGroups = new List<BillingDataGroupResponse>();

        foreach (var g in groupBuckets)
        {
            var first = g.First();
            var itemsInGroup = g.Select(x => x.Item)
                .OrderByDescending(x => x.BillingDate)
                .ThenBy(x => x.InvoiceNumber ?? "")
                .ThenBy(x => x.Id)
                .ToList();

            // DateTotals: subtotal billing amount per tanggal kalender (yyyy-MM-dd) di dalam group yang sama
            var dateTotals = itemsInGroup
                .GroupBy(x => x.BillingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
                .OrderByDescending(dg => dg.Key)
                .Select(dg => new BillingDataDateTotalResponse
                {
                    Date = dg.Key,
                    Total = dg.Sum(x => x.BillingAmount)
                })
                .ToList();

            // Ambil groupTotal penuh; jika fallback belum ada di fullGroupTotals, gunakan sum item
            var (fullTotal, fullItemCount, fullPatientCount) = fullGroupTotals.TryGetValue(g.Key, out var aggVal)
                ? aggVal
                : (itemsInGroup.Sum(x => x.BillingAmount), itemsInGroup.Count, itemsInGroup.Select(x => x.PatientId).Where(p => p != null).Distinct().Count());

            resultGroups.Add(new BillingDataGroupResponse
            {
                GroupId = g.Key,
                GroupName = first.GroupName,
                GroupMeta = first.Meta,
                Items = itemsInGroup,
                DateTotals = dateTotals,
                GroupTotal = fullTotal,
                ItemCount = fullItemCount,
                PatientCount = fullPatientCount
            });
        }

        return resultGroups;
    }

    // Petunjuk tampilan untuk tombol "Buat Tagihan". Aturannya sejajar dengan POST receivable-invoice-batches
    // (FinanceReceivableInvoiceBatchService.CreateAsync: OUTSTANDING/PARTIAL/SETTLED boleh ditagihkan), ditambah
    // keadaan nyata di database: anggota dari batch yang sudah CANCELLED masih menempati indeks unik
    // IX_FinReceivableInvoiceBatchItem_ActiveReceivable (batch yang dibatalkan tidak menghapus baris anggotanya),
    // sehingga belum bisa digabung ulang.
    private static string? ResolveCreateBlockedReason(
        string debtorType, Guid? debtorReferenceId, bool inActiveBatch, bool anyMembership, string receivableStatus,
        bool isInternalBenefit, bool hasActiveSettlement = false)
    {
        if (debtorType != FinReceivableDebtorTypes.Payer) return BillingDataCreateBlockedReasons.NotPayer;
        if (hasActiveSettlement) return BillingDataCreateBlockedReasons.ActiveSettlement;
        if (debtorReferenceId is null) return BillingDataCreateBlockedReasons.NoDebtorReference;
        if (inActiveBatch) return BillingDataCreateBlockedReasons.AlreadyInBatch;
        if (anyMembership) return BillingDataCreateBlockedReasons.InCancelledBatch;
        if (receivableStatus != FinReceivableStatuses.Outstanding
            && receivableStatus != FinReceivableStatuses.Partial
            && receivableStatus != FinReceivableStatuses.Settled)
            return BillingDataCreateBlockedReasons.StatusNotEligible;
        return null;
    }

    private static string? MapPatientType(EncounterType? encounterType) => encounterType switch
    {
        null => null,
        EncounterType.Outpatient => BillingDataPatientTypes.Outpatient,
        EncounterType.Inpatient => BillingDataPatientTypes.Inpatient,
        EncounterType.Emergency => BillingDataPatientTypes.Emergency,
        _ => BillingDataPatientTypes.Other
    };

    // ------------------------------------------------------------------------------------
    // Validasi dan penerjemahan parameter
    // ------------------------------------------------------------------------------------

    private async Task<string> ResolveEntityNameAsync(BillingDataFilter filter, CancellationToken cancellationToken)
    {
        if (!filter.EntityId.HasValue)
        {
            return filter.Category switch
            {
                BillingDataCategories.Company => "Semua Perusahaan",
                BillingDataCategories.Employee => "Semua Karyawan",
                BillingDataCategories.InternalBenefit => "Semua Penjamin Internal",
                _ => "Semua Pasien"
            };
        }

        var entityId = filter.EntityId.Value;
        if (filter.Category == BillingDataCategories.Company || filter.Category == BillingDataCategories.InternalBenefit)
        {
            // Pilihan penjamin = asuransi atau perusahaan penjamin; Id keduanya berasal dari tabel master berbeda.
            var insuranceName = await _dbContext.MstInsuranceProviders.AsNoTracking()
                .Where(x => x.Id == entityId && !x.IsDelete)
                .Select(x => x.InsuranceProviderName)
                .FirstOrDefaultAsync(cancellationToken);
            if (insuranceName is not null) return insuranceName;

            return await _dbContext.MstCompanyGuarantors.AsNoTracking()
                    .Where(x => x.Id == entityId && !x.IsDelete)
                    .Select(x => x.CompanyGuarantorName)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Penjamin tidak ditemukan.");
        }

        if (filter.Category == BillingDataCategories.Employee)
        {
            var profile = await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(w => w.Id == entityId && !w.IsDelete)
                .Select(w => new
                {
                    w.DisplayName,
                    EmployeeName = w.Employee != null ? w.Employee.FullName : null,
                    DoctorName = w.Doctor != null ? w.Doctor.FullName : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (profile != null)
            {
                return !string.IsNullOrWhiteSpace(profile.EmployeeName)
                    ? profile.EmployeeName
                    : (!string.IsNullOrWhiteSpace(profile.DoctorName) ? profile.DoctorName : profile.DisplayName);
            }

            var employeeName = await _dbContext.MstEmployees.AsNoTracking()
                .Where(e => e.Id == entityId && !e.IsDelete)
                .Select(e => e.FullName)
                .FirstOrDefaultAsync(cancellationToken);

            if (employeeName != null) return employeeName;

            throw new KeyNotFoundException("Karyawan tidak ditemukan.");
        }

        return await _dbContext.MstPatients.AsNoTracking()
                .Where(x => x.Id == entityId && !x.IsDelete)
                .Select(x => x.FullName)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Pasien tidak ditemukan.");
    }

    private static BillingDataFilter ResolveFilter(BillingDataQuery request)
    {
        var category = Canonical(request.Category, BillingDataCategories.Company, "category",
            BillingDataCategories.Company, BillingDataCategories.Employee, BillingDataCategories.GeneralPatient,
            BillingDataCategories.InternalBenefit);
        var status = Canonical(request.BillingStatus, BillingDataStatuses.All, "billingStatus",
            BillingDataStatuses.All, BillingDataStatuses.NotCreated, BillingDataStatuses.Created);
        var patientType = Canonical(request.PatientType, BillingDataPatientTypes.All, "patientType",
            BillingDataPatientTypes.All, BillingDataPatientTypes.Outpatient, BillingDataPatientTypes.Inpatient,
            BillingDataPatientTypes.Emergency);
        var period = Canonical(request.Period, BillingDataPeriods.All, "period",
            BillingDataPeriods.All, BillingDataPeriods.Today, BillingDataPeriods.Yesterday,
            BillingDataPeriods.ThisWeek, BillingDataPeriods.ThisMonth, BillingDataPeriods.LastMonth);

        var (rangeFrom, rangeToExclusive, periodLabel) = ResolvePeriod(request, period);

        // resolutionStatus (opsional) hanya menerima nilai milik kategorinya. Kategori company/generalPatient tidak punya
        // status penyelesaian sumber; saringan keanggotaan Batch Tagihan untuk company tetap lewat billingStatus.
        var resolutionAllowed = category switch
        {
            BillingDataCategories.InternalBenefit => new[]
            {
                BillingDataResolutionStatuses.All, BillingDataResolutionStatuses.AwaitingSettlement,
                BillingDataResolutionStatuses.SettlementDraft, BillingDataResolutionStatuses.Settled
            },
            BillingDataCategories.Employee => new[]
            {
                BillingDataResolutionStatuses.All, BillingDataResolutionStatuses.NoPlan,
                BillingDataResolutionStatuses.PlanPending, BillingDataResolutionStatuses.PlanApproved,
                BillingDataResolutionStatuses.Paid
            },
            _ => new[] { BillingDataResolutionStatuses.All }
        };
        var resolutionStatus = Canonical(request.ResolutionStatus, BillingDataResolutionStatuses.All, "resolutionStatus",
            resolutionAllowed);

        var search = request.Search?.Trim();
        return new BillingDataFilter
        {
            ResolutionStatus = resolutionStatus,
            Category = category,
            DebtorType = category switch
            {
                BillingDataCategories.Employee => FinReceivableDebtorTypes.EmployeeBenefit,
                BillingDataCategories.GeneralPatient => FinReceivableDebtorTypes.PatientGuarantor,
                _ => FinReceivableDebtorTypes.Payer
            },
            EntityId = request.EntityId == Guid.Empty ? null : request.EntityId,
            BillingStatus = status,
            EncounterType = patientType switch
            {
                BillingDataPatientTypes.Outpatient => EncounterType.Outpatient,
                BillingDataPatientTypes.Inpatient => EncounterType.Inpatient,
                BillingDataPatientTypes.Emergency => EncounterType.Emergency,
                _ => null
            },
            From = rangeFrom,
            ToExclusive = rangeToExclusive,
            PeriodLabel = periodLabel,
            SearchPattern = string.IsNullOrEmpty(search) ? null : BuildContainsPattern(search),
            SortBy = (request.SortBy ?? "date").Trim().ToLowerInvariant(),
            Descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase),
            PageNumber = Math.Max(1, request.PageNumber),
            PageSize = Math.Clamp(request.PageSize, 1, 1000)
        };
    }

    // Aturan prioritas: bila StartDate/EndDate dikirim, rentang eksplisit itulah yang dipakai dan Period
    // diabaikan. Selain itu, Period yang dipakai. Hari dan batas minggu/bulan mengikuti kalender WIB
    // (FinanceBusinessDate). Minggu dimulai hari Minggu, sama dengan V1.
    private static (DateTimeOffset? From, DateTimeOffset? ToExclusive, string Label) ResolvePeriod(
        BillingDataQuery request, string period)
    {
        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate.Value > request.EndDate.Value)
                throw new BillingDataBadRequestException("Tanggal mulai tidak boleh setelah tanggal akhir.");

            DateTimeOffset? rangeFrom = request.StartDate.HasValue
                ? FinanceBusinessDate.GetStartOfDayUtc(request.StartDate.Value)
                : null;
            DateTimeOffset? rangeToExclusive = request.EndDate.HasValue
                ? FinanceBusinessDate.GetEndOfDayUtc(request.EndDate.Value)
                : null;

            var label = request.StartDate.HasValue && request.EndDate.HasValue
                ? $"{FormatDate(request.StartDate.Value)} - {FormatDate(request.EndDate.Value)}"
                : request.StartDate.HasValue
                    ? $"Mulai {FormatDate(request.StartDate.Value)}"
                    : $"Sampai {FormatDate(request.EndDate!.Value)}";
            return (rangeFrom, rangeToExclusive, label);
        }

        var today = FinanceBusinessDate.Today();
        switch (period)
        {
            case BillingDataPeriods.Today:
                return (FinanceBusinessDate.GetStartOfDayUtc(today), FinanceBusinessDate.GetStartOfDayUtc(today.AddDays(1)), "Hari Ini");
            case BillingDataPeriods.Yesterday:
                return (FinanceBusinessDate.GetStartOfDayUtc(today.AddDays(-1)), FinanceBusinessDate.GetStartOfDayUtc(today), "Kemarin");
            case BillingDataPeriods.ThisWeek:
                var weekStart = today.AddDays(-(int)today.DayOfWeek);
                return (FinanceBusinessDate.GetStartOfDayUtc(weekStart), FinanceBusinessDate.GetStartOfDayUtc(weekStart.AddDays(7)), "Minggu Ini");
            case BillingDataPeriods.ThisMonth:
                var monthStart = new DateOnly(today.Year, today.Month, 1);
                return (FinanceBusinessDate.GetStartOfDayUtc(monthStart), FinanceBusinessDate.GetStartOfDayUtc(monthStart.AddMonths(1)), "Bulan Ini");
            case BillingDataPeriods.LastMonth:
                var thisMonthStart = new DateOnly(today.Year, today.Month, 1);
                return (FinanceBusinessDate.GetStartOfDayUtc(thisMonthStart.AddMonths(-1)), FinanceBusinessDate.GetStartOfDayUtc(thisMonthStart), "Bulan Lalu");
            default:
                return (null, null, "Semua Periode");
        }
    }

    private static string FormatDate(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static string Canonical(string? value, string defaultValue, string parameterName, params string[] allowed)
    {
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        var match = allowed.FirstOrDefault(x => string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? throw new BillingDataBadRequestException(
            $"Nilai {parameterName} '{value.Trim()}' tidak dikenal. Nilai yang diterima: {string.Join(", ", allowed)}.");
    }

    // Pola ILIKE "mengandung teks": karakter khusus pola di-escape dengan backslash, lalu dibungkus %.
    // Dipanggil bersama EF.Functions.ILike(kolom, pola, "\\").
    private static string BuildContainsPattern(string search)
    {
        var escaped = search
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
        return $"%{escaped}%";
    }

    private sealed class BillingDataFilter
    {
        public string Category { get; init; } = BillingDataCategories.Company;
        public string DebtorType { get; init; } = FinReceivableDebtorTypes.Payer;
        public Guid? EntityId { get; init; }
        public string BillingStatus { get; init; } = BillingDataStatuses.All;
        public string ResolutionStatus { get; init; } = BillingDataResolutionStatuses.All;
        public EncounterType? EncounterType { get; init; }
        public DateTimeOffset? From { get; init; }
        public DateTimeOffset? ToExclusive { get; init; }
        public string PeriodLabel { get; init; } = "Semua Periode";
        public string? SearchPattern { get; init; }
        public string SortBy { get; init; } = "date";
        public bool Descending { get; init; } = true;
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 25;
    }
}

/// <summary>Baris gabungan internal untuk query Data Tagihan. Internal agar dapat dipakai sebagai tipe proyeksi EF.</summary>
internal sealed class BillingDataRow
{
    public FinReceivableItem Item { get; set; } = null!;
    public FinReceivable Receivable { get; set; } = null!;
    public BilInvoice? Invoice { get; set; }
    public RegPatientEncounter? Encounter { get; set; }
    public MstPatient? Patient { get; set; }
    public MstInsuranceProvider? Provider { get; set; }

    /// <summary>Perusahaan penjamin dari penjamin aktif pada kunjungan; null bila tidak ada.</summary>
    public Guid? CompanyGuarantorId { get; set; }

    /// <summary>Nomor identitas/NIP karyawan pasien bila tersedia pada penjamin kunjungan.</summary>
    public string? EmployeeNumber { get; set; }
}

/// <summary>Parameter tidak valid (kategori, status, jenis pasien, periode, atau rentang tanggal) → 400.</summary>
public sealed class BillingDataBadRequestException(string message) : Exception(message);
