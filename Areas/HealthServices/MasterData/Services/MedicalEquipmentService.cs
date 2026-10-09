using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
public class MedicalEquipmentService(ApplicationDbContext db, LoggerService logger)
{
    private IQueryable<MstMedicalEquipment> Query() => db.MstMedicalEquipments.AsNoTracking().Where(x => !x.IsDelete);
    public async Task<MedicalEquipmentSummary> SummaryAsync(CancellationToken ct) => new() { Total = await Query().CountAsync(ct), Active = await Query().CountAsync(x => x.IsActive, ct), Inactive = await Query().CountAsync(x => !x.IsActive, ct) };
    public async Task<List<MedicalEquipmentOption>> OptionsAsync(string? search, CancellationToken ct)
    {
        var q = Query().Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(x => x.EquipmentName.ToLower().Contains(s) || x.EquipmentCode.ToLower().Contains(s));
        }
        var list = await q.OrderBy(x => x.EquipmentName).ToListAsync(ct);
        var eqIds = list.Select(x => x.Id).ToList();
        var tariffs = await db.Set<MstTariff>().AsNoTracking()
            .Where(t => t.MedicalEquipmentId.HasValue && eqIds.Contains(t.MedicalEquipmentId.Value) && t.IsActive && !t.IsDelete && !t.IsCancel)
            .Select(t => new { t.MedicalEquipmentId, t.NormalPrice, t.TariffCode })
            .ToListAsync(ct);
        var tariffMap = tariffs.GroupBy(t => t.MedicalEquipmentId!.Value).ToDictionary(g => g.Key, g => g.First());

        return list.Select(x =>
        {
            var hasTariff = tariffMap.TryGetValue(x.Id, out var trf);
            return new MedicalEquipmentOption
            {
                Id = x.Id,
                EquipmentCode = x.EquipmentCode,
                EquipmentName = x.EquipmentName,
                ChargeUnit = x.ChargeUnit,
                NormalPrice = hasTariff ? trf?.NormalPrice : null,
                TariffCode = hasTariff ? trf?.TariffCode : null
            };
        }).ToList();
    }
    public async Task<PagedResult<MedicalEquipmentResponse>> ListAsync(MedicalEquipmentQuery request, CancellationToken ct)
    {
        var q = Query();
        if (request.IsActive.HasValue) q = q.Where(x => x.IsActive == request.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLower();
            q = q.Where(x => x.EquipmentName.ToLower().Contains(s) || x.EquipmentCode.ToLower().Contains(s));
        }
        var total = await q.CountAsync(ct);
        var desc = request.SortDirection?.ToLower() == "desc";
        q = request.SortBy?.ToLower() switch
        {
            "equipmentcode" => desc ? q.OrderByDescending(x => x.EquipmentCode) : q.OrderBy(x => x.EquipmentCode),
            "createdatetime" => desc ? q.OrderByDescending(x => x.CreateDateTime) : q.OrderBy(x => x.CreateDateTime),
            _ => desc ? q.OrderByDescending(x => x.EquipmentName) : q.OrderBy(x => x.EquipmentName)
        };
        var page = Math.Max(1, request.PageNumber);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var eqIds = items.Select(x => x.Id).ToList();
        var tariffs = await db.Set<MstTariff>().AsNoTracking()
            .Where(t => t.MedicalEquipmentId.HasValue && eqIds.Contains(t.MedicalEquipmentId.Value) && t.IsActive && !t.IsDelete && !t.IsCancel)
            .Select(t => new { t.MedicalEquipmentId, t.NormalPrice, t.TariffCode })
            .ToListAsync(ct);
        var tariffMap = tariffs.GroupBy(t => t.MedicalEquipmentId!.Value).ToDictionary(g => g.Key, g => g.First());

        return new()
        {
            PageNumber = page,
            PageSize = size,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)size),
            Items = items.Select(x =>
            {
                var resp = Map(x);
                if (tariffMap.TryGetValue(x.Id, out var trf))
                {
                    resp.NormalPrice = trf.NormalPrice;
                    resp.TariffCode = trf.TariffCode;
                }
                return resp;
            }).ToList()
        };
    }
    public async Task<NursingResult<MedicalEquipmentResponse>> DetailAsync(Guid id, CancellationToken ct)
    {
        var row = await Query().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (row == null) return NursingResult<MedicalEquipmentResponse>.Fail(404, "Jenis alat tidak ditemukan.");
        var resp = Map(row);
        var trf = await db.Set<MstTariff>().AsNoTracking()
            .Where(t => t.MedicalEquipmentId == id && t.IsActive && !t.IsDelete && !t.IsCancel)
            .Select(t => new { t.NormalPrice, t.TariffCode })
            .FirstOrDefaultAsync(ct);
        if (trf != null)
        {
            resp.NormalPrice = trf.NormalPrice;
            resp.TariffCode = trf.TariffCode;
        }
        return NursingResult<MedicalEquipmentResponse>.Ok(resp, "Jenis alat berhasil diambil.");
    }
    public async Task<NursingResult<MedicalEquipmentResponse>> SaveAsync(Guid? id, CreateMedicalEquipmentRequest request, Guid actor, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.EquipmentCode) || string.IsNullOrWhiteSpace(request.EquipmentName) || !Enum.IsDefined(request.ChargeUnit) || !Enum.IsDefined(request.RoundingRule)) return NursingResult<MedicalEquipmentResponse>.Fail(400, "Kode, nama, satuan, dan pembulatan wajib sah.");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var row = id.HasValue ? await db.MstMedicalEquipments.FromSqlInterpolated($"SELECT * FROM public.\"MstMedicalEquipment\" WHERE \"Id\" = {id.Value} AND \"IsDelete\" = false FOR UPDATE").FirstOrDefaultAsync(ct) : new MstMedicalEquipment { CreateBy = actor };
        if (row == null) return NursingResult<MedicalEquipmentResponse>.Fail(404, "Jenis alat tidak ditemukan.");
        if (id.HasValue && request is UpdateMedicalEquipmentRequest update && row.RowVersion != update.RowVersion) return NursingResult<MedicalEquipmentResponse>.Fail(409, "Versi berubah. Muat ulang alat.");
        if (id.HasValue && (row.ChargeUnit != request.ChargeUnit || row.RoundingRule != request.RoundingRule) &&
            await db.Set<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliEquipmentUsage>().AnyAsync(x => x.MedicalEquipmentId == row.Id && x.Status == QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums.CliEquipmentUsageStatus.Running && !x.IsDelete, ct))
            return NursingResult<MedicalEquipmentResponse>.Fail(422, "Satuan dan pembulatan tidak dapat diubah selama alat sedang dipakai.", "MST-EQP-002");
        var code = request.EquipmentCode.Trim().ToUpperInvariant();
        if (await db.MstMedicalEquipments.AnyAsync(x => x.Id != row.Id && x.EquipmentCode == code, ct)) return NursingResult<MedicalEquipmentResponse>.Fail(409, "Kode alat sudah dipakai.", "MST-EQP-001");
        row.EquipmentCode = code; row.EquipmentName = request.EquipmentName.Trim(); row.CategoryName = request.CategoryName?.Trim(); row.Description = request.Description?.Trim();
        row.ChargeUnit = request.ChargeUnit; row.RoundingRule = request.RoundingRule; row.RowVersion = Guid.NewGuid();
        if (id.HasValue) { row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow; } else db.Add(row);
        try { await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" }) { return NursingResult<MedicalEquipmentResponse>.Fail(409, "Kode alat sudah dipakai.", "MST-EQP-001"); }
        await LogAsync(id.HasValue ? "Update" : "Create", row.Id, actor);
        return NursingResult<MedicalEquipmentResponse>.Ok(Map(row), "Jenis alat tersimpan.", id.HasValue ? 200 : 201);
    }
    public async Task<NursingResult<MedicalEquipmentResponse>> SetStatusAsync(Guid id, MedicalEquipmentStatusRequest request, Guid actor, CancellationToken ct)
    {
        var row = await db.MstMedicalEquipments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
        if (row == null) return NursingResult<MedicalEquipmentResponse>.Fail(404, "Jenis alat tidak ditemukan.");
        if (request.RowVersion.HasValue && row.RowVersion != request.RowVersion) return NursingResult<MedicalEquipmentResponse>.Fail(409, "Versi berubah.");
        row.IsActive = request.IsActive; row.RowVersion = Guid.NewGuid(); row.UpdateBy = actor; row.UpdateDateTime = DateTime.UtcNow;
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return NursingResult<MedicalEquipmentResponse>.Fail(409, "Versi berubah."); }
        await LogAsync("Status", id, actor); return NursingResult<MedicalEquipmentResponse>.Ok(Map(row), "Status alat tersimpan.");
    }
    public async Task<NursingResult<MedicalEquipmentResponse>> DeleteAsync(Guid id, Guid actor, CancellationToken ct)
    {
        var row = await db.MstMedicalEquipments.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
        if (row == null) return NursingResult<MedicalEquipmentResponse>.Fail(404, "Jenis alat tidak ditemukan.");
        if (await db.Set<MstTariff>().AnyAsync(x => x.MedicalEquipmentId == id, ct) || await db.Set<QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models.CliEquipmentUsage>().AnyAsync(x => x.MedicalEquipmentId == id, ct)) return NursingResult<MedicalEquipmentResponse>.Fail(400, "Alat sudah dipakai tarif. Nonaktifkan bila tidak digunakan lagi.");
        row.IsDelete = true; row.IsActive = false; row.DeleteBy = actor; row.DeleteDateTime = DateTime.UtcNow; row.RowVersion = Guid.NewGuid();
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return NursingResult<MedicalEquipmentResponse>.Fail(409, "Versi berubah."); }
        await LogAsync("Delete", id, actor); return NursingResult<MedicalEquipmentResponse>.Ok(Map(row), "Jenis alat ditandai terhapus.");
    }
    private Task LogAsync(string action, Guid id, Guid actor) => logger.InfoAsync("MasterData.MedicalEquipment", action, "Master jenis alat berubah.", new { Id = id, ActorUserId = actor });
    private static MedicalEquipmentResponse Map(MstMedicalEquipment x) => new() { Id = x.Id, EquipmentCode = x.EquipmentCode, EquipmentName = x.EquipmentName, CategoryName = x.CategoryName, ChargeUnit = x.ChargeUnit, RoundingRule = x.RoundingRule, Description = x.Description, IsActive = x.IsActive, RowVersion = x.RowVersion };
}
