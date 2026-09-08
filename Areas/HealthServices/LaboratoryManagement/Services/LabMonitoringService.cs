using Microsoft.EntityFrameworkCore;
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
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabMonitoringItemResponse
                {
                    LabOrderId = x.Id,
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
                    CreateDateTime = x.CreateDateTime
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

            if (!string.IsNullOrWhiteSpace(query.EncounterNumber))
            {
                var nomor = query.EncounterNumber.Trim();

                source = source.Where(x =>
                    x.Encounter != null && x.Encounter.EncounterNumber.Contains(nomor));
            }

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
