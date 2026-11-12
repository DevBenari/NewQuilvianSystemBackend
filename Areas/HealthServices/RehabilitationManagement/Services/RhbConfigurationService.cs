using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Konfigurasi layanan rehab: disiplin, profesinya, tindakan yang boleh dipakai, dan jenis
    /// instrumen yang ditawarkan — <c>BE-RHB-005</c>, RHB-DEC-004/017/020.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kewenangan profesi melekat pada data</b>: layanan hanya boleh aktif bila punya profesi
    /// klinis aktif (<c>RHB-VAL-070</c>). Tidak ada nama profesi atau peran yang dibandingkan
    /// sebagai teks.
    /// </para>
    /// <para>
    /// Daftar tindakan dan instrumen disimpan dengan pola "nonaktifkan, bukan hapus", sehingga
    /// program dan asesmen lama tetap dapat menunjuk baris yang pernah berlaku.
    /// </para>
    /// </remarks>
    public partial class RhbConfigurationService
    {
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public RhbConfigurationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [GeneratedRegex("^[A-Z][A-Z0-9_]{1,39}$")]
        private static partial Regex DisciplineCodePattern();

        /// <summary>Label Bahasa Indonesia jenis instrumen klinis yang dapat ditawarkan.</summary>
        public static string InstrumentKindLabel(ClinicalInstrumentKind kind) => kind switch
        {
            ClinicalInstrumentKind.GeneralNursingAssessmentForm => "Kajian Umum Keperawatan",
            ClinicalInstrumentKind.FallRiskScale => "Skala Risiko Jatuh",
            ClinicalInstrumentKind.PainScale => "Skala Nyeri (NRS/VAS)",
            ClinicalInstrumentKind.EducationAssessmentForm => "Asesmen Edukasi",
            ClinicalInstrumentKind.DischargePlanningForm => "Perencanaan Pulang",
            ClinicalInstrumentKind.CaseManagementChecklist => "Checklist Evaluasi MPP",
            ClinicalInstrumentKind.SurgicalSiteSurveillanceForm => "Surveilans Infeksi Luka Operasi",
            ClinicalInstrumentKind.RehabBarthelIndex => "Indeks Barthel",
            ClinicalInstrumentKind.RehabBergBalanceScale => "Berg Balance Scale",
            ClinicalInstrumentKind.RehabTimedUpAndGo => "Timed Up and Go",
            ClinicalInstrumentKind.RehabLawtonIadl => "Lawton IADL",
            ClinicalInstrumentKind.RehabGussSwallowing => "Gugging Swallowing Screen (GUSS)",
            ClinicalInstrumentKind.RehabFoisOralIntake => "Functional Oral Intake Scale (FOIS)",
            _ => kind.ToString()
        };

        /// <summary>Jenis instrumen yang relevan bagi asesmen rehab (RHB-DEC-017).</summary>
        public static readonly IReadOnlyList<ClinicalInstrumentKind> RehabInstrumentKinds =
        [
            ClinicalInstrumentKind.PainScale,
            ClinicalInstrumentKind.RehabBarthelIndex,
            ClinicalInstrumentKind.RehabBergBalanceScale,
            ClinicalInstrumentKind.RehabTimedUpAndGo,
            ClinicalInstrumentKind.RehabLawtonIadl,
            ClinicalInstrumentKind.RehabGussSwallowing,
            ClinicalInstrumentKind.RehabFoisOralIntake
        ];

        // =====================================================================
        // Metadata, ringkasan, daftar, pilihan, rincian
        // =====================================================================

        public static RehabDisciplineFilterMetadataResponse BuildFilterMetadata() => new()
        {
            CustomPeriods =
            [
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thisMonth", Label = "Bulan ini" },
                new() { Value = "thisYear", Label = "Tahun ini" },
                new() { Value = "custom", Label = "Pilih tanggal", UsesStartDate = true, UsesEndDate = true }
            ],
            StatusOptions =
            [
                new() { Value = "true", Label = "Aktif" },
                new() { Value = "false", Label = "Tidak aktif" }
            ],
            InstrumentKindOptions = RehabInstrumentKinds
                .Select(x => new RehabOptionResponse { Value = ((int)x).ToString(), Label = InstrumentKindLabel(x) })
                .ToList(),
            SortOptions =
            [
                new() { Value = "disciplineCode", Label = "Kode layanan" },
                new() { Value = "disciplineName", Label = "Nama layanan" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            ],
            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100],
            QueryParameters =
            [
                new() { Name = "startDate", Type = "date", Description = "Awal rentang tanggal dibuat", Example = "2026-11-01" },
                new() { Name = "endDate", Type = "date", Description = "Akhir rentang tanggal dibuat", Example = "2026-11-30" },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, thisMonth, thisYear, custom", Example = "thisMonth" },
                new() { Name = "search", Type = "string", Description = "Cari kode atau nama layanan", Example = "FISIO" },
                new() { Name = "isActive", Type = "boolean", Description = "Saring status aktif", Example = "true" },
                new() { Name = "hasProfession", Type = "boolean", Description = "Saring layanan yang sudah/belum punya profesi", Example = "false" },
                new() { Name = "sortBy", Type = "string", Description = "disciplineCode, disciplineName, createDateTime", Example = "disciplineCode" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, mulai 1", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, maksimum 100", Example = "25" }
            ],
            CreateFields =
            [
                new() { Name = "disciplineCode", Label = "Kode layanan", Section = "Identitas", InputType = "text", IsRequiredOnCreate = true, RequiredType = "Required", MaxLength = 40, Description = "Huruf besar, angka, garis bawah. Tidak dapat diubah.", Example = "PHYSIOTHERAPY", SortOrder = 1 },
                new() { Name = "disciplineName", Label = "Nama layanan", Section = "Identitas", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 100, Example = "Fisioterapi", SortOrder = 2 },
                new() { Name = "professionId", Label = "Profesi klinis", Section = "Kewenangan", InputType = "select", RequiredType = "RequiredWhenActive", OptionsSource = "MstProfession (IsClinicalProfession)", Description = "Wajib sebelum layanan diaktifkan.", SortOrder = 3 },
                new() { Name = "isActive", Label = "Aktif", Section = "Status", InputType = "switch", SortOrder = 4 }
            ],
            UpdateFields =
            [
                new() { Name = "disciplineName", Label = "Nama layanan", Section = "Identitas", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 100, Example = "Fisioterapi", SortOrder = 1 },
                new() { Name = "professionId", Label = "Profesi klinis", Section = "Kewenangan", InputType = "select", RequiredType = "RequiredWhenActive", OptionsSource = "MstProfession (IsClinicalProfession)", Description = "Wajib selama layanan aktif.", SortOrder = 2 }
            ]
        };

        public async Task<RehabDisciplineSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
        {
            var rows = _dbContext.RhbDisciplines.AsNoTracking().Where(x => !x.IsDelete);

            return new RehabDisciplineSummaryResponse
            {
                TotalDiscipline = await rows.CountAsync(cancellationToken),
                ActiveDiscipline = await rows.CountAsync(x => x.IsActive, cancellationToken),
                InactiveDiscipline = await rows.CountAsync(x => !x.IsActive, cancellationToken),
                WithoutProfessionDiscipline = await rows.CountAsync(x => x.ProfessionId == null, cancellationToken)
            };
        }

        public async Task<RhbResult<PagedResult<RehabDisciplineListItem>>> GetListAsync(
            RehabDisciplineQuery query,
            CancellationToken cancellationToken)
        {
            var (start, end, rangeError) = RhbServiceSupport.ResolveRange(query.StartDate, query.EndDate, query.CustomPeriod);
            if (rangeError != null)
                return RhbResult<PagedResult<RehabDisciplineListItem>>.Invalid(RhbErrorCodes.PageRangeInvalid, rangeError);

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

            var rows = _dbContext.RhbDisciplines.AsNoTracking().Where(x => !x.IsDelete);

            if (start.HasValue)
                rows = rows.Where(x => x.CreateDateTime >= start.Value);
            if (end.HasValue)
                rows = rows.Where(x => x.CreateDateTime < end.Value);
            if (query.IsActive.HasValue)
                rows = rows.Where(x => x.IsActive == query.IsActive.Value);
            if (query.HasProfession.HasValue)
                rows = query.HasProfession.Value
                    ? rows.Where(x => x.ProfessionId != null)
                    : rows.Where(x => x.ProfessionId == null);
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                rows = rows.Where(x => x.DisciplineCode.ToLower().Contains(keyword) || x.DisciplineName.ToLower().Contains(keyword));
            }

            var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy?.Trim().ToLowerInvariant()) switch
            {
                "disciplinename" => descending ? rows.OrderByDescending(x => x.DisciplineName) : rows.OrderBy(x => x.DisciplineName),
                "createdatetime" => descending ? rows.OrderByDescending(x => x.CreateDateTime) : rows.OrderBy(x => x.CreateDateTime),
                _ => descending ? rows.OrderByDescending(x => x.DisciplineCode) : rows.OrderBy(x => x.DisciplineCode)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectList(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize))
                .ToListAsync(cancellationToken);

            return RhbResult<PagedResult<RehabDisciplineListItem>>.Ok(new PagedResult<RehabDisciplineListItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            });
        }

        public Task<List<RehabDisciplineOptionResponse>> GetOptionsAsync(bool onlyActive, CancellationToken cancellationToken) =>
            _dbContext.RhbDisciplines.AsNoTracking()
                .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
                .OrderBy(x => x.DisciplineName)
                .Select(x => new RehabDisciplineOptionResponse
                {
                    Id = x.Id,
                    DisciplineCode = x.DisciplineCode,
                    DisciplineName = x.DisciplineName,
                    ProfessionId = x.ProfessionId
                })
                .ToListAsync(cancellationToken);

        public async Task<RehabDisciplineDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var row = await _dbContext.RhbDisciplines.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new RehabDisciplineDetailResponse
                {
                    Id = x.Id,
                    DisciplineCode = x.DisciplineCode,
                    DisciplineName = x.DisciplineName,
                    ProfessionId = x.ProfessionId,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                return null;

            if (row.ProfessionId.HasValue)
            {
                var profession = await _dbContext.Set<MstProfession>().AsNoTracking()
                    .Where(p => p.Id == row.ProfessionId.Value)
                    .Select(p => new { p.ProfessionCode, p.ProfessionName })
                    .FirstOrDefaultAsync(cancellationToken);
                row.ProfessionCode = profession?.ProfessionCode;
                row.ProfessionName = profession?.ProfessionName;
            }

            row.Procedures = await ReadProceduresAsync(id, cancellationToken);
            row.Instruments = await ReadInstrumentsAsync(id, cancellationToken);
            row.ActiveProcedureCount = row.Procedures.Count(x => x.IsActive);
            row.ActiveInstrumentCount = row.Instruments.Count(x => x.IsActive);
            row.IsInUse = await IsInUseAsync(id, cancellationToken);
            return row;
        }

        // =====================================================================
        // Tulis
        // =====================================================================

        public async Task<RhbResult<RehabDisciplineDetailResponse>> CreateAsync(
            CreateRehabDisciplineRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var code = (request.DisciplineCode ?? string.Empty).Trim().ToUpperInvariant();
            var name = (request.DisciplineName ?? string.Empty).Trim();

            if (!DisciplineCodePattern().IsMatch(code))
                return RhbResult<RehabDisciplineDetailResponse>.Invalid(
                    RhbErrorCodes.DisciplineCodeInvalid,
                    "Kode layanan hanya boleh huruf besar, angka, dan garis bawah, diawali huruf.")
                    .WithFields("disciplineCode");

            if (name.Length == 0)
                return RhbResult<RehabDisciplineDetailResponse>.Invalid(RhbErrorCodes.DisciplineCodeInvalid, "Nama layanan wajib diisi.")
                    .WithFields("disciplineName");

            // Index unik DisciplineCode tidak bersyarat, sehingga kode baris terhapus pun tidak boleh dipakai ulang.
            if (await _dbContext.RhbDisciplines.AnyAsync(x => x.DisciplineCode == code, cancellationToken))
                return RhbResult<RehabDisciplineDetailResponse>.Conflict(RhbErrorCodes.DisciplineCodeTaken, "Kode layanan sudah dipakai.")
                    .WithFields("disciplineCode");

            var professionError = await ValidateProfessionAsync(request.ProfessionId, request.IsActive, cancellationToken);
            if (professionError != null)
                return RhbResult<RehabDisciplineDetailResponse>.Rule(RhbErrorCodes.DisciplineProfessionRequired, professionError)
                    .WithFields("professionId");

            var entity = new RhbDiscipline
            {
                DisciplineCode = code,
                DisciplineName = name,
                ProfessionId = request.ProfessionId,
                IsActive = request.IsActive,
                CreateBy = actorUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _dbContext.RhbDisciplines.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RhbResult<RehabDisciplineDetailResponse>.Created((await GetDetailAsync(entity.Id, cancellationToken))!);
        }

        public async Task<RhbResult<RehabDisciplineDetailResponse>> UpdateAsync(
            Guid id,
            UpdateRehabDisciplineRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbDisciplines.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return RhbResult<RehabDisciplineDetailResponse>.NotFound("Layanan rehab tidak ditemukan.");

            var name = (request.DisciplineName ?? string.Empty).Trim();
            if (name.Length == 0)
                return RhbResult<RehabDisciplineDetailResponse>.Invalid(RhbErrorCodes.DisciplineCodeInvalid, "Nama layanan wajib diisi.")
                    .WithFields("disciplineName");

            var professionError = await ValidateProfessionAsync(request.ProfessionId, entity.IsActive, cancellationToken);
            if (professionError != null)
                return RhbResult<RehabDisciplineDetailResponse>.Rule(RhbErrorCodes.DisciplineProfessionRequired, professionError)
                    .WithFields("professionId");

            entity.DisciplineName = name;
            entity.ProfessionId = request.ProfessionId;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RhbResult<RehabDisciplineDetailResponse>.Ok((await GetDetailAsync(id, cancellationToken))!);
        }

        public async Task<RhbResult<RehabDisciplineDetailResponse>> SetStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbDisciplines.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return RhbResult<RehabDisciplineDetailResponse>.NotFound("Layanan rehab tidak ditemukan.");

            if (isActive)
            {
                var professionError = await ValidateProfessionAsync(entity.ProfessionId, true, cancellationToken);
                if (professionError != null)
                    return RhbResult<RehabDisciplineDetailResponse>.Rule(RhbErrorCodes.DisciplineProfessionRequired, professionError)
                        .WithFields("professionId");
            }

            entity.IsActive = isActive;
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RhbResult<RehabDisciplineDetailResponse>.Ok((await GetDetailAsync(id, cancellationToken))!);
        }

        public async Task<RhbResult<RehabDisciplineDetailResponse>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbDisciplines.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return RhbResult<RehabDisciplineDetailResponse>.NotFound("Layanan rehab tidak ditemukan.");

            if (await IsInUseAsync(id, cancellationToken))
                return RhbResult<RehabDisciplineDetailResponse>.Conflict(
                    RhbErrorCodes.DisciplineInUse,
                    "Layanan sudah dipakai permintaan atau episode, sehingga tidak dapat dihapus. Nonaktifkan saja.");

            var detail = (await GetDetailAsync(id, cancellationToken))!;
            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteBy = actorUserId;
            entity.DeleteDateTime = now;
            await _dbContext.SaveChangesAsync(cancellationToken);

            detail.IsActive = false;
            return RhbResult<RehabDisciplineDetailResponse>.Ok(detail);
        }

        // =====================================================================
        // Sub-resource: tindakan dan instrumen
        // =====================================================================

        public async Task<RhbResult<List<RehabDisciplineProcedureResponse>>> GetProceduresAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await DisciplineExistsAsync(id, cancellationToken))
                return RhbResult<List<RehabDisciplineProcedureResponse>>.NotFound("Layanan rehab tidak ditemukan.");

            return RhbResult<List<RehabDisciplineProcedureResponse>>.Ok(await ReadProceduresAsync(id, cancellationToken));
        }

        public async Task<RhbResult<List<RehabDisciplineProcedureResponse>>> SaveProceduresAsync(
            Guid id,
            SaveRehabDisciplineProceduresRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (!await DisciplineExistsAsync(id, cancellationToken))
                return RhbResult<List<RehabDisciplineProcedureResponse>>.NotFound("Layanan rehab tidak ditemukan.");

            var wanted = (request.Procedures ?? new List<RehabDisciplineProcedureItemRequest>())
                .Where(x => x.ProcedureId != Guid.Empty)
                .GroupBy(x => x.ProcedureId)
                .ToDictionary(g => g.Key, g => g.Last().IsActive);

            var activeWanted = wanted.Where(x => x.Value).Select(x => x.Key).ToList();
            if (activeWanted.Count > 0)
            {
                var validCount = await _dbContext.Set<MstProcedure>().AsNoTracking()
                    .CountAsync(p => activeWanted.Contains(p.Id) && !p.IsDelete && p.IsActive, cancellationToken);
                if (validCount != activeWanted.Count)
                    return RhbResult<List<RehabDisciplineProcedureResponse>>.Rule(
                        RhbErrorCodes.ProcedureNotAvailable,
                        "Sebagian tindakan tidak ditemukan atau tidak aktif di master tindakan.")
                        .WithFields("procedures");
            }

            var existing = await _dbContext.RhbDisciplineProcedures
                .Where(x => x.DisciplineId == id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;

            foreach (var row in existing)
            {
                var active = wanted.TryGetValue(row.ProcedureId, out var flag) && flag;
                if (row.IsActive != active)
                {
                    row.IsActive = active;
                    row.UpdateBy = actorUserId;
                    row.UpdateDateTime = now;
                }
            }

            foreach (var (procedureId, isActive) in wanted)
            {
                if (existing.Any(x => x.ProcedureId == procedureId) || !isActive)
                    continue;

                _dbContext.RhbDisciplineProcedures.Add(new RhbDisciplineProcedure
                {
                    DisciplineId = id,
                    ProcedureId = procedureId,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RhbResult<List<RehabDisciplineProcedureResponse>>.Ok(await ReadProceduresAsync(id, cancellationToken));
        }

        public async Task<RhbResult<List<RehabDisciplineInstrumentResponse>>> GetInstrumentsAsync(Guid? id, CancellationToken cancellationToken)
        {
            if (id.HasValue && !await DisciplineExistsAsync(id.Value, cancellationToken))
                return RhbResult<List<RehabDisciplineInstrumentResponse>>.NotFound("Layanan rehab tidak ditemukan.");

            return RhbResult<List<RehabDisciplineInstrumentResponse>>.Ok(await ReadInstrumentsAsync(id, cancellationToken));
        }

        /// <summary>
        /// Menyimpan jenis instrumen untuk satu disiplin, atau untuk asesmen medis bila
        /// <paramref name="id"/> kosong.
        /// </summary>
        public async Task<RhbResult<List<RehabDisciplineInstrumentResponse>>> SaveInstrumentsAsync(
            Guid? id,
            SaveRehabDisciplineInstrumentsRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            if (id.HasValue && !await DisciplineExistsAsync(id.Value, cancellationToken))
                return RhbResult<List<RehabDisciplineInstrumentResponse>>.NotFound("Layanan rehab tidak ditemukan.");

            var wanted = (request.InstrumentKinds ?? new List<ClinicalInstrumentKind>()).Distinct().ToList();
            if (wanted.Any(x => !Enum.IsDefined(x)))
                return RhbResult<List<RehabDisciplineInstrumentResponse>>.Invalid(
                    RhbErrorCodes.InstrumentKindInvalid, "Jenis instrumen tidak dikenal.")
                    .WithFields("instrumentKinds");

            var existing = await _dbContext.RhbDisciplineInstruments
                .Where(x => x.DisciplineId == id && !x.IsDelete)
                .ToListAsync(cancellationToken);
            var now = DateTime.UtcNow;

            foreach (var row in existing)
            {
                var active = wanted.Contains(row.InstrumentKind);
                if (row.IsActive != active)
                {
                    row.IsActive = active;
                    row.UpdateBy = actorUserId;
                    row.UpdateDateTime = now;
                }
            }

            foreach (var kind in wanted.Where(k => existing.All(x => x.InstrumentKind != k)))
            {
                _dbContext.RhbDisciplineInstruments.Add(new RhbDisciplineInstrument
                {
                    DisciplineId = id,
                    InstrumentKind = kind,
                    IsActive = true,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return RhbResult<List<RehabDisciplineInstrumentResponse>>.Ok(await ReadInstrumentsAsync(id, cancellationToken));
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private IQueryable<RehabDisciplineListItem> ProjectList(IQueryable<RhbDiscipline> rows) =>
            rows.Select(x => new RehabDisciplineListItem
            {
                Id = x.Id,
                DisciplineCode = x.DisciplineCode,
                DisciplineName = x.DisciplineName,
                ProfessionId = x.ProfessionId,
                ProfessionName = _dbContext.Set<MstProfession>()
                    .Where(p => p.Id == x.ProfessionId)
                    .Select(p => p.ProfessionName)
                    .FirstOrDefault(),
                IsActive = x.IsActive,
                ActiveProcedureCount = x.Procedures.Count(p => p.IsActive && !p.IsDelete),
                ActiveInstrumentCount = x.Instruments.Count(i => i.IsActive && !i.IsDelete),
                CreateDateTime = x.CreateDateTime
            });

        private Task<List<RehabDisciplineProcedureResponse>> ReadProceduresAsync(Guid disciplineId, CancellationToken cancellationToken) =>
            (from row in _dbContext.RhbDisciplineProcedures.AsNoTracking()
             join procedure in _dbContext.Set<MstProcedure>().AsNoTracking() on row.ProcedureId equals procedure.Id
             where row.DisciplineId == disciplineId && !row.IsDelete
             orderby procedure.ProcedureName
             select new RehabDisciplineProcedureResponse
             {
                 Id = row.Id,
                 ProcedureId = row.ProcedureId,
                 ProcedureCode = procedure.ProcedureCode,
                 ProcedureName = procedure.ProcedureName,
                 IsProcedureActive = procedure.IsActive && !procedure.IsDelete,
                 IsActive = row.IsActive
             })
            .ToListAsync(cancellationToken);

        private async Task<List<RehabDisciplineInstrumentResponse>> ReadInstrumentsAsync(Guid? disciplineId, CancellationToken cancellationToken)
        {
            var rows = await _dbContext.RhbDisciplineInstruments.AsNoTracking()
                .Where(x => x.DisciplineId == disciplineId && !x.IsDelete)
                .OrderBy(x => x.InstrumentKind)
                .Select(x => new { x.Id, x.InstrumentKind, x.IsActive })
                .ToListAsync(cancellationToken);

            return rows.Select(x => new RehabDisciplineInstrumentResponse
            {
                Id = x.Id,
                InstrumentKind = x.InstrumentKind,
                InstrumentKindLabel = InstrumentKindLabel(x.InstrumentKind),
                IsActive = x.IsActive
            }).ToList();
        }

        private Task<bool> DisciplineExistsAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.RhbDisciplines.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        /// <summary>Layanan dipakai bila dirujuk permintaan, resep episode, atau program.</summary>
        private async Task<bool> IsInUseAsync(Guid id, CancellationToken cancellationToken) =>
            await _dbContext.RhbRequestDisciplines.AsNoTracking().AnyAsync(x => x.DisciplineId == id && !x.IsDelete, cancellationToken)
            || await _dbContext.RhbEpisodeDisciplines.AsNoTracking().AnyAsync(x => x.DisciplineId == id && !x.IsDelete, cancellationToken)
            || await _dbContext.RhbPrograms.AsNoTracking().AnyAsync(x => x.DisciplineId == id && !x.IsDelete, cancellationToken);

        /// <summary>
        /// Profesi wajib ada, aktif, dan klinis bila layanan aktif; bila diisi saat layanan tidak
        /// aktif, tetap wajib sah. Mengembalikan pesan penolakan atau <c>null</c>.
        /// </summary>
        private async Task<string?> ValidateProfessionAsync(Guid? professionId, bool requireForActive, CancellationToken cancellationToken)
        {
            if (!professionId.HasValue)
                return requireForActive ? "Tetapkan profesi klinis untuk layanan ini." : null;

            var valid = await _dbContext.Set<MstProfession>().AsNoTracking()
                .AnyAsync(p => p.Id == professionId.Value && !p.IsDelete && p.IsActive && p.IsClinicalProfession, cancellationToken);

            return valid ? null : "Tetapkan profesi klinis untuk layanan ini.";
        }
    }
}
