using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Data penggajian per orang yang DIKELOLA HRD: penetapan gaji utama (WfpSalaryAssignment) dan rekening payroll (WfpBankAccount).
    /// Populasi diambil dari Master Karyawan dan Master Dokter yang aktif; perawat adalah karyawan yang profesinya perawat. Menyediakan
    /// daftar kelengkapan, ringkasan, detail untuk diubah, dan simpan per orang. Modul Payroll hanya membaca data ini; profil payroll
    /// milik Payroll dan tidak disentuh di sini.
    /// Simpan memeriksa lebih dulu seluruh isian (struktur dan grade aktif, bank aktif, nomor rekening tidak ganda, periode dengan
    /// struktur dan grade yang sama tidak beririsan), lalu menulis dalam satu SaveChanges (satu transaksi database). Perubahan atas
    /// penetapan gaji yang sudah disetujui, atau atas rekening yang sudah terverifikasi, mengembalikannya menunggu persetujuan atau
    /// verifikasi ulang kecuali pemanggil yang berwenang memilih menyetujui atau memverifikasi sekaligus (diputuskan controller).
    /// Nominal dan nomor rekening tidak pernah ditulis ke log.
    /// </summary>
    public sealed class WorkforcePayrollDataService
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore.PayrollData";
        private const string PersonEmployee = "Karyawan";
        private const string PersonNurse = "Perawat";
        private const string PersonDoctor = "Dokter";
        private const string CompleteFilter = "Lengkap";
        private const string IncompleteFilter = "Belum lengkap";
        private const string BankNone = "Belum ada";
        private const string BankUnverified = "Belum terverifikasi";
        private const string BankVerified = "Terverifikasi";
        private const string NotChanged = "Tidak diubah";
        private const int MaxPageSize = 100;
        private const int AccountNumberMaxLength = 100;
        private const int AccountHolderMaxLength = 200;
        private const string RecordDescription = "Diisi lewat Data Penggajian Pegawai.";

        private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforcePayrollDataService(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        private sealed record Person(
            Guid WorkforceProfileId, string Type, string Number, string Name, string? Department, string? Position);

        private sealed record AssignmentView(bool Approved, string? StructureName, DateTime EffectiveStart);

        private sealed record BankView(bool Verified, string? BankName, string AccountNumber);

        // ------------------------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------------------------

        public async Task<PagedResult<WorkforcePayrollDataItemResponse>> ListAsync(
            WorkforcePayrollDataQuery query, CancellationToken cancellationToken)
        {
            IEnumerable<WorkforcePayrollDataItemResponse> rows = await LoadOverviewAsync(cancellationToken);

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
            var all = await LoadOverviewAsync(cancellationToken);
            return new WorkforcePayrollDataSummaryResponse
            {
                TotalPeople = all.Count,
                WithApprovedSalaryAssignment = all.Count(x => x.HasApprovedSalaryAssignment),
                WithVerifiedBankAccount = all.Count(x => x.BankStatus == BankVerified),
                Complete = all.Count(x => x.IsComplete)
            };
        }

        /// <summary>
        /// Isian satu orang untuk diubah, termasuk gaji pokok dan nomor rekening lengkap. Controller hanya mengizinkan pemegang hak
        /// ubah memuatnya.
        /// </summary>
        public async Task<WorkforcePayrollDataDetailResponse> GetDetailAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            var person = await FindPersonAsync(workforceProfileId, cancellationToken);

            var salary = await _dbContext.WfpSalaryAssignments.AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive && x.IsPrimary)
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => new
                {
                    x.Id,
                    x.SalaryStructureId,
                    StructureCode = x.SalaryStructure != null ? x.SalaryStructure.SalaryStructureCode : string.Empty,
                    StructureName = x.SalaryStructure != null ? x.SalaryStructure.SalaryStructureName : string.Empty,
                    GradeCode = x.SalaryGrade != null ? x.SalaryGrade.SalaryGradeCode : string.Empty,
                    GradeMinimum = x.SalaryGrade != null ? x.SalaryGrade.MinimumSalary : 0m,
                    GradeMaximum = x.SalaryGrade != null ? x.SalaryGrade.MaximumSalary : 0m,
                    x.BaseSalary,
                    x.EffectiveStartDate,
                    x.ApprovedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            var bank = await _dbContext.WfpBankAccounts.AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive && x.IsPayrollAccount)
                .OrderByDescending(x => x.IsVerified)
                .ThenByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new { x.Id, x.BankId, x.BankName, x.AccountNumber, x.AccountHolderName, x.IsVerified })
                .FirstOrDefaultAsync(cancellationToken);

            return new WorkforcePayrollDataDetailResponse
            {
                WorkforceProfileId = person.WorkforceProfileId,
                PersonType = person.Type,
                Number = person.Number,
                Name = person.Name,
                DepartmentName = person.Department,
                PositionName = person.Position,
                Salary = salary is null
                    ? null
                    : new WorkforcePayrollSalaryDetail
                    {
                        Id = salary.Id,
                        SalaryStructureId = salary.SalaryStructureId,
                        SalaryStructureCode = salary.StructureCode,
                        SalaryStructureName = salary.StructureName,
                        SalaryGradeCode = salary.GradeCode,
                        GradeMinimum = salary.GradeMinimum,
                        GradeMaximum = salary.GradeMaximum,
                        BaseSalary = salary.BaseSalary,
                        EffectiveStartDate = salary.EffectiveStartDate,
                        IsApproved = salary.ApprovedAt != null,
                        ApprovedAt = salary.ApprovedAt
                    },
                BankAccount = bank is null
                    ? null
                    : new WorkforcePayrollBankDetail
                    {
                        Id = bank.Id,
                        BankId = bank.BankId,
                        BankName = bank.BankName,
                        AccountNumber = bank.AccountNumber,
                        AccountHolderName = bank.AccountHolderName,
                        IsVerified = bank.IsVerified
                    }
            };
        }

        /// <summary>
        /// Populasi (karyawan, perawat, dan dokter aktif dari master HRD) dipertemukan dengan penetapan gaji dan rekening di memori.
        /// Jumlahnya dibatasi jumlah SDM rumah sakit, dan penyaringan lewat kelengkapan tidak dapat dipaginasi di database tanpa
        /// menggabungkan tiga tabel. Penetapan gaji yang dipakai adalah penetapan utama aktif dengan tanggal mulai terbaru, dan rekening
        /// yang dipakai adalah rekening payroll aktif (terverifikasi, lalu utama, lalu terbaru); sama dengan yang dibaca modul Payroll.
        /// </summary>
        private async Task<List<WorkforcePayrollDataItemResponse>> LoadOverviewAsync(CancellationToken cancellationToken)
        {
            var people = await LoadPeopleAsync(null, cancellationToken);

            var assignments = (await _dbContext.WfpSalaryAssignments.AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.IsPrimary)
                    .Select(x => new
                    {
                        x.WorkforceProfileId,
                        x.EffectiveStartDate,
                        Approved = x.ApprovedAt != null,
                        StructureName = x.SalaryStructure != null ? x.SalaryStructure.SalaryStructureName : null
                    })
                    .ToListAsync(cancellationToken))
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(a => a.EffectiveStartDate)
                        .Select(a => new AssignmentView(a.Approved, a.StructureName, a.EffectiveStartDate))
                        .First());

            var banks = (await _dbContext.WfpBankAccounts.AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.IsPayrollAccount)
                    .Select(x => new
                    {
                        x.WorkforceProfileId,
                        x.IsVerified,
                        x.IsPrimary,
                        x.CreateDateTime,
                        x.BankName,
                        x.AccountNumber
                    })
                    .ToListAsync(cancellationToken))
                .GroupBy(x => x.WorkforceProfileId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(b => b.IsVerified)
                        .ThenByDescending(b => b.IsPrimary)
                        .ThenByDescending(b => b.CreateDateTime)
                        .Select(b => new BankView(b.IsVerified, b.BankName, b.AccountNumber))
                        .First());

            var items = new List<WorkforcePayrollDataItemResponse>(people.Count);
            foreach (var person in people)
            {
                var issues = new List<string>();

                var hasAssignment = assignments.TryGetValue(person.WorkforceProfileId, out var assignment);
                var approved = hasAssignment && assignment!.Approved;
                if (!hasAssignment) issues.Add("Belum ada penetapan gaji utama.");
                else if (!approved) issues.Add("Penetapan gaji utama belum disetujui.");

                var hasBank = banks.TryGetValue(person.WorkforceProfileId, out var bank);
                var bankStatus = !hasBank ? BankNone : bank!.Verified ? BankVerified : BankUnverified;
                if (bankStatus == BankNone) issues.Add("Belum ada rekening payroll.");
                else if (bankStatus == BankUnverified) issues.Add("Rekening payroll belum terverifikasi.");

                items.Add(new WorkforcePayrollDataItemResponse
                {
                    WorkforceProfileId = person.WorkforceProfileId,
                    PersonType = person.Type,
                    Number = person.Number,
                    Name = person.Name,
                    DepartmentName = person.Department,
                    PositionName = person.Position,
                    HasApprovedSalaryAssignment = approved,
                    HasPendingSalaryAssignment = hasAssignment && !approved,
                    SalaryStructureName = hasAssignment ? assignment!.StructureName : null,
                    SalaryEffectiveStartDate = hasAssignment ? assignment!.EffectiveStart : null,
                    BankStatus = bankStatus,
                    BankName = hasBank ? bank!.BankName : null,
                    AccountNumberMasked = hasBank ? MaskAccountNumber(bank!.AccountNumber) : null,
                    IsComplete = issues.Count == 0,
                    Issues = issues
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
                people.Add(new Person(e.WorkforceProfileId, type, e.EmployeeNumber, e.FullName, e.Department, e.Position));
            }

            var doctors = await _dbContext.MstDoctors.AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && (workforceProfileId == null || x.WorkforceProfileId == workforceProfileId))
                .Select(x => new
                {
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
                    people.Add(new Person(d.WorkforceProfileId, PersonDoctor, d.DoctorNumber, d.FullName, d.Department, d.Position));
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
        // Simpan
        // ------------------------------------------------------------------------------------

        private sealed record StructureInfo(Guid Id, Guid SalaryGradeId);

        private sealed record GradeInfo(string Code, decimal Minimum, decimal Maximum);

        private sealed record BankInfo(Guid Id, string Name);

        /// <summary>
        /// Menyimpan penetapan gaji utama dan/atau rekening payroll satu orang. Bagian yang null tidak disentuh. Seluruh isian diperiksa
        /// lebih dulu; bila ada yang ditolak, tidak ada yang tersimpan.
        /// </summary>
        public async Task<WorkforcePayrollDataSaveResponse> SaveAsync(
            Guid workforceProfileId,
            UpdateWorkforcePayrollDataRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (request.Salary is null && request.BankAccount is null && !request.ApproveSalaryAssignment && !request.MarkBankAccountVerified)
                throw new WorkforcePayrollDataException(StatusCodes.Status400BadRequest, "Tidak ada perubahan untuk disimpan.");

            await FindPersonAsync(workforceProfileId, cancellationToken);

            var assignment = await _dbContext.WfpSalaryAssignments
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive && x.IsPrimary)
                .OrderByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            var accounts = await _dbContext.WfpBankAccounts
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var account = accounts
                .Where(a => a.IsActive && a.IsPayrollAccount)
                .OrderByDescending(a => a.IsVerified)
                .ThenByDescending(a => a.IsPrimary)
                .ThenByDescending(a => a.CreateDateTime)
                .FirstOrDefault();

            // ---- Pemeriksaan (belum ada yang diubah) ----
            var errors = new List<string>();
            var warnings = new List<string>();

            StructureInfo? structure = null;
            GradeInfo? grade = null;
            DateTime salaryStart = default;
            if (request.Salary is not null)
            {
                (structure, grade, salaryStart) = await ValidateSalaryAsync(
                    request.Salary, workforceProfileId, assignment, errors, cancellationToken);
            }

            BankInfo? bank = null;
            var accountNumber = string.Empty;
            var accountHolder = string.Empty;
            if (request.BankAccount is not null)
            {
                (bank, accountNumber, accountHolder) = await ValidateBankAsync(request.BankAccount, accounts, account, errors, cancellationToken);
            }

            if (request.ApproveSalaryAssignment && assignment is null && request.Salary is null)
                errors.Add("Belum ada penetapan gaji utama untuk disetujui. Isi struktur gaji, gaji pokok, dan tanggal mulai berlaku.");
            if (request.MarkBankAccountVerified && account is null && request.BankAccount is null)
                errors.Add("Belum ada rekening payroll untuk diverifikasi. Isi bank, nomor rekening, dan nama pemilik rekening.");

            if (errors.Count > 0)
                throw new WorkforcePayrollDataException(StatusCodes.Status400BadRequest, string.Join(" ", errors));

            // ---- Terapkan ----
            var now = DateTime.UtcNow;
            var response = new WorkforcePayrollDataSaveResponse { WorkforceProfileId = workforceProfileId };

            var salaryCreated = false;
            var salaryChanged = false;
            if (request.Salary is not null)
            {
                var salary = request.Salary;
                if (assignment is null)
                {
                    assignment = new WfpSalaryAssignment
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = workforceProfileId,
                        SalaryStructureId = structure!.Id,
                        SalaryGradeId = structure.SalaryGradeId,
                        BaseSalary = salary.BaseSalary,
                        CurrencyCode = "IDR",
                        PaymentFrequency = "Monthly",
                        EffectiveStartDate = salaryStart,
                        IsPrimary = true,
                        IsConfidential = true,
                        IsActive = true,
                        Description = RecordDescription,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        IsDelete = false,
                        IsCancel = false
                    };
                    _dbContext.WfpSalaryAssignments.Add(assignment);
                    salaryCreated = true;
                    salaryChanged = true;
                }
                else if (assignment.SalaryStructureId != structure!.Id
                         || assignment.BaseSalary != salary.BaseSalary
                         || assignment.EffectiveStartDate.Date != salaryStart)
                {
                    assignment.SalaryStructureId = structure.Id;
                    assignment.SalaryGradeId = structure.SalaryGradeId;
                    assignment.BaseSalary = salary.BaseSalary;
                    assignment.EffectiveStartDate = salaryStart;
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                    salaryChanged = true;

                    if (assignment.ApprovedAt.HasValue && !request.ApproveSalaryAssignment)
                    {
                        assignment.ApprovedAt = null;
                        assignment.ApprovedByUserId = null;
                        warnings.Add("Penetapan gaji berubah, jadi persetujuannya dicabut dan perlu disetujui kembali.");
                    }
                }

                // Aplikasi tidak menjaga gaji terhadap rentang grade (DEC-PRL-002 masih terbuka); tampilkan sebagai peringatan.
                if (salaryChanged && grade is not null
                    && (salary.BaseSalary < grade.Minimum || (grade.Maximum > 0m && salary.BaseSalary > grade.Maximum)))
                {
                    warnings.Add(
                        $"Gaji pokok di luar rentang grade {grade.Code} ({Rupiah(grade.Minimum)} – {Rupiah(grade.Maximum)}).");
                }
            }

            var salaryApproved = false;
            if (request.ApproveSalaryAssignment && assignment is not null && (assignment.ApprovedAt is null || salaryChanged))
            {
                assignment.ApprovedAt = now;
                assignment.ApprovedByUserId = actorUserId;
                if (!salaryCreated)
                {
                    assignment.UpdateDateTime = now;
                    assignment.UpdateBy = actorUserId;
                }
                salaryApproved = true;
            }

            response.SalaryAssignment = DescribeChange(salaryCreated, salaryChanged && !salaryCreated, salaryApproved, "disetujui");

            var accountCreated = false;
            var accountChanged = false;
            if (request.BankAccount is not null)
            {
                if (account is null)
                {
                    account = new WfpBankAccount
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = workforceProfileId,
                        BankId = bank!.Id,
                        BankName = bank.Name,
                        AccountNumber = accountNumber,
                        AccountHolderName = accountHolder,
                        AccountType = "Savings",
                        CurrencyCode = "IDR",
                        IsPayrollAccount = true,
                        IsPrimary = !accounts.Any(a => a.IsActive && a.IsPrimary),
                        IsVerified = false,
                        Description = RecordDescription,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        IsDelete = false,
                        IsCancel = false
                    };
                    _dbContext.WfpBankAccounts.Add(account);
                    accountCreated = true;
                    accountChanged = true;
                }
                else if (account.BankId != bank!.Id
                         || !string.Equals(account.AccountNumber, accountNumber, StringComparison.OrdinalIgnoreCase)
                         || !string.Equals(account.AccountHolderName, accountHolder, StringComparison.Ordinal))
                {
                    account.BankId = bank.Id;
                    account.BankName = bank.Name;
                    account.AccountNumber = accountNumber;
                    account.AccountHolderName = accountHolder;
                    account.UpdateDateTime = now;
                    account.UpdateBy = actorUserId;
                    accountChanged = true;

                    if (account.IsVerified && !request.MarkBankAccountVerified)
                    {
                        account.IsVerified = false;
                        account.VerifiedAt = null;
                        account.VerifiedByUserId = null;
                        warnings.Add("Data rekening berubah, jadi verifikasinya dicabut dan perlu diverifikasi kembali.");
                    }
                }
            }

            var accountVerified = false;
            if (request.MarkBankAccountVerified && account is not null && (!account.IsVerified || accountChanged))
            {
                account.IsVerified = true;
                account.VerifiedAt = now;
                account.VerifiedByUserId = actorUserId;
                if (!accountCreated)
                {
                    account.UpdateDateTime = now;
                    account.UpdateBy = actorUserId;
                }
                accountVerified = true;
            }

            response.BankAccount = DescribeChange(accountCreated, accountChanged && !accountCreated, accountVerified, "terverifikasi");
            response.Warnings = warnings;

            if (response.SalaryAssignment == NotChanged && response.BankAccount == NotChanged)
                return response;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Tanpa nominal dan tanpa nomor rekening: hanya apa yang berubah dan siapa pelakunya.
            await _loggerService.AuditAsync(LogCategory, "WorkforcePayrollData.Update",
                "Data penggajian pegawai diperbarui.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    SalaryAssignment = response.SalaryAssignment,
                    BankAccount = response.BankAccount,
                    Status = "Updated",
                    ActorUserId = actorUserId
                });

            return response;
        }

        private async Task<(StructureInfo?, GradeInfo?, DateTime)> ValidateSalaryAsync(
            UpdateWorkforcePayrollSalaryRequest salary,
            Guid workforceProfileId,
            WfpSalaryAssignment? current,
            List<string> errors,
            CancellationToken cancellationToken)
        {
            var before = errors.Count;

            if (salary.BaseSalary <= 0m) errors.Add("Gaji pokok harus lebih dari nol.");

            var start = default(DateTime);
            if (salary.EffectiveStartDate is null || salary.EffectiveStartDate.Value.Year < 2000)
                errors.Add("Tanggal mulai berlaku wajib diisi.");
            else
                start = salary.EffectiveStartDate.Value.Date;

            StructureInfo? structure = null;
            GradeInfo? grade = null;
            if (salary.SalaryStructureId == Guid.Empty)
            {
                errors.Add("Struktur gaji wajib dipilih.");
            }
            else
            {
                var structureRow = await _dbContext.MstSalaryStructures.AsNoTracking()
                    .Where(x => x.Id == salary.SalaryStructureId && !x.IsDelete && x.IsActive)
                    .Select(x => new { x.Id, x.SalaryGradeId })
                    .FirstOrDefaultAsync(cancellationToken);
                if (structureRow is null)
                {
                    errors.Add("Struktur gaji tidak ditemukan atau tidak aktif.");
                }
                else
                {
                    structure = new StructureInfo(structureRow.Id, structureRow.SalaryGradeId);
                    var gradeRow = await _dbContext.MstSalaryGrades.AsNoTracking()
                        .Where(x => x.Id == structureRow.SalaryGradeId && !x.IsDelete && x.IsActive)
                        .Select(x => new { x.SalaryGradeCode, x.MinimumSalary, x.MaximumSalary })
                        .FirstOrDefaultAsync(cancellationToken);
                    if (gradeRow is null)
                        errors.Add("Grade gaji dari struktur ini tidak ditemukan atau tidak aktif.");
                    else
                        grade = new GradeInfo(gradeRow.SalaryGradeCode, gradeRow.MinimumSalary, gradeRow.MaximumSalary);
                }
            }

            if (errors.Count == before && structure is not null)
            {
                // Tanggal berakhir penetapan yang sedang diubah tidak ikut diubah di sini; tanggal mulai tidak boleh melewatinya.
                var end = current?.EffectiveEndDate?.Date;
                if (end.HasValue && end.Value < start)
                {
                    errors.Add($"Tanggal mulai berlaku tidak boleh lewat dari tanggal berakhir penetapan ini ({end.Value:dd/MM/yyyy}).");
                }
                else
                {
                    var currentId = current?.Id;
                    // Tanpa tanggal berakhir, periode terbuka tanpa batas (sama dengan endpoint penetapan gaji HRD).
                    var endBound = end ?? DateTime.MaxValue;
                    var overlaps = await _dbContext.WfpSalaryAssignments.AsNoTracking().AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId
                        && !x.IsDelete
                        && (currentId == null || x.Id != currentId)
                        && x.SalaryStructureId == structure.Id
                        && x.SalaryGradeId == structure.SalaryGradeId
                        && x.EffectiveStartDate <= endBound
                        && (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= start),
                        cancellationToken);
                    if (overlaps)
                        errors.Add("Sudah ada penetapan gaji dengan struktur dan grade yang sama pada periode yang beririsan.");
                }
            }

            return (structure, grade, start);
        }

        private async Task<(BankInfo?, string, string)> ValidateBankAsync(
            UpdateWorkforcePayrollBankRequest bankRequest,
            List<WfpBankAccount> accounts,
            WfpBankAccount? target,
            List<string> errors,
            CancellationToken cancellationToken)
        {
            BankInfo? bank = null;
            if (!bankRequest.BankId.HasValue || bankRequest.BankId.Value == Guid.Empty)
            {
                errors.Add("Bank wajib dipilih.");
            }
            else
            {
                var bankRow = await _dbContext.MstBanks.AsNoTracking()
                    .Where(x => x.Id == bankRequest.BankId.Value && !x.IsDelete && x.IsActive)
                    .Select(x => new { x.Id, x.BankName })
                    .FirstOrDefaultAsync(cancellationToken);
                if (bankRow is null) errors.Add("Bank tidak ditemukan atau tidak aktif.");
                else bank = new BankInfo(bankRow.Id, bankRow.BankName);
            }

            var accountNumber = NormalizeAccountNumber(bankRequest.AccountNumber);
            if (accountNumber.Length == 0) errors.Add("Nomor rekening wajib diisi.");
            else if (accountNumber.Length > AccountNumberMaxLength)
                errors.Add($"Nomor rekening paling panjang {AccountNumberMaxLength} karakter.");
            else if (accounts.Any(a => a.Id != target?.Id
                                       && string.Equals(a.AccountNumber, accountNumber, StringComparison.OrdinalIgnoreCase)))
                errors.Add("Nomor rekening sudah tercatat untuk orang ini.");

            var holder = bankRequest.AccountHolderName?.Trim() ?? string.Empty;
            if (holder.Length == 0) errors.Add("Nama pemilik rekening wajib diisi.");
            else if (holder.Length > AccountHolderMaxLength)
                errors.Add($"Nama pemilik rekening paling panjang {AccountHolderMaxLength} karakter.");

            return (bank, accountNumber, holder);
        }

        // ------------------------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------------------------

        /// <summary>Aturan teks yang sama dengan pemetaan profesi CPPT (kode, nama, atau kelompok profesi); bidan bukan perawat.</summary>
        private static bool IsNurse(string? professionCode, string? professionName, string? professionGroup)
        {
            var identity = $"{professionCode} {professionName} {professionGroup}".ToLowerInvariant();
            if (identity.Contains("bidan") || identity.Contains("midwi")) return false;
            return identity.Contains("perawat") || identity.Contains("nurs");
        }

        private static string DescribeChange(bool created, bool updated, bool decided, string decidedWord)
        {
            var parts = new List<string>();
            if (created) parts.Add("Dibuat");
            else if (updated) parts.Add("Diperbarui");
            if (decided) parts.Add(created || updated ? decidedWord : char.ToUpperInvariant(decidedWord[0]) + decidedWord[1..]);
            return parts.Count == 0 ? NotChanged : string.Join(" dan ", parts);
        }

        /// <summary>Hanya empat karakter terakhir yang tampak.</summary>
        private static string MaskAccountNumber(string accountNumber)
        {
            var value = accountNumber?.Trim() ?? string.Empty;
            return value.Length <= 4 ? "****" : "****" + value[^4..];
        }

        /// <summary>Sama dengan NormalizeAccountNumber pada WfpBankAccountController: huruf dan angka saja, huruf besar.</summary>
        private static string NormalizeAccountNumber(string? value) =>
            new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        private static string Rupiah(decimal amount) => "Rp " + amount.ToString("N0", Indonesian);
    }
}
