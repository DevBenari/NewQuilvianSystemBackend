using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan master kebijakan batas waktu pengkajian.
    /// Controller tidak menyentuh <c>ApplicationDbContext</c> sendiri — QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>BE-RWI-055</c>. Tiga aturan melekat di sini, dan ketiganya menentukan apakah pemantauan
    /// kepatuhan pengkajian berguna atau justru menyesatkan:
    /// </para>
    /// <list type="number">
    /// <item>
    /// <b>Master kosong tidak menahan apa pun.</b> <see cref="ResolveEffectiveAsync"/>
    /// mengembalikan kosong, pengkajian tetap tersimpan, dan tidak satu pun dinyatakan terlambat
    /// — <c>VAL-KEP-17</c>.
    /// </item>
    /// <item>
    /// <b>Kebijakan dipilih menurut saat yang ditanyakan</b>, bukan menurut hari ini. Pengkajian
    /// yang dibuat bulan lalu dinilai dengan kebijakan yang berlaku bulan lalu —
    /// <c>AC-CAP012-04</c>.
    /// </item>
    /// <item>
    /// <b>Kebijakan yang lebih khusus menang.</b> Kebijakan yang menyebut jenis pelayanan
    /// tertentu mengalahkan kebijakan tanpa jenis pelayanan, sehingga rumah sakit dapat
    /// menetapkan satu angka umum lalu mengecualikan unit tertentu tanpa menghapus yang umum.
    /// </item>
    /// </list>
    /// <para>
    /// Tidak memakai interface, mengikuti pola service pada repository ini.
    /// </para>
    /// </remarks>
    public class ClinicalAssessmentPolicyService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        private const string NotFoundMessage = "Kebijakan batas waktu pengkajian tidak ditemukan.";

        private readonly ApplicationDbContext _dbContext;

        public ClinicalAssessmentPolicyService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Pemakaian oleh jalur klinis
        // =====================================================================

        /// <summary>
        /// Menemukan kebijakan yang berlaku bagi satu jenis pengkajian pada satu jenis pelayanan,
        /// menurut keadaan pada saat tertentu.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Mengembalikan <c>null</c> bila tidak ada kebijakan yang berlaku. Itu <b>bukan</b>
        /// kesalahan: selama clinical governance belum menetapkan angkanya, pengkajian tetap
        /// dicatat dan tidak satu pun dinyatakan terlambat — <c>VAL-KEP-17</c>, <c>FR-KEP-011</c>.
        /// </para>
        /// <para>
        /// <b>Contoh berangka.</b> Master memuat dua baris: <c>KEP-AWAL-UMUM</c> tanpa jenis
        /// pelayanan dengan batas 1440 menit, dan <c>KEP-AWAL-RI</c> untuk
        /// <c>ServiceUnitType.Inpatient</c> dengan batas 480 menit. Pengkajian awal Tn. Budi di
        /// unit rawat inap memperoleh <c>KEP-AWAL-RI</c>, karena kebijakan yang menyebut jenis
        /// pelayanan mengalahkan kebijakan umum.
        /// </para>
        /// </remarks>
        /// <param name="assessmentType">Jenis pengkajian yang dinilai.</param>
        /// <param name="serviceUnitType">
        /// Jenis pelayanan unit tempat pengkajian dibuat. Kosong berarti hanya kebijakan umum
        /// yang dipertimbangkan.
        /// </param>
        /// <param name="atUtc">Saat yang dipakai menilai masa berlaku kebijakan.</param>
        /// <param name="cancellationToken">Token pembatalan permintaan.</param>
        public async Task<MstClinicalAssessmentPolicy?> ResolveEffectiveAsync(
            PatientAssessmentType assessmentType,
            ServiceUnitType? serviceUnitType,
            DateTime atUtc,
            CancellationToken cancellationToken = default)
        {
            var kandidat = await BaseQuery()
                .Where(x =>
                    x.IsActive &&
                    x.AssessmentType == assessmentType &&
                    x.EffectiveFrom <= atUtc &&
                    (x.EffectiveTo == null || x.EffectiveTo > atUtc) &&
                    (x.ServiceUnitType == null ||
                     (serviceUnitType != null && x.ServiceUnitType == serviceUnitType)))
                .ToListAsync(cancellationToken);

            // Yang lebih khusus menang; bila sama-sama khusus atau sama-sama umum, yang paling
            // baru berlaku yang dipakai.
            return kandidat
                .OrderByDescending(x => x.ServiceUnitType.HasValue)
                .ThenByDescending(x => x.EffectiveFrom)
                .FirstOrDefault();
        }

        /// <summary>
        /// Menghitung tenggat sebuah pengkajian dari kebijakan yang berlaku saat itu.
        /// </summary>
        /// <returns>
        /// Penunjuk kebijakan beserta tenggatnya, atau dua nilai kosong bila memang belum ada
        /// kebijakan yang berlaku.
        /// </returns>
        public async Task<(Guid? PolicyId, DateTime? DueAt)> CalculateDueAsync(
            PatientAssessmentType assessmentType,
            ServiceUnitType? serviceUnitType,
            DateTime createdAtUtc,
            CancellationToken cancellationToken = default)
        {
            var kebijakan = await ResolveEffectiveAsync(
                assessmentType, serviceUnitType, createdAtUtc, cancellationToken);

            if (kebijakan == null)
                return (null, null);

            return (kebijakan.Id, createdAtUtc.AddMinutes(kebijakan.DueWithinMinutes));
        }

        /// <summary>
        /// Benar bila master kebijakan belum memuat satu pun baris aktif yang dapat dipakai.
        /// </summary>
        /// <remarks>
        /// Dipakai daftar pantau untuk membedakan "sudah tepat waktu" dari "batas waktu belum
        /// ditetapkan" — <c>FR-KEP-025</c>. Keduanya menghasilkan daftar kosong, tetapi artinya
        /// bagi kepala ruangan sama sekali berbeda.
        /// </remarks>
        public async Task<bool> IsMasterEmptyAsync(CancellationToken cancellationToken = default)
            => !await BaseQuery().AnyAsync(x => x.IsActive, cancellationToken);

        // =====================================================================
        // Permukaan master data
        // =====================================================================

        public async Task<PagedResult<ClinicalAssessmentPolicyResponse>> GetPagedAsync(
            string? search,
            bool? isActive,
            PatientAssessmentType? assessmentType,
            ServiceUnitType? serviceUnitType,
            bool? onlyCurrentlyEffective,
            DateTime? startDate,
            DateTime? endDate,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var now = DateTime.UtcNow;
            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kata = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PolicyCode.ToLower().Contains(kata) ||
                    x.PolicyName.ToLower().Contains(kata) ||
                    (x.Description != null && x.Description.ToLower().Contains(kata)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (assessmentType.HasValue)
                query = query.Where(x => x.AssessmentType == assessmentType.Value);

            if (serviceUnitType.HasValue)
                query = query.Where(x => x.ServiceUnitType == serviceUnitType.Value);

            if (onlyCurrentlyEffective == true)
            {
                query = query.Where(x =>
                    x.IsActive &&
                    x.EffectiveFrom <= now &&
                    (x.EffectiveTo == null || x.EffectiveTo > now));
            }

            if (startDate.HasValue)
                query = query.Where(x => x.EffectiveFrom >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.EffectiveFrom <= endDate.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<ClinicalAssessmentPolicyResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => ToResponse(x, now)).ToList()
            };
        }

        public async Task<PagedResult<ClinicalAssessmentPolicyOptionResponse>> GetOptionsAsync(
            string? search,
            PatientAssessmentType? assessmentType,
            ServiceUnitType? serviceUnitType,
            bool onlyActive,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = BaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var kata = search.Trim().ToLower();

                query = query.Where(x =>
                    x.PolicyCode.ToLower().Contains(kata) ||
                    x.PolicyName.ToLower().Contains(kata));
            }

            if (assessmentType.HasValue)
                query = query.Where(x => x.AssessmentType == assessmentType.Value);

            if (serviceUnitType.HasValue)
                query = query.Where(x => x.ServiceUnitType == serviceUnitType.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.AssessmentType)
                .ThenBy(x => x.PolicyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ClinicalAssessmentPolicyOptionResponse
                {
                    Id = x.Id,
                    PolicyCode = x.PolicyCode,
                    PolicyName = x.PolicyName,
                    AssessmentType = x.AssessmentType,
                    ServiceUnitType = x.ServiceUnitType,
                    DueWithinMinutes = x.DueWithinMinutes
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ClinicalAssessmentPolicyOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<ClinicalAssessmentPolicySummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var query = BaseQuery();

            var total = await query.CountAsync(cancellationToken);
            var aktif = await query.CountAsync(x => x.IsActive, cancellationToken);

            var berlaku = await query.CountAsync(x =>
                x.IsActive &&
                x.EffectiveFrom <= now &&
                (x.EffectiveTo == null || x.EffectiveTo > now), cancellationToken);

            var berakhir = await query.CountAsync(x =>
                x.EffectiveTo != null && x.EffectiveTo <= now, cancellationToken);

            return new ClinicalAssessmentPolicySummaryResponse
            {
                TotalPolicy = total,
                ActivePolicy = aktif,
                InactivePolicy = total - aktif,
                CurrentlyEffectivePolicy = berlaku,
                ExpiredPolicy = berakhir,
                IsPolicyMasterEmpty = aktif == 0
            };
        }

        public Task<MstClinicalAssessmentPolicy?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => BaseQuery().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        public async Task<ClinicalAssessmentPolicyResult> CreateAsync(
            CreateClinicalAssessmentPolicyRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var pesanValidasi = ValidateRequest(request);

            if (pesanValidasi != null)
                return Failed(ClinicalAssessmentPolicyStatus.Invalid, pesanValidasi);

            var kode = NormalizeCode(request.PolicyCode);

            if (await PolicyCodeIsUsedAsync(kode, excludeId: null, cancellationToken))
                return Failed(ClinicalAssessmentPolicyStatus.DuplicateCode, DuplicateCodeMessage(kode));

            var now = DateTime.UtcNow;

            var entity = new MstClinicalAssessmentPolicy
            {
                Id = Guid.NewGuid(),
                PolicyCode = kode,
                PolicyName = NormalizeText(request.PolicyName) ?? string.Empty,
                AssessmentType = request.AssessmentType,
                ServiceUnitType = request.ServiceUnitType,
                DueWithinMinutes = request.DueWithinMinutes,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo,
                Description = NormalizeText(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstClinicalAssessmentPolicy>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir: dua petugas yang menyimpan kode yang sama pada saat hampir
                // bersamaan sama-sama lolos pemeriksaan di atas, dan index unik di database yang
                // menolak salah satunya.
                _dbContext.Entry(entity).State = EntityState.Detached;

                return Failed(ClinicalAssessmentPolicyStatus.DuplicateCode, DuplicateCodeMessage(kode));
            }

            return new ClinicalAssessmentPolicyResult(
                ClinicalAssessmentPolicyStatus.Success,
                entity,
                "Kebijakan batas waktu pengkajian berhasil dibuat.");
        }

        public async Task<ClinicalAssessmentPolicyResult> UpdateAsync(
            Guid id,
            UpdateClinicalAssessmentPolicyRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(ClinicalAssessmentPolicyStatus.NotFound, NotFoundMessage);

            var pesanValidasi = ValidateRequest(request);

            if (pesanValidasi != null)
                return Failed(ClinicalAssessmentPolicyStatus.Invalid, pesanValidasi);

            var kode = NormalizeCode(request.PolicyCode);

            if (await PolicyCodeIsUsedAsync(kode, excludeId: id, cancellationToken))
                return Failed(ClinicalAssessmentPolicyStatus.DuplicateCode, DuplicateCodeMessage(kode));

            entity.PolicyCode = kode;
            entity.PolicyName = NormalizeText(request.PolicyName) ?? entity.PolicyName;
            entity.AssessmentType = request.AssessmentType;
            entity.ServiceUnitType = request.ServiceUnitType;
            entity.DueWithinMinutes = request.DueWithinMinutes;
            entity.EffectiveFrom = request.EffectiveFrom;
            entity.EffectiveTo = request.EffectiveTo;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Failed(ClinicalAssessmentPolicyStatus.DuplicateCode, DuplicateCodeMessage(kode));
            }

            return new ClinicalAssessmentPolicyResult(
                ClinicalAssessmentPolicyStatus.Success,
                entity,
                "Kebijakan batas waktu pengkajian berhasil diubah.");
        }

        /// <remarks>
        /// Menonaktifkan kebijakan <b>tidak</b> menyentuh satu pun pengkajian yang sudah
        /// memakainya. Pengkajian menyimpan penunjuk kebijakannya sendiri, sehingga penilaian
        /// keterlambatan yang lalu tetap terbaca apa adanya.
        /// </remarks>
        public async Task<ClinicalAssessmentPolicyResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(ClinicalAssessmentPolicyStatus.NotFound, NotFoundMessage);

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ClinicalAssessmentPolicyResult(
                ClinicalAssessmentPolicyStatus.Success,
                entity,
                isActive
                    ? "Kebijakan batas waktu pengkajian berhasil diaktifkan."
                    : "Kebijakan batas waktu pengkajian berhasil dinonaktifkan.");
        }

        /// <summary>
        /// Menandai kebijakan terhapus. Selalu soft delete, dan ditolak bila masih dipakai
        /// pengkajian mana pun.
        /// </summary>
        /// <remarks>
        /// Penolakannya bukan kerewelan. Pengkajian menyimpan <c>PolicyId</c> sebagai bukti
        /// menurut kebijakan mana ia dinilai; membiarkan kebijakannya lenyap membuat penilaian
        /// keterlambatan yang lalu tidak dapat dijelaskan lagi.
        /// </remarks>
        public async Task<ClinicalAssessmentPolicyResult> SoftDeleteAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entity = await TrackedAsync(id, cancellationToken);

            if (entity == null)
                return Failed(ClinicalAssessmentPolicyStatus.NotFound, NotFoundMessage);

            var dipakai = await _dbContext.Set<TrxPatientAssessment>()
                .AsNoTracking()
                .AnyAsync(x => x.PolicyId == id && !x.IsDelete, cancellationToken);

            if (dipakai)
            {
                return Failed(
                    ClinicalAssessmentPolicyStatus.InUse,
                    "Kebijakan ini sudah dipakai menilai pengkajian, sehingga tidak dapat " +
                    "dihapus. Nonaktifkan saja bila tidak berlaku lagi.");
            }

            var now = DateTime.UtcNow;

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new ClinicalAssessmentPolicyResult(
                ClinicalAssessmentPolicyStatus.Success,
                entity,
                "Kebijakan batas waktu pengkajian berhasil dihapus.");
        }

        public static ClinicalAssessmentPolicyResponse ToResponse(
            MstClinicalAssessmentPolicy entity,
            DateTime nowUtc) => new()
            {
                Id = entity.Id,
                PolicyCode = entity.PolicyCode,
                PolicyName = entity.PolicyName,
                AssessmentType = entity.AssessmentType,
                ServiceUnitType = entity.ServiceUnitType,
                DueWithinMinutes = entity.DueWithinMinutes,
                EffectiveFrom = entity.EffectiveFrom,
                EffectiveTo = entity.EffectiveTo,
                Description = entity.Description,
                IsActive = entity.IsActive,
                IsCurrentlyEffective =
                    entity.IsActive &&
                    entity.EffectiveFrom <= nowUtc &&
                    (entity.EffectiveTo == null || entity.EffectiveTo > nowUtc),
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };

        /// <summary>Konfigurasi halaman master: penyaring, pengurutan, dan metadata form.</summary>
        public static ClinicalAssessmentPolicyFilterMetadataResponse BuildFilterMetadata() => new()
        {
            DefaultFilter = new ClinicalAssessmentPolicyDefaultFilterResponse(),
            CustomPeriods =
            [
                new()
                {
                    Value = "currentlyEffective",
                    Label = "Sedang berlaku",
                    Description = "Kebijakan yang periodenya sedang berjalan hari ini.",
                    UsesStartDate = false,
                    UsesEndDate = false
                },
                new()
                {
                    Value = "custom",
                    Label = "Rentang awal berlaku",
                    Description = "Menyaring menurut tanggal awal berlaku kebijakan.",
                    UsesStartDate = true,
                    UsesEndDate = true
                }
            ],
            SortOptions =
            [
                new() { Value = "effectiveFrom", Label = "Awal berlaku" },
                new() { Value = "policyCode", Label = "Kode kebijakan" },
                new() { Value = "policyName", Label = "Nama kebijakan" },
                new() { Value = "assessmentType", Label = "Jenis pengkajian" },
                new() { Value = "dueWithinMinutes", Label = "Batas waktu" }
            ],
            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100],
            AssessmentTypeOptions = Enum.GetValues<PatientAssessmentType>()
                .Select(x => new ClinicalAssessmentPolicyEnumOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = NamaJenisPengkajian(x)
                })
                .ToList(),
            ServiceUnitTypeOptions = Enum.GetValues<ServiceUnitType>()
                .Select(x => new ClinicalAssessmentPolicyEnumOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = x.ToString()
                })
                .ToList(),
            QueryParameters =
            [
                new() { Name = "search", Type = "string", Description = "Mencari kode, nama, atau keterangan kebijakan." },
                new() { Name = "isActive", Type = "boolean", Description = "Menyaring kebijakan aktif atau nonaktif." },
                new() { Name = "assessmentType", Type = "enum", Description = "Menyaring menurut jenis pengkajian.", Example = "Initial" },
                new() { Name = "serviceUnitType", Type = "enum", Description = "Menyaring menurut jenis pelayanan.", Example = "Inpatient" },
                new() { Name = "onlyCurrentlyEffective", Type = "boolean", Description = "Hanya kebijakan yang periodenya sedang berjalan." },
                new() { Name = "startDate", Type = "date", Description = "Awal rentang tanggal berlaku.", Example = "2026-09-01" },
                new() { Name = "endDate", Type = "date", Description = "Akhir rentang tanggal berlaku.", Example = "2026-09-30" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan.", Example = "effectiveFrom" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan.", Example = "desc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Halaman keberapa.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Banyak baris per halaman.", Example = "25" }
            ],
            CreateFields = FormFields(isCreate: true),
            UpdateFields = FormFields(isCreate: false)
        };

        private static List<ClinicalAssessmentPolicyFormFieldMetadataResponse> FormFields(bool isCreate) =>
        [
            new()
            {
                Name = "policyCode", Label = "Kode kebijakan", Section = "Identitas", InputType = "text",
                IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required",
                MaxLength = 50, Example = "KEP-AWAL-RI-2026", SortOrder = 1
            },
            new()
            {
                Name = "policyName", Label = "Nama kebijakan", Section = "Identitas", InputType = "text",
                IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required",
                MaxLength = 150, Example = "Pengkajian awal rawat inap 2026", SortOrder = 2
            },
            new()
            {
                Name = "assessmentType", Label = "Jenis pengkajian", Section = "Cakupan", InputType = "select",
                IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required",
                OptionsSource = "assessmentTypeOptions", Example = "Initial", SortOrder = 3
            },
            new()
            {
                Name = "serviceUnitType", Label = "Jenis pelayanan", Section = "Cakupan", InputType = "select",
                IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional",
                OptionsSource = "serviceUnitTypeOptions",
                Description = "Kosongkan bila kebijakan berlaku untuk seluruh jenis pelayanan.",
                Example = "Inpatient", SortOrder = 4
            },
            new()
            {
                Name = "dueWithinMinutes", Label = "Batas waktu (menit)", Section = "Batas waktu", InputType = "number",
                IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required",
                Description = "Dihitung sejak pengkajian dibuat. 1440 menit berarti 24 jam.",
                Example = "1440", SortOrder = 5
            },
            new()
            {
                Name = "effectiveFrom", Label = "Berlaku mulai", Section = "Masa berlaku", InputType = "datetime",
                IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required",
                Example = "2026-09-01T00:00:00Z", SortOrder = 6
            },
            new()
            {
                Name = "effectiveTo", Label = "Berlaku sampai", Section = "Masa berlaku", InputType = "datetime",
                IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional",
                Description = "Kosongkan bila kebijakan masih berlaku sampai digantikan.",
                SortOrder = 7
            },
            new()
            {
                Name = "description", Label = "Keterangan", Section = "Keterangan", InputType = "textarea",
                IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "Optional",
                MaxLength = 500, SortOrder = 8
            },
            new()
            {
                Name = "isActive", Label = "Aktif", Section = "Status", InputType = "switch",
                IsRequiredOnCreate = false, IsRequiredOnUpdate = !isCreate, RequiredType = "Optional",
                SortOrder = 9
            }
        ];

        /// <summary>Nama jenis pengkajian dalam bahasa layar.</summary>
        public static string NamaJenisPengkajian(PatientAssessmentType jenis) => jenis switch
        {
            PatientAssessmentType.Initial => "Pengkajian awal keperawatan",
            PatientAssessmentType.Reassessment => "Pengkajian ulang keperawatan",
            PatientAssessmentType.DailyReassessment => "Pengkajian ulang harian keperawatan",
            PatientAssessmentType.DischargePlanning => "Pengkajian rencana pemulangan",
            PatientAssessmentType.MedicalInitial => "Kajian medis awal",
            PatientAssessmentType.MedicalReassessment => "Kajian medis ulang",
            _ => jenis.ToString()
        };

        private IQueryable<MstClinicalAssessmentPolicy> BaseQuery()
            => _dbContext.Set<MstClinicalAssessmentPolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

        private Task<MstClinicalAssessmentPolicy?> TrackedAsync(
            Guid id,
            CancellationToken cancellationToken)
            => _dbContext.Set<MstClinicalAssessmentPolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        private static IQueryable<MstClinicalAssessmentPolicy> ApplySorting(
            IQueryable<MstClinicalAssessmentPolicy> query,
            string? sortBy,
            string? sortDirection)
        {
            var turun = !string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "effectiveFrom").ToLowerInvariant() switch
            {
                "policycode" => turun ? query.OrderByDescending(x => x.PolicyCode) : query.OrderBy(x => x.PolicyCode),
                "policyname" => turun ? query.OrderByDescending(x => x.PolicyName) : query.OrderBy(x => x.PolicyName),
                "assessmenttype" => turun ? query.OrderByDescending(x => x.AssessmentType) : query.OrderBy(x => x.AssessmentType),
                "duewithinminutes" => turun ? query.OrderByDescending(x => x.DueWithinMinutes) : query.OrderBy(x => x.DueWithinMinutes),
                _ => turun ? query.OrderByDescending(x => x.EffectiveFrom) : query.OrderBy(x => x.EffectiveFrom)
            };
        }

        private Task<bool> PolicyCodeIsUsedAsync(
            string policyCode,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstClinicalAssessmentPolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.PolicyCode.ToLower() == policyCode.ToLower());

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private static string DuplicateCodeMessage(string policyCode)
            => $"Kode kebijakan {policyCode} sudah dipakai kebijakan lain.";

        private static ClinicalAssessmentPolicyResult Failed(
            ClinicalAssessmentPolicyStatus status,
            string message)
            => new(status, null, message);

        private static string? ValidateRequest(CreateClinicalAssessmentPolicyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PolicyCode))
                return "Kode kebijakan wajib diisi.";

            if (string.IsNullOrWhiteSpace(request.PolicyName))
                return "Nama kebijakan wajib diisi.";

            if (request.DueWithinMinutes <= 0)
                return "Batas waktu wajib diisi dan harus lebih dari nol menit.";

            if (request.EffectiveFrom == default)
                return "Awal masa berlaku wajib diisi.";

            if (request.EffectiveTo.HasValue && request.EffectiveTo.Value <= request.EffectiveFrom)
                return "Akhir masa berlaku harus setelah awal masa berlaku.";

            return null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            return (pageNumber, pageSize);
        }

        private static string NormalizeCode(string value)
            => value.Trim().ToUpperInvariant();

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public enum ClinicalAssessmentPolicyStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        DuplicateCode = 3,
        InUse = 4
    }

    public sealed record ClinicalAssessmentPolicyResult(
        ClinicalAssessmentPolicyStatus Status,
        MstClinicalAssessmentPolicy? Entity,
        string Message);
}
