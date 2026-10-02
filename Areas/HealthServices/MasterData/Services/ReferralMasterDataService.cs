using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Services
{
    /// <summary>
    /// Daftar pilihan data induk perujuk — <b>baca saja</b> (<c>LAB-DEC-035</c>,
    /// <c>BE-EXT-02</c>).
    ///
    /// <b>Kenapa berkas ini ada.</b> `BE-EXT-02` membuat <c>MstReferralInstitution</c> dan
    /// <c>MstReferralDoctor</c> beserta <c>DbSet</c>-nya, tetapi tidak membuat satu pun
    /// endpoint. Selama itu, butir Verifikasi task tersebut — *"kedua data induk dapat dipilih
    /// dari daftar"* — tidak dapat dipenuhi siapa pun, dan layar pendaftaran rujukan luar
    /// (<c>FE-LAB-05</c>) tidak punya sumber pilihan. Berkas ini menutup celah itu.
    ///
    /// <b>Batasnya tegas: tidak ada satu pun jalur ubah di sini.</b> Penambahan dan penyuntingan
    /// data induk perujuk adalah pekerjaan modul Data Induk, bukan pemakainya. Ketiadaan jalur
    /// ubah itu disengaja, sama seperti pada grup katalog Laboratorium.
    /// </summary>
    public class ReferralMasterDataService
    {
        private const int MinPageSize = 1;
        private const int MaxPageSize = 100;

        private readonly ApplicationDbContext _dbContext;

        public ReferralMasterDataService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Daftar pilihan instansi perujuk, terurut menurut nama.
        /// </summary>
        public async Task<PagedResult<ReferralInstitutionOptionResponse>> GetInstitutionOptionsAsync(
            ReferralInstitutionOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, MinPageSize, MaxPageSize);

            var source = _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.OnlyActive)
            {
                source = source.Where(x => x.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x =>
                    x.InstitutionCode.Contains(search) ||
                    x.InstitutionName.Contains(search));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.InstitutionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReferralInstitutionOptionResponse
                {
                    Id = x.Id,
                    InstitutionCode = x.InstitutionCode,
                    InstitutionName = x.InstitutionName,
                    Address = x.Address,
                    PhoneNumber = x.PhoneNumber,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ReferralInstitutionOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        /// <summary>
        /// Daftar pilihan dokter perujuk, terurut menurut nama.
        ///
        /// Penyaring instansi tersedia dan sangat dianjurkan dipakai, karena pendaftaran
        /// rujukan menolak dokter yang tidak berpraktik pada instansi yang dipilih.
        /// </summary>
        public async Task<PagedResult<ReferralDoctorOptionResponse>> GetDoctorOptionsAsync(
            ReferralDoctorOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, MinPageSize, MaxPageSize);

            var source = _dbContext.Set<MstReferralDoctor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.OnlyActive)
            {
                source = source.Where(x => x.IsActive);
            }

            if (query.ReferralInstitutionId.HasValue &&
                query.ReferralInstitutionId.Value != Guid.Empty)
            {
                source = source.Where(x =>
                    x.ReferralInstitutionId == query.ReferralInstitutionId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();

                source = source.Where(x => x.DoctorName.Contains(search));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var items = await source
                .OrderBy(x => x.DoctorName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ReferralDoctorOptionResponse
                {
                    Id = x.Id,
                    ReferralInstitutionId = x.ReferralInstitutionId,
                    DoctorName = x.DoctorName,
                    InstitutionName = x.ReferralInstitution != null
                        ? x.ReferralInstitution.InstitutionName
                        : null,
                    IsActive = x.IsActive
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ReferralDoctorOptionResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }
    }
}
