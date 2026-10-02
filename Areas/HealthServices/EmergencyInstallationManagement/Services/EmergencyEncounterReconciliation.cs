using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    public static class EmergencyEncounterReconciliation
    {
        public const string PrefixNomorRun = "REKIGD";

        private const string KunciRekonsiliasi = "EMG_ENCOUNTER_RECONCILIATION";
        private const int PanjangMaksimalAlasan = 500;

        public const string PesanAlasanRunWajib =
            "Alasan rekonsiliasi wajib diisi (maksimal 500 karakter).";

        public const string PesanAlasanPembalikanWajib =
            "Alasan pembalikan wajib diisi (maksimal 500 karakter).";

        public const string PesanDataBerubah =
            "Data berubah sejak pratinjau; muat ulang pratinjau.";

        public const string PesanRunSudahDibalik = "Run ini sudah dibalik.";

        public sealed record Hasil<T>(T? Data, int StatusCode, string? Penolakan)
        {
            public bool Berhasil => Penolakan == null;
            public static Hasil<T> Ok(T data) => new(data, StatusCodes.Status200OK, null);
            public static Hasil<T> Dibuat(T data) => new(data, StatusCodes.Status201Created, null);
            public static Hasil<T> Gagal(int statusCode, string penolakan) => new(default, statusCode, penolakan);
        }

        public sealed record JumlahKelas(int K1, int K1Outpatient, int K2, int K3, int K4)
        {
            public int Ditulis => K1 + K1Outpatient;
        }

        public sealed class BarisKandidat
        {
            public Guid EncounterId { get; set; }
            public string EncounterNumber { get; set; } = string.Empty;
            public EncounterType EncounterType { get; set; }
            public EncounterStatus EncounterStatus { get; set; }
            public DateTime RegisteredAt { get; set; }
            public Guid PatientId { get; set; }
            public string? PatientName { get; set; }
            public string? MedicalRecordNumber { get; set; }
            public Guid EmergencyVisitId { get; set; }
            public string EmergencyVisitNumber { get; set; } = string.Empty;
            public EmergencyVisitStatus VisitStatus { get; set; }
            public DateTime? VisitCompletedAt { get; set; }
        }

        public static async Task<EmergencyEncounterReconciliationPreviewResponse> PreviewAsync(
            ApplicationDbContext dbContext,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            var jumlah = await HitungKelasAsync(dbContext, cancellationToken);

            var kueri = KueriKandidat(dbContext);
            var total = await kueri.CountAsync(cancellationToken);
            var baris = await kueri
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new EmergencyEncounterReconciliationPreviewResponse
            {
                CountK1 = jumlah.K1,
                CountK1Outpatient = jumlah.K1Outpatient,
                CountK2 = jumlah.K2,
                CountK3 = jumlah.K3,
                CountK4 = jumlah.K4,
                ExpectedCount = jumlah.Ditulis,
                Rows = new PagedResult<EmergencyEncounterReconciliationPreviewRowResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = total,
                    TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                    Items = baris.Select(ToPreviewRow).ToList()
                }
            };
        }

        public static async Task<Hasil<EmgEncounterReconciliationRun>> ExecuteAsync(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService,
            ExecuteEmergencyEncounterReconciliationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            ArgumentNullException.ThrowIfNull(documentNumberService);

            var alasan = NormalizeText(request?.Reason);
            if (alasan == null || alasan.Length > PanjangMaksimalAlasan)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(StatusCodes.Status400BadRequest, PesanAlasanRunWajib);

            if (request!.ExpectedCount == null || request.ExpectedCount.Value < 0)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "expectedCount wajib diisi dengan jumlah baris K1 dan K1-Outpatient yang terlihat pada pratinjau.");

            if (actorUserId == Guid.Empty)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "Pelaku rekonsiliasi tidak dapat ditentukan dari token.");

            await using var transaksi = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await KunciRekonsiliasiAsync(dbContext, cancellationToken);

            var now = DateTime.UtcNow;
            var jumlah = await HitungKelasAsync(dbContext, cancellationToken);
            var kandidat = await KueriKandidat(dbContext).ToListAsync(cancellationToken);

            if (kandidat.Count != request.ExpectedCount.Value)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(StatusCodes.Status409Conflict, PesanDataBerubah);

            var run = new EmgEncounterReconciliationRun
            {
                Id = Guid.NewGuid(),
                RunNumber = await BentukNomorRunAsync(dbContext, documentNumberService, now, cancellationToken),
                Status = EmergencyReconciliationRunStatus.Executed,
                Reason = alasan,
                CountK1 = jumlah.K1,
                CountK1Outpatient = jumlah.K1Outpatient,
                CountK2 = jumlah.K2,
                CountK3 = jumlah.K3,
                CountK4 = jumlah.K4,
                ExecutedByUserId = actorUserId,
                ExecutedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            dbContext.Set<EmgEncounterReconciliationRun>().Add(run);

            foreach (var baris in kandidat)
            {
                var encounter = await dbContext.Set<RegPatientEncounter>()
                    .FirstOrDefaultAsync(x => x.Id == baris.EncounterId && !x.IsDelete, cancellationToken);

                if (encounter == null || EmergencyEpisodeRule.IsEncounterEnded(encounter))
                    return Hasil<EmgEncounterReconciliationRun>.Gagal(StatusCodes.Status409Conflict, PesanDataBerubah);

                var statusBaru = baris.VisitStatus == EmergencyVisitStatus.Completed
                    ? EncounterStatus.Completed
                    : EncounterStatus.Cancelled;

                var waktuSelesaiBaru = baris.VisitStatus == EmergencyVisitStatus.Completed
                    ? baris.VisitCompletedAt
                    : encounter.CompletedAt;

                var item = new EmgEncounterReconciliationItem
                {
                    Id = Guid.NewGuid(),
                    RunId = run.Id,
                    EncounterId = encounter.Id,
                    EmergencyVisitId = baris.EmergencyVisitId,
                    Class = baris.EncounterType == EncounterType.Emergency
                        ? EmergencyReconciliationClass.K1
                        : EmergencyReconciliationClass.K1Outpatient,
                    StatusBefore = encounter.EncounterStatus,
                    StatusAfter = statusBaru,
                    CompletedAtBefore = encounter.CompletedAt,
                    CompletedAtAfter = waktuSelesaiBaru,
                    IsReversed = false,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                encounter.EncounterStatus = statusBaru;
                encounter.CompletedAt = waktuSelesaiBaru;
                encounter.UpdateDateTime = now;
                encounter.UpdateBy = actorUserId;

                dbContext.Set<EmgEncounterReconciliationItem>().Add(item);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            return Hasil<EmgEncounterReconciliationRun>.Dibuat(
                await MuatRunAsync(dbContext, run.Id, cancellationToken) ?? run);
        }

        public static async Task<Hasil<EmgEncounterReconciliationRun>> ReverseAsync(
            ApplicationDbContext dbContext,
            Guid runId,
            ReverseEmergencyEncounterReconciliationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            var alasan = NormalizeText(request?.Reason);
            if (alasan == null || alasan.Length > PanjangMaksimalAlasan)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(StatusCodes.Status400BadRequest, PesanAlasanPembalikanWajib);

            if (actorUserId == Guid.Empty)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(
                    StatusCodes.Status400BadRequest,
                    "Pelaku pembalikan tidak dapat ditentukan dari token.");

            await using var transaksi = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await KunciRekonsiliasiAsync(dbContext, cancellationToken);

            var run = await dbContext.Set<EmgEncounterReconciliationRun>()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == runId && !x.IsDelete, cancellationToken);

            if (run == null)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(
                    StatusCodes.Status404NotFound,
                    "Run rekonsiliasi tidak ditemukan.");

            if (run.Status == EmergencyReconciliationRunStatus.Reversed)
                return Hasil<EmgEncounterReconciliationRun>.Gagal(StatusCodes.Status409Conflict, PesanRunSudahDibalik);

            var now = DateTime.UtcNow;

            foreach (var item in run.Items)
            {
                if (item.IsReversed)
                    continue;

                var encounter = await dbContext.Set<RegPatientEncounter>()
                    .FirstOrDefaultAsync(x => x.Id == item.EncounterId, cancellationToken);

                if (encounter == null)
                {
                    item.ReverseSkipReason = "Encounter tidak ditemukan lagi, jadi nilainya tidak dikembalikan.";
                    item.UpdateDateTime = now;
                    item.UpdateBy = actorUserId;
                    continue;
                }

                if (encounter.EncounterStatus != item.StatusAfter || encounter.CompletedAt != item.CompletedAtAfter)
                {
                    item.ReverseSkipReason = "Nilai encounter sudah berubah sesudah run, jadi tidak dikembalikan.";
                    item.UpdateDateTime = now;
                    item.UpdateBy = actorUserId;
                    continue;
                }

                encounter.EncounterStatus = item.StatusBefore;
                encounter.CompletedAt = item.CompletedAtBefore;
                encounter.UpdateDateTime = now;
                encounter.UpdateBy = actorUserId;

                item.IsReversed = true;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            run.Status = EmergencyReconciliationRunStatus.Reversed;
            run.ReversedByUserId = actorUserId;
            run.ReversedAt = now;
            run.ReverseReason = alasan;
            run.UpdateDateTime = now;
            run.UpdateBy = actorUserId;

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            return Hasil<EmgEncounterReconciliationRun>.Ok(
                await MuatRunAsync(dbContext, run.Id, cancellationToken) ?? run);
        }

        public static async Task<JumlahKelas> HitungKelasAsync(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            var dasar = KueriEncounterTerbuka(dbContext);

            var k1 = await dasar.CountAsync(
                e => e.EncounterType == EncounterType.Emergency &&
                     dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id && !v.IsDelete &&
                         (v.VisitStatus == EmergencyVisitStatus.Completed || v.VisitStatus == EmergencyVisitStatus.Cancelled)),
                cancellationToken);

            var k1Outpatient = await dasar.CountAsync(
                e => e.EncounterType == EncounterType.Outpatient &&
                     dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id && !v.IsDelete &&
                         (v.VisitStatus == EmergencyVisitStatus.Completed || v.VisitStatus == EmergencyVisitStatus.Cancelled)),
                cancellationToken);

            var k2 = await dasar.CountAsync(
                e => dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id && !v.IsDelete &&
                         v.VisitStatus != EmergencyVisitStatus.Completed && v.VisitStatus != EmergencyVisitStatus.Cancelled),
                cancellationToken);

            var k3 = await dasar.CountAsync(
                e => e.EncounterType == EncounterType.Emergency &&
                     !dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id),
                cancellationToken);

            var k4 = await dasar.CountAsync(
                e => dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id) &&
                     !dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id && !v.IsDelete),
                cancellationToken);

            return new JumlahKelas(k1, k1Outpatient, k2, k3, k4);
        }

        public static IQueryable<EmgEncounterReconciliationRun> KueriRun(ApplicationDbContext dbContext)
            => dbContext.Set<EmgEncounterReconciliationRun>()
                .AsNoTracking()
                .Include(x => x.ExecutedByUser)
                .Include(x => x.ReversedByUser)
                .Where(x => !x.IsDelete);

        public static async Task<PagedResult<EmergencyEncounterReconciliationRunResponse>> RiwayatAsync(
            ApplicationDbContext dbContext,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            var kueri = KueriRun(dbContext).OrderByDescending(x => x.ExecutedAt);

            var totalData = await kueri.CountAsync(cancellationToken);
            var entities = await kueri
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<EmergencyEncounterReconciliationRunResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => ToRunResponse(x, false)).ToList()
            };
        }

        public static Task<EmgEncounterReconciliationRun?> FindRunAsync(
            ApplicationDbContext dbContext,
            Guid runId,
            CancellationToken cancellationToken = default)
            => KueriRun(dbContext)
                .Include(x => x.Items).ThenInclude(x => x.Encounter)
                .Include(x => x.Items).ThenInclude(x => x.EmergencyVisit)
                .FirstOrDefaultAsync(x => x.Id == runId, cancellationToken);

        public static EmergencyEncounterReconciliationRunResponse ToRunResponse(
            EmgEncounterReconciliationRun run,
            bool sertakanBaris)
        {
            ArgumentNullException.ThrowIfNull(run);

            var response = new EmergencyEncounterReconciliationRunResponse
            {
                Id = run.Id,
                RunNumber = run.RunNumber,
                Status = run.Status,
                Reason = run.Reason,
                CountK1 = run.CountK1,
                CountK1Outpatient = run.CountK1Outpatient,
                CountK2 = run.CountK2,
                CountK3 = run.CountK3,
                CountK4 = run.CountK4,
                CountWritten = run.CountK1 + run.CountK1Outpatient,
                ExecutedByUserId = run.ExecutedByUserId,
                ExecutedByName = NamaPengguna(run.ExecutedByUser),
                ExecutedAt = run.ExecutedAt,
                ReversedByUserId = run.ReversedByUserId,
                ReversedByName = NamaPengguna(run.ReversedByUser),
                ReversedAt = run.ReversedAt,
                ReverseReason = run.ReverseReason
            };

            if (sertakanBaris)
            {
                response.Items = run.Items.Select(ToItemResponse).ToList();
                response.CountReversed = run.Items.Count(x => x.IsReversed);
                response.CountSkipped = run.Items.Count(x => x.ReverseSkipReason != null);
            }

            return response;
        }

        public static EmergencyEncounterReconciliationItemResponse ToItemResponse(EmgEncounterReconciliationItem item)
        {
            ArgumentNullException.ThrowIfNull(item);

            return new EmergencyEncounterReconciliationItemResponse
            {
                Id = item.Id,
                EncounterId = item.EncounterId,
                EncounterNumber = item.Encounter?.EncounterNumber,
                EmergencyVisitId = item.EmergencyVisitId,
                EmergencyVisitNumber = item.EmergencyVisit?.EmergencyVisitNumber,
                Class = item.Class,
                StatusBefore = item.StatusBefore,
                StatusAfter = item.StatusAfter,
                CompletedAtBefore = item.CompletedAtBefore,
                CompletedAtAfter = item.CompletedAtAfter,
                IsReversed = item.IsReversed,
                ReverseSkipReason = item.ReverseSkipReason
            };
        }

        private static IQueryable<RegPatientEncounter> KueriEncounterTerbuka(ApplicationDbContext dbContext)
            => dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Where(EmergencyEpisodeRule.EncounterNotEnded);

        private static IQueryable<BarisKandidat> KueriKandidat(ApplicationDbContext dbContext)
            => from e in KueriEncounterTerbuka(dbContext)
               where e.EncounterType == EncounterType.Emergency || e.EncounterType == EncounterType.Outpatient
               from v in dbContext.Set<EmgVisit>().AsNoTracking()
                   .Where(x => x.EncounterId == e.Id
                               && !x.IsDelete
                               && (x.VisitStatus == EmergencyVisitStatus.Completed
                                   || x.VisitStatus == EmergencyVisitStatus.Cancelled))
               orderby e.RegisteredAt
               select new BarisKandidat
               {
                   EncounterId = e.Id,
                   EncounterNumber = e.EncounterNumber,
                   EncounterType = e.EncounterType,
                   EncounterStatus = e.EncounterStatus,
                   RegisteredAt = e.RegisteredAt,
                   PatientId = e.PatientId,
                   PatientName = v.Patient == null ? null : v.Patient.FullName,
                   MedicalRecordNumber = v.Patient == null ? null : v.Patient.MedicalRecordNumber,
                   EmergencyVisitId = v.Id,
                   EmergencyVisitNumber = v.EmergencyVisitNumber,
                   VisitStatus = v.VisitStatus,
                   VisitCompletedAt = v.VisitCompletedAt
               };

        private static EmergencyEncounterReconciliationPreviewRowResponse ToPreviewRow(BarisKandidat baris)
        {
            var statusBaru = baris.VisitStatus == EmergencyVisitStatus.Completed
                ? EncounterStatus.Completed
                : EncounterStatus.Cancelled;

            return new EmergencyEncounterReconciliationPreviewRowResponse
            {
                EncounterId = baris.EncounterId,
                EncounterNumber = baris.EncounterNumber,
                EncounterType = baris.EncounterType,
                EncounterStatus = baris.EncounterStatus,
                RegisteredAt = baris.RegisteredAt,
                PatientId = baris.PatientId,
                PatientName = baris.PatientName,
                MedicalRecordNumber = baris.MedicalRecordNumber,
                EmergencyVisitId = baris.EmergencyVisitId,
                EmergencyVisitNumber = baris.EmergencyVisitNumber,
                VisitStatus = baris.VisitStatus,
                VisitCompletedAt = baris.VisitCompletedAt,
                Class = baris.EncounterType == EncounterType.Emergency
                    ? EmergencyReconciliationClass.K1
                    : EmergencyReconciliationClass.K1Outpatient,
                StatusAfter = statusBaru,
                CompletedAtAfter = baris.VisitStatus == EmergencyVisitStatus.Completed
                    ? baris.VisitCompletedAt
                    : null
            };
        }

        private static Task<EmgEncounterReconciliationRun?> MuatRunAsync(
            ApplicationDbContext dbContext,
            Guid runId,
            CancellationToken cancellationToken)
            => FindRunAsync(dbContext, runId, cancellationToken);

        private static async Task<string> BentukNomorRunAsync(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService,
            DateTime now,
            CancellationToken cancellationToken)
        {
            for (var percobaan = 0; percobaan < 10; percobaan++)
            {
                var nomor = documentNumberService.Generate(PrefixNomorRun, now);

                var sudahDipakai = await dbContext.Set<EmgEncounterReconciliationRun>()
                    .AsNoTracking()
                    .AnyAsync(x => x.RunNumber == nomor, cancellationToken);

                if (!sudahDipakai)
                    return nomor;
            }

            throw new InvalidOperationException("Nomor run rekonsiliasi unik gagal dibentuk.");
        }

        private static async Task KunciRekonsiliasiAsync(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            if (dbContext.Database.CurrentTransaction is null)
                throw new InvalidOperationException("Kunci rekonsiliasi IGD wajib diambil di dalam transaksi.");

            if (!dbContext.Database.IsNpgsql())
                return;

            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtext({KunciRekonsiliasi})::bigint)",
                cancellationToken);
        }

        private static string? NamaPengguna(ApplicationUser? pengguna)
            => pengguna == null
                ? null
                : pengguna.DisplayName ?? pengguna.UserName ?? pengguna.Email ?? pengguna.UserCode;

        private static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
