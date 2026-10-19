using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Adapter sempit sisi Registrasi ke layanan HR (BE-FIN-106; FIN-INTEGRATION-1.9 R.2, INT-REG-HR-001;
    /// FIN-VAL-259, 260, 268, 269). Hanya membaca HR, tidak pernah menulisnya, dan tidak pernah menerima
    /// saldo plafon. Aturan "harus terdaftar" ditegakkan di sini untuk kartu atas penjamin internal
    /// (MstCompanyGuarantor.IsEmployeeBenefitGuarantor), dan gagal tertutup bila HR tidak dapat dibaca.
    ///
    /// Kode hubungan keluarga selalu berasal dari hasil pemeriksaan HR; klien tidak pernah menentukannya.
    /// </summary>
    public sealed class RegistrationEmployeeBenefitLookupService
    {
        private const string LogCategory = "HealthServices.RegistrationManagement.EmployeeBenefitLookup";

        public const string CardNotLinkedCode = "CARD_NOT_LINKED";

        public const string EmployeeRequiredMessage =
            "Penerima benefit (karyawan atau dokter tetap) wajib dipilih dari data HR untuk kartu penjamin internal. " +
            "Cari orangnya, atau daftarkan pasien sebagai Pasien Umum.";

        public const string RelationshipFromClientMessage = "Hubungan keluarga ditentukan sistem dari data HR.";

        public const string LinkOnlyForInternalMessage =
            "Tautan pegawai HR hanya dapat diisi untuk kartu penjamin internal.";

        public const string CardNotLinkedMessage =
            "Kartu penjamin internal ini belum tertaut ke data pegawai HR. " +
            "Perbarui kartu pasien, atau daftarkan sebagai Pasien Umum.";

        private readonly ApplicationDbContext _dbContext;
        private readonly EmployeeBenefitEligibilityService _eligibilityService;
        private readonly LoggerService _loggerService;

        public RegistrationEmployeeBenefitLookupService(
            ApplicationDbContext dbContext,
            EmployeeBenefitEligibilityService eligibilityService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _eligibilityService = eligibilityService;
            _loggerService = loggerService;
        }

        // ------------------------------------------------------------------------------------
        // INT-REG-HR-001 R.2.1 - R.2.3 sebagai endpoint hanya-baca
        // ------------------------------------------------------------------------------------

        /// <summary>CariPegawai. Kata kunci minimal 3 karakter; paling banyak 20 baris.</summary>
        public async Task<(List<EmployeeLookupResponse> Items, bool IsTruncated)> SearchEmployeesAsync(
            string keyword, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _eligibilityService.SearchEmployeesAsync(keyword, cancellationToken);
                return (result.Items.Select(x => new EmployeeLookupResponse
                {
                    EmployeeId = x.EmployeeId,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeNumber = x.EmployeeNumber,
                    NationalIdMasked = x.NationalIdMasked,
                    FullName = x.FullName,
                    OrganizationUnitName = x.OrganizationUnitName,
                    HasActiveMedicalEnrollment = x.HasActiveMedicalEnrollment
                }).ToList(), result.IsTruncated);
            }
            catch (Exception exception) when (IsHrFailure(exception))
            {
                throw await HrUnavailableAsync("SearchEmployees", exception);
            }
        }

        /// <summary>
        /// CariPemilikBenefit: karyawan dan dokter tetap aktif dalam satu daftar, berbasis WorkforceProfileId.
        /// Kata kunci minimal 3 karakter; paling banyak 20 baris; NIK tersamar.
        /// </summary>
        public async Task<(List<WorkforceProfileLookupResponse> Items, bool IsTruncated)> SearchWorkforceAsync(
            string keyword, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _eligibilityService.SearchWorkforceAsync(keyword, cancellationToken);
                return (result.Items.Select(x => new WorkforceProfileLookupResponse
                {
                    WorkforceProfileId = x.WorkforceProfileId,
                    OwnerType = x.OwnerType,
                    EmployeeId = x.EmployeeId,
                    DoctorId = x.DoctorId,
                    StaffNumber = x.StaffNumber,
                    FullName = x.FullName,
                    NationalIdMasked = x.NationalIdMasked,
                    OrganizationUnitName = x.OrganizationUnitName,
                    EmploymentType = x.EmploymentType,
                    IsActive = x.IsActive,
                    HasActiveMedicalEnrollment = x.HasActiveMedicalEnrollment
                }).ToList(), result.IsTruncated);
            }
            catch (Exception exception) when (IsHrFailure(exception))
            {
                throw await HrUnavailableAsync("SearchWorkforce", exception);
            }
        }

        /// <summary>DaftarKeluarga berbasis WorkforceProfileId (karyawan atau dokter tetap).</summary>
        public async Task<List<FamilyMemberLookupResponse>> GetFamilyMembersByWorkforceAsync(
            Guid workforceProfileId, CancellationToken cancellationToken)
        {
            try
            {
                var items = await _eligibilityService.ListFamilyMembersByWorkforceAsync(workforceProfileId, cancellationToken);
                return items.Select(x => new FamilyMemberLookupResponse
                {
                    FamilyMemberId = x.FamilyMemberId,
                    FullName = x.FullName,
                    RelationshipCode = x.RelationshipCode,
                    AgeYears = x.AgeYears,
                    IdentityMasked = x.IdentityMasked,
                    DependentVerification = x.DependentVerification,
                    CoveredByPlan = x.CoveredByPlan
                }).ToList();
            }
            catch (Exception exception) when (IsHrFailure(exception))
            {
                throw await HrUnavailableAsync("GetFamilyMembersByWorkforce", exception);
            }
        }

        /// <summary>DaftarKeluarga. KeyNotFoundException bila pegawainya tidak ada atau tidak aktif.</summary>
        public async Task<List<FamilyMemberLookupResponse>> GetFamilyMembersAsync(
            Guid employeeId, CancellationToken cancellationToken)
        {
            try
            {
                var items = await _eligibilityService.ListFamilyMembersAsync(employeeId, cancellationToken);
                return items.Select(x => new FamilyMemberLookupResponse
                {
                    FamilyMemberId = x.FamilyMemberId,
                    FullName = x.FullName,
                    RelationshipCode = x.RelationshipCode,
                    AgeYears = x.AgeYears,
                    IdentityMasked = x.IdentityMasked,
                    DependentVerification = x.DependentVerification,
                    CoveredByPlan = x.CoveredByPlan
                }).ToList();
            }
            catch (Exception exception) when (IsHrFailure(exception))
            {
                throw await HrUnavailableAsync("GetFamilyMembers", exception);
            }
        }

        /// <summary>
        /// PeriksaKelayakan. Tidak melempar bila HR gagal dibaca: hasilnya ReasonCode HR_UNAVAILABLE
        /// supaya pemanggil dapat menampilkan jalur Pasien Umum.
        /// </summary>
        public Task<BenefitEligibilityResponse> CheckEligibilityAsync(
            Guid employeeId, Guid? familyMemberId, DateTime serviceDate, CancellationToken cancellationToken) =>
            CheckEligibilityAsync(null, employeeId, familyMemberId, serviceDate, cancellationToken);

        /// <summary>
        /// PeriksaKelayakan berbasis WorkforceProfileId (identitas utama). EmployeeId boleh kosong (dokter) atau
        /// diisi (kontrak lama); bila keduanya diisi harus orang yang sama.
        /// </summary>
        public async Task<BenefitEligibilityResponse> CheckEligibilityAsync(
            Guid? workforceProfileId, Guid? employeeId, Guid? familyMemberId, DateTime serviceDate, CancellationToken cancellationToken)
        {
            var check = await TryCheckAsync(workforceProfileId, employeeId, familyMemberId, serviceDate, cancellationToken);
            return new BenefitEligibilityResponse
            {
                IsEligible = check.IsEligible,
                ReasonCode = check.ReasonCode,
                Message = check.Message,
                RelationshipCode = check.RelationshipCode,
                BenefitPersonKind = check.BenefitPersonKind,
                BenefitPersonId = check.BenefitPersonId,
                EnrollmentId = check.EnrollmentId,
                WorkforceProfileId = check.WorkforceProfileId,
                OwnerType = check.OwnerType,
                OwnerName = check.StaffName,
                OwnerStaffNumber = check.StaffNumber,
                PlanCode = check.PlanCode,
                PlanName = check.PlanName,
                CoverageType = check.CoverageType,
                EffectiveStartDate = check.EffectiveStartDate,
                EffectiveEndDate = check.EffectiveEndDate
            };
        }

        // ------------------------------------------------------------------------------------
        // Penegakan aturan "harus terdaftar"
        // ------------------------------------------------------------------------------------

        /// <summary>
        /// Dipanggil saat kartu pasien dibuat atau diubah. Untuk penjamin biasa kartu tidak berubah; untuk
        /// penjamin internal pegawai wajib dipilih, kelayakan harus OK (FIN-VAL-259), identitas pasien
        /// dicocokkan dengan data HR (FIN-DEC-228), dan tautan serta kode hubungan diisi dari hasil pemeriksaan HR.
        /// </summary>
        public async Task<InternalCardLinkResult> ResolveCardLinkAsync(
            Guid companyGuarantorId, Guid? employeeId, Guid? familyMemberId, string? clientRelationship,
            Guid? patientId, CancellationToken cancellationToken) =>
            await ResolveCardLinkAsync(companyGuarantorId, null, employeeId, familyMemberId, clientRelationship, patientId, cancellationToken);

        /// <summary>
        /// Versi berbasis WorkforceProfileId: pemilik benefit bisa karyawan atau dokter tetap. Identitas pasien
        /// (NIK) wajib cocok dengan pemilik untuk hubungan SELF, sehingga seorang dokter tidak dapat memakai
        /// manfaat milik orang lain. Kartu tidak pernah dibuat otomatis hanya karena NIK cocok.
        /// </summary>
        public async Task<InternalCardLinkResult> ResolveCardLinkAsync(
            Guid companyGuarantorId, Guid? workforceProfileId, Guid? employeeId, Guid? familyMemberId, string? clientRelationship,
            Guid? patientId, CancellationToken cancellationToken)
        {
            var hasEmployee = (employeeId.HasValue && employeeId.Value != Guid.Empty)
                || (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty);
            var hasFamilyMember = familyMemberId.HasValue && familyMemberId.Value != Guid.Empty;

            // FIN-VAL-260: kode hubungan ditentukan sistem, klien tidak boleh mengirimnya.
            if (!string.IsNullOrWhiteSpace(clientRelationship))
                return InternalCardLinkResult.Reject(StatusCodes.Status400BadRequest, "RELATIONSHIP_FROM_CLIENT", RelationshipFromClientMessage);

            if (!await IsInternalGuarantorAsync(companyGuarantorId, cancellationToken))
            {
                return hasEmployee || hasFamilyMember
                    ? InternalCardLinkResult.Reject(StatusCodes.Status400BadRequest, "LINK_NOT_ALLOWED", LinkOnlyForInternalMessage)
                    : InternalCardLinkResult.NotInternal();
            }

            if (!hasEmployee)
                return InternalCardLinkResult.Reject(StatusCodes.Status422UnprocessableEntity, "EMPLOYEE_REQUIRED", EmployeeRequiredMessage);

            var check = await TryCheckAsync(
                workforceProfileId, employeeId, hasFamilyMember ? familyMemberId : null, AppDateTimeHelper.OperationalDate(), cancellationToken);
            if (!check.IsEligible)
            {
                var status = check.ReasonCode == BenefitEligibilityReasonCodes.HrUnavailable
                    ? StatusCodes.Status503ServiceUnavailable
                    : StatusCodes.Status422UnprocessableEntity;
                return InternalCardLinkResult.Reject(status, check.ReasonCode, check.Message ?? string.Empty);
            }

            // FIN-DEC-228: Validasi kecocokan identitas pasien dengan orang di HR
            if (patientId.HasValue && patientId.Value != Guid.Empty)
            {
                var patient = await _dbContext.Set<MstPatient>().AsNoTracking()
                    .Where(x => x.Id == patientId.Value && !x.IsDelete)
                    .Select(x => new { x.Id, x.FullName, x.BirthDate, x.IdentityNumber })
                    .SingleOrDefaultAsync(cancellationToken);

                if (patient == null)
                {
                    return InternalCardLinkResult.Reject(
                        StatusCodes.Status404NotFound,
                        "PATIENT_NOT_FOUND",
                        "Data pasien tidak ditemukan.");
                }

                var patientNik = NormalizeNik(patient.IdentityNumber);
                var targetNik = NormalizeNik(check.TargetPersonNationalId);

                if (check.RelationshipCode == BenefitRelationshipCodes.Self)
                {
                    // 1. Pegawai Sendiri (SELF): NIK wajib cocok
                    if (string.IsNullOrWhiteSpace(patientNik) || string.IsNullOrWhiteSpace(targetNik) ||
                        !string.Equals(patientNik, targetNik, StringComparison.OrdinalIgnoreCase))
                    {
                        return InternalCardLinkResult.Reject(
                            StatusCodes.Status422UnprocessableEntity,
                            "PATIENT_IDENTITY_MISMATCH",
                            "Identitas pasien (NIK) tidak sesuai dengan NIK pegawai di HR.");
                    }
                }
                else
                {
                    // 2. Tanggungan Keluarga (SPOUSE / CHILD / PARENT / dll.)
                    if (!string.IsNullOrWhiteSpace(patientNik) && !string.IsNullOrWhiteSpace(targetNik))
                    {
                        if (!string.Equals(patientNik, targetNik, StringComparison.OrdinalIgnoreCase))
                        {
                            return InternalCardLinkResult.Reject(
                                StatusCodes.Status422UnprocessableEntity,
                                "PATIENT_IDENTITY_MISMATCH",
                                "Identitas pasien (NIK) tidak sesuai dengan NIK anggota keluarga di HR.");
                        }
                    }
                    else
                    {
                        // NIK belum lengkap di salah satu atau keduanya (misal anak/bayi baru lahir): validasi Nama Lengkap dan Tanggal Lahir
                        var nameMatches = IsNameMatch(patient.FullName, check.TargetPersonFullName);
                        var dobMatches = patient.BirthDate.HasValue && check.TargetPersonBirthDate.HasValue &&
                                         patient.BirthDate.Value.Date == check.TargetPersonBirthDate.Value.Date;

                        if (!nameMatches || !dobMatches)
                        {
                            return InternalCardLinkResult.Reject(
                                StatusCodes.Status422UnprocessableEntity,
                                "PATIENT_IDENTITY_MISMATCH",
                                "Identitas pasien (nama lengkap atau tanggal lahir) tidak sesuai dengan data anggota keluarga di HR.");
                        }
                    }
                }
            }

            return InternalCardLinkResult.Linked(
                check.WorkforceProfileId!.Value, check.OwnerType ?? WorkforceBenefitOwnerTypes.Employee, check.EmployeeId, check.DoctorId,
                check.FamilyMemberId, check.RelationshipCode!, check.StaffNumber ?? check.EmployeeNumber ?? string.Empty,
                check.StaffName ?? check.EmployeeName);
        }

        public Task<InternalCardLinkResult> ResolveCardLinkAsync(
            Guid companyGuarantorId, Guid? employeeId, Guid? familyMemberId, string? clientRelationship,
            CancellationToken cancellationToken) =>
            ResolveCardLinkAsync(companyGuarantorId, employeeId, familyMemberId, clientRelationship, null, cancellationToken);

        /// <summary>
        /// Dipanggil saat kunjungan didaftarkan memakai kartu dan saat penanggung kunjungan diganti ke kartu.
        /// Kartu atas penjamin biasa selalu lolos. Kartu internal harus bertaut dan layak pada tanggal pelayanan.
        /// </summary>
        public Task<CardServiceCheckResult> ValidateCardForServiceAsync(
            Guid companyGuarantorId, Guid? employeeId, Guid? familyMemberId, DateTime serviceDate,
            CancellationToken cancellationToken) =>
            ValidateCardForServiceAsync(companyGuarantorId, null, employeeId, familyMemberId, serviceDate, cancellationToken);

        /// <summary>
        /// Versi berbasis WorkforceProfileId (kartu baru memakai BenefitOwnerWorkforceProfileId; kartu lama masih
        /// membawa EmployeeId saja dan diterjemahkan ke Workforce oleh pemeriksaan kelayakan).
        /// </summary>
        public async Task<CardServiceCheckResult> ValidateCardForServiceAsync(
            Guid companyGuarantorId, Guid? workforceProfileId, Guid? employeeId, Guid? familyMemberId, DateTime serviceDate,
            CancellationToken cancellationToken)
        {
            if (!await IsInternalGuarantorAsync(companyGuarantorId, cancellationToken))
                return CardServiceCheckResult.Valid();

            var hasOwner = (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
                || (employeeId.HasValue && employeeId.Value != Guid.Empty);
            if (!hasOwner)
                return CardServiceCheckResult.Invalid(CardNotLinkedCode, CardNotLinkedMessage);

            var check = await TryCheckAsync(workforceProfileId, employeeId, familyMemberId, serviceDate, cancellationToken);
            return check.IsEligible
                ? CardServiceCheckResult.Valid()
                : CardServiceCheckResult.Invalid(check.ReasonCode, check.Message ?? string.Empty);
        }

        // ------------------------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------------------------

        private Task<bool> IsInternalGuarantorAsync(Guid companyGuarantorId, CancellationToken cancellationToken) =>
            companyGuarantorId == Guid.Empty
                ? Task.FromResult(false)
                : _dbContext.Set<MstCompanyGuarantor>().AsNoTracking()
                    .AnyAsync(x => x.Id == companyGuarantorId && !x.IsDelete && x.IsEmployeeBenefitGuarantor, cancellationToken);

        /// <summary>Gagal tertutup: galat membaca HR menjadi hasil HR_UNAVAILABLE, bukan kelayakan.</summary>
        private async Task<BenefitEligibilityResult> TryCheckAsync(
            Guid? workforceProfileId, Guid? employeeId, Guid? familyMemberId, DateTime serviceDate, CancellationToken cancellationToken)
        {
            try
            {
                return await _eligibilityService.CheckAsync(new BenefitEligibilityRequest
                {
                    WorkforceProfileId = workforceProfileId,
                    EmployeeId = employeeId ?? Guid.Empty,
                    FamilyMemberId = familyMemberId,
                    ServiceDate = serviceDate
                }, cancellationToken);
            }
            catch (Exception exception) when (IsHrFailure(exception))
            {
                await LogHrFailureAsync("CheckEligibility", exception);
                return new BenefitEligibilityResult
                {
                    ReasonCode = BenefitEligibilityReasonCodes.HrUnavailable,
                    Message = EmployeeBenefitEligibilityService.MessageFor(BenefitEligibilityReasonCodes.HrUnavailable)
                };
            }
        }

        private static bool IsHrFailure(Exception exception) =>
            exception is not (OperationCanceledException or KeyNotFoundException or ArgumentException);

        private async Task<EmployeeBenefitHrUnavailableException> HrUnavailableAsync(string action, Exception exception)
        {
            await LogHrFailureAsync(action, exception);
            return new EmployeeBenefitHrUnavailableException(
                EmployeeBenefitEligibilityService.MessageFor(BenefitEligibilityReasonCodes.HrUnavailable), exception);
        }

        private static string NormalizeNik(string? nik)
        {
            if (string.IsNullOrWhiteSpace(nik)) return string.Empty;
            return new string(nik.Where(char.IsLetterOrDigit).ToArray()).Trim();
        }

        private static bool IsNameMatch(string? name1, string? name2)
        {
            if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2)) return false;
            return string.Equals(name1.Trim(), name2.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        // QBE-LOG-001: hanya nama aksi dan galatnya; tanpa kata kunci, NIP, NIK, atau nama.
        private Task LogHrFailureAsync(string action, Exception exception) =>
            _loggerService.ErrorAsync(LogCategory, action, $"Data HR tidak dapat dibaca pada {action}.", exception);
    }

    /// <summary>Hasil ResolveCardLinkAsync: bukan penjamin internal, tertaut, atau ditolak.</summary>
    public sealed class InternalCardLinkResult
    {
        public bool IsInternal { get; private init; }
        public bool IsAccepted { get; private init; }
        public int StatusCode { get; private init; }
        public string? ReasonCode { get; private init; }
        public string? Message { get; private init; }

        /// <summary>Identitas pemilik benefit (utama). Terisi pada kartu internal yang diterima.</summary>
        public Guid? WorkforceProfileId { get; private init; }
        public string? OwnerType { get; private init; }

        /// <summary>Identitas peran; EmployeeId kosong untuk dokter tanpa data karyawan, dan DoctorId tidak pernah masuk ke EmployeeId.</summary>
        public Guid? EmployeeId { get; private init; }
        public Guid? DoctorId { get; private init; }
        public Guid? FamilyMemberId { get; private init; }
        public string? BenefitRelationship { get; private init; }
        public string? EmployeeNumber { get; private init; }
        public string? EmployeeName { get; private init; }

        public static InternalCardLinkResult NotInternal() => new() { IsAccepted = true };

        public static InternalCardLinkResult Linked(
            Guid workforceProfileId, string ownerType, Guid? employeeId, Guid? doctorId, Guid? familyMemberId,
            string relationship, string employeeNumber, string? employeeName) => new()
        {
            IsInternal = true,
            IsAccepted = true,
            WorkforceProfileId = workforceProfileId,
            OwnerType = ownerType,
            EmployeeId = employeeId,
            DoctorId = doctorId,
            FamilyMemberId = familyMemberId,
            BenefitRelationship = relationship,
            EmployeeNumber = employeeNumber,
            EmployeeName = employeeName
        };

        public static InternalCardLinkResult Reject(int statusCode, string reasonCode, string message) => new()
        {
            StatusCode = statusCode,
            ReasonCode = reasonCode,
            Message = message
        };
    }

    /// <summary>Hasil ValidateCardForServiceAsync.</summary>
    public sealed class CardServiceCheckResult
    {
        public bool IsValid { get; private init; }
        public string? ReasonCode { get; private init; }
        public string? Message { get; private init; }

        public static CardServiceCheckResult Valid() => new() { IsValid = true };

        public static CardServiceCheckResult Invalid(string reasonCode, string message) => new()
        {
            ReasonCode = reasonCode,
            Message = message
        };
    }

    /// <summary>Data HR tidak dapat dibaca; endpoint memetakannya ke 503 (FIN-API-1.10 R.3).</summary>
    public sealed class EmployeeBenefitHrUnavailableException(string message, Exception? innerException = null)
        : Exception(message, innerException);
}
