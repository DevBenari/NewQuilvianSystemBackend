using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Daftar pilihan data induk perujuk — <b>baca saja</b> (<c>LAB-DEC-035</c>,
    /// <c>BE-EXT-02</c>).
    ///
    /// <b>Kenapa berkas ini ada.</b> `BE-EXT-02` membuat <c>MstReferralInstitution</c> dan
    /// <c>MstReferralDoctor</c> beserta <c>DbSet</c>-nya, tetapi tidak membuat satu pun
    /// endpoint. Selama itu, butir Verifikasi task tersebut — *"kedua data induk dapat dipilih
    /// dari daftar"* — tidak dapat dipenuhi siapa pun, dan layar pendaftaran rujukan luar
    /// (<c>FE-LAB-05</c>) tidak punya sumber pilihan. Berkas ini menutup celah itu.
    ///
    /// <b>Jalur ubah ditambahkan <c>RJ-DOC-REV-BE-017</c></b> (<c>RJ-DOC-DEC-080</c>): pemilik
    /// memutuskan master Institusi dan Dokter Perujuk dikelola lewat layar master data standar,
    /// termasuk tanda <c>IsPartner</c>. Pemiliknya tetap Health Service Master Data; modul
    /// pemakai (Laboratorium, Rawat Jalan, IGD) tetap hanya membaca.
    /// </summary>
    public class ReferralMasterDataService
    {
        private const int MinPageSize = 1;
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public ReferralMasterDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Daftar pilihan instansi perujuk, terurut menurut nama.
        /// </summary>
        public async Task<PagedResult<ReferralInstitutionOptionResponse>> GetInstitutionOptionsAsync(
            ReferralInstitutionOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, MinPageSize, MaxPageSize);

            var source = _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.OnlyActive)
            {
                source = source.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.InstitutionCode.Contains(search) ||
                    x.InstitutionName.Contains(search));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.InstitutionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReferralInstitutionOptionResponse
                {
                    Id = x.Id,
                    InstitutionCode = x.InstitutionCode,
                    InstitutionName = x.InstitutionName,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    IsPartner = x.IsPartner,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ReferralInstitutionOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Daftar pilihan dokter perujuk, terurut menurut nama.
        ///
        /// Penyaring instansi tersedia dan sangat dianjurkan dipakai, karena pendaftaran
        /// rujukan menolak dokter yang tidak berpraktik pada instansi yang dipilih.
        /// </summary>
        public async Task<PagedResult<ReferralDoctorOptionResponse>> GetDoctorOptionsAsync(
            ReferralDoctorOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, MinPageSize, MaxPageSize);

            var source = _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.OnlyActive)
            {
                source = source.Where(x => x.IsActive);
            }

            if (query.ReferralInstitutionId.HasValue &&
                query.ReferralInstitutionId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.ReferralInstitutionId == query.ReferralInstitutionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x => x.DoctorName.Contains(search));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.DoctorName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReferralDoctorOptionResponse
                {
                    Id = x.Id,
                    ReferralInstitutionId = x.ReferralInstitutionId,
                    DoctorName = x.DoctorName,
                    InstitutionName = x.ReferralInstitution != null
                        ? x.ReferralInstitution.InstitutionName
                        : null,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ReferralDoctorOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        // =================================================================
        // Master Fasilitas Perujuk mitra — RJ-DOC-REV-BE-017, DEC-FRJ-001
        // =================================================================

        /// <summary>
        /// Feed pilihan mitra <b>layak</b> untuk Kiosk dan Pendaftaran Rawat Jalan
        /// (<c>DEC-FRJ-001</c>): aktif, mitra, dan punya perjanjian yang berlaku pada tanggal
        /// layanan. Berbeda dengan <see cref="GetInstitutionOptionsAsync"/> yang tetap dipakai
        /// Laboratorium dengan perilaku lamanya.
        /// </summary>
        public async Task<PagedResult<ReferralInstitutionOptionResponse>> GetPartnerOptionsAsync(
            ReferralPartnerOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, MinPageSize, MaxPageSize);
            var serviceDate = query.ServiceDate ?? ReferralPartnerEligibility.Today();

            var source = _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(ReferralPartnerEligibility.IsEligibleOn(serviceDate));

            var keyword = NormalizeSearch(query.Search);

            if (keyword != null)
            {
                source = source.Where(x =>
                    x.InstitutionCode.ToLower().Contains(keyword) ||
                    x.InstitutionName.ToLower().Contains(keyword));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.InstitutionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReferralInstitutionOptionResponse
                {
                    Id = x.Id,
                    InstitutionCode = x.InstitutionCode,
                    InstitutionName = x.InstitutionName,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    IsPartner = x.IsPartner,
                    IsActive = x.IsActive,
                    AgreementEndDate = x.Agreements
                        .Where(a => !a.IsDelete && a.StartDate <= serviceDate && a.EndDate >= serviceDate)
                        .OrderByDescending(a => a.EndDate)
                        .Select(a => (DateOnly?)a.EndDate)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return BuildPage(items, pageNumber, pageSize, totalData);
        }

        public async Task<ReferralMasterResult<PagedResult<ReferralInstitutionResponse>>> GetInstitutionsPagedAsync(
            ReferralInstitutionListQuery request,
            CancellationToken cancellationToken = default)
        {
            var (pageNumber, pageSize) = NormalizePaging(request.PageNumber, request.PageSize);

            var range = ResolveCreatedRange(request.StartDate, request.EndDate, request.CustomPeriod);

            if (range.Error != null)
                return ReferralMasterResult<PagedResult<ReferralInstitutionResponse>>.Invalid(range.Error);

            if (request.InstitutionType.HasValue && !Enum.IsDefined(request.InstitutionType.Value))
                return ReferralMasterResult<PagedResult<ReferralInstitutionResponse>>.Invalid("Jenis fasilitas tidak dikenal.");

            if (request.PartnershipStatus.HasValue && !Enum.IsDefined(request.PartnershipStatus.Value))
                return ReferralMasterResult<PagedResult<ReferralInstitutionResponse>>.Invalid("Status kerja sama tidak dikenal.");

            var today = ReferralPartnerEligibility.Today();
            var query = InstitutionBaseQuery();
            var keyword = NormalizeSearch(request.Search);

            if (keyword != null)
            {
                query = query.Where(x =>
                    x.InstitutionCode.ToLower().Contains(keyword) ||
                    x.InstitutionName.ToLower().Contains(keyword));
            }

            if (range.StartUtc.HasValue)
                query = query.Where(x => x.CreateDateTime >= range.StartUtc.Value);

            if (range.EndExclusiveUtc.HasValue)
                query = query.Where(x => x.CreateDateTime < range.EndExclusiveUtc.Value);

            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);

            if (request.IsPartner.HasValue)
                query = query.Where(x => x.IsPartner == request.IsPartner.Value);

            if (request.InstitutionType.HasValue)
                query = query.Where(x => x.InstitutionType == request.InstitutionType.Value);

            if (request.PartnershipStatus.HasValue)
                query = query.Where(ReferralPartnerEligibility.HasStatus(request.PartnershipStatus.Value, today));

            var totalData = await query.CountAsync(cancellationToken);

            // Bawaan: terbaru dibuat di atas, sesuai kolom pertama tabel "Tanggal Dibuat".
            var descending = string.IsNullOrWhiteSpace(request.SortDirection)
                ? string.IsNullOrWhiteSpace(request.SortBy)
                : IsDescending(request.SortDirection);

            query = (request.SortBy?.Trim().ToLowerInvariant()) switch
            {
                "institutioncode" => descending
                    ? query.OrderByDescending(x => x.InstitutionCode)
                    : query.OrderBy(x => x.InstitutionCode),
                "institutionname" => descending
                    ? query.OrderByDescending(x => x.InstitutionName)
                    : query.OrderBy(x => x.InstitutionName),
                "ispartner" => descending
                    ? query.OrderByDescending(x => x.IsPartner).ThenBy(x => x.InstitutionName)
                    : query.OrderBy(x => x.IsPartner).ThenBy(x => x.InstitutionName),
                "isactive" => descending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.InstitutionName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.InstitutionName),
                _ => descending
                    ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.InstitutionName)
                    : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.InstitutionName)
            };

            var rows = await ProjectRows(query.Skip((pageNumber - 1) * pageSize).Take(pageSize), today)
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(rows.Select(x => x.CreateBy), cancellationToken);

            var items = rows
                .Select(x => ToInstitutionResponse(x, today, actorNames))
                .ToList();

            return ReferralMasterResult<PagedResult<ReferralInstitutionResponse>>.Ok(
                BuildPage(items, pageNumber, pageSize, totalData),
                "Daftar fasilitas perujuk berhasil diambil.");
        }

        public async Task<ReferralInstitutionSummaryResponse> GetInstitutionSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var today = ReferralPartnerEligibility.Today();
            var source = InstitutionBaseQuery();

            // Indikator dihitung terpisah di database; satu fasilitas paling banyak satu kali
            // per indikator karena setiap hitungan adalah COUNT baris master, bukan perjanjian.
            var total = await source.CountAsync(cancellationToken);
            var active = await source.CountAsync(x => x.IsActive, cancellationToken);
            var partner = await source.CountAsync(x => x.IsPartner, cancellationToken);
            var activePartner = await source
                .Where(ReferralPartnerEligibility.HasStatus(ReferralPartnershipStatus.Active, today))
                .CountAsync(cancellationToken);
            var inactivePartner = await source.CountAsync(x => x.IsPartner && !x.IsActive, cancellationToken);
            var expiredPartner = await source
                .CountAsync(x => x.IsPartner &&
                                 x.Agreements.Any(a => !a.IsDelete) &&
                                 !x.Agreements.Any(a => !a.IsDelete && a.EndDate >= today),
                    cancellationToken);

            return new ReferralInstitutionSummaryResponse
            {
                TotalReferralInstitution = total,
                ActiveReferralInstitution = active,
                InactiveReferralInstitution = total - active,
                PartnerReferralInstitution = partner,
                TotalPartner = partner,
                ActivePartner = activePartner,
                InactivePartner = inactivePartner,
                ExpiredContractPartner = expiredPartner
            };
        }

        public async Task<ReferralInstitutionResponse?> GetInstitutionByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var today = ReferralPartnerEligibility.Today();

            var row = await ProjectRows(InstitutionBaseQuery().Where(x => x.Id == id), today)
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                return null;

            var agreements = await _dbContext.Set<MstReferralInstitutionAgreement>()
                .AsNoTracking()
                .Where(x => x.ReferralInstitutionId == id && !x.IsDelete)
                .OrderByDescending(x => x.StartDate)
                .Select(x => new ReferralInstitutionAgreementResponse
                {
                    Id = x.Id,
                    AgreementNumber = x.AgreementNumber,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy
                })
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(
                agreements.Select(x => x.CreateBy).Append(row.CreateBy).Append(row.UpdateBy),
                cancellationToken);

            var response = ToInstitutionResponse(row, today, actorNames);

            foreach (var agreement in agreements)
            {
                agreement.PeriodStatus = agreement.EndDate < today
                    ? "Berakhir"
                    : agreement.StartDate > today ? "Akan Berlaku" : "Berlaku";
                agreement.CreatedByName = GetActorName(actorNames, agreement.CreateBy);
            }

            response.Agreements = agreements;
            response.UpdatedByName = GetActorName(actorNames, row.UpdateBy);

            return response;
        }

        public async Task<ReferralMasterResult<ReferralInstitutionResponse>> CreateInstitutionAsync(
            CreateReferralInstitutionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateInstitutionRequestAsync(request, null, cancellationToken);

            if (validation.Error != null)
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(validation.Error);

            var input = validation.Input!;

            if (await InstitutionCodeIsUsedAsync(input.Code, null, cancellationToken))
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);

            var now = DateTime.UtcNow;

            var entity = new MstReferralInstitution
            {
                Id = Guid.NewGuid(),
                IsPartner = true,
                IsActive = request.IsActive,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            ApplyInput(entity, input);

            var agreement = new MstReferralInstitutionAgreement
            {
                Id = Guid.NewGuid(),
                ReferralInstitutionId = entity.Id,
                AgreementNumber = input.AgreementNumber,
                StartDate = input.AgreementStartDate,
                EndDate = input.AgreementEndDate,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstReferralInstitution>().Add(entity);
            _dbContext.Set<MstReferralInstitutionAgreement>().Add(agreement);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir index unik kode (RJ-VAL-PM-16).
                _dbContext.Entry(agreement).State = EntityState.Detached;
                _dbContext.Entry(entity).State = EntityState.Detached;
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);
            }

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                (await GetInstitutionByIdAsync(entity.Id, cancellationToken))!,
                "Fasilitas perujuk berhasil dibuat.");
        }

        /// <summary>
        /// Update penuh. Ruas perjanjian mengoreksi perjanjian terakhir (atau membuat yang pertama
        /// untuk data lama); perpanjangan lewat <see cref="AddAgreementAsync"/>.
        /// </summary>
        /// <remarks>
        /// Data lama nonmitra yang disunting di sini menjadi mitra, karena form mewajibkan
        /// perjanjian. Itu tindakan eksplisit petugas master, bukan konversi otomatis.
        /// </remarks>
        public async Task<ReferralMasterResult<ReferralInstitutionResponse>> UpdateInstitutionAsync(
            Guid id,
            UpdateReferralInstitutionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedInstitutionAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralInstitutionResponse>.NotFound(InstitutionNotFoundMessage);

            if (request.ExpectedRowVersion.HasValue && request.ExpectedRowVersion.Value != entity.RowVersion)
                return ReferralMasterResult<ReferralInstitutionResponse>.Conflict(ConcurrencyMessage);

            var validation = await ValidateInstitutionRequestAsync(request, entity, cancellationToken);

            if (validation.Error != null)
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(validation.Error);

            var input = validation.Input!;

            if (await InstitutionCodeIsUsedAsync(input.Code, id, cancellationToken))
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);

            var agreements = await _dbContext.Set<MstReferralInstitutionAgreement>()
                .Where(x => x.ReferralInstitutionId == id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var latest = agreements
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefault();

            var overlap = FindOverlap(
                agreements.Where(x => latest == null || x.Id != latest.Id),
                input.AgreementStartDate,
                input.AgreementEndDate);

            if (overlap != null)
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(OverlapMessage(overlap));

            if (agreements.Any(x =>
                    (latest == null || x.Id != latest.Id) &&
                    string.Equals(x.AgreementNumber, input.AgreementNumber, StringComparison.OrdinalIgnoreCase)))
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(DuplicateAgreementMessage);
            }

            var now = DateTime.UtcNow;

            ApplyInput(entity, input);
            entity.IsPartner = true;
            entity.IsActive = request.IsActive;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (latest == null)
            {
                _dbContext.Set<MstReferralInstitutionAgreement>().Add(new MstReferralInstitutionAgreement
                {
                    Id = Guid.NewGuid(),
                    ReferralInstitutionId = id,
                    AgreementNumber = input.AgreementNumber,
                    StartDate = input.AgreementStartDate,
                    EndDate = input.AgreementEndDate,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }
            else if (latest.AgreementNumber != input.AgreementNumber ||
                     latest.StartDate != input.AgreementStartDate ||
                     latest.EndDate != input.AgreementEndDate)
            {
                latest.AgreementNumber = input.AgreementNumber;
                latest.StartDate = input.AgreementStartDate;
                latest.EndDate = input.AgreementEndDate;
                latest.UpdateDateTime = now;
                latest.UpdateBy = actorUserId;
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Conflict(ConcurrencyMessage);
            }
            catch (DbUpdateException)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);
            }

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                (await GetInstitutionByIdAsync(id, cancellationToken))!,
                "Fasilitas perujuk berhasil diubah.");
        }

        /// <summary>
        /// Menambah periode perjanjian (perpanjangan). Ditolak bila bertumpang tindih dengan
        /// perjanjian lain milik fasilitas yang sama.
        /// </summary>
        public async Task<ReferralMasterResult<ReferralInstitutionResponse>> AddAgreementAsync(
            Guid id,
            CreateReferralInstitutionAgreementRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedInstitutionAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralInstitutionResponse>.NotFound(InstitutionNotFoundMessage);

            if (request.ExpectedRowVersion.HasValue && request.ExpectedRowVersion.Value != entity.RowVersion)
                return ReferralMasterResult<ReferralInstitutionResponse>.Conflict(ConcurrencyMessage);

            if (!entity.IsPartner)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(
                    "Data lama nonmitra belum dapat diperpanjang. Lengkapi data fasilitas lewat Ubah terlebih dahulu.");
            }

            var agreementError = ValidateAgreement(
                request.AgreementNumber,
                request.StartDate,
                request.EndDate,
                out var number,
                out var startDate,
                out var endDate);

            if (agreementError != null)
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(agreementError);

            var agreements = await _dbContext.Set<MstReferralInstitutionAgreement>()
                .AsNoTracking()
                .Where(x => x.ReferralInstitutionId == id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            if (agreements.Any(x => string.Equals(x.AgreementNumber, number, StringComparison.OrdinalIgnoreCase)))
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(DuplicateAgreementMessage);

            var overlap = FindOverlap(agreements, startDate, endDate);

            if (overlap != null)
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(OverlapMessage(overlap));

            var now = DateTime.UtcNow;

            _dbContext.Set<MstReferralInstitutionAgreement>().Add(new MstReferralInstitutionAgreement
            {
                Id = Guid.NewGuid(),
                ReferralInstitutionId = id,
                AgreementNumber = number,
                StartDate = startDate,
                EndDate = endDate,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            // Mengganti token master membuat dua perpanjangan serentak saling menolak.
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Conflict(ConcurrencyMessage);
            }
            catch (DbUpdateException)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(DuplicateAgreementMessage);
            }

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                (await GetInstitutionByIdAsync(id, cancellationToken))!,
                "Perjanjian kerja sama berhasil ditambahkan.");
        }

        public async Task<ReferralMasterResult<ReferralInstitutionResponse>> UpdateInstitutionStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedInstitutionAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralInstitutionResponse>.NotFound(InstitutionNotFoundMessage);

            // DEC-FRJ-001: data lama nonmitra tidak boleh diaktifkan sebagai master mitra.
            if (isActive && !entity.IsPartner)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(
                    "Data lama nonmitra tidak dapat diaktifkan. Ubah data fasilitas dan lengkapi perjanjian kerja sama terlebih dahulu.");
            }

            entity.IsActive = isActive;
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Conflict(ConcurrencyMessage);
            }

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                (await GetInstitutionByIdAsync(id, cancellationToken))!,
                isActive
                    ? "Fasilitas perujuk berhasil diaktifkan."
                    : "Fasilitas perujuk berhasil dinonaktifkan. Fasilitas ini tidak lagi muncul di pilihan pendaftaran.");
        }

        /// <summary>
        /// Menandai institusi terhapus. Ditolak bila sudah dirujuk kunjungan atau masih punya
        /// dokter perujuk yang belum dihapus — riwayat kunjungan lama wajib tetap terbaca.
        /// </summary>
        public async Task<ReferralMasterResult<ReferralInstitutionResponse>> DeleteInstitutionAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedInstitutionAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralInstitutionResponse>.NotFound(InstitutionNotFoundMessage);

            var usedByEncounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.ReferralInstitutionId == id, cancellationToken);

            if (usedByEncounter)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.InUse(
                    "Institusi perujuk ini sudah dipakai kunjungan, sehingga tidak dapat dihapus. Nonaktifkan saja bila tidak dipakai lagi.");
            }

            var hasDoctor = await _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.ReferralInstitutionId == id && !x.IsDelete, cancellationToken);

            if (hasDoctor)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.InUse(
                    "Institusi perujuk ini masih punya dokter perujuk. Hapus atau pindahkan dokternya lebih dulu, atau nonaktifkan institusinya.");
            }

            var response = (await GetInstitutionByIdAsync(id, cancellationToken))!;
            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            entity.RowVersion = Guid.NewGuid();

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Conflict(ConcurrencyMessage);
            }

            response.IsActive = false;

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                response,
                "Institusi perujuk berhasil dihapus.");
        }

        public static ReferralMasterFilterMetadataResponse BuildInstitutionFilterMetadata()
            => new()
            {
                DefaultFilter = new ReferralMasterDefaultFilterResponse
                {
                    CustomPeriod = "last30days",
                    SortBy = "createDateTime",
                    SortDirection = "desc"
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CustomPeriods = BuildCustomPeriodOptions(),
                InstitutionTypeOptions = Enum.GetValues<ReferralInstitutionType>()
                    .Where(x => x != ReferralInstitutionType.Unknown)
                    .Select(x => new ReferralMasterEnumOptionResponse { Value = (int)x, Label = InstitutionTypeLabel(x) })
                    .ToList(),
                PartnershipStatusOptions = Enum.GetValues<ReferralPartnershipStatus>()
                    .Select(x => new ReferralMasterEnumOptionResponse { Value = (int)x, Label = ReferralPartnerEligibility.Label(x) })
                    .ToList(),
                SortOptions = new List<ReferralMasterSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" },
                    new() { Value = "institutionCode", Label = "Kode Fasilitas" },
                    new() { Value = "institutionName", Label = "Nama Fasilitas" },
                    new() { Value = "isActive", Label = "Status Aktif" }
                },
                QueryParameters = new List<ReferralMasterQueryParameterInfoResponse>
                {
                    new() { Name = "startDate", Type = "date", Description = "Tanggal dibuat mulai (yyyy-MM-dd, WIB). Dipakai bila customPeriod kosong atau custom.", Example = "2026-09-01" },
                    new() { Name = "endDate", Type = "date", Description = "Tanggal dibuat sampai (yyyy-MM-dd, WIB, inklusif).", Example = "2026-09-30" },
                    new() { Name = "customPeriod", Type = "string", Description = "Periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "last30days" },
                    new() { Name = "institutionType", Type = "integer", Description = "Jenis fasilitas (lihat InstitutionTypeOptions).", Example = "1" },
                    new() { Name = "partnershipStatus", Type = "integer", Description = "Status kerja sama terhitung hari ini (lihat PartnershipStatusOptions).", Example = "1" },
                    new() { Name = "search", Type = "string", Description = "Mencari pada kode dan nama fasilitas.", Example = "Sehat" },
                    new() { Name = "isActive", Type = "boolean", Description = "Menyaring fasilitas aktif atau nonaktif.", Example = "true" },
                    new() { Name = "isPartner", Type = "boolean", Description = "Menyaring mitra atau data lama nonmitra.", Example = "true" },
                    new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan. Bawaannya createDateTime.", Example = "createDateTime" },
                    new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan, asc atau desc. Bawaannya desc.", Example = "desc" },
                    new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "25" }
                },
                CreateFields = BuildInstitutionFormFields(),
                UpdateFields = BuildInstitutionFormFields()
            };

        private static List<ReferralMasterFormFieldMetadataResponse> BuildInstitutionFormFields()
            => new()
            {
                new() { Name = "institutionCode", Label = "Kode Fasilitas", Section = "Identitas Fasilitas", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 50, Description = "Huruf besar, angka, titik, garis miring, garis bawah, atau tanda hubung. Unik.", Example = "KLN-SHT-01", SortOrder = 1 },
                new() { Name = "institutionName", Label = "Nama Fasilitas", Section = "Identitas Fasilitas", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "Klinik Sehat Sentosa", SortOrder = 2 },
                new() { Name = "institutionType", Label = "Jenis Fasilitas", Section = "Identitas Fasilitas", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "InstitutionTypeOptions", SortOrder = 3 },
                new() { Name = "externalFacilityCode", Label = "Kode Faskes Eksternal", Section = "Identitas Fasilitas", InputType = "text", RequiredType = "Optional", MaxLength = 50, Example = "0112R001", SortOrder = 4 },
                new() { Name = "address", Label = "Alamat Lengkap", Section = "Lokasi Fasilitas", InputType = "textarea", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 500, Example = "Jl. Merdeka No. 10, Bekasi", SortOrder = 5 },
                new() { Name = "provinceId", Label = "Provinsi", Section = "Lokasi Fasilitas", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "/api/v1/administrator/master-data/provinces/options", SortOrder = 6 },
                new() { Name = "cityId", Label = "Kabupaten/Kota", Section = "Lokasi Fasilitas", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "/api/v1/administrator/master-data/cities/options", Description = "Harus berada di provinsi yang dipilih.", SortOrder = 7 },
                new() { Name = "phoneNumber", Label = "Nomor Telepon", Section = "Informasi Kontak", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 25, Example = "021-8801234", SortOrder = 8 },
                new() { Name = "email", Label = "Email", Section = "Informasi Kontak", InputType = "email", RequiredType = "Optional", MaxLength = 150, Example = "admin@kliniksehat.co.id", SortOrder = 9 },
                new() { Name = "picName", Label = "Nama PIC Mitra", Section = "Informasi Kontak", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "Budi Santoso", SortOrder = 10 },
                new() { Name = "agreementNumber", Label = "Nomor Perjanjian Kerja Sama", Section = "Informasi Perjanjian Kerja Sama", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 100, Example = "PKS/RSMMC/001/2026", SortOrder = 11 },
                new() { Name = "agreementStartDate", Label = "Tanggal Mulai Perjanjian", Section = "Informasi Perjanjian Kerja Sama", InputType = "date", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Example = "2026-01-01", SortOrder = 12 },
                new() { Name = "agreementEndDate", Label = "Tanggal Berakhir Perjanjian", Section = "Informasi Perjanjian Kerja Sama", InputType = "date", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", Description = "Tidak boleh sebelum tanggal mulai dan tidak boleh tumpang tindih dengan perjanjian lain.", Example = "2026-12-31", SortOrder = 13 },
                new() { Name = "isActive", Label = "Aktif", Section = "Status & Informasi Tambahan", InputType = "switch", RequiredType = "Optional", Description = "Fasilitas nonaktif tidak muncul di pilihan pendaftaran.", Example = "true", SortOrder = 14 },
                new() { Name = "description", Label = "Keterangan", Section = "Status & Informasi Tambahan", InputType = "textarea", RequiredType = "Optional", MaxLength = 1000, SortOrder = 15 }
            };

        public static string InstitutionTypeLabel(ReferralInstitutionType type) => type switch
        {
            ReferralInstitutionType.Clinic => "Klinik",
            ReferralInstitutionType.CommunityHealthCenter => "Puskesmas",
            ReferralInstitutionType.Hospital => "Rumah Sakit",
            ReferralInstitutionType.IndependentPractice => "Praktik Mandiri",
            ReferralInstitutionType.Laboratory => "Laboratorium",
            ReferralInstitutionType.Other => "Lainnya",
            _ => "Belum Diisi"
        };

        // ---------- Bantuan Fasilitas Perujuk (DEC-FRJ-001) ----------

        private const string ConcurrencyMessage =
            "Data fasilitas perujuk sudah diubah pengguna lain. Muat ulang lalu ulangi perubahan.";

        private const string DuplicateAgreementMessage =
            "Nomor perjanjian itu sudah tercatat pada fasilitas ini.";

        private static readonly Regex CodePattern = new("^[A-Z0-9][A-Z0-9._/-]{0,49}$", RegexOptions.Compiled);
        private static readonly Regex ExternalCodePattern = new("^[A-Za-z0-9][A-Za-z0-9._/-]{0,49}$", RegexOptions.Compiled);
        private static readonly Regex PhonePattern = new(@"^\+?[0-9][0-9 ()\-]{5,24}$", RegexOptions.Compiled);
        private static readonly Regex EmailPattern = new(@"^[^@\s<>]+@[^@\s<>]+\.[^@\s<>]+$", RegexOptions.Compiled);

        private sealed class InstitutionInput
        {
            public string Code { get; init; } = string.Empty;
            public string Name { get; init; } = string.Empty;
            public ReferralInstitutionType Type { get; init; }
            public string Address { get; init; } = string.Empty;
            public Guid ProvinceId { get; init; }
            public Guid CityId { get; init; }
            public string Phone { get; init; } = string.Empty;
            public string? Email { get; init; }
            public string PicName { get; init; } = string.Empty;
            public string? ExternalFacilityCode { get; init; }
            public string? Description { get; init; }
            public string AgreementNumber { get; init; } = string.Empty;
            public DateOnly AgreementStartDate { get; init; }
            public DateOnly AgreementEndDate { get; init; }
        }

        private sealed record InstitutionValidation(InstitutionInput? Input, string? Error);

        /// <summary>
        /// Validasi server seluruh isian master (<c>DEC-FRJ-001</c>): wajib, panjang, format,
        /// enum, referensi wilayah, karakter berbahaya, dan rentang tanggal perjanjian.
        /// </summary>
        private async Task<InstitutionValidation> ValidateInstitutionRequestAsync(
            CreateReferralInstitutionRequest request,
            MstReferralInstitution? existing,
            CancellationToken cancellationToken)
        {
            static InstitutionValidation Fail(string message) => new(null, message);

            var code = NormalizeCode(request.InstitutionCode);
            var name = NormalizeText(request.InstitutionName);
            var address = NormalizeText(request.Address);
            var phone = NormalizeText(request.PhoneNumber);
            var email = NormalizeText(request.Email)?.ToLowerInvariant();
            var picName = NormalizeText(request.PicName);
            var externalCode = NormalizeText(request.ExternalFacilityCode);
            var description = NormalizeText(request.Description);

            if (code.Length == 0) return Fail("Kode fasilitas wajib diisi.");
            if (!CodePattern.IsMatch(code))
                return Fail("Kode fasilitas hanya boleh berisi huruf, angka, titik, garis miring, garis bawah, atau tanda hubung (maksimal 50 karakter).");

            if (name == null) return Fail("Nama fasilitas wajib diisi.");
            if (name.Length < 3 || name.Length > 200) return Fail("Nama fasilitas harus 3 sampai 200 karakter.");

            if (request.InstitutionType == ReferralInstitutionType.Unknown || !Enum.IsDefined(request.InstitutionType))
                return Fail("Jenis fasilitas wajib dipilih.");

            if (address == null) return Fail("Alamat lengkap wajib diisi.");
            if (address.Length > 500) return Fail("Alamat terlalu panjang. Batasnya 500 karakter.");

            if (!request.ProvinceId.HasValue || request.ProvinceId.Value == Guid.Empty)
                return Fail("Provinsi wajib dipilih.");

            if (!request.CityId.HasValue || request.CityId.Value == Guid.Empty)
                return Fail("Kabupaten/Kota wajib dipilih.");

            if (phone == null) return Fail("Nomor telepon wajib diisi.");
            if (!PhonePattern.IsMatch(phone) || phone.Count(char.IsDigit) < 6)
                return Fail("Nomor telepon tidak valid. Gunakan angka, spasi, tanda kurung, tanda hubung, atau awalan +.");

            if (email != null && (email.Length > 150 || !EmailPattern.IsMatch(email)))
                return Fail("Format email tidak valid.");

            if (picName == null) return Fail("Nama PIC mitra wajib diisi.");
            if (picName.Length > 150) return Fail("Nama PIC terlalu panjang. Batasnya 150 karakter.");

            if (externalCode != null && !ExternalCodePattern.IsMatch(externalCode))
                return Fail("Kode faskes eksternal hanya boleh berisi huruf, angka, titik, garis miring, garis bawah, atau tanda hubung (maksimal 50 karakter).");

            if (description != null && description.Length > 1000)
                return Fail("Keterangan terlalu panjang. Batasnya 1000 karakter.");

            var unsafeField = FirstUnsafeField(
                ("Nama fasilitas", name),
                ("Alamat", address),
                ("Nama PIC", picName),
                ("Keterangan", description));

            if (unsafeField != null)
                return Fail($"{unsafeField} mengandung karakter yang tidak diizinkan (< atau >).");

            var agreementError = ValidateAgreement(
                request.AgreementNumber,
                request.AgreementStartDate,
                request.AgreementEndDate,
                out var agreementNumber,
                out var startDate,
                out var endDate);

            if (agreementError != null) return Fail(agreementError);

            // Wilayah: wajib ada dan kabupaten/kota berada di provinsi itu. Wilayah yang sudah
            // nonaktif hanya boleh dipertahankan, tidak boleh dipilih baru.
            var province = await _dbContext.Set<MstProvince>()
                .AsNoTracking()
                .Where(x => x.Id == request.ProvinceId.Value && !x.IsDelete)
                .Select(x => new { x.IsActive })
                .FirstOrDefaultAsync(cancellationToken);

            if (province == null || (!province.IsActive && existing?.ProvinceId != request.ProvinceId))
                return Fail("Provinsi tidak ditemukan atau sudah tidak aktif.");

            var city = await _dbContext.Set<MstCity>()
                .AsNoTracking()
                .Where(x => x.Id == request.CityId.Value && !x.IsDelete)
                .Select(x => new { x.ProvinceId, x.IsActive })
                .FirstOrDefaultAsync(cancellationToken);

            if (city == null || (!city.IsActive && existing?.CityId != request.CityId))
                return Fail("Kabupaten/Kota tidak ditemukan atau sudah tidak aktif.");

            if (city.ProvinceId != request.ProvinceId.Value)
                return Fail("Kabupaten/Kota tidak berada di provinsi yang dipilih.");

            return new InstitutionValidation(new InstitutionInput
            {
                Code = code,
                Name = name,
                Type = request.InstitutionType,
                Address = address,
                ProvinceId = request.ProvinceId.Value,
                CityId = request.CityId.Value,
                Phone = phone,
                Email = email,
                PicName = picName,
                ExternalFacilityCode = externalCode,
                Description = description,
                AgreementNumber = agreementNumber,
                AgreementStartDate = startDate,
                AgreementEndDate = endDate
            }, null);
        }

        private static string? ValidateAgreement(
            string? rawNumber,
            DateOnly? rawStart,
            DateOnly? rawEnd,
            out string number,
            out DateOnly startDate,
            out DateOnly endDate)
        {
            number = NormalizeText(rawNumber) ?? string.Empty;
            startDate = rawStart ?? default;
            endDate = rawEnd ?? default;

            if (number.Length == 0) return "Nomor perjanjian kerja sama wajib diisi.";
            if (number.Length > 100) return "Nomor perjanjian terlalu panjang. Batasnya 100 karakter.";
            if (FirstUnsafeField(("Nomor perjanjian", number)) != null)
                return "Nomor perjanjian mengandung karakter yang tidak diizinkan (< atau >).";
            if (!rawStart.HasValue) return "Tanggal mulai perjanjian wajib diisi.";
            if (!rawEnd.HasValue) return "Tanggal berakhir perjanjian wajib diisi.";
            if (rawStart.Value.Year < 1900 || rawEnd.Value.Year > 2200)
                return "Tanggal perjanjian di luar rentang yang diizinkan.";
            if (rawEnd.Value < rawStart.Value)
                return "Tanggal berakhir perjanjian tidak boleh sebelum tanggal mulai.";

            return null;
        }

        private static MstReferralInstitutionAgreement? FindOverlap(
            IEnumerable<MstReferralInstitutionAgreement> others,
            DateOnly startDate,
            DateOnly endDate)
            => others
                .Where(x => !x.IsDelete && x.StartDate <= endDate && x.EndDate >= startDate)
                .OrderBy(x => x.StartDate)
                .FirstOrDefault();

        private static string OverlapMessage(MstReferralInstitutionAgreement overlap)
            => $"Periode perjanjian bertumpang tindih dengan perjanjian {overlap.AgreementNumber} " +
               $"({overlap.StartDate:dd-MM-yyyy} s.d. {overlap.EndDate:dd-MM-yyyy}).";

        private static string? FirstUnsafeField(params (string Label, string? Value)[] fields)
            => fields
                .Where(x => x.Value != null &&
                            x.Value.Any(c => c == '<' || c == '>' || (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')))
                .Select(x => x.Label)
                .FirstOrDefault();

        private static void ApplyInput(MstReferralInstitution entity, InstitutionInput input)
        {
            entity.InstitutionCode = input.Code;
            entity.InstitutionName = input.Name;
            entity.InstitutionType = input.Type;
            entity.Address = input.Address;
            entity.ProvinceId = input.ProvinceId;
            entity.CityId = input.CityId;
            entity.PhoneNumber = input.Phone;
            entity.Email = input.Email;
            entity.PicName = input.PicName;
            entity.ExternalFacilityCode = input.ExternalFacilityCode;
            entity.Description = input.Description;
        }

        private static string? NormalizeSearch(string? search)
        {
            var keyword = NormalizeText(search);

            if (keyword == null)
                return null;

            return (keyword.Length > 100 ? keyword[..100] : keyword).ToLower();
        }

        private sealed record CreatedRange(DateTime? StartUtc, DateTime? EndExclusiveUtc, string? Error);

        /// <summary>
        /// Rentang tanggal dibuat dalam kalender WIB, diubah ke batas UTC karena
        /// <c>CreateDateTime</c> disimpan UTC. Contoh: <c>today</c> pada 2026-10-09 menjadi
        /// <c>[2026-10-08T17:00Z, 2026-10-09T17:00Z)</c>.
        /// </summary>
        private static CreatedRange ResolveCreatedRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = AppDateTimeHelper.OperationalDate();

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case null:
                case "":
                case "custom":
                    start = startDate?.Date;
                    endExclusive = endDate?.Date.AddDays(1);
                    break;
                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;
                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;
                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;
                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1);
                    endExclusive = start.Value.AddMonths(1);
                    break;
                case "lastmonth":
                    endExclusive = new DateTime(today.Year, today.Month, 1);
                    start = endExclusive.Value.AddMonths(-1);
                    break;
                default:
                    return new CreatedRange(null, null, "Periode tidak dikenal. Gunakan custom, today, last7days, last30days, thismonth, atau lastmonth.");
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return new CreatedRange(null, null, "Tanggal awal tidak boleh melewati tanggal akhir.");

            return new CreatedRange(
                start.HasValue ? AppDateTimeHelper.OperationalDateToUtc(start.Value) : null,
                endExclusive.HasValue ? AppDateTimeHelper.OperationalDateToUtc(endExclusive.Value) : null,
                null);
        }

        private static List<ReferralMasterCustomPeriodOptionResponse> BuildCustomPeriodOptions()
            => new()
            {
                new() { Value = "custom", Label = "Custom", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "last30days", Label = "30 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };

        private sealed class AgreementPeriodRow
        {
            public Guid Id { get; init; }
            public string AgreementNumber { get; init; } = string.Empty;
            public DateOnly StartDate { get; init; }
            public DateOnly EndDate { get; init; }
        }

        private sealed class InstitutionRow
        {
            public Guid Id { get; init; }
            public string InstitutionCode { get; init; } = string.Empty;
            public string InstitutionName { get; init; } = string.Empty;
            public ReferralInstitutionType InstitutionType { get; init; }
            public string? Address { get; init; }
            public Guid? ProvinceId { get; init; }
            public string? ProvinceName { get; init; }
            public Guid? CityId { get; init; }
            public string? CityName { get; init; }
            public string? PhoneNumber { get; init; }
            public string? Email { get; init; }
            public string? PicName { get; init; }
            public string? ExternalFacilityCode { get; init; }
            public string? Description { get; init; }
            public bool IsPartner { get; init; }
            public bool IsActive { get; init; }
            public int ActiveDoctorCount { get; init; }
            public bool HasAnyAgreement { get; init; }
            public AgreementPeriodRow? CurrentAgreement { get; init; }
            public AgreementPeriodRow? FutureAgreement { get; init; }
            public AgreementPeriodRow? LastAgreement { get; init; }
            public Guid RowVersion { get; init; }
            public DateTime CreateDateTime { get; init; }
            public Guid CreateBy { get; init; }
            public DateTime? UpdateDateTime { get; init; }
            public Guid UpdateBy { get; init; }
        }

        private static IQueryable<InstitutionRow> ProjectRows(IQueryable<MstReferralInstitution> source, DateOnly today)
            => source.Select(x => new InstitutionRow
            {
                Id = x.Id,
                InstitutionCode = x.InstitutionCode,
                InstitutionName = x.InstitutionName,
                InstitutionType = x.InstitutionType,
                Address = x.Address,
                ProvinceId = x.ProvinceId,
                ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                CityId = x.CityId,
                CityName = x.City != null ? x.City.CityName : null,
                PhoneNumber = x.PhoneNumber,
                Email = x.Email,
                PicName = x.PicName,
                ExternalFacilityCode = x.ExternalFacilityCode,
                Description = x.Description,
                IsPartner = x.IsPartner,
                IsActive = x.IsActive,
                ActiveDoctorCount = x.Doctors.Count(d => !d.IsDelete && d.IsActive),
                HasAnyAgreement = x.Agreements.Any(a => !a.IsDelete),
                CurrentAgreement = x.Agreements
                    .Where(a => !a.IsDelete && a.StartDate <= today && a.EndDate >= today)
                    .OrderByDescending(a => a.StartDate)
                    .Select(a => new AgreementPeriodRow { Id = a.Id, AgreementNumber = a.AgreementNumber, StartDate = a.StartDate, EndDate = a.EndDate })
                    .FirstOrDefault(),
                FutureAgreement = x.Agreements
                    .Where(a => !a.IsDelete && a.StartDate > today)
                    .OrderBy(a => a.StartDate)
                    .Select(a => new AgreementPeriodRow { Id = a.Id, AgreementNumber = a.AgreementNumber, StartDate = a.StartDate, EndDate = a.EndDate })
                    .FirstOrDefault(),
                LastAgreement = x.Agreements
                    .Where(a => !a.IsDelete)
                    .OrderByDescending(a => a.EndDate)
                    .Select(a => new AgreementPeriodRow { Id = a.Id, AgreementNumber = a.AgreementNumber, StartDate = a.StartDate, EndDate = a.EndDate })
                    .FirstOrDefault(),
                RowVersion = x.RowVersion,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy
            });

        private static ReferralInstitutionResponse ToInstitutionResponse(
            InstitutionRow row,
            DateOnly today,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var status = ReferralPartnerEligibility.Resolve(
                row.IsPartner,
                row.IsActive,
                row.CurrentAgreement != null,
                row.FutureAgreement != null,
                row.HasAnyAgreement);

            var relevant = row.CurrentAgreement ?? row.FutureAgreement ?? row.LastAgreement;

            return new ReferralInstitutionResponse
            {
                Id = row.Id,
                InstitutionCode = row.InstitutionCode,
                InstitutionName = row.InstitutionName,
                InstitutionType = row.InstitutionType,
                InstitutionTypeName = InstitutionTypeLabel(row.InstitutionType),
                Address = row.Address,
                ProvinceId = row.ProvinceId,
                ProvinceName = row.ProvinceName,
                CityId = row.CityId,
                CityName = row.CityName,
                PhoneNumber = row.PhoneNumber,
                Email = row.Email,
                PicName = row.PicName,
                ExternalFacilityCode = row.ExternalFacilityCode,
                Description = row.Description,
                IsPartner = row.IsPartner,
                IsActive = row.IsActive,
                PartnershipStatus = status,
                PartnershipStatusName = ReferralPartnerEligibility.Label(status),
                IsEligible = status == ReferralPartnershipStatus.Active,
                AgreementId = relevant?.Id,
                AgreementNumber = relevant?.AgreementNumber,
                AgreementStartDate = relevant?.StartDate,
                AgreementEndDate = relevant?.EndDate,
                ActiveDoctorCount = row.ActiveDoctorCount,
                RowVersion = row.RowVersion,
                CreateDateTime = row.CreateDateTime,
                CreateBy = row.CreateBy,
                CreatedByName = GetActorName(actorNames, row.CreateBy),
                UpdateDateTime = row.UpdateDateTime,
                UpdateBy = row.UpdateBy
            };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds,
            CancellationToken cancellationToken)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (ids.Count == 0)
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, Name = (string?)(x.DisplayName ?? x.UserName ?? x.UserCode) })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId)
            => actorId != Guid.Empty && actorNames.TryGetValue(actorId, out var name) ? name : null;

        // =================================================================
        // Master Dokter Perujuk — RJ-DOC-REV-BE-017
        // =================================================================

        public async Task<PagedResult<ReferralDoctorResponse>> GetDoctorsPagedAsync(
            string? search,
            bool? isActive,
            Guid? referralInstitutionId,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = DoctorBaseQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.DoctorName.ToLower().Contains(keyword) ||
                    (x.ReferralInstitution != null &&
                     x.ReferralInstitution.InstitutionName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (referralInstitutionId.HasValue && referralInstitutionId.Value != Guid.Empty)
                query = query.Where(x => x.ReferralInstitutionId == referralInstitutionId.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var descending = IsDescending(sortDirection);

            query = (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "institutionname" => descending
                    ? query.OrderByDescending(x => x.ReferralInstitution!.InstitutionName).ThenBy(x => x.DoctorName)
                    : query.OrderBy(x => x.ReferralInstitution!.InstitutionName).ThenBy(x => x.DoctorName),
                "isactive" => descending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.DoctorName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.DoctorName),
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.DoctorName)
                    : query.OrderBy(x => x.DoctorName)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ToDoctorResponseExpression())
                .ToListAsync(cancellationToken);

            return BuildPage(items, pageNumber, pageSize, totalData);
        }

        public async Task<ReferralDoctorSummaryResponse> GetDoctorSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var statuses = await DoctorBaseQuery()
                .Select(x => x.IsActive)
                .ToListAsync(cancellationToken);

            var active = statuses.Count(x => x);

            return new ReferralDoctorSummaryResponse
            {
                TotalReferralDoctor = statuses.Count,
                ActiveReferralDoctor = active,
                InactiveReferralDoctor = statuses.Count - active
            };
        }

        public Task<ReferralDoctorResponse?> GetDoctorByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return DoctorBaseQuery()
                .Where(x => x.Id == id)
                .Select(ToDoctorResponseExpression())
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ReferralMasterResult<ReferralDoctorResponse>> CreateDoctorAsync(
            CreateReferralDoctorRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var name = NormalizeText(request.DoctorName);

            if (name == null)
                return ReferralMasterResult<ReferralDoctorResponse>.Invalid("Nama dokter perujuk wajib diisi.");

            var institutionError = await ValidateDoctorInstitutionAsync(request.ReferralInstitutionId, cancellationToken);

            if (institutionError != null)
                return ReferralMasterResult<ReferralDoctorResponse>.Invalid(institutionError);

            if (await DoctorNameIsUsedAsync(request.ReferralInstitutionId, name, null, cancellationToken))
                return ReferralMasterResult<ReferralDoctorResponse>.Duplicate(DuplicateDoctorMessage);

            var entity = new MstReferralDoctor
            {
                Id = Guid.NewGuid(),
                ReferralInstitutionId = request.ReferralInstitutionId,
                DoctorName = name,
                IsActive = request.IsActive,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstReferralDoctor>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ReferralMasterResult<ReferralDoctorResponse>.Ok(
                (await GetDoctorByIdAsync(entity.Id, cancellationToken))!,
                "Dokter perujuk berhasil dibuat.");
        }

        public async Task<ReferralMasterResult<ReferralDoctorResponse>> UpdateDoctorAsync(
            Guid id,
            UpdateReferralDoctorRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedDoctorAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralDoctorResponse>.NotFound(DoctorNotFoundMessage);

            var name = NormalizeText(request.DoctorName);

            if (name == null)
                return ReferralMasterResult<ReferralDoctorResponse>.Invalid("Nama dokter perujuk wajib diisi.");

            // Institusi lama yang sudah nonaktif tetap boleh dipertahankan saat menyunting nama;
            // pindah ke institusi lain mewajibkan institusi tujuan aktif.
            if (request.ReferralInstitutionId != entity.ReferralInstitutionId)
            {
                var institutionError = await ValidateDoctorInstitutionAsync(request.ReferralInstitutionId, cancellationToken);

                if (institutionError != null)
                    return ReferralMasterResult<ReferralDoctorResponse>.Invalid(institutionError);
            }

            if (await DoctorNameIsUsedAsync(request.ReferralInstitutionId, name, id, cancellationToken))
                return ReferralMasterResult<ReferralDoctorResponse>.Duplicate(DuplicateDoctorMessage);

            entity.ReferralInstitutionId = request.ReferralInstitutionId;
            entity.DoctorName = name;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ReferralMasterResult<ReferralDoctorResponse>.Ok(
                (await GetDoctorByIdAsync(id, cancellationToken))!,
                "Dokter perujuk berhasil diubah.");
        }

        public async Task<ReferralMasterResult<ReferralDoctorResponse>> UpdateDoctorStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedDoctorAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralDoctorResponse>.NotFound(DoctorNotFoundMessage);

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ReferralMasterResult<ReferralDoctorResponse>.Ok(
                (await GetDoctorByIdAsync(id, cancellationToken))!,
                isActive ? "Dokter perujuk berhasil diaktifkan." : "Dokter perujuk berhasil dinonaktifkan.");
        }

        public async Task<ReferralMasterResult<ReferralDoctorResponse>> DeleteDoctorAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedDoctorAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralDoctorResponse>.NotFound(DoctorNotFoundMessage);

            var usedByEncounter = await _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.ReferralDoctorId == id, cancellationToken);

            if (usedByEncounter)
            {
                return ReferralMasterResult<ReferralDoctorResponse>.InUse(
                    "Dokter perujuk ini sudah dipakai kunjungan, sehingga tidak dapat dihapus. Nonaktifkan saja bila tidak dipakai lagi.");
            }

            var response = (await GetDoctorByIdAsync(id, cancellationToken))!;
            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            response.IsActive = false;

            return ReferralMasterResult<ReferralDoctorResponse>.Ok(response, "Dokter perujuk berhasil dihapus.");
        }

        public static ReferralMasterFilterMetadataResponse BuildDoctorFilterMetadata()
            => new()
            {
                DefaultFilter = new ReferralMasterDefaultFilterResponse { SortBy = "doctorName" },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SortOptions = new List<ReferralMasterSortOptionResponse>
                {
                    new() { Value = "doctorName", Label = "Nama Dokter" },
                    new() { Value = "institutionName", Label = "Institusi" },
                    new() { Value = "isActive", Label = "Status Aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                QueryParameters = new List<ReferralMasterQueryParameterInfoResponse>
                {
                    new() { Name = "search", Type = "string", Description = "Mencari pada nama dokter dan nama institusi.", Example = "Rina" },
                    new() { Name = "isActive", Type = "boolean", Description = "Menyaring dokter aktif atau nonaktif.", Example = "true" },
                    new() { Name = "referralInstitutionId", Type = "guid", Description = "Menyaring dokter pada satu institusi perujuk.", Example = "6b1e0000-0000-0000-0000-000000000000" },
                    new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan. Bawaannya doctorName.", Example = "doctorName" },
                    new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan, asc atau desc.", Example = "asc" },
                    new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "25" }
                },
                CreateFields = BuildDoctorFormFields(),
                UpdateFields = BuildDoctorFormFields()
            };

        private static List<ReferralMasterFormFieldMetadataResponse> BuildDoctorFormFields()
            => new()
            {
                new() { Name = "referralInstitutionId", Label = "Institusi Perujuk", Section = "Identitas", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "/api/v1/health-services/master-data/referral-institutions/options", Description = "Institusi tempat dokter berpraktik. Harus aktif.", SortOrder = 1 },
                new() { Name = "doctorName", Label = "Nama Dokter", Section = "Identitas", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "dr. Rina Lestari", SortOrder = 2 },
                new() { Name = "isActive", Label = "Aktif", Section = "Status", InputType = "switch", RequiredType = "Optional", Description = "Dokter nonaktif tidak muncul di pilihan pendaftaran.", Example = "true", SortOrder = 3 }
            };

        // ---------- Bantuan bersama ----------

        private const int DefaultPageSize = 25;

        private const string InstitutionNotFoundMessage =
            "Institusi perujuk tidak ditemukan atau sudah dihapus.";

        private const string DoctorNotFoundMessage =
            "Dokter perujuk tidak ditemukan atau sudah dihapus.";

        private const string DuplicateInstitutionCodeMessage =
            "Kode institusi perujuk itu sudah dipakai. Gunakan kode lain.";

        private const string DuplicateDoctorMessage =
            "Dokter perujuk dengan nama itu sudah terdaftar pada institusi yang sama.";

        private IQueryable<MstReferralInstitution> InstitutionBaseQuery()
            => _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private IQueryable<MstReferralDoctor> DoctorBaseQuery()
            => _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel);

        private Task<MstReferralInstitution?> TrackedInstitutionAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<MstReferralInstitution>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken);

        private Task<MstReferralDoctor?> TrackedDoctorAsync(Guid id, CancellationToken cancellationToken)
            => _dbContext.Set<MstReferralDoctor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken);

        private Task<bool> InstitutionCodeIsUsedAsync(string code, Guid? excludeId, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.InstitutionCode.ToLower() == code.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private Task<bool> DoctorNameIsUsedAsync(
            Guid institutionId,
            string name,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.ReferralInstitutionId == institutionId &&
                            x.DoctorName.ToLower() == name.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private async Task<string?> ValidateDoctorInstitutionAsync(Guid institutionId, CancellationToken cancellationToken)
        {
            if (institutionId == Guid.Empty)
                return "Institusi perujuk wajib dipilih.";

            var institution = await InstitutionBaseQuery()
                .Where(x => x.Id == institutionId)
                .Select(x => new { x.IsActive })
                .FirstOrDefaultAsync(cancellationToken);

            if (institution == null)
                return InstitutionNotFoundMessage;

            return institution.IsActive
                ? null
                : "Institusi perujuk itu nonaktif. Pilih institusi yang aktif.";
        }

        private static System.Linq.Expressions.Expression<Func<MstReferralDoctor, ReferralDoctorResponse>> ToDoctorResponseExpression()
            => x => new ReferralDoctorResponse
            {
                Id = x.Id,
                ReferralInstitutionId = x.ReferralInstitutionId,
                InstitutionCode = x.ReferralInstitution != null ? x.ReferralInstitution.InstitutionCode : null,
                InstitutionName = x.ReferralInstitution != null ? x.ReferralInstitution.InstitutionName : null,
                InstitutionIsPartner = x.ReferralInstitution != null && x.ReferralInstitution.IsPartner,
                DoctorName = x.DoctorName,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            };

        private static PagedResult<T> BuildPage<T>(List<T> items, int pageNumber, int pageSize, int totalData)
            => new()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static bool IsDescending(string? sortDirection)
            => string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        private static string NormalizeCode(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public enum ReferralMasterStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateIdentity = 3,

        /// <summary>Sudah dipakai kunjungan atau masih punya data turunan; tidak dapat dihapus.</summary>
        InUse = 4,

        /// <summary>Data sudah diubah pengguna lain (token konkurensi berbeda).</summary>
        Conflict = 5
    }

    public sealed record ReferralMasterResult<T>(ReferralMasterStatus Status, T? Data, string Message)
    {
        public static ReferralMasterResult<T> Ok(T data, string message) => new(ReferralMasterStatus.Success, data, message);
        public static ReferralMasterResult<T> NotFound(string message) => new(ReferralMasterStatus.NotFound, default, message);
        public static ReferralMasterResult<T> Invalid(string message) => new(ReferralMasterStatus.Invalid, default, message);
        public static ReferralMasterResult<T> Duplicate(string message) => new(ReferralMasterStatus.DuplicateIdentity, default, message);
        public static ReferralMasterResult<T> InUse(string message) => new(ReferralMasterStatus.InUse, default, message);
        public static ReferralMasterResult<T> Conflict(string message) => new(ReferralMasterStatus.Conflict, default, message);
    }
}
