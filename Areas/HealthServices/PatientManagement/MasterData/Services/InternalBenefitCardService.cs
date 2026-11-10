using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    public sealed record InternalBenefitCompanyRow(
        Guid Id,
        string CompanyGuarantorName,
        bool IsActive,
        bool IsCancel,
        DateTime? ContractStartDate,
        DateTime? ContractEndDate);

    public sealed class CreateSelfBenefitCardArgs
    {
        public Guid PatientId { get; init; }
        public Guid CompanyGuarantorId { get; init; }
        public Guid BenefitOwnerWorkforceProfileId { get; init; }
        public Guid? EmployeeId { get; init; }
        public string? EmployeeNumber { get; init; }
        public string? EmployeeName { get; init; }
        public string? DepartmentName { get; init; }
        public string? PositionName { get; init; }
        public string? GradeLevel { get; init; }
        public string? BenefitPlanCode { get; init; }
        public string? BenefitPlanName { get; init; }
        public string? ClassName { get; init; }
        public string? LastEligibilityReferenceNumber { get; init; }
        public string? EligibilityNote { get; init; }
        public bool? ForcePrimary { get; init; }
        public Guid ActorUserId { get; init; }
    }

    public sealed class CreateInternalBenefitCardResult
    {
        public bool IsSuccess { get; private init; }
        public int StatusCode { get; private init; }
        public string? ReasonCode { get; private init; }
        public string? Message { get; private init; }
        public MstPatientCompanyGuarantor? Card { get; private init; }

        public static CreateInternalBenefitCardResult Success(MstPatientCompanyGuarantor card) => new()
        {
            IsSuccess = true,
            StatusCode = StatusCodes.Status200OK,
            Card = card
        };

        public static CreateInternalBenefitCardResult Fail(int statusCode, string? reasonCode, string message) => new()
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ReasonCode = reasonCode,
            Message = message
        };
    }

    /// <summary>
    /// FIN-DES-114 (BE-FIN-134): Ekstraksi layanan kartu penjamin internal pasien dari PatientCompanyGuarantorController.
    /// Menyediakan pencarian/penyediaan penjamin internal sistem SYS-RS-BENEFIT, pengecekan kartu aktif,
    /// dan pembuatan kartu internal SELF yang melewati validasi ResolveCardLinkAsync yang sama.
    /// </summary>
    public sealed class InternalBenefitCardService
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";

        private readonly ApplicationDbContext _dbContext;
        private readonly RegistrationEmployeeBenefitLookupService _employeeBenefitLookup;
        private readonly LoggerService _loggerService;

        public InternalBenefitCardService(
            ApplicationDbContext dbContext,
            RegistrationEmployeeBenefitLookupService employeeBenefitLookup,
            LoggerService loggerService)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _employeeBenefitLookup = employeeBenefitLookup ?? throw new ArgumentNullException(nameof(employeeBenefitLookup));
            _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
        }

        /// <summary>
        /// Indeks unik UX_MstCompanyGuarantor_EmployeeBenefit: paling banyak satu penjamin bertanda internal yang belum
        /// dihapus, sehingga penjamin yang nonaktif atau kontraknya lewat tetap menempati satu-satunya slot itu.
        /// </summary>
        public async Task<InternalBenefitCompanyRow?> FindInternalBenefitCompanyAsync(CancellationToken cancellationToken = default)
        {
            var rows = await _dbContext.Set<MstCompanyGuarantor>().AsNoTracking()
                .Where(x => x.IsEmployeeBenefitGuarantor && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.CompanyGuarantorName,
                    x.IsActive,
                    x.IsCancel,
                    x.ContractStartDate,
                    x.ContractEndDate
                })
                .ToListAsync(cancellationToken);

            return rows
                .OrderByDescending(x => x.IsActive && !x.IsCancel)
                .Select(x => new InternalBenefitCompanyRow(
                    x.Id, x.CompanyGuarantorName, x.IsActive, x.IsCancel, x.ContractStartDate, x.ContractEndDate))
                .FirstOrDefault();
        }

        /// <summary>
        /// Membuat penjamin internal sistem ("Benefit Karyawan RS") saat belum ada satu pun yang bertanda internal.
        /// Tanpa kontrak, tanpa surat jaminan, dan tanpa batasan grade: kelayakan dan plafon sepenuhnya dari HR.
        /// Dua permintaan bersamaan dijaga indeks unik; yang kalah memakai baris pemenang.
        /// </summary>
        public async Task<InternalBenefitCompanyRow> ProvisionInternalBenefitCompanyAsync(
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            const string systemCode = "SYS-RS-BENEFIT";

            var codeTaken = await _dbContext.Set<MstCompanyGuarantor>().AsNoTracking()
                .AnyAsync(x => x.CompanyGuarantorCode == systemCode, cancellationToken);

            var entity = new MstCompanyGuarantor
            {
                Id = Guid.NewGuid(),
                CompanyGuarantorCode = codeTaken ? $"{systemCode}-{DateTime.UtcNow:yyyyMMddHHmmss}" : systemCode,
                CompanyGuarantorName = "Benefit Karyawan RS",
                GuarantorType = "Other",
                BillingMethod = "Invoice",
                IsUsingHospitalTariff = true,
                IsUsingCompanyTariffBook = false,
                IsNeedGuaranteeLetter = false,
                IsNeedEmployeeVerification = false,
                IsNeedApprovalForProcedure = false,
                IsNeedApprovalForDrug = false,
                IsCoverageLimitedByEmployeeGrade = false,
                IsAllowExcessPaymentByPatient = true,
                IsEmployeeBenefitGuarantor = true,
                Description = "Dibuat otomatis oleh sistem untuk Benefit Karyawan. Bukan asuransi dan bukan perusahaan eksternal.",
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstCompanyGuarantor>().Add(entity);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                _dbContext.Entry(entity).State = EntityState.Detached;
                var winner = await FindInternalBenefitCompanyAsync(cancellationToken);
                if (winner is null) throw;
                return winner;
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InternalBenefitCardService.ProvisionInternalBenefitCompany",
                "Penjamin internal Benefit Karyawan dibuat otomatis oleh sistem.",
                new { entity.Id, entity.CompanyGuarantorCode });

            return new InternalBenefitCompanyRow(entity.Id, entity.CompanyGuarantorName, true, false, null, null);
        }

        /// <summary>
        /// Mengambil penjamin internal sistem yang sudah ada atau membuatnya bila belum tersedia.
        /// </summary>
        public async Task<InternalBenefitCompanyRow> FindOrProvisionInternalGuarantorAsync(
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var existing = await FindInternalBenefitCompanyAsync(cancellationToken);
            if (existing != null)
            {
                return existing;
            }

            return await ProvisionInternalBenefitCompanyAsync(actorUserId, cancellationToken);
        }

        /// <summary>
        /// Mengambil Id kartu penjamin internal aktif milik pasien (diprioritaskan IsPrimary lalu waktu pembuatan terbaru).
        /// Digunakan oleh EnsureInternalBenefitCard agar kartu aktif yang ada dipakai ulang tanpa membuat duplikat.
        /// </summary>
        public async Task<Guid?> FindActiveInternalCardIdAsync(
            Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking()
                .Where(x => x.PatientId == patientId
                    && !x.IsDelete && !x.IsCancel && x.IsActive
                    && x.CompanyGuarantor != null
                    && x.CompanyGuarantor.IsEmployeeBenefitGuarantor
                    && x.CompanyGuarantor.IsActive && !x.CompanyGuarantor.IsDelete)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Mengambil seluruh kartu penjamin internal aktif milik pasien.
        /// Digunakan oleh sinkronisasi otomatis (BE-FIN-135) untuk mengevaluasi status kartu eksisting (FIN-DEC-251).
        /// </summary>
        public async Task<List<MstPatientCompanyGuarantor>> FindActiveInternalCardsAsync(
            Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking()
                .Include(x => x.CompanyGuarantor)
                .Where(x => x.PatientId == patientId
                    && !x.IsDelete && !x.IsCancel && x.IsActive
                    && x.CompanyGuarantor != null
                    && x.CompanyGuarantor.IsEmployeeBenefitGuarantor
                    && x.CompanyGuarantor.IsActive && !x.CompanyGuarantor.IsDelete)
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Membuat kartu penjamin internal jenis SELF dengan melewati ResolveCardLinkAsync yang sama,
        /// menjaga kesesuaian IsPrimary (hanya true bila pasien belum punya primary aktif), serta mencatat audit trail.
        /// </summary>
        public async Task<CreateInternalBenefitCardResult> CreateSelfCardAsync(
            CreateSelfBenefitCardArgs args, CancellationToken cancellationToken = default)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            // Validasi tautan kartu lewat ResolveCardLinkAsync (kelayakan, NIK FIN-DEC-228, hubungan SELF)
            var cardLink = await _employeeBenefitLookup.ResolveCardLinkAsync(
                args.CompanyGuarantorId,
                args.BenefitOwnerWorkforceProfileId,
                args.EmployeeId,
                familyMemberId: null,
                clientRelationship: null, // ditentukan oleh sistem
                patientId: args.PatientId,
                cancellationToken: cancellationToken);

            if (!cardLink.IsAccepted)
            {
                return CreateInternalBenefitCardResult.Fail(
                    cardLink.StatusCode != 0 ? cardLink.StatusCode : StatusCodes.Status422UnprocessableEntity,
                    cardLink.ReasonCode,
                    cardLink.Message ?? "Data pegawai HR pada kartu tidak valid.");
            }

            var now = DateTime.UtcNow;
            var employeeNumber = !string.IsNullOrWhiteSpace(cardLink.EmployeeNumber)
                ? cardLink.EmployeeNumber
                : args.EmployeeNumber?.Trim() ?? string.Empty;

            var employeeName = !string.IsNullOrWhiteSpace(cardLink.EmployeeName)
                ? cardLink.EmployeeName
                : args.EmployeeName?.Trim();

            // IsPrimary = true hanya bila pasien belum punya kartu penjamin aktif lain yang IsPrimary
            var hasActivePrimary = await _dbContext.Set<MstPatientCompanyGuarantor>().AsNoTracking()
                .AnyAsync(x => x.PatientId == args.PatientId && x.IsPrimary && x.IsActive && !x.IsDelete, cancellationToken);

            var isPrimary = args.ForcePrimary ?? !hasActivePrimary;

            // Buka transaksi bila belum ada transaksi aktif di context pemanggil
            var hasExistingTransaction = _dbContext.Database.CurrentTransaction != null;
            await using var transaction = hasExistingTransaction
                ? null
                : await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (isPrimary)
                {
                    var otherPrimaries = await _dbContext.Set<MstPatientCompanyGuarantor>()
                        .Where(x => x.PatientId == args.PatientId && x.IsPrimary && !x.IsDelete)
                        .ToListAsync(cancellationToken);

                    foreach (var card in otherPrimaries)
                    {
                        card.IsPrimary = false;
                        card.UpdateDateTime = now;
                        card.UpdateBy = args.ActorUserId;
                    }
                }

                static string? Cut(string? val, int max) =>
                    string.IsNullOrWhiteSpace(val) ? null : (val.Length <= max ? val.Trim() : val[..max].Trim());

                var entity = new MstPatientCompanyGuarantor
                {
                    Id = Guid.NewGuid(),
                    PatientId = args.PatientId,
                    CompanyGuarantorId = args.CompanyGuarantorId,
                    EmployeeNumber = Cut(employeeNumber, 100) ?? string.Empty,
                    EmployeeName = Cut(employeeName, 200),
                    DepartmentName = Cut(args.DepartmentName, 100),
                    PositionName = Cut(args.PositionName, 100),
                    GradeLevel = Cut(args.GradeLevel, 100),
                    BenefitPlanCode = Cut(args.BenefitPlanCode, 100),
                    BenefitPlanName = Cut(args.BenefitPlanName, 150),
                    ClassName = Cut(args.ClassName, 100),
                    EffectiveStartDate = null,
                    EffectiveEndDate = null,
                    IsPrimary = isPrimary,
                    IsEligible = true,
                    LastEligibilityCheckAt = now,
                    LastEligibilityReferenceNumber = Cut(args.LastEligibilityReferenceNumber, 100),
                    EligibilityNote = Cut(args.EligibilityNote, 500) ?? "Dibuat otomatis dari benefit aktif di HR saat registrasi.",
                    AnnualLimitAmount = null,
                    RemainingLimitAmount = null,
                    CoPaymentPercent = null,
                    CoPaymentAmount = null,
                    IsNeedGuaranteeLetter = false,
                    IsNeedEmployeeVerification = false,
                    IsAllowExcessPaymentByPatient = true,
                    GuaranteeDocumentPath = null,
                    CardImagePath = null,
                    Notes = null,
                    EmployeeId = cardLink.EmployeeId,
                    BenefitOwnerWorkforceProfileId = cardLink.WorkforceProfileId ?? args.BenefitOwnerWorkforceProfileId,
                    BenefitOwnerType = cardLink.OwnerType,
                    FamilyMemberId = cardLink.FamilyMemberId,
                    BenefitRelationship = cardLink.BenefitRelationship ?? BenefitRelationshipCodes.Self,
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = args.ActorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<MstPatientCompanyGuarantor>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction != null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                await _loggerService.InfoAsync(
                    LogCategory,
                    "InternalBenefitCardService.CreateSelfCard",
                    "Kartu penjamin internal benefit karyawan berhasil dibuat.",
                    new { entity.Id, entity.PatientId, entity.CompanyGuarantorId, entity.EmployeeNumber, entity.BenefitOwnerWorkforceProfileId });

                return CreateInternalBenefitCardResult.Success(entity);
            }
            catch (Exception ex)
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "InternalBenefitCardService.CreateSelfCard",
                    "Gagal membuat kartu penjamin internal benefit karyawan.",
                    ex);

                throw;
            }
        }
    }
}
