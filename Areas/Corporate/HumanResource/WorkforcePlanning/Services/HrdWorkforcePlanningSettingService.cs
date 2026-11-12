using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Services
{
    /// <summary>
    /// Implementasi layanan pengaturan perencanaan tenaga kerja per rumah sakit.
    /// Memenuhi kriteria BE-HRD-031, HRD-DEC-061, HRD-DEC-071, VR-WFP-40, VR-WFP-41, dan NFR-010/NFR-011.
    /// </summary>
    public sealed class HrdWorkforcePlanningSettingService : IHrdWorkforcePlanningSettingService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<HrdWorkforcePlanningSettingService> _logger;

        public HrdWorkforcePlanningSettingService(
            ApplicationDbContext dbContext,
            ILogger<HrdWorkforcePlanningSettingService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> GetByHospitalSiteIdAsync(
            Guid hospitalSiteId,
            CancellationToken cancellationToken = default)
        {
            if (hospitalSiteId == Guid.Empty)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "ID rumah sakit wajib diisi.");
            }

            var site = await _dbContext.MstHospitalSites
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (site == null)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rumah sakit tidak ditemukan.");
            }

            var setting = await _dbContext.HrdWorkforcePlanningSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.HospitalSiteId == hospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting == null)
            {
                // Kriteria 1: Rumah sakit yang belum punya baris mendapat kedua nilai kosong (null), bukan angka bawaan.
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Ok(new WorkforcePlanningSettingResponse
                {
                    Id = null,
                    HospitalSiteId = site.Id,
                    HospitalSiteName = site.SiteName,
                    LongLeaveThresholdDays = null,
                    OverdueRequestThresholdDays = null,
                    ChangeReason = null,
                    RowVersion = null,
                    UpdatedAt = null,
                    UpdatedBy = null
                }, "Pengaturan rumah sakit berhasil diambil.");
            }

            return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Ok(new WorkforcePlanningSettingResponse
            {
                Id = setting.Id,
                HospitalSiteId = setting.HospitalSiteId,
                HospitalSiteName = site.SiteName,
                LongLeaveThresholdDays = setting.LongLeaveThresholdDays,
                OverdueRequestThresholdDays = setting.OverdueRequestThresholdDays,
                ChangeReason = setting.ChangeReason,
                RowVersion = setting.RowVersion,
                UpdatedAt = setting.UpdateDateTime ?? setting.CreateDateTime,
                UpdatedBy = setting.UpdateBy != Guid.Empty ? setting.UpdateBy : setting.CreateBy
            }, "Pengaturan rumah sakit berhasil diambil.");
        }

        public async Task<WorkforcePlanningResult<WorkforcePlanningSettingResponse>> UpdateAsync(
            UpdateWorkforcePlanningSettingRequest request,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.HospitalSiteId == Guid.Empty)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "ID rumah sakit wajib diisi.");
            }

            // Kriteria 3 (VR-WFP-40): Alasan perubahan pengaturan tidak boleh kosong
            if (string.IsNullOrWhiteSpace(request.ChangeReason))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tuliskan alasan perubahan pengaturan.");
            }

            // Kriteria 4 (VR-WFP-41): Batas cuti panjang atau umur permintaan < 1 atau > 365
            if ((request.LongLeaveThresholdDays.HasValue && (request.LongLeaveThresholdDays.Value < 1 || request.LongLeaveThresholdDays.Value > 365)) ||
                (request.OverdueRequestThresholdDays.HasValue && (request.OverdueRequestThresholdDays.Value < 1 || request.OverdueRequestThresholdDays.Value > 365)))
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Isi angka antara 1 dan 365 hari.");
            }

            var site = await _dbContext.MstHospitalSites
                .FirstOrDefaultAsync(s => s.Id == request.HospitalSiteId && !s.IsDelete, cancellationToken);

            if (site == null)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Rumah sakit tidak ditemukan.");
            }

            var setting = await _dbContext.HrdWorkforcePlanningSettings
                .FirstOrDefaultAsync(s => s.HospitalSiteId == request.HospitalSiteId && !s.IsDelete, cancellationToken);

            if (setting != null)
            {
                // Kriteria 5: Optimistic concurrency check terhadap rowVersion
                if (!request.RowVersion.HasValue || request.RowVersion.Value == Guid.Empty || request.RowVersion.Value != setting.RowVersion)
                {
                    return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Data telah diubah oleh pengguna lain. Silakan muat ulang data terbaru.");
                }

                setting.LongLeaveThresholdDays = request.LongLeaveThresholdDays;
                setting.OverdueRequestThresholdDays = request.OverdueRequestThresholdDays;
                setting.ChangeReason = request.ChangeReason.Trim();
                setting.RowVersion = Guid.NewGuid();
                setting.UpdateDateTime = DateTime.UtcNow;
                setting.UpdateBy = currentUserId;
            }
            else
            {
                if (request.RowVersion.HasValue && request.RowVersion.Value != Guid.Empty)
                {
                    return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        "Data telah diubah oleh pengguna lain. Silakan muat ulang data terbaru.");
                }

                setting = new HrdWorkforcePlanningSetting
                {
                    Id = Guid.NewGuid(),
                    HospitalSiteId = request.HospitalSiteId,
                    LongLeaveThresholdDays = request.LongLeaveThresholdDays,
                    OverdueRequestThresholdDays = request.OverdueRequestThresholdDays,
                    ChangeReason = request.ChangeReason.Trim(),
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = currentUserId,
                    IsDelete = false
                };

                _dbContext.HrdWorkforcePlanningSettings.Add(setting);
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Data telah diubah oleh pengguna lain. Silakan muat ulang data terbaru.");
            }

            // Kriteria 6: Audit trail sesuai permission-audit-matrix.md bagian 11.4.
            // Payload logger HANYA memuat EntityId, controller, action, dan status — TANPA kolom bertanda sensitif (alasan/biaya).
            _logger.LogInformation(
                "WorkforcePlanningSetting updated. EntityId: {EntityId}, Controller: {Controller}, Action: {Action}, Status: {Status}",
                setting.Id,
                "WorkforcePlanningSetting",
                "Update",
                "Success");

            return WorkforcePlanningResult<WorkforcePlanningSettingResponse>.Ok(new WorkforcePlanningSettingResponse
            {
                Id = setting.Id,
                HospitalSiteId = setting.HospitalSiteId,
                HospitalSiteName = site.SiteName,
                LongLeaveThresholdDays = setting.LongLeaveThresholdDays,
                OverdueRequestThresholdDays = setting.OverdueRequestThresholdDays,
                ChangeReason = setting.ChangeReason,
                RowVersion = setting.RowVersion,
                UpdatedAt = setting.UpdateDateTime ?? setting.CreateDateTime,
                UpdatedBy = setting.UpdateBy != Guid.Empty ? setting.UpdateBy : setting.CreateBy
            }, "Pengaturan perencanaan berhasil disimpan.");
        }
    }
}
