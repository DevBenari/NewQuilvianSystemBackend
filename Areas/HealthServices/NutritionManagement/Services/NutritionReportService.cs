using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>
/// Laporan operasional modul Gizi: pelayanan, diet pasien, dan kebutuhan nutrisi.
/// </summary>
/// <remarks>
/// <para>
/// Seluruh query hanya membaca — <c>AsNoTracking</c>, berpaginasi, dan tidak menulis audit log
/// tersendiri karena tidak mengubah data. Pola ini mengikuti laporan modul Operasi.
/// </para>
/// <para>
/// Laporan hanya menyusun ulang data yang sudah ada. Tidak ada angka yang dihitung dengan
/// aturan klinis baru, dan tidak ada kolom yang diarang: setiap kolom laporan berasal dari
/// kolom yang memang tersimpan.
/// </para>
/// </remarks>
public sealed class NutritionReportService(ApplicationDbContext dbContext)
{
    // ===================================================== A. Pelayanan Gizi

    public async Task<PagedResult<GziServiceReportRow>> GetServicesAsync(GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.GziNutritionCareRecords.AsNoTracking().Where(x => !x.IsDelete);

        if (request.From.HasValue) query = query.Where(x => x.VisitAt >= request.From.Value.ToUniversalTime());
        if (request.To.HasValue) query = query.Where(x => x.VisitAt <= ToInclusive(request.To.Value));
        if (request.WorkforceId.HasValue) query = query.Where(x => x.RecordedByWorkforceId == request.WorkforceId);
        if (request.PatientId.HasValue)
            query = query.Where(x => x.NutritionOrder != null && x.NutritionOrder.PatientId == request.PatientId);
        if (request.OrderStatus.HasValue)
            query = query.Where(x => x.NutritionOrder != null && x.NutritionOrder.Status == request.OrderStatus);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.NutritionOrder != null && x.NutritionOrder.OrderNumber.ToLower().Contains(search)) ||
                (x.NutritionOrder != null && x.NutritionOrder.Patient != null &&
                 x.NutritionOrder.Patient.FullName.ToLower().Contains(search)) ||
                (x.NutritionOrder != null && x.NutritionOrder.Patient != null &&
                 x.NutritionOrder.Patient.MedicalRecordNumber.ToLower().Contains(search)));
        }

        var totalData = await query.CountAsync(cancellationToken);

        // Daftar diagnosis diambil sebagai daftar, lalu dirangkai di memori. `string.Join`
        // tidak dapat diterjemahkan ke SQL, dan memaksakannya membuat laporan gagal saat
        // dijalankan walaupun kodenya lolos kompilasi.
        var raw = await query
            .OrderByDescending(x => x.VisitAt).ThenByDescending(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new
            {
                x.Id,
                x.NutritionOrderId,
                OrderNumber = x.NutritionOrder != null ? x.NutritionOrder.OrderNumber : string.Empty,
                PatientName = x.NutritionOrder != null && x.NutritionOrder.Patient != null
                    ? x.NutritionOrder.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.NutritionOrder != null && x.NutritionOrder.Patient != null
                    ? x.NutritionOrder.Patient.MedicalRecordNumber : string.Empty,
                x.VisitSequence,
                x.VisitAt,
                x.RecordType,
                RecordedByName = x.RecordedByWorkforce != null ? x.RecordedByWorkforce.DisplayName : string.Empty,
                Diagnoses = x.Diagnoses
                    .Where(d => !d.IsDelete && d.NutritionDiagnosis != null)
                    .OrderByDescending(d => d.IsPrimary).ThenBy(d => d.SortOrder)
                    .Select(d => d.NutritionDiagnosis!.DiagnosisCode + " " + d.NutritionDiagnosis.DiagnosisName)
                    .ToList(),
                DietTypeName = x.PatientDiet != null && x.PatientDiet.DietType != null
                    ? x.PatientDiet.DietType.DietTypeName : string.Empty,
                OrderStatus = x.NutritionOrder != null ? x.NutritionOrder.Status : GziOrderStatus.Requested,
                x.IntakePercent
            })
            .ToListAsync(cancellationToken);

        var rows = raw.Select(x => new GziServiceReportRow
        {
            CareRecordId = x.Id,
            NutritionOrderId = x.NutritionOrderId,
            OrderNumber = x.OrderNumber,
            PatientName = x.PatientName,
            MedicalRecordNumber = x.MedicalRecordNumber,
            VisitSequence = x.VisitSequence,
            VisitAt = x.VisitAt,
            RecordType = x.RecordType,
            RecordedByName = x.RecordedByName,
            DiagnosisSummary = string.Join(", ", x.Diagnoses),
            DietTypeName = x.DietTypeName,
            OrderStatus = x.OrderStatus,
            IntakePercent = x.IntakePercent
        }).ToList();

        return Page(request, totalData, rows);
    }

    // ======================================================= B. Diet Pasien

    public async Task<PagedResult<GziDietReportRow>> GetDietsAsync(GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.GziPatientDiets.AsNoTracking().Where(x => !x.IsDelete);

        if (request.From.HasValue) query = query.Where(x => x.StartAt >= request.From.Value.ToUniversalTime());
        if (request.To.HasValue) query = query.Where(x => x.StartAt <= ToInclusive(request.To.Value));
        if (request.PatientId.HasValue) query = query.Where(x => x.PatientId == request.PatientId);
        if (request.DietTypeId.HasValue) query = query.Where(x => x.DietTypeId == request.DietTypeId);
        if (request.DietStatus.HasValue) query = query.Where(x => x.Status == request.DietStatus);
        if (request.WorkforceId.HasValue) query = query.Where(x => x.PrescribedByWorkforceId == request.WorkforceId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(search)) ||
                (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(search)) ||
                (x.Encounter != null && x.Encounter.EncounterNumber.ToLower().Contains(search)));
        }

        var totalData = await query.CountAsync(cancellationToken);

        var rows = await query
            .OrderByDescending(x => x.StartAt).ThenByDescending(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new GziDietReportRow
            {
                PatientDietId = x.Id,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                OrderNumber = x.NutritionOrder != null ? x.NutritionOrder.OrderNumber : string.Empty,
                DietTypeName = x.DietType != null ? x.DietType.DietTypeName : string.Empty,
                FoodFormName = x.FoodForm != null ? x.FoodForm.FoodFormName : string.Empty,
                EnergyRequirementKcal = x.EnergyRequirementKcal,
                RequirementRevisionNumber = x.NutritionRequirement != null
                    ? x.NutritionRequirement.RevisionNumber : (int?)null,
                Status = x.Status,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                PrescribedByName = x.PrescribedByWorkforce != null ? x.PrescribedByWorkforce.DisplayName : string.Empty,
                ChangeReason = x.ChangeReason
            })
            .ToListAsync(cancellationToken);

        return Page(request, totalData, rows);
    }

    // ================================================= C. Kebutuhan Nutrisi

    public async Task<PagedResult<GziRequirementReportRow>> GetRequirementsAsync(GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.GziNutritionRequirements.AsNoTracking().Where(x => !x.IsDelete);

        if (request.From.HasValue) query = query.Where(x => x.EffectiveFrom >= request.From.Value.ToUniversalTime());
        if (request.To.HasValue) query = query.Where(x => x.EffectiveFrom <= ToInclusive(request.To.Value));
        if (request.WorkforceId.HasValue) query = query.Where(x => x.DeterminedByWorkforceId == request.WorkforceId);
        if (request.PatientId.HasValue)
            query = query.Where(x => x.NutritionOrder != null && x.NutritionOrder.PatientId == request.PatientId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x =>
                (x.NutritionOrder != null && x.NutritionOrder.OrderNumber.ToLower().Contains(search)) ||
                (x.NutritionOrder != null && x.NutritionOrder.Patient != null &&
                 x.NutritionOrder.Patient.FullName.ToLower().Contains(search)) ||
                (x.NutritionOrder != null && x.NutritionOrder.Patient != null &&
                 x.NutritionOrder.Patient.MedicalRecordNumber.ToLower().Contains(search)));
        }

        var totalData = await query.CountAsync(cancellationToken);

        // Nilai per parameter diambil lewat kode parameter, bukan lewat urutan kolom. Urutan
        // dapat berubah ketika admin menambah parameter; kodenya tidak.
        var rows = await query
            .OrderByDescending(x => x.EffectiveFrom).ThenByDescending(x => x.RevisionNumber)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new GziRequirementReportRow
            {
                RequirementId = x.Id,
                PatientName = x.NutritionOrder != null && x.NutritionOrder.Patient != null
                    ? x.NutritionOrder.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.NutritionOrder != null && x.NutritionOrder.Patient != null
                    ? x.NutritionOrder.Patient.MedicalRecordNumber : string.Empty,
                EncounterNumber = x.NutritionOrder != null && x.NutritionOrder.Encounter != null
                    ? x.NutritionOrder.Encounter.EncounterNumber : string.Empty,
                OrderNumber = x.NutritionOrder != null ? x.NutritionOrder.OrderNumber : string.Empty,
                RevisionNumber = x.RevisionNumber,
                IsCurrent = x.IsCurrent,
                EffectiveFrom = x.EffectiveFrom,
                EnergyKcal = x.Items.Where(i => !i.IsDelete && i.NutritionParameter != null && i.NutritionParameter.ParameterCode == "ENERGY").Select(i => (decimal?)i.FinalValue).FirstOrDefault(),
                ProteinGram = x.Items.Where(i => !i.IsDelete && i.NutritionParameter != null && i.NutritionParameter.ParameterCode == "PROTEIN").Select(i => (decimal?)i.FinalValue).FirstOrDefault(),
                FatGram = x.Items.Where(i => !i.IsDelete && i.NutritionParameter != null && i.NutritionParameter.ParameterCode == "FAT").Select(i => (decimal?)i.FinalValue).FirstOrDefault(),
                CarbohydrateGram = x.Items.Where(i => !i.IsDelete && i.NutritionParameter != null && i.NutritionParameter.ParameterCode == "CARBOHYDRATE").Select(i => (decimal?)i.FinalValue).FirstOrDefault(),
                FluidMl = x.Items.Where(i => !i.IsDelete && i.NutritionParameter != null && i.NutritionParameter.ParameterCode == "FLUID").Select(i => (decimal?)i.FinalValue).FirstOrDefault(),
                DeterminedByName = x.DeterminedByWorkforce != null
                    ? x.DeterminedByWorkforce.DisplayName : string.Empty,
                ChangeReason = x.ChangeReason
            })
            .ToListAsync(cancellationToken);

        return Page(request, totalData, rows);
    }

    // ============================================================ D. Ringkasan

    public async Task<GziReportSummary> GetSummaryAsync(GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var from = request.From?.ToUniversalTime();
        var to = request.To.HasValue ? ToInclusive(request.To.Value) : (DateTime?)null;

        var orders = dbContext.GziNutritionOrders.AsNoTracking().Where(x => !x.IsDelete);
        if (from.HasValue) orders = orders.Where(x => x.RequestedAt >= from.Value);
        if (to.HasValue) orders = orders.Where(x => x.RequestedAt <= to.Value);
        if (request.PatientId.HasValue) orders = orders.Where(x => x.PatientId == request.PatientId);

        var records = dbContext.GziNutritionCareRecords.AsNoTracking().Where(x => !x.IsDelete);
        if (from.HasValue) records = records.Where(x => x.VisitAt >= from.Value);
        if (to.HasValue) records = records.Where(x => x.VisitAt <= to.Value);

        var diets = dbContext.GziPatientDiets.AsNoTracking().Where(x => !x.IsDelete);
        if (from.HasValue) diets = diets.Where(x => x.StartAt >= from.Value);
        if (to.HasValue) diets = diets.Where(x => x.StartAt <= to.Value);

        var requirements = dbContext.GziNutritionRequirements.AsNoTracking().Where(x => !x.IsDelete);
        if (from.HasValue) requirements = requirements.Where(x => x.EffectiveFrom >= from.Value);
        if (to.HasValue) requirements = requirements.Where(x => x.EffectiveFrom <= to.Value);

        var energyValues = await dbContext.GziNutritionRequirementItems.AsNoTracking()
            .Where(i => !i.IsDelete &&
                        i.NutritionParameter != null && i.NutritionParameter.ParameterCode == "ENERGY" &&
                        i.NutritionRequirement != null && !i.NutritionRequirement.IsDelete &&
                        i.NutritionRequirement.IsCurrent &&
                        (!from.HasValue || i.NutritionRequirement.EffectiveFrom >= from.Value) &&
                        (!to.HasValue || i.NutritionRequirement.EffectiveFrom <= to.Value))
            .Select(i => i.FinalValue)
            .ToListAsync(cancellationToken);

        return new GziReportSummary
        {
            From = request.From,
            To = request.To,
            TotalOrders = await orders.CountAsync(cancellationToken),
            OpenOrders = await orders.CountAsync(x =>
                x.Status == GziOrderStatus.Requested || x.Status == GziOrderStatus.InProgress, cancellationToken),
            ClosedOrders = await orders.CountAsync(x => x.Status == GziOrderStatus.Closed, cancellationToken),
            TotalCareRecords = await records.CountAsync(cancellationToken),
            TotalPatients = await orders.Select(x => x.PatientId).Distinct().CountAsync(cancellationToken),
            ActiveDiets = await diets.CountAsync(x => x.Status == GziPatientDietStatus.Active, cancellationToken),
            RequirementRevisions = await requirements.CountAsync(cancellationToken),
            AverageEnergyKcal = energyValues.Count == 0
                ? null
                : Math.Round(energyValues.Average(), 1)
        };
    }

    // ============================================================== penolong

    /// <summary>
    /// Menjadikan tanggal akhir mencakup seluruh harinya.
    /// </summary>
    /// <remarks>
    /// Penyaring layar mengirim tanggal tanpa jam. Memperlakukannya apa adanya berarti pukul
    /// 00:00, sehingga laporan yang diminta "sampai hari ini" justru membuang seluruh data hari
    /// ini — persis yang terjadi saat penelusuran layar dan membuat ringkasan menyebut satu order
    /// padahal ada dua.
    /// </remarks>
    private static DateTime ToInclusive(DateTime value)
    {
        // Jam diperiksa SEBELUM konversi ke UTC. Memeriksanya sesudah konversi keliru: tanggal
        // lokal pukul 00:00 berubah menjadi 17:00 UTC, sehingga tidak pernah dikenali sebagai
        // "tanggal tanpa jam" dan laporan tetap membuang data hari ini.
        var akhirHari = value.TimeOfDay == TimeSpan.Zero ? value.AddDays(1).AddTicks(-1) : value;
        return akhirHari.ToUniversalTime();
    }

    private static PagedResult<T> Page<T>(GziReportQuery request, int totalData, List<T> items) => new()
    {
        PageNumber = request.PageNumber,
        PageSize = request.PageSize,
        TotalData = totalData,
        TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize),
        Items = items
    };
}
