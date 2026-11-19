using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Layanan monitoring READ-ONLY Data Penggajian Pegawai (HRD): menampilkan kelengkapan penetapan gaji utama
    /// (WfpSalaryAssignment) dan rekening payroll (WfpBankAccount) seluruh karyawan, perawat, dan dokter aktif
    /// dari Master Karyawan (MstEmployee) dan Master Dokter (MstDoctor) melalui penghubung WorkforceProfileId.
    ///
    /// Aturan pemilihan penetapan gaji berlaku, rekening payroll berlaku, dan penilaian kelengkapan HRD diselaraskan
    /// dengan <see cref="PayrollHrdSourceReader"/> dan <see cref="PayrollReadinessEvaluator"/> agar hasil monitoring
    /// HRD selalu konsisten dengan Kelengkapan Data Payroll, Profil Penggajian Payroll, dan Persiapan Payroll Run.
    ///
    /// Layanan ini murni baca (seluruh query memakai AsNoTracking dan tidak pernah memanggil SaveChanges).
    /// Perubahan penetapan gaji, persetujuan gaji, pendaftaran rekening, dan verifikasi rekening hanya dilakukan
    /// melalui endpoint master HRD (WfpSalaryAssignmentController dan WfpBankAccountController).
    /// Nominal gaji hanya dikirim bila pemanggil berwenang ViewAmount (null bila tidak berwenang atau belum tersedia),
    /// dan nomor rekening selalu disamarkan di Backend.
    /// </summary>
    public sealed class WorkforcePayrollDataService
    {
        private const string PersonEmployee = "Karyawan";
        private const string PersonNurse = "Perawat";
        private const string PersonDoctor = "Dokter";
        private const string CompleteFilter = "Lengkap";
        private const string IncompleteFilter = "Belum lengkap";

        private const string SalaryApprovedLabel = "Berlaku";
        private const string SalaryPendingApproval = "Menunggu Persetujuan HRD";
        private const string SalaryConflictLabel = "Konflik Penetapan Gaji";
        private const string SalaryNotYetEffective = "Terjadwal";
        private const string SalaryExpiredLabel = "Berakhir";
        private const string SalaryNoneLabel = "Belum Ada";

        private const string BankNotYetEffective = "Belum berlaku";
        private const string BankExpiredLabel = "Kedaluwarsa";

        private const string NoSalaryMasterMessage = "Belum ada penetapan gaji pada Master HRD.";
        private const string NoBankMasterMessage = "Belum ada rekening payroll pada Master HRD.";
        private const string DefaultPayrollNoBankMessage = "Belum ada rekening payroll di HRD.";

        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;
        private readonly PayrollHrdSourceReader _hrdSource;

        public WorkforcePayrollDataService(ApplicationDbContext dbContext, PayrollHrdSourceReader hrdSource)
        {
            _dbContext = dbContext;
            _hrdSource = hrdSource;
        }

        private sealed record Person(
            Guid WorkforceProfileId,
            Guid? EmployeeId,
            Guid? DoctorId,
            string Type,
            string Number,
            string Name,
            string? Department,
            string? Position);

        private sealed record SalaryProjection(
            Guid Id,
            Guid SalaryStructureId,
            string SalaryStructureCode,
            string SalaryStructureName,
            Guid SalaryGradeId,
            string SalaryGradeCode,
            string? SalaryGradeName,
            decimal GradeMinimum,
            decimal GradeMaximum,
            decimal? StructureMinimum,
            decimal? StructureMaximum,
            decimal BaseSalary,
            string CurrencyCode,
            string PaymentFrequency,
            DateTime EffectiveStartDate,
            DateTime? EffectiveEndDate,
            DateTime? ApprovedAt);

        private sealed record BankProjection(
            Guid Id,
            Guid? BankId,
            string? BankName,
            string AccountNumber,
            string AccountHolderName,
            bool IsPayrollAccount,
            bool IsPrimary,
            bool IsVerified,
            DateTime? VerifiedAt,
            DateTime? EffectiveStartDate,
            DateTime? EffectiveEndDate);

        private sealed record DateWindow(DateTime? EffectiveStartDate, DateTime? EffectiveEndDate);

        private sealed record MissingClassification(string Status, string Message);

        // ------------------------------------------------------------------------------------
        // Baca (Read-Only)
        // ------------------------------------------------------------------------------------

        public async Task<PagedResult<WorkforcePayrollDataItemResponse>> ListAsync(
            WorkforcePayrollDataQuery query, bool includeAmounts, CancellationToken cancellationToken)
        {
            IEnumerable<WorkforcePayrollDataItemResponse> rows = await LoadOverviewAsync(includeAmounts, cancellationToken);

            if (!string.IsNullOrWhiteSpace(query.PersonType))
            {
                var type = query.PersonType.Trim();
                rows = rows.Where(x => string.Equals(x.PersonType, type, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                rows = rows.Where(x =>
                    x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || x.Number.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var completeness = query.Completeness?.Trim();
            if (string.Equals(completeness, CompleteFilter, StringComparison.OrdinalIgnoreCase))
                rows = rows.Where(x => x.IsComplete);
            else if (string.Equals(completeness, IncompleteFilter, StringComparison.OrdinalIgnoreCase))
                rows = rows.Where(x => !x.IsComplete);

            var filtered = rows
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.Number, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            return new PagedResult<WorkforcePayrollDataItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = filtered.Count,
                TotalPage = (int)Math.Ceiling(filtered.Count / (double)pageSize),
                Items = filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
            };
        }

        public async Task<WorkforcePayrollDataSummaryResponse> SummaryAsync(CancellationToken cancellationToken)
        {
            var all = await LoadOverviewAsync(includeAmounts: false, cancellationToken);
            return new WorkforcePayrollDataSummaryResponse
            {
                TotalPeople = all.Count,
                WithApprovedSalaryAssignment = all.Count(x => x.HasApprovedSalaryAssignment),
                WithVerifiedBankAccount = all.Count(x => x.BankStatus == PayrollReadinessEvaluator.BankVerified),
                Complete = all.Count(x => x.IsComplete)
            };
        }

        /// <summary>
        /// Rincian read-only data penggajian satu orang pada tanggal acuan hari ini. Nominal gaji hanya diisi bagi pemegang izin
        /// ViewAmount; nomor rekening selalu disamarkan.
        /// </summary>
        public async Task<WorkforcePayrollDataDetailResponse> GetDetailAsync(
            Guid workforceProfileId, bool includeAmounts, CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            var person = await FindPersonAsync(workforceProfileId, cancellationToken);
            var scope = new[] { workforceProfileId };

            var salaries = await _hrdSource.LoadSalariesAsync(today, today, cancellationToken, scope);
            var banks = await _hrdSource.LoadBanksAsync(today, cancellationToken, scope);

            salaries.TryGetValue(workforceProfileId, out var salaryRef);
            banks.TryGetValue(workforceProfileId, out var bankRef);

            SalaryProjection? salaryRow = null;
            MissingClassification? missingSalary = null;
            if (salaryRef is not null)
            {
                var details = await LoadSalaryProjectionsAsync(new[] { salaryRef.AssignmentId }, cancellationToken);
                details.TryGetValue(salaryRef.AssignmentId, out salaryRow);
            }
            else
            {
                var nonEffective = await LoadNonEffectiveSalaryWindowsAsync(scope, cancellationToken);
                missingSalary = ClassifyMissingSalary(nonEffective.GetValueOrDefault(workforceProfileId), today);
            }

            BankProjection? bankRow = null;
            MissingClassification? missingBank = null;
            if (bankRef is not null)
            {
                var details = await LoadBankProjectionsAsync(new[] { bankRef.BankAccountId }, cancellationToken);
                details.TryGetValue(bankRef.BankAccountId, out bankRow);
            }
            else
            {
                var nonEffective = await LoadNonEffectiveBankWindowsAsync(scope, cancellationToken);
                missingBank = ClassifyMissingBank(nonEffective.GetValueOrDefault(workforceProfileId), today);
            }

            var evaluated = EvaluateHrd(salaryRef, bankRef, missingSalary, missingBank);

            return new WorkforcePayrollDataDetailResponse
            {
                WorkforceProfileId = person.WorkforceProfileId,
                EmployeeId = person.EmployeeId,
                DoctorId = person.DoctorId,
                PersonType = person.Type,
                Number = person.Number,
                Name = person.Name,
                DepartmentName = person.Department,
                PositionName = person.Position,
                AmountsVisible = includeAmounts,
                SalaryStatus = evaluated.SalaryStatus,
                BankStatus = evaluated.BankStatus,
                IsComplete = evaluated.IsComplete,
                Issues = evaluated.Issues,
                Salary = salaryRow is null
                    ? null
                    : BuildSalaryDetail(salaryRow, includeAmounts),
                BankAccount = bankRow is null
                    ? null
                    : new WorkforcePayrollBankDetail
                    {
                        Id = bankRow.Id,
                        BankId = bankRow.BankId,
                        BankName = bankRow.BankName,
                        AccountNumberMasked = MaskAccountNumber(bankRow.AccountNumber),
                        AccountHolderName = bankRow.AccountHolderName,
                        IsPayrollAccount = bankRow.IsPayrollAccount,
                        IsPrimary = bankRow.IsPrimary,
                        IsVerified = bankRow.IsVerified,
                        VerifiedAt = bankRow.VerifiedAt,
                        EffectiveStartDate = bankRow.EffectiveStartDate,
                        EffectiveEndDate = bankRow.EffectiveEndDate
                    }
            };
        }

        /// <summary>
        /// Populasi (karyawan, perawat, dan dokter aktif dari master HRD) dipertemukan dengan penetapan gaji dan rekening payroll
        /// yang berlaku pada tanggal acuan hari ini melalui <see cref="PayrollHrdSourceReader"/> dan dinilai melalui
        /// <see cref="PayrollReadinessEvaluator"/>, sehingga aturan pemilihan dan kelengkapan identik dengan modul Payroll.
        /// </summary>
        private async Task<List<WorkforcePayrollDataItemResponse>> LoadOverviewAsync(
            bool includeAmounts, CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            var people = await LoadPeopleAsync(null, cancellationToken);
            if (people.Count == 0) return new List<WorkforcePayrollDataItemResponse>();

            var salaries = await _hrdSource.LoadSalariesAsync(today, today, cancellationToken);
            var banks = await _hrdSource.LoadBanksAsync(today, cancellationToken);

            var chosenSalaryIds = salaries.Values.Select(x => x.AssignmentId).Distinct().ToList();
            var salaryProjections = chosenSalaryIds.Count > 0
                ? await LoadSalaryProjectionsAsync(chosenSalaryIds, cancellationToken)
                : new Dictionary<Guid, SalaryProjection>();

            var missingSalaryProfileIds = people
                .Select(p => p.WorkforceProfileId)
                .Where(id => !salaries.ContainsKey(id))
                .ToList();
            var nonEffectiveSalaries = missingSalaryProfileIds.Count > 0
                ? await LoadNonEffectiveSalaryWindowsAsync(missingSalaryProfileIds, cancellationToken)
                : new Dictionary<Guid, List<DateWindow>>();

            var chosenBankIds = banks.Values.Select(x => x.BankAccountId).Distinct().ToList();
            var bankProjections = chosenBankIds.Count > 0
                ? await LoadBankProjectionsAsync(chosenBankIds, cancellationToken)
                : new Dictionary<Guid, BankProjection>();

            var missingBankProfileIds = people
                .Select(p => p.WorkforceProfileId)
                .Where(id => !banks.ContainsKey(id))
                .ToList();
            var nonEffectiveBanks = missingBankProfileIds.Count > 0
                ? await LoadNonEffectiveBankWindowsAsync(missingBankProfileIds, cancellationToken)
                : new Dictionary<Guid, List<DateWindow>>();

            var items = new List<WorkforcePayrollDataItemResponse>(people.Count);
            foreach (var person in people)
            {
                salaries.TryGetValue(person.WorkforceProfileId, out var salaryRef);
                banks.TryGetValue(person.WorkforceProfileId, out var bankRef);

                SalaryProjection? salaryRow = null;
                MissingClassification? missingSalary = null;
                if (salaryRef is not null)
                    salaryProjections.TryGetValue(salaryRef.AssignmentId, out salaryRow);
                else
                    missingSalary = ClassifyMissingSalary(nonEffectiveSalaries.GetValueOrDefault(person.WorkforceProfileId), today);

                BankProjection? bankRow = null;
                MissingClassification? missingBank = null;
                if (bankRef is not null)
                    bankProjections.TryGetValue(bankRef.BankAccountId, out bankRow);
                else
                    missingBank = ClassifyMissingBank(nonEffectiveBanks.GetValueOrDefault(person.WorkforceProfileId), today);

                var evaluated = EvaluateHrd(salaryRef, bankRef, missingSalary, missingBank);

                items.Add(new WorkforcePayrollDataItemResponse
                {
                    WorkforceProfileId = person.WorkforceProfileId,
                    EmployeeId = person.EmployeeId,
                    DoctorId = person.DoctorId,
                    PersonType = person.Type,
                    Number = person.Number,
                    Name = person.Name,
                    DepartmentName = person.Department,
                    PositionName = person.Position,
                    HasApprovedSalaryAssignment = evaluated.HasApprovedSalary,
                    HasPendingSalaryAssignment = evaluated.HasPendingSalary,
                    HasSalaryConflict = evaluated.HasSalaryConflict,
                    SalaryAssignmentId = salaryRow?.Id,
                    SalaryStructureId = salaryRow?.SalaryStructureId,
                    SalaryStructureName = salaryRow?.SalaryStructureName,
                    SalaryGradeId = salaryRow?.SalaryGradeId,
                    SalaryGradeCode = salaryRow?.SalaryGradeCode,
                    SalaryGradeName = salaryRow?.SalaryGradeName,
                    AmountsVisible = includeAmounts,
                    // Nominal hanya dari WfpSalaryAssignment yang berlaku saat ini; null bila tanpa ViewAmount atau belum tersedia.
                    BaseSalary = includeAmounts && salaryRow is not null ? salaryRow.BaseSalary : null,
                    SalaryRangeText = includeAmounts && salaryRow is not null
                        ? PayrollSalaryRangeRuleService.CalculateEffectiveRange(salaryRow.GradeMinimum, salaryRow.GradeMaximum, salaryRow.StructureMinimum, salaryRow.StructureMaximum).SalaryRangeText
                        : null,
                    CurrencyCode = salaryRow?.CurrencyCode,
                    PaymentFrequency = salaryRow?.PaymentFrequency,
                    SalaryStatus = evaluated.SalaryStatus,
                    SalaryEffectiveStartDate = salaryRow?.EffectiveStartDate,
                    SalaryEffectiveEndDate = salaryRow?.EffectiveEndDate,
                    BankAccountId = bankRow?.Id,
                    BankStatus = evaluated.BankStatus,
                    BankName = bankRow?.BankName,
                    AccountNumberMasked = bankRow is not null ? MaskAccountNumber(bankRow.AccountNumber) : null,
                    BankIsVerified = bankRow?.IsVerified ?? false,
                    BankVerifiedAt = bankRow?.VerifiedAt,
                    IsComplete = evaluated.IsComplete,
                    Issues = evaluated.Issues
                });
            }

            return items;
        }

        /// <summary>
        /// Karyawan dan dokter aktif. Perawat tidak punya master sendiri: ia karyawan yang profesinya perawat, dikenali dengan aturan
        /// teks yang sama dengan pemetaan profesi CPPT (kode, nama, atau kelompok profesi memuat "perawat" atau "nurs"; bidan bukan
        /// perawat). Orang yang tercatat sebagai karyawan sekaligus dokter dihitung sekali, sebagai karyawan.
        /// </summary>
        private async Task<List<Person>> LoadPeopleAsync(Guid? workforceProfileId, CancellationToken cancellationToken)
        {
            var seen = new HashSet<Guid>();
            var people = new List<Person>();

            var employees = await _dbContext.MstEmployees.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && (workforceProfileId == null || x.WorkforceProfileId == workforceProfileId))
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.EmployeeNumber,
                    x.FullName,
                    Department = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : null,
                    Position = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    ProfessionGroup = x.Profession != null ? x.Profession.ProfessionGroup : null
                })
                .ToListAsync(cancellationToken);
            foreach (var e in employees)
            {
                if (!seen.Add(e.WorkforceProfileId)) continue;
                var type = IsNurse(e.ProfessionCode, e.ProfessionName, e.ProfessionGroup) ? PersonNurse : PersonEmployee;
                people.Add(new Person(e.WorkforceProfileId, e.Id, null, type, e.EmployeeNumber, e.FullName, e.Department, e.Position));
            }

            var doctors = await _dbContext.MstDoctors.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && (workforceProfileId == null || x.WorkforceProfileId == workforceProfileId))
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.DoctorNumber,
                    x.FullName,
                    Department = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : null,
                    Position = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null
                })
                .ToListAsync(cancellationToken);
            foreach (var d in doctors)
            {
                if (seen.Add(d.WorkforceProfileId))
                    people.Add(new Person(d.WorkforceProfileId, null, d.Id, PersonDoctor, d.DoctorNumber, d.FullName, d.Department, d.Position));
            }

            return people;
        }

        private async Task<Person> FindPersonAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var people = await LoadPeopleAsync(workforceProfileId, cancellationToken);
            return people.FirstOrDefault()
                ?? throw new WorkforcePayrollDataException(StatusCodes.Status404NotFound,
                    "Pegawai, perawat, atau dokter tidak ditemukan atau sudah tidak aktif.");
        }

        // ------------------------------------------------------------------------------------
        // Proyeksi dan klasifikasi sumber HRD (AsNoTracking)
        // ------------------------------------------------------------------------------------

        private async Task<Dictionary<Guid, SalaryProjection>> LoadSalaryProjectionsAsync(
            IReadOnlyCollection<Guid> assignmentIds, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(x => assignmentIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.SalaryStructureId,
                    StructureCode = x.SalaryStructure != null ? x.SalaryStructure.SalaryStructureCode : string.Empty,
                    StructureName = x.SalaryStructure != null ? x.SalaryStructure.SalaryStructureName : string.Empty,
                    x.SalaryGradeId,
                    GradeCode = x.SalaryGrade != null ? x.SalaryGrade.SalaryGradeCode : string.Empty,
                    GradeName = x.SalaryGrade != null ? x.SalaryGrade.SalaryGradeName : null,
                    GradeMinimum = x.SalaryGrade != null ? x.SalaryGrade.MinimumSalary : 0m,
                    GradeMaximum = x.SalaryGrade != null ? x.SalaryGrade.MaximumSalary : 0m,
                    StructureMinimum = x.SalaryStructure != null ? x.SalaryStructure.MinimumBaseSalary : null,
                    StructureMaximum = x.SalaryStructure != null ? x.SalaryStructure.MaximumBaseSalary : null,
                    x.BaseSalary,
                    x.CurrencyCode,
                    x.PaymentFrequency,
                    x.EffectiveStartDate,
                    x.EffectiveEndDate,
                    x.ApprovedAt
                })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                x => x.Id,
                x => new SalaryProjection(
                    x.Id,
                    x.SalaryStructureId,
                    x.StructureCode,
                    x.StructureName,
                    x.SalaryGradeId,
                    x.GradeCode,
                    x.GradeName,
                    x.GradeMinimum,
                    x.GradeMaximum,
                    x.StructureMinimum,
                    x.StructureMaximum,
                    x.BaseSalary,
                    x.CurrencyCode,
                    x.PaymentFrequency,
                    x.EffectiveStartDate,
                    x.EffectiveEndDate,
                    x.ApprovedAt));
        }

        private async Task<Dictionary<Guid, BankProjection>> LoadBankProjectionsAsync(
            IReadOnlyCollection<Guid> bankAccountIds, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(x => bankAccountIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.BankId,
                    BankName = x.Bank != null ? x.Bank.BankName : x.BankName,
                    x.AccountNumber,
                    x.AccountHolderName,
                    x.IsPayrollAccount,
                    x.IsPrimary,
                    x.IsVerified,
                    x.VerifiedAt,
                    x.EffectiveStartDate,
                    x.EffectiveEndDate
                })
                .ToListAsync(cancellationToken);

            return rows.ToDictionary(
                x => x.Id,
                x => new BankProjection(
                    x.Id,
                    x.BankId,
                    x.BankName,
                    x.AccountNumber,
                    x.AccountHolderName,
                    x.IsPayrollAccount,
                    x.IsPrimary,
                    x.IsVerified,
                    x.VerifiedAt,
                    x.EffectiveStartDate,
                    x.EffectiveEndDate));
        }

        private async Task<Dictionary<Guid, List<DateWindow>>> LoadNonEffectiveSalaryWindowsAsync(
            IReadOnlyCollection<Guid> workforceProfileIds, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsPrimary && workforceProfileIds.Contains(x.WorkforceProfileId))
                .Select(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new DateWindow(x.EffectiveStartDate, x.EffectiveEndDate)).ToList());
        }

        private async Task<Dictionary<Guid, List<DateWindow>>> LoadNonEffectiveBankWindowsAsync(
            IReadOnlyCollection<Guid> workforceProfileIds, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollAccount && workforceProfileIds.Contains(x.WorkforceProfileId))
                .Select(x => new { x.WorkforceProfileId, x.EffectiveStartDate, x.EffectiveEndDate })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new DateWindow(x.EffectiveStartDate, x.EffectiveEndDate)).ToList());
        }

        // ------------------------------------------------------------------------------------
        // Pembantu penilaian dan pemformatan
        // ------------------------------------------------------------------------------------

        private sealed record HrdEvaluation(
            bool HasApprovedSalary,
            bool HasPendingSalary,
            bool HasSalaryConflict,
            string SalaryStatus,
            string BankStatus,
            bool IsComplete,
            List<string> Issues);

        private static WorkforcePayrollSalaryDetail BuildSalaryDetail(SalaryProjection salaryRow, bool includeAmounts)
        {
            var range = PayrollSalaryRangeRuleService.CalculateEffectiveRange(
                salaryRow.GradeMinimum, salaryRow.GradeMaximum, salaryRow.StructureMinimum, salaryRow.StructureMaximum);

            return new WorkforcePayrollSalaryDetail
            {
                Id = salaryRow.Id,
                SalaryStructureId = salaryRow.SalaryStructureId,
                SalaryStructureCode = salaryRow.SalaryStructureCode,
                SalaryStructureName = salaryRow.SalaryStructureName,
                SalaryGradeId = salaryRow.SalaryGradeId,
                SalaryGradeCode = salaryRow.SalaryGradeCode,
                SalaryGradeName = salaryRow.SalaryGradeName,
                GradeMinimum = includeAmounts ? range.GradeMinimumSalary : null,
                GradeMaximum = includeAmounts ? range.GradeMaximumSalary : null,
                StructureMinimum = includeAmounts ? range.StructureMinimumSalary : null,
                StructureMaximum = includeAmounts ? range.StructureMaximumSalary : null,
                EffectiveMinimum = includeAmounts ? range.EffectiveMinimumSalary : null,
                EffectiveMaximum = includeAmounts ? range.EffectiveMaximumSalary : null,
                SalaryRangeText = includeAmounts ? range.SalaryRangeText : "Terselubung (Memerlukan Wewenang ViewAmount)",
                BaseSalary = includeAmounts ? salaryRow.BaseSalary : null,
                CurrencyCode = salaryRow.CurrencyCode,
                PaymentFrequency = salaryRow.PaymentFrequency,
                EffectiveStartDate = salaryRow.EffectiveStartDate,
                EffectiveEndDate = salaryRow.EffectiveEndDate,
                IsApproved = salaryRow.ApprovedAt.HasValue,
                ApprovedAt = salaryRow.ApprovedAt
            };
        }

        private static HrdEvaluation EvaluateHrd(
            PayrollHrdSalary? salary,
            PayrollHrdBank? bank,
            MissingClassification? missingSalary,
            MissingClassification? missingBank)
        {
            var assessment = PayrollReadinessEvaluator.AssessHrd(salary, bank, missingSalary?.Message ?? NoSalaryMasterMessage);
            var issues = new List<string>(assessment.Issues);

            string salaryStatus;
            if (salary is null)
            {
                salaryStatus = missingSalary?.Status ?? SalaryNoneLabel;
            }
            else
            {
                salaryStatus = assessment.SalaryConflict ? SalaryConflictLabel : SalaryApprovedLabel;
            }

            string bankStatus;
            if (bank is null)
            {
                bankStatus = missingBank?.Status ?? PayrollReadinessEvaluator.BankNone;
                var bankMessage = missingBank?.Message ?? NoBankMasterMessage;
                var index = issues.IndexOf(DefaultPayrollNoBankMessage);
                if (index >= 0) issues[index] = bankMessage;
            }
            else
            {
                bankStatus = assessment.BankStatus;
            }

            var hasValidApprovedSalary = assessment.SalaryApproved && !assessment.SalaryConflict;

            return new HrdEvaluation(
                HasApprovedSalary: hasValidApprovedSalary,
                HasPendingSalary: assessment.SalaryPending,
                HasSalaryConflict: assessment.SalaryConflict,
                SalaryStatus: salaryStatus,
                BankStatus: bankStatus,
                IsComplete: assessment.HrdComplete,
                Issues: issues);
        }

        private static MissingClassification ClassifyMissingSalary(List<DateWindow>? windows, DateTime today)
        {
            if (windows is null || windows.Count == 0)
                return new MissingClassification(SalaryNoneLabel, NoSalaryMasterMessage);

            var upcoming = windows
                .Where(w => w.EffectiveStartDate.HasValue && w.EffectiveStartDate.Value.Date > today)
                .OrderBy(w => w.EffectiveStartDate)
                .FirstOrDefault();
            if (upcoming?.EffectiveStartDate is not null)
            {
                return new MissingClassification(
                    SalaryNotYetEffective,
                    $"Penetapan gaji belum berlaku (mulai {upcoming.EffectiveStartDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}).");
            }

            var expired = windows
                .Where(w => w.EffectiveEndDate.HasValue && w.EffectiveEndDate.Value.Date < today)
                .OrderByDescending(w => w.EffectiveEndDate)
                .FirstOrDefault();
            if (expired?.EffectiveEndDate is not null)
            {
                return new MissingClassification(
                    SalaryExpiredLabel,
                    $"Penetapan gaji sudah kedaluwarsa (berakhir {expired.EffectiveEndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}).");
            }

            return new MissingClassification(SalaryNoneLabel, NoSalaryMasterMessage);
        }

        private static MissingClassification ClassifyMissingBank(List<DateWindow>? windows, DateTime today)
        {
            if (windows is null || windows.Count == 0)
                return new MissingClassification(PayrollReadinessEvaluator.BankNone, NoBankMasterMessage);

            var upcoming = windows
                .Where(w => w.EffectiveStartDate.HasValue && w.EffectiveStartDate.Value.Date > today)
                .OrderBy(w => w.EffectiveStartDate)
                .FirstOrDefault();
            if (upcoming?.EffectiveStartDate is not null)
            {
                return new MissingClassification(
                    BankNotYetEffective,
                    $"Rekening payroll belum berlaku (mulai {upcoming.EffectiveStartDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}).");
            }

            var expired = windows
                .Where(w => w.EffectiveEndDate.HasValue && w.EffectiveEndDate.Value.Date < today)
                .OrderByDescending(w => w.EffectiveEndDate)
                .FirstOrDefault();
            if (expired?.EffectiveEndDate is not null)
            {
                return new MissingClassification(
                    BankExpiredLabel,
                    $"Rekening payroll sudah kedaluwarsa (berakhir {expired.EffectiveEndDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}).");
            }

            return new MissingClassification(PayrollReadinessEvaluator.BankNone, NoBankMasterMessage);
        }

        /// <summary>Aturan teks yang sama dengan pemetaan profesi CPPT (kode, nama, atau kelompok profesi); bidan bukan perawat.</summary>
        private static bool IsNurse(string? professionCode, string? professionName, string? professionGroup)
        {
            var identity = $"{professionCode} {professionName} {professionGroup}".ToLowerInvariant();
            if (identity.Contains("bidan") || identity.Contains("midwi")) return false;
            return identity.Contains("perawat") || identity.Contains("nurs");
        }

        /// <summary>Hanya empat karakter terakhir yang tampak; nomor rekening lengkap tidak pernah keluar dari endpoint monitoring.</summary>
        private static string MaskAccountNumber(string? accountNumber)
        {
            var value = accountNumber?.Trim() ?? string.Empty;
            return value.Length <= 4 ? "****" : "****" + value[^4..];
        }
    }
}
