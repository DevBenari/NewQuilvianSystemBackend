using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Satu definisi hitung <b>beban permintaan</b> Laboratorium — pesanan dan pemeriksaan yang diminta
    /// dalam sebuah rentang waktu (<c>INV-59</c>, <c>LAB-DEC-242</c>, <c>LAB-DEC-246</c>,
    /// <c>02-backend-architecture.md</c> bagian 32).
    ///
    /// <para>
    /// <b>Dipindah, bukan ditulis ulang</b>, dari <c>LabDashboardService.GetYearlyAsync</c>
    /// (<c>r44</c> 39.8, <c>BE-LAB-96</c>). Beranda dan Statistik Pesanan wajib menghitung dengan
    /// definisi yang sama; salinan kedua akan bercabang tanpa satu galat pun.
    /// </para>
    ///
    /// <para>
    /// Setiap fungsi hanya <b>membentuk kueri</b> atas rentang UTC <c>[start, end)</c>; pemanggil yang
    /// mengelompokkan dan menjalankannya. Waktu acuan sebuah pesanan = <c>RequestedAt</c>, jatuh ke
    /// <c>CreateDateTime</c> bagi pesanan lama yang belum punya waktu diminta.
    /// </para>
    ///
    /// <para>
    /// <b>Sengaja tidak simetris</b> (<c>ARCH-GAP-LAB-15</c>): hitungan pesanan menyaring <c>Draft</c>,
    /// hitungan pemeriksaan dan permintaan tidak. Dipindah apa adanya — merapikannya mengubah angka
    /// Beranda tanpa persetujuan. Hari ini tidak ada jalur yang menulis <c>Draft</c>.
    /// </para>
    /// </summary>
    public class LabOrderDemandQuery
    {
        /// <summary>
        /// Disiplin sebuah pemeriksaan: disiplin pesanan, lalu disiplin katalog — sama dengan
        /// <c>VAL-126</c>. Kosong = belum terklasifikasi.
        /// </summary>
        public static readonly Expression<Func<LabExamination, LabDiscipline?>> ExaminationDiscipline =
            e => e.LabOrder!.Discipline ?? (e.Procedure != null ? e.Procedure.LabDiscipline : null);

        /// <summary>Disiplin sebuah permintaan, aturan yang sama dengan <see cref="ExaminationDiscipline"/>.</summary>
        public static readonly Expression<Func<LabOrderedProcedure, LabDiscipline?>> OrderedProcedureDiscipline =
            p => p.LabOrder!.Discipline ?? (p.Procedure != null ? p.Procedure.LabDiscipline : null);

        /// <summary>
        /// Disiplin sebuah pesanan bagi Statistik Pesanan (<c>BE-LAB-98</c>): disiplin pesanan, lalu
        /// disiplin katalog pemeriksaan utamanya — urutan yang sama dengan <c>VAL-126</c>. Beranda
        /// tidak merinci pesanan per disiplin, sehingga jumlah seluruh disiplin tetap sama dengan
        /// tren Beranda.
        /// </summary>
        public static readonly Expression<Func<LabOrder, LabDiscipline?>> OrderDiscipline =
            o => o.Discipline ?? (o.Procedure != null ? o.Procedure.LabDiscipline : null);

        /// <summary>
        /// Satu baris beban permintaan beserta kunjungannya — disusun dari
        /// <see cref="ExaminationDiscipline"/> apa adanya (bukan salinan), ditambah jenis kunjungan dan
        /// unit layanan kunjungan pesanannya (<c>LAB-DEC-243</c>).
        /// </summary>
        public static readonly Expression<Func<LabExamination, LabDemandRow>> ExaminationDemand =
            Susun(ExaminationDiscipline, e => e.LabOrder!.Encounter);

        /// <summary>Sama dengan <see cref="ExaminationDemand"/> bagi permintaan yang belum masuk wadah.</summary>
        public static readonly Expression<Func<LabOrderedProcedure, LabDemandRow>> OrderedProcedureDemand =
            Susun(OrderedProcedureDiscipline, p => p.LabOrder!.Encounter);

        /// <summary>Sama dengan <see cref="ExaminationDemand"/> bagi pesanan.</summary>
        public static readonly Expression<Func<LabOrder, LabDemandRow>> OrderDemand =
            Susun(OrderDiscipline, o => o.Encounter);

        private readonly ApplicationDbContext _dbContext;

        public LabOrderDemandQuery(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Pesanan yang dihitung: bukan <c>Cancelled</c>, bukan <c>Draft</c>, tidak dihapus, diminta
        /// dalam rentang.
        /// </summary>
        public IQueryable<LabOrder> CountedOrders(DateTime startUtc, DateTime endUtc) =>
            _dbContext.LabOrders
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.OrderStatus != LabOrderStatus.Cancelled &&
                    x.OrderStatus != LabOrderStatus.Draft &&
                    (x.RequestedAt ?? x.CreateDateTime) >= startUtc &&
                    (x.RequestedAt ?? x.CreateDateTime) < endUtc);

        /// <summary>
        /// Pemeriksaan yang dihitung: tidak gugur, tidak batal, tidak dihapus, pada pesanan yang
        /// <b>tidak</b> dibatalkan dan diminta dalam rentang. Membatalkan pesanan tidak ikut membatalkan
        /// pemeriksaannya, sehingga syarat pesanan wajib ditulis di sini (<c>ARCH-GAP-LAB-16</c>).
        /// </summary>
        public IQueryable<LabExamination> CountedExaminations(DateTime startUtc, DateTime endUtc) =>
            _dbContext.LabExaminations
                .AsNoTracking()
                .Where(e =>
                    !e.IsDelete &&
                    e.ExaminationStatus != LabExaminationStatus.Voided &&
                    e.ExaminationStatus != LabExaminationStatus.Cancelled &&
                    e.LabOrder != null &&
                    !e.LabOrder.IsDelete &&
                    e.LabOrder.OrderStatus != LabOrderStatus.Cancelled &&
                    (e.LabOrder.RequestedAt ?? e.LabOrder.CreateDateTime) >= startUtc &&
                    (e.LabOrder.RequestedAt ?? e.LabOrder.CreateDateTime) < endUtc);

        /// <summary>
        /// Permintaan yang belum masuk wadah (<c>Ordered</c>) pada pesanan yang tidak dibatalkan dan
        /// diminta dalam rentang. Permintaan <c>Fulfilled</c> tidak dihitung lagi — ia sudah terwakili
        /// barisnya di pemeriksaan.
        /// </summary>
        public IQueryable<LabOrderedProcedure> PendingOrderedProcedures(DateTime startUtc, DateTime endUtc) =>
            _dbContext.LabOrderedProcedures
                .AsNoTracking()
                .Where(p =>
                    !p.IsDelete &&
                    p.OrderedStatus == LabOrderedProcedureStatus.Ordered &&
                    p.LabOrder != null &&
                    !p.LabOrder.IsDelete &&
                    p.LabOrder.OrderStatus != LabOrderStatus.Cancelled &&
                    (p.LabOrder.RequestedAt ?? p.LabOrder.CreateDateTime) >= startUtc &&
                    (p.LabOrder.RequestedAt ?? p.LabOrder.CreateDateTime) < endUtc);

        /// <summary>
        /// Membentuk <c>x => new LabDemandRow { Discipline = …, EncounterType = …, ServiceUnitId = … }</c>
        /// dengan <b>menanam badan ekspresi disiplin yang sudah ada</b>, supaya aturan disiplin tidak
        /// pernah ditulis dua kali. Hasilnya tetap pohon ekspresi biasa yang diterjemahkan EF ke SQL.
        /// </summary>
        private static Expression<Func<T, LabDemandRow>> Susun<T>(
            Expression<Func<T, LabDiscipline?>> discipline,
            Expression<Func<T, RegPatientEncounter?>> encounter)
        {
            var x = Expression.Parameter(typeof(T), "x");
            var disiplin = new GantiParameter(discipline.Parameters[0], x).Visit(discipline.Body)!;
            var kunjungan = new GantiParameter(encounter.Parameters[0], x).Visit(encounter.Body)!;

            var body = Expression.MemberInit(
                Expression.New(typeof(LabDemandRow)),
                Expression.Bind(typeof(LabDemandRow).GetProperty(nameof(LabDemandRow.Discipline))!, disiplin),
                Expression.Bind(
                    typeof(LabDemandRow).GetProperty(nameof(LabDemandRow.EncounterType))!,
                    Expression.Convert(
                        Expression.Property(kunjungan, nameof(RegPatientEncounter.EncounterType)),
                        typeof(EncounterType?))),
                Expression.Bind(
                    typeof(LabDemandRow).GetProperty(nameof(LabDemandRow.ServiceUnitId))!,
                    Expression.Convert(
                        Expression.Property(kunjungan, nameof(RegPatientEncounter.ServiceUnitId)),
                        typeof(Guid?))));

            return Expression.Lambda<Func<T, LabDemandRow>>(body, x);
        }

        private sealed class GantiParameter(ParameterExpression lama, Expression baru) : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node) =>
                node == lama ? baru : base.VisitParameter(node);
        }
    }

    /// <summary>
    /// Satu baris beban permintaan: disiplin, jenis kunjungan, dan unit layanan kunjungan. Proyeksi
    /// baca saja — bukan entity, tidak disimpan.
    /// </summary>
    public sealed class LabDemandRow
    {
        public LabDiscipline? Discipline { get; set; }

        public EncounterType? EncounterType { get; set; }

        public Guid? ServiceUnitId { get; set; }
    }
}
