using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Pemilik seluruh pembacaan dan perubahan master profil dokumen wajib rekam medis
    /// (`BE-37`, `AGG-RM-04`, kontrak API 0.2.0 A3). Controller
    /// <c>MedicalRecordCompletenessProfileController</c> tidak menyentuh <c>ApplicationDbContext</c>
    /// sendiri, sesuai QBE-SVC-001.
    /// </summary>
    /// <remarks>
    /// Master ini menentukan dokumen apa yang dituntut sebelum berkas sebuah kunjungan dinyatakan
    /// lengkap (`RM-DEC-045` syarat 3). Ia disimpan sebagai data, bukan ditulis di program, supaya
    /// perubahan SOP rumah sakit tidak menuntut rilis.
    ///
    /// Empat aturan melekat di sini:
    ///
    /// 1. Satu kunci, satu profil (`VAL-RM-A05`). Kunci = jenis kunjungan + jenis unit + kondisi;
    ///    kolom kosong berarti "semua" dan dibandingkan sebagai nilai yang sama. Kode profil juga unik.
    /// 2. Profil tanpa kondisi wajib menyebut jenis kunjungan (`VAL-RM-A04`).
    /// 3. Kriteria hanya dipilih dari katalog <see cref="CompletenessCriterion"/> (`VAL-RM-A06`) dan
    ///    tidak boleh kembar (`VAL-RM-A07`). Profil tanpa kriteria sah (`RM-DEC-060`).
    /// 4. Profil dan kriterianya TIDAK PERNAH dihapus. Profil dinonaktifkan; kriteria yang tidak
    ///    dipilih lagi dinonaktifkan dan diaktifkan kembali bila dipilih ulang.
    ///
    /// Setiap perubahan menghasilkan kalimat jejak yang memuat keadaan sebelum dan sesudah
    /// (<see cref="MedicalRecordCompletenessProfileResult.AuditMessage"/>). Kalimat itu ditulis
    /// lengkap di teks pesan, karena logger tidak menyimpan objek data yang menyertainya.
    /// </remarks>
    public class MedicalRecordCompletenessProfileService
    {
        private const int DefaultPageSize = 25;
        private const int MaxPageSize = 100;

        public const string NotFoundMessage = "Profil dokumen wajib tidak ditemukan.";

        // Bunyi pesan mengikuti validation matrix amandemen 0.2.0.
        public const string EncounterTypeRequiredMessage = "Pilih jenis kunjungan untuk profil ini.";
        public const string DuplicateKeyMessage = "Profil untuk jenis kunjungan dan unit ini sudah ada.";
        public const string UnknownCriterionMessage = "Kriteria tidak dikenal.";
        public const string DuplicateCriterionMessage = "Kriteria yang sama tidak boleh dipilih dua kali.";
        public const string UnknownTypeMessage = "Jenis kunjungan atau jenis unit tidak dikenal.";
        public const string UnknownConditionMessage = "Kondisi tidak dikenal.";

        private readonly ApplicationDbContext _dbContext;

        public MedicalRecordCompletenessProfileService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =====================================================================
        // Pembacaan
        // =====================================================================

        public async Task<MedicalRecordCompletenessProfileSummaryResponse> GetSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var baris = await BaseQuery()
                .Select(x => new
                {
                    x.IsActive,
                    AdaKriteria = x.Items.Any(i => i.IsActive && !i.IsDelete)
                })
                .ToListAsync(cancellationToken);

            return new MedicalRecordCompletenessProfileSummaryResponse
            {
                TotalProfile = baris.Count,
                ActiveProfile = baris.Count(x => x.IsActive),
                InactiveProfile = baris.Count(x => !x.IsActive),
                ActiveProfileWithoutCriterion = baris.Count(x => x.IsActive && !x.AdaKriteria)
            };
        }

        /// <summary>
        /// Memeriksa nilai penyaring enum daftar. Mengembalikan pesan galat, atau <c>null</c> bila sah.
        /// Nilai di luar enum ditolak, bukan dijawab daftar kosong (`VAL-RM-A10`).
        /// </summary>
        public static string? ValidateFilter(
            EncounterType? encounterType,
            ServiceUnitType? serviceUnitType,
            CompletenessCondition? condition)
        {
            if ((encounterType.HasValue && !Enum.IsDefined(encounterType.Value))
                || (serviceUnitType.HasValue && !Enum.IsDefined(serviceUnitType.Value)))
                return UnknownTypeMessage;

            if (condition.HasValue && !Enum.IsDefined(condition.Value))
                return UnknownConditionMessage;

            return null;
        }

        public async Task<PagedResult<MedicalRecordCompletenessProfileListItemResponse>> GetPagedAsync(
            string? search,
            bool? isActive,
            EncounterType? encounterType,
            ServiceUnitType? serviceUnitType,
            CompletenessCondition? condition,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

            var query = BaseQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.Code.ToLower().Contains(keyword) ||
                    x.Name.ToLower().Contains(keyword));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (encounterType.HasValue)
                query = query.Where(x => x.EncounterType == encounterType.Value);

            if (serviceUnitType.HasValue)
                query = query.Where(x => x.ServiceUnitType == serviceUnitType.Value);

            if (condition.HasValue)
                query = query.Where(x => x.Condition == condition.Value);

            var menurun = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            query = (sortBy?.Trim().ToLowerInvariant()) switch
            {
                "name" => menurun ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "createdatetime" => menurun ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "updatedatetime" => menurun ? query.OrderByDescending(x => x.UpdateDateTime) : query.OrderBy(x => x.UpdateDateTime),
                _ => menurun ? query.OrderByDescending(x => x.Code) : query.OrderBy(x => x.Code)
            };

            var totalData = await query.CountAsync(cancellationToken);

            var baris = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.EncounterType,
                    x.ServiceUnitType,
                    x.Condition,
                    CriterionCount = x.Items.Count(i => i.IsActive && !i.IsDelete),
                    x.IsActive,
                    x.CreateDateTime,
                    x.UpdateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<MedicalRecordCompletenessProfileListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = baris.Select(x => new MedicalRecordCompletenessProfileListItemResponse
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    EncounterType = x.EncounterType,
                    EncounterTypeName = EncounterTypeLabel(x.EncounterType),
                    ServiceUnitType = x.ServiceUnitType,
                    ServiceUnitTypeName = ServiceUnitTypeLabel(x.ServiceUnitType),
                    Condition = x.Condition,
                    ConditionName = ConditionLabel(x.Condition),
                    CriterionCount = x.CriterionCount,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                }).ToList()
            };
        }

        public async Task<MedicalRecordCompletenessProfileDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var profil = await BaseQuery()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            return profil == null ? null : ToDetail(profil);
        }

        /// <summary>Katalog kriteria beserta artinya, terurut menurut nilai.</summary>
        public static List<MedicalRecordCompletenessCriterionOptionResponse> GetCriterionOptions()
            => Enum.GetValues<CompletenessCriterion>()
                .OrderBy(x => (int)x)
                .Select(x => new MedicalRecordCompletenessCriterionOptionResponse
                {
                    Value = (int)x,
                    Name = x.ToString(),
                    Label = CriterionLabel(x),
                    Description = CriterionDescription(x)
                })
                .ToList();

        // =====================================================================
        // Perubahan
        // =====================================================================

        public async Task<MedicalRecordCompletenessProfileResult> CreateAsync(
            CreateMedicalRecordCompletenessProfileRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Code))
                return Failed(MedicalRecordCompletenessProfileStatus.Invalid, "Kode profil wajib diisi.");

            var gagal = Validate(request.Name, request.EncounterType, request.ServiceUnitType, request.Condition, request.Items);
            if (gagal != null)
                return gagal;

            var kode = request.Code.Trim().ToUpperInvariant();

            if (await KodeTerpakaiAsync(kode, cancellationToken))
                return Failed(MedicalRecordCompletenessProfileStatus.Conflict, DuplicateCodeMessage(kode));

            if (await KunciTerpakaiAsync(request.EncounterType, request.ServiceUnitType, request.Condition, excludeId: null, cancellationToken))
                return Failed(MedicalRecordCompletenessProfileStatus.Conflict, DuplicateKeyMessage);

            var sekarang = DateTime.UtcNow;
            var profil = new MstMedicalRecordCompletenessProfile
            {
                Id = Guid.NewGuid(),
                Code = kode,
                Name = request.Name.Trim(),
                EncounterType = request.EncounterType,
                ServiceUnitType = request.ServiceUnitType,
                Condition = request.Condition,
                IsActive = true,
                CreateDateTime = sekarang,
                CreateBy = actorUserId
            };

            foreach (var kriteria in request.Items.Select(x => x.Criterion))
            {
                profil.Items.Add(new MstMedicalRecordCompletenessProfileItem
                {
                    Id = Guid.NewGuid(),
                    ProfileId = profil.Id,
                    Criterion = kriteria,
                    IsActive = true,
                    CreateDateTime = sekarang,
                    CreateBy = actorUserId
                });
            }

            _dbContext.Set<MstMedicalRecordCompletenessProfile>().Add(profil);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Penjaga terakhir: dua petugas menyimpan kode atau kunci yang sama hampir bersamaan,
                // keduanya lolos pemeriksaan di atas, dan index unik menolak salah satunya.
                _dbContext.Entry(profil).State = EntityState.Detached;
                foreach (var butir in profil.Items)
                    _dbContext.Entry(butir).State = EntityState.Detached;

                return Failed(MedicalRecordCompletenessProfileStatus.Conflict, DuplicateKeyMessage);
            }

            await TandaiPenilaianUlangAsync(sekarang, cancellationToken);

            return new MedicalRecordCompletenessProfileResult(
                MedicalRecordCompletenessProfileStatus.Success,
                ToDetail(profil),
                "Profil dokumen wajib berhasil dibuat.",
                $"Membuat profil dokumen wajib {kode}. Sesudah: {Uraian(profil)}.");
        }

        /// <summary>
        /// Mengubah profil dan mengganti seluruh kriterianya (full update). Kode tidak dapat diubah.
        /// </summary>
        public async Task<MedicalRecordCompletenessProfileResult> UpdateAsync(
            Guid id,
            UpdateMedicalRecordCompletenessProfileRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var profil = await TrackedAsync(id, cancellationToken);
            if (profil == null)
                return Failed(MedicalRecordCompletenessProfileStatus.NotFound, NotFoundMessage);

            var gagal = Validate(request.Name, request.EncounterType, request.ServiceUnitType, request.Condition, request.Items);
            if (gagal != null)
                return gagal;

            if (await KunciTerpakaiAsync(request.EncounterType, request.ServiceUnitType, request.Condition, excludeId: id, cancellationToken))
                return Failed(MedicalRecordCompletenessProfileStatus.Conflict, DuplicateKeyMessage);

            var sebelum = Uraian(profil);
            var sekarang = DateTime.UtcNow;

            profil.Name = request.Name.Trim();
            profil.EncounterType = request.EncounterType;
            profil.ServiceUnitType = request.ServiceUnitType;
            profil.Condition = request.Condition;
            profil.UpdateDateTime = sekarang;
            profil.UpdateBy = actorUserId;

            var dipilih = request.Items.Select(x => x.Criterion).ToHashSet();

            // Kriteria tidak pernah dihapus: yang tidak dipilih lagi dinonaktifkan, yang dipilih
            // ulang diaktifkan kembali. Index unik (profil, kriteria) karenanya tidak pernah
            // menolak kriteria yang dulu pernah ada.
            foreach (var butir in profil.Items)
            {
                var aktif = dipilih.Contains(butir.Criterion);
                if (butir.IsActive == aktif)
                    continue;

                butir.IsActive = aktif;
                butir.UpdateDateTime = sekarang;
                butir.UpdateBy = actorUserId;
            }

            foreach (var kriteria in dipilih.Where(x => profil.Items.All(i => i.Criterion != x)))
            {
                var butir = new MstMedicalRecordCompletenessProfileItem
                {
                    Id = Guid.NewGuid(),
                    ProfileId = profil.Id,
                    Criterion = kriteria,
                    IsActive = true,
                    CreateDateTime = sekarang,
                    CreateBy = actorUserId
                };
                profil.Items.Add(butir);
                _dbContext.Set<MstMedicalRecordCompletenessProfileItem>().Add(butir);
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                return Failed(MedicalRecordCompletenessProfileStatus.Conflict, DuplicateKeyMessage);
            }

            await TandaiPenilaianUlangAsync(sekarang, cancellationToken);

            return new MedicalRecordCompletenessProfileResult(
                MedicalRecordCompletenessProfileStatus.Success,
                ToDetail(profil),
                "Profil dokumen wajib berhasil diubah.",
                $"Mengubah profil dokumen wajib {profil.Code}. Sebelum: {sebelum}. Sesudah: {Uraian(profil)}.");
        }

        /// <summary>
        /// Mengaktifkan atau menonaktifkan profil. Satu-satunya cara "menghapus" profil: tidak ada
        /// jalur penghapusan, supaya penilaian lama yang menyebut profil ini tetap dapat dibaca.
        /// </summary>
        public async Task<MedicalRecordCompletenessProfileResult> UpdateStatusAsync(
            Guid id,
            bool isActive,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var profil = await TrackedAsync(id, cancellationToken);
            if (profil == null)
                return Failed(MedicalRecordCompletenessProfileStatus.NotFound, NotFoundMessage);

            var sebelum = profil.IsActive;
            var sekarang = DateTime.UtcNow;

            profil.IsActive = isActive;
            profil.UpdateDateTime = sekarang;
            profil.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (sebelum != isActive)
                await TandaiPenilaianUlangAsync(sekarang, cancellationToken);

            return new MedicalRecordCompletenessProfileResult(
                MedicalRecordCompletenessProfileStatus.Success,
                ToDetail(profil),
                isActive
                    ? "Profil dokumen wajib berhasil diaktifkan."
                    : "Profil dokumen wajib berhasil dinonaktifkan.",
                $"Mengubah status profil dokumen wajib {profil.Code}. Sebelum: {StatusLabel(sebelum)}. Sesudah: {StatusLabel(isActive)}.");
        }

        /// <summary>
        /// `BE-40` kriteria 3 (`EVT-RM-03`): sesudah aturan berubah, penilaian yang belum lengkap
        /// dijadikan jatuh tempo supaya putaran pemeriksaan berkala berikutnya menilainya ulang.
        /// </summary>
        /// <remarks>
        /// Perubahan profil sudah tersimpan saat ini dipanggil. Bila penandaan gagal (basis data
        /// sesaat tak terjangkau), perubahan profil tetap berhasil: penilaian itu tetap tersusul
        /// pada jadwal <c>NextCheckAt</c>-nya sendiri, paling lambat 24 jam.
        /// </remarks>
        private async Task TandaiPenilaianUlangAsync(DateTime sekarang, CancellationToken cancellationToken)
        {
            try
            {
                await QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services.MedicalRecordCompletenessService
                    .MarkIncompleteDueAsync(_dbContext, sekarang, cancellationToken);
            }
            catch (DbException)
            {
            }
        }

        // =====================================================================
        // Metadata
        // =====================================================================

        public static MedicalRecordCompletenessProfileFilterMetadataResponse BuildFilterMetadata()
            => new()
            {
                DefaultFilter = new MedicalRecordCompletenessProfileDefaultFilterResponse(),
                EncounterTypeOptions = Enum.GetValues<EncounterType>()
                    .Select(x => Opsi((int)x, x.ToString(), EncounterTypeLabel(x)))
                    .ToList(),
                ServiceUnitTypeOptions = Enum.GetValues<ServiceUnitType>()
                    .Select(x => Opsi((int)x, x.ToString(), ServiceUnitTypeLabel(x)))
                    .ToList(),
                ConditionOptions = Enum.GetValues<CompletenessCondition>()
                    .Select(x => Opsi((int)x, x.ToString(), ConditionLabel(x)))
                    .ToList(),
                SortOptions = new List<MedicalRecordCompletenessProfileSortOptionResponse>
                {
                    new() { Value = "code", Label = "Kode Profil" },
                    new() { Value = "name", Label = "Nama Profil" },
                    new() { Value = "createDateTime", Label = "Tanggal Dibuat" },
                    new() { Value = "updateDateTime", Label = "Tanggal Diubah" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                QueryParameters = new List<MedicalRecordCompletenessProfileQueryParameterInfoResponse>
                {
                    new() { Name = "search", Type = "string", Description = "Mencari pada kode dan nama profil.", Example = "RJ" },
                    new() { Name = "isActive", Type = "boolean", Description = "Menyaring profil aktif atau nonaktif.", Example = "true" },
                    new() { Name = "encounterType", Type = "integer", Description = "Jenis kunjungan, dari encounterTypeOptions.", Example = "1" },
                    new() { Name = "serviceUnitType", Type = "integer", Description = "Jenis unit layanan, dari serviceUnitTypeOptions.", Example = "4" },
                    new() { Name = "condition", Type = "integer", Description = "Kondisi bersyarat, dari conditionOptions. 0 = tanpa kondisi.", Example = "0" },
                    new() { Name = "sortBy", Type = "string", Description = "Kolom pengurutan. Bawaannya code.", Example = "code" },
                    new() { Name = "sortDirection", Type = "string", Description = "Arah pengurutan, asc atau desc.", Example = "asc" },
                    new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, dimulai dari 1.", Example = "1" },
                    new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, paling banyak 100.", Example = "25" }
                },
                CreateFields = BuildFormFields(isCreate: true),
                UpdateFields = BuildFormFields(isCreate: false)
            };

        private static List<MedicalRecordCompletenessProfileFormFieldMetadataResponse> BuildFormFields(bool isCreate)
        {
            var isian = new List<MedicalRecordCompletenessProfileFormFieldMetadataResponse>();

            if (isCreate)
            {
                isian.Add(new()
                {
                    Name = "code", Label = "Kode Profil", Section = "Profil", InputType = "text",
                    IsRequiredOnCreate = true, RequiredType = "Required", MaxLength = 30,
                    Description = "Kode singkat profil. Tidak dapat diubah sesudah disimpan.", Example = "RJ-GIGI", SortOrder = 1
                });
            }

            isian.Add(new()
            {
                Name = "name", Label = "Nama Profil", Section = "Profil", InputType = "text",
                IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150,
                Example = "Rawat Jalan Gigi", SortOrder = 2
            });
            isian.Add(new()
            {
                Name = "condition", Label = "Kondisi", Section = "Kunci", InputType = "select",
                IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required",
                OptionsSource = "conditionOptions",
                Description = "Tanpa kondisi = profil jenis kunjungan. Profil kondisi berlaku di atas profil jenis kunjungan.",
                Example = "0", SortOrder = 3
            });
            isian.Add(new()
            {
                Name = "encounterType", Label = "Jenis Kunjungan", Section = "Kunci", InputType = "select",
                RequiredType = "Conditional", OptionsSource = "encounterTypeOptions",
                Description = "Wajib bila kondisi = tanpa kondisi. Kosong = semua jenis kunjungan.",
                Example = "1", SortOrder = 4
            });
            isian.Add(new()
            {
                Name = "serviceUnitType", Label = "Jenis Unit Layanan", Section = "Kunci", InputType = "select",
                OptionsSource = "serviceUnitTypeOptions",
                Description = "Kosong = semua jenis unit. Profil berunit menggantikan profil umum jenis kunjungan yang sama.",
                Example = "4", SortOrder = 5
            });
            isian.Add(new()
            {
                Name = "items", Label = "Dokumen Wajib", Section = "Kriteria", InputType = "multiselect",
                OptionsSource = "/criteria/options",
                Description = "Kriteria dari katalog. Boleh kosong: berarti tidak ada dokumen dokter yang dituntut.",
                Example = "[{\"criterion\":4}]", SortOrder = 6
            });

            return isian;
        }

        // =====================================================================
        // Label
        // =====================================================================

        public static string EncounterTypeLabel(EncounterType? jenis) => jenis switch
        {
            null => "Semua jenis kunjungan",
            EncounterType.Outpatient => "Rawat Jalan",
            EncounterType.Emergency => "Gawat Darurat",
            EncounterType.Inpatient => "Rawat Inap",
            EncounterType.MedicalCheckup => "Medical Check Up",
            EncounterType.Telemedicine => "Telemedicine",
            _ => "Tidak diketahui"
        };

        public static string ServiceUnitTypeLabel(ServiceUnitType? jenis) => jenis switch
        {
            null => "Semua jenis unit",
            ServiceUnitType.Outpatient => "Rawat Jalan",
            ServiceUnitType.Inpatient => "Rawat Inap",
            ServiceUnitType.Emergency => "Gawat Darurat",
            ServiceUnitType.Laboratory => "Laboratorium",
            ServiceUnitType.Radiology => "Radiologi",
            ServiceUnitType.Pharmacy => "Farmasi",
            ServiceUnitType.MedicalCheckup => "Medical Check Up",
            ServiceUnitType.OperatingRoom => "Kamar Operasi",
            ServiceUnitType.DeliveryRoom => "Kamar Bersalin",
            ServiceUnitType.Hemodialysis => "Hemodialisa",
            ServiceUnitType.Other => "Lainnya",
            _ => "Tidak diketahui"
        };

        public static string ConditionLabel(CompletenessCondition kondisi) => kondisi switch
        {
            CompletenessCondition.None => "Tanpa kondisi",
            CompletenessCondition.Surgery => "Operasi",
            CompletenessCondition.HemodialysisSession => "Sesi hemodialisa",
            CompletenessCondition.Transfusion => "Transfusi",
            _ => kondisi.ToString()
        };

        public static string CriterionLabel(CompletenessCriterion kriteria) => kriteria switch
        {
            CompletenessCriterion.InitialAssessmentAny => "Asesmen awal (keperawatan atau medis)",
            CompletenessCriterion.InitialNursingAssessment => "Asesmen awal keperawatan",
            CompletenessCriterion.InitialMedicalAssessment => "Asesmen awal medis",
            CompletenessCriterion.PrimaryDiagnosis => "Diagnosis utama",
            CompletenessCriterion.DoctorNoteOrConsultation => "CPPT dokter atau konsultasi dokter",
            CompletenessCriterion.ProgressNoteAny => "CPPT",
            CompletenessCriterion.EmergencyTriage => "Triase IGD",
            CompletenessCriterion.EmergencyDisposition => "Disposisi IGD",
            CompletenessCriterion.InpatientGeneralConsent => "Persetujuan umum rawat inap",
            CompletenessCriterion.DischargeSummary => "Ringkasan pulang",
            CompletenessCriterion.McuResume => "Resume MCU",
            CompletenessCriterion.SurgeryReport => "Laporan operasi",
            CompletenessCriterion.AnesthesiaRecord => "Catatan anestesi",
            CompletenessCriterion.SurgicalSafetyChecklist => "Daftar tilik keselamatan operasi",
            CompletenessCriterion.HemodialysisSessionSigned => "Catatan sesi hemodialisa ditandatangani",
            CompletenessCriterion.TransfusionMonitoring => "Monitoring transfusi",
            _ => kriteria.ToString()
        };

        /// <summary>Arti "terpenuhi" per kriteria, mengikuti katalog A4.11.</summary>
        public static string CriterionDescription(CompletenessCriterion kriteria) => kriteria switch
        {
            CompletenessCriterion.InitialAssessmentAny => "Ada asesmen awal keperawatan atau asesmen awal medis yang berstatus selesai.",
            CompletenessCriterion.InitialNursingAssessment => "Ada asesmen awal keperawatan yang berstatus selesai.",
            CompletenessCriterion.InitialMedicalAssessment => "Ada asesmen awal medis yang berstatus selesai.",
            CompletenessCriterion.PrimaryDiagnosis => "Ada diagnosis bertanda utama atau berjenis primer, yang tidak dibatalkan atau disingkirkan.",
            CompletenessCriterion.DoctorNoteOrConsultation => "Ada CPPT berprofesi dokter atau konsultasi dokter yang tidak dibatalkan.",
            CompletenessCriterion.ProgressNoteAny => "Ada minimal satu CPPT yang tidak dibatalkan.",
            CompletenessCriterion.EmergencyTriage => "Ada triase IGD yang berstatus selesai.",
            CompletenessCriterion.EmergencyDisposition => "Ada disposisi IGD yang sudah dikonfirmasi atau dijalankan.",
            CompletenessCriterion.InpatientGeneralConsent => "Ada persetujuan umum rawat inap pada dokumen admisi yang berstatus selesai.",
            CompletenessCriterion.DischargeSummary => "Ada ringkasan pulang yang sudah ditandatangani.",
            CompletenessCriterion.McuResume => "Ada versi resume MCU yang pernah diverifikasi.",
            CompletenessCriterion.SurgeryReport => "Ada laporan operasi yang sudah final.",
            CompletenessCriterion.AnesthesiaRecord => "Ada catatan anestesi yang sudah final.",
            CompletenessCriterion.SurgicalSafetyChecklist => "Ada daftar tilik keselamatan operasi yang berstatus selesai.",
            CompletenessCriterion.HemodialysisSessionSigned => "Setiap sesi hemodialisa yang selesai sudah ditandatangani.",
            CompletenessCriterion.TransfusionMonitoring => "Setiap monitoring transfusi yang tidak dibatalkan berstatus selesai.",
            _ => string.Empty
        };

        private static string StatusLabel(bool aktif) => aktif ? "aktif" : "nonaktif";

        // =====================================================================
        // Alat bantu
        // =====================================================================

        /// <summary>
        /// Urutan pemeriksaan mengikuti kode status kontrak: nilai enum tidak sah dan jenis
        /// kunjungan yang hilang (`400`) lebih dulu, lalu kriteria tidak dikenal (`400`), lalu
        /// kriteria kembar (`409`). Kode dan kunci kembar diperiksa pemanggil ke basis data.
        /// </summary>
        private static MedicalRecordCompletenessProfileResult? Validate(
            string? name,
            EncounterType? encounterType,
            ServiceUnitType? serviceUnitType,
            CompletenessCondition condition,
            IReadOnlyCollection<MedicalRecordCompletenessProfileItemRequest>? items)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Failed(MedicalRecordCompletenessProfileStatus.Invalid, "Nama profil wajib diisi.");

            var galatSaringan = ValidateFilter(encounterType, serviceUnitType, condition);
            if (galatSaringan != null)
                return Failed(MedicalRecordCompletenessProfileStatus.Invalid, galatSaringan);

            if (condition == CompletenessCondition.None && encounterType == null)
                return Failed(MedicalRecordCompletenessProfileStatus.Invalid, EncounterTypeRequiredMessage);

            var kriteria = (items ?? []).Select(x => x?.Criterion).ToList();

            if (kriteria.Any(x => x == null || !Enum.IsDefined(x.Value)))
                return Failed(MedicalRecordCompletenessProfileStatus.Invalid, UnknownCriterionMessage);

            if (kriteria.Distinct().Count() != kriteria.Count)
                return Failed(MedicalRecordCompletenessProfileStatus.Conflict, DuplicateCriterionMessage);

            return null;
        }

        private IQueryable<MstMedicalRecordCompletenessProfile> BaseQuery()
            => _dbContext.Set<MstMedicalRecordCompletenessProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

        private async Task<MstMedicalRecordCompletenessProfile?> TrackedAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var profil = await _dbContext.Set<MstMedicalRecordCompletenessProfile>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (profil == null)
                return null;

            // Butir terhapus tidak dimuat, supaya index unik (profil, kriteria) yang hanya berlaku
            // pada baris belum terhapus tetap selaras dengan yang dilihat service.
            await _dbContext.Entry(profil)
                .Collection(x => x.Items)
                .Query()
                .Where(x => !x.IsDelete)
                .LoadAsync(cancellationToken);

            return profil;
        }

        private Task<bool> KodeTerpakaiAsync(string kode, CancellationToken cancellationToken)
            => _dbContext.Set<MstMedicalRecordCompletenessProfile>()
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && x.Code.ToUpper() == kode, cancellationToken);

        private Task<bool> KunciTerpakaiAsync(
            EncounterType? encounterType,
            ServiceUnitType? serviceUnitType,
            CompletenessCondition condition,
            Guid? excludeId,
            CancellationToken cancellationToken)
        {
            // Kosong dibandingkan sebagai sama dengan kosong, seperti NULLS NOT DISTINCT pada index.
            var query = _dbContext.Set<MstMedicalRecordCompletenessProfile>()
                .AsNoTracking()
                .Where(x => !x.IsDelete
                            && x.Condition == condition
                            && (encounterType == null ? x.EncounterType == null : x.EncounterType == encounterType)
                            && (serviceUnitType == null ? x.ServiceUnitType == null : x.ServiceUnitType == serviceUnitType));

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return query.AnyAsync(cancellationToken);
        }

        private static string DuplicateCodeMessage(string kode)
            => $"Kode profil {kode} sudah dipakai profil lain.";

        /// <summary>Keadaan profil dalam satu kalimat, untuk jejak sebelum-sesudah.</summary>
        private static string Uraian(MstMedicalRecordCompletenessProfile profil)
        {
            var kriteria = profil.Items
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => (int)x.Criterion)
                .Select(x => x.Criterion.ToString())
                .ToList();

            // Kutip tunggal: kutip ganda di-escape logger menjadi \" sehingga sulit dibaca di log.
            return $"nama '{profil.Name}', jenis kunjungan {EncounterTypeLabel(profil.EncounterType)}, "
                   + $"jenis unit {ServiceUnitTypeLabel(profil.ServiceUnitType)}, kondisi {ConditionLabel(profil.Condition)}, "
                   + $"status {StatusLabel(profil.IsActive)}, kriteria [{(kriteria.Count == 0 ? "tanpa kriteria" : string.Join(", ", kriteria))}]";
        }

        public static MedicalRecordCompletenessProfileDetailResponse ToDetail(MstMedicalRecordCompletenessProfile profil)
        {
            var butir = profil.Items
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => (int)x.Criterion)
                .Select(x => new MedicalRecordCompletenessProfileItemResponse
                {
                    Id = x.Id,
                    Criterion = x.Criterion,
                    CriterionName = CriterionLabel(x.Criterion),
                    Description = CriterionDescription(x.Criterion)
                })
                .ToList();

            return new MedicalRecordCompletenessProfileDetailResponse
            {
                Id = profil.Id,
                Code = profil.Code,
                Name = profil.Name,
                EncounterType = profil.EncounterType,
                EncounterTypeName = EncounterTypeLabel(profil.EncounterType),
                ServiceUnitType = profil.ServiceUnitType,
                ServiceUnitTypeName = ServiceUnitTypeLabel(profil.ServiceUnitType),
                Condition = profil.Condition,
                ConditionName = ConditionLabel(profil.Condition),
                CriterionCount = butir.Count,
                IsActive = profil.IsActive,
                CreateDateTime = profil.CreateDateTime,
                CreateBy = profil.CreateBy,
                UpdateDateTime = profil.UpdateDateTime,
                UpdateBy = profil.UpdateBy == Guid.Empty ? null : profil.UpdateBy,
                Items = butir
            };
        }

        private static MedicalRecordCompletenessProfileEnumOptionResponse Opsi(int nilai, string nama, string label)
            => new() { Value = nilai, Name = nama, Label = label };

        private static MedicalRecordCompletenessProfileResult Failed(
            MedicalRecordCompletenessProfileStatus status,
            string message)
            => new(status, null, message, null);
    }

    public enum MedicalRecordCompletenessProfileStatus
    {
        Success = 0,
        NotFound = 1,
        Invalid = 2,
        Conflict = 3
    }

    /// <param name="AuditMessage">
    /// Kalimat jejak berisi keadaan sebelum dan sesudah. Hanya terisi bila perubahan tersimpan.
    /// </param>
    public sealed record MedicalRecordCompletenessProfileResult(
        MedicalRecordCompletenessProfileStatus Status,
        MedicalRecordCompletenessProfileDetailResponse? Profile,
        string Message,
        string? AuditMessage);
}
