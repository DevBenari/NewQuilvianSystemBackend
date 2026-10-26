using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Services
{
    /// <summary>
    /// Service pengelola kesepakatan tarif sharing (MdfSharingAgreement) dan baris aturan tarif berversi (MdfSharingRule)
    /// sesuai spesifikasi MDF-DES-008, MDF-DES-009, MF-DEC-004, MF-DEC-012, dan matriks validasi MDF-VAL-1.0 §2-3.
    /// </summary>
    public class MedicalFeeSharingService
    {
        private readonly ApplicationDbContext _dbContext;

        public MedicalFeeSharingService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Query Operations
        // =====================================================================

        /// <summary>
        /// Mengambil daftar kesepakatan tarif sharing terpaginasi.
        /// </summary>
        public async Task<PagedResult<SharingAgreementHeaderResponse>> GetPagedAsync(
            SharingAgreementPagedQuery query,
            CancellationToken ct = default)
        {
            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize < 1 ? 25 : (query.PageSize > 200 ? 200 : query.PageSize);

            var q = _dbContext.MdfSharingAgreements
                .AsNoTracking()
                .Include(x => x.SourceContractHistory)
                .Where(x => !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keyword = query.Search.Trim().ToLower();
                q = q.Where(x =>
                    x.AgreementNumber.ToLower().Contains(keyword) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)) ||
                    (x.SourceContractHistory != null && x.SourceContractHistory.ContractNumber.ToLower().Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(query.PayeeType))
            {
                q = q.Where(x => x.PayeeType == query.PayeeType);
            }

            if (query.PayeeReferenceId.HasValue)
            {
                q = q.Where(x => x.PayeeReferenceId == query.PayeeReferenceId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(x => x.Status == query.Status);
            }

            if (query.EffectiveOn.HasValue)
            {
                var d = query.EffectiveOn.Value;
                q = q.Where(x => x.EffectiveStart <= d && (x.EffectiveEnd == null || x.EffectiveEnd >= d));
            }

            var totalData = await q.CountAsync(ct);

            var sortDirDesc = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "status" => sortDirDesc ? q.OrderByDescending(x => x.Status) : q.OrderBy(x => x.Status),
                "effectivestart" => sortDirDesc ? q.OrderByDescending(x => x.EffectiveStart) : q.OrderBy(x => x.EffectiveStart),
                "effectiveend" => sortDirDesc ? q.OrderByDescending(x => x.EffectiveEnd) : q.OrderBy(x => x.EffectiveEnd),
                "createdatetime" => sortDirDesc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
                _ => sortDirDesc ? q.OrderByDescending(x => x.AgreementNumber) : q.OrderBy(x => x.AgreementNumber),
            };

            var rawItems = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.AgreementNumber,
                    x.SourceContractHistoryId,
                    ContractNumber = x.SourceContractHistory != null ? x.SourceContractHistory.ContractNumber : string.Empty,
                    x.PayeeType,
                    x.PayeeReferenceId,
                    x.EffectiveStart,
                    x.EffectiveEnd,
                    x.Status,
                    x.Notes,
                    RulesCount = x.SharingRules.Count(r => !r.IsDelete),
                    x.RowVersion,
                    x.CreateDateTime,
                    x.CreateBy,
                    x.UpdateDateTime,
                    x.UpdateBy
                })
                .ToListAsync(ct);

            // Resolve Payee Names efficiently
            var doctorIds = rawItems.Where(x => x.PayeeType == MdfPayeeTypes.Doctor).Select(x => x.PayeeReferenceId).Distinct().ToList();
            var workforceIds = rawItems.Where(x => x.PayeeType == MdfPayeeTypes.Workforce).Select(x => x.PayeeReferenceId).Distinct().ToList();

            var doctorNames = doctorIds.Count > 0
                ? await _dbContext.MstDoctors.AsNoTracking().Where(d => doctorIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => (d.FullName, d.DoctorCode), ct)
                : new();

            var workforceNames = workforceIds.Count > 0
                ? await _dbContext.MstWorkforceProfiles.AsNoTracking().Where(w => workforceIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => (w.DisplayName, w.ProfileCode), ct)
                : new();

            var items = rawItems.Select(x =>
            {
                string payeeName = string.Empty;
                string? payeeCode = null;

                if (x.PayeeType == MdfPayeeTypes.Doctor && doctorNames.TryGetValue(x.PayeeReferenceId, out var docInfo))
                {
                    payeeName = docInfo.FullName;
                    payeeCode = docInfo.DoctorCode;
                }
                else if (x.PayeeType == MdfPayeeTypes.Workforce && workforceNames.TryGetValue(x.PayeeReferenceId, out var wfInfo))
                {
                    payeeName = wfInfo.DisplayName;
                    payeeCode = wfInfo.ProfileCode;
                }

                return new SharingAgreementHeaderResponse
                {
                    Id = x.Id,
                    AgreementNumber = x.AgreementNumber,
                    SourceContractHistoryId = x.SourceContractHistoryId,
                    ContractNumber = x.ContractNumber,
                    PayeeType = x.PayeeType,
                    PayeeReferenceId = x.PayeeReferenceId,
                    PayeeName = payeeName,
                    PayeeCode = payeeCode,
                    EffectiveStart = x.EffectiveStart,
                    EffectiveEnd = x.EffectiveEnd,
                    Status = x.Status,
                    Notes = x.Notes,
                    RulesCount = x.RulesCount,
                    RowVersion = x.RowVersion,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy
                };
            }).ToList();

            return new PagedResult<SharingAgreementHeaderResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Mengambil detail satu kesepakatan beserta seluruh baris aturan tarifnya.
        /// </summary>
        public async Task<SharingAgreementDetailResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _dbContext.MdfSharingAgreements
                .AsNoTracking()
                .Include(x => x.SourceContractHistory)
                .Include(x => x.SharingRules)
                    .ThenInclude(r => r.Role)
                .Include(x => x.SharingRules)
                    .ThenInclude(r => r.Tariff)
                .Include(x => x.SharingRules)
                    .ThenInclude(r => r.TariffCategory)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null) return null;

            string payeeName = string.Empty;
            string? payeeCode = null;

            if (entity.PayeeType == MdfPayeeTypes.Doctor)
            {
                var doc = await _dbContext.MstDoctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == entity.PayeeReferenceId, ct);
                if (doc != null) { payeeName = doc.FullName; payeeCode = doc.DoctorCode; }
            }
            else
            {
                var wf = await _dbContext.MstWorkforceProfiles.AsNoTracking().FirstOrDefaultAsync(w => w.Id == entity.PayeeReferenceId, ct);
                if (wf != null) { payeeName = wf.DisplayName; payeeCode = wf.ProfileCode; }
            }

            var rules = entity.SharingRules
                .Where(r => !r.IsDelete)
                .OrderByDescending(r => r.EffectiveStart)
                .ThenBy(r => r.SupersededByRuleId.HasValue)
                .Select(r => new SharingRuleResponse
                {
                    Id = r.Id,
                    AgreementId = r.AgreementId,
                    TariffId = r.TariffId,
                    TariffCode = r.Tariff?.TariffCode,
                    TariffName = r.Tariff?.TariffName,
                    TariffCategoryId = r.TariffCategoryId,
                    TariffCategoryName = r.TariffCategory?.TariffCategoryName,
                    RoleId = r.RoleId,
                    RoleCode = r.Role?.RoleCode ?? string.Empty,
                    RoleName = r.Role?.RoleName ?? string.Empty,
                    SharingPercentage = r.SharingPercentage,
                    EffectiveStart = r.EffectiveStart,
                    EffectiveEnd = r.EffectiveEnd,
                    SupersededByRuleId = r.SupersededByRuleId,
                    Notes = r.Notes
                }).ToList();

            return new SharingAgreementDetailResponse
            {
                Id = entity.Id,
                AgreementNumber = entity.AgreementNumber,
                SourceContractHistoryId = entity.SourceContractHistoryId,
                ContractNumber = entity.SourceContractHistory?.ContractNumber ?? string.Empty,
                ContractStartDate = entity.SourceContractHistory?.StartDate,
                ContractEndDate = entity.SourceContractHistory?.EndDate,
                ContractStatus = entity.SourceContractHistory?.ContractStatus,
                PayeeType = entity.PayeeType,
                PayeeReferenceId = entity.PayeeReferenceId,
                PayeeName = payeeName,
                PayeeCode = payeeCode,
                EffectiveStart = entity.EffectiveStart,
                EffectiveEnd = entity.EffectiveEnd,
                Status = entity.Status,
                Notes = entity.Notes,
                RulesCount = rules.Count,
                RowVersion = entity.RowVersion,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy,
                Rules = rules
            };
        }

        /// <summary>
        /// Mengambil baris aturan tarif dari kesepakatan, opsional disaring pada tanggal tertentu.
        /// </summary>
        public async Task<List<SharingRuleResponse>> GetRulesAsync(
            Guid agreementId,
            DateOnly? effectiveOn = null,
            CancellationToken ct = default)
        {
            var q = _dbContext.MdfSharingRules
                .AsNoTracking()
                .Include(r => r.Role)
                .Include(r => r.Tariff)
                .Include(r => r.TariffCategory)
                .Where(r => r.AgreementId == agreementId && !r.IsDelete);

            if (effectiveOn.HasValue)
            {
                var d = effectiveOn.Value;
                q = q.Where(r => r.EffectiveStart <= d && (r.EffectiveEnd == null || r.EffectiveEnd >= d));
            }

            return await q
                .OrderByDescending(r => r.EffectiveStart)
                .Select(r => new SharingRuleResponse
                {
                    Id = r.Id,
                    AgreementId = r.AgreementId,
                    TariffId = r.TariffId,
                    TariffCode = r.Tariff != null ? r.Tariff.TariffCode : null,
                    TariffName = r.Tariff != null ? r.Tariff.TariffName : null,
                    TariffCategoryId = r.TariffCategoryId,
                    TariffCategoryName = r.TariffCategory != null ? r.TariffCategory.TariffCategoryName : null,
                    RoleId = r.RoleId,
                    RoleCode = r.Role != null ? r.Role.RoleCode : string.Empty,
                    RoleName = r.Role != null ? r.Role.RoleName : string.Empty,
                    SharingPercentage = r.SharingPercentage,
                    EffectiveStart = r.EffectiveStart,
                    EffectiveEnd = r.EffectiveEnd,
                    SupersededByRuleId = r.SupersededByRuleId,
                    Notes = r.Notes
                })
                .ToListAsync(ct);
        }

        /// <summary>
        /// Mengambil ringkasan statistik kesepakatan tarif sharing.
        /// </summary>
        public async Task<SharingAgreementSummaryResponse> GetSummaryAsync(CancellationToken ct = default)
        {
            var rows = await _dbContext.MdfSharingAgreements
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Select(x => new { x.Status, x.PayeeType })
                .ToListAsync(ct);

            return new SharingAgreementSummaryResponse
            {
                TotalAgreements = rows.Count,
                ActiveAgreements = rows.Count(x => x.Status == MdfSharingAgreementStatuses.Active),
                DraftAgreements = rows.Count(x => x.Status == MdfSharingAgreementStatuses.Draft),
                TerminatedAgreements = rows.Count(x => x.Status == MdfSharingAgreementStatuses.Terminated),
                ExpiredAgreements = rows.Count(x => x.Status == MdfSharingAgreementStatuses.Expired),
                DoctorAgreements = rows.Count(x => x.PayeeType == MdfPayeeTypes.Doctor),
                WorkforceAgreements = rows.Count(x => x.PayeeType == MdfPayeeTypes.Workforce)
            };
        }

        /// <summary>
        /// Opsi ringan kesepakatan untuk dropdown.
        /// </summary>
        public async Task<List<SharingAgreementOptionResponse>> GetOptionsAsync(CancellationToken ct = default)
        {
            var agreements = await _dbContext.MdfSharingAgreements
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == MdfSharingAgreementStatuses.Active)
                .OrderBy(x => x.AgreementNumber)
                .Select(x => new { x.Id, x.AgreementNumber, x.PayeeType, x.PayeeReferenceId, x.Status })
                .ToListAsync(ct);

            var doctorIds = agreements.Where(x => x.PayeeType == MdfPayeeTypes.Doctor).Select(x => x.PayeeReferenceId).Distinct().ToList();
            var workforceIds = agreements.Where(x => x.PayeeType == MdfPayeeTypes.Workforce).Select(x => x.PayeeReferenceId).Distinct().ToList();

            var doctorNames = doctorIds.Count > 0
                ? await _dbContext.MstDoctors.AsNoTracking().Where(d => doctorIds.Contains(d.Id)).ToDictionaryAsync(d => d.Id, d => d.FullName, ct)
                : new();

            var workforceNames = workforceIds.Count > 0
                ? await _dbContext.MstWorkforceProfiles.AsNoTracking().Where(w => workforceIds.Contains(w.Id)).ToDictionaryAsync(w => w.Id, w => w.DisplayName, ct)
                : new();

            return agreements.Select(x =>
            {
                var payeeName = x.PayeeType == MdfPayeeTypes.Doctor
                    ? (doctorNames.TryGetValue(x.PayeeReferenceId, out var doc) ? doc : "Dokter")
                    : (workforceNames.TryGetValue(x.PayeeReferenceId, out var wf) ? wf : "Tenaga Kerja");

                return new SharingAgreementOptionResponse
                {
                    Id = x.Id,
                    AgreementNumber = x.AgreementNumber,
                    PayeeType = x.PayeeType,
                    PayeeName = payeeName,
                    Status = x.Status
                };
            }).ToList();
        }

        /// <summary>
        /// Metadata filter untuk antarmuka pengguna.
        /// </summary>
        public SharingAgreementFilterMetadataResponse GetFilterMetadata()
            => new();

        // =====================================================================
        // Command Operations — Kesepakatan
        // =====================================================================

        /// <summary>
        /// Menyusun kesepakatan tarif sharing baru — aturan MDF-VAL-1.0 §2.
        /// </summary>
        public async Task<MedicalFeeServiceResult<SharingAgreementDetailResponse>> CreateAsync(
            CreateSharingAgreementRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var agreementNumber = request.AgreementNumber.Trim();

            // Aturan 2.1: AgreementNumber unik di antara baris yang belum dihapus
            var isDuplicate = await _dbContext.MdfSharingAgreements
                .AnyAsync(x => !x.IsDelete && x.AgreementNumber == agreementNumber, ct);

            if (isDuplicate)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Nomor kesepakatan '{agreementNumber}' sudah digunakan.",
                    "MDF_AGREEMENT_NUMBER_DUPLICATE");
            }

            // Aturan 2.2: Rujukan kontrak HR wajib ada
            var contract = await _dbContext.WfpContractHistories
                .FirstOrDefaultAsync(x => x.Id == request.SourceContractHistoryId && !x.IsDelete, ct);

            if (contract == null)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Data riwayat kontrak HR yang ditunjuk tidak ditemukan atau sudah dihapus.");
            }

            // Aturan 2.3 - 2.5: Validasi kecocokan payeeReferenceId dengan kontrak HR
            string payeeName = string.Empty;
            if (request.PayeeType == MdfPayeeTypes.Doctor)
            {
                var doctor = await _dbContext.MstDoctors
                    .FirstOrDefaultAsync(d => d.Id == request.PayeeReferenceId && !d.IsDelete, ct);

                if (doctor == null)
                {
                    return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Data dokter penerima hak tidak ditemukan.");
                }

                // Aturan 2.5: Wajib cocok dengan tenaga kerja pada kontrak yang ditunjuk
                if (doctor.WorkforceProfileId != contract.WorkforceProfileId)
                {
                    return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        "Dokter yang dipilih tidak cocok dengan pemilik kontrak HR yang ditunjuk.");
                }
                payeeName = doctor.FullName;
            }
            else if (request.PayeeType == MdfPayeeTypes.Workforce)
            {
                var wf = await _dbContext.MstWorkforceProfiles
                    .FirstOrDefaultAsync(w => w.Id == request.PayeeReferenceId && !w.IsDelete, ct);

                if (wf == null)
                {
                    return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Data tenaga kerja penerima hak tidak ditemukan.");
                }

                if (wf.Id != contract.WorkforceProfileId)
                {
                    return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        "Tenaga kerja yang dipilih tidak cocok dengan pemilik kontrak HR yang ditunjuk.");
                }
                payeeName = wf.DisplayName;
            }

            // Aturan 2.8: EffectiveEnd >= EffectiveStart
            if (request.EffectiveEnd.HasValue && request.EffectiveEnd.Value < request.EffectiveStart)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal akhir kesepakatan tidak boleh mendahului tanggal awal.");
            }

            // Aturan 2.6: EffectiveStart tidak boleh mendahului StartDate kontrak HR
            var contractStart = DateOnly.FromDateTime(contract.StartDate);
            if (request.EffectiveStart < contractStart)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Tanggal awal kesepakatan ({request.EffectiveStart:yyyy-MM-dd}) tidak boleh mendahului tanggal awal kontrak HR ({contractStart:yyyy-MM-dd}).",
                    "MDF_AGREEMENT_CONTRACT_MISMATCH");
            }

            // Aturan 2.7: EffectiveEnd tidak boleh melewati EndDate kontrak HR (bila kontrak punya akhir)
            if (contract.EndDate.HasValue && request.EffectiveEnd.HasValue)
            {
                var contractEnd = DateOnly.FromDateTime(contract.EndDate.Value);
                if (request.EffectiveEnd.Value > contractEnd)
                {
                    return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Tanggal akhir kesepakatan ({request.EffectiveEnd.Value:yyyy-MM-dd}) tidak boleh melewati tanggal akhir kontrak HR ({contractEnd:yyyy-MM-dd}).",
                        "MDF_AGREEMENT_CONTRACT_MISMATCH");
                }
            }

            var agreementId = Guid.NewGuid();
            var agreement = new MdfSharingAgreement
            {
                Id = agreementId,
                AgreementNumber = agreementNumber,
                SourceContractHistoryId = request.SourceContractHistoryId,
                PayeeType = request.PayeeType,
                PayeeReferenceId = request.PayeeReferenceId,
                EffectiveStart = request.EffectiveStart,
                EffectiveEnd = request.EffectiveEnd,
                Status = MdfSharingAgreementStatuses.Draft,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            // Validasi dan persiapkan initial rules bila disertakan
            var rulesToInsert = new List<MdfSharingRule>();
            if (request.Rules.Count > 0)
            {
                foreach (var item in request.Rules)
                {
                    var ruleValidation = await ValidateRuleAsync(
                        agreementId,
                        item.TariffId,
                        item.TariffCategoryId,
                        item.RoleId,
                        item.SharingPercentage,
                        item.EffectiveStart,
                        item.EffectiveEnd,
                        request.EffectiveStart,
                        request.EffectiveEnd,
                        excludeRuleId: null,
                        ct);

                    if (!ruleValidation.Success)
                    {
                        return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                            ruleValidation.StatusCode,
                            ruleValidation.Message,
                            ruleValidation.ErrorCode);
                    }

                    rulesToInsert.Add(new MdfSharingRule
                    {
                        Id = Guid.NewGuid(),
                        AgreementId = agreementId,
                        TariffId = item.TariffId,
                        TariffCategoryId = item.TariffCategoryId,
                        RoleId = item.RoleId,
                        SharingPercentage = item.SharingPercentage,
                        EffectiveStart = item.EffectiveStart,
                        EffectiveEnd = item.EffectiveEnd,
                        Notes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim(),
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    });
                }
            }

            using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                _dbContext.MdfSharingAgreements.Add(agreement);
                if (rulesToInsert.Count > 0)
                {
                    _dbContext.MdfSharingRules.AddRange(rulesToInsert);
                }
                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Gagal menyimpan kesepakatan tarif sharing: {ex.Message}");
            }

            var detail = await GetByIdAsync(agreementId, ct);
            return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Ok(
                detail!,
                "Kesepakatan tarif sharing berhasil dibuat.",
                StatusCodes.Status201Created);
        }

        /// <summary>
        /// Mengubah kepala kesepakatan tarif sharing — PUT /master-data/sharing-agreements/{id}.
        /// </summary>
        public async Task<MedicalFeeServiceResult<SharingAgreementDetailResponse>> UpdateAsync(
            Guid id,
            UpdateSharingAgreementRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MdfSharingAgreements
                .Include(x => x.SourceContractHistory)
                .Include(x => x.SharingRules)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kesepakatan tarif sharing tidak ditemukan atau sudah dihapus.");
            }

            if (entity.Status == MdfSharingAgreementStatuses.Terminated || entity.Status == MdfSharingAgreementStatuses.Expired)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Kesepakatan berstatus {entity.Status} tidak dapat diubah.");
            }

            // Concurrency check
            if (request.ExpectedRowVersion.HasValue && entity.RowVersion != request.ExpectedRowVersion.Value)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data kesepakatan telah diperbarui oleh pengguna lain. Silakan muat ulang halaman.",
                    "MDF_ROWVERSION_MISMATCH");
            }

            var agreementNumber = request.AgreementNumber.Trim();
            var isDuplicate = await _dbContext.MdfSharingAgreements
                .AnyAsync(x => x.Id != id && !x.IsDelete && x.AgreementNumber == agreementNumber, ct);

            if (isDuplicate)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Nomor kesepakatan '{agreementNumber}' sudah digunakan oleh kesepakatan lain.",
                    "MDF_AGREEMENT_NUMBER_DUPLICATE");
            }

            if (request.EffectiveEnd.HasValue && request.EffectiveEnd.Value < request.EffectiveStart)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal akhir kesepakatan tidak boleh mendahului tanggal awal.");
            }

            // Validasi terhadap kontrak HR
            if (entity.SourceContractHistory != null)
            {
                var contractStart = DateOnly.FromDateTime(entity.SourceContractHistory.StartDate);
                if (request.EffectiveStart < contractStart)
                {
                    return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Tanggal awal kesepakatan ({request.EffectiveStart:yyyy-MM-dd}) tidak boleh mendahului tanggal awal kontrak HR ({contractStart:yyyy-MM-dd}).",
                        "MDF_AGREEMENT_CONTRACT_MISMATCH");
                }

                if (entity.SourceContractHistory.EndDate.HasValue && request.EffectiveEnd.HasValue)
                {
                    var contractEnd = DateOnly.FromDateTime(entity.SourceContractHistory.EndDate.Value);
                    if (request.EffectiveEnd.Value > contractEnd)
                    {
                        return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            $"Tanggal akhir kesepakatan ({request.EffectiveEnd.Value:yyyy-MM-dd}) tidak boleh melewati tanggal akhir kontrak HR ({contractEnd:yyyy-MM-dd}).",
                            "MDF_AGREEMENT_CONTRACT_MISMATCH");
                    }
                }
            }

            // Aturan 3.9: Seluruh baris aturan aktif harus tetap berada dalam masa berlaku baru
            var invalidRule = entity.SharingRules.FirstOrDefault(r =>
                !r.IsDelete &&
                (r.EffectiveStart < request.EffectiveStart ||
                (request.EffectiveEnd.HasValue &&
                 (r.EffectiveStart > request.EffectiveEnd.Value ||
                  (r.EffectiveEnd.HasValue && r.EffectiveEnd.Value > request.EffectiveEnd.Value)))));

            if (invalidRule != null)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Terdapat baris aturan tarif yang berada di luar rentang tanggal kesepakatan yang baru. Sesuaikan aturan tarif terlebih dahulu.",
                    "MDF_RULE_OUT_OF_BOUNDS");
            }

            entity.AgreementNumber = agreementNumber;
            entity.EffectiveStart = request.EffectiveStart;
            entity.EffectiveEnd = request.EffectiveEnd;
            entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);

            var detail = await GetByIdAsync(id, ct);
            return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Ok(
                detail!,
                "Kepala kesepakatan tarif sharing berhasil diperbarui.");
        }

        /// <summary>
        /// Mengaktifkan kesepakatan tarif sharing — POST /master-data/sharing-agreements/{id}/activation.
        /// </summary>
        public async Task<MedicalFeeServiceResult<SharingAgreementDetailResponse>> ActivateAsync(
            Guid id,
            ActivateSharingAgreementRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MdfSharingAgreements
                .Include(x => x.SourceContractHistory)
                .Include(x => x.SharingRules)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kesepakatan tarif sharing tidak ditemukan atau sudah dihapus.");
            }

            if (entity.Status != MdfSharingAgreementStatuses.Draft)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Hanya kesepakatan berstatus Draft yang dapat diaktifkan. Status saat ini: {entity.Status}.");
            }

            if (request.ExpectedRowVersion.HasValue && entity.RowVersion != request.ExpectedRowVersion.Value)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data kesepakatan telah diperbarui oleh pengguna lain.",
                    "MDF_ROWVERSION_MISMATCH");
            }

            // Periksa status kontrak HR
            if (entity.SourceContractHistory != null)
            {
                var invalidStatuses = new[] { "Ended", "Expired", "Terminated", "Cancelled" };
                if (invalidStatuses.Contains(entity.SourceContractHistory.ContractStatus))
                {
                    return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Kontrak HR yang dirujuk berstatus '{entity.SourceContractHistory.ContractStatus}' dan tidak lagi berlaku.");
                }
            }

            // Aturan 2.10: Menuntut sekurang-kurangnya satu baris tarif aktif
            var activeRules = entity.SharingRules.Where(r => !r.IsDelete).ToList();
            if (activeRules.Count == 0)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Kesepakatan harus memiliki sekurang-kurangnya satu baris aturan tarif sebelum diaktifkan.",
                    "MDF_AGREEMENT_NO_RULES");
            }

            // Aturan 2.9: Satu penerima tidak boleh punya dua kesepakatan Active yang tumpang-tindih
            var hasOverlap = await _dbContext.MdfSharingAgreements.AnyAsync(x =>
                x.Id != id &&
                !x.IsDelete &&
                x.PayeeType == entity.PayeeType &&
                x.PayeeReferenceId == entity.PayeeReferenceId &&
                x.Status == MdfSharingAgreementStatuses.Active &&
                (entity.EffectiveEnd == null || x.EffectiveStart <= entity.EffectiveEnd.Value) &&
                (x.EffectiveEnd == null || x.EffectiveEnd.Value >= entity.EffectiveStart), ct);

            if (hasOverlap)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Terdapat kesepakatan aktif lain untuk penerima ini yang masa berlakunya tumpang-tindih.",
                    "MDF_AGREEMENT_OVERLAP");
            }

            entity.Status = MdfSharingAgreementStatuses.Active;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);

            var detail = await GetByIdAsync(id, ct);
            return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Ok(
                detail!,
                "Kesepakatan tarif sharing berhasil diaktifkan.");
        }

        /// <summary>
        /// Menghentikan kesepakatan tarif sharing — POST /master-data/sharing-agreements/{id}/termination.
        /// </summary>
        public async Task<MedicalFeeServiceResult<SharingAgreementDetailResponse>> TerminateAsync(
            Guid id,
            TerminateSharingAgreementRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MdfSharingAgreements
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kesepakatan tarif sharing tidak ditemukan atau sudah dihapus.");
            }

            if (entity.Status != MdfSharingAgreementStatuses.Active)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Hanya kesepakatan berstatus Active yang dapat dihentikan. Status saat ini: {entity.Status}.");
            }

            if (request.ExpectedRowVersion.HasValue && entity.RowVersion != request.ExpectedRowVersion.Value)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data kesepakatan telah diperbarui oleh pengguna lain.",
                    "MDF_ROWVERSION_MISMATCH");
            }

            var termDate = request.TerminationDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            if (termDate < entity.EffectiveStart)
            {
                return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal penghentian tidak boleh mendahului tanggal awal kesepakatan.");
            }

            entity.Status = MdfSharingAgreementStatuses.Terminated;
            entity.EffectiveEnd = termDate;
            entity.Notes = string.IsNullOrWhiteSpace(entity.Notes)
                ? $"Dihentikan ({termDate:yyyy-MM-dd}): {request.Reason.Trim()}"
                : $"{entity.Notes}\nDihentikan ({termDate:yyyy-MM-dd}): {request.Reason.Trim()}";

            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(ct);

            var detail = await GetByIdAsync(id, ct);
            return MedicalFeeServiceResult<SharingAgreementDetailResponse>.Ok(
                detail!,
                "Kesepakatan tarif sharing berhasil dihentikan.");
        }

        // =====================================================================
        // Command Operations — Baris Aturan Tarif
        // =====================================================================

        /// <summary>
        /// Menambahkan satu baris aturan tarif baru ke kesepakatan — POST /master-data/sharing-agreements/{id}/rules.
        /// </summary>
        public async Task<MedicalFeeServiceResult<SharingRuleResponse>> AddRuleAsync(
            Guid agreementId,
            AddSharingRuleRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var agreement = await _dbContext.MdfSharingAgreements
                .FirstOrDefaultAsync(x => x.Id == agreementId && !x.IsDelete, ct);

            if (agreement == null)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kesepakatan tarif sharing tidak ditemukan atau sudah dihapus.");
            }

            if (agreement.Status == MdfSharingAgreementStatuses.Terminated || agreement.Status == MdfSharingAgreementStatuses.Expired)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Tidak dapat menambahkan aturan pada kesepakatan berstatus {agreement.Status}.");
            }

            if (request.ExpectedRowVersion.HasValue && agreement.RowVersion != request.ExpectedRowVersion.Value)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data kesepakatan telah diperbarui oleh pengguna lain.",
                    "MDF_ROWVERSION_MISMATCH");
            }

            var validation = await ValidateRuleAsync(
                agreementId,
                request.TariffId,
                request.TariffCategoryId,
                request.RoleId,
                request.SharingPercentage,
                request.EffectiveStart,
                request.EffectiveEnd,
                agreement.EffectiveStart,
                agreement.EffectiveEnd,
                excludeRuleId: null,
                ct);

            if (!validation.Success)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    validation.StatusCode,
                    validation.Message,
                    validation.ErrorCode);
            }

            var ruleId = Guid.NewGuid();
            var newRule = new MdfSharingRule
            {
                Id = ruleId,
                AgreementId = agreementId,
                TariffId = request.TariffId,
                TariffCategoryId = request.TariffCategoryId,
                RoleId = request.RoleId,
                SharingPercentage = request.SharingPercentage,
                EffectiveStart = request.EffectiveStart,
                EffectiveEnd = request.EffectiveEnd,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            agreement.RowVersion = Guid.NewGuid();
            agreement.UpdateDateTime = DateTime.UtcNow;
            agreement.UpdateBy = actorUserId;

            _dbContext.MdfSharingRules.Add(newRule);
            await _dbContext.SaveChangesAsync(ct);

            var createdRule = await _dbContext.MdfSharingRules
                .AsNoTracking()
                .Include(r => r.Role)
                .Include(r => r.Tariff)
                .Include(r => r.TariffCategory)
                .FirstOrDefaultAsync(r => r.Id == ruleId, ct);

            return MedicalFeeServiceResult<SharingRuleResponse>.Ok(
                MapToRuleResponse(createdRule!),
                "Baris aturan tarif berhasil ditambahkan.",
                StatusCodes.Status201Created);
        }

        /// <summary>
        /// Mengganti baris aturan tarif dengan versi baru — MDF-DES-009, MDF-API-1.0 §3.
        /// Menutup baris lama pada (effectiveStart - 1 hari), membuat baris baru, dan menautkan SupersededByRuleId.
        /// </summary>
        public async Task<MedicalFeeServiceResult<SharingRuleResponse>> SupersedeRuleAsync(
            Guid agreementId,
            Guid ruleId,
            SupersedeSharingRuleRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var agreement = await _dbContext.MdfSharingAgreements
                .FirstOrDefaultAsync(x => x.Id == agreementId && !x.IsDelete, ct);

            if (agreement == null)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kesepakatan tarif sharing tidak ditemukan atau sudah dihapus.");
            }

            if (agreement.Status == MdfSharingAgreementStatuses.Terminated || agreement.Status == MdfSharingAgreementStatuses.Expired)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Tidak dapat mengganti aturan pada kesepakatan berstatus {agreement.Status}.");
            }

            var oldRule = await _dbContext.MdfSharingRules
                .FirstOrDefaultAsync(r => r.Id == ruleId && r.AgreementId == agreementId && !r.IsDelete, ct);

            if (oldRule == null)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Baris aturan tarif yang akan diganti tidak ditemukan.");
            }

            if (oldRule.SupersededByRuleId.HasValue)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Baris aturan tarif ini sudah pernah digantikan oleh versi yang lebih baru.");
            }

            // Aturan 3.7: effectiveStart baris pengganti MUST setelah effectiveStart baris lama
            if (request.EffectiveStart <= oldRule.EffectiveStart)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Tanggal awal berlakunya tarif baru ({request.EffectiveStart:yyyy-MM-dd}) harus lebih besar dari tanggal awal aturan lama ({oldRule.EffectiveStart:yyyy-MM-dd}).",
                    "MDF_RULE_SUPERSEDE_DATE_INVALID");
            }

            // Aturan 3.9: Tanggal mulai berada dalam masa berlaku kesepakatan
            if (request.EffectiveStart < agreement.EffectiveStart ||
                (agreement.EffectiveEnd.HasValue && request.EffectiveStart > agreement.EffectiveEnd.Value))
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Tanggal awal berlakunya tarif baru berada di luar masa berlaku kesepakatan induk.",
                    "MDF_RULE_OUT_OF_BOUNDS");
            }

            // Aturan 3.6: Jumlah persentase seluruh peran pada tanggal mulai baru tidak melebihi 100%
            var otherRolesPercentage = await _dbContext.MdfSharingRules
                .Where(r =>
                    r.AgreementId == agreementId &&
                    !r.IsDelete &&
                    r.Id != oldRule.Id &&
                    r.TariffId == oldRule.TariffId &&
                    r.TariffCategoryId == oldRule.TariffCategoryId &&
                    r.EffectiveStart <= request.EffectiveStart &&
                    (r.EffectiveEnd == null || r.EffectiveEnd.Value >= request.EffectiveStart))
                .SumAsync(r => (decimal?)r.SharingPercentage, ct) ?? 0m;

            if (otherRolesPercentage + request.SharingPercentage > 100m)
            {
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Jumlah persentase seluruh peran pada tanggal {request.EffectiveStart:yyyy-MM-dd} ({otherRolesPercentage + request.SharingPercentage:F2}%) melebihi batas 100%.",
                    "MDF_RULE_PERCENTAGE_EXCEEDED");
            }

            var newRuleId = Guid.NewGuid();
            var newRule = new MdfSharingRule
            {
                Id = newRuleId,
                AgreementId = agreementId,
                TariffId = oldRule.TariffId,
                TariffCategoryId = oldRule.TariffCategoryId,
                RoleId = oldRule.RoleId,
                SharingPercentage = request.SharingPercentage,
                EffectiveStart = request.EffectiveStart,
                EffectiveEnd = oldRule.EffectiveEnd,
                Notes = string.IsNullOrWhiteSpace(request.Reason) ? oldRule.Notes : request.Reason.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            // Tutup baris lama pada (EffectiveStart baru - 1 hari)
            oldRule.EffectiveEnd = request.EffectiveStart.AddDays(-1);
            oldRule.SupersededByRuleId = newRuleId;
            oldRule.UpdateDateTime = DateTime.UtcNow;
            oldRule.UpdateBy = actorUserId;

            agreement.RowVersion = Guid.NewGuid();
            agreement.UpdateDateTime = DateTime.UtcNow;
            agreement.UpdateBy = actorUserId;

            using var tx = await _dbContext.Database.BeginTransactionAsync(ct);
            try
            {
                _dbContext.MdfSharingRules.Add(newRule);
                await _dbContext.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return MedicalFeeServiceResult<SharingRuleResponse>.Fail(
                    StatusCodes.Status500InternalServerError,
                    $"Gagal mengganti baris aturan tarif: {ex.Message}");
            }

            var createdRule = await _dbContext.MdfSharingRules
                .AsNoTracking()
                .Include(r => r.Role)
                .Include(r => r.Tariff)
                .Include(r => r.TariffCategory)
                .FirstOrDefaultAsync(r => r.Id == newRuleId, ct);

            return MedicalFeeServiceResult<SharingRuleResponse>.Ok(
                MapToRuleResponse(createdRule!),
                "Baris aturan tarif berhasil digantikan dengan versi baru.");
        }

        /// <summary>
        /// Menghapus kesepakatan secara soft delete (hanya diperbolehkan bila status masih Draft).
        /// </summary>
        public async Task<MedicalFeeServiceResult<bool>> DeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var entity = await _dbContext.MdfSharingAgreements
                .Include(x => x.SharingRules)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null)
            {
                return MedicalFeeServiceResult<bool>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kesepakatan tarif sharing tidak ditemukan atau sudah dihapus.");
            }

            if (entity.Status != MdfSharingAgreementStatuses.Draft)
            {
                return MedicalFeeServiceResult<bool>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Hanya kesepakatan berstatus Draft yang dapat dihapus. Status saat ini: {entity.Status}.");
            }

            var now = DateTime.UtcNow;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            foreach (var rule in entity.SharingRules.Where(r => !r.IsDelete))
            {
                rule.IsDelete = true;
                rule.DeleteDateTime = now;
                rule.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(ct);
            return MedicalFeeServiceResult<bool>.Ok(true, "Kesepakatan tarif sharing berhasil dihapus.");
        }

        // =====================================================================
        // Internal Validation Helpers
        // =====================================================================

        private async Task<(bool Success, int StatusCode, string Message, string? ErrorCode)> ValidateRuleAsync(
            Guid agreementId,
            Guid? tariffId,
            Guid? tariffCategoryId,
            Guid roleId,
            decimal sharingPercentage,
            DateOnly effectiveStart,
            DateOnly? effectiveEnd,
            DateOnly agreementStart,
            DateOnly? agreementEnd,
            Guid? excludeRuleId,
            CancellationToken ct)
        {
            // Aturan 3.1: Persentase antara 0 dan 100
            if (sharingPercentage < 0.00m || sharingPercentage > 100.00m)
            {
                return (false, StatusCodes.Status400BadRequest, "Persentase tarif sharing harus berada di antara 0.00 dan 100.00.", null);
            }

            if (effectiveEnd.HasValue && effectiveEnd.Value < effectiveStart)
            {
                return (false, StatusCodes.Status400BadRequest, "Tanggal akhir aturan tidak boleh mendahului tanggal awal.", null);
            }

            // Aturan 3.2: Peran wajib ada dan aktif
            var role = await _dbContext.MstMedicalFeeRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == roleId && !x.IsDelete, ct);

            if (role == null)
            {
                return (false, StatusCodes.Status404NotFound, "Peran pelaksana jasa medis tidak ditemukan.", null);
            }

            if (!role.IsActive)
            {
                return (false, StatusCodes.Status422UnprocessableEntity, $"Peran '{role.RoleCode}' sedang tidak aktif dan tidak dapat dipilih pada aturan baru.", "MDF_ROLE_INACTIVE");
            }

            // Aturan 3.3: Tarif jika diisi harus ada
            if (tariffId.HasValue)
            {
                var tariffExists = await _dbContext.MstTariffs
                    .AnyAsync(t => t.Id == tariffId.Value && !t.IsDelete, ct);

                if (!tariffExists)
                {
                    return (false, StatusCodes.Status400BadRequest, "Tarif tindakan yang dipilih tidak ditemukan.", null);
                }
            }

            // Aturan 3.4: Kategori tarif jika diisi harus ada
            if (tariffCategoryId.HasValue)
            {
                var categoryExists = await _dbContext.MstTariffCategories
                    .AnyAsync(c => c.Id == tariffCategoryId.Value && !c.IsDelete, ct);

                if (!categoryExists)
                {
                    return (false, StatusCodes.Status400BadRequest, "Kategori tarif yang dipilih tidak ditemukan.", null);
                }
            }

            // Aturan 3.9: Masa berlaku aturan harus berada di dalam masa berlaku kesepakatan
            if (effectiveStart < agreementStart ||
                (agreementEnd.HasValue && (effectiveStart > agreementEnd.Value || (effectiveEnd.HasValue && effectiveEnd.Value > agreementEnd.Value))))
            {
                return (false, StatusCodes.Status422UnprocessableEntity, "Masa berlaku baris aturan tarif harus berada di dalam masa berlaku kesepakatan induknya.", "MDF_RULE_OUT_OF_BOUNDS");
            }

            // Aturan 3.5: Dua baris lingkup dan peran sama tidak boleh tumpang-tindih
            var hasOverlap = await _dbContext.MdfSharingRules.AnyAsync(r =>
                r.AgreementId == agreementId &&
                !r.IsDelete &&
                (excludeRuleId == null || r.Id != excludeRuleId.Value) &&
                r.RoleId == roleId &&
                r.TariffId == tariffId &&
                r.TariffCategoryId == tariffCategoryId &&
                (effectiveEnd == null || r.EffectiveStart <= effectiveEnd.Value) &&
                (r.EffectiveEnd == null || r.EffectiveEnd.Value >= effectiveStart), ct);

            if (hasOverlap)
            {
                return (false, StatusCodes.Status422UnprocessableEntity, "Terdapat baris aturan tarif lain dengan lingkup dan peran yang sama yang masa berlakunya tumpang-tindih.", "MDF_RULE_OVERLAP");
            }

            // Aturan 3.6: Jumlah persentase seluruh peran pada satu lingkup dan tanggal mulai tidak melebihi 100%
            var existingSum = await _dbContext.MdfSharingRules
                .Where(r =>
                    r.AgreementId == agreementId &&
                    !r.IsDelete &&
                    (excludeRuleId == null || r.Id != excludeRuleId.Value) &&
                    r.TariffId == tariffId &&
                    r.TariffCategoryId == tariffCategoryId &&
                    r.EffectiveStart <= effectiveStart &&
                    (r.EffectiveEnd == null || r.EffectiveEnd.Value >= effectiveStart))
                .SumAsync(r => (decimal?)r.SharingPercentage, ct) ?? 0m;

            if (existingSum + sharingPercentage > 100.00m)
            {
                return (false, StatusCodes.Status422UnprocessableEntity, $"Jumlah persentase seluruh peran pada lingkup ini ({existingSum + sharingPercentage:F2}%) melebihi batas 100%.", "MDF_RULE_PERCENTAGE_EXCEEDED");
            }

            return (true, StatusCodes.Status200OK, string.Empty, null);
        }

        private static SharingRuleResponse MapToRuleResponse(MdfSharingRule r)
        {
            return new SharingRuleResponse
            {
                Id = r.Id,
                AgreementId = r.AgreementId,
                TariffId = r.TariffId,
                TariffCode = r.Tariff?.TariffCode,
                TariffName = r.Tariff?.TariffName,
                TariffCategoryId = r.TariffCategoryId,
                TariffCategoryName = r.TariffCategory?.TariffCategoryName,
                RoleId = r.RoleId,
                RoleCode = r.Role?.RoleCode ?? string.Empty,
                RoleName = r.Role?.RoleName ?? string.Empty,
                SharingPercentage = r.SharingPercentage,
                EffectiveStart = r.EffectiveStart,
                EffectiveEnd = r.EffectiveEnd,
                SupersededByRuleId = r.SupersededByRuleId,
                Notes = r.Notes
            };
        }
    }
}
