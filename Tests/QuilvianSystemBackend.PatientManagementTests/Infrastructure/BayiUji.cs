using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.PatientManagement.Infrastructure
{
    /// <summary>Ibu dan kasus persalinan obstetri yang sedang berlangsung, sasaran pendaftaran bayi uji.</summary>
    public sealed record KasusPersalinanUji(Guid IbuId, Guid KasusId);

    /// <summary>
    /// Data dan service pendaftaran bayi untuk uji `BE-PAT-MIG-003A`, dipakai SQLite maupun PostgreSQL
    /// sekali pakai. Tidak ada data pasien sungguhan.
    /// </summary>
    public static class BayiUji
    {
        public static readonly DateTime WaktuDibuat = new(2026, 10, 1, 8, 0, 0, DateTimeKind.Utc);

        public static async Task<KasusPersalinanUji> TambahIbuDanKasusAsync(
            ApplicationDbContext konteks,
            string kodeIbu,
            string mrnIbu)
        {
            var ibu = new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = kodeIbu,
                MedicalRecordNumber = mrnIbu,
                FullName = "Ny. Ibu Uji Persalinan",
                Gender = Gender.Female,
                CreateDateTime = WaktuDibuat,
                CreateBy = ReservasiHarness.AktorUji
            };

            konteks.Set<MstPatient>().Add(ibu);
            var kasus = BuatKasus(ibu.Id);
            konteks.Set<OprCase>().Add(kasus);
            await konteks.SaveChangesAsync();

            return new KasusPersalinanUji(ibu.Id, kasus.Id);
        }

        /// <summary>Kasus persalinan tambahan untuk ibu yang sama — kelahiran pada kasus berbeda.</summary>
        public static async Task<KasusPersalinanUji> TambahKasusAsync(ApplicationDbContext konteks, Guid ibuId)
        {
            var kasus = BuatKasus(ibuId);
            konteks.Set<OprCase>().Add(kasus);
            await konteks.SaveChangesAsync();

            return new KasusPersalinanUji(ibuId, kasus.Id);
        }

        public static CreateNewbornPatientRequest Permintaan(KasusPersalinanUji kasus, string tempatLahir = "RS Uji") => new()
        {
            MotherPatientId = kasus.IbuId,
            DeliveryCaseId = kasus.KasusId,
            BirthDate = DateTime.UtcNow.Date.AddDays(-1),
            BirthTime = new TimeSpan(8, 30, 0),
            Gender = Gender.Male,
            BirthPlace = tempatLahir,
            BirthWeightGram = 3100,
            BirthLengthCm = 49,
            DeliveryMethod = "Spontan"
        };

        public static NewbornPatientRegistrationService BuatLayanan(
            ApplicationDbContext konteks,
            PatientIdentifierAllocationService alokasi,
            IDbContextFactory<ApplicationDbContext> pabrikNumberSeries) =>
            new(konteks, new NumberSeriesAllocator(pabrikNumberSeries), alokasi);

        public static async Task<int> JumlahDeretAsync(ApplicationDbContext konteks) =>
            await konteks.NumNumberSeries.AsNoTracking().CountAsync();

        public static async Task<long?> NilaiDeretAsync(ApplicationDbContext konteks, string sequenceKey) =>
            await konteks.NumNumberSeries
                .AsNoTracking()
                .Where(x => x.SequenceKey == sequenceKey && x.ScopeKey == "GLOBAL")
                .Select(x => (long?)x.CurrentValue)
                .SingleOrDefaultAsync();

        public static async Task<int> JumlahUrutanKelahiranAsync(ApplicationDbContext konteks) =>
            await konteks.PatNewbornBirthSequences.AsNoTracking().CountAsync();

        private static OprCase BuatKasus(Guid ibuId) => new()
        {
            Id = Guid.NewGuid(),
            CaseNumber = "OPR-UJI-" + Guid.NewGuid().ToString("N")[..12],
            PatientId = ibuId,
            EncounterId = Guid.NewGuid(),
            RequesterDoctorId = Guid.NewGuid(),
            PrimarySurgeonId = Guid.NewGuid(),
            CaseType = OprCaseType.Elective,
            Priority = OprPriority.Routine,
            Status = OprCaseStatus.InProgress,
            SurgicalServiceType = OprSurgicalServiceType.Obstetric,
            Indication = "Persalinan uji",
            EstimatedMinutes = 60,
            RequestedAt = WaktuDibuat,
            Version = 1,
            CreateDateTime = WaktuDibuat,
            CreateBy = ReservasiHarness.AktorUji
        };
    }

    /// <summary>Tanda bahwa jalur bayi meminta nomor dari <see cref="NumberSeriesAllocator"/>.</summary>
    public sealed class NumberSeriesDipakaiException()
        : Exception("NumberSeriesAllocator dipanggil oleh jalur pendaftaran bayi.");

    /// <summary>
    /// Pabrik konteks <see cref="NumberSeriesAllocator"/> yang menolak dipakai. Pada SQLite, allocator
    /// platform tidak dapat membuka transaksi kedua pada koneksi yang sama, sehingga uji membuktikan
    /// <b>cabang</b> yang dipilih: dipakai → cabang <c>NumberSeries</c>, tidak dipakai → cabang lain.
    /// Jalur <c>NumberSeries</c> ujung-ke-ujung dibuktikan pada PostgreSQL sungguhan.
    /// </summary>
    public sealed class PabrikNumberSeriesTerlarang : IDbContextFactory<ApplicationDbContext>
    {
        public int JumlahPemakaian { get; private set; }

        public ApplicationDbContext CreateDbContext()
        {
            JumlahPemakaian++;
            throw new NumberSeriesDipakaiException();
        }
    }

    /// <summary>Menghitung perintah SQL yang dikirim sebuah konteks — bukti "tanpa akses basis data".</summary>
    public sealed class PenghitungPerintah : DbCommandInterceptor
    {
        private int _jumlah;

        public int Jumlah => _jumlah;

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Interlocked.Increment(ref _jumlah);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _jumlah);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<int> NonQueryExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            Interlocked.Increment(ref _jumlah);
            return base.NonQueryExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _jumlah);
            return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult<object> ScalarExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
        {
            Interlocked.Increment(ref _jumlah);
            return base.ScalarExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _jumlah);
            return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
