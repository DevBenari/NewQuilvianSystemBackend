using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;

public enum NewbornRegistrationStatus
{
    Success,
    Invalid,
    NotFound,
    Conflict,
    BusinessRuleRejected
}

public sealed record NewbornRegistrationResult(
    NewbornRegistrationStatus Status,
    string Message,
    NewbornPatientCreateResponse? Data = null);

/// <summary>
/// Membuat identitas pasien bayi dari satu kasus persalinan. Controller hanya meneruskan
/// request; seluruh validasi, idempotency, alokasi urutan, dan transaksi dimiliki service ini.
/// </summary>
public sealed class NewbornPatientRegistrationService
{
    private const string PatientCodeSequenceKey = "PATIENT_CODE";
    private const string MedicalRecordSequenceKey = "PATIENT_MEDICAL_RECORD";
    private readonly ApplicationDbContext _dbContext;
    private readonly NumberSeriesAllocator _numberSeriesAllocator;

    public NewbornPatientRegistrationService(
        ApplicationDbContext dbContext,
        NumberSeriesAllocator numberSeriesAllocator)
    {
        _dbContext = dbContext;
        _numberSeriesAllocator = numberSeriesAllocator;
    }

    public async Task<NewbornRegistrationResult> RegisterAsync(
        CreateNewbornPatientRequest request,
        string? idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            return Invalid("Data bayi belum dikirim.");
        if (actorUserId == Guid.Empty)
            return Invalid("Petugas pendaftaran tidak dikenali.");
        if (request.MotherPatientId == Guid.Empty)
            return Invalid("Ibu bayi wajib dipilih dari kasus persalinan.");
        if (request.DeliveryCaseId == Guid.Empty)
            return Invalid("Kasus persalinan wajib dipilih.");
        if (string.IsNullOrWhiteSpace(idempotencyKey) || idempotencyKey.Trim().Length is < 8 or > 200)
            return Invalid("Idempotency-Key wajib diisi antara 8 sampai 200 karakter.");
        if (!request.BirthDate.HasValue || request.BirthDate.Value.Date > DateTime.UtcNow.Date)
            return Invalid("Tanggal lahir bayi wajib valid dan tidak boleh di masa depan.");
        if (!request.BirthTime.HasValue)
            return Invalid("Jam lahir bayi wajib diisi.");
        if (!request.Gender.HasValue || request.Gender is not (Gender.Male or Gender.Female))
            return Invalid("Jenis kelamin bayi wajib dipilih.");
        if (request.BirthWeightGram is < 0 || request.BirthLengthCm is < 0)
            return Invalid("Berat dan panjang lahir tidak boleh bernilai negatif.");

        var normalizedKey = idempotencyKey.Trim();
        var keyHash = Sha256(normalizedKey);
        var fingerprint = BuildFingerprint(request);

        var existing = await FindByIdempotencyAsync(keyHash, cancellationToken);
        if (existing != null)
            return ReplayOrConflict(existing, fingerprint);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await AcquireLockAsync($"PAT_NEWBORN_KEY_{keyHash}", cancellationToken);
            await AcquireLockAsync($"PAT_NEWBORN_CASE_{request.DeliveryCaseId:N}", cancellationToken);

            existing = await FindByIdempotencyAsync(keyHash, cancellationToken);
            if (existing != null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ReplayOrConflict(existing, fingerprint);
            }

            var mother = await _dbContext.MstPatients
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.MotherPatientId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (mother == null)
                return await RollbackAsync(transaction, NewbornRegistrationStatus.NotFound,
                    "Ibu bayi tidak ditemukan atau tidak aktif.", cancellationToken);
            if (mother.Gender != Gender.Female)
                return await RollbackAsync(transaction, NewbornRegistrationStatus.BusinessRuleRejected,
                    "Pasien pada kasus persalinan tidak tercatat sebagai ibu.", cancellationToken);

            var deliveryCase = await _dbContext.Set<OprCase>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.DeliveryCaseId &&
                    !x.IsDelete,
                    cancellationToken);

            if (deliveryCase == null)
                return await RollbackAsync(transaction, NewbornRegistrationStatus.NotFound,
                    "Kasus persalinan tidak ditemukan.", cancellationToken);
            if (deliveryCase.PatientId != mother.Id)
                return await RollbackAsync(transaction, NewbornRegistrationStatus.BusinessRuleRejected,
                    "Kasus persalinan tidak sesuai dengan ibu yang dipilih.", cancellationToken);
            if (deliveryCase.SurgicalServiceType != OprSurgicalServiceType.Obstetric ||
                deliveryCase.Status is not (OprCaseStatus.InProgress or OprCaseStatus.Completed))
                return await RollbackAsync(transaction, NewbornRegistrationStatus.BusinessRuleRejected,
                    "Kasus harus berupa persalinan obstetri yang sedang berlangsung atau sudah selesai.", cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.IdentityNumber))
            {
                var identityExists = await _dbContext.MstPatients
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.IdentityNumber == request.IdentityNumber.Trim(),
                        cancellationToken);
                if (identityExists)
                    return await RollbackAsync(transaction, NewbornRegistrationStatus.Conflict,
                        "NIK bayi sudah digunakan oleh pasien lain.", cancellationToken);
            }

            var sequence = await _dbContext.PatNewbornBirthSequences
                .SingleOrDefaultAsync(x => x.DeliveryCaseId == deliveryCase.Id, cancellationToken);

            var now = DateTime.UtcNow;
            var birthOrder = 1;
            if (sequence == null)
            {
                sequence = new PatNewbornBirthSequence
                {
                    Id = Guid.NewGuid(),
                    DeliveryCaseId = deliveryCase.Id,
                    LastAllocatedBirthOrder = 1,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                _dbContext.PatNewbornBirthSequences.Add(sequence);
            }
            else
            {
                birthOrder = checked(sequence.LastAllocatedBirthOrder + 1);
                sequence.LastAllocatedBirthOrder = birthOrder;
                sequence.UpdateDateTime = now;
                sequence.UpdateBy = actorUserId;
            }

            var patientCode = await AllocatePatientCodeAsync(actorUserId, cancellationToken);
            var medicalRecordNumber = await AllocateMedicalRecordNumberAsync(actorUserId, cancellationToken);
            var motherName = NormalizeMotherName(mother.FullName);

            var patient = new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = patientCode,
                MedicalRecordNumber = medicalRecordNumber,
                PatientType = PatientType.Newborn,
                PatientStatus = PatientStatus.Active,
                RegistrationSource = PatientRegistrationSource.InpatientAdmission,
                FullName = $"By. Ny. {motherName}",
                BirthPlace = Normalize(request.BirthPlace),
                BirthDate = request.BirthDate.Value.Date,
                BirthTime = request.BirthTime,
                Gender = request.Gender,
                Religion = Religion.Unknown,
                MaritalStatus = MaritalStatus.Unknown,
                BloodType = BloodType.Unknown,
                IdentityType = string.IsNullOrWhiteSpace(request.IdentityNumber) ? null : "NIK",
                IdentityNumber = Normalize(request.IdentityNumber),
                Address = mother.Address,
                CountryId = mother.CountryId,
                Citizenship = mother.Citizenship,
                NationalityCountryId = mother.NationalityCountryId,
                ProvinceId = mother.ProvinceId,
                CityId = mother.CityId,
                DistrictId = mother.DistrictId,
                PostalCodeId = mother.PostalCodeId,
                IsMember = false,
                IsNewborn = true,
                MotherPatientId = mother.Id,
                DeliveryCaseId = deliveryCase.Id,
                BirthOrder = birthOrder,
                BirthWeightGram = request.BirthWeightGram,
                BirthLengthCm = request.BirthLengthCm,
                DeliveryMethod = Normalize(request.DeliveryMethod),
                Notes = Normalize(request.Notes),
                NewbornRegistrationKeyHash = keyHash,
                NewbornRegistrationFingerprint = fingerprint,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstPatients.Add(patient);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new NewbornRegistrationResult(
                NewbornRegistrationStatus.Success,
                "Pasien bayi berhasil didaftarkan.",
                Map(patient, wasExisting: false));
        }
        catch (OverflowException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new NewbornRegistrationResult(
                NewbornRegistrationStatus.Conflict,
                "Urutan kelahiran pada kasus persalinan ini sudah mencapai batas sistem.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new NewbornRegistrationResult(
                NewbornRegistrationStatus.Conflict,
                "Registrasi bayi berbenturan dengan permintaan lain. Muat ulang lalu periksa hasil registrasi.");
        }
    }

    private async Task AcquireLockAsync(string lockKey, CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsNpgsql()) return;
        await _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));",
            [lockKey],
            cancellationToken);
    }

    private async Task<MstPatient?> FindByIdempotencyAsync(string keyHash, CancellationToken cancellationToken)
        => await _dbContext.MstPatients
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.NewbornRegistrationKeyHash == keyHash &&
                !x.IsDelete,
                cancellationToken);

    private static NewbornRegistrationResult ReplayOrConflict(MstPatient patient, string fingerprint)
    {
        if (!string.Equals(patient.NewbornRegistrationFingerprint, fingerprint, StringComparison.Ordinal))
            return new NewbornRegistrationResult(
                NewbornRegistrationStatus.Conflict,
                "Idempotency-Key sudah dipakai untuk data bayi yang berbeda.");

        return new NewbornRegistrationResult(
            NewbornRegistrationStatus.Success,
            "Registrasi bayi yang sama sudah tersimpan; hasil sebelumnya dikembalikan.",
            Map(patient, wasExisting: true));
    }

    private async Task<string> AllocatePatientCodeAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var minimum = await GetLegacyMaximumAsync(
            PatientCodeSequenceKey,
            _dbContext.MstPatients.IgnoreQueryFilters().Select(x => x.PatientCode),
            cancellationToken);
        return await _numberSeriesAllocator.AllocateAsync(
            new NumberAllocationRequest(
                PatientCodeSequenceKey,
                "PAT-RSMMC",
                NumberSeriesResetPolicies.Never,
                5,
                actorUserId,
                DateTimeOffset.UtcNow,
                minimum),
            cancellationToken);
    }

    private async Task<string> AllocateMedicalRecordNumberAsync(Guid actorUserId, CancellationToken cancellationToken)
    {
        var minimum = await GetLegacyMaximumAsync(
            MedicalRecordSequenceKey,
            _dbContext.MstPatients.IgnoreQueryFilters().Select(x => x.MedicalRecordNumber),
            cancellationToken);
        var allocated = await _numberSeriesAllocator.AllocateAsync(
            new NumberAllocationRequest(
                MedicalRecordSequenceKey,
                "MR",
                NumberSeriesResetPolicies.Never,
                8,
                actorUserId,
                DateTimeOffset.UtcNow,
                minimum),
            cancellationToken);
        var digits = new string(allocated.Where(char.IsDigit).ToArray()).PadLeft(8, '0');
        return string.Join('-', Enumerable.Range(0, 4).Select(index => digits.Substring(index * 2, 2)));
    }

    private async Task<long> GetLegacyMaximumAsync(
        string sequenceKey,
        IQueryable<string> values,
        CancellationToken cancellationToken)
    {
        var hasSeries = await _dbContext.NumNumberSeries
            .AsNoTracking()
            .AnyAsync(x => x.SequenceKey == sequenceKey && x.ScopeKey == "GLOBAL", cancellationToken);
        if (hasSeries) return 0;

        var existing = await values.ToListAsync(cancellationToken);
        return existing
            .Select(value => new string((value ?? string.Empty).Where(char.IsDigit).ToArray()))
            .Select(value => long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();
    }

    private static string BuildFingerprint(CreateNewbornPatientRequest request)
    {
        var canonical = string.Join('|',
            request.MotherPatientId.ToString("N"),
            request.DeliveryCaseId.ToString("N"),
            request.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            request.BirthTime?.ToString("c", CultureInfo.InvariantCulture),
            ((int?)request.Gender)?.ToString(CultureInfo.InvariantCulture),
            Normalize(request.BirthPlace)?.ToUpperInvariant(),
            Normalize(request.IdentityNumber),
            request.BirthWeightGram?.ToString(CultureInfo.InvariantCulture),
            request.BirthLengthCm?.ToString(CultureInfo.InvariantCulture),
            Normalize(request.DeliveryMethod)?.ToUpperInvariant());
        return Sha256(canonical);
    }

    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    private static string NormalizeMotherName(string value)
    {
        var normalized = value.Trim();
        foreach (var prefix in new[] { "Ny. ", "Ny ", "Nyonya ", "Ibu " })
        {
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return normalized[prefix.Length..].Trim();
        }
        return normalized;
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static NewbornPatientCreateResponse Map(MstPatient patient, bool wasExisting)
        => new()
        {
            Id = patient.Id,
            PatientCode = patient.PatientCode,
            MedicalRecordNumber = patient.MedicalRecordNumber,
            FullName = patient.FullName,
            PhotoPath = patient.PhotoPath,
            QrCodePath = patient.QrCodePath,
            PatientType = patient.PatientType,
            PatientTypeName = patient.PatientType.ToString(),
            PatientStatus = patient.PatientStatus,
            PatientStatusName = patient.PatientStatus.ToString(),
            IsNewborn = patient.IsNewborn,
            IsMember = patient.IsMember,
            IsActive = patient.IsActive,
            MotherPatientId = patient.MotherPatientId!.Value,
            DeliveryCaseId = patient.DeliveryCaseId!.Value,
            BirthOrder = patient.BirthOrder!.Value,
            BirthDate = patient.BirthDate!.Value,
            BirthTime = patient.BirthTime!.Value,
            Gender = patient.Gender!.Value,
            WasExisting = wasExisting
        };

    private static NewbornRegistrationResult Invalid(string message)
        => new(NewbornRegistrationStatus.Invalid, message);

    private static async Task<NewbornRegistrationResult> RollbackAsync(
        Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
        NewbornRegistrationStatus status,
        string message,
        CancellationToken cancellationToken)
    {
        await transaction.RollbackAsync(cancellationToken);
        return new NewbornRegistrationResult(status, message);
    }
}
