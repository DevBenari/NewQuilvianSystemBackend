using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Pilihan untuk form rehab (<c>BE-RHB-006</c>, <c>contracts/api-contract.md</c> v1 grup Rehab
    /// Lookup): pasien, kunjungan terbuka milik pasien, surat rujukan internal, dokter, disiplin
    /// aktif, tindakan per disiplin, dan instrumen yang berlaku.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Baca saja; tidak membuka transaksi. Data milik modul lain dibaca di sini supaya petugas rehab
    /// tidak perlu diberi hak akses modul pendaftaran (RHB-DEC-016, INT-RHB-01..04, 07).
    /// </para>
    /// <para>
    /// <b>Anti-IDOR.</b> Kunjungan, surat rujukan, dan instrumen selalu tersaring ke satu
    /// <c>patientId</c> yang diminta; tanpa <c>patientId</c> permintaan ditolak. Tidak ada daftar
    /// kunjungan lintas pasien.
    /// </para>
    /// </remarks>
    public class RhbLookupService
    {
        public const int PatientSearchMinLength = 3;
        public const int PatientMaxResult = 20;
        public const int DoctorMaxResult = 50;

        private readonly ApplicationDbContext _dbContext;
        private readonly EncounterReferralService _referralService;
        private readonly ClinicalInstrumentService _instrumentService;

        public RhbLookupService(
            ApplicationDbContext dbContext,
            EncounterReferralService referralService,
            ClinicalInstrumentService instrumentService)
        {
            _dbContext = dbContext;
            _referralService = referralService;
            _instrumentService = instrumentService;
        }

        public async Task<RhbResult<List<RehabPatientOption>>> SearchPatientsAsync(
            RehabPatientLookupQuery query,
            CancellationToken cancellationToken)
        {
            var keyword = RhbServiceSupport.Normalize(query.Search);
            if (keyword == null || keyword.Length < PatientSearchMinLength)
                return RhbResult<List<RehabPatientOption>>.Invalid(
                    RhbErrorCodes.LookupSearchTooShort,
                    $"Ketik minimal {PatientSearchMinLength} karakter nama atau nomor rekam medis.")
                    .WithFields("search");

            var take = query.PageSize <= 0 || query.PageSize > PatientMaxResult ? PatientMaxResult : query.PageSize;
            var lowered = keyword.ToLower();

            var rows = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive &&
                            (x.MedicalRecordNumber.ToLower().Contains(lowered) || x.FullName.ToLower().Contains(lowered)))
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.MedicalRecordNumber)
                .Take(take)
                .Select(x => new RehabPatientOption
                {
                    Id = x.Id,
                    MedicalRecordNumber = x.MedicalRecordNumber,
                    FullName = x.FullName,
                    BirthDate = x.BirthDate,
                    Gender = x.Gender
                })
                .ToListAsync(cancellationToken);

            return RhbResult<List<RehabPatientOption>>.Ok(rows);
        }

        public async Task<RhbResult<List<RehabEncounterOption>>> GetOpenEncountersAsync(
            Guid? patientId,
            EncounterType? encounterType,
            CancellationToken cancellationToken)
        {
            if (!patientId.HasValue || patientId.Value == Guid.Empty)
                return PatientRequired<List<RehabEncounterOption>>();

            var query = RhbServiceSupport.OpenEncountersOfPatient(_dbContext, patientId.Value);
            if (encounterType.HasValue)
                query = query.Where(x => x.EncounterType == encounterType.Value);

            var rows = await query
                .OrderByDescending(x => x.EncounterDate)
                .Select(x => new RehabEncounterOption
                {
                    Id = x.Id,
                    EncounterNumber = x.EncounterNumber,
                    EncounterType = x.EncounterType,
                    EncounterStatus = x.EncounterStatus,
                    EncounterDate = x.EncounterDate,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    DoctorId = x.DoctorId,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null
                })
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
                row.EncounterTypeName = EncounterTypeName(row.EncounterType);

            return RhbResult<List<RehabEncounterOption>>.Ok(rows);
        }

        /// <summary>
        /// Surat rujukan internal milik pasien yang masih dapat dipakai — memakai aturan pendaftaran
        /// yang sudah ada (terbit dan belum dipakai kunjungan aktif lain).
        /// </summary>
        public async Task<RhbResult<List<RehabReferralLetterOption>>> GetInternalReferralLettersAsync(
            Guid? patientId,
            CancellationToken cancellationToken)
        {
            if (!patientId.HasValue || patientId.Value == Guid.Empty)
                return PatientRequired<List<RehabReferralLetterOption>>();

            var letters = await _referralService.GetInternalLetterOptionsAsync(patientId.Value, null, cancellationToken);

            return RhbResult<List<RehabReferralLetterOption>>.Ok(letters
                .Select(x => new RehabReferralLetterOption
                {
                    Id = x.CertificateId,
                    CertificateNumber = x.CertificateNumber,
                    IssuedDate = x.IssuedDate,
                    DoctorName = x.DoctorName,
                    SourceEncounterId = x.SourceEncounterId,
                    SourceEncounterNumber = x.SourceEncounterNumber,
                    SourceClinicName = x.SourceClinicName,
                    TargetClinicId = x.TargetClinicId,
                    TargetName = x.TargetName
                })
                .ToList());
        }

        public async Task<List<RehabDoctorOption>> GetDoctorsAsync(string? search, CancellationToken cancellationToken)
        {
            var rows = _dbContext.Set<MstDoctor>().AsNoTracking().Where(x => !x.IsDelete && x.IsActive);

            var keyword = RhbServiceSupport.Normalize(search)?.ToLower();
            if (keyword != null)
                rows = rows.Where(x => x.FullName.ToLower().Contains(keyword) || x.DoctorCode.ToLower().Contains(keyword));

            return await rows
                .OrderBy(x => x.FullName)
                .Take(DoctorMaxResult)
                .Select(x => new RehabDoctorOption
                {
                    Id = x.Id,
                    DoctorCode = x.DoctorCode,
                    FullName = x.FullName,
                    ProfessionName = _dbContext.Set<MstProfession>()
                        .Where(p => p.Id == x.ProfessionId)
                        .Select(p => p.ProfessionName)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Disiplin yang dapat diresepkan: aktif dan sudah punya profesi. Validasi
        /// <c>RHB-VAL-070</c> menjamin disiplin aktif selalu berprofesi; penyaring profesi di sini
        /// hanya penjaga tambahan.
        /// </summary>
        public async Task<List<RehabDisciplineLookupOption>> GetDisciplinesAsync(CancellationToken cancellationToken) =>
            await _dbContext.RhbDisciplines
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.ProfessionId != null)
                .OrderBy(x => x.DisciplineName)
                .Select(x => new RehabDisciplineLookupOption
                {
                    Id = x.Id,
                    DisciplineCode = x.DisciplineCode,
                    DisciplineName = x.DisciplineName,
                    ProfessionId = x.ProfessionId!.Value,
                    ProfessionName = _dbContext.Set<MstProfession>()
                        .Where(p => p.Id == x.ProfessionId)
                        .Select(p => p.ProfessionName)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

        /// <summary>Tindakan aktif yang boleh dipakai disiplin aktif, dan masih aktif di master tindakan.</summary>
        public async Task<RhbResult<List<RehabProcedureOption>>> GetDisciplineProceduresAsync(
            Guid disciplineId,
            CancellationToken cancellationToken)
        {
            if (!await IsActiveDisciplineAsync(disciplineId, cancellationToken))
                return RhbResult<List<RehabProcedureOption>>.NotFound("Layanan rehab tidak ditemukan atau tidak aktif.");

            var rows = await (
                    from map in _dbContext.RhbDisciplineProcedures.AsNoTracking()
                    join procedure in _dbContext.Set<MstProcedure>().AsNoTracking()
                        on map.ProcedureId equals procedure.Id
                    where map.DisciplineId == disciplineId && map.IsActive && !map.IsDelete &&
                          procedure.IsActive && !procedure.IsDelete
                    orderby procedure.ProcedureName
                    select new RehabProcedureOption
                    {
                        ProcedureId = procedure.Id,
                        ProcedureCode = procedure.ProcedureCode,
                        ProcedureName = procedure.ProcedureName
                    })
                .ToListAsync(cancellationToken);

            return RhbResult<List<RehabProcedureOption>>.Ok(rows);
        }

        /// <summary>
        /// Instrumen yang berlaku bagi pasien untuk satu disiplin, atau untuk asesmen medis bila
        /// <c>disciplineId</c> kosong (INT-RHB-07).
        /// </summary>
        /// <remarks>
        /// Versi diambil dari <see cref="ClinicalInstrumentService.ResolveForPatientAsync"/>. Mesin itu
        /// juga mengembalikan versi konsep; di sini konsep hanya diteruskan bila lingkungan menyalakan
        /// <c>ClinicalConfiguration:AllowDraftVersionsForTesting</c>. Selain itu jenisnya tetap
        /// tercantum dengan <c>IsAvailable = false</c>, supaya layar tahu harus mencatat pengukuran
        /// terstruktur (RHB-DEC-017).
        /// </remarks>
        public async Task<RhbResult<List<RehabResolvedInstrument>>> GetInstrumentsAsync(
            RehabInstrumentLookupQuery query,
            CancellationToken cancellationToken)
        {
            if (!query.PatientId.HasValue || query.PatientId.Value == Guid.Empty)
                return PatientRequired<List<RehabResolvedInstrument>>();

            var patientExists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == query.PatientId.Value && !x.IsDelete, cancellationToken);
            if (!patientExists)
                return RhbResult<List<RehabResolvedInstrument>>.NotFound("Pasien tidak ditemukan.");

            if (query.DisciplineId.HasValue && !await IsActiveDisciplineAsync(query.DisciplineId.Value, cancellationToken))
                return RhbResult<List<RehabResolvedInstrument>>.NotFound("Layanan rehab tidak ditemukan atau tidak aktif.");

            var kinds = await _dbContext.RhbDisciplineInstruments
                .AsNoTracking()
                .Where(x => x.DisciplineId == query.DisciplineId && x.IsActive && !x.IsDelete)
                .OrderBy(x => x.InstrumentKind)
                .Select(x => x.InstrumentKind)
                .ToListAsync(cancellationToken);

            var draftAllowed = _instrumentService.IsDraftAllowedInThisEnvironment;
            var now = DateTime.UtcNow;
            var result = new List<RehabResolvedInstrument>();

            foreach (var kind in kinds)
            {
                var item = new RehabResolvedInstrument
                {
                    InstrumentKind = kind,
                    InstrumentKindName = RhbConfigurationService.InstrumentKindLabel(kind)
                };

                var resolved = await _instrumentService.ResolveForPatientAsync(kind, query.PatientId.Value, now, cancellationToken);
                if (!resolved.IsSuccess || resolved.Value == null)
                {
                    item.UnavailableReason = resolved.ErrorMessage ?? "Instrumen belum tersedia.";
                }
                else if (!resolved.Value.IsApproved && !draftAllowed)
                {
                    item.UnavailableReason = $"Instrumen {resolved.Value.InstrumentName} belum disahkan komite.";
                }
                else
                {
                    item.IsAvailable = true;
                    item.Instrument = resolved.Value;
                }

                result.Add(item);
            }

            return RhbResult<List<RehabResolvedInstrument>>.Ok(result);
        }

        private Task<bool> IsActiveDisciplineAsync(Guid disciplineId, CancellationToken cancellationToken) =>
            _dbContext.RhbDisciplines.AsNoTracking()
                .AnyAsync(x => x.Id == disciplineId && !x.IsDelete && x.IsActive, cancellationToken);

        private static RhbResult<T> PatientRequired<T>() =>
            RhbResult<T>.Invalid(RhbErrorCodes.PatientRequired, "Pilih pasien lebih dulu.").WithFields("patientId");

        private static string EncounterTypeName(EncounterType type) => type switch
        {
            EncounterType.Outpatient => "Rawat Jalan",
            EncounterType.Emergency => "Gawat Darurat",
            EncounterType.Inpatient => "Rawat Inap",
            EncounterType.MedicalCheckup => "Medical Check Up",
            EncounterType.Telemedicine => "Telemedisin",
            _ => "Tidak diketahui"
        };
    }
}
