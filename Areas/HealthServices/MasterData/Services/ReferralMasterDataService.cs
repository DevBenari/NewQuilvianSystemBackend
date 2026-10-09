using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

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
        // Master Institusi Perujuk — RJ-DOC-REV-BE-017 (RJ-DOC-DEC-073, 080)
        // =================================================================

        public async Task<PagedResult<ReferralInstitutionResponse>> GetInstitutionsPagedAsync(
            string? search,
            bool? isActive,
            bool? isPartner,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = InstitutionBaseQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.InstitutionCode.ToLower().Contains(keyword) ||
                    x.InstitutionName.ToLower().Contains(keyword) ||
                    (x.Address != null && x.Address.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isPartner.HasValue)
                query = query.Where(x => x.IsPartner == isPartner.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var descending = IsDescending(sortDirection);

            query = (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "institutioncode" => descending
                    ? query.OrderByDescending(x => x.InstitutionCode)
                    : query.OrderBy(x => x.InstitutionCode),
                "ispartner" => descending
                    ? query.OrderByDescending(x => x.IsPartner).ThenBy(x => x.InstitutionName)
                    : query.OrderBy(x => x.IsPartner).ThenBy(x => x.InstitutionName),
                "isactive" => descending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.InstitutionName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.InstitutionName),
                "createdatetime" => descending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => descending
                    ? query.OrderByDescending(x => x.InstitutionName)
                    : query.OrderBy(x => x.InstitutionName)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReferralInstitutionResponse
                {
                    Id = x.Id,
                    InstitutionCode = x.InstitutionCode,
                    InstitutionName = x.InstitutionName,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    IsPartner = x.IsPartner,
                    IsActive = x.IsActive,
                    ActiveDoctorCount = x.Doctors.Count(d => !d.IsDelete && d.IsActive),
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return BuildPage(items, pageNumber, pageSize, totalData);
        }

        public async Task<ReferralInstitutionSummaryResponse> GetInstitutionSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var rows = await InstitutionBaseQuery()
                .Select(x => new { x.IsActive, x.IsPartner })
                .ToListAsync(cancellationToken);

            var active = rows.Count(x => x.IsActive);

            return new ReferralInstitutionSummaryResponse
            {
                TotalReferralInstitution = rows.Count,
                ActiveReferralInstitution = active,
                InactiveReferralInstitution = rows.Count - active,
                PartnerReferralInstitution = rows.Count(x => x.IsPartner)
            };
        }

        public async Task<ReferralInstitutionResponse?> GetInstitutionByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await InstitutionBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new ReferralInstitutionResponse
                {
                    Id = x.Id,
                    InstitutionCode = x.InstitutionCode,
                    InstitutionName = x.InstitutionName,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    IsPartner = x.IsPartner,
                    IsActive = x.IsActive,
                    ActiveDoctorCount = x.Doctors.Count(d => !d.IsDelete && d.IsActive),
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<ReferralMasterResult<ReferralInstitutionResponse>> CreateInstitutionAsync(
            CreateReferralInstitutionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var code = NormalizeCode(request.InstitutionCode);
            var name = NormalizeText(request.InstitutionName);

            if (code.Length == 0 || name == null)
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(
                    "Kode dan nama institusi perujuk wajib diisi.");

            if (await InstitutionCodeIsUsedAsync(code, null, cancellationToken))
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);

            var entity = new MstReferralInstitution
            {
                Id = Guid.NewGuid(),
                InstitutionCode = code,
                InstitutionName = name,
                Address = NormalizeText(request.Address),
                PhoneNumber = NormalizeText(request.PhoneNumber),
                IsPartner = request.IsPartner,
                IsActive = request.IsActive,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstReferralInstitution>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir index unik kode (RJ-VAL-PM-16).
                _dbContext.Entry(entity).State = EntityState.Detached;
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);
            }

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                ToInstitutionResponse(entity, 0),
                "Institusi perujuk berhasil dibuat.");
        }

        public async Task<ReferralMasterResult<ReferralInstitutionResponse>> UpdateInstitutionAsync(
            Guid id,
            UpdateReferralInstitutionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedInstitutionAsync(id, cancellationToken);

            if (entity == null)
                return ReferralMasterResult<ReferralInstitutionResponse>.NotFound(InstitutionNotFoundMessage);

            var code = NormalizeCode(request.InstitutionCode);
            var name = NormalizeText(request.InstitutionName);

            if (code.Length == 0 || name == null)
                return ReferralMasterResult<ReferralInstitutionResponse>.Invalid(
                    "Kode dan nama institusi perujuk wajib diisi.");

            if (await InstitutionCodeIsUsedAsync(code, id, cancellationToken))
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);

            entity.InstitutionCode = code;
            entity.InstitutionName = name;
            entity.Address = NormalizeText(request.Address);
            entity.PhoneNumber = NormalizeText(request.PhoneNumber);
            entity.IsPartner = request.IsPartner;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return ReferralMasterResult<ReferralInstitutionResponse>.Duplicate(DuplicateInstitutionCodeMessage);
            }

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                ToInstitutionResponse(entity, await CountActiveDoctorsAsync(id, cancellationToken)),
                "Institusi perujuk berhasil diubah.");
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

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                ToInstitutionResponse(entity, await CountActiveDoctorsAsync(id, cancellationToken)),
                isActive
                    ? "Institusi perujuk berhasil diaktifkan."
                    : "Institusi perujuk berhasil dinonaktifkan. Institusi ini tidak lagi muncul di pilihan pendaftaran.");
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

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ReferralMasterResult<ReferralInstitutionResponse>.Ok(
                ToInstitutionResponse(entity, 0),
                "Institusi perujuk berhasil dihapus.");
        }

        public static ReferralMasterFilterMetadataResponse BuildInstitutionFilterMetadata()
            => new()
            {
                DefaultFilter = new ReferralMasterDefaultFilterResponse { SortBy = "institutionName" },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SortOptions = new List<ReferralMasterSortOptionResponse>
                {
                    new() { Value = "institutionName", Label = "Nama Institusi" },
                    new() { Value = "institutionCode", Label = "Kode Institusi" },
                    new() { Value = "isPartner", Label = "Bermitra" },
                    new() { Value = "isActive", Label = "Status Aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" }
                },
                QueryParameters = new List<ReferralMasterQueryParameterInfoResponse>
                {
                    new() { Name = "search", Type = "string", Description = "Mencari pada kode, nama, alamat, dan telepon institusi.", Example = "Sehat" },
                    new() { Name = "isActive", Type = "boolean", Description = "Menyaring institusi aktif atau nonaktif.", Example = "true" },
                    new() { Name = "isPartner", Type = "boolean", Description = "Menyaring institusi yang bermitra dengan rumah sakit.", Example = "true" },
                    new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan. Bawaannya institutionName.", Example = "institutionName" },
                    new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan, asc atau desc.", Example = "asc" },
                    new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "25" }
                },
                CreateFields = BuildInstitutionFormFields(),
                UpdateFields = BuildInstitutionFormFields()
            };

        private static List<ReferralMasterFormFieldMetadataResponse> BuildInstitutionFormFields()
            => new()
            {
                new() { Name = "institutionCode", Label = "Kode Institusi", Section = "Identitas", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 50, Description = "Kode institusi, misalnya kode faskes. Unik.", Example = "KLN-SHT-01", SortOrder = 1 },
                new() { Name = "institutionName", Label = "Nama Institusi", Section = "Identitas", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "Klinik Sehat Sentosa", SortOrder = 2 },
                new() { Name = "address", Label = "Alamat", Section = "Kontak", InputType = "textarea", RequiredType = "Optional", MaxLength = 500, Example = "Jl. Merdeka No. 10, Bekasi", SortOrder = 3 },
                new() { Name = "phoneNumber", Label = "Telepon", Section = "Kontak", InputType = "text", RequiredType = "Optional", MaxLength = 50, Example = "021-8801234", SortOrder = 4 },
                new() { Name = "isPartner", Label = "Bermitra", Section = "Status", InputType = "switch", RequiredType = "Optional", Description = "Bila aktif, pendaftaran rujukan menampilkan alert \"Fasilitas Perujuk Bermitra dengan Rumah Sakit\".", Example = "true", SortOrder = 5 },
                new() { Name = "isActive", Label = "Aktif", Section = "Status", InputType = "switch", RequiredType = "Optional", Description = "Institusi nonaktif tidak muncul di pilihan pendaftaran.", Example = "true", SortOrder = 6 }
            };

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

        private Task<int> CountActiveDoctorsAsync(Guid institutionId, CancellationToken cancellationToken)
            => _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .CountAsync(x => x.ReferralInstitutionId == institutionId && !x.IsDelete && x.IsActive, cancellationToken);

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

        private static ReferralInstitutionResponse ToInstitutionResponse(MstReferralInstitution entity, int activeDoctorCount)
            => new()
            {
                Id = entity.Id,
                InstitutionCode = entity.InstitutionCode,
                InstitutionName = entity.InstitutionName,
                Address = entity.Address,
                PhoneNumber = entity.PhoneNumber,
                IsPartner = entity.IsPartner,
                IsActive = entity.IsActive,
                ActiveDoctorCount = activeDoctorCount,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
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
        InUse = 4
    }

    public sealed record ReferralMasterResult<T>(ReferralMasterStatus Status, T? Data, string Message)
    {
        public static ReferralMasterResult<T> Ok(T data, string message) => new(ReferralMasterStatus.Success, data, message);
        public static ReferralMasterResult<T> NotFound(string message) => new(ReferralMasterStatus.NotFound, default, message);
        public static ReferralMasterResult<T> Invalid(string message) => new(ReferralMasterStatus.Invalid, default, message);
        public static ReferralMasterResult<T> Duplicate(string message) => new(ReferralMasterStatus.DuplicateIdentity, default, message);
        public static ReferralMasterResult<T> InUse(string message) => new(ReferralMasterStatus.InUse, default, message);
    }
}
