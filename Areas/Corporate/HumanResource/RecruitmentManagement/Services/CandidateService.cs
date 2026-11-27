using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Services
{
    public class CandidateService : ICandidateService
    {
        private const string CandidateCodeSequenceKey = "HRD_CANDIDATE_CODE";
        private const string CandidateCodePrefix = "CAN";
        private const string ApplicationNumberSequenceKey = "HRD_CANDIDATE_APPLICATION";
        private const string ApplicationNumberPrefix = "APP";
        private const string CandidateDocumentAttachmentCategory = "recruitment-candidate-document";

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly HrAttachmentStorage _attachmentStorage;

        public CandidateService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            HrAttachmentStorage attachmentStorage)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _attachmentStorage = attachmentStorage;
        }

        public async Task<RecruitmentResult<PagedResult<CandidateListItemResponse>>> GetPagedAsync(
            CandidateQuery query,
            CancellationToken cancellationToken = default)
        {
            var dbQuery = _dbContext.HrdCandidates
                .AsNoTracking()
                .Where(c => !c.IsDelete);

            if (query.IsRehire.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.IsRehire == query.IsRehire.Value);
            }

            if (query.Status.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.Status == query.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchNik))
            {
                var nik = query.SearchNik.Trim();
                dbQuery = dbQuery.Where(c => c.NationalIdNumber.Contains(nik));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim().ToLower();
                dbQuery = dbQuery.Where(c =>
                    c.CandidateCode.ToLower().Contains(term) ||
                    c.FullName.ToLower().Contains(term) ||
                    c.Email.ToLower().Contains(term) ||
                    c.PhoneNumber.Contains(term));
            }

            var totalData = await dbQuery.CountAsync(cancellationToken);

            var pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;

            var items = await dbQuery
                .OrderByDescending(c => c.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CandidateListItemResponse
                {
                    Id = c.Id,
                    CandidateCode = c.CandidateCode,
                    NationalIdNumber = c.NationalIdNumber,
                    FullName = c.FullName,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    IsRehire = c.IsRehire,
                    Status = c.Status,
                    CreateDateTime = c.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            var candidateIds = items.Select(i => i.Id).ToList();
            if (candidateIds.Count > 0)
            {
                var apps = await _dbContext.HrdCandidateApplications
                    .AsNoTracking()
                    .Include(a => a.Vacancy)
                    .Where(a => candidateIds.Contains(a.CandidateId) && !a.IsDelete)
                    .OrderByDescending(a => a.ApplicationDate)
                    .ToListAsync(cancellationToken);

                var docCounts = await _dbContext.HrdCandidateDocuments
                    .AsNoTracking()
                    .Where(d => candidateIds.Contains(d.CandidateId) && !d.IsDelete)
                    .GroupBy(d => d.CandidateId)
                    .Select(g => new { CandidateId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CandidateId, x => x.Count, cancellationToken);

                var appsByCandidate = apps.GroupBy(a => a.CandidateId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var item in items)
                {
                    item.MaskedNationalIdNumber = RecruitmentJourneyPolicy.MaskNationalIdNumber(item.NationalIdNumber);
                    if (appsByCandidate.TryGetValue(item.Id, out var cApps) && cApps.Count > 0)
                    {
                        var latest = cApps[0];
                        item.ApplicationsCount = cApps.Count;
                        item.ActiveApplicationsCount = cApps.Count(a => !RecruitmentJourneyPolicy.IsInactiveApplicationForCandidateStatus(a));
                        item.LatestApplicationId = latest.Id;
                        item.LatestApplicationNumber = latest.ApplicationNumber;
                        item.LatestVacancyTitle = latest.Vacancy?.Title;
                        item.LatestApplicationStage = latest.CurrentStage;
                        item.LatestApplicationStageStatus = latest.StageStatus;
                    }

                    if (docCounts.TryGetValue(item.Id, out var dCount))
                    {
                        item.DocumentsCount = dCount;
                    }
                }
            }
            else
            {
                foreach (var item in items)
                {
                    item.MaskedNationalIdNumber = RecruitmentJourneyPolicy.MaskNationalIdNumber(item.NationalIdNumber);
                }
            }

            var result = new PagedResult<CandidateListItemResponse>
            {
                Items = items,
                TotalData = totalData,
                TotalPage = pageSize > 0 ? (int)Math.Ceiling((double)totalData / pageSize) : 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return RecruitmentResult<PagedResult<CandidateListItemResponse>>.Ok(result, "Daftar pelamar berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var c = await _dbContext.HrdCandidates
                .AsNoTracking()
                .Include(x => x.Applications)
                    .ThenInclude(a => a.Vacancy)
                .Include(x => x.Documents)
                .Include(x => x.PreviousWorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (c == null)
            {
                return RecruitmentResult<CandidateResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            string? previousEmployeeNumber = null;
            if (c.PreviousWorkforceProfileId.HasValue)
            {
                var emp = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.WorkforceProfileId == c.PreviousWorkforceProfileId.Value && !e.IsDelete, cancellationToken);
                previousEmployeeNumber = emp?.EmployeeNumber;
            }

            var response = MapToResponse(c, previousEmployeeNumber);
            return RecruitmentResult<CandidateResponse>.Ok(response, "Detail kandidat berhasil diambil.");
        }

        public async Task<RecruitmentResult<CandidateResponse>> RegisterAsync(
            RegisterCandidateRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<CandidateResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            var cleanNik = request.NationalIdNumber?.Trim() ?? string.Empty;
            if (cleanNik.Length != 16 || !cleanNik.All(char.IsDigit))
            {
                return RecruitmentResult<CandidateResponse>.Fail("NIK KTP harus terdiri dari tepat 16 digit angka.", 400);
            }

            var validationError = ValidateProfileFields(request.FullName, request.Gender, request.BirthDate, request.Email, request.PhoneNumber);
            if (validationError != null)
            {
                return RecruitmentResult<CandidateResponse>.Fail(validationError, 400);
            }

            // Anti-duplikasi kandidat berdasarkan NIK
            var exists = await _dbContext.HrdCandidates
                .AnyAsync(c => c.NationalIdNumber == cleanNik && !c.IsDelete, cancellationToken);

            if (exists)
            {
                return RecruitmentResult<CandidateResponse>.Conflict($"Pelamar dengan NIK '{cleanNik}' sudah terdaftar dalam sistem.");
            }

            // Aturan Bisnis VR-REC-06: Deteksi rehire berdasarkan NIK di master pegawai (MstEmployee)
            var matchedEmployee = await _dbContext.MstEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdentityNumber == cleanNik && !e.IsDelete, cancellationToken);

            var isRehire = matchedEmployee != null;
            Guid? previousProfileId = matchedEmployee?.WorkforceProfileId;

            // Alokasi nomor urut CandidateCode atomik melalui NumberSeriesAllocator (QBE-CODE-003/006)
            var candidateCode = await AllocateCandidateCodeAsync(userId, cancellationToken);

            var candidate = new HrdCandidate
            {
                Id = Guid.NewGuid(),
                CandidateCode = candidateCode,
                NationalIdNumber = cleanNik,
                FullName = request.FullName.Trim(),
                Gender = request.Gender?.Trim() ?? string.Empty,
                BirthDate = request.BirthDate,
                BirthPlace = request.BirthPlace?.Trim() ?? string.Empty,
                Email = request.Email.Trim().ToLowerInvariant(),
                PhoneNumber = request.PhoneNumber.Trim(),
                Address = request.Address?.Trim() ?? string.Empty,
                IsRehire = isRehire,
                PreviousWorkforceProfileId = previousProfileId,
                Status = CandidateStatus.Registered,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdCandidates.AddAsync(candidate, cancellationToken);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                return RecruitmentResult<CandidateResponse>.Conflict($"Pelamar dengan NIK '{cleanNik}' sudah terdaftar dalam sistem.");
            }

            return await GetByIdAsync(candidate.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateResponse>> UpdateAsync(
            Guid id,
            UpdateCandidateRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<CandidateResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            var entity = await _dbContext.HrdCandidates
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDelete, cancellationToken);

            if (entity == null)
            {
                return RecruitmentResult<CandidateResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            if (entity.RowVersion != request.RowVersion)
            {
                return RecruitmentResult<CandidateResponse>.Conflict("Data kandidat telah diubah oleh pengguna lain. Silakan muat ulang data terbaru.");
            }

            var validationError = ValidateProfileFields(request.FullName, request.Gender, request.BirthDate, request.Email, request.PhoneNumber);
            if (validationError != null)
            {
                return RecruitmentResult<CandidateResponse>.Fail(validationError, 400);
            }

            entity.FullName = request.FullName.Trim();
            entity.Gender = request.Gender?.Trim() ?? string.Empty;
            entity.BirthDate = request.BirthDate;
            entity.BirthPlace = request.BirthPlace?.Trim() ?? string.Empty;
            entity.Email = request.Email.Trim().ToLowerInvariant();
            entity.PhoneNumber = request.PhoneNumber.Trim();
            entity.Address = request.Address?.Trim() ?? string.Empty;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = userId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return RecruitmentResult<CandidateResponse>.Conflict("Data kandidat telah diubah oleh pengguna lain. Silakan muat ulang data terbaru.");
            }

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<RecruitmentResult<CandidateApplicationResponse>> ApplyVacancyAsync(
            Guid candidateId,
            ApplyVacancyRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            var candidate = await _dbContext.HrdCandidates
                .FirstOrDefaultAsync(c => c.Id == candidateId && !c.IsDelete, cancellationToken);

            if (candidate == null)
            {
                return RecruitmentResult<CandidateApplicationResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            if (candidate.Status == CandidateStatus.Blacklisted)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Unprocessable(
                    "Pelamar dengan status daftar hitam (Blacklisted) tidak dapat mengajukan lamaran baru.");
            }

            // Validasi keberadaan dan status lowongan kerja
            var vacancy = await _dbContext.HrdJobVacancies
                .FirstOrDefaultAsync(v => v.Id == request.VacancyId && !v.IsDelete, cancellationToken);

            if (vacancy == null)
            {
                return RecruitmentResult<CandidateApplicationResponse>.NotFound("Lowongan kerja tidak ditemukan.");
            }

            // Aturan Bisnis VR-REC-07: Lowongan harus berstatus Published
            if (vacancy.Status != JobVacancyStatus.Published)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Unprocessable("Lamaran hanya dapat diajukan pada lowongan kerja yang berstatus Published (Aktif).");
            }

            if (vacancy.ClosingDate.HasValue && vacancy.ClosingDate.Value < DateTime.UtcNow)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Unprocessable("Lowongan kerja ini sudah melewati batas waktu penutupan pendaftaran.");
            }

            // Validasi anti-duplikasi lamaran untuk lowongan yang sama
            var alreadyApplied = await _dbContext.HrdCandidateApplications
                .AnyAsync(a => a.CandidateId == candidateId && a.VacancyId == request.VacancyId && !a.IsDelete, cancellationToken);

            if (alreadyApplied)
            {
                return RecruitmentResult<CandidateApplicationResponse>.Conflict("Kandidat sudah pernah mengajukan lamaran untuk lowongan kerja ini.");
            }

            // Alokasi ApplicationNumber atomik melalui NumberSeriesAllocator sebelum membuka transaksi (QBE-CODE-003/006)
            var applicationNumber = await AllocateApplicationNumberAsync(userId, cancellationToken);

            await using var transaction = _dbContext.Database.IsRelational() && _dbContext.Database.CurrentTransaction == null
                ? await _dbContext.Database.BeginTransactionAsync(cancellationToken)
                : null;

            var application = new HrdCandidateApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = applicationNumber,
                CandidateId = candidateId,
                VacancyId = request.VacancyId,
                ApplicationDate = DateTime.UtcNow,
                CurrentStage = ApplicationStage.Screening,
                StageStatus = "Submitted",
                Notes = request.Notes?.Trim(),
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            // Auto-screening Pending atomik per ApplicationId (BE-HRD-FIN-REC-002/003)
            var initialScreening = RecruitmentJourneyPolicy.CreateInitialPendingScreening(
                application.Id,
                vacancy.IsClinical,
                userId);

            // Update status kandidat jika masih Registered -> Active
            if (candidate.Status == CandidateStatus.Registered)
            {
                candidate.Status = CandidateStatus.Active;
                candidate.RowVersion = Guid.NewGuid();
                candidate.UpdateDateTime = DateTime.UtcNow;
                candidate.UpdateBy = userId;
            }

            await _dbContext.HrdCandidateApplications.AddAsync(application, cancellationToken);
            await _dbContext.HrdCandidateScreenings.AddAsync(initialScreening, cancellationToken);
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                return RecruitmentResult<CandidateApplicationResponse>.Conflict("Kandidat sudah pernah mengajukan lamaran untuk lowongan kerja ini.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }
                return RecruitmentResult<CandidateApplicationResponse>.Conflict("Data kandidat sedang diperbarui oleh sesi lain. Silakan muat ulang dan coba kembali.");
            }

            var response = new CandidateApplicationResponse
            {
                Id = application.Id,
                ApplicationNumber = application.ApplicationNumber,
                VacancyId = application.VacancyId,
                VacancyTitle = vacancy.Title,
                ApplicationDate = application.ApplicationDate,
                CurrentStage = application.CurrentStage,
                StageStatus = application.StageStatus,
                Notes = application.Notes
            };

            return RecruitmentResult<CandidateApplicationResponse>.Ok(response, "Lamaran pekerjaan berhasil diajukan.");
        }

        public async Task<RecruitmentResult<CandidateDocumentResponse>> UploadDocumentAsync(
            Guid candidateId,
            UploadCandidateDocumentRequest request,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty)
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Identitas user pada token tidak valid.", 401);
            }

            if (!Enum.IsDefined(typeof(DocumentType), request.DocumentType))
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Jenis dokumen kandidat tidak valid.", 400);
            }

            if (string.IsNullOrWhiteSpace(request.DocumentTitle))
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Judul dokumen wajib diisi.", 400);
            }

            if (string.IsNullOrWhiteSpace(request.FilePath))
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Path lampiran dokumen wajib diisi.", 400);
            }

            if (request.ValidFrom.HasValue && request.ValidUntil.HasValue && request.ValidFrom.Value > request.ValidUntil.Value)
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Tanggal mulai berlaku dokumen tidak boleh melebihi tanggal akhir berlaku.", 400);
            }

            var candidateExists = await _dbContext.HrdCandidates
                .AnyAsync(c => c.Id == candidateId && !c.IsDelete, cancellationToken);

            if (!candidateExists)
            {
                return RecruitmentResult<CandidateDocumentResponse>.NotFound("Kandidat tidak ditemukan.");
            }

            if (request.ApplicationId.HasValue)
            {
                var appExists = await _dbContext.HrdCandidateApplications
                    .AnyAsync(a => a.Id == request.ApplicationId.Value && a.CandidateId == candidateId && !a.IsDelete, cancellationToken);
                if (!appExists)
                {
                    return RecruitmentResult<CandidateDocumentResponse>.NotFound("Lamaran yang diasosiasikan tidak ditemukan untuk kandidat ini.");
                }
            }

            var cleanPath = request.FilePath.Trim();
            HrAttachmentFile attachment;
            try
            {
                attachment = _attachmentStorage.Open(cleanPath, CandidateDocumentAttachmentCategory);
            }
            catch (KeyNotFoundException)
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Lampiran berkas tidak ditemukan atau bukan berasal dari unggahan kandidat yang sah.", 400);
            }

            if (attachment.OwnerUserId != userId)
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Anda tidak berwenang menautkan berkas lampiran milik pengguna lain.", 403);
            }

            // Idempotensi pada retry metadata (UAT-13): key storage yang sama tidak membentuk baris ganda.
            var existingDoc = await _dbContext.HrdCandidateDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.CandidateId == candidateId && d.FilePath == cleanPath && !d.IsDelete, cancellationToken);

            if (existingDoc != null)
            {
                return RecruitmentResult<CandidateDocumentResponse>.Ok(MapDocumentToResponse(existingDoc), "Dokumen pelamar sudah tercatat.");
            }

            var actualSize = new FileInfo(attachment.PhysicalPath).Length;
            if (actualSize <= 0 || actualSize > _attachmentStorage.MaximumFileSizeBytes)
            {
                return RecruitmentResult<CandidateDocumentResponse>.Fail("Ukuran berkas lampiran tidak valid atau melebihi batas maksimal 10 MB.", 400);
            }

            var doc = new HrdCandidateDocument
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                ApplicationId = request.ApplicationId,
                DocumentType = request.DocumentType,
                DocumentNumber = request.DocumentNumber?.Trim(),
                DocumentTitle = request.DocumentTitle.Trim(),
                FilePath = cleanPath,
                FileSize = actualSize,
                MimeType = string.IsNullOrWhiteSpace(request.MimeType) ? attachment.ContentType : request.MimeType.Trim(),
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                IsVerified = false,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = userId
            };

            await _dbContext.HrdCandidateDocuments.AddAsync(doc, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return RecruitmentResult<CandidateDocumentResponse>.Ok(MapDocumentToResponse(doc), "Dokumen pelamar berhasil disimpan.");
        }

        private async Task<string> AllocateCandidateCodeAsync(Guid actorUserId, CancellationToken cancellationToken)
        {
            var instant = DateTimeOffset.UtcNow;
            var dateCode = GetBusinessDate(instant).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"{CandidateCodePrefix}-{dateCode}-";

            var existingNumbers = await _dbContext.HrdCandidates
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(c => c.CandidateCode.StartsWith(prefix))
                .Select(c => c.CandidateCode)
                .ToListAsync(cancellationToken);

            var legacyHighWatermark = ExtractMaxSequence(existingNumbers, prefix.Length);

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: CandidateCodeSequenceKey,
                    Prefix: CandidateCodePrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Daily,
                    SequenceDigits: 4,
                    ActorUserId: actorUserId,
                    Instant: instant,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        private async Task<string> AllocateApplicationNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
        {
            var instant = DateTimeOffset.UtcNow;
            var dateCode = GetBusinessDate(instant).ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefix = $"{ApplicationNumberPrefix}-{dateCode}-";

            var existingNumbers = await _dbContext.HrdCandidateApplications
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(a => a.ApplicationNumber.StartsWith(prefix))
                .Select(a => a.ApplicationNumber)
                .ToListAsync(cancellationToken);

            var legacyHighWatermark = ExtractMaxSequence(existingNumbers, prefix.Length);

            return await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: ApplicationNumberSequenceKey,
                    Prefix: ApplicationNumberPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Daily,
                    SequenceDigits: 4,
                    ActorUserId: actorUserId,
                    Instant: instant,
                    MinimumValue: legacyHighWatermark),
                cancellationToken);
        }

        private static long ExtractMaxSequence(IEnumerable<string> numbers, int prefixLength)
        {
            return numbers
                .Select(n => n.Length > prefixLength && long.TryParse(n[prefixLength..], NumberStyles.None, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0L)
                .DefaultIfEmpty(0L)
                .Max();
        }

        private static string? ValidateProfileFields(
            string fullName,
            string? gender,
            DateOnly birthDate,
            string email,
            string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "Nama lengkap wajib diisi.";
            }

            if (!string.IsNullOrWhiteSpace(gender) && gender.Trim().Length > 10)
            {
                return "Jenis kelamin maksimal 10 karakter.";
            }

            if (birthDate == default || birthDate.Year < 1900 || birthDate > GetBusinessDate(DateTimeOffset.UtcNow))
            {
                return "Tanggal lahir tidak valid atau tidak boleh melebihi hari ini.";
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return "Email wajib diisi.";
            }

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return "Nomor telepon wajib diisi.";
            }

            return null;
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }

            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
        }

        private static DateOnly GetBusinessDate(DateTimeOffset instant)
        {
            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }

            return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(instant, tz).DateTime);
        }

        private static CandidateDocumentResponse MapDocumentToResponse(HrdCandidateDocument d)
        {
            return new CandidateDocumentResponse
            {
                Id = d.Id,
                CandidateId = d.CandidateId,
                ApplicationId = d.ApplicationId,
                DocumentType = d.DocumentType,
                DocumentNumber = d.DocumentNumber,
                DocumentTitle = d.DocumentTitle,
                FilePath = d.FilePath,
                FileSize = d.FileSize,
                MimeType = d.MimeType,
                ValidFrom = d.ValidFrom,
                ValidUntil = d.ValidUntil,
                IsVerified = d.IsVerified,
                VerifiedBy = d.VerifiedBy,
                VerifiedAt = d.VerifiedAt
            };
        }

        private static CandidateResponse MapToResponse(HrdCandidate c, string? previousEmployeeNumber)
        {
            return new CandidateResponse
            {
                Id = c.Id,
                CandidateCode = c.CandidateCode,
                NationalIdNumber = c.NationalIdNumber,
                MaskedNationalIdNumber = RecruitmentJourneyPolicy.MaskNationalIdNumber(c.NationalIdNumber),
                FullName = c.FullName,
                Gender = c.Gender,
                BirthDate = c.BirthDate,
                BirthPlace = c.BirthPlace,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                IsRehire = c.IsRehire,
                PreviousWorkforceProfileId = c.PreviousWorkforceProfileId,
                PreviousEmployeeNumber = previousEmployeeNumber,
                Status = c.Status,
                RowVersion = c.RowVersion,
                CreateDateTime = c.CreateDateTime,
                UpdateDateTime = c.UpdateDateTime,
                Applications = c.Applications
                    .Where(a => !a.IsDelete)
                    .OrderByDescending(a => a.ApplicationDate)
                    .Select(a => new CandidateApplicationResponse
                    {
                        Id = a.Id,
                        ApplicationNumber = a.ApplicationNumber,
                        VacancyId = a.VacancyId,
                        VacancyTitle = a.Vacancy?.Title,
                        ApplicationDate = a.ApplicationDate,
                        CurrentStage = a.CurrentStage,
                        StageStatus = a.StageStatus,
                        Notes = a.Notes
                    }).ToList(),
                Documents = c.Documents
                    .Where(d => !d.IsDelete)
                    .OrderByDescending(d => d.CreateDateTime)
                    .Select(MapDocumentToResponse)
                    .ToList()
            };
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var actorId = user.GetUserId();
            if (actorId != Guid.Empty)
            {
                return actorId;
            }

            var sub = user.FindFirst("sub")?.Value;
            return Guid.TryParse(sub, out var guid) ? guid : Guid.Empty;
        }
    }
}
