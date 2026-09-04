using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Katalog pemeriksaan, harga, dan cakupan penjamin — <b>baca saja</b>
    /// (<c>FR-09.1</c> .. <c>FR-09.5</c>, <c>LAB-DEC-033</c>, <c>LAB-DEC-036</c>).
    ///
    /// <b>Nol tabel milik Laboratorium.</b> Seluruh isinya berasal dari data induk milik Master
    /// Data: <c>MstProcedure</c>, <c>MstTariff</c>, dan <c>MstInsuranceTariff</c>. Laboratorium
    /// tidak membuat tabel tarif sendiri, dan tidak menyalin harga ke mana pun dari sini —
    /// salinan tarif hanya terjadi saat pemeriksaan benar-benar dipesan, dan itu pekerjaan
    /// <c>LabExaminationService</c>.
    ///
    /// Tidak ada satu pun jalur ubah di berkas ini. Harga diubah lewat modul Data Induk
    /// (<c>VAL-50</c>).
    /// </summary>
    public class LabCatalogService
    {
        private readonly ApplicationDbContext _dbContext;

        public LabCatalogService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Daftar pemeriksaan laboratorium yang dapat dipesan, disaring per disiplin
        /// (<c>AC-43</c>).
        ///
        /// Pemeriksaan yang belum digolongkan disiplinnya <b>tetap ditampilkan</b> ketika
        /// penyaring disiplin tidak dikirim. Menyembunyikannya akan membuat katalog tampak
        /// kosong pada rumah sakit yang penggolongannya belum diisi, dan petugas menyimpulkan
        /// sistemnya rusak.
        /// </summary>
        public async Task<PagedResult<LabCatalogItemResponse>> GetExaminationsAsync(
            LabCatalogQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);
            var now = DateTime.UtcNow;

            var source = _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x => x.IsLaboratory && x.IsActive && !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(query.Discipline) &&
                Enum.TryParse<LabDiscipline>(query.Discipline.Trim(), ignoreCase: true, out var discipline))
            {
                source = source.Where(x => x.LabDiscipline == discipline);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.ProcedureCode.Contains(search) || x.ProcedureName.Contains(search));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var procedures = await source
                .OrderBy(x => x.ProcedureName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.Id,
                    x.ProcedureCode,
                    x.ProcedureName,
                    x.LabDiscipline,
                    x.IsCoveredByInsuranceDefault
                })
                .ToListAsync(cancellationToken);

            var procedureIds = procedures.Select(x => x.Id).ToList();
            var tarif = await TarifBerlakuAsync(procedureIds, now, cancellationToken);

            var kontrak = query.InsuranceProviderId.HasValue && query.InsuranceProviderId.Value != Guid.Empty
                ? await KontrakPenjaminAsync(
                    tarif.Values.Select(x => x.TariffId).ToList(),
                    query.InsuranceProviderId.Value,
                    query.PatientClassId,
                    now,
                    cancellationToken)
                : new Dictionary<Guid, KontrakPenjamin>();

            var memintaPenjamin = query.InsuranceProviderId.HasValue &&
                                  query.InsuranceProviderId.Value != Guid.Empty;

            var items = new List<LabCatalogItemResponse>();

            foreach (var procedure in procedures)
            {
                tarif.TryGetValue(procedure.Id, out var berlaku);

                KontrakPenjamin? kontrakBaris = null;

                if (berlaku != null && kontrak.TryGetValue(berlaku.TariffId, out var ditemukan))
                    kontrakBaris = ditemukan;

                items.Add(new LabCatalogItemResponse
                {
                    ProcedureId = procedure.Id,
                    ProcedureCode = procedure.ProcedureCode,
                    ProcedureName = procedure.ProcedureName,
                    Discipline = procedure.LabDiscipline?.ToString(),
                    UnitPrice = berlaku?.NormalPrice,
                    TariffId = berlaku?.TariffId,
                    TariffCode = berlaku?.TariffCode,
                    IsCoveredByInsuranceDefault = procedure.IsCoveredByInsuranceDefault,
                    ContractPrice = kontrakBaris?.ContractPrice,
                    IsCoveredByThisInsurance = memintaPenjamin ? kontrakBaris != null : null
                });
            }

            return new PagedResult<LabCatalogItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Harga berlaku satu pemeriksaan beserta status cakupan penjaminnya
        /// (<c>AC-43</c>, <c>FR-09.3</c>).
        ///
        /// Membacanya <b>tidak</b> membentuk baris tagihan apa pun. Itulah batas yang dijaga
        /// <c>AC-43</c>: melihat harga bukan memesan, dan memesan bukan menagih.
        /// </summary>
        public async Task<LabPriceResponse> GetPriceAsync(
            Guid procedureId,
            LabPriceQuery query,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .Where(x => x.Id == procedureId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ProcedureCode,
                    x.ProcedureName,
                    x.LabDiscipline,
                    x.IsLaboratory,
                    x.IsCoveredByInsuranceDefault
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException("Jenis pemeriksaan tidak ditemukan.");

            if (!procedure.IsLaboratory)
                throw new LabCatalogValidationException("Tindakan yang dipilih bukan pemeriksaan laboratorium.");

            var tarif = await TarifBerlakuAsync(new List<Guid> { procedure.Id }, now, cancellationToken);
            tarif.TryGetValue(procedure.Id, out var berlaku);

            var hasil = new LabPriceResponse
            {
                ProcedureId = procedure.Id,
                ProcedureCode = procedure.ProcedureCode,
                ProcedureName = procedure.ProcedureName,
                Discipline = procedure.LabDiscipline?.ToString(),
                HospitalPrice = berlaku?.NormalPrice,
                TariffId = berlaku?.TariffId,
                TariffCode = berlaku?.TariffCode,
                IsCoveredByInsuranceDefault = procedure.IsCoveredByInsuranceDefault
            };

            if (berlaku == null)
            {
                hasil.Note = "Tarif untuk pemeriksaan ini belum diatur. Hubungi bagian data induk.";
                return hasil;
            }

            if (!query.InsuranceProviderId.HasValue || query.InsuranceProviderId.Value == Guid.Empty)
                return hasil;

            var kontrak = await KontrakPenjaminAsync(
                new List<Guid> { berlaku.TariffId },
                query.InsuranceProviderId.Value,
                query.PatientClassId,
                now,
                cancellationToken);

            if (kontrak.TryGetValue(berlaku.TariffId, out var ditemukan))
            {
                hasil.ContractPrice = ditemukan.ContractPrice;
                hasil.InsuranceTariffCode = ditemukan.InsuranceTariffCode;
                return hasil;
            }

            // Tidak ada kontrak yang cocok. Ini bukan berarti gratis, dan bukan pula berarti
            // pasien pasti membayar sendiri — yang pasti hanya: penjamin ini tidak punya harga
            // kontrak untuk pemeriksaan ini, dan keputusan finansialnya milik Billing.
            hasil.IsNotCovered = true;
            hasil.Note = "Penjamin ini tidak memiliki harga kontrak untuk pemeriksaan tersebut.";

            return hasil;
        }

        /// <summary>
        /// Tampilan tersaring daftar tarif pemeriksaan laboratorium (<c>AC-48</c>).
        ///
        /// Hanya tarif yang menunjuk tindakan berpenanda <c>IsLaboratory</c> yang tampil, supaya
        /// menu ini benar-benar menjadi tampilan Laboratorium dan bukan salinan menu tarif
        /// rumah sakit.
        /// </summary>
        public async Task<PagedResult<LabTariffViewResponse>> GetTariffsAsync(
            LabTariffQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var source = _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.ProcedureId != null &&
                    _dbContext.Set<MstProcedure>().Any(p =>
                        p.Id == x.ProcedureId!.Value && p.IsLaboratory && !p.IsDelete));

            if (query.ProcedureId.HasValue && query.ProcedureId.Value != Guid.Empty)
                source = source.Where(x => x.ProcedureId == query.ProcedureId.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.TariffCode.Contains(search) ||
                    x.TariffName.Contains(search) ||
                    _dbContext.Set<MstProcedure>().Any(p =>
                        p.Id == x.ProcedureId!.Value &&
                        (p.ProcedureCode.Contains(search) || p.ProcedureName.Contains(search))));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.TariffName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LabTariffViewResponse
                {
                    TariffId = x.Id,
                    TariffCode = x.TariffCode,
                    TariffName = x.TariffName,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = _dbContext.Set<MstProcedure>()
                        .Where(p => p.Id == x.ProcedureId!.Value)
                        .Select(p => p.ProcedureCode)
                        .FirstOrDefault(),
                    ProcedureName = _dbContext.Set<MstProcedure>()
                        .Where(p => p.Id == x.ProcedureId!.Value)
                        .Select(p => p.ProcedureName)
                        .FirstOrDefault(),
                    Discipline = _dbContext.Set<MstProcedure>()
                        .Where(p => p.Id == x.ProcedureId!.Value && p.LabDiscipline != null)
                        .Select(p => p.LabDiscipline!.Value.ToString())
                        .FirstOrDefault(),
                    NormalPrice = x.NormalPrice,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<LabTariffViewResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        // =================================================================
        // Pembantu
        // =================================================================

        /// <summary>
        /// Tarif rumah sakit yang berlaku pada saat ini untuk sekumpulan tindakan.
        ///
        /// Aturannya sama persis dengan <c>ResolveTariffAsync</c> pada jalur pemesanan: aktif,
        /// belum dihapus, dan berada dalam rentang berlakunya. Yang paling akhir mulai berlaku
        /// yang menang. Menyamakan aturannya disengaja — harga yang dilihat petugas saat memesan
        /// harus sama dengan harga yang kelak disalin ke baris pemeriksaan.
        /// </summary>
        private async Task<Dictionary<Guid, TarifBerlaku>> TarifBerlakuAsync(
            IReadOnlyList<Guid> procedureIds,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (procedureIds.Count == 0)
                return new Dictionary<Guid, TarifBerlaku>();

            var rows = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .Where(x =>
                    x.ProcedureId != null &&
                    procedureIds.Contains(x.ProcedureId.Value) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    (x.EffectiveStartDate == null || x.EffectiveStartDate <= now) &&
                    (x.EffectiveEndDate == null || x.EffectiveEndDate >= now))
                .Select(x => new
                {
                    ProcedureId = x.ProcedureId!.Value,
                    TariffId = x.Id,
                    x.TariffCode,
                    x.NormalPrice,
                    x.EffectiveStartDate
                })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.ProcedureId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var terpilih = g
                            .OrderByDescending(x => x.EffectiveStartDate ?? DateTime.MinValue)
                            .First();

                        return new TarifBerlaku(terpilih.TariffId, terpilih.TariffCode, terpilih.NormalPrice);
                    });
        }

        /// <summary>
        /// Harga kontrak penjamin untuk sekumpulan tarif.
        ///
        /// Kelas perawatan diperlakukan sebagai penyaring opsional: kontrak yang tidak menyebut
        /// kelas berlaku untuk semua kelas. Bila keduanya ada, yang menyebut kelas didahulukan,
        /// lalu <c>Priority</c> yang lebih kecil.
        /// </summary>
        private async Task<Dictionary<Guid, KontrakPenjamin>> KontrakPenjaminAsync(
            IReadOnlyList<Guid> tariffIds,
            Guid insuranceProviderId,
            Guid? patientClassId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (tariffIds.Count == 0)
                return new Dictionary<Guid, KontrakPenjamin>();

            var rows = await _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Where(x =>
                    tariffIds.Contains(x.TariffId) &&
                    x.InsuranceProviderId == insuranceProviderId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    (x.EffectiveStartDate == null || x.EffectiveStartDate <= now) &&
                    (x.EffectiveEndDate == null || x.EffectiveEndDate >= now) &&
                    (x.PatientClassId == null ||
                     (patientClassId != null && x.PatientClassId == patientClassId)))
                .Select(x => new
                {
                    x.TariffId,
                    x.PatientClassId,
                    x.InsuranceTariffCode,
                    x.ContractPrice,
                    x.Priority
                })
                .ToListAsync(cancellationToken);

            return rows
                .GroupBy(x => x.TariffId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var terpilih = g
                            .OrderByDescending(x => x.PatientClassId != null)
                            .ThenBy(x => x.Priority)
                            .First();

                        return new KontrakPenjamin(terpilih.InsuranceTariffCode, terpilih.ContractPrice);
                    });
        }

        private sealed record TarifBerlaku(Guid TariffId, string TariffCode, decimal NormalPrice);

        private sealed record KontrakPenjamin(string InsuranceTariffCode, decimal ContractPrice);
    }

    /// <summary>Pelanggaran aturan isi pada jalur katalog. Dipetakan menjadi <c>422</c>.</summary>
    public sealed class LabCatalogValidationException(string message) : Exception(message);
}
