using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Daftar kerja dan daftar pantau keterlambatan cito (<c>FR-04.1</c> .. <c>FR-04.4</c>,
    /// <c>LAB-DEC-013</c>).
    ///
    /// <b>Tidak ada tabel daftar kerja, dan itu keputusan.</b> Seluruh isinya diturunkan dari
    /// pesanan, wadah, dan pemeriksaan yang sudah ada (<c>FR-04.4</c>). Menyimpannya sebagai
    /// tabel tersendiri akan menciptakan sumber kebenaran kedua: begitu ada satu jalur yang lupa
    /// memperbaruinya, petugas melihat daftar yang tidak lagi sama dengan keadaan sebenarnya —
    /// dan kesalahan seperti itu tidak menghasilkan pesan galat apa pun.
    ///
    /// Service ini <b>hanya membaca</b>. Tidak ada satu pun jalur di sini yang mengubah data.
    /// </summary>
    public class LabWorklistService
    {
        private readonly ApplicationDbContext _dbContext;

        public LabWorklistService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Pekerjaan yang belum selesai, cito di urutan atas (<c>AC-10</c>, <c>AC-39</c>).
        ///
        /// Urutannya tiga tingkat: kesegeraan lebih dulu, lalu waktu pesanan masuk, lalu waktu
        /// pemeriksaan dibuat. Tingkat kedua itulah yang membuat empat belas pesanan biasa pukul
        /// 10.00 tetap berada di bawah satu pesanan cito pukul 10.05, sementara dua pesanan cito
        /// tetap urut menurut waktu masuknya sendiri.
        /// </summary>
        public async Task<PagedResult<LabWorklistItemResponse>> GetPendingAsync(
            LabWorklistPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = BelumSelesai();

            if (query.OnlyCito == true)
                source = source.Where(x => x.Urgency == LabExaminationUrgency.Cito);

            source = TerapkanPenyaringBersama(source, query);

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderByDescending(x => x.Urgency)
                .ThenBy(x => x.LabOrder != null ? x.LabOrder.RequestedAt : null)
                .ThenBy(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabWorklistItemResponse
                {
                    ExaminationId = x.Id,
                    LabOrderId = x.LabOrderId,
                    SpecimenId = x.SpecimenId,
                    SpecimenBarcode = x.Specimen != null ? x.Specimen.SpecimenBarcode : null,
                    EncounterId = x.LabOrder != null ? x.LabOrder.EncounterId : Guid.Empty,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.ProcedureCodeSnapshot,
                    ProcedureName = x.ProcedureNameSnapshot,
                    Discipline = x.LabOrder != null && x.LabOrder.Discipline != null
                        ? x.LabOrder.Discipline.ToString()
                        : null,
                    Urgency = x.Urgency.ToString(),
                    UrgencyMarkedAt = x.UrgencyMarkedAt,
                    IsDuplo = x.IsDuplo,
                    ExaminationStatus = x.ExaminationStatus.ToString(),
                    SpecimenStatus = x.Specimen != null ? x.Specimen.SpecimenStatus.ToString() : string.Empty,
                    RequestedAt = x.LabOrder != null ? x.LabOrder.RequestedAt : null,
                    ChargeEligibleAt = x.ChargeEligibleAt
                })
                .ToListAsync(cancellationToken);

            return Halaman(pageNumber, pageSize, totalData, items);
        }

        /// <summary>
        /// Pesanan cito yang melewati batas waktunya (<c>AC-17</c>, <c>VAL-39</c>).
        ///
        /// Perhitungannya dilakukan setelah baris ditarik ke memori, bukan di dalam kueri.
        /// Alasannya bukan kemalasan: batas waktu berbeda-beda per jenis pemeriksaan, sehingga
        /// penjumlahan waktu di dalam SQL menjadi aritmetika tanggal yang berbeda bentuk pada
        /// setiap provider. Yang ditarik hanya pemeriksaan cito yang wadahnya sudah layak dan
        /// pekerjaannya belum selesai — himpunan yang secara wajar berukuran kecil.
        /// </summary>
        public async Task<PagedResult<LabCitoOverdueResponse>> GetCitoOverdueAsync(
            LabWorklistPagedQuery query,
            DateTime? asOf = null,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            var sekarang = asOf ?? DateTime.UtcNow;

            var source = TerapkanPenyaringBersama(
                BelumSelesai().Where(x =>
                    x.Urgency == LabExaminationUrgency.Cito &&
                    x.ChargeEligibleAt != null),
                query);

            var kandidat = await source
                .Select(x => new Kandidat(
                    x.Id,
                    x.LabOrderId,
                    x.SpecimenId,
                    x.Specimen != null ? x.Specimen.SpecimenBarcode : null,
                    x.LabOrder != null ? x.LabOrder.EncounterId : Guid.Empty,
                    x.ProcedureId,
                    x.ProcedureCodeSnapshot,
                    x.ProcedureNameSnapshot,
                    x.LabOrder != null && x.LabOrder.Discipline != null
                        ? x.LabOrder.Discipline.ToString()
                        : null,
                    x.LabOrder != null ? x.LabOrder.RequestedAt : null,
                    x.ChargeEligibleAt!.Value))
                .ToListAsync(cancellationToken);

            if (kandidat.Count == 0)
                return Halaman(pageNumber, pageSize, 0, new List<LabCitoOverdueResponse>());

            var batasWaktu = await BatasWaktuCitoAsync(
                kandidat.Select(x => x.ProcedureId).Distinct().ToList(),
                cancellationToken);

            var semua = new List<LabCitoOverdueResponse>();

            foreach (var item in kandidat)
            {
                batasWaktu.TryGetValue(item.ProcedureId, out var menit);

                // VAL-39. Tanpa batas waktu tidak ada yang dapat dilewati, sehingga baris ini
                // tidak dianggap terlambat — tetapi tetap ditampilkan supaya kepala instalasi
                // tahu ada data induk yang belum lengkap.
                if (menit == null)
                {
                    semua.Add(Baris(item, null, null, null, hasTurnaround: false,
                        note: "Batas waktu cito untuk pemeriksaan ini belum diatur."));
                    continue;
                }

                var tenggat = item.ChargeEligibleAt.AddMinutes(menit.Value);

                if (sekarang <= tenggat)
                    continue;

                var kelebihan = (int)Math.Floor((sekarang - tenggat).TotalMinutes);

                semua.Add(Baris(item, menit, tenggat, kelebihan, hasTurnaround: true, note: null));
            }

            // Yang paling lama terlambat lebih dulu; baris tanpa batas waktu berada di bawah
            // seluruh keterlambatan yang sesungguhnya, lalu diurutkan menurut waktu masuk.
            var terurut = semua
                .OrderByDescending(x => x.OverdueMinutes ?? -1)
                .ThenBy(x => x.RequestedAt)
                .ThenBy(x => x.ChargeEligibleAt)
                .ToList();

            var items = terurut
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Halaman(pageNumber, pageSize, terurut.Count, items);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        /// <summary>
        /// Pemeriksaan yang pekerjaannya belum selesai.
        ///
        /// Yang dikeluarkan: pemeriksaan yang sudah gugur atau dibatalkan, dan pemeriksaan yang
        /// pesanannya sudah selesai atau dibatalkan. Wadah yang ditolak tidak perlu disebut
        /// tersendiri — menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya
        /// (<c>AC-36</c>), sehingga keduanya sudah tersaring lewat status pemeriksaannya.
        /// </summary>
        private IQueryable<LabExamination> BelumSelesai() =>
            _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ExaminationStatus != LabExaminationStatus.Voided &&
                    x.ExaminationStatus != LabExaminationStatus.Cancelled &&
                    x.LabOrder != null &&
                    !x.LabOrder.IsDelete &&
                    x.LabOrder.OrderStatus != LabOrderStatus.Completed &&
                    x.LabOrder.OrderStatus != LabOrderStatus.Cancelled);

        private static IQueryable<LabExamination> TerapkanPenyaringBersama(
            IQueryable<LabExamination> source,
            LabWorklistPagedQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.Discipline) &&
                Enum.TryParse<LabDiscipline>(query.Discipline.Trim(), ignoreCase: true, out var discipline))
            {
                source = source.Where(x => x.LabOrder != null && x.LabOrder.Discipline == discipline);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    (x.ProcedureCodeSnapshot != null && x.ProcedureCodeSnapshot.Contains(search)) ||
                    (x.ProcedureNameSnapshot != null && x.ProcedureNameSnapshot.Contains(search)) ||
                    (x.Specimen != null && x.Specimen.SpecimenBarcode.Contains(search)));
            }

            return source;
        }

        /// <summary>
        /// Batas waktu cito yang berlaku bagi setiap jenis pemeriksaan.
        ///
        /// <b>Satu turunan yang perlu diketahui.</b> <c>LabValueBound</c> dipecah menurut jenis
        /// kelamin dan kelompok umur untuk keperluan batas nilai, sementara batas waktu cito
        /// adalah janji layanan yang tidak bergantung pada keduanya. Blueprint tidak menyebut
        /// baris mana yang berlaku, sehingga yang dipakai adalah baris umum — <c>All</c> tanpa
        /// kelompok umur — dan bila baris itu tidak mengisinya, nilai terkecil di antara baris
        /// aktif lainnya. Memilih yang terkecil berarti memilih janji yang paling ketat, bukan
        /// yang paling longgar.
        /// </summary>
        private async Task<Dictionary<Guid, int?>> BatasWaktuCitoAsync(
            IReadOnlyList<Guid> procedureIds,
            CancellationToken cancellationToken)
        {
            var bounds = await _dbContext.LabValueBounds
                .AsNoTracking()
                .Where(x =>
                    procedureIds.Contains(x.ProcedureId) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.CitoTurnaroundMinutes != null)
                .Select(x => new
                {
                    x.ProcedureId,
                    x.GenderScope,
                    x.AgeCategoryId,
                    x.CitoTurnaroundMinutes
                })
                .ToListAsync(cancellationToken);

            var hasil = new Dictionary<Guid, int?>();

            foreach (var procedureId in procedureIds)
            {
                var milikProcedure = bounds.Where(x => x.ProcedureId == procedureId).ToList();

                if (milikProcedure.Count == 0)
                {
                    hasil[procedureId] = null;
                    continue;
                }

                var umum = milikProcedure.FirstOrDefault(x =>
                    x.GenderScope == LabGenderScope.All && x.AgeCategoryId == null);

                hasil[procedureId] = umum?.CitoTurnaroundMinutes
                    ?? milikProcedure.Min(x => x.CitoTurnaroundMinutes);
            }

            return hasil;
        }

        private static LabCitoOverdueResponse Baris(
            Kandidat item,
            int? menit,
            DateTime? tenggat,
            int? kelebihan,
            bool hasTurnaround,
            string? note) =>
            new()
            {
                ExaminationId = item.ExaminationId,
                LabOrderId = item.LabOrderId,
                SpecimenId = item.SpecimenId,
                SpecimenBarcode = item.SpecimenBarcode,
                EncounterId = item.EncounterId,
                ProcedureId = item.ProcedureId,
                ProcedureCode = item.ProcedureCode,
                ProcedureName = item.ProcedureName,
                Discipline = item.Discipline,
                RequestedAt = item.RequestedAt,
                ChargeEligibleAt = item.ChargeEligibleAt,
                CitoTurnaroundMinutes = menit,
                DeadlineAt = tenggat,
                OverdueMinutes = kelebihan,
                HasCitoTurnaround = hasTurnaround,
                Note = note
            };

        private static PagedResult<T> Halaman<T>(int pageNumber, int pageSize, int totalData, List<T> items) =>
            new()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

        private sealed record Kandidat(
            Guid ExaminationId,
            Guid LabOrderId,
            Guid SpecimenId,
            string? SpecimenBarcode,
            Guid EncounterId,
            Guid ProcedureId,
            string? ProcedureCode,
            string? ProcedureName,
            string? Discipline,
            DateTime? RequestedAt,
            DateTime ChargeEligibleAt);
    }
}
