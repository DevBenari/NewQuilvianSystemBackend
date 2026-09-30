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
        private readonly LabExaminationService _labExaminationService;
        private readonly LabCitoTurnaroundPolicy _labCitoTurnaroundPolicy;

        public LabWorklistService(
            ApplicationDbContext dbContext,
            LabExaminationService labExaminationService,
            LabCitoTurnaroundPolicy labCitoTurnaroundPolicy)
        {
            _dbContext = dbContext;
            _labExaminationService = labExaminationService;
            _labCitoTurnaroundPolicy = labCitoTurnaroundPolicy;
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
                .ThenBy(x => x.Id)
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
                    // r22. Waktu, bukan status — lihat DTO.
                    ExaminedAt = x.ExaminedAt,
                    ResultEnteredAt = x.ResultEnteredAt,
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

            // INV-57 — batas yang sama dengan laporan waktu penyelesaian (BE-LAB-82).
            var batasWaktu = await _labCitoTurnaroundPolicy.GetLimitsAsync(
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
                .ThenBy(x => x.ExaminationId)
                .ToList();

            var items = terurut
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Halaman(pageNumber, pageSize, terurut.Count, items);
        }

        /// <summary>
        /// Antrean hasil Patologi Klinik yang <b>menunggu validasi</b> atau <b>menunggu rilis</b>
        /// (<c>LAB-API-v1</c> <c>r34</c> 29.4, <c>LAB-DEC-135</c> butir 2). Cito lebih dulu, lalu
        /// yang paling lama menunggu (<c>LAB-FE-006</c>).
        ///
        /// <para>
        /// <b>Tahap diturunkan dengan rumus yang sama dengan <c>resultStatus</c></b> —
        /// <see cref="LabExaminationService.HasResultStatus"/>: menunggu validasi = <c>Final</c>,
        /// menunggu rilis = <c>Validated</c>. Antrean yang menurunkan keadaannya sendiri dapat
        /// menampilkan hasil yang di halaman hasil terbaca lain.
        /// </para>
        ///
        /// <para>
        /// <b>Isinya hanya yang dapat ditindak.</b> Disiplin dibaca sama dengan tindakan validasi —
        /// order, lalu jatuh ke katalog pemeriksaan (<c>VAL-126</c>) — dan pemeriksaan batal, gugur,
        /// atau ber-order batal keluar (<c>VAL-127</c>). Order yang sudah ditandai selesai secara
        /// manual <b>tidak</b> dikeluarkan: hasil Final di dalamnya tetap menunggu disahkan.
        /// </para>
        /// </summary>
        /// <exception cref="LabExaminationValidationException"><c>VAL-139</c> — tahap kosong atau tidak sah.</exception>
        public async Task<PagedResult<LabValidationQueueItemResponse>> GetValidationQueueAsync(
            LabValidationQueueQuery query,
            CancellationToken cancellationToken = default)
        {
            // VAL-139. Hanya nama tahap; angka enum tidak diterima.
            var stageText = query.Stage?.Trim();
            if (string.IsNullOrEmpty(stageText) ||
                int.TryParse(stageText, out _) ||
                !Enum.TryParse<LabValidationQueueStage>(stageText, ignoreCase: true, out var stage) ||
                !Enum.IsDefined(stage))
            {
                throw new LabExaminationValidationException(
                    "Tahap antrean wajib dipilih: menunggu validasi atau menunggu rilis.");
            }

            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            var menungguValidasi = stage == LabValidationQueueStage.AwaitingValidation;

            var source = _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ExaminationStatus != LabExaminationStatus.Voided &&
                    x.ExaminationStatus != LabExaminationStatus.Cancelled &&
                    x.LabOrder != null &&
                    !x.LabOrder.IsDelete &&
                    x.LabOrder.OrderStatus != LabOrderStatus.Cancelled &&
                    (x.LabOrder.Discipline == LabDiscipline.ClinicalPathology ||
                     (x.LabOrder.Discipline == null && x.Procedure != null &&
                      x.Procedure.LabDiscipline == LabDiscipline.ClinicalPathology)))
                .Where(LabExaminationService.HasResultStatus(
                    menungguValidasi ? LabResultStatus.Final : LabResultStatus.Validated));

            if (query.OnlyCito == true)
                source = source.Where(x => x.Urgency == LabExaminationUrgency.Cito);

            // Penyaring disiplin daftar kerja sengaja TIDAK dipakai — r34 menyatakannya diabaikan.
            source = TerapkanPencarian(source, query.Search);

            var totalData = await source.CountAsync(cancellationToken);

            var terurut = menungguValidasi
                ? source.OrderByDescending(x => x.Urgency).ThenBy(x => x.FinalizedAt)
                : source.OrderByDescending(x => x.Urgency).ThenBy(x => x.ValidatedAt);

            var rows = await terurut
                .ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.LabOrderId,
                    OrderNumber = x.LabOrder!.OrderNumber,
                    EncounterId = x.LabOrder!.EncounterId,
                    PatientId = x.LabOrder!.Encounter != null ? (Guid?)x.LabOrder.Encounter.PatientId : null,
                    ProcedureName = x.ProcedureNameSnapshot ?? (x.Procedure != null ? x.Procedure.ProcedureName : null),
                    x.Urgency,
                    x.ResultEnteredAt,
                    x.FinalizedAt,
                    x.ValidatedAt,
                    x.ValidatedByUserId,
                    x.ReleasedAt
                })
                .ToListAsync(cancellationToken);

            if (rows.Count == 0)
            {
                return Halaman(pageNumber, pageSize, totalData, new List<LabValidationQueueItemResponse>());
            }

            // Pasien, nama pemvalidasi, dan penanda — masing-masing SATU kueri untuk seluruh halaman.
            var patientIds = rows.Where(x => x.PatientId.HasValue).Select(x => x.PatientId!.Value).Distinct().ToList();
            var pasien = await _dbContext.MstPatients
                .AsNoTracking()
                .Where(p => patientIds.Contains(p.Id))
                .Select(p => new { p.Id, p.FullName, p.MedicalRecordNumber })
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            var validatorIds = rows.Where(x => x.ValidatedByUserId.HasValue).Select(x => x.ValidatedByUserId!.Value).Distinct().ToList();
            var namaPemvalidasi = validatorIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => validatorIds.Contains(u.Id))
                    .Select(u => new { u.Id, Name = u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode })
                    .ToDictionaryAsync(x => x.Id, x => (string?)x.Name, cancellationToken);

            var penanda = await _labExaminationService.ResolveReferenceFlagsByIdAsync(
                rows.Select(x => x.Id).ToList(), cancellationToken);

            var items = rows.Select(x =>
            {
                var p = x.PatientId is Guid pid && pasien.TryGetValue(pid, out var ketemu) ? ketemu : null;

                return new LabValidationQueueItemResponse
                {
                    ExaminationId = x.Id,
                    LabOrderId = x.LabOrderId,
                    OrderNumber = x.OrderNumber,
                    EncounterId = x.EncounterId,
                    PatientName = p?.FullName,
                    MedicalRecordNumber = p?.MedicalRecordNumber,
                    ProcedureName = x.ProcedureName,
                    Urgency = x.Urgency.ToString(),
                    ResultStatus = LabExaminationService
                        .DeriveResultStatus(x.ReleasedAt, x.ValidatedAt, x.FinalizedAt, x.ResultEnteredAt)
                        .ToString(),
                    ReferenceFlag = penanda.TryGetValue(x.Id, out var flag) ? flag.ToString() : null,
                    FinalizedAt = x.FinalizedAt,
                    ValidatedAt = x.ValidatedAt,
                    ValidatedByName = x.ValidatedByUserId is Guid vid && namaPemvalidasi.TryGetValue(vid, out var nama) ? nama : null,
                    WaitingSince = menungguValidasi ? x.FinalizedAt : x.ValidatedAt
                };
            }).ToList();

            return Halaman(pageNumber, pageSize, totalData, items);
        }

        // =================================================================
        // Pembantu
        // =================================================================

        /// <summary>
        /// Pemeriksaan yang pekerjaannya belum selesai.
        ///
        /// Yang dikeluarkan: pemeriksaan yang sudah gugur atau dibatalkan, pemeriksaan yang
        /// pesanannya sudah selesai atau dibatalkan, dan — sejak <c>BE-LAB-77</c> — pemeriksaan
        /// yang hasilnya sudah <b>dirilis</b>. Wadah yang ditolak tidak perlu disebut tersendiri —
        /// menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya (<c>AC-36</c>),
        /// sehingga keduanya sudah tersaring lewat status pemeriksaannya.
        ///
        /// <b>Kenapa yang dirilis keluar</b> (<c>AC-17</c>, <c>r34</c> 29.8): pemeriksaan yang
        /// sudah dirilis sudah selesai dikerjakan. Tanpa penyaring ini, Kalium cito yang dirilis
        /// tepat waktu tetap tercatat terlambat sampai seseorang menandai ordernya selesai.
        /// </summary>
        private IQueryable<LabExamination> BelumSelesai() =>
            _dbContext.LabExaminations
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ReleasedAt == null &&
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

            return TerapkanPencarian(source, query.Search);
        }

        /// <summary>
        /// Pencarian bebas pada kode dan nama pemeriksaan serta barcode wadah — satu aturan bagi
        /// daftar kerja, daftar pantau cito, dan antrean validasi.
        /// </summary>
        private static IQueryable<LabExamination> TerapkanPencarian(
            IQueryable<LabExamination> source,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return source;
            }

            var kata = search.Trim();

            return source.Where(x =>
                (x.ProcedureCodeSnapshot != null && x.ProcedureCodeSnapshot.Contains(kata)) ||
                (x.ProcedureNameSnapshot != null && x.ProcedureNameSnapshot.Contains(kata)) ||
                (x.Specimen != null && x.Specimen.SpecimenBarcode.Contains(kata)));
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
