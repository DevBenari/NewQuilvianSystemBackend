using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Tiga daftar pantau sejajar — Patologi Klinik, Patologi Anatomi, dan Mikrobiologi
    /// (<c>FR-10.1</c> .. <c>FR-10.3</c>, <c>LAB-DEC-025</c>).
    ///
    /// <b>Tiga jalur, satu perilaku.</b> Ketiganya memakai penyaring, proyeksi, dan pengurutan
    /// yang sama persis; yang membedakan hanya disiplin yang dikunci pemanggilnya. Itu keputusan
    /// sadar, bukan duplikasi: bukti lapangan menunjukkan laboratorium memakai tiga daftar
    /// sebagai tiga menu berbeda karena petugasnya pun berbeda. Menyatukannya menjadi satu jalur
    /// berpenyaring akan memaksa petugas memilih disiplin setiap kali membuka layar.
    ///
    /// Karena perilakunya satu, kodenya pun satu — <see cref="GetByDisciplineAsync"/>. Yang tiga
    /// adalah jalurnya di controller, bukan logikanya di sini.
    ///
    /// Service ini <b>hanya membaca</b>, dan seluruh isinya diturunkan dari
    /// <c>LabOrder.Discipline</c>. Tidak ada tabel monitoring.
    /// </summary>
    public class LabMonitoringService
    {
        private readonly ApplicationDbContext _dbContext;

        public LabMonitoringService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Daftar pantau satu disiplin (<c>AC-41</c>).
        ///
        /// Disiplin datang dari jalur yang dipanggil, bukan dari penyaring yang dikirim, sehingga
        /// tidak ada cara memanggil jalur Patologi Klinik lalu memperoleh pesanan Mikrobiologi.
        /// </summary>
        public async Task<PagedResult<LabMonitoringItemResponse>> GetByDisciplineAsync(
            LabDiscipline discipline,
            LabMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.Discipline == discipline);

            source = TerapkanPenyaring(source, query);

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderByDescending(x => x.RequestedAt ?? x.CreateDateTime)
                .ThenByDescending(x => x.CreateDateTime)
                .ThenBy(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabMonitoringItemResponse
                {
                    LabOrderId = x.Id,

                    OrderNumber = x.OrderNumber,
                    EncounterId = x.EncounterId,
                    EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
                    PatientId = x.Encounter != null ? x.Encounter.PatientId : null,
                    PatientName = x.Encounter == null
                        ? null
                        : _dbContext.MstPatients
                            .Where(p => p.Id == x.Encounter.PatientId)
                            .Select(p => p.FullName)
                            .FirstOrDefault(),
                    MedicalRecordNumber = x.Encounter == null
                        ? null
                        : _dbContext.MstPatients
                            .Where(p => p.Id == x.Encounter.PatientId)
                            .Select(p => p.MedicalRecordNumber)
                            .FirstOrDefault(),
                    // r19. Dibaca lewat sub-query ke MstPatient, cara yang sama dengan kedua
                    // ruas di atas. Nama enum, bukan angka — mengikuti OrderStatus,
                    // EncounterType, dan PaymentType pada DTO yang sama.
                    Gender = x.Encounter == null
                        ? null
                        : _dbContext.MstPatients
                            .Where(p => p.Id == x.Encounter.PatientId)
                            .Select(p => p.Gender.HasValue ? p.Gender.Value.ToString() : null)
                            .FirstOrDefault(),
                    // r20. Dipakai Label Goldar. Sub-query yang sama dengan Gender di atas.
                    BloodType = x.Encounter == null
                        ? null
                        : _dbContext.MstPatients
                            .Where(p => p.Id == x.Encounter.PatientId)
                            .Select(p => p.BloodType.ToString())
                            .FirstOrDefault(),
                    Discipline = discipline.ToString(),
                    OrderStatus = x.OrderStatus.ToString(),
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    RequestedAt = x.RequestedAt,
                    CompletedAt = x.CompletedAt,
                    EncounterType = x.Encounter != null ? x.Encounter.EncounterType.ToString() : null,
                    VisitType = x.Encounter != null ? x.Encounter.VisitType.ToString() : null,
                    ServiceUnitId = x.Encounter != null ? x.Encounter.ServiceUnitId : null,
                    RoomId = x.Encounter != null ? x.Encounter.RoomId : null,
                    PaymentType = x.Encounter != null ? x.Encounter.PaymentType.ToString() : null,
                    SpecimenCount = _dbContext.LabSpecimens
                        .Count(s => s.LabOrderId == x.Id && !s.IsDelete),
                    AcceptedSpecimenCount = _dbContext.LabSpecimens
                        .Count(s => s.LabOrderId == x.Id && !s.IsDelete &&
                                    s.SpecimenStatus == LabSpecimenStatus.Accepted),
                    ExaminationCount = _dbContext.LabExaminations
                        .Count(e => e.LabOrderId == x.Id && !e.IsDelete),
                    HasCito = _dbContext.LabExaminations
                        .Any(e => e.LabOrderId == x.Id && !e.IsDelete &&
                                  e.Urgency == LabExaminationUrgency.Cito),
                    CreateDateTime = x.CreateDateTime,

                    // LAB-API-v1 r14. Kedua nama diterjemahkan di dalam proyeksi yang sama,
                    // mengikuti cara PatientName dan MedicalRecordNumber di atas — bukan lewat
                    // pencarian per baris sesudahnya. Jalur terjemahannya sama persis dengan
                    // yang dipakai LabOrderService, supaya satu orang tidak terbaca dengan dua
                    // nama berbeda antar layar.
                    ConfirmedAt = x.ConfirmedAt,
                    ConfirmedByName = x.ConfirmedByUserId == null
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.ConfirmedByUserId)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    ExaminerDoctorName = x.ExaminerDoctorId == null
                        ? null
                        : _dbContext.Set<MstDoctor>()
                            .Where(d => d.Id == x.ExaminerDoctorId)
                            .Select(d => d.FullName)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabMonitoringItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Penyaring yang sama bagi ketiga jalur.
        ///
        /// Ditulis satu kali dan dipakai bertiga, supaya "penyaring identik" pada DoD benar-benar
        /// identik dan bukan tiga salinan yang lambat laun menyimpang satu sama lain.
        /// </summary>
        private IQueryable<LabOrder> TerapkanPenyaring(IQueryable<LabOrder> source, LabMonitoringQuery query)
        {
            if (query.PatientId.HasValue && query.PatientId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.PatientId == query.PatientId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.MedicalRecordNumber))
            {
                var nomor = query.MedicalRecordNumber.Trim();

                source = source.Where(x =>
                    x.Encounter != null &&
                    _dbContext.MstPatients.Any(p =>
                        p.Id == x.Encounter.PatientId && p.MedicalRecordNumber.Contains(nomor)));
            }

            // NIK (r18). Dibaca lewat sub-query ke MstPatient — cara yang sama dengan
            // MedicalRecordNumber di atas, bukan lewat navigation property baru. BR-50 butir 1
            // melarang mengarang kolom; IdentityNumber memang sudah ada pada data induk pasien.
            if (!string.IsNullOrWhiteSpace(query.IdentityNumber))
            {
                var nik = query.IdentityNumber.Trim();

                source = source.Where(x =>
                    x.Encounter != null &&
                    _dbContext.MstPatients.Any(p =>
                        p.Id == x.Encounter.PatientId &&
                        p.IdentityNumber != null &&
                        p.IdentityNumber.Contains(nik)));
            }

            if (!string.IsNullOrWhiteSpace(query.EncounterNumber))
            {
                var nomor = query.EncounterNumber.Trim();

                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.EncounterNumber.Contains(nomor));
            }

            // Kategori Periode (r18). Kosong berarti OrderDate — cabang yang persis sama dengan
            // sebelum ruas ini ada, sehingga pemanggil lama tidak berubah perilakunya.
            if (query.DateCategory == LabDateCategory.SamplingDate)
            {
                var mulai = query.StartDate;
                var sampai = query.EndDate;

                if (mulai.HasValue || sampai.HasValue)
                {
                    // Satu pesanan dapat memiliki beberapa wadah dengan waktu pengambilan
                    // berbeda; yang dicari keberadaan salah satunya di dalam rentang — pola yang
                    // sama dengan penyaring status wadah di bawah.
                    //
                    // CollectedAt kosong TIDAK PERNAH cocok, dan tidak jatuh-tempo ke
                    // CreateDateTime. Pertanyaannya "pesanan mana yang diambil sampelnya dalam
                    // rentang ini"; pesanan yang wadahnya belum pernah dinyatakan diambil tidak
                    // termasuk jawabannya, dan mensubstitusinya akan memunculkannya sebagai
                    // "diambil tanggal sekian" tanpa satu pun tanda di layar.
                    source = source.Where(x => _dbContext.LabSpecimens
                        .Any(s => s.LabOrderId == x.Id &&
                                  !s.IsDelete &&
                                  s.CollectedAt.HasValue &&
                                  (!mulai.HasValue || s.CollectedAt.Value >= mulai.Value) &&
                                  (!sampai.HasValue || s.CollectedAt.Value <= sampai.Value)));
                }
            }
            else if (query.DateCategory == LabDateCategory.ExaminationDate)
            {
                var mulai = query.StartDate;
                var sampai = query.EndDate;

                if (mulai.HasValue || sampai.HasValue)
                {
                    // r23. Aturan yang sama persis dengan SamplingDate di atas — satu pesanan
                    // dapat memuat beberapa pemeriksaan yang dikerjakan pada waktu berbeda,
                    // dan ExaminedAt kosong TIDAK PERNAH cocok.
                    //
                    // Nilai ini baru dicantumkan sesudah DUA syarat terpenuhi: kolomnya ada
                    // (r21), dan ada yang mengisinya (r21/r22). Kolom tanpa penulis adalah
                    // persis kesalahan BE-EXT-04.
                    source = source.Where(x => _dbContext.LabExaminations
                        .Any(e => e.LabOrderId == x.Id &&
                                  !e.IsDelete &&
                                  e.ExaminedAt.HasValue &&
                                  (!mulai.HasValue || e.ExaminedAt.Value >= mulai.Value) &&
                                  (!sampai.HasValue || e.ExaminedAt.Value <= sampai.Value)));
                }
            }
            else
            {
                if (query.StartDate.HasValue)
                {
                    var mulai = query.StartDate.Value;
                    source = source.Where(x => (x.RequestedAt ?? x.CreateDateTime) >= mulai);
                }

                if (query.EndDate.HasValue)
                {
                    var sampai = query.EndDate.Value;
                    source = source.Where(x => (x.RequestedAt ?? x.CreateDateTime) <= sampai);
                }
            }

            if (query.EncounterType.HasValue)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.EncounterType == query.EncounterType.Value);
            }

            if (query.VisitType.HasValue)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.VisitType == query.VisitType.Value);
            }

            if (query.ServiceUnitId.HasValue && query.ServiceUnitId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.ServiceUnitId == query.ServiceUnitId.Value);
            }

            if (query.RoomId.HasValue && query.RoomId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.RoomId == query.RoomId.Value);
            }

            if (query.PaymentType.HasValue)
            {
                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.PaymentType == query.PaymentType.Value);
            }

            if (query.OrderStatus.HasValue)
                source = source.Where(x => x.OrderStatus == query.OrderStatus.Value);

            // Satu pesanan dapat memiliki beberapa wadah dengan status berbeda, sehingga yang
            // dicari adalah keberadaan salah satunya — bukan status pesanan itu sendiri.
            if (query.SpecimenStatus.HasValue)
            {
                var status = query.SpecimenStatus.Value;

                source = source.Where(x => _dbContext.LabSpecimens
                    .Any(s => s.LabOrderId == x.Id && !s.IsDelete && s.SpecimenStatus == status));
            }

            if (query.OnlyCito == true)
            {
                source = source.Where(x => _dbContext.LabExaminations
                    .Any(e => e.LabOrderId == x.Id && !e.IsDelete &&
                              e.Urgency == LabExaminationUrgency.Cito));
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.Encounter != null &&
                    (x.Encounter.EncounterNumber.Contains(search) ||
                     _dbContext.MstPatients.Any(p =>
                         p.Id == x.Encounter.PatientId &&
                         (p.FullName.Contains(search) || p.MedicalRecordNumber.Contains(search)))));
            }

            return source;
        }
    }
}
