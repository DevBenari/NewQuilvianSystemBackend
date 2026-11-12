using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Implementasi Dispatcher Sinkronisasi Wewenang Klinis ke Modul SIMRS EMR (INT-HR-76, VR-OPP-21, VR-OPP-22, VR-OPP-25)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Keputusan jalur integrasi (Gelombang 2, 10 November 2026).</b> SIMRS/EMR berjalan di aplikasi yang sama, dan
    /// modul klinis (laboratorium, kamar operasi, gerbang kewenangan klinis) membaca tabel wewenang klinis bersama
    /// <c>WfpClinicalPrivilege</c>. Karena itu dispatcher <b>menulis langsung</b> ke tabel itu, bukan memanggil API
    /// terpisah: level 1–2 memblokir layanan klinis dan penjadwalan (hard-stop, VR-OPP-22), level 3 mewajibkan supervisi
    /// (VR-OPP-21), level 4 memulihkan wewenang mandiri. Putusan <c>Revoked</c> yang sudah dieksekusi tidak dipulihkan
    /// otomatis oleh level 4; pemulihannya lewat kredensialing ulang.
    /// </para>
    /// <para>
    /// Remediasi 10 November 2026 (BE-HRD-235) sebelumnya: log disimpan di <c>HrdSimrsPrivilegeSyncLog</c>, latensi diukur
    /// nyata, pengiriman hanya diterima untuk putusan yang tersimpan. Status log kini <c>SYNCED</c> (200) bila ada baris
    /// wewenang yang diubah, atau <c>NO_PRIVILEGE_ROWS</c> (404) bila dokter belum punya baris wewenang klinis.
    /// </para>
    /// </remarks>
    public class SimrsClinicalPrivilegeDispatcher : ISimrsClinicalPrivilegeDispatcher
    {
        public const string StatusSynced = "SYNCED";
        public const string StatusNoPrivilegeRows = "NO_PRIVILEGE_ROWS";
        public const string StatusPendingIntegration = "PENDING_INTEGRATION";

        private readonly ApplicationDbContext _context;

        public SimrsClinicalPrivilegeDispatcher(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SimrsPrivilegeSyncAuditLogDto> DispatchToSimrsAsync(
            Guid verdictId,
            DispatchSimrsPrivilegeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // 1. Validasi Struktur Kontrak Transmisi SIMRS (VR-OPP-25)
            if (verdictId == Guid.Empty || string.IsNullOrWhiteSpace(request.DoctorCode) || request.Level < 1 || request.Level > 4)
            {
                throw new InvalidOperationException("Payload transmisi ke SIMRS wajib memenuhi skema kontrak pertukaran data yang valid (VR-OPP-25).");
            }

            // 2. Putusan asal wajib ada di database — pengiriman tidak boleh menempel pada putusan yang tidak pernah disahkan
            var verdict = await _context.HrdRecredentialingVerdicts
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == verdictId && !v.IsDelete, cancellationToken);
            if (verdict == null)
            {
                throw new KeyNotFoundException($"Putusan rekredensialing {verdictId} tidak ditemukan. Sahkan putusan sidang lebih dulu sebelum mengirim wewenang ke SIMRS.");
            }

            var stopwatch = Stopwatch.StartNew();
            var now = DateTime.UtcNow;

            // 3. Evaluasi Efek Wewenang & Hard-Stop
            bool isHardStop;
            string syncMessage;

            if (request.Level <= 2)
            {
                isHardStop = true;
                syncMessage = "Instruksi Hard-Stop Mutlak (VR-OPP-22): akses peresepan obat keras dan penjadwalan tindakan operasi di modul SIMRS EMR diblokir total.";
            }
            else if (request.Level == 3)
            {
                isHardStop = false;
                syncMessage = "Instruksi Level 3 Supervisi (VR-OPP-21): seluruh instruksi tindakan mewajibkan co-signature digital supervisor dalam 24 jam.";
            }
            else
            {
                isHardStop = false;
                syncMessage = "Instruksi wewenang klinis Level 4 Mandiri penuh diterapkan pada SIMRS EMR.";
            }

            // 4. Terapkan ke tabel wewenang klinis bersama yang dibaca modul klinis
            var privilegesQuery = _context.WfpClinicalPrivileges
                .Where(p => p.WorkforceProfileId == verdict.WorkforceProfileId && !p.IsDelete);
            if (request.TindakanId.HasValue && request.TindakanId.Value != Guid.Empty)
            {
                privilegesQuery = privilegesQuery.Where(p => p.ClinicalPrivilegeCatalogId == request.TindakanId.Value || p.Id == request.TindakanId.Value);
            }

            var privileges = await privilegesQuery.ToListAsync(cancellationToken);
            var affected = new List<object>();

            foreach (var privilege in privileges)
            {
                var before = privilege.PrivilegeStatus;
                ApplyLevel(privilege, request.Level, verdict, now);
                affected.Add(new
                {
                    privilegeId = privilege.Id,
                    privilegeCode = privilege.PrivilegeCode,
                    statusBefore = before.ToString(),
                    statusAfter = privilege.PrivilegeStatus.ToString(),
                    privilege.IsClinicalServiceBlocked,
                    privilege.IsSchedulingBlocked,
                    privilege.IsSupervisionRequired
                });
            }

            var hasRows = privileges.Count > 0;
            var status = hasRows ? StatusSynced : StatusNoPrivilegeRows;
            var statusCode = hasRows ? 200 : 404;

            var payloadObject = new
            {
                verdictId,
                verdict = verdict.Verdict,
                workforceProfileId = verdict.WorkforceProfileId,
                doctorCode = request.DoctorCode,
                tindakanId = request.TindakanId,
                clinicalLevel = request.Level,
                isHardStopBlocked = isHardStop,
                timestamp = now,
                sourceModule = "Quilvian-Sub15-OPPE-Recred",
                target = "WfpClinicalPrivilege",
                affectedPrivileges = affected
            };

            var log = new HrdSimrsPrivilegeSyncLog
            {
                Id = Guid.NewGuid(),
                VerdictId = verdictId,
                DoctorCode = request.DoctorCode.Trim(),
                TargetLevel = request.Level,
                IsHardStopBlocked = isHardStop,
                PayloadJson = JsonSerializer.Serialize(payloadObject),
                ResponseStatusCode = statusCode,
                DispatchedAt = now,
                Status = status,
                Message = hasRows
                    ? $"{syncMessage} {privileges.Count} baris wewenang klinis diperbarui."
                    : $"{syncMessage} Namun dokter belum memiliki baris wewenang klinis (WfpClinicalPrivilege); tidak ada yang diubah. Terbitkan SPK/RKK lebih dulu.",
                CreateDateTime = now
            };

            _context.HrdSimrsPrivilegeSyncLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            log.LatencyMs = stopwatch.ElapsedMilliseconds;
            await _context.SaveChangesAsync(cancellationToken);

            return ToDto(log);
        }

        public async Task<List<SimrsPrivilegeSyncAuditLogDto>> GetSyncAuditLogsAsync(CancellationToken cancellationToken = default)
        {
            var logs = await _context.HrdSimrsPrivilegeSyncLogs
                .AsNoTracking()
                .Where(l => !l.IsDelete)
                .OrderByDescending(l => l.DispatchedAt)
                .ToListAsync(cancellationToken);

            return logs.Select(ToDto).ToList();
        }

        private static void ApplyLevel(WfpClinicalPrivilege privilege, int level, HrdRecredentialingVerdict verdict, DateTime now)
        {
            var reason = $"Putusan rekredensialing {verdict.Verdict} ({verdict.DecidedAt:yyyy-MM-dd}), level {level} (INT-HR-76).";

            switch (level)
            {
                case 1:
                    privilege.PrivilegeStatus = ClinicalPrivilegeStatus.Revoked;
                    privilege.IsClinicalServiceBlocked = true;
                    privilege.IsSchedulingBlocked = true;
                    privilege.IsSupervisionRequired = false;
                    privilege.RevokedAt = now;
                    privilege.RevokedByUserId = verdict.ApprovedByCommitteeChairmanId;
                    privilege.RevocationReason = reason;
                    privilege.IsActive = false;
                    break;
                case 2:
                    if (privilege.PrivilegeStatus != ClinicalPrivilegeStatus.Revoked)
                    {
                        privilege.PrivilegeStatus = ClinicalPrivilegeStatus.Suspended;
                        privilege.SuspendedAt = now;
                        privilege.SuspendedByUserId = verdict.ApprovedByCommitteeChairmanId;
                        privilege.SuspensionReason = reason;
                    }
                    privilege.IsClinicalServiceBlocked = true;
                    privilege.IsSchedulingBlocked = true;
                    break;
                case 3:
                    if (privilege.PrivilegeStatus == ClinicalPrivilegeStatus.Suspended)
                    {
                        privilege.PrivilegeStatus = ClinicalPrivilegeStatus.Active;
                        privilege.SuspendedAt = null;
                    }
                    if (privilege.PrivilegeStatus != ClinicalPrivilegeStatus.Revoked)
                    {
                        privilege.IsClinicalServiceBlocked = false;
                        privilege.IsSchedulingBlocked = false;
                        privilege.IsSupervisionRequired = true;
                        privilege.Restrictions = reason + " Wajib co-signature supervisor dalam 24 jam (VR-OPP-21).";
                    }
                    break;
                default:
                    if (privilege.PrivilegeStatus == ClinicalPrivilegeStatus.Suspended)
                    {
                        privilege.PrivilegeStatus = ClinicalPrivilegeStatus.Active;
                        privilege.SuspendedAt = null;
                    }
                    if (privilege.PrivilegeStatus != ClinicalPrivilegeStatus.Revoked)
                    {
                        privilege.IsClinicalServiceBlocked = false;
                        privilege.IsSchedulingBlocked = false;
                        privilege.IsSupervisionRequired = false;
                    }
                    break;
            }

            privilege.UpdateDateTime = now;
            privilege.UpdateBy = verdict.ApprovedByCommitteeChairmanId;
        }

        private static SimrsPrivilegeSyncAuditLogDto ToDto(HrdSimrsPrivilegeSyncLog log) => new()
        {
            Id = log.Id,
            VerdictId = log.VerdictId,
            DoctorCode = log.DoctorCode,
            TargetLevel = log.TargetLevel,
            IsHardStopBlocked = log.IsHardStopBlocked,
            PayloadJson = log.PayloadJson,
            ResponseStatusCode = log.ResponseStatusCode,
            LatencyMs = log.LatencyMs,
            DispatchedAt = log.DispatchedAt,
            Status = log.Status,
            Message = log.Message ?? string.Empty
        };
    }
}
