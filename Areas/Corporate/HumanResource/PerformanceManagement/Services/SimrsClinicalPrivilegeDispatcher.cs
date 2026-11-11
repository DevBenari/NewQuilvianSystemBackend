using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services
{
    /// <summary>
    /// Implementasi Dispatcher Sinkronisasi Wewenang Klinis ke Modul SIMRS EMR (INT-HR-76, VR-OPP-21, VR-OPP-22, VR-OPP-25)
    /// </summary>
    /// <remarks>
    /// Remediasi 10 November 2026 (BE-HRD-235):
    /// <list type="bullet">
    /// <item>Log pengiriman disimpan di tabel <c>HrdSimrsPrivilegeSyncLog</c>, bukan <c>ConcurrentBag</c> statis yang hilang saat restart.</item>
    /// <item>Pengiriman hanya diterima untuk putusan yang benar-benar tersimpan di <c>HrdRecredentialingVerdict</c>.</item>
    /// <item>Durasi (<c>LatencyMs</c>) diukur nyata memakai <see cref="Stopwatch"/>, bukan angka acak 25–65 ms.</item>
    /// <item>Status dicatat <c>PENDING_INTEGRATION</c> dengan kode respons <c>202</c>: payload sudah dibentuk dan tersimpan,
    /// tetapi pemanggilan ke modul EMR SIMRS menunggu keputusan pemilik tentang jalur integrasinya (tabel wewenang klinis
    /// bersama atau API terpisah — Gelombang 2 <c>backend-unfinished-work.md</c>). Sebelumnya status <c>200 SUCCESS</c> dikarang.</item>
    /// </list>
    /// </remarks>
    public class SimrsClinicalPrivilegeDispatcher : ISimrsClinicalPrivilegeDispatcher
    {
        public const string StatusPendingIntegration = "PENDING_INTEGRATION";
        private const int AcceptedStatusCode = 202;

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
            var verdictExists = await _context.HrdRecredentialingVerdicts
                .AsNoTracking()
                .AnyAsync(v => v.Id == verdictId && !v.IsDelete, cancellationToken);
            if (!verdictExists)
            {
                throw new KeyNotFoundException($"Putusan rekredensialing {verdictId} tidak ditemukan. Sahkan putusan sidang lebih dulu sebelum mengirim wewenang ke SIMRS.");
            }

            var stopwatch = Stopwatch.StartNew();

            // 3. Evaluasi Efek Wewenang & Hard-Stop
            bool isHardStop;
            string syncMessage;

            if (request.Level <= 2)
            {
                // VR-OPP-22: Hard-Stop Blokir Order SIMRS untuk Revokasi / Penurunan Tingkat Mutlak
                isHardStop = true;
                syncMessage = "Instruksi Hard-Stop Mutlak (VR-OPP-22) tersimpan: akses peresepan obat keras dan penjadwalan tindakan operasi di modul SIMRS EMR harus diblokir total.";
            }
            else if (request.Level == 3)
            {
                // VR-OPP-21: Level 3 Supervisi (Wajib Co-Signature 24 Jam)
                isHardStop = false;
                syncMessage = "Instruksi Level 3 Supervisi (VR-OPP-21) tersimpan: seluruh instruksi tindakan mewajibkan co-signature digital supervisor dalam 24 jam.";
            }
            else
            {
                // Level 4: Mandiri Penuh
                isHardStop = false;
                syncMessage = "Instruksi wewenang klinis Level 4 Mandiri penuh tersimpan untuk SIMRS EMR.";
            }

            var dispatchedAt = DateTime.UtcNow;
            var payloadObject = new
            {
                verdictId,
                doctorCode = request.DoctorCode,
                tindakanId = request.TindakanId,
                clinicalLevel = request.Level,
                isHardStopBlocked = isHardStop,
                timestamp = dispatchedAt,
                sourceModule = "Quilvian-Sub15-OPPE-Recred"
            };

            var payloadJson = JsonSerializer.Serialize(payloadObject);

            // 4. Simpan log pengiriman. Pemanggilan nyata ke modul EMR SIMRS menunggu keputusan jalur integrasi (Gelombang 2).
            var log = new HrdSimrsPrivilegeSyncLog
            {
                Id = Guid.NewGuid(),
                VerdictId = verdictId,
                DoctorCode = request.DoctorCode.Trim(),
                TargetLevel = request.Level,
                IsHardStopBlocked = isHardStop,
                PayloadJson = payloadJson,
                ResponseStatusCode = AcceptedStatusCode,
                DispatchedAt = dispatchedAt,
                Status = StatusPendingIntegration,
                Message = $"{syncMessage} Pengiriman ke modul EMR SIMRS menunggu jalur integrasi yang ditetapkan pemilik (INT-HR-76).",
                CreateDateTime = dispatchedAt
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
