using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.ChartOfAccount.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.EventType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.JournalType.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Enums;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Models;
using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.MasterData.PostingRule.Services
{
    /// <summary>
    /// Aturan posting berbaris — <c>BE-ACC-P2-018</c>, kontrak <c>ACC-API</c> grup Posting Rule dan
    /// <c>ACC-VALIDATION</c> Phase 2 bagian 2. Mewujudkan <c>ACC-DEC-045</c>, <c>ACC-DEC-058</c>,
    /// dan <c>ACC-DEC-074</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Berkas ini hanya menyimpan aturan; ia tidak menerapkannya.</b> Mesin yang membaca
    /// kejadian keuangan lalu menyusun jurnal menurut aturan ini adalah bagian kotak masuk kejadian,
    /// yang menunggu keputusan lintas modul dengan Finance. Yang disediakan untuk mesin itu kelak
    /// hanyalah <see cref="CariAturanAktifAsync"/>.
    /// </para>
    /// <para>
    /// <b>Yang sengaja belum ditegakkan:</b> penonaktifan aturan yang masih ditunggu kejadian
    /// Tertahan (<c>ACC-VALIDATION</c> Phase 2 bagian 2 baris terakhir). Tabel kejadiannya belum
    /// ada, sehingga tidak ada yang dapat diperiksa. Dicatat sebagai kekurangan terencana.
    /// </para>
    /// </remarks>
    public class AccPostingRuleService
    {
        private readonly ApplicationDbContext _db;

        public AccPostingRuleService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>Baris minimum satu aturan — sebuah jurnal selalu punya dua sisi.</summary>
        private const int BarisMinimum = 2;

        private const int PanjangKomponenMaksimum = 50;

        /// <summary>
        /// Nama unique index <c>(LegalEntityId, EventTypeId)</c> berfilter aturan aktif — nama bawaan
        /// EF untuk index pada <c>AccPostingRuleConfiguration</c>. Dipakai mengenali dua penyimpanan
        /// bersamaan yang sama-sama lolos pemeriksaan kode.
        /// </summary>
        private const string NamaIndexSatuAturanAktif = "IX_AccPostingRule_LegalEntityId_EventTypeId";

        // ------------------------------------------------------------------
        // Baca
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PagedResult<PostingRuleListResponse>>> GetPagedAsync(
            PostingRulePagedQuery query,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard
                .PeriksaAsync<PagedResult<PostingRuleListResponse>>(_db, ct);
            if (penjaga is not null) return penjaga;

            var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

            IQueryable<AccPostingRule> q = _db.Set<AccPostingRule>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (query.LegalEntityId.HasValue)
                q = q.Where(x => x.LegalEntityId == query.LegalEntityId.Value);

            if (query.EventTypeId.HasValue)
                q = q.Where(x => x.EventTypeId == query.EventTypeId.Value);

            if (query.JournalTypeId.HasValue)
                q = q.Where(x => x.JournalTypeId == query.JournalTypeId.Value);

            if (query.Treatment.HasValue)
                q = q.Where(x => x.Treatment == query.Treatment.Value);

            if (query.IsActive.HasValue)
                q = q.Where(x => x.IsActive == query.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var cari = query.Search.Trim().ToLower();
                q = q.Where(x => x.EventType != null
                                 && (x.EventType.EventTypeCode.ToLower().Contains(cari)
                                     || x.EventType.EventTypeName.ToLower().Contains(cari)));
            }

            var menurun = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            q = (query.SortBy?.ToLowerInvariant()) switch
            {
                "journaltypecode" => menurun
                    ? q.OrderByDescending(x => x.JournalType!.JournalTypeCode)
                    : q.OrderBy(x => x.JournalType!.JournalTypeCode),
                "createdatetime" => menurun
                    ? q.OrderByDescending(x => x.CreateDateTime)
                    : q.OrderBy(x => x.CreateDateTime),
                _ => menurun
                    ? q.OrderByDescending(x => x.EventType!.EventTypeCode).ThenByDescending(x => x.IsActive)
                    : q.OrderBy(x => x.EventType!.EventTypeCode).ThenByDescending(x => x.IsActive)
            };

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PostingRuleListResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityName = x.LegalEntity != null ? x.LegalEntity.LegalEntityName : string.Empty,
                    EventTypeId = x.EventTypeId,
                    EventTypeCode = x.EventType != null ? x.EventType.EventTypeCode : string.Empty,
                    EventTypeName = x.EventType != null ? x.EventType.EventTypeName : string.Empty,
                    JournalTypeId = x.JournalTypeId,
                    JournalTypeCode = x.JournalType != null ? x.JournalType.JournalTypeCode : string.Empty,
                    JournalTypeName = x.JournalType != null ? x.JournalType.JournalTypeName : string.Empty,
                    Treatment = x.Treatment,
                    IsActive = x.IsActive,
                    LineCount = x.Lines.Count(l => !l.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(ct);

            return AccountingServiceResult<PagedResult<PostingRuleListResponse>>.Ok(
                new PagedResult<PostingRuleListResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = total,
                    TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                    Items = items
                },
                "Daftar aturan posting berhasil diambil.");
        }

        public async Task<AccountingServiceResult<PostingRuleDetailResponse>> GetByIdAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<PostingRuleDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var aturan = await MuatLengkapAsync(_db, id, lacak: false, ct);

            return aturan is null
                ? TidakDitemukan<PostingRuleDetailResponse>()
                : AccountingServiceResult<PostingRuleDetailResponse>.Ok(
                    Petakan(aturan), "Rincian aturan posting berhasil diambil.");
        }

        // ------------------------------------------------------------------
        // Tulis
        // ------------------------------------------------------------------

        public async Task<AccountingServiceResult<PostingRuleDetailResponse>> CreateAsync(
            CreatePostingRuleRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<PostingRuleDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            if (request.LegalEntityId == Guid.Empty)
                return Gagal<PostingRuleDetailResponse>(StatusCodes.Status400BadRequest, "Badan hukum wajib dipilih.");

            var badanHukumAda = await _db.Set<MstLegalEntity>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.LegalEntityId && !x.IsDelete && x.IsActive, ct);

            if (!badanHukumAda)
            {
                return Gagal<PostingRuleDetailResponse>(
                    StatusCodes.Status422UnprocessableEntity, "Badan hukum tidak ditemukan atau tidak aktif.");
            }

            var jenisKejadian = request.EventTypeId == Guid.Empty
                ? null
                : await _db.Set<AccEventType>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.EventTypeId && !x.IsDelete && x.IsActive, ct);

            if (jenisKejadian is null)
            {
                return Gagal<PostingRuleDetailResponse>(
                    StatusCodes.Status400BadRequest, "Jenis kejadian wajib dipilih dan harus aktif.");
            }

            var siap = await SiapkanAsync(
                request.LegalEntityId, request.JournalTypeId, request.Treatment, request.Lines, ct);

            if (siap.Gagal is not null) return siap.Gagal;

            // ACC-VALIDATION Phase 2 bagian 2 — satu jenis kejadian satu aturan aktif per badan
            // hukum. Pemeriksaan ini memberi pesan yang enak dibaca; penjaga terakhirnya unique index.
            if (await AdaAturanAktifLainAsync(request.LegalEntityId, request.EventTypeId, kecualiId: null, ct))
                return SatuAturanAktif<PostingRuleDetailResponse>(jenisKejadian.EventTypeCode);

            var sekarang = DateTime.UtcNow;

            var aturan = new AccPostingRule
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId,
                EventTypeId = request.EventTypeId,
                JournalTypeId = request.JournalTypeId,
                Treatment = request.Treatment,
                IsActive = true,
                CreateDateTime = sekarang,
                CreateBy = actorUserId
            };

            foreach (var baris in siap.Baris)
            {
                baris.PostingRuleId = aturan.Id;
                baris.CreateDateTime = sekarang;
                baris.CreateBy = actorUserId;
            }

            _db.Set<AccPostingRule>().Add(aturan);
            _db.Set<AccPostingRuleLine>().AddRange(siap.Baris);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (MelanggarIndex(ex, NamaIndexSatuAturanAktif))
            {
                return SatuAturanAktif<PostingRuleDetailResponse>(jenisKejadian.EventTypeCode);
            }

            var tersimpan = await MuatLengkapAsync(_db, aturan.Id, lacak: false, ct);

            return AccountingServiceResult<PostingRuleDetailResponse>.Ok(
                Petakan(tersimpan!),
                $"Aturan posting untuk jenis kejadian {jenisKejadian.EventTypeCode} berhasil disimpan.",
                StatusCodes.Status201Created);
        }

        /// <remarks>
        /// <para>
        /// Baris dikirim <b>utuh</b> dan menggantikan seluruh baris sebelumnya. Badan hukum dan
        /// jenis kejadian tidak berubah — keduanya identitas aturan.
        /// </para>
        /// <para>
        /// <b>Jebakan EF yang sama dengan <c>AccRecurringJournalService.UpdateAsync</c>.</b>
        /// Penghapusan baris lama disimpan lebih dahulu, lalu baris baru ditambahkan lewat
        /// <c>DbSet.AddRange</c> — bukan lewat navigation yang terlacak — dalam satu transaction.
        /// Tanpa itu EF mencocokkan baris baru bernomor sama dengan baris lama dan mengirim
        /// <c>UPDATE</c> alih-alih <c>INSERT</c>.
        /// </para>
        /// </remarks>
        public async Task<AccountingServiceResult<PostingRuleDetailResponse>> UpdateAsync(
            Guid id,
            UpdatePostingRuleRequest request,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<PostingRuleDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var aturan = await MuatLengkapAsync(_db, id, lacak: true, ct);
            if (aturan is null) return TidakDitemukan<PostingRuleDetailResponse>();

            var siap = await SiapkanAsync(
                aturan.LegalEntityId, request.JournalTypeId, request.Treatment, request.Lines, ct);

            if (siap.Gagal is not null) return siap.Gagal;

            var sekarang = DateTime.UtcNow;

            var transaksiSendiri = _db.Database.CurrentTransaction is null;
            var transaksi = transaksiSendiri
                ? await _db.Database.BeginTransactionAsync(ct)
                : null;

            try
            {
                var barisLama = aturan.Lines.ToList();

                _db.Set<AccPostingRuleLine>().RemoveRange(barisLama);
                aturan.Lines.Clear();

                // Penghapusan disimpan LEBIH DAHULU — lihat catatan jebakan EF di atas.
                await _db.SaveChangesAsync(ct);

                aturan.JournalTypeId = request.JournalTypeId;
                aturan.Treatment = request.Treatment;
                aturan.UpdateDateTime = sekarang;
                aturan.UpdateBy = actorUserId;

                foreach (var baris in siap.Baris)
                {
                    baris.PostingRuleId = aturan.Id;
                    baris.CreateDateTime = sekarang;
                    baris.CreateBy = actorUserId;
                }

                // Lewat DbSet, bukan lewat aturan.Lines yang terlacak.
                _db.Set<AccPostingRuleLine>().AddRange(siap.Baris);

                await _db.SaveChangesAsync(ct);

                if (transaksi is not null) await transaksi.CommitAsync(ct);
            }
            catch
            {
                if (transaksi is not null) await transaksi.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (transaksi is not null) await transaksi.DisposeAsync();
            }

            var tersimpan = await MuatLengkapAsync(_db, aturan.Id, lacak: false, ct);

            return AccountingServiceResult<PostingRuleDetailResponse>.Ok(
                Petakan(tersimpan!), "Aturan posting berhasil diperbarui.");
        }

        /// <summary>
        /// Menonaktifkan aturan posting. Aturan tidak dihapus — ia disimpan nonaktif sebagai riwayat
        /// (kamus data bagian 12), dan slot "satu aturan aktif" untuk jenis kejadian itu terbuka lagi.
        /// </summary>
        public async Task<AccountingServiceResult<PostingRuleDetailResponse>> DeactivateAsync(
            Guid id,
            Guid actorUserId,
            CancellationToken ct = default)
        {
            var penjaga = await AccountingLegalEntityGuard.PeriksaAsync<PostingRuleDetailResponse>(_db, ct);
            if (penjaga is not null) return penjaga;

            var aturan = await _db.Set<AccPostingRule>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (aturan is null) return TidakDitemukan<PostingRuleDetailResponse>();

            if (!aturan.IsActive)
            {
                return Gagal<PostingRuleDetailResponse>(
                    StatusCodes.Status409Conflict, "Aturan posting ini sudah tidak aktif.");
            }

            // DITUNDA: "masih ada kejadian tertahan yang menunggu aturan ini" (409) belum dapat
            // diperiksa — tabel kejadian belum ada. Lihat catatan kelas.
            aturan.IsActive = false;
            aturan.UpdateDateTime = DateTime.UtcNow;
            aturan.UpdateBy = actorUserId;

            await _db.SaveChangesAsync(ct);

            var tersimpan = await MuatLengkapAsync(_db, aturan.Id, lacak: false, ct);

            return AccountingServiceResult<PostingRuleDetailResponse>.Ok(
                Petakan(tersimpan!), "Aturan posting berhasil dinonaktifkan.");
        }

        // ------------------------------------------------------------------
        // Dipakai bersama task lain
        // ------------------------------------------------------------------

        /// <summary>
        /// Aturan posting <b>aktif</b> untuk satu jenis kejadian pada satu badan hukum, beserta
        /// barisnya, atau <c>null</c> bila belum ada.
        /// </summary>
        /// <remarks>
        /// Disediakan untuk mesin posting kotak masuk kejadian kelak — kejadian yang aturannya
        /// <c>null</c> berstatus Tertahan (<c>ACC-DEC-046</c>). <c>public static</c> menerima
        /// <see cref="ApplicationDbContext"/> supaya dapat dipakai tanpa registrasi DI baru,
        /// sesuai <c>02-backend-architecture.md</c> bagian 16.
        /// </remarks>
        public static Task<AccPostingRule?> CariAturanAktifAsync(
            ApplicationDbContext db,
            Guid legalEntityId,
            Guid eventTypeId,
            CancellationToken ct = default)
        {
            return db.Set<AccPostingRule>()
                .AsNoTracking()
                .Include(x => x.Lines.Where(l => !l.IsDelete).OrderBy(l => l.LineNumber))
                .FirstOrDefaultAsync(
                    x => !x.IsDelete
                         && x.IsActive
                         && x.LegalEntityId == legalEntityId
                         && x.EventTypeId == eventTypeId,
                    ct);
        }

        // ------------------------------------------------------------------
        // Persiapan
        // ------------------------------------------------------------------

        private sealed class HasilPersiapan
        {
            public AccountingServiceResult<PostingRuleDetailResponse>? Gagal { get; init; }

            public List<AccPostingRuleLine> Baris { get; init; } = new();
        }

        private static HasilPersiapan Tolak(int statusCode, string pesan)
            => new() { Gagal = Gagal<PostingRuleDetailResponse>(statusCode, pesan) };

        /// <summary>
        /// Memeriksa jenis jurnal, perlakuan, dan seluruh baris. Dipakai bersama
        /// <see cref="CreateAsync"/> dan <see cref="UpdateAsync"/> supaya aturan yang sama tidak
        /// ditulis dua kali dengan pesan berbeda.
        /// </summary>
        private async Task<HasilPersiapan> SiapkanAsync(
            Guid legalEntityId,
            Guid journalTypeId,
            AccountingEventTreatment treatment,
            List<PostingRuleLineRequest>? lines,
            CancellationToken ct)
        {
            var jenisJurnal = journalTypeId == Guid.Empty
                ? null
                : await _db.Set<AccJournalType>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == journalTypeId && !x.IsDelete && x.IsActive, ct);

            if (jenisJurnal is null)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest, "Jenis jurnal wajib dipilih dan harus aktif.");
            }

            if (!Enum.IsDefined(treatment))
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    "Perlakuan aturan posting harus langsung disahkan atau buat draft.");
            }

            return await SusunBarisAsync(legalEntityId, lines, ct);
        }

        /// <remarks>
        /// Pesan penolakan selalu menyebut <b>nomor baris</b>, mengikuti baris jurnal dan template.
        /// Kode status mengikuti <c>ACC-VALIDATION</c> Phase 2 bagian 2: akun induk <c>422</c>,
        /// akun badan hukum lain <c>409</c>, akun beban tanpa cost center <c>400</c>.
        /// </remarks>
        private async Task<HasilPersiapan> SusunBarisAsync(
            Guid legalEntityId,
            List<PostingRuleLineRequest>? lines,
            CancellationToken ct)
        {
            var permintaan = lines ?? new List<PostingRuleLineRequest>();

            if (permintaan.Count < BarisMinimum)
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    $"Aturan posting minimal memiliki {BarisMinimum} baris.");
            }

            if (permintaan.Select(x => x.LineNumber).Distinct().Count() != permintaan.Count)
                return Tolak(StatusCodes.Status400BadRequest, "Nomor baris tidak boleh kembar.");

            var idAkun = permintaan.Select(x => x.AccountId).Distinct().ToList();

            var akunTersedia = await _db.Set<AccChartOfAccount>()
                .AsNoTracking()
                .Where(x => idAkun.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var idUnitBiaya = permintaan
                .Where(x => x.CostCenterId.HasValue)
                .Select(x => x.CostCenterId!.Value)
                .Distinct()
                .ToList();

            var unitBiayaTersedia = await _db.Set<MstCostCenter>()
                .AsNoTracking()
                .Where(x => idUnitBiaya.Contains(x.Id) && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, ct);

            var hasil = new List<AccPostingRuleLine>();

            foreach (var baris in permintaan.OrderBy(x => x.LineNumber))
            {
                var n = baris.LineNumber;

                if (n < 1)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Nomor baris harus dimulai dari 1; ditemukan {n}.");
                }

                if (!Enum.IsDefined(baris.Side))
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: sisi harus debit atau kredit.");
                }

                var komponen = string.IsNullOrWhiteSpace(baris.ComponentCode)
                    ? AccPostingRuleLine.KomponenTotal
                    : baris.ComponentCode.Trim();

                if (komponen.Length > PanjangKomponenMaksimum)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: kode komponen maksimal {PanjangKomponenMaksimum} karakter.");
                }

                if (!akunTersedia.TryGetValue(baris.AccountId, out var akun) || !akun.IsActive)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: akun tidak ditemukan atau sudah tidak aktif.");
                }

                if (!akun.IsPostable)
                {
                    return Tolak(
                        StatusCodes.Status422UnprocessableEntity,
                        $"Baris ke-{n}: akun {akun.AccountCode} adalah akun induk dan tidak dapat "
                        + "menerima transaksi.");
                }

                if (akun.LegalEntityId != legalEntityId)
                {
                    return Tolak(
                        StatusCodes.Status409Conflict,
                        $"Baris ke-{n}: seluruh akun pada aturan posting harus berasal dari badan "
                        + $"hukum yang sama; akun {akun.AccountCode} milik badan hukum lain.");
                }

                // ACC-DEC-019.
                if (akun.AccountType == AccountType.Expense && !baris.CostCenterId.HasValue)
                {
                    return Tolak(
                        StatusCodes.Status400BadRequest,
                        $"Baris ke-{n}: baris akun beban {akun.AccountCode} wajib mencantumkan cost center.");
                }

                if (baris.CostCenterId.HasValue)
                {
                    if (!unitBiayaTersedia.TryGetValue(baris.CostCenterId.Value, out var unitBiaya)
                        || !unitBiaya.IsActive
                        || unitBiaya.LegalEntityId != legalEntityId)
                    {
                        return Tolak(
                            StatusCodes.Status409Conflict,
                            $"Baris ke-{n}: cost center tidak aktif atau bukan milik badan hukum aturan ini.");
                    }
                }

                // Control account SENGAJA tidak ditolak: aturan posting adalah jalur otomatis yang
                // sah menuju Kas Kasir, Piutang, dan Utang (ACC-DEC-064).
                hasil.Add(new AccPostingRuleLine
                {
                    Id = Guid.NewGuid(),
                    LineNumber = n,
                    ComponentCode = komponen,
                    AccountId = baris.AccountId,
                    CostCenterId = baris.CostCenterId,
                    Side = baris.Side,
                    Description = baris.Description?.Trim()
                });
            }

            // "Aturan harus dapat seimbang" — diperiksa PALING AKHIR. Baris aturan tidak membawa
            // nominal, jadi keseimbangan sesungguhnya baru diketahui saat nilai komponen kejadian
            // dimasukkan. Yang dapat dipastikan sekarang hanyalah kasus yang TIDAK AKAN PERNAH
            // seimbang untuk nilai berapa pun: seluruh baris berada di satu sisi. Contoh: dua baris
            // debit TOTAL dan debit JASA_MEDIS tanpa satu baris kredit pun.
            if (hasil.All(x => x.Side == PostingSide.Debit) || hasil.All(x => x.Side == PostingSide.Kredit))
            {
                return Tolak(
                    StatusCodes.Status400BadRequest,
                    "Aturan posting ini tidak akan pernah menghasilkan jurnal yang seimbang: "
                    + "aturan wajib memiliki sekurang-kurangnya satu baris debit dan satu baris kredit.");
            }

            return new HasilPersiapan { Baris = hasil };
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private Task<bool> AdaAturanAktifLainAsync(
            Guid legalEntityId, Guid eventTypeId, Guid? kecualiId, CancellationToken ct)
        {
            return _db.Set<AccPostingRule>()
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete
                               && x.IsActive
                               && x.LegalEntityId == legalEntityId
                               && x.EventTypeId == eventTypeId
                               && (kecualiId == null || x.Id != kecualiId.Value), ct);
        }

        private static bool MelanggarIndex(DbUpdateException exception, string namaIndex)
        {
            var postgresException = exception.InnerException as PostgresException;

            return postgresException?.SqlState == PostgresErrorCodes.UniqueViolation
                   && string.Equals(postgresException.ConstraintName, namaIndex, StringComparison.Ordinal);
        }

        private static AccountingServiceResult<T> SatuAturanAktif<T>(string kodeJenis)
            => Gagal<T>(
                StatusCodes.Status409Conflict,
                $"Jenis kejadian {kodeJenis} sudah punya aturan posting aktif pada badan hukum ini. "
                + "Nonaktifkan aturan lama lebih dahulu, atau ubah aturan yang sudah ada.");

        private static AccountingServiceResult<T> Gagal<T>(int statusCode, string pesan)
            => AccountingServiceResult<T>.Fail(statusCode, pesan);

        private static AccountingServiceResult<T> TidakDitemukan<T>()
            => Gagal<T>(StatusCodes.Status404NotFound, "Aturan posting tidak ditemukan atau sudah dihapus.");

        private static Task<AccPostingRule?> MuatLengkapAsync(
            ApplicationDbContext db, Guid id, bool lacak, CancellationToken ct)
        {
            var q = db.Set<AccPostingRule>()
                .Include(x => x.LegalEntity)
                .Include(x => x.EventType)
                .Include(x => x.JournalType)
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.Account)
                .Include(x => x.Lines.Where(l => !l.IsDelete))
                    .ThenInclude(l => l.CostCenter)
                .AsSplitQuery();

            if (!lacak) q = q.AsNoTracking();

            return q.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);
        }

        private static PostingRuleDetailResponse Petakan(AccPostingRule x)
        {
            var baris = x.Lines.Where(l => !l.IsDelete).OrderBy(l => l.LineNumber).ToList();

            return new PostingRuleDetailResponse
            {
                Id = x.Id,
                LegalEntityId = x.LegalEntityId,
                LegalEntityName = x.LegalEntity?.LegalEntityName ?? string.Empty,
                EventTypeId = x.EventTypeId,
                EventTypeCode = x.EventType?.EventTypeCode ?? string.Empty,
                EventTypeName = x.EventType?.EventTypeName ?? string.Empty,
                JournalTypeId = x.JournalTypeId,
                JournalTypeCode = x.JournalType?.JournalTypeCode ?? string.Empty,
                JournalTypeName = x.JournalType?.JournalTypeName ?? string.Empty,
                Treatment = x.Treatment,
                IsActive = x.IsActive,
                LineCount = baris.Count,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy,
                Lines = baris.Select(l => new PostingRuleLineResponse
                {
                    Id = l.Id,
                    LineNumber = l.LineNumber,
                    ComponentCode = l.ComponentCode,
                    AccountId = l.AccountId,
                    AccountCode = l.Account?.AccountCode ?? string.Empty,
                    AccountName = l.Account?.AccountName ?? string.Empty,
                    AccountType = l.Account?.AccountType ?? default,
                    IsControlAccount = l.Account?.IsControlAccount ?? false,
                    CostCenterId = l.CostCenterId,
                    CostCenterName = l.CostCenter?.CostCenterName,
                    Side = l.Side,
                    Description = l.Description
                }).ToList()
            };
        }
    }
}
